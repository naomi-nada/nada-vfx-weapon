using NADA.VFX.Weapon.Core.State;
using UnityEngine;
using NADA.VFX.Weapon.Modules.Motion;
using NADA.VFX.Weapon.Runtime.Binding;

namespace NADA.VFX.Weapon.Runtime.Structure
{
    internal static class NadaOrbitalsRigAssembly
    {
        private static Mesh _runtimeSphereMesh;

        internal readonly struct OrbitalsOrbsInstanceStructure
        {
            internal Transform OrbsRootTransform { get; }
            internal Transform RigRootTransform { get; }
            internal Transform MotionRootTransform { get; }
            internal Transform HeadVisualTransform { get; }
            internal Transform PoolRootTransform { get; }

            internal bool IsValid =>
                OrbsRootTransform != null &&
                RigRootTransform != null &&
                MotionRootTransform != null &&
                HeadVisualTransform != null &&
                PoolRootTransform != null;

            internal OrbitalsOrbsInstanceStructure(
                Transform orbsRootTransform,
                Transform rigRootTransform,
                Transform motionRootTransform,
                Transform headVisualTransform,
                Transform poolRootTransform)
            {
                OrbsRootTransform =
                    orbsRootTransform;

                RigRootTransform =
                    rigRootTransform;

                MotionRootTransform =
                    motionRootTransform;

                HeadVisualTransform =
                    headVisualTransform;

                PoolRootTransform =
                    poolRootTransform;
            }
        }

        internal static OrbitalsOrbsInstanceStructure
            EnsureOrbitalsOrbsInstanceStructure(
                Transform instanceRootTransform,
                string ownerNameForLogs)
        {
            if (!NadaRigCache.CacheReady ||
                NadaRigCache.DemisterTemplateInactive == null ||
                instanceRootTransform == null)
            {
                return default;
            }

            Transform orbsRootTransform =
                NadaRigPaths.FindDirectChild(
                    instanceRootTransform,
                    Plugin.OrbitalsOrbsName);

            if (orbsRootTransform == null)
            {
                GameObject orbsRootObject =
                    Object.Instantiate(
                        NadaRigCache.DemisterTemplateInactive,
                        instanceRootTransform,
                        false);

                orbsRootObject.name =
                    Plugin.OrbitalsOrbsName;

                StripNetworkArtifactsBeforeActivation(
                    orbsRootObject.transform);

                orbsRootObject.SetActive(
                    true);

                orbsRootTransform =
                    orbsRootObject.transform;

                NadaRigTransforms.ResetLocalTransform(
                    orbsRootTransform);

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [OrbitalsOrbsBranchCreated] " +
                    $"owner='{ownerNameForLogs}' " +
                    $"parent='{instanceRootTransform.name}' " +
                    $"root='{orbsRootTransform.name}' " +
                    $"rootId={orbsRootTransform.GetInstanceID()}");
            }

            FinalizeLocalOrbsBranch(
                orbsRootTransform);

            Transform headVisualTransform =
                ResolveOrbsHeadVisualTransform(
                    orbsRootTransform);

            Transform rigRootTransform =
                EnsureLocalOrbitalsRig(
                    instanceRootTransform,
                    ownerNameForLogs);

            if (rigRootTransform == null ||
                headVisualTransform == null)
            {
                return default;
            }

            Transform motionRootTransform =
                EnsureOrbitalsMotionRoot(
                    rigRootTransform,
                    Plugin.OrbitalsOrbsMotionRootName,
                    orbsRootTransform,
                    ownerNameForLogs);

            Transform poolRootTransform =
                EnsureOrbitalsPool(
                    rigRootTransform,
                    Plugin.OrbitalsOrbsPoolName,
                    headVisualTransform,
                    Plugin.OrbitalsOrbsName,
                    Plugin.MaxOrbitalsOrbsVisuals,
                    ownerNameForLogs);

            StripRuntimeArtifacts(
                motionRootTransform);

            StripRuntimeArtifacts(
                orbsRootTransform);

            StripRuntimeArtifacts(
                headVisualTransform);

            StripRuntimeArtifacts(
                poolRootTransform);

            return new OrbitalsOrbsInstanceStructure(
                orbsRootTransform,
                rigRootTransform,
                motionRootTransform,
                headVisualTransform,
                poolRootTransform);
        }

        internal static Transform
            EnsureLegacyOrbitalsRigWithoutOrbs(
                Transform localWeaponRootTransform,
                global::ItemDrop.ItemData itemData,
                string ownerNameForLogs)
        {
            if (!NadaRigCache.CacheReady ||
                NadaRigCache.DemisterTemplateInactive == null ||
                localWeaponRootTransform == null)
            {
                return null;
            }

            Transform localEffectsRootTransform =
                NadaRigPaths.FindLocalEffectsRoot(
                    localWeaponRootTransform);

            if (localEffectsRootTransform == null)
                return null;

            Transform orbitalsRootTransform =
                NadaRigPaths.FindDirectChild(
                    localEffectsRootTransform,
                    Plugin.OrbitalsName);

            if (orbitalsRootTransform == null)
            {
                orbitalsRootTransform =
                    NadaRigTransforms.EnsureChild(
                        localEffectsRootTransform,
                        Plugin.OrbitalsName);
            }

            if (orbitalsRootTransform == null)
                return null;

            EnsureLegacyOrbitalsChildrenWithoutDirectOrbs(
                orbitalsRootTransform,
                ownerNameForLogs);

            EnsureLocalCoresBranch(
                localWeaponRootTransform,
                ownerNameForLogs);

            Transform orbitalsRigRootTransform =
                EnsureLocalOrbitalsRig(
                    orbitalsRootTransform,
                    ownerNameForLogs);

            if (orbitalsRigRootTransform == null)
                return null;

            RemoveLegacyOrbsRuntime(
                orbitalsRootTransform,
                orbitalsRigRootTransform,
                ownerNameForLogs);

            EnsureOrbitalsFamilyMotion(
                orbitalsRigRootTransform,
                orbitalsRootTransform,
                itemData,
                ownerNameForLogs);

            return orbitalsRootTransform;
        }

