namespace NADA.VFX.Weapon.Core.State.Blocks.Effects
{
    /// <summary>
    /// Settings unique to one orbital Embers effect instance.
    ///
    /// Enabled and placement belong to VfxEffectBlock.
    /// Formation behavior is shared with every orbital effect type.
    /// </summary>
    internal sealed class OrbitalsEmbersVfxSettings : VfxEffectSettings
    {
        internal float Energy { get; set; }
        internal float Scale { get; set; }
        internal float Luminance { get; set; }
        internal float Hue { get; set; }
        internal float Lifetime { get; set; }
        internal float SimulationSpeed { get; set; }

        internal OrbitalsFormationVfxSettings Formation { get; set; } =
            new();
    }
}