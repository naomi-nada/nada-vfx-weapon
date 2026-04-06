using UnityEngine;
using NADA.VFX.Runtime.Structure;

namespace NADA.VFX.Runtime.Binding
{
    internal static class NadaEffectMotionRootAssembly
    {
        internal static Transform EnsureMirageMotionRoot(
            Transform localEffectsRoot,
            Transform sourceVisualTransform)
        {
            return EnsureStandaloneMotionRoot(
                localEffectsRoot,
                sourceVisualTransform,
                "Mirage Anchor");
        }

        internal static Transform EnsureSparksMotionRoot(
            Transform localEffectsRoot,
            Transform sourceVisualTransform)
        {
            return EnsureStandaloneMotionRoot(
                localEffectsRoot,
                sourceVisualTransform,
                "Sparks Anchor");
        }

        private static Transform EnsureStandaloneMotionRoot(
            Transform localEffectsRoot,
            Transform sourceVisualTransform,
            string motionRootName)
        {
            if (localEffectsRoot == null)
                return null;

            Transform motionRootTransform =
                NadaRigTransforms.EnsureChild(localEffectsRoot, motionRootName);

            if (motionRootTransform == null)
                return null;

            if (sourceVisualTransform != null && motionRootTransform.childCount == 0)
            {
                motionRootTransform.localPosition = sourceVisualTransform.localPosition;
                motionRootTransform.localRotation = sourceVisualTransform.localRotation;
                motionRootTransform.localScale = Vector3.one;
            }

            return motionRootTransform;
        }
    }
}