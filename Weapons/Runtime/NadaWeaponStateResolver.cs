using NADA.VFX.Core.State;

namespace NADA.VFX.Runtime.Persistence
{
    internal static class NadaWeaponStateResolver
    {
        public static VfxState Resolve(global::ItemDrop.ItemData itemData)
        {
            if (itemData != null && VfxStateIO.TryRead(itemData, out var loaded))
                return loaded;

            if (itemData != null)
            {
                VfxStateIO.EnsureInitializedFromConfig(itemData);

                if (VfxStateIO.TryRead(itemData, out loaded))
                    return loaded;
            }

            return VfxStateIO.FromConfig();
        }
    }
}