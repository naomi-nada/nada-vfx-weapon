using System.Collections.Generic;
using System.Globalization;
using NADA.VFX.Core.Config;

namespace NADA.VFX.Core.State
{
    internal static class VfxStateIO
    {
        // Cleanup for legacy pre-state system inner flame preset key.
        private const string LegacyInnerPresetKey = "nada.vfxfix.inner";

        internal static VfxState FromConfig()
        {
            return new VfxState
            {
                // Inner Flames
                InnerFlamesEnabled = PluginConfig.InnerFlames.Value,
                InnerFlamesScale = PluginConfig.InnerFlamesScale.Value,
                InnerFlamesHue = PluginConfig.InnerFlamesHue.Value,
                InnerFlamesEnergy = PluginConfig.InnerFlamesEnergy.Value,
                InnerFlamesChaos = PluginConfig.InnerFlamesChaos.Value,

                // Outer Flames
                OuterFlamesEnabled = PluginConfig.OuterFlames.Value,
                OuterFlamesDragEnabled = PluginConfig.OuterFlamesDragEnabled.Value,
                OuterFlamesScale = PluginConfig.OuterFlamesScale.Value,
                OuterFlamesHue = PluginConfig.OuterFlamesHue.Value,
                OuterFlamesEnergy = PluginConfig.OuterFlamesEnergy.Value,
                OuterFlamesChaos = PluginConfig.OuterFlamesChaos.Value,

                // Flare
                FlareEnabled = PluginConfig.Flare.Value,
                FlareScale = PluginConfig.FlareScale.Value,
                FlareHue = PluginConfig.FlareHue.Value,

                // Sparks
                SparksEnabled = PluginConfig.Sparks.Value,
                SparksHue = PluginConfig.SparksHue.Value,
                SparksEnergy = PluginConfig.SparksEnergy.Value,

                // Mirage
                MirageEnabled = PluginConfig.Mirage.Value,
                MirageScale = PluginConfig.MirageScale.Value,
                MirageHue = PluginConfig.MirageHue.Value,

                // Orbitals - Orbs
                OrbitalsOrbsEnabled = PluginConfig.OrbitalsOrbs.Value,
                OrbitalsOrbsCount = PluginConfig.OrbitalsOrbsCount.Value,
                OrbitalsOrbsScale = PluginConfig.OrbitalsOrbsScale.Value,
                OrbitalsOrbsHue = PluginConfig.OrbitalsOrbsHue.Value,
                OrbitalsOrbsSpeed = PluginConfig.OrbitalsOrbsSpeed.Value,
                OrbitalsOrbsSpacing = PluginConfig.OrbitalsOrbsSpacing.Value,
                OrbitalsOrbsLength = PluginConfig.OrbitalsOrbsLength.Value,
                OrbitalsOrbsRadius = PluginConfig.OrbitalsOrbsRadius.Value,
                OrbitalsOrbsCycles = PluginConfig.OrbitalsOrbsCycles.Value,
                OrbitalsOrbsDrift = PluginConfig.OrbitalsOrbsDrift.Value,

                // Orbitals - Flames
                OrbitalsFlamesEnabled = PluginConfig.OrbitalsFlames.Value,
                OrbitalsFlamesCount = PluginConfig.OrbitalsFlamesCount.Value,
                OrbitalsFlamesHue = PluginConfig.OrbitalsFlamesHue.Value,
                OrbitalsFlamesEnergy = PluginConfig.OrbitalsFlamesEnergy.Value,
                OrbitalsFlamesSpeed = PluginConfig.OrbitalsFlamesSpeed.Value,
                OrbitalsFlamesSpacing = PluginConfig.OrbitalsFlamesSpacing.Value,
                OrbitalsFlamesLength = PluginConfig.OrbitalsFlamesLength.Value,
                OrbitalsFlamesRadius = PluginConfig.OrbitalsFlamesRadius.Value,
                OrbitalsFlamesCycles = PluginConfig.OrbitalsFlamesCycles.Value,
                OrbitalsFlamesDrift = PluginConfig.OrbitalsFlamesDrift.Value,

                // Orbitals - Embers
                OrbitalsEmbersEnabled = PluginConfig.OrbitalsEmbers.Value,
                OrbitalsEmbersCount = PluginConfig.OrbitalsEmbersCount.Value,
                OrbitalsEmbersHue = PluginConfig.OrbitalsEmbersHue.Value,
                OrbitalsEmbersEnergy = PluginConfig.OrbitalsEmbersEnergy.Value,
                OrbitalsEmbersSpeed = PluginConfig.OrbitalsEmbersSpeed.Value,
                OrbitalsEmbersSpacing = PluginConfig.OrbitalsEmbersSpacing.Value,
                OrbitalsEmbersLength = PluginConfig.OrbitalsEmbersLength.Value,
                OrbitalsEmbersRadius = PluginConfig.OrbitalsEmbersRadius.Value,
                OrbitalsEmbersCycles = PluginConfig.OrbitalsEmbersCycles.Value,
                OrbitalsEmbersDrift = PluginConfig.OrbitalsEmbersDrift.Value
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
            state.InnerFlamesEnabled = ReadBool(
                customData,
                VfxStateKeys.InnerFlamesEnabled,
                PluginConfig.InnerFlames.Value);
            state.InnerFlamesScale = ReadFloat(
                customData,
                VfxStateKeys.InnerFlamesScale,
                PluginConfig.InnerFlamesScale.Value);
            state.InnerFlamesHue = ReadFloat(
                customData,
                VfxStateKeys.InnerFlamesHue,
                PluginConfig.InnerFlamesHue.Value);
            state.InnerFlamesEnergy = ReadFloat(
                customData,
                VfxStateKeys.InnerFlamesEnergy,
                PluginConfig.InnerFlamesEnergy.Value);
            state.InnerFlamesChaos = ReadFloat(
                customData,
                VfxStateKeys.InnerFlamesChaos,
                PluginConfig.InnerFlamesChaos.Value);

            // Outer Flames
            state.OuterFlamesEnabled = ReadBool(
                customData,
                VfxStateKeys.OuterFlamesEnabled,
                PluginConfig.OuterFlames.Value);
            state.OuterFlamesDragEnabled = ReadBool(
                customData,
                VfxStateKeys.OuterFlamesDragEnabled,
                PluginConfig.OuterFlamesDragEnabled.Value);
            state.OuterFlamesScale = ReadFloat(
                customData,
                VfxStateKeys.OuterFlamesScale,
                PluginConfig.OuterFlamesScale.Value);
            state.OuterFlamesHue = ReadFloat(
                customData,
                VfxStateKeys.OuterFlamesHue,
                PluginConfig.OuterFlamesHue.Value);
            state.OuterFlamesEnergy = ReadFloat(
                customData,
                VfxStateKeys.OuterFlamesEnergy,
                PluginConfig.OuterFlamesEnergy.Value);
            state.OuterFlamesChaos = ReadFloat(
                customData,
                VfxStateKeys.OuterFlamesChaos,
                PluginConfig.OuterFlamesChaos.Value);

            // Flare
            state.FlareEnabled = ReadBool(
                customData,
                VfxStateKeys.FlareEnabled,
                PluginConfig.Flare.Value);
            state.FlareScale = ReadFloat(
                customData,
                VfxStateKeys.FlareScale,
                PluginConfig.FlareScale.Value);
            state.FlareHue = ReadFloat(
                customData,
                VfxStateKeys.FlareHue,
                PluginConfig.FlareHue.Value);

            // Sparks
            state.SparksEnabled = ReadBool(
                customData,
                VfxStateKeys.SparksEnabled,
                PluginConfig.Sparks.Value);
            state.SparksHue = ReadFloat(
                customData,
                VfxStateKeys.SparksHue,
                PluginConfig.SparksHue.Value);
            state.SparksEnergy = ReadFloat(
                customData,
                VfxStateKeys.SparksEnergy,
                PluginConfig.SparksEnergy.Value);

            // Mirage
            state.MirageEnabled = ReadBool(
                customData,
                VfxStateKeys.MirageEnabled,
                PluginConfig.Mirage.Value);
            state.MirageScale = ReadFloat(
                customData,
                VfxStateKeys.MirageScale,
                PluginConfig.MirageScale.Value);
            state.MirageHue = ReadFloat(
                customData,
                VfxStateKeys.MirageHue,
                PluginConfig.MirageHue.Value);

            // Orbitals - Orbs
            state.OrbitalsOrbsEnabled = ReadBool(
                customData,
                VfxStateKeys.OrbitalsOrbsEnabled,
                PluginConfig.OrbitalsOrbs.Value);
            state.OrbitalsOrbsCount = ReadFloat(
                customData,
                VfxStateKeys.OrbitalsOrbsCount,
                PluginConfig.OrbitalsOrbsCount.Value);
            state.OrbitalsOrbsScale = ReadFloat(
                customData,
                VfxStateKeys.OrbitalsOrbsScale,
                PluginConfig.OrbitalsOrbsScale.Value);
            state.OrbitalsOrbsHue = ReadFloat(
                customData,
                VfxStateKeys.OrbitalsOrbsHue,
                PluginConfig.OrbitalsOrbsHue.Value);
            state.OrbitalsOrbsSpeed = ReadFloat(
                customData,
                VfxStateKeys.OrbitalsOrbsSpeed,
                PluginConfig.OrbitalsOrbsSpeed.Value);
            state.OrbitalsOrbsSpacing = ReadFloat(
                customData,
                VfxStateKeys.OrbitalsOrbsSpacing,
                PluginConfig.OrbitalsOrbsSpacing.Value);
            state.OrbitalsOrbsLength = ReadFloat(
                customData,
                VfxStateKeys.OrbitalsOrbsLength,
                PluginConfig.OrbitalsOrbsLength.Value);
            state.OrbitalsOrbsRadius = ReadFloat(
                customData,
                VfxStateKeys.OrbitalsOrbsRadius,
                PluginConfig.OrbitalsOrbsRadius.Value);
            state.OrbitalsOrbsCycles = ReadFloat(
                customData,
                VfxStateKeys.OrbitalsOrbsCycles,
                PluginConfig.OrbitalsOrbsCycles.Value);
            state.OrbitalsOrbsDrift = ReadFloat(
                customData,
                VfxStateKeys.OrbitalsOrbsDrift,
                PluginConfig.OrbitalsOrbsDrift.Value);

            // Orbitals - Flames
            state.OrbitalsFlamesEnabled = ReadBool(
                customData,
                VfxStateKeys.OrbitalsFlamesEnabled,
                PluginConfig.OrbitalsFlames.Value);
            state.OrbitalsFlamesCount = ReadFloat(
                customData,
                VfxStateKeys.OrbitalsFlamesCount,
                PluginConfig.OrbitalsFlamesCount.Value);
            state.OrbitalsFlamesHue = ReadFloat(
                customData,
                VfxStateKeys.OrbitalsFlamesHue,
                PluginConfig.OrbitalsFlamesHue.Value);
            state.OrbitalsFlamesEnergy = ReadFloat(
                customData,
                VfxStateKeys.OrbitalsFlamesEnergy,
                PluginConfig.OrbitalsFlamesEnergy.Value);
            state.OrbitalsFlamesSpeed = ReadFloat(
                customData,
                VfxStateKeys.OrbitalsFlamesSpeed,
                PluginConfig.OrbitalsFlamesSpeed.Value);
            state.OrbitalsFlamesSpacing = ReadFloat(
                customData,
                VfxStateKeys.OrbitalsFlamesSpacing,
                PluginConfig.OrbitalsFlamesSpacing.Value);
            state.OrbitalsFlamesLength = ReadFloat(
                customData,
                VfxStateKeys.OrbitalsFlamesLength,
                PluginConfig.OrbitalsFlamesLength.Value);
            state.OrbitalsFlamesRadius = ReadFloat(
                customData,
                VfxStateKeys.OrbitalsFlamesRadius,
                PluginConfig.OrbitalsFlamesRadius.Value);
            state.OrbitalsFlamesCycles = ReadFloat(
                customData,
                VfxStateKeys.OrbitalsFlamesCycles,
                PluginConfig.OrbitalsFlamesCycles.Value);
            state.OrbitalsFlamesDrift = ReadFloat(
                customData,
                VfxStateKeys.OrbitalsFlamesDrift,
                PluginConfig.OrbitalsFlamesDrift.Value);

            // Orbitals - Embers
            state.OrbitalsEmbersEnabled = ReadBool(
                customData,
                VfxStateKeys.OrbitalsEmbersEnabled,
                PluginConfig.OrbitalsEmbers.Value);
            state.OrbitalsEmbersCount = ReadFloat(
                customData,
                VfxStateKeys.OrbitalsEmbersCount,
                PluginConfig.OrbitalsEmbersCount.Value);
            state.OrbitalsEmbersHue = ReadFloat(
                customData,
                VfxStateKeys.OrbitalsEmbersHue,
                PluginConfig.OrbitalsEmbersHue.Value);
            state.OrbitalsEmbersEnergy = ReadFloat(
                customData,
                VfxStateKeys.OrbitalsEmbersEnergy,
                PluginConfig.OrbitalsEmbersEnergy.Value);
            state.OrbitalsEmbersSpeed = ReadFloat(
                customData,
                VfxStateKeys.OrbitalsEmbersSpeed,
                PluginConfig.OrbitalsEmbersSpeed.Value);
            state.OrbitalsEmbersSpacing = ReadFloat(
                customData,
                VfxStateKeys.OrbitalsEmbersSpacing,
                PluginConfig.OrbitalsEmbersSpacing.Value);
            state.OrbitalsEmbersLength = ReadFloat(
                customData,
                VfxStateKeys.OrbitalsEmbersLength,
                PluginConfig.OrbitalsEmbersLength.Value);
            state.OrbitalsEmbersRadius = ReadFloat(
                customData,
                VfxStateKeys.OrbitalsEmbersRadius,
                PluginConfig.OrbitalsEmbersRadius.Value);
            state.OrbitalsEmbersCycles = ReadFloat(
                customData,
                VfxStateKeys.OrbitalsEmbersCycles,
                PluginConfig.OrbitalsEmbersCycles.Value);
            state.OrbitalsEmbersDrift = ReadFloat(
                customData,
                VfxStateKeys.OrbitalsEmbersDrift,
                PluginConfig.OrbitalsEmbersDrift.Value);

            return true;
        }

        // Full item-state persistence entry point.
        // Currently kept as the canonical writer for VfxState -> item custom data.
        internal static void Write(global::ItemDrop.ItemData item, VfxState state)
        {
            if (item == null)
                return;

            if (item.m_customData == null)
                item.m_customData = new Dictionary<string, string>();

            Dictionary<string, string> customData = item.m_customData;

            // Inner Flames
            WriteBool(customData, VfxStateKeys.InnerFlamesEnabled, state.InnerFlamesEnabled);
            WriteFloat(customData, VfxStateKeys.InnerFlamesScale, state.InnerFlamesScale);
            WriteFloat(customData, VfxStateKeys.InnerFlamesHue, state.InnerFlamesHue);
            WriteFloat(customData, VfxStateKeys.InnerFlamesEnergy, state.InnerFlamesEnergy);
            WriteFloat(customData, VfxStateKeys.InnerFlamesChaos, state.InnerFlamesChaos);

            // Outer Flames
            WriteBool(customData, VfxStateKeys.OuterFlamesEnabled, state.OuterFlamesEnabled);
            WriteBool(customData, VfxStateKeys.OuterFlamesDragEnabled, state.OuterFlamesDragEnabled);
            WriteFloat(customData, VfxStateKeys.OuterFlamesScale, state.OuterFlamesScale);
            WriteFloat(customData, VfxStateKeys.OuterFlamesHue, state.OuterFlamesHue);
            WriteFloat(customData, VfxStateKeys.OuterFlamesEnergy, state.OuterFlamesEnergy);
            WriteFloat(customData, VfxStateKeys.OuterFlamesChaos, state.OuterFlamesChaos);

            // Flare
            WriteBool(customData, VfxStateKeys.FlareEnabled, state.FlareEnabled);
            WriteFloat(customData, VfxStateKeys.FlareScale, state.FlareScale);
            WriteFloat(customData, VfxStateKeys.FlareHue, state.FlareHue);

            // Sparks
            WriteBool(customData, VfxStateKeys.SparksEnabled, state.SparksEnabled);
            WriteFloat(customData, VfxStateKeys.SparksHue, state.SparksHue);
            WriteFloat(customData, VfxStateKeys.SparksEnergy, state.SparksEnergy);

            // Mirage
            WriteBool(customData, VfxStateKeys.MirageEnabled, state.MirageEnabled);
            WriteFloat(customData, VfxStateKeys.MirageScale, state.MirageScale);
            WriteFloat(customData, VfxStateKeys.MirageHue, state.MirageHue);

            // Orbitals - Orbs
            WriteBool(customData, VfxStateKeys.OrbitalsOrbsEnabled, state.OrbitalsOrbsEnabled);
            WriteFloat(customData, VfxStateKeys.OrbitalsOrbsCount, state.OrbitalsOrbsCount);
            WriteFloat(customData, VfxStateKeys.OrbitalsOrbsScale, state.OrbitalsOrbsScale);
            WriteFloat(customData, VfxStateKeys.OrbitalsOrbsHue, state.OrbitalsOrbsHue);
            WriteFloat(customData, VfxStateKeys.OrbitalsOrbsSpeed, state.OrbitalsOrbsSpeed);
            WriteFloat(customData, VfxStateKeys.OrbitalsOrbsSpacing, state.OrbitalsOrbsSpacing);
            WriteFloat(customData, VfxStateKeys.OrbitalsOrbsLength, state.OrbitalsOrbsLength);
            WriteFloat(customData, VfxStateKeys.OrbitalsOrbsRadius, state.OrbitalsOrbsRadius);
            WriteFloat(customData, VfxStateKeys.OrbitalsOrbsCycles, state.OrbitalsOrbsCycles);
            WriteFloat(customData, VfxStateKeys.OrbitalsOrbsDrift, state.OrbitalsOrbsDrift);

            // Orbitals - Flames
            WriteBool(customData, VfxStateKeys.OrbitalsFlamesEnabled, state.OrbitalsFlamesEnabled);
            WriteFloat(customData, VfxStateKeys.OrbitalsFlamesCount, state.OrbitalsFlamesCount);
            WriteFloat(customData, VfxStateKeys.OrbitalsFlamesHue, state.OrbitalsFlamesHue);
            WriteFloat(customData, VfxStateKeys.OrbitalsFlamesEnergy, state.OrbitalsFlamesEnergy);
            WriteFloat(customData, VfxStateKeys.OrbitalsFlamesSpeed, state.OrbitalsFlamesSpeed);
            WriteFloat(customData, VfxStateKeys.OrbitalsFlamesSpacing, state.OrbitalsFlamesSpacing);
            WriteFloat(customData, VfxStateKeys.OrbitalsFlamesLength, state.OrbitalsFlamesLength);
            WriteFloat(customData, VfxStateKeys.OrbitalsFlamesRadius, state.OrbitalsFlamesRadius);
            WriteFloat(customData, VfxStateKeys.OrbitalsFlamesCycles, state.OrbitalsFlamesCycles);
            WriteFloat(customData, VfxStateKeys.OrbitalsFlamesDrift, state.OrbitalsFlamesDrift);

            // Orbitals - Embers
            WriteBool(customData, VfxStateKeys.OrbitalsEmbersEnabled, state.OrbitalsEmbersEnabled);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersCount, state.OrbitalsEmbersCount);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersHue, state.OrbitalsEmbersHue);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersEnergy, state.OrbitalsEmbersEnergy);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersSpeed, state.OrbitalsEmbersSpeed);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersSpacing, state.OrbitalsEmbersSpacing);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersLength, state.OrbitalsEmbersLength);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersRadius, state.OrbitalsEmbersRadius);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersCycles, state.OrbitalsEmbersCycles);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersDrift, state.OrbitalsEmbersDrift);

            customData.Remove(LegacyInnerPresetKey);
        }

        internal static void EnsureInitializedFromConfig(global::ItemDrop.ItemData item)
        {
            if (item == null)
                return;

            if (item.m_customData == null)
                item.m_customData = new Dictionary<string, string>();

            Dictionary<string, string> customData = item.m_customData;

            // Always backfill missing keys from current config.

            // Inner Flames
            BackfillMissing(customData, VfxStateKeys.InnerFlamesEnabled, PluginConfig.InnerFlames.Value);
            BackfillMissing(customData, VfxStateKeys.InnerFlamesScale, PluginConfig.InnerFlamesScale.Value);
            BackfillMissing(customData, VfxStateKeys.InnerFlamesHue, PluginConfig.InnerFlamesHue.Value);
            BackfillMissing(customData, VfxStateKeys.InnerFlamesEnergy, PluginConfig.InnerFlamesEnergy.Value);
            BackfillMissing(customData, VfxStateKeys.InnerFlamesChaos, PluginConfig.InnerFlamesChaos.Value);

            // Outer Flames
            BackfillMissing(customData, VfxStateKeys.OuterFlamesEnabled, PluginConfig.OuterFlames.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesDragEnabled, PluginConfig.OuterFlamesDragEnabled.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesScale, PluginConfig.OuterFlamesScale.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesHue, PluginConfig.OuterFlamesHue.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesEnergy, PluginConfig.OuterFlamesEnergy.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesChaos, PluginConfig.OuterFlamesChaos.Value);

            // Flare
            BackfillMissing(customData, VfxStateKeys.FlareEnabled, PluginConfig.Flare.Value);
            BackfillMissing(customData, VfxStateKeys.FlareScale, PluginConfig.FlareScale.Value);
            BackfillMissing(customData, VfxStateKeys.FlareHue, PluginConfig.FlareHue.Value);

            // Sparks
            BackfillMissing(customData, VfxStateKeys.SparksEnabled, PluginConfig.Sparks.Value);
            BackfillMissing(customData, VfxStateKeys.SparksHue, PluginConfig.SparksHue.Value);
            BackfillMissing(customData, VfxStateKeys.SparksEnergy, PluginConfig.SparksEnergy.Value);

            // Mirage
            BackfillMissing(customData, VfxStateKeys.MirageEnabled, PluginConfig.Mirage.Value);
            BackfillMissing(customData, VfxStateKeys.MirageScale, PluginConfig.MirageScale.Value);
            BackfillMissing(customData, VfxStateKeys.MirageHue, PluginConfig.MirageHue.Value);

            // Orbitals - Orbs
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsEnabled, PluginConfig.OrbitalsOrbs.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsCount, PluginConfig.OrbitalsOrbsCount.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsScale, PluginConfig.OrbitalsOrbsScale.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsHue, PluginConfig.OrbitalsOrbsHue.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsSpeed, PluginConfig.OrbitalsOrbsSpeed.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsSpacing, PluginConfig.OrbitalsOrbsSpacing.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsLength, PluginConfig.OrbitalsOrbsLength.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsRadius, PluginConfig.OrbitalsOrbsRadius.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsCycles, PluginConfig.OrbitalsOrbsCycles.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsDrift, PluginConfig.OrbitalsOrbsDrift.Value);

            // Orbitals - Flames
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesEnabled, PluginConfig.OrbitalsFlames.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesCount, PluginConfig.OrbitalsFlamesCount.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesHue, PluginConfig.OrbitalsFlamesHue.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesEnergy, PluginConfig.OrbitalsFlamesEnergy.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesSpeed, PluginConfig.OrbitalsFlamesSpeed.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesSpacing, PluginConfig.OrbitalsFlamesSpacing.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesLength, PluginConfig.OrbitalsFlamesLength.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesRadius, PluginConfig.OrbitalsFlamesRadius.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesCycles, PluginConfig.OrbitalsFlamesCycles.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesDrift, PluginConfig.OrbitalsFlamesDrift.Value);

            // Orbitals - Embers
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersEnabled, PluginConfig.OrbitalsEmbers.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersCount, PluginConfig.OrbitalsEmbersCount.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersHue, PluginConfig.OrbitalsEmbersHue.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersEnergy, PluginConfig.OrbitalsEmbersEnergy.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersSpeed, PluginConfig.OrbitalsEmbersSpeed.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersSpacing, PluginConfig.OrbitalsEmbersSpacing.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersLength, PluginConfig.OrbitalsEmbersLength.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersRadius, PluginConfig.OrbitalsEmbersRadius.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersCycles, PluginConfig.OrbitalsEmbersCycles.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersDrift, PluginConfig.OrbitalsEmbersDrift.Value);
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

        private static void WriteFloat(Dictionary<string, string> customData, string key, float value)
        {
            if (customData == null)
                return;

            customData[key] = value.ToString(CultureInfo.InvariantCulture);
        }

        private static void WriteBool(Dictionary<string, string> customData, string key, bool value)
        {
            if (customData == null)
                return;

            customData[key] = value ? "true" : "false";
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