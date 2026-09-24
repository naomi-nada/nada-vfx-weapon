using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NADA.VFX.Weapon.Core.State;

namespace NADA.VFX.Weapon.Core.Network
{
    internal static class NadaVfxNetworkCodecV2
    {
        internal static byte[] SerializeState(
            int itemHash,
            uint revision,
            bool bound,
            VfxState state)
        {
            NadaVfxNetworkPacket packet =
                BuildPacketFromState(
                    itemHash,
                    revision,
                    bound,
                    state);

            return SerializePacket(
                packet);
        }

        internal static bool TryDeserializeState(
            byte[] bytes,
            out NadaVfxNetworkPacket packet,
            out VfxState state)
        {
            packet = null;
            state = default;

            if (!TryDeserializePacket(
                    bytes,
                    out packet))
            {
                return false;
            }

            if (!TryProjectCanonicalState(
                    packet,
                    out state))
            {
                packet = null;
                state = default;
                return false;
            }

            return true;
        }

        internal static NadaVfxNetworkPacket BuildPacketFromState(
            int itemHash,
            uint revision,
            bool bound,
            VfxState state)
        {
            var records =
                new List<NadaVfxNetworkRecord>();

            if (!bound)
            {
                return new NadaVfxNetworkPacket(
                    itemHash,
                    revision,
                    false,
                    records);
            }

            // Zero is the protocol default for the rig transform,
            // so don't spend bytes on a completely untouched transform.
            if (HasRigTransform(state))
            {
                records.Add(
                    BuildRigTransformRecord(
                        state));
            }

            if (state.InnerFlamesEnabled)
            {
                records.Add(
                    BuildInnerFlamesRecord(
                        state));
            }

            if (state.OuterFlamesEnabled)
            {
                records.Add(
                    BuildOuterFlamesRecord(
                        state));
            }

            if (state.StrandsEnabled)
            {
                records.Add(
                    BuildStrandsRecord(
                        state));
            }

            if (state.SparksEnabled)
            {
                records.Add(
                    BuildSparksRecord(
                        state));
            }

            if (state.FlareEnabled)
            {
                records.Add(
                    BuildFlareRecord(
                        state));
            }

            if (state.AuraEnabled)
            {
                records.Add(
                    BuildAuraRecord(
                        state));
            }

            if (state.OrbitalsOrbsEnabled)
            {
                records.Add(
                    BuildOrbitalsOrbsRecord(
                        state));
            }

            if (state.OrbitalsCoresEnabled)
            {
                records.Add(
                    BuildOrbitalsCoresRecord(
                        state));
            }

            if (state.OrbitalsFlamesEnabled)
            {
                records.Add(
                    BuildOrbitalsFlamesRecord(
                        state));
            }

            if (state.OrbitalsEmbersEnabled)
            {
                records.Add(
                    BuildOrbitalsEmbersRecord(
                        state));
            }

            return new NadaVfxNetworkPacket(
                itemHash,
                revision,
                true,
                records);
        }

        internal static byte[] SerializePacket(
            NadaVfxNetworkPacket packet)
        {
            if (packet == null)
                throw new ArgumentNullException(nameof(packet));

            if (packet.Records.Count >
                NadaVfxNetworkProtocol.MaxRecords)
            {
                throw new InvalidDataException(
                    $"Too many NADA VFX records: {packet.Records.Count}.");
            }

            if (!packet.Bound &&
                packet.Records.Count != 0)
            {
                throw new InvalidDataException(
                    "An unbound NADA VFX packet cannot contain effect records.");
            }

            using var stream =
                new MemoryStream();

            // This method owns the MemoryStream. The BinaryWriter is only
            // borrowing it, so disposing the writer must not close the stream
            // before we validate its final size and copy the bytes.
            using (var writer =
                   new BinaryWriter(
                       stream,
                       System.Text.Encoding.UTF8,
                       leaveOpen: true))
            {
                writer.Write(
                    NadaVfxNetworkProtocol.Magic);

                writer.Write(
                    NadaVfxNetworkProtocol.CurrentProtocolVersion);

                var flags =
                    packet.Bound
                        ? NadaVfxNetworkProtocol.PacketFlags.Bound
                        : NadaVfxNetworkProtocol.PacketFlags.None;

                writer.Write(
                    (byte)flags);

                writer.Write(
                    packet.ItemHash);

                writer.Write(
                    packet.Revision);

                writer.Write(
                    (ushort)packet.Records.Count);

                foreach (NadaVfxNetworkRecord record in
                         packet.Records)
                {
                    if (record == null)
                    {
                        throw new InvalidDataException(
                            "NADA VFX packet contains a null record.");
                    }

                    if (record.SchemaVersion == 0)
                    {
                        throw new InvalidDataException(
                            "NADA VFX record schema version cannot be zero.");
                    }

                    int payloadLength =
                        record.Payload?.Length ?? 0;

                    if (payloadLength >
                        NadaVfxNetworkProtocol.MaxRecordPayloadBytes)
                    {
                        throw new InvalidDataException(
                            $"NADA VFX record {record.TypeId} is too large: " +
                            $"{payloadLength} bytes.");
                    }

                    writer.Write(
                        record.TypeId);

                    writer.Write(
                        record.InstanceId);

                    writer.Write(
                        record.SchemaVersion);

                    writer.Write(
                        (ushort)payloadLength);

                    if (payloadLength > 0)
                    {
                        writer.Write(
                            record.Payload);
                    }
                }
            }

            if (stream.Length >
                NadaVfxNetworkProtocol.MaxPacketBytes)
            {
                throw new InvalidDataException(
                    $"NADA VFX packet is too large: {stream.Length} bytes.");
            }

            return stream.ToArray();
        }

