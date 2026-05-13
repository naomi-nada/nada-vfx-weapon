using UnityEngine;

namespace NADA.VFX.Runtime.Structure
{
    internal sealed class NadaRigCatalog
    {
        // Core
        public Transform LocalWeaponRootTransform { get; private set; }
        public Transform LocalEffectsRootTransform { get; private set; }

        // Core Effects
        public Transform InnerFlamesTransform { get; private set; }
        public Transform OuterFlamesTransform { get; private set; }
        public Transform SparksTransform { get; private set; }
        public Transform FlareTransform { get; private set; }
        public Transform AuraTransform { get; private set; }
        
        // Orbitals
        public Transform OrbitalsRootTransform { get; private set; }
        public Transform OrbitalsRigRootTransform { get; private set; }

        public Transform OrbitalsOrbsRootTransform { get; private set; }
        public Transform OrbitalsStrandsTransform { get; private set; }
        public Transform OrbitalsFlamesRootTransform { get; private set; }
        public Transform OrbitalsEmbersRootTransform { get; private set; }
        
        public Transform OrbitalsFlamesPoolRootTransform { get; private set; }
        public Transform OrbitalsEmbersPoolRootTransform { get; private set; }

        public bool IsValid =>
            LocalWeaponRootTransform != null &&
            LocalEffectsRootTransform != null;

        internal static NadaRigCatalog Build(Transform localWeaponRootTransform)
        {
            var catalog = new NadaRigCatalog
            {
                LocalWeaponRootTransform = localWeaponRootTransform
            };

            if (localWeaponRootTransform != null)
            {
                catalog.LocalEffectsRootTransform =
                    NadaRigPaths.FindLocalEffectsRoot(localWeaponRootTransform);

                // Core effects
                catalog.InnerFlamesTransform =
                    NadaRigPaths.FindDirectChild(catalog.LocalEffectsRootTransform, Plugin.InnerFlamesName);
                
                catalog.OuterFlamesTransform =
                    NadaRigPaths.FindDirectChild(catalog.LocalEffectsRootTransform, Plugin.OuterFlamesName);
                
                catalog.SparksTransform =
                    NadaRigPaths.FindDirectChild(catalog.LocalEffectsRootTransform, Plugin.SparksName);

                catalog.FlareTransform =
                    NadaRigPaths.FindDirectChild(catalog.LocalEffectsRootTransform, Plugin.FlareName);
                
                catalog.AuraTransform =
                    NadaRigPaths.FindDirectChild(catalog.LocalEffectsRootTransform, Plugin.AuraName);

                // Orbitals
                catalog.OrbitalsRootTransform =
                    NadaRigPaths.FindLocalOrbitalsRoot(localWeaponRootTransform);

                catalog.OrbitalsRigRootTransform =
                    NadaRigPaths.FindLocalOrbitalsRigRoot(localWeaponRootTransform);

                catalog.OrbitalsOrbsRootTransform =
                    NadaRigPaths.FindLocalOrbsRoot(localWeaponRootTransform);
                
                catalog.OrbitalsStrandsTransform =
                    NadaRigPaths.FindDirectChild(catalog.LocalEffectsRootTransform, Plugin.OrbitalsStrandsName);

                if (catalog.OrbitalsRootTransform != null)
                {
                    catalog.OrbitalsFlamesRootTransform =
                        NadaRigPaths.FindDirectChild(catalog.OrbitalsRootTransform, Plugin.OrbitalsFlamesName);

                    catalog.OrbitalsEmbersRootTransform =
                        NadaRigPaths.FindDirectChild(catalog.OrbitalsRootTransform, Plugin.OrbitalsEmbersName);
                }

                if (catalog.OrbitalsRigRootTransform != null)
                {
                    Transform poolsRoot =
                        NadaRigPaths.FindDirectChild(catalog.OrbitalsRigRootTransform, Plugin.OrbitalsPoolsRootName);

                    if (poolsRoot != null)
                    {
                        catalog.OrbitalsFlamesPoolRootTransform =
                            NadaRigPaths.FindDirectChild(poolsRoot, Plugin.OrbitalsFlamesPoolName);

                        catalog.OrbitalsEmbersPoolRootTransform =
                            NadaRigPaths.FindDirectChild(poolsRoot, Plugin.OrbitalsEmbersPoolName);
                    }
                }
            }

            return catalog;
        }
    }
}