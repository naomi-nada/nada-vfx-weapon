using System.Collections.Generic;

namespace NADA.VFX.Weapon.Core.State.Blocks
{
    /// <summary>
    /// The complete resolved VFX state for one weapon.
    ///
    /// This is pure data. It does not know whether its values came from
    /// config, ItemData, a style, multiplayer, or the public API.
    /// </summary>
    internal sealed class WeaponVfxState
    {
        internal const int CurrentSchemaVersion = 2;

        internal int SchemaVersion { get; set; } =
            CurrentSchemaVersion;

        internal VfxTransformState RigTransform { get; set; } =
            new();

        // Weapon geometry opacity, independent of all effect blocks.
        // 1 = original appearance; 0 = invisible weapon mesh.
        // Validate incoming values before applying or persisting state.
        internal float WeaponOpacity { get; set; } = 1f;

        // List position is the effect order.
        // Don't add a second Order property and create two owners.
        internal List<VfxEffectBlock> Effects { get; set; } =
            new();
    }
}