using NADA.VFX.Weapon.Core.Config;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Blocks.Effects;
using NADA.VFX.Weapon.Core.State.Defaults;

namespace NADA.VFX.Weapon.Editor.Inspectors
{
    internal static class NadaStrandsInspector
    {
        private const float MinEnergy = 0f;
        private const float MaxEnergy = 1f;

        private const float MinHue = -1f;
        private const float MaxHue = 1f;

        private const float MinDrift = 0f;
        private const float MaxDrift = 1f;

        internal static bool Draw(
            VfxEffectBlock block)
        {
            if (block == null ||
                block.TypeId != VfxEffectTypeIds.Strands)
            {
                return false;
            }

            if (block.Settings is not StrandsVfxSettings strands)
            {
                NadaVfxEditorControls.Placeholder(
                    "Invalid Strands block: expected StrandsVfxSettings.");

                NadaVfxCommonBlockInspector.DrawTransform(
                    block);

                return false;
            }

            bool changed =
                false;

            string prefix =
                $"strands:{block.InstanceId}";

            if (NadaVfxEditorControls.Section(
                    $"{prefix}:visual",
                    "VISUAL"))
            {
                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:spectrum-speed",
                        "Spectrum Speed",
                        strands.SpectrumSpeed,
                        PluginConfig.MinSpectrumSpeed,
                        PluginConfig.MaxSpectrumSpeed,
                        out float spectrumSpeed,
                        decimals: 3,
                        enabled:
                            strands.SpectrumEnabled,
                        disabledReason:
                            strands.SpectrumEnabled
                                ? null
                                : "Spectrum Speed is stored but inactive while Spectrum is off.",
                        description:
                            "Controls how quickly Spectrum cycles through its colors.",
                        resetValue:
                            WeaponVfxBlockDefaults.DefaultSpectrumSpeed))
                {
                    strands.SpectrumSpeed =
                        spectrumSpeed;

                    changed =
                        true;
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:luminance",
                        "Luminance",
                        strands.Luminance,
                        PluginConfig.MinLuminance,
                        PluginConfig.MaxLuminance,
                        out float luminance,
                        description:
                            "Changes the brightness of the strands.",
                        resetValue:
                            WeaponVfxBlockDefaults.DefaultLuminance))
                {
                    strands.Luminance =
                        luminance;

                    changed =
                        true;
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:hue",
                        "Hue",
                        strands.Hue,
                        MinHue,
                        MaxHue,
                        out float hue,
                        enabled:
                            !strands.SpectrumEnabled,
                        disabledReason:
                            strands.SpectrumEnabled
                                ? "Hue is stored but inactive while Spectrum is running."
                                : null,
                        description:
                            "Shifts the strands' static color while preserving authored gradients.",
                        resetValue:
                            WeaponVfxBlockDefaults.DefaultHue))
                {
                    strands.Hue =
                        hue;

                    changed =
                        true;
                }
                if (NadaVfxEditorControls.Toggle(
                        $"{prefix}:spectrum",
                        "Spectrum",
                        strands.SpectrumEnabled,
                        out bool spectrumEnabled,
                        description:
                            "Animates the effect continuously through the color spectrum."))
                {
                    strands.SpectrumEnabled =
                        spectrumEnabled;

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
                        $"{prefix}:scale-whole",
                        "Scale Whole",
                        strands.ScaleWhole,
                        PluginConfig.MinScaleMult,
                        PluginConfig.MaxScaleMult,
                        out float scaleWhole,
                        description:
                            "Scales the entire Strands effect as one object.",
                        resetValue:
                            WeaponVfxBlockDefaults.DefaultScale))
                {
                    strands.ScaleWhole =
                        scaleWhole;

                    changed =
                        true;
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:scale-parts",
                        "Scale Parts",
                        strands.ScaleParts,
                        PluginConfig.MinScaleMult,
                        PluginConfig.MaxScaleMult,
                        out float scaleParts,
                        description:
                            "Scales the individual strand particles without resizing the whole effect root.",
                        resetValue:
                            WeaponVfxBlockDefaults.DefaultScale))
                {
                    strands.ScaleParts =
                        scaleParts;

                    changed =
                        true;
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:length",
                        "Length",
                        strands.Length,
                        PluginConfig.MinOrbitalsLength,
                        PluginConfig.MaxOrbitalsLength,
                        out float length,
                        description:
                            "Changes the length of the strands emission shape.",
                        resetValue:
                            WeaponVfxBlockDefaults.DefaultOrbitalsLength))
                {
                    strands.Length =
                        length;

                    changed =
                        true;
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:radius",
                        "Radius",
                        strands.Radius,
                        PluginConfig.MinOrbitalsRadiusMultiplier,
                        PluginConfig.MaxOrbitalsRadiusMultiplier,
                        out float radius,
                        description:
                            "Changes the radius of the strands emission shape.",
                        resetValue:
                            WeaponVfxBlockDefaults.DefaultOrbitalsRadius))
                {
                    strands.Radius =
                        radius;

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
                        strands.Energy,
                        MinEnergy,
                        MaxEnergy,
                        out float energy,
                        description:
                            "Controls particle emission intensity.",
                        resetValue:
                            WeaponVfxBlockDefaults.DefaultEnergy))
                {
                    strands.Energy =
                        energy;

                    changed =
                        true;
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:lifetime",
                        "Lifetime",
                        strands.Lifetime,
                        PluginConfig.MinLifetime,
                        PluginConfig.MaxLifetime,
                        out float lifetime,
                        description:
                            "Changes how long emitted strand particles remain alive.",
                        resetValue:
                            WeaponVfxBlockDefaults.DefaultLifetime))
                {
                    strands.Lifetime =
                        lifetime;

                    changed =
                        true;
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:speed",
                        "Speed",
                        strands.Speed,
                        PluginConfig.MinOrbitalsSpeed,
                        PluginConfig.MaxOrbitalsSpeed,
                        out float speed,
                        decimals: 3,
                        description:
                            "Controls the particle simulation speed.",
                        resetValue:
                            WeaponVfxBlockDefaults.DefaultOrbitalsSpeed))
                {
                    strands.Speed =
                        speed;

                    changed =
                        true;
                }
            }

            NadaVfxEditorControls.SpaceAfterSection();

            if (NadaVfxEditorControls.Section(
                    $"{prefix}:motion",
                    "MOTION"))
            {
                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:drift",
                        "Drift",
                        strands.Drift,
                        MinDrift,
                        MaxDrift,
                        out float drift,
                        description:
                            "Controls how loosely the effect follows its target in world space.",
                        resetValue:
                            WeaponVfxBlockDefaults.DefaultOrbitalsDrift))
                {
                    strands.Drift =
                        drift;

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