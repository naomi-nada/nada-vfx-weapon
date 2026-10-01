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
        internal const string Sparks = "sparks";
        internal const string Flare = "flare";
    }
}