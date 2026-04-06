using System.Collections.Generic;
using NADA.VFX.Core.Config;
using NADA.VFX.Core.State;
using NADA.VFX.Runtime.Binding;
using NADA.VFX.Runtime.Structure;
using UnityEngine;

namespace NADA.VFX.Modules.Motion
{
    internal sealed class NadaOrbitalsOrbsMotion : MonoBehaviour
    {
        private const float DefaultTravelLength = 2.0f;
        private const float DefaultSpiralRadius = 0.35f;
        private const float DefaultBladeCenterOffset = 0.85f;

        private const float MinOrbRadiusMultiplier = 0.50f;
        private const float MaxOrbRadiusMultiplier = 1.50f;
        private const float DefaultOrbRadiusMultiplier = 1.00f;

        private const float DefaultOneWayPassDuration = 5.0f;
        private const float DefaultTurnDuration = 0.80f;
        private const float DefaultTurnsPerOneWayPass = 2.0f;

        private const int MaxOrbVisuals = 20;
        private const int ExtraHistoryPadding = 24;

        private const int MinHistoryStepPerFollower = 2;
        private const int MaxHistoryStepPerFollower = 36;

        private const float FollowerFollowSharpness = 14f;

        private global::ItemDrop.ItemData _itemData;

        private Transform _headOrbVisualTransform;
        private Vector3 _baseHeadLocalPosition;
        private bool _initialized;

        private float _currentHistoryStepPerFollower;
        private bool _hasCurrentHistoryStepPerFollower;

        private readonly List<Transform> _orbVisualTransforms = new();
        private readonly List<Vector3> _headWorldPositionHistorySamples = new();
        private readonly List<Vector3> _currentOrbWorldPositions = new();
        private readonly List<bool> _hasInitializedOrbWorldPosition = new();

        internal void SetItemData(global::ItemDrop.ItemData itemData)
        {
            _itemData = itemData;
        }

        private void Awake()
        {
            _baseHeadLocalPosition = transform.localPosition;
            ResolveHeadOrbVisual();
            EnsureOrbVisualTransformsPool();
        }

        private void LateUpdate()
        {
            ResolveHeadOrbVisual();
            if (!_initialized)
                return;

            EnsureOrbVisualTransformsPool();
            EnsureFollowerPositionStateCapacity();

            VfxState state = ResolveState();

            if (!state.OrbitalsOrbsEnabled)
            {
                if (_headOrbVisualTransform != null && _headOrbVisualTransform.gameObject.activeSelf)
                    _headOrbVisualTransform.gameObject.SetActive(false);

                for (int visualIndex = 1; visualIndex < _orbVisualTransforms.Count; visualIndex++)
                {
                    Transform orbVisualTransform = _orbVisualTransforms[visualIndex];
                    if (orbVisualTransform != null && orbVisualTransform.gameObject.activeSelf)
                        orbVisualTransform.gameObject.SetActive(false);
                }

                _headWorldPositionHistorySamples.Clear();
                _hasCurrentHistoryStepPerFollower = false;

                for (int visualIndex = 0; visualIndex < _currentOrbWorldPositions.Count; visualIndex++)
                {
                    _currentOrbWorldPositions[visualIndex] = Vector3.zero;
                    _hasInitializedOrbWorldPosition[visualIndex] = false;
                }

                return;
            }

            if (_headOrbVisualTransform != null && !_headOrbVisualTransform.gameObject.activeSelf)
                _headOrbVisualTransform.gameObject.SetActive(true);

            int desiredOrbCount = ResolveDesiredOrbCount(state);
            float orbRadiusMultiplier = ResolveOrbRadiusMultiplier(state);
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

            float orbitAdherence = Mathf.Clamp01(
                NadaMotionTuningResolver.GetOrbsOrbitAdherence(_itemData));

            ApplyOrbVisualCount(desiredOrbCount);

            Vector3 currentHeadLocalPosition =
                EvaluateHeadLocalPosition(Time.time, orbRadiusMultiplier);

            transform.localPosition = currentHeadLocalPosition;

            RecordHeadWorldHistory(transform.position);

            ApplyFollowerOrbPositions(
                desiredOrbCount,
                _currentHistoryStepPerFollower,
                orbRadiusMultiplier,
                orbitAdherence);
        }

        private void ResolveHeadOrbVisual()
        {
            if (_initialized && _headOrbVisualTransform != null)
                return;

            _headOrbVisualTransform = NadaRigPaths.FindDirectChild(transform, "Orb_00");
            _initialized = _headOrbVisualTransform != null;

            if (_initialized)
            {
                _orbVisualTransforms.Clear();
                _orbVisualTransforms.Add(_headOrbVisualTransform);
            }
        }

