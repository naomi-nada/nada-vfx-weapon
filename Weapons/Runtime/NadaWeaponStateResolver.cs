using NADA.VFX.Weapon.Core.State;

namespace NADA.VFX.Weapon.Weapons.Runtime
{
    internal static class NadaWeaponStateResolver
    {
        public static VfxState Resolve(global::ItemDrop.ItemData itemData)
        {
            if (itemData != null && VfxStateIO.IsBound(itemData))
            {
                if (VfxStateIO.TryRead(itemData, out VfxState loadedState))
                    return loadedState;
            }

            return VfxStateIO.FromConfig();
        }
    }
}