using UnityEngine;
using NADA.VFX.Weapons.Targets;
using NADA.VFX.Weapons.Runtime;
using NADA.VFX.Core.Debug;

namespace NADA.VFX.Runtime.Structure
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
                NadaRigTransforms.EnsureChild(localWeaponRootTransform, Plugin.EffectsRootName);

                localWeaponRootTransform.localPosition = alignment.LocalPosition;
                localWeaponRootTransform.localEulerAngles = alignment.LocalEulerAngles;
                localWeaponRootTransform.localScale = alignment.LocalScale;

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
            
            localWeaponRootTransform.localPosition = alignment.LocalPosition;
            localWeaponRootTransform.localEulerAngles = alignment.LocalEulerAngles;
            localWeaponRootTransform.localScale = alignment.LocalScale;

            return localWeaponRootTransform;
        }
    }
}