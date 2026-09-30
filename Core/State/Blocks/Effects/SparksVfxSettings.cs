namespace NADA.VFX.Weapon.Core.State.Blocks.Effects
{
    /// <summary>
    /// Settings unique to one Sparks effect instance.
    ///
    /// Enabled and placement live on VfxEffectBlock because those concepts
    /// are shared by every effect type.
    /// </summary>
    internal sealed class SparksVfxSettings : VfxEffectSettings
    {
        internal float Energy { get; set; }
        internal float Scale { get; set; }
        internal float Luminance { get; set; }
        internal float Hue { get; set; }
        internal float Lifetime { get; set; }
        internal float SimulationSpeed { get; set; }
        internal float Length { get; set; }
        internal float Width { get; set; }
    }
}