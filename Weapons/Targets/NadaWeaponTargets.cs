using System;
using System.Collections.Generic;
using UnityEngine;

namespace NADA.VFX.Weapons.Targets
{
    internal static class NadaWeaponTargets
    {
        internal static bool TryGetPrefab(string prefabName, out GameObject prefab)
        {
            prefab = null;

            if (ZNetScene.instance != null)
            {
                prefab = ZNetScene.instance.GetPrefab(prefabName);
                if (prefab != null)
                    return true;
            }

            if (ObjectDB.instance != null && ObjectDB.instance.m_items != null)
            {
                foreach (GameObject itemObject in ObjectDB.instance.m_items)
                {
                    if (itemObject == null)
                        continue;

                    if (string.Equals(itemObject.name, prefabName, StringComparison.Ordinal))
                    {
                        prefab = itemObject;
                        return true;
                    }
                }
            }

            foreach (GameObject gameObject in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (gameObject == null)
                    continue;

                if (string.Equals(gameObject.name, prefabName, StringComparison.Ordinal))
                {
                    prefab = gameObject;
                    return true;
                }
            }

            return false;
        }

        internal static bool IsTargetRoot(GameObject gameObject)
        {
            if (gameObject == null)
                return false;

            string objectName = gameObject.name ?? string.Empty;

            return objectName == Plugin.TargetPrefabName ||
                   objectName.StartsWith(Plugin.TargetPrefabName + "(Clone)", StringComparison.Ordinal);
        }

        internal static bool IsTargetOrAttachClone(GameObject gameObject)
        {
            if (gameObject == null)
                return false;

            if (IsTargetRoot(gameObject))
                return true;

            return IsEquippedAttachClone(gameObject);
        }

        internal static bool IsEquippedAttachClone(GameObject gameObject)
        {
            if (gameObject == null)
                return false;

            string objectName = gameObject.name ?? string.Empty;

            return objectName.StartsWith("attach", StringComparison.OrdinalIgnoreCase) &&
                   FindEquippedWeaponVisualRoot(gameObject.transform) != null;
        }

        internal static Transform FindEquippedWeaponVisualRoot(Transform searchRootTransform)
        {
            if (searchRootTransform == null)
                return null;

            Transform bestTransform = null;
            float bestScore = float.MinValue;

            foreach (Transform childTransform in searchRootTransform)
            {
                if (childTransform == null)
                    continue;

                if (ShouldIgnoreRootCandidate(childTransform))
                    continue;

                float score = ScoreRootCandidate(childTransform);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestTransform = childTransform;
                }
            }

            return bestTransform;
        }
        
        internal static Transform FindVisualMeshRoot(Transform root)
        {
            if (root == null)
                return null;

            // Prefer MeshRenderer (most weapon meshes)
            var meshRenderers = root.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var r in meshRenderers)
            {
                if (r == null) continue;
                return r.transform;
            }

            // Fallback: SkinnedMeshRenderer (rare but safe)
            var skinned = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (var r in skinned)
            {
                if (r == null) continue;
                return r.transform;
            }

            return null;
        }

        private static bool ShouldIgnoreRootCandidate(Transform candidateTransform)
        {
            string candidateName = candidateTransform.name ?? string.Empty;

            if (candidateName == Plugin.LocalWeaponRootName)
                return true;

            if (candidateName.StartsWith("attach", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        private static float ScoreRootCandidate(Transform candidateTransform)
        {
            Renderer[] renderers =
                candidateTransform.GetComponentsInChildren<Renderer>(true);

            int solidRendererCount = 0;
            int vfxRendererCount = 0;
            float totalBoundsVolume = 0f;

            foreach (Renderer renderer in renderers)
            {
                if (renderer == null)
                    continue;

                if (IsLikelyVfxRenderer(renderer))
                {
                    vfxRendererCount++;
                    continue;
                }

                solidRendererCount++;

                Vector3 size = renderer.bounds.size;
                totalBoundsVolume += size.x * size.y * size.z;
            }

            if (solidRendererCount <= 0)
                return float.MinValue;

            return
                solidRendererCount * 1000f +
                totalBoundsVolume * 10f -
                vfxRendererCount * 100f;
        }

        private static bool IsLikelyVfxRenderer(Renderer renderer)
        {
            if (renderer == null)
                return true;

            return renderer is ParticleSystemRenderer ||
                   renderer is TrailRenderer ||
                   renderer is LineRenderer;
        }

        internal static string FullPath(Transform transform)
        {
            if (transform == null)
                return "<null>";

            var pathParts = new Stack<string>();

            while (transform != null)
            {
                pathParts.Push(transform.name);
                transform = transform.parent;
            }

            return string.Join("/", pathParts);
        }
    }
}