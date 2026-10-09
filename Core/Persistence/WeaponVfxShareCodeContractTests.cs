using System;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Blocks.Effects;
using NADA.VFX.Weapon.Core.State.Defaults;

namespace NADA.VFX.Weapon.Core.Persistence
{
    internal static class WeaponVfxShareCodeContractTests
    {
        internal static void Run()
        {
            try
            {
                int passed = 0;
                TestEmptyRoundTrip(); passed++;
                TestOrderedDuplicatesAndGlue(); passed++;
                TestCompressedRoundTrip(); passed++;
                TestSourceAndDecodedStateIndependent(); passed++;
                TestUnknownVersion(); passed++;
                TestCorruptedChecksum(); passed++;
                TestTruncatedCode(); passed++;
                TestOversizeAndInvalidCharacters(); passed++;
                TestInvalidStateRejected(); passed++;
                TestBadCompressionRejected(); passed++;
                Plugin.Log?.LogInfo(
                    $"{Plugin.ModName}: [WeaponVfxShareCodeTests OK] tests={passed}");
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError(
                    $"{Plugin.ModName}: [WeaponVfxShareCodeTests FAIL] {ex}");
            }
        }

        private static void TestEmptyRoundTrip()
        {
            var empty = new WeaponVfxState();
            string text = Encode(empty, 1);
            Require(text.StartsWith("NADA1:", StringComparison.Ordinal),
                "Missing share-code format prefix.");
            Require(WeaponVfxShareCode.TryDecode("  " + text + "  ",
                out WeaponVfxState decoded, out uint cursor, out string reason), reason);
            Require(cursor == 1 && decoded.Effects.Count == 0,
                "Empty rig did not round-trip.");
            Require(text == Encode(empty, 1),
                "Uncompressed empty state must produce the same code twice.");
        }

        private static void TestOrderedDuplicatesAndGlue()
        {
            WeaponVfxState state = MakeGlueRig();
            string code = Encode(state, 17);
            Require(WeaponVfxShareCode.TryDecode(code,
                out WeaponVfxState decoded, out uint cursor, out string reason), reason);
            Require(cursor == 17 && decoded.Effects.Count == 3,
                "Share code lost effects or allocation cursor.");
            Require(decoded.Effects[0].InstanceId == 8 &&
                decoded.Effects[1].InstanceId == 2 &&
                decoded.Effects[2].InstanceId == 10,
                "Share code changed effect order or instance IDs.");
            Require(decoded.Effects[0].TypeId == VfxEffectTypeIds.OrbitalsOrbs &&
                decoded.Effects[1].TypeId == VfxEffectTypeIds.OrbitalsOrbs,
                "Duplicate Orbs were not preserved.");
            Require(decoded.Effects[1].DisplayName == "Blue Orb α" &&
                ((OrbitalsOrbsVfxSettings)decoded.Effects[1].Settings)
                .Formation.GlueTargetInstanceIds[0] == 10,
                "Share code lost display name or Glue target.");
        }

        private static void TestCompressedRoundTrip()
        {
            var state = new WeaponVfxState();
            for (uint id = 1; id <= 40; id++)
                state.Effects.Add(
                    WeaponVfxBlockDefaults.Create(VfxEffectTypeIds.Sparks, id));
            string code = Encode(state, 41);
            byte[] envelope = DecodeEnvelope(code);
            Require(envelope[0] == 1,
                "Repetitive state should use the smaller compressed encoding.");
            Require(WeaponVfxShareCode.TryDecode(code,
                out WeaponVfxState decoded, out uint cursor, out string reason), reason);
            Require(cursor == 41 && decoded.Effects.Count == 40,
                "Compressed state did not round-trip.");
        }

        private static void TestSourceAndDecodedStateIndependent()
        {
            var state = new WeaponVfxState();
            state.Effects.Add(WeaponVfxBlockDefaults.Create(VfxEffectTypeIds.Sparks, 1));
            string code = Encode(state, 2);
            ((SparksVfxSettings)state.Effects[0].Settings).Hue = 0.8f;
            Require(WeaponVfxShareCode.TryDecode(code,
                out WeaponVfxState first, out _, out _), "First import failed.");
            ((SparksVfxSettings)first.Effects[0].Settings).Hue = 0.4f;
            Require(WeaponVfxShareCode.TryDecode(code,
                out WeaponVfxState second, out _, out _), "Second import failed.");
            Require(((SparksVfxSettings)second.Effects[0].Settings).Hue == 0f,
                "Import reused source or previous mutable settings.");
        }

