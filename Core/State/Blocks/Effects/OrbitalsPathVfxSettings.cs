namespace NADA.VFX.Weapon.Core.State.Blocks.Effects
{
    /// <summary>
    /// Reusable pure-data settings for an effect that moves along NADA's
    /// orbital path.
    ///
    /// This is intentionally not an effect type. Different effect types can
    /// compose orbital path behavior without becoming members of a fixed
    /// Orbitals family.
    /// </summary>
    internal sealed class OrbitalsPathVfxSettings
    {
        internal bool SnakeEnabled { get; set; }

        internal float Count { get; set; }
        internal float Speed { get; set; }
        internal float Spacing { get; set; }
        internal float Length { get; set; }
        internal float Radius { get; set; }
        internal float Cycles { get; set; }
        internal float Drift { get; set; }
    }
}