
using NADA.VFX.Weapon.Core.Debug;
using NADA.VFX.Weapon.Modules.WeaponOpacity;
using NADA.VFX.Weapon.Weapons.Targets;
using UnityEngine;

namespace NADA.VFX.Weapon.Runtime.Structure
{
    internal static class NadaWeaponRigRemoval
    {
        internal static bool RemoveFromEquippedRoot(
            GameObject root)
        {
            if (root == null)
                return false;

            Transform weaponVisualRootTransform =
                NadaWeaponTargets.FindEquippedWeaponVisualRoot(
                    root.transform);

            if (weaponVisualRootTransform == null)
                return false;

            Transform nadaRootTransform =
                NadaRigPaths.FindDirectChild(
                    weaponVisualRootTransform,
                    Plugin.LocalWeaponRootName);

            if (nadaRootTransform == null)
                return false;

            int nadaRootInstanceId =
                nadaRootTransform.GetInstanceID();

            RetireRig(nadaRootTransform);

            NadaLogControl.Info(
                $"rig-remove:{nadaRootInstanceId}",
                $"{Plugin.ModName}: [RigRemove] " +
                $"root='{root.name}' " +
                $"visual='{weaponVisualRootTransform.name}' " +
                $"nadaRootId={nadaRootInstanceId}");

            return true;
        }

        internal static bool RemoveTrackedRig(
            Transform rigRoot)
        {
            if (rigRoot == null ||
                rigRoot.name != Plugin.LocalWeaponRootName)
            {
                return false;
            }

            // The old visual might no longer be under the equipped wrapper.
            // Remove the exact rig we previously attached instead of
            // resolving whichever visual happens to be current now.
            int rigRootId =
                rigRoot.GetInstanceID();

            RetireRig(rigRoot);

            NadaLogControl.Info(
                $"rig-remove-tracked:{rigRootId}",
                $"{Plugin.ModName}: [RigRemoveTracked] " +
                $"nadaRootId={rigRootId}");

            return true;
        }

        private static void RetireRig(
            Transform rigRoot)
        {
            // Restore the weapon's original materials before the owner
            // disappears. Only NADA-owned opacity clones are released.
            NadaWeaponOpacityRuntime.Restore(rigRoot);

            // Destroy is deferred. Disable effects immediately, then
            // detach the stale rig so another reconciliation pass cannot
            // rediscover and reuse it before Unity destroys the object.
            rigRoot.gameObject.SetActive(false);
            rigRoot.SetParent(null, true);

            Object.Destroy(rigRoot.gameObject);
        }
    }
}
