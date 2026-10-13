
using NADA.VFX.Weapon.Core.Config;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Blocks.Effects;
using NADA.VFX.Weapon.Core.State.Defaults;
using UnityEngine;

namespace NADA.VFX.Weapon.Editor.Inspectors
{
    internal static class NadaOrbitalsCoresInspector
    {
        private const float MinHue = -1f;
        private const float MaxHue = 1f;

        internal static bool Draw(VfxEffectBlock block)
        {
            if (block == null ||
                block.TypeId != VfxEffectTypeIds.OrbitalsCores)
            {
                return false;
            }

            if (block.Settings is not OrbitalsCoresVfxSettings cores ||
                cores.Formation?.Path == null)
            {
                NadaVfxEditorControls.Placeholder(
                    "Invalid Cores block: expected Cores settings with a formation path.");

                return false;
            }

            OrbitalsPathVfxSettings path =
                cores.Formation.Path;

            string prefix =
                $"orbitals-cores:{block.InstanceId}";

            bool isFollower =
                NadaOrbitalsGlueInspector.IsActiveFollower(block);

            bool attached =
                cores.Formation.MovementMode ==
                OrbitalsMovementMode.Attached;

            bool changed = false;

            if (NadaVfxEditorControls.Section(
                    $"{prefix}:movement-mode",
                    "MOVEMENT MODE"))
            {
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
                    cores.Formation.MovementMode =
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
                        cores.Luminance,
                        PluginConfig.MinLuminance,
                        PluginConfig.MaxLuminance,
                        out float luminance,
                        description: "Changes how brightly the cores glow.",
                        resetValue: WeaponVfxBlockDefaults.DefaultLuminance))
                {
                    cores.Luminance = luminance;
                    changed = true;
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:hue",
                        "Hue",
                        cores.Hue,
                        MinHue,
                        MaxHue,
                        out float hue,
                        description: "Shifts the cores' color.",
                        resetValue: WeaponVfxBlockDefaults.DefaultHue))
                {
                    cores.Hue = hue;
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
                        cores.Scale,
                        PluginConfig.MinCoreScaleMult,
                        PluginConfig.MaxCoreScaleMult,
                        out float scale,
                        description: "Changes the size of the cores.",
                        resetValue: WeaponVfxBlockDefaults.DefaultScale))
                {
                    cores.Scale = scale;
                    changed = true;
                }
            }

            NadaVfxEditorControls.SpaceAfterSection();

            if (NadaVfxEditorControls.Section(
                    $"{prefix}:formation",
                    "FORMATION"))
            {
                // Count belongs to the effect regardless of movement mode.
                // Attached adds bodies toward the hilt at fixed intervals.
                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:count",
                        "Count",
                        path.Count,
                        PluginConfig.MinCountNormalized,
                        PluginConfig.MaxCountNormalized,
                        out float count,
                        description: attached
                            ? "Adds cores toward the hilt without moving existing ones."
                            : "Controls the number of additional cores beyond the leading core.",
                        resetValue: WeaponVfxBlockDefaults.DefaultOrbitalsCount))
                {
                    path.Count = count;
                    changed = true;
                }

                if (NadaVfxEditorControls.Toggle(
                        $"{prefix}:snake",
                        "Snake",
                        path.SnakeEnabled,
                        out bool snakeEnabled,
                        enabled: !isFollower && !attached,
                        description: "Makes the cores follow in a snake formation."))
                {
                    path.SnakeEnabled = snakeEnabled;
                    changed = true;
                }

                if (!attached)
                {
                    changed |= NadaOrbitalsGlueInspector.Draw(
                        block,
                        cores.Formation);
                }
                else
                {
                    NadaVfxEditorControls.Placeholder(
                        "Attached cores form a straight line. Snake and Glue are unavailable.");
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
                        description: "Controls how quickly the cores travel along their orbit.",
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
                        enabled: !isFollower &&
                                 !attached &&
                                 !path.SnakeEnabled,
                        disabledReason: "Spacing is locked while Snake, Glue, or Attached is active.",
                        description: "Adjusts the separation between orbiting cores.",
                        resetValue: WeaponVfxBlockDefaults.DefaultOrbitalsSpacing))
                {
                    path.Spacing = spacing;
                    changed = true;
                }

                // Path: length of the orbital travel path.
                // Attached: spacing between neighboring cores.
                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:length",
                        "Length",
                        path.Length,
                        PluginConfig.MinOrbitalsLength,
                        PluginConfig.MaxOrbitalsLength,
                        out float length,
                        enabled: !isFollower,
                        description: attached
                            ? "Changes the distance between neighboring attached cores."
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

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:cycles",
                        "Cycles",
                        path.Cycles,
                        PluginConfig.MinOrbitalsCycles,
                        PluginConfig.MaxOrbitalsCycles,
                        out float cycles,
                        enabled: !isFollower && !attached,
                        description: "Changes how many turns the cores make before reversing.",
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
                        description: "Changes how freely the cores drift off their orbital path.",
                        resetValue: WeaponVfxBlockDefaults.DefaultOrbitalsDrift))
                {
                    path.Drift = drift;
                    changed = true;
                }

                GUI.enabled = wasEnabled;
                GUI.color = previousTint;

                // Spin belongs to this Core block, not the movement
                // trajectory. It stays editable in both modes,
                // including while following a Glue leader.
                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:spin-speed",
                        "Spin Speed",
                        cores.SpinSpeed,
                        PluginConfig.MinCoreSpinSpeed,
                        PluginConfig.MaxCoreSpinSpeed,
                        out float spinSpeed,
                        enabled: cores.SpinEnabled,
                        disabledReason: "Spin Speed is inactive while Spin is off.",
                        description: "Changes how quickly the cores rotate.",
                        resetValue: WeaponVfxBlockDefaults.DefaultCoreSpinSpeed))
                {
                    cores.SpinSpeed = spinSpeed;
                    changed = true;
                }

                if (NadaVfxEditorControls.Toggle(
                        $"{prefix}:spin",
                        "Spin",
                        cores.SpinEnabled,
                        out bool spinEnabled,
                        description: "Rotates each core independently of its movement mode."))
                {
                    cores.SpinEnabled = spinEnabled;
                    changed = true;
                }
            }

            NadaVfxEditorControls.SpaceAfterSection();

            changed |= NadaVfxCommonBlockInspector.DrawTransform(
                block,
                editable: !isFollower);

            return changed;
        }
    }
}
