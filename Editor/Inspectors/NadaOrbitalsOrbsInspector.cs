
using NADA.VFX.Weapon.Core.Config;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Blocks.Effects;
using NADA.VFX.Weapon.Core.State.Defaults;

namespace NADA.VFX.Weapon.Editor.Inspectors
{
    internal static class NadaOrbitalsOrbsInspector
    {
        private const float MinHue = -1f;
        private const float MaxHue = 1f;

        internal static bool Draw(
            VfxEffectBlock block)
        {
            if (block == null ||
                block.TypeId != VfxEffectTypeIds.OrbitalsOrbs)
            {
                return false;
            }

            if (block.Settings is not OrbitalsOrbsVfxSettings orbs ||
                orbs.Formation?.Path == null)
            {
                NadaVfxEditorControls.Placeholder(
                    "Invalid Orbs block: expected Orbs settings with a formation path.");

                return false;
            }

            OrbitalsPathVfxSettings path =
                orbs.Formation.Path;

            string prefix =
                $"orbitals-orbs:{block.InstanceId}";

            bool changed = false;

            if (NadaVfxEditorControls.Section(
                    $"{prefix}:visual",
                    "VISUAL"))
            {
                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:scale",
                        "Scale",
                        orbs.Scale,
                        PluginConfig.MinOrbScaleMult,
                        PluginConfig.MaxOrbScaleMult,
                        out float scale,
                        description: "Changes the size of the orbiting orbs.",
                        resetValue: WeaponVfxBlockDefaults.DefaultScale))
                {
                    orbs.Scale = scale;
                    changed = true;
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:luminance",
                        "Luminance",
                        orbs.Luminance,
                        PluginConfig.MinLuminance,
                        PluginConfig.MaxLuminance,
                        out float luminance,
                        description: "Changes how brightly the orbs glow.",
                        resetValue: WeaponVfxBlockDefaults.DefaultLuminance))
                {
                    orbs.Luminance = luminance;
                    changed = true;
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:hue",
                        "Hue",
                        orbs.Hue,
                        MinHue,
                        MaxHue,
                        out float hue,
                        description: "Shifts the orbs' color.",
                        resetValue: WeaponVfxBlockDefaults.DefaultHue))
                {
                    orbs.Hue = hue;
                    changed = true;
                }
            }

            NadaVfxEditorControls.SpaceAfterSection();

            if (NadaVfxEditorControls.Section(
                    $"{prefix}:formation",
                    "FORMATION"))
            {
                if (NadaVfxEditorControls.Toggle(
                        $"{prefix}:snake",
                        "Snake",
                        path.SnakeEnabled,
                        out bool snakeEnabled,
                        description: "Makes the orbs follow in a snake formation."))
                {
                    path.SnakeEnabled = snakeEnabled;
                    changed = true;
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:count",
                        "Count",
                        path.Count,
                        PluginConfig.MinCountNormalized,
                        PluginConfig.MaxCountNormalized,
                        out float count,
                        description: "Controls the number of additional orbs beyond the leading orb.",
                        resetValue: WeaponVfxBlockDefaults.DefaultOrbitalsCount))
                {
                    path.Count = count;
                    changed = true;
                }

                changed |= NadaOrbitalsGlueInspector.Draw(
                    block,
                    orbs.Formation);
            }

            NadaVfxEditorControls.SpaceAfterSection();

            if (NadaVfxEditorControls.Section(
                    $"{prefix}:motion",
                    "MOTION"))
            {
                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:speed",
                        "Speed",
                        path.Speed,
                        PluginConfig.MinOrbitalsSpeed,
                        PluginConfig.MaxOrbitalsSpeed,
                        out float speed,
                        decimals: 3,
                        description: "Controls how quickly the orbs orbit.",
                        resetValue: WeaponVfxBlockDefaults.DefaultOrbitalsSpeed))
                {
                    path.Speed = speed;
                    changed = true;
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:spacing",
                        "Spacing",
                        path.Spacing,
                        PluginConfig.MinOrbitalsSpacing,
                        PluginConfig.MaxOrbitalsSpacing,
                        out float spacing,
                        enabled: !path.SnakeEnabled,
                        disabledReason: "Spacing is locked while Snake is enabled.",
                        description: "Adjusts the separation between orbiting orbs.",
                        resetValue: WeaponVfxBlockDefaults.DefaultOrbitalsSpacing))
                {
                    path.Spacing = spacing;
                    changed = true;
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:length",
                        "Length",
                        path.Length,
                        PluginConfig.MinOrbitalsLength,
                        PluginConfig.MaxOrbitalsLength,
                        out float length,
                        description: "Changes how far the orbit travels along the weapon.",
                        resetValue: WeaponVfxBlockDefaults.DefaultOrbitalsLength))
                {
                    path.Length = length;
                    changed = true;
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:radius",
                        "Radius",
                        path.Radius,
                        PluginConfig.MinOrbitalsRadiusMultiplier,
                        PluginConfig.MaxOrbitalsRadiusMultiplier,
                        out float radius,
                        description: "Changes how wide the orbit wraps around the weapon.",
                        resetValue: WeaponVfxBlockDefaults.DefaultOrbitalsRadius))
                {
                    path.Radius = radius;
                    changed = true;
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:cycles",
                        "Cycles",
                        path.Cycles,
                        PluginConfig.MinOrbitalsCycles,
                        PluginConfig.MaxOrbitalsCycles,
                        out float cycles,
                        description: "Changes how many turns happen before reversing.",
                        resetValue: WeaponVfxBlockDefaults.DefaultOrbitalsCycles))
                {
                    path.Cycles = cycles;
                    changed = true;
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:drift",
                        "Drift",
                        path.Drift,
                        PluginConfig.MinDrift,
                        PluginConfig.MaxDrift,
                        out float drift,
                        description: "Changes how freely orbs drift off their orbit path.",
                        resetValue: WeaponVfxBlockDefaults.DefaultOrbitalsDrift))
                {
                    path.Drift = drift;
                    changed = true;
                }
            }

            NadaVfxEditorControls.SpaceAfterSection();

            changed |= NadaVfxCommonBlockInspector.DrawTransform(block);

            return changed;
        }
    }
}
