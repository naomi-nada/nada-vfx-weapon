using System.Collections.Generic;
using System.Globalization;
using NADA.VFX.Core.Config;

namespace NADA.VFX.Core.State
{
    internal struct VfxState
    {
        public bool InnerFlamesEnabled;
        public float InnerFlamesScale;
        public float InnerFlamesHue;
        public float InnerFlamesEnergy;
        public float InnerFlamesChaos;

        public bool OuterFlamesEnabled;
        public float OuterFlamesScale;
        public float OuterFlamesHue;
        public float OuterFlamesEnergy;
        public float OuterFlamesChaos;

        public bool FlareEnabled;
        public float FlareScale;
        public float FlareHue;

        public bool MirageEnabled;
        public float MirageScale;
        public float MirageHue;

        public bool SparksEnabled;
        public float SparksHue;
        public float SparksEnergy;

        public bool OrbitalsOrbsEnabled;
        public float OrbitalsOrbsCount;
        public float OrbitalsOrbsScale;
        public float OrbitalsOrbsHue;
        public float OrbitalsOrbsSpacing;
        public float OrbitalsOrbsRadius;

        public bool OrbitalsFlamesEnabled;
        public float OrbitalsFlamesCount;
        public float OrbitalsFlamesHue;
        public float OrbitalsFlamesEnergy;
        public float OrbitalsFlamesSpacing;
        public float OrbitalsFlamesRadius;

        public bool OrbitalsEmbersEnabled;
        public float OrbitalsEmbersCount;
        public float OrbitalsEmbersHue;
        public float OrbitalsEmbersEnergy;
        public float OrbitalsEmbersSpacing;
        public float OrbitalsEmbersRadius;
    }

    internal static class VfxStateIO
    {
        private const string LegacyInnerPresetKey = "nada.vfxfix.inner";

        internal static VfxState FromConfig()
        {
            return new VfxState
            {
                InnerFlamesEnabled = PluginConfig.InnerFlames.Value,
                InnerFlamesScale = PluginConfig.InnerFlamesScale.Value,
                InnerFlamesHue = PluginConfig.InnerFlamesHue.Value,
                InnerFlamesEnergy = PluginConfig.InnerFlamesEnergy.Value,
                InnerFlamesChaos = PluginConfig.InnerFlamesChaos.Value,
                
                OuterFlamesEnabled = PluginConfig.OuterFlames.Value,
                OuterFlamesScale = PluginConfig.OuterFlamesScale.Value,
                OuterFlamesHue = PluginConfig.OuterFlamesHue.Value,
                OuterFlamesEnergy = PluginConfig.OuterFlamesEnergy.Value,
                OuterFlamesChaos = PluginConfig.OuterFlamesChaos.Value,
                
                FlareEnabled = PluginConfig.Flare.Value,
                FlareScale = PluginConfig.FlareScale.Value,
                FlareHue = PluginConfig.FlareHue.Value,
                
                SparksEnabled = PluginConfig.Sparks.Value,
                SparksHue = PluginConfig.SparksHue.Value,
                SparksEnergy = PluginConfig.SparksEnergy.Value,
                
                MirageEnabled = PluginConfig.Mirage.Value,
                MirageScale = PluginConfig.MirageScale.Value,
                MirageHue = PluginConfig.MirageHue.Value,
                
                OrbitalsOrbsEnabled = PluginConfig.OrbitalsOrbs.Value,
                OrbitalsOrbsScale = PluginConfig.OrbitalsOrbsScale.Value,
                OrbitalsOrbsHue = PluginConfig.OrbitalsOrbsHue.Value,
                OrbitalsOrbsCount = PluginConfig.OrbitalsOrbsCount.Value,
                OrbitalsOrbsSpacing = PluginConfig.OrbitalsOrbsSpacing.Value,
                OrbitalsOrbsRadius = PluginConfig.OrbitalsOrbsRadius.Value,
                
                OrbitalsFlamesEnabled = PluginConfig.OrbitalsFlames.Value,
                OrbitalsFlamesCount = PluginConfig.OrbitalsFlamesCount.Value,
                OrbitalsFlamesHue = PluginConfig.OrbitalsFlamesHue.Value,
                OrbitalsFlamesEnergy = PluginConfig.OrbitalsFlamesEnergy.Value,
                OrbitalsFlamesSpacing = PluginConfig.OrbitalsFlamesSpacing.Value,
                OrbitalsFlamesRadius = PluginConfig.OrbitalsFlamesRadius.Value,
                
                OrbitalsEmbersEnabled = PluginConfig.OrbitalsEmbers.Value,
                OrbitalsEmbersCount = PluginConfig.OrbitalsEmbersCount.Value,
                OrbitalsEmbersHue = PluginConfig.OrbitalsEmbersHue.Value,
                OrbitalsEmbersEnergy = PluginConfig.OrbitalsEmbersEnergy.Value,
                OrbitalsEmbersSpacing = PluginConfig.OrbitalsEmbersSpacing.Value,
                OrbitalsEmbersRadius = PluginConfig.OrbitalsEmbersRadius.Value
            };
        }

