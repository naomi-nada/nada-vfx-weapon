using NADA.VFX.Runtime.Binding;
using NADA.VFX.Runtime.Structure;
using NADA.VFX.Weapons.Targets;
using NADA.VFX.Core.State;
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
            {
                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [OrchestratorSkip] cache not ready " +
                    $"root='{rootObject?.name}' " +
                    $"visual='{weaponVisualRootTransform?.name}' " +
                    $"item='{itemData?.m_shared?.m_name}' " +
                    $"bound={VfxStateIO.IsBound(itemData)}");

                return;
            }

            bool isEquippedTarget =
                NadaWeaponTargets.IsTargetOrAttachClone(rootObject);

            bool isBoundDroppedItem =
                itemData != null &&
                VfxStateIO.IsBound(itemData) &&
                rootObject.GetComponent<global::ItemDrop>() != null;

            bool isBoundPreviewOrEquippedVisual =
                itemData != null &&
                VfxStateIO.IsBound(itemData) &&
                weaponVisualRootTransform != null;

            if (!isEquippedTarget &&
                !isBoundDroppedItem &&
                !isBoundPreviewOrEquippedVisual)
            {
                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [OrchestratorSkip] " +
                    $"root='{rootObject?.name}' " +
                    $"visual='{weaponVisualRootTransform?.name}' " +
                    $"item='{itemData?.m_shared?.m_name}' " +
                    $"bound={VfxStateIO.IsBound(itemData)} " +
                    $"isEquippedTarget={isEquippedTarget} " +
                    $"isBoundDroppedItem={isBoundDroppedItem} " +
                    $"isBoundPreviewOrEquippedVisual={isBoundPreviewOrEquippedVisual}");

                return;
            }

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

            Transform strandsTransform =
                NadaRigAssembly.EnsureLocalStrandsBranch(
                    localWeaponRootTransform,
                    rootObject.name);

            Transform outerFlamesTransform =
                NadaRigAssembly.EnsureLocalFlameBranch(
                    localWeaponRootTransform,
                    rootObject.name);

            Transform sparksTransform =
                NadaRigAssembly.EnsureLocalSparksBranch(
                    localWeaponRootTransform,
                    rootObject.name);

            Transform auraTransform =
                NadaRigAssembly.EnsureLocalAuraBranch(
                    localWeaponRootTransform,
                    weaponVisualRootTransform,
                    rootObject.name);

            if (localOrbsRootTransform != null)
                NadaRigAssembly.FinalizeLocalOrbsBranch(localOrbsRootTransform);

            NadaRigCatalog catalog = NadaRigCatalog.Build(localWeaponRootTransform);
            if (catalog == null)
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

            NadaEffectBinder.BindInnerFlamesEffect(
                catalog.InnerFlamesTransform,
                itemData);

            NadaEffectBinder.BindOuterFlamesEffect(
                catalog.OuterFlamesTransform,
                itemData);

            NadaMotionBinder.BindOuterFlamesMotion(
                catalog.OuterFlamesTransform,
                itemData);

            NadaEffectBinder.BindSparksEffect(
                catalog.SparksTransform ?? sparksTransform,
                itemData);

            NadaEffectBinder.BindFlareEffect(
                catalog.FlareTransform,
                itemData);

            NadaEffectBinder.BindAuraEffect(
                auraTransform,
                itemData);

            NadaEffectBinder.BindOrbitalsEffect(
                catalog.OrbitalsRootTransform,
                catalog.OrbitalsOrbsRootTransform ?? localOrbsRootTransform,
                itemData);

            NadaEffectBinder.BindOrbitalsStrandsEffect(
                strandsTransform,
                itemData);

            if (catalog.OrbitalsOrbsRootTransform != null || localOrbsRootTransform != null)
            {
                NadaMotionBinder.BindOrbsMotion(
                    catalog.OrbitalsOrbsRootTransform ?? localOrbsRootTransform,
                    itemData);
            }

            NadaRigTransformApplier.Apply(
                localWeaponRootTransform,
                context.State);

            NadaMotionBinder.BindOrbitalsRigFollow(
                activeOrbitalsRigRootTransform,
                localWeaponRootTransform,
                Vector3.zero,
                Quaternion.Euler(-90f, 0f, 0f));

            NadaRigMaintenance.ApplyPickupFix(rootObject);
        }
    }
}