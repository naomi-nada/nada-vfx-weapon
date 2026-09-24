using System.Collections.Generic;

namespace NADA.VFX.Weapon.Core.Network
{
    internal sealed class NadaVfxNetworkPacket
    {
        internal readonly int ItemHash;
        internal readonly uint Revision;
        internal readonly bool Bound;

        internal readonly List<NadaVfxNetworkRecord> Records;

        internal NadaVfxNetworkPacket(
            int itemHash,
            uint revision,
            bool bound,
            List<NadaVfxNetworkRecord> records)
        {
            ItemHash = itemHash;
            Revision = revision;
            Bound = bound;

            Records =
                records ??
                new List<NadaVfxNetworkRecord>();
        }
    }

    internal sealed class NadaVfxNetworkRecord
    {
        // Store the raw ushort instead of only the enum.
        // That lets an older client safely carry/skip a newer effect ID
        // it doesn't know about yet.
        internal readonly ushort TypeId;

        // Instance 0 is today's single canonical effect.
        // Future duplicate effects get their own stable instance IDs.
        internal readonly ushort InstanceId;

        internal readonly byte SchemaVersion;

        internal readonly byte[] Payload;

        internal NadaVfxNetworkRecord(
            ushort typeId,
            ushort instanceId,
            byte schemaVersion,
            byte[] payload)
        {
            TypeId = typeId;
            InstanceId = instanceId;
            SchemaVersion = schemaVersion;

            Payload =
                payload ??
                System.Array.Empty<byte>();
        }
    }
}