using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using NADA.VFX.Core.State;

namespace NADA.VFX.Core.Visuals
{
    internal static class VfxStyleStore
    {
        private const string DefaultStyleName = "Default";

        private static readonly Dictionary<string, VfxState> Styles =
            new(StringComparer.OrdinalIgnoreCase);

        private static readonly string StylesDirectory =
            Path.Combine(Paths.ConfigPath, "NADA.VFX", "Styles");

        private static bool _loaded;

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

            name = string.IsNullOrWhiteSpace(name)
                ? DefaultStyleName
                : name.Trim();

            if (string.Equals(name, DefaultStyleName, StringComparison.OrdinalIgnoreCase))
            {
                state = VfxStateIO.FromDefaults();
                return true;
            }

            return Styles.TryGetValue(name, out state);
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
            SaveStyleToDisk(name, state);

            Plugin.Log?.LogInfo($"{Plugin.ModName}: [Styles] saved '{name}'.");

            return true;
        }

        internal static bool Delete(string name)
        {
            EnsureLoaded();

            if (string.IsNullOrWhiteSpace(name))
                return false;

            name = name.Trim();

            if (string.Equals(name, DefaultStyleName, StringComparison.OrdinalIgnoreCase))
                return false;

            bool removedFromMemory = Styles.Remove(name);
            bool deletedAnyFile = false;

            try
            {
                if (Directory.Exists(StylesDirectory))
                {
                    foreach (string filePath in Directory.GetFiles(StylesDirectory, "*.json"))
                    {
                        try
                        {
                            string json = File.ReadAllText(filePath);
                            VfxStyleSave save = VfxStyleJson.Read(json);

                            if (save == null || string.IsNullOrWhiteSpace(save.Name))
                                continue;

                            if (!string.Equals(save.Name.Trim(), name, StringComparison.OrdinalIgnoreCase))
                                continue;

                            File.Delete(filePath);
                            deletedAnyFile = true;

                            Plugin.Log?.LogInfo(
                                $"{Plugin.ModName}: [Styles] deleted '{name}' file '{filePath}'.");
                        }
                        catch (Exception e)
                        {
                            Plugin.Log?.LogWarning(
                                $"{Plugin.ModName}: [Styles] failed deleting style file '{filePath}': {e.Message}");
                        }
                    }
                }

                ReloadFromDisk();

                Plugin.Log?.LogInfo(
                    $"{Plugin.ModName}: [Styles] delete result name='{name}' memory={removedFromMemory} file={deletedAnyFile}.");

                return removedFromMemory || deletedAnyFile;
            }
            catch (Exception e)
            {
                Plugin.Log?.LogWarning($"{Plugin.ModName}: Failed to delete style '{name}': {e}");
                return false;
            }
        }

        internal static void ReloadFromDisk()
        {
            Styles.Clear();
            _loaded = false;
            EnsureLoaded();
        }

        private static void EnsureLoaded()
        {
            if (_loaded)
                return;

            _loaded = true;
            Styles.Clear();

            if (!Directory.Exists(StylesDirectory))
            {
                Plugin.Log?.LogInfo(
                    $"{Plugin.ModName}: [Styles] no styles directory yet: '{StylesDirectory}'.");
                return;
            }

            try
            {
                int loaded = 0;
                int skipped = 0;

                foreach (string filePath in Directory.GetFiles(StylesDirectory, "*.json"))
                {
                    try
                    {
                        string json = File.ReadAllText(filePath);

                        VfxStyleSave save = VfxStyleJson.Read(json);

                        if (save == null || string.IsNullOrWhiteSpace(save.Name))
                        {
                            skipped++;
                            continue;
                        }

                        string styleName = save.Name.Trim();

                        if (string.Equals(styleName, DefaultStyleName, StringComparison.OrdinalIgnoreCase))
                        {
                            skipped++;
                            continue;
                        }

                        Styles[styleName] = VfxStateIO.FromStyleSave(save);
                        loaded++;
                    }
                    catch (Exception e)
                    {
                        skipped++;
                        Plugin.Log?.LogWarning(
                            $"{Plugin.ModName}: [Styles] skipped broken style file '{filePath}': {e.Message}");
                    }
                }

                Plugin.Log?.LogInfo(
                    $"{Plugin.ModName}: [Styles] loaded={loaded} skipped={skipped} dir='{StylesDirectory}'.");
            }
            catch (Exception e)
            {
                Plugin.Log?.LogWarning($"{Plugin.ModName}: Failed to load styles: {e}");
            }
        }

        private static void SaveStyleToDisk(string name, VfxState state)
        {
            try
            {
                Directory.CreateDirectory(StylesDirectory);

                VfxStyleSave save = VfxStateIO.ToStyleSave(name, state);

                Plugin.Log?.LogInfo(
                    $"{Plugin.ModName}: [Styles] serializing '{name}' entries={save.Entries?.Count ?? -1}.");

                string json = VfxStyleJson.Write(save);

                Plugin.Log?.LogInfo(
                    $"{Plugin.ModName}: [Styles] json length={json.Length} containsEntries={json.Contains("Entries")}");

                string filePath = GetStyleFilePath(name);

                Plugin.Log?.LogInfo(
                    $"{Plugin.ModName}: [Styles] writing '{name}' to '{filePath}'.");

                File.WriteAllText(filePath, json);
            }
            catch (Exception e)
            {
                Plugin.Log?.LogWarning($"{Plugin.ModName}: Failed to save style '{name}': {e}");
            }
        }

        private static string GetStyleFilePath(string name)
        {
            string safeName = MakeSafeFileName(name);
            string hash = GetStableStyleHash(name);

            return Path.Combine(StylesDirectory, $"{safeName}_{hash}.json");
        }

        private static string MakeSafeFileName(string name)
        {
            foreach (char invalidChar in Path.GetInvalidFileNameChars())
                name = name.Replace(invalidChar, '_');

            return string.IsNullOrWhiteSpace(name)
                ? "Unnamed"
                : name.Trim();
        }

        private static string GetStableStyleHash(string name)
        {
            unchecked
            {
                const uint offsetBasis = 2166136261;
                const uint prime = 16777619;

                uint hash = offsetBasis;

                string normalized = (name ?? string.Empty)
                    .Trim()
                    .ToLowerInvariant();

                for (int i = 0; i < normalized.Length; i++)
                {
                    hash ^= normalized[i];
                    hash *= prime;
                }

                return hash.ToString("x8");
            }
        }
    }
}