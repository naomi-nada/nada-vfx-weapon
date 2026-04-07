using System.Collections.Generic;
using NADA.VFX.Core.Config;
using NADA.VFX.Core.State;
using NADA.VFX.Runtime.Binding;
using UnityEngine;

namespace NADA.VFX.Modules.Motion
{
    // Orbitals motion model:
    // - transform = motion root (true simulated head position)
    // - _headVisualTransform = visible head representation
    // - _followerVisualTransforms = trailing visuals
    // - All orbital families use explicit external visual chains
    internal sealed class NadaOrbitalsMotion : MonoBehaviour
    {
        private const int MaxOrbitalsVisuals = 20;

        private const float MinOrbitalsRadiusMultiplier = 0.50f;
        private const float MaxOrbitalsRadiusMultiplier = 1.50f;
        private const float DefaultOrbitalsRadiusMultiplier = 1.00f;

        private const int ExtraHistoryPadding = 24;

        private const int MinHistoryStepPerFollower = 2;
        private const int MaxHistoryStepPerFollower = 36;

        private const float FollowerFollowSharpness = 14f;
        private const float HardLockAdherenceThreshold = 0.999f;

        private NadaOrbitalsFamily _orbitalsFamily = NadaOrbitalsFamily.Orbs;
        private global::ItemDrop.ItemData _itemData;

        private Transform _headVisualTransform;
        private Transform _followerPoolRootTransform;
        private bool _configuredVisualChain;

        private Vector3 _baseHeadLocalPosition;
        private bool _initialized;

        private float _currentHistoryStepPerFollower;
        private bool _hasCurrentHistoryStepPerFollower;

        private readonly List<Transform> _followerVisualTransforms = new();
        private readonly List<Vector3> _headWorldPositionHistorySamples = new();
        private readonly List<Vector3> _currentFollowerWorldPositions = new();
        private readonly List<bool> _hasInitializedFollowerWorldPosition = new();

        internal void Configure(
            NadaOrbitalsFamily orbitalsFamily,
            global::ItemDrop.ItemData itemData)
        {
            _orbitalsFamily = orbitalsFamily;
            _itemData = itemData;

            _headVisualTransform = null;
            _followerPoolRootTransform = null;
            _configuredVisualChain = false;
            _initialized = false;
        }

        internal void SetExternalVisualChain(
            Transform headVisualTransform,
            Transform followerPoolRootTransform)
        {
            _headVisualTransform = headVisualTransform;
            _followerPoolRootTransform = followerPoolRootTransform;
            _configuredVisualChain =
                headVisualTransform != null &&
                followerPoolRootTransform != null;
        }

        private void Awake()
        {
            _baseHeadLocalPosition = transform.localPosition;
        }

        private void LateUpdate()
        {
            ResolveVisualChain();
            if (!_initialized)
                return;

            EnsureFollowerPositionStateCapacity();

            VfxState state = ResolveState();

            if (!ResolveFamilyEnabled(state))
            {
                DisableAllVisualsAndResetState();
                return;
            }

            int desiredTotalVisualCount = ResolveDesiredVisualCount(state);
            int desiredFollowerCount = Mathf.Clamp(
                desiredTotalVisualCount - 1,
                0,
                MaxOrbitalsVisuals - 1);

            float radiusMultiplier = ResolveRadiusMultiplier(state);
            float targetHistoryStepPerFollower = ResolveHistoryStepPerFollowerFloat(state);

            if (!_hasCurrentHistoryStepPerFollower)
            {
                _currentHistoryStepPerFollower = targetHistoryStepPerFollower;
                _hasCurrentHistoryStepPerFollower = true;
            }
            else
            {
                float smoothingSpeed =
                    targetHistoryStepPerFollower > _currentHistoryStepPerFollower ? 4f : 10f;

                _currentHistoryStepPerFollower = Mathf.Lerp(
                    _currentHistoryStepPerFollower,
                    targetHistoryStepPerFollower,
                    1f - Mathf.Exp(-smoothingSpeed * Time.deltaTime));
            }

            float orbitAdherence = Mathf.Clamp01(ResolveOrbitAdherence());

            ApplyHeadVisualEnabledState(true);
            ApplyFollowerVisualCount(desiredFollowerCount);

            Vector3 currentHeadLocalPosition =
                EvaluateHeadLocalPosition(Time.time, radiusMultiplier);

            transform.localPosition = currentHeadLocalPosition;
            SyncHeadVisualToMotionRoot();

            RecordHeadWorldHistory(transform.position);

            ApplyFollowerPositions(
                desiredFollowerCount,
                _currentHistoryStepPerFollower,
                radiusMultiplier,
                orbitAdherence);
        }

