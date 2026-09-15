using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NADA.VFX.Weapon.Core.State;

namespace NADA.VFX.Weapon.Core.Network
{
    internal static class NadaVfxNetworkCodec
    {
        internal const int CurrentVersion = 1;

        // Current payloads are around 8 KB. Keep enough room for the format to grow without accepting arbitrarily large network data.
        private const int MaxPayloadBytes = 64 * 1024;
        private const int MaxStateEntries = 512;

        internal static string Serialize(
            int itemHash,
            bool bound,
            VfxState state)
        {
            Dictionary<string, string> stateEntries =
                VfxStateIO.ToStateEntries(state);

            using var stream = new MemoryStream();

            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(CurrentVersion);
                writer.Write(itemHash);
                writer.Write(bound);
                writer.Write(stateEntries.Count);

                // Keep the wire format deterministic. The same state should
                // produce the same payload regardless of dictionary order.
                foreach (var pair in stateEntries
                             .OrderBy(
                                 pair => pair.Key,
                                 StringComparer.Ordinal))
                {
                    writer.Write(pair.Key ?? string.Empty);
                    writer.Write(pair.Value ?? string.Empty);
                }
            }

            return Convert.ToBase64String(
                stream.ToArray());
        }

        internal static bool TryDeserialize(
            string payload,
            out NadaVfxNetworkSnapshot snapshot,
            out VfxState state)
        {
            snapshot = null;
            state = default;

            if (string.IsNullOrWhiteSpace(payload))
                return false;

            try
            {
                byte[] bytes =
                    Convert.FromBase64String(payload);

                if (bytes.Length == 0 ||
                    bytes.Length > MaxPayloadBytes)
                {
                    return false;
                }

                using var stream =
                    new MemoryStream(bytes);

                using var reader =
                    new BinaryReader(stream);

                int version =
                    reader.ReadInt32();

                if (version != CurrentVersion)
                    return false;

                int itemHash =
                    reader.ReadInt32();

                bool bound =
                    reader.ReadBoolean();

                int entryCount =
                    reader.ReadInt32();

                if (entryCount < 0 ||
                    entryCount > MaxStateEntries)
                {
                    return false;
                }

                var entries =
                    new Dictionary<string, string>(
                        entryCount);

                var snapshotEntries =
                    new List<NadaVfxNetworkEntry>(
                        entryCount);

                for (int i = 0; i < entryCount; i++)
                {
                    string key =
                        reader.ReadString();

                    string value =
                        reader.ReadString();

                    if (string.IsNullOrWhiteSpace(key))
                        continue;

                    entries[key] = value;

                    snapshotEntries.Add(
                        new NadaVfxNetworkEntry
                        {
                            Key = key,
                            Value = value
                        });
                }

                // One payload should contain exactly one snapshot.
                // Don't silently accept trailing or malformed data.
                if (stream.Position != stream.Length)
                    return false;

                snapshot =
                    new NadaVfxNetworkSnapshot
                    {
                        Version = version,
                        ItemHash = itemHash,
                        Bound = bound,
                        Entries = snapshotEntries
                    };

                state =
                    VfxStateIO.FromStateEntries(entries);

                return true;
            }
            catch
            {
                snapshot = null;
                state = default;
                return false;
            }
        }
    }
}