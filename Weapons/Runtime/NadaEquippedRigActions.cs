using NADA.VFX.Weapon.Core.State;
using NADA.VFX.Weapon.Runtime.Structure;
using NADA.VFX.Weapon.Weapons.Targets;
using UnityEngine;

namespace NADA.VFX.Weapon.Weapons.Runtime
{
    internal static class NadaEquippedRigActions
    {
        internal static void TryAttachToEquipped()
        {
            Player player = Player.m_localPlayer;
            if (player == null)
                return;

            var controller = new NadaWeaponRigController();

            Transform rightHandAttach =
                FindDescendantByName(player.transform, "RightHand_Attach");

            Transform leftHandAttach =
                FindDescendantByName(player.transform, "LeftHand_Attach");

            global::ItemDrop.ItemData rightItem =
                NadaEquippedItemResolver.ResolveRightHandItem();

            global::ItemDrop.ItemData leftItem =
                NadaEquippedItemResolver.ResolveLeftHandItem();

            if (TryApplyToFirstAttachChild(rightHandAttach, controller, rightItem, requireMissingRig: true))
                return;

            if (TryApplyToFirstAttachChild(leftHandAttach, controller, leftItem, requireMissingRig: true))
                return;

            if (TryApplyToFirstAttachChild(rightHandAttach, controller, rightItem, requireMissingRig: false))
                return;

            TryApplyToFirstAttachChild(leftHandAttach, controller, leftItem, requireMissingRig: false);
        }

        internal static void TryBindEquipped()
        {
            Player player = Player.m_localPlayer;
            if (player == null)
                return;

            Transform rightHandAttach =
                FindDescendantByName(player.transform, "RightHand_Attach");

            Transform leftHandAttach =
                FindDescendantByName(player.transform, "LeftHand_Attach");

            global::ItemDrop.ItemData rightItem =
                NadaEquippedItemResolver.ResolveRightHandItem();

            global::ItemDrop.ItemData leftItem =
                NadaEquippedItemResolver.ResolveLeftHandItem();

            if (TryBindUnboundAttachedItem(rightHandAttach, rightItem))
                return;

            if (TryBindUnboundAttachedItem(leftHandAttach, leftItem))
                return;

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [Bind] no unbound equipped NADA rig found to bind.");
        }

        internal static void TryUnbindEquipped()
        {
            bool unboundAny = false;

            global::ItemDrop.ItemData rightItem =
                NadaEquippedItemResolver.ResolveRightHandItem();

            global::ItemDrop.ItemData leftItem =
                NadaEquippedItemResolver.ResolveLeftHandItem();

            if (rightItem != null && VfxStateIO.IsBound(rightItem))
            {
                NadaWeaponBinder.Unbind(rightItem);
                unboundAny = true;
            }

            if (leftItem != null && leftItem != rightItem && VfxStateIO.IsBound(leftItem))
            {
                NadaWeaponBinder.Unbind(leftItem);
                unboundAny = true;
            }

            if (!unboundAny)
            {
                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [Unbind] no bound equipped items found.");
                return;
            }

            RefreshExistingEquippedRigsOnly();
        }

        internal static void RefreshExistingEquippedRigsOnly()
        {
            Player player = Player.m_localPlayer;
            if (player == null)
                return;

            var controller = new NadaWeaponRigController();

            Transform rightHandAttach =
                FindDescendantByName(player.transform, "RightHand_Attach");

            Transform leftHandAttach =
                FindDescendantByName(player.transform, "LeftHand_Attach");

            global::ItemDrop.ItemData rightItem =
                NadaEquippedItemResolver.ResolveRightHandItem();

            global::ItemDrop.ItemData leftItem =
                NadaEquippedItemResolver.ResolveLeftHandItem();

            TryApplyToFirstAttachChild(rightHandAttach, controller, rightItem, requireMissingRig: false);
            TryApplyToFirstAttachChild(leftHandAttach, controller, leftItem, requireMissingRig: false);
        }

        internal static void RefreshExistingUnboundEquippedRigsOnly()
        {
            Player player = Player.m_localPlayer;
            if (player == null)
                return;

            var controller = new NadaWeaponRigController();

            Transform rightHandAttach =
                FindDescendantByName(player.transform, "RightHand_Attach");

            Transform leftHandAttach =
                FindDescendantByName(player.transform, "LeftHand_Attach");

            global::ItemDrop.ItemData rightItem =
                NadaEquippedItemResolver.ResolveRightHandItem();

            global::ItemDrop.ItemData leftItem =
                NadaEquippedItemResolver.ResolveLeftHandItem();

            if (rightItem != null && !VfxStateIO.IsBound(rightItem))
                TryApplyToFirstAttachChild(rightHandAttach, controller, rightItem, requireMissingRig: false);

            if (leftItem != null && leftItem != rightItem && !VfxStateIO.IsBound(leftItem))
                TryApplyToFirstAttachChild(leftHandAttach, controller, leftItem, requireMissingRig: false);
        }

        private static bool TryApplyToFirstAttachChild(
            Transform attachTransform,
            NadaWeaponRigController controller,
            global::ItemDrop.ItemData itemData,
            bool requireMissingRig)
        {
            if (attachTransform == null || controller == null || itemData == null)
                return false;

            foreach (Transform childTransform in attachTransform)
            {
                if (childTransform == null)
                    continue;

                Transform weaponVisualRootTransform =
                    NadaWeaponTargets.FindEquippedWeaponVisualRoot(childTransform);

                if (weaponVisualRootTransform == null)
                    continue;

                Transform attachTarget =
                    ResolveRigAttachTarget(weaponVisualRootTransform);

                if (attachTarget == null)
                    continue;

                Transform existingRig =
                    NadaRigPaths.FindDirectChild(
                        attachTarget,
                        Plugin.LocalWeaponRootName);

                if (requireMissingRig && existingRig != null)
                    continue;

                if (!requireMissingRig && existingRig == null)
                    continue;

                return controller.TryApply(childTransform.gameObject, itemData);
            }

            return false;
        }

        private static bool TryBindUnboundAttachedItem(
            Transform attachTransform,
            global::ItemDrop.ItemData itemData)
        {
            if (attachTransform == null || itemData == null)
                return false;

            if (VfxStateIO.IsBound(itemData))
                return false;

            foreach (Transform childTransform in attachTransform)
            {
                if (childTransform == null)
                    continue;

                Transform weaponVisualRootTransform =
                    NadaWeaponTargets.FindEquippedWeaponVisualRoot(childTransform);

                if (weaponVisualRootTransform == null)
                    continue;

                Transform attachTarget =
                    ResolveRigAttachTarget(weaponVisualRootTransform);

                if (attachTarget == null)
                    continue;

                Transform existingRig =
                    NadaRigPaths.FindDirectChild(
                        attachTarget,
                        Plugin.LocalWeaponRootName);

                if (existingRig == null)
                    continue;

                if (!NadaWeaponBinder.Bind(itemData))
                    return false;

                var controller = new NadaWeaponRigController();
                controller.TryApply(childTransform.gameObject, itemData);

                return true;
            }

            return false;
        }

        private static Transform ResolveRigAttachTarget(Transform weaponVisualRootTransform)
        {
            return weaponVisualRootTransform;
        }

        private static Transform FindDescendantByName(Transform rootTransform, string targetName)
        {
            if (rootTransform == null)
                return null;

            foreach (Transform childTransform in rootTransform.GetComponentsInChildren<Transform>(true))
            {
                if (childTransform != null && childTransform.name == targetName)
                    return childTransform;
            }

            return null;
        }
    }
}