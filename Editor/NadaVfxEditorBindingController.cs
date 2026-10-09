using NADA.VFX.Weapon.Core.Persistence;
using NADA.VFX.Weapon.Core.Network;
using NADA.VFX.Weapon.Core.State;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Weapons.Runtime;

namespace NADA.VFX.Weapon.Editor
{
    // Coordinates user-initiated binding transitions only. Persistence owns
    // encoding/keys, working state owns drafts, and runtime owns VFX objects.
    internal static class NadaVfxEditorBindingController
    {
        internal static bool TryBind(
            NadaVfxEditorTarget target,
            out string reason)
        {
            reason = null;
            if (!IsCurrentTarget(target, out reason))
                return false;

            global::ItemDrop.ItemData item = target.ItemData;
            NadaWeaponLocalSourceKind source =
                NadaWeaponLocalSourceResolver.Resolve(item).Kind;
            if (source != NadaWeaponLocalSourceKind.Unbound &&
                source != NadaWeaponLocalSourceKind.EditorPreview)
            {
                reason = "Weapon is already bound or has invalid native state.";
                return false;
            }

            if (!NadaVfxEditorWorkingState.TryGetDraftForBinding(
                    item, out WeaponVfxState state, out uint nextInstanceId))
            {
                reason = "No editable draft for the equipped weapon.";
                return false;
            }

            // TryWrite encodes and validates before touching ItemData.
            // A rejected state leaves the original weapon untouched.
            if (!WeaponVfxItemStore.TryWrite(
                    item, state, nextInstanceId, out reason))
                return false;

            // Binding is permitted only from an unbound draft. Legacy saves
            // are retired during an explicit Unbind, after the codec has
            // validated a separate copy of the weapon's old state.
            NadaVfxLocalStatePublisher.MarkDirty(item);

            Plugin.Log?.LogInfo(
                $"{Plugin.ModName}: [EditorNativeBind] " +
                $"item='{target.PrefabName}' " +
                $"effects={state.Effects.Count} cursor={nextInstanceId}.");
            return true;
        }

        internal static bool TryUnbind(
            NadaVfxEditorTarget target,
            out string reason)
        {
            reason = null;
            if (!IsCurrentTarget(target, out reason))
                return false;

            global::ItemDrop.ItemData item = target.ItemData;
            NadaWeaponLocalSourceSelection source =
                NadaWeaponLocalSourceResolver.Resolve(item);
            if (source.Kind == NadaWeaponLocalSourceKind.InvalidNative)
            {
                reason = source.FailureReason ??
                         "Invalid native state. Unbind is disabled to protect data.";
                return false;
            }
            if (source.Kind != NadaWeaponLocalSourceKind.NativeBound &&
                source.Kind != NadaWeaponLocalSourceKind.LegacyBound)
            {
                reason = "Weapon is not bound.";
                return false;
            }

            // A freshly resolved target, not a cached GUI snapshot, supplies
            // the legacy migration view. A missing/invalid view fails closed.
            WeaponVfxState boundState =
                source.Kind == NadaWeaponLocalSourceKind.NativeBound
                    ? source.State
                    : target.BoundViewState;
            if (boundState == null || target.BoundViewError != null)
            {
                reason = target.BoundViewError ??
                         "Bound weapon has no valid state to transfer.";
                return false;
            }

            uint cursor = source.Kind == NadaWeaponLocalSourceKind.NativeBound
                ? source.NextInstanceId
                : GetNextLegacyId(boundState);
            if (cursor == 0)
            {
                reason = "The next effect InstanceId is exhausted.";
                return false;
            }

            // Use the tested codec as a deep-copy and validation boundary.
            // Glue, transforms, order, names, and IDs must not share objects
            // with a saved/read-only bound snapshot.
            if (!WeaponVfxStateCodec.TryEncode(
                    boundState, cursor, out byte[] bytes, out reason) ||
                !WeaponVfxStateCodec.TryDecode(
                    bytes, out WeaponVfxState draft,
                    out uint decodedCursor, out reason))
                return false;

            // Only after the draft is fully validated may we retire keys.
            // Clear legacy too, or removing native would resurrect an older
            // legacy binding on a weapon containing both source formats.
            if (source.Kind == NadaWeaponLocalSourceKind.NativeBound &&
                !WeaponVfxItemStore.Remove(item))
            {
                reason = "Native binding disappeared before it could be removed.";
                return false;
            }
            VfxStateIO.Clear(item);
            NadaVfxEditorWorkingState.InstallUnboundDraft(
                item, draft, decodedCursor);

            // This rig may have existed before the editor was opened.
            // Adopt it so Close() can clean it up like any other preview.
            NadaVfxEditorPreviewRigSession.AdoptUnboundRig(target);
            NadaVfxLocalStatePublisher.MarkDirty(item);

            Plugin.Log?.LogInfo(
                $"{Plugin.ModName}: [EditorNativeUnbind] " +
                $"item='{target.PrefabName}' " +
                $"from={source.Kind} effects={draft.Effects.Count} " +
                $"cursor={decodedCursor}.");
            return true;
        }

        private static bool IsCurrentTarget(
            NadaVfxEditorTarget target,
            out string reason)
        {
            reason = null;
            if (target?.ItemData == null || target.SourceRoot == null ||
                target.VisualRoot == null)
            {
                reason = "No equipped weapon target.";
                return false;
            }

            if (!object.ReferenceEquals(
                    NadaVfxEditorTargetRegistry.Current?.ItemData,
                    target.ItemData))
            {
                reason = "Equipped weapon changed. Reopen the editor.";
                return false;
            }

            return true;
        }

        private static uint GetNextLegacyId(WeaponVfxState state)
        {
            uint highest = 0;
            foreach (VfxEffectBlock block in state.Effects)
            {
                if (block != null && block.InstanceId > highest)
                    highest = block.InstanceId;
            }
            return highest == uint.MaxValue ? 0 : highest + 1;
        }
    }
}
