using UnityEngine;
using NADA.VFX.Core.State;

namespace NADA.VFX.Runtime.Structure
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

            rigRoot.localPosition =
                basePosition + new Vector3(
                    0f,
                    state.RigLengthPosition,
                    state.RigSidePosition);

            rigRoot.localRotation =
                Quaternion.Euler(baseEuler) *
                Quaternion.Euler(
                    state.RigSideRotation,
                    state.RigRotation,
                    0f);

            rigRoot.localScale = baseScale;
        }
    }
}