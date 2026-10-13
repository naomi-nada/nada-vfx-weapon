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
        // v3 places opacity after the fixed header and six rig floats.
        // This offset is used only to construct compatibility/corruption
        // fixtures. It isn't a second runtime serializer.
        private const int OpacityOffset =
            sizeof(uint) + sizeof(ushort) + sizeof(int) +
            sizeof(uint) + 6 * sizeof(float);

        internal static bool Run()
        {
            try
            {
                TestEmptyState();
                TestAllTenTypesAndDeterminism();
                TestGlueAndDuplicateTypes();
                TestInvalidStateRejection();
                TestMalformedPayloadRejection();
                TestOpacityRoundTripAndDeterminism();
                TestFormatOneCompatibility();
                TestFormatTwoCompatibility();
                TestInvalidOpacityRejection();
                TestMalformedOpacityPayloadRejection();

                Plugin.Log?.LogInfo(
                    $"{Plugin.ModName}: [WeaponVfxStateCodecTests OK] tests=10");
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
            Require(decoded.Effects.Count == 0 && cursor == 1 &&
                    decoded.WeaponOpacity == 1f,
                "Empty state, default opacity, or initial cursor changed.");
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

        private static void TestOpacityRoundTripAndDeterminism()
        {
            // Opacity is weapon-wide. An empty effect list must still carry it.
            foreach (float opacity in new[] { 0f, 0.5f, 1f })
            {
                var state = new WeaponVfxState
                {
                    WeaponOpacity = opacity
                };

                byte[] encoded = Encode(state, 1);
                Require(encoded[4] == 3 && encoded[5] == 0,
                    "Opacity payload did not use format 3.");
                Require(WeaponVfxStateCodec.TryDecode(
                    encoded, out WeaponVfxState loaded, out uint cursor,
                    out string reason), "Opacity decode: " + reason);
                Require(loaded.WeaponOpacity == opacity &&
                        loaded.Effects.Count == 0 && cursor == 1,
                    "Weapon-wide opacity or empty effects changed.");
                Require(EqualBytes(encoded, Encode(loaded, cursor)),
                    "Opacity re-encoding is not deterministic.");
            }
        }

        private static void TestFormatOneCompatibility()
        {
            // Construct a v1-layout fixture: no opacity field and no
            // orbital movement byte. Only one orbital is used, so its
            // movement byte is the final byte of the v3 payload.
            var state = new WeaponVfxState
            {
                WeaponOpacity = 0.25f
            };
            VfxEffectBlock orb = WeaponVfxBlockDefaults.Create(
                VfxEffectTypeIds.OrbitalsOrbs, 7);
            orb.DisplayName = "Old Path";
            var settings = (OrbitalsOrbsVfxSettings)orb.Settings;
            settings.Formation.MovementMode = OrbitalsMovementMode.Path;
            settings.Formation.Path.Speed = 0.31f;
            state.Effects.Add(orb);

            byte[] current = Encode(state, 8);
            Require(current[current.Length - 1] ==
                    (byte)OrbitalsMovementMode.Path,
                "v1 fixture layout changed: movement is not the last byte.");
            byte[] old = MakeLegacyPayload(
                current, 1, removeFinalOrbitalMovementByte: true);

            Require(WeaponVfxStateCodec.TryDecode(
                old, out WeaponVfxState loaded, out uint cursor,
                out string reason), "Format 1 decode: " + reason);
            Require(cursor == 8 && loaded.WeaponOpacity == 1f &&
                    loaded.Effects.Count == 1 &&
                    loaded.Effects[0].InstanceId == 7 &&
                    loaded.Effects[0].DisplayName == "Old Path",
                "Format 1 lost its cursor, effect, name, or opacity default.");

            var loadedSettings = (OrbitalsOrbsVfxSettings)loaded.Effects[0].Settings;
            Require(loadedSettings.Formation.MovementMode ==
                        OrbitalsMovementMode.Path &&
                    loadedSettings.Formation.Path.Speed == 0.31f,
                "Format 1 orbital path did not survive migration.");

            // Also verify v1's zero-effect envelope; it has no movement byte.
            byte[] oldEmpty = MakeLegacyPayload(
                Encode(new WeaponVfxState { WeaponOpacity = 0.1f }, 1),
                1, removeFinalOrbitalMovementByte: false);
            Require(WeaponVfxStateCodec.TryDecode(
                oldEmpty, out WeaponVfxState empty, out uint emptyCursor,
                out reason), "Format 1 empty decode: " + reason);
            Require(emptyCursor == 1 && empty.Effects.Count == 0 &&
                    empty.WeaponOpacity == 1f,
                "Format 1 empty state did not default to opaque.");
        }

        private static void TestFormatTwoCompatibility()
        {
            // v2 already stores orbital Path/Attached mode, but no opacity.
            var state = new WeaponVfxState
            {
                WeaponOpacity = 0.4f
            };
            VfxEffectBlock orb = WeaponVfxBlockDefaults.Create(
                VfxEffectTypeIds.OrbitalsOrbs, 12);
            orb.DisplayName = "Old Attached";
            var settings = (OrbitalsOrbsVfxSettings)orb.Settings;
            settings.Formation.MovementMode = OrbitalsMovementMode.Attached;
            settings.Formation.Path.Radius = 1.25f;
            state.Effects.Add(orb);

            byte[] old = MakeLegacyPayload(
                Encode(state, 13), 2,
                removeFinalOrbitalMovementByte: false);

            Require(WeaponVfxStateCodec.TryDecode(
                old, out WeaponVfxState loaded, out uint cursor,
                out string reason), "Format 2 decode: " + reason);
            Require(cursor == 13 && loaded.WeaponOpacity == 1f &&
                    loaded.Effects.Count == 1 &&
                    loaded.Effects[0].InstanceId == 12 &&
                    loaded.Effects[0].DisplayName == "Old Attached",
                "Format 2 lost its cursor, effect, name, or opacity default.");

            var loadedSettings = (OrbitalsOrbsVfxSettings)loaded.Effects[0].Settings;
            Require(loadedSettings.Formation.MovementMode ==
                        OrbitalsMovementMode.Attached &&
                    loadedSettings.Formation.Path.Radius == 1.25f,
                "Format 2 Attached mode or path values changed.");

            // Re-saving a legacy state upgrades its envelope to v3.
            byte[] upgraded = Encode(loaded, cursor);
            Require(upgraded[4] == 3 && upgraded[5] == 0,
                "Re-encoded legacy state did not use format 3.");
        }

        private static void TestInvalidOpacityRejection()
        {
            var state = new WeaponVfxState();
            foreach (float invalid in new[]
                     {
                         float.NaN, float.PositiveInfinity,
                         float.NegativeInfinity, -0.01f, 1.01f
                     })
            {
                state.WeaponOpacity = invalid;
                RejectEncode(state, 1,
                    "Invalid opacity must fail encoding: " + invalid);
            }
        }

        private static void TestMalformedOpacityPayloadRejection()
        {
            byte[] valid = Encode(new WeaponVfxState(), 1);

            foreach (float invalid in new[]
                     {
                         float.NaN, float.PositiveInfinity,
                         float.NegativeInfinity, -0.01f, 1.01f
                     })
            {
                byte[] corrupt = (byte[])valid.Clone();
                byte[] invalidBytes = BitConverter.GetBytes(invalid);
                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(invalidBytes);
                Array.Copy(invalidBytes, 0, corrupt,
                    OpacityOffset, sizeof(float));
                RejectDecode(corrupt,
                    "Invalid opacity must fail decoding: " + invalid);
            }
        }

        private static byte[] MakeLegacyPayload(
            byte[] formatThree,
            ushort oldFormat,
            bool removeFinalOrbitalMovementByte)
        {
            Require(oldFormat == 1 || oldFormat == 2,
                "Legacy fixture must use v1 or v2.");
            Require(formatThree != null &&
                    formatThree.Length >= OpacityOffset + sizeof(float) +
                    sizeof(ushort) +
                    (removeFinalOrbitalMovementByte ? 1 : 0) &&
                    formatThree[4] == 3 && formatThree[5] == 0,
                "Expected a complete format 3 fixture.");
            Require(!removeFinalOrbitalMovementByte || oldFormat == 1,
                "Only format 1 omitted orbital movement bytes.");

            int sourceTailOffset = OpacityOffset + sizeof(float);
            int sourceTailLength = formatThree.Length - sourceTailOffset -
                                   (removeFinalOrbitalMovementByte ? 1 : 0);
            var legacy = new byte[OpacityOffset + sourceTailLength];
            Array.Copy(formatThree, 0, legacy, 0, OpacityOffset);
            Array.Copy(formatThree, sourceTailOffset,
                legacy, OpacityOffset, sourceTailLength);
            legacy[4] = (byte)oldFormat;
            legacy[5] = 0;
            return legacy;
        }

        private static byte[] Encode(WeaponVfxState state, uint nextId)
        {
            Require(WeaponVfxStateCodec.TryEncode(
                state, nextId, out byte[] bytes, out string reason),
                "Encode failed: " + reason);
            return bytes;
        }

        private static void RejectEncode(
            WeaponVfxState state, uint nextId, string message)
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
