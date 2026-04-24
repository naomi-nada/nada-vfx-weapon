using System.Collections.Generic;
using System.Globalization;
using NADA.VFX.Core.Config;

namespace NADA.VFX.Core.State
{
    internal static class VfxStateIO
    {
        private const string LegacyInnerPresetKey = "nada.vfxfix.inner";

        internal static VfxState FromConfig()
        {
            return new VfxState
            {
                // Inner Flames
                InnerFlamesEnabled = PluginConfig.InnerFlames.Value,
                InnerFlamesEnergy = PluginConfig.InnerFlamesEnergy.Value,
                InnerFlamesScale = PluginConfig.InnerFlamesScale.Value,
                InnerFlamesHue = PluginConfig.InnerFlamesHue.Value,

                // Outer Flames
                OuterFlamesEnabled = PluginConfig.OuterFlames.Value,
                OuterFlamesDragEnabled = PluginConfig.OuterFlamesDragEnabled.Value,
                OuterFlamesEnergy = PluginConfig.OuterFlamesEnergy.Value,
                OuterFlamesScale = PluginConfig.OuterFlamesScale.Value,
                OuterFlamesHue = PluginConfig.OuterFlamesHue.Value,
                
                // Flare
                FlareEnabled = PluginConfig.Flare.Value,
                FlareScale = PluginConfig.FlareScale.Value,
                FlareHue = PluginConfig.FlareHue.Value,

                // Orbitals - Orbs
                OrbitalsOrbsEnabled = PluginConfig.OrbitalsOrbs.Value,
                OrbitalsOrbsCount = PluginConfig.OrbitalsOrbsCount.Value,
                OrbitalsOrbsDrift = PluginConfig.OrbitalsOrbsDrift.Value,
                OrbitalsOrbsScale = PluginConfig.OrbitalsOrbsScale.Value,
                OrbitalsOrbsHue = PluginConfig.OrbitalsOrbsHue.Value,
                OrbitalsOrbsSpeed = PluginConfig.OrbitalsOrbsSpeed.Value,
                OrbitalsOrbsSpacing = PluginConfig.OrbitalsOrbsSpacing.Value,
                OrbitalsOrbsLength = PluginConfig.OrbitalsOrbsLength.Value,
                OrbitalsOrbsRadius = PluginConfig.OrbitalsOrbsRadius.Value,
                OrbitalsOrbsCycles = PluginConfig.OrbitalsOrbsCycles.Value,

                // Orbitals - Flames
                OrbitalsFlamesEnabled = PluginConfig.OrbitalsFlames.Value,
                OrbitalsFlamesCount = PluginConfig.OrbitalsFlamesCount.Value,
                OrbitalsFlamesEnergy = PluginConfig.OrbitalsFlamesEnergy.Value,
                OrbitalsFlamesDrift = PluginConfig.OrbitalsFlamesDrift.Value,
                OrbitalsFlamesHue = PluginConfig.OrbitalsFlamesHue.Value,
                OrbitalsFlamesSpeed = PluginConfig.OrbitalsFlamesSpeed.Value,
                OrbitalsFlamesSpacing = PluginConfig.OrbitalsFlamesSpacing.Value,
                OrbitalsFlamesLength = PluginConfig.OrbitalsFlamesLength.Value,
                OrbitalsFlamesRadius = PluginConfig.OrbitalsFlamesRadius.Value,
                OrbitalsFlamesCycles = PluginConfig.OrbitalsFlamesCycles.Value,
                
                // Orbitals - Embers
                OrbitalsEmbersEnabled = PluginConfig.OrbitalsEmbers.Value,
                OrbitalsEmbersCount = PluginConfig.OrbitalsEmbersCount.Value,
                OrbitalsEmbersEnergy = PluginConfig.OrbitalsEmbersEnergy.Value,
                OrbitalsEmbersDrift = PluginConfig.OrbitalsEmbersDrift.Value,
                OrbitalsEmbersHue = PluginConfig.OrbitalsEmbersHue.Value,
                OrbitalsEmbersSpeed = PluginConfig.OrbitalsEmbersSpeed.Value,
                OrbitalsEmbersSpacing = PluginConfig.OrbitalsEmbersSpacing.Value,
                OrbitalsEmbersLength = PluginConfig.OrbitalsEmbersLength.Value,
                OrbitalsEmbersRadius = PluginConfig.OrbitalsEmbersRadius.Value,
                OrbitalsEmbersCycles = PluginConfig.OrbitalsEmbersCycles.Value
            };
        }

