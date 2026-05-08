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
                entry.BoxedValue = false;
            }

            GUI.enabled = oldEnabled;

            GUILayout.EndHorizontal();
        }

        internal static void DrawLoadStyleDropdown(ConfigEntryBase entry)
        {
            string current = entry.BoxedValue as string ?? "Default";

            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

            if (GUILayout.Button(current, GUILayout.ExpandWidth(true)))
            {
                Plugin.Instance.LoadStyleIntoManager(current);
                _loadStyleDropdownOpen = !_loadStyleDropdownOpen;
            }

            if (GUILayout.Button("Reset", GUILayout.Width(StyleButtonWidth)))
            {
                entry.BoxedValue = "Default";
                Plugin.Instance.LoadStyleIntoManager("Default");
                _loadStyleDropdownOpen = false;
            }

            GUILayout.EndHorizontal();

            if (_loadStyleDropdownOpen)
            {
                foreach (string styleName in VfxStyleStore.GetStyleNames())
                {
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

        internal static void DrawSectionSpacer(ConfigEntryBase entry)
        {
            GUILayout.Label(
                GUIContent.none,
                GUILayout.Height(12),
                GUILayout.ExpandWidth(true));
        }
    }
}