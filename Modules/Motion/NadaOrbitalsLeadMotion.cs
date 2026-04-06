using NADA.VFX.Core.State;
using UnityEngine;

namespace NADA.VFX.Modules.Motion
{
    internal sealed class NadaOrbitalsLeadMotion : MonoBehaviour
    {
        private const float DefaultTravelLength = 2.0f;
        private const float DefaultSpiralRadius = 0.35f;
        private const float DefaultBladeCenterOffset = 0.85f;

        private const float DefaultOneWayPassDuration = 5.0f;
        private const float DefaultTurnDuration = 0.80f;
        private const float DefaultTurnsPerOneWayPass = 2.0f;

        private const float DefaultRadiusMultiplier = 1.0f;
        private const float MinRadiusMultiplier = 0.5f;
        private const float MaxRadiusMultiplier = 1.5f;

        private global::ItemDrop.ItemData _itemData;
        private OrbitalsVisualKind _visualKind = OrbitalsVisualKind.Flames;

        private Vector3 _baseLocalPosition;
        private Quaternion _baseLocalRotation = Quaternion.identity;
        private bool _initialized;

        internal bool IsInitialized => _initialized;

        internal void Initialize(
            OrbitalsVisualKind visualKind,
            Vector3 baseLocalPosition,
            Quaternion baseLocalRotation,
            global::ItemDrop.ItemData itemData)
        {
            if (!_initialized)
            {
                _baseLocalPosition = baseLocalPosition;
                _baseLocalRotation = baseLocalRotation;
                _initialized = true;
            }

            _visualKind = visualKind;
            _itemData = itemData;
        }

        internal void UpdateKindAndItemData(
            OrbitalsVisualKind visualKind,
            global::ItemDrop.ItemData itemData)
        {
            _visualKind = visualKind;
            _itemData = itemData;
        }

        private void LateUpdate()
        {
            if (!_initialized)
                return;

            VfxState state = ResolveState();
            float radiusMultiplier = ResolveLeadRadiusMultiplier(state);

            transform.localPosition = EvaluateLeadLocalPosition(Time.time, radiusMultiplier);
            transform.localRotation = _baseLocalRotation;
        }

        private Vector3 EvaluateLeadLocalPosition(float timeValue, float radiusMultiplier)
        {
            float halfTravelLength = DefaultTravelLength * 0.5f;
            float spiralRadius = DefaultSpiralRadius * radiusMultiplier;

            float upwardPassDuration = DefaultOneWayPassDuration;
            float topTurnDuration = DefaultTurnDuration;
            float downwardPassDuration = DefaultOneWayPassDuration;
            float bottomTurnDuration = DefaultTurnDuration;

            float cycleDuration =
                upwardPassDuration +
                topTurnDuration +
                downwardPassDuration +
                bottomTurnDuration;

            float cycleTime = Mathf.Repeat(timeValue, cycleDuration);

            Vector3 localOffset;

            if (cycleTime < upwardPassDuration)
            {
                float normalizedSegmentTime = cycleTime / upwardPassDuration;
                float y = Mathf.Lerp(-halfTravelLength, halfTravelLength, normalizedSegmentTime) + DefaultBladeCenterOffset;

                float angle = normalizedSegmentTime * DefaultTurnsPerOneWayPass * Mathf.PI * 2f;
                float x = Mathf.Cos(angle) * spiralRadius;
                float z = Mathf.Sin(angle) * spiralRadius;

                localOffset = new Vector3(x, y, z);
            }
            else if (cycleTime < upwardPassDuration + topTurnDuration)
            {
                float normalizedSegmentTime = (cycleTime - upwardPassDuration) / topTurnDuration;

                float angle =
                    (DefaultTurnsPerOneWayPass * Mathf.PI * 2f) +
                    (normalizedSegmentTime * Mathf.PI);

                float x = Mathf.Cos(angle) * spiralRadius;
                float z = Mathf.Sin(angle) * spiralRadius;
                float y = halfTravelLength + DefaultBladeCenterOffset;

                localOffset = new Vector3(x, y, z);
            }
            else if (cycleTime < upwardPassDuration + topTurnDuration + downwardPassDuration)
            {
                float normalizedSegmentTime =
                    (cycleTime - upwardPassDuration - topTurnDuration) / downwardPassDuration;

                float y = Mathf.Lerp(halfTravelLength, -halfTravelLength, normalizedSegmentTime) + DefaultBladeCenterOffset;

                float angle = Mathf.PI + (normalizedSegmentTime * DefaultTurnsPerOneWayPass * Mathf.PI * 2f);
                float x = Mathf.Cos(angle) * spiralRadius;
                float z = Mathf.Sin(angle) * spiralRadius;

                localOffset = new Vector3(x, y, z);
            }
            else
            {
                float normalizedSegmentTime =
                    (cycleTime - upwardPassDuration - topTurnDuration - downwardPassDuration) / bottomTurnDuration;

                float angle =
                    Mathf.PI +
                    (DefaultTurnsPerOneWayPass * Mathf.PI * 2f) +
                    (normalizedSegmentTime * Mathf.PI);

                float x = Mathf.Cos(angle) * spiralRadius;
                float z = Mathf.Sin(angle) * spiralRadius;
                float y = -halfTravelLength + DefaultBladeCenterOffset;

                localOffset = new Vector3(x, y, z);
            }

            return _baseLocalPosition + localOffset;
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

        private float ResolveLeadRadiusMultiplier(VfxState state)
        {
            float radiusMultiplier = _visualKind == OrbitalsVisualKind.Embers
                ? state.OrbitalsEmbersRadius
                : state.OrbitalsFlamesRadius;

            if (float.IsNaN(radiusMultiplier) || float.IsInfinity(radiusMultiplier))
                return DefaultRadiusMultiplier;

            return Mathf.Clamp(
                radiusMultiplier,
                MinRadiusMultiplier,
                MaxRadiusMultiplier);
        }
    }
}