        internal static bool TryRead(global::ItemDrop.ItemData item, out VfxState state)
        {
            state = default;
            if (item == null)
                return false;

            Dictionary<string, string> customData = item.m_customData;
            if (customData == null)
                return false;

            // Inner Flames
            state.InnerFlamesEnabled = ReadBool(customData, VfxStateKeys.InnerFlamesEnabled, PluginConfig.InnerFlames.Value);
            state.InnerFlamesEnergy = ReadFloat(customData, VfxStateKeys.InnerFlamesEnergy, PluginConfig.InnerFlamesEnergy.Value);
            state.InnerFlamesScale = ReadFloat(customData, VfxStateKeys.InnerFlamesScale, PluginConfig.InnerFlamesScale.Value);
            state.InnerFlamesHue = ReadFloat(customData, VfxStateKeys.InnerFlamesHue, PluginConfig.InnerFlamesHue.Value);

            // Outer Flames
            state.OuterFlamesEnabled = ReadBool(customData, VfxStateKeys.OuterFlamesEnabled, PluginConfig.OuterFlames.Value);
            state.OuterFlamesDragEnabled = ReadBool(customData, VfxStateKeys.OuterFlamesDragEnabled, PluginConfig.OuterFlamesDragEnabled.Value);
            state.OuterFlamesEnergy = ReadFloat(customData, VfxStateKeys.OuterFlamesEnergy, PluginConfig.OuterFlamesEnergy.Value);
            state.OuterFlamesScale = ReadFloat(customData, VfxStateKeys.OuterFlamesScale, PluginConfig.OuterFlamesScale.Value);
            state.OuterFlamesHue = ReadFloat(customData, VfxStateKeys.OuterFlamesHue, PluginConfig.OuterFlamesHue.Value);

            // Flare
            state.FlareEnabled = ReadBool(customData, VfxStateKeys.FlareEnabled, PluginConfig.Flare.Value);
            state.FlareScale = ReadFloat(customData, VfxStateKeys.FlareScale, PluginConfig.FlareScale.Value);
            state.FlareHue = ReadFloat(customData, VfxStateKeys.FlareHue, PluginConfig.FlareHue.Value);

            // Orbitals - Orbs
            state.OrbitalsOrbsEnabled = ReadBool(customData, VfxStateKeys.OrbitalsOrbsEnabled, PluginConfig.OrbitalsOrbs.Value);
            state.OrbitalsOrbsCount = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsCount, PluginConfig.OrbitalsOrbsCount.Value);
            state.OrbitalsOrbsDrift = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsDrift, PluginConfig.OrbitalsOrbsDrift.Value);
            state.OrbitalsOrbsScale = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsScale, PluginConfig.OrbitalsOrbsScale.Value);
            state.OrbitalsOrbsHue = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsHue, PluginConfig.OrbitalsOrbsHue.Value);
            state.OrbitalsOrbsSpeed = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsSpeed, PluginConfig.OrbitalsOrbsSpeed.Value);
            state.OrbitalsOrbsSpacing = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsSpacing, PluginConfig.OrbitalsOrbsSpacing.Value);
            state.OrbitalsOrbsLength = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsLength, PluginConfig.OrbitalsOrbsLength.Value);
            state.OrbitalsOrbsRadius = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsRadius, PluginConfig.OrbitalsOrbsRadius.Value);
            state.OrbitalsOrbsCycles = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsCycles, PluginConfig.OrbitalsOrbsCycles.Value);

            // Orbitals - Flames
            state.OrbitalsFlamesEnabled = ReadBool(customData, VfxStateKeys.OrbitalsFlamesEnabled, PluginConfig.OrbitalsFlames.Value);
            state.OrbitalsFlamesCount = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesCount, PluginConfig.OrbitalsFlamesCount.Value);
            state.OrbitalsFlamesEnergy = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesEnergy, PluginConfig.OrbitalsFlamesEnergy.Value);
            state.OrbitalsFlamesDrift = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesDrift, PluginConfig.OrbitalsFlamesDrift.Value);
            state.OrbitalsFlamesHue = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesHue, PluginConfig.OrbitalsFlamesHue.Value);
            state.OrbitalsFlamesSpeed = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesSpeed, PluginConfig.OrbitalsFlamesSpeed.Value);
            state.OrbitalsFlamesSpacing = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesSpacing, PluginConfig.OrbitalsFlamesSpacing.Value);
            state.OrbitalsFlamesLength = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesLength, PluginConfig.OrbitalsFlamesLength.Value);
            state.OrbitalsFlamesRadius = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesRadius, PluginConfig.OrbitalsFlamesRadius.Value);
            state.OrbitalsFlamesCycles = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesCycles, PluginConfig.OrbitalsFlamesCycles.Value);

            // Orbitals - Embers
            state.OrbitalsEmbersEnabled = ReadBool(customData, VfxStateKeys.OrbitalsEmbersEnabled, PluginConfig.OrbitalsEmbers.Value);
            state.OrbitalsEmbersCount = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersCount, PluginConfig.OrbitalsEmbersCount.Value);
            state.OrbitalsEmbersEnergy = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersEnergy, PluginConfig.OrbitalsEmbersEnergy.Value);
            state.OrbitalsEmbersDrift = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersDrift, PluginConfig.OrbitalsEmbersDrift.Value);
            state.OrbitalsEmbersHue = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersHue, PluginConfig.OrbitalsEmbersHue.Value);
            state.OrbitalsEmbersSpeed = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersSpeed, PluginConfig.OrbitalsEmbersSpeed.Value);
            state.OrbitalsEmbersSpacing = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersSpacing, PluginConfig.OrbitalsEmbersSpacing.Value);
            state.OrbitalsEmbersLength = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersLength, PluginConfig.OrbitalsEmbersLength.Value);
            state.OrbitalsEmbersRadius = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersRadius, PluginConfig.OrbitalsEmbersRadius.Value);
            state.OrbitalsEmbersCycles = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersCycles, PluginConfig.OrbitalsEmbersCycles.Value);

            return true;
        }

        internal static void EnsureInitializedFromConfig(global::ItemDrop.ItemData item)
        {
            if (item == null)
                return;

            if (item.m_customData == null)
                item.m_customData = new Dictionary<string, string>();

            Dictionary<string, string> customData = item.m_customData;

            // Inner Flames
            BackfillMissing(customData, VfxStateKeys.InnerFlamesEnabled, PluginConfig.InnerFlames.Value);
            BackfillMissing(customData, VfxStateKeys.InnerFlamesEnergy, PluginConfig.InnerFlamesEnergy.Value);
            BackfillMissing(customData, VfxStateKeys.InnerFlamesScale, PluginConfig.InnerFlamesScale.Value);
            BackfillMissing(customData, VfxStateKeys.InnerFlamesHue, PluginConfig.InnerFlamesHue.Value);

            // Outer Flames
            BackfillMissing(customData, VfxStateKeys.OuterFlamesEnabled, PluginConfig.OuterFlames.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesDragEnabled, PluginConfig.OuterFlamesDragEnabled.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesEnergy, PluginConfig.OuterFlamesEnergy.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesScale, PluginConfig.OuterFlamesScale.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesHue, PluginConfig.OuterFlamesHue.Value);

            // Flare
            BackfillMissing(customData, VfxStateKeys.FlareEnabled, PluginConfig.Flare.Value);
            BackfillMissing(customData, VfxStateKeys.FlareScale, PluginConfig.FlareScale.Value);
            BackfillMissing(customData, VfxStateKeys.FlareHue, PluginConfig.FlareHue.Value);

            // Orbitals - Orbs
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsEnabled, PluginConfig.OrbitalsOrbs.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsCount, PluginConfig.OrbitalsOrbsCount.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsDrift, PluginConfig.OrbitalsOrbsDrift.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsScale, PluginConfig.OrbitalsOrbsScale.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsHue, PluginConfig.OrbitalsOrbsHue.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsSpeed, PluginConfig.OrbitalsOrbsSpeed.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsSpacing, PluginConfig.OrbitalsOrbsSpacing.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsLength, PluginConfig.OrbitalsOrbsLength.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsRadius, PluginConfig.OrbitalsOrbsRadius.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsCycles, PluginConfig.OrbitalsOrbsCycles.Value);

            // Orbitals - Flames
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesEnabled, PluginConfig.OrbitalsFlames.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesCount, PluginConfig.OrbitalsFlamesCount.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesEnergy, PluginConfig.OrbitalsFlamesEnergy.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesDrift, PluginConfig.OrbitalsFlamesDrift.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesHue, PluginConfig.OrbitalsFlamesHue.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesSpeed, PluginConfig.OrbitalsFlamesSpeed.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesSpacing, PluginConfig.OrbitalsFlamesSpacing.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesLength, PluginConfig.OrbitalsFlamesLength.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesRadius, PluginConfig.OrbitalsFlamesRadius.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesCycles, PluginConfig.OrbitalsFlamesCycles.Value);

            // Orbitals - Embers
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersEnabled, PluginConfig.OrbitalsEmbers.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersCount, PluginConfig.OrbitalsEmbersCount.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersEnergy, PluginConfig.OrbitalsEmbersEnergy.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersDrift, PluginConfig.OrbitalsEmbersDrift.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersHue, PluginConfig.OrbitalsEmbersHue.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersSpeed, PluginConfig.OrbitalsEmbersSpeed.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersSpacing, PluginConfig.OrbitalsEmbersSpacing.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersLength, PluginConfig.OrbitalsEmbersLength.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersRadius, PluginConfig.OrbitalsEmbersRadius.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersCycles, PluginConfig.OrbitalsEmbersCycles.Value);
        }

        private static float ReadFloat(Dictionary<string, string> customData, string key, float fallback)
        {
            if (customData == null)
                return fallback;

            if (!customData.TryGetValue(key, out string stringValue) || string.IsNullOrWhiteSpace(stringValue))
                return fallback;

            if (float.TryParse(
                    stringValue,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out float parsedValue))
            {
                return parsedValue;
            }

            return fallback;
        }

        private static bool ReadBool(Dictionary<string, string> customData, string key, bool fallback)
        {
            if (customData == null)
                return fallback;

            if (!customData.TryGetValue(key, out string stringValue) || string.IsNullOrWhiteSpace(stringValue))
                return fallback;

            if (bool.TryParse(stringValue, out bool parsedValue))
                return parsedValue;

            return fallback;
        }

        private static void BackfillMissing(Dictionary<string, string> customData, string key, float value)
        {
            if (customData == null || customData.ContainsKey(key))
                return;

            customData[key] = value.ToString(CultureInfo.InvariantCulture);
        }

        private static void BackfillMissing(Dictionary<string, string> customData, string key, bool value)
        {
            if (customData == null || customData.ContainsKey(key))
                return;

            customData[key] = value ? "true" : "false";
        }
    }
}