namespace NADA.VFX.Weapon.Core.State.Blocks.Effects
{
    /// <summary>
    /// Settings unique to one Inner Flames effect instance.
    ///
    /// Enabled and placement belong to VfxEffectBlock.
    /// </summary>
    internal sealed class InnerFlamesVfxSettings : VfxEffectSettings
    {
        internal bool WorldEnabled { get; set; }

        internal bool BlackEnabled { get; set; }
        internal bool WhiteEnabled { get; set; }

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