using NADA.VFX.Weapon.Core.Debug;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Modules.Effects;
using NADA.VFX.Weapon.Modules.Motion;
using NADA.VFX.Weapon.Runtime.Formation;
using UnityEngine;

namespace NADA.VFX.Weapon.Runtime.Binding
{
    internal static class NadaOrbitalsBlockBinder
    {
        internal static bool BindCoresEffect(
            Transform instanceRootTransform,
            Transform coresRootTransform,
            Transform poolRootTransform,
            VfxEffectBlock block)
        {
            if (instanceRootTransform == null)
                return false;

            NadaOrbitalsCoresBlockEffect effect =
                GetOrAdd<NadaOrbitalsCoresBlockEffect>(
                    instanceRootTransform);

            bool accepted =
                effect.SetBlockState(
                    coresRootTransform,
                    poolRootTransform,
                    block);

            NadaLogControl.Info(
                $"orbitals-cores-block-bind:{instanceRootTransform.GetInstanceID()}",
                $"{Plugin.ModName}: [OrbitalsCoresBlockBind] " +
                $"root='{instanceRootTransform.name}' " +
                $"id={(block != null ? block.InstanceId.ToString() : "null")} " +
                $"accepted={accepted}");

            return accepted;
        }

        internal static bool BindParticleEffect(
            Transform instanceRootTransform,
            Transform visualRootTransform,
            Transform poolRootTransform,
            NadaOrbitalsFamily family,
            VfxEffectBlock block)
        {
            if (instanceRootTransform == null)
                return false;

            NadaOrbitalsParticleBlockEffect effect =
                GetOrAdd<NadaOrbitalsParticleBlockEffect>(
                    instanceRootTransform);

            bool accepted =
                effect.SetBlockState(
                    family,
                    visualRootTransform,
                    poolRootTransform,
                    block);

            NadaLogControl.Info(
                $"orbitals-particle-block-bind:{instanceRootTransform.GetInstanceID()}",
                $"{Plugin.ModName}: [OrbitalsParticleBlockBind] " +
                $"root='{instanceRootTransform.name}' " +
                $"family='{family}' " +
                $"id={(block != null ? block.InstanceId.ToString() : "null")} " +
                $"type='{block?.TypeId ?? "null"}' " +
                $"accepted={accepted}");

            return accepted;
        }

        internal static bool BindMotion(
            Transform motionRootTransform,
            Transform headVisualTransform,
            Transform followerPoolRootTransform,
            VfxEffectBlock block,
            ResolvedOrbitalsFormation resolved)
        {
            if (motionRootTransform == null)
                return false;

            NadaOrbitalsBlockMotion motion =
                GetOrAdd<NadaOrbitalsBlockMotion>(
                    motionRootTransform);

            bool accepted =
                motion.SetBlockState(
                    block,
                    resolved,
                    headVisualTransform,
                    followerPoolRootTransform);

            string mode =
                resolved == null
                    ? "invalid"
                    : resolved.IsFollower
                        ? "follower"
                        : resolved.IsLeader
                            ? "leader"
                            : "independent";

            string trajectorySource =
                resolved != null
                    ? resolved.TrajectorySourceInstanceId.ToString()
                    : "null";

            NadaLogControl.Info(
                $"orbitals-block-motion-bind:{motionRootTransform.GetInstanceID()}",
                $"{Plugin.ModName}: [OrbitalsBlockMotionBind] " +
                $"root='{motionRootTransform.name}' " +
                $"id={(block != null ? block.InstanceId.ToString() : "null")} " +
                $"type='{block?.TypeId ?? "null"}' " +
                $"mode={mode} " +
                $"trajectorySource={trajectorySource} " +
                $"accepted={accepted}");

            return accepted;
        }

        internal static bool BindTrajectorySource(
            Transform followerMotionRootTransform,
            Transform sourceMotionRootTransform)
        {
            if (followerMotionRootTransform == null)
                return false;

            NadaOrbitalsBlockMotion followerMotion =
                followerMotionRootTransform
                    .GetComponent<NadaOrbitalsBlockMotion>();

            if (followerMotion == null)
            {
                NadaLogControl.Info(
                    $"orbitals-trajectory-source-bind-missing-follower:" +
                    $"{followerMotionRootTransform.GetInstanceID()}",
                    $"{Plugin.ModName}: [OrbitalsTrajectorySourceBind] " +
                    $"followerRoot='{followerMotionRootTransform.name}' " +
                    $"accepted=False " +
                    $"reason='missing-follower-motion'");

                return false;
            }

            INadaOrbitalsPhaseSource source =
                ResolvePhaseSource(
                    sourceMotionRootTransform);

            bool accepted =
                followerMotion.BindTrajectorySource(
                    source);

            uint expectedSourceInstanceId =
                followerMotion
                    .GetTrajectorySourceInstanceId();

            string actualSourceInstanceId =
                source != null
                    ? source.OrbitalsInstanceId.ToString()
                    : "none";

            NadaLogControl.Info(
                $"orbitals-trajectory-source-bind:" +
                $"{followerMotionRootTransform.GetInstanceID()}",
                $"{Plugin.ModName}: [OrbitalsTrajectorySourceBind] " +
                $"followerRoot='{followerMotionRootTransform.name}' " +
                $"expectedSource={expectedSourceInstanceId} " +
                $"actualSource={actualSourceInstanceId} " +
                $"sourceRoot='{sourceMotionRootTransform?.name ?? "null"}' " +
                $"accepted={accepted}");

            return accepted;
        }

        internal static void ClearTrajectorySource(
            Transform followerMotionRootTransform)
        {
            if (followerMotionRootTransform == null)
                return;

            NadaOrbitalsBlockMotion followerMotion =
                followerMotionRootTransform
                    .GetComponent<NadaOrbitalsBlockMotion>();

            if (followerMotion == null)
                return;

            uint previousSourceInstanceId =
                followerMotion
                    .GetTrajectorySourceInstanceId();

            bool wasFollower =
                followerMotion.IsFollower();

            followerMotion.ClearTrajectorySource();

            if (!wasFollower)
                return;

            NadaLogControl.Info(
                $"orbitals-trajectory-source-clear:" +
                $"{followerMotionRootTransform.GetInstanceID()}",
                $"{Plugin.ModName}: [OrbitalsTrajectorySourceClear] " +
                $"followerRoot='{followerMotionRootTransform.name}' " +
                $"source={previousSourceInstanceId}");
        }

        private static INadaOrbitalsPhaseSource ResolvePhaseSource(
            Transform sourceMotionRootTransform)
        {
            if (sourceMotionRootTransform == null)
                return null;

            // Fully migrated orbital families use the generic block motion.
            NadaOrbitalsBlockMotion blockMotion =
                sourceMotionRootTransform
                    .GetComponent<NadaOrbitalsBlockMotion>();

            if (blockMotion != null)
                return blockMotion;

            // ID7 Orbs still uses the transitional NadaOrbitalsMotion
            // implementation, but block-backed Orbs now exposes the same
            // phase-source contract.
            NadaOrbitalsMotion orbitalsMotion =
                sourceMotionRootTransform
                    .GetComponent<NadaOrbitalsMotion>();

            if (orbitalsMotion != null)
                return orbitalsMotion;

            return null;
        }

        private static TComponent GetOrAdd<TComponent>(
            Transform rootTransform)
            where TComponent : Component
        {
            TComponent component =
                rootTransform.GetComponent<TComponent>();

            if (component == null)
            {
                component =
                    rootTransform.gameObject
                        .AddComponent<TComponent>();
            }

            return component;
        }
    }
}