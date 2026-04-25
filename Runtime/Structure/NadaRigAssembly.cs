using UnityEngine;
using NADA.VFX.Weapons.Targets;
using NADA.VFX.Core.Debug;

namespace NADA.VFX.Runtime.Structure
{
    internal static class NadaRigAssembly
    {
        internal static Transform EnsureLocalFlameBranch(
            Transform localWeaponRootTransform,
            string ownerNameForLogs)
        {
            if (!NadaRigCache.CacheReady)
                return null;

            if (NadaRigCache.RefRigTemplateInactive == null)
                return null;

            if (localWeaponRootTransform == null)
                return null;

            Transform localEffectsRootTransform =
                NadaRigPaths.FindLocalEffectsRoot(localWeaponRootTransform);

            if (localEffectsRootTransform == null)
                return null;

            Transform outerFlamesTransform =
                NadaRigPaths.FindDirectChild(localEffectsRootTransform, Plugin.OuterFlamesName);

            if (outerFlamesTransform == null)
            {
                var outerFlamesObject =
                    Object.Instantiate(NadaRigCache.RefRigTemplateInactive, localEffectsRootTransform, false);

                outerFlamesObject.name = Plugin.OuterFlamesName;
                outerFlamesObject.SetActive(true);
                outerFlamesTransform = outerFlamesObject.transform;
                
                NadaLogControl.Info(
                    $"outer-flames:{outerFlamesTransform.GetInstanceID()}",
                    $"{Plugin.ModName}: Added local outer flames branch under '{NadaWeaponTargets.FullPath(localEffectsRootTransform)}' " +
                    $"as '{Plugin.OuterFlamesName}' (owner='{ownerNameForLogs}').");
            }

            NadaRigTransforms.ResetLocalTransform(outerFlamesTransform);

            SplitFlameChildrenIntoEffects(localEffectsRootTransform, outerFlamesTransform);

            NadaRigTransforms.NormalizeParticleSpacesUnder(
                outerFlamesTransform,
                ParticleSystemSimulationSpace.Local);

            Transform flareTransform =
                NadaRigPaths.FindDirectChild(localEffectsRootTransform, Plugin.FlareName);

            if (flareTransform != null)
            {
                NadaRigTransforms.NormalizeParticleSpacesUnder(
                    flareTransform,
                    ParticleSystemSimulationSpace.Local);
            }

            Transform innerFlamesTransform =
                NadaRigPaths.FindDirectChild(localEffectsRootTransform, Plugin.InnerFlamesName);

            if (innerFlamesTransform != null)
            {
                NadaRigTransforms.NormalizeParticleSpacesUnder(
                    innerFlamesTransform,
                    ParticleSystemSimulationSpace.Local);
            }

            return outerFlamesTransform;
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
                NadaRigPaths.FindLocalEffectsRoot(localWeaponRootTransform);

            if (localEffectsRootTransform == null)
                return null;

            Transform orbitalsRootTransform =
                NadaRigPaths.FindDirectChild(localEffectsRootTransform, Plugin.OrbitalsName);

            if (orbitalsRootTransform == null)
            {
                orbitalsRootTransform =
                    NadaRigTransforms.EnsureChild(localEffectsRootTransform, Plugin.OrbitalsName);

                NadaLogControl.Info(
                    $"orbitals-branch:{orbitalsRootTransform.GetInstanceID()}",
                    $"{Plugin.ModName}: Added local Orbitals branch under '{NadaWeaponTargets.FullPath(localEffectsRootTransform)}' as '{Plugin.OrbitalsName}' (owner='{ownerNameForLogs}').");
            }

            Transform orbsRootTransform =
                NadaRigPaths.FindDirectChild(orbitalsRootTransform, Plugin.OrbitalsOrbsName);

            if (orbsRootTransform == null)
            {
                var orbsRootObject =
                    Object.Instantiate(NadaRigCache.DemisterTemplateInactive, orbitalsRootTransform, false);

                orbsRootObject.name = Plugin.OrbitalsOrbsName;
                orbsRootObject.SetActive(true);
                orbsRootTransform = orbsRootObject.transform;

                NadaRigTransforms.ResetLocalTransform(orbsRootTransform);

                NadaLogControl.Info(
                    $"orbs-subtree:{orbsRootTransform.GetInstanceID()}",
                    $"{Plugin.ModName}: Added local orb subtree under '{NadaWeaponTargets.FullPath(orbitalsRootTransform)}' " +
                    $"as '{Plugin.OrbitalsOrbsName}' (owner='{ownerNameForLogs}').");
            }

            var zSyncTransform = orbsRootTransform.GetComponent<ZSyncTransform>();
            if (zSyncTransform != null)
            {
                Object.Destroy(zSyncTransform);

                NadaLogControl.Info(
                    $"remove-zsync:{orbsRootTransform.GetInstanceID()}",
                    $"{Plugin.ModName}: Removed ZSyncTransform from '{NadaWeaponTargets.FullPath(orbsRootTransform)}'.");
            }

            EnsureLocalOrbitalsChildren(orbitalsRootTransform, orbsRootTransform, ownerNameForLogs);

            return orbsRootTransform;
        }

