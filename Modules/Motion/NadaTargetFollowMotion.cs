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
                return;

            ApplyFollowTransform();
        }

        private void ApplyFollowTransform()
        {
            transform.position =
                _targetTransform.TransformPoint(_localPositionOffset);

            transform.rotation =
                _targetTransform.rotation * _localRotationOffset;
        }
    }
}