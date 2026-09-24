using NADA.VFX.Weapon.Core.Debug;
using NADA.VFX.Weapon.Core.State;
using NADA.VFX.Weapon.Runtime.Binding;
using NADA.VFX.Weapon.Runtime.Structure;
using NADA.VFX.Weapon.Weapons.Targets;
using UnityEngine;

namespace NADA.VFX.Weapon.Weapons.Runtime
{
    internal static class NadaWeaponRigOrchestrator
    {
        internal static void Run(NadaWeaponRigContext context)
        {
            if (context == null || !context.IsValid)
                return;

            GameObject rootObject =
                context.Root;

            global::ItemDrop.ItemData itemData =
                context.ItemData;

            Transform weaponVisualRootTransform =
                context.WeaponVisualRoot;

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
                NadaWeaponTargets.IsTargetOrAttachClone(
                    rootObject);

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

            if (weaponVisualRootTransform.name.StartsWith(
                    "Sword15_Lava",
                    System.StringComparison.Ordinal))
            {
                NadaRigMaintenance.DisableBrokenFlameRenderer(
                    weaponVisualRootTransform,
                    rootObject.name);
            }

            NadaWeaponRigAlignment alignment =
                NadaWeaponRigAlignmentResolver.Resolve(
                    itemData,
                    weaponVisualRootTransform);

            Transform localWeaponRootTransform =
                NadaRigRootAssembly.EnsureAttachedLocalWeaponBranch(
                    weaponVisualRootTransform,
                    rootObject.name,
                    alignment);

            if (localWeaponRootTransform == null)
                return;

            Transform localEffectsRootTransform =
                NadaRigPaths.FindLocalEffectsRoot(
                    localWeaponRootTransform);

            if (localEffectsRootTransform == null)
                return;

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

            NadaRigCatalog catalog =
                NadaRigCatalog.Build(
                    localWeaponRootTransform);

            if (catalog == null)
                return;

            if (itemData != null)
            {
                NadaEffectBinder.BindInnerFlamesEffect(
                    catalog.InnerFlamesTransform,
                    itemData);
            }
            else
            {
                NadaEffectBinder.BindInnerFlamesEffect(
                    catalog.InnerFlamesTransform,
                    context.State);
            }

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

            NadaEffectBinder.BindStrandsEffect(
                catalog.StrandsTransform,
                itemData);

            if (catalog.OrbitalsOrbsRootTransform != null)
            {
                NadaMotionBinder.BindOrbsMotion(
                    catalog.OrbitalsOrbsRootTransform,
                    itemData);
            }

            NadaRigTransformApplier.Apply(
                localWeaponRootTransform,
                context.State);

            NadaMotionBinder.BindOrbitalsRigFollow(
                catalog.OrbitalsRigRootTransform,
                localWeaponRootTransform,
                Vector3.zero,
                Quaternion.Euler(
                    -90f,
                    0f,
                    0f));

            NadaRigMaintenance.ApplyPickupFix(
                rootObject);
        }

        internal static void RunRemote(
            NadaWeaponRigContext context,
            NadaWeaponMetadata metadata)
        {
            if (context == null || !context.IsValid)
                return;
            
            NadaRuntimeDiagnostics.RecordRemoteApply();

            GameObject rootObject =
                context.Root;

            Transform weaponVisualRootTransform =
                context.WeaponVisualRoot;

            if (!NadaRigCache.CacheReady)
            {
                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [RemoteRigSkip] cache not ready " +
                    $"root='{rootObject?.name}' " +
                    $"visual='{weaponVisualRootTransform?.name}'");

                return;
            }

            if (!NadaWeaponTargets.IsTargetOrAttachClone(
                    rootObject))
            {
                return;
            }

            // Remote weapons get behavior from replicated VfxState.
            // The vanilla item hash only provides static metadata needed
            // for things like weapon alignment.
            NadaWeaponRigAlignment alignment =
                NadaWeaponRigAlignmentResolver.Resolve(
                    metadata,
                    weaponVisualRootTransform);

            Transform localWeaponRootTransform =
                NadaRigRootAssembly.EnsureAttachedLocalWeaponBranch(
                    weaponVisualRootTransform,
                    rootObject.name,
                    alignment);

