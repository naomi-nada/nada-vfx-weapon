using System.Collections.Generic;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Blocks.Effects;

namespace NADA.VFX.Weapon.Runtime.Formation
{
    /// <summary>
    /// Resolves orbital formation relationships from pure WeaponVfxState.
    ///
    /// This class does not know about Unity objects, hierarchy, ItemData,
    /// config, persistence, networking, or motion components.
    /// </summary>
    internal static class OrbitalsFormationResolver
    {
        internal static bool TryResolve(
            WeaponVfxState state,
            out OrbitalsFormationResolution resolution)
        {
            // Temporary pre-1.0 contract verification.
            // The runner is guarded and executes only once.
            OrbitalsFormationResolverContractTests.RunOnce();

            if (state == null)
            {
                return Fail(
                    "state-null",
                    out resolution);
            }

            if (state.Effects == null)
            {
                return Fail(
                    "effects-null",
                    out resolution);
            }

            var allBlocksByInstanceId =
                new Dictionary<uint, VfxEffectBlock>();

            var orbitalBlocksByInstanceId =
                new Dictionary<uint, VfxEffectBlock>();

            var formationByInstanceId =
                new Dictionary<uint, OrbitalsFormationVfxSettings>();

            var orbitalBlocksInStateOrder =
                new List<VfxEffectBlock>();

            foreach (VfxEffectBlock block in state.Effects)
            {
                if (block == null)
                {
                    return Fail(
                        "null-effect-block",
                        out resolution);
                }

                if (allBlocksByInstanceId.ContainsKey(
                        block.InstanceId))
                {
                    return Fail(
                        $"duplicate-instance-id:{block.InstanceId}",
                        out resolution);
                }

                allBlocksByInstanceId.Add(
                    block.InstanceId,
                    block);

                if (!IsOrbitalType(
                        block.TypeId))
                {
                    continue;
                }

                if (block.Transform == null)
                {
                    return Fail(
                        $"missing-transform:{block.InstanceId}",
                        out resolution);
                }

                if (!TryGetFormation(
                        block,
                        out OrbitalsFormationVfxSettings formation) ||
                    formation == null)
                {
                    return Fail(
                        $"invalid-orbital-settings:{block.InstanceId}:{block.TypeId}",
                        out resolution);
                }

                if (formation.Path == null)
                {
                    return Fail(
                        $"missing-path:{block.InstanceId}",
                        out resolution);
                }

                if (formation.GlueTargetInstanceIds == null)
                {
                    return Fail(
                        $"missing-glue-target-list:{block.InstanceId}",
                        out resolution);
                }

                orbitalBlocksByInstanceId.Add(
                    block.InstanceId,
                    block);

                formationByInstanceId.Add(
                    block.InstanceId,
                    formation);

                orbitalBlocksInStateOrder.Add(
                    block);
            }

            // A block declaring itself a leader remains leader-shaped even
            // while disabled. This prevents another active leader from
            // targeting it and creating a latent chain that becomes invalid
            // when the disabled block is re-enabled.
            var declaredLeaderInstanceIds =
                new HashSet<uint>();

            foreach (VfxEffectBlock block in
                     orbitalBlocksInStateOrder)
            {
                OrbitalsFormationVfxSettings formation =
                    formationByInstanceId[block.InstanceId];

                if (formation.GlueLeaderEnabled)
                {
                    declaredLeaderInstanceIds.Add(
                        block.InstanceId);
                }
            }

            // Membership ownership is validated separately from active
            // binding. A disabled follower may remain persisted in a leader's
            // target list, but it does not become an active follower until
            // re-enabled.
            var claimedTargetToLeader =
                new Dictionary<uint, uint>();

            var activeFollowerToLeader =
                new Dictionary<uint, uint>();

            int activeLeaderCount =
                0;

            foreach (VfxEffectBlock leaderBlock in
                     orbitalBlocksInStateOrder)
            {
                OrbitalsFormationVfxSettings leaderFormation =
                    formationByInstanceId[
                        leaderBlock.InstanceId];

                // Disabled leaders release their followers.
                // LeaderEnabled=false also makes the stored target list
                // dormant.
                if (!leaderBlock.Enabled ||
                    !leaderFormation.GlueLeaderEnabled)
                {
                    continue;
                }

                activeLeaderCount++;

                var targetsSeenByThisLeader =
                    new HashSet<uint>();

                foreach (uint targetInstanceId in
                         leaderFormation.GlueTargetInstanceIds)
                {
                    if (!targetsSeenByThisLeader.Add(
                            targetInstanceId))
                    {
                        return Fail(
                            $"duplicate-target:" +
                            $"leader={leaderBlock.InstanceId}:" +
                            $"target={targetInstanceId}",
                            out resolution);
                    }

                    if (targetInstanceId ==
                        leaderBlock.InstanceId)
                    {
                        return Fail(
                            $"self-target:" +
                            $"leader={leaderBlock.InstanceId}",
                            out resolution);
                    }

                    if (!allBlocksByInstanceId.TryGetValue(
                            targetInstanceId,
                            out VfxEffectBlock targetBlock))
                    {
                        return Fail(
                            $"missing-target:" +
                            $"leader={leaderBlock.InstanceId}:" +
                            $"target={targetInstanceId}",
                            out resolution);
                    }

                    if (!IsOrbitalType(
                            targetBlock.TypeId))
                    {
                        return Fail(
                            $"non-orbital-target:" +
                            $"leader={leaderBlock.InstanceId}:" +
                            $"target={targetInstanceId}:" +
                            $"type={targetBlock.TypeId}",
                            out resolution);
                    }

                    if (!orbitalBlocksByInstanceId.ContainsKey(
                            targetInstanceId))
                    {
                        return Fail(
                            $"invalid-orbital-target:" +
                            $"leader={leaderBlock.InstanceId}:" +
                            $"target={targetInstanceId}",
                            out resolution);
                    }

                    // A leader cannot also be a follower. This rule also
                    // prevents Glue chains and cycles without requiring
                    // runtime graph walking.
                    if (declaredLeaderInstanceIds.Contains(
                            targetInstanceId))
                    {
                        return Fail(
                            $"leader-targeted:" +
                            $"leader={leaderBlock.InstanceId}:" +
                            $"targetLeader={targetInstanceId}",
                            out resolution);
                    }

                    if (claimedTargetToLeader.TryGetValue(
                            targetInstanceId,
                            out uint existingLeaderInstanceId))
                    {
                        if (existingLeaderInstanceId !=
                            leaderBlock.InstanceId)
                        {
                            return Fail(
                                $"multiple-leaders:" +
                                $"target={targetInstanceId}:" +
                                $"leaderA={existingLeaderInstanceId}:" +
                                $"leaderB={leaderBlock.InstanceId}",
                                out resolution);
                        }
                    }
                    else
                    {
                        claimedTargetToLeader.Add(
                            targetInstanceId,
                            leaderBlock.InstanceId);
                    }

                    // Persisted membership stays intact while the follower is
                    // disabled, but no live Glue relationship is resolved.
                    if (!targetBlock.Enabled)
                        continue;

                    activeFollowerToLeader[targetInstanceId] =
                        leaderBlock.InstanceId;
                }
            }

            var resolvedEntries =
                new List<ResolvedOrbitalsFormation>();

            foreach (VfxEffectBlock block in
                     orbitalBlocksInStateOrder)
            {
                OrbitalsFormationVfxSettings ownFormation =
                    formationByInstanceId[
                        block.InstanceId];

                OrbitalsPathVfxSettings ownPath =
                    ownFormation.Path;

                bool isLeader =
                    block.Enabled &&
                    ownFormation.GlueLeaderEnabled;

                uint leaderInstanceId =
                    0;

                bool hasLeader =
                    activeFollowerToLeader.TryGetValue(
                        block.InstanceId,
                        out leaderInstanceId);

                bool isFollower =
                    block.Enabled &&
                    hasLeader;

                VfxEffectBlock trajectorySourceBlock =
                    block;

                OrbitalsPathVfxSettings trajectorySourcePath =
                    ownPath;

                uint? resolvedLeaderInstanceId =
                    null;

                if (isFollower)
                {
                    trajectorySourceBlock =
                        orbitalBlocksByInstanceId[
                            leaderInstanceId];

                    trajectorySourcePath =
                        formationByInstanceId[
                            leaderInstanceId]
                            .Path;

                    resolvedLeaderInstanceId =
                        leaderInstanceId;
                }

                VfxTransformState effectiveTransform =
                    trajectorySourceBlock.Transform;

                resolvedEntries.Add(
                    new ResolvedOrbitalsFormation(
                        block.InstanceId,
                        block.TypeId,
                        block.Enabled,
                        isLeader,
                        isFollower,
                        resolvedLeaderInstanceId,
                        trajectorySourceBlock.InstanceId,

                        // Count always stays with this block.
                        ownPath.Count,

                        // The remaining path data describes trajectory and is
                        // borrowed from the leader while glued.
                        trajectorySourcePath.SnakeEnabled,
                        trajectorySourcePath.Speed,
                        trajectorySourcePath.Spacing,
                        trajectorySourcePath.Length,
                        trajectorySourcePath.Radius,
                        trajectorySourcePath.Cycles,
                        trajectorySourcePath.Drift,

                        effectiveTransform.XOffset,
                        effectiveTransform.YOffset,
                        effectiveTransform.ZOffset,

                        effectiveTransform.XRotation,
                        effectiveTransform.YRotation,
                        effectiveTransform.ZRotation));
            }

            resolution =
                OrbitalsFormationResolution.Valid(
                    resolvedEntries,
                    activeLeaderCount,
                    activeFollowerToLeader.Count);

            return true;
        }