        internal static bool TryRead(global::ItemDrop.ItemData item, out VfxState state)
        {
            state = default;
            if (item == null) return false;

            var cd = item.m_customData;
            if (cd == null) return false;
            
            state.InnerFlamesEnabled = ReadBool(cd, VfxStateKeys.InnerFlamesEnabled, PluginConfig.InnerFlames.Value);
            state.InnerFlamesScale = ReadFloat(cd, VfxStateKeys.InnerFlamesScale, PluginConfig.InnerFlamesScale.Value);
            state.InnerFlamesHue = ReadFloat(cd, VfxStateKeys.InnerFlamesHue, PluginConfig.InnerFlamesHue.Value);
            state.InnerFlamesEnergy = ReadFloat(cd, VfxStateKeys.InnerFlamesEnergy, PluginConfig.InnerFlamesEnergy.Value);
            state.InnerFlamesChaos = ReadFloat(cd, VfxStateKeys.InnerFlamesChaos, PluginConfig.InnerFlamesChaos.Value);
            
            state.OuterFlamesEnabled = ReadBool(cd, VfxStateKeys.OuterFlamesEnabled, PluginConfig.OuterFlames.Value);
            state.OuterFlamesScale = ReadFloat(cd, VfxStateKeys.OuterFlamesScale, PluginConfig.OuterFlamesScale.Value);
            state.OuterFlamesHue = ReadFloat(cd, VfxStateKeys.OuterFlamesHue, PluginConfig.OuterFlamesHue.Value);
            state.OuterFlamesEnergy = ReadFloat(cd, VfxStateKeys.OuterFlamesEnergy, PluginConfig.OuterFlamesEnergy.Value);
            state.OuterFlamesChaos = ReadFloat(cd, VfxStateKeys.OuterFlamesChaos, PluginConfig.OuterFlamesChaos.Value);
            
            state.FlareEnabled = ReadBool(cd, VfxStateKeys.FlareEnabled, PluginConfig.Flare.Value);
            state.FlareScale = ReadFloat(cd, VfxStateKeys.FlareScale, PluginConfig.FlareScale.Value);
            state.FlareHue = ReadFloat(cd, VfxStateKeys.FlareHue, PluginConfig.FlareHue.Value);
            
            state.MirageEnabled = ReadBool(cd, VfxStateKeys.MirageEnabled, PluginConfig.Mirage.Value);
            state.MirageScale = ReadFloat(cd, VfxStateKeys.MirageScale, PluginConfig.MirageScale.Value);
            state.MirageHue = ReadFloat(cd, VfxStateKeys.MirageHue, PluginConfig.MirageHue.Value);
            
            state.SparksEnabled = ReadBool(cd, VfxStateKeys.SparksEnabled, PluginConfig.Sparks.Value);
            state.SparksEnergy = ReadFloat(cd, VfxStateKeys.SparksEnergy, PluginConfig.SparksEnergy.Value);
            state.SparksHue = ReadFloat(cd, VfxStateKeys.SparksHue, PluginConfig.SparksHue.Value);

            state.OrbitalsOrbsEnabled = ReadBool(cd, VfxStateKeys.OrbitalsOrbsEnabled, PluginConfig.OrbitalsOrbs.Value);
            state.OrbitalsOrbsCount = ReadFloat(cd, VfxStateKeys.OrbitalsOrbsCount, PluginConfig.OrbitalsOrbsCount.Value);
            state.OrbitalsOrbsScale = ReadFloat(cd, VfxStateKeys.OrbitalsOrbsScale, PluginConfig.OrbitalsOrbsScale.Value);
            state.OrbitalsOrbsHue = ReadFloat(cd, VfxStateKeys.OrbitalsOrbsHue, PluginConfig.OrbitalsOrbsHue.Value);
            state.OrbitalsOrbsSpacing = ReadFloat(cd, VfxStateKeys.OrbitalsOrbsSpacing, PluginConfig.OrbitalsOrbsSpacing.Value);
            state.OrbitalsOrbsRadius = ReadFloat(cd, VfxStateKeys.OrbitalsOrbsRadius, PluginConfig.OrbitalsOrbsRadius.Value);

            state.OrbitalsFlamesEnabled = ReadBool(cd, VfxStateKeys.OrbitalsFlamesEnabled, PluginConfig.OrbitalsFlames.Value);
            state.OrbitalsFlamesCount = ReadFloat(cd, VfxStateKeys.OrbitalsFlamesCount, PluginConfig.OrbitalsFlamesCount.Value);
            state.OrbitalsFlamesHue = ReadFloat(cd, VfxStateKeys.OrbitalsFlamesHue, PluginConfig.OrbitalsFlamesHue.Value);
            state.OrbitalsFlamesEnergy = ReadFloat(cd, VfxStateKeys.OrbitalsFlamesEnergy, PluginConfig.OrbitalsFlamesEnergy.Value);
            state.OrbitalsFlamesSpacing = ReadFloat(cd, VfxStateKeys.OrbitalsFlamesSpacing, PluginConfig.OrbitalsFlamesSpacing.Value);
            state.OrbitalsFlamesRadius = ReadFloat(cd, VfxStateKeys.OrbitalsFlamesRadius, PluginConfig.OrbitalsFlamesRadius.Value);

            state.OrbitalsEmbersEnabled = ReadBool(cd, VfxStateKeys.OrbitalsEmbersEnabled, PluginConfig.OrbitalsEmbers.Value);
            state.OrbitalsEmbersCount = ReadFloat(cd, VfxStateKeys.OrbitalsEmbersCount, PluginConfig.OrbitalsEmbersCount.Value);
            state.OrbitalsEmbersHue = ReadFloat(cd, VfxStateKeys.OrbitalsEmbersHue, PluginConfig.OrbitalsEmbersHue.Value);
            state.OrbitalsEmbersEnergy = ReadFloat(cd, VfxStateKeys.OrbitalsEmbersEnergy, PluginConfig.OrbitalsEmbersEnergy.Value);
            state.OrbitalsEmbersSpacing = ReadFloat(cd, VfxStateKeys.OrbitalsEmbersSpacing, PluginConfig.OrbitalsEmbersSpacing.Value);
            state.OrbitalsEmbersRadius = ReadFloat(cd, VfxStateKeys.OrbitalsEmbersRadius, PluginConfig.OrbitalsEmbersRadius.Value);

            return true;
        }

