using UnityEngine;

namespace NADA.VFX.Runtime.Structure

{
    public class NadaSparksRigAssembly
    {
    
        internal static Transform EnsureLocalSparksBranch(
            Transform localWeaponRootTransform,
            string ownerNameForLogs)
        {
            if (!NadaRigCache.CacheReady)
                return null;

            if (NadaRigCache.SparksTemplateInactive == null)
                return null;

            if (localWeaponRootTransform == null)
                return null;

            Transform localEffectsRootTransform =
                NadaRigPaths.FindLocalEffectsRoot(localWeaponRootTransform);

            if (localEffectsRootTransform == null)
                return null;

            Transform sparksRootTransform =
                NadaRigPaths.FindDirectChild(localEffectsRootTransform, Plugin.SparksName);

            if (sparksRootTransform == null)
            {
                sparksRootTransform =
                    NadaRigTransforms.EnsureChild(localEffectsRootTransform, Plugin.SparksName);

                for (int i = 0; i < 8; i++)
                {
                    GameObject sparkObject =
                        Object.Instantiate(
                            NadaRigCache.SparksTemplateInactive,
                            sparksRootTransform,
                            false);

                    sparkObject.name = $"Spark_{i:00}";
                    sparkObject.SetActive(true);

                    sparkObject.transform.localPosition = Vector3.zero;
                    sparkObject.transform.localScale = Vector3.one;
                }
            }

            sparksRootTransform.localPosition = Vector3.zero;
            sparksRootTransform.localRotation = Quaternion.identity;
            sparksRootTransform.localScale = Vector3.one;

            NadaRigTransforms.NormalizeParticleSpacesUnder(
                sparksRootTransform,
                ParticleSystemSimulationSpace.Local);

            return sparksRootTransform;
        }    
    }
}