        internal static bool TryDeserializePacket(
            byte[] bytes,
            out NadaVfxNetworkPacket packet)
        {
            packet = null;

            if (bytes == null ||
                bytes.Length == 0 ||
                bytes.Length >
                    NadaVfxNetworkProtocol.MaxPacketBytes)
            {
                return false;
            }

            try
            {
                using var stream =
                    new MemoryStream(
                        bytes,
                        writable: false);

                using var reader =
                    new BinaryReader(stream);

                uint magic =
                    reader.ReadUInt32();

                if (magic !=
                    NadaVfxNetworkProtocol.Magic)
                {
                    return false;
                }

                byte protocolVersion =
                    reader.ReadByte();

                if (protocolVersion !=
                    NadaVfxNetworkProtocol.CurrentProtocolVersion)
                {
                    return false;
                }

                var flags =
                    (NadaVfxNetworkProtocol.PacketFlags)
                    reader.ReadByte();
                
                if ((flags &
                     ~NadaVfxNetworkProtocol.KnownPacketFlags) !=
                    NadaVfxNetworkProtocol.PacketFlags.None)
                {
                    return false;
                }

                bool bound =
                    (flags &
                     NadaVfxNetworkProtocol.PacketFlags.Bound) != 0;

                int itemHash =
                    reader.ReadInt32();

                uint revision =
                    reader.ReadUInt32();

                ushort recordCount =
                    reader.ReadUInt16();

                if (recordCount >
                    NadaVfxNetworkProtocol.MaxRecords)
                {
                    return false;
                }

                if (!bound &&
                    recordCount != 0)
                {
                    return false;
                }

                var records =
                    new List<NadaVfxNetworkRecord>(
                        recordCount);

                // Duplicate type + instance IDs are ambiguous.
                // Reject them instead of letting the last one silently win.
                var seenRecords =
                    new HashSet<uint>();

                for (int i = 0;
                     i < recordCount;
                     i++)
                {
                    ushort typeId =
                        reader.ReadUInt16();

                    ushort instanceId =
                        reader.ReadUInt16();

                    byte schemaVersion =
                        reader.ReadByte();

                    ushort payloadLength =
                        reader.ReadUInt16();

                    if (schemaVersion == 0)
                        return false;

                    if (payloadLength >
                        NadaVfxNetworkProtocol
                            .MaxRecordPayloadBytes)
                    {
                        return false;
                    }

                    long remaining =
                        stream.Length -
                        stream.Position;

                    if (remaining <
                        payloadLength)
                    {
                        return false;
                    }

                    uint uniqueRecordKey =
                        ((uint)typeId << 16) |
                        instanceId;

                    if (!seenRecords.Add(
                            uniqueRecordKey))
                    {
                        return false;
                    }

                    byte[] payload =
                        reader.ReadBytes(
                            payloadLength);

                    if (payload.Length !=
                        payloadLength)
                    {
                        return false;
                    }

                    records.Add(
                        new NadaVfxNetworkRecord(
                            typeId,
                            instanceId,
                            schemaVersion,
                            payload));
                }

                if (stream.Position !=
                    stream.Length)
                {
                    return false;
                }

                packet =
                    new NadaVfxNetworkPacket(
                        itemHash,
                        revision,
                        bound,
                        records);

                return true;
            }
            catch
            {
                packet = null;
                return false;
            }
        }

        private static bool TryProjectCanonicalState(
            NadaVfxNetworkPacket packet,
            out VfxState state)
        {
            state = default;

            if (packet == null)
                return false;

            if (!packet.Bound)
                return true;

            VfxState result =
                default;

            foreach (NadaVfxNetworkRecord record in
                     packet.Records)
            {
                if (record == null)
                    return false;

                // Today's VfxState can only represent one instance.
                //
                // The packet format already supports extra instances for
                // future effects, but the current adapter intentionally
                // ignores them.
                if (record.InstanceId !=
                    NadaVfxNetworkProtocol.CanonicalInstanceId)
                {
                    continue;
                }

                bool success;

                switch ((NadaVfxNetworkRecordType)
                        record.TypeId)
                {
                    case NadaVfxNetworkRecordType.RigTransform:
                        success =
                            TryReadRigTransform(
                                record,
                                ref result);
                        break;

                    case NadaVfxNetworkRecordType.InnerFlames:
                        success =
                            TryReadInnerFlames(
                                record,
                                ref result);
                        break;

                    case NadaVfxNetworkRecordType.OuterFlames:
                        success =
                            TryReadOuterFlames(
                                record,
                                ref result);
                        break;

                    case NadaVfxNetworkRecordType.Strands:
                        success =
                            TryReadStrands(
                                record,
                                ref result);
                        break;

                    case NadaVfxNetworkRecordType.Sparks:
                        success =
                            TryReadSparks(
                                record,
                                ref result);
                        break;

                    case NadaVfxNetworkRecordType.Flare:
                        success =
                            TryReadFlare(
                                record,
                                ref result);
                        break;

                    case NadaVfxNetworkRecordType.Aura:
                        success =
                            TryReadAura(
                                record,
                                ref result);
                        break;

                    case NadaVfxNetworkRecordType.OrbitalsOrbs:
                        success =
                            TryReadOrbitalsOrbs(
                                record,
                                ref result);
                        break;

                    case NadaVfxNetworkRecordType.OrbitalsCores:
                        success =
                            TryReadOrbitalsCores(
                                record,
                                ref result);
                        break;

                    case NadaVfxNetworkRecordType.OrbitalsFlames:
                        success =
                            TryReadOrbitalsFlames(
                                record,
                                ref result);
                        break;

                    case NadaVfxNetworkRecordType.OrbitalsEmbers:
                        success =
                            TryReadOrbitalsEmbers(
                                record,
                                ref result);
                        break;

                    default:
                        // Newer effects are safe to ignore.
                        success = true;
                        break;
                }

                if (!success)
                {
                    state = default;
                    return false;
                }
            }

            state = result;
            return true;
        }

        private static bool HasRigTransform(
            VfxState state)
        {
            return
                state.RigXOffset != 0f ||
                state.RigYOffset != 0f ||
                state.RigZOffset != 0f ||
                state.RigXRotation != 0f ||
                state.RigYRotation != 0f ||
                state.RigZRotation != 0f;
        }

        private static NadaVfxNetworkRecord BuildRigTransformRecord(
            VfxState state)
        {
            return BuildRecord(
                NadaVfxNetworkRecordType.RigTransform,
                writer =>
                {
                    WriteFiniteSingle(writer, state.RigXOffset);
                    WriteFiniteSingle(writer, state.RigYOffset);
                    WriteFiniteSingle(writer, state.RigZOffset);
                    WriteFiniteSingle(writer, state.RigXRotation);
                    WriteFiniteSingle(writer, state.RigYRotation);
                    WriteFiniteSingle(writer, state.RigZRotation);
                });
        }