        internal static Transform
            EnsureLegacyOrbitalsRigWithoutOrbs(
                Transform localWeaponRootTransform,
                VfxState state,
                string ownerNameForLogs)
        {
            if (!NadaRigCache.CacheReady ||
                NadaRigCache.DemisterTemplateInactive == null ||
                localWeaponRootTransform == null)
            {
                return null;
            }

            Transform localEffectsRootTransform =
                NadaRigPaths.FindLocalEffectsRoot(
                    localWeaponRootTransform);

            if (localEffectsRootTransform == null)
                return null;

            Transform orbitalsRootTransform =
                NadaRigPaths.FindDirectChild(
                    localEffectsRootTransform,
                    Plugin.OrbitalsName);

            if (orbitalsRootTransform == null)
            {
                orbitalsRootTransform =
                    NadaRigTransforms.EnsureChild(
                        localEffectsRootTransform,
                        Plugin.OrbitalsName);
            }

            if (orbitalsRootTransform == null)
                return null;

            EnsureLegacyOrbitalsChildrenWithoutDirectOrbs(
                orbitalsRootTransform,
                ownerNameForLogs);

            EnsureLocalCoresBranch(
                localWeaponRootTransform,
                ownerNameForLogs);

            Transform orbitalsRigRootTransform =
                EnsureLocalOrbitalsRig(
                    orbitalsRootTransform,
                    ownerNameForLogs);

            if (orbitalsRigRootTransform == null)
                return null;

            RemoveLegacyOrbsRuntime(
                orbitalsRootTransform,
                orbitalsRigRootTransform,
                ownerNameForLogs);

            EnsureOrbitalsFamilyMotion(
                orbitalsRigRootTransform,
                orbitalsRootTransform,
                state,
                ownerNameForLogs);

            return orbitalsRootTransform;
        }

        internal static Transform EnsureLocalOrbsBranch(
            Transform localWeaponRootTransform,
            string ownerNameForLogs)
        {
            if (!NadaRigCache.CacheReady)
                return null;

            if (NadaRigCache.DemisterTemplateInactive == null)
                return null;

            if (localWeaponRootTransform == null)
                return null;

            Transform localEffectsRootTransform =
                NadaRigPaths.FindLocalEffectsRoot(
                    localWeaponRootTransform);

            if (localEffectsRootTransform == null)
                return null;

            Transform orbitalsRootTransform =
                NadaRigPaths.FindDirectChild(
                    localEffectsRootTransform,
                    Plugin.OrbitalsName);

            if (orbitalsRootTransform == null)
            {
                orbitalsRootTransform =
                    NadaRigTransforms.EnsureChild(
                        localEffectsRootTransform,
                        Plugin.OrbitalsName);
            }

            Transform orbsRootTransform =
                NadaRigPaths.FindDirectChild(
                    orbitalsRootTransform,
                    Plugin.OrbitalsOrbsName);

            if (orbsRootTransform == null)
            {
                GameObject orbsRootObject =
                    Object.Instantiate(
                        NadaRigCache.DemisterTemplateInactive,
                        orbitalsRootTransform,
                        false);

                orbsRootObject.name =
                    Plugin.OrbitalsOrbsName;

                StripNetworkArtifactsBeforeActivation(
                    orbsRootObject.transform);

                orbsRootObject.SetActive(
                    true);

                orbsRootTransform =
                    orbsRootObject.transform;

                NadaRigTransforms.ResetLocalTransform(
                    orbsRootTransform);
            }

            EnsureLocalOrbitalsChildren(
                orbitalsRootTransform,
                orbsRootTransform,
                ownerNameForLogs);

            return orbsRootTransform;
        }

        internal static Transform EnsureLocalCoresBranch(
            Transform localWeaponRootTransform,
            string ownerNameForLogs)
        {
            if (!NadaRigCache.CacheReady)
                return null;

            if (NadaRigCache.CoresTemplateInactive == null)
                return null;

            if (localWeaponRootTransform == null)
                return null;

            Transform localEffectsRootTransform =
                NadaRigPaths.FindLocalEffectsRoot(
                    localWeaponRootTransform);

            if (localEffectsRootTransform == null)
                return null;

            Transform orbitalsRootTransform =
                NadaRigPaths.FindDirectChild(
                    localEffectsRootTransform,
                    Plugin.OrbitalsName);

            if (orbitalsRootTransform == null)
            {
                orbitalsRootTransform =
                    NadaRigTransforms.EnsureChild(
                        localEffectsRootTransform,
                        Plugin.OrbitalsName);
            }

            Transform coresRootTransform =
                NadaRigPaths.FindDirectChild(
                    orbitalsRootTransform,
                    Plugin.OrbitalsCoresName);

            if (coresRootTransform != null)
            {
                coresRootTransform.gameObject.SetActive(
                    true);

                StripRuntimeArtifacts(
                    coresRootTransform);

                return coresRootTransform;
            }

            GameObject coresObject =
                Object.Instantiate(
                    NadaRigCache.CoresTemplateInactive,
                    orbitalsRootTransform,
                    false);

            coresObject.name =
                Plugin.OrbitalsCoresName;

            coresObject.SetActive(
                true);

            coresRootTransform =
                coresObject.transform;

            NadaRigTransforms.ResetLocalTransform(
                coresRootTransform);

            StripRuntimeArtifacts(
                coresRootTransform);

            return coresRootTransform;
        }

