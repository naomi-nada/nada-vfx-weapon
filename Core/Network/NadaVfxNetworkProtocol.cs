using System;

namespace NADA.VFX.Weapon.Core.Network
{
    internal static class NadaVfxNetworkProtocol
    {
        // Written little-endian this becomes the ASCII bytes "NVX2".
        internal const uint Magic = 0x3258564E;

        internal const byte CurrentProtocolVersion = 2;

        // This is a deliberate network budget, not just a safety guard.
        // Current all-effects state is 702 bytes, so 4 KB leaves a lot of
        // room for new effects without letting the protocol quietly bloat.
        internal const int MaxPacketBytes = 4 * 1024;

        internal const int MaxRecords = 128;
        internal const int MaxRecordPayloadBytes = 1024;

        // Current VfxState only supports one instance of each effect.
        // Instance 0 is the canonical instance so the packet format is
        // already ready for multiple copies later.
        internal const ushort CanonicalInstanceId = 0;

        // Record schemas evolve independently.
        //
        // Existing fields keep their meaning and order forever.
        // New fields may be appended to the end of that record.
        //
        // A reader for a newer schema must still understand older schemas
        // and provide protocol-defined defaults for fields that did not
        // exist yet. Never fall back to the receiving player's config.
        //
        // If an effect ever needs an incompatible redesign, give that
        // representation a new record type instead of silently changing
        // the meaning of an existing one.
        internal const byte RigTransformSchemaVersion = 1;

        internal const byte InnerFlamesSchemaVersion = 1;
        internal const byte OuterFlamesSchemaVersion = 1;
        internal const byte StrandsSchemaVersion = 1;
        internal const byte SparksSchemaVersion = 1;
        internal const byte FlareSchemaVersion = 1;
        internal const byte AuraSchemaVersion = 1;

        internal const byte OrbitalsOrbsSchemaVersion = 1;
        internal const byte OrbitalsCoresSchemaVersion = 1;
        internal const byte OrbitalsFlamesSchemaVersion = 1;
        internal const byte OrbitalsEmbersSchemaVersion = 1;

        internal static byte GetCurrentRecordSchemaVersion(
            NadaVfxNetworkRecordType type)
        {
            switch (type)
            {
                case NadaVfxNetworkRecordType.RigTransform:
                    return RigTransformSchemaVersion;

                case NadaVfxNetworkRecordType.InnerFlames:
                    return InnerFlamesSchemaVersion;

                case NadaVfxNetworkRecordType.OuterFlames:
                    return OuterFlamesSchemaVersion;

                case NadaVfxNetworkRecordType.Strands:
                    return StrandsSchemaVersion;

                case NadaVfxNetworkRecordType.Sparks:
                    return SparksSchemaVersion;

                case NadaVfxNetworkRecordType.Flare:
                    return FlareSchemaVersion;

                case NadaVfxNetworkRecordType.Aura:
                    return AuraSchemaVersion;

                case NadaVfxNetworkRecordType.OrbitalsOrbs:
                    return OrbitalsOrbsSchemaVersion;

                case NadaVfxNetworkRecordType.OrbitalsCores:
                    return OrbitalsCoresSchemaVersion;

                case NadaVfxNetworkRecordType.OrbitalsFlames:
                    return OrbitalsFlamesSchemaVersion;

                case NadaVfxNetworkRecordType.OrbitalsEmbers:
                    return OrbitalsEmbersSchemaVersion;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(type),
                        type,
                        "Unknown NADA VFX record type.");
            }
        }

        [Flags]
        internal enum PacketFlags : byte
        {
            None = 0,
            Bound = 1 << 0
        }

        internal const PacketFlags KnownPacketFlags =
            PacketFlags.Bound;
    }

    internal enum NadaVfxNetworkRecordType : ushort
    {
        RigTransform = 1,

        InnerFlames = 10,
        OuterFlames = 11,
        Strands = 12,
        Sparks = 13,
        Flare = 14,
        Aura = 15,

        OrbitalsOrbs = 20,
        OrbitalsCores = 21,
        OrbitalsFlames = 22,
        OrbitalsEmbers = 23
    }
}