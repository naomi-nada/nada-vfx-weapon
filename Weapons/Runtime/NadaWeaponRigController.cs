using NADA.VFX.Core.State;
using NADA.VFX.Weapons.Targets;
using UnityEngine;

namespace NADA.VFX.Weapons.Runtime
{
    internal sealed class NadaWeaponRigController
    {
        public bool TryApply(GameObject root, global::ItemDrop.ItemData itemData = null)
        {
            if (root == null)
                return false;

            if (!NadaWeaponTargets.IsTargetOrAttachClone(root))
                return false;

            Transform weaponVisualRootTransform =
                NadaWeaponTargets.FindEquippedWeaponVisualRoot(root.transform);
            if (weaponVisualRootTransform == null)
                return false;

            itemData ??= ResolveItemData(root);

            VfxState state = NadaWeaponStateResolver.Resolve(itemData);

            var context = new NadaWeaponRigContext(
                root,
                itemData,
                state,
                weaponVisualRootTransform);

            if (!context.IsValid)
                return false;

            NadaWeaponRigOrchestrator.Run(context);
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