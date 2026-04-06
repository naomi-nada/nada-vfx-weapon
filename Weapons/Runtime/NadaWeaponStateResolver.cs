using NADA.VFX.Core.State;

namespace NADA.VFX.Weapons.Runtime
{
    internal static class NadaWeaponStateResolver
    {
        public static VfxState Resolve(global::ItemDrop.ItemData itemData)
        {
            if (itemData != null && VfxStateIO.TryRead(itemData, out VfxState loadedState))
                return loadedState;

            if (itemData != null)
            {
                VfxStateIO.EnsureInitializedFromConfig(itemData);

                if (VfxStateIO.TryRead(itemData, out loadedState))
                    return loadedState;
            }

            return VfxStateIO.FromConfig();
        }
    }
}