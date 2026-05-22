using System;
using System.Globalization;
using UnityEngine;
using BepInEx.Configuration;
using NADA.VFX.Core.Visuals;

namespace NADA.VFX.Core.Config
{
    internal static class ConfigurationManagerDrawers
    {
        private const float StyleButtonWidth = 46f;

        private static bool _loadStyleDropdownOpen;

        internal static void DrawUnbindWeaponButton(ConfigEntryBase entry)
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

            GUILayout.Space(264f);

            if (GUILayout.Button("Unbind Current Weapon", GUILayout.Width(230f)))
            {
                Plugin.Instance.TryUnbindEquipped();
                entry.BoxedValue = false;
            }

            GUILayout.EndHorizontal();
        }

        internal static void DrawEnabledCheckboxWithLabel(ConfigEntryBase entry)
        {
            bool current = (bool)entry.BoxedValue;

            string label = entry.Definition.Key;

            if (entry.Description?.Tags != null)
            {
                foreach (object tag in entry.Description.Tags)
                {
                    if (tag is ConfigurationManagerAttributes attributes &&
                        !string.IsNullOrEmpty(attributes.DispName))
                    {
                        label = attributes.DispName;
                        break;
                    }
                }
            }

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

            GUILayout.Label(label, GUILayout.ExpandWidth(false));
            GUILayout.Space(4);

            bool next = GUILayout.Toggle(current, GUIContent.none, GUILayout.Width(18));
            if (next != current)
                entry.BoxedValue = next;

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        internal static void DrawDisabledCheckboxWithLabel(ConfigEntryBase entry)
        {
            if (entry != null && entry.BoxedValue is bool current && current)
                entry.BoxedValue = false;

            bool oldEnabled = GUI.enabled;
            GUI.enabled = false;

            DrawEnabledCheckboxWithLabel(entry);

            GUI.enabled = oldEnabled;
        }

        internal static void DrawInnerFlamesBlackCheckbox(ConfigEntryBase entry)
        {
            DrawDisabledCheckboxWithLabel(entry);
        }

        internal static void DrawInnerFlamesWhiteCheckbox(ConfigEntryBase entry)
        {
            DrawExclusiveCheckboxWithLabel(
                entry,
                PluginConfig.InnerFlamesBlack);
        }

        internal static void DrawInnerFlamesColorSlider(ConfigEntryBase entry)
        {
            bool disabled =
                IsEnabled(PluginConfig.InnerFlamesWhite);

            DrawFloatSlider(entry, disabled);
        }

        internal static void DrawOuterFlamesBlackCheckbox(ConfigEntryBase entry)
        {
            DrawDisabledCheckboxWithLabel(entry);
        }

        internal static void DrawOuterFlamesWhiteCheckbox(ConfigEntryBase entry)
        {
            DrawExclusiveCheckboxWithLabel(
                entry,
                PluginConfig.OuterFlamesBlack);
        }

        internal static void DrawOuterFlamesColorSlider(ConfigEntryBase entry)
        {
            bool disabled =
                IsEnabled(PluginConfig.OuterFlamesWhite);

            DrawFloatSlider(entry, disabled);
        }

        private static void DrawExclusiveCheckboxWithLabel(
            ConfigEntryBase entry,
            ConfigEntry<bool> mutuallyExclusiveEntry)
        {
            bool current = (bool)entry.BoxedValue;

            string label = entry.Definition.Key;

            if (entry.Description?.Tags != null)
            {
                foreach (object tag in entry.Description.Tags)
                {
                    if (tag is ConfigurationManagerAttributes attributes &&
                        !string.IsNullOrEmpty(attributes.DispName))
                    {
                        label = attributes.DispName;
                        break;
                    }
                }
            }

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

            GUILayout.Label(label, GUILayout.ExpandWidth(false));
            GUILayout.Space(4);

            bool next = GUILayout.Toggle(current, GUIContent.none, GUILayout.Width(18));
            if (next != current)
            {
                entry.BoxedValue = next;

                if (next && mutuallyExclusiveEntry != null)
                    mutuallyExclusiveEntry.Value = false;
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private static bool IsEnabled(ConfigEntry<bool> entry)
        {
            return entry != null && entry.Value;
        }

        internal static void DrawOrbsSpacingSlider(ConfigEntryBase entry)
        {
            bool snakeEnabled =
                PluginConfig.OrbitalsOrbsSnake != null &&
                PluginConfig.OrbitalsOrbsSnake.Value;

            DrawFloatSlider(entry, snakeEnabled);
        }

        internal static void DrawStrandsColorSlider(ConfigEntryBase entry)
        {
            bool disabled =
                PluginConfig.StrandsSpectrum != null &&
                PluginConfig.StrandsSpectrum.Value;

            DrawFloatSlider(entry, disabled);
        }

        internal static void DrawStrandsSpectrumSpeedSlider(ConfigEntryBase entry)
        {
            bool disabled =
                PluginConfig.StrandsSpectrum == null ||
                !PluginConfig.StrandsSpectrum.Value;

            DrawFloatSlider(entry, disabled);
        }

        internal static void DrawOrbitalsCoresSpinSpeedSlider(ConfigEntryBase entry)
        {
            bool disabled =
                PluginConfig.OrbitalsCoresSpin == null ||
                !PluginConfig.OrbitalsCoresSpin.Value;

            DrawFloatSlider(entry, disabled);
        }

        private static void DrawFloatSlider(ConfigEntryBase entry, bool disabled)
        {
            if (entry == null)
                return;

            if (entry.SettingType != typeof(float))
            {
                GUILayout.Label(entry.BoxedValue?.ToString() ?? string.Empty);
                return;
            }

            float current = (float)entry.BoxedValue;

            float min = 0f;
            float max = 1f;

            AcceptableValueRange<float> range = TryGetFloatRange(entry);
            if (range != null)
            {
                min = range.MinValue;
                max = range.MaxValue;
            }

            bool oldEnabled = GUI.enabled;
            GUI.enabled = oldEnabled && !disabled;

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

            float next = GUILayout.HorizontalSlider(
                current,
                min,
                max,
                GUILayout.ExpandWidth(true));

            GUILayout.Label(
                next.ToString("0.###", CultureInfo.InvariantCulture),
                GUILayout.Width(48f));

            if (!Mathf.Approximately(next, current))
                entry.BoxedValue = next;

            GUILayout.EndHorizontal();

            GUI.enabled = oldEnabled;
        }

        private static AcceptableValueRange<float> TryGetFloatRange(ConfigEntryBase entry)
        {
            if (entry?.Description?.AcceptableValues is AcceptableValueRange<float> range)
                return range;

            return null;
        }

        internal static void DrawSaveStyleRow(ConfigEntryBase entry)
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

            string currentName = PluginConfig.StyleName?.Value ?? string.Empty;

            string nextName = GUILayout.TextField(
                currentName,
                GUILayout.ExpandWidth(true));

            if (PluginConfig.StyleName != null && nextName != currentName)
                PluginConfig.StyleName.Value = nextName;

            bool oldEnabled = GUI.enabled;
            GUI.enabled = oldEnabled && !string.IsNullOrWhiteSpace(currentName);

            if (GUILayout.Button("Save", GUILayout.Width(46f)))
            {
                Plugin.Instance.SaveCurrentStyleFromManager(currentName);
                VfxStyleStore.ReloadFromDisk();
                entry.BoxedValue = false;
            }

            GUI.enabled = oldEnabled;

            GUILayout.EndHorizontal();
        }

        internal static void DrawLoadStyleDropdown(ConfigEntryBase entry)
        {
            string current = entry.BoxedValue as string ?? "Default";
            if (string.IsNullOrWhiteSpace(current))
                current = "Default";

            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

            if (GUILayout.Button(current, GUILayout.ExpandWidth(true)))
            {
                _loadStyleDropdownOpen = !_loadStyleDropdownOpen;
            }

            if (GUILayout.Button("Reset", GUILayout.Width(StyleButtonWidth)))
            {
                if (!string.Equals(current, "Default", StringComparison.OrdinalIgnoreCase))
                {
                    entry.BoxedValue = "Default";
                    Plugin.Instance.LoadStyleIntoManager("Default");
                }

                _loadStyleDropdownOpen = false;
            }

            bool oldEnabled = GUI.enabled;
            bool canDelete =
                !string.Equals(current, "Default", StringComparison.OrdinalIgnoreCase);

            GUI.enabled = oldEnabled && canDelete;

            if (GUILayout.Button("Delete", GUILayout.Width(StyleButtonWidth)))
            {
                if (VfxStyleStore.Delete(current))
                {
                    entry.BoxedValue = "Default";
                    Plugin.Instance.LoadStyleIntoManager("Default");
                    VfxStyleStore.ReloadFromDisk();
                }

                _loadStyleDropdownOpen = false;
            }

            GUI.enabled = oldEnabled;

            GUILayout.EndHorizontal();

            if (_loadStyleDropdownOpen)
            {
                foreach (string styleName in VfxStyleStore.GetStyleNames())
                {
                    bool isCurrent =
                        string.Equals(styleName, current, StringComparison.OrdinalIgnoreCase);

                    bool isDefault =
                        string.Equals(styleName, "Default", StringComparison.OrdinalIgnoreCase);

                    if (isCurrent && !isDefault)
                        continue;

                    if (GUILayout.Button(styleName, GUILayout.ExpandWidth(true)))
                    {
                        entry.BoxedValue = styleName;
                        Plugin.Instance.LoadStyleIntoManager(styleName);
                        _loadStyleDropdownOpen = false;
                    }
                }
            }

            GUILayout.EndVertical();
        }

        internal static void DrawOrbitalsOrbsSyncButton(ConfigEntryBase entry)
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

            GUILayout.Space(264f);

            if (GUILayout.Button("Sync", GUILayout.Width(230f)))
            {
                PluginConfig.SyncOrbitalsToOrbs();
                entry.BoxedValue = false;
            }

            GUILayout.EndHorizontal();
        }

        internal static void DrawGlueLockedFloatSlider(ConfigEntryBase entry)
        {
            bool orbsGlueActive =
                PluginConfig.OrbitalsOrbs != null &&
                PluginConfig.OrbitalsOrbs.Value &&
                PluginConfig.OrbitalsOrbsGlue != null &&
                PluginConfig.OrbitalsOrbsGlue.Value;

            bool coresGlueActive =
                PluginConfig.OrbitalsCores != null &&
                PluginConfig.OrbitalsCores.Value &&
                PluginConfig.OrbitalsCoresGlue != null &&
                PluginConfig.OrbitalsCoresGlue.Value;

            string key = entry?.Definition.Key ?? string.Empty;
            string section = entry?.Definition.Section ?? string.Empty;

            bool isOrbsSlider = section.Contains("ORBS");
            bool isCoresSlider = section.Contains("CORES");
            bool isFlamesSlider = section.Contains("FLAMES");
            bool isEmbersSlider = section.Contains("EMBERS");

            bool disabled =
                (orbsGlueActive && (isCoresSlider || isFlamesSlider || isEmbersSlider)) ||
                (coresGlueActive && (isOrbsSlider || isFlamesSlider || isEmbersSlider));

            DrawFloatSlider(entry, disabled);
        }
        
        internal static void DrawSectionSpacer(ConfigEntryBase entry)
        {
            GUILayout.Label(
                GUIContent.none,
                GUILayout.Height(12),
                GUILayout.ExpandWidth(true));
        }
        
        internal static void DrawOrbitalsCoresSyncButton(ConfigEntryBase entry)
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

            GUILayout.Space(264f);

            if (GUILayout.Button("Sync", GUILayout.Width(230f)))
            {
                PluginConfig.SyncOrbitalsToCores();
                entry.BoxedValue = false;
            }

            GUILayout.EndHorizontal();
        }
    }
}