        private void EnsureOrbVisualTransformsPool()
        {
            if (_headOrbVisualTransform == null)
                return;

            while (_orbVisualTransforms.Count < MaxOrbVisuals)
            {
                int visualIndex = _orbVisualTransforms.Count;

                Transform clonedOrbVisualTransform =
                    Instantiate(_headOrbVisualTransform.gameObject, transform, false).transform;

                clonedOrbVisualTransform.name = $"Orb_{visualIndex:00}";
                clonedOrbVisualTransform.localPosition = Vector3.zero;
                clonedOrbVisualTransform.localRotation = Quaternion.identity;
                clonedOrbVisualTransform.localScale = Vector3.one;
                clonedOrbVisualTransform.gameObject.SetActive(false);

                _orbVisualTransforms.Add(clonedOrbVisualTransform);
            }
        }

        private void EnsureFollowerPositionStateCapacity()
        {
            while (_currentOrbWorldPositions.Count < MaxOrbVisuals)
                _currentOrbWorldPositions.Add(Vector3.zero);

            while (_hasInitializedOrbWorldPosition.Count < MaxOrbVisuals)
                _hasInitializedOrbWorldPosition.Add(false);
        }

        private static int ResolveDesiredOrbCount(VfxState state)
        {
            float normalizedCount = Mathf.Clamp01(state.OrbitalsOrbsCount);
            return 1 + Mathf.RoundToInt(normalizedCount * (MaxOrbVisuals - 1));
        }

        private static float ResolveHistoryStepPerFollowerFloat(VfxState state)
        {
            float spacingT = Mathf.Clamp(
                state.OrbitalsOrbsSpacing,
                PluginConfig.MinOrbSpacing,
                PluginConfig.MaxOrbSpacing);

            return Mathf.Lerp(
                MinHistoryStepPerFollower,
                MaxHistoryStepPerFollower,
                spacingT);
        }

