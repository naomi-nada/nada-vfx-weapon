using System;
using System.IO;
using System.Text;
using NADA.VFX.Weapon.Core.Persistence;
using NADA.VFX.Weapon.Core.State.Blocks;

namespace NADA.VFX.Weapon.Core.Network
{
    // A decoded, source-owned network value. Never an ItemData or a Unity object.
    internal sealed class WeaponVfxNetworkStatePacket
    {
        internal int ItemHash { get; }
        internal uint Revision { get; }
        internal bool Bound { get; }
        internal WeaponVfxState State { get; }
        internal uint NextInstanceId { get; }

        internal WeaponVfxNetworkStatePacket(
            int itemHash,
            uint revision,
            bool bound,
            WeaponVfxState state,
            uint nextInstanceId)
        {
            ItemHash = itemHash;
            Revision = revision;
            Bound = bound;
            State = state;
            NextInstanceId = nextInstanceId;
        }
    }

    /// <summary>
    /// Block-native wire envelope, distinct from legacy network V2.
    ///
    /// For now its payload reuses the deterministic, validated native-state
    /// binary format. This avoids inventing a second block serializer during
    /// migration. Network size/version/identity checks belong here; item
    /// persistence remains owned by WeaponVfxItemStore.
    ///
    /// Layout (little-endian): magic:u32, version:u8, flags:u8,
    /// itemHash:i32, revision:u32, payloadLength:u16, payload:bytes.
    /// The persistence payload includes the block schema and ID cursor.
    /// </summary>
    internal static class WeaponVfxNetworkCodec
    {
        private const uint Magic = 0x334E564E; // "NVN3"
        private const byte Version = 1;
        private const byte BoundFlag = 1;
        private const int HeaderBytes = 16;

        // An explicit wire budget independent of the larger item-store limit.
        // A state too large for this format fails closed; it is never clipped.
        internal const int MaxPacketBytes = 8 * 1024;
        private const int MaxPayloadBytes = MaxPacketBytes - HeaderBytes;

        internal static bool TryEncode(
            int itemHash,
            uint revision,
            bool bound,
            WeaponVfxState state,
            uint nextInstanceId,
            out byte[] bytes,
            out string reason)
        {
            bytes = null;
            reason = null;

            if (itemHash == 0 || revision == 0)
            {
                reason = "Missing vanilla item identity or state revision.";
                return false;
            }

            byte[] stateBytes = Array.Empty<byte>();
            if (bound)
            {
                if (!WeaponVfxStateCodec.TryEncode(
                        state, nextInstanceId, out stateBytes, out reason))
                    return false;

                if (stateBytes == null || stateBytes.Length == 0)
                {
                    reason = "Bound state has no serialized payload.";
                    return false;
                }
            }
            else if (state != null || nextInstanceId != 0)
            {
                reason = "Unbound packet must not carry state or an ID cursor.";
                return false;
            }

            if (stateBytes.Length > MaxPayloadBytes)
            {
                reason = "Native network payload exceeds the 8 KiB packet budget.";
                return false;
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    using (var writer = new BinaryWriter(
                               stream, Encoding.UTF8, leaveOpen: true))
                    {
                        writer.Write(Magic);
                        writer.Write(Version);
                        writer.Write(bound ? BoundFlag : (byte)0);
                        writer.Write(itemHash);
                        writer.Write(revision);
                        writer.Write((ushort)stateBytes.Length);
                        writer.Write(stateBytes);
                    }

                    bytes = stream.ToArray();
                }

                return true;
            }
            catch (Exception error) when (IsWireError(error))
            {
                reason = error.Message;
                return false;
            }
        }

        internal static bool TryDecode(
            byte[] bytes,
            out WeaponVfxNetworkStatePacket packet,
            out string reason)
        {
            packet = null;
            reason = null;

            if (bytes == null || bytes.Length < HeaderBytes ||
                bytes.Length > MaxPacketBytes)
            {
                reason = "Truncated or oversized native network packet.";
                return false;
            }

            try
            {
                using (var stream = new MemoryStream(bytes, writable: false))
                using (var reader = new BinaryReader(
                           stream, Encoding.UTF8, leaveOpen: true))
                {
                    if (reader.ReadUInt32() != Magic)
                        throw new InvalidDataException("Invalid native network magic.");

                    if (reader.ReadByte() != Version)
                        throw new InvalidDataException("Unsupported native network version.");

                    byte flags = reader.ReadByte();
                    if ((flags & ~BoundFlag) != 0)
                        throw new InvalidDataException("Unknown native network flags.");

                    bool bound = (flags & BoundFlag) != 0;
                    int itemHash = reader.ReadInt32();
                    uint revision = reader.ReadUInt32();
                    int payloadLength = reader.ReadUInt16();

                    if (itemHash == 0 || revision == 0)
                        throw new InvalidDataException("Invalid network item identity or revision.");

                    if (payloadLength > MaxPayloadBytes ||
                        payloadLength != stream.Length - stream.Position)
                    {
                        throw new InvalidDataException("Native network payload length mismatch.");
                    }

                    if (!bound && payloadLength != 0)
                        throw new InvalidDataException("Unbound packet contains unexpected state.");

                    if (bound && payloadLength == 0)
                        throw new InvalidDataException("Bound packet is missing its state.");

                    WeaponVfxState state = null;
                    uint nextInstanceId = 0;

                    if (bound)
                    {
                        byte[] payload = reader.ReadBytes(payloadLength);
                        if (payload.Length != payloadLength)
                            throw new InvalidDataException("Truncated native block payload.");

                        if (!WeaponVfxStateCodec.TryDecode(
                                payload,
                                out state,
                                out nextInstanceId,
                                out string decodeReason))
                        {
                            throw new InvalidDataException(
                                "Invalid native block state: " + decodeReason);
                        }
                    }

                    packet = new WeaponVfxNetworkStatePacket(
                        itemHash, revision, bound, state, nextInstanceId);
                    return true;
                }
            }
            catch (Exception error) when (IsWireError(error))
            {
                reason = error.Message;
                return false;
            }
        }

        private static bool IsWireError(Exception error)
        {
            return error is InvalidDataException ||
                   error is IOException ||
                   error is ArgumentException ||
                   error is OverflowException;
        }
    }
}