        private static NadaVfxNetworkRecord BuildInnerFlamesRecord(
            VfxState state)
        {
            return BuildRecord(
                NadaVfxNetworkRecordType.InnerFlames,
                writer =>
                {
                    byte flags = 0;

                    if (state.InnerFlamesWorldEnabled)
                        flags |= 1 << 0;

                    if (state.InnerFlamesBlackEnabled)
                        flags |= 1 << 1;

                    if (state.InnerFlamesWhiteEnabled)
                        flags |= 1 << 2;

                    writer.Write(flags);

                    WriteFiniteSingle(writer, state.InnerFlamesEnergy);
                    WriteFiniteSingle(writer, state.InnerFlamesScale);
                    WriteFiniteSingle(writer, state.InnerFlamesLuminance);
                    WriteFiniteSingle(writer, state.InnerFlamesHue);
                    WriteFiniteSingle(writer, state.InnerFlamesLifetime);
                    WriteFiniteSingle(writer, state.InnerFlamesSimulationSpeed);
                    WriteFiniteSingle(writer, state.InnerFlamesLength);
                    WriteFiniteSingle(writer, state.InnerFlamesWidth);
                    WriteFiniteSingle(writer, state.InnerFlamesXOffset);
                    WriteFiniteSingle(writer, state.InnerFlamesYOffset);
                    WriteFiniteSingle(writer, state.InnerFlamesZOffset);
                    WriteFiniteSingle(writer, state.InnerFlamesXRotation);
                    WriteFiniteSingle(writer, state.InnerFlamesYRotation);
                    WriteFiniteSingle(writer, state.InnerFlamesZRotation);
                });
        }

        private static NadaVfxNetworkRecord BuildOuterFlamesRecord(
            VfxState state)
        {
            return BuildRecord(
                NadaVfxNetworkRecordType.OuterFlames,
                writer =>
                {
                    byte flags = 0;

                    if (state.OuterFlamesWorldEnabled)
                        flags |= 1 << 0;

                    if (state.OuterFlamesBlackEnabled)
                        flags |= 1 << 1;

                    if (state.OuterFlamesWhiteEnabled)
                        flags |= 1 << 2;

                    if (state.OuterFlamesDragEnabled)
                        flags |= 1 << 3;

                    writer.Write(flags);

                    WriteFiniteSingle(writer, state.OuterFlamesEnergy);
                    WriteFiniteSingle(writer, state.OuterFlamesScale);
                    WriteFiniteSingle(writer, state.OuterFlamesLuminance);
                    WriteFiniteSingle(writer, state.OuterFlamesHue);
                    WriteFiniteSingle(writer, state.OuterFlamesLifetime);
                    WriteFiniteSingle(writer, state.OuterFlamesSimulationSpeed);
                    WriteFiniteSingle(writer, state.OuterFlamesLength);
                    WriteFiniteSingle(writer, state.OuterFlamesWidth);
                    WriteFiniteSingle(writer, state.OuterFlamesXOffset);
                    WriteFiniteSingle(writer, state.OuterFlamesYOffset);
                    WriteFiniteSingle(writer, state.OuterFlamesZOffset);
                    WriteFiniteSingle(writer, state.OuterFlamesXRotation);
                    WriteFiniteSingle(writer, state.OuterFlamesYRotation);
                    WriteFiniteSingle(writer, state.OuterFlamesZRotation);
                });
        }

        private static NadaVfxNetworkRecord BuildStrandsRecord(
            VfxState state)
        {
            return BuildRecord(
                NadaVfxNetworkRecordType.Strands,
                writer =>
                {
                    byte flags = 0;

                    if (state.StrandsSpectrumEnabled)
                        flags |= 1 << 0;

                    writer.Write(flags);

                    WriteFiniteSingle(writer, state.StrandsEnergy);
                    WriteFiniteSingle(writer, state.StrandsScaleWhole);
                    WriteFiniteSingle(writer, state.StrandsScaleParts);
                    WriteFiniteSingle(writer, state.StrandsLuminance);
                    WriteFiniteSingle(writer, state.StrandsHue);
                    WriteFiniteSingle(writer, state.StrandsLifetime);
                    WriteFiniteSingle(writer, state.StrandsLength);
                    WriteFiniteSingle(writer, state.StrandsSpectrumSpeed);
                    WriteFiniteSingle(writer, state.StrandsSpeed);
                    WriteFiniteSingle(writer, state.StrandsRadius);
                    WriteFiniteSingle(writer, state.StrandsXOffset);
                    WriteFiniteSingle(writer, state.StrandsYOffset);
                    WriteFiniteSingle(writer, state.StrandsZOffset);
                    WriteFiniteSingle(writer, state.StrandsXRotation);
                    WriteFiniteSingle(writer, state.StrandsYRotation);
                    WriteFiniteSingle(writer, state.StrandsZRotation);
                    WriteFiniteSingle(writer, state.StrandsDrift);
                });
        }

        private static NadaVfxNetworkRecord BuildSparksRecord(
            VfxState state)
        {
            return BuildRecord(
                NadaVfxNetworkRecordType.Sparks,
                writer =>
                {
                    WriteFiniteSingle(writer, state.SparksEnergy);
                    WriteFiniteSingle(writer, state.SparksScale);
                    WriteFiniteSingle(writer, state.SparksLuminance);
                    WriteFiniteSingle(writer, state.SparksHue);
                    WriteFiniteSingle(writer, state.SparksLifetime);
                    WriteFiniteSingle(writer, state.SparksSimulationSpeed);
                    WriteFiniteSingle(writer, state.SparksLength);
                    WriteFiniteSingle(writer, state.SparksWidth);
                    WriteFiniteSingle(writer, state.SparksXOffset);
                    WriteFiniteSingle(writer, state.SparksYOffset);
                    WriteFiniteSingle(writer, state.SparksZOffset);
                    WriteFiniteSingle(writer, state.SparksXRotation);
                    WriteFiniteSingle(writer, state.SparksYRotation);
                    WriteFiniteSingle(writer, state.SparksZRotation);
                });
        }

        private static NadaVfxNetworkRecord BuildFlareRecord(
            VfxState state)
        {
            return BuildRecord(
                NadaVfxNetworkRecordType.Flare,
                writer =>
                {
                    WriteFiniteSingle(writer, state.FlareScale);
                    WriteFiniteSingle(writer, state.FlareLuminance);
                    WriteFiniteSingle(writer, state.FlareHue);
                    WriteFiniteSingle(writer, state.FlareXOffset);
                    WriteFiniteSingle(writer, state.FlareYOffset);
                    WriteFiniteSingle(writer, state.FlareZOffset);
                });
        }

