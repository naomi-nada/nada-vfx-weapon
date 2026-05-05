using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using BepInEx;
using NADA.VFX.Core.State;

namespace NADA.VFX.Core.Styles
{
    internal static class VfxStyleStore
    {
        private const string DefaultStyleName = "Default";

        private static readonly Dictionary<string, VfxState> Styles =
            new(StringComparer.OrdinalIgnoreCase);

        private static readonly string FilePath =
            Path.Combine(Paths.ConfigPath, "naomi.nada.vfx.styles.txt");

        internal static IReadOnlyList<string> GetStyleNames()
        {
            EnsureLoaded();

            var names = new List<string> { DefaultStyleName };
            names.AddRange(Styles.Keys);
            names.Sort(StringComparer.OrdinalIgnoreCase);

            names.Remove(DefaultStyleName);
            names.Insert(0, DefaultStyleName);

            return names;
        }

        internal static bool TryGet(string name, out VfxState state)
        {
            EnsureLoaded();

            if (string.Equals(name, DefaultStyleName, StringComparison.OrdinalIgnoreCase))
            {
                state = VfxStateIO.FromDefaults();
                return true;
            }

            return Styles.TryGetValue(name ?? string.Empty, out state);
        }

        internal static bool Save(string name, VfxState state)
        {
            EnsureLoaded();

            if (string.IsNullOrWhiteSpace(name))
                return false;

            name = name.Trim();

            if (string.Equals(name, DefaultStyleName, StringComparison.OrdinalIgnoreCase))
                return false;

            Styles[name] = state;
            SaveToDisk();
            return true;
        }

        private static void EnsureLoaded()
        {
            Plugin.Log?.LogInfo($"{Plugin.ModName}: [Styles] EnsureLoaded() called.");
            
            if (Styles.Count > 0)
                return;

            if (!File.Exists(FilePath))
            {
                Plugin.Log?.LogWarning(
                    $"{Plugin.ModName}: [Styles] file not found at '{FilePath}'.");
                return;
            }

            try
            {
                int loaded = 0;
                int skipped = 0;

                foreach (string line in File.ReadAllLines(FilePath))
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    string[] parts = line.Split('|');

                    if (parts.Length < 2)
                    {
                        skipped++;
                        continue;
                    }

                    string name = parts[0].Trim();
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        skipped++;
                        continue;
                    }

                    Styles[name] = Deserialize(parts);
                    loaded++;
                }

