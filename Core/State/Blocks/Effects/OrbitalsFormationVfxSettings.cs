using System.Collections.Generic;

namespace NADA.VFX.Weapon.Core.State.Blocks.Effects
{
    /// <summary>
    /// Movement belongs to formation state, not the visual effect type.
    /// Path remains the default for existing orbital blocks.
    /// </summary>
    internal enum OrbitalsMovementMode : byte
    {
        Path = 0,
        Attached = 1
    }

    /// <summary>
    /// Pure-data formation state shared by every orbital effect instance.
    ///
    /// Path retains this orbital's independent trajectory settings even
    /// when the selected movement mode does not consume them.
    ///
    /// Glue relationships reference stable InstanceIds, never runtime
    /// objects. Disabling Glue does not erase its saved target list.
    /// </summary>
    internal sealed class OrbitalsFormationVfxSettings
    {
        internal OrbitalsMovementMode MovementMode { get; set; } =
            OrbitalsMovementMode.Path;

        internal OrbitalsPathVfxSettings Path { get; set; } =
            new();

        internal bool GlueLeaderEnabled { get; set; }

        internal List<uint> GlueTargetInstanceIds { get; set; } =
            new();
    }
}