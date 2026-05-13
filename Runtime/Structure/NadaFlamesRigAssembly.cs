using UnityEngine;

namespace NADA.VFX.Runtime.Structure
{
    public class NadaFlamesRigAssembly
    {
        internal static Transform EnsureLocalFlameBranch(
            Transform localWeaponRootTransform,
            string ownerNameForLogs)
        {
            if (!NadaRigCache.CacheReady)
                return null;

            if (NadaRigCache.RefRigTemplateInactive == null)
                return null;

            if (localWeaponRootTransform == null)
                return null;

            Transform localEffectsRootTransform =
                NadaRigPaths.FindLocalEffectsRoot(localWeaponRootTransform);

            if (localEffectsRootTransform == null)
                return null;

            Transform outerFlamesTransform =
                NadaRigPaths.FindDirectChild(localEffectsRootTransform, Plugin.OuterFlamesName);

            if (outerFlamesTransform == null)
            {
                var outerFlamesObject =
                    Object.Instantiate(NadaRigCache.RefRigTemplateInactive, localEffectsRootTransform, false);

                outerFlamesObject.name = Plugin.OuterFlamesName;
                outerFlamesObject.SetActive(true);
                outerFlamesTransform = outerFlamesObject.transform;
            }

            NadaRigTransforms.ResetLocalTransform(outerFlamesTransform);

            SplitFlameChildrenIntoEffects(localEffectsRootTransform, outerFlamesTransform);

            NadaRigTransforms.NormalizeParticleSpacesUnder(
                outerFlamesTransform,
                ParticleSystemSimulationSpace.Local);

            Transform flareTransform =
                NadaRigPaths.FindDirectChild(localEffectsRootTransform, Plugin.FlareName);

            if (flareTransform != null)
            {
                NadaRigTransforms.NormalizeParticleSpacesUnder(
                    flareTransform,
                    ParticleSystemSimulationSpace.Local);
            }

            Transform innerFlamesTransform =
                NadaRigPaths.FindDirectChild(localEffectsRootTransform, Plugin.InnerFlamesName);

            if (innerFlamesTransform != null)
            {
                NadaRigTransforms.NormalizeParticleSpacesUnder(
                    innerFlamesTransform,
                    ParticleSystemSimulationSpace.Local);
            }

            return outerFlamesTransform;
        }
        
        private static void SplitFlameChildrenIntoEffects(
            Transform localEffectsRootTransform,
            Transform outerFlamesTransform)
        {
            if (localEffectsRootTransform == null || outerFlamesTransform == null)
                return;

            Transform flareTransform =
                NadaRigPaths.FindDescendantByName(outerFlamesTransform, "flare");

            if (flareTransform != null)
            {
                flareTransform.name = Plugin.FlareName;

                if (flareTransform.parent != localEffectsRootTransform)
                    flareTransform.SetParent(localEffectsRootTransform, true);
            }

            Transform innerFlamesTransform =
                NadaRigPaths.FindDescendantByName(outerFlamesTransform, "fx_Torch_Basic");

            if (innerFlamesTransform != null)
            {
                innerFlamesTransform.name = Plugin.InnerFlamesName;

                if (innerFlamesTransform.parent != localEffectsRootTransform)
                    innerFlamesTransform.SetParent(localEffectsRootTransform, true);
            }
        }
    }
}