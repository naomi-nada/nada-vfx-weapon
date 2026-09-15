using System;
using System.Collections.Generic;
using System.IO;
using NADA.VFX.Weapon.Core.State;

namespace NADA.VFX.Weapon.Core.Network
{
    internal static class NadaVfxNetworkCodec
    {
        internal const int CurrentVersion = 1;

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

                foreach (var pair in stateEntries)
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

                if (entryCount < 0)
                    return false;

                var entries =
                    new Dictionary<string, string>(
                        entryCount);

                snapshot =
                    new NadaVfxNetworkSnapshot
                    {
                        Version = version,
                        ItemHash = itemHash,
                        Bound = bound
                    };

                for (int i = 0; i < entryCount; i++)
                {
                    string key =
                        reader.ReadString();

                    string value =
                        reader.ReadString();

                    if (string.IsNullOrWhiteSpace(key))
                        continue;

                    entries[key] = value;

                    snapshot.Entries.Add(
                        new NadaVfxNetworkEntry(
                            key,
                            value));
                }

                state =
                    VfxStateIO.FromStateEntries(
                        entries);

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