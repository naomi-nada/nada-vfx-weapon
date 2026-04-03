using UnityEngine;
using NADA.VFX.Weapons.Targets;

namespace NADA.VFX.Runtime.Binding
{
    internal static class NadaRigAssembly
    {
        internal static Transform EnsureLocalFlameBranchAndAlign(Transform localWeaponRootTf, string ownerNameForLogs)
        {
            if (!NadaRigCache.CacheReady) return null;
            if (NadaRigCache.RefRigTemplateInactive == null) return null;
            if (localWeaponRootTf == null) return null;

            Transform effectsRoot = NadaRigPaths.FindLocalEffectsRoot(localWeaponRootTf);
            if (effectsRoot == null) return null;

            Transform outerFlamesTf = NadaRigPaths.FindDirectChild(effectsRoot, Plugin.OuterFlamesName);
            if (outerFlamesTf == null)
            {
                var go = Object.Instantiate(NadaRigCache.RefRigTemplateInactive, effectsRoot, false);
                go.name = Plugin.OuterFlamesName;
                go.SetActive(true);
                outerFlamesTf = go.transform;

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Added local outer flames branch under '{NadaWeaponTargets.FullPath(effectsRoot)}' " +
                    $"as '{Plugin.OuterFlamesName}' (owner='{ownerNameForLogs}').");
            }

            outerFlamesTf.localPosition = Plugin.RigLocalPosition;
            outerFlamesTf.localEulerAngles = Plugin.RigLocalEulerAngles;
            outerFlamesTf.localScale = Plugin.RigLocalScale;

            SplitFlameChildrenIntoEffects(effectsRoot, outerFlamesTf);

            NadaRigTransforms.NormalizeParticleSpacesUnder(outerFlamesTf, ParticleSystemSimulationSpace.Local);

            Transform flareTf = NadaRigPaths.FindDirectChild(effectsRoot, Plugin.FlareName);
            if (flareTf != null)
                NadaRigTransforms.NormalizeParticleSpacesUnder(flareTf, ParticleSystemSimulationSpace.Local);

            Transform innerTf = NadaRigPaths.FindDirectChild(effectsRoot, Plugin.InnerFlamesName);
            if (innerTf != null)
                NadaRigTransforms.NormalizeParticleSpacesUnder(innerTf, ParticleSystemSimulationSpace.Local);

            return outerFlamesTf;
        }

        internal static Transform EnsureLocalOrbsBranch(Transform localWeaponRootTf, string ownerNameForLogs)
        {
            if (!NadaRigCache.CacheReady) return null;
            if (NadaRigCache.DemisterTemplateInactive == null) return null;
            if (localWeaponRootTf == null) return null;

            Transform effectsRoot = NadaRigPaths.FindLocalEffectsRoot(localWeaponRootTf);
            if (effectsRoot == null) return null;

            Transform orbitalsRoot = NadaRigPaths.FindDirectChild(effectsRoot, Plugin.OrbitalsName);
            if (orbitalsRoot == null)
            {
                orbitalsRoot = NadaRigTransforms.EnsureChild(effectsRoot, Plugin.OrbitalsName);

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Added local Orbitals branch under '{NadaWeaponTargets.FullPath(effectsRoot)}' " +
                    $"as '{Plugin.OrbitalsName}' (owner='{ownerNameForLogs}').");
            }

