using UnityEngine;

namespace NADA.VFX.Weapon.Weapons.Runtime
{
    internal static class NadaWeaponMetadataResolver
    {
        internal static NadaWeaponMetadata FromItemData(
            global::ItemDrop.ItemData itemData)
        {
            if (itemData?.m_shared == null)
                return NadaWeaponMetadata.Empty;

            return new NadaWeaponMetadata(
                itemData.m_shared.m_name,
                itemData.m_shared.m_itemType.ToString());
        }

        internal static NadaWeaponMetadata FromItemHash(
            int itemHash)
        {
            if (itemHash == 0)
                return NadaWeaponMetadata.Empty;

            if (ObjectDB.instance == null)
                return NadaWeaponMetadata.Empty;

            GameObject itemPrefab =
                ObjectDB.instance.GetItemPrefab(itemHash);

            if (itemPrefab == null)
                return NadaWeaponMetadata.Empty;

            global::ItemDrop itemDrop =
                itemPrefab.GetComponent<global::ItemDrop>();

            if (itemDrop?.m_itemData?.m_shared == null)
                return NadaWeaponMetadata.Empty;

            return new NadaWeaponMetadata(
                itemDrop.m_itemData.m_shared.m_name,
                itemDrop.m_itemData.m_shared.m_itemType.ToString());
        }
    }
}