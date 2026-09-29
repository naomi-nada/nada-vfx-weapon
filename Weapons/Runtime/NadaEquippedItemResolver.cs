using System.Reflection;
using UnityEngine;

namespace NADA.VFX.Weapon.Weapons.Runtime
{
    internal static class NadaEquippedItemResolver
    {
        // These fields belong to the Humanoid type, not to a particular
        // player or weapon. Look them up once instead of on every
        // equipment observation.
        private static readonly FieldInfo RightItemField =
            typeof(Humanoid).GetField(
                "m_rightItem",
                BindingFlags.Instance |
                BindingFlags.NonPublic |
                BindingFlags.Public);

        private static readonly FieldInfo LeftItemField =
            typeof(Humanoid).GetField(
                "m_leftItem",
                BindingFlags.Instance |
                BindingFlags.NonPublic |
                BindingFlags.Public);

        internal static global::ItemDrop.ItemData ResolveRightHandItem()
        {
            Player player = Player.m_localPlayer;
            if (player == null)
                return null;

            Humanoid humanoid = player.GetComponent<Humanoid>();
            if (humanoid == null)
                return null;

            return ReadItemDataField(humanoid, RightItemField);
        }

        internal static global::ItemDrop.ItemData ResolveLeftHandItem()
        {
            Player player = Player.m_localPlayer;
            if (player == null)
                return null;

            Humanoid humanoid = player.GetComponent<Humanoid>();
            if (humanoid == null)
                return null;

            return ReadItemDataField(humanoid, LeftItemField);
        }

        internal static global::ItemDrop.ItemData ResolveFirstEquippedItem()
        {
            global::ItemDrop.ItemData rightItem = ResolveRightHandItem();
            if (rightItem != null)
                return rightItem;

            return ResolveLeftHandItem();
        }

        private static global::ItemDrop.ItemData ReadItemDataField(
            Humanoid humanoid,
            FieldInfo fieldInfo)
        {
            return fieldInfo?.GetValue(humanoid) as global::ItemDrop.ItemData;
        }
    }
}