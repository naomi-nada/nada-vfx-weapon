using NADA.VFX.Weapon.Core.State;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Blocks.Effects;

namespace NADA.VFX.Weapon.Core.State.Migration
{
    /// <summary>
    /// Temporary bridge from the old flat VfxState schema into the new
    /// block-based state model.
    ///
    /// This adapter disappears once legacy state migration is no longer needed.
    /// </summary>
    internal static class LegacyVfxStateAdapter
    {
        // These IDs reproduce the old effect ordering during migration.
        // They are compatibility seeds only, not permanent runtime slots and
        // not limits on how many instances or effect types may exist.
        private const uint LegacyInnerFlamesInstanceId = 1;
        private const uint LegacyOuterFlamesInstanceId = 2;
        private const uint LegacyStrandsInstanceId = 3;
        private const uint LegacySparksInstanceId = 4;
        private const uint LegacyFlareInstanceId = 5;
        private const uint LegacyAuraInstanceId = 6;
        private const uint LegacyOrbitalsOrbsInstanceId = 7;

        internal static WeaponVfxState CreateInnerFlamesPrototype(
            VfxState legacyState)
        {
            var state =
                CreateStateWithLegacyRigTransform(
                    legacyState);

            var innerFlamesBlock =
                new VfxEffectBlock
                {
                    InstanceId = LegacyInnerFlamesInstanceId,
                    TypeId = VfxEffectTypeIds.InnerFlames,
                    Enabled = legacyState.InnerFlamesEnabled,

                    Transform =
                        new VfxTransformState
                        {
                            XOffset = legacyState.InnerFlamesXOffset,
                            YOffset = legacyState.InnerFlamesYOffset,
                            ZOffset = legacyState.InnerFlamesZOffset,

                            XRotation = legacyState.InnerFlamesXRotation,
                            YRotation = legacyState.InnerFlamesYRotation,
                            ZRotation = legacyState.InnerFlamesZRotation
                        },

                    Settings =
                        new InnerFlamesVfxSettings
                        {
                            WorldEnabled =
                                legacyState.InnerFlamesWorldEnabled,

                            BlackEnabled =
                                legacyState.InnerFlamesBlackEnabled,

                            WhiteEnabled =
                                legacyState.InnerFlamesWhiteEnabled,

                            Energy =
                                legacyState.InnerFlamesEnergy,

                            Scale =
                                legacyState.InnerFlamesScale,

                            Luminance =
                                legacyState.InnerFlamesLuminance,

                            Hue =
                                legacyState.InnerFlamesHue,

                            Lifetime =
                                legacyState.InnerFlamesLifetime,

                            SimulationSpeed =
                                legacyState.InnerFlamesSimulationSpeed,

                            Length =
                                legacyState.InnerFlamesLength,

                            Width =
                                legacyState.InnerFlamesWidth
                        }
                };

            state.Effects.Add(
                innerFlamesBlock);

            return state;
        }

        internal static WeaponVfxState CreateOuterFlamesPrototype(
            VfxState legacyState)
        {
            var state =
                CreateStateWithLegacyRigTransform(
                    legacyState);

            var outerFlamesBlock =
                new VfxEffectBlock
                {
                    InstanceId = LegacyOuterFlamesInstanceId,
                    TypeId = VfxEffectTypeIds.OuterFlames,
                    Enabled = legacyState.OuterFlamesEnabled,

                    Transform =
                        new VfxTransformState
                        {
                            XOffset = legacyState.OuterFlamesXOffset,
                            YOffset = legacyState.OuterFlamesYOffset,
                            ZOffset = legacyState.OuterFlamesZOffset,

                            XRotation = legacyState.OuterFlamesXRotation,
                            YRotation = legacyState.OuterFlamesYRotation,
                            ZRotation = legacyState.OuterFlamesZRotation
                        },

                    Settings =
                        new OuterFlamesVfxSettings
                        {
                            WorldEnabled =
                                legacyState.OuterFlamesWorldEnabled,

                            BlackEnabled =
                                legacyState.OuterFlamesBlackEnabled,

                            WhiteEnabled =
                                legacyState.OuterFlamesWhiteEnabled,

                            DragEnabled =
                                legacyState.OuterFlamesDragEnabled,

                            Energy =
                                legacyState.OuterFlamesEnergy,

                            Scale =
                                legacyState.OuterFlamesScale,

                            Luminance =
                                legacyState.OuterFlamesLuminance,

                            Hue =
                                legacyState.OuterFlamesHue,

                            Lifetime =
                                legacyState.OuterFlamesLifetime,

                            SimulationSpeed =
                                legacyState.OuterFlamesSimulationSpeed,

                            Length =
                                legacyState.OuterFlamesLength,

                            Width =
                                legacyState.OuterFlamesWidth
                        }
                };

            state.Effects.Add(
                outerFlamesBlock);

            return state;
        }