        internal static void Write(global::ItemDrop.ItemData item, VfxState state)
        {
            if (item == null) return;
            if (item.m_customData == null)
                item.m_customData = new Dictionary<string, string>();

            WriteBool(item.m_customData, VfxStateKeys.InnerFlamesEnabled, state.InnerFlamesEnabled);
            WriteFloat(item.m_customData, VfxStateKeys.InnerFlamesScale, state.InnerFlamesScale);
            WriteFloat(item.m_customData, VfxStateKeys.InnerFlamesHue, state.InnerFlamesHue);
            WriteFloat(item.m_customData, VfxStateKeys.InnerFlamesEnergy, state.InnerFlamesEnergy);
            WriteFloat(item.m_customData, VfxStateKeys.InnerFlamesChaos, state.InnerFlamesChaos);
            
            WriteBool(item.m_customData, VfxStateKeys.OuterFlamesEnabled, state.OuterFlamesEnabled);
            WriteFloat(item.m_customData, VfxStateKeys.OuterFlamesScale, state.OuterFlamesScale);
            WriteFloat(item.m_customData, VfxStateKeys.OuterFlamesHue, state.OuterFlamesHue);
            WriteFloat(item.m_customData, VfxStateKeys.OuterFlamesEnergy, state.OuterFlamesEnergy);
            WriteFloat(item.m_customData, VfxStateKeys.OuterFlamesChaos, state.OuterFlamesChaos);
            
            WriteBool(item.m_customData, VfxStateKeys.FlareEnabled, state.FlareEnabled);
            WriteFloat(item.m_customData, VfxStateKeys.FlareScale, state.FlareScale);
            WriteFloat(item.m_customData, VfxStateKeys.FlareHue, state.FlareHue);

            WriteBool(item.m_customData, VfxStateKeys.OrbitalsOrbsEnabled, state.OrbitalsOrbsEnabled);
            WriteFloat(item.m_customData, VfxStateKeys.OrbitalsOrbsScale, state.OrbitalsOrbsScale);
            WriteFloat(item.m_customData, VfxStateKeys.OrbitalsOrbsCount, state.OrbitalsOrbsCount);
            WriteFloat(item.m_customData, VfxStateKeys.OrbitalsOrbsHue, state.OrbitalsOrbsHue);
            WriteFloat(item.m_customData, VfxStateKeys.OrbitalsOrbsSpacing, state.OrbitalsOrbsSpacing);
            WriteFloat(item.m_customData, VfxStateKeys.OrbitalsOrbsRadius, state.OrbitalsOrbsRadius);

            WriteBool(item.m_customData, VfxStateKeys.MirageEnabled, state.MirageEnabled);
            WriteFloat(item.m_customData, VfxStateKeys.MirageHue, state.MirageHue);
            WriteFloat(item.m_customData, VfxStateKeys.MirageScale, state.MirageScale);

            WriteBool(item.m_customData, VfxStateKeys.SparksEnabled, state.SparksEnabled);
            WriteFloat(item.m_customData, VfxStateKeys.SparksHue, state.SparksHue);
            WriteFloat(item.m_customData, VfxStateKeys.SparksEnergy, state.SparksEnergy);

            WriteBool(item.m_customData, VfxStateKeys.OrbitalsFlamesEnabled, state.OrbitalsFlamesEnabled);
            WriteFloat(item.m_customData, VfxStateKeys.OrbitalsFlamesHue, state.OrbitalsFlamesHue);
            WriteFloat(item.m_customData, VfxStateKeys.OrbitalsFlamesEnergy, state.OrbitalsFlamesEnergy);
            WriteFloat(item.m_customData, VfxStateKeys.OrbitalsFlamesCount, state.OrbitalsFlamesCount);
            WriteFloat(item.m_customData, VfxStateKeys.OrbitalsFlamesSpacing, state.OrbitalsFlamesSpacing);
            WriteFloat(item.m_customData, VfxStateKeys.OrbitalsFlamesRadius, state.OrbitalsFlamesRadius);

            WriteBool(item.m_customData, VfxStateKeys.OrbitalsEmbersEnabled, state.OrbitalsEmbersEnabled);
            WriteFloat(item.m_customData, VfxStateKeys.OrbitalsEmbersHue, state.OrbitalsEmbersHue);
            WriteFloat(item.m_customData, VfxStateKeys.OrbitalsEmbersEnergy, state.OrbitalsEmbersEnergy);
            WriteFloat(item.m_customData, VfxStateKeys.OrbitalsEmbersCount, state.OrbitalsEmbersCount);
            WriteFloat(item.m_customData, VfxStateKeys.OrbitalsEmbersSpacing, state.OrbitalsEmbersSpacing);
            WriteFloat(item.m_customData, VfxStateKeys.OrbitalsEmbersRadius, state.OrbitalsEmbersRadius);

            item.m_customData.Remove(LegacyInnerPresetKey);
        }

