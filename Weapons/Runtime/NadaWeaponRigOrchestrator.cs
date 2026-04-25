using NADA.VFX.Runtime.Binding;
using NADA.VFX.Runtime.Structure;
using NADA.VFX.Weapons.Targets;
using UnityEngine;

namespace NADA.VFX.Weapons.Runtime
{
    internal static class NadaWeaponRigOrchestrator
    {
        internal static void Run(NadaWeaponRigContext context)
        {
            if (context == null || !context.IsValid)
                return;

            GameObject rootObject = context.Root;
            global::ItemDrop.ItemData itemData = context.ItemData;
            Transform weaponVisualRootTransform = context.WeaponVisualRoot;

            if (!NadaRigCache.CacheReady)
                return;

            if (!NadaWeaponTargets.IsTargetOrAttachClone(rootObject))
                return;

            if (weaponVisualRootTransform.name.StartsWith("Sword15_Lava", System.StringComparison.Ordinal))
            {
                NadaRigMaintenance.DisableBrokenFlameRenderer(
                    weaponVisualRootTransform,
                    rootObject.name);
            }

            NadaWeaponRigAlignment alignment =
                NadaWeaponRigAlignmentResolver.Resolve(itemData, weaponVisualRootTransform);

            Transform localWeaponRootTransform =
                NadaRigRootAssembly.EnsureAttachedLocalWeaponBranch(
                    weaponVisualRootTransform,
                    rootObject.name,
                    alignment);

            if (localWeaponRootTransform == null)
                return;

            Transform localEffectsRootTransform =
                NadaRigPaths.FindLocalEffectsRoot(localWeaponRootTransform);

            if (localEffectsRootTransform == null)
                return;

            Transform localOrbsRootTransform =
                NadaRigAssembly.EnsureLocalOrbsBranch(
                    localWeaponRootTransform,
                    rootObject.name);

            if (localOrbsRootTransform == null)
                return;

            Transform outerFlamesTransform =
                NadaRigAssembly.EnsureLocalFlameBranch(
                    localWeaponRootTransform,
                    rootObject.name);

            if (outerFlamesTransform == null)
                return;

            NadaRigAssembly.FinalizeLocalOrbsBranch(localOrbsRootTransform);

            NadaRigCatalog catalog = NadaRigCatalog.Build(localWeaponRootTransform);
            if (catalog == null || !catalog.IsValid)
                return;

            Transform activeOrbitalsRigRootTransform = catalog.OrbitalsRigRootTransform;

            if (catalog.OrbitalsRootTransform != null)
            {
                activeOrbitalsRigRootTransform =
                    NadaOrbitalsRigAssembly.EnsureLocalOrbitalsRig(
                        catalog.OrbitalsRootTransform,
                        rootObject.name);

                if (activeOrbitalsRigRootTransform != null)
                {
                    NadaOrbitalsRigAssembly.EnsureOrbitalsFamilyMotion(
                        activeOrbitalsRigRootTransform,
                        catalog.OrbitalsRootTransform,
                        itemData,
                        rootObject.name);
                }
            }

            float orbOrbitAdherence = NadaMotionTuningResolver.GetOrbsOrbitAdherence(itemData);
            _ = orbOrbitAdherence;

            // Inner Flames, Outer Flames, Flare
            NadaEffectBinder.BindInnerFlamesEffect(
                catalog.InnerFlamesTransform,
                itemData);

            NadaEffectBinder.BindOuterFlamesEffect(
                catalog.OuterFlamesTransform,
                itemData);

            NadaMotionBinder.BindOuterFlamesMotion(
                catalog.OuterFlamesTransform,
                itemData);

            NadaEffectBinder.BindFlareEffect(
                catalog.FlareTransform,
                itemData);

            // Orbitals: Orbs, Flames, Embers
            NadaEffectBinder.BindOrbitalsEffect(
                catalog.OrbitalsRootTransform,
                catalog.OrbitalsOrbsRootTransform ?? localOrbsRootTransform,
                itemData);

            NadaMotionBinder.BindOrbsMotion(
                catalog.OrbitalsOrbsRootTransform ?? localOrbsRootTransform,
                itemData);

            NadaMotionBinder.BindOrbitalsRigFollow(
                activeOrbitalsRigRootTransform,
                weaponVisualRootTransform);

            NadaRigMaintenance.ApplyPickupFix(rootObject);
        }
    }
}