        private static NadaVfxNetworkRecord BuildAuraRecord(
            VfxState state)
        {
            return BuildRecord(
                NadaVfxNetworkRecordType.Aura,
                writer =>
                {
                    WriteFiniteSingle(writer, state.AuraScale);
                    WriteFiniteSingle(writer, state.AuraLuminance);
                    WriteFiniteSingle(writer, state.AuraHue);
                    WriteFiniteSingle(writer, state.AuraXOffset);
                    WriteFiniteSingle(writer, state.AuraYOffset);
                    WriteFiniteSingle(writer, state.AuraZOffset);
                    WriteFiniteSingle(writer, state.AuraXRotation);
                    WriteFiniteSingle(writer, state.AuraYRotation);
                    WriteFiniteSingle(writer, state.AuraZRotation);
                });
        }

        private static NadaVfxNetworkRecord BuildOrbitalsOrbsRecord(
            VfxState state)
        {
            return BuildRecord(
                NadaVfxNetworkRecordType.OrbitalsOrbs,
                writer =>
                {
                    byte flags = 0;

                    if (state.OrbitalsOrbsSnakeEnabled)
                        flags |= 1 << 0;

                    if (state.OrbitalsOrbsGlueEnabled)
                        flags |= 1 << 1;

                    writer.Write(flags);

                    WriteFiniteSingle(writer, state.OrbitalsOrbsCount);
                    WriteFiniteSingle(writer, state.OrbitalsOrbsScale);
                    WriteFiniteSingle(writer, state.OrbitalsOrbsLuminance);
                    WriteFiniteSingle(writer, state.OrbitalsOrbsHue);
                    WriteFiniteSingle(writer, state.OrbitalsOrbsSpeed);
                    WriteFiniteSingle(writer, state.OrbitalsOrbsSpacing);
                    WriteFiniteSingle(writer, state.OrbitalsOrbsLength);
                    WriteFiniteSingle(writer, state.OrbitalsOrbsRadius);
                    WriteFiniteSingle(writer, state.OrbitalsOrbsCycles);
                    WriteFiniteSingle(writer, state.OrbitalsOrbsXOffset);
                    WriteFiniteSingle(writer, state.OrbitalsOrbsYOffset);
                    WriteFiniteSingle(writer, state.OrbitalsOrbsZOffset);
                    WriteFiniteSingle(writer, state.OrbitalsOrbsXRotation);
                    WriteFiniteSingle(writer, state.OrbitalsOrbsYRotation);
                    WriteFiniteSingle(writer, state.OrbitalsOrbsZRotation);
                    WriteFiniteSingle(writer, state.OrbitalsOrbsDrift);
                });
        }

        private static NadaVfxNetworkRecord BuildOrbitalsCoresRecord(
            VfxState state)
        {
            return BuildRecord(
                NadaVfxNetworkRecordType.OrbitalsCores,
                writer =>
                {
                    byte flags = 0;

                    if (state.OrbitalsCoresSnakeEnabled)
                        flags |= 1 << 0;

                    if (state.OrbitalsCoresGlueEnabled)
                        flags |= 1 << 1;

                    if (state.OrbitalsCoresSpinEnabled)
                        flags |= 1 << 2;

                    writer.Write(flags);

                    WriteFiniteSingle(writer, state.OrbitalsCoresCount);
                    WriteFiniteSingle(writer, state.OrbitalsCoresScale);
                    WriteFiniteSingle(writer, state.OrbitalsCoresLuminance);
                    WriteFiniteSingle(writer, state.OrbitalsCoresHue);
                    WriteFiniteSingle(writer, state.OrbitalsCoresSpinSpeed);
                    WriteFiniteSingle(writer, state.OrbitalsCoresLength);
                    WriteFiniteSingle(writer, state.OrbitalsCoresSpeed);
                    WriteFiniteSingle(writer, state.OrbitalsCoresSpacing);
                    WriteFiniteSingle(writer, state.OrbitalsCoresRadius);
                    WriteFiniteSingle(writer, state.OrbitalsCoresCycles);
                    WriteFiniteSingle(writer, state.OrbitalsCoresXOffset);
                    WriteFiniteSingle(writer, state.OrbitalsCoresYOffset);
                    WriteFiniteSingle(writer, state.OrbitalsCoresZOffset);
                    WriteFiniteSingle(writer, state.OrbitalsCoresXRotation);
                    WriteFiniteSingle(writer, state.OrbitalsCoresYRotation);
                    WriteFiniteSingle(writer, state.OrbitalsCoresZRotation);
                    WriteFiniteSingle(writer, state.OrbitalsCoresDrift);
                });
        }

        private static NadaVfxNetworkRecord BuildOrbitalsFlamesRecord(
            VfxState state)
        {
            return BuildRecord(
                NadaVfxNetworkRecordType.OrbitalsFlames,
                writer =>
                {
                    WriteFiniteSingle(writer, state.OrbitalsFlamesCount);
                    WriteFiniteSingle(writer, state.OrbitalsFlamesEnergy);
                    WriteFiniteSingle(writer, state.OrbitalsFlamesScale);
                    WriteFiniteSingle(writer, state.OrbitalsFlamesLuminance);
                    WriteFiniteSingle(writer, state.OrbitalsFlamesHue);
                    WriteFiniteSingle(writer, state.OrbitalsFlamesLifetime);
                    WriteFiniteSingle(writer, state.OrbitalsFlamesSimulationSpeed);
                    WriteFiniteSingle(writer, state.OrbitalsFlamesLength);
                    WriteFiniteSingle(writer, state.OrbitalsFlamesSpeed);
                    WriteFiniteSingle(writer, state.OrbitalsFlamesSpacing);
                    WriteFiniteSingle(writer, state.OrbitalsFlamesRadius);
                    WriteFiniteSingle(writer, state.OrbitalsFlamesCycles);
                    WriteFiniteSingle(writer, state.OrbitalsFlamesXOffset);
                    WriteFiniteSingle(writer, state.OrbitalsFlamesYOffset);
                    WriteFiniteSingle(writer, state.OrbitalsFlamesZOffset);
                    WriteFiniteSingle(writer, state.OrbitalsFlamesXRotation);
                    WriteFiniteSingle(writer, state.OrbitalsFlamesYRotation);
                    WriteFiniteSingle(writer, state.OrbitalsFlamesZRotation);
                    WriteFiniteSingle(writer, state.OrbitalsFlamesDrift);
                });
        }

