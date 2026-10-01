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
        // Temporary second instance used only to prove that the block runtime
        // can own more than one effect of the same type.
        private const uint PrototypeSecondSparksInstanceId = 1004;

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

            bool hasBoundSparksState =
                itemData != null &&
                VfxStateIO.IsBound(itemData);

            if (hasBoundSparksState)
            {
                // Block mode owns Sparks now. Make sure a legacy direct branch
                // cannot survive an unbound -> bound transition.
                NadaSparksRigAssembly.RemoveDirectSparksBranch(
                    localEffectsRootTransform,
                    rootObject.name);
            }
            else
            {
                NadaSparksRigAssembly.EnsureLocalSparksBranch(
                    localWeaponRootTransform,
                    rootObject.name);
            }

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

            if (hasBoundSparksState)
            {
                WeaponVfxState blockState =
                    CreateSparksPrototypeState(
                        context,
                        "local",
                        includeSecondPrototype: true);

                BindSparksBlocks(
                    localEffectsRootTransform,
                    blockState,
                    rootObject.name);
            }
            else
            {
                // Legacy mode owns Sparks now. Any block-driven Sparks instances
                // from the previous bound state are stale.
                NadaEffectInstanceAssembly.ReconcileInstancesOfType(
                    localEffectsRootTransform,
                    null,
                    VfxEffectTypeIds.Sparks,
                    rootObject.name);

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

            Transform remoteEffectsRootTransform =
                NadaRigPaths.FindLocalEffectsRoot(
                    localWeaponRootTransform);

            NadaSparksRigAssembly.RemoveDirectSparksBranch(
                remoteEffectsRootTransform,
                rootObject.name);

            WeaponVfxState blockState =
                CreateSparksPrototypeState(
                    context,
                    "remote",
                    includeSecondPrototype: false);

            BindSparksBlocks(
                remoteEffectsRootTransform,
                blockState,
                rootObject.name);

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

        private static void BindSparksBlocks(
            Transform localEffectsRootTransform,
            WeaponVfxState state,
            string ownerNameForLogs)
        {
            if (localEffectsRootTransform == null)
                return;

            NadaEffectInstanceAssembly.ReconcileInstancesOfType(
                localEffectsRootTransform,
                state,
                VfxEffectTypeIds.Sparks,
                ownerNameForLogs);

            if (state?.Effects == null)
                return;

            foreach (VfxEffectBlock block in state.Effects)
            {
                if (block == null)
                    continue;

                if (!string.Equals(
                        block.TypeId,
                        VfxEffectTypeIds.Sparks,
                        System.StringComparison.Ordinal))
                {
                    continue;
                }

                NadaEffectInstance sparksInstance =
                    NadaEffectInstanceAssembly.EnsureInstance(
                        localEffectsRootTransform,
                        block.InstanceId,
                        block.TypeId,
                        ownerNameForLogs);

                if (sparksInstance == null ||
                    !sparksInstance.IsValid)
                {
                    continue;
                }

                Transform sparksTransform =
                    NadaSparksRigAssembly.EnsureSparksBranch(
                        sparksInstance.RootTransform,
                        ownerNameForLogs);

                if (sparksTransform == null)
                    continue;

                sparksTransform
                    .gameObject
                    .SetActive(true);

                NadaEffectBinder.BindSparksBlockEffect(
                    sparksTransform,
                    block);
            }
        }

        private static WeaponVfxState CreateSparksPrototypeState(
            NadaWeaponRigContext context,
            string source,
            bool includeSecondPrototype)
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
                    $"Sparks prototype did not contain exactly one migrated effect block.");

                return null;
            }

            VfxEffectBlock legacyBlock =
                blockState.Effects[0];

            if (legacyBlock == null ||
                legacyBlock.Transform == null ||
                legacyBlock.Settings is not SparksVfxSettings legacySparks)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [BlockPrototype] " +
                    $"Sparks prototype contained invalid settings.");

                return null;
            }

            VfxState legacy =
                context.State;

            bool matchesLegacy =
                legacyBlock.InstanceId == 4 &&
                legacyBlock.TypeId == VfxEffectTypeIds.Sparks &&
                legacyBlock.Enabled == legacy.SparksEnabled &&

                legacyBlock.Transform.XOffset == legacy.SparksXOffset &&
                legacyBlock.Transform.YOffset == legacy.SparksYOffset &&
                legacyBlock.Transform.ZOffset == legacy.SparksZOffset &&

                legacyBlock.Transform.XRotation == legacy.SparksXRotation &&
                legacyBlock.Transform.YRotation == legacy.SparksYRotation &&
                legacyBlock.Transform.ZRotation == legacy.SparksZRotation &&

                legacySparks.Energy == legacy.SparksEnergy &&
                legacySparks.Scale == legacy.SparksScale &&
                legacySparks.Luminance == legacy.SparksLuminance &&
                legacySparks.Hue == legacy.SparksHue &&
                legacySparks.Lifetime == legacy.SparksLifetime &&
                legacySparks.SimulationSpeed == legacy.SparksSimulationSpeed &&
                legacySparks.Length == legacy.SparksLength &&
                legacySparks.Width == legacy.SparksWidth;

            NadaLogControl.Info(
                $"block-prototype:sparks:{source}:{context.Root.GetInstanceID()}",
                $"{Plugin.ModName}: [BlockPrototype] " +
                $"source='{source}' " +
                $"root='{context.Root.name}' " +
                $"type='{legacyBlock.TypeId}' " +
                $"id={legacyBlock.InstanceId} " +
                $"enabled={legacyBlock.Enabled} " +
                $"hue={legacySparks.Hue} " +
                $"scale={legacySparks.Scale} " +
                $"position=({legacyBlock.Transform.XOffset}, " +
                $"{legacyBlock.Transform.YOffset}, " +
                $"{legacyBlock.Transform.ZOffset}) " +
                $"matchesLegacy={matchesLegacy}");

            if (!includeSecondPrototype)
                return blockState;

            VfxEffectBlock secondBlock =
                CreateSecondSparksPrototype(
                    legacyBlock,
                    legacySparks);

            blockState.Effects.Add(
                secondBlock);

            SparksVfxSettings secondSparks =
                (SparksVfxSettings)secondBlock.Settings;

            NadaLogControl.Info(
                $"multi-sparks-prototype:{source}:{context.Root.GetInstanceID()}",
                $"{Plugin.ModName}: [MultiSparksPrototype] " +
                $"source='{source}' " +
                $"count={blockState.Effects.Count} " +
                $"firstId={legacyBlock.InstanceId} " +
                $"secondId={secondBlock.InstanceId} " +
                $"firstHue={legacySparks.Hue} " +
                $"secondHue={secondSparks.Hue} " +
                $"secondPosition=({secondBlock.Transform.XOffset}, " +
                $"{secondBlock.Transform.YOffset}, " +
                $"{secondBlock.Transform.ZOffset})");

            return blockState;
        }

        private static VfxEffectBlock CreateSecondSparksPrototype(
            VfxEffectBlock sourceBlock,
            SparksVfxSettings sourceSettings)
        {
            // This is intentionally an explicit deep copy.
            // Each effect block needs its own mutable settings/transform objects
            // or editing one instance would silently mutate the other.
            var secondTransform =
                new VfxTransformState
                {
                    XOffset =
                        sourceBlock.Transform.XOffset + 0.35f,

                    YOffset =
                        sourceBlock.Transform.YOffset,

                    ZOffset =
                        sourceBlock.Transform.ZOffset,

                    XRotation =
                        sourceBlock.Transform.XRotation,

                    YRotation =
                        sourceBlock.Transform.YRotation,

                    ZRotation =
                        sourceBlock.Transform.ZRotation
                };

            var secondSettings =
                new SparksVfxSettings
                {
                    Energy =
                        sourceSettings.Energy,

                    Scale =
                        sourceSettings.Scale * 0.8f,

                    Luminance =
                        sourceSettings.Luminance,

                    // Deliberately obvious for the runtime proof.
                    Hue =
                        sourceSettings.Hue >= 0f
                            ? -0.65f
                            : 0.65f,

                    Lifetime =
                        sourceSettings.Lifetime,

                    SimulationSpeed =
                        sourceSettings.SimulationSpeed,

                    Length =
                        sourceSettings.Length,

                    Width =
                        sourceSettings.Width
                };

            return new VfxEffectBlock
            {
                InstanceId =
                    PrototypeSecondSparksInstanceId,

                TypeId =
                    VfxEffectTypeIds.Sparks,

                Enabled =
                    sourceBlock.Enabled,

                Transform =
                    secondTransform,

                Settings =
                    secondSettings
            };
        }
    }
}