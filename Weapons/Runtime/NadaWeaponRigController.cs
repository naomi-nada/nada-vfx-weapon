using NADA.VFX.Core.State;
using NADA.VFX.Weapons.Targets;
using NADA.VFX.Runtime.Structure;
using NADA.VFX.Core.Debug;
using UnityEngine;
using System.Reflection;

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

            Transform existingRoot =
                NadaRigPaths.FindDirectChild(
                    weaponVisualRootTransform,
                    Plugin.LocalWeaponRootName);

            if (existingRoot != null)
            {
                NadaLogControl.Equip(
                    $"refresh:{existingRoot.GetInstanceID()}",
                    $"{Plugin.ModName}: [Attach] refreshing existing rig on '{weaponVisualRootTransform.name}'.");

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

            itemData ??= ResolveItemData(root);

            VfxState refreshState = NadaWeaponStateResolver.Resolve(itemData);

            var refreshContext = new NadaWeaponRigContext(
                root,
                itemData,
                refreshState,
                weaponVisualRootTransform);

            if (!refreshContext.IsValid)
                return false;

            NadaWeaponRigOrchestrator.Run(refreshContext);
            return true;
        }

        private static global::ItemDrop.ItemData ResolveItemData(GameObject root)
        {
            var itemDrop = root.GetComponent<global::ItemDrop>();
            if (itemDrop != null)
                return itemDrop.m_itemData;

            Player player = Player.m_localPlayer;
            if (player == null)
                return null;

            Humanoid humanoid = player.GetComponent<Humanoid>();
            if (humanoid == null)
                return null;

            global::ItemDrop.ItemData rightItem =
                ReadItemDataField(humanoid, "m_rightItem");

            if (rightItem != null)
                return rightItem;

            global::ItemDrop.ItemData leftItem =
                ReadItemDataField(humanoid, "m_leftItem");

            if (leftItem != null)
                return leftItem;

            return null;
        }

        private static global::ItemDrop.ItemData ReadItemDataField(
            Humanoid humanoid,
            string fieldName)
        {
            FieldInfo fieldInfo =
                typeof(Humanoid).GetField(
                    fieldName,
                    BindingFlags.Instance |
                    BindingFlags.NonPublic |
                    BindingFlags.Public);

            return fieldInfo?.GetValue(humanoid) as global::ItemDrop.ItemData;
        }
    }
}