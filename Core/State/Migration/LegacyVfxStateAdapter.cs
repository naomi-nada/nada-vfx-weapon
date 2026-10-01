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
        // Legacy effects get deterministic IDs during migration.
        // Old order:
        // Inner, Outer, Strands, Sparks, Flare, Aura, then Orbitals.
        private const uint LegacySparksInstanceId = 4;
        private const uint LegacyFlareInstanceId = 5;

        internal static WeaponVfxState CreateSparksPrototype(
            VfxState legacyState)
        {
            var state =
                new WeaponVfxState
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
                new WeaponVfxState
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

                            // Legacy Flare had no rotation controls.
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
    }
}