        private static NadaVfxNetworkRecord BuildOrbitalsEmbersRecord(
            VfxState state)
        {
            return BuildRecord(
                NadaVfxNetworkRecordType.OrbitalsEmbers,
                writer =>
                {
                    WriteFiniteSingle(writer, state.OrbitalsEmbersCount);
                    WriteFiniteSingle(writer, state.OrbitalsEmbersEnergy);
                    WriteFiniteSingle(writer, state.OrbitalsEmbersScale);
                    WriteFiniteSingle(writer, state.OrbitalsEmbersLuminance);
                    WriteFiniteSingle(writer, state.OrbitalsEmbersHue);
                    WriteFiniteSingle(writer, state.OrbitalsEmbersLifetime);
                    WriteFiniteSingle(writer, state.OrbitalsEmbersSimulationSpeed);
                    WriteFiniteSingle(writer, state.OrbitalsEmbersLength);
                    WriteFiniteSingle(writer, state.OrbitalsEmbersSpeed);
                    WriteFiniteSingle(writer, state.OrbitalsEmbersSpacing);
                    WriteFiniteSingle(writer, state.OrbitalsEmbersRadius);
                    WriteFiniteSingle(writer, state.OrbitalsEmbersCycles);
                    WriteFiniteSingle(writer, state.OrbitalsEmbersXOffset);
                    WriteFiniteSingle(writer, state.OrbitalsEmbersYOffset);
                    WriteFiniteSingle(writer, state.OrbitalsEmbersZOffset);
                    WriteFiniteSingle(writer, state.OrbitalsEmbersXRotation);
                    WriteFiniteSingle(writer, state.OrbitalsEmbersYRotation);
                    WriteFiniteSingle(writer, state.OrbitalsEmbersZRotation);
                    WriteFiniteSingle(writer, state.OrbitalsEmbersDrift);
                });
        }

        private static NadaVfxNetworkRecord BuildRecord(
            NadaVfxNetworkRecordType type,
            Action<BinaryWriter> writePayload)
        {
            using var stream =
                new MemoryStream();

            // Same ownership rule as SerializePacket(): BuildRecord owns the
            // MemoryStream and the writer only borrows it.
            using (var writer =
                   new BinaryWriter(
                       stream,
                       System.Text.Encoding.UTF8,
                       leaveOpen: true))
            {
                writePayload(writer);
            }

            byte[] payload =
                stream.ToArray();

            if (payload.Length >
                NadaVfxNetworkProtocol.MaxRecordPayloadBytes)
            {
                throw new InvalidDataException(
                    $"NADA VFX record '{type}' is too large: " +
                    $"{payload.Length} bytes.");
            }

            return new NadaVfxNetworkRecord(
                (ushort)type,
                NadaVfxNetworkProtocol.CanonicalInstanceId,
                NadaVfxNetworkProtocol.GetCurrentRecordSchemaVersion(type),
                payload);
        }

        private static bool TryReadRigTransform(
            NadaVfxNetworkRecord record,
            ref VfxState state)
        {
            try
            {
                using var stream =
                    OpenRecord(record);

                using var reader =
                    new BinaryReader(stream);

                state.RigXOffset = ReadFiniteSingle(reader);
                state.RigYOffset = ReadFiniteSingle(reader);
                state.RigZOffset = ReadFiniteSingle(reader);
                state.RigXRotation = ReadFiniteSingle(reader);
                state.RigYRotation = ReadFiniteSingle(reader);
                state.RigZRotation = ReadFiniteSingle(reader);

                return ValidateKnownRecordTail(
                    record,
                    stream,
                    NadaVfxNetworkProtocol.RigTransformSchemaVersion);
            }
            catch
            {
                return false;
            }
        }

        private static bool TryReadInnerFlames(
            NadaVfxNetworkRecord record,
            ref VfxState state)
        {
            try
            {
                using var stream =
                    OpenRecord(record);

                using var reader =
                    new BinaryReader(stream);

                byte flags =
                    reader.ReadByte();

                state.InnerFlamesEnabled = true;
                state.InnerFlamesWorldEnabled = (flags & (1 << 0)) != 0;
                state.InnerFlamesBlackEnabled = (flags & (1 << 1)) != 0;
                state.InnerFlamesWhiteEnabled = (flags & (1 << 2)) != 0;

                state.InnerFlamesEnergy = ReadFiniteSingle(reader);
                state.InnerFlamesScale = ReadFiniteSingle(reader);
                state.InnerFlamesLuminance = ReadFiniteSingle(reader);
                state.InnerFlamesHue = ReadFiniteSingle(reader);
                state.InnerFlamesLifetime = ReadFiniteSingle(reader);
                state.InnerFlamesSimulationSpeed = ReadFiniteSingle(reader);
                state.InnerFlamesLength = ReadFiniteSingle(reader);
                state.InnerFlamesWidth = ReadFiniteSingle(reader);
                state.InnerFlamesXOffset = ReadFiniteSingle(reader);
                state.InnerFlamesYOffset = ReadFiniteSingle(reader);
                state.InnerFlamesZOffset = ReadFiniteSingle(reader);
                state.InnerFlamesXRotation = ReadFiniteSingle(reader);
                state.InnerFlamesYRotation = ReadFiniteSingle(reader);
                state.InnerFlamesZRotation = ReadFiniteSingle(reader);

                return ValidateKnownRecordTail(
                    record,
                    stream,
                    NadaVfxNetworkProtocol.InnerFlamesSchemaVersion);
            }
            catch
            {
                return false;
            }
        }

