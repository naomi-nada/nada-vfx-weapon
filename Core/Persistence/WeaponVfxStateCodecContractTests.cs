using System;
using System.Collections.Generic;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Blocks.Effects;
using NADA.VFX.Weapon.Core.State.Defaults;

namespace NADA.VFX.Weapon.Core.Persistence
{
    /// <summary>
    /// Temporary, explicit codec verification. Call Run() from a debug
    /// bootstrap when testing; don't execute this every frame or on every bind.
    /// </summary>
    internal static class WeaponVfxStateCodecContractTests
    {
        internal static bool Run()
        {
            try
            {
                TestEmptyState();
                TestAllTenTypesAndDeterminism();
                TestGlueAndDuplicateTypes();
                TestInvalidStateRejection();
                TestMalformedPayloadRejection();
                Plugin.Log?.LogInfo(
                    $"{Plugin.ModName}: [WeaponVfxStateCodecTests OK] tests=5");
                return true;
            }
            catch (Exception error)
            {
                Plugin.Log?.LogError(
                    $"{Plugin.ModName}: [WeaponVfxStateCodecTests FAIL] {error}");
                return false;
            }
        }

        private static void TestEmptyState()
        {
            var state = new WeaponVfxState();
            byte[] encoded = Encode(state, 1);
            Require(WeaponVfxStateCodec.TryDecode(
                encoded, out WeaponVfxState decoded, out uint cursor,
                out string reason), "Empty decode: " + reason);
            Require(decoded.Effects.Count == 0 && cursor == 1,
                "Empty state or initial cursor changed.");
        }

        private static void TestAllTenTypesAndDeterminism()
        {
            string[] types =
            {
                VfxEffectTypeIds.InnerFlames,
                VfxEffectTypeIds.OuterFlames,
                VfxEffectTypeIds.Strands,
                VfxEffectTypeIds.Sparks,
                VfxEffectTypeIds.Flare,
                VfxEffectTypeIds.Aura,
                VfxEffectTypeIds.OrbitalsOrbs,
                VfxEffectTypeIds.OrbitalsCores,
                VfxEffectTypeIds.OrbitalsFlames,
                VfxEffectTypeIds.OrbitalsEmbers
            };
            var state = new WeaponVfxState();
            state.RigTransform.XOffset = 0.375f;
            state.RigTransform.ZRotation = -12.5f;
            for (uint i = 0; i < types.Length; i++)
            {
                VfxEffectBlock block = WeaponVfxBlockDefaults.Create(
                    types[i], i + 1);
                Require(block != null, "Missing default: " + types[i]);
                block.DisplayName = "Effect " + i;
                block.Transform.YRotation = i * 2.25f;
                block.Enabled = i % 2 == 0;
                state.Effects.Add(block);
            }
            ((InnerFlamesVfxSettings)state.Effects[0].Settings).Hue = 0.35f;
            ((OuterFlamesVfxSettings)state.Effects[1].Settings).DragEnabled = true;
            ((StrandsVfxSettings)state.Effects[2].Settings).SpectrumSpeed = 1.7f;
            ((SparksVfxSettings)state.Effects[3].Settings).Width = 2.25f;
            ((FlareVfxSettings)state.Effects[4].Settings).Scale = 0.4f;
            ((AuraVfxSettings)state.Effects[5].Settings).Luminance = 2f;
            ((OrbitalsOrbsVfxSettings)state.Effects[6].Settings).Formation.Path.Speed = 0.12f;
            ((OrbitalsCoresVfxSettings)state.Effects[7].Settings).SpinSpeed = 2.8f;
            ((OrbitalsFlamesVfxSettings)state.Effects[8].Settings).Energy = 0.75f;
            ((OrbitalsEmbersVfxSettings)state.Effects[9].Settings).SimulationSpeed = 1.35f;

            byte[] first = Encode(state, 11);
            Require(WeaponVfxStateCodec.TryDecode(
                first, out WeaponVfxState loaded, out uint cursor,
                out string reason), "Ten-effect decode: " + reason);
            Require(cursor == 11 && loaded.Effects.Count == types.Length,
                "Effect count or cursor changed.");
            for (int i = 0; i < types.Length; i++)
            {
                Require(loaded.Effects[i].TypeId == types[i] &&
                        loaded.Effects[i].InstanceId == i + 1 &&
                        loaded.Effects[i].Enabled == (i % 2 == 0),
                    "Type, order, ID, or enabled flag changed at " + i);
            }
            Require(((OrbitalsCoresVfxSettings)loaded.Effects[7].Settings)
                    .SpinSpeed == 2.8f, "Cores spin speed changed.");
            Require(((StrandsVfxSettings)loaded.Effects[2].Settings)
                    .SpectrumSpeed == 1.7f, "Strands spectrum changed.");
            Require(EqualBytes(first, Encode(loaded, cursor)),
                "All-ten-types re-encoding is not deterministic.");
        }

