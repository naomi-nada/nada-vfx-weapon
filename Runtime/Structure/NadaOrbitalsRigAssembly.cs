using UnityEngine;
using NADA.VFX.Modules.Motion;
using NADA.VFX.Runtime.Binding;
using NADA.VFX.Weapons.Targets;
using NADA.VFX.Core.Debug;

namespace NADA.VFX.Runtime.Structure
{
    internal static class NadaOrbitalsRigAssembly
    {
        internal static Transform EnsureLocalOrbitalsRig(
            Transform orbitalsRootTransform,
            string ownerNameForLogs)
        {
            if (orbitalsRootTransform == null)
                return null;

            Transform orbitalsRigTransform =
                NadaRigPaths.FindDirectChild(orbitalsRootTransform, Plugin.OrbitalsRigRootName);

            if (orbitalsRigTransform == null)
            {
                var orbitalsRigObject = new GameObject(Plugin.OrbitalsRigRootName);
                orbitalsRigTransform = orbitalsRigObject.transform;
                orbitalsRigTransform.SetParent(orbitalsRootTransform, false);
                NadaRigTransforms.ResetLocalTransform(orbitalsRigTransform);

                NadaLogControl.Info(
                    $"orbitals-rig:{orbitalsRigTransform.GetInstanceID()}",
                    $"{Plugin.ModName}: Added Orbitals rig '{orbitalsRigTransform.name}' under '{orbitalsRigTransform.parent?.name}'."
                );
            }

            NadaRigTransforms.EnsureChild(orbitalsRigTransform, Plugin.OrbitalsMotionRootsName);
            NadaRigTransforms.EnsureChild(orbitalsRigTransform, Plugin.OrbitalsPoolsRootName);

            return orbitalsRigTransform;
        }

        internal static void EnsureOrbitalsFamilyMotion(
            Transform orbitalsRigTransform,
            Transform orbitalsRootTransform,
            global::ItemDrop.ItemData itemData,
            string ownerNameForLogs)
        {
            if (orbitalsRigTransform == null || orbitalsRootTransform == null)
                return;

            Transform liveOrbsTransform =
                NadaRigPaths.FindDirectChild(orbitalsRootTransform, Plugin.OrbitalsOrbsName);

            Transform liveFlamesTransform =
                NadaRigPaths.FindDirectChild(orbitalsRootTransform, Plugin.OrbitalsFlamesName);

            Transform liveEmbersTransform =
                NadaRigPaths.FindDirectChild(orbitalsRootTransform, Plugin.OrbitalsEmbersName);

            Transform liveOrbsHeadVisualTransform = ResolveOrbsHeadVisualTransform(liveOrbsTransform);

            Transform orbsMotionRootTransform = EnsureOrbitalsMotionRoot(
                orbitalsRigTransform,
                Plugin.OrbitalsOrbsMotionRootName,
                liveOrbsTransform,
                ownerNameForLogs);

            Transform flamesMotionRootTransform = EnsureOrbitalsMotionRoot(
                orbitalsRigTransform,
                Plugin.OrbitalsFlamesMotionRootName,
                liveFlamesTransform,
                ownerNameForLogs);

            Transform embersMotionRootTransform = EnsureOrbitalsMotionRoot(
                orbitalsRigTransform,
                Plugin.OrbitalsEmbersMotionRootName,
                liveEmbersTransform,
                ownerNameForLogs);

            Transform orbsPoolRootTransform = EnsureOrbitalsPool(
                orbitalsRigTransform,
                Plugin.OrbitalsOrbsPoolName,
                liveOrbsHeadVisualTransform,
                Plugin.OrbitalsOrbsName,
                Plugin.MaxOrbitalsOrbsVisuals,
                ownerNameForLogs);
            
            Transform flamesPoolRootTransform = EnsureOrbitalsPool(
                orbitalsRigTransform,
                Plugin.OrbitalsFlamesPoolName,
                liveFlamesTransform,
                Plugin.OrbitalsFlamesName,
                Plugin.MaxOrbitalsFlameVisuals,
                ownerNameForLogs);

            Transform embersPoolRootTransform = EnsureOrbitalsPool(
                orbitalsRigTransform,
                Plugin.OrbitalsEmbersPoolName,
                liveEmbersTransform,
                Plugin.OrbitalsEmbersName,
                Plugin.MaxOrbitalsEmberVisuals,
                ownerNameForLogs);

            StripRuntimeArtifacts(orbsMotionRootTransform);
            StripRuntimeArtifacts(flamesMotionRootTransform);
            StripRuntimeArtifacts(embersMotionRootTransform);

            StripRuntimeArtifacts(liveOrbsTransform);
            StripRuntimeArtifacts(liveOrbsHeadVisualTransform);
            StripRuntimeArtifacts(liveFlamesTransform);
            StripRuntimeArtifacts(liveEmbersTransform);

            StripRuntimeArtifacts(orbsPoolRootTransform);
            StripRuntimeArtifacts(flamesPoolRootTransform);
            StripRuntimeArtifacts(embersPoolRootTransform);

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
        }

