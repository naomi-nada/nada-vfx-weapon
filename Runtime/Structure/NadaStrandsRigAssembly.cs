using UnityEngine;

namespace NADA.VFX.Weapon.Runtime.Structure
{
    public class NadaStrandsRigAssembly
    {
        internal static Transform EnsureLocalStrandsBranch(
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

            return EnsureStrandsBranch(
                localEffectsRootTransform,
                ownerNameForLogs);
        }

        internal static Transform EnsureStrandsBranch(
            Transform parentTransform,
            string ownerNameForLogs)
        {
            if (!NadaRigCache.CacheReady)
                return null;

            if (NadaRigCache.StrandsTemplateInactive == null)
                return null;

            if (parentTransform == null)
                return null;

            Transform strandsTransform =
                NadaRigPaths.FindDirectChild(
                    parentTransform,
                    Plugin.StrandsName);

            if (strandsTransform != null)
                return strandsTransform;

            GameObject strandsObject =
                Object.Instantiate(
                    NadaRigCache.StrandsTemplateInactive,
                    parentTransform,
                    false);

            strandsObject.name =
                Plugin.StrandsName;

            NadaRigTransforms.ResetLocalTransform(
                strandsObject.transform);

            // Preserve the donor orientation used by the legacy branch.
            // The instance root stays normalized; authored orientation
            // belongs to the Strands visual itself.
            strandsObject.transform.localRotation =
                Quaternion.Euler(
                    90f,
                    0f,
                    0f);

            strandsObject.SetActive(true);

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [StrandsBranchCreated] " +
                $"owner='{ownerNameForLogs}' " +
                $"parent='{parentTransform.name}' " +
                $"root='{strandsObject.name}' " +
                $"rootId={strandsObject.transform.GetInstanceID()}");

            return strandsObject.transform;
        }

        internal static bool RemoveDirectStrandsBranch(
            Transform parentTransform,
            string ownerNameForLogs)
        {
            if (parentTransform == null)
                return false;

            Transform strandsTransform =
                NadaRigPaths.FindDirectChild(
                    parentTransform,
                    Plugin.StrandsName);

            if (strandsTransform == null)
                return false;

            string parentName =
                parentTransform.name;

            int rootId =
                strandsTransform.GetInstanceID();

            strandsTransform
                .gameObject
                .SetActive(false);

            // Destroy is deferred. Detaching immediately prevents another
            // owner from rediscovering this stale direct branch this frame.
            strandsTransform.SetParent(
                null,
                false);

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [StrandsBranchRemoved] " +
                $"owner='{ownerNameForLogs}' " +
                $"parent='{parentName}' " +
                $"root='{Plugin.StrandsName}' " +
                $"rootId={rootId} " +
                $"reason='ownership-change'");

            Object.Destroy(
                strandsTransform.gameObject);

            return true;
        }
    }
}