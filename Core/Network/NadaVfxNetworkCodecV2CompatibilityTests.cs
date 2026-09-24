using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NADA.VFX.Weapon.Core.State;

namespace NADA.VFX.Weapon.Core.Network
{
    internal static class NadaVfxNetworkCodecV2CompatibilityTests
    {
        internal static void Run()
        {
            try
            {
                TestSparseStateRoundTrip();
                TestUnboundPacket();
                TestUnknownFutureRecordIsIgnored();
                TestFutureAppendOnlyRecordIsAccepted();
                TestTruncatedPacketIsRejected();
                TestTrailingPacketDataIsRejected();
                TestDuplicateRecordIsRejected();
                TestUnknownPacketFlagsAreRejected();
                TestNonFinitePayloadIsRejected();
                TestOversizedRecordIsRejectedBySerializer();
                TestOversizedPacketIsRejectedBySerializer();

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [NetworkCodecV2CompatibilityTests OK] " +
                    $"tests=11");
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(
                    $"{Plugin.ModName}: [NetworkCodecV2CompatibilityTests FAIL] {e}");
            }
        }

        private static void TestSparseStateRoundTrip()
        {
            VfxState source =
                default;

            source.InnerFlamesEnabled = true;
            source.InnerFlamesWorldEnabled = true;
            source.InnerFlamesEnergy = 0.50f;
            source.InnerFlamesScale = 1.25f;
            source.InnerFlamesLuminance = 1.10f;
            source.InnerFlamesHue = -0.125f;

            source.AuraEnabled = true;
            source.AuraScale = 1.60f;
            source.AuraLuminance = 1.20f;
            source.AuraHue = 0.25f;

            byte[] bytes =
                NadaVfxNetworkCodecV2.SerializeState(
                    itemHash: 101,
                    revision: 1,
                    bound: true,
                    state: source);

            Require(
                NadaVfxNetworkCodecV2.TryDeserializeState(
                    bytes,
                    out NadaVfxNetworkPacket packet,
                    out VfxState decoded),
                "Sparse state failed to decode.");

            Require(
                packet.Bound,
                "Sparse packet lost its bound flag.");

            Require(
                packet.ItemHash == 101,
                "Sparse packet item hash changed.");

            Require(
                packet.Revision == 1,
                "Sparse packet revision changed.");

            Require(
                packet.Records.Count == 2,
                $"Sparse packet expected 2 records but got {packet.Records.Count}.");

            RequireStatesEqual(
                source,
                decoded,
                "Sparse state");
        }

        private static void TestUnboundPacket()
        {
            VfxState source =
                default;

            // None of this should survive an unbound packet.
            source.InnerFlamesEnabled = true;
            source.InnerFlamesEnergy = 0.75f;
            source.AuraEnabled = true;
            source.AuraScale = 1.50f;

            byte[] bytes =
                NadaVfxNetworkCodecV2.SerializeState(
                    itemHash: 202,
                    revision: 7,
                    bound: false,
                    state: source);

            Require(
                NadaVfxNetworkCodecV2.TryDeserializeState(
                    bytes,
                    out NadaVfxNetworkPacket packet,
                    out VfxState decoded),
                "Unbound packet failed to decode.");

            Require(
                !packet.Bound,
                "Unbound packet decoded as bound.");

            Require(
                packet.Records.Count == 0,
                "Unbound packet contained effect records.");

            RequireStatesEqual(
                default,
                decoded,
                "Unbound state");
        }

        private static void TestUnknownFutureRecordIsIgnored()
        {
            VfxState source =
                default;

            source.AuraEnabled = true;
            source.AuraScale = 1.40f;
            source.AuraLuminance = 1.10f;
            source.AuraHue = 0.10f;

            NadaVfxNetworkPacket basePacket =
                NadaVfxNetworkCodecV2.BuildPacketFromState(
                    itemHash: 303,
                    revision: 2,
                    bound: true,
                    state: source);

            var records =
                new List<NadaVfxNetworkRecord>(
                    basePacket.Records);

            records.Add(
                new NadaVfxNetworkRecord(
                    typeId: 60000,
                    instanceId:
                        NadaVfxNetworkProtocol.CanonicalInstanceId,
                    schemaVersion: 1,
                    payload: new byte[]
                    {
                        0x11,
                        0x22,
                        0x33,
                        0x44
                    }));

            var packet =
                new NadaVfxNetworkPacket(
                    basePacket.ItemHash,
                    basePacket.Revision,
                    true,
                    records);

            byte[] bytes =
                NadaVfxNetworkCodecV2.SerializePacket(
                    packet);

            Require(
                NadaVfxNetworkCodecV2.TryDeserializeState(
                    bytes,
                    out _,
                    out VfxState decoded),
                "Packet containing an unknown future record was rejected.");

            RequireStatesEqual(
                source,
                decoded,
                "Unknown future record");
        }

