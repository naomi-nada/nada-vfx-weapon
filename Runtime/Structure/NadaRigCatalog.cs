using UnityEngine;

namespace NADA.VFX.Runtime.Binding
{
    internal sealed class NadaRigCatalog
    {
        public Transform LocalWeaponRoot { get; private set; }

        public Transform LocalEffectsRoot { get; private set; }

        public Transform OuterFlames { get; private set; }
        public Transform Flare { get; private set; }
        public Transform InnerFlames { get; private set; }

        public Transform OrbitalsRoot { get; private set; }

        public Transform OrbitalsOrbs { get; private set; }
        public Transform OrbitalsFlames { get; private set; }
        public Transform OrbitalsEmbers { get; private set; }

        public Transform Mirage { get; private set; }
        public Transform Sparks { get; private set; }

        public Transform OrbitalsRig { get; private set; }

        public bool IsValid =>
            LocalWeaponRoot != null;

        internal static NadaRigCatalog Build(Transform localWeaponRootTf)
        {
            var catalog = new NadaRigCatalog
            {
                LocalWeaponRoot = localWeaponRootTf
            };

            if (localWeaponRootTf != null)
            {
                catalog.LocalEffectsRoot = NadaRigPaths.FindLocalEffectsRoot(localWeaponRootTf);

                catalog.OrbitalsRoot = NadaRigPaths.FindLocalOrbitalsRoot(localWeaponRootTf);
                catalog.OrbitalsRig = NadaRigPaths.FindLocalOrbitalsRigRoot(localWeaponRootTf);
                catalog.OrbitalsOrbs = NadaRigPaths.FindLocalOrbsRoot(localWeaponRootTf);
                catalog.Mirage = NadaRigPaths.FindDirectChild(catalog.LocalEffectsRoot, Plugin.MirageName);
                catalog.Sparks = NadaRigPaths.FindDirectChild(catalog.LocalEffectsRoot, Plugin.SparksName);

                catalog.OuterFlames = NadaRigPaths.FindDirectChild(catalog.LocalEffectsRoot, Plugin.OuterFlamesName);
                catalog.Flare = NadaRigPaths.FindDirectChild(catalog.LocalEffectsRoot, Plugin.FlareName);
                catalog.InnerFlames = NadaRigPaths.FindDirectChild(catalog.LocalEffectsRoot, Plugin.InnerFlamesName);

                if (catalog.OrbitalsRoot != null)
                {
                    catalog.OrbitalsFlames = NadaRigPaths.FindDirectChild(catalog.OrbitalsRoot, Plugin.OrbitalsFlamesName);
                    catalog.OrbitalsEmbers = NadaRigPaths.FindDirectChild(catalog.OrbitalsRoot, Plugin.OrbitalsEmbersName);
                }
            }

            return catalog;
        }
    }
}