        private static bool TryReadOuterFlames(
            NadaVfxNetworkRecord record,
            ref VfxState state)
        {
            try
            {
                using var stream =
                    OpenRecord(record);

                using var reader =
                    new BinaryReader(stream);

                byte flags =
                    reader.ReadByte();

                state.OuterFlamesEnabled = true;
                state.OuterFlamesWorldEnabled = (flags & (1 << 0)) != 0;
                state.OuterFlamesBlackEnabled = (flags & (1 << 1)) != 0;
                state.OuterFlamesWhiteEnabled = (flags & (1 << 2)) != 0;
                state.OuterFlamesDragEnabled = (flags & (1 << 3)) != 0;

                state.OuterFlamesEnergy = ReadFiniteSingle(reader);
                state.OuterFlamesScale = ReadFiniteSingle(reader);
                state.OuterFlamesLuminance = ReadFiniteSingle(reader);
                state.OuterFlamesHue = ReadFiniteSingle(reader);
                state.OuterFlamesLifetime = ReadFiniteSingle(reader);
                state.OuterFlamesSimulationSpeed = ReadFiniteSingle(reader);
                state.OuterFlamesLength = ReadFiniteSingle(reader);
                state.OuterFlamesWidth = ReadFiniteSingle(reader);
                state.OuterFlamesXOffset = ReadFiniteSingle(reader);
                state.OuterFlamesYOffset = ReadFiniteSingle(reader);
                state.OuterFlamesZOffset = ReadFiniteSingle(reader);
                state.OuterFlamesXRotation = ReadFiniteSingle(reader);
                state.OuterFlamesYRotation = ReadFiniteSingle(reader);
                state.OuterFlamesZRotation = ReadFiniteSingle(reader);

                return ValidateKnownRecordTail(
                    record,
                    stream,
                    NadaVfxNetworkProtocol.InnerFlamesSchemaVersion);
            }
            catch
            {
                return false;
            }
        }

        private static bool TryReadStrands(
            NadaVfxNetworkRecord record,
            ref VfxState state)
        {
            try
            {
                using var stream =
                    OpenRecord(record);

                using var reader =
                    new BinaryReader(stream);

                byte flags =
                    reader.ReadByte();

                state.StrandsEnabled = true;
                state.StrandsSpectrumEnabled = (flags & (1 << 0)) != 0;

                state.StrandsEnergy = ReadFiniteSingle(reader);
                state.StrandsScaleWhole = ReadFiniteSingle(reader);
                state.StrandsScaleParts = ReadFiniteSingle(reader);
                state.StrandsLuminance = ReadFiniteSingle(reader);
                state.StrandsHue = ReadFiniteSingle(reader);
                state.StrandsLifetime = ReadFiniteSingle(reader);
                state.StrandsLength = ReadFiniteSingle(reader);
                state.StrandsSpectrumSpeed = ReadFiniteSingle(reader);
                state.StrandsSpeed = ReadFiniteSingle(reader);
                state.StrandsRadius = ReadFiniteSingle(reader);
                state.StrandsXOffset = ReadFiniteSingle(reader);
                state.StrandsYOffset = ReadFiniteSingle(reader);
                state.StrandsZOffset = ReadFiniteSingle(reader);
                state.StrandsXRotation = ReadFiniteSingle(reader);
                state.StrandsYRotation = ReadFiniteSingle(reader);
                state.StrandsZRotation = ReadFiniteSingle(reader);
                state.StrandsDrift = ReadFiniteSingle(reader);

                return ValidateKnownRecordTail(
                    record,
                    stream,
                    NadaVfxNetworkProtocol.StrandsSchemaVersion);
            }
            catch
            {
                return false;
            }
        }

        private static bool TryReadSparks(
            NadaVfxNetworkRecord record,
            ref VfxState state)
        {
            try
            {
                using var stream =
                    OpenRecord(record);

                using var reader =
                    new BinaryReader(stream);

                state.SparksEnabled = true;

                state.SparksEnergy = ReadFiniteSingle(reader);
                state.SparksScale = ReadFiniteSingle(reader);
                state.SparksLuminance = ReadFiniteSingle(reader);
                state.SparksHue = ReadFiniteSingle(reader);
                state.SparksLifetime = ReadFiniteSingle(reader);
                state.SparksSimulationSpeed = ReadFiniteSingle(reader);
                state.SparksLength = ReadFiniteSingle(reader);
                state.SparksWidth = ReadFiniteSingle(reader);
                state.SparksXOffset = ReadFiniteSingle(reader);
                state.SparksYOffset = ReadFiniteSingle(reader);
                state.SparksZOffset = ReadFiniteSingle(reader);
                state.SparksXRotation = ReadFiniteSingle(reader);
                state.SparksYRotation = ReadFiniteSingle(reader);
                state.SparksZRotation = ReadFiniteSingle(reader);

                return ValidateKnownRecordTail(
                    record,
                    stream,
                    NadaVfxNetworkProtocol.SparksSchemaVersion);
            }
            catch
            {
                return false;
            }
        }

        private static bool TryReadFlare(
            NadaVfxNetworkRecord record,
            ref VfxState state)
        {
            try
            {
                using var stream =
                    OpenRecord(record);

                using var reader =
                    new BinaryReader(stream);

                state.FlareEnabled = true;

                state.FlareScale = ReadFiniteSingle(reader);
                state.FlareLuminance = ReadFiniteSingle(reader);
                state.FlareHue = ReadFiniteSingle(reader);
                state.FlareXOffset = ReadFiniteSingle(reader);
                state.FlareYOffset = ReadFiniteSingle(reader);
                state.FlareZOffset = ReadFiniteSingle(reader);

                return ValidateKnownRecordTail(
                    record,
                    stream,
                    NadaVfxNetworkProtocol.FlareSchemaVersion);
            }
            catch
            {
                return false;
            }
        }

        private static bool TryReadAura(
            NadaVfxNetworkRecord record,
            ref VfxState state)
        {
            try
            {
                using var stream =
                    OpenRecord(record);

                using var reader =
                    new BinaryReader(stream);

                state.AuraEnabled = true;

                state.AuraScale = ReadFiniteSingle(reader);
                state.AuraLuminance = ReadFiniteSingle(reader);
                state.AuraHue = ReadFiniteSingle(reader);
                state.AuraXOffset = ReadFiniteSingle(reader);
                state.AuraYOffset = ReadFiniteSingle(reader);
                state.AuraZOffset = ReadFiniteSingle(reader);
                state.AuraXRotation = ReadFiniteSingle(reader);
                state.AuraYRotation = ReadFiniteSingle(reader);
                state.AuraZRotation = ReadFiniteSingle(reader);

                return ValidateKnownRecordTail(
                    record,
                    stream,
                    NadaVfxNetworkProtocol.AuraSchemaVersion);
            }
            catch
            {
                return false;
            }
        }

