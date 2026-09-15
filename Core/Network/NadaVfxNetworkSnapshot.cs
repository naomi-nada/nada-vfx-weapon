using System;
using System.Collections.Generic;

namespace NADA.VFX.Weapon.Core.Network
{
    /// <summary>
    /// Temporary network representation of the VFX state for one equipped item.
    /// This is presentation state only. ItemData remains the persistent authority.
    /// </summary>
    [Serializable]
    internal sealed class NadaVfxNetworkSnapshot
    {
        public int Version;
        public int ItemHash;
        public bool Bound;

        public List<NadaVfxNetworkEntry> Entries =
            new List<NadaVfxNetworkEntry>();
    }

    [Serializable]
    internal sealed class NadaVfxNetworkEntry
    {
        public string Key;
        public string Value;

        public NadaVfxNetworkEntry()
        {
        }

        public NadaVfxNetworkEntry(
            string key,
            string value)
        {
            Key = key;
            Value = value;
        }
    }
}