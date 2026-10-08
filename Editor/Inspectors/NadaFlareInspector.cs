using NADA.VFX.Weapon.Core.Config;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Blocks.Effects;
using NADA.VFX.Weapon.Core.State.Defaults;

namespace NADA.VFX.Weapon.Editor.Inspectors
{
    internal static class NadaFlareInspector
    {
        private const float MinHue = -1f;
        private const float MaxHue = 1f;

        internal static bool Draw(
            VfxEffectBlock block)
        {
            if (block == null ||
                block.TypeId != VfxEffectTypeIds.Flare)
            {
                return false;
            }

            if (block.Settings is not FlareVfxSettings flare)
            {
                NadaVfxEditorControls.Placeholder(
                    "Invalid Flare block: expected FlareVfxSettings.");

                NadaVfxCommonBlockInspector.DrawTransform(
                    block);

                return false;
            }

            bool changed =
                false;

            string prefix =
                $"flare:{block.InstanceId}";

            if (NadaVfxEditorControls.Section(
                    $"{prefix}:visual",
                    "VISUAL"))
            {
                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:luminance",
                        "Luminance",
                        flare.Luminance,
                        PluginConfig.MinLuminance,
                        PluginConfig.MaxLuminance,
                        out float luminance,
                        description:
                            "Changes the brightness of the flare particles, materials, and lights.",
                        resetValue:
                            WeaponVfxBlockDefaults.DefaultLuminance))
                {
                    flare.Luminance =
                        luminance;

                    changed =
                        true;
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:hue",
                        "Hue",
                        flare.Hue,
                        MinHue,
                        MaxHue,
                        out float hue,
                        description:
                            "Shifts the flare color while preserving its authored gradients.",
                        resetValue:
                            WeaponVfxBlockDefaults.DefaultHue))
                {
                    flare.Hue =
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
                        flare.Scale,
                        PluginConfig.MinScaleMult,
                        PluginConfig.MaxScaleMult,
                        out float scale,
                        description:
                            "Changes the overall size of the flare.",
                        resetValue:
                            WeaponVfxBlockDefaults.DefaultScale))
                {
                    flare.Scale =
                        scale;

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