        internal static WeaponVfxState CreateStrandsPrototype(
            VfxState legacyState)
        {
            var state =
                CreateStateWithLegacyRigTransform(
                    legacyState);

            var strandsBlock =
                new VfxEffectBlock
                {
                    InstanceId = LegacyStrandsInstanceId,
                    TypeId = VfxEffectTypeIds.Strands,
                    Enabled = legacyState.StrandsEnabled,

                    Transform =
                        new VfxTransformState
                        {
                            XOffset = legacyState.StrandsXOffset,
                            YOffset = legacyState.StrandsYOffset,
                            ZOffset = legacyState.StrandsZOffset,

                            XRotation = legacyState.StrandsXRotation,
                            YRotation = legacyState.StrandsYRotation,
                            ZRotation = legacyState.StrandsZRotation
                        },

                    Settings =
                        new StrandsVfxSettings
                        {
                            SpectrumEnabled =
                                legacyState.StrandsSpectrumEnabled,

                            Energy =
                                legacyState.StrandsEnergy,

                            ScaleWhole =
                                legacyState.StrandsScaleWhole,

                            ScaleParts =
                                legacyState.StrandsScaleParts,

                            Luminance =
                                legacyState.StrandsLuminance,

                            Hue =
                                legacyState.StrandsHue,

                            Lifetime =
                                legacyState.StrandsLifetime,

                            Length =
                                legacyState.StrandsLength,

                            SpectrumSpeed =
                                legacyState.StrandsSpectrumSpeed,

                            Speed =
                                legacyState.StrandsSpeed,

                            Radius =
                                legacyState.StrandsRadius,

                            Drift =
                                legacyState.StrandsDrift
                        }
                };

            state.Effects.Add(
                strandsBlock);

            return state;
        }

        internal static WeaponVfxState CreateSparksPrototype(
            VfxState legacyState)
        {
            var state =
                CreateStateWithLegacyRigTransform(
                    legacyState);

            var sparksBlock =
                new VfxEffectBlock
                {
                    InstanceId = LegacySparksInstanceId,
                    TypeId = VfxEffectTypeIds.Sparks,
                    Enabled = legacyState.SparksEnabled,

                    Transform =
                        new VfxTransformState
                        {
                            XOffset = legacyState.SparksXOffset,
                            YOffset = legacyState.SparksYOffset,
                            ZOffset = legacyState.SparksZOffset,

                            XRotation = legacyState.SparksXRotation,
                            YRotation = legacyState.SparksYRotation,
                            ZRotation = legacyState.SparksZRotation
                        },

                    Settings =
                        new SparksVfxSettings
                        {
                            Energy = legacyState.SparksEnergy,
                            Scale = legacyState.SparksScale,
                            Luminance = legacyState.SparksLuminance,
                            Hue = legacyState.SparksHue,
                            Lifetime = legacyState.SparksLifetime,

                            SimulationSpeed =
                                legacyState.SparksSimulationSpeed,

                            Length = legacyState.SparksLength,
                            Width = legacyState.SparksWidth
                        }
                };

            state.Effects.Add(
                sparksBlock);

            return state;
        }

        internal static WeaponVfxState CreateFlarePrototype(
            VfxState legacyState)
        {
            var state =
                CreateStateWithLegacyRigTransform(
                    legacyState);

            var flareBlock =
                new VfxEffectBlock
                {
                    InstanceId = LegacyFlareInstanceId,
                    TypeId = VfxEffectTypeIds.Flare,
                    Enabled = legacyState.FlareEnabled,

                    Transform =
                        new VfxTransformState
                        {
                            XOffset = legacyState.FlareXOffset,
                            YOffset = legacyState.FlareYOffset,
                            ZOffset = legacyState.FlareZOffset,

                            // Legacy Flare never exposed rotation controls.
                            XRotation = 0f,
                            YRotation = 0f,
                            ZRotation = 0f
                        },

                    Settings =
                        new FlareVfxSettings
                        {
                            Scale = legacyState.FlareScale,
                            Luminance = legacyState.FlareLuminance,
                            Hue = legacyState.FlareHue
                        }
                };

            state.Effects.Add(
                flareBlock);

            return state;
        }

