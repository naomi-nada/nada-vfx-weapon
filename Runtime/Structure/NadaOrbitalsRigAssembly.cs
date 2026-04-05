using UnityEngine;
using NADA.VFX.Modules.Motion;
using NADA.VFX.Weapons.Targets;

namespace NADA.VFX.Runtime.Binding
{
    internal static class NadaOrbitalsRigAssembly
    {
        internal static Transform EnsureLocalOrbitalsRig(Transform orbitalsRootTf, string ownerNameForLogs)
        {
            if (orbitalsRootTf == null) return null;

            Transform orbitalsRigTf = NadaRigPaths.FindDirectChild(orbitalsRootTf, Plugin.OrbitalsRigRootName);
            if (orbitalsRigTf == null)
            {
                var go = new GameObject(Plugin.OrbitalsRigRootName);
                orbitalsRigTf = go.transform;
                orbitalsRigTf.SetParent(orbitalsRootTf, false);
                NadaRigTransforms.ResetLocalTransform(orbitalsRigTf);

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Added Orbitals rig '{Plugin.OrbitalsRigRootName}' under '{NadaWeaponTargets.FullPath(orbitalsRootTf)}' " +
                    $"(owner='{ownerNameForLogs}').");
            }

            NadaRigTransforms.EnsureChild(orbitalsRigTf, Plugin.OrbitalsAnchorsRootName);
            NadaRigTransforms.EnsureChild(orbitalsRigTf, Plugin.OrbitalsPoolsRootName);

            return orbitalsRigTf;
        }

        internal static void EnsureOrbitalsVisualPools(
            Transform orbitalsRigTf,
            Transform orbitalsRootTf,
            global::ItemDrop.ItemData itemData,
            string ownerNameForLogs)
        {
            if (orbitalsRigTf == null || orbitalsRootTf == null) return;

            Transform liveFlamesTf = NadaRigPaths.FindDirectChild(orbitalsRootTf, Plugin.OrbitalsFlamesName);
            Transform liveEmbersTf = NadaRigPaths.FindDirectChild(orbitalsRootTf, Plugin.OrbitalsEmbersName);

            Transform flamesLeadAnchorTf = EnsureOrbitalsLeadAnchor(
                orbitalsRigTf,
                Plugin.OrbitalsFlamesLeadAnchorName,
                liveFlamesTf,
                ownerNameForLogs);

            if (flamesLeadAnchorTf != null && liveFlamesTf != null)
            {
                var leadMotion = flamesLeadAnchorTf.gameObject.GetComponent<NadaOrbitalsLeadMotion>();
                if (leadMotion == null)
                {
                    leadMotion = flamesLeadAnchorTf.gameObject.AddComponent<NadaOrbitalsLeadMotion>();

                    Plugin.Log.LogInfo(
                        $"{Plugin.ModName}: Attached Flames lead motion to '{NadaWeaponTargets.FullPath(flamesLeadAnchorTf)}'.");
                }

                if (!leadMotion.IsInitialized)
                {
                    leadMotion.Initialize(
                        OrbitalsVisualKind.Flames,
                        flamesLeadAnchorTf.localPosition,
                        flamesLeadAnchorTf.localRotation,
                        itemData);
                }

                leadMotion.SetKindAndItemData(OrbitalsVisualKind.Flames, itemData);

                var leadFollow = liveFlamesTf.gameObject.GetComponent<NadaWorldFollowMotion>();
                if (leadFollow == null)
                    leadFollow = liveFlamesTf.gameObject.AddComponent<NadaWorldFollowMotion>();

                leadFollow.SetOffset(Vector3.zero, Quaternion.identity);
                leadFollow.SetTarget(flamesLeadAnchorTf);
            }

            Transform embersLeadAnchorTf = EnsureOrbitalsLeadAnchor(
                orbitalsRigTf,
                Plugin.OrbitalsEmbersLeadAnchorName,
                liveEmbersTf,
                ownerNameForLogs);

            if (embersLeadAnchorTf != null && liveEmbersTf != null)
            {
                var leadMotion = embersLeadAnchorTf.gameObject.GetComponent<NadaOrbitalsLeadMotion>();
                if (leadMotion == null)
                {
                    leadMotion = embersLeadAnchorTf.gameObject.AddComponent<NadaOrbitalsLeadMotion>();

                    Plugin.Log.LogInfo(
                        $"{Plugin.ModName}: Attached Embers lead motion to '{NadaWeaponTargets.FullPath(embersLeadAnchorTf)}'.");
                }

                if (!leadMotion.IsInitialized)
                {
                    leadMotion.Initialize(
                        OrbitalsVisualKind.Embers,
                        embersLeadAnchorTf.localPosition,
                        embersLeadAnchorTf.localRotation,
                        itemData);
                }

                leadMotion.SetKindAndItemData(OrbitalsVisualKind.Embers, itemData);

                var leadFollow = liveEmbersTf.gameObject.GetComponent<NadaWorldFollowMotion>();
                if (leadFollow == null)
                    leadFollow = liveEmbersTf.gameObject.AddComponent<NadaWorldFollowMotion>();

                leadFollow.SetOffset(Vector3.zero, Quaternion.identity);
                leadFollow.SetTarget(embersLeadAnchorTf);
            }

            EnsureOrbitalsPool(
                orbitalsRigTf,
                Plugin.OrbitalsFlamesPoolName,
                liveFlamesTf,
                flamesLeadAnchorTf,
                Plugin.OrbitalsFlamesName,
                Plugin.MaxOrbitalsFlameVisuals,
                itemData,
                ownerNameForLogs);

            EnsureOrbitalsPool(
                orbitalsRigTf,
                Plugin.OrbitalsEmbersPoolName,
                liveEmbersTf,
                embersLeadAnchorTf,
                Plugin.OrbitalsEmbersName,
                Plugin.MaxOrbitalsEmberVisuals,
                itemData,
                ownerNameForLogs);
        }

