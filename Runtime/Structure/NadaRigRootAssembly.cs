using UnityEngine;
using NADA.VFX.Weapon.Core.Debug;
using NADA.VFX.Weapon.Weapons.Runtime;
using NADA.VFX.Weapon.Weapons.Targets;

namespace NADA.VFX.Weapon.Runtime.Structure
{
    internal static class NadaRigRootAssembly
    {
        internal static Transform EnsureAttachedLocalWeaponBranch(
            Transform weaponVisualRootTransform,
            string ownerNameForLogs,
            NadaWeaponRigAlignment alignment)
        {
            if (weaponVisualRootTransform == null)
                return null;

            Transform localWeaponRootTransform =
                NadaRigPaths.FindDirectChild(weaponVisualRootTransform, Plugin.LocalWeaponRootName);

            if (localWeaponRootTransform != null)
            {
                NadaLogControl.Equip(
                    $"rig-exists:{weaponVisualRootTransform.GetInstanceID()}",
                    $"{Plugin.ModName}: [Equip] rig already exists on '{weaponVisualRootTransform.name}'");

                NadaRigTransforms.EnsureChild(localWeaponRootTransform, Plugin.EffectsRootName);

                ApplyAlignment(localWeaponRootTransform, alignment);

                return localWeaponRootTransform;
            }

            var localWeaponRootObject = new GameObject(Plugin.LocalWeaponRootName);
            localWeaponRootTransform = localWeaponRootObject.transform;
            localWeaponRootTransform.SetParent(weaponVisualRootTransform, false);

            NadaRigTransforms.EnsureChild(localWeaponRootTransform, Plugin.EffectsRootName);

            NadaLogControl.Info(
                $"local-root:{localWeaponRootTransform.GetInstanceID()}",
                $"{Plugin.ModName}: Attached local weapon subtree under '{NadaWeaponTargets.FullPath(weaponVisualRootTransform)}' " +
                $"as '{Plugin.LocalWeaponRootName}' " +
                $"weaponRootId={weaponVisualRootTransform.GetInstanceID()} " +
                $"nadaRootId={localWeaponRootTransform.GetInstanceID()} " +
                $"(owner='{ownerNameForLogs}').");

            ApplyAlignment(localWeaponRootTransform, alignment);

            return localWeaponRootTransform;
        }

        private static void ApplyAlignment(
            Transform localWeaponRootTransform,
            NadaWeaponRigAlignment alignment)
        {
            if (localWeaponRootTransform == null)
                return;

            var anchor =
                localWeaponRootTransform.GetComponent<NadaRigAlignmentAnchor>();

            if (anchor == null)
                anchor = localWeaponRootTransform.gameObject.AddComponent<NadaRigAlignmentAnchor>();

            anchor.Set(alignment);

            localWeaponRootTransform.localPosition = alignment.LocalPosition;
            localWeaponRootTransform.localEulerAngles = alignment.LocalEulerAngles;
            localWeaponRootTransform.localScale = alignment.LocalScale;
        }
    }
}