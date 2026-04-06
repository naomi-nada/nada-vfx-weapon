using UnityEngine;
using NADA.VFX.Modules.Motion;
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

            NadaRigTransforms.EnsureChild(orbitalsRigTransform, Plugin.OrbitalsAnchorsRootName);
            NadaRigTransforms.EnsureChild(orbitalsRigTransform, Plugin.OrbitalsPoolsRootName);

            return orbitalsRigTransform;
        }

        internal static void EnsureOrbitalsLeadAndFollowerMotion(
            Transform orbitalsRigTransform,
            Transform orbitalsRootTransform,
            global::ItemDrop.ItemData itemData,
            string ownerNameForLogs)
        {
            if (orbitalsRigTransform == null || orbitalsRootTransform == null)
                return;

            Transform liveFlamesTransform =
                NadaRigPaths.FindDirectChild(orbitalsRootTransform, Plugin.OrbitalsFlamesName);

            Transform liveEmbersTransform =
                NadaRigPaths.FindDirectChild(orbitalsRootTransform, Plugin.OrbitalsEmbersName);

            Transform flamesLeadAnchorTransform = EnsureLeadAnchorAndLiveFollow(
                orbitalsRigTransform,
                liveFlamesTransform,
                OrbitalsVisualKind.Flames,
                Plugin.OrbitalsFlamesLeadAnchorName,
                itemData,
                ownerNameForLogs);

            Transform embersLeadAnchorTransform = EnsureLeadAnchorAndLiveFollow(
                orbitalsRigTransform,
                liveEmbersTransform,
                OrbitalsVisualKind.Embers,
                Plugin.OrbitalsEmbersLeadAnchorName,
                itemData,
                ownerNameForLogs);

            EnsureOrbitalsPool(
                orbitalsRigTransform,
                OrbitalsVisualKind.Flames,
                Plugin.OrbitalsFlamesPoolName,
                liveFlamesTransform,
                flamesLeadAnchorTransform,
                Plugin.OrbitalsFlamesName,
                Plugin.MaxOrbitalsFlameVisuals,
                itemData,
                ownerNameForLogs);

            EnsureOrbitalsPool(
                orbitalsRigTransform,
                OrbitalsVisualKind.Embers,
                Plugin.OrbitalsEmbersPoolName,
                liveEmbersTransform,
                embersLeadAnchorTransform,
                Plugin.OrbitalsEmbersName,
                Plugin.MaxOrbitalsEmberVisuals,
                itemData,
                ownerNameForLogs);
        }

        internal static Transform EnsureOrbitalsLeadAnchor(
            Transform orbitalsRigTransform,
            string leadAnchorName,
            Transform sourceVisualTransform,
            string ownerNameForLogs)
        {
            if (orbitalsRigTransform == null || sourceVisualTransform == null)
                return null;

            Transform anchorsRootTransform =
                NadaRigPaths.FindDirectChild(orbitalsRigTransform, Plugin.OrbitalsAnchorsRootName);

            if (anchorsRootTransform == null)
            {
                anchorsRootTransform =
                    NadaRigTransforms.EnsureChild(orbitalsRigTransform, Plugin.OrbitalsAnchorsRootName);

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Added Orbitals anchors root '{Plugin.OrbitalsAnchorsRootName}' under '{NadaWeaponTargets.FullPath(orbitalsRigTransform)}' " +
                    $"(owner='{ownerNameForLogs}').");
            }

            Transform leadAnchorTransform =
                NadaRigPaths.FindDirectChild(anchorsRootTransform, leadAnchorName);

            bool createdLeadAnchor = false;

            if (leadAnchorTransform == null)
            {
                var leadAnchorObject = new GameObject(leadAnchorName);
                leadAnchorTransform = leadAnchorObject.transform;
                leadAnchorTransform.SetParent(anchorsRootTransform, false);
                createdLeadAnchor = true;

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Added Orbitals lead anchor '{leadAnchorName}' under '{NadaWeaponTargets.FullPath(anchorsRootTransform)}' " +
                    $"(owner='{ownerNameForLogs}').");
            }

            if (createdLeadAnchor)
            {
                leadAnchorTransform.localPosition = sourceVisualTransform.localPosition;
                leadAnchorTransform.localRotation = sourceVisualTransform.localRotation;
                leadAnchorTransform.localScale = Vector3.one;
            }

            return leadAnchorTransform;
        }

        private static Transform EnsureLeadAnchorAndLiveFollow(
            Transform orbitalsRigTransform,
            Transform liveVisualTransform,
            OrbitalsVisualKind visualKind,
            string leadAnchorName,
            global::ItemDrop.ItemData itemData,
            string ownerNameForLogs)
        {
            if (liveVisualTransform == null)
                return null;

            Transform leadAnchorTransform = EnsureOrbitalsLeadAnchor(
                orbitalsRigTransform,
                leadAnchorName,
                liveVisualTransform,
                ownerNameForLogs);

            if (leadAnchorTransform == null)
                return null;

            var leadMotion = leadAnchorTransform.gameObject.GetComponent<NadaOrbitalsLeadMotion>();
            if (leadMotion == null)
            {
                leadMotion = leadAnchorTransform.gameObject.AddComponent<NadaOrbitalsLeadMotion>();

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Attached {visualKind} lead motion to '{NadaWeaponTargets.FullPath(leadAnchorTransform)}'.");
            }

            if (!leadMotion.IsInitialized)
            {
                leadMotion.Initialize(
                    visualKind,
                    leadAnchorTransform.localPosition,
                    leadAnchorTransform.localRotation,
                    itemData);
            }

            leadMotion.UpdateKindAndItemData(visualKind, itemData);

            var liveVisualFollow = liveVisualTransform.gameObject.GetComponent<NadaTargetFollowMotion>();
            if (liveVisualFollow == null)
                liveVisualFollow = liveVisualTransform.gameObject.AddComponent<NadaTargetFollowMotion>();

            liveVisualFollow.SetLocalOffset(Vector3.zero, Quaternion.identity);
            liveVisualFollow.SetTargetTransform(leadAnchorTransform);

            return leadAnchorTransform;
        }

        private static void EnsureOrbitalsPool(
            Transform orbitalsRigTransform,
            OrbitalsVisualKind visualKind,
            string poolName,
            Transform sourceVisualTransform,
            Transform leadAnchorTransform,
            string visualBaseName,
            int maxVisuals,
            global::ItemDrop.ItemData itemData,
            string ownerNameForLogs)
        {
            if (orbitalsRigTransform == null || sourceVisualTransform == null)
                return;

            if (maxVisuals <= 0)
                return;

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

            EnsureFollowerChainMotion(
                poolRootTransform,
                leadAnchorTransform,
                visualKind,
                itemData);

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
        }

        private static void EnsureFollowerChainMotion(
            Transform poolRootTransform,
            Transform leadAnchorTransform,
            OrbitalsVisualKind visualKind,
            global::ItemDrop.ItemData itemData)
        {
            if (poolRootTransform == null || leadAnchorTransform == null)
                return;

            var chainMotion = poolRootTransform.gameObject.GetComponent<NadaOrbitalsChainMotion>();
            if (chainMotion == null)
            {
                chainMotion = poolRootTransform.gameObject.AddComponent<NadaOrbitalsChainMotion>();

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Attached {visualKind} follower chain motion to '{NadaWeaponTargets.FullPath(poolRootTransform)}' " +
                    $"using lead '{NadaWeaponTargets.FullPath(leadAnchorTransform)}'.");
            }

            if (!chainMotion.IsInitialized)
            {
                chainMotion.Initialize(
                    visualKind,
                    poolRootTransform,
                    leadAnchorTransform,
                    itemData);
            }

            chainMotion.UpdateKindLeadAndItemData(
                visualKind,
                leadAnchorTransform,
                itemData);
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

                NadaRigTransforms.ResetLocalTransform(pooledVisualObject.transform);
                StripRuntimeMotionComponents(pooledVisualObject.transform);
                NadaRigTransforms.NormalizeParticleSpacesUnder(
                    pooledVisualObject.transform,
                    ParticleSystemSimulationSpace.World);

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Added pooled Orbitals visual '{pooledVisualName}' under '{NadaWeaponTargets.FullPath(poolRootTransform)}' " +
                    $"(owner='{ownerNameForLogs}').");
            }
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

            foreach (var leadMotion in rootTransform.GetComponentsInChildren<NadaOrbitalsLeadMotion>(true))
            {
                if (leadMotion == null)
                    continue;

                Object.Destroy(leadMotion);
            }

            foreach (var chainMotion in rootTransform.GetComponentsInChildren<NadaOrbitalsChainMotion>(true))
            {
                if (chainMotion == null)
                    continue;

                Object.Destroy(chainMotion);
            }
        }
    }
}