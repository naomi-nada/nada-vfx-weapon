using System;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Blocks.Effects;
using NADA.VFX.Weapon.Core.State.Defaults;
using HarmonyLib;

namespace NADA.VFX.Weapon.Core.Network
{
    /// <summary>
    /// Temporary transition tests. A small Game.Start hook runs once per
    /// session so the codec can be verified without changing existing V2
    /// publication or remote rendering. Remove this hook after migration.
    /// </summary>
    internal static class WeaponVfxNetworkCodecContractTests
    {
        internal static bool Run()
        {
            try
            {
                TestUnbound();
                TestEmptyBound();
                TestDuplicateIdentityAndOrder();
                TestOrbitalsGlue();
                TestDeterminism();
                TestInvalidSource();
                TestMalformedHeaders();
                TestMalformedState();
                TestWireBudget();

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [NetworkNativeCodecTests OK] tests=9");
                return true;
            }
            catch (Exception error)
            {
                Plugin.Log.LogError(
                    $"{Plugin.ModName}: [NetworkNativeCodecTests FAIL] {error}");
                return false;
            }
        }

        private static void TestUnbound()
        {
            byte[] bytes = Encode(1001, 5, false, null, 0);
            Require(bytes.Length == 16, "Unbound packet must contain just the header.");
            WeaponVfxNetworkStatePacket packet = Decode(bytes);
            Require(!packet.Bound && packet.ItemHash == 1001 &&
                    packet.Revision == 5 && packet.State == null &&
                    packet.NextInstanceId == 0,
                "Unbound packet changed its identity or acquired state.");
        }

        private static void TestEmptyBound()
        {
            WeaponVfxNetworkStatePacket packet = Decode(
                Encode(2002, 1, true, new WeaponVfxState(), 1));
            Require(packet.Bound && packet.State != null &&
                    packet.State.Effects.Count == 0 && packet.NextInstanceId == 1,
                "Bound empty state lost its binding or cursor.");
        }

        private static void TestDuplicateIdentityAndOrder()
        {
            WeaponVfxState state = CreateDuplicateState();
            WeaponVfxNetworkStatePacket packet = Decode(
                Encode(2003, 7, true, state, 61));

            Require(packet.State.Effects.Count == 2 &&
                    packet.State.Effects[0].InstanceId == 60 &&
                    packet.State.Effects[1].InstanceId == 12 &&
                    packet.State.Effects[0].TypeId == VfxEffectTypeIds.Sparks &&
                    packet.State.Effects[1].TypeId == VfxEffectTypeIds.Sparks &&
                    packet.State.Effects[1].DisplayName == "Second sparks" &&
                    !packet.State.Effects[0].Enabled &&
                    packet.State.RigTransform.ZRotation == 15.25f &&
                    ((SparksVfxSettings)packet.State.Effects[1].Settings).Hue == -0.27f &&
                    packet.NextInstanceId == 61,
                "Duplicate types, order, labels or stable IDs were lost.");
        }

        private static void TestOrbitalsGlue()
        {
            var state = new WeaponVfxState();
            VfxEffectBlock leader = WeaponVfxBlockDefaults.Create(
                VfxEffectTypeIds.OrbitalsOrbs, 5);
            VfxEffectBlock follower = WeaponVfxBlockDefaults.Create(
                VfxEffectTypeIds.OrbitalsEmbers, 9);
            leader.Enabled = true;
            ((OrbitalsOrbsVfxSettings)leader.Settings).Formation.GlueLeaderEnabled = true;
            ((OrbitalsOrbsVfxSettings)leader.Settings).Formation.GlueTargetInstanceIds.Add(9);
            state.Effects.Add(follower);
            state.Effects.Add(leader);

            WeaponVfxNetworkStatePacket packet = Decode(
                Encode(2004, 4, true, state, 10));
            var formation = (OrbitalsOrbsVfxSettings)packet.State.Effects[1].Settings;
            Require(packet.State.Effects[0].InstanceId == 9 &&
                    formation.Formation.GlueLeaderEnabled &&
                    formation.Formation.GlueTargetInstanceIds.Count == 1 &&
                    formation.Formation.GlueTargetInstanceIds[0] == 9,
                "Orbital formation relationship or list order was lost.");
        }

        private static void TestDeterminism()
        {
            WeaponVfxState state = CreateDuplicateState();
            byte[] first = Encode(2099, 3, true, state, 61);
            WeaponVfxNetworkStatePacket packet = Decode(first);
            byte[] second = Encode(
                packet.ItemHash, packet.Revision, packet.Bound,
                packet.State, packet.NextInstanceId);
            Require(Equal(first, second), "Native network round-trip isn't deterministic.");
        }

        private static void TestInvalidSource()
        {
            var state = new WeaponVfxState();
            state.Effects.Add(WeaponVfxBlockDefaults.Create(VfxEffectTypeIds.Sparks, 1));
            state.Effects.Add(WeaponVfxBlockDefaults.Create(VfxEffectTypeIds.Sparks, 1));
            RejectEncode(1, 1, true, state, 2, "Duplicate instance IDs must fail.");
            RejectEncode(0, 1, true, new WeaponVfxState(), 1, "Zero item hash must fail.");
            RejectEncode(1, 0, true, new WeaponVfxState(), 1, "Zero revision must fail.");
            RejectEncode(1, 1, false, new WeaponVfxState(), 1, "Unbound state leakage must fail.");
            RejectEncode(1, 1, true, null, 0, "Missing bound state must fail.");
        }

