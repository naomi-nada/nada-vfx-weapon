using System;
using System.IO;
using System.Text;
using NADA.VFX.Weapon.Core.State.Blocks;

namespace NADA.VFX.Weapon.Core.Persistence
{
    // Style files contain a name and one complete, validated native-state
    // snapshot. The weapon codec owns effect serialization and validation.
    // This file format is intentionally separate from legacy style JSON.
    internal static class WeaponVfxStyleCodec
    {
        private const uint Magic = 0x5453444E; // "NDST" little-endian
        private const ushort FormatVersion = 1;
        internal const int MaxFileBytes = 66000;
        private const int MaxStyleNameLength = 64;
        private const int MaxPayloadBytes = 65536;
        private static readonly UTF8Encoding StrictUtf8 =
            new UTF8Encoding(false, true);

        internal static bool TryNormalizeName(string name, out string normalized)
        {
            normalized = name?.Trim();
            if (string.IsNullOrEmpty(normalized) ||
                normalized.Length > MaxStyleNameLength ||
                normalized == "." || normalized == ".." ||
                normalized.EndsWith(".", StringComparison.Ordinal) ||
                string.Equals(normalized, "Default", StringComparison.OrdinalIgnoreCase))
                return false;

            foreach (char ch in normalized)
            {
                // Reject filename separators and reserved characters on *both*
                // Windows and macOS, regardless of the current platform.
                if (char.IsControl(ch) || "<>:\"/\\|?*".IndexOf(ch) >= 0)
                    return false;
            }
            return true;
        }

        internal static bool TryEncode(
            string name,
            WeaponVfxState state,
            uint nextInstanceId,
            out byte[] fileBytes,
            out string reason)
        {
            fileBytes = null;
            reason = null;
            if (!TryNormalizeName(name, out string normalized))
            {
                reason = "Invalid native style name (1–64 characters, not Default).";
                return false;
            }

            if (!WeaponVfxStateCodec.TryEncode(
                    state, nextInstanceId, out byte[] payload, out reason))
                return false;

            try
            {
                using (var stream = new MemoryStream())
                {
                    using (var writer = new BinaryWriter(stream, StrictUtf8, true))
                    {
                        writer.Write(Magic);
                        writer.Write(FormatVersion);
                        writer.Write(normalized);
                        writer.Write(payload.Length);
                        writer.Write(payload);
                    }
                    if (stream.Length > MaxFileBytes)
                    {
                        reason = "Native style file exceeds size limit.";
                        return false;
                    }
                    fileBytes = stream.ToArray();
                    return true;
                }
            }
            catch (Exception ex) when (ex is ArgumentException ||
                                       ex is IOException ||
                                       ex is EncoderFallbackException)
            {
                reason = ex.Message;
                return false;
            }
        }

        internal static bool TryDecode(
            byte[] fileBytes,
            out string name,
            out WeaponVfxState state,
            out uint nextInstanceId,
            out string reason)
        {
            name = null;
            state = null;
            nextInstanceId = 0;
            reason = null;
            if (fileBytes == null || fileBytes.Length < 15 ||
                fileBytes.Length > MaxFileBytes)
            {
                reason = "Empty, truncated, or oversized native style file.";
                return false;
            }

            try
            {
                using (var stream = new MemoryStream(fileBytes, false))
                using (var reader = new BinaryReader(stream, StrictUtf8, true))
                {
                    if (reader.ReadUInt32() != Magic ||
                        reader.ReadUInt16() != FormatVersion)
                        throw new InvalidDataException("Unsupported native style format.");

                    string savedName = reader.ReadString();
                    if (!TryNormalizeName(savedName, out string normalized) ||
                        !string.Equals(savedName, normalized, StringComparison.Ordinal))
                        throw new InvalidDataException("Invalid native style name.");

                    int length = reader.ReadInt32();
                    if (length <= 0 || length > MaxPayloadBytes ||
                        length != stream.Length - stream.Position)
                        throw new InvalidDataException("Invalid native style payload length.");

                    byte[] payload = reader.ReadBytes(length);
                    if (!WeaponVfxStateCodec.TryDecode(
                            payload, out WeaponVfxState decoded,
                            out uint cursor, out string decodeReason))
                        throw new InvalidDataException(decodeReason ?? "Invalid block state.");

                    name = normalized;
                    state = decoded;
                    nextInstanceId = cursor;
                    return true;
                }
            }
            catch (Exception ex) when (ex is InvalidDataException ||
                                       ex is ArgumentException ||
                                       ex is IOException ||
                                       ex is DecoderFallbackException ||
                                       ex is OverflowException)
            {
                reason = ex.Message;
                return false;
            }
        }
    }
}
