using System.Collections.Generic;
using NADA.VFX.Core.Config;
using NADA.VFX.Core.State;
using NADA.VFX.Runtime.Binding;
using UnityEngine;

namespace NADA.VFX.Modules.Motion
{
    internal sealed class NadaOrbitalsOrbsMotion : MonoBehaviour
    {
        private const float DefaultTravelLength = 2.0f;
        private const float DefaultSpiralRadius = 0.35f;
        private const float DefaultBladeCenterOffset = 0.85f;

        private const float MinOrbRadiusMult = 0.50f;
        private const float MaxOrbRadiusMult = 1.50f;
        private const float DefaultOrbRadiusMult = 1.00f;

        private const float DefaultOneWayPassDuration = 5.0f;
        private const float DefaultTurnDuration = 0.80f;
        private const float DefaultTurnsPerOneWayPass = 2.0f;

        private const int MaxOrbVisuals = 20;
        private const int ExtraHistoryPadding = 24;

        private const int MinHistoryStepPerFollower = 2;
        private const int MaxHistoryStepPerFollower = 36;
        
        private float _currentSpacing;
        private bool _hasCurrentSpacing;
        
        private const float FollowerFollowSharpness = 14f;

        private global::ItemDrop.ItemData _itemData;

        private Transform _orbVisual;
        private Vector3 _baseLocalPosition;
        private bool _initialized;

        private readonly List<Transform> _orbVisuals = new();
        private readonly List<Vector3> _headWorldPositionHistory = new();
        private readonly List<Vector3> _orbCurrentWorldPositions = new();
        private readonly List<bool> _orbPositionInitialized = new();

        internal void SetItemData(global::ItemDrop.ItemData itemData)
        {
            _itemData = itemData;
        }

        private void Awake()
        {
            _baseLocalPosition = transform.localPosition;
            ResolveOrbVisual();
            EnsureOrbVisualPool();
        }

        private void LateUpdate()
        {
            ResolveOrbVisual();
            if (!_initialized)
                return;

            EnsureOrbVisualPool();
            EnsureFollowerStatePool();

            VfxState state = ResolveState();

            if (!state.OrbitalsOrbsEnabled)
            {
                if (_orbVisual != null && _orbVisual.gameObject.activeSelf)
                    _orbVisual.gameObject.SetActive(false);

                for (int i = 1; i < _orbVisuals.Count; i++)
                {
                    Transform orb = _orbVisuals[i];
                    if (orb != null && orb.gameObject.activeSelf)
                        orb.gameObject.SetActive(false);
                }

                _headWorldPositionHistory.Clear();
                _hasCurrentSpacing = false;

                for (int i = 0; i < _orbCurrentWorldPositions.Count; i++)
                {
                    _orbCurrentWorldPositions[i] = Vector3.zero;
                    _orbPositionInitialized[i] = false;
                }

                return;
            }

            if (_orbVisual != null && !_orbVisual.gameObject.activeSelf)
                _orbVisual.gameObject.SetActive(true);

            int desiredCount = ResolveDesiredOrbCount(state);
            float radiusMult = ResolveOrbRadiusMultiplier(state);
            float targetSpacing = ResolveHistoryStepPerFollowerFloat(state);

            if (!_hasCurrentSpacing)
            {
                _currentSpacing = targetSpacing;
                _hasCurrentSpacing = true;
            }
            else
            {
                float speed = targetSpacing > _currentSpacing ? 4f : 10f;
                _currentSpacing = Mathf.Lerp(
                    _currentSpacing,
                    targetSpacing,
                    1f - Mathf.Exp(-speed * Time.deltaTime));
            }
            
            float orbitAdherence = Mathf.Clamp01(
                NadaMotionTuningResolver.GetOrbOrbitAdherence(_itemData));

            ApplyOrbVisualCount(desiredCount);

            Vector3 currentHeadLocalPosition = EvaluateHeadLocalPosition(Time.time, radiusMult);
            transform.localPosition = currentHeadLocalPosition;

            RecordHeadWorldHistory(transform.position);
            ApplyOrbFollow(desiredCount, _currentSpacing, radiusMult, orbitAdherence);
        }

        private void ResolveOrbVisual()
        {
            if (_initialized && _orbVisual != null)
                return;

            _orbVisual = FindDirectChild(transform, "Orb_00");
            _initialized = _orbVisual != null;

            if (_initialized)
            {
                _orbVisuals.Clear();
                _orbVisuals.Add(_orbVisual);
            }
        }

