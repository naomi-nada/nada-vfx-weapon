using NADA.VFX.Weapon.Core.Debug;
using NADA.VFX.Weapon.Core.State;
using NADA.VFX.Weapon.Runtime.Structure;
using NADA.VFX.Weapon.Weapons.Targets;
using UnityEngine;

namespace NADA.VFX.Weapon.Weapons.Runtime
{
    internal sealed class NadaWeaponRigController
    {
        public bool TryApply(
            GameObject root,
            global::ItemDrop.ItemData itemData = null)
        {
            if (root == null)
                return false;

            if (!NadaWeaponTargets.IsTargetOrAttachClone(root))
                return false;

            Transform weaponVisualRootTransform =
                NadaWeaponTargets.FindEquippedWeaponVisualRoot(
                    root.transform);

            if (weaponVisualRootTransform == null)
                return false;

            Transform attachTarget =
                weaponVisualRootTransform;

            itemData ??=
                ResolveItemData(root);

            VfxState state =
                NadaWeaponStateResolver.Resolve(itemData);

            return TryApplyInternal(
                root,
                itemData,
                state,
                attachTarget);
        }

        public bool TryApplyResolvedState(
            GameObject root,
            int itemHash,
            VfxState state)
        {
            if (root == null)
                return false;

            if (!NadaWeaponTargets.IsTargetOrAttachClone(root))
                return false;

            Transform weaponVisualRootTransform =
                NadaWeaponTargets.FindEquippedWeaponVisualRoot(
                    root.transform);

            if (weaponVisualRootTransform == null)
                return false;

            NadaLogControl.Info(
                $"remote-rig-target:{root.GetInstanceID()}",
                $"{Plugin.ModName}: [RemoteRigTarget] " +
                $"root='{root.name}' " +
                $"visualRoot='{weaponVisualRootTransform.name}' " +
                $"itemHash={itemHash} " +
                $"path='{NadaWeaponTargets.FullPath(weaponVisualRootTransform)}'");

            NadaWeaponMetadata metadata =
                NadaWeaponMetadataResolver.FromItemHash(
                    itemHash);

            var context =
                new NadaWeaponRigContext(
                    root,
                    itemData: null,
                    state,
                    weaponVisualRootTransform);

            if (!context.IsValid)
                return false;

            NadaWeaponRigOrchestrator.RunRemote(
                context,
                metadata);

            Transform createdRoot =
                NadaRigPaths.FindDirectChild(
                    weaponVisualRootTransform,
                    Plugin.LocalWeaponRootName);

            return createdRoot != null;
        }

        public bool TryApplyDroppedItem(
            GameObject root,
            global::ItemDrop.ItemData itemData)
        {
            if (root == null || itemData == null)
                return false;

            if (!VfxStateIO.IsBound(itemData))
                return false;

            // Dropped items have their own wrapper/root, so attach the rig to the actual weapon visual instead.
            // Using the ItemDrop root here can put the rig in a completely different local coordinate space.
            Transform attachTarget =
                NadaWeaponTargets.FindDroppedWeaponVisualRoot(
                    root.transform);

            if (attachTarget == null)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [DropApply] could not resolve visual root " +
                    $"for '{root.name}'.");

                return false;
            }

            NadaLogControl.Info(
                $"drop-target:{root.GetInstanceID()}",
                $"{Plugin.ModName}: [DropTarget] " +
                $"root='{root.name}' " +
                $"visualRoot='{attachTarget.name}' " +
                $"path='{NadaWeaponTargets.FullPath(attachTarget)}'");

            Transform existingRoot =
                NadaRigPaths.FindDirectChild(
                    attachTarget,
                    Plugin.LocalWeaponRootName);

            if (existingRoot != null)
                return true;

            VfxState state =
                NadaWeaponStateResolver.Resolve(itemData);

            var context =
                new NadaWeaponRigContext(
                    root,
                    itemData,
                    state,
                    attachTarget);

            if (!context.IsValid)
                return false;

            NadaWeaponRigOrchestrator.Run(context);

            Transform refreshedRoot =
                NadaRigPaths.FindDirectChild(
                    attachTarget,
                    Plugin.LocalWeaponRootName);

            return refreshedRoot != null;
        }

        private static bool TryApplyInternal(
            GameObject root,
            global::ItemDrop.ItemData itemData,
            VfxState state,
            Transform attachTarget)
        {
            if (root == null ||
                attachTarget == null)
            {
                return false;
            }

            Transform existingRoot =
                NadaRigPaths.FindDirectChild(
                    attachTarget,
                    Plugin.LocalWeaponRootName);

            if (existingRoot != null)
            {
                NadaLogControl.Equip(
                    $"refresh:{existingRoot.GetInstanceID()}",
                    $"{Plugin.ModName}: [Attach] refreshing existing rig on '{attachTarget.name}'.");
            }
            else
            {
                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [AttachTarget] " +
                    $"root='{root.name}' " +
                    $"visualRoot='{attachTarget.name}' " +
                    $"attachTarget='{attachTarget.name}' " +
                    $"path='{NadaWeaponTargets.FullPath(attachTarget)}'");
            }

            var context =
                new NadaWeaponRigContext(
                    root,
                    itemData,
                    state,
                    attachTarget);

            if (!context.IsValid)
                return false;

            NadaWeaponRigOrchestrator.Run(context);

            Transform refreshedRoot =
                NadaRigPaths.FindDirectChild(
                    attachTarget,
                    Plugin.LocalWeaponRootName);

            return refreshedRoot != null;
        }

        private static global::ItemDrop.ItemData ResolveItemData(
            GameObject root)
        {
            var itemDrop =
                root.GetComponent<global::ItemDrop>();

            if (itemDrop != null)
                return itemDrop.m_itemData;

            return NadaEquippedItemResolver
                .ResolveFirstEquippedItem();
        }
    }
}