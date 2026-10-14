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

        // Transitional v3 compatibility scalar. New editor authoring uses
        // WeaponAppearance blocks, and editable-draft migration clears this.
        // Keep for old item/style/network payloads until a format migration.
        internal float WeaponOpacity { get; set; } = 1f;

        // List position is the effect order.
        // Don't add a second Order property and create two owners.
        internal List<VfxEffectBlock> Effects { get; set; } =
            new();
    }
}