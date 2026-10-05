namespace NADA.VFX.Weapon.Core.State.Blocks.Effects
{
    /// <summary>
    /// Settings unique to one orbital Orbs effect instance.
    ///
    /// Enabled and placement belong to VfxEffectBlock.
    /// Orbital-path behavior is composed through Path so future orbital effect
    /// types can reuse it without joining a fixed family enum.
    /// </summary>
    internal sealed class OrbitalsOrbsVfxSettings : VfxEffectSettings
    {
        internal float Scale { get; set; }
        internal float Luminance { get; set; }
        internal float Hue { get; set; }

        internal OrbitalsPathVfxSettings Path { get; set; } =
            new();
    }
}