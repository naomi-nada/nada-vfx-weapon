using UnityEngine;

namespace NADA.VFX.Weapon.Runtime.Structure
{
    internal static class NadaRigPaths
    {
        internal static Transform FindLocalOrbitalsRigRoot(Transform localWeaponRootTf)
        {
            Transform orbitalsRoot = FindLocalOrbitalsRoot(localWeaponRootTf);
            if (orbitalsRoot == null) return null;

            return FindDirectChild(orbitalsRoot, Plugin.OrbitalsRigRootName);
        }

        internal static Transform FindLocalEffectsRoot(Transform localWeaponRootTf)
        {
            if (localWeaponRootTf == null) return null;
            return FindDirectChild(localWeaponRootTf, Plugin.EffectsRootName);
        }

        internal static Transform FindLocalOrbitalsRoot(Transform localWeaponRootTf)
        {
            if (localWeaponRootTf == null) return null;

            Transform effectsRoot = FindDirectChild(localWeaponRootTf, Plugin.EffectsRootName);
            if (effectsRoot == null) return null;

            return FindDirectChild(effectsRoot, Plugin.OrbitalsName);
        }

        internal static Transform FindLocalOrbsRoot(Transform localWeaponRootTf)
        {
            Transform orbitalsRoot = FindLocalOrbitalsRoot(localWeaponRootTf);
            return FindDirectChild(orbitalsRoot, Plugin.OrbitalsOrbsName);
        }

        internal static Transform FindDirectChild(Transform parent, string name)
        {
            if (parent == null) return null;

            foreach (Transform child in parent)
            {
                if (child != null && child.name == name)
                    return child;
            }

            return null;
        }

        internal static Transform FindDescendantByName(Transform root, string name)
        {
            if (root == null) return null;

            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t != null && t.name == name)
                    return t;
            }

            return null;
        }
    }
}