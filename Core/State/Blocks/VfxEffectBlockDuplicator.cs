using System.Collections.Generic;
using NADA.VFX.Weapon.Core.State.Blocks.Effects;

namespace NADA.VFX.Weapon.Core.State.Blocks
{
    /// <summary>
    /// Creates an independent copy of one effect block for duplication.
    ///
    /// InstanceId is always replaced.
    /// Glue relationships are deliberately not duplicated because a copied
    /// leader must not immediately compete with its source for the same
    /// followers.
    /// </summary>
    internal static class VfxEffectBlockDuplicator
    {
        internal static bool TryDuplicate(
            VfxEffectBlock source,
            uint newInstanceId,
            string newDisplayName,
            out VfxEffectBlock duplicate)
        {
            duplicate =
                null;

            if (source == null ||
                newInstanceId == 0 ||
                string.IsNullOrWhiteSpace(source.TypeId) ||
                source.Settings == null)
            {
                return false;
            }

            VfxEffectSettings settings =
                CloneSettings(
                    source.Settings);

            if (settings == null)
                return false;

            duplicate =
                new VfxEffectBlock
                {
                    InstanceId =
                        newInstanceId,

                    TypeId =
                        source.TypeId,

                    DisplayName =
                        newDisplayName ?? string.Empty,

                    Enabled =
                        source.Enabled,

                    Transform =
                        CloneTransform(
                            source.Transform),

                    Settings =
                        settings
                };

            return true;
        }

        private static VfxTransformState CloneTransform(
            VfxTransformState source)
        {
            if (source == null)
            {
                return new VfxTransformState();
            }

            return new VfxTransformState
            {
                XOffset =
                    source.XOffset,

                YOffset =
                    source.YOffset,

                ZOffset =
                    source.ZOffset,

                XRotation =
                    source.XRotation,

                YRotation =
                    source.YRotation,

                ZRotation =
                    source.ZRotation
            };
        }

