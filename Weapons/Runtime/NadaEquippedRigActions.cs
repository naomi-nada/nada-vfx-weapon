using System.Reflection;
using HarmonyLib;
using NADA.VFX.Weapon.Core.State;
using NADA.VFX.Weapon.Runtime.Structure;
using NADA.VFX.Weapon.Weapons.Targets;
using UnityEngine;

namespace NADA.VFX.Weapon.Weapons.Runtime
{
    internal enum NadaExactEquippedRefresh
    {
        NotEquipped,
        Refreshed,
        Deferred
    }

    internal static class NadaEquippedRigActions
    {
        private static readonly FieldInfo RightInstanceField =
            AccessTools.Field(typeof(global::VisEquipment), "m_rightItemInstance");

        private static readonly FieldInfo LeftInstanceField =
            AccessTools.Field(typeof(global::VisEquipment), "m_leftItemInstance");

        // Only an exact equipped ItemData match may affect a live visual.
        // A missing or swapped weapon instance can wait for Valheim's normal
        // equipment refresh. Never grab the first child that looks like one.
        internal static NadaExactEquippedRefresh RefreshExactItem(
            global::ItemDrop.ItemData item,
            bool isBound)
        {
            Player player = Player.m_localPlayer;
            if (player == null || item == null)
                return NadaExactEquippedRefresh.NotEquipped;

            bool right = object.ReferenceEquals(
                item, NadaEquippedItemResolver.ResolveRightHandItem());
            bool left = object.ReferenceEquals(
                item, NadaEquippedItemResolver.ResolveLeftHandItem());

            if (!right && !left)
                return NadaExactEquippedRefresh.NotEquipped;

            if (right && left)
                return NadaExactEquippedRefresh.Deferred;

            global::VisEquipment visualOwner =
                player.GetComponentInChildren<global::VisEquipment>(true);
            FieldInfo instanceField = right ? RightInstanceField : LeftInstanceField;
            GameObject instance = visualOwner != null
                ? instanceField?.GetValue(visualOwner) as GameObject
                : null;

            string handName = right ? "RightHand_Attach" : "LeftHand_Attach";
            if (instance == null ||
                !instance.activeInHierarchy ||
                !instance.transform.IsChildOf(player.transform) ||
                !IsUnderHand(instance.transform, player.transform, handName) ||
                !NadaWeaponTargets.IsTargetOrAttachClone(instance))
            {
                return NadaExactEquippedRefresh.Deferred;
            }

            if (!isBound)
            {
                NadaWeaponRigRemoval.RemoveFromEquippedRoot(instance);
                return NadaExactEquippedRefresh.Refreshed;
            }

            var controller = new NadaWeaponRigController();
            return controller.TryApply(instance, item)
                ? NadaExactEquippedRefresh.Refreshed
                : NadaExactEquippedRefresh.Deferred;
        }

        private static bool IsUnderHand(
            Transform visual,
            Transform player,
            string handName)
        {
            for (Transform node = visual.parent;
                 node != null && node != player;
                 node = node.parent)
            {
                if (node.name == handName)
                    return true;
            }

            return false;
        }

        internal static void RefreshExistingEquippedRigsOnly()
        {
            Player player =
                Player.m_localPlayer;

            if (player == null)
                return;

            var controller =
                new NadaWeaponRigController();

            Transform rightHandAttach =
                FindDescendantByName(
                    player.transform,
                    "RightHand_Attach");

            Transform leftHandAttach =
                FindDescendantByName(
                    player.transform,
                    "LeftHand_Attach");

            global::ItemDrop.ItemData rightItem =
                NadaEquippedItemResolver.ResolveRightHandItem();

            global::ItemDrop.ItemData leftItem =
                NadaEquippedItemResolver.ResolveLeftHandItem();

            TryApplyToFirstAttachChild(
                rightHandAttach,
                controller,
                rightItem,
                requireMissingRig: false);

            TryApplyToFirstAttachChild(
                leftHandAttach,
                controller,
                leftItem,
                requireMissingRig: false);
        }

        internal static void RefreshExistingUnboundEquippedRigsOnly()
        {
            Player player =
                Player.m_localPlayer;

            if (player == null)
                return;

            var controller =
                new NadaWeaponRigController();

            Transform rightHandAttach =
                FindDescendantByName(
                    player.transform,
                    "RightHand_Attach");

            Transform leftHandAttach =
                FindDescendantByName(
                    player.transform,
                    "LeftHand_Attach");

            global::ItemDrop.ItemData rightItem =
                NadaEquippedItemResolver.ResolveRightHandItem();

            global::ItemDrop.ItemData leftItem =
                NadaEquippedItemResolver.ResolveLeftHandItem();

            // Config is only authoritative for an explicitly unbound item.
            // Native bindings and editor drafts must not be refreshed from it.
            if (rightItem != null &&
                NadaWeaponLocalSourceResolver.Resolve(rightItem).Kind ==
                    NadaWeaponLocalSourceKind.Unbound)
            {
                TryApplyToFirstAttachChild(
                    rightHandAttach,
                    controller,
                    rightItem,
                    requireMissingRig: false);
            }

            if (leftItem != null &&
                leftItem != rightItem &&
                NadaWeaponLocalSourceResolver.Resolve(leftItem).Kind ==
                    NadaWeaponLocalSourceKind.Unbound)
            {
                TryApplyToFirstAttachChild(
                    leftHandAttach,
                    controller,
                    leftItem,
                    requireMissingRig: false);
            }
        }

        private static bool TryApplyToFirstAttachChild(
            Transform attachTransform,
            NadaWeaponRigController controller,
            global::ItemDrop.ItemData itemData,
            bool requireMissingRig)
        {
            if (attachTransform == null ||
                controller == null ||
                itemData == null)
            {
                return false;
            }

            foreach (Transform childTransform in attachTransform)
            {
                if (childTransform == null)
                    continue;

                Transform weaponVisualRootTransform =
                    NadaWeaponTargets.FindEquippedWeaponVisualRoot(
                        childTransform);

                if (weaponVisualRootTransform == null)
                    continue;

                Transform attachTarget =
                    ResolveRigAttachTarget(
                        weaponVisualRootTransform);

                if (attachTarget == null)
                    continue;

                Transform existingRig =
                    NadaRigPaths.FindDirectChild(
                        attachTarget,
                        Plugin.LocalWeaponRootName);

                if (requireMissingRig &&
                    existingRig != null)
                {
                    continue;
                }

                if (!requireMissingRig &&
                    existingRig == null)
                {
                    continue;
                }

                return controller.TryApply(
                    childTransform.gameObject,
                    itemData);
            }

            return false;
        }

        private static Transform ResolveRigAttachTarget(
            Transform weaponVisualRootTransform)
        {
            return weaponVisualRootTransform;
        }

        private static Transform FindDescendantByName(
            Transform rootTransform,
            string targetName)
        {
            if (rootTransform == null)
                return null;

            foreach (Transform childTransform in
                     rootTransform.GetComponentsInChildren<Transform>(true))
            {
                if (childTransform != null &&
                    childTransform.name == targetName)
                {
                    return childTransform;
                }
            }

            return null;
        }
    }
}