        internal static void FinalizeLocalOrbsBranch(Transform localOrbsRootTransform)
        {
            if (localOrbsRootTransform == null)
                return;

            CollapseOrbMeshIntoParent(localOrbsRootTransform);
            OrganizeLocalOrbsBranch(localOrbsRootTransform);

            NadaRigTransforms.NormalizeParticleSpacesUnder(
                localOrbsRootTransform,
                ParticleSystemSimulationSpace.Local);

            Transform localOrbVisualTransform =
                EnsureLocalOrbVisualChild(localOrbsRootTransform);

            if (localOrbVisualTransform != null)
            {
                NadaRigTransforms.NormalizeParticleSpacesUnder(
                    localOrbVisualTransform,
                    ParticleSystemSimulationSpace.Local);
            }
        }

        internal static void EnsureLocalOrbitalsChildren(
            Transform orbitalsRootTransform,
            Transform localOrbsRootTransform,
            string ownerNameForLogs)
        {
            if (orbitalsRootTransform == null || localOrbsRootTransform == null)
                return;

            Transform orbitalsEffectsRootTransform =
                NadaRigPaths.FindDirectChild(localOrbsRootTransform, "effects");

            if (orbitalsEffectsRootTransform == null)
                return;

            Transform flameRootTransform =
                NadaRigPaths.FindDirectChild(orbitalsEffectsRootTransform, "flame");

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

        internal static Transform EnsureLocalOrbVisualChild(Transform localOrbsRootTransform)
        {
            if (localOrbsRootTransform == null)
                return null;

            Transform existingOrbVisualTransform =
                NadaRigPaths.FindDirectChild(localOrbsRootTransform, "Orb_00");

            if (existingOrbVisualTransform != null)
            {
                NadaRigTransforms.DisableRootVisualContent(localOrbsRootTransform);
                return existingOrbVisualTransform;
            }

            MeshFilter rootMeshFilter = localOrbsRootTransform.GetComponent<MeshFilter>();
            MeshRenderer rootMeshRenderer = localOrbsRootTransform.GetComponent<MeshRenderer>();

            if (rootMeshFilter == null || rootMeshRenderer == null)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: Could not create Orb_00 because root orb visual mesh/renderer was missing at '{NadaWeaponTargets.FullPath(localOrbsRootTransform)}'.");

                return null;
            }

            var orbVisualObject = new GameObject("Orb_00");
            Transform orbVisualTransform = orbVisualObject.transform;
            orbVisualTransform.SetParent(localOrbsRootTransform, false);

            NadaRigTransforms.CopyMeshFilterIfMissing(localOrbsRootTransform, orbVisualTransform);
            NadaRigTransforms.CopyMeshRendererIfMissing(localOrbsRootTransform, orbVisualTransform);

            orbVisualTransform.localPosition = Vector3.zero;
            orbVisualTransform.localRotation = Quaternion.identity;
            orbVisualTransform.localScale = Vector3.one;

            NadaRigTransforms.DisableRootVisualContent(localOrbsRootTransform);

            NadaLogControl.Info(
                $"orb-visual:{orbVisualTransform.GetInstanceID()}",
                $"{Plugin.ModName}: Created clean orb visual child 'Orb_00' under '{NadaWeaponTargets.FullPath(localOrbsRootTransform)}'.");

            return orbVisualTransform;
        }

