namespace NADA.VFX.Weapon.Weapons.Runtime
{
    internal readonly struct NadaWeaponMetadata
    {
        internal string ItemName { get; }
        internal string ItemType { get; }

        internal NadaWeaponMetadata(
            string itemName,
            string itemType)
        {
            ItemName = itemName ?? string.Empty;
            ItemType = itemType ?? string.Empty;
        }

        internal static NadaWeaponMetadata Empty =>
            new(
                string.Empty,
                string.Empty);
    }
}