        private static void TestMalformedHeaders()
        {
            byte[] valid = Encode(3001, 12, false, null, 0);
            byte[] badMagic = (byte[])valid.Clone();
            badMagic[0] ^= 0xff;
            RejectDecode(badMagic, "Corrupt magic must fail.");
            byte[] badVersion = (byte[])valid.Clone();
            badVersion[4] = 99;
            RejectDecode(badVersion, "Future version must fail closed.");
            byte[] badFlags = (byte[])valid.Clone();
            badFlags[5] = 2;
            RejectDecode(badFlags, "Unknown flags must fail closed.");
            byte[] missingHash = (byte[])valid.Clone();
            Array.Clear(missingHash, 6, 4);
            RejectDecode(missingHash, "Zero item hash must fail on decode.");
            byte[] missingRevision = (byte[])valid.Clone();
            Array.Clear(missingRevision, 10, 4);
            RejectDecode(missingRevision, "Zero revision must fail on decode.");
            byte[] extra = new byte[valid.Length + 1];
            Array.Copy(valid, extra, valid.Length);
            RejectDecode(extra, "Trailing bytes must fail.");
            byte[] truncated = new byte[valid.Length - 1];
            Array.Copy(valid, truncated, truncated.Length);
            RejectDecode(truncated, "Truncated packet must fail.");
        }

        private static void TestMalformedState()
        {
            byte[] valid = Encode(3002, 2, true, CreateDuplicateState(), 61);
            byte[] malformed = (byte[])valid.Clone();
            malformed[16] ^= 0xff; // Corrupt native-state magic, not the wire header.
            RejectDecode(malformed, "Corrupt nested state must fail.");
            byte[] badLength = (byte[])valid.Clone();
            badLength[14] = 0;
            badLength[15] = 0;
            RejectDecode(badLength, "Bound packet with no payload must fail.");
            byte[] extraPayload = (byte[])valid.Clone();
            extraPayload[14]--;
            RejectDecode(extraPayload, "Mismatched payload size must fail.");
            byte[] unboundWithPayload = (byte[])valid.Clone();
            unboundWithPayload[5] = 0;
            RejectDecode(unboundWithPayload, "Unbound payload must fail.");
        }

        private static void TestWireBudget()
        {
            var state = new WeaponVfxState();
            for (uint id = 1; id <= 128; id++)
            {
                VfxEffectBlock block = WeaponVfxBlockDefaults.Create(
                    VfxEffectTypeIds.Sparks, id);
                block.DisplayName = new string('x', 64);
                state.Effects.Add(block);
            }
            RejectEncode(4001, 1, true, state, 129,
                "States over wire budget must fail rather than be truncated.");
            RejectDecode(new byte[WeaponVfxNetworkCodec.MaxPacketBytes + 1],
                "Oversized received packet must fail.");
        }

        private static WeaponVfxState CreateDuplicateState()
        {
            var state = new WeaponVfxState();
            state.RigTransform.ZRotation = 15.25f;
            VfxEffectBlock a = WeaponVfxBlockDefaults.Create(
                VfxEffectTypeIds.Sparks, 60);
            VfxEffectBlock b = WeaponVfxBlockDefaults.Create(
                VfxEffectTypeIds.Sparks, 12);
            a.Enabled = false;
            b.DisplayName = "Second sparks";
            ((SparksVfxSettings)b.Settings).Hue = -0.27f;
            state.Effects.Add(a);
            state.Effects.Add(b);
            return state;
        }

        private static byte[] Encode(
            int hash, uint revision, bool bound, WeaponVfxState state, uint cursor)
        {
            Require(WeaponVfxNetworkCodec.TryEncode(
                hash, revision, bound, state, cursor,
                out byte[] bytes, out string reason),
                "Encode failed: " + reason);
            return bytes;
        }

        private static WeaponVfxNetworkStatePacket Decode(byte[] bytes)
        {
            Require(WeaponVfxNetworkCodec.TryDecode(
                bytes, out WeaponVfxNetworkStatePacket packet, out string reason),
                "Decode failed: " + reason);
            return packet;
        }

        private static void RejectEncode(
            int hash, uint revision, bool bound, WeaponVfxState state,
            uint cursor, string message)
        {
            Require(!WeaponVfxNetworkCodec.TryEncode(
                hash, revision, bound, state, cursor, out _, out _), message);
        }

        private static void RejectDecode(byte[] bytes, string message)
        {
            Require(!WeaponVfxNetworkCodec.TryDecode(bytes, out _, out _), message);
        }

        private static bool Equal(byte[] left, byte[] right)
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

    [HarmonyPatch(typeof(global::Game), "Start")]
    internal static class WeaponVfxNetworkCodecStartupTests
    {
        private static bool _ran;

        private static void Postfix()
        {
            if (_ran)
                return;
            _ran = true;
            WeaponVfxNetworkCodecContractTests.Run();
        }
    }
}
