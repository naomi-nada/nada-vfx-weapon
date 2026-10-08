using System;
using System.Collections.Generic;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Blocks.Effects;
using NADA.VFX.Weapon.Core.State.Defaults;

namespace NADA.VFX.Weapon.Core.Persistence
{
    /// <summary>
    /// Startup-only contract tests for native ItemData dictionary storage.
    /// Does not create weapons or change equipped items.
    /// </summary>
    internal static class WeaponVfxItemStoreContractTests
    {
        internal static bool Run()
        {
            try
            {
                TestMissingAndNativeRemoval();
                TestOrderedRoundTripWithGlue();
                TestRejectedWritePreservesExistingData();
                TestMalformedNativeIsNotMissing();
                TestOversizedPayloadRejection();

                Plugin.Log?.LogInfo(
                    $"{Plugin.ModName}: [WeaponVfxItemStoreTests OK] tests=5");
                return true;
            }
            catch (Exception error)
            {
                Plugin.Log?.LogError(
                    $"{Plugin.ModName}: [WeaponVfxItemStoreTests FAIL] {error}");
                return false;
            }
        }

        private static void TestMissingAndNativeRemoval()
        {
            var data = new Dictionary<string, string>
            {
                ["nada.vfx.bound"] = "true",
                ["some.other.mod"] = "keep"
            };

            Require(Read(data, out _, out _) == WeaponVfxStateReadStatus.Missing,
                "Legacy marker alone is not a native binding.");
            Require(!WeaponVfxItemStore.Remove(data),
                "Removing an absent native binding must be a no-op.");

            Require(WeaponVfxItemStore.TryWrite(
                data, new WeaponVfxState(), 1, out string reason),
                "Empty state write failed: " + reason);
            Require(Read(data, out WeaponVfxState loaded, out uint next) ==
                    WeaponVfxStateReadStatus.Valid &&
                    loaded.Effects.Count == 0 && next == 1,
                "Empty native binding did not survive read.");
            Require(WeaponVfxItemStore.Remove(data),
                "Native binding could not be removed.");
            Require(Read(data, out _, out _) == WeaponVfxStateReadStatus.Missing &&
                    data["nada.vfx.bound"] == "true" &&
                    data["some.other.mod"] == "keep",
                "Native removal damaged legacy or unrelated data.");
        }

        private static void TestOrderedRoundTripWithGlue()
        {
            WeaponVfxState original = MakeState();
            var data = new Dictionary<string, string>();
            Require(WeaponVfxItemStore.TryWrite(
                data, original, 42, out string reason),
                "Native write failed: " + reason);
            string payload = data[WeaponVfxItemStore.PayloadKey];
            Require(Read(data, out WeaponVfxState loaded, out uint cursor) ==
                    WeaponVfxStateReadStatus.Valid,
                "Native round trip failed.");
            Require(cursor == 42 && loaded.Effects.Count == 3 &&
                    loaded.Effects[0].InstanceId == 11 &&
                    loaded.Effects[1].InstanceId == 9 &&
                    loaded.Effects[2].InstanceId == 10 &&
                    loaded.Effects[1].DisplayName == "Leading ✨" &&
                    loaded.RigTransform.ZRotation == 17.5f,
                "Effect order, IDs, names, cursor, or transform changed.");
            var formation =
                ((OrbitalsOrbsVfxSettings)loaded.Effects[1].Settings).Formation;
            Require(formation.GlueLeaderEnabled &&
                    formation.GlueTargetInstanceIds.Count == 2 &&
                    formation.GlueTargetInstanceIds[0] == 10 &&
                    formation.GlueTargetInstanceIds[1] == 11,
                "Glue target order or membership changed.");

            Require(WeaponVfxItemStore.TryWrite(
                data, loaded, cursor, out reason) &&
                    data[WeaponVfxItemStore.PayloadKey] == payload,
                "Native payload changed after deterministic re-encode.");
            loaded.Effects[1].DisplayName = "Modified after load";
            Require(original.Effects[1].DisplayName == "Leading ✨",
                "Loaded state still shares the caller's object graph.");
        }