        private void ResolveVisualChain()
        {
            _followerVisualTransforms.Clear();
            _initialized = false;

            if (!_configuredVisualChain || _headVisualTransform == null || _followerPoolRootTransform == null)
                return;

            foreach (Transform childTransform in _followerPoolRootTransform)
            {
                if (childTransform != null)
                    _followerVisualTransforms.Add(childTransform);
            }

            _initialized = true;
        }

        private void EnsureFollowerPositionStateCapacity()
        {
            while (_currentFollowerWorldPositions.Count < MaxOrbitalsVisuals - 1)
                _currentFollowerWorldPositions.Add(Vector3.zero);

            while (_hasInitializedFollowerWorldPosition.Count < MaxOrbitalsVisuals - 1)
                _hasInitializedFollowerWorldPosition.Add(false);
        }

        private bool ResolveFamilyEnabled(VfxState state)
        {
            return _orbitalsFamily switch
            {
                NadaOrbitalsFamily.Orbs => state.OrbitalsOrbsEnabled,
                NadaOrbitalsFamily.Flames => state.OrbitalsFlamesEnabled,
                NadaOrbitalsFamily.Embers => state.OrbitalsEmbersEnabled,
                _ => false
            };
        }

        private int ResolveDesiredVisualCount(VfxState state)
        {
            float normalizedCount = _orbitalsFamily switch
            {
                NadaOrbitalsFamily.Orbs => state.OrbitalsOrbsCount,
                NadaOrbitalsFamily.Flames => state.OrbitalsFlamesCount,
                NadaOrbitalsFamily.Embers => state.OrbitalsEmbersCount,
                _ => 0f
            };

            normalizedCount = Mathf.Clamp01(normalizedCount);
            return 1 + Mathf.RoundToInt(normalizedCount * (MaxOrbitalsVisuals - 1));
        }

        private float ResolveHistoryStepPerFollowerFloat(VfxState state)
        {
            float spacingT = _orbitalsFamily switch
            {
                NadaOrbitalsFamily.Orbs => Mathf.Clamp(
                    state.OrbitalsOrbsSpacing,
                    PluginConfig.MinOrbitalsSpacing,
                    PluginConfig.MaxOrbitalsSpacing),

                NadaOrbitalsFamily.Flames => Mathf.Clamp(
                    state.OrbitalsFlamesSpacing,
                    PluginConfig.MinOrbitalsSpacing,
                    PluginConfig.MaxOrbitalsSpacing),

                NadaOrbitalsFamily.Embers => Mathf.Clamp(
                    state.OrbitalsEmbersSpacing,
                    PluginConfig.MinOrbitalsSpacing,
                    PluginConfig.MaxOrbitalsSpacing),

                _ => 0f
            };

            return Mathf.Lerp(
                MinHistoryStepPerFollower,
                MaxHistoryStepPerFollower,
                spacingT);
        }

        private float ResolveRadiusMultiplier(VfxState state)
        {
            float value = _orbitalsFamily switch
            {
                NadaOrbitalsFamily.Orbs => state.OrbitalsOrbsRadius,
                NadaOrbitalsFamily.Flames => state.OrbitalsFlamesRadius,
                NadaOrbitalsFamily.Embers => state.OrbitalsEmbersRadius,
                _ => DefaultOrbitalsRadiusMultiplier
            };

            if (float.IsNaN(value) || float.IsInfinity(value))
                return DefaultOrbitalsRadiusMultiplier;

            return Mathf.Clamp(
                value,
                MinOrbitalsRadiusMultiplier,
                MaxOrbitalsRadiusMultiplier);
        }

