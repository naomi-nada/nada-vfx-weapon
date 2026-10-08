using BepInEx.Configuration;
using UnityEngine;

namespace NADA.VFX.Weapon.Editor
{
    internal static class NadaVfxEditorConfig
    {
        internal static ConfigEntry<bool>
            EditingEnabled;

        internal static ConfigEntry<KeyboardShortcut>
            ToggleHotkey;

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
                    "Enables the in-game NADA VFX editor.");

            ToggleHotkey =
                config.Bind(
                    "Editor",
                    "Toggle Hotkey",
                    new KeyboardShortcut(
                        KeyCode.F6),
                    "Opens or closes the in-game NADA VFX editor.");

            Theme =
                config.Bind(
                    "Editor",
                    "Theme",
                    NadaVfxEditorThemePreset.NadaClassic,
                    "Editor skin. " +
                    "Available themes: " +
                    "NadaClassic, NadaDark, NadaVal, " +
                    "NadaDesert, NadaWinter.");

            AllowMovementWhileEditing =
                config.Bind(
                    "Editor",
                    "Allow Movement While Editing",
                    false,
                    "Allows normal movement keys while the NADA editor is open. " +
                    "Combat, jumping, blocking and dodging remain disabled.");
        }
    }
}