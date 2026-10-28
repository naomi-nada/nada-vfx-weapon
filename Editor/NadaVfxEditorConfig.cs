using BepInEx.Configuration;
using UnityEngine;
using NADA.VFX.Weapon.Core.Config;
using NADA.VFX.Weapon.Weapons.Runtime;

namespace NADA.VFX.Weapon.Editor
{
    internal static class NadaVfxEditorConfig
    {
        internal static ConfigEntry<bool>
            EditingEnabled;

        internal static ConfigEntry<KeyboardShortcut>
            ToggleHotkey;

        internal static ConfigEntry<KeyboardShortcut>
            HideHotkey;

        internal static ConfigEntry<KeyboardShortcut>
            BindHotkey;

        internal static ConfigEntry<KeyboardShortcut>
            UnbindHotkey;

        internal static ConfigEntry<NadaVfxEditorThemePreset>
            Theme;

        internal static ConfigEntry<bool>
            LargeUi;

        internal static ConfigEntry<float>
            Opacity;

        internal static ConfigEntry<bool>
            AllowMovementWhileEditing;

        internal static ConfigEntry<bool>
            MultiplayerVisibility;

        // Editor-only layout preferences. Values are saved when the window
        // closes, never during OnGUI dragging or resizing.
        internal static ConfigEntry<int> WindowX;
        internal static ConfigEntry<int> WindowY;
        // Position of the small restore tab when the editor is minimized.
        internal static ConfigEntry<int> RestoreTabX;
        internal static ConfigEntry<int> RestoreTabY;
        internal static ConfigEntry<int> WindowWidth;
        internal static ConfigEntry<int> WindowHeight;

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

            HideHotkey = config.Bind(
                "Editor", "Hide Hotkey", KeyboardShortcut.Empty,
                Hidden("Temporarily hide/show the editor without closing the draft. F6 also restores it."));

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

            WindowX = config.Bind("Editor", "Window X", -1,
                Hidden("Last editor window horizontal position; -1 uses the default."));
            WindowY = config.Bind("Editor", "Window Y", -1,
                Hidden("Last editor window vertical position; -1 uses the default."));
            RestoreTabX = config.Bind("Editor", "Restore Tab X", -1,
                Hidden("Last minimized NADA tab horizontal position; -1 follows the editor initially."));
            RestoreTabY = config.Bind("Editor", "Restore Tab Y", -1,
                Hidden("Last minimized NADA tab vertical position; -1 follows the editor initially."));
            WindowWidth = config.Bind("Editor", "Window Width", 660,
                Hidden("Editor window width in pixels."));
            WindowHeight = config.Bind("Editor", "Window Height", 720,
                Hidden("Editor window height in pixels."));

            MultiplayerVisibility = config.Bind(
                "Editor", "Multiplayer Visibility", true,
                Hidden("Display other players' NADA weapon rigs. Does not alter replication."));
            MultiplayerVisibility.SettingChanged += (_, __) =>
            {
                config.Save();
                NadaVfxRightHandRemoteStateTracker.RefreshPresentationVisibility();
            };

            ToggleHotkey.SettingChanged += (_, __) => config.Save();
            HideHotkey.SettingChanged += (_, __) => config.Save();
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
                    "NadaDesert, NadaWinter, NadaNeo."));

            LargeUi = config.Bind(
                "Editor", "Large UI", false,
                Hidden("Enlarge the entire editor, including text and controls, by 30%."));

            Opacity = config.Bind(
                "Editor", "Opacity", 0.90f,
                new ConfigDescription(
                    "Editor-only translucency, from 35% to 100%.",
                    new AcceptableValueRange<float>(0.35f, 1f),
                    new ConfigurationManagerAttributes { Browsable = false }));

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
