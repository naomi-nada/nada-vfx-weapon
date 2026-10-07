using System;
using System.Collections.Generic;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Blocks.Effects;

namespace NADA.VFX.Weapon.Runtime.Formation
{
    /// <summary>
    /// Temporary pre-1.0 contract tests for orbital formation resolution.
    ///
    /// These operate only on synthetic pure state. They never touch live
    /// weapon state, Unity hierarchy, config, persistence, or networking.
    /// </summary>
    internal static class OrbitalsFormationResolverContractTests
    {
        private static bool _hasRun;
        private static bool _isRunning;

        internal static void RunOnce()
        {
            if (_hasRun ||
                _isRunning)
            {
                return;
            }

            _isRunning =
                true;

            try
            {
                var failures =
                    new List<string>();

                Run(
                    failures,
                    "cores-can-lead",
                    TestCoresCanLead);

                Run(
                    failures,
                    "multiple-independent-leaders",
                    TestMultipleIndependentLeaders);

                Run(
                    failures,
                    "follower-keeps-own-count",
                    TestFollowerKeepsOwnCount);

                Run(
                    failures,
                    "zero-target-leader",
                    TestZeroTargetLeader);

                Run(
                    failures,
                    "self-target-fails",
                    TestSelfTargetFails);

                Run(
                    failures,
                    "duplicate-target-fails",
                    TestDuplicateTargetFails);

                Run(
                    failures,
                    "multiple-leaders-fail",
                    TestMultipleLeadersFail);

                Run(
                    failures,
                    "leader-targeted-fails",
                    TestLeaderTargetedFails);

                Run(
                    failures,
                    "missing-target-fails",
                    TestMissingTargetFails);

                Run(
                    failures,
                    "non-orbital-target-fails",
                    TestNonOrbitalTargetFails);

                Run(
                    failures,
                    "disabled-follower-not-bound",
                    TestDisabledFollowerNotBound);

                Run(
                    failures,
                    "disabled-leader-releases-follower",
                    TestDisabledLeaderReleasesFollower);

                Run(
                    failures,
                    "dormant-target-list",
                    TestDormantTargetList);

                Run(
                    failures,
                    "duplicate-instance-id-fails",
                    TestDuplicateInstanceIdFails);

                Run(
                    failures,
                    "null-block-fails",
                    TestNullBlockFails);

                if (failures.Count == 0)
                {
                    Plugin.Log.LogInfo(
                        $"{Plugin.ModName}: " +
                        $"[OrbitalsFormationResolverContractTests OK] " +
                        $"tests=15");

                    return;
                }

                Plugin.Log.LogError(
                    $"{Plugin.ModName}: " +
                    $"[OrbitalsFormationResolverContractTests FAIL] " +
                    $"failed={failures.Count}/15 " +
                    $"details='{string.Join(" | ", failures)}'");
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(
                    $"{Plugin.ModName}: " +
                    $"[OrbitalsFormationResolverContractTests FAIL] " +
                    $"{e}");
            }
            finally
            {
                _hasRun =
                    true;

                _isRunning =
                    false;
            }
        }

        private static void Run(
            List<string> failures,
            string name,
            Func<string> test)
        {
            string failure =
                test();

            if (failure == null)
                return;

            failures.Add(
                $"{name}:{failure}");
        }

