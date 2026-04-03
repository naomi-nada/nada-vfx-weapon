using NADA.VFX.Core.State;
using UnityEngine;

namespace NADA.VFX.Modules.Motion
{
    internal enum OrbitalsVisualKind
    {
        Flames,
        Embers
    }

    internal sealed class NadaOrbitalsLeadMotion : MonoBehaviour
    {
        private const float DefaultTravelLength = 2.0f;
        private const float DefaultSpiralRadius = 0.35f;
        private const float DefaultBladeCenterOffset = 0.85f;

        private const float DefaultOneWayPassDuration = 5.0f;
        private const float DefaultTurnDuration = 0.80f;
        private const float DefaultTurnsPerOneWayPass = 2.0f;

        private const float DefaultRadius = 1.0f;
        private const float MinRadius = 0.5f;
        private const float MaxRadius = 1.5f;

        private global::ItemDrop.ItemData _itemData;
        private OrbitalsVisualKind _kind = OrbitalsVisualKind.Flames;

        private Vector3 _baseLocalPosition;
        private Quaternion _baseLocalRotation = Quaternion.identity;
        private bool _initialized;

        internal bool IsInitialized => _initialized;

        internal void Initialize(
            OrbitalsVisualKind kind,
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

            _kind = kind;
            _itemData = itemData;
        }

        internal void SetKindAndItemData(OrbitalsVisualKind kind, global::ItemDrop.ItemData itemData)
        {
            _kind = kind;
            _itemData = itemData;
        }

        private void LateUpdate()
        {
            if (!_initialized)
                return;

            VfxState state = ResolveState();
            float radius = ResolveRadius(state);

            transform.localPosition = EvaluateLocalPosition(Time.time, radius);
            transform.localRotation = _baseLocalRotation;
        }

        private Vector3 EvaluateLocalPosition(float timeValue, float radiusMult)
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

                float angle = (DefaultTurnsPerOneWayPass * Mathf.PI * 2f) + (u * Mathf.PI);
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

                float angle = Mathf.PI + (DefaultTurnsPerOneWayPass * Mathf.PI * 2f) + (u * Mathf.PI);
                float x = Mathf.Cos(angle) * spiralRadius;
                float z = Mathf.Sin(angle) * spiralRadius;
                float y = -halfLength + DefaultBladeCenterOffset;

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

        private float ResolveRadius(VfxState state)
        {
            float value = _kind == OrbitalsVisualKind.Embers
                ? state.OrbitalsEmbersRadius
                : state.OrbitalsFlamesRadius;

            if (float.IsNaN(value) || float.IsInfinity(value))
                return DefaultRadius;

            return Mathf.Clamp(value, MinRadius, MaxRadius);
        }
    }
}