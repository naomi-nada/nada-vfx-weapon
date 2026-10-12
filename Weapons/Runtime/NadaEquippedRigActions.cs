using NADA.VFX.Weapon.Core.State;
using NADA.VFX.Weapon.Runtime.Structure;
using NADA.VFX.Weapon.Weapons.Targets;
using UnityEngine;

namespace NADA.VFX.Weapon.Weapons.Runtime
{
    internal static class NadaEquippedRigActions
    {
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