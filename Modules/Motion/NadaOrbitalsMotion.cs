using UnityEngine;

namespace NADA.VFX.Modules.Motion
{
    internal sealed class NadaOrbitalsMotion : MonoBehaviour
    {
        private Transform _target;

        internal void SetTarget(Transform target)
        {
            _target = target;
        }

        private void LateUpdate()
        {
            if (_target == null)
                return;

            transform.position = _target.position;
            transform.rotation = _target.rotation;
        }
    }
}