using System;
using System.IO;
using System.IO.Compression;
using NADA.VFX.Weapon.Core.State.Blocks;

namespace NADA.VFX.Weapon.Core.Persistence
{
    // A portable, versioned text envelope around the one native state codec.
    // Only editor copy/import actions should call this; never call on a tick.
    // Format: NADA1: + base64url([flags:1][raw length:4][crc32:4][body]).
    internal static class WeaponVfxShareCode
    {
        private const string Prefix = "NADA1:";
        private const byte CompressedFlag = 1;
        private const int HeaderBytes = 9;
        private const int MaxStateBytes = 65536; // Matches WeaponVfxStateCodec.
        private const int MaxCodeChars = 90000;

        internal static bool TryEncode(
            WeaponVfxState state,
            uint nextInstanceId,
            out string code,
            out string reason)
        {
            code = null;
            reason = null;

            if (!WeaponVfxStateCodec.TryEncode(
                    state, nextInstanceId, out byte[] raw, out reason))
                return false;

            try
            {
                byte[] body = raw;
                byte flags = 0;
                if (raw.Length >= 128)
                {
                    using (var buffer = new MemoryStream())
                    {
                        using (var zipper = new DeflateStream(
                                   buffer, CompressionLevel.Optimal, true))
                            zipper.Write(raw, 0, raw.Length);
                        byte[] zipped = buffer.ToArray();
                        if (zipped.Length < raw.Length)
                        {
                            body = zipped;
                            flags = CompressedFlag;
                        }
                    }
                }

                using (var buffer = new MemoryStream(HeaderBytes + body.Length))
                {
                    using (var writer = new BinaryWriter(buffer, System.Text.Encoding.UTF8, true))
                    {
                        writer.Write(flags);
                        writer.Write(raw.Length);
                        writer.Write(Crc32(raw));
                        writer.Write(body);
                    }
                    string encoded = Convert.ToBase64String(buffer.ToArray())
                        .TrimEnd('=')
                        .Replace('+', '-')
                        .Replace('/', '_');
                    if (encoded.Length + Prefix.Length > MaxCodeChars)
                    {
                        reason = "Share code exceeds size limit.";
                        return false;
                    }
                    code = Prefix + encoded;
                    return true;
                }
            }
            catch (Exception ex) when (ex is IOException ||
                                       ex is ArgumentException ||
                                       ex is InvalidOperationException)
            {
                reason = "Could not encode share code: " + ex.Message;
                return false;
            }
        }

        internal static bool TryDecode(
            string code,
            out WeaponVfxState state,
            out uint nextInstanceId,
            out string reason)
        {
            state = null;
            nextInstanceId = 0;
            reason = null;

            string text = code?.Trim();
            if (string.IsNullOrEmpty(text) || text.Length > MaxCodeChars ||
                !text.StartsWith(Prefix, StringComparison.Ordinal))
            {
                reason = "Missing, oversized, or unsupported NADA share-code version.";
                return false;
            }

            string body = text.Substring(Prefix.Length);
            if (body.Length == 0 || body.Length % 4 == 1)
            {
                reason = "Invalid share-code length.";
                return false;
            }
            foreach (char ch in body)
            {
                if ((ch >= 'A' && ch <= 'Z') ||
                    (ch >= 'a' && ch <= 'z') ||
                    (ch >= '0' && ch <= '9') || ch == '-' || ch == '_')
                    continue;
                reason = "Share code contains invalid characters.";
                return false;
            }

            try
            {
                string base64 = body.Replace('-', '+').Replace('_', '/');
                base64 = base64.PadRight((base64.Length + 3) / 4 * 4, '=');
                byte[] envelope = Convert.FromBase64String(base64);
                if (envelope.Length < HeaderBytes ||
                    envelope.Length > MaxStateBytes + HeaderBytes)
                    throw new InvalidDataException("Invalid share-code envelope size.");

                using (var stream = new MemoryStream(envelope, false))
                using (var reader = new BinaryReader(stream))
                {
                    byte flags = reader.ReadByte();
                    if (flags != 0 && flags != CompressedFlag)
                        throw new InvalidDataException("Unsupported share-code flags.");

                    int expectedLength = reader.ReadInt32();
                    uint expectedCrc = reader.ReadUInt32();
                    if (expectedLength <= 0 || expectedLength > MaxStateBytes)
                        throw new InvalidDataException("Invalid decoded state length.");

                    int bodyLength = (int)(stream.Length - stream.Position);
                    byte[] raw;
                    if (flags == 0)
                    {
                        if (bodyLength != expectedLength)
                            throw new InvalidDataException("Share-code state length mismatch.");
                        raw = reader.ReadBytes(bodyLength);
                    }
                    else
                    {
                        // Decode with a hard output cap, before touching the state codec.
                        // Never allow an untrusted code to inflate without bounds.
                        using (var compressed = new MemoryStream(
                                   envelope, HeaderBytes, bodyLength, false))
                        using (var zipper = new DeflateStream(
                                   compressed, CompressionMode.Decompress))
                        using (var inflated = new MemoryStream(expectedLength))
                        {
                            var chunk = new byte[4096];
                            int count;
                            while ((count = zipper.Read(chunk, 0, chunk.Length)) > 0)
                            {
                                if (count > expectedLength - inflated.Length)
                                    throw new InvalidDataException("Decompressed share code is oversized.");
                                inflated.Write(chunk, 0, count);
                            }
                            if (inflated.Length != expectedLength)
                                throw new InvalidDataException("Decompressed share code length mismatch.");
                            raw = inflated.ToArray();
                        }
                    }

                    if (Crc32(raw) != expectedCrc)
                        throw new InvalidDataException("Share-code checksum mismatch.");

                    return WeaponVfxStateCodec.TryDecode(
                        raw, out state, out nextInstanceId, out reason);
                }
            }
            catch (Exception ex) when (ex is InvalidDataException ||
                                       ex is FormatException ||
                                       ex is IOException ||
                                       ex is ArgumentException ||
                                       ex is OverflowException)
            {
                reason = "Invalid share code: " + ex.Message;
                return false;
            }
        }

        // IEEE CRC-32 detects accidental copy/paste corruption; it is not a
        // signature and should never be treated as proof of trusted content.
        private static uint Crc32(byte[] bytes)
        {
            uint crc = 0xFFFFFFFFu;
            foreach (byte value in bytes)
            {
                crc ^= value;
                for (int bit = 0; bit < 8; bit++)
                    crc = (crc >> 1) ^ ((crc & 1) != 0 ? 0xEDB88320u : 0u);
            }
            return ~crc;
        }
    }
}
