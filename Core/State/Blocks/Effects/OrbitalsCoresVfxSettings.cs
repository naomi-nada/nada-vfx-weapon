namespace NADA.VFX.Weapon.Core.State.Blocks.Effects
{
    /// <summary>
    /// Settings unique to one orbital Cores effect instance.
    ///
    /// Enabled and placement belong to VfxEffectBlock.
    /// Formation behavior is shared with every orbital effect type.
    /// </summary>
    internal sealed class OrbitalsCoresVfxSettings : VfxEffectSettings
    {
        internal float Scale { get; set; }
        internal float Luminance { get; set; }
        internal float Hue { get; set; }

        internal bool SpinEnabled { get; set; }
        internal float SpinSpeed { get; set; }

        internal OrbitalsFormationVfxSettings Formation { get; set; } =
            new();
    }
}