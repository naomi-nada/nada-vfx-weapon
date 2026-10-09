using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using NADA.VFX.Weapon.Core.State.Blocks;

namespace NADA.VFX.Weapon.Core.Persistence
{
    // Native editor/API style storage. Legacy config styles remain in
    // Core/Visuals/VfxStyleStore and are never modified by this class.
    // No ItemData, ConfigEntry, runtime rig, or Unity object is involved.
    internal static class WeaponVfxStyleStore
    {
        private const string Extension = ".nadastyle";
        private static readonly string DirectoryPath = Path.Combine(
            Paths.ConfigPath, "NADA.VFX.Weapon", "Styles", "Blocks");

        // Enumeration is triggered by the editor opening or a user action,
        // never by every IMGUI Layout/Repaint. Save/Delete invalidates it.
        private static IReadOnlyList<string> _cachedNames;

        internal static IReadOnlyList<string> GetNames()
        {
            if (_cachedNames != null)
                return _cachedNames;

            var result = new List<string> { "Default" };
            try
            {
                if (Directory.Exists(DirectoryPath))
                {
                    foreach (string path in Directory.GetFiles(
                                 DirectoryPath, "*" + Extension))
                    {
                        if (TryReadFile(path, out string name,
                                out _, out _, out _))
                            result.Add(name);
                    }
                }
            }
            catch (Exception ex) when (ex is IOException ||
                                       ex is UnauthorizedAccessException ||
                                       ex is ArgumentException)
            {
                Plugin.Log?.LogWarning(
                    $"{Plugin.ModName}: [NativeStyleListUnavailable] {ex.Message}");
            }
            result.Sort(1, result.Count - 1, StringComparer.OrdinalIgnoreCase);
            _cachedNames = result.AsReadOnly();
            return _cachedNames;
        }

        internal static IReadOnlyList<string> RefreshNames()
        {
            // Explicit refresh is for changes made outside NADA while running.
            _cachedNames = null;
            return GetNames();
        }

        internal static bool TryLoad(
            string name,
            out WeaponVfxState state,
            out uint nextInstanceId,
            out string reason)
        {
            state = null;
            nextInstanceId = 0;
            reason = null;
            if (string.Equals(name?.Trim(), "Default", StringComparison.OrdinalIgnoreCase))
            {
                // An empty editor stack is the native default. Adding an
                // effect supplies its typed defaults at that moment.
                state = new WeaponVfxState();
                nextInstanceId = 1;
                return true;
            }
            if (!WeaponVfxStyleCodec.TryNormalizeName(name, out string normalized))
            {
                reason = "Invalid native style name.";
                return false;
            }

            string path = GetPath(normalized);
            if (!TryReadFile(path, out string savedName,
                    out state, out nextInstanceId, out reason))
                return false;

            if (!string.Equals(savedName, normalized, StringComparison.OrdinalIgnoreCase))
            {
                state = null;
                nextInstanceId = 0;
                reason = "Native style file name does not match requested style.";
                return false;
            }
            return true;
        }

        internal static bool TrySave(
            string name,
            WeaponVfxState state,
            uint nextInstanceId,
            out string reason)
        {
            if (!WeaponVfxStyleCodec.TryEncode(
                    name, state, nextInstanceId, out byte[] bytes, out reason))
                return false;

            WeaponVfxStyleCodec.TryNormalizeName(name, out string normalized);
            string path = GetPath(normalized);
            string temporary = path + ".tmp";
            try
            {
                Directory.CreateDirectory(DirectoryPath);
                File.WriteAllBytes(temporary, bytes);
                if (File.Exists(path))
                {
                    // A failed replacement must leave the previous saved
                    // style intact. Never delete the destination first.
                    File.Replace(temporary, path, null);
                }
                else
                {
                    File.Move(temporary, path);
                }
                _cachedNames = null;
                return true;
            }
            catch (Exception ex) when (ex is IOException ||
                                       ex is UnauthorizedAccessException ||
                                       ex is ArgumentException ||
                                       ex is NotSupportedException)
            {
                reason = "Could not write native style: " + ex.Message;
                return false;
            }
            finally
            {
                try
                {
                    if (File.Exists(temporary)) File.Delete(temporary);
                }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
        }

        internal static bool TryDelete(string name, out string reason)
        {
            reason = null;
            if (!WeaponVfxStyleCodec.TryNormalizeName(name, out string normalized))
            {
                reason = "Invalid native style name.";
                return false;
            }
            try
            {
                string path = GetPath(normalized);
                if (!File.Exists(path))
                {
                    reason = "Native style does not exist.";
                    return false;
                }
                File.Delete(path);
                _cachedNames = null;
                return true;
            }
            catch (Exception ex) when (ex is IOException ||
                                       ex is UnauthorizedAccessException ||
                                       ex is ArgumentException)
            {
                reason = "Could not delete native style: " + ex.Message;
                return false;
            }
        }

        private static bool TryReadFile(
            string path,
            out string name,
            out WeaponVfxState state,
            out uint nextInstanceId,
            out string reason)
        {
            name = null;
            state = null;
            nextInstanceId = 0;
            reason = null;
            try
            {
                var info = new FileInfo(path);
                if (!info.Exists || info.Length > WeaponVfxStyleCodec.MaxFileBytes)
                {
                    reason = "Native style missing or oversized.";
                    return false;
                }
                return WeaponVfxStyleCodec.TryDecode(
                    File.ReadAllBytes(path), out name, out state,
                    out nextInstanceId, out reason);
            }
            catch (Exception ex) when (ex is IOException ||
                                       ex is UnauthorizedAccessException ||
                                       ex is ArgumentException)
            {
                reason = "Could not read native style: " + ex.Message;
                return false;
            }
        }

        private static string GetPath(string name)
        {
            // Cross-platform deterministic name hashing; avoid the platform's
            // GetHashCode and never interpret a style name as a directory path.
            uint hash = 2166136261;
            string lower = name.ToLowerInvariant();
            unchecked
            {
                foreach (char ch in lower)
                {
                    hash ^= ch;
                    hash *= 16777619;
                }
            }
            var safeName = new System.Text.StringBuilder();
            foreach (char ch in lower)
            {
                if (safeName.Length >= 32) break;
                safeName.Append((ch >= 'a' && ch <= 'z') ||
                                (ch >= 'A' && ch <= 'Z') ||
                                (ch >= '0' && ch <= '9') || ch == '-'
                    ? ch : '_');
            }
            return Path.Combine(DirectoryPath,
                safeName + "_" + hash.ToString("x8") + Extension);
        }
    }
}
