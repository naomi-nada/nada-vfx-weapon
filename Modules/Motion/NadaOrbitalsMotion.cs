using System.Collections.Generic;
using NADA.VFX.Core.Config;
using NADA.VFX.Core.State;
using NADA.VFX.Runtime.Binding;
using UnityEngine;

namespace NADA.VFX.Modules.Motion
{
    // Orbitals motion model:
    // - transform = motion root / true locked head position
    // - _headVisualTransform = visible head representation
    // - _followerVisualTransforms = trailing visuals
    // - Drift is applied by lagging the parent/world carrier frame
    internal sealed class NadaOrbitalsMotion : MonoBehaviour
    {
        private const int MaxOrbitalsVisuals = 30;
        private const int ArcLengthSampleCount = 192;

        internal const float DefaultCycleProgressPerSecond = 0.10f;

        private const int ExtraHistoryPadding = 20;
        private const int MinHistoryStepPerFollower = 1;
        private const int MaxHistoryStepPerFollower = 36;
        private const float HardLockAdherenceThreshold = 0.999f;

        // Ownership / state
        private NadaOrbitalsFamily _orbitalsFamily = NadaOrbitalsFamily.Orbs;
        private global::ItemDrop.ItemData _itemData;

        // External visual chain
        private Transform _headVisualTransform;
        private Transform _followerPoolRootTransform;
        private readonly List<Transform> _followerVisualTransforms = new();

        private bool _configuredVisualChain;
        private bool _visualChainDirty = true;
        private bool _initialized;

        // Path cache
        private readonly List<Vector3> _sampledLocalPositions = new();
        private readonly List<float> _sampledCumulativeLengths = new();

        private float _cachedRadiusMultiplier = -1f;
        private float _cachedOrbitLengthMultiplier = -1f;
        private float _cachedTurnsPerOneWayPass = -1f;
        private float _cachedCycleLength;
        private bool _hasArcLengthCache;

        // Motion state
        private bool _lastGlueEnabled;
        private bool _hasLastGlueEnabled;

        private Vector3 _baseHeadLocalPosition;
        private Vector3 _currentLocalOffset;
        private Quaternion _currentLocalRotationOffset = Quaternion.identity;

        private float _currentCycleProgress01;
        private bool _hasCurrentCycleProgress01;

        private float _currentHistoryStepPerFollower;
        private bool _hasCurrentHistoryStepPerFollower;

        // Drift history
        private readonly List<Vector3> _parentWorldPositionHistorySamples = new();
        private readonly List<Quaternion> _parentWorldRotationHistorySamples = new();

        internal void Configure(
            NadaOrbitalsFamily orbitalsFamily,
            global::ItemDrop.ItemData itemData)
        {
            _orbitalsFamily = orbitalsFamily;
            _itemData = itemData;
        }

        internal void SetExternalVisualChain(
            Transform headVisualTransform,
            Transform followerPoolRootTransform)
        {
            if (_headVisualTransform == headVisualTransform &&
                _followerPoolRootTransform == followerPoolRootTransform)
            {
                return;
            }

            _headVisualTransform = headVisualTransform;
            _followerPoolRootTransform = followerPoolRootTransform;

            _configuredVisualChain =
                headVisualTransform != null &&
                followerPoolRootTransform != null;

            _visualChainDirty = true;
        }

        private void Awake()
        {
            _baseHeadLocalPosition = transform.localPosition;
        }

        private void LateUpdate()
        {
            if (_visualChainDirty)
            {
                ResolveVisualChain();
                _visualChainDirty = false;
            }

            if (!_initialized)
                return;

            VfxState state = ResolveState();

            if (!ResolveFamilyEnabled(state))
            {
                DisableAllVisualsAndResetState();
                return;
            }

            ApplyOrbitalsMotion(state);
        }

        // Main motion flow