        internal static void OrganizeLocalOrbsBranch(Transform localOrbsRootTransform)
        {
            if (localOrbsRootTransform == null)
                return;

            Transform orbitalsEffectsRootTransform =
                NadaRigPaths.FindDirectChild(localOrbsRootTransform, "effects");

            if (orbitalsEffectsRootTransform == null)
                return;

            NadaRigTransforms.RemoveDirectChildIfPresent(orbitalsEffectsRootTransform, "SFX Start");
            NadaRigTransforms.RemoveDirectChildIfPresent(orbitalsEffectsRootTransform, "SFX");
            NadaRigTransforms.RemoveDirectChildIfPresent(orbitalsEffectsRootTransform, "Point light");
            NadaRigTransforms.RemoveDirectChildIfPresent(orbitalsEffectsRootTransform, "Particle System Force Field");
            NadaRigTransforms.RemoveDirectChildIfPresent(orbitalsEffectsRootTransform, "flame");

            if (orbitalsEffectsRootTransform.childCount == 0)
                Object.Destroy(orbitalsEffectsRootTransform.gameObject);
        }

        private static void CloneLocalOrbitalsChild(
            Transform sourceParentTransform,
            Transform targetParentTransform,
            string sourceChildName,
            string targetChildName)
        {
            if (sourceParentTransform == null || targetParentTransform == null)
                return;

            Transform existingChildTransform =
                NadaRigPaths.FindDirectChild(targetParentTransform, targetChildName);

            if (existingChildTransform != null)
                return;

            Transform sourceChildTransform =
                NadaRigPaths.FindDirectChild(sourceParentTransform, sourceChildName);

            if (sourceChildTransform == null)
                return;

            var clonedChildObject =
                Object.Instantiate(sourceChildTransform.gameObject, targetParentTransform, false);

            clonedChildObject.name = targetChildName;
            clonedChildObject.SetActive(true);

            clonedChildObject.transform.localPosition = sourceChildTransform.localPosition;
            clonedChildObject.transform.localRotation = sourceChildTransform.localRotation;
            clonedChildObject.transform.localScale = sourceChildTransform.localScale;

            NadaRigTransforms.NormalizeParticleSpacesUnder(
                clonedChildObject.transform,
                ParticleSystemSimulationSpace.World);
        }

        private static void SplitFlameChildrenIntoEffects(
            Transform localEffectsRootTransform,
            Transform outerFlamesTransform)
        {
            if (localEffectsRootTransform == null || outerFlamesTransform == null)
                return;

            Transform flareTransform =
                NadaRigPaths.FindDescendantByName(outerFlamesTransform, "flare");

            if (flareTransform != null)
            {
                flareTransform.name = Plugin.FlareName;

                if (flareTransform.parent != localEffectsRootTransform)
                    flareTransform.SetParent(localEffectsRootTransform, true);
            }

            Transform innerFlamesTransform =
                NadaRigPaths.FindDescendantByName(outerFlamesTransform, "fx_Torch_Basic");

            if (innerFlamesTransform != null)
            {
                innerFlamesTransform.name = Plugin.InnerFlamesName;

                if (innerFlamesTransform.parent != localEffectsRootTransform)
                    innerFlamesTransform.SetParent(localEffectsRootTransform, true);
            }
        }

        private static void CollapseOrbMeshIntoParent(Transform localOrbsRootTransform)
        {
            if (localOrbsRootTransform == null)
                return;

            Transform orbMeshTransform =
                NadaRigPaths.FindDirectChild(localOrbsRootTransform, "demister_ball");

            if (orbMeshTransform == null)
                return;

            Vector3 originalLocalScale = orbMeshTransform.localScale;

            NadaRigTransforms.CopyMeshFilterIfMissing(orbMeshTransform, localOrbsRootTransform);
            NadaRigTransforms.CopyMeshRendererIfMissing(orbMeshTransform, localOrbsRootTransform);

            while (orbMeshTransform.childCount > 0)
            {
                Transform childTransform = orbMeshTransform.GetChild(0);
                childTransform.SetParent(localOrbsRootTransform, true);
            }

            localOrbsRootTransform.localScale = originalLocalScale;

            Object.Destroy(orbMeshTransform.gameObject);
        }
    }
}