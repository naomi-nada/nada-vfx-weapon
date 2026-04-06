using UnityEngine;

namespace NADA.VFX.Modules.Motion
{
    internal sealed class NadaTargetFollowMotion : MonoBehaviour
    {
        private Transform _targetTransform;
        private Vector3 _localPositionOffset = Vector3.zero;
        private Quaternion _localRotationOffset = Quaternion.identity;

        internal void SetTargetTransform(Transform target)
        {
            _targetTransform = target;
        }

        internal void SetLocalOffset(Vector3 localOffset, Quaternion localRotationOffset)
        {
            _localPositionOffset = localOffset;
            _localRotationOffset = localRotationOffset;
        }

        private void LateUpdate()
        {
            if (_targetTransform == null)
                return;

            transform.position = _targetTransform.TransformPoint(_localPositionOffset);
            transform.rotation = _targetTransform.rotation * _localRotationOffset;
        }
    }
}