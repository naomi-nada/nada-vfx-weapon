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

            foreach (Transform childTransform in searchRootTransform)
            {
                if (childTransform == null)
                    continue;

                string childName = childTransform.name ?? string.Empty;

                if (childName == Plugin.LocalWeaponRootName)
                    continue;

                if (childName.StartsWith("attach", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (childTransform.GetComponentInChildren<Renderer>(true) != null ||
                    childTransform.GetComponentInChildren<ParticleSystem>(true) != null)
                {
                    return childTransform;
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