        internal static void FinalizeLocalOrbsBranch(
            Transform localOrbsRootTransform)
        {
            if (localOrbsRootTransform == null)
                return;

            Transform localOrbVisualTransform =
                EnsureLocalOrbVisualChild(
                    localOrbsRootTransform);

            OrganizeLocalOrbsBranch(
                localOrbsRootTransform);

            NadaRigTransforms.NormalizeParticleSpacesUnder(
                localOrbsRootTransform,
                ParticleSystemSimulationSpace.Local);

            if (localOrbVisualTransform != null)
            {
                NadaRigTransforms.NormalizeParticleSpacesUnder(
                    localOrbVisualTransform,
                    ParticleSystemSimulationSpace.Local);
            }
        }

        internal static Transform EnsureLocalOrbitalsRig(
            Transform orbitalsRootTransform,
            string ownerNameForLogs)
        {
            if (orbitalsRootTransform == null)
                return null;

            Transform orbitalsRigTransform =
                NadaRigPaths.FindDirectChild(
                    orbitalsRootTransform,
                    Plugin.OrbitalsRigRootName);

            if (orbitalsRigTransform == null)
            {
                GameObject orbitalsRigObject =
                    new GameObject(
                        Plugin.OrbitalsRigRootName);

                orbitalsRigTransform =
                    orbitalsRigObject.transform;

                orbitalsRigTransform.SetParent(
                    orbitalsRootTransform,
                    false);

                NadaRigTransforms.ResetLocalTransform(
                    orbitalsRigTransform);
            }

            NadaRigTransforms.EnsureChild(
                orbitalsRigTransform,
                Plugin.OrbitalsMotionRootsName);

            NadaRigTransforms.EnsureChild(
                orbitalsRigTransform,
                Plugin.OrbitalsPoolsRootName);

            return orbitalsRigTransform;
        }

