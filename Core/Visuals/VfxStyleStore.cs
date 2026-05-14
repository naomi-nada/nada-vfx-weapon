using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using NADA.VFX.Core.State;
using UnityEngine;

namespace NADA.VFX.Core.Visuals
{
    internal static class VfxStyleStore
    {
        private const string DefaultStyleName = "Default";

        private static readonly Dictionary<string, VfxState> Styles =
            new(StringComparer.OrdinalIgnoreCase);

        private static readonly string StylesDirectory =
            Path.Combine(Paths.ConfigPath, "NADA.VFX", "Styles");

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
            SaveStyleToDisk(name, state);

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

            if (!Styles.Remove(name))
                return false;

            try
            {
                string filePath = GetStyleFilePath(name);

                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch (Exception e)
            {
                Plugin.Log?.LogWarning($"{Plugin.ModName}: Failed to delete style '{name}': {e}");
            }

            return true;
        }

        private static void EnsureLoaded()
        {
            if (Styles.Count > 0)
                return;

            if (!Directory.Exists(StylesDirectory))
                return;

            try
            {
                int loaded = 0;
                int skipped = 0;

                foreach (string filePath in Directory.GetFiles(StylesDirectory, "*.json"))
                {
                    try
                    {
                        string json = File.ReadAllText(filePath);
                        VfxStyleSave save = JsonUtility.FromJson<VfxStyleSave>(json);

                        if (save == null || string.IsNullOrWhiteSpace(save.Name))
                        {
                            skipped++;
                            continue;
                        }

                        if (string.Equals(save.Name, DefaultStyleName, StringComparison.OrdinalIgnoreCase))
                        {
                            skipped++;
                            continue;
                        }

                        Styles[save.Name.Trim()] = VfxStateIO.FromStyleSave(save);
                        loaded++;
                    }
                    catch
                    {
                        skipped++;
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
                string json = JsonUtility.ToJson(save, true);

                File.WriteAllText(GetStyleFilePath(name), json);
            }
            catch (Exception e)
            {
                Plugin.Log?.LogWarning($"{Plugin.ModName}: Failed to save style '{name}': {e}");
            }
        }

        private static string GetStyleFilePath(string name)
        {
            string safeName = MakeSafeFileName(name);
            return Path.Combine(StylesDirectory, $"{safeName}.json");
        }

        private static string MakeSafeFileName(string name)
        {
            foreach (char invalidChar in Path.GetInvalidFileNameChars())
                name = name.Replace(invalidChar, '_');

            return string.IsNullOrWhiteSpace(name)
                ? "Unnamed"
                : name.Trim();
        }
    }
}