        private static string TestCoresCanLead()
        {
            VfxEffectBlock orbs =
                CreateOrbital(
                    7,
                    VfxEffectTypeIds.OrbitalsOrbs);

            VfxEffectBlock cores =
                CreateOrbital(
                    8,
                    VfxEffectTypeIds.OrbitalsCores,
                    glueLeaderEnabled: true,
                    targets: new uint[] { 7 });

            WeaponVfxState state =
                CreateState(
                    orbs,
                    cores);

            if (!TryResolveValid(
                    state,
                    out OrbitalsFormationResolution resolution,
                    out string failure))
            {
                return failure;
            }

            if (resolution.LeaderCount != 1 ||
                resolution.FollowerCount != 1)
            {
                return
                    $"counts leaders={resolution.LeaderCount} " +
                    $"followers={resolution.FollowerCount}";
            }

            if (!resolution.TryGet(
                    7,
                    out ResolvedOrbitalsFormation resolvedOrbs))
            {
                return "missing-orbs";
            }

            if (!resolution.TryGet(
                    8,
                    out ResolvedOrbitalsFormation resolvedCores))
            {
                return "missing-cores";
            }

            if (!resolvedCores.IsLeader)
                return "cores-not-leader";

            if (!resolvedOrbs.IsFollower ||
                resolvedOrbs.LeaderInstanceId != 8 ||
                resolvedOrbs.TrajectorySourceInstanceId != 8)
            {
                return "orbs-not-following-cores";
            }

            return null;
        }

        private static string TestMultipleIndependentLeaders()
        {
            WeaponVfxState state =
                CreateState(
                    CreateOrbital(
                        7,
                        VfxEffectTypeIds.OrbitalsOrbs,
                        glueLeaderEnabled: true,
                        targets: new uint[] { 9 }),

                    CreateOrbital(
                        8,
                        VfxEffectTypeIds.OrbitalsCores,
                        glueLeaderEnabled: true,
                        targets: new uint[] { 10 }),

                    CreateOrbital(
                        9,
                        VfxEffectTypeIds.OrbitalsFlames),

                    CreateOrbital(
                        10,
                        VfxEffectTypeIds.OrbitalsEmbers));

            if (!TryResolveValid(
                    state,
                    out OrbitalsFormationResolution resolution,
                    out string failure))
            {
                return failure;
            }

            if (resolution.LeaderCount != 2 ||
                resolution.FollowerCount != 2)
            {
                return
                    $"counts leaders={resolution.LeaderCount} " +
                    $"followers={resolution.FollowerCount}";
            }

            if (!resolution.TryGet(
                    9,
                    out ResolvedOrbitalsFormation flames) ||
                flames.LeaderInstanceId != 7)
            {
                return "flames-not-following-7";
            }

            if (!resolution.TryGet(
                    10,
                    out ResolvedOrbitalsFormation embers) ||
                embers.LeaderInstanceId != 8)
            {
                return "embers-not-following-8";
            }

            return null;
        }

        private static string TestFollowerKeepsOwnCount()
        {
            VfxEffectBlock leader =
                CreateOrbital(
                    7,
                    VfxEffectTypeIds.OrbitalsOrbs,
                    glueLeaderEnabled: true,
                    targets: new uint[] { 9 },
                    count: 2f,
                    radius: 7f,
                    xOffset: 11f);

            VfxEffectBlock follower =
                CreateOrbital(
                    9,
                    VfxEffectTypeIds.OrbitalsFlames,
                    count: 5f,
                    radius: 3f,
                    xOffset: 22f);

            WeaponVfxState state =
                CreateState(
                    leader,
                    follower);

            if (!TryResolveValid(
                    state,
                    out OrbitalsFormationResolution resolution,
                    out string failure))
            {
                return failure;
            }

            if (!resolution.TryGet(
                    9,
                    out ResolvedOrbitalsFormation resolved))
            {
                return "missing-follower";
            }

            if (resolved.OwnCount != 5f)
            {
                return
                    $"count={resolved.OwnCount}";
            }

            if (resolved.EffectiveRadius != 7f)
            {
                return
                    $"radius={resolved.EffectiveRadius}";
            }

            if (resolved.EffectiveXOffset != 11f)
            {
                return
                    $"xOffset={resolved.EffectiveXOffset}";
            }

            if (resolved.TrajectorySourceInstanceId != 7)
            {
                return
                    $"trajectory={resolved.TrajectorySourceInstanceId}";
            }

            return null;
        }

