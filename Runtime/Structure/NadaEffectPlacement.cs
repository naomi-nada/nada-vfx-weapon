using UnityEngine;

namespace NADA.VFX.Runtime.Structure
{
    internal static class NadaEffectPlacement
    {
        internal static void ApplyLocalPlacement(
            Transform target,
            Vector3 baseLocalPosition,
            Quaternion baseLocalRotation,
            float xOffset,
            float yOffset,
            float zOffset,
            float xRotationDegrees = 0f,
            float yRotationDegrees = 0f,
            float zRotationDegrees = 0f)
        {
            if (target == null)
                return;

            target.localPosition =
                baseLocalPosition + new Vector3(xOffset, yOffset, zOffset);

            target.localRotation =
                baseLocalRotation *
                Quaternion.Euler(
                    xRotationDegrees,
                    yRotationDegrees,
                    zRotationDegrees);
        }
    }
}