            Transform orbsTf = NadaRigPaths.FindDirectChild(orbitalsRoot, Plugin.OrbitalsOrbsName);
            if (orbsTf == null)
            {
                var go = Object.Instantiate(NadaRigCache.DemisterTemplateInactive, orbitalsRoot, false);
                go.name = Plugin.OrbitalsOrbsName;
                go.SetActive(true);
                orbsTf = go.transform;

                NadaRigTransforms.ResetLocalTransform(orbsTf);

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Added local orb subtree under '{NadaWeaponTargets.FullPath(orbitalsRoot)}' " +
                    $"as '{Plugin.OrbitalsOrbsName}' (owner='{ownerNameForLogs}').");
            }

            var sync = orbsTf.GetComponent<ZSyncTransform>();
            if (sync != null)
            {
                Object.Destroy(sync);
                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Removed ZSyncTransform from '{NadaWeaponTargets.FullPath(orbsTf)}'.");
            }

            return orbsTf;
        }

        internal static Transform EnsureWorldOrbitalsBranch(
            Transform worldWeaponRootTf,
            Transform localOrbsTf,
            global::ItemDrop.ItemData itemData,
            string ownerNameForLogs)
        {
            if (worldWeaponRootTf == null || localOrbsTf == null) return null;

            Transform effectsRoot = NadaRigPaths.FindDirectChild(worldWeaponRootTf, Plugin.EffectsRootName);
            if (effectsRoot == null) return null;

            Transform orbitalsTf = NadaRigPaths.FindDirectChild(effectsRoot, Plugin.OrbitalsName);
            if (orbitalsTf == null)
            {
                var go = new GameObject(Plugin.OrbitalsName);
                orbitalsTf = go.transform;
                orbitalsTf.SetParent(effectsRoot, false);

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Added world Orbitals branch under '{NadaWeaponTargets.FullPath(effectsRoot)}' " +
                    $"as '{Plugin.OrbitalsName}' (owner='{ownerNameForLogs}').");
            }

            EnsureWorldMirage(effectsRoot, localOrbsTf);
            EnsureWorldSparks(effectsRoot, localOrbsTf);
            ExtractWorldOrbitalsChildren(localOrbsTf, orbitalsTf);

            NadaRigTransforms.NormalizeParticleSpacesUnder(orbitalsTf, ParticleSystemSimulationSpace.World);

            Transform mirageTf = NadaRigPaths.FindDirectChild(effectsRoot, Plugin.MirageName);
            if (mirageTf != null)
                NadaRigTransforms.NormalizeParticleSpacesUnder(mirageTf, ParticleSystemSimulationSpace.World);

            Transform sparksTf = NadaRigPaths.FindDirectChild(effectsRoot, Plugin.SparksName);
            if (sparksTf != null)
                NadaRigTransforms.NormalizeParticleSpacesUnder(sparksTf, ParticleSystemSimulationSpace.World);

            Transform orbitalsRigTf = NadaOrbitalsRigAssembly.EnsureWorldOrbitalsRig(effectsRoot, ownerNameForLogs);
            if (orbitalsRigTf != null)
                NadaOrbitalsRigAssembly.EnsureWorldOrbitalsVisualPools(orbitalsRigTf, orbitalsTf, itemData, ownerNameForLogs);

            return orbitalsTf;
        }

        internal static Transform EnsureWorldMirage(
            Transform worldEffectsRoot,
            Transform localOrbsTf)
        {
            if (worldEffectsRoot == null || localOrbsTf == null) return null;

            Transform existing = NadaRigPaths.FindDirectChild(worldEffectsRoot, Plugin.MirageName);
            if (existing != null) return existing;

            Transform effectsRoot = NadaRigPaths.FindDirectChild(localOrbsTf, "effects");
            if (effectsRoot == null) return null;

            Transform flameRoot = NadaRigPaths.FindDirectChild(effectsRoot, "flame");
            if (flameRoot == null) return null;

            Transform src = NadaRigPaths.FindDirectChild(flameRoot, "distortiion");
            if (src == null) return null;

            var clone = Object.Instantiate(src.gameObject, worldEffectsRoot, false);
            clone.name = Plugin.MirageName;
            clone.SetActive(true);

            clone.transform.localPosition = src.localPosition;
            clone.transform.localRotation = src.localRotation;
            clone.transform.localScale = src.localScale;

            return clone.transform;
        }

        internal static Transform EnsureWorldSparks(
            Transform worldEffectsRoot,
            Transform localOrbsTf)
        {
            if (worldEffectsRoot == null || localOrbsTf == null) return null;

            Transform existing = NadaRigPaths.FindDirectChild(worldEffectsRoot, Plugin.SparksName);
            if (existing != null) return existing;

            Transform effectsRoot = NadaRigPaths.FindDirectChild(localOrbsTf, "effects");
            if (effectsRoot == null) return null;

            Transform flameRoot = NadaRigPaths.FindDirectChild(effectsRoot, "flame");
            if (flameRoot == null) return null;

            Transform src = NadaRigPaths.FindDirectChild(flameRoot, "sparcs_front");
            if (src == null) return null;

            var clone = Object.Instantiate(src.gameObject, worldEffectsRoot, false);
            clone.name = Plugin.SparksName;
            clone.SetActive(true);

            clone.transform.localPosition = src.localPosition;
            clone.transform.localRotation = src.localRotation;
            clone.transform.localScale = src.localScale;

            return clone.transform;
        }

        internal static void ExtractWorldOrbitalsChildren(Transform localOrbsTf, Transform orbitalsTf)
        {
            if (localOrbsTf == null || orbitalsTf == null) return;

            Transform effectsRoot = NadaRigPaths.FindDirectChild(localOrbsTf, "effects");
            if (effectsRoot == null) return;

            Transform flameRoot = NadaRigPaths.FindDirectChild(effectsRoot, "flame");
            if (flameRoot == null) return;

            MoveOrCloneOrbitalsChild(flameRoot, orbitalsTf, "flames", Plugin.OrbitalsFlamesName);
            MoveOrCloneOrbitalsChild(flameRoot, orbitalsTf, "embers", Plugin.OrbitalsEmbersName);
        }

        private static void MoveOrCloneOrbitalsChild(
            Transform sourceParent,
            Transform targetParent,
            string sourceName,
            string targetName)
        {
            if (sourceParent == null || targetParent == null) return;

            Transform existing = NadaRigPaths.FindDirectChild(targetParent, targetName);
            if (existing != null) return;

            Transform src = NadaRigPaths.FindDirectChild(sourceParent, sourceName);
            if (src == null) return;

            var clone = Object.Instantiate(src.gameObject, targetParent, false);
            clone.name = targetName;
            clone.SetActive(true);

            NadaRigTransforms.ResetLocalTransform(clone.transform);
        }

        internal static void FinalizeLocalOrbsBranch(Transform localOrbsTf)
        {
            if (localOrbsTf == null) return;

            CollapseOrbMeshIntoParent(localOrbsTf);
            OrganizeLocalOrbsBranch(localOrbsTf);
            NadaRigTransforms.NormalizeParticleSpacesUnder(localOrbsTf, ParticleSystemSimulationSpace.Local);

            Transform orbVisualTf = EnsureLocalOrbVisualChild(localOrbsTf);
            if (orbVisualTf != null)
                NadaRigTransforms.NormalizeParticleSpacesUnder(orbVisualTf, ParticleSystemSimulationSpace.Local);
        }

        internal static Transform EnsureLocalOrbVisualChild(Transform localOrbsTf)
        {
            if (localOrbsTf == null) return null;

            Transform existing = NadaRigPaths.FindDirectChild(localOrbsTf, "Orb_00");
            if (existing != null)
            {
                NadaRigTransforms.DisableRootVisualContent(localOrbsTf);
                return existing;
            }

            MeshFilter rootMf = localOrbsTf.GetComponent<MeshFilter>();
            MeshRenderer rootMr = localOrbsTf.GetComponent<MeshRenderer>();

            if (rootMf == null || rootMr == null)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: Could not create Orb_00 because root orb visual mesh/renderer was missing at '{NadaWeaponTargets.FullPath(localOrbsTf)}'.");
                return null;
            }

            var go = new GameObject("Orb_00");
            Transform orbVisualTf = go.transform;
            orbVisualTf.SetParent(localOrbsTf, false);

            NadaRigTransforms.CopyMeshFilterIfMissing(localOrbsTf, orbVisualTf);
            NadaRigTransforms.CopyMeshRendererIfMissing(localOrbsTf, orbVisualTf);

            orbVisualTf.localPosition = Vector3.zero;
            orbVisualTf.localRotation = Quaternion.identity;
            orbVisualTf.localScale = Vector3.one;

            NadaRigTransforms.DisableRootVisualContent(localOrbsTf);

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: Created clean orb visual child 'Orb_00' under '{NadaWeaponTargets.FullPath(localOrbsTf)}'.");

            return orbVisualTf;
        }

        internal static void OrganizeLocalOrbsBranch(Transform orbsTf)
        {
            if (orbsTf == null) return;

            Transform effectsRoot = NadaRigPaths.FindDirectChild(orbsTf, "effects");
            if (effectsRoot == null) return;

            NadaRigTransforms.RemoveDirectChildIfPresent(effectsRoot, "SFX Start");
            NadaRigTransforms.RemoveDirectChildIfPresent(effectsRoot, "SFX");
            NadaRigTransforms.RemoveDirectChildIfPresent(effectsRoot, "Point light");
            NadaRigTransforms.RemoveDirectChildIfPresent(effectsRoot, "Particle System Force Field");
            NadaRigTransforms.RemoveDirectChildIfPresent(effectsRoot, "flame");

            if (effectsRoot.childCount == 0)
                Object.Destroy(effectsRoot.gameObject);
        }

        private static void SplitFlameChildrenIntoEffects(Transform effectsRoot, Transform outerFlamesTf)
        {
            if (effectsRoot == null || outerFlamesTf == null) return;

            Transform flareTf = NadaRigPaths.FindDescendantByName(outerFlamesTf, "flare");
            if (flareTf != null)
            {
                flareTf.name = Plugin.FlareName;

                if (flareTf.parent != effectsRoot)
                    flareTf.SetParent(effectsRoot, true);
            }

            Transform innerTf = NadaRigPaths.FindDescendantByName(outerFlamesTf, "fx_Torch_Basic");
            if (innerTf != null)
            {
                innerTf.name = Plugin.InnerFlamesName;

                if (innerTf.parent != effectsRoot)
                    innerTf.SetParent(effectsRoot, true);
            }
        }

        private static void CollapseOrbMeshIntoParent(Transform orbsTf)
        {
            if (orbsTf == null) return;

            Transform orbMeshTf = NadaRigPaths.FindDirectChild(orbsTf, "demister_ball");
            if (orbMeshTf == null) return;

            Vector3 oldLocalScale = orbMeshTf.localScale;

            NadaRigTransforms.CopyMeshFilterIfMissing(orbMeshTf, orbsTf);
            NadaRigTransforms.CopyMeshRendererIfMissing(orbMeshTf, orbsTf);

            while (orbMeshTf.childCount > 0)
            {
                Transform child = orbMeshTf.GetChild(0);
                child.SetParent(orbsTf, true);
            }

            orbsTf.localScale = oldLocalScale;

            Object.Destroy(orbMeshTf.gameObject);
        }
    }
}