        internal static void EnsureOrbitalsFamilyMotion(
            Transform orbitalsRigTransform,
            Transform orbitalsRootTransform,
            global::ItemDrop.ItemData itemData,
            string ownerNameForLogs)
        {
            if (orbitalsRigTransform == null ||
                orbitalsRootTransform == null)
            {
                return;
            }

            Transform liveOrbsTransform =
                NadaRigPaths.FindDirectChild(
                    orbitalsRootTransform,
                    Plugin.OrbitalsOrbsName);

            Transform liveFlamesTransform =
                NadaRigPaths.FindDirectChild(
                    orbitalsRootTransform,
                    Plugin.OrbitalsFlamesName);

            Transform liveEmbersTransform =
                NadaRigPaths.FindDirectChild(
                    orbitalsRootTransform,
                    Plugin.OrbitalsEmbersName);

            Transform liveCoresTransform =
                NadaRigPaths.FindDirectChild(
                    orbitalsRootTransform,
                    Plugin.OrbitalsCoresName);

            Transform liveOrbsHeadVisualTransform =
                ResolveOrbsHeadVisualTransform(
                    liveOrbsTransform);

            Transform orbsMotionRootTransform =
                EnsureOrbitalsMotionRoot(
                    orbitalsRigTransform,
                    Plugin.OrbitalsOrbsMotionRootName,
                    liveOrbsTransform,
                    ownerNameForLogs);

            Transform flamesMotionRootTransform =
                EnsureOrbitalsMotionRoot(
                    orbitalsRigTransform,
                    Plugin.OrbitalsFlamesMotionRootName,
                    liveFlamesTransform,
                    ownerNameForLogs);

            Transform embersMotionRootTransform =
                EnsureOrbitalsMotionRoot(
                    orbitalsRigTransform,
                    Plugin.OrbitalsEmbersMotionRootName,
                    liveEmbersTransform,
                    ownerNameForLogs);

            Transform coresMotionRootTransform =
                EnsureOrbitalsMotionRoot(
                    orbitalsRigTransform,
                    Plugin.OrbitalsCoresMotionRootName,
                    liveCoresTransform,
                    ownerNameForLogs);

            Transform orbsPoolRootTransform =
                EnsureOrbitalsPool(
                    orbitalsRigTransform,
                    Plugin.OrbitalsOrbsPoolName,
                    liveOrbsHeadVisualTransform,
                    Plugin.OrbitalsOrbsName,
                    Plugin.MaxOrbitalsOrbsVisuals,
                    ownerNameForLogs);

            Transform flamesPoolRootTransform =
                EnsureOrbitalsPool(
                    orbitalsRigTransform,
                    Plugin.OrbitalsFlamesPoolName,
                    liveFlamesTransform,
                    Plugin.OrbitalsFlamesName,
                    Plugin.MaxOrbitalsFlameVisuals,
                    ownerNameForLogs);

            Transform embersPoolRootTransform =
                EnsureOrbitalsPool(
                    orbitalsRigTransform,
                    Plugin.OrbitalsEmbersPoolName,
                    liveEmbersTransform,
                    Plugin.OrbitalsEmbersName,
                    Plugin.MaxOrbitalsEmberVisuals,
                    ownerNameForLogs);

            Transform coresPoolRootTransform =
                EnsureOrbitalsPool(
                    orbitalsRigTransform,
                    Plugin.OrbitalsCoresPoolName,
                    liveCoresTransform,
                    Plugin.OrbitalsCoresName,
                    Plugin.MaxOrbitalsCoreVisuals,
                    ownerNameForLogs);

            StripRuntimeArtifacts(
                orbsMotionRootTransform);

            StripRuntimeArtifacts(
                flamesMotionRootTransform);

            StripRuntimeArtifacts(
                embersMotionRootTransform);

            StripRuntimeArtifacts(
                coresMotionRootTransform);

            StripRuntimeArtifacts(
                liveOrbsTransform);

            StripRuntimeArtifacts(
                liveOrbsHeadVisualTransform);

            StripRuntimeArtifacts(
                liveFlamesTransform);

            StripRuntimeArtifacts(
                liveEmbersTransform);

            StripRuntimeArtifacts(
                liveCoresTransform);

            StripRuntimeArtifacts(
                orbsPoolRootTransform);

            StripRuntimeArtifacts(
                flamesPoolRootTransform);

            StripRuntimeArtifacts(
                embersPoolRootTransform);

            StripRuntimeArtifacts(
                coresPoolRootTransform);

            if (orbsMotionRootTransform != null &&
                liveOrbsHeadVisualTransform != null &&
                orbsPoolRootTransform != null)
            {
                NadaMotionBinder.BindOrbitalsMotion(
                    orbsMotionRootTransform,
                    NadaOrbitalsFamily.Orbs,
                    itemData,
                    liveOrbsHeadVisualTransform,
                    orbsPoolRootTransform);
            }

            if (flamesMotionRootTransform != null &&
                liveFlamesTransform != null &&
                flamesPoolRootTransform != null)
            {
                NadaMotionBinder.BindOrbitalsMotion(
                    flamesMotionRootTransform,
                    NadaOrbitalsFamily.Flames,
                    itemData,
                    liveFlamesTransform,
                    flamesPoolRootTransform);
            }

            if (embersMotionRootTransform != null &&
                liveEmbersTransform != null &&
                embersPoolRootTransform != null)
            {
                NadaMotionBinder.BindOrbitalsMotion(
                    embersMotionRootTransform,
                    NadaOrbitalsFamily.Embers,
                    itemData,
                    liveEmbersTransform,
                    embersPoolRootTransform);
            }

            if (coresMotionRootTransform != null &&
                liveCoresTransform != null &&
                coresPoolRootTransform != null)
            {
                NadaMotionBinder.BindOrbitalsMotion(
                    coresMotionRootTransform,
                    NadaOrbitalsFamily.Cores,
                    itemData,
                    liveCoresTransform,
                    coresPoolRootTransform);
            }
        }

