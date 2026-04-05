using UnityEngine;
using NADA.VFX.Weapons.Targets;

namespace NADA.VFX.Runtime.Binding
{
    internal static class NadaMotionAnchorAssembly
    {
        internal static Transform EnsureStandaloneMirageMotionRoot(
            Transform parentEffectsRoot,
            Transform sourceVisualTf)
        {
            if (parentEffectsRoot == null) return null;

            Transform anchorTf = NadaRigTransforms.EnsureChild(parentEffectsRoot, "Mirage Anchor");
            if (anchorTf == null) return null;

            if (sourceVisualTf != null && anchorTf.childCount == 0)
            {
                anchorTf.localPosition = sourceVisualTf.localPosition;
                anchorTf.localRotation = sourceVisualTf.localRotation;
                anchorTf.localScale = Vector3.one;
            }

            return anchorTf;
        }
        
        internal static Transform EnsureStandaloneSparksMotionRoot(
            Transform parentEffectsRoot,
            Transform sourceVisualTf)
        {
            if (parentEffectsRoot == null) return null;

            Transform anchorTf = NadaRigTransforms.EnsureChild(parentEffectsRoot, "Sparks Anchor");
            if (anchorTf == null) return null;

            if (sourceVisualTf != null && anchorTf.childCount == 0)
            {
                anchorTf.localPosition = sourceVisualTf.localPosition;
                anchorTf.localRotation = sourceVisualTf.localRotation;
                anchorTf.localScale = Vector3.one;
            }

            return anchorTf;
        }
    }
}