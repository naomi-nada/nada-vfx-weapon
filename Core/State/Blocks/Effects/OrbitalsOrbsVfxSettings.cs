namespace NADA.VFX.Weapon.Core.State.Blocks.Effects
{
    /// <summary>
    /// Settings unique to one orbital Orbs effect instance.
    ///
    /// Enabled and placement belong to VfxEffectBlock.
    /// Formation behavior is composed through Formation so Orbs remains a peer
    /// of every other orbital effect type rather than owning special path or
    /// Glue semantics.
    /// </summary>
    internal sealed class OrbitalsOrbsVfxSettings : VfxEffectSettings
    {
        internal float Scale { get; set; }
        internal float Luminance { get; set; }
        internal float Hue { get; set; }

        internal OrbitalsFormationVfxSettings Formation { get; set; } =
            new();

        /// <summary>
        /// Transitional source-compatibility alias while the existing Orbs
        /// consumers are migrated from Path to Formation.Path.
        ///
        /// This does not own separate state. Remove it before block-state
        /// persistence/networking/API become authoritative.
        /// </summary>
        internal OrbitalsPathVfxSettings Path
        {
            get =>
                Formation?.Path;

            set
            {
                Formation ??=
                    new OrbitalsFormationVfxSettings();

                Formation.Path =
                    value ??
                    new OrbitalsPathVfxSettings();
            }
        }
    }
}