        private static bool TryGetFormation(
            VfxEffectBlock block,
            out OrbitalsFormationVfxSettings formation)
        {
            formation =
                null;

            if (block == null)
                return false;

            if (block.TypeId ==
                    VfxEffectTypeIds.OrbitalsOrbs &&
                block.Settings is OrbitalsOrbsVfxSettings orbs)
            {
                formation =
                    orbs.Formation;

                return formation != null;
            }

            if (block.TypeId ==
                    VfxEffectTypeIds.OrbitalsCores &&
                block.Settings is OrbitalsCoresVfxSettings cores)
            {
                formation =
                    cores.Formation;

                return formation != null;
            }

            if (block.TypeId ==
                    VfxEffectTypeIds.OrbitalsFlames &&
                block.Settings is OrbitalsFlamesVfxSettings flames)
            {
                formation =
                    flames.Formation;

                return formation != null;
            }

            if (block.TypeId ==
                    VfxEffectTypeIds.OrbitalsEmbers &&
                block.Settings is OrbitalsEmbersVfxSettings embers)
            {
                formation =
                    embers.Formation;

                return formation != null;
            }

            return false;
        }

        private static bool IsOrbitalType(
            string typeId)
        {
            return
                typeId ==
                    VfxEffectTypeIds.OrbitalsOrbs ||
                typeId ==
                    VfxEffectTypeIds.OrbitalsCores ||
                typeId ==
                    VfxEffectTypeIds.OrbitalsFlames ||
                typeId ==
                    VfxEffectTypeIds.OrbitalsEmbers;
        }

        private static bool Fail(
            string failureReason,
            out OrbitalsFormationResolution resolution)
        {
            resolution =
                OrbitalsFormationResolution.Invalid(
                    failureReason);

            return false;
        }
    }
}