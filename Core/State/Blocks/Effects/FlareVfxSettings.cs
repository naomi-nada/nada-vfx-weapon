namespace NADA.VFX.Weapon.Core.State.Blocks.Effects
{
    /// <summary>
    /// Settings unique to one Flare effect instance.
    ///
    /// Enabled and placement belong to VfxEffectBlock.
    /// </summary>
    internal sealed class FlareVfxSettings : VfxEffectSettings
    {
        internal float Scale { get; set; }
        internal float Luminance { get; set; }
        internal float Hue { get; set; }
    }
}