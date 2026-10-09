using NADA.VFX.Weapon.Weapons.Runtime;
using UnityEngine;

namespace NADA.VFX.Weapon.Weapons.Targets
{
    /// <summary>
    /// Resolves the local player's actual equipped weapon wrapper and visual.
    ///
    /// This does not depend on a NADA rig already existing. Editor targeting
    /// must be able to discover an equipped weapon before runtime VFX has ever
    /// been attached to it.
    /// </summary>
    internal static class NadaEquippedWeaponTargetResolver
    {
        internal enum Hand { Right, Left }

        internal sealed class ResolvedTarget
        {
            internal global::ItemDrop.ItemData ItemData { get; }
            internal GameObject Root { get; }
            internal Transform VisualRoot { get; }
            internal Hand SelectedHand { get; }

            internal ResolvedTarget(
                global::ItemDrop.ItemData itemData,
                GameObject root,
                Transform visualRoot,
                Hand selectedHand)
            {
                SelectedHand = selectedHand;
                ItemData =
                    itemData;

                Root =
                    root;

                VisualRoot =
                    visualRoot;
            }
        }

        internal static bool TryResolveFirst(
            out ResolvedTarget target)
        {
            target =
                null;

            global::Player player =
                Player.m_localPlayer;

            if (player == null)
                return false;

            global::ItemDrop.ItemData rightItem =
                NadaEquippedItemResolver
                    .ResolveRightHandItem();

            if (TryResolveHand(
                    player.transform,
                    "RightHand_Attach",
                    rightItem,
                    Hand.Right,
                    out target))
            {
                return true;
            }

            global::ItemDrop.ItemData leftItem =
                NadaEquippedItemResolver
                    .ResolveLeftHandItem();

            return TryResolveHand(
                player.transform,
                "LeftHand_Attach",
                leftItem,
                Hand.Left,
                out target);
        }

        // An explicitly selected empty hand must not fall back to the other.
        internal static bool TryResolveSelected(
            Hand hand, out ResolvedTarget target)
        {
            target = null;
            global::Player player = Player.m_localPlayer;
            if (player == null)
                return false;

            return TryResolveHand(
                player.transform,
                hand == Hand.Right ? "RightHand_Attach" : "LeftHand_Attach",
                hand == Hand.Right
                    ? NadaEquippedItemResolver.ResolveRightHandItem()
                    : NadaEquippedItemResolver.ResolveLeftHandItem(),
                hand,
                out target);
        }

        private static bool TryResolveHand(
            Transform playerTransform,
            string attachName,
            global::ItemDrop.ItemData itemData,
            Hand hand,
            out ResolvedTarget target)
        {
            target =
                null;

            if (playerTransform == null ||
                itemData == null)
            {
                return false;
            }

            Transform handAttach =
                FindDescendantByName(
                    playerTransform,
                    attachName);

            if (handAttach == null)
                return false;

            foreach (Transform child in
                     handAttach)
            {
                if (child == null)
                    continue;

                Transform visualRoot =
                    NadaWeaponTargets
                        .FindEquippedWeaponVisualRoot(
                            child);

                if (visualRoot == null)
                    continue;

                target =
                    new ResolvedTarget(
                        itemData,
                        child.gameObject,
                        visualRoot,
                        hand);

                return true;
            }

            return false;
        }

        private static Transform FindDescendantByName(
            Transform root,
            string targetName)
        {
            if (root == null)
                return null;

            foreach (Transform child in
                     root.GetComponentsInChildren<Transform>(
                         true))
            {
                if (child != null &&
                    child.name ==
                    targetName)
                {
                    return child;
                }
            }

            return null;
        }
    }
}