        internal static void EnsureOrbitalsFamilyMotion(
            Transform orbitalsRigTransform,
            Transform orbitalsRootTransform,
            VfxState state,
            string ownerNameForLogs)
        {
            if (orbitalsRigTransform == null ||
                orbitalsRootTransform == null)
            {
                return;
            }

            Transform liveOrbsTransform =
                NadaRigPaths.FindDirectChild(
                    orbitalsRootTransform,
                    Plugin.OrbitalsOrbsName);

            Transform liveFlamesTransform =
                NadaRigPaths.FindDirectChild(
                    orbitalsRootTransform,
                    Plugin.OrbitalsFlamesName);

            Transform liveEmbersTransform =
                NadaRigPaths.FindDirectChild(
                    orbitalsRootTransform,
                    Plugin.OrbitalsEmbersName);

            Transform liveCoresTransform =
                NadaRigPaths.FindDirectChild(
                    orbitalsRootTransform,
                    Plugin.OrbitalsCoresName);

            Transform liveOrbsHeadVisualTransform =
                ResolveOrbsHeadVisualTransform(
                    liveOrbsTransform);

            Transform orbsMotionRootTransform =
                EnsureOrbitalsMotionRoot(
                    orbitalsRigTransform,
                    Plugin.OrbitalsOrbsMotionRootName,
                    liveOrbsTransform,
                    ownerNameForLogs);

            Transform flamesMotionRootTransform =
                EnsureOrbitalsMotionRoot(
                    orbitalsRigTransform,
                    Plugin.OrbitalsFlamesMotionRootName,
                    liveFlamesTransform,
                    ownerNameForLogs);

            Transform embersMotionRootTransform =
                EnsureOrbitalsMotionRoot(
                    orbitalsRigTransform,
                    Plugin.OrbitalsEmbersMotionRootName,
                    liveEmbersTransform,
                    ownerNameForLogs);

            Transform coresMotionRootTransform =
                EnsureOrbitalsMotionRoot(
                    orbitalsRigTransform,
                    Plugin.OrbitalsCoresMotionRootName,
                    liveCoresTransform,
                    ownerNameForLogs);

            Transform orbsPoolRootTransform =
                EnsureOrbitalsPool(
                    orbitalsRigTransform,
                    Plugin.OrbitalsOrbsPoolName,
                    liveOrbsHeadVisualTransform,
                    Plugin.OrbitalsOrbsName,
                    Plugin.MaxOrbitalsOrbsVisuals,
                    ownerNameForLogs);

            Transform flamesPoolRootTransform =
                EnsureOrbitalsPool(
                    orbitalsRigTransform,
                    Plugin.OrbitalsFlamesPoolName,
                    liveFlamesTransform,
                    Plugin.OrbitalsFlamesName,
                    Plugin.MaxOrbitalsFlameVisuals,
                    ownerNameForLogs);

            Transform embersPoolRootTransform =
                EnsureOrbitalsPool(
                    orbitalsRigTransform,
                    Plugin.OrbitalsEmbersPoolName,
                    liveEmbersTransform,
                    Plugin.OrbitalsEmbersName,
                    Plugin.MaxOrbitalsEmberVisuals,
                    ownerNameForLogs);

            Transform coresPoolRootTransform =
                EnsureOrbitalsPool(
                    orbitalsRigTransform,
                    Plugin.OrbitalsCoresPoolName,
                    liveCoresTransform,
                    Plugin.OrbitalsCoresName,
                    Plugin.MaxOrbitalsCoreVisuals,
                    ownerNameForLogs);

            StripRuntimeArtifacts(
                orbsMotionRootTransform);

            StripRuntimeArtifacts(
                flamesMotionRootTransform);

            StripRuntimeArtifacts(
                embersMotionRootTransform);

            StripRuntimeArtifacts(
                coresMotionRootTransform);

            StripRuntimeArtifacts(
                liveOrbsTransform);

            StripRuntimeArtifacts(
                liveOrbsHeadVisualTransform);

            StripRuntimeArtifacts(
                liveFlamesTransform);

            StripRuntimeArtifacts(
                liveEmbersTransform);

            StripRuntimeArtifacts(
                liveCoresTransform);

            StripRuntimeArtifacts(
                orbsPoolRootTransform);

            StripRuntimeArtifacts(
                flamesPoolRootTransform);

            StripRuntimeArtifacts(
                embersPoolRootTransform);

            StripRuntimeArtifacts(
                coresPoolRootTransform);

            if (orbsMotionRootTransform != null &&
                liveOrbsHeadVisualTransform != null &&
                orbsPoolRootTransform != null)
            {
                NadaMotionBinder.BindOrbitalsMotion(
                    orbsMotionRootTransform,
                    NadaOrbitalsFamily.Orbs,
                    state,
                    liveOrbsHeadVisualTransform,
                    orbsPoolRootTransform);
            }

            if (flamesMotionRootTransform != null &&
                liveFlamesTransform != null &&
                flamesPoolRootTransform != null)
            {
                NadaMotionBinder.BindOrbitalsMotion(
                    flamesMotionRootTransform,
                    NadaOrbitalsFamily.Flames,
                    state,
                    liveFlamesTransform,
                    flamesPoolRootTransform);
            }

            if (embersMotionRootTransform != null &&
                liveEmbersTransform != null &&
                embersPoolRootTransform != null)
            {
                NadaMotionBinder.BindOrbitalsMotion(
                    embersMotionRootTransform,
                    NadaOrbitalsFamily.Embers,
                    state,
                    liveEmbersTransform,
                    embersPoolRootTransform);
            }

            if (coresMotionRootTransform != null &&
                liveCoresTransform != null &&
                coresPoolRootTransform != null)
            {
                NadaMotionBinder.BindOrbitalsMotion(
                    coresMotionRootTransform,
                    NadaOrbitalsFamily.Cores,
                    state,
                    liveCoresTransform,
                    coresPoolRootTransform);
            }
        }

        internal static Transform EnsureCompleteOrbitalsRig(
            Transform localWeaponRootTransform,
            global::ItemDrop.ItemData itemData,
            string ownerNameForLogs)
        {
            if (localWeaponRootTransform == null)
                return null;

            Transform localOrbsRootTransform =
                EnsureLocalOrbsBranch(
                    localWeaponRootTransform,
                    ownerNameForLogs);

            if (localOrbsRootTransform != null)
            {
                FinalizeLocalOrbsBranch(
                    localOrbsRootTransform);
            }

            EnsureLocalCoresBranch(
                localWeaponRootTransform,
                ownerNameForLogs);

            Transform orbitalsRootTransform =
                NadaRigPaths.FindLocalOrbitalsRoot(
                    localWeaponRootTransform);

            if (orbitalsRootTransform == null)
                return null;

            Transform orbitalsRigRootTransform =
                EnsureLocalOrbitalsRig(
                    orbitalsRootTransform,
                    ownerNameForLogs);

            if (orbitalsRigRootTransform != null)
            {
                EnsureOrbitalsFamilyMotion(
                    orbitalsRigRootTransform,
                    orbitalsRootTransform,
                    itemData,
                    ownerNameForLogs);
            }

            return orbitalsRootTransform;
        }

        internal static Transform EnsureCompleteOrbitalsRig(
            Transform localWeaponRootTransform,
            VfxState state,
            string ownerNameForLogs)
        {
            if (localWeaponRootTransform == null)
                return null;

            Transform localOrbsRootTransform =
                EnsureLocalOrbsBranch(
                    localWeaponRootTransform,
                    ownerNameForLogs);

            if (localOrbsRootTransform != null)
            {
                FinalizeLocalOrbsBranch(
                    localOrbsRootTransform);
            }

            EnsureLocalCoresBranch(
                localWeaponRootTransform,
                ownerNameForLogs);

            Transform orbitalsRootTransform =
                NadaRigPaths.FindLocalOrbitalsRoot(
                    localWeaponRootTransform);

            if (orbitalsRootTransform == null)
                return null;

            Transform orbitalsRigRootTransform =
                EnsureLocalOrbitalsRig(
                    orbitalsRootTransform,
                    ownerNameForLogs);

            if (orbitalsRigRootTransform != null)
            {
                EnsureOrbitalsFamilyMotion(
                    orbitalsRigRootTransform,
                    orbitalsRootTransform,
                    state,
                    ownerNameForLogs);
            }

            return orbitalsRootTransform;
        }

