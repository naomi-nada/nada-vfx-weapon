using System.Collections.Generic;

namespace NADA.VFX.Weapon.Core.Debug
{
    internal static class NadaLogControl
    {
        private static readonly HashSet<string> _infoKeys = new();
        private static readonly HashSet<string> _equipKeys = new();

        internal static void Info(string key, string message)
        {
            if (!Plugin.DebugLoggingEnabled.Value)
                return;

            if (!_infoKeys.Add(key))
                return;

            Plugin.Log.LogInfo(message);
        }

        internal static void Equip(string key, string message)
        {
            if (!Plugin.EquipLoggingEnabled.Value)
                return;

            if (!_equipKeys.Add(key))
                return;

            Plugin.Log.LogInfo(message);
        }
    }
}