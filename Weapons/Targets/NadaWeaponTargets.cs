// File: Targets/NadaWeaponTargets.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NADA.VFX.Weapons.Targets
{
    internal static class NadaWeaponTargets
    {
        internal static bool TryGetPrefab(string name, out GameObject prefab)
        {
            prefab = null;

            if (ZNetScene.instance != null)
            {
                prefab = ZNetScene.instance.GetPrefab(name);
                if (prefab != null) return true;
            }

            if (ObjectDB.instance != null && ObjectDB.instance.m_items != null)
            {
                foreach (var go in ObjectDB.instance.m_items)
                {
                    if (go != null && string.Equals(go.name, name, StringComparison.Ordinal))
                    {
                        prefab = go;
                        return true;
                    }
                }
            }

            return false;
        }

        internal static bool IsTargetRoot(GameObject go)
        {
            if (go == null) return false;
            var n = go.name ?? "";
            return n == Plugin.TargetPrefabName || n.StartsWith(Plugin.TargetPrefabName + "(Clone)", StringComparison.Ordinal);
        }

        internal static bool IsTargetOrAttachClone(GameObject go)
        {
            if (go == null) return false;
            if (IsTargetRoot(go)) return true;

            return (go.name ?? "").StartsWith("attach", StringComparison.OrdinalIgnoreCase)
                   && FindSword15Lava(go.transform) != null;
        }

        /// <summary> Find Sword15_Lava* that lives under an ancestor attach*. </summary>
        internal static Transform FindSword15Lava(Transform root)
        {
            if (root == null) return null;

            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t == null) continue;
                if (!t.name.StartsWith("Sword15_Lava", StringComparison.Ordinal)) continue;

                var p = t.parent;
                while (p != null)
                {
                    if ((p.name ?? "").StartsWith("attach", StringComparison.OrdinalIgnoreCase))
                        return t;
                    p = p.parent;
                }
            }

            return null;
        }

        internal static string FullPath(Transform t)
        {
            if (t == null) return "<null>";
            var stack = new Stack<string>();
            while (t != null)
            {
                stack.Push(t.name);
                t = t.parent;
            }
            return string.Join("/", stack);
        }
    }
}