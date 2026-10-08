using System;
using System.Collections.Generic;
using NADA.VFX.Weapon.Core.Persistence;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Defaults;

namespace NADA.VFX.Weapon.Weapons.Runtime
{
    /// <summary>
    /// Startup tests for local state ownership decisions. These only use
    /// dictionaries; they never change a player's equipment or create rigs.
    /// </summary>
    internal static class NadaWeaponLocalSourceResolverContractTests
    {
        internal static bool Run()
        {
            try
            {
                TestUnbound();
                TestPreviewOnly();
                TestLegacyOverridesPreview();
                TestNativeOverridesLegacyAndPreview();
                TestInvalidNativeNeverFallsBack();
                TestRemovingNativeRestoresPriorSource();

                Plugin.Log?.LogInfo(
                    $"{Plugin.ModName}: [WeaponLocalSourceTests OK] tests=6");
                return true;
            }
            catch (Exception error)
            {
                Plugin.Log?.LogError(
                    $"{Plugin.ModName}: [WeaponLocalSourceTests FAIL] {error}");
                return false;
            }
        }

        private static void TestUnbound()
        {
            Check(NadaWeaponLocalSourceResolver.Resolve(
                    (Dictionary<string, string>)null, false, null),
                NadaWeaponLocalSourceKind.Unbound,
                "Null ItemData-equivalent should be unbound.");

            Check(NadaWeaponLocalSourceResolver.Resolve(
                    new Dictionary<string, string>(), false, null),
                NadaWeaponLocalSourceKind.Unbound,
                "Empty customData should be unbound.");
        }

        private static void TestPreviewOnly()
        {
            var preview = new WeaponVfxState();
            NadaWeaponLocalSourceSelection result =
                NadaWeaponLocalSourceResolver.Resolve(
                    new Dictionary<string, string>(), false, preview);

            Check(result, NadaWeaponLocalSourceKind.EditorPreview,
                "Unbound editor preview should win.");
            Require(ReferenceEquals(result.State, preview) &&
                    result.NextInstanceId == 0,
                "Editor preview must retain its in-memory state reference.");
        }

        private static void TestLegacyOverridesPreview()
        {
            var preview = new WeaponVfxState();
            NadaWeaponLocalSourceSelection result =
                NadaWeaponLocalSourceResolver.Resolve(
                    new Dictionary<string, string>(), true, preview);

            Check(result, NadaWeaponLocalSourceKind.LegacyBound,
                "Legacy bound state should override an orphan preview.");
            Require(result.State == null,
                "Legacy state must not be silently converted here.");
        }

        private static void TestNativeOverridesLegacyAndPreview()
        {
            WeaponVfxState state = CreateNativeState();
            var customData = new Dictionary<string, string>
            {
                ["nada.vfx.bound"] = "true"
            };

            Require(WeaponVfxItemStore.TryWrite(
                    customData, state, 27, out string reason),
                "Could not write native setup data: " + reason);

            NadaWeaponLocalSourceSelection result =
                NadaWeaponLocalSourceResolver.Resolve(
                    customData, true, new WeaponVfxState());

            Check(result, NadaWeaponLocalSourceKind.NativeBound,
                "Native state should override legacy and preview.");
            Require(result.State != null &&
                    result.State.Effects.Count == 1 &&
                    result.State.Effects[0].InstanceId == 12 &&
                    result.NextInstanceId == 27,
                "Native selection did not retain saved block identity/cursor.");
            Require(!ReferenceEquals(result.State, state),
                "Native selection must use a decoded state, not the save input.");
        }

        private static void TestInvalidNativeNeverFallsBack()
        {
            var invalid = new Dictionary<string, string>
            {
                ["nada.vfx.bound"] = "true",
                [WeaponVfxItemStore.PayloadKey] = "corrupted-payload"
            };

            NadaWeaponLocalSourceSelection result =
                NadaWeaponLocalSourceResolver.Resolve(
                    invalid, true, new WeaponVfxState());

            Check(result, NadaWeaponLocalSourceKind.InvalidNative,
                "Invalid native data cannot fall back to legacy/preview.");
            Require(result.State == null &&
                    result.NextInstanceId == 0 &&
                    !string.IsNullOrWhiteSpace(result.FailureReason),
                "Invalid native selection must carry a failure reason only.");
        }

        private static void TestRemovingNativeRestoresPriorSource()
        {
            var data = new Dictionary<string, string>();
            Require(WeaponVfxItemStore.TryWrite(
                    data, new WeaponVfxState(), 1, out string reason),
                "Could not write empty native binding: " + reason);

            Check(NadaWeaponLocalSourceResolver.Resolve(data, false, null),
                NadaWeaponLocalSourceKind.NativeBound,
                "An empty saved native state is still bound.");

            Require(WeaponVfxItemStore.Remove(data),
                "Could not remove test native state.");
            Check(NadaWeaponLocalSourceResolver.Resolve(data, false, null),
                NadaWeaponLocalSourceKind.Unbound,
                "Removing native state should return to unbound if legacy absent.");
            Check(NadaWeaponLocalSourceResolver.Resolve(data, true, null),
                NadaWeaponLocalSourceKind.LegacyBound,
                "Legacy binding must remain separately owned after native removal.");
        }

        private static WeaponVfxState CreateNativeState()
        {
            var state = new WeaponVfxState();
            VfxEffectBlock sparks = WeaponVfxBlockDefaults.Create(
                VfxEffectTypeIds.Sparks, 12);
            Require(sparks != null, "Sparks defaults unavailable.");
            state.Effects.Add(sparks);
            return state;
        }

        private static void Check(
            NadaWeaponLocalSourceSelection result,
            NadaWeaponLocalSourceKind expected,
            string failureMessage)
        {
            Require(result != null && result.Kind == expected,
                failureMessage);
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }
    }
}
