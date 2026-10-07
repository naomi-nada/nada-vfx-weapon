using NADA.VFX.Weapon.Modules.Motion;
using UnityEngine;

namespace NADA.VFX.Weapon.Runtime.Structure
{
    /// <summary>
    /// Builds instance-owned structure for particle-based orbital families.
    /// Currently supports Flames and Embers.
    /// </summary>
    internal static class NadaOrbitalsParticleBlockStructure
    {
        internal readonly struct Structure
        {
            internal Transform VisualRootTransform { get; }
            internal Transform RigRootTransform { get; }
            internal Transform MotionRootTransform { get; }
            internal Transform HeadVisualTransform { get; }
            internal Transform PoolRootTransform { get; }

            internal bool IsValid =>
                VisualRootTransform != null &&
                RigRootTransform != null &&
                MotionRootTransform != null &&
                HeadVisualTransform != null &&
                PoolRootTransform != null;

            internal Structure(
                Transform visualRootTransform,
                Transform rigRootTransform,
                Transform motionRootTransform,
                Transform headVisualTransform,
                Transform poolRootTransform)
            {
                VisualRootTransform =
                    visualRootTransform;

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

        internal static Structure Ensure(
            Transform instanceRootTransform,
            NadaOrbitalsFamily family,
            string ownerNameForLogs)
        {
            if (!NadaRigCache.CacheReady ||
                NadaRigCache.DemisterTemplateInactive == null ||
                instanceRootTransform == null ||
                !IsSupportedFamily(family))
            {
                return default;
            }

            string visualRootName =
                ResolveVisualRootName(
                    family);

            string motionRootName =
                ResolveMotionRootName(
                    family);

            string poolName =
                ResolvePoolName(
                    family);

            int maxVisuals =
                ResolveMaxVisuals(
                    family);

            Transform visualRootTransform =
                NadaRigPaths.FindDirectChild(
                    instanceRootTransform,
                    visualRootName);

            if (visualRootTransform == null)
            {
                visualRootTransform =
                    CreateVisualRoot(
                        instanceRootTransform,
                        family,
                        visualRootName,
                        ownerNameForLogs);
            }

            if (visualRootTransform == null)
                return default;

            Transform rigRootTransform =
                EnsureRigRoot(
                    instanceRootTransform);

            if (rigRootTransform == null)
                return default;

            Transform motionRootTransform =
                EnsureMotionRoot(
                    rigRootTransform,
                    motionRootName,
                    visualRootTransform);

            Transform poolRootTransform =
                EnsurePool(
                    rigRootTransform,
                    poolName,
                    visualRootTransform,
                    visualRootName,
                    maxVisuals);

            StripRuntimeArtifacts(
                visualRootTransform);

            StripRuntimeArtifacts(
                motionRootTransform);

            StripRuntimeArtifacts(
                poolRootTransform);

            return new Structure(
                visualRootTransform,
                rigRootTransform,
                motionRootTransform,
                visualRootTransform,
                poolRootTransform);
        }

        internal static void RemoveLegacyRuntime(
            Transform legacyOrbitalsRootTransform,
            Transform legacyOrbitalsRigRootTransform,
            NadaOrbitalsFamily family,
            string ownerNameForLogs)
        {
            if (!IsSupportedFamily(family))
                return;

            string visualRootName =
                ResolveVisualRootName(
                    family);

            string motionRootName =
                ResolveMotionRootName(
                    family);

            string poolName =
                ResolvePoolName(
                    family);

            bool removedVisual =
                false;

            bool removedMotion =
                false;

            bool removedPool =
                false;

            if (legacyOrbitalsRootTransform != null)
            {
                Transform visualRootTransform =
                    NadaRigPaths.FindDirectChild(
                        legacyOrbitalsRootTransform,
                        visualRootName);

                removedVisual =
                    DisableDetachAndDestroy(
                        visualRootTransform);
            }

            if (legacyOrbitalsRigRootTransform != null)
            {
                Transform motionRootsTransform =
                    NadaRigPaths.FindDirectChild(
                        legacyOrbitalsRigRootTransform,
                        Plugin.OrbitalsMotionRootsName);

                Transform motionRootTransform =
                    NadaRigPaths.FindDirectChild(
                        motionRootsTransform,
                        motionRootName);

                removedMotion =
                    DisableDetachAndDestroy(
                        motionRootTransform);

                Transform poolsRootTransform =
                    NadaRigPaths.FindDirectChild(
                        legacyOrbitalsRigRootTransform,
                        Plugin.OrbitalsPoolsRootName);

                Transform poolRootTransform =
                    NadaRigPaths.FindDirectChild(
                        poolsRootTransform,
                        poolName);

                removedPool =
                    DisableDetachAndDestroy(
                        poolRootTransform);
            }

            if (!removedVisual &&
                !removedMotion &&
                !removedPool)
            {
                return;
            }

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [OrbitalsParticleLegacyOwnershipRemoved] " +
                $"owner='{ownerNameForLogs}' " +
                $"family='{family}' " +
                $"visual={removedVisual} " +
                $"motion={removedMotion} " +
                $"pool={removedPool}");
        }

        private static Transform CreateVisualRoot(
            Transform instanceRootTransform,
            NadaOrbitalsFamily family,
            string visualRootName,
            string ownerNameForLogs)
        {
            GameObject donorObject =
                Object.Instantiate(
                    NadaRigCache.DemisterTemplateInactive,
                    instanceRootTransform,
                    false);

            donorObject.name =
                "__NADA_Orbitals_Particle_Source";

            donorObject.SetActive(
                false);

            StripNetworkArtifactsBeforeActivation(
                donorObject.transform);

            Transform effectsTransform =
                NadaRigPaths.FindDirectChild(
                    donorObject.transform,
                    "effects");

            Transform flameTransform =
                NadaRigPaths.FindDirectChild(
                    effectsTransform,
                    "flame");

            string sourceChildName =
                family ==
                    NadaOrbitalsFamily.Flames
                    ? "flames"
                    : "embers";

            Transform sourceTransform =
                NadaRigPaths.FindDirectChild(
                    flameTransform,
                    sourceChildName);

            if (sourceTransform == null)
            {
                donorObject.transform.SetParent(
                    null,
                    false);

                Object.Destroy(
                    donorObject);

                return null;
            }

            GameObject visualObject =
                Object.Instantiate(
                    sourceTransform.gameObject,
                    instanceRootTransform,
                    false);

            visualObject.name =
                visualRootName;

            visualObject.transform.localPosition =
                sourceTransform.localPosition;

            visualObject.transform.localRotation =
                sourceTransform.localRotation;

            visualObject.transform.localScale =
                sourceTransform.localScale;

            StripRuntimeArtifacts(
                visualObject.transform);

            NadaRigTransforms.NormalizeParticleSpacesUnder(
                visualObject.transform,
                ParticleSystemSimulationSpace.World);

            visualObject.SetActive(
                true);

            donorObject.transform.SetParent(
                null,
                false);

            Object.Destroy(
                donorObject);

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [OrbitalsParticleBlockBranchCreated] " +
                $"owner='{ownerNameForLogs}' " +
                $"family='{family}' " +
                $"parent='{instanceRootTransform.name}' " +
                $"root='{visualObject.name}' " +
                $"rootId={visualObject.transform.GetInstanceID()}");

            return visualObject.transform;
        }

