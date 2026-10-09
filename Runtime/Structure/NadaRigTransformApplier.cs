using NADA.VFX.Weapon.Core.State;
using NADA.VFX.Weapon.Core.State.Blocks;
using UnityEngine;

namespace NADA.VFX.Weapon.Runtime.Structure
{
    internal static class NadaRigTransformApplier
    {
        public static void Apply(Transform rigRoot, VfxState state)
        {
            if (rigRoot == null)
                return;

            ApplyOffsets(
                rigRoot,
                state.RigXOffset,
                state.RigYOffset,
                state.RigZOffset,
                state.RigXRotation,
                state.RigYRotation,
                state.RigZRotation);
        }

        // Native bindings and editor previews own this transform. Don't
        // reconstruct it from config or a legacy VfxState placeholder.
        public static void Apply(Transform rigRoot, VfxTransformState transform)
        {
            if (rigRoot == null || transform == null)
                return;

            ApplyOffsets(
                rigRoot,
                transform.XOffset,
                transform.YOffset,
                transform.ZOffset,
                transform.XRotation,
                transform.YRotation,
                transform.ZRotation);
        }

        private static void ApplyOffsets(
            Transform rigRoot,
            float xOffset,
            float yOffset,
            float zOffset,
            float xRotation,
            float yRotation,
            float zRotation)
        {
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

            // Keep the weapon's established alignment and apply only the
            // source-owned offset/rotation on top of it.
            rigRoot.localPosition =
                basePosition + new Vector3(xOffset, yOffset, zOffset);

            rigRoot.localRotation =
                Quaternion.Euler(baseEuler) *
                Quaternion.Euler(xRotation, yRotation, zRotation);

            rigRoot.localScale = baseScale;
        }
    }
}
