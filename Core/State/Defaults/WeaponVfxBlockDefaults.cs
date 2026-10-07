using System.Collections.Generic;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Blocks.Effects;

namespace NADA.VFX.Weapon.Core.State.Defaults
{
    /// <summary>
    /// Creates valid block-native default effect state.
    ///
    /// These defaults are independent of the legacy manager. Config may
    /// eventually expose or supply defaults, but it does not own live block
    /// state.
    /// </summary>
    internal static class WeaponVfxBlockDefaults
    {
        internal const float DefaultEnergy = 0f;
        internal const float DefaultScale = 1f;
        internal const float DefaultLuminance = 1f;
        internal const float DefaultHue = 0f;
        internal const float DefaultLifetime = 1f;
        internal const float DefaultSimulationSpeed = 1f;

        internal const float DefaultFlameLength = 0.80f;
        internal const float DefaultFlameWidth = 2.60f;
        internal const float DefaultSparksWidth = 1f;

        internal const float DefaultAuraScale = 1.60f;

        internal const float DefaultSpectrumSpeed = 1f;

        internal const float DefaultOrbitalsCount = 0f;
        internal const float DefaultOrbitalsSpeed = 0.08f;
        internal const float DefaultOrbitalsSpacing = 0.50f;
        internal const float DefaultOrbitalsLength = 1f;
        internal const float DefaultOrbitalsRadius = 1f;
        internal const float DefaultOrbitalsCycles = 3f;
        internal const float DefaultOrbitalsDrift = 0f;

        internal const float DefaultCoreSpinSpeed = 1f;

        internal const float DefaultEffectOffset = 0f;
        internal const float DefaultEffectRotation = 0f;

        internal static VfxEffectBlock Create(
            string typeId,
            uint instanceId)
        {
            if (instanceId == 0 ||
                string.IsNullOrWhiteSpace(typeId))
            {
                return null;
            }

            VfxEffectSettings settings =
                CreateSettings(
                    typeId);

            if (settings == null)
                return null;

            return new VfxEffectBlock
            {
                InstanceId =
                    instanceId,

                TypeId =
                    typeId,

                Enabled =
                    true,

                Transform =
                    CreateTransform(),

                Settings =
                    settings
            };
        }

        private static VfxTransformState CreateTransform()
        {
            return new VfxTransformState
            {
                XOffset =
                    DefaultEffectOffset,

                YOffset =
                    DefaultEffectOffset,

                ZOffset =
                    DefaultEffectOffset,

                XRotation =
                    DefaultEffectRotation,

                YRotation =
                    DefaultEffectRotation,

                ZRotation =
                    DefaultEffectRotation
            };
        }

        private static VfxEffectSettings CreateSettings(
            string typeId)
        {
            switch (typeId)
            {
                case VfxEffectTypeIds.InnerFlames:
                    return CreateInnerFlames();

                case VfxEffectTypeIds.OuterFlames:
                    return CreateOuterFlames();

                case VfxEffectTypeIds.Strands:
                    return CreateStrands();

                case VfxEffectTypeIds.Sparks:
                    return CreateSparks();

                case VfxEffectTypeIds.Flare:
                    return CreateFlare();

                case VfxEffectTypeIds.Aura:
                    return CreateAura();

                case VfxEffectTypeIds.OrbitalsOrbs:
                    return CreateOrbitalsOrbs();

                case VfxEffectTypeIds.OrbitalsCores:
                    return CreateOrbitalsCores();

                case VfxEffectTypeIds.OrbitalsFlames:
                    return CreateOrbitalsFlames();

                case VfxEffectTypeIds.OrbitalsEmbers:
                    return CreateOrbitalsEmbers();

                default:
                    return null;
            }
        }

        private static InnerFlamesVfxSettings
            CreateInnerFlames()
        {
            return new InnerFlamesVfxSettings
            {
                WorldEnabled =
                    true,

                BlackEnabled =
                    false,

                WhiteEnabled =
                    false,

                Energy =
                    DefaultEnergy,

                Scale =
                    DefaultScale,

                Luminance =
                    DefaultLuminance,

                Hue =
                    DefaultHue,

                Lifetime =
                    DefaultLifetime,

                SimulationSpeed =
                    DefaultSimulationSpeed,

                Length =
                    DefaultFlameLength,

                Width =
                    DefaultFlameWidth
            };
        }

        private static OuterFlamesVfxSettings
            CreateOuterFlames()
        {
            return new OuterFlamesVfxSettings
            {
                WorldEnabled =
                    true,

                BlackEnabled =
                    false,

                WhiteEnabled =
                    false,

                DragEnabled =
                    false,

                Energy =
                    DefaultEnergy,

                Scale =
                    DefaultScale,

                Luminance =
                    DefaultLuminance,

                Hue =
                    DefaultHue,

                Lifetime =
                    DefaultLifetime,

                SimulationSpeed =
                    DefaultSimulationSpeed,

                Length =
                    DefaultFlameLength,

                Width =
                    DefaultFlameWidth
            };
        }

