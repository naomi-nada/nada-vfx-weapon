using UnityEngine;
using NADA.VFX.Weapon.Core.State;

namespace NADA.VFX.Weapon.Runtime.Structure
{
    internal static class NadaRigTransformApplier
    {
        public static void Apply(Transform rigRoot, VfxState state)
        {
            if (rigRoot == null)
                return;

            NadaRigAlignmentAnchor anchor =
                rigRoot.GetComponent<NadaRigAlignmentAnchor>();

            Vector3 basePosition = anchor != null
                ? anchor.BaseLocalPosition
                : Plugin.RigLocalPosition;

            Vector3 baseEuler = anchor != null
                ? anchor.BaseLocalEulerAngles
                : Plugin.RigLocalEulerAngles;

            Vector3 baseScale = anchor != null
                ? anchor.BaseLocalScale
                : Plugin.RigLocalScale;

            // Start from the weapon's base alignment, then apply the saved per-item/style offsets on top.
            rigRoot.localPosition =
                basePosition + new Vector3(
                    state.RigXOffset,
                    state.RigYOffset,
                    state.RigZOffset);

            rigRoot.localRotation =
                Quaternion.Euler(baseEuler) *
                Quaternion.Euler(
                    state.RigXRotation,
                    state.RigYRotation,
                    state.RigZRotation);

            rigRoot.localScale = baseScale;
        }
    }
}