        internal static void EnsureInitializedFromConfig(global::ItemDrop.ItemData item)
        {
            if (item == null) return;
            if (item.m_customData == null)
                item.m_customData = new Dictionary<string, string>();

            // Always backfill missing keys from current config.
            
            BackfillMissing(item.m_customData, VfxStateKeys.InnerFlamesEnabled, PluginConfig.InnerFlames.Value);
            BackfillMissing(item.m_customData, VfxStateKeys.InnerFlamesScale, PluginConfig.InnerFlamesScale.Value);
            BackfillMissing(item.m_customData, VfxStateKeys.InnerFlamesHue, PluginConfig.InnerFlamesHue.Value);
            BackfillMissing(item.m_customData, VfxStateKeys.InnerFlamesEnergy, PluginConfig.InnerFlamesEnergy.Value);
            BackfillMissing(item.m_customData, VfxStateKeys.InnerFlamesChaos, PluginConfig.InnerFlamesChaos.Value);

            BackfillMissing(item.m_customData, VfxStateKeys.OuterFlamesEnabled, PluginConfig.OuterFlames.Value);
            BackfillMissing(item.m_customData, VfxStateKeys.OuterFlamesScale, PluginConfig.OuterFlamesScale.Value);
            BackfillMissing(item.m_customData, VfxStateKeys.OuterFlamesHue, PluginConfig.OuterFlamesHue.Value);
            BackfillMissing(item.m_customData, VfxStateKeys.OuterFlamesEnergy, PluginConfig.OuterFlamesEnergy.Value);
            BackfillMissing(item.m_customData, VfxStateKeys.OuterFlamesChaos, PluginConfig.OuterFlamesChaos.Value);
            
            BackfillMissing(item.m_customData, VfxStateKeys.FlareHue, PluginConfig.FlareHue.Value);

            BackfillMissing(item.m_customData, VfxStateKeys.OrbitalsOrbsEnabled, PluginConfig.OrbitalsOrbs.Value);
            BackfillMissing(item.m_customData, VfxStateKeys.OrbitalsOrbsScale, PluginConfig.OrbitalsOrbsScale.Value);
            BackfillMissing(item.m_customData, VfxStateKeys.OrbitalsOrbsCount, PluginConfig.OrbitalsOrbsCount.Value);
            BackfillMissing(item.m_customData, VfxStateKeys.OrbitalsOrbsHue, PluginConfig.OrbitalsOrbsHue.Value);
            BackfillMissing(item.m_customData, VfxStateKeys.OrbitalsOrbsSpacing, PluginConfig.OrbitalsOrbsSpacing.Value);
            BackfillMissing(item.m_customData, VfxStateKeys.OrbitalsOrbsRadius, PluginConfig.OrbitalsOrbsRadius.Value);

            BackfillMissing(item.m_customData, VfxStateKeys.MirageEnabled, PluginConfig.Mirage.Value);
            BackfillMissing(item.m_customData, VfxStateKeys.MirageHue, PluginConfig.MirageHue.Value);
            BackfillMissing(item.m_customData, VfxStateKeys.MirageScale, PluginConfig.MirageScale.Value);

            BackfillMissing(item.m_customData, VfxStateKeys.SparksEnabled, PluginConfig.Sparks.Value);
            BackfillMissing(item.m_customData, VfxStateKeys.SparksHue, PluginConfig.SparksHue.Value);
            BackfillMissing(item.m_customData, VfxStateKeys.SparksEnergy, PluginConfig.SparksEnergy.Value);

            BackfillMissing(item.m_customData, VfxStateKeys.OrbitalsFlamesEnabled, PluginConfig.OrbitalsFlames.Value);
            BackfillMissing(item.m_customData, VfxStateKeys.OrbitalsFlamesHue, PluginConfig.OrbitalsFlamesHue.Value);
            BackfillMissing(item.m_customData, VfxStateKeys.OrbitalsFlamesEnergy, PluginConfig.OrbitalsFlamesEnergy.Value);
            BackfillMissing(item.m_customData, VfxStateKeys.OrbitalsFlamesCount, PluginConfig.OrbitalsFlamesCount.Value);
            BackfillMissing(item.m_customData, VfxStateKeys.OrbitalsFlamesSpacing, PluginConfig.OrbitalsFlamesSpacing.Value);
            BackfillMissing(item.m_customData, VfxStateKeys.OrbitalsFlamesRadius, PluginConfig.OrbitalsFlamesRadius.Value);

            BackfillMissing(item.m_customData, VfxStateKeys.OrbitalsEmbersEnabled, PluginConfig.OrbitalsEmbers.Value);
            BackfillMissing(item.m_customData, VfxStateKeys.OrbitalsEmbersHue, PluginConfig.OrbitalsEmbersHue.Value);
            BackfillMissing(item.m_customData, VfxStateKeys.OrbitalsEmbersEnergy, PluginConfig.OrbitalsEmbersEnergy.Value);
            BackfillMissing(item.m_customData, VfxStateKeys.OrbitalsEmbersCount, PluginConfig.OrbitalsEmbersCount.Value);
            BackfillMissing(item.m_customData, VfxStateKeys.OrbitalsEmbersSpacing, PluginConfig.OrbitalsEmbersSpacing.Value);
            BackfillMissing(item.m_customData, VfxStateKeys.OrbitalsEmbersRadius, PluginConfig.OrbitalsEmbersRadius.Value);
        }