        internal static WeaponVfxState CreateAuraPrototype(
            VfxState legacyState)
        {
            var state =
                CreateStateWithLegacyRigTransform(
                    legacyState);

            var auraBlock =
                new VfxEffectBlock
                {
                    InstanceId = LegacyAuraInstanceId,
                    TypeId = VfxEffectTypeIds.Aura,
                    Enabled = legacyState.AuraEnabled,

                    Transform =
                        new VfxTransformState
                        {
                            XOffset = legacyState.AuraXOffset,
                            YOffset = legacyState.AuraYOffset,
                            ZOffset = legacyState.AuraZOffset,

                            XRotation = legacyState.AuraXRotation,
                            YRotation = legacyState.AuraYRotation,
                            ZRotation = legacyState.AuraZRotation
                        },

                    Settings =
                        new AuraVfxSettings
                        {
                            Scale = legacyState.AuraScale,
                            Luminance = legacyState.AuraLuminance,
                            Hue = legacyState.AuraHue
                        }
                };

            state.Effects.Add(
                auraBlock);

            return state;
        }

        internal static WeaponVfxState CreateOrbitalsOrbsPrototype(
            VfxState legacyState)
        {
            var state =
                CreateStateWithLegacyRigTransform(
                    legacyState);

            var orbsBlock =
                new VfxEffectBlock
                {
                    InstanceId =
                        LegacyOrbitalsOrbsInstanceId,

                    TypeId =
                        VfxEffectTypeIds.OrbitalsOrbs,

                    Enabled =
                        legacyState.OrbitalsOrbsEnabled,

                    Transform =
                        new VfxTransformState
                        {
                            XOffset =
                                legacyState.OrbitalsOrbsXOffset,

                            YOffset =
                                legacyState.OrbitalsOrbsYOffset,

                            ZOffset =
                                legacyState.OrbitalsOrbsZOffset,

                            XRotation =
                                legacyState.OrbitalsOrbsXRotation,

                            YRotation =
                                legacyState.OrbitalsOrbsYRotation,

                            ZRotation =
                                legacyState.OrbitalsOrbsZRotation
                        },

                    Settings =
                        new OrbitalsOrbsVfxSettings
                        {
                            Scale =
                                legacyState.OrbitalsOrbsScale,

                            Luminance =
                                legacyState.OrbitalsOrbsLuminance,

                            Hue =
                                legacyState.OrbitalsOrbsHue,

                            Path =
                                new OrbitalsPathVfxSettings
                                {
                                    SnakeEnabled =
                                        legacyState
                                            .OrbitalsOrbsSnakeEnabled,

                                    Count =
                                        legacyState.OrbitalsOrbsCount,

                                    Speed =
                                        legacyState.OrbitalsOrbsSpeed,

                                    Spacing =
                                        legacyState.OrbitalsOrbsSpacing,

                                    Length =
                                        legacyState.OrbitalsOrbsLength,

                                    Radius =
                                        legacyState.OrbitalsOrbsRadius,

                                    Cycles =
                                        legacyState.OrbitalsOrbsCycles,

                                    Drift =
                                        legacyState.OrbitalsOrbsDrift
                                }
                        }
                };

            // OrbitalsOrbsGlueEnabled is intentionally not copied here.
            // In the legacy schema it controls coupling of dependent orbital
            // effects to Orbs rather than Orbs' own behavior. That relationship
            // will move with those dependent blocks when they are migrated.

            state.Effects.Add(
                orbsBlock);

            return state;
        }

        private static WeaponVfxState CreateStateWithLegacyRigTransform(
            VfxState legacyState)
        {
            return new WeaponVfxState
            {
                RigTransform =
                    new VfxTransformState
                    {
                        XOffset = legacyState.RigXOffset,
                        YOffset = legacyState.RigYOffset,
                        ZOffset = legacyState.RigZOffset,

                        XRotation = legacyState.RigXRotation,
                        YRotation = legacyState.RigYRotation,
                        ZRotation = legacyState.RigZRotation
                    }
            };
        }
    }
}