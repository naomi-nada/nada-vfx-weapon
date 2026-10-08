using NADA.VFX.Weapon.Core.Config;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Blocks.Effects;
using NADA.VFX.Weapon.Core.State.Defaults;

namespace NADA.VFX.Weapon.Editor.Inspectors
{
    internal static class NadaAuraInspector
    {
        private const float MinHue = -1f;
        private const float MaxHue = 1f;

        internal static bool Draw(
            VfxEffectBlock block)
        {
            if (block == null ||
                block.TypeId != VfxEffectTypeIds.Aura)
            {
                return false;
            }

            if (block.Settings is not AuraVfxSettings aura)
            {
                NadaVfxEditorControls.Placeholder(
                    "Invalid Aura block: expected AuraVfxSettings.");

                return false;
            }

            bool changed = false;
            string prefix = $"aura:{block.InstanceId}";

            if (NadaVfxEditorControls.Section(
                    $"{prefix}:visual",
                    "VISUAL"))
            {
                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:scale",
                        "Scale",
                        aura.Scale,
                        PluginConfig.MinAuraScale,
                        PluginConfig.MaxAuraScale,
                        out float scale,
                        description:
                            "Expands the Aura shells around the weapon mesh.",
                        resetValue:
                            WeaponVfxBlockDefaults.DefaultAuraScale))
                {
                    aura.Scale = scale;
                    changed = true;
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:luminance",
                        "Luminance",
                        aura.Luminance,
                        PluginConfig.MinLuminance,
                        PluginConfig.MaxLuminance,
                        out float luminance,
                        description:
                            "Changes the brightness of the Aura shell glow.",
                        resetValue:
                            WeaponVfxBlockDefaults.DefaultLuminance))
                {
                    aura.Luminance = luminance;
                    changed = true;
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:hue",
                        "Hue",
                        aura.Hue,
                        MinHue,
                        MaxHue,
                        out float hue,
                        description:
                            "Changes the Aura shell color.",
                        resetValue:
                            WeaponVfxBlockDefaults.DefaultHue))
                {
                    aura.Hue = hue;
                    changed = true;
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
