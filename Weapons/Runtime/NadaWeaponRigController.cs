using NADA.VFX.Runtime.Binding;
using NADA.VFX.Runtime.Execution;
using NADA.VFX.Weapons.Targets;
using UnityEngine;

namespace NADA.VFX.Weapons.Runtime
{
    internal sealed class NadaWeaponRigController
    {
        private readonly NadaModuleRunner _moduleRunner = new();

        public bool TryApply(GameObject root, global::ItemDrop.ItemData itemData = null)
        {
            if (root == null)
                return false;

            if (!NadaWeaponTargets.IsTargetOrAttachClone(root))
                return false;

            Transform weaponVisualRoot = NadaWeaponTargets.FindSword15Lava(root.transform);
            if (weaponVisualRoot == null)
                return false;

            itemData ??= ResolveItemData(root);

            RigGroups groups = NadaRigFinder.BuildGroups(root.transform);

            var context = new NadaWeaponRigContext(
                root,
                itemData,
                weaponVisualRoot,
                groups);

            if (!context.IsValid)
                return false;

            _moduleRunner.RunInitialApply(context);
            return true;
        }

        private static global::ItemDrop.ItemData ResolveItemData(GameObject root)
        {
            var itemDrop = root.GetComponent<global::ItemDrop>();
            if (itemDrop != null)
                return itemDrop.m_itemData;

            return null;
        }
    }
}