        private static bool TryReadOrbitalsOrbs(
            NadaVfxNetworkRecord record,
            ref VfxState state)
        {
            try
            {
                using var stream =
                    OpenRecord(record);

                using var reader =
                    new BinaryReader(stream);

                byte flags =
                    reader.ReadByte();

                state.OrbitalsOrbsEnabled = true;
                state.OrbitalsOrbsSnakeEnabled = (flags & (1 << 0)) != 0;
                state.OrbitalsOrbsGlueEnabled = (flags & (1 << 1)) != 0;

                state.OrbitalsOrbsCount = ReadFiniteSingle(reader);
                state.OrbitalsOrbsScale = ReadFiniteSingle(reader);
                state.OrbitalsOrbsLuminance = ReadFiniteSingle(reader);
                state.OrbitalsOrbsHue = ReadFiniteSingle(reader);
                state.OrbitalsOrbsSpeed = ReadFiniteSingle(reader);
                state.OrbitalsOrbsSpacing = ReadFiniteSingle(reader);
                state.OrbitalsOrbsLength = ReadFiniteSingle(reader);
                state.OrbitalsOrbsRadius = ReadFiniteSingle(reader);
                state.OrbitalsOrbsCycles = ReadFiniteSingle(reader);
                state.OrbitalsOrbsXOffset = ReadFiniteSingle(reader);
                state.OrbitalsOrbsYOffset = ReadFiniteSingle(reader);
                state.OrbitalsOrbsZOffset = ReadFiniteSingle(reader);
                state.OrbitalsOrbsXRotation = ReadFiniteSingle(reader);
                state.OrbitalsOrbsYRotation = ReadFiniteSingle(reader);
                state.OrbitalsOrbsZRotation = ReadFiniteSingle(reader);
                state.OrbitalsOrbsDrift = ReadFiniteSingle(reader);

                return ValidateKnownRecordTail(
                    record,
                    stream,
                    NadaVfxNetworkProtocol.OrbitalsOrbsSchemaVersion);
            }
            catch
            {
                return false;
            }
        }

        private static bool TryReadOrbitalsCores(
            NadaVfxNetworkRecord record,
            ref VfxState state)
        {
            try
            {
                using var stream =
                    OpenRecord(record);

                using var reader =
                    new BinaryReader(stream);

                byte flags =
                    reader.ReadByte();

                state.OrbitalsCoresEnabled = true;
                state.OrbitalsCoresSnakeEnabled = (flags & (1 << 0)) != 0;
                state.OrbitalsCoresGlueEnabled = (flags & (1 << 1)) != 0;
                state.OrbitalsCoresSpinEnabled = (flags & (1 << 2)) != 0;

                state.OrbitalsCoresCount = ReadFiniteSingle(reader);
                state.OrbitalsCoresScale = ReadFiniteSingle(reader);
                state.OrbitalsCoresLuminance = ReadFiniteSingle(reader);
                state.OrbitalsCoresHue = ReadFiniteSingle(reader);
                state.OrbitalsCoresSpinSpeed = ReadFiniteSingle(reader);
                state.OrbitalsCoresLength = ReadFiniteSingle(reader);
                state.OrbitalsCoresSpeed = ReadFiniteSingle(reader);
                state.OrbitalsCoresSpacing = ReadFiniteSingle(reader);
                state.OrbitalsCoresRadius = ReadFiniteSingle(reader);
                state.OrbitalsCoresCycles = ReadFiniteSingle(reader);
                state.OrbitalsCoresXOffset = ReadFiniteSingle(reader);
                state.OrbitalsCoresYOffset = ReadFiniteSingle(reader);
                state.OrbitalsCoresZOffset = ReadFiniteSingle(reader);
                state.OrbitalsCoresXRotation = ReadFiniteSingle(reader);
                state.OrbitalsCoresYRotation = ReadFiniteSingle(reader);
                state.OrbitalsCoresZRotation = ReadFiniteSingle(reader);
                state.OrbitalsCoresDrift = ReadFiniteSingle(reader);

                return ValidateKnownRecordTail(
                    record,
                    stream,
                    NadaVfxNetworkProtocol.OrbitalsCoresSchemaVersion);
            }
            catch
            {
                return false;
            }
        }

        private static bool TryReadOrbitalsFlames(
            NadaVfxNetworkRecord record,
            ref VfxState state)
        {
            try
            {
                using var stream =
                    OpenRecord(record);

                using var reader =
                    new BinaryReader(stream);

                state.OrbitalsFlamesEnabled = true;

                state.OrbitalsFlamesCount = ReadFiniteSingle(reader);
                state.OrbitalsFlamesEnergy = ReadFiniteSingle(reader);
                state.OrbitalsFlamesScale = ReadFiniteSingle(reader);
                state.OrbitalsFlamesLuminance = ReadFiniteSingle(reader);
                state.OrbitalsFlamesHue = ReadFiniteSingle(reader);
                state.OrbitalsFlamesLifetime = ReadFiniteSingle(reader);
                state.OrbitalsFlamesSimulationSpeed = ReadFiniteSingle(reader);
                state.OrbitalsFlamesLength = ReadFiniteSingle(reader);
                state.OrbitalsFlamesSpeed = ReadFiniteSingle(reader);
                state.OrbitalsFlamesSpacing = ReadFiniteSingle(reader);
                state.OrbitalsFlamesRadius = ReadFiniteSingle(reader);
                state.OrbitalsFlamesCycles = ReadFiniteSingle(reader);
                state.OrbitalsFlamesXOffset = ReadFiniteSingle(reader);
                state.OrbitalsFlamesYOffset = ReadFiniteSingle(reader);
                state.OrbitalsFlamesZOffset = ReadFiniteSingle(reader);
                state.OrbitalsFlamesXRotation = ReadFiniteSingle(reader);
                state.OrbitalsFlamesYRotation = ReadFiniteSingle(reader);
                state.OrbitalsFlamesZRotation = ReadFiniteSingle(reader);
                state.OrbitalsFlamesDrift = ReadFiniteSingle(reader);

                return ValidateKnownRecordTail(
                    record,
                    stream,
                    NadaVfxNetworkProtocol.OrbitalsFlamesSchemaVersion);
            }
            catch
            {
                return false;
            }
        }

