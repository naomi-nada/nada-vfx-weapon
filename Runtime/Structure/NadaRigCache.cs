using System.Collections;
using UnityEngine;
using NADA.VFX.Weapons.Targets;

namespace NADA.VFX.Runtime.Structure
{
    internal static class NadaRigCache
    {
        internal static bool CacheReady { get; private set; }
        internal static Material PickupMat { get; private set; }
        internal static GameObject RefRigTemplateInactive { get; private set; }
        internal static GameObject DemisterTemplateInactive { get; private set; }

        internal static IEnumerator CacheReferenceAssetsWhenReady()
        {
            DemisterTemplateInactive = null;
            CacheReady = false;
            PickupMat = null;
            RefRigTemplateInactive = null;

            while (ObjectDB.instance == null && ZNetScene.instance == null)
                yield return null;

            GameObject reference = null;
            while (!NadaWeaponTargets.TryGetPrefab(Plugin.ReferencePrefabName, out reference) || reference == null)
                yield return null;

            var refRootPsr = reference.GetComponent<ParticleSystemRenderer>();
            if (refRootPsr != null && refRootPsr.sharedMaterial != null)
            {
                PickupMat = refRootPsr.sharedMaterial;
                Plugin.Log.LogInfo($"{Plugin.ModName}: Cached pickup PSR material from '{Plugin.ReferencePrefabName}'.");
            }
            else
            {
                Plugin.Log.LogWarning($"{Plugin.ModName}: Reference root PSR/material missing (ItemDrop pickup may not be fixed).");
            }

            var refRigTf = reference.transform.Find(Plugin.ReferenceRigPath);
            if (refRigTf == null)
            {
                Plugin.Log.LogWarning($"{Plugin.ModName}: Could not cache reference rig '{Plugin.ReferenceRigPath}' from '{Plugin.ReferencePrefabName}'.");
                yield break;
            }

            RefRigTemplateInactive = Object.Instantiate(refRigTf.gameObject);
            RefRigTemplateInactive.name = "NADA_BurnyTemplate";
            RefRigTemplateInactive.SetActive(false);
            RefRigTemplateInactive.hideFlags = HideFlags.HideAndDontSave;

            var demisterSource = FindTopLevelDemisterSource();
            if (demisterSource != null)
            {
                DemisterTemplateInactive = Object.Instantiate(demisterSource);
                DemisterTemplateInactive.name = "NADA_DemisterTemplate";
                DemisterTemplateInactive.SetActive(false);
                DemisterTemplateInactive.hideFlags = HideFlags.HideAndDontSave;

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Cached top-level demister template from '{demisterSource.name}' " +
                    $"at '{NadaWeaponTargets.FullPath(demisterSource.transform)}'.");
            }
            else
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: Could not find top-level demister source '{Plugin.DemisterPrefabName}'.");
            }

            CacheReady = true;
            Plugin.Log.LogInfo($"{Plugin.ModName}: Cached Burny rig template '{Plugin.ReferenceRigPath}' from '{Plugin.ReferencePrefabName}'.");
        }

        private static GameObject FindTopLevelDemisterSource()
        {
            if (NadaWeaponTargets.TryGetPrefab(Plugin.DemisterPrefabName, out var prefab) &&
                IsValidTopLevelDemisterRoot(prefab))
            {
                return prefab;
            }

            var all = Resources.FindObjectsOfTypeAll<GameObject>();
            if (all == null) return null;

            GameObject best = null;
            int bestScore = int.MinValue;

            foreach (var go in all)
            {
                if (!IsValidTopLevelDemisterRoot(go))
                    continue;

                int score = ScoreDemisterCandidate(go);
                if (score > bestScore)
                {
                    best = go;
                    bestScore = score;
                }
            }

            return best;
        }

        private static bool IsValidTopLevelDemisterRoot(GameObject go)
        {
            if (go == null) return false;
            if (go.name != Plugin.DemisterPrefabName) return false;
            if (go.transform.parent != null) return false;
            if (go.transform.Find("effects") == null) return false;

            return true;
        }

        private static int ScoreDemisterCandidate(GameObject go)
        {
            int score = 0;

            try
            {
                if (!go.scene.IsValid()) score += 10;
            }
            catch { }

            if (go.GetComponent<ZNetView>() != null) score += 5;
            if (go.GetComponent<ZSyncTransform>() != null) score += 5;
            if (go.transform.Find("effects") != null) score += 10;
            if (go.transform.Find("flame") != null) score += 4;
            if (go.transform.Find("Point light") != null) score += 2;

            return score;
        }
    }
}