        private static Transform EnsureRigRoot(
            Transform instanceRootTransform)
        {
            Transform rigRootTransform =
                NadaRigPaths.FindDirectChild(
                    instanceRootTransform,
                    Plugin.OrbitalsRigRootName);

            if (rigRootTransform == null)
            {
                GameObject rigObject =
                    new GameObject(
                        Plugin.OrbitalsRigRootName);

                rigRootTransform =
                    rigObject.transform;

                rigRootTransform.SetParent(
                    instanceRootTransform,
                    false);

                NadaRigTransforms.ResetLocalTransform(
                    rigRootTransform);
            }

            NadaRigTransforms.EnsureChild(
                rigRootTransform,
                Plugin.OrbitalsMotionRootsName);

            NadaRigTransforms.EnsureChild(
                rigRootTransform,
                Plugin.OrbitalsPoolsRootName);

            return rigRootTransform;
        }

        private static Transform EnsureMotionRoot(
            Transform rigRootTransform,
            string motionRootName,
            Transform sourceVisualTransform)
        {
            if (rigRootTransform == null ||
                sourceVisualTransform == null)
            {
                return null;
            }

            Transform motionRootsTransform =
                NadaRigPaths.FindDirectChild(
                    rigRootTransform,
                    Plugin.OrbitalsMotionRootsName);

            if (motionRootsTransform == null)
            {
                motionRootsTransform =
                    NadaRigTransforms.EnsureChild(
                        rigRootTransform,
                        Plugin.OrbitalsMotionRootsName);
            }

            Transform motionRootTransform =
                NadaRigPaths.FindDirectChild(
                    motionRootsTransform,
                    motionRootName);

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

                motionRootTransform.localPosition =
                    sourceVisualTransform.localPosition;

                motionRootTransform.localRotation =
                    sourceVisualTransform.localRotation;

                motionRootTransform.localScale =
                    Vector3.one;
            }