        private static void TestFutureAppendOnlyRecordIsAccepted()
        {
            VfxState source =
                default;

            source.AuraEnabled = true;
            source.AuraScale = 1.35f;
            source.AuraLuminance = 1.15f;
            source.AuraHue = -0.20f;
            source.AuraXOffset = 0.10f;
            source.AuraYOffset = -0.10f;
            source.AuraZOffset = 0.20f;
            source.AuraXRotation = 10f;
            source.AuraYRotation = 20f;
            source.AuraZRotation = 30f;

            NadaVfxNetworkPacket basePacket =
                NadaVfxNetworkCodecV2.BuildPacketFromState(
                    itemHash: 404,
                    revision: 3,
                    bound: true,
                    state: source);

            Require(
                basePacket.Records.Count == 1,
                "Future-schema test expected exactly one Aura record.");

            NadaVfxNetworkRecord aura =
                basePacket.Records[0];

            byte[] extendedPayload =
                new byte[
                    aura.Payload.Length + 4];

            Buffer.BlockCopy(
                aura.Payload,
                0,
                extendedPayload,
                0,
                aura.Payload.Length);

            // Pretend a future Aura schema appended one float.
            using (var stream =
                   new MemoryStream(
                       extendedPayload,
                       writable: true))
            {
                stream.Position =
                    aura.Payload.Length;

                using var writer =
                    new BinaryWriter(
                        stream);

                writer.Write(
                    123.456f);
            }

            byte futureSchema =
                checked(
                    (byte)(
                        NadaVfxNetworkProtocol.AuraSchemaVersion +
                        1));

            var futureAura =
                new NadaVfxNetworkRecord(
                    aura.TypeId,
                    aura.InstanceId,
                    futureSchema,
                    extendedPayload);

            var packet =
                new NadaVfxNetworkPacket(
                    basePacket.ItemHash,
                    basePacket.Revision,
                    true,
                    new List<NadaVfxNetworkRecord>
                    {
                        futureAura
                    });

            byte[] bytes =
                NadaVfxNetworkCodecV2.SerializePacket(
                    packet);

            Require(
                NadaVfxNetworkCodecV2.TryDeserializeState(
                    bytes,
                    out _,
                    out VfxState decoded),
                "Append-only future Aura schema was rejected.");

            RequireStatesEqual(
                source,
                decoded,
                "Future append-only Aura schema");
        }

        private static void TestTruncatedPacketIsRejected()
        {
            byte[] valid =
                CreateSmallValidPacket();

            Require(
                valid.Length > 1,
                "Valid test packet was unexpectedly tiny.");

            byte[] truncated =
                new byte[
                    valid.Length - 1];

            Buffer.BlockCopy(
                valid,
                0,
                truncated,
                0,
                truncated.Length);

            Require(
                !NadaVfxNetworkCodecV2.TryDeserializeState(
                    truncated,
                    out _,
                    out _),
                "Truncated packet was accepted.");
        }

        private static void TestTrailingPacketDataIsRejected()
        {
            byte[] valid =
                CreateSmallValidPacket();

            byte[] extended =
                new byte[
                    valid.Length + 1];

            Buffer.BlockCopy(
                valid,
                0,
                extended,
                0,
                valid.Length);

            extended[extended.Length - 1] =
                0x7F;

            Require(
                !NadaVfxNetworkCodecV2.TryDeserializePacket(
                    extended,
                    out _),
                "Packet with trailing data was accepted.");
        }

        private static void TestDuplicateRecordIsRejected()
        {
            VfxState source =
                default;

            source.AuraEnabled = true;
            source.AuraScale = 1.25f;

            NadaVfxNetworkPacket basePacket =
                NadaVfxNetworkCodecV2.BuildPacketFromState(
                    itemHash: 505,
                    revision: 4,
                    bound: true,
                    state: source);

            Require(
                basePacket.Records.Count == 1,
                "Duplicate-record test expected exactly one record.");

            NadaVfxNetworkRecord record =
                basePacket.Records[0];

            var duplicatePacket =
                new NadaVfxNetworkPacket(
                    basePacket.ItemHash,
                    basePacket.Revision,
                    true,
                    new List<NadaVfxNetworkRecord>
                    {
                        record,
                        record
                    });

            byte[] bytes =
                NadaVfxNetworkCodecV2.SerializePacket(
                    duplicatePacket);

            Require(
                !NadaVfxNetworkCodecV2.TryDeserializePacket(
                    bytes,
                    out _),
                "Duplicate type + instance record was accepted.");
        }

        private static void TestUnknownPacketFlagsAreRejected()
        {
            byte[] bytes =
                CreateSmallValidPacket();

            // Protocol v2 envelope:
            // 0..3 = magic
            // 4    = protocol version
            // 5    = packet flags
            const int flagsOffset = 5;

            Require(
                bytes.Length > flagsOffset,
                "Packet was too small to contain flags.");

            bytes[flagsOffset] |=
                1 << 7;

            Require(
                !NadaVfxNetworkCodecV2.TryDeserializePacket(
                    bytes,
                    out _),
                "Packet containing unknown flag bits was accepted.");
        }