        private static void TestGlueAndDuplicateTypes()
        {
            var state = new WeaponVfxState();
            VfxEffectBlock leader = WeaponVfxBlockDefaults.Create(
                VfxEffectTypeIds.OrbitalsOrbs, 9);
            VfxEffectBlock follower = WeaponVfxBlockDefaults.Create(
                VfxEffectTypeIds.OrbitalsOrbs, 10);
            VfxEffectBlock flames = WeaponVfxBlockDefaults.Create(
                VfxEffectTypeIds.OrbitalsFlames, 11);
            leader.DisplayName = "Leading ✨";
            leader.Enabled = true;
            follower.Enabled = false;
            ((OrbitalsOrbsVfxSettings)leader.Settings).Formation.GlueLeaderEnabled = true;
            ((OrbitalsOrbsVfxSettings)leader.Settings).Formation.GlueTargetInstanceIds
                .AddRange(new uint[] { 10, 11 });
            ((OrbitalsOrbsVfxSettings)follower.Settings).Formation.Path.Count = 0.3f;
            // Order deliberately differs from numeric IDs.
            state.Effects.Add(flames);
            state.Effects.Add(leader);
            state.Effects.Add(follower);
            byte[] bytes = Encode(state, 25); // ID 24 was previously deleted.
            Require(WeaponVfxStateCodec.TryDecode(
                bytes, out WeaponVfxState loaded, out uint cursor,
                out string reason), "Glue decode: " + reason);
            Require(cursor == 25 && loaded.Effects[0].InstanceId == 11 &&
                    loaded.Effects[1].InstanceId == 9 &&
                    loaded.Effects[2].InstanceId == 10,
                "Ordered duplicate effects lost their identities.");
            var decodedLeader = (OrbitalsOrbsVfxSettings)loaded.Effects[1].Settings;
            Require(decodedLeader.Formation.GlueLeaderEnabled &&
                    decodedLeader.Formation.GlueTargetInstanceIds.Count == 2 &&
                    decodedLeader.Formation.GlueTargetInstanceIds[0] == 10 &&
                    decodedLeader.Formation.GlueTargetInstanceIds[1] == 11,
                "Glue membership or order changed.");
            Require(loaded.Effects[1].DisplayName == "Leading ✨" &&
                    loaded.Effects[2].DisplayName == null &&
                    !loaded.Effects[2].Enabled,
                "Unicode/null display name or disabled follower changed.");
            Require(EqualBytes(bytes, Encode(loaded, cursor)),
                "Glue re-encoding is not deterministic.");
        }

        private static void TestInvalidStateRejection()
        {
            var state = new WeaponVfxState();
            state.Effects.Add(WeaponVfxBlockDefaults.Create(
                VfxEffectTypeIds.Sparks, 7));
            state.Effects.Add(WeaponVfxBlockDefaults.Create(
                VfxEffectTypeIds.Sparks, 7));
            RejectEncode(state, 8, "Duplicate IDs must fail.");
            state.Effects[1].InstanceId = 8;
            RejectEncode(state, 8, "Reused cursor must fail.");
            state.Effects[1].InstanceId = 9;
            ((SparksVfxSettings)state.Effects[1].Settings).Hue = float.NaN;
            RejectEncode(state, 10, "NaN setting must fail.");
            ((SparksVfxSettings)state.Effects[1].Settings).Hue = 0.3f;
            state.Effects[1].TypeId = "unknown_future_effect";
            RejectEncode(state, 10, "Unknown effect must fail.");
            state.Effects[1].TypeId = VfxEffectTypeIds.Sparks;
            state.Effects[1].DisplayName = new string('x', 65);
            RejectEncode(state, 10, "Oversized name must fail.");
        }

        private static void TestMalformedPayloadRejection()
        {
            var state = new WeaponVfxState();
            state.Effects.Add(WeaponVfxBlockDefaults.Create(
                VfxEffectTypeIds.Aura, 42));
            byte[] valid = Encode(state, 43);
            byte[] truncated = new byte[valid.Length - 1];
            Array.Copy(valid, truncated, truncated.Length);
            RejectDecode(truncated, "Truncated payload must fail.");
            byte[] extra = new byte[valid.Length + 1];
            Array.Copy(valid, extra, valid.Length);
            RejectDecode(extra, "Trailing data must fail.");
            byte[] badMagic = (byte[])valid.Clone();
            badMagic[0] ^= 0xff;
            RejectDecode(badMagic, "Incorrect magic must fail.");
            byte[] badVersion = (byte[])valid.Clone();
            badVersion[4] = 99;
            RejectDecode(badVersion, "Unsupported format must fail.");
        }

        private static byte[] Encode(WeaponVfxState state, uint nextId)
        {
            Require(WeaponVfxStateCodec.TryEncode(
                state, nextId, out byte[] bytes, out string reason),
                "Encode failed: " + reason);
            return bytes;
        }

        private static void RejectEncode(WeaponVfxState state, uint nextId, string message)
        {
            Require(!WeaponVfxStateCodec.TryEncode(
                state, nextId, out _, out _), message);
        }

        private static void RejectDecode(byte[] bytes, string message)
        {
            Require(!WeaponVfxStateCodec.TryDecode(
                bytes, out _, out _, out _), message);
        }

        private static bool EqualBytes(byte[] left, byte[] right)
        {
            if (left.Length != right.Length)
                return false;
            for (int i = 0; i < left.Length; i++)
                if (left[i] != right[i])
                    return false;
            return true;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }
    }
}