                Plugin.Log?.LogInfo(
                    $"{Plugin.ModName}: [Styles] loaded={loaded} skipped={skipped} file='{FilePath}'.");
            }
            catch (Exception e)
            {
                Plugin.Log?.LogWarning($"{Plugin.ModName}: Failed to load style file: {e}");
            }
        }

        private static void SaveToDisk()
        {
            try
            {
                var lines = new List<string>();

                foreach (KeyValuePair<string, VfxState> pair in Styles)
                    lines.Add(Serialize(pair.Key, pair.Value));

                File.WriteAllLines(FilePath, lines);
            }
            catch (Exception e)
            {
                Plugin.Log?.LogWarning($"{Plugin.ModName}: Failed to save style file: {e}");
            }
        }
        
        internal static bool Delete(string name)
        {
            EnsureLoaded();

            if (string.IsNullOrWhiteSpace(name))
                return false;

            name = name.Trim();

            if (string.Equals(name, DefaultStyleName, StringComparison.OrdinalIgnoreCase))
                return false;

            if (!Styles.Remove(name))
                return false;

            SaveToDisk();
            return true;
        }

        private static string Serialize(string name, VfxState state)
        {
            return string.Join("|", new[]
            {
                name,
                
                F(state.RigRotation),
                F(state.RigSideRotation),
                F(state.RigLengthPosition),
                F(state.RigSidePosition),
                
                B(state.InnerFlamesEnabled),
                F(state.InnerFlamesEnergy),
                F(state.InnerFlamesScale),
                F(state.InnerFlamesLength),
                F(state.InnerFlamesHue),
                F(state.InnerFlamesPosition),

                B(state.OuterFlamesEnabled),
                B(state.OuterFlamesDragEnabled),
                F(state.OuterFlamesEnergy),
                F(state.OuterFlamesScale),
                F(state.OuterFlamesLength),
                F(state.OuterFlamesHue),
                F(state.OuterFlamesPosition),

                B(state.FlareEnabled),
                F(state.FlareScale),
                F(state.FlareHue),
                F(state.FlarePosition),
                
                B(state.SparksEnabled),
                F(state.SparksEnergy),
                F(state.SparksScale),
                F(state.SparksLength),
                F(state.SparksWidth),
                F(state.SparksHue),
                F(state.SparksPosition),

                B(state.OrbitalsOrbsEnabled),
                F(state.OrbitalsOrbsCount),
                F(state.OrbitalsOrbsDrift),
                F(state.OrbitalsOrbsScale),
                F(state.OrbitalsOrbsHue),
                F(state.OrbitalsOrbsSpeed),
                F(state.OrbitalsOrbsSpacing),
                F(state.OrbitalsOrbsLength),
                F(state.OrbitalsOrbsRadius),
                F(state.OrbitalsOrbsCycles),

                B(state.OrbitalsFlamesEnabled),
                F(state.OrbitalsFlamesCount),
                F(state.OrbitalsFlamesEnergy),
                F(state.OrbitalsFlamesDrift),
                F(state.OrbitalsFlamesHue),
                F(state.OrbitalsFlamesSpeed),
                F(state.OrbitalsFlamesSpacing),
                F(state.OrbitalsFlamesLength),
                F(state.OrbitalsFlamesRadius),
                F(state.OrbitalsFlamesCycles),

                B(state.OrbitalsEmbersEnabled),
                F(state.OrbitalsEmbersCount),
                F(state.OrbitalsEmbersEnergy),
                F(state.OrbitalsEmbersDrift),
                F(state.OrbitalsEmbersHue),
                F(state.OrbitalsEmbersSpeed),
                F(state.OrbitalsEmbersSpacing),
                F(state.OrbitalsEmbersLength),
                F(state.OrbitalsEmbersRadius),
                F(state.OrbitalsEmbersCycles)
            });
        }

        private static VfxState Deserialize(string[] parts)
        {
            int i = 1;
            VfxState defaults = VfxStateIO.FromDefaults();

            return new VfxState
            {
                RigRotation = ReadFloat(parts, ref i, defaults.RigRotation),
                RigSideRotation = ReadFloat(parts, ref i, defaults.RigSideRotation),
                RigLengthPosition = ReadFloat(parts, ref i, defaults.RigLengthPosition),
                RigSidePosition = ReadFloat(parts, ref i, defaults.RigSidePosition),

                InnerFlamesEnabled = ReadBool(parts, ref i, defaults.InnerFlamesEnabled),
                InnerFlamesEnergy = ReadFloat(parts, ref i, defaults.InnerFlamesEnergy),
                InnerFlamesScale = ReadFloat(parts, ref i, defaults.InnerFlamesScale),
                InnerFlamesLength =  ReadFloat(parts, ref i, defaults.InnerFlamesLength),
                InnerFlamesHue = ReadFloat(parts, ref i, defaults.InnerFlamesHue),
                InnerFlamesPosition = ReadFloat(parts, ref i, defaults.InnerFlamesPosition),

                OuterFlamesEnabled = ReadBool(parts, ref i, defaults.OuterFlamesEnabled),
                OuterFlamesDragEnabled = ReadBool(parts, ref i, defaults.OuterFlamesDragEnabled),
                OuterFlamesEnergy = ReadFloat(parts, ref i, defaults.OuterFlamesEnergy),
                OuterFlamesScale = ReadFloat(parts, ref i, defaults.OuterFlamesScale),
                OuterFlamesLength = ReadFloat(parts, ref i, defaults.OuterFlamesLength),
                OuterFlamesHue = ReadFloat(parts, ref i, defaults.OuterFlamesHue),
                OuterFlamesPosition = ReadFloat(parts, ref i, defaults.OuterFlamesPosition),

                FlareEnabled = ReadBool(parts, ref i, defaults.FlareEnabled),
                FlareScale = ReadFloat(parts, ref i, defaults.FlareScale),
                FlareHue = ReadFloat(parts, ref i, defaults.FlareHue),
                FlarePosition = ReadFloat(parts, ref i, defaults.FlarePosition),
                
                SparksEnabled = ReadBool(parts, ref i, defaults.SparksEnabled),
                SparksEnergy = ReadFloat(parts, ref i, defaults.SparksEnergy),
                SparksScale = ReadFloat(parts, ref i, defaults.SparksScale),
                SparksLength = ReadFloat(parts, ref i, defaults.SparksLength),
                SparksWidth =  ReadFloat(parts, ref i, defaults.SparksWidth),
                SparksHue = ReadFloat(parts, ref i, defaults.SparksHue),
                SparksPosition = ReadFloat(parts, ref i, defaults.SparksPosition),

                OrbitalsOrbsEnabled = ReadBool(parts, ref i, defaults.OrbitalsOrbsEnabled),
                OrbitalsOrbsCount = ReadFloat(parts, ref i, defaults.OrbitalsOrbsCount),
                OrbitalsOrbsDrift = ReadFloat(parts, ref i, defaults.OrbitalsOrbsDrift),
                OrbitalsOrbsScale = ReadFloat(parts, ref i, defaults.OrbitalsOrbsScale),
                OrbitalsOrbsHue = ReadFloat(parts, ref i, defaults.OrbitalsOrbsHue),
                OrbitalsOrbsSpeed = ReadFloat(parts, ref i, defaults.OrbitalsOrbsSpeed),
                OrbitalsOrbsSpacing = ReadFloat(parts, ref i, defaults.OrbitalsOrbsSpacing),
                OrbitalsOrbsLength = ReadFloat(parts, ref i, defaults.OrbitalsOrbsLength),
                OrbitalsOrbsRadius = ReadFloat(parts, ref i, defaults.OrbitalsOrbsRadius),
                OrbitalsOrbsCycles = ReadFloat(parts, ref i, defaults.OrbitalsOrbsCycles),

                OrbitalsFlamesEnabled = ReadBool(parts, ref i, defaults.OrbitalsFlamesEnabled),
                OrbitalsFlamesCount = ReadFloat(parts, ref i, defaults.OrbitalsFlamesCount),
                OrbitalsFlamesEnergy = ReadFloat(parts, ref i, defaults.OrbitalsFlamesEnergy),
                OrbitalsFlamesDrift = ReadFloat(parts, ref i, defaults.OrbitalsFlamesDrift),
                OrbitalsFlamesHue = ReadFloat(parts, ref i, defaults.OrbitalsFlamesHue),
                OrbitalsFlamesSpeed = ReadFloat(parts, ref i, defaults.OrbitalsFlamesSpeed),
                OrbitalsFlamesSpacing = ReadFloat(parts, ref i, defaults.OrbitalsFlamesSpacing),
                OrbitalsFlamesLength = ReadFloat(parts, ref i, defaults.OrbitalsFlamesLength),
                OrbitalsFlamesRadius = ReadFloat(parts, ref i, defaults.OrbitalsFlamesRadius),
                OrbitalsFlamesCycles = ReadFloat(parts, ref i, defaults.OrbitalsFlamesCycles),

                OrbitalsEmbersEnabled = ReadBool(parts, ref i, defaults.OrbitalsEmbersEnabled),
                OrbitalsEmbersCount = ReadFloat(parts, ref i, defaults.OrbitalsEmbersCount),
                OrbitalsEmbersEnergy = ReadFloat(parts, ref i, defaults.OrbitalsEmbersEnergy),
                OrbitalsEmbersDrift = ReadFloat(parts, ref i, defaults.OrbitalsEmbersDrift),
                OrbitalsEmbersHue = ReadFloat(parts, ref i, defaults.OrbitalsEmbersHue),
                OrbitalsEmbersSpeed = ReadFloat(parts, ref i, defaults.OrbitalsEmbersSpeed),
                OrbitalsEmbersSpacing = ReadFloat(parts, ref i, defaults.OrbitalsEmbersSpacing),
                OrbitalsEmbersLength = ReadFloat(parts, ref i, defaults.OrbitalsEmbersLength),
                OrbitalsEmbersRadius = ReadFloat(parts, ref i, defaults.OrbitalsEmbersRadius),
                OrbitalsEmbersCycles = ReadFloat(parts, ref i, defaults.OrbitalsEmbersCycles)
            };
        }

        private static string B(bool value)
        {
            return value ? "true" : "false";
        }

        private static string F(float value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
        
        private static bool ReadBool(string[] parts, ref int index, bool fallback)
        {
            if (parts == null || index >= parts.Length)
                return fallback;

            string value = parts[index++];
            return bool.TryParse(value, out bool parsed)
                ? parsed
                : fallback;
        }

        private static float ReadFloat(string[] parts, ref int index, float fallback)
        {
            if (parts == null || index >= parts.Length)
                return fallback;

            string value = parts[index++];

            return float.TryParse(
                value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out float parsed)
                ? parsed
                : fallback;
        }
    }
}