using BepInEx.Configuration;
using UnityEngine;
using NADA.VFX.Weapon.Core.Config;

namespace NADA.VFX.Weapon.Editor
{
    internal static class NadaVfxEditorConfig
    {
        internal static ConfigEntry<bool>
            EditingEnabled;

        internal static ConfigEntry<KeyboardShortcut>
            ToggleHotkey;

        internal static ConfigEntry<KeyboardShortcut>
            BindHotkey;

        internal static ConfigEntry<KeyboardShortcut>
            UnbindHotkey;

        internal static ConfigEntry<NadaVfxEditorThemePreset>
            Theme;

        internal static ConfigEntry<bool>
            AllowMovementWhileEditing;

        internal static void Bind(
            ConfigFile config)
        {
            if (config == null)
                return;

            EditingEnabled =
                config.Bind(
                    "Editor",
                    "Editing Enabled",
                    true,
                    Hidden("Enables the in-game NADA VFX editor."));

            ToggleHotkey =
                config.Bind(
                    "Editor",
                    "Toggle Hotkey",
                    new KeyboardShortcut(
                        KeyCode.F6),
                    Hidden("Opens or closes the in-game NADA VFX editor."));

            BindHotkey = config.Bind(
                "Editor",
                "Bind Hotkey",
                KeyboardShortcut.Empty,
                Hidden("Bind the editor draft to the equipped weapon while the editor is open."));

            UnbindHotkey = config.Bind(
                "Editor",
                "Unbind Hotkey",
                KeyboardShortcut.Empty,
                Hidden("Unbind the equipped weapon into an editable draft while the editor is open."));

            ToggleHotkey.SettingChanged += (_, __) => config.Save();
            BindHotkey.SettingChanged += (_, __) => config.Save();
            UnbindHotkey.SettingChanged += (_, __) => config.Save();

            Theme =
                config.Bind(
                    "Editor",
                    "Theme",
                    NadaVfxEditorThemePreset.NadaClassic,
                    Hidden("Editor skin. " +
                    "Available themes: " +
                    "NadaClassic, NadaDark, NadaVal, " +
                    "NadaDesert, NadaWinter."));

            AllowMovementWhileEditing =
                config.Bind(
                    "Editor",
                    "Allow Movement While Editing",
                    false,
                    Hidden("Allows normal movement keys while the NADA editor is open. " +
                    "Combat, jumping, blocking and dodging remain disabled."));
        }

        // Persist values for migration without duplicating controls in Configuration Manager.
        private static ConfigDescription Hidden(string description)
        {
            return new ConfigDescription(
                description,
                null,
                new ConfigurationManagerAttributes { Browsable = false });
        }
    }
}