        private static string TestZeroTargetLeader()
        {
            WeaponVfxState state =
                CreateState(
                    CreateOrbital(
                        7,
                        VfxEffectTypeIds.OrbitalsOrbs,
                        glueLeaderEnabled: true));

            if (!TryResolveValid(
                    state,
                    out OrbitalsFormationResolution resolution,
                    out string failure))
            {
                return failure;
            }

            if (resolution.LeaderCount != 1 ||
                resolution.FollowerCount != 0)
            {
                return
                    $"counts leaders={resolution.LeaderCount} " +
                    $"followers={resolution.FollowerCount}";
            }

            if (!resolution.TryGet(
                    7,
                    out ResolvedOrbitalsFormation leader) ||
                !leader.IsLeader)
            {
                return "leader-flag-lost";
            }

            return null;
        }

        private static string TestSelfTargetFails()
        {
            WeaponVfxState state =
                CreateState(
                    CreateOrbital(
                        7,
                        VfxEffectTypeIds.OrbitalsOrbs,
                        glueLeaderEnabled: true,
                        targets: new uint[] { 7 }));

            return ExpectInvalid(
                state,
                "self-target:leader=7");
        }

        private static string TestDuplicateTargetFails()
        {
            WeaponVfxState state =
                CreateState(
                    CreateOrbital(
                        7,
                        VfxEffectTypeIds.OrbitalsOrbs,
                        glueLeaderEnabled: true,
                        targets: new uint[] { 9, 9 }),

                    CreateOrbital(
                        9,
                        VfxEffectTypeIds.OrbitalsFlames));

            return ExpectInvalid(
                state,
                "duplicate-target:leader=7:target=9");
        }

        private static string TestMultipleLeadersFail()
        {
            WeaponVfxState state =
                CreateState(
                    CreateOrbital(
                        7,
                        VfxEffectTypeIds.OrbitalsOrbs,
                        glueLeaderEnabled: true,
                        targets: new uint[] { 9 }),

                    CreateOrbital(
                        8,
                        VfxEffectTypeIds.OrbitalsCores,
                        glueLeaderEnabled: true,
                        targets: new uint[] { 9 }),

                    CreateOrbital(
                        9,
                        VfxEffectTypeIds.OrbitalsFlames));

            return ExpectInvalid(
                state,
                "multiple-leaders:" +
                "target=9:" +
                "leaderA=7:" +
                "leaderB=8");
        }

        private static string TestLeaderTargetedFails()
        {
            WeaponVfxState state =
                CreateState(
                    CreateOrbital(
                        7,
                        VfxEffectTypeIds.OrbitalsOrbs,
                        glueLeaderEnabled: true,
                        targets: new uint[] { 8 }),

                    CreateOrbital(
                        8,
                        VfxEffectTypeIds.OrbitalsCores,
                        glueLeaderEnabled: true,
                        targets: new uint[] { 9 }),

                    CreateOrbital(
                        9,
                        VfxEffectTypeIds.OrbitalsFlames));

            return ExpectInvalid(
                state,
                "leader-targeted:" +
                "leader=7:" +
                "targetLeader=8");
        }

        private static string TestMissingTargetFails()
        {
            WeaponVfxState state =
                CreateState(
                    CreateOrbital(
                        7,
                        VfxEffectTypeIds.OrbitalsOrbs,
                        glueLeaderEnabled: true,
                        targets: new uint[] { 999 }));

            return ExpectInvalid(
                state,
                "missing-target:" +
                "leader=7:" +
                "target=999");
        }

        private static string TestNonOrbitalTargetFails()
        {
            WeaponVfxState state =
                CreateState(
                    CreateOrbital(
                        7,
                        VfxEffectTypeIds.OrbitalsOrbs,
                        glueLeaderEnabled: true,
                        targets: new uint[] { 4 }),

                    CreateSparks(
                        4));

            return ExpectInvalid(
                state,
                "non-orbital-target:" +
                "leader=7:" +
                "target=4:" +
                $"type={VfxEffectTypeIds.Sparks}");
        }

