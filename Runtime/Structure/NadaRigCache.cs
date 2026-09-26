using System.Collections;
using NADA.VFX.Weapon.Weapons.Targets;
using UnityEngine;

namespace NADA.VFX.Weapon.Runtime.Structure
{
    internal static class NadaRigCache
    {
        internal static bool CacheReady { get; private set; }
        internal static Material PickupMat { get; private set; }
        internal static GameObject RefRigTemplateInactive { get; private set; }
        internal static GameObject DemisterTemplateInactive { get; private set; }
        internal static GameObject SparksTemplateInactive { get; private set; }
        internal static Material AuraMaterial { get; private set; }
        internal static GameObject StrandsTemplateInactive { get; private set; }
        internal static GameObject CoresTemplateInactive { get; private set; }

        internal static IEnumerator CacheReferenceAssetsWhenReady()
        {
            CacheReady = false;

            // These templates are runtime objects owned by this cache.
            // If the cache is rebuilt, clean up the old copies before
            // replacing them instead of abandoning hidden objects.
            DestroyOwnedTemplates();

            PickupMat = null;
            AuraMaterial = null;

            while (ObjectDB.instance == null)
                yield return null;

            GameObject reference = null;

            while (!NadaWeaponTargets.TryGetPrefab(
                       Plugin.ReferencePrefabName,
                       out reference) ||
                   reference == null)
            {
                yield return null;
            }

            var refRootPsr =
                reference.GetComponent<ParticleSystemRenderer>();

            if (refRootPsr != null &&
                refRootPsr.sharedMaterial != null)
            {
                PickupMat =
                    refRootPsr.sharedMaterial;

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Cached pickup PSR material from '{Plugin.ReferencePrefabName}'.");
            }
            else
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: Reference root PSR/material missing (ItemDrop pickup may not be fixed).");
            }

            var refRigTf =
                reference.transform.Find(
                    Plugin.ReferenceRigPath);

            if (refRigTf == null)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: Could not cache reference rig '{Plugin.ReferenceRigPath}' from '{Plugin.ReferencePrefabName}'.");

                yield break;
            }

            RefRigTemplateInactive =
                Object.Instantiate(
                    refRigTf.gameObject);

            RefRigTemplateInactive.name =
                "NADA_BurnyTemplate";

            RefRigTemplateInactive.SetActive(false);

            RefRigTemplateInactive.hideFlags =
                HideFlags.HideAndDontSave;

            GameObject demisterSource =
                FindTopLevelDemisterSource();