        internal static void EnsureLocalOrbitalsChildren(
            Transform orbitalsRootTransform,
            Transform localOrbsRootTransform,
            string ownerNameForLogs)
        {
            if (orbitalsRootTransform == null ||
                localOrbsRootTransform == null)
            {
                return;
            }

            Transform orbitalsEffectsRootTransform =
                NadaRigPaths.FindDirectChild(
                    localOrbsRootTransform,
                    "effects");

            if (orbitalsEffectsRootTransform == null)
                return;

            Transform flameRootTransform =
                NadaRigPaths.FindDirectChild(
                    orbitalsEffectsRootTransform,
                    "flame");

            if (flameRootTransform == null)
                return;

            CloneLocalOrbitalsChild(
                flameRootTransform,
                orbitalsRootTransform,
                "flames",
                Plugin.OrbitalsFlamesName);

            CloneLocalOrbitalsChild(
                flameRootTransform,
                orbitalsRootTransform,
                "embers",
                Plugin.OrbitalsEmbersName);
        }

        internal static Transform EnsureLocalOrbVisualChild(
            Transform localOrbsRootTransform)
        {
            if (localOrbsRootTransform == null)
                return null;

            Transform existingOrbVisualTransform =
                NadaRigPaths.FindDirectChild(
                    localOrbsRootTransform,
                    "Orb_00");

            if (existingOrbVisualTransform != null)
            {
                ReplaceOrbVisualMeshWithRuntimeSphere(
                    existingOrbVisualTransform);

                NadaRigTransforms.ForceUniformWorldScale(
                    existingOrbVisualTransform);

                NadaRigTransforms.DisableRootVisualContent(
                    localOrbsRootTransform);

                return existingOrbVisualTransform;
            }

            Transform sourceOrbVisualTransform =
                NadaRigPaths.FindDirectChild(
                    localOrbsRootTransform,
                    "demister_ball");

            if (sourceOrbVisualTransform != null)
            {
                sourceOrbVisualTransform.name =
                    "Orb_00";

                ReplaceOrbVisualMeshWithRuntimeSphere(
                    sourceOrbVisualTransform);

                NadaRigTransforms.ForceUniformWorldScale(
                    sourceOrbVisualTransform);

                NadaRigTransforms.DisableRootVisualContent(
                    localOrbsRootTransform);

                return sourceOrbVisualTransform;
            }

            return null;
        }

        internal static void OrganizeLocalOrbsBranch(
            Transform localOrbsRootTransform)
        {
            if (localOrbsRootTransform == null)
                return;

            Transform orbitalsEffectsRootTransform =
                NadaRigPaths.FindDirectChild(
                    localOrbsRootTransform,
                    "effects");

            if (orbitalsEffectsRootTransform == null)
                return;

            NadaRigTransforms.RemoveDirectChildIfPresent(
                orbitalsEffectsRootTransform,
                "SFX Start");

            NadaRigTransforms.RemoveDirectChildIfPresent(
                orbitalsEffectsRootTransform,
                "SFX");

            NadaRigTransforms.RemoveDirectChildIfPresent(
                orbitalsEffectsRootTransform,
                "Point light");

            NadaRigTransforms.RemoveDirectChildIfPresent(
                orbitalsEffectsRootTransform,
                "Particle System Force Field");

            NadaRigTransforms.RemoveDirectChildIfPresent(
                orbitalsEffectsRootTransform,
                "flame");

            if (orbitalsEffectsRootTransform.childCount == 0)
            {
                Object.Destroy(
                    orbitalsEffectsRootTransform.gameObject);
            }
        }

        private static void
            EnsureLegacyOrbitalsChildrenWithoutDirectOrbs(
                Transform orbitalsRootTransform,
                string ownerNameForLogs)
        {
            if (orbitalsRootTransform == null)
                return;

            Transform flamesRootTransform =
                NadaRigPaths.FindDirectChild(
                    orbitalsRootTransform,
                    Plugin.OrbitalsFlamesName);

            Transform embersRootTransform =
                NadaRigPaths.FindDirectChild(
                    orbitalsRootTransform,
                    Plugin.OrbitalsEmbersName);

            if (flamesRootTransform != null &&
                embersRootTransform != null)
            {
                return;
            }

            if (NadaRigCache.DemisterTemplateInactive == null)
                return;

            GameObject donorObject =
                Object.Instantiate(
                    NadaRigCache.DemisterTemplateInactive,
                    orbitalsRootTransform,
                    false);

            donorObject.name =
                "__NADA_Orbitals_Source";

            donorObject.SetActive(
                false);

            StripNetworkArtifactsBeforeActivation(
                donorObject.transform);

            EnsureLocalOrbitalsChildren(
                orbitalsRootTransform,
                donorObject.transform,
                ownerNameForLogs);

            donorObject.SetActive(
                false);

            donorObject.transform.SetParent(
                null,
                false);

            Object.Destroy(
                donorObject);
        }

