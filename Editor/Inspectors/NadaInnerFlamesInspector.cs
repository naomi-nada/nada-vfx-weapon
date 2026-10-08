using NADA.VFX.Weapon.Core.Config;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Blocks.Effects;
using NADA.VFX.Weapon.Core.State.Defaults;

namespace NADA.VFX.Weapon.Editor.Inspectors
{
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
                        out int nextColorModeIndex,
                        description:
                            "Normal uses Hue. White and Black replace hue coloring without erasing the stored Hue value."))
                {
                    ApplyColorMode(
                        flames,
                        (ColorMode)nextColorModeIndex);

                    colorMode =
                        (ColorMode)nextColorModeIndex;

                    changed =
                        true;
                }
            }

            NadaVfxEditorControls.SpaceAfterSection();

            if (NadaVfxEditorControls.Section(
                    $"{prefix}:visual",
                    "VISUAL"))
            {
                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:luminance",
                        "Luminance",
                        flames.Luminance,
                        PluginConfig.MinLuminance,
                        PluginConfig.MaxLuminance,
                        out float luminance,
                        description:
                            "Changes the brightness of the flames, materials, and lights.",
                        resetValue:
                            WeaponVfxBlockDefaults.DefaultLuminance))
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
                                : hueDisabledReason,
                        description:
                            "Shifts the flame color while preserving the authored gradients.",
                        resetValue:
                            WeaponVfxBlockDefaults.DefaultHue))
                {
                    flames.Hue =
                        hue;

                    changed =
                        true;
                }
            }

            NadaVfxEditorControls.SpaceAfterSection();

            if (NadaVfxEditorControls.Section(
                    $"{prefix}:shape",
                    "SHAPE"))
            {
                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:scale",
                        "Scale",
                        flames.Scale,
                        PluginConfig.MinScaleMult,
                        PluginConfig.MaxScaleMult,
                        out float scale,
                        description:
                            "Changes the overall particle size of the flames.",
                        resetValue:
                            WeaponVfxBlockDefaults.DefaultScale))
                {
                    flames.Scale =
                        scale;

                    changed =
                        true;
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:length",
                        "Length",
                        flames.Length,
                        PluginConfig.MinFlameLength,
                        PluginConfig.MaxFlameLength,
                        out float length,
                        description:
                            "Changes the length of the flame emitter along the target.",
                        resetValue:
                            WeaponVfxBlockDefaults.DefaultFlameLength))
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
                        out float width,
                        description:
                            "Changes the width of the flame emitter around the target.",
                        resetValue:
                            WeaponVfxBlockDefaults.DefaultFlameWidth))
                {
                    flames.Width =
                        width;

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
                        out float energy,
                        description:
                            "Controls particle emission intensity.",
                        resetValue:
                            WeaponVfxBlockDefaults.DefaultEnergy))
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
                        out float lifetime,
                        description:
                            "Changes how long emitted flame particles remain alive.",
                        resetValue:
                            WeaponVfxBlockDefaults.DefaultLifetime))
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
                        out float simulationSpeed,
                        description:
                            "Changes how quickly the flame particle simulation runs.",
                        resetValue:
                            WeaponVfxBlockDefaults.DefaultSimulationSpeed))
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
                        out bool worldEnabled,
                        description:
                            "When enabled, emitted particles remain in world space as the target moves."))
                {
                    flames.WorldEnabled =
                        worldEnabled;

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