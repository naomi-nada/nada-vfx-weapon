using System.Collections.Generic;

namespace NADA.VFX.Weapon.Core.State.Blocks.Effects
{
    /// <summary>
    /// Pure-data formation state shared by every orbital effect instance.
    ///
    /// Path describes this orbital's own independent formation.
    ///
    /// A Glue leader may control other orbital blocks identified by stable
    /// InstanceId. Those IDs describe logical relationships only; runtime
    /// object references are resolved separately.
    ///
    /// Disabling Glue does not clear its target list, and being controlled by
    /// another leader does not overwrite this orbital's own formation state.
    /// </summary>
    internal sealed class OrbitalsFormationVfxSettings
    {
        internal OrbitalsPathVfxSettings Path { get; set; } =
            new();

        internal bool GlueLeaderEnabled { get; set; }

        internal List<uint> GlueTargetInstanceIds { get; set; } =
            new();
    }
}