        private static VfxEffectSettings CloneSettings(
            VfxEffectSettings source)
        {
            switch (source)
            {
                case InnerFlamesVfxSettings inner:
                    return new InnerFlamesVfxSettings
                    {
                        WorldEnabled =
                            inner.WorldEnabled,

                        BlackEnabled =
                            inner.BlackEnabled,

                        WhiteEnabled =
                            inner.WhiteEnabled,

                        Energy =
                            inner.Energy,

                        Scale =
                            inner.Scale,

                        Luminance =
                            inner.Luminance,

                        Hue =
                            inner.Hue,

                        Lifetime =
                            inner.Lifetime,

                        SimulationSpeed =
                            inner.SimulationSpeed,

                        Length =
                            inner.Length,

                        Width =
                            inner.Width
                    };

                case OuterFlamesVfxSettings outer:
                    return new OuterFlamesVfxSettings
                    {
                        WorldEnabled =
                            outer.WorldEnabled,

                        BlackEnabled =
                            outer.BlackEnabled,

                        WhiteEnabled =
                            outer.WhiteEnabled,

                        DragEnabled =
                            outer.DragEnabled,

                        Energy =
                            outer.Energy,

                        Scale =
                            outer.Scale,

                        Luminance =
                            outer.Luminance,

                        Hue =
                            outer.Hue,

                        Lifetime =
                            outer.Lifetime,

                        SimulationSpeed =
                            outer.SimulationSpeed,

                        Length =
                            outer.Length,

                        Width =
                            outer.Width
                    };

                case StrandsVfxSettings strands:
                    return new StrandsVfxSettings
                    {
                        SpectrumEnabled =
                            strands.SpectrumEnabled,

                        Energy =
                            strands.Energy,

                        ScaleWhole =
                            strands.ScaleWhole,

                        ScaleParts =
                            strands.ScaleParts,

                        Luminance =
                            strands.Luminance,

                        Hue =
                            strands.Hue,

                        Lifetime =
                            strands.Lifetime,

                        Length =
                            strands.Length,

                        SpectrumSpeed =
                            strands.SpectrumSpeed,

                        Speed =
                            strands.Speed,

                        Radius =
                            strands.Radius,

                        Drift =
                            strands.Drift
                    };

                case SparksVfxSettings sparks:
                    return new SparksVfxSettings
                    {
                        Energy =
                            sparks.Energy,

                        Scale =
                            sparks.Scale,

                        Luminance =
                            sparks.Luminance,

                        Hue =
                            sparks.Hue,

                        Lifetime =
                            sparks.Lifetime,

                        SimulationSpeed =
                            sparks.SimulationSpeed,

                        Length =
                            sparks.Length,

                        Width =
                            sparks.Width
                    };

                case FlareVfxSettings flare:
                    return new FlareVfxSettings
                    {
                        Scale =
                            flare.Scale,

                        Luminance =
                            flare.Luminance,

                        Hue =
                            flare.Hue
                    };

                case AuraVfxSettings aura:
                    return new AuraVfxSettings
                    {
                        Scale =
                            aura.Scale,

                        Luminance =
                            aura.Luminance,

                        Hue =
                            aura.Hue
                    };

                case OrbitalsOrbsVfxSettings orbs:
                {
                    OrbitalsFormationVfxSettings formation =
                        CloneFormationForDuplicate(
                            orbs.Formation);

                    if (formation == null)
                        return null;

                    return new OrbitalsOrbsVfxSettings
                    {
                        Scale =
                            orbs.Scale,

                        Luminance =
                            orbs.Luminance,

                        Hue =
                            orbs.Hue,

                        Formation =
                            formation
                    };
                }

                case OrbitalsCoresVfxSettings cores:
                {
                    OrbitalsFormationVfxSettings formation =
                        CloneFormationForDuplicate(
                            cores.Formation);

                    if (formation == null)
                        return null;

                    return new OrbitalsCoresVfxSettings
                    {
                        Scale =
                            cores.Scale,

                        Luminance =
                            cores.Luminance,

                        Hue =
                            cores.Hue,

                        SpinEnabled =
                            cores.SpinEnabled,

                        SpinSpeed =
                            cores.SpinSpeed,

                        Formation =
                            formation
                    };
                }

                case OrbitalsFlamesVfxSettings flames:
                {
                    OrbitalsFormationVfxSettings formation =
                        CloneFormationForDuplicate(
                            flames.Formation);

                    if (formation == null)
                        return null;

                    return new OrbitalsFlamesVfxSettings
                    {
                        Energy =
                            flames.Energy,

                        Scale =
                            flames.Scale,

                        Luminance =
                            flames.Luminance,

                        Hue =
                            flames.Hue,

                        Lifetime =
                            flames.Lifetime,

                        SimulationSpeed =
                            flames.SimulationSpeed,

                        Formation =
                            formation
                    };
                }

                case OrbitalsEmbersVfxSettings embers:
                {
                    OrbitalsFormationVfxSettings formation =
                        CloneFormationForDuplicate(
                            embers.Formation);

                    if (formation == null)
                        return null;

                    return new OrbitalsEmbersVfxSettings
                    {
                        Energy =
                            embers.Energy,

                        Scale =
                            embers.Scale,

                        Luminance =
                            embers.Luminance,

                        Hue =
                            embers.Hue,

                        Lifetime =
                            embers.Lifetime,

                        SimulationSpeed =
                            embers.SimulationSpeed,

                        Formation =
                            formation
                    };
                }

                default:
                    return null;
            }
        }

        private static OrbitalsFormationVfxSettings
            CloneFormationForDuplicate(
                OrbitalsFormationVfxSettings source)
        {
            if (source?.Path == null)
                return null;

            return new OrbitalsFormationVfxSettings
            {
                Path =
                    new OrbitalsPathVfxSettings
                    {
                        SnakeEnabled =
                            source.Path.SnakeEnabled,

                        Count =
                            source.Path.Count,

                        Speed =
                            source.Path.Speed,

                        Spacing =
                            source.Path.Spacing,

                        Length =
                            source.Path.Length,

                        Radius =
                            source.Path.Radius,

                        Cycles =
                            source.Path.Cycles,

                        Drift =
                            source.Path.Drift
                    },

                // A duplicated orbital starts independently. Copying these
                // relationships would allow two logical leaders to claim the
                // same followers and make the resolved state ambiguous.
                GlueLeaderEnabled =
                    false,

                GlueTargetInstanceIds =
                    new List<uint>()
            };
        }
    }
}