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

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [RigTransformApply] " +
                $"root='{rigRoot.name}' " +
                $"stateRot={state.RigRotation} " +
                $"stateLen={state.RigLengthPosition} " +
                $"beforePos={rigRoot.localPosition} " +
                $"beforeRot={rigRoot.localEulerAngles}");

            Vector3 basePosition = Plugin.RigLocalPosition;
            Vector3 baseEuler = Plugin.RigLocalEulerAngles;

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

            if (rigRoot.parent != null)
            {
                Vector3 parentScale = rigRoot.parent.lossyScale;

                rigRoot.localScale =
                    new Vector3(
                        SafeDivide(Plugin.RigLocalScale.x, parentScale.x),
                        SafeDivide(Plugin.RigLocalScale.y, parentScale.y),
                        SafeDivide(Plugin.RigLocalScale.z, parentScale.z));
            }
            else
            {
                rigRoot.localScale = Plugin.RigLocalScale;
            }

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [RigTransformApplied] " +
                $"root='{rigRoot.name}' " +
                $"afterPos={rigRoot.localPosition} " +
                $"afterRot={rigRoot.localEulerAngles}");
        }

        private static float SafeDivide(float value, float divisor)
        {
            return Mathf.Abs(divisor) > 0.0001f
                ? value / divisor
                : value;
        }
    }
}