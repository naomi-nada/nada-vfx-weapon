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

            // Reuse the existing rig if one is already attached. This keeps the assembly idempotent
            // and lets us refresh alignment/state without creating duplicate rig roots.
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
            
            // Parent the rig directly to the resolved weapon visual root, not the outer item wrapper.
            // This keeps the rig in the same local coordinate space as the actual weapon mesh.
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