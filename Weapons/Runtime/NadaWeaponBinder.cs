using NADA.VFX.Weapon.Core.Debug;
using NADA.VFX.Weapon.Core.State;

namespace NADA.VFX.Weapon.Weapons.Runtime
{
    internal static class NadaWeaponBinder
    {
        internal static bool Bind(
            global::ItemDrop.ItemData itemData)
        {
            if (itemData == null)
                return false;

            VfxState currentState =
                VfxStateIO.FromConfig();

            VfxStateIO.Write(
                itemData,
                currentState,
                bound: true);

            string itemName =
                itemData.m_shared?.m_name ??
                "<unknown>";

            NadaLogControl.Equip(
                $"bind:{itemName}:{itemData.GetHashCode()}",
                $"{Plugin.ModName}: [Bind] bound NADA VFX to '{itemName}'.");

            return true;
        }

        internal static bool Unbind(
            global::ItemDrop.ItemData itemData)
        {
            if (itemData == null)
                return false;

            // This only clears the item's NADA state.
            // The caller owns removing any live rig that represents it.
            VfxStateIO.Clear(
                itemData);

            string itemName =
                itemData.m_shared?.m_name ??
                "<unknown>";

            NadaLogControl.Equip(
                $"unbind:{itemName}:{itemData.GetHashCode()}",
                $"{Plugin.ModName}: [Unbind] cleared NADA VFX from '{itemName}'.");

            return true;
        }
    }
}