        private static void TestNonFinitePayloadIsRejected()
        {
            VfxState source =
                default;

            source.FlareEnabled = true;
            source.FlareScale = 1f;

            NadaVfxNetworkPacket basePacket =
                NadaVfxNetworkCodecV2.BuildPacketFromState(
                    itemHash: 606,
                    revision: 5,
                    bound: true,
                    state: source);

            Require(
                basePacket.Records.Count == 1,
                "NaN test expected exactly one Flare record.");

            NadaVfxNetworkRecord flare =
                basePacket.Records[0];

            byte[] payload =
                (byte[])flare.Payload.Clone();

            using (var stream =
                   new MemoryStream(
                       payload,
                       writable: true))
            {
                using var writer =
                    new BinaryWriter(
                        stream);

                writer.Write(
                    float.NaN);
            }

            var invalidFlare =
                new NadaVfxNetworkRecord(
                    flare.TypeId,
                    flare.InstanceId,
                    flare.SchemaVersion,
                    payload);

            var packet =
                new NadaVfxNetworkPacket(
                    basePacket.ItemHash,
                    basePacket.Revision,
                    true,
                    new List<NadaVfxNetworkRecord>
                    {
                        invalidFlare
                    });

            byte[] bytes =
                NadaVfxNetworkCodecV2.SerializePacket(
                    packet);

            Require(
                !NadaVfxNetworkCodecV2.TryDeserializeState(
                    bytes,
                    out _,
                    out _),
                "Packet containing NaN was accepted.");
        }

        private static void TestOversizedRecordIsRejectedBySerializer()
        {
            var oversizedRecord =
                new NadaVfxNetworkRecord(
                    typeId: 60001,
                    instanceId: 0,
                    schemaVersion: 1,
                    payload:
                        new byte[
                            NadaVfxNetworkProtocol
                                .MaxRecordPayloadBytes +
                            1]);

            var packet =
                new NadaVfxNetworkPacket(
                    itemHash: 707,
                    revision: 6,
                    bound: true,
                    records:
                        new List<NadaVfxNetworkRecord>
                        {
                            oversizedRecord
                        });

            bool rejected =
                false;

            try
            {
                NadaVfxNetworkCodecV2.SerializePacket(
                    packet);
            }
            catch (InvalidDataException)
            {
                rejected = true;
            }

            Require(
                rejected,
                "Oversized record was accepted by serializer.");
        }

        private static void TestOversizedPacketIsRejectedBySerializer()
        {
            var records =
                new List<NadaVfxNetworkRecord>();

            // Four 1024-byte records plus envelope/header data exceed
            // the deliberate 4 KB packet budget.
            for (ushort i = 0;
                 i < 4;
                 i++)
            {
                records.Add(
                    new NadaVfxNetworkRecord(
                        typeId: 61000,
                        instanceId: i,
                        schemaVersion: 1,
                        payload:
                            new byte[
                                NadaVfxNetworkProtocol
                                    .MaxRecordPayloadBytes]));
            }

            var packet =
                new NadaVfxNetworkPacket(
                    itemHash: 808,
                    revision: 7,
                    bound: true,
                    records: records);

            bool rejected =
                false;

            try
            {
                NadaVfxNetworkCodecV2.SerializePacket(
                    packet);
            }
            catch (InvalidDataException)
            {
                rejected = true;
            }

            Require(
                rejected,
                "Packet exceeding MaxPacketBytes was accepted by serializer.");
        }

        private static byte[] CreateSmallValidPacket()
        {
            VfxState state =
                default;

            state.AuraEnabled = true;
            state.AuraScale = 1.50f;
            state.AuraLuminance = 1.00f;
            state.AuraHue = 0.10f;

            return NadaVfxNetworkCodecV2.SerializeState(
                itemHash: 999,
                revision: 1,
                bound: true,
                state: state);
        }

        private static void RequireStatesEqual(
            VfxState expected,
            VfxState actual,
            string testName)
        {
            FieldInfo[] fields =
                typeof(VfxState).GetFields(
                    BindingFlags.Instance |
                    BindingFlags.Public);

            object expectedBox =
                expected;

            object actualBox =
                actual;

            foreach (FieldInfo field in
                     fields)
            {
                object expectedValue =
                    field.GetValue(
                        expectedBox);

                object actualValue =
                    field.GetValue(
                        actualBox);

                if (!Equals(
                        expectedValue,
                        actualValue))
                {
                    throw new InvalidOperationException(
                        $"{testName}: field '{field.Name}' mismatch. " +
                        $"Expected='{expectedValue}' actual='{actualValue}'.");
                }
            }
        }

        private static void Require(
            bool condition,
            string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(
                    message);
            }
        }
    }
}