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

            // Validate runtime readiness / ownership.
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

            // Apply one-off compatibility fixes before building the rig.
            if (weaponVisualRootTransform.name.StartsWith("Sword15_Lava", System.StringComparison.Ordinal))
            {
                NadaRigMaintenance.DisableBrokenFlameRenderer(
                    weaponVisualRootTransform,
                    rootObject.name);
            }

            // 3. Ensure root structure.
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

            // Ensure effect structure.
            NadaOrbitalsRigAssembly.EnsureCompleteOrbitalsRig(
                localWeaponRootTransform,
                itemData,
                rootObject.name);

            NadaStrandsRigAssembly.EnsureLocalStrandsBranch(
                localWeaponRootTransform,
                rootObject.name);

            NadaFlamesRigAssembly.EnsureLocalFlameBranch(
                localWeaponRootTransform,
                rootObject.name);

            NadaSparksRigAssembly.EnsureLocalSparksBranch(
                localWeaponRootTransform,
                rootObject.name);

            NadaAuraRigAssembly.EnsureLocalAuraBranch(
                localWeaponRootTransform,
                weaponVisualRootTransform,
                rootObject.name);

            // Resolve authoritative structure references.
            NadaRigCatalog catalog = NadaRigCatalog.Build(localWeaponRootTransform);
            if (catalog == null)
                return;

            // Bind behavior modules.
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
                catalog.SparksTransform,
                itemData);

            NadaEffectBinder.BindFlareEffect(
                catalog.FlareTransform,
                itemData);

            NadaEffectBinder.BindAuraEffect(
                catalog.AuraTransform,
                itemData);

            NadaEffectBinder.BindOrbitalsEffect(
                catalog.OrbitalsRootTransform,
                catalog.OrbitalsOrbsRootTransform,
                itemData);

            NadaEffectBinder.BindOrbitalsStrandsEffect(
                catalog.OrbitalsStrandsTransform,
                itemData);

            if (catalog.OrbitalsOrbsRootTransform != null)
            {
                NadaMotionBinder.BindOrbsMotion(
                    catalog.OrbitalsOrbsRootTransform,
                    itemData);
            }

            // Apply current state / motion follow / maintenance.
            NadaRigTransformApplier.Apply(
                localWeaponRootTransform,
                context.State);

            NadaMotionBinder.BindOrbitalsRigFollow(
                catalog.OrbitalsRigRootTransform,
                localWeaponRootTransform,
                Vector3.zero,
                Quaternion.Euler(-90f, 0f, 0f));

            NadaRigMaintenance.ApplyPickupFix(rootObject);
        }
    }
}