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

            NadaRigMaintenance.DisableBrokenFlameRenderer(
                weaponVisualRootTransform,
                rootObject.name);

            Transform localWeaponRootTransform =
                NadaRigRootAssembly.EnsureAttachedLocalWeaponBranch(
                    weaponVisualRootTransform,
                    rootObject.name);

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
                NadaRigAssembly.EnsureLocalFlameBranchAndAlign(
                    localWeaponRootTransform,
                    rootObject.name);

            if (outerFlamesTransform == null)
                return;

            NadaRigAssembly.FinalizeLocalOrbsBranch(localOrbsRootTransform);

            NadaRigAssembly.EnsureLocalSparks(
                localEffectsRootTransform,
                localOrbsRootTransform,
                rootObject.name);

            NadaRigAssembly.EnsureLocalMirage(
                localEffectsRootTransform,
                localOrbsRootTransform,
                rootObject.name);

            NadaRigCatalog catalog = NadaRigCatalog.Build(localWeaponRootTransform);
            if (catalog == null || !catalog.IsValid)
                return;

            if (catalog.OrbitalsRootTransform != null)
            {
                Transform localOrbitalsRigRootTransform =
                    NadaOrbitalsRigAssembly.EnsureLocalOrbitalsRig(
                        catalog.OrbitalsRootTransform,
                        rootObject.name);

                if (localOrbitalsRigRootTransform != null)
                {
                    NadaOrbitalsRigAssembly.EnsureOrbitalsFamilyMotion(
                        localOrbitalsRigRootTransform,
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

            NadaEffectBinder.BindFlareEffect(
                catalog.FlareTransform,
                itemData);

            // Sparks, Mirage
            NadaEffectBinder.BindSparksEffect(
                catalog.SparksTransform,
                itemData);

            Transform sparksMotionRootTransform =
                NadaEffectMotionRootAssembly.EnsureSparksMotionRoot(
                    catalog.LocalEffectsRootTransform,
                    catalog.SparksTransform);

            NadaMotionBinder.BindTargetFollow(
                sparksMotionRootTransform,
                catalog.FlareTransform);

            NadaMotionBinder.BindTargetFollow(
                catalog.SparksTransform,
                sparksMotionRootTransform);

            NadaEffectBinder.BindMirageEffect(
                catalog.MirageTransform,
                itemData);

            Transform mirageMotionRootTransform =
                NadaEffectMotionRootAssembly.EnsureMirageMotionRoot(
                    catalog.LocalEffectsRootTransform,
                    catalog.MirageTransform);

            NadaMotionBinder.BindTargetFollow(
                mirageMotionRootTransform,
                catalog.FlareTransform);

            NadaMotionBinder.BindTargetFollow(
                catalog.MirageTransform,
                mirageMotionRootTransform);

            // Orbitals: Orbs, Flames, Embers
            NadaEffectBinder.BindOrbitalsEffect(
                catalog.OrbitalsRootTransform,
                catalog.OrbitalsOrbsRootTransform ?? localOrbsRootTransform,
                itemData);

            NadaMotionBinder.BindOrbsMotion(
                catalog.OrbitalsOrbsRootTransform ?? localOrbsRootTransform,
                itemData);

            NadaMotionBinder.BindOrbitalsRigFollow(
                catalog.OrbitalsRigRootTransform,
                weaponVisualRootTransform);

            NadaRigMaintenance.ApplyPickupFix(rootObject);
        }
    }
}