        private static StrandsVfxSettings
            CreateStrands()
        {
            return new StrandsVfxSettings
            {
                SpectrumEnabled =
                    false,

                Energy =
                    DefaultEnergy,

                ScaleWhole =
                    DefaultScale,

                ScaleParts =
                    DefaultScale,

                Luminance =
                    DefaultLuminance,

                Hue =
                    DefaultHue,

                Lifetime =
                    DefaultLifetime,

                Length =
                    DefaultOrbitalsLength,

                SpectrumSpeed =
                    DefaultSpectrumSpeed,

                Speed =
                    DefaultOrbitalsSpeed,

                Radius =
                    DefaultOrbitalsRadius,

                Drift =
                    DefaultOrbitalsDrift
            };
        }

        private static SparksVfxSettings
            CreateSparks()
        {
            return new SparksVfxSettings
            {
                Energy =
                    DefaultEnergy,

                Scale =
                    DefaultScale,

                Luminance =
                    DefaultLuminance,

                Hue =
                    DefaultHue,

                Lifetime =
                    DefaultLifetime,

                SimulationSpeed =
                    DefaultSimulationSpeed,

                Length =
                    DefaultFlameLength,

                Width =
                    DefaultSparksWidth
            };
        }

        private static FlareVfxSettings
            CreateFlare()
        {
            return new FlareVfxSettings
            {
                Scale =
                    DefaultScale,

                Luminance =
                    DefaultLuminance,

                Hue =
                    DefaultHue
            };
        }

        private static AuraVfxSettings
            CreateAura()
        {
            return new AuraVfxSettings
            {
                Scale =
                    DefaultAuraScale,

                Luminance =
                    DefaultLuminance,

                Hue =
                    DefaultHue
            };
        }

        private static OrbitalsOrbsVfxSettings
            CreateOrbitalsOrbs()
        {
            return new OrbitalsOrbsVfxSettings
            {
                Scale =
                    DefaultScale,

                Luminance =
                    DefaultLuminance,

                Hue =
                    DefaultHue,

                Formation =
                    CreateOrbitalsFormation(
                        snakeEnabled: false)
            };
        }

        private static OrbitalsCoresVfxSettings
            CreateOrbitalsCores()
        {
            return new OrbitalsCoresVfxSettings
            {
                Scale =
                    DefaultScale,

                Luminance =
                    DefaultLuminance,

                Hue =
                    DefaultHue,

                SpinEnabled =
                    true,

                SpinSpeed =
                    DefaultCoreSpinSpeed,

                Formation =
                    CreateOrbitalsFormation(
                        snakeEnabled: false)
            };
        }

        private static OrbitalsFlamesVfxSettings
            CreateOrbitalsFlames()
        {
            return new OrbitalsFlamesVfxSettings
            {
                Energy =
                    DefaultEnergy,

                Scale =
                    DefaultScale,

                Luminance =
                    DefaultLuminance,

                Hue =
                    DefaultHue,

                Lifetime =
                    DefaultLifetime,

                SimulationSpeed =
                    DefaultSimulationSpeed,

                Formation =
                    CreateOrbitalsFormation(
                        snakeEnabled: false)
            };
        }

        private static OrbitalsEmbersVfxSettings
            CreateOrbitalsEmbers()
        {
            return new OrbitalsEmbersVfxSettings
            {
                Energy =
                    DefaultEnergy,

                Scale =
                    DefaultScale,

                Luminance =
                    DefaultLuminance,

                Hue =
                    DefaultHue,

                Lifetime =
                    DefaultLifetime,

                SimulationSpeed =
                    DefaultSimulationSpeed,

                Formation =
                    CreateOrbitalsFormation(
                        snakeEnabled: false)
            };
        }

        private static OrbitalsFormationVfxSettings
            CreateOrbitalsFormation(
                bool snakeEnabled)
        {
            return new OrbitalsFormationVfxSettings
            {
                Path =
                    new OrbitalsPathVfxSettings
                    {
                        SnakeEnabled =
                            snakeEnabled,

                        Count =
                            DefaultOrbitalsCount,

                        Speed =
                            DefaultOrbitalsSpeed,

                        Spacing =
                            DefaultOrbitalsSpacing,

                        Length =
                            DefaultOrbitalsLength,

                        Radius =
                            DefaultOrbitalsRadius,

                        Cycles =
                            DefaultOrbitalsCycles,

                        Drift =
                            DefaultOrbitalsDrift
                    },

                GlueLeaderEnabled =
                    false,

                GlueTargetInstanceIds =
                    new List<uint>()
            };
        }
    }
}