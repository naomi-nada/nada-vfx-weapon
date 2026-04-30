using NADA.VFX.Core.State;
using NADA.VFX.Weapons.Targets;
using NADA.VFX.Runtime.Structure;
using NADA.VFX.Core.Debug;
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

            Transform attachTarget =
                NadaWeaponTargets.FindVisualMeshRoot(weaponVisualRootTransform)
                ?? weaponVisualRootTransform;

            Transform existingRoot =
                NadaRigPaths.FindDirectChild(
                    attachTarget,
                    Plugin.LocalWeaponRootName);

            if (existingRoot != null)
            {
                NadaLogControl.Equip(
                    $"refresh:{existingRoot.GetInstanceID()}",
                    $"{Plugin.ModName}: [Attach] refreshing existing rig on '{attachTarget.name}'.");

                itemData ??= ResolveItemData(root);

                VfxState state = NadaWeaponStateResolver.Resolve(itemData);

                var context = new NadaWeaponRigContext(
                    root,
                    itemData,
                    state,
                    attachTarget);

                if (!context.IsValid)
                    return false;

                NadaWeaponRigOrchestrator.Run(context);

                return true;
            }

            itemData ??= ResolveItemData(root);

            VfxState refreshState = NadaWeaponStateResolver.Resolve(itemData);

            var refreshContext = new NadaWeaponRigContext(
                root,
                itemData,
                refreshState,
                attachTarget);

            if (!refreshContext.IsValid)
                return false;

            NadaWeaponRigOrchestrator.Run(refreshContext);
            return true;
        }

        public bool TryApplyDroppedItem(GameObject root, global::ItemDrop.ItemData itemData)
        {
            if (root == null || itemData == null)
                return false;

            if (!VfxStateIO.IsBound(itemData))
                return false;

            Transform attachTarget =
                NadaWeaponTargets.FindVisualMeshRoot(root.transform)
                ?? root.transform;

            Transform existingRoot =
                NadaRigPaths.FindDirectChild(
                    attachTarget,
                    Plugin.LocalWeaponRootName);

            if (existingRoot != null)
                return true;

            VfxState state = NadaWeaponStateResolver.Resolve(itemData);

            var context = new NadaWeaponRigContext(
                root,
                itemData,
                state,
                attachTarget);

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

            return NadaEquippedItemResolver.ResolveFirstEquippedItem();
        }
    }
}