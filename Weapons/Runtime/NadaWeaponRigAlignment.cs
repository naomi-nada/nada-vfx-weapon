using UnityEngine;

namespace NADA.VFX.Weapons.Runtime
{
    internal readonly struct NadaWeaponRigAlignment
    {
        public readonly Vector3 LocalPosition;
        public readonly Vector3 LocalEulerAngles;
        public readonly Vector3 LocalScale;

        public NadaWeaponRigAlignment(
            Vector3 localPosition,
            Vector3 localEulerAngles,
            Vector3 localScale)
        {
            LocalPosition = localPosition;
            LocalEulerAngles = localEulerAngles;
            LocalScale = localScale;
        }
    }

    internal static class NadaWeaponRigAlignmentResolver
    {
        internal static NadaWeaponRigAlignment Resolve(Transform weaponVisualRootTransform)
        {
            // First pass: preserve the existing lava-sword-tuned default.
            return new NadaWeaponRigAlignment(
                Plugin.RigLocalPosition,
                Plugin.RigLocalEulerAngles,
                Plugin.RigLocalScale);
        }
    }
}