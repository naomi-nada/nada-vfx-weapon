using NADA.VFX.Weapon.Core.Debug;
using NADA.VFX.Weapon.Core.State;
using NADA.VFX.Weapon.Runtime.Binding;
using NADA.VFX.Weapon.Runtime.Structure;
using NADA.VFX.Weapon.Weapons.Targets;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Blocks.Effects;
using NADA.VFX.Weapon.Core.State.Migration;
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

            VfxEffectBlock sparksBlock =
                CreateSparksBlockPrototype(
                    context,
                    "local");

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

            if (itemData != null &&
                VfxStateIO.IsBound(itemData))
            {
                if (sparksBlock != null)
                {
                    NadaEffectInstanceAssembly.EnsureInstance(
                        localEffectsRootTransform,
                        sparksBlock.InstanceId,
                        sparksBlock.TypeId,
                        rootObject.name);
                }

                NadaEffectBinder.BindSparksBlockEffect(
                    catalog.SparksTransform,
                    sparksBlock);
            }
            else
            {
                // Keep the legacy source temporarily so unbound config editing
                // continues updating live while the block-state pipeline is built.
                NadaEffectBinder.BindSparksEffect(
                    catalog.SparksTransform,
                    itemData);
            }

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

        internal static bool RunRemote(
            NadaWeaponRigContext context,
            NadaWeaponMetadata metadata)
        {
            if (context == null || !context.IsValid)
                return false;

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

                return false;
            }

            if (!NadaWeaponTargets.IsTargetOrAttachClone(
                    rootObject))
            {
                return false;
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
                return false;

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

            if (catalog == null || !catalog.IsValid)
                return false;

            VfxEffectBlock sparksBlock =
                CreateSparksBlockPrototype(
                    context,
                    "remote");
            
            Transform remoteEffectsRootTransform =
                NadaRigPaths.FindLocalEffectsRoot(
                    localWeaponRootTransform);

            if (sparksBlock != null &&
                remoteEffectsRootTransform != null)
            {
                NadaEffectInstanceAssembly.EnsureInstance(
                    remoteEffectsRootTransform,
                    sparksBlock.InstanceId,
                    sparksBlock.TypeId,
                    rootObject.name);
            }

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

                NadaEffectBinder.BindSparksBlockEffect(
                    catalog.SparksTransform,
                    sparksBlock);
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

            return true;
        }
        
        private static VfxEffectBlock CreateSparksBlockPrototype(
            NadaWeaponRigContext context,
            string source)
        {
            if (context == null ||
                context.Root == null)
            {
                return null;
            }

            WeaponVfxState blockState =
                LegacyVfxStateAdapter.CreateSparksPrototype(
                    context.State);

            if (blockState?.Effects == null ||
                blockState.Effects.Count != 1)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [BlockPrototype] " +
                    $"Sparks prototype did not contain exactly one effect block.");

                return null;
            }

            VfxEffectBlock block =
                blockState.Effects[0];

            if (block == null ||
                block.Settings is not SparksVfxSettings sparks)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [BlockPrototype] " +
                    $"Sparks prototype contained invalid settings.");

                return null;
            }

            VfxState legacy =
                context.State;

            bool matchesLegacy =
                block.InstanceId == 4 &&
                block.TypeId == VfxEffectTypeIds.Sparks &&
                block.Enabled == legacy.SparksEnabled &&

                block.Transform.XOffset == legacy.SparksXOffset &&
                block.Transform.YOffset == legacy.SparksYOffset &&
                block.Transform.ZOffset == legacy.SparksZOffset &&

                block.Transform.XRotation == legacy.SparksXRotation &&
                block.Transform.YRotation == legacy.SparksYRotation &&
                block.Transform.ZRotation == legacy.SparksZRotation &&

                sparks.Energy == legacy.SparksEnergy &&
                sparks.Scale == legacy.SparksScale &&
                sparks.Luminance == legacy.SparksLuminance &&
                sparks.Hue == legacy.SparksHue &&
                sparks.Lifetime == legacy.SparksLifetime &&
                sparks.SimulationSpeed == legacy.SparksSimulationSpeed &&
                sparks.Length == legacy.SparksLength &&
                sparks.Width == legacy.SparksWidth;

            NadaLogControl.Info(
                $"block-prototype:sparks:{source}:{context.Root.GetInstanceID()}",
                $"{Plugin.ModName}: [BlockPrototype] " +
                $"source='{source}' " +
                $"root='{context.Root.name}' " +
                $"type='{block.TypeId}' " +
                $"id={block.InstanceId} " +
                $"enabled={block.Enabled} " +
                $"hue={sparks.Hue} " +
                $"scale={sparks.Scale} " +
                $"position=({block.Transform.XOffset}, " +
                $"{block.Transform.YOffset}, " +
                $"{block.Transform.ZOffset}) " +
                $"matchesLegacy={matchesLegacy}");

            return block;
        }
    }
}