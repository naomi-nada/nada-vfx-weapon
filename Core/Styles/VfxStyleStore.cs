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
            if (Styles.Count > 0)
                return;

            if (!File.Exists(FilePath))
                return;

            try
            {
                foreach (string line in File.ReadAllLines(FilePath))
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    string[] parts = line.Split('|');
                    if (parts.Length != 51)
                        continue;

                    string name = parts[0].Trim();
                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    Styles[name] = Deserialize(parts);
                }
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
                F(state.InnerFlamesHue),
                F(state.InnerFlamesPosition),

                B(state.OuterFlamesEnabled),
                B(state.OuterFlamesDragEnabled),
                F(state.OuterFlamesEnergy),
                F(state.OuterFlamesScale),
                F(state.OuterFlamesHue),
                F(state.OuterFlamesPosition),

                B(state.FlareEnabled),
                F(state.FlareScale),
                F(state.FlareHue),

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

            return new VfxState
            {
                RigRotation = ReadFloat(parts[i++]),
                RigSideRotation = ReadFloat(parts[i++]),
                RigLengthPosition = ReadFloat(parts[i++]),
                RigSidePosition = ReadFloat(parts[i++]),
                
                InnerFlamesEnabled = ReadBool(parts[i++]),
                InnerFlamesEnergy = ReadFloat(parts[i++]),
                InnerFlamesScale = ReadFloat(parts[i++]),
                InnerFlamesHue = ReadFloat(parts[i++]),
                InnerFlamesPosition = ReadFloat(parts[i++]),

                OuterFlamesEnabled = ReadBool(parts[i++]),
                OuterFlamesDragEnabled = ReadBool(parts[i++]),
                OuterFlamesEnergy = ReadFloat(parts[i++]),
                OuterFlamesScale = ReadFloat(parts[i++]),
                OuterFlamesHue = ReadFloat(parts[i++]),
                OuterFlamesPosition = ReadFloat(parts[i++]),

                FlareEnabled = ReadBool(parts[i++]),
                FlareScale = ReadFloat(parts[i++]),
                FlareHue = ReadFloat(parts[i++]),

                OrbitalsOrbsEnabled = ReadBool(parts[i++]),
                OrbitalsOrbsCount = ReadFloat(parts[i++]),
                OrbitalsOrbsDrift = ReadFloat(parts[i++]),
                OrbitalsOrbsScale = ReadFloat(parts[i++]),
                OrbitalsOrbsHue = ReadFloat(parts[i++]),
                OrbitalsOrbsSpeed = ReadFloat(parts[i++]),
                OrbitalsOrbsSpacing = ReadFloat(parts[i++]),
                OrbitalsOrbsLength = ReadFloat(parts[i++]),
                OrbitalsOrbsRadius = ReadFloat(parts[i++]),
                OrbitalsOrbsCycles = ReadFloat(parts[i++]),

                OrbitalsFlamesEnabled = ReadBool(parts[i++]),
                OrbitalsFlamesCount = ReadFloat(parts[i++]),
                OrbitalsFlamesEnergy = ReadFloat(parts[i++]),
                OrbitalsFlamesDrift = ReadFloat(parts[i++]),
                OrbitalsFlamesHue = ReadFloat(parts[i++]),
                OrbitalsFlamesSpeed = ReadFloat(parts[i++]),
                OrbitalsFlamesSpacing = ReadFloat(parts[i++]),
                OrbitalsFlamesLength = ReadFloat(parts[i++]),
                OrbitalsFlamesRadius = ReadFloat(parts[i++]),
                OrbitalsFlamesCycles = ReadFloat(parts[i++]),

                OrbitalsEmbersEnabled = ReadBool(parts[i++]),
                OrbitalsEmbersCount = ReadFloat(parts[i++]),
                OrbitalsEmbersEnergy = ReadFloat(parts[i++]),
                OrbitalsEmbersDrift = ReadFloat(parts[i++]),
                OrbitalsEmbersHue = ReadFloat(parts[i++]),
                OrbitalsEmbersSpeed = ReadFloat(parts[i++]),
                OrbitalsEmbersSpacing = ReadFloat(parts[i++]),
                OrbitalsEmbersLength = ReadFloat(parts[i++]),
                OrbitalsEmbersRadius = ReadFloat(parts[i++]),
                OrbitalsEmbersCycles = ReadFloat(parts[i++])
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

        private static bool ReadBool(string value)
        {
            return bool.TryParse(value, out bool parsed) && parsed;
        }

        private static float ReadFloat(string value)
        {
            return float.TryParse(
                value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out float parsed)
                ? parsed
                : 0f;
        }
    }
}