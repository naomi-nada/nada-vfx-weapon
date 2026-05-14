using System;
using System.Collections.Generic;

namespace NADA.VFX.Core.State
{
    [Serializable]
    internal sealed class VfxStyleSave
    {
        public int Version = 1;
        public string Name = "Default";
        public List<VfxStyleEntry> Entries = new();
    }

    [Serializable]
    internal sealed class VfxStyleEntry
    {
        public string Key;
        public string Value;

        public VfxStyleEntry() { }

        public VfxStyleEntry(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}