        private float ResolveOrbitAdherence()
        {
            return _orbitalsFamily switch
            {
                NadaOrbitalsFamily.Orbs => NadaMotionTuningResolver.GetOrbsOrbitAdherence(_itemData),
                NadaOrbitalsFamily.Flames => NadaMotionTuningResolver.GetFlamesOrbitAdherence(_itemData),
                NadaOrbitalsFamily.Embers => NadaMotionTuningResolver.GetEmbersOrbitAdherence(_itemData),
                _ => 1f
            };
        }

        private VfxState ResolveState()
        {
            if (_itemData != null)
            {
                if (VfxStateIO.TryRead(_itemData, out var itemState))
                    return itemState;

                VfxStateIO.EnsureInitializedFromConfig(_itemData);

                if (VfxStateIO.TryRead(_itemData, out itemState))
                    return itemState;
            }

            return VfxStateIO.FromConfig();
        }

        private void ApplyHeadVisualEnabledState(bool enabled)
        {
            if (_headVisualTransform == null)
                return;

            if (_headVisualTransform.gameObject.activeSelf != enabled)
                _headVisualTransform.gameObject.SetActive(enabled);
        }

        private void ApplyFollowerVisualCount(int desiredFollowerCount)
        {
            desiredFollowerCount = Mathf.Clamp(desiredFollowerCount, 0, MaxOrbitalsVisuals - 1);

            for (int visualIndex = 0; visualIndex < _followerVisualTransforms.Count; visualIndex++)
            {
                Transform followerVisualTransform = _followerVisualTransforms[visualIndex];
                if (followerVisualTransform == null)
                    continue;

                bool shouldBeActive = visualIndex < desiredFollowerCount;
                if (followerVisualTransform.gameObject.activeSelf != shouldBeActive)
                    followerVisualTransform.gameObject.SetActive(shouldBeActive);

                if (!shouldBeActive && visualIndex < _hasInitializedFollowerWorldPosition.Count)
                    _hasInitializedFollowerWorldPosition[visualIndex] = false;
            }
        }

        private void DisableAllVisualsAndResetState()
        {
            ApplyHeadVisualEnabledState(false);

            for (int visualIndex = 0; visualIndex < _followerVisualTransforms.Count; visualIndex++)
            {
                Transform followerVisualTransform = _followerVisualTransforms[visualIndex];
                if (followerVisualTransform != null && followerVisualTransform.gameObject.activeSelf)
                    followerVisualTransform.gameObject.SetActive(false);
            }

            _headWorldPositionHistorySamples.Clear();
            _hasCurrentHistoryStepPerFollower = false;

            for (int visualIndex = 0; visualIndex < _currentFollowerWorldPositions.Count; visualIndex++)
            {
                _currentFollowerWorldPositions[visualIndex] = Vector3.zero;
                _hasInitializedFollowerWorldPosition[visualIndex] = false;
            }
        }

        private void ApplyFollowerPositions(
            int desiredFollowerCount,
            float historyStepPerFollower,
            float radiusMultiplier,
            float orbitAdherence)
        {
            desiredFollowerCount = Mathf.Clamp(desiredFollowerCount, 0, MaxOrbitalsVisuals - 1);

            float followT = 1f - Mathf.Exp(-FollowerFollowSharpness * Time.deltaTime);

            for (int visibleIndex = 0; visibleIndex < desiredFollowerCount; visibleIndex++)
            {
                Transform followerVisualTransform = _followerVisualTransforms[visibleIndex];
                if (followerVisualTransform == null)
                    continue;

                int followerIndex = visibleIndex + 1;

                Vector3 lockedWorldPosition =
                    EvaluateLockedFollowerWorldPosition(
                        followerIndex,
                        historyStepPerFollower,
                        radiusMultiplier);

                float historySampleIndex = followerIndex * historyStepPerFollower;

                Vector3 driftingWorldPosition =
                    SampleHistoryPosition(historySampleIndex, lockedWorldPosition);

                bool hardLock = orbitAdherence >= HardLockAdherenceThreshold;

                if (hardLock)
                {
                    _currentFollowerWorldPositions[visibleIndex] = lockedWorldPosition;
                    _hasInitializedFollowerWorldPosition[visibleIndex] = true;
                }
                else
                {
                    Vector3 targetWorldPosition = Vector3.Lerp(
                        driftingWorldPosition,
                        lockedWorldPosition,
                        orbitAdherence);

                    if (!_hasInitializedFollowerWorldPosition[visibleIndex])
                    {
                        _currentFollowerWorldPositions[visibleIndex] = targetWorldPosition;
                        _hasInitializedFollowerWorldPosition[visibleIndex] = true;
                    }
                    else
                    {
                        _currentFollowerWorldPositions[visibleIndex] = Vector3.Lerp(
                            _currentFollowerWorldPositions[visibleIndex],
                            targetWorldPosition,
                            followT);
                    }
                }

                followerVisualTransform.position = _currentFollowerWorldPositions[visibleIndex];
                followerVisualTransform.rotation = Quaternion.identity;
            }

            for (int visualIndex = desiredFollowerCount; visualIndex < _followerVisualTransforms.Count; visualIndex++)
            {
                if (visualIndex < _hasInitializedFollowerWorldPosition.Count)
                    _hasInitializedFollowerWorldPosition[visualIndex] = false;
            }
        }

