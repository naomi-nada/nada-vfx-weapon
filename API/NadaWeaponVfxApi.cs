using NADA.VFX.Weapon.Core.Config;
using NADA.VFX.Weapon.Core.Persistence;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Runtime.Structure;
using NADA.VFX.Weapon.Weapons.Targets;
using NADA.VFX.Weapon.Weapons.Runtime;

namespace NADA.VFX.Weapon.API
{
    // An immutable public handle to one resolved weapon VFX snapshot.
    // Callers can reuse it, but don't get access to our runtime or save format.
    public sealed class WeaponVfxSnapshot
    {
        internal readonly WeaponVfxState State;
        internal readonly uint NextInstanceId;

        internal WeaponVfxSnapshot(WeaponVfxState state, uint nextInstanceId)
        {
            State = state;
            NextInstanceId = nextInstanceId;
        }

        public int EffectCount => State?.Effects?.Count ?? 0;
    }

    // Weapon is the first NADA target API. Future Player/World/Building
    // modules can use the same conventions without sharing ItemData or rigs.
    public static class NadaWeaponVfxApi
    {
        public const int ContractVersion = 1;

        // Loading a style only reads a local style file. Apply resolves the
        // actual state into ItemData, so remote players don't need that file.
        public static bool TryLoadStyle(
            string styleName,
            out WeaponVfxSnapshot snapshot,
            out string reason)
        {
            snapshot = null;
            if (!WeaponVfxStyleStore.TryLoad(
                    styleName, out WeaponVfxState state,
                    out uint nextInstanceId, out reason))
                return false;

            snapshot = new WeaponVfxSnapshot(state, nextInstanceId);
            return true;
        }

        public static bool TryParseShareCode(
            string shareCode,
            out WeaponVfxSnapshot snapshot,
            out string reason)
        {
            snapshot = null;
            if (!WeaponVfxShareCode.TryDecode(
                    shareCode, out WeaponVfxState state,
                    out uint nextInstanceId, out reason))
                return false;

            snapshot = new WeaponVfxSnapshot(state, nextInstanceId);
            return true;
        }

        public static bool TryReadState(
            global::ItemDrop.ItemData item,
            out WeaponVfxSnapshot snapshot,
            out string reason)
        {
            snapshot = null;
            if (item == null)
            {
                reason = "No target ItemData.";
                return false;
            }

            WeaponVfxStateReadStatus status = WeaponVfxItemStore.Read(
                item, out WeaponVfxState state,
                out uint nextInstanceId, out reason);

            if (status != WeaponVfxStateReadStatus.Valid)
            {
                if (status == WeaponVfxStateReadStatus.Missing)
                    reason = "Item has no native NADA weapon binding.";
                return false;
            }

            snapshot = new WeaponVfxSnapshot(state, nextInstanceId);
            return true;
        }

        public static bool TryCreateShareCode(
            WeaponVfxSnapshot snapshot,
            out string shareCode,
            out string reason)
        {
            shareCode = null;
            reason = null;
            if (snapshot?.State == null)
            {
                reason = "No weapon VFX snapshot.";
                return false;
            }

            return WeaponVfxShareCode.TryEncode(
                snapshot.State, snapshot.NextInstanceId,
                out shareCode, out reason);
        }

        // Success means the validated state was saved. An unequipped item
        // will render on next equip; a missing equipped visual may defer.
        public static bool TryApplyState(
            global::ItemDrop.ItemData item,
            WeaponVfxSnapshot snapshot,
            out string reason)
        {
            if (snapshot?.State == null)
            {
                reason = "No weapon VFX snapshot.";
                return false;
            }

            return NadaWeaponNativeStateActions.TryApplyState(
                item, snapshot.State, snapshot.NextInstanceId,
                out reason);
        }

        public static bool TryApplyStyle(
            global::ItemDrop.ItemData item,
            string styleName,
            out string reason)
        {
            if (!TryLoadStyle(styleName, out WeaponVfxSnapshot snapshot,
                    out reason))
                return false;

            return TryApplyState(item, snapshot, out reason);
        }

        public static bool TryApplyShareCode(
            global::ItemDrop.ItemData item,
            string shareCode,
            out string reason)
        {
            if (!TryParseShareCode(shareCode, out WeaponVfxSnapshot snapshot,
                    out reason))
                return false;

            return TryApplyState(item, snapshot, out reason);
        }

        // ItemData doesn't tell us which dropped object is showing it.
        // The caller supplies that exact world instance; no scene scan.
        // This refreshes the local visual only. It doesn't save the drop's
        // network/ZDO data or claim ownership of the world object.
        public static bool TryRefreshDroppedVisual(
            global::ItemDrop dropped,
            out string reason)
        {
            reason = null;
            if (dropped == null || dropped.gameObject == null ||
                dropped.m_itemData == null ||
                !dropped.gameObject.scene.IsValid() ||
                !dropped.gameObject.activeInHierarchy)
            {
                reason = "No live dropped item to refresh.";
                return false;
            }

            var source = NadaWeaponLocalSourceResolver.Resolve(dropped.m_itemData);
            bool bound = source.Kind == NadaWeaponLocalSourceKind.NativeBound ||
                         source.Kind == NadaWeaponLocalSourceKind.LegacyBound;

            if (bound && PluginConfig.DroppedItemVisibility?.Value != true)
            {
                reason = "Dropped-weapon visuals are disabled in NADA settings.";
                return false;
            }

            // Even an invalid or unbound item needs a pass to retire any
            // rig that was attached before its state changed.
            var controller = new NadaWeaponRigController();
            bool applied = controller.TryApplyDroppedItem(
                dropped.gameObject, dropped.m_itemData);

            if (source.Kind == NadaWeaponLocalSourceKind.InvalidNative)
            {
                reason = source.FailureReason ?? "Invalid dropped weapon state.";
                return false;
            }

            if (bound)
            {
                if (!applied)
                {
                    reason = "Dropped state is bound, but its visual isn't ready.";
                    return false;
                }

                return true;
            }

            // TryApplyDroppedItem returns false for unbound items on purpose.
            // Confirm the old rig really disappeared before calling it done.
            var visual = NadaWeaponTargets.FindDroppedWeaponVisualRoot(
                dropped.transform);
            if (visual == null)
            {
                reason = "Couldn't resolve the dropped weapon visual.";
                return false;
            }

            var rig = NadaRigPaths.FindDirectChild(
                visual, Plugin.LocalWeaponRootName);
            if (rig != null && rig.gameObject.activeSelf)
            {
                reason = "An active dropped weapon rig is still attached.";
                return false;
            }

            return true;
        }

        // Remove means unbound. It clears native AND old legacy NADA data;
        // an empty saved state is a different operation.
        public static bool TryRemove(
            global::ItemDrop.ItemData item,
            out string reason)
        {
            return NadaWeaponNativeStateActions.TryRemove(item, out reason);
        }
    }
}
