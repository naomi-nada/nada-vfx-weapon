namespace NADA.VFX.Weapon.Core.State.Blocks
{
    /// <summary>
    /// Stable effect type identifiers used by state, persistence, networking,
    /// and eventually the public API.
    ///
    /// These are schema IDs. Don't rename them just because a C# class changes.
    /// </summary>
    internal static class VfxEffectTypeIds
    {
        internal const string InnerFlames = "inner_flames";
        internal const string OuterFlames = "outer_flames";
        internal const string Strands = "strands";
        internal const string Sparks = "sparks";
        internal const string Flare = "flare";
        internal const string Aura = "aura";
        internal const string OrbitalsOrbs = "orbitals_orbs";
        internal const string OrbitalsCores = "orbitals_cores";
        internal const string OrbitalsFlames = "orbitals_flames";
        internal const string OrbitalsEmbers = "orbitals_embers";
    }
}