            if (demisterSource != null)
            {
                DemisterTemplateInactive =
                    BuildDemisterVisualTemplate(
                        demisterSource);

                if (DemisterTemplateInactive != null)
                {
                    Plugin.Log.LogInfo(
                        $"{Plugin.ModName}: Cached visual-only demister template from '{demisterSource.name}' " +
                        $"at '{NadaWeaponTargets.FullPath(demisterSource.transform)}'.");
                }
                else
                {
                    Plugin.Log.LogWarning(
                        $"{Plugin.ModName}: Could not build visual-only demister template from '{demisterSource.name}'.");
                }
            }
            else
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: Could not find top-level demister source '{Plugin.DemisterPrefabName}'.");
            }

            CacheSparksTemplate();
            CacheAuraMaterial();
            CacheStrandsTemplate();
            CacheCoresTemplate();

            CacheReady = true;

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: Cached Burny rig template '{Plugin.ReferenceRigPath}' from '{Plugin.ReferencePrefabName}'.");
        }

        private static void DestroyOwnedTemplates()
        {
            DestroyOwnedTemplate(
                RefRigTemplateInactive);

            DestroyOwnedTemplate(
                DemisterTemplateInactive);

            DestroyOwnedTemplate(
                SparksTemplateInactive);

            DestroyOwnedTemplate(
                StrandsTemplateInactive);

            DestroyOwnedTemplate(
                CoresTemplateInactive);

            RefRigTemplateInactive = null;
            DemisterTemplateInactive = null;
            SparksTemplateInactive = null;
            StrandsTemplateInactive = null;
            CoresTemplateInactive = null;
        }

        private static void DestroyOwnedTemplate(
            GameObject template)
        {
            if (template == null)
                return;

            Object.Destroy(template);
        }

        private static GameObject BuildDemisterVisualTemplate(
            GameObject demisterSource)
        {
            if (demisterSource == null)
                return null;

            Transform effectsSourceTransform =
                demisterSource.transform.Find("effects");

            Transform orbSourceTransform =
                demisterSource.transform.Find("demister_ball");

            if (effectsSourceTransform == null ||
                orbSourceTransform == null)
            {
                return null;
            }

            // Don't clone the vanilla networked root. NADA only needs the
            // visual branches, so build a clean inactive template of our own.
            var templateObject =
                new GameObject("NADA_DemisterTemplate");

            templateObject.SetActive(false);

            templateObject.hideFlags =
                HideFlags.HideAndDontSave;

            CloneDemisterVisualBranch(
                effectsSourceTransform,
                templateObject.transform);

            CloneDemisterVisualBranch(
                orbSourceTransform,
                templateObject.transform);

            StripNetworkArtifactsImmediate(
                templateObject.transform);

            return templateObject;
        }

        private static void CloneDemisterVisualBranch(
            Transform sourceTransform,
            Transform templateRootTransform)
        {
            if (sourceTransform == null ||
                templateRootTransform == null)
            {
                return;
            }

            GameObject clonedObject =
                Object.Instantiate(
                    sourceTransform.gameObject,
                    templateRootTransform,
                    false);

            clonedObject.name =
                sourceTransform.name;

            clonedObject.transform.localPosition =
                sourceTransform.localPosition;

            clonedObject.transform.localRotation =
                sourceTransform.localRotation;

            clonedObject.transform.localScale =
                sourceTransform.localScale;
        }

        private static void StripNetworkArtifactsImmediate(
            Transform rootTransform)
        {
            if (rootTransform == null)
                return;

            foreach (ZSyncTransform zSyncTransform in
                     rootTransform.GetComponentsInChildren<ZSyncTransform>(true))
            {
                if (zSyncTransform != null)
                    Object.DestroyImmediate(zSyncTransform);
            }

            foreach (ZNetView zNetView in
                     rootTransform.GetComponentsInChildren<ZNetView>(true))
            {
                if (zNetView != null)
                    Object.DestroyImmediate(zNetView);
            }
        }

        private static GameObject FindTopLevelDemisterSource()
        {
            if (NadaWeaponTargets.TryGetPrefab(
                    Plugin.DemisterPrefabName,
                    out var prefab) &&
                IsValidTopLevelDemisterRoot(prefab))
            {
                return prefab;
            }

            var all =
                Resources.FindObjectsOfTypeAll<GameObject>();

            if (all == null)
                return null;

            GameObject best = null;
            int bestScore = int.MinValue;

            foreach (var go in all)
            {
                if (!IsValidTopLevelDemisterRoot(go))
                    continue;

                int score =
                    ScoreDemisterCandidate(go);

                if (score > bestScore)
                {
                    best = go;
                    bestScore = score;
                }
            }

            return best;
        }

        private static bool IsValidTopLevelDemisterRoot(
            GameObject go)
        {
            if (go == null)
                return false;

            if (go.name != Plugin.DemisterPrefabName)
                return false;

            if (go.transform.parent != null)
                return false;

            if (go.transform.Find("effects") == null)
                return false;

            return true;
        }

        private static int ScoreDemisterCandidate(
            GameObject go)
        {
            int score = 0;

            try
            {
                if (!go.scene.IsValid())
                    score += 10;
            }
            catch
            {
            }

            if (go.GetComponent<ZNetView>() != null)
                score += 5;

            if (go.GetComponent<ZSyncTransform>() != null)
                score += 5;

            if (go.transform.Find("effects") != null)
                score += 10;

            if (go.transform.Find("flame") != null)
                score += 4;

            if (go.transform.Find("Point light") != null)
                score += 2;

            return score;
        }

        private static void CacheSparksTemplate()
        {
            if (!NadaWeaponTargets.TryGetPrefab(
                    Plugin.SparksReferencePrefabName,
                    out GameObject sparksReference) ||
                sparksReference == null)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: Could not find Sparks reference prefab '{Plugin.SparksReferencePrefabName}'.");

                return;
            }

            Transform sparksTransform =
                sparksReference.transform.Find(
                    Plugin.SparksReferencePath);

            if (sparksTransform == null)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: Could not find Sparks reference path '{Plugin.SparksReferencePath}' " +
                    $"under '{Plugin.SparksReferencePrefabName}'.");

                return;
            }

            SparksTemplateInactive =
                Object.Instantiate(
                    sparksTransform.gameObject);

            SparksTemplateInactive.name =
                "NADA_SparksTemplate";

            SparksTemplateInactive.SetActive(false);

            SparksTemplateInactive.hideFlags =
                HideFlags.HideAndDontSave;

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: Cached Sparks template from " +
                $"'{Plugin.SparksReferencePrefabName}/{Plugin.SparksReferencePath}'.");
        }

        private static void CacheAuraMaterial()
        {
            Material bestMaterial = null;

            Material[] materials =
                Resources.FindObjectsOfTypeAll<Material>();

            foreach (Material material in materials)
            {
                if (material == null)
                    continue;

                if (material.name.StartsWith(
                        Plugin.AuraReferenceMaterialName,
                        System.StringComparison.Ordinal))
                {
                    bestMaterial = material;
                    break;
                }
            }

            if (bestMaterial == null)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: Could not cache Aura material '{Plugin.AuraReferenceMaterialName}'.");

                return;
            }

            // Borrowed vanilla asset. We cache the reference but do not own
            // or destroy the material itself.
            AuraMaterial = bestMaterial;

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: Cached Aura material '{AuraMaterial.name}'.");
        }

        private static void CacheStrandsTemplate()
        {
            if (!NadaWeaponTargets.TryGetPrefab(
                    Plugin.StrandsReferencePrefabName,
                    out GameObject strandsReference) ||
                strandsReference == null)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: Could not find Strands reference prefab '{Plugin.StrandsReferencePrefabName}'.");

                return;
            }

            Transform strandsTransform =
                strandsReference.transform.Find(
                    Plugin.StrandsReferencePath);

            if (strandsTransform == null)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: Could not find Strands reference path '{Plugin.StrandsReferencePath}' " +
                    $"under '{Plugin.StrandsReferencePrefabName}'.");

                return;
            }

            StrandsTemplateInactive =
                Object.Instantiate(
                    strandsTransform.gameObject);

            StrandsTemplateInactive.name =
                "NADA_StrandsTemplate";

            StrandsTemplateInactive.SetActive(false);

            StrandsTemplateInactive.hideFlags =
                HideFlags.HideAndDontSave;

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: Cached Strands template from " +
                $"'{Plugin.StrandsReferencePrefabName}/{Plugin.StrandsReferencePath}'.");
        }

        private static void CacheCoresTemplate()
        {
            if (!NadaWeaponTargets.TryGetPrefab(
                    Plugin.CoresReferencePrefabName,
                    out GameObject coresReference) ||
                coresReference == null)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: Could not find Cores reference prefab '{Plugin.CoresReferencePrefabName}'.");

                return;
            }

            Transform coreTransform =
                coresReference.transform.Find(
                    Plugin.CoresReferencePath);

            if (coreTransform == null)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: Could not find Cores reference path '{Plugin.CoresReferencePath}' " +
                    $"under '{Plugin.CoresReferencePrefabName}'.");

                return;
            }

            CoresTemplateInactive =
                Object.Instantiate(
                    coreTransform.gameObject);

            CoresTemplateInactive.name =
                "NADA_CoresTemplate";

            CoresTemplateInactive.SetActive(false);

            CoresTemplateInactive.hideFlags =
                HideFlags.HideAndDontSave;

            foreach (Collider collider in
                     CoresTemplateInactive.GetComponentsInChildren<Collider>(
                         true))
            {
                if (collider != null)
                    Object.Destroy(collider);
            }

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: Cached Cores template from " +
                $"'{Plugin.CoresReferencePrefabName}/{Plugin.CoresReferencePath}'.");
        }
    }
}