        private static bool TryReadOrbitalsEmbers(
            NadaVfxNetworkRecord record,
            ref VfxState state)
        {
            try
            {
                using var stream =
                    OpenRecord(record);

                using var reader =
                    new BinaryReader(stream);

                state.OrbitalsEmbersEnabled = true;

                state.OrbitalsEmbersCount = ReadFiniteSingle(reader);
                state.OrbitalsEmbersEnergy = ReadFiniteSingle(reader);
                state.OrbitalsEmbersScale = ReadFiniteSingle(reader);
                state.OrbitalsEmbersLuminance = ReadFiniteSingle(reader);
                state.OrbitalsEmbersHue = ReadFiniteSingle(reader);
                state.OrbitalsEmbersLifetime = ReadFiniteSingle(reader);
                state.OrbitalsEmbersSimulationSpeed = ReadFiniteSingle(reader);
                state.OrbitalsEmbersLength = ReadFiniteSingle(reader);
                state.OrbitalsEmbersSpeed = ReadFiniteSingle(reader);
                state.OrbitalsEmbersSpacing = ReadFiniteSingle(reader);
                state.OrbitalsEmbersRadius = ReadFiniteSingle(reader);
                state.OrbitalsEmbersCycles = ReadFiniteSingle(reader);
                state.OrbitalsEmbersXOffset = ReadFiniteSingle(reader);
                state.OrbitalsEmbersYOffset = ReadFiniteSingle(reader);
                state.OrbitalsEmbersZOffset = ReadFiniteSingle(reader);
                state.OrbitalsEmbersXRotation = ReadFiniteSingle(reader);
                state.OrbitalsEmbersYRotation = ReadFiniteSingle(reader);
                state.OrbitalsEmbersZRotation = ReadFiniteSingle(reader);
                state.OrbitalsEmbersDrift = ReadFiniteSingle(reader);

                return ValidateKnownRecordTail(
                    record,
                    stream,
                    NadaVfxNetworkProtocol.OrbitalsEmbersSchemaVersion);
            }
            catch
            {
                return false;
            }
        }

        private static MemoryStream OpenRecord(
            NadaVfxNetworkRecord record)
        {
            if (record == null ||
                record.SchemaVersion == 0 ||
                record.Payload == null)
            {
                throw new InvalidDataException(
                    "Invalid NADA VFX record.");
            }

            return new MemoryStream(
                record.Payload,
                writable: false);
        }

        private static bool ValidateKnownRecordTail(
            NadaVfxNetworkRecord record,
            MemoryStream stream,
            byte currentSchemaVersion)
        {
            if (record.SchemaVersion == 0)
                return false;

            // Any schema this client understands should have been consumed exactly.
            //
            // When we eventually add schema 2 to a record, its reader will branch
            // on SchemaVersion so schema 1 reads only its old fields and schema 2
            // reads the appended fields as well.
            if (record.SchemaVersion <=
                currentSchemaVersion)
            {
                return stream.Position ==
                       stream.Length;
            }

            // A newer append-only schema may contain fields this older client
            // doesn't know yet. The known prefix is still valid, so skip the tail.
            return stream.Position <=
                   stream.Length;
        }

        private static void WriteFiniteSingle(
            BinaryWriter writer,
            float value)
        {
            if (float.IsNaN(value) ||
                float.IsInfinity(value))
            {
                throw new InvalidDataException(
                    "NADA VFX state contains a non-finite float.");
            }

            writer.Write(
                value);
        }

        private static float ReadFiniteSingle(
            BinaryReader reader)
        {
            float value =
                reader.ReadSingle();

            if (float.IsNaN(value) ||
                float.IsInfinity(value))
            {
                throw new InvalidDataException(
                    "NADA VFX packet contains a non-finite float.");
            }

            return value;
        }

        internal static void RunSelfTest()
        {
            try
            {
                VfxState source =
                    CreateSyntheticState(
                        out int fieldCount);

                byte[] bytes =
                    SerializeState(
                        itemHash: 123456789,
                        revision: 42,
                        bound: true,
                        state: source);

                if (!TryDeserializeState(
                        bytes,
                        out NadaVfxNetworkPacket packet,
                        out VfxState decoded))
                {
                    Plugin.Log.LogError(
                        $"{Plugin.ModName}: [NetworkCodecV2SelfTest FAIL] " +
                        $"decode failed.");

                    return;
                }

                string mismatch =
                    FindFirstStateMismatch(
                        source,
                        decoded);

                if (mismatch != null)
                {
                    Plugin.Log.LogError(
                        $"{Plugin.ModName}: [NetworkCodecV2SelfTest FAIL] " +
                        $"field='{mismatch}' did not survive round-trip.");

                    return;
                }

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [NetworkCodecV2SelfTest OK] " +
                    $"fields={fieldCount} " +
                    $"records={packet.Records.Count} " +
                    $"bytes={bytes.Length} " +
                    $"roundTrip=True");
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(
                    $"{Plugin.ModName}: [NetworkCodecV2SelfTest FAIL] {e}");
            }
        }

        private static VfxState CreateSyntheticState(
            out int fieldCount)
        {
            FieldInfo[] fields =
                typeof(VfxState).GetFields(
                    BindingFlags.Instance |
                    BindingFlags.Public);

            object boxed =
                new VfxState();

            int floatIndex = 0;

            foreach (FieldInfo field in
                     fields)
            {
                if (field.FieldType ==
                    typeof(bool))
                {
                    // This deliberately enables every effect and every
                    // option so sparse serialization exercises all records.
                    field.SetValue(
                        boxed,
                        true);

                    continue;
                }

                if (field.FieldType ==
                    typeof(float))
                {
                    // Give every float a deterministic non-zero value.
                    float value =
                        0.125f +
                        floatIndex *
                        0.03125f;

                    field.SetValue(
                        boxed,
                        value);

                    floatIndex++;
                    continue;
                }

                throw new NotSupportedException(
                    $"VfxState field '{field.Name}' uses unsupported type " +
                    $"'{field.FieldType.Name}'. Update the V2 network codec.");
            }

            fieldCount =
                fields.Length;

            return (VfxState)boxed;
        }

        private static string FindFirstStateMismatch(
            VfxState expected,
            VfxState actual)
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
                    return field.Name;
                }
            }

            return null;
        }
    }
}