        private void EnsureOrbVisualPool()
        {
            if (_orbVisual == null)
                return;

            while (_orbVisuals.Count < MaxOrbVisuals)
            {
                int index = _orbVisuals.Count;

                Transform clone = Instantiate(_orbVisual.gameObject, transform, false).transform;
                clone.name = $"Orb_{index:00}";
                clone.localPosition = Vector3.zero;
                clone.localRotation = Quaternion.identity;
                clone.localScale = Vector3.one;
                clone.gameObject.SetActive(false);

                _orbVisuals.Add(clone);
            }
        }
        
        private void EnsureFollowerStatePool()
        {
            while (_orbCurrentWorldPositions.Count < MaxOrbVisuals)
                _orbCurrentWorldPositions.Add(Vector3.zero);

            while (_orbPositionInitialized.Count < MaxOrbVisuals)
                _orbPositionInitialized.Add(false);
        }

        private static int ResolveDesiredOrbCount(VfxState state)
        {
            float normalized = Mathf.Clamp01(state.OrbitalsOrbsCount);
            return 1 + Mathf.RoundToInt(normalized * (MaxOrbVisuals - 1));
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
                return DefaultOrbRadiusMult;

            return Mathf.Clamp(value, MinOrbRadiusMult, MaxOrbRadiusMult);
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

        private void ApplyOrbVisualCount(int desiredCount)
        {
            desiredCount = Mathf.Clamp(desiredCount, 1, MaxOrbVisuals);

            for (int i = 0; i < _orbVisuals.Count; i++)
            {
                Transform orb = _orbVisuals[i];
                if (orb == null) continue;

                bool shouldBeActive = i < desiredCount;
                if (orb.gameObject.activeSelf != shouldBeActive)
                    orb.gameObject.SetActive(shouldBeActive);

                if (!shouldBeActive && i < _orbPositionInitialized.Count)
                    _orbPositionInitialized[i] = false;
            }
        }
        
        private void ApplyOrbFollow(int desiredCount, float spacing, float radiusMult, float orbitAdherence)
        {
            desiredCount = Mathf.Clamp(desiredCount, 1, MaxOrbVisuals);

            float followT = 1f - Mathf.Exp(-FollowerFollowSharpness * Time.deltaTime);

            for (int visibleIndex = 0; visibleIndex < desiredCount; visibleIndex++)
            {
                Transform orb = _orbVisuals[visibleIndex];
                if (orb == null) continue;

                // The invisible authoritative head is segment 0.
                // The visible chain begins at segment 1 so that the visible head
                // obeys the same spacing/adherence rules as every other visible orb.
                int segmentIndex = visibleIndex + 1;

                Vector3 lockedWorldPosition =
                    EvaluateLockedFollowerWorldPosition(segmentIndex, spacing, radiusMult);

                float historySample = segmentIndex * spacing;

                Vector3 driftingWorldPosition = SampleHistoryPosition(
                    historySample,
                    lockedWorldPosition);

                Vector3 targetWorldPosition = Vector3.Lerp(
                    driftingWorldPosition,
                    lockedWorldPosition,
                    orbitAdherence);

                if (!_orbPositionInitialized[visibleIndex])
                {
                    _orbCurrentWorldPositions[visibleIndex] = targetWorldPosition;
                    _orbPositionInitialized[visibleIndex] = true;
                }
                else
                {
                    _orbCurrentWorldPositions[visibleIndex] = Vector3.Lerp(
                        _orbCurrentWorldPositions[visibleIndex],
                        targetWorldPosition,
                        followT);
                }

                orb.position = _orbCurrentWorldPositions[visibleIndex];
                orb.rotation = Quaternion.identity;
            }

            for (int i = desiredCount; i < _orbVisuals.Count; i++)
            {
                if (i < _orbPositionInitialized.Count)
                    _orbPositionInitialized[i] = false;
            }
        }

        private Vector3 EvaluateHeadLocalPosition(float timeValue, float radiusMult)
        {
            float halfLength = DefaultTravelLength * 0.5f;
            float spiralRadius = DefaultSpiralRadius * radiusMult;

            float upTime = DefaultOneWayPassDuration;
            float turnTime = DefaultTurnDuration;
            float downTime = DefaultOneWayPassDuration;
            float bottomTurnTime = DefaultTurnDuration;

            float cycleDuration = upTime + turnTime + downTime + bottomTurnTime;
            float t = Mathf.Repeat(timeValue, cycleDuration);

            Vector3 localOffset;

            if (t < upTime)
            {
                float u = t / upTime;
                float y = Mathf.Lerp(-halfLength, halfLength, u) + DefaultBladeCenterOffset;

                float angle = u * DefaultTurnsPerOneWayPass * Mathf.PI * 2f;
                float x = Mathf.Cos(angle) * spiralRadius;
                float z = Mathf.Sin(angle) * spiralRadius;

                localOffset = new Vector3(x, y, z);
            }
            else if (t < upTime + turnTime)
            {
                float u = (t - upTime) / turnTime;

                float startAngle = DefaultTurnsPerOneWayPass * Mathf.PI * 2f;
                float angle = startAngle + (u * Mathf.PI);

                float x = Mathf.Cos(angle) * spiralRadius;
                float z = Mathf.Sin(angle) * spiralRadius;
                float y = halfLength + DefaultBladeCenterOffset;

                localOffset = new Vector3(x, y, z);
            }
            else if (t < upTime + turnTime + downTime)
            {
                float u = (t - upTime - turnTime) / downTime;
                float y = Mathf.Lerp(halfLength, -halfLength, u) + DefaultBladeCenterOffset;

                float angle = Mathf.PI + (u * DefaultTurnsPerOneWayPass * Mathf.PI * 2f);
                float x = Mathf.Cos(angle) * spiralRadius;
                float z = Mathf.Sin(angle) * spiralRadius;

                localOffset = new Vector3(x, y, z);
            }
            else
            {
                float u = (t - upTime - turnTime - downTime) / bottomTurnTime;

                float startAngle = Mathf.PI + (DefaultTurnsPerOneWayPass * Mathf.PI * 2f);
                float angle = startAngle + (u * Mathf.PI);

                float x = Mathf.Cos(angle) * spiralRadius;
                float z = Mathf.Sin(angle) * spiralRadius;
                float y = -halfLength + DefaultBladeCenterOffset;

                localOffset = new Vector3(x, y, z);
            }

            return _baseLocalPosition + localOffset;
        }

        private Vector3 EvaluateHeadWorldPosition(float timeValue, float radiusMult)
        {
            Vector3 local = EvaluateHeadLocalPosition(timeValue, radiusMult);

            Transform parentTf = transform.parent;
            if (parentTf != null)
                return parentTf.TransformPoint(local);

            return local;
        }

        private float EvaluateFollowerTimeOffset(int followerIndex, float spacing)
        {
            float cycleDuration =
                DefaultOneWayPassDuration +
                DefaultTurnDuration +
                DefaultOneWayPassDuration +
                DefaultTurnDuration;

            float spacingT = Mathf.InverseLerp(
                MinHistoryStepPerFollower,
                MaxHistoryStepPerFollower,
                spacing);

            // Tunable fraction of total orbit cycle used per follower.
            float minSecondsPerFollower = cycleDuration * 0.01f;
            float maxSecondsPerFollower = cycleDuration * 0.04f;

            float secondsPerFollower = Mathf.Lerp(
                minSecondsPerFollower,
                maxSecondsPerFollower,
                spacingT);

            return followerIndex * secondsPerFollower;
        }

        private Vector3 EvaluateLockedFollowerWorldPosition(int followerIndex, float spacing, float radiusMult)
        {
            float timeOffset = EvaluateFollowerTimeOffset(followerIndex, spacing);
            float sampleTime = Time.time - timeOffset;
            return EvaluateHeadWorldPosition(sampleTime, radiusMult);
        }

        private void RecordHeadWorldHistory(Vector3 headWorldPosition)
        {
            _headWorldPositionHistory.Insert(0, headWorldPosition);

            int maxHistory =
                ((MaxOrbVisuals - 1) * MaxHistoryStepPerFollower) + ExtraHistoryPadding;

            if (_headWorldPositionHistory.Count > maxHistory)
            {
                _headWorldPositionHistory.RemoveRange(
                    maxHistory,
                    _headWorldPositionHistory.Count - maxHistory);
            }
        }
        
        private Vector3 SampleHistoryPosition(float sampleIndex, Vector3 fallback)
        {
            if (_headWorldPositionHistory.Count == 0)
                return fallback;

            if (sampleIndex <= 0f)
                return _headWorldPositionHistory[0];

            int lower = Mathf.FloorToInt(sampleIndex);
            int upper = Mathf.CeilToInt(sampleIndex);

            if (lower >= _headWorldPositionHistory.Count)
                return _headWorldPositionHistory[_headWorldPositionHistory.Count - 1];

            if (upper >= _headWorldPositionHistory.Count)
                return _headWorldPositionHistory[_headWorldPositionHistory.Count - 1];

            if (lower == upper)
                return _headWorldPositionHistory[lower];

            float t = sampleIndex - lower;
            return Vector3.Lerp(
                _headWorldPositionHistory[lower],
                _headWorldPositionHistory[upper],
                t);
        }

        private static Transform FindDirectChild(Transform parent, string name)
        {
            if (parent == null) return null;

            foreach (Transform child in parent)
            {
                if (child != null && child.name == name)
                    return child;
            }

            return null;
        }
    }
}