namespace NADA.VFX.Weapon.Core.State.Blocks
{
    /// <summary>
    /// One independently configurable instance of an effect.
    /// </summary>
    internal sealed class VfxEffectBlock
    {
        // Stable only within this weapon state. Two different weapons may
        // legitimately contain the same numeric instance IDs.
        internal uint InstanceId { get; set; }

        // Stable serialized identifier such as "sparks" or "inner_flames".
        // Don't use class names here; class names are implementation details.
        internal string TypeId { get; set; }

        internal bool Enabled { get; set; } = true;

        // Placement is common to effect instances, so it belongs here instead
        // of being duplicated throughout every effect-specific settings type.
        internal VfxTransformState Transform { get; set; } =
            new();

        internal VfxEffectSettings Settings { get; set; }
    }
}