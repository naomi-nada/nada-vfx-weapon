using System.Reflection;
using UnityEngine;

namespace NADA.VFX.Weapon.Weapons.Runtime
{
    internal static class NadaEquippedItemResolver
    {
        internal static global::ItemDrop.ItemData ResolveRightHandItem()
        {
            Player player = Player.m_localPlayer;
            if (player == null)
                return null;

            Humanoid humanoid = player.GetComponent<Humanoid>();
            if (humanoid == null)
                return null;

            return ReadItemDataField(humanoid, "m_rightItem");
        }

        internal static global::ItemDrop.ItemData ResolveLeftHandItem()
        {
            Player player = Player.m_localPlayer;
            if (player == null)
                return null;

            Humanoid humanoid = player.GetComponent<Humanoid>();
            if (humanoid == null)
                return null;

            return ReadItemDataField(humanoid, "m_leftItem");
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