        private Vector3 EvaluateHeadLocalPosition(float timeValue, float radiusMultiplier)
        {
            return NadaOrbitalsPath.EvaluateLocalPosition(
                timeValue,
                radiusMultiplier,
                _baseHeadLocalPosition);
        }

        private Vector3 EvaluateHeadWorldPosition(float timeValue, float radiusMultiplier)
        {
            Vector3 localPosition = EvaluateHeadLocalPosition(timeValue, radiusMultiplier);

            Transform parentTransform = transform.parent;
            if (parentTransform != null)
                return parentTransform.TransformPoint(localPosition);

            return localPosition;
        }

        private float EvaluateFollowerTemporalOffset(
            int followerIndex,
            float historyStepPerFollower)
        {
            return NadaOrbitalsPath.EvaluateFollowerTemporalOffsetSeconds(
                followerIndex,
                historyStepPerFollower,
                MinHistoryStepPerFollower,
                MaxHistoryStepPerFollower);
        }

        private Vector3 EvaluateLockedFollowerWorldPosition(
            int followerIndex,
            float historyStepPerFollower,
            float radiusMultiplier)
        {
            float temporalOffset =
                EvaluateFollowerTemporalOffset(followerIndex, historyStepPerFollower);

            float sampleTime = Time.time - temporalOffset;
            return EvaluateHeadWorldPosition(sampleTime, radiusMultiplier);
        }

        private void RecordHeadWorldHistory(Vector3 headWorldPosition)
        {
            _headWorldPositionHistorySamples.Insert(0, headWorldPosition);

            int maxHistorySamples =
                ((MaxOrbitalsVisuals - 1) * MaxHistoryStepPerFollower) + ExtraHistoryPadding;

            if (_headWorldPositionHistorySamples.Count > maxHistorySamples)
            {
                _headWorldPositionHistorySamples.RemoveRange(
                    maxHistorySamples,
                    _headWorldPositionHistorySamples.Count - maxHistorySamples);
            }
        }

        private Vector3 SampleHistoryPosition(
            float sampleIndex,
            Vector3 fallbackWorldPosition)
        {
            if (_headWorldPositionHistorySamples.Count == 0)
                return fallbackWorldPosition;

            if (sampleIndex <= 0f)
                return _headWorldPositionHistorySamples[0];

            int lowerIndex = Mathf.FloorToInt(sampleIndex);
            int upperIndex = Mathf.CeilToInt(sampleIndex);

            if (lowerIndex >= _headWorldPositionHistorySamples.Count)
                return _headWorldPositionHistorySamples[_headWorldPositionHistorySamples.Count - 1];

            if (upperIndex >= _headWorldPositionHistorySamples.Count)
                return _headWorldPositionHistorySamples[_headWorldPositionHistorySamples.Count - 1];

            if (lowerIndex == upperIndex)
                return _headWorldPositionHistorySamples[lowerIndex];

            float interpolationT = sampleIndex - lowerIndex;

            return Vector3.Lerp(
                _headWorldPositionHistorySamples[lowerIndex],
                _headWorldPositionHistorySamples[upperIndex],
                interpolationT);
        }

        private void SyncHeadVisualToMotionRoot()
        {
            if (_headVisualTransform == null)
                return;

            _headVisualTransform.position = transform.position;
            _headVisualTransform.rotation = transform.rotation;
        }
    }
}