        private static void RemoveLegacyOrbsRuntime(
            Transform orbitalsRootTransform,
            Transform orbitalsRigRootTransform,
            string ownerNameForLogs)
        {
            bool removedVisual = false;
            bool removedMotion = false;
            bool removedPool = false;

            if (orbitalsRootTransform != null)
            {
                Transform directOrbsRootTransform =
                    NadaRigPaths.FindDirectChild(
                        orbitalsRootTransform,
                        Plugin.OrbitalsOrbsName);

                removedVisual =
                    DisableDetachAndDestroy(
                        directOrbsRootTransform);
            }

            if (orbitalsRigRootTransform != null)
            {
                Transform motionRootsTransform =
                    NadaRigPaths.FindDirectChild(
                        orbitalsRigRootTransform,
                        Plugin.OrbitalsMotionRootsName);

                Transform orbsMotionRootTransform =
                    NadaRigPaths.FindDirectChild(
                        motionRootsTransform,
                        Plugin.OrbitalsOrbsMotionRootName);

                removedMotion =
                    DisableDetachAndDestroy(
                        orbsMotionRootTransform);

                Transform poolsRootTransform =
                    NadaRigPaths.FindDirectChild(
                        orbitalsRigRootTransform,
                        Plugin.OrbitalsPoolsRootName);

                Transform orbsPoolRootTransform =
                    NadaRigPaths.FindDirectChild(
                        poolsRootTransform,
                        Plugin.OrbitalsOrbsPoolName);

                removedPool =
                    DisableDetachAndDestroy(
                        orbsPoolRootTransform);
            }

            if (!removedVisual &&
                !removedMotion &&
                !removedPool)
            {
                return;
            }

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [OrbitalsOrbsLegacyOwnershipRemoved] " +
                $"owner='{ownerNameForLogs}' " +
                $"visual={removedVisual} " +
                $"motion={removedMotion} " +
                $"pool={removedPool}");
        }

        private static bool DisableDetachAndDestroy(
            Transform targetTransform)
        {
            if (targetTransform == null)
                return false;

            targetTransform.gameObject.SetActive(
                false);

            targetTransform.SetParent(
                null,
                false);

            Object.Destroy(
                targetTransform.gameObject);

            return true;
        }

        private static void CloneLocalOrbitalsChild(
            Transform sourceParentTransform,
            Transform targetParentTransform,
            string sourceChildName,
            string targetChildName)
        {
            if (sourceParentTransform == null ||
                targetParentTransform == null)
            {
                return;
            }

            Transform existingChildTransform =
                NadaRigPaths.FindDirectChild(
                    targetParentTransform,
                    targetChildName);

            if (existingChildTransform != null)
                return;

            Transform sourceChildTransform =
                NadaRigPaths.FindDirectChild(
                    sourceParentTransform,
                    sourceChildName);

            if (sourceChildTransform == null)
                return;

            GameObject clonedChildObject =
                Object.Instantiate(
                    sourceChildTransform.gameObject,
                    targetParentTransform,
                    false);

            clonedChildObject.name =
                targetChildName;

            clonedChildObject.SetActive(
                true);

            clonedChildObject.transform.localPosition =
                sourceChildTransform.localPosition;

            clonedChildObject.transform.localRotation =
                sourceChildTransform.localRotation;

            clonedChildObject.transform.localScale =
                sourceChildTransform.localScale;

            NadaRigTransforms.NormalizeParticleSpacesUnder(
                clonedChildObject.transform,
                ParticleSystemSimulationSpace.World);
        }

        internal static Transform EnsureOrbitalsMotionRoot(
            Transform orbitalsRigTransform,
            string motionRootName,
            Transform sourceVisualTransform,
            string ownerNameForLogs)
        {
            if (orbitalsRigTransform == null ||
                sourceVisualTransform == null)
            {
                return null;
            }

            Transform motionRootsTransform =
                NadaRigPaths.FindDirectChild(
                    orbitalsRigTransform,
                    Plugin.OrbitalsMotionRootsName);

            if (motionRootsTransform == null)
            {
                motionRootsTransform =
                    NadaRigTransforms.EnsureChild(
                        orbitalsRigTransform,
                        Plugin.OrbitalsMotionRootsName);
            }

            Transform motionRootTransform =
                NadaRigPaths.FindDirectChild(
                    motionRootsTransform,
                    motionRootName);

            bool createdMotionRoot =
                false;

            if (motionRootTransform == null)
            {
                GameObject motionRootObject =
                    new GameObject(
                        motionRootName);

                motionRootTransform =
                    motionRootObject.transform;

                motionRootTransform.SetParent(
                    motionRootsTransform,
                    false);

                createdMotionRoot =
                    true;
            }

            if (createdMotionRoot)
            {
                motionRootTransform.localPosition =
                    sourceVisualTransform.localPosition;

                motionRootTransform.localRotation =
                    sourceVisualTransform.localRotation;

                motionRootTransform.localScale =
                    Vector3.one;
            }

            return motionRootTransform;
        }

        private static Transform EnsureOrbitalsPool(
            Transform orbitalsRigTransform,
            string poolName,
            Transform sourceVisualTransform,
            string visualBaseName,
            int maxVisuals,
            string ownerNameForLogs)
        {
            if (orbitalsRigTransform == null ||
                sourceVisualTransform == null)
            {
                return null;
            }

            if (maxVisuals <= 0)
                return null;

            Transform poolsRootTransform =
                NadaRigPaths.FindDirectChild(
                    orbitalsRigTransform,
                    Plugin.OrbitalsPoolsRootName);

            if (poolsRootTransform == null)
            {
                poolsRootTransform =
                    NadaRigTransforms.EnsureChild(
                        orbitalsRigTransform,
                        Plugin.OrbitalsPoolsRootName);
            }

            Transform poolRootTransform =
                NadaRigPaths.FindDirectChild(
                    poolsRootTransform,
                    poolName);

            if (poolRootTransform == null)
            {
                GameObject poolRootObject =
                    new GameObject(
                        poolName);

                poolRootTransform =
                    poolRootObject.transform;

                poolRootTransform.SetParent(
                    poolsRootTransform,
                    false);
            }

            NadaRigTransforms.ResetLocalTransform(
                poolRootTransform);

            NadaRigTransforms.MatchWorldScale(
                poolRootTransform,
                Vector3.one);

            foreach (Transform pooledVisualTransform in
                     poolRootTransform)
            {
                if (pooledVisualTransform == null)
                    continue;

                StripRuntimeArtifacts(
                    pooledVisualTransform);
            }

            EnsurePooledVisualChildren(
                poolRootTransform,
                sourceVisualTransform,
                visualBaseName,
                maxVisuals,
                ownerNameForLogs);

            return poolRootTransform;
        }

        private static void EnsurePooledVisualChildren(
            Transform poolRootTransform,
            Transform sourceVisualTransform,
            string visualBaseName,
            int maxVisuals,
            string ownerNameForLogs)
        {
            if (poolRootTransform == null ||
                sourceVisualTransform == null)
            {
                return;
            }

            for (int visualIndex = 0;
                 visualIndex < maxVisuals;
                 visualIndex++)
            {
                string pooledVisualName =
                    $"{visualBaseName}_{visualIndex:00}";

                Transform existingPooledVisualTransform =
                    NadaRigPaths.FindDirectChild(
                        poolRootTransform,
                        pooledVisualName);

                if (existingPooledVisualTransform != null)
                {
                    // Existing pooled visuals are runtime-owned once the rig is built.
                    // Don't reset their pose here or we'll fight NadaOrbitalsMotion.
                    StripRuntimeArtifacts(
                        existingPooledVisualTransform);

                    continue;
                }

                GameObject pooledVisualObject =
                    Object.Instantiate(
                        sourceVisualTransform.gameObject,
                        poolRootTransform,
                        false);

                pooledVisualObject.name =
                    pooledVisualName;

                pooledVisualObject.SetActive(
                    false);

                NadaRigTransforms
                    .ResetLocalPosePreserveScaleFromSource(
                        pooledVisualObject.transform,
                        sourceVisualTransform);

                StripRuntimeArtifacts(
                    pooledVisualObject.transform);
            }
        }

        private static Transform ResolveOrbsHeadVisualTransform(
            Transform liveOrbsTransform)
        {
            if (liveOrbsTransform == null)
                return null;

            Transform headVisualTransform =
                NadaRigPaths.FindDirectChild(
                    liveOrbsTransform,
                    "Orb_00");

            return headVisualTransform != null
                ? headVisualTransform
                : liveOrbsTransform;
        }

        private static void StripRuntimeArtifacts(
            Transform rootTransform)
        {
            if (rootTransform == null)
                return;

            foreach (NadaTargetFollowMotion followMotion in
                     rootTransform
                         .GetComponentsInChildren<NadaTargetFollowMotion>(
                             true))
            {
                if (followMotion == null)
                    continue;

                Object.Destroy(
                    followMotion);
            }

            foreach (Collider collider in
                     rootTransform
                         .GetComponentsInChildren<Collider>(
                             true))
            {
                if (collider == null)
                    continue;

                Object.Destroy(
                    collider);
            }
        }

        private static void StripNetworkArtifactsBeforeActivation(
            Transform rootTransform)
        {
            if (rootTransform == null)
                return;

            foreach (ZSyncTransform zSyncTransform in
                     rootTransform
                         .GetComponentsInChildren<ZSyncTransform>(
                             true))
            {
                if (zSyncTransform == null)
                    continue;

                Object.DestroyImmediate(
                    zSyncTransform);
            }
        }

        private static void ReplaceOrbVisualMeshWithRuntimeSphere(
            Transform orbVisualTransform)
        {
            if (orbVisualTransform == null)
                return;

            MeshFilter meshFilter =
                orbVisualTransform.GetComponent<MeshFilter>();

            MeshRenderer meshRenderer =
                orbVisualTransform.GetComponent<MeshRenderer>();

            if (meshFilter == null ||
                meshRenderer == null)
            {
                return;
            }

            Mesh sphereMesh =
                GetRuntimeSphereMesh();

            if (sphereMesh == null)
                return;

            meshFilter.sharedMesh =
                sphereMesh;
        }

        private static Mesh GetRuntimeSphereMesh()
        {
            if (_runtimeSphereMesh != null)
                return _runtimeSphereMesh;

            GameObject sphereObject =
                GameObject.CreatePrimitive(
                    PrimitiveType.Sphere);

            MeshFilter meshFilter =
                sphereObject.GetComponent<MeshFilter>();

            if (meshFilter != null)
            {
                _runtimeSphereMesh =
                    meshFilter.sharedMesh;
            }

            Object.Destroy(
                sphereObject);

            return _runtimeSphereMesh;
        }
    }
}