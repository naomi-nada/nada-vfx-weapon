using NADA.VFX.Weapon.Core.Config;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Blocks.Effects;
using UnityEngine;

namespace NADA.VFX.Weapon.Editor.Inspectors
{
    /// <summary>
    /// Strongly typed editor for one independent Sparks block.
    ///
    /// State is edited directly. Runtime presentation remains owned by the
    /// normal block reconciliation path.
    /// </summary>
    internal static class NadaSparksInspector
    {
        // Sparks energy is consumed by Mathf.Clamp01() in the runtime.
        private const float MinEnergy = 0f;
        private const float MaxEnergy = 1f;

        // Current hue state is a normalized signed hue control.
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
                        out float scale))
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
                        out float luminance))
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
                        out float hue))
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
                        out float energy))
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
                        out float lifetime))
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
                        out float simulationSpeed))
                {
                    sparks.SimulationSpeed =
                        simulationSpeed;

                    changed =
                        true;
                }

                NadaVfxEditorControls.Hint(
                    "Energy controls particle emission intensity.");
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
                        out float length))
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
                        out float width))
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