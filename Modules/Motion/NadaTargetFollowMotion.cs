using UnityEngine;

namespace NADA.VFX.Weapon.Modules.Motion
{
    // Simple runtime follow motion:
    // - Follows a target transform
    // - Applies explicit local position/rotation offsets
    // - Owns transform motion only
    internal sealed class NadaTargetFollowMotion : MonoBehaviour
    {
        private Transform _targetTransform;

        private Vector3 _localPositionOffset = Vector3.zero;
        private Quaternion _localRotationOffset = Quaternion.identity;

        internal void SetTargetTransform(Transform targetTransform)
        {
            _targetTransform = targetTransform;

            // There's nothing for this component to do without a target.
            // SetTargetTransform will wake it back up when one is assigned.
            enabled = targetTransform != null;
        }

        internal void SetLocalOffset(
            Vector3 localPositionOffset,
            Quaternion localRotationOffset)
        {
            _localPositionOffset = localPositionOffset;
            _localRotationOffset = localRotationOffset;
        }

        private void LateUpdate()
        {
            if (_targetTransform == null)
            {
                enabled = false;
                return;
            }

            ApplyFollowTransform();
        }

        private void ApplyFollowTransform()
        {
            Vector3 worldPosition =
                _targetTransform.TransformPoint(
                    _localPositionOffset);

            Quaternion worldRotation =
                _targetTransform.rotation *
                _localRotationOffset;

            transform.SetPositionAndRotation(
                worldPosition,
                worldRotation);
        }
    }
}