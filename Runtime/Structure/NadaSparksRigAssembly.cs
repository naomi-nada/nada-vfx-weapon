using UnityEngine;

namespace NADA.VFX.Weapon.Runtime.Structure
{
    public class NadaSparksRigAssembly
    {
        internal static Transform EnsureLocalSparksBranch(
            Transform localWeaponRootTransform,
            string ownerNameForLogs)
        {
            if (localWeaponRootTransform == null)
                return null;

            Transform localEffectsRootTransform =
                NadaRigPaths.FindLocalEffectsRoot(
                    localWeaponRootTransform);

            if (localEffectsRootTransform == null)
                return null;

            return EnsureSparksBranch(
                localEffectsRootTransform,
                ownerNameForLogs);
        }

        internal static Transform EnsureSparksBranch(
            Transform parentTransform,
            string ownerNameForLogs)
        {
            if (!NadaRigCache.CacheReady)
                return null;

            if (NadaRigCache.SparksTemplateInactive == null)
                return null;

            if (parentTransform == null)
                return null;

            bool createdSparksRoot = false;

            Transform sparksRootTransform =
                NadaRigPaths.FindDirectChild(
                    parentTransform,
                    Plugin.SparksName);

            if (sparksRootTransform == null)
            {
                sparksRootTransform =
                    NadaRigTransforms.EnsureChild(
                        parentTransform,
                        Plugin.SparksName);

                createdSparksRoot = true;

                for (int i = 0; i < 8; i++)
                {
                    GameObject sparkObject =
                        Object.Instantiate(
                            NadaRigCache.SparksTemplateInactive,
                            sparksRootTransform,
                            false);

                    sparkObject.name =
                        $"Spark_{i:00}";

                    sparkObject.SetActive(true);

                    sparkObject.transform.localPosition =
                        Vector3.zero;

                    sparkObject.transform.localRotation =
                        Quaternion.identity;

                    sparkObject.transform.localScale =
                        Vector3.one;
                }
            }

            if (createdSparksRoot)
            {
                sparksRootTransform.localPosition =
                    Vector3.zero;

                sparksRootTransform.localRotation =
                    Quaternion.identity;

                sparksRootTransform.localScale =
                    Vector3.one;

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [SparksBranchCreated] " +
                    $"owner='{ownerNameForLogs}' " +
                    $"parent='{parentTransform.name}' " +
                    $"root='{sparksRootTransform.name}' " +
                    $"rootId={sparksRootTransform.GetInstanceID()}");
            }

            NadaRigTransforms.NormalizeParticleSpacesUnder(
                sparksRootTransform,
                ParticleSystemSimulationSpace.Local);

            return sparksRootTransform;
        }
    }
}