            if (localWeaponRootTransform == null)
                return;

            NadaOrbitalsRigAssembly.EnsureCompleteOrbitalsRig(
                localWeaponRootTransform,
                context.State,
                rootObject.name);

            NadaFlamesRigAssembly.EnsureLocalFlameBranch(
                localWeaponRootTransform,
                rootObject.name);

            NadaSparksRigAssembly.EnsureLocalSparksBranch(
                localWeaponRootTransform,
                rootObject.name);

            NadaStrandsRigAssembly.EnsureLocalStrandsBranch(
                localWeaponRootTransform,
                rootObject.name);

            NadaAuraRigAssembly.EnsureLocalAuraBranch(
                localWeaponRootTransform,
                weaponVisualRootTransform,
                rootObject.name);

            NadaRigCatalog catalog =
                NadaRigCatalog.Build(
                    localWeaponRootTransform);

            if (catalog == null)
                return;

            NadaEffectBinder.BindInnerFlamesEffect(
                catalog.InnerFlamesTransform,
                context.State);

            if (catalog.OuterFlamesTransform != null)
            {
                catalog.OuterFlamesTransform
                    .gameObject
                    .SetActive(true);

                NadaEffectBinder.BindOuterFlamesEffect(
                    catalog.OuterFlamesTransform,
                    context.State);

                NadaMotionBinder.BindOuterFlamesMotion(
                    catalog.OuterFlamesTransform,
                    context.State);
            }

            if (catalog.FlareTransform != null)
            {
                catalog.FlareTransform
                    .gameObject
                    .SetActive(true);

                NadaEffectBinder.BindFlareEffect(
                    catalog.FlareTransform,
                    context.State);
            }

            if (catalog.SparksTransform != null)
            {
                catalog.SparksTransform
                    .gameObject
                    .SetActive(true);

                NadaEffectBinder.BindSparksEffect(
                    catalog.SparksTransform,
                    context.State);
            }

            if (catalog.StrandsTransform != null)
            {
                catalog.StrandsTransform
                    .gameObject
                    .SetActive(true);

                NadaEffectBinder.BindStrandsEffect(
                    catalog.StrandsTransform,
                    context.State);
            }

            if (catalog.AuraTransform != null)
            {
                catalog.AuraTransform
                    .gameObject
                    .SetActive(true);

                NadaEffectBinder.BindAuraEffect(
                    catalog.AuraTransform,
                    context.State);
            }

            if (catalog.OrbitalsRootTransform != null)
            {
                NadaEffectBinder.BindOrbitalsEffect(
                    catalog.OrbitalsRootTransform,
                    catalog.OrbitalsOrbsRootTransform,
                    context.State);
            }

            NadaRigTransformApplier.Apply(
                localWeaponRootTransform,
                context.State);

            NadaMotionBinder.BindOrbitalsRigFollow(
                catalog.OrbitalsRigRootTransform,
                localWeaponRootTransform,
                Vector3.zero,
                Quaternion.Euler(
                    -90f,
                    0f,
                    0f));

            NadaLogControl.Info(
                $"remote-rig-applied:{rootObject.GetInstanceID()}",
                $"{Plugin.ModName}: [RemoteRigApplied] " +
                $"root='{rootObject.name}' " +
                $"visual='{weaponVisualRootTransform.name}' " +
                $"item='{metadata.ItemName}' " +
                $"type='{metadata.ItemType}' " +
                $"inner={context.State.InnerFlamesEnabled} " +
                $"outer={context.State.OuterFlamesEnabled} " +
                $"flare={context.State.FlareEnabled} " +
                $"sparks={context.State.SparksEnabled} " +
                $"strands={context.State.StrandsEnabled} " +
                $"aura={context.State.AuraEnabled} " +
                $"orbs={context.State.OrbitalsOrbsEnabled} " +
                $"cores={context.State.OrbitalsCoresEnabled} " +
                $"flames={context.State.OrbitalsFlamesEnabled} " +
                $"embers={context.State.OrbitalsEmbersEnabled}");
        }
    }
}