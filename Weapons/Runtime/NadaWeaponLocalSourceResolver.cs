using System.Collections.Generic;
using NADA.VFX.Weapon.Core.Persistence;
using NADA.VFX.Weapon.Core.State;
using NADA.VFX.Weapon.Core.State.Blocks;

namespace NADA.VFX.Weapon.Weapons.Runtime
{
    internal enum NadaWeaponLocalSourceKind
    {
        Unbound,
        EditorPreview,
        NativeBound,
        LegacyBound,
        InvalidNative
    }

    /// <summary>
    /// Describes the winning local source; it never migrates legacy state,
    /// modifies ItemData, or starts/stops runtime objects.
    /// </summary>
    internal sealed class NadaWeaponLocalSourceSelection
    {
        internal NadaWeaponLocalSourceKind Kind { get; }
        internal WeaponVfxState State { get; }
        internal uint NextInstanceId { get; }
        internal string FailureReason { get; }

        internal NadaWeaponLocalSourceSelection(
            NadaWeaponLocalSourceKind kind,
            WeaponVfxState state = null,
            uint nextInstanceId = 0,
            string failureReason = null)
        {
            Kind = kind;
            State = state;
            NextInstanceId = nextInstanceId;
            FailureReason = failureReason;
        }
    }

    /// <summary>
    /// Resolves local weapon state ownership without converting it into
    /// legacy VfxState. Remote visuals must use decoded replication instead.
    /// 
    /// Precedence: native (including invalid native), legacy binding,
    /// editor preview, then unbound. An invalid native payload wins with a
    /// failure result so no caller can accidentally fall back to config.
    /// </summary>
    internal static class NadaWeaponLocalSourceResolver
    {
        internal static NadaWeaponLocalSourceSelection Resolve(
            global::ItemDrop.ItemData itemData)
        {
            WeaponVfxStateReadStatus status =
                WeaponVfxItemStore.Read(
                    itemData,
                    out WeaponVfxState nativeState,
                    out uint nextInstanceId,
                    out string reason);

            // A bound item must never be overridden by a stray editor preview.
            // We still collect all inputs here so both entry points have the
            // same deterministic precedence rules.
            bool legacyBound = VfxStateIO.IsBound(itemData);
            NadaWeaponEditorPreviewState.TryGet(
                itemData,
                out WeaponVfxState previewState);

            return Select(
                status,
                nativeState,
                nextInstanceId,
                reason,
                legacyBound,
                previewState);
        }

        // Test seam: no fake ItemData and no Unity objects. Callers must pass
        // the legacy binding fact independently; the store does not own it.
        internal static NadaWeaponLocalSourceSelection Resolve(
            Dictionary<string, string> customData,
            bool legacyBound,
            WeaponVfxState previewState)
        {
            WeaponVfxStateReadStatus status =
                WeaponVfxItemStore.Read(
                    customData,
                    out WeaponVfxState nativeState,
                    out uint nextInstanceId,
                    out string reason);

            return Select(
                status,
                nativeState,
                nextInstanceId,
                reason,
                legacyBound,
                previewState);
        }

        private static NadaWeaponLocalSourceSelection Select(
            WeaponVfxStateReadStatus nativeStatus,
            WeaponVfxState nativeState,
            uint nextInstanceId,
            string nativeFailureReason,
            bool legacyBound,
            WeaponVfxState previewState)
        {
            if (nativeStatus == WeaponVfxStateReadStatus.Invalid)
            {
                return new NadaWeaponLocalSourceSelection(
                    NadaWeaponLocalSourceKind.InvalidNative,
                    failureReason: nativeFailureReason ??
                                   "Native weapon state is invalid.");
            }

            if (nativeStatus == WeaponVfxStateReadStatus.Valid)
            {
                // Even a valid empty block list is an explicit binding.
                // It must not resurrect old legacy effects or config defaults.
                return new NadaWeaponLocalSourceSelection(
                    NadaWeaponLocalSourceKind.NativeBound,
                    nativeState,
                    nextInstanceId);
            }

            if (legacyBound)
            {
                // Only the old local path may explicitly migrate this source.
                return new NadaWeaponLocalSourceSelection(
                    NadaWeaponLocalSourceKind.LegacyBound);
            }

            if (previewState != null)
            {
                return new NadaWeaponLocalSourceSelection(
                    NadaWeaponLocalSourceKind.EditorPreview,
                    previewState);
            }

            return new NadaWeaponLocalSourceSelection(
                NadaWeaponLocalSourceKind.Unbound);
        }
    }
}