        private static void TestRejectedWritePreservesExistingData()
        {
            var data = new Dictionary<string, string>
            {
                ["some.other.mod"] = "keep"
            };
            WeaponVfxState state = MakeState();
            Require(WeaponVfxItemStore.TryWrite(
                data, state, 42, out string reason),
                "Setup write failed: " + reason);
            string originalPayload = data[WeaponVfxItemStore.PayloadKey];
            state.Effects[0].InstanceId = state.Effects[1].InstanceId;
            Require(!WeaponVfxItemStore.TryWrite(
                data, state, 42, out reason) &&
                    !string.IsNullOrEmpty(reason),
                "Duplicate InstanceId was not rejected.");
            Require(data[WeaponVfxItemStore.PayloadKey] == originalPayload &&
                    data["some.other.mod"] == "keep" &&
                    Read(data, out _, out _) == WeaponVfxStateReadStatus.Valid,
                "Rejected write corrupted a previous save.");
            Require(!WeaponVfxItemStore.TryWrite(
                (Dictionary<string, string>)null,
                new WeaponVfxState(), 1, out reason),
                "Null dictionary write should fail.");
        }

        private static void TestMalformedNativeIsNotMissing()
        {
            var data = new Dictionary<string, string>
            {
                ["nada.vfx.bound"] = "true"
            };
            string[] invalidValues =
            {
                "",
                "not-base64",
                " AQID ",
                Convert.ToBase64String(new byte[] { 1, 2, 3, 4 })
            };
            foreach (string payload in invalidValues)
            {
                data[WeaponVfxItemStore.PayloadKey] = payload;
                WeaponVfxStateReadStatus status =
                    WeaponVfxItemStore.Read(
                        data, out WeaponVfxState state,
                        out uint cursor, out string reason);
                Require(status == WeaponVfxStateReadStatus.Invalid &&
                        state == null && cursor == 0 &&
                        !string.IsNullOrEmpty(reason),
                    "Malformed native payload must be Invalid, not Missing.");
            }
        }

        private static void TestOversizedPayloadRejection()
        {
            var data = new Dictionary<string, string>
            {
                [WeaponVfxItemStore.PayloadKey] = new string('A', 90000)
            };
            Require(WeaponVfxItemStore.Read(
                data, out WeaponVfxState state, out uint cursor,
                out string reason) == WeaponVfxStateReadStatus.Invalid &&
                    state == null && cursor == 0 &&
                    !string.IsNullOrEmpty(reason),
                "Oversized native payload should be rejected before decode.");
        }

        private static WeaponVfxState MakeState()
        {
            var state = new WeaponVfxState();
            state.RigTransform.ZRotation = 17.5f;

            VfxEffectBlock leader = WeaponVfxBlockDefaults.Create(
                VfxEffectTypeIds.OrbitalsOrbs, 9);
            VfxEffectBlock follower = WeaponVfxBlockDefaults.Create(
                VfxEffectTypeIds.OrbitalsOrbs, 10);
            VfxEffectBlock flames = WeaponVfxBlockDefaults.Create(
                VfxEffectTypeIds.OrbitalsFlames, 11);

            leader.DisplayName = "Leading ✨";
            ((OrbitalsOrbsVfxSettings)leader.Settings).Formation.GlueLeaderEnabled = true;
            ((OrbitalsOrbsVfxSettings)leader.Settings).Formation.GlueTargetInstanceIds
                .AddRange(new uint[] { 10, 11 });

            state.Effects.Add(flames);
            state.Effects.Add(leader);
            state.Effects.Add(follower);
            return state;
        }

        private static WeaponVfxStateReadStatus Read(
            Dictionary<string, string> data,
            out WeaponVfxState state,
            out uint next)
        {
            return WeaponVfxItemStore.Read(
                data, out state, out next, out _);
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }
    }
}
