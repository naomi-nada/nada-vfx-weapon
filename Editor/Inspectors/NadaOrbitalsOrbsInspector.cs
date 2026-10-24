
using NADA.VFX.Weapon.Core.Config;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Blocks.Effects;
using NADA.VFX.Weapon.Core.State.Defaults;
using UnityEngine;

namespace NADA.VFX.Weapon.Editor.Inspectors
{
    internal static class NadaOrbitalsOrbsInspector
    {
        private const float MinHue = -1f;
        private const float MaxHue = 1f;

        internal static bool Draw(VfxEffectBlock block)
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

            bool isFollower =
                NadaOrbitalsGlueInspector.IsActiveFollower(block);

            bool attached =
                orbs.Formation.MovementMode ==
                OrbitalsMovementMode.Attached;

            bool changed = false;

            if (NadaVfxEditorControls.Section(
                    $"{prefix}:movement-mode",
                    "MOVEMENT MODE"))
            {
                // Attached has no trajectory phase, so it cannot
                // participate in declared Glue relationships.
                // Path remains selectable in either state.
                bool canAttach =
                    attached ||
                    !NadaOrbitalsGlueInspector.HasDeclaredGlueParticipation(
                        block);

                if (NadaVfxEditorControls.MovementModeSelector(
                        $"{prefix}:movement",
                        attached,
                        out bool nextAttached,
                        allowAttached: canAttach))
                {
                    orbs.Formation.MovementMode =
                        nextAttached
                            ? OrbitalsMovementMode.Attached
                            : OrbitalsMovementMode.Path;

                    attached = nextAttached;
                    changed = true;
                }

                if (!canAttach)
                {
                    NadaVfxEditorControls.Placeholder(
                        "Remove this orbital's Glue relationships before enabling Attached.");
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
                    $"{prefix}:shape",
                    "SHAPE"))
            {
                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:scale",
                        "Scale",
                        orbs.Scale,
                        PluginConfig.MinOrbScaleMult,
                        PluginConfig.MaxOrbScaleMult,
                        out float scale,
                        description: "Changes the size of the orbs.",
                        resetValue: WeaponVfxBlockDefaults.DefaultScale))
                {
                    orbs.Scale = scale;
                    changed = true;
                }

                // Count belongs to this effect regardless of movement mode.
                // Path uses orbiting bodies; Attached extends a fixed line.
                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:count",
                        "Count",
                        path.Count,
                        PluginConfig.MinCountNormalized,
                        PluginConfig.MaxCountNormalized,
                        out float count,
                        description: attached
                            ? "Adds bodies toward the hilt without moving existing ones."
                            : "Controls the number of additional orbs beyond the leading orb.",
                        resetValue: WeaponVfxBlockDefaults.DefaultOrbitalsCount))
                {
                    path.Count = count;
                    changed = true;
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:spacing",
                        "Spacing",
                        path.Spacing,
                        PluginConfig.MinOrbitalsSpacing,
                        PluginConfig.MaxOrbitalsSpacing,
                        out float spacing,
                        enabled: !isFollower &&
                                 !attached &&
                                 !path.SnakeEnabled,
                        disabledReason: "Spacing is locked while Snake, Glue, or Attached is active.",
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
                        enabled: !isFollower,
                        description: attached
                            ? "Changes the distance between neighboring attached orbs."
                            : "Changes how far the orbit travels along the weapon.",
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
                        enabled: !isFollower && !attached,
                        description: "Changes how wide the orbit wraps around the weapon.",
                        resetValue: WeaponVfxBlockDefaults.DefaultOrbitalsRadius))
                {
                    path.Radius = radius;
                    changed = true;
                }
                if (NadaVfxEditorControls.Toggle(
                        $"{prefix}:snake",
                        "Snake",
                        path.SnakeEnabled,
                        out bool snakeEnabled,
                        enabled: !isFollower && !attached,
                        description: "Makes the orbs follow in a snake formation."))
                {
                    path.SnakeEnabled = snakeEnabled;
                    changed = true;
                }

                if (attached)
                {
                    // Attached cannot create Glue relationships. Show the
                    // current flag read-only instead of hiding the control.
                    NadaVfxEditorControls.Toggle(
                        $"glue:{block.InstanceId}:leader", "Glue",
                        orbs.Formation.GlueLeaderEnabled,
                        out bool _, enabled: false);
                }
                else
                {
                    changed |= NadaOrbitalsGlueInspector.Draw(
                        block, orbs.Formation);
                }

            }

            NadaVfxEditorControls.SpaceAfterSection();

            if (NadaVfxEditorControls.Section(
                    $"{prefix}:motion",
                    "MOTION"))
            {
                bool wasEnabled = GUI.enabled;
                Color previousTint = GUI.color;

                if (isFollower)
                {
                    GUI.enabled = false;

                    GUI.color = new Color(
                        previousTint.r,
                        previousTint.g,
                        previousTint.b,
                        previousTint.a * 0.78f);
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:speed",
                        "Speed",
                        path.Speed,
                        PluginConfig.MinOrbitalsSpeed,
                        PluginConfig.MaxOrbitalsSpeed,
                        out float speed,
                        decimals: 3,
                        enabled: !isFollower && !attached,
                        description: "Controls how quickly the orbs orbit.",
                        resetValue: WeaponVfxBlockDefaults.DefaultOrbitalsSpeed))
                {
                    path.Speed = speed;
                    changed = true;
                }


                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:cycles",
                        "Cycles",
                        path.Cycles,
                        PluginConfig.MinOrbitalsCycles,
                        PluginConfig.MaxOrbitalsCycles,
                        out float cycles,
                        enabled: !isFollower && !attached,
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
                        enabled: !isFollower && !attached,
                        description: "Changes how freely the orbs drift off their orbital path.",
                        resetValue: WeaponVfxBlockDefaults.DefaultOrbitalsDrift))
                {
                    path.Drift = drift;
                    changed = true;
                }

                GUI.enabled = wasEnabled;
                GUI.color = previousTint;
            }

            NadaVfxEditorControls.SpaceAfterSection();

            changed |= NadaVfxCommonBlockInspector.DrawTransform(
                block,
                editable: !isFollower);

            return changed;
        }
    }
}