        private void ApplyOrbitalsMotion(VfxState state)
        {
            bool glueEnabled = ResolveGlueEnabled(state);

            if (!_hasLastGlueEnabled || _lastGlueEnabled != glueEnabled)
            {
                ResetOrbitStartPoint();
                _lastGlueEnabled = glueEnabled;
                _hasLastGlueEnabled = true;
            }

            int desiredFollowerCount = ResolveDesiredFollowerCount(state);

            float radiusMultiplier = ResolveRadiusMultiplier(state);
            float orbitLengthMultiplier = ResolveOrbitLengthMultiplier(state);
            float turnsPerOneWayPass = ResolveTurnsPerOneWayPass(state);

            _currentLocalOffset = ResolveLocalOffset(state);
            _currentLocalRotationOffset = ResolveLocalRotationOffset(state);

            EnsureArcLengthCache(
                radiusMultiplier,
                orbitLengthMultiplier,
                turnsPerOneWayPass);

            float historyStepPerFollower =
                SmoothHistoryStepPerFollower(ResolveHistoryStepPerFollowerFloat(state));

            float orbitAdherence =
                Mathf.Clamp01(ResolveOrbitAdherence(state));

            ApplyHeadVisualEnabledState(true);
            ApplyFollowerVisualCount(desiredFollowerCount);

            AdvanceCycleProgress(ResolveCycleProgressPerSecond(state));

            float currentDistanceAlongCycle =
                _cachedCycleLength > 0f
                    ? _currentCycleProgress01 * _cachedCycleLength
                    : 0f;

            Vector3 currentHeadLocalPosition =
                EvaluateHeadLocalPositionAtDistance(currentDistanceAlongCycle);

            transform.localPosition = currentHeadLocalPosition;

            RecordParentWorldHistory();

            ApplyHeadVisualPosition(
                currentDistanceAlongCycle,
                orbitAdherence);

            ApplyFollowerPositions(
                desiredFollowerCount,
                historyStepPerFollower,
                currentDistanceAlongCycle,
                orbitAdherence);
        }

        private void AdvanceCycleProgress(float cycleProgressPerSecond)
        {
            if (!_hasCurrentCycleProgress01)
            {
                _currentCycleProgress01 = 0f;
                _hasCurrentCycleProgress01 = true;
                return;
            }

            _currentCycleProgress01 = Mathf.Repeat(
                _currentCycleProgress01 + (cycleProgressPerSecond * Time.deltaTime),
                1f);
        }

        private float SmoothHistoryStepPerFollower(float targetHistoryStepPerFollower)
        {
            if (!_hasCurrentHistoryStepPerFollower)
            {
                _currentHistoryStepPerFollower = targetHistoryStepPerFollower;
                _hasCurrentHistoryStepPerFollower = true;
                return _currentHistoryStepPerFollower;
            }

            float smoothingSpeed =
                targetHistoryStepPerFollower > _currentHistoryStepPerFollower ? 4f : 10f;

            _currentHistoryStepPerFollower = Mathf.Lerp(
                _currentHistoryStepPerFollower,
                targetHistoryStepPerFollower,
                1f - Mathf.Exp(-smoothingSpeed * Time.deltaTime));

            return _currentHistoryStepPerFollower;
        }

        // Visual chain

        private void ResolveVisualChain()
        {
            _followerVisualTransforms.Clear();
            _initialized = false;

            if (!_configuredVisualChain ||
                _headVisualTransform == null ||
                _followerPoolRootTransform == null)
            {
                return;
            }

            foreach (Transform childTransform in _followerPoolRootTransform)
            {
                if (childTransform != null)
                    _followerVisualTransforms.Add(childTransform);
            }

            _initialized = true;
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
            desiredFollowerCount = Mathf.Clamp(
                desiredFollowerCount,
                0,
                MaxOrbitalsVisuals - 1);

            for (int visualIndex = 0; visualIndex < _followerVisualTransforms.Count; visualIndex++)
            {
                Transform followerVisualTransform = _followerVisualTransforms[visualIndex];
                if (followerVisualTransform == null)
                    continue;

                bool shouldBeActive = visualIndex < desiredFollowerCount;
                if (followerVisualTransform.gameObject.activeSelf != shouldBeActive)
                    followerVisualTransform.gameObject.SetActive(shouldBeActive);
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

            ResetOrbitStartPoint();
        }

        // Config-facing resolvers

        private VfxState ResolveState()
        {
            if (_itemData != null && VfxStateIO.IsBound(_itemData))
            {
                if (VfxStateIO.TryRead(_itemData, out var itemState))
                    return itemState;
            }

            return VfxStateIO.FromConfig();
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

        private bool ResolveGlueEnabled(VfxState state)
        {
            if (_orbitalsFamily == NadaOrbitalsFamily.Orbs)
                return false;

            return state.OrbitalsOrbsEnabled &&
                   state.OrbitalsOrbsGlueEnabled &&
                   (_orbitalsFamily == NadaOrbitalsFamily.Flames ||
                    _orbitalsFamily == NadaOrbitalsFamily.Embers);
        }

        private int ResolveDesiredFollowerCount(VfxState state)
        {
            float normalizedCount = _orbitalsFamily switch
            {
                NadaOrbitalsFamily.Orbs => state.OrbitalsOrbsCount,
                NadaOrbitalsFamily.Flames => state.OrbitalsFlamesCount,
                NadaOrbitalsFamily.Embers => state.OrbitalsEmbersCount,
                _ => 0f
            };

            normalizedCount = Mathf.Clamp01(normalizedCount);

            int desiredTotalVisualCount =
                1 + Mathf.RoundToInt(normalizedCount * (MaxOrbitalsVisuals - 1));

            return Mathf.Clamp(
                desiredTotalVisualCount - 1,
                0,
                MaxOrbitalsVisuals - 1);
        }

        private float ResolveHistoryStepPerFollowerFloat(VfxState state)
        {
            bool glueEnabled = ResolveGlueEnabled(state);

            float spacingT = _orbitalsFamily switch
            {
                NadaOrbitalsFamily.Orbs => ResolveOrbsSpacingT(state),
                NadaOrbitalsFamily.Flames => glueEnabled ? ResolveOrbsSpacingT(state) : ClampOrbitalsSpacing(state.OrbitalsFlamesSpacing),
                NadaOrbitalsFamily.Embers => glueEnabled ? ResolveOrbsSpacingT(state) : ClampOrbitalsSpacing(state.OrbitalsEmbersSpacing),
                _ => 0f
            };

            return Mathf.Lerp(
                MinHistoryStepPerFollower,
                MaxHistoryStepPerFollower,
                spacingT);
        }

        private float ResolveOrbsSpacingT(VfxState state)
        {
            if (state.OrbitalsOrbsSnakeEnabled)
                return 0.15f;

            return ClampOrbitalsSpacing(state.OrbitalsOrbsSpacing);
        }

        private static float ClampOrbitalsSpacing(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                return PluginConfig.DefaultOrbitalsSpacing;

            return Mathf.Clamp(
                value,
                PluginConfig.MinOrbitalsSpacing,
                PluginConfig.MaxOrbitalsSpacing);
        }

        private float ResolveRadiusMultiplier(VfxState state)
        {
            bool glueEnabled = ResolveGlueEnabled(state);

            float value = _orbitalsFamily switch
            {
                NadaOrbitalsFamily.Orbs => state.OrbitalsOrbsRadius,
                NadaOrbitalsFamily.Flames => glueEnabled ? state.OrbitalsOrbsRadius : state.OrbitalsFlamesRadius,
                NadaOrbitalsFamily.Embers => glueEnabled ? state.OrbitalsOrbsRadius : state.OrbitalsEmbersRadius,
                _ => PluginConfig.DefaultOrbitalsRadiusMultiplier
            };

            if (float.IsNaN(value) || float.IsInfinity(value))
                return PluginConfig.DefaultOrbitalsRadiusMultiplier;

            return Mathf.Clamp(
                value,
                PluginConfig.MinOrbitalsRadiusMultiplier,
                PluginConfig.MaxOrbitalsRadiusMultiplier);
        }

        private float ResolveOrbitLengthMultiplier(VfxState state)
        {
            bool glueEnabled = ResolveGlueEnabled(state);

            float value = _orbitalsFamily switch
            {
                NadaOrbitalsFamily.Orbs => state.OrbitalsOrbsLength,
                NadaOrbitalsFamily.Flames => glueEnabled ? state.OrbitalsOrbsLength : state.OrbitalsFlamesLength,
                NadaOrbitalsFamily.Embers => glueEnabled ? state.OrbitalsOrbsLength : state.OrbitalsEmbersLength,
                _ => NadaOrbitalsPath.DefaultOrbitLengthMultiplier
            };

            if (float.IsNaN(value) || float.IsInfinity(value))
                return NadaOrbitalsPath.DefaultOrbitLengthMultiplier;

            return Mathf.Clamp(
                value,
                PluginConfig.MinOrbitalsLength,
                PluginConfig.MaxOrbitalsLength);
        }

        private float ResolveTurnsPerOneWayPass(VfxState state)
        {
            bool glueEnabled = ResolveGlueEnabled(state);

            float value = _orbitalsFamily switch
            {
                NadaOrbitalsFamily.Orbs => state.OrbitalsOrbsCycles,
                NadaOrbitalsFamily.Flames => glueEnabled ? state.OrbitalsOrbsCycles : state.OrbitalsFlamesCycles,
                NadaOrbitalsFamily.Embers => glueEnabled ? state.OrbitalsOrbsCycles : state.OrbitalsEmbersCycles,
                _ => NadaOrbitalsPath.DefaultTurnsPerOneWayPass
            };

            if (float.IsNaN(value) || float.IsInfinity(value))
                return NadaOrbitalsPath.DefaultTurnsPerOneWayPass;

            return Mathf.Clamp(
                value,
                PluginConfig.MinOrbitalsCycles,
                PluginConfig.MaxOrbitalsCycles);
        }

        private float ResolveCycleProgressPerSecond(VfxState state)
        {
            bool glueEnabled = ResolveGlueEnabled(state);

            float value = _orbitalsFamily switch
            {
                NadaOrbitalsFamily.Orbs => state.OrbitalsOrbsSpeed,
                NadaOrbitalsFamily.Flames => glueEnabled ? state.OrbitalsOrbsSpeed : state.OrbitalsFlamesSpeed,
                NadaOrbitalsFamily.Embers => glueEnabled ? state.OrbitalsOrbsSpeed : state.OrbitalsEmbersSpeed,
                _ => DefaultCycleProgressPerSecond
            };

            if (float.IsNaN(value) || float.IsInfinity(value))
                return DefaultCycleProgressPerSecond;

            return Mathf.Clamp(
                value,
                PluginConfig.MinOrbitalsSpeed,
                PluginConfig.MaxOrbitalsSpeed);
        }

        private float ResolveOrbitAdherence(VfxState state)
        {
            bool glueEnabled = ResolveGlueEnabled(state);

            return _orbitalsFamily switch
            {
                NadaOrbitalsFamily.Orbs => NadaMotionTuningResolver.GetOrbsOrbitAdherence(_itemData),
                NadaOrbitalsFamily.Flames => glueEnabled
                    ? NadaMotionTuningResolver.GetOrbsOrbitAdherence(_itemData)
                    : NadaMotionTuningResolver.GetFlamesOrbitAdherence(_itemData),
                NadaOrbitalsFamily.Embers => glueEnabled
                    ? NadaMotionTuningResolver.GetOrbsOrbitAdherence(_itemData)
                    : NadaMotionTuningResolver.GetEmbersOrbitAdherence(_itemData),
                _ => 1f
            };
        }

        // Path cache / evaluation

        private void EnsureArcLengthCache(
            float radiusMultiplier,
            float orbitLengthMultiplier,
            float turnsPerOneWayPass)
        {
            if (_hasArcLengthCache &&
                Mathf.Approximately(_cachedRadiusMultiplier, radiusMultiplier) &&
                Mathf.Approximately(_cachedOrbitLengthMultiplier, orbitLengthMultiplier) &&
                Mathf.Approximately(_cachedTurnsPerOneWayPass, turnsPerOneWayPass))
            {
                return;
            }

            _cachedCycleLength = NadaOrbitalsPath.BuildArcLengthTable(
                radiusMultiplier,
                orbitLengthMultiplier,
                turnsPerOneWayPass,
                ArcLengthSampleCount,
                _sampledLocalPositions,
                _sampledCumulativeLengths);

            _cachedRadiusMultiplier = radiusMultiplier;
            _cachedOrbitLengthMultiplier = orbitLengthMultiplier;
            _cachedTurnsPerOneWayPass = turnsPerOneWayPass;
            _hasArcLengthCache = _cachedCycleLength > 0f;
        }

        private Vector3 EvaluateHeadLocalPositionAtDistance(float distanceAlongCycle)
        {
            Vector3 basePosition =
                _baseHeadLocalPosition + _currentLocalOffset;

            if (!_hasArcLengthCache ||
                _sampledLocalPositions.Count == 0 ||
                _sampledCumulativeLengths.Count == 0 ||
                _cachedCycleLength <= 0f)
            {
                return basePosition;
            }

            float wrappedDistance = Mathf.Repeat(distanceAlongCycle, _cachedCycleLength);

            int upperIndex = _sampledCumulativeLengths.BinarySearch(wrappedDistance);
            if (upperIndex < 0)
                upperIndex = ~upperIndex;

            if (upperIndex <= 0)
                return basePosition + (_currentLocalRotationOffset * _sampledLocalPositions[0]);

            if (upperIndex >= _sampledCumulativeLengths.Count)
                return basePosition + (_currentLocalRotationOffset * _sampledLocalPositions[_sampledLocalPositions.Count - 1]);

            int lowerIndex = upperIndex - 1;

            float lowerDistance = _sampledCumulativeLengths[lowerIndex];
            float upperDistance = _sampledCumulativeLengths[upperIndex];

            float interpolationT = Mathf.Approximately(lowerDistance, upperDistance)
                ? 0f
                : Mathf.InverseLerp(lowerDistance, upperDistance, wrappedDistance);

            Vector3 localPosition = Vector3.Lerp(
                _sampledLocalPositions[lowerIndex],
                _sampledLocalPositions[upperIndex],
                interpolationT);

            return basePosition + (_currentLocalRotationOffset * localPosition);
        }

        private float EvaluateFollowerDistanceOffset(
            int followerIndex,
            float historyStepPerFollower)
        {
            float spacingT = Mathf.InverseLerp(
                MinHistoryStepPerFollower,
                MaxHistoryStepPerFollower,
                historyStepPerFollower);

            const float minDistancePerFollower = 0.06f;
            const float maxDistancePerFollower = 0.32f;

            float distancePerFollower = Mathf.Lerp(
                minDistancePerFollower,
                maxDistancePerFollower,
                spacingT);

            return followerIndex * distancePerFollower;
        }

        private Vector3 EvaluateLockedFollowerWorldPosition(
            int followerIndex,
            float historyStepPerFollower,
            float currentDistanceAlongCycle)
        {
            float distanceOffset =
                EvaluateFollowerDistanceOffset(followerIndex, historyStepPerFollower);

            return EvaluateHeadWorldPositionAtDistance(currentDistanceAlongCycle - distanceOffset);
        }

        // World placement / drift

        private void ApplyHeadVisualPosition(
            float currentDistanceAlongCycle,
            float orbitAdherence)
        {
            if (_headVisualTransform == null)
                return;

            Vector3 lockedHeadWorldPosition = transform.position;

            if (orbitAdherence >= HardLockAdherenceThreshold)
            {
                _headVisualTransform.position = lockedHeadWorldPosition;
                _headVisualTransform.rotation = Quaternion.identity;
                return;
            }

            const float headHistorySampleIndex = 1f;

            Vector3 driftedHeadWorldPosition =
                EvaluateDriftedWorldPosition(
                    currentDistanceAlongCycle,
                    headHistorySampleIndex);

            _headVisualTransform.position = Vector3.Lerp(
                driftedHeadWorldPosition,
                lockedHeadWorldPosition,
                orbitAdherence);

            _headVisualTransform.rotation = Quaternion.identity;
        }

        private void ApplyFollowerPositions(
            int desiredFollowerCount,
            float historyStepPerFollower,
            float currentDistanceAlongCycle,
            float orbitAdherence)
        {
            desiredFollowerCount = Mathf.Clamp(
                desiredFollowerCount,
                0,
                MaxOrbitalsVisuals - 1);

            int availableFollowerCount = Mathf.Min(
                desiredFollowerCount,
                _followerVisualTransforms.Count);

            for (int visibleIndex = 0; visibleIndex < availableFollowerCount; visibleIndex++)
            {
                Transform followerVisualTransform = _followerVisualTransforms[visibleIndex];
                if (followerVisualTransform == null)
                    continue;

                int followerIndex = visibleIndex + 1;

                Vector3 lockedWorldPosition =
                    EvaluateLockedFollowerWorldPosition(
                        followerIndex,
                        historyStepPerFollower,
                        currentDistanceAlongCycle);

                if (orbitAdherence >= HardLockAdherenceThreshold)
                {
                    followerVisualTransform.position = lockedWorldPosition;
                    followerVisualTransform.rotation = Quaternion.identity;
                    continue;
                }

                float historySampleIndex = followerIndex * historyStepPerFollower;

                float followerDistanceOffset =
                    EvaluateFollowerDistanceOffset(followerIndex, historyStepPerFollower);

                float followerDistanceAlongCycle =
                    currentDistanceAlongCycle - followerDistanceOffset;

                Vector3 driftedWorldPosition =
                    EvaluateDriftedWorldPosition(
                        followerDistanceAlongCycle,
                        historySampleIndex);

                followerVisualTransform.position = Vector3.Lerp(
                    driftedWorldPosition,
                    lockedWorldPosition,
                    orbitAdherence);

                followerVisualTransform.rotation = Quaternion.identity;
            }
        }

        internal void ResetOrbitStartPoint()
        {
            _currentCycleProgress01 = 0f;
            _hasCurrentCycleProgress01 = false;

            _currentHistoryStepPerFollower = 0f;
            _hasCurrentHistoryStepPerFollower = false;

            _parentWorldPositionHistorySamples.Clear();
            _parentWorldRotationHistorySamples.Clear();
        }

        private void RecordParentWorldHistory()
        {
            Transform parentTransform = transform.parent;
            if (parentTransform == null)
                return;

            _parentWorldPositionHistorySamples.Insert(0, parentTransform.position);
            _parentWorldRotationHistorySamples.Insert(0, parentTransform.rotation);

            int maxHistorySamples =
                ((MaxOrbitalsVisuals - 1) * MaxHistoryStepPerFollower) + ExtraHistoryPadding;

            if (_parentWorldPositionHistorySamples.Count > maxHistorySamples)
            {
                _parentWorldPositionHistorySamples.RemoveRange(
                    maxHistorySamples,
                    _parentWorldPositionHistorySamples.Count - maxHistorySamples);
            }

            if (_parentWorldRotationHistorySamples.Count > maxHistorySamples)
            {
                _parentWorldRotationHistorySamples.RemoveRange(
                    maxHistorySamples,
                    _parentWorldRotationHistorySamples.Count - maxHistorySamples);
            }
        }

        private Vector3 EvaluateHeadWorldPositionAtDistance(float distanceAlongCycle)
        {
            Vector3 localPosition = EvaluateHeadLocalPositionAtDistance(distanceAlongCycle);

            Transform parentTransform = transform.parent;
            if (parentTransform != null)
                return parentTransform.TransformPoint(localPosition);

            return localPosition;
        }

        private Vector3 EvaluateDriftedWorldPosition(
            float distanceAlongCycle,
            float historySampleIndex)
        {
            Vector3 localPosition = EvaluateHeadLocalPositionAtDistance(distanceAlongCycle);

            Transform parentTransform = transform.parent;
            if (parentTransform == null)
                return localPosition;

            Vector3 sampledParentPosition =
                SampleParentWorldPosition(historySampleIndex, parentTransform.position);

            Quaternion sampledParentRotation =
                SampleParentWorldRotation(historySampleIndex, parentTransform.rotation);

            return sampledParentPosition + (sampledParentRotation * localPosition);
        }

        private Vector3 SampleParentWorldPosition(float sampleIndex, Vector3 fallbackPosition)
        {
            if (_parentWorldPositionHistorySamples.Count == 0)
                return fallbackPosition;

            if (sampleIndex <= 0f)
                return _parentWorldPositionHistorySamples[0];

            int lowerIndex = Mathf.FloorToInt(sampleIndex);
            int upperIndex = Mathf.CeilToInt(sampleIndex);

            if (lowerIndex >= _parentWorldPositionHistorySamples.Count)
                return _parentWorldPositionHistorySamples[_parentWorldPositionHistorySamples.Count - 1];

            if (upperIndex >= _parentWorldPositionHistorySamples.Count)
                return _parentWorldPositionHistorySamples[_parentWorldPositionHistorySamples.Count - 1];

            if (lowerIndex == upperIndex)
                return _parentWorldPositionHistorySamples[lowerIndex];

            float interpolationT = sampleIndex - lowerIndex;

            return Vector3.Lerp(
                _parentWorldPositionHistorySamples[lowerIndex],
                _parentWorldPositionHistorySamples[upperIndex],
                interpolationT);
        }

        private Quaternion SampleParentWorldRotation(float sampleIndex, Quaternion fallbackRotation)
        {
            if (_parentWorldRotationHistorySamples.Count == 0)
                return fallbackRotation;

            if (sampleIndex <= 0f)
                return _parentWorldRotationHistorySamples[0];

            int lowerIndex = Mathf.FloorToInt(sampleIndex);
            int upperIndex = Mathf.CeilToInt(sampleIndex);

            if (lowerIndex >= _parentWorldRotationHistorySamples.Count)
                return _parentWorldRotationHistorySamples[_parentWorldRotationHistorySamples.Count - 1];

            if (upperIndex >= _parentWorldRotationHistorySamples.Count)
                return _parentWorldRotationHistorySamples[_parentWorldRotationHistorySamples.Count - 1];

            if (lowerIndex == upperIndex)
                return _parentWorldRotationHistorySamples[lowerIndex];

            float interpolationT = sampleIndex - lowerIndex;

            return Quaternion.Slerp(
                _parentWorldRotationHistorySamples[lowerIndex],
                _parentWorldRotationHistorySamples[upperIndex],
                interpolationT);
        }

        private Vector3 ResolveLocalOffset(VfxState state)
        {
            bool glueEnabled = ResolveGlueEnabled(state);

            float x = _orbitalsFamily switch
            {
                NadaOrbitalsFamily.Orbs => state.OrbitalsOrbsXOffset,
                NadaOrbitalsFamily.Flames => glueEnabled ? state.OrbitalsOrbsXOffset : state.OrbitalsFlamesXOffset,
                NadaOrbitalsFamily.Embers => glueEnabled ? state.OrbitalsOrbsXOffset : state.OrbitalsEmbersXOffset,
                _ => PluginConfig.DefaultEffectOffset
            };

            float y = _orbitalsFamily switch
            {
                NadaOrbitalsFamily.Orbs => state.OrbitalsOrbsYOffset,
                NadaOrbitalsFamily.Flames => glueEnabled ? state.OrbitalsOrbsYOffset : state.OrbitalsFlamesYOffset,
                NadaOrbitalsFamily.Embers => glueEnabled ? state.OrbitalsOrbsYOffset : state.OrbitalsEmbersYOffset,
                _ => PluginConfig.DefaultEffectOffset
            };

            float z = _orbitalsFamily switch
            {
                NadaOrbitalsFamily.Orbs => state.OrbitalsOrbsZOffset,
                NadaOrbitalsFamily.Flames => glueEnabled ? state.OrbitalsOrbsZOffset : state.OrbitalsFlamesZOffset,
                NadaOrbitalsFamily.Embers => glueEnabled ? state.OrbitalsOrbsZOffset : state.OrbitalsEmbersZOffset,
                _ => PluginConfig.DefaultEffectOffset
            };

            return new Vector3(
                ClampOffset(x),
                ClampOffset(y),
                ClampOffset(z));
        }

        private Quaternion ResolveLocalRotationOffset(VfxState state)
        {
            bool glueEnabled = ResolveGlueEnabled(state);

            float x = _orbitalsFamily switch
            {
                NadaOrbitalsFamily.Orbs => state.OrbitalsOrbsXRotation,
                NadaOrbitalsFamily.Flames => glueEnabled ? state.OrbitalsOrbsXRotation : state.OrbitalsFlamesXRotation,
                NadaOrbitalsFamily.Embers => glueEnabled ? state.OrbitalsOrbsXRotation : state.OrbitalsEmbersXRotation,
                _ => PluginConfig.DefaultEffectRotation
            };

            float y = _orbitalsFamily switch
            {
                NadaOrbitalsFamily.Orbs => state.OrbitalsOrbsYRotation,
                NadaOrbitalsFamily.Flames => glueEnabled ? state.OrbitalsOrbsYRotation : state.OrbitalsFlamesYRotation,
                NadaOrbitalsFamily.Embers => glueEnabled ? state.OrbitalsOrbsYRotation : state.OrbitalsEmbersYRotation,
                _ => PluginConfig.DefaultEffectRotation
            };

            float z = _orbitalsFamily switch
            {
                NadaOrbitalsFamily.Orbs => state.OrbitalsOrbsZRotation,
                NadaOrbitalsFamily.Flames => glueEnabled ? state.OrbitalsOrbsZRotation : state.OrbitalsFlamesZRotation,
                NadaOrbitalsFamily.Embers => glueEnabled ? state.OrbitalsOrbsZRotation : state.OrbitalsEmbersZRotation,
                _ => PluginConfig.DefaultEffectRotation
            };

            return Quaternion.Euler(
                ClampRotation(x),
                ClampRotation(y),
                ClampRotation(z));
        }

        private static float ClampOffset(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                return PluginConfig.DefaultEffectOffset;

            return Mathf.Clamp(
                value,
                PluginConfig.MinEffectOffset,
                PluginConfig.MaxEffectOffset);
        }

        private static float ClampRotation(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                return PluginConfig.DefaultEffectRotation;

            return Mathf.Clamp(
                value,
                PluginConfig.MinEffectRotation,
                PluginConfig.MaxEffectRotation);
        }
    }
}