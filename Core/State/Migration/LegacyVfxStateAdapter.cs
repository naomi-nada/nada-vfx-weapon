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
        // Legacy effects get deterministic IDs during migration.
        // Sparks is slot 4 in the old effect ordering:
        // Inner, Outer, Strands, Sparks, Flare, Aura, then Orbitals.
        private const uint LegacySparksInstanceId = 4;

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

            state.Effects.Add(sparksBlock);

            return state;
        }
    }
}