        private static void TestUnknownVersion()
        {
            string code = Encode(new WeaponVfxState(), 1);
            Require(!WeaponVfxShareCode.TryDecode("NADA2:" + code.Substring(6),
                out _, out _, out _), "Future share-code version must fail closed.");
        }

        private static void TestCorruptedChecksum()
        {
            string code = Encode(new WeaponVfxState(), 1);
            byte[] bytes = DecodeEnvelope(code);
            bytes[8] ^= 0x10; // Corrupt the stored CRC, not the state codec.
            Require(!WeaponVfxShareCode.TryDecode(EncodeEnvelope(bytes),
                out _, out _, out _), "Corrupt checksum was accepted.");
        }

        private static void TestTruncatedCode()
        {
            string code = Encode(new WeaponVfxState(), 1);
            Require(!WeaponVfxShareCode.TryDecode(code.Substring(0, 12),
                out _, out _, out _), "Truncated share code was accepted.");
        }

        private static void TestOversizeAndInvalidCharacters()
        {
            Require(!WeaponVfxShareCode.TryDecode("NADA1:" + new string('A', 90000),
                out _, out _, out _), "Oversized code was accepted.");
            Require(!WeaponVfxShareCode.TryDecode("NADA1:@AB?",
                out _, out _, out _), "Invalid code characters were accepted.");
        }

        private static void TestInvalidStateRejected()
        {
            var invalid = new WeaponVfxState();
            invalid.Effects.Add(WeaponVfxBlockDefaults.Create(VfxEffectTypeIds.Sparks, 5));
            Require(!WeaponVfxShareCode.TryEncode(invalid, 5,
                out _, out _), "Invalid allocation cursor was exported.");
        }

        private static void TestBadCompressionRejected()
        {
            byte[] bytes = DecodeEnvelope(Encode(new WeaponVfxState(), 1));
            bytes[0] = 1; // Claim that a raw payload is Deflate compressed.
            Require(!WeaponVfxShareCode.TryDecode(EncodeEnvelope(bytes),
                out _, out _, out _), "Invalid compressed data was accepted.");
        }

        private static WeaponVfxState MakeGlueRig()
        {
            var state = new WeaponVfxState();
            var first = WeaponVfxBlockDefaults.Create(VfxEffectTypeIds.OrbitalsOrbs, 8);
            var leader = WeaponVfxBlockDefaults.Create(VfxEffectTypeIds.OrbitalsOrbs, 2);
            var follower = WeaponVfxBlockDefaults.Create(VfxEffectTypeIds.OrbitalsCores, 10);
            leader.DisplayName = "Blue Orb α";
            ((OrbitalsOrbsVfxSettings)leader.Settings).Formation.GlueLeaderEnabled = true;
            ((OrbitalsOrbsVfxSettings)leader.Settings).Formation.GlueTargetInstanceIds.Add(10);
            state.Effects.Add(first);
            state.Effects.Add(leader);
            state.Effects.Add(follower);
            return state;
        }

        private static string Encode(WeaponVfxState state, uint cursor)
        {
            Require(WeaponVfxShareCode.TryEncode(
                state, cursor, out string code, out string reason), reason);
            return code;
        }

        private static byte[] DecodeEnvelope(string code)
        {
            string text = code.Substring(6).Replace('-', '+').Replace('_', '/');
            return Convert.FromBase64String(text.PadRight((text.Length + 3) / 4 * 4, '='));
        }

        private static string EncodeEnvelope(byte[] bytes) =>
            "NADA1:" + Convert.ToBase64String(bytes).TrimEnd('=')
                .Replace('+', '-').Replace('/', '_');

        private static void Require(bool value, string reason)
        {
            if (!value) throw new InvalidOperationException(reason ?? "Contract failed.");
        }
    }
}
