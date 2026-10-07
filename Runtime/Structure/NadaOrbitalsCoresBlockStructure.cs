using NADA.VFX.Weapon.Modules.Motion;
using UnityEngine;

namespace NADA.VFX.Weapon.Runtime.Structure
{
    /// <summary>
    /// Builds the runtime-owned hierarchy for one Cores block instance.
    ///
    /// This deliberately does not know about ItemData, config, persistence,
    /// networking, or formation relationships.
    /// </summary>
    internal static class NadaOrbitalsCoresBlockStructure
    {
        internal readonly struct Structure
        {
            internal Transform CoresRootTransform { get; }
            internal Transform RigRootTransform { get; }
            internal Transform MotionRootTransform { get; }
            internal Transform HeadVisualTransform { get; }
            internal Transform PoolRootTransform { get; }

            internal bool IsValid =>
                CoresRootTransform != null &&
                RigRootTransform != null &&
                MotionRootTransform != null &&
                HeadVisualTransform != null &&
                PoolRootTransform != null;

            internal Structure(
                Transform coresRootTransform,
                Transform rigRootTransform,
                Transform motionRootTransform,
                Transform headVisualTransform,
                Transform poolRootTransform)
            {
                CoresRootTransform =
                    coresRootTransform;

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
            string ownerNameForLogs)
        {
            if (!NadaRigCache.CacheReady ||
                NadaRigCache.CoresTemplateInactive == null ||
                instanceRootTransform == null)
            {
                return default;
            }

            Transform coresRootTransform =
                NadaRigPaths.FindDirectChild(
                    instanceRootTransform,
                    Plugin.OrbitalsCoresName);

            if (coresRootTransform == null)
            {
                GameObject coresObject =
                    Object.Instantiate(
                        NadaRigCache.CoresTemplateInactive,
                        instanceRootTransform,
                        false);

                coresObject.name =
                    Plugin.OrbitalsCoresName;

                coresRootTransform =
                    coresObject.transform;

                NadaRigTransforms.ResetLocalTransform(
                    coresRootTransform);

                StripRuntimeArtifacts(
                    coresRootTransform);

                coresObject.SetActive(
                    true);

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [OrbitalsCoresBlockBranchCreated] " +
                    $"owner='{ownerNameForLogs}' " +
                    $"parent='{instanceRootTransform.name}' " +
                    $"root='{coresRootTransform.name}' " +
                    $"rootId={coresRootTransform.GetInstanceID()}");
            }
            else
            {
                StripRuntimeArtifacts(
                    coresRootTransform);

                coresRootTransform.gameObject.SetActive(
                    true);
            }

            Transform rigRootTransform =
                EnsureRigRoot(
                    instanceRootTransform);

            if (rigRootTransform == null)
                return default;

            Transform motionRootTransform =
                EnsureMotionRoot(
                    rigRootTransform,
                    coresRootTransform);

            Transform poolRootTransform =
                EnsurePool(
                    rigRootTransform,
                    coresRootTransform,
                    ownerNameForLogs);

            StripRuntimeArtifacts(
                motionRootTransform);

            StripRuntimeArtifacts(
                coresRootTransform);

            StripRuntimeArtifacts(
                poolRootTransform);

            return new Structure(
                coresRootTransform,
                rigRootTransform,
                motionRootTransform,
                coresRootTransform,
                poolRootTransform);
        }

        internal static void RemoveLegacyRuntime(
            Transform legacyOrbitalsRootTransform,
            Transform legacyOrbitalsRigRootTransform,
            string ownerNameForLogs)
        {
            bool removedVisual =
                false;

            bool removedMotion =
                false;

            bool removedPool =
                false;

            if (legacyOrbitalsRootTransform != null)
            {
                Transform coresRootTransform =
                    NadaRigPaths.FindDirectChild(
                        legacyOrbitalsRootTransform,
                        Plugin.OrbitalsCoresName);

                removedVisual =
                    DisableDetachAndDestroy(
                        coresRootTransform);
            }

            if (legacyOrbitalsRigRootTransform != null)
            {
                Transform motionRootsTransform =
                    NadaRigPaths.FindDirectChild(
                        legacyOrbitalsRigRootTransform,
                        Plugin.OrbitalsMotionRootsName);

                Transform coresMotionRootTransform =
                    NadaRigPaths.FindDirectChild(
                        motionRootsTransform,
                        Plugin.OrbitalsCoresMotionRootName);

                removedMotion =
                    DisableDetachAndDestroy(
                        coresMotionRootTransform);

                Transform poolsRootTransform =
                    NadaRigPaths.FindDirectChild(
                        legacyOrbitalsRigRootTransform,
                        Plugin.OrbitalsPoolsRootName);

                Transform coresPoolRootTransform =
                    NadaRigPaths.FindDirectChild(
                        poolsRootTransform,
                        Plugin.OrbitalsCoresPoolName);

                removedPool =
                    DisableDetachAndDestroy(
                        coresPoolRootTransform);
            }

            if (!removedVisual &&
                !removedMotion &&
                !removedPool)
            {
                return;
            }

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [OrbitalsCoresLegacyOwnershipRemoved] " +
                $"owner='{ownerNameForLogs}' " +
                $"visual={removedVisual} " +
                $"motion={removedMotion} " +
                $"pool={removedPool}");
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
                GameObject rigRootObject =
                    new GameObject(
                        Plugin.OrbitalsRigRootName);

                rigRootTransform =
                    rigRootObject.transform;

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
                    Plugin.OrbitalsCoresMotionRootName);

            if (motionRootTransform == null)
            {
                GameObject motionRootObject =
                    new GameObject(
                        Plugin.OrbitalsCoresMotionRootName);

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
            Transform sourceVisualTransform,
            string ownerNameForLogs)
        {
            if (rigRootTransform == null ||
                sourceVisualTransform == null ||
                Plugin.MaxOrbitalsCoreVisuals <= 0)
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
                    Plugin.OrbitalsCoresPoolName);

            if (poolRootTransform == null)
            {
                GameObject poolRootObject =
                    new GameObject(
                        Plugin.OrbitalsCoresPoolName);

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

            for (int visualIndex = 0;
                 visualIndex < Plugin.MaxOrbitalsCoreVisuals;
                 visualIndex++)
            {
                string pooledVisualName =
                    $"{Plugin.OrbitalsCoresName}_{visualIndex:00}";

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
    }
}