        private static string TestDisabledFollowerNotBound()
        {
            WeaponVfxState state =
                CreateState(
                    CreateOrbital(
                        7,
                        VfxEffectTypeIds.OrbitalsOrbs,
                        glueLeaderEnabled: true,
                        targets: new uint[] { 9 }),

                    CreateOrbital(
                        9,
                        VfxEffectTypeIds.OrbitalsFlames,
                        enabled: false));

            if (!TryResolveValid(
                    state,
                    out OrbitalsFormationResolution resolution,
                    out string failure))
            {
                return failure;
            }

            if (resolution.LeaderCount != 1 ||
                resolution.FollowerCount != 0)
            {
                return
                    $"counts leaders={resolution.LeaderCount} " +
                    $"followers={resolution.FollowerCount}";
            }

            if (!resolution.TryGet(
                    9,
                    out ResolvedOrbitalsFormation follower))
            {
                return "missing-follower";
            }

            if (follower.IsFollower ||
                follower.LeaderInstanceId.HasValue ||
                follower.TrajectorySourceInstanceId != 9)
            {
                return "disabled-follower-still-bound";
            }

            return null;
        }

        private static string TestDisabledLeaderReleasesFollower()
        {
            WeaponVfxState state =
                CreateState(
                    CreateOrbital(
                        7,
                        VfxEffectTypeIds.OrbitalsOrbs,
                        enabled: false,
                        glueLeaderEnabled: true,
                        targets: new uint[] { 9 }),

                    CreateOrbital(
                        9,
                        VfxEffectTypeIds.OrbitalsFlames));

            if (!TryResolveValid(
                    state,
                    out OrbitalsFormationResolution resolution,
                    out string failure))
            {
                return failure;
            }

            if (resolution.LeaderCount != 0 ||
                resolution.FollowerCount != 0)
            {
                return
                    $"counts leaders={resolution.LeaderCount} " +
                    $"followers={resolution.FollowerCount}";
            }

            if (!resolution.TryGet(
                    9,
                    out ResolvedOrbitalsFormation follower))
            {
                return "missing-follower";
            }

            if (follower.IsFollower ||
                follower.LeaderInstanceId.HasValue ||
                follower.TrajectorySourceInstanceId != 9)
            {
                return "follower-not-released";
            }

            return null;
        }

        private static string TestDormantTargetList()
        {
            WeaponVfxState state =
                CreateState(
                    CreateOrbital(
                        7,
                        VfxEffectTypeIds.OrbitalsOrbs,
                        glueLeaderEnabled: false,
                        targets: new uint[] { 9 }),

                    CreateOrbital(
                        9,
                        VfxEffectTypeIds.OrbitalsFlames));

            if (!TryResolveValid(
                    state,
                    out OrbitalsFormationResolution resolution,
                    out string failure))
            {
                return failure;
            }

            if (resolution.LeaderCount != 0 ||
                resolution.FollowerCount != 0)
            {
                return
                    $"counts leaders={resolution.LeaderCount} " +
                    $"followers={resolution.FollowerCount}";
            }

            if (!resolution.TryGet(
                    9,
                    out ResolvedOrbitalsFormation follower))
            {
                return "missing-follower";
            }

            if (follower.IsFollower ||
                follower.TrajectorySourceInstanceId != 9)
            {
                return "dormant-target-became-active";
            }

            return null;
        }

        private static string TestDuplicateInstanceIdFails()
        {
            WeaponVfxState state =
                CreateState(
                    CreateOrbital(
                        7,
                        VfxEffectTypeIds.OrbitalsOrbs),

                    CreateOrbital(
                        7,
                        VfxEffectTypeIds.OrbitalsCores));

            return ExpectInvalid(
                state,
                "duplicate-instance-id:7");
        }

        private static string TestNullBlockFails()
        {
            WeaponVfxState state =
                new WeaponVfxState();

            state.Effects.Add(
                null);

            return ExpectInvalid(
                state,
                "null-effect-block");
        }

