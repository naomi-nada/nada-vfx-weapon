using System;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Blocks.Effects;
using NADA.VFX.Weapon.Core.State.Defaults;

namespace NADA.VFX.Weapon.Core.Persistence
{
    internal static class WeaponVfxStyleCodecContractTests
    {
        internal static void Run()
        {
            try
            {
                int passed = 0;
                TestEmpty(); passed++;
                TestDuplicateTypeAndGlue(); passed++;
                TestDetachedSnapshots(); passed++;
                TestUnknownVersion(); passed++;
                TestTruncatedFile(); passed++;
                TestInvalidName(); passed++;
                TestInvalidBlockState(); passed++;
                Plugin.Log?.LogInfo(
                    $"{Plugin.ModName}: [WeaponVfxStyleCodecTests OK] tests={passed}");
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError(
                    $"{Plugin.ModName}: [WeaponVfxStyleCodecTests FAIL] {ex}");
            }
        }

        private static void TestEmpty()
        {
            byte[] bytes = Encode("Clean Slate", new WeaponVfxState(), 1);
            Require(WeaponVfxStyleCodec.TryDecode(bytes, out string name,
                out WeaponVfxState state, out uint cursor, out string reason), reason);
            Require(name == "Clean Slate" && cursor == 1 && state.Effects.Count == 0,
                "Empty style was not preserved.");
            Require(WeaponVfxStyleCodec.TryDecode(
                Encode(name, state, cursor), out _, out _, out _, out _),
                "Empty native style failed round-trip.");
        }

        private static void TestDuplicateTypeAndGlue()
        {
            var state = new WeaponVfxState();
            VfxEffectBlock a = WeaponVfxBlockDefaults.Create(VfxEffectTypeIds.OrbitalsOrbs, 2);
            VfxEffectBlock b = WeaponVfxBlockDefaults.Create(VfxEffectTypeIds.OrbitalsOrbs, 8);
            VfxEffectBlock c = WeaponVfxBlockDefaults.Create(VfxEffectTypeIds.OrbitalsFlames, 10);
            a.DisplayName = "Orbs α";
            ((OrbitalsOrbsVfxSettings)a.Settings).Formation.GlueLeaderEnabled = true;
            ((OrbitalsOrbsVfxSettings)a.Settings).Formation.GlueTargetInstanceIds.Add(10);
            state.Effects.Add(c);
            state.Effects.Add(a);
            state.Effects.Add(b);

            byte[] bytes = Encode("Two Orbs", state, 24);
            Require(WeaponVfxStyleCodec.TryDecode(bytes, out string name,
                out WeaponVfxState decoded, out uint cursor, out string reason), reason);
            Require(name == "Two Orbs" && cursor == 24 && decoded.Effects.Count == 3,
                "Native style metadata lost.");
            Require(decoded.Effects[0].InstanceId == 10 &&
                decoded.Effects[1].InstanceId == 2 &&
                decoded.Effects[2].InstanceId == 8,
                "Native style reordered effects or replaced duplicate IDs.");
            Require(((OrbitalsOrbsVfxSettings)decoded.Effects[1].Settings)
                .Formation.GlueTargetInstanceIds[0] == 10,
                "Native style lost Glue relationship.");
        }

        private static void TestDetachedSnapshots()
        {
            var state = new WeaponVfxState();
            state.Effects.Add(WeaponVfxBlockDefaults.Create(VfxEffectTypeIds.Sparks, 1));
            byte[] bytes = Encode("One Spark", state, 8);
            Require(WeaponVfxStyleCodec.TryDecode(bytes, out _,
                out WeaponVfxState first, out _, out _), "First decode failed.");
            ((SparksVfxSettings)first.Effects[0].Settings).Hue = 0.9f;
            Require(WeaponVfxStyleCodec.TryDecode(bytes, out _,
                out WeaponVfxState second, out _, out _), "Second decode failed.");
            Require(((SparksVfxSettings)second.Effects[0].Settings).Hue == 0f,
                "Loading a style reused mutable settings from a previous load.");
        }

        private static void TestUnknownVersion()
        {
            byte[] bytes = Encode("Version Test", new WeaponVfxState(), 1);
            bytes[4] = 99;
            Require(!WeaponVfxStyleCodec.TryDecode(bytes, out _, out _, out _, out _),
                "Unknown style version must fail closed.");
        }

        private static void TestTruncatedFile()
        {
            byte[] bytes = Encode("Truncate Test", new WeaponVfxState(), 1);
            Array.Resize(ref bytes, bytes.Length - 1);
            Require(!WeaponVfxStyleCodec.TryDecode(bytes, out _, out _, out _, out _),
                "Truncated native style must fail closed.");
        }

        private static void TestInvalidName()
        {
            Require(!WeaponVfxStyleCodec.TryEncode("../escape", new WeaponVfxState(),
                1, out _, out _), "Path-like style name must fail.");
            Require(!WeaponVfxStyleCodec.TryEncode("Default", new WeaponVfxState(),
                1, out _, out _), "Reserved Default style cannot be overwritten.");
        }

        private static void TestInvalidBlockState()
        {
            var state = new WeaponVfxState();
            state.Effects.Add(WeaponVfxBlockDefaults.Create(VfxEffectTypeIds.Sparks, 5));
            Require(!WeaponVfxStyleCodec.TryEncode("Invalid", state, 5,
                out _, out _), "Reused next-ID cursor must fail closed.");
        }

        private static byte[] Encode(string name, WeaponVfxState state, uint cursor)
        {
            Require(WeaponVfxStyleCodec.TryEncode(name, state, cursor,
                out byte[] bytes, out string reason), reason);
            return bytes;
        }

        private static void Require(bool value, string message)
        {
            if (!value) throw new InvalidOperationException(message ?? "Contract failed.");
        }
    }
}