            return motionRootTransform;
        }

        private static Transform EnsurePool(
            Transform rigRootTransform,
            string poolName,
            Transform sourceVisualTransform,
            string visualBaseName,
            int maxVisuals)
        {
            if (rigRootTransform == null ||
                sourceVisualTransform == null ||
                maxVisuals <= 0)
            {
                return null;
            }

            Transform poolsRootTransform =
                NadaRigPaths.FindDirectChild(
                    rigRootTransform,
                    Plugin.OrbitalsPoolsRootName);

            if (poolsRootTransform == null)
            {
                poolsRootTransform =
                    NadaRigTransforms.EnsureChild(
                        rigRootTransform,
                        Plugin.OrbitalsPoolsRootName);
            }

            Transform poolRootTransform =
                NadaRigPaths.FindDirectChild(
                    poolsRootTransform,
                    poolName);

            if (poolRootTransform == null)
            {
                GameObject poolObject =
                    new GameObject(
                        poolName);

                poolRootTransform =
                    poolObject.transform;

                poolRootTransform.SetParent(
                    poolsRootTransform,
                    false);
            }

            NadaRigTransforms.ResetLocalTransform(
                poolRootTransform);

            NadaRigTransforms.MatchWorldScale(
                poolRootTransform,
                Vector3.one);

            for (int visualIndex = 0;
                 visualIndex < maxVisuals;
                 visualIndex++)
            {
                string pooledVisualName =
                    $"{visualBaseName}_{visualIndex:00}";

                Transform pooledVisualTransform =
                    NadaRigPaths.FindDirectChild(
                        poolRootTransform,
                        pooledVisualName);

                if (pooledVisualTransform != null)
                {
                    StripRuntimeArtifacts(
                        pooledVisualTransform);

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

            return poolRootTransform;
        }

        private static bool IsSupportedFamily(
            NadaOrbitalsFamily family)
        {
            return
                family == NadaOrbitalsFamily.Flames ||
                family == NadaOrbitalsFamily.Embers;
        }

        private static string ResolveVisualRootName(
            NadaOrbitalsFamily family)
        {
            return family ==
                   NadaOrbitalsFamily.Flames
                ? Plugin.OrbitalsFlamesName
                : Plugin.OrbitalsEmbersName;
        }

        private static string ResolveMotionRootName(
            NadaOrbitalsFamily family)
        {
            return family ==
                   NadaOrbitalsFamily.Flames
                ? Plugin.OrbitalsFlamesMotionRootName
                : Plugin.OrbitalsEmbersMotionRootName;
        }

        private static string ResolvePoolName(
            NadaOrbitalsFamily family)
        {
            return family ==
                   NadaOrbitalsFamily.Flames
                ? Plugin.OrbitalsFlamesPoolName
                : Plugin.OrbitalsEmbersPoolName;
        }

        private static int ResolveMaxVisuals(
            NadaOrbitalsFamily family)
        {
            return family ==
                   NadaOrbitalsFamily.Flames
                ? Plugin.MaxOrbitalsFlameVisuals
                : Plugin.MaxOrbitalsEmberVisuals;
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
                if (followMotion != null)
                {
                    Object.Destroy(
                        followMotion);
                }
            }

            foreach (Collider collider in
                     rootTransform
                         .GetComponentsInChildren<Collider>(
                             true))
            {
                if (collider != null)
                {
                    Object.Destroy(
                        collider);
                }
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
                if (zSyncTransform != null)
                {
                    Object.DestroyImmediate(
                        zSyncTransform);
                }
            }
        }
    }
}