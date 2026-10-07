using NADA.VFX.Weapon.Core.Config;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Blocks.Effects;
using NADA.VFX.Weapon.Core.State.Defaults;

namespace NADA.VFX.Weapon.Editor.Inspectors
{
    internal static class NadaSparksInspector
    {
        private const float MinEnergy = 0f;
        private const float MaxEnergy = 1f;

        private const float MinHue = -1f;
        private const float MaxHue = 1f;

        internal static bool Draw(
            VfxEffectBlock block)
        {
            if (block == null ||
                block.TypeId != VfxEffectTypeIds.Sparks)
            {
                return false;
            }

            if (block.Settings is not SparksVfxSettings sparks)
            {
                NadaVfxEditorControls.Placeholder(
                    "Invalid Sparks block: expected SparksVfxSettings.");

                NadaVfxCommonBlockInspector.DrawTransform(
                    block);

                return false;
            }

            bool changed =
                false;

            string prefix =
                $"sparks:{block.InstanceId}";

            if (NadaVfxEditorControls.Section(
                    $"{prefix}:visual",
                    "VISUAL"))
            {
                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:scale",
                        "Scale",
                        sparks.Scale,
                        PluginConfig.MinScaleMult,
                        PluginConfig.MaxScaleMult,
                        out float scale,
                        description:
                            "Changes the overall size of the sparks.",
                        resetValue:
                            WeaponVfxBlockDefaults.DefaultScale))
                {
                    sparks.Scale =
                        scale;

                    changed =
                        true;
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:luminance",
                        "Luminance",
                        sparks.Luminance,
                        PluginConfig.MinLuminance,
                        PluginConfig.MaxLuminance,
                        out float luminance,
                        description:
                            "Changes the brightness of the effect.",
                        resetValue:
                            WeaponVfxBlockDefaults.DefaultLuminance))
                {
                    sparks.Luminance =
                        luminance;

                    changed =
                        true;
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:hue",
                        "Hue",
                        sparks.Hue,
                        MinHue,
                        MaxHue,
                        out float hue,
                        description:
                            "Shifts the effect's color while preserving its authored gradients.",
                        resetValue:
                            WeaponVfxBlockDefaults.DefaultHue))
                {
                    sparks.Hue =
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
                        sparks.Energy,
                        MinEnergy,
                        MaxEnergy,
                        out float energy,
                        description:
                            "Controls particle emission intensity.",
                        resetValue:
                            WeaponVfxBlockDefaults.DefaultEnergy))
                {
                    sparks.Energy =
                        energy;

                    changed =
                        true;
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:lifetime",
                        "Lifetime",
                        sparks.Lifetime,
                        PluginConfig.MinLifetime,
                        PluginConfig.MaxLifetime,
                        out float lifetime,
                        description:
                            "Changes how long emitted particles remain alive.",
                        resetValue:
                            WeaponVfxBlockDefaults.DefaultLifetime))
                {
                    sparks.Lifetime =
                        lifetime;

                    changed =
                        true;
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:simulation-speed",
                        "Simulation Speed",
                        sparks.SimulationSpeed,
                        PluginConfig.MinSimulationSpeed,
                        PluginConfig.MaxSimulationSpeed,
                        out float simulationSpeed,
                        description:
                            "Changes how quickly the particle simulation runs.",
                        resetValue:
                            WeaponVfxBlockDefaults.DefaultSimulationSpeed))
                {
                    sparks.SimulationSpeed =
                        simulationSpeed;

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
                        $"{prefix}:length",
                        "Length",
                        sparks.Length,
                        PluginConfig.MinFlameLength,
                        PluginConfig.MaxFlameLength,
                        out float length,
                        description:
                            "Changes the length of the sparks field along the target.",
                        resetValue:
                            WeaponVfxBlockDefaults.DefaultFlameLength))
                {
                    sparks.Length =
                        length;

                    changed =
                        true;
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:width",
                        "Width",
                        sparks.Width,
                        PluginConfig.MinSparksWidth,
                        PluginConfig.MaxSparksWidth,
                        out float width,
                        description:
                            "Changes the width of the sparks field around the target.",
                        resetValue:
                            WeaponVfxBlockDefaults.DefaultSparksWidth))
                {
                    sparks.Width =
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
    }
}