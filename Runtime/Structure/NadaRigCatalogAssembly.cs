using UnityEngine;
using NADA.VFX.Weapons.Targets;

namespace NADA.VFX.Runtime.Binding
{
    internal static class NadaRigCatalogAssembly
    {
        internal static Transform EnsureWorldCatalogRoot()
        {
            var existing = GameObject.Find(Plugin.WorldRootName);
            if (existing != null) return existing.transform;

            var go = new GameObject(Plugin.WorldRootName);
            Plugin.Log.LogInfo($"{Plugin.ModName}: Added catalog root '{Plugin.WorldRootName}'.");
            return go.transform;
        }

        internal static Transform EnsureWorldWeaponBranch(Transform ownerKeyTf, string ownerNameForLogs)
        {
            if (ownerKeyTf == null) return null;

            Transform worldRoot = EnsureWorldCatalogRoot();
            if (worldRoot == null) return null;

            int ownerId = ownerKeyTf.GetInstanceID();
            string branchName = $"{Plugin.WorldWeaponRootName} [{ownerId}]";

            Transform weaponRoot = NadaRigPaths.FindDirectChild(worldRoot, branchName);
            if (weaponRoot == null)
            {
                var go = new GameObject(branchName);
                weaponRoot = go.transform;
                weaponRoot.SetParent(worldRoot, false);
                NadaRigTransforms.ResetLocalTransform(weaponRoot);

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Added per-owner world weapon branch '{branchName}' under '{Plugin.WorldRootName}' " +
                    $"for ownerPath='{NadaWeaponTargets.FullPath(ownerKeyTf)}' (owner='{ownerNameForLogs}', ownerId={ownerId}).");
            }

            NadaRigTransforms.EnsureChild(weaponRoot, Plugin.EffectsRootName);
            return weaponRoot;
        }

        internal static Transform EnsureAttachedLocalWeaponBranch(Transform sword15LavaTf, string ownerNameForLogs)
        {
            if (sword15LavaTf == null) return null;

            Transform attached = NadaRigPaths.FindDirectChild(sword15LavaTf, Plugin.LocalWeaponRootName);
            if (attached != null)
            {
                NadaRigTransforms.EnsureChild(attached, Plugin.EffectsRootName);
                return attached;
            }

            var go = new GameObject(Plugin.LocalWeaponRootName);
            attached = go.transform;
            attached.SetParent(sword15LavaTf, false);

            NadaRigTransforms.EnsureChild(attached, Plugin.EffectsRootName);

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: Attached local weapon subtree under '{NadaWeaponTargets.FullPath(sword15LavaTf)}' " +
                $"as '{Plugin.LocalWeaponRootName}' (owner='{ownerNameForLogs}').");

            return attached;
        }

        internal static void DestroyCatalogRoots()
        {
            var worldRoot = GameObject.Find(Plugin.WorldRootName);
            if (worldRoot != null)
                Object.Destroy(worldRoot);

            Plugin.Log.LogInfo($"{Plugin.ModName}: Destroyed stale world catalog root.");
        }
    }
}