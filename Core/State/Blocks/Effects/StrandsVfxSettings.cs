namespace NADA.VFX.Weapon.Core.State.Blocks.Effects
{
    /// <summary>
    /// Settings unique to one Strands effect instance.
    ///
    /// Enabled and placement belong to VfxEffectBlock.
    /// </summary>
    internal sealed class StrandsVfxSettings : VfxEffectSettings
    {
        internal bool SpectrumEnabled { get; set; }

        internal float Energy { get; set; }

        internal float ScaleWhole { get; set; }
        internal float ScaleParts { get; set; }

        internal float Luminance { get; set; }
        internal float Hue { get; set; }

        internal float Lifetime { get; set; }
        internal float Length { get; set; }

        internal float SpectrumSpeed { get; set; }
        internal float Speed { get; set; }
        internal float Radius { get; set; }
        internal float Drift { get; set; }
    }
}