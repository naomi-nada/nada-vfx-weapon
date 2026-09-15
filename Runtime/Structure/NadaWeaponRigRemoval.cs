using NADA.VFX.Weapon.Core.Debug;
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

            Object.Destroy(
                nadaRootTransform.gameObject);

            NadaLogControl.Info(
                $"rig-remove:{nadaRootInstanceId}",
                $"{Plugin.ModName}: [RigRemove] " +
                $"root='{root.name}' " +
                $"visual='{weaponVisualRootTransform.name}' " +
                $"nadaRootId={nadaRootInstanceId}");

            return true;
        }
    }
}