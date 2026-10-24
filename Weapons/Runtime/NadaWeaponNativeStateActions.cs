using NADA.VFX.Weapon.Core.Network;
using NADA.VFX.Weapon.Core.Persistence;
using NADA.VFX.Weapon.Core.State;
using NADA.VFX.Weapon.Core.State.Blocks;

namespace NADA.VFX.Weapon.Weapons.Runtime
{
    // One place for item state changes that come from outside the editor.
    // State is saved first; the live visual is only refreshed when we can
    // match it to this exact ItemData.
    internal static class NadaWeaponNativeStateActions
    {
        internal static bool TryApplyState(
            global::ItemDrop.ItemData item,
            WeaponVfxState state,
            uint nextInstanceId,
            out string reason)
        {
            if (!CanChangeItem(item, out reason))
                return false;

            // The codec validates the whole snapshot before ItemData changes.
            // Nothing from the caller's state graph is kept by the runtime.
            if (!WeaponVfxItemStore.TryWrite(
                    item, state, nextInstanceId, out reason))
                return false;

            // Native state now wins. Don't leave an old legacy binding behind
            // where Remove could accidentally bring it back later.
            VfxStateIO.Clear(item);

            FinishChange(item, isBound: true, "apply");
            return true;
        }

        internal static bool TryRemove(
            global::ItemDrop.ItemData item,
            out string reason)
        {
            if (!CanChangeItem(item, out reason))
                return false;

            // Remove means genuinely unbound, not a saved empty VFX state.
            // Clear both formats so no old effects can resurface.
            WeaponVfxItemStore.Remove(item);
            VfxStateIO.Clear(item);

            FinishChange(item, isBound: false, "remove");
            return true;
        }

        private static bool CanChangeItem(
            global::ItemDrop.ItemData item,
            out string reason)
        {
            reason = null;
            if (item == null)
            {
                reason = "No target ItemData.";
                return false;
            }

            if (NadaWeaponEditorPreviewState.TryGet(item, out _))
            {
                reason = "Close the NADA editor before changing this weapon externally.";
                return false;
            }

            return true;
        }

        private static void FinishChange(
            global::ItemDrop.ItemData item,
            bool isBound,
            string action)
        {
            NadaVfxLocalStatePublisher.MarkDirty(item);

            NadaExactEquippedRefresh refreshed =
                NadaEquippedRigActions.RefreshExactItem(item, isBound);

            Plugin.Log?.LogInfo(
                $"{Plugin.ModName}: [NativeStateTransition] " +
                $"action={action} " +
                $"item='{item.m_dropPrefab?.name ?? "<unknown>"}' " +
                $"equipped={refreshed}.");
        }
    }
}