        internal static Transform EnsureOrbitalsLeadAnchor(
            Transform orbitalsRigTf,
            string anchorName,
            Transform sourceTf,
            string ownerNameForLogs)
        {
            if (orbitalsRigTf == null || sourceTf == null) return null;

            Transform anchorsRootTf = NadaRigPaths.FindDirectChild(orbitalsRigTf, Plugin.OrbitalsAnchorsRootName);
            if (anchorsRootTf == null)
            {
                anchorsRootTf = NadaRigTransforms.EnsureChild(orbitalsRigTf, Plugin.OrbitalsAnchorsRootName);

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Added Orbitals anchors root '{Plugin.OrbitalsAnchorsRootName}' under '{NadaWeaponTargets.FullPath(orbitalsRigTf)}' " +
                    $"(owner='{ownerNameForLogs}').");
            }

            Transform anchorTf = NadaRigPaths.FindDirectChild(anchorsRootTf, anchorName);
            bool created = false;

            if (anchorTf == null)
            {
                var go = new GameObject(anchorName);
                anchorTf = go.transform;
                anchorTf.SetParent(anchorsRootTf, false);
                created = true;

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Added Orbitals lead anchor '{anchorName}' under '{NadaWeaponTargets.FullPath(anchorsRootTf)}' " +
                    $"(owner='{ownerNameForLogs}').");
            }

            if (created)
            {
                anchorTf.localPosition = sourceTf.localPosition;
                anchorTf.localRotation = sourceTf.localRotation;
                anchorTf.localScale = Vector3.one;
            }

            return anchorTf;
        }

        private static void EnsureOrbitalsPool(
            Transform orbitalsRigTf,
            string poolName,
            Transform sourceTf,
            Transform leadTransform,
            string visualBaseName,
            int maxVisuals,
            global::ItemDrop.ItemData itemData,
            string ownerNameForLogs)
        {
            if (orbitalsRigTf == null || sourceTf == null) return;
            if (maxVisuals <= 0) return;

            Transform poolsRootTf = NadaRigPaths.FindDirectChild(orbitalsRigTf, Plugin.OrbitalsPoolsRootName);
            if (poolsRootTf == null)
            {
                poolsRootTf = NadaRigTransforms.EnsureChild(orbitalsRigTf, Plugin.OrbitalsPoolsRootName);

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Added Orbitals pools root '{Plugin.OrbitalsPoolsRootName}' under '{NadaWeaponTargets.FullPath(orbitalsRigTf)}' " +
                    $"(owner='{ownerNameForLogs}').");
            }

            Transform poolRootTf = NadaRigPaths.FindDirectChild(poolsRootTf, poolName);
            if (poolRootTf == null)
            {
                var go = new GameObject(poolName);
                poolRootTf = go.transform;
                poolRootTf.SetParent(poolsRootTf, false);

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Added Orbitals visual pool '{poolName}' under '{NadaWeaponTargets.FullPath(poolsRootTf)}' " +
                    $"(owner='{ownerNameForLogs}').");
            }

            NadaRigTransforms.ResetLocalTransform(poolRootTf);

            if (poolName == Plugin.OrbitalsFlamesPoolName && leadTransform != null)
            {
                var motion = poolRootTf.gameObject.GetComponent<NadaOrbitalsChainMotion>();
                if (motion == null)
                {
                    motion = poolRootTf.gameObject.AddComponent<NadaOrbitalsChainMotion>();

                    Plugin.Log.LogInfo(
                        $"{Plugin.ModName}: Attached Flames follower chain motion to '{NadaWeaponTargets.FullPath(poolRootTf)}' " +
                        $"using lead '{NadaWeaponTargets.FullPath(leadTransform)}'.");
                }

                if (!motion.IsInitialized)
                    motion.Initialize(OrbitalsVisualKind.Flames, poolRootTf, leadTransform, itemData);

                motion.SetKindLeadAndItemData(OrbitalsVisualKind.Flames, leadTransform, itemData);
            }
            else if (poolName == Plugin.OrbitalsEmbersPoolName && leadTransform != null)
            {
                var motion = poolRootTf.gameObject.GetComponent<NadaOrbitalsChainMotion>();
                if (motion == null)
                {
                    motion = poolRootTf.gameObject.AddComponent<NadaOrbitalsChainMotion>();

                    Plugin.Log.LogInfo(
                        $"{Plugin.ModName}: Attached Embers follower chain motion to '{NadaWeaponTargets.FullPath(poolRootTf)}' " +
                        $"using lead '{NadaWeaponTargets.FullPath(leadTransform)}'.");
                }

                if (!motion.IsInitialized)
                    motion.Initialize(OrbitalsVisualKind.Embers, poolRootTf, leadTransform, itemData);

                motion.SetKindLeadAndItemData(OrbitalsVisualKind.Embers, leadTransform, itemData);
            }

            foreach (Transform child in poolRootTf)
            {
                if (child == null) continue;
                StripRuntimeMotionComponents(child);
            }

            for (int i = 0; i < maxVisuals; i++)
            {
                string visualName = $"{visualBaseName}_{i:00}";
                Transform existing = NadaRigPaths.FindDirectChild(poolRootTf, visualName);
                if (existing != null)
                    continue;

                var clone = Object.Instantiate(sourceTf.gameObject, poolRootTf, false);
                clone.name = visualName;
                clone.SetActive(false);

                NadaRigTransforms.ResetLocalTransform(clone.transform);
                StripRuntimeMotionComponents(clone.transform);
                NadaRigTransforms.NormalizeParticleSpacesUnder(clone.transform, ParticleSystemSimulationSpace.World);

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Added pooled Orbitals visual '{visualName}' under '{NadaWeaponTargets.FullPath(poolRootTf)}' " +
                    $"(owner='{ownerNameForLogs}').");
            }
        }

        private static void StripRuntimeMotionComponents(Transform root)
        {
            if (root == null) return;

            foreach (var follow in root.GetComponentsInChildren<NadaWorldFollowMotion>(true))
            {
                if (follow == null) continue;
                Object.Destroy(follow);
            }

            foreach (var lead in root.GetComponentsInChildren<NadaOrbitalsLeadMotion>(true))
            {
                if (lead == null) continue;
                Object.Destroy(lead);
            }

            foreach (var chain in root.GetComponentsInChildren<NadaOrbitalsChainMotion>(true))
            {
                if (chain == null) continue;
                Object.Destroy(chain);
            }
        }
    }
}