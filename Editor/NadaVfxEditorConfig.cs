using BepInEx.Configuration;
using NADA.VFX.Weapon.Core.Config;
using UnityEngine;

namespace NADA.VFX.Weapon.Editor
{
    internal static class NadaVfxEditorConfig
    {
        internal static ConfigEntry<bool> EditingEnabled;
        internal static ConfigEntry<KeyboardShortcut> ToggleHotkey;

        internal static void Bind(
            ConfigFile config)
        {
            EditingEnabled =
                config.Bind(
                    "EDITOR",
                    "Enable Editing",
                    false,
                    new ConfigDescription(
                        "Allow the in-game NADA VFX editor to be opened.",
                        null,
                        new ConfigurationManagerAttributes
                        {
                            Order = 10000,
                            DispName = "Enable Editing"
                        }));

            ToggleHotkey =
                config.Bind(
                    "EDITOR",
                    "Editor Hotkey",
                    new KeyboardShortcut(
                        KeyCode.F6),
                    new ConfigDescription(
                        "Open or close the NADA VFX editor.",
                        null,
                        new ConfigurationManagerAttributes
                        {
                            Order = 9900,
                            DispName = "Editor Hotkey"
                        }));

            EditingEnabled.SettingChanged +=
                (_, __) =>
                {
                    if (!EditingEnabled.Value)
                    {
                        NadaVfxEditor.Close();
                    }

                    config.Save();
                };

            ToggleHotkey.SettingChanged +=
                (_, __) =>
                {
                    config.Save();
                };
        }
    }
}