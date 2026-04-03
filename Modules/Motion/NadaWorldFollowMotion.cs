using UnityEngine;

namespace NADA.VFX.Modules.Motion
{
    internal sealed class NadaWorldFollowMotion : MonoBehaviour
    {
        private Transform _target;
        private Vector3 _localOffset = Vector3.zero;
        private Quaternion _localRotationOffset = Quaternion.identity;

        internal void SetTarget(Transform target)
        {
            _target = target;
        }

        internal void SetOffset(Vector3 localOffset, Quaternion localRotationOffset)
        {
            _localOffset = localOffset;
            _localRotationOffset = localRotationOffset;
        }

        private void LateUpdate()
        {
            if (_target == null)
                return;

            transform.position = _target.TransformPoint(_localOffset);
            transform.rotation = _target.rotation * _localRotationOffset;
        }
    }
}