namespace NADA.VFX.Weapon.Core.State.Blocks
{
    /// <summary>
    /// Serializable transform data without depending on UnityEngine types.
    /// </summary>
    internal sealed class VfxTransformState
    {
        internal float XOffset { get; set; }
        internal float YOffset { get; set; }
        internal float ZOffset { get; set; }

        internal float XRotation { get; set; }
        internal float YRotation { get; set; }
        internal float ZRotation { get; set; }
    }
}