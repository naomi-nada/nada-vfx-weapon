using NADA.VFX.Weapon.Core.Config;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Blocks.Effects; 

namespace NADA.VFX.Weapon.Editor.Inspectors
{
    /// <summary>
    /// Strongly typed editor for one Inner Flames block.
    ///
    /// The current state still stores White/Black as two legacy-transition
    /// booleans. The editor presents them as one mutually exclusive mode.
    /// </summary>
    internal static class NadaInnerFlamesInspector
    {
        private const float MinEnergy = 0f;
        private const float MaxEnergy = 1f;

        private const float MinHue = -1f;
        private const float MaxHue = 1f;

        private static readonly string[] ColorModeOptions =
        {
            "Normal",
            "White",
            "Black"
        };

        private enum ColorMode
        {
            Normal = 0,
            White = 1,
            Black = 2
        }

        internal static bool Draw(
            VfxEffectBlock block)
        {
            if (block == null ||
                block.TypeId != VfxEffectTypeIds.InnerFlames)
            {
                return false;
            }

            if (block.Settings is not InnerFlamesVfxSettings flames)
            {
                NadaVfxEditorControls.Placeholder(
                    "Invalid Inner Flames block: expected InnerFlamesVfxSettings.");

                NadaVfxCommonBlockInspector.DrawTransform(
                    block);

                return false;
            }

            bool changed =
                false;

            string prefix =
                $"inner-flames:{block.InstanceId}";

            ColorMode colorMode =
                ResolveColorMode(
                    flames);

            if (NadaVfxEditorControls.Section(
                    $"{prefix}:color-mode",
                    "COLOR MODE"))
            {
                if (NadaVfxEditorControls.SegmentedSelector(
                        $"{prefix}:color-mode-selector",
                        "Mode",
                        (int)colorMode,
                        ColorModeOptions,
                        out int nextColorModeIndex))
                {
                    ApplyColorMode(
                        flames,
                        (ColorMode)nextColorModeIndex);

                    colorMode =
                        (ColorMode)nextColorModeIndex;

                    changed =
                        true;
                }

                NadaVfxEditorControls.Hint(
                    "White and Black replace hue coloring. " +
                    "Your Normal hue remains stored underneath.");
            }

            NadaVfxEditorControls.SpaceAfterSection();

            if (NadaVfxEditorControls.Section(
                    $"{prefix}:visual",
                    "VISUAL"))
            {
                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:scale",
                        "Scale",
                        flames.Scale,
                        PluginConfig.MinScaleMult,
                        PluginConfig.MaxScaleMult,
                        out float scale))
                {
                    flames.Scale =
                        scale;

                    changed =
                        true;
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:luminance",
                        "Luminance",
                        flames.Luminance,
                        PluginConfig.MinLuminance,
                        PluginConfig.MaxLuminance,
                        out float luminance))
                {
                    flames.Luminance =
                        luminance;

                    changed =
                        true;
                }

                bool hueEditable =
                    colorMode ==
                    ColorMode.Normal;

                string hueDisabledReason =
                    colorMode ==
                    ColorMode.White
                        ? "Hue is stored but inactive while White mode is selected."
                        : "Hue is stored but inactive while Black mode is selected.";

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:hue",
                        "Hue",
                        flames.Hue,
                        MinHue,
                        MaxHue,
                        out float hue,
                        enabled: hueEditable,
                        disabledReason:
                            hueEditable
                                ? null
                                : hueDisabledReason))
                {
                    flames.Hue =
                        hue;

                    changed =
                        true;
                }
            }

            NadaVfxEditorControls.SpaceAfterSection();

            if (NadaVfxEditorControls.Section(
                    $"{prefix}:particles",
                    "PARTICLES"))
            {
                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:energy",
                        "Energy",
                        flames.Energy,
                        MinEnergy,
                        MaxEnergy,
                        out float energy))
                {
                    flames.Energy =
                        energy;

                    changed =
                        true;
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:lifetime",
                        "Lifetime",
                        flames.Lifetime,
                        PluginConfig.MinLifetime,
                        PluginConfig.MaxLifetime,
                        out float lifetime))
                {
                    flames.Lifetime =
                        lifetime;

                    changed =
                        true;
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:simulation-speed",
                        "Simulation Speed",
                        flames.SimulationSpeed,
                        PluginConfig.MinSimulationSpeed,
                        PluginConfig.MaxSimulationSpeed,
                        out float simulationSpeed))
                {
                    flames.SimulationSpeed =
                        simulationSpeed;

                    changed =
                        true;
                }

                if (NadaVfxEditorControls.Toggle(
                        $"{prefix}:world-space",
                        "World Space",
                        flames.WorldEnabled,
                        out bool worldEnabled))
                {
                    flames.WorldEnabled =
                        worldEnabled;

                    changed =
                        true;
                }

                NadaVfxEditorControls.Hint(
                    "Energy controls particle emission. " +
                    "World Space lets emitted particles remain in world space as the weapon moves.");
            }

            NadaVfxEditorControls.SpaceAfterSection();

            if (NadaVfxEditorControls.Section(
                    $"{prefix}:shape",
                    "SHAPE"))
            {
                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:length",
                        "Length",
                        flames.Length,
                        PluginConfig.MinFlameLength,
                        PluginConfig.MaxFlameLength,
                        out float length))
                {
                    flames.Length =
                        length;

                    changed =
                        true;
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:width",
                        "Width",
                        flames.Width,
                        PluginConfig.MinFlameWidth,
                        PluginConfig.MaxFlameWidth,
                        out float width))
                {
                    flames.Width =
                        width;

                    changed =
                        true;
                }
            }

            NadaVfxEditorControls.SpaceAfterSection();

            changed |=
                NadaVfxCommonBlockInspector.DrawTransform(
                    block);

            return changed;
        }

        private static ColorMode ResolveColorMode(
            InnerFlamesVfxSettings settings)
        {
            // Match the existing runtime's precedence exactly.
            if (settings.WhiteEnabled)
            {
                return ColorMode.White;
            }

            if (settings.BlackEnabled)
            {
                return ColorMode.Black;
            }

            return ColorMode.Normal;
        }

        private static void ApplyColorMode(
            InnerFlamesVfxSettings settings,
            ColorMode mode)
        {
            settings.WhiteEnabled =
                mode ==
                ColorMode.White;

            settings.BlackEnabled =
                mode ==
                ColorMode.Black;
        }
    }
}