        private static string ExpectInvalid(
            WeaponVfxState state,
            string expectedReason)
        {
            bool valid =
                OrbitalsFormationResolver.TryResolve(
                    state,
                    out OrbitalsFormationResolution resolution);

            if (valid)
                return "unexpected-valid";

            if (resolution == null)
                return "missing-resolution";

            if (resolution.IsValid)
                return "resolution-marked-valid";

            if (!string.Equals(
                    resolution.FailureReason,
                    expectedReason,
                    StringComparison.Ordinal))
            {
                return
                    $"reason='{resolution.FailureReason}' " +
                    $"expected='{expectedReason}'";
            }

            return null;
        }

        private static bool TryResolveValid(
            WeaponVfxState state,
            out OrbitalsFormationResolution resolution,
            out string failure)
        {
            bool valid =
                OrbitalsFormationResolver.TryResolve(
                    state,
                    out resolution);

            if (!valid)
            {
                failure =
                    $"invalid:{resolution?.FailureReason ?? "null"}";

                return false;
            }

            if (resolution == null)
            {
                failure =
                    "resolution-null";

                return false;
            }

            if (!resolution.IsValid)
            {
                failure =
                    $"marked-invalid:{resolution.FailureReason}";

                return false;
            }

            failure =
                null;

            return true;
        }

        private static WeaponVfxState CreateState(
            params VfxEffectBlock[] blocks)
        {
            var state =
                new WeaponVfxState();

            foreach (VfxEffectBlock block in blocks)
            {
                state.Effects.Add(
                    block);
            }

            return state;
        }

        private static VfxEffectBlock CreateOrbital(
            uint instanceId,
            string typeId,
            bool enabled = true,
            bool glueLeaderEnabled = false,
            uint[] targets = null,
            float count = 1f,
            float radius = 1f,
            float xOffset = 0f)
        {
            var formation =
                new OrbitalsFormationVfxSettings
                {
                    Path =
                        new OrbitalsPathVfxSettings
                        {
                            SnakeEnabled = true,
                            Count = count,
                            Speed = 2f,
                            Spacing = 3f,
                            Length = 4f,
                            Radius = radius,
                            Cycles = 6f,
                            Drift = 7f
                        },

                    GlueLeaderEnabled =
                        glueLeaderEnabled,

                    GlueTargetInstanceIds =
                        targets != null
                            ? new List<uint>(targets)
                            : new List<uint>()
                };

            VfxEffectSettings settings;

            if (typeId ==
                VfxEffectTypeIds.OrbitalsOrbs)
            {
                settings =
                    new OrbitalsOrbsVfxSettings
                    {
                        Formation =
                            formation
                    };
            }
            else if (typeId ==
                     VfxEffectTypeIds.OrbitalsCores)
            {
                settings =
                    new OrbitalsCoresVfxSettings
                    {
                        Formation =
                            formation
                    };
            }
            else if (typeId ==
                     VfxEffectTypeIds.OrbitalsFlames)
            {
                settings =
                    new OrbitalsFlamesVfxSettings
                    {
                        Formation =
                            formation
                    };
            }
            else if (typeId ==
                     VfxEffectTypeIds.OrbitalsEmbers)
            {
                settings =
                    new OrbitalsEmbersVfxSettings
                    {
                        Formation =
                            formation
                    };
            }
            else
            {
                throw new ArgumentOutOfRangeException(
                    nameof(typeId),
                    typeId,
                    "Unsupported synthetic orbital type.");
            }

            return new VfxEffectBlock
            {
                InstanceId =
                    instanceId,

                TypeId =
                    typeId,

                Enabled =
                    enabled,

                Transform =
                    new VfxTransformState
                    {
                        XOffset = xOffset,
                        YOffset = 12f,
                        ZOffset = 13f,

                        XRotation = 14f,
                        YRotation = 15f,
                        ZRotation = 16f
                    },

                Settings =
                    settings
            };
        }

        private static VfxEffectBlock CreateSparks(
            uint instanceId)
        {
            return new VfxEffectBlock
            {
                InstanceId =
                    instanceId,

                TypeId =
                    VfxEffectTypeIds.Sparks,

                Enabled =
                    true,

                Transform =
                    new VfxTransformState(),

                Settings =
                    new SparksVfxSettings()
            };
        }
    }
}