        internal static Transform EnsureOrbitalsMotionRoot(
            Transform orbitalsRigTransform,
            string motionRootName,
            Transform sourceVisualTransform,
            string ownerNameForLogs)
        {
            if (orbitalsRigTransform == null || sourceVisualTransform == null)
                return null;

            Transform motionRootsTransform =
                NadaRigPaths.FindDirectChild(orbitalsRigTransform, Plugin.OrbitalsMotionRootsName);

            if (motionRootsTransform == null)
            {
                motionRootsTransform =
                    NadaRigTransforms.EnsureChild(orbitalsRigTransform, Plugin.OrbitalsMotionRootsName);

                NadaLogControl.Info(
                    $"orbitals-motion-roots:{orbitalsRigTransform.GetInstanceID()}",
                    $"{Plugin.ModName}: Added Orbitals motion roots '{Plugin.OrbitalsMotionRootsName}' under '{NadaWeaponTargets.FullPath(orbitalsRigTransform)}' (owner='{ownerNameForLogs}')."
                );
            }

            Transform motionRootTransform =
                NadaRigPaths.FindDirectChild(motionRootsTransform, motionRootName);

            bool createdMotionRoot = false;

            if (motionRootTransform == null)
            {
                var motionRootObject = new GameObject(motionRootName);
                motionRootTransform = motionRootObject.transform;
                motionRootTransform.SetParent(motionRootsTransform, false);
                createdMotionRoot = true;

                NadaLogControl.Info(
                    $"orbitals-motion-root:{motionRootName}:{motionRootsTransform.GetInstanceID()}",
                    $"{Plugin.ModName}: Added Orbitals motion root '{motionRootName}' under '{NadaWeaponTargets.FullPath(motionRootsTransform)}' (owner='{ownerNameForLogs}')."
                );
            }

            if (createdMotionRoot)
            {
                motionRootTransform.localPosition = sourceVisualTransform.localPosition;
                motionRootTransform.localRotation = sourceVisualTransform.localRotation;
                motionRootTransform.localScale = Vector3.one;
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
            if (orbitalsRigTransform == null || sourceVisualTransform == null)
                return null;

            if (maxVisuals <= 0)
                return null;

            Transform poolsRootTransform =
                NadaRigPaths.FindDirectChild(orbitalsRigTransform, Plugin.OrbitalsPoolsRootName);

            if (poolsRootTransform == null)
            {
                poolsRootTransform =
                    NadaRigTransforms.EnsureChild(orbitalsRigTransform, Plugin.OrbitalsPoolsRootName);

                NadaLogControl.Info(
                    $"orbitals-pools-root:{orbitalsRigTransform.GetInstanceID()}",
                    $"{Plugin.ModName}: Added Orbitals pools root '{Plugin.OrbitalsPoolsRootName}' under '{NadaWeaponTargets.FullPath(orbitalsRigTransform)}' (owner='{ownerNameForLogs}')."
                );
            }

            Transform poolRootTransform =
                NadaRigPaths.FindDirectChild(poolsRootTransform, poolName);

            if (poolRootTransform == null)
            {
                var poolRootObject = new GameObject(poolName);
                poolRootTransform = poolRootObject.transform;
                poolRootTransform.SetParent(poolsRootTransform, false);

                NadaLogControl.Info(
                    $"orbitals-visual-pool:{poolName}:{poolsRootTransform.GetInstanceID()}",
                    $"{Plugin.ModName}: Added Orbitals visual pool '{poolName}' under '{NadaWeaponTargets.FullPath(poolsRootTransform)}' (owner='{ownerNameForLogs}')."
                );
            }

            NadaRigTransforms.ResetLocalTransform(poolRootTransform);

            foreach (Transform pooledVisualTransform in poolRootTransform)
            {
                if (pooledVisualTransform == null)
                    continue;

                StripRuntimeArtifacts(pooledVisualTransform);
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
            if (poolRootTransform == null || sourceVisualTransform == null)
                return;

            for (int visualIndex = 0; visualIndex < maxVisuals; visualIndex++)
            {
                string pooledVisualName = $"{visualBaseName}_{visualIndex:00}";

                Transform existingPooledVisualTransform =
                    NadaRigPaths.FindDirectChild(poolRootTransform, pooledVisualName);

                if (existingPooledVisualTransform != null)
                {
                    StripRuntimeArtifacts(existingPooledVisualTransform);
                    continue;
                }

                var pooledVisualObject =
                    Object.Instantiate(sourceVisualTransform.gameObject, poolRootTransform, false);

                pooledVisualObject.name = pooledVisualName;
                pooledVisualObject.SetActive(false);

                NadaRigTransforms.ResetLocalPosePreserveScaleFromSource(
                    pooledVisualObject.transform,
                    sourceVisualTransform);

                StripRuntimeArtifacts(pooledVisualObject.transform);

                NadaRigTransforms.NormalizeParticleSpacesUnder(
                    pooledVisualObject.transform,
                    ParticleSystemSimulationSpace.World);
            }
        }

        private static Transform ResolveOrbsHeadVisualTransform(Transform liveOrbsTransform)
        {
            if (liveOrbsTransform == null)
                return null;

            Transform headVisualTransform =
                NadaRigPaths.FindDirectChild(liveOrbsTransform, "Orb_00");

            return headVisualTransform != null
                ? headVisualTransform
                : liveOrbsTransform;
        }

        private static void StripRuntimeArtifacts(Transform rootTransform)
        {
            if (rootTransform == null)
                return;

            foreach (var followMotion in rootTransform.GetComponentsInChildren<NadaTargetFollowMotion>(true))
            {
                if (followMotion == null)
                    continue;

                Object.Destroy(followMotion);
            }

            foreach (var collider in rootTransform.GetComponentsInChildren<Collider>(true))
            {
                if (collider == null)
                    continue;

                Object.Destroy(collider);
            }
        }
    }
}