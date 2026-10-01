using UnityEngine;

namespace NADA.VFX.Weapon.Runtime.Structure
{
    internal static class NadaFlareRigAssembly
    {
        internal static Transform EnsureFlareBranch(
            Transform parentTransform,
            string ownerNameForLogs)
        {
            if (!NadaRigCache.CacheReady)
                return null;

            if (NadaRigCache.RefRigTemplateInactive == null)
                return null;

            if (parentTransform == null)
                return null;

            Transform existing =
                NadaRigPaths.FindDirectChild(
                    parentTransform,
                    Plugin.FlareName);

            if (existing != null)
                return existing;

            GameObject donorObject =
                Object.Instantiate(
                    NadaRigCache.RefRigTemplateInactive,
                    parentTransform,
                    false);

            donorObject.name =
                "NADA Flare Donor";

            donorObject.SetActive(false);

            Transform donorTransform =
                donorObject.transform;

            NadaRigTransforms.ResetLocalTransform(
                donorTransform);

            NadaRigTransforms.NormalizeParticleSpacesUnder(
                donorTransform,
                ParticleSystemSimulationSpace.Local);

            Transform flareTransform =
                NadaRigPaths.FindDescendantByName(
                    donorTransform,
                    "flare");

            if (flareTransform == null)
            {
                donorObject.SetActive(false);
                donorTransform.SetParent(null, false);
                Object.Destroy(donorObject);

                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [FlareBranchCreateFailed] " +
                    $"owner='{ownerNameForLogs}' " +
                    $"parent='{parentTransform.name}' " +
                    $"reason='donor-flare-not-found'");

                return null;
            }

            flareTransform.name =
                Plugin.FlareName;

            // Keep the extracted visual inactive until its behavior has
            // received state. That avoids a brief default-colored flare.
            flareTransform.gameObject.SetActive(false);

            flareTransform.SetParent(
                parentTransform,
                true);

            // Destroy is deferred, so get the temporary donor completely
            // out of the owned hierarchy before scheduling destruction.
            donorObject.SetActive(false);
            donorTransform.SetParent(null, false);
            Object.Destroy(donorObject);

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [FlareBranchCreated] " +
                $"owner='{ownerNameForLogs}' " +
                $"parent='{parentTransform.name}' " +
                $"root='{flareTransform.name}' " +
                $"rootId={flareTransform.GetInstanceID()}");

            return flareTransform;
        }

        internal static void RemoveDirectFlareBranch(
            Transform localEffectsRootTransform,
            string ownerNameForLogs)
        {
            if (localEffectsRootTransform == null)
                return;

            Transform flareTransform =
                NadaRigPaths.FindDirectChild(
                    localEffectsRootTransform,
                    Plugin.FlareName);

            if (flareTransform == null)
                return;

            int rootId =
                flareTransform.GetInstanceID();

            string rootName =
                flareTransform.name;

            flareTransform.gameObject.SetActive(false);
            flareTransform.SetParent(null, false);
            Object.Destroy(flareTransform.gameObject);

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [FlareBranchRemoved] " +
                $"owner='{ownerNameForLogs}' " +
                $"parent='{localEffectsRootTransform.name}' " +
                $"root='{rootName}' " +
                $"rootId={rootId} " +
                $"reason='ownership-change'");
        }
    }
}