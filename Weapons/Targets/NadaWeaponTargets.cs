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

            string objectName = gameObject.name ?? string.Empty;

            return objectName.StartsWith("attach", StringComparison.OrdinalIgnoreCase) &&
                   FindSword15Lava(gameObject.transform) != null;
        }

        // Finds Sword15_Lava* beneath the supplied root, but only if it lives under an attach* ancestor.
        internal static Transform FindSword15Lava(Transform searchRootTransform)
        {
            if (searchRootTransform == null)
                return null;

            foreach (Transform candidateTransform in searchRootTransform.GetComponentsInChildren<Transform>(true))
            {
                if (candidateTransform == null)
                    continue;

                if (!candidateTransform.name.StartsWith("Sword15_Lava", StringComparison.Ordinal))
                    continue;

                Transform parentTransform = candidateTransform.parent;
                while (parentTransform != null)
                {
                    string parentName = parentTransform.name ?? string.Empty;
                    if (parentName.StartsWith("attach", StringComparison.OrdinalIgnoreCase))
                        return candidateTransform;

                    parentTransform = parentTransform.parent;
                }
            }

            return null;
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