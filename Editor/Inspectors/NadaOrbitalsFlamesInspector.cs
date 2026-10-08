
using NADA.VFX.Weapon.Core.Config;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Blocks.Effects;
using NADA.VFX.Weapon.Core.State.Defaults;
using UnityEngine;

namespace NADA.VFX.Weapon.Editor.Inspectors
{
    internal static class NadaOrbitalsFlamesInspector
    {
        private const float MinHue = -1f;
        private const float MaxHue = 1f;

        internal static bool Draw(VfxEffectBlock block)
        {
            if (block == null ||
                block.TypeId != VfxEffectTypeIds.OrbitalsFlames)
            {
                return false;
            }

            if (block.Settings is not OrbitalsFlamesVfxSettings flames ||
                flames.Formation?.Path == null ||
                flames.Formation.GlueTargetInstanceIds == null)
            {
                NadaVfxEditorControls.Placeholder(
                    "Invalid Flames block: expected Flames settings with a formation path and Glue targets.");
                return false;
            }

            OrbitalsPathVfxSettings path = flames.Formation.Path;
            string prefix = $"orbitals-flames:{block.InstanceId}";
            bool isFollower = NadaOrbitalsGlueInspector.IsActiveFollower(block);
            bool changed = false;

            if (NadaVfxEditorControls.Section($"{prefix}:visual", "VISUAL"))
            {
                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:luminance", "Luminance", flames.Luminance,
                        PluginConfig.MinLuminance, PluginConfig.MaxLuminance,
                        out float luminance,
                        description: "Changes how brightly the orbital flames glow.",
                        resetValue: WeaponVfxBlockDefaults.DefaultLuminance))
                {
                    flames.Luminance = luminance;
                    changed = true;
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:hue", "Hue", flames.Hue,
                        MinHue, MaxHue, out float hue,
                        description: "Shifts the flame color while preserving its authored gradients.",
                        resetValue: WeaponVfxBlockDefaults.DefaultHue))
                {
                    flames.Hue = hue;
                    changed = true;
                }
            }

            NadaVfxEditorControls.SpaceAfterSection();

            if (NadaVfxEditorControls.Section($"{prefix}:shape", "SHAPE"))
            {
                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:scale", "Scale", flames.Scale,
                        PluginConfig.MinScaleMult, PluginConfig.MaxScaleMult,
                        out float scale,
                        description: "Changes the size of the orbiting flames.",
                        resetValue: WeaponVfxBlockDefaults.DefaultScale))
                {
                    flames.Scale = scale;
                    changed = true;
                }
            }

            NadaVfxEditorControls.SpaceAfterSection();

            if (NadaVfxEditorControls.Section($"{prefix}:particles", "PARTICLES"))
            {
                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:energy", "Energy", flames.Energy,
                        PluginConfig.MinEnergy, PluginConfig.MaxEnergy,
                        out float energy,
                        description: "Changes the intensity of the flame particles.",
                        resetValue: WeaponVfxBlockDefaults.DefaultEnergy))
                {
                    flames.Energy = energy;
                    changed = true;
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:lifetime", "Lifetime", flames.Lifetime,
                        PluginConfig.MinLifetime, PluginConfig.MaxLifetime,
                        out float lifetime,
                        description: "Changes how long emitted flames remain visible.",
                        resetValue: WeaponVfxBlockDefaults.DefaultLifetime))
                {
                    flames.Lifetime = lifetime;
                    changed = true;
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:simulation-speed", "Simulation Speed",
                        flames.SimulationSpeed,
                        PluginConfig.MinSimulationSpeed,
                        PluginConfig.MaxSimulationSpeed,
                        out float simulationSpeed,
                        description: "Changes how quickly the flame particles animate, not their orbital travel speed.",
                        resetValue: WeaponVfxBlockDefaults.DefaultSimulationSpeed))
                {
                    flames.SimulationSpeed = simulationSpeed;
                    changed = true;
                }
            }

            NadaVfxEditorControls.SpaceAfterSection();

            if (NadaVfxEditorControls.Section($"{prefix}:formation", "FORMATION"))
            {
                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:count", "Count", path.Count,
                        PluginConfig.MinCountNormalized,
                        PluginConfig.MaxCountNormalized,
                        out float count,
                        description: "Controls the number of additional flames beyond the leading flame.",
                        resetValue: WeaponVfxBlockDefaults.DefaultOrbitalsCount))
                {
                    path.Count = count;
                    changed = true;
                }

                if (NadaVfxEditorControls.Toggle(
                        $"{prefix}:snake", "Snake", path.SnakeEnabled,
                        out bool snakeEnabled,
                        enabled: !isFollower,
                        description: "Keeps the flames in a tighter snake formation."))
                {
                    path.SnakeEnabled = snakeEnabled;
                    changed = true;
                }

                changed |= NadaOrbitalsGlueInspector.Draw(block, flames.Formation);
            }

            NadaVfxEditorControls.SpaceAfterSection();

            if (NadaVfxEditorControls.Section($"{prefix}:motion", "MOTION"))
            {
                // A follower borrows its leader's path. Its own stored
                // motion values remain untouched until Glue is removed.
                bool previousEnabled = GUI.enabled;
                Color previousTint = GUI.color;

                if (isFollower)
                {
                    GUI.enabled = false;
                    GUI.color = new Color(previousTint.r, previousTint.g,
                        previousTint.b, previousTint.a * 0.78f);
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:speed", "Speed", path.Speed,
                        PluginConfig.MinOrbitalsSpeed, PluginConfig.MaxOrbitalsSpeed,
                        out float speed, decimals: 3,
                        enabled: !isFollower,
                        description: "Controls how quickly the flames travel around the weapon.",
                        resetValue: WeaponVfxBlockDefaults.DefaultOrbitalsSpeed))
                {
                    path.Speed = speed;
                    changed = true;
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:spacing", "Spacing", path.Spacing,
                        PluginConfig.MinOrbitalsSpacing,
                        PluginConfig.MaxOrbitalsSpacing,
                        out float spacing,
                        enabled: !isFollower && !path.SnakeEnabled,
                        disabledReason: "Spacing is locked while Snake or Glue is active.",
                        description: "Adjusts the separation between orbiting flames.",
                        resetValue: WeaponVfxBlockDefaults.DefaultOrbitalsSpacing))
                {
                    path.Spacing = spacing;
                    changed = true;
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:length", "Length", path.Length,
                        PluginConfig.MinOrbitalsLength, PluginConfig.MaxOrbitalsLength,
                        out float length, enabled: !isFollower,
                        description: "Changes how far the flames travel along the weapon.",
                        resetValue: WeaponVfxBlockDefaults.DefaultOrbitalsLength))
                {
                    path.Length = length;
                    changed = true;
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:radius", "Radius", path.Radius,
                        PluginConfig.MinOrbitalsRadiusMultiplier,
                        PluginConfig.MaxOrbitalsRadiusMultiplier,
                        out float radius, enabled: !isFollower,
                        description: "Changes how wide the orbit wraps around the weapon.",
                        resetValue: WeaponVfxBlockDefaults.DefaultOrbitalsRadius))
                {
                    path.Radius = radius;
                    changed = true;
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:cycles", "Cycles", path.Cycles,
                        PluginConfig.MinOrbitalsCycles, PluginConfig.MaxOrbitalsCycles,
                        out float cycles, enabled: !isFollower,
                        description: "Changes how many turns the flames make before reversing.",
                        resetValue: WeaponVfxBlockDefaults.DefaultOrbitalsCycles))
                {
                    path.Cycles = cycles;
                    changed = true;
                }

                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:drift", "Drift", path.Drift,
                        PluginConfig.MinDrift, PluginConfig.MaxDrift,
                        out float drift, enabled: !isFollower,
                        description: "Changes how freely the flames drift off their orbit path.",
                        resetValue: WeaponVfxBlockDefaults.DefaultOrbitalsDrift))
                {
                    path.Drift = drift;
                    changed = true;
                }

                GUI.enabled = previousEnabled;
                GUI.color = previousTint;
            }

            NadaVfxEditorControls.SpaceAfterSection();
            changed |= NadaVfxCommonBlockInspector.DrawTransform(
                block, editable: !isFollower);
            return changed;
        }
    }
}
