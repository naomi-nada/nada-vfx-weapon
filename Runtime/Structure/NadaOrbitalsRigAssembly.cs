using UnityEngine;
using NADA.VFX.Modules.Motion;
using NADA.VFX.Runtime.Binding;
using NADA.VFX.Weapons.Targets;

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

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Added Orbitals rig '{Plugin.OrbitalsRigRootName}' under '{NadaWeaponTargets.FullPath(orbitalsRootTransform)}' " +
                    $"(owner='{ownerNameForLogs}').");
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

            StripRuntimeMotionComponents(orbsMotionRootTransform);
            StripRuntimeMotionComponents(flamesMotionRootTransform);
            StripRuntimeMotionComponents(embersMotionRootTransform);

            StripRuntimeMotionComponents(liveOrbsTransform);
            StripRuntimeMotionComponents(liveOrbsHeadVisualTransform);
            StripRuntimeMotionComponents(liveFlamesTransform);
            StripRuntimeMotionComponents(liveEmbersTransform);

            StripRuntimeMotionComponents(orbsPoolRootTransform);
            StripRuntimeMotionComponents(flamesPoolRootTransform);
            StripRuntimeMotionComponents(embersPoolRootTransform);

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

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Bound unified Orbitals motion for Orbs on '{NadaWeaponTargets.FullPath(orbsMotionRootTransform)}' " +
                    $"using head='{NadaWeaponTargets.FullPath(liveOrbsHeadVisualTransform)}' and pool='{NadaWeaponTargets.FullPath(orbsPoolRootTransform)}'.");
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

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Bound unified Orbitals motion for Flames on '{NadaWeaponTargets.FullPath(flamesMotionRootTransform)}' " +
                    $"using live='{NadaWeaponTargets.FullPath(liveFlamesTransform)}' and pool='{NadaWeaponTargets.FullPath(flamesPoolRootTransform)}'.");
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

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Bound unified Orbitals motion for Embers on '{NadaWeaponTargets.FullPath(embersMotionRootTransform)}' " +
                    $"using live='{NadaWeaponTargets.FullPath(liveEmbersTransform)}' and pool='{NadaWeaponTargets.FullPath(embersPoolRootTransform)}'.");
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

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Added Orbitals motion roots '{Plugin.OrbitalsMotionRootsName}' under '{NadaWeaponTargets.FullPath(orbitalsRigTransform)}' " +
                    $"(owner='{ownerNameForLogs}').");
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

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Added Orbitals motion root '{motionRootName}' under '{NadaWeaponTargets.FullPath(motionRootsTransform)}' " +
                    $"(owner='{ownerNameForLogs}').");
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

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Added Orbitals pools root '{Plugin.OrbitalsPoolsRootName}' under '{NadaWeaponTargets.FullPath(orbitalsRigTransform)}' " +
                    $"(owner='{ownerNameForLogs}').");
            }

            Transform poolRootTransform =
                NadaRigPaths.FindDirectChild(poolsRootTransform, poolName);

            if (poolRootTransform == null)
            {
                var poolRootObject = new GameObject(poolName);
                poolRootTransform = poolRootObject.transform;
                poolRootTransform.SetParent(poolsRootTransform, false);

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Added Orbitals visual pool '{poolName}' under '{NadaWeaponTargets.FullPath(poolsRootTransform)}' " +
                    $"(owner='{ownerNameForLogs}').");
            }

            NadaRigTransforms.ResetLocalTransform(poolRootTransform);

            foreach (Transform pooledVisualTransform in poolRootTransform)
            {
                if (pooledVisualTransform == null)
                    continue;

                StripRuntimeMotionComponents(pooledVisualTransform);
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
                    continue;

                var pooledVisualObject =
                    Object.Instantiate(sourceVisualTransform.gameObject, poolRootTransform, false);

                pooledVisualObject.name = pooledVisualName;
                pooledVisualObject.SetActive(false);

                NadaRigTransforms.ResetLocalPosePreserveScaleFromSource(
                    pooledVisualObject.transform,
                    sourceVisualTransform);

                StripRuntimeMotionComponents(pooledVisualObject.transform);
                NadaRigTransforms.NormalizeParticleSpacesUnder(
                    pooledVisualObject.transform,
                    ParticleSystemSimulationSpace.World);

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Added pooled Orbitals visual '{pooledVisualName}' under '{NadaWeaponTargets.FullPath(poolRootTransform)}' " +
                    $"(owner='{ownerNameForLogs}').");
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

        private static void StripRuntimeMotionComponents(Transform rootTransform)
        {
            if (rootTransform == null)
                return;

            foreach (var followMotion in rootTransform.GetComponentsInChildren<NadaTargetFollowMotion>(true))
            {
                if (followMotion == null)
                    continue;

                Object.Destroy(followMotion);
            }
        }
        
        private static float SafeDivide(float a, float b)
        {
            if (Mathf.Approximately(b, 0f))
                return a;
            return a / b;
        }
    }
}