        private static float ReadFloat(Dictionary<string, string> cd, string key, float fallback)
        {
            if (cd == null) return fallback;
            if (!cd.TryGetValue(key, out var s) || string.IsNullOrWhiteSpace(s)) return fallback;

            if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var f))
                return f;

            return fallback;
        }

        private static bool ReadBool(Dictionary<string, string> cd, string key, bool fallback)
        {
            if (cd == null) return fallback;
            if (!cd.TryGetValue(key, out var s) || string.IsNullOrWhiteSpace(s)) return fallback;

            if (bool.TryParse(s, out var b))
                return b;

            return fallback;
        }

        private static void WriteFloat(Dictionary<string, string> cd, string key, float value)
        {
            if (cd == null) return;
            cd[key] = value.ToString(CultureInfo.InvariantCulture);
        }

        private static void WriteBool(Dictionary<string, string> cd, string key, bool value)
        {
            if (cd == null) return;
            cd[key] = value ? "true" : "false";
        }

        private static void BackfillMissing(Dictionary<string, string> cd, string key, float value)
        {
            if (cd == null) return;
            if (cd.ContainsKey(key)) return;
            cd[key] = value.ToString(CultureInfo.InvariantCulture);
        }

        private static void BackfillMissing(Dictionary<string, string> cd, string key, bool value)
        {
            if (cd == null) return;
            if (cd.ContainsKey(key)) return;
            cd[key] = value ? "true" : "false";
        }
    }
}