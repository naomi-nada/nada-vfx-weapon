using System;
using System.Globalization;
using UnityEngine;
using BepInEx.Configuration;

namespace NADA.VFX.Core.Config
{
    internal static class ConfigurationManagerDrawers
    {
        internal static void DrawUnbindWeaponButton(ConfigEntryBase entry)
        {
            if (GUILayout.Button("Unbind Current Weapon"))
            {
                Plugin.Instance.TryUnbindEquipped();

                // Keep this config value from behaving like a real toggle.
                entry.BoxedValue = false;
            }
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

        internal static void DrawDisabledSlider(ConfigEntryBase entry)
        {
            float value = (float)Convert.ToDouble(entry.BoxedValue, CultureInfo.InvariantCulture);

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            {
                bool oldEnabled = GUI.enabled;
                GUI.enabled = false;

                GUILayout.HorizontalSlider(value, 0f, 1f, GUILayout.ExpandWidth(true));
                GUILayout.Label($"{Mathf.RoundToInt(value * 100f)}%", GUILayout.Width(40));

                GUI.enabled = oldEnabled;
            }
            GUILayout.EndHorizontal();
        }
    }
}