        private static float ResolveOrbRadiusMultiplier(VfxState state)
        {
            float value = state.OrbitalsOrbsRadius;
            if (float.IsNaN(value) || float.IsInfinity(value))
                return DefaultOrbRadiusMultiplier;

            return Mathf.Clamp(value, MinOrbRadiusMultiplier, MaxOrbRadiusMultiplier);
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

        private void ApplyOrbVisualCount(int desiredOrbCount)
        {
            desiredOrbCount = Mathf.Clamp(desiredOrbCount, 1, MaxOrbVisuals);

            for (int visualIndex = 0; visualIndex < _orbVisualTransforms.Count; visualIndex++)
            {
                Transform orbVisualTransform = _orbVisualTransforms[visualIndex];
                if (orbVisualTransform == null)
                    continue;

                bool shouldBeActive = visualIndex < desiredOrbCount;
                if (orbVisualTransform.gameObject.activeSelf != shouldBeActive)
                    orbVisualTransform.gameObject.SetActive(shouldBeActive);

                if (!shouldBeActive && visualIndex < _hasInitializedOrbWorldPosition.Count)
                    _hasInitializedOrbWorldPosition[visualIndex] = false;
            }
        }

        private void ApplyFollowerOrbPositions(
            int desiredOrbCount,
            float historyStepPerFollower,
            float orbRadiusMultiplier,
            float orbitAdherence)
        {
            desiredOrbCount = Mathf.Clamp(desiredOrbCount, 1, MaxOrbVisuals);

            float followT = 1f - Mathf.Exp(-FollowerFollowSharpness * Time.deltaTime);

            for (int visibleIndex = 0; visibleIndex < desiredOrbCount; visibleIndex++)
            {
                Transform orbVisualTransform = _orbVisualTransforms[visibleIndex];
                if (orbVisualTransform == null)
                    continue;

                int followerIndex = visibleIndex + 1;

                Vector3 lockedWorldPosition =
                    EvaluateLockedFollowerWorldPosition(
                        followerIndex,
                        historyStepPerFollower,
                        orbRadiusMultiplier);

                float historySampleIndex = followerIndex * historyStepPerFollower;

                Vector3 driftingWorldPosition =
                    SampleHistoryPosition(historySampleIndex, lockedWorldPosition);

                Vector3 targetWorldPosition = Vector3.Lerp(
                    driftingWorldPosition,
                    lockedWorldPosition,
                    orbitAdherence);

                if (!_hasInitializedOrbWorldPosition[visibleIndex])
                {
                    _currentOrbWorldPositions[visibleIndex] = targetWorldPosition;
                    _hasInitializedOrbWorldPosition[visibleIndex] = true;
                }
                else
                {
                    _currentOrbWorldPositions[visibleIndex] = Vector3.Lerp(
                        _currentOrbWorldPositions[visibleIndex],
                        targetWorldPosition,
                        followT);
                }

                orbVisualTransform.position = _currentOrbWorldPositions[visibleIndex];
                orbVisualTransform.rotation = Quaternion.identity;
            }

            for (int visualIndex = desiredOrbCount; visualIndex < _orbVisualTransforms.Count; visualIndex++)
            {
                if (visualIndex < _hasInitializedOrbWorldPosition.Count)
                    _hasInitializedOrbWorldPosition[visualIndex] = false;
            }
        }

        private Vector3 EvaluateHeadLocalPosition(float timeValue, float orbRadiusMultiplier)
        {
            float halfLength = DefaultTravelLength * 0.5f;
            float spiralRadius = DefaultSpiralRadius * orbRadiusMultiplier;

            float upTime = DefaultOneWayPassDuration;
            float turnTime = DefaultTurnDuration;
            float downTime = DefaultOneWayPassDuration;
            float bottomTurnTime = DefaultTurnDuration;

            float cycleDuration = upTime + turnTime + downTime + bottomTurnTime;
            float cycleTime = Mathf.Repeat(timeValue, cycleDuration);

            Vector3 localOffset;

            if (cycleTime < upTime)
            {
                float normalizedSegmentTime = cycleTime / upTime;
                float y = Mathf.Lerp(-halfLength, halfLength, normalizedSegmentTime) + DefaultBladeCenterOffset;

                float angle = normalizedSegmentTime * DefaultTurnsPerOneWayPass * Mathf.PI * 2f;
                float x = Mathf.Cos(angle) * spiralRadius;
                float z = Mathf.Sin(angle) * spiralRadius;

                localOffset = new Vector3(x, y, z);
            }
            else if (cycleTime < upTime + turnTime)
            {
                float normalizedSegmentTime = (cycleTime - upTime) / turnTime;

                float startAngle = DefaultTurnsPerOneWayPass * Mathf.PI * 2f;
                float angle = startAngle + (normalizedSegmentTime * Mathf.PI);

                float x = Mathf.Cos(angle) * spiralRadius;
                float z = Mathf.Sin(angle) * spiralRadius;
                float y = halfLength + DefaultBladeCenterOffset;

                localOffset = new Vector3(x, y, z);
            }
            else if (cycleTime < upTime + turnTime + downTime)
            {
                float normalizedSegmentTime = (cycleTime - upTime - turnTime) / downTime;
                float y = Mathf.Lerp(halfLength, -halfLength, normalizedSegmentTime) + DefaultBladeCenterOffset;

                float angle = Mathf.PI + (normalizedSegmentTime * DefaultTurnsPerOneWayPass * Mathf.PI * 2f);
                float x = Mathf.Cos(angle) * spiralRadius;
                float z = Mathf.Sin(angle) * spiralRadius;

                localOffset = new Vector3(x, y, z);
            }
            else
            {
                float normalizedSegmentTime = (cycleTime - upTime - turnTime - downTime) / bottomTurnTime;

                float startAngle = Mathf.PI + (DefaultTurnsPerOneWayPass * Mathf.PI * 2f);
                float angle = startAngle + (normalizedSegmentTime * Mathf.PI);

                float x = Mathf.Cos(angle) * spiralRadius;
                float z = Mathf.Sin(angle) * spiralRadius;
                float y = -halfLength + DefaultBladeCenterOffset;

                localOffset = new Vector3(x, y, z);
            }

            return _baseHeadLocalPosition + localOffset;
        }

        private Vector3 EvaluateHeadWorldPosition(float timeValue, float orbRadiusMultiplier)
        {
            Vector3 localPosition = EvaluateHeadLocalPosition(timeValue, orbRadiusMultiplier);

            Transform parentTransform = transform.parent;
            if (parentTransform != null)
                return parentTransform.TransformPoint(localPosition);

            return localPosition;
        }

        private float EvaluateFollowerTemporalOffset(
            int followerIndex,
            float historyStepPerFollower)
        {
            float cycleDuration =
                DefaultOneWayPassDuration +
                DefaultTurnDuration +
                DefaultOneWayPassDuration +
                DefaultTurnDuration;

            float spacingT = Mathf.InverseLerp(
                MinHistoryStepPerFollower,
                MaxHistoryStepPerFollower,
                historyStepPerFollower);

            float minSecondsPerFollower = cycleDuration * 0.01f;
            float maxSecondsPerFollower = cycleDuration * 0.04f;

            float secondsPerFollower = Mathf.Lerp(
                minSecondsPerFollower,
                maxSecondsPerFollower,
                spacingT);

            return followerIndex * secondsPerFollower;
        }

        private Vector3 EvaluateLockedFollowerWorldPosition(
            int followerIndex,
            float historyStepPerFollower,
            float orbRadiusMultiplier)
        {
            float temporalOffset =
                EvaluateFollowerTemporalOffset(followerIndex, historyStepPerFollower);

            float sampleTime = Time.time - temporalOffset;
            return EvaluateHeadWorldPosition(sampleTime, orbRadiusMultiplier);
        }

        private void RecordHeadWorldHistory(Vector3 headWorldPosition)
        {
            _headWorldPositionHistorySamples.Insert(0, headWorldPosition);

            int maxHistorySamples =
                ((MaxOrbVisuals - 1) * MaxHistoryStepPerFollower) + ExtraHistoryPadding;

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
    }
}