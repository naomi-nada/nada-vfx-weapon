using NADA.VFX.Weapon.Core.Debug;
using NADA.VFX.Weapon.Core.State;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Blocks.Effects;
using NADA.VFX.Weapon.Core.State.Migration;
using NADA.VFX.Weapon.Runtime.Binding;
using NADA.VFX.Weapon.Runtime.Structure;
using NADA.VFX.Weapon.Weapons.Targets;
using UnityEngine;

namespace NADA.VFX.Weapon.Weapons.Runtime
{
    internal static class NadaWeaponRigOrchestrator
    {
        private const uint PrototypeLegacyInnerFlamesInstanceId = 1;
        private const uint PrototypeLegacyStrandsInstanceId = 3;
        private const uint PrototypeLegacySparksInstanceId = 4;
        private const uint PrototypeLegacyFlareInstanceId = 5;

        internal static void Run(
            NadaWeaponRigContext context)
        {
            if (context == null ||
                !context.IsValid)
            {
                return;
            }

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

            NadaFlamesRigAssembly.EnsureLocalFlameBranch(
                localWeaponRootTransform,
                rootObject.name);

            bool hasBoundBlockState =
                itemData != null &&
                VfxStateIO.IsBound(itemData);

            if (hasBoundBlockState)
            {
                // Migrated effects belong exclusively to block-owned
                // instance roots while the item is bound.
                NadaFlamesRigAssembly.RemoveDirectInnerFlamesBranch(
                    localEffectsRootTransform,
                    rootObject.name);

                NadaStrandsRigAssembly.RemoveDirectStrandsBranch(
                    localEffectsRootTransform,
                    rootObject.name);

                NadaSparksRigAssembly.RemoveDirectSparksBranch(
                    localEffectsRootTransform,
                    rootObject.name);

                NadaFlareRigAssembly.RemoveDirectFlareBranch(
                    localEffectsRootTransform,
                    rootObject.name);
            }
            else
            {
                // Unbound editing temporarily retains the legacy direct
                // hierarchy so config remains the live preview authority.
                NadaStrandsRigAssembly.EnsureLocalStrandsBranch(
                    localWeaponRootTransform,
                    rootObject.name);

                NadaSparksRigAssembly.EnsureLocalSparksBranch(
                    localWeaponRootTransform,
                    rootObject.name);

                NadaFlareRigAssembly.EnsureFlareBranch(
                    localEffectsRootTransform,
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

            NadaEffectBinder.BindOuterFlamesEffect(
                catalog.OuterFlamesTransform,
                itemData);

            NadaMotionBinder.BindOuterFlamesMotion(
                catalog.OuterFlamesTransform,
                itemData);

            if (hasBoundBlockState)
            {
                WeaponVfxState blockState =
                    CreateMigratedPrototypeState(
                        context,
                        "local");

                BindInnerFlamesBlocks(
                    localEffectsRootTransform,
                    blockState,
                    rootObject.name);

                BindStrandsBlocks(
                    localEffectsRootTransform,
                    blockState,
                    rootObject.name);

                BindSparksBlocks(
                    localEffectsRootTransform,
                    blockState,
                    rootObject.name);

                BindFlareBlocks(
                    localEffectsRootTransform,
                    blockState,
                    rootObject.name);

                NadaEffectInstanceAssembly.ReconcileInstanceOrder(
                    localEffectsRootTransform,
                    blockState,
                    rootObject.name);
            }
            else
            {
                // Legacy/config ownership means migrated instances left
                // from a previous bound state are stale.
                NadaEffectInstanceAssembly.ReconcileInstancesOfType(
                    localEffectsRootTransform,
                    null,
                    VfxEffectTypeIds.InnerFlames,
                    rootObject.name);

                NadaEffectInstanceAssembly.ReconcileInstancesOfType(
                    localEffectsRootTransform,
                    null,
                    VfxEffectTypeIds.Strands,
                    rootObject.name);

                NadaEffectInstanceAssembly.ReconcileInstancesOfType(
                    localEffectsRootTransform,
                    null,
                    VfxEffectTypeIds.Sparks,
                    rootObject.name);

                NadaEffectInstanceAssembly.ReconcileInstancesOfType(
                    localEffectsRootTransform,
                    null,
                    VfxEffectTypeIds.Flare,
                    rootObject.name);

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

                NadaEffectBinder.BindStrandsEffect(
                    catalog.StrandsTransform,
                    itemData);

                NadaEffectBinder.BindSparksEffect(
                    catalog.SparksTransform,
                    itemData);

                NadaEffectBinder.BindFlareEffect(
                    catalog.FlareTransform,
                    itemData);
            }

            NadaEffectBinder.BindAuraEffect(
                catalog.AuraTransform,
                itemData);

            NadaEffectBinder.BindOrbitalsEffect(
                catalog.OrbitalsRootTransform,
                catalog.OrbitalsOrbsRootTransform,
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
            if (context == null ||
                !context.IsValid)
            {
                return false;
            }

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

            Transform remoteEffectsRootTransform =
                NadaRigPaths.FindLocalEffectsRoot(
                    localWeaponRootTransform);

            if (remoteEffectsRootTransform == null)
                return false;

            // Remote migrated effects are block-owned too. Any direct
            // legacy branches must disappear before reconciliation so
            // there is only one owner of each visual.
            NadaFlamesRigAssembly.RemoveDirectInnerFlamesBranch(
                remoteEffectsRootTransform,
                rootObject.name);

            NadaStrandsRigAssembly.RemoveDirectStrandsBranch(
                remoteEffectsRootTransform,
                rootObject.name);

            NadaSparksRigAssembly.RemoveDirectSparksBranch(
                remoteEffectsRootTransform,
                rootObject.name);

            NadaFlareRigAssembly.RemoveDirectFlareBranch(
                remoteEffectsRootTransform,
                rootObject.name);

            NadaAuraRigAssembly.EnsureLocalAuraBranch(
                localWeaponRootTransform,
                weaponVisualRootTransform,
                rootObject.name);

            NadaRigCatalog catalog =
                NadaRigCatalog.Build(
                    localWeaponRootTransform);

            if (catalog == null ||
                !catalog.IsValid)
            {
                return false;
            }

            WeaponVfxState blockState =
                CreateMigratedPrototypeState(
                    context,
                    "remote");

            BindInnerFlamesBlocks(
                remoteEffectsRootTransform,
                blockState,
                rootObject.name);

            BindStrandsBlocks(
                remoteEffectsRootTransform,
                blockState,
                rootObject.name);

            BindSparksBlocks(
                remoteEffectsRootTransform,
                blockState,
                rootObject.name);

            BindFlareBlocks(
                remoteEffectsRootTransform,
                blockState,
                rootObject.name);

            // One global ordering pass after every currently migrated
            // effect type has reconciled its own instances.
            NadaEffectInstanceAssembly.ReconcileInstanceOrder(
                remoteEffectsRootTransform,
                blockState,
                rootObject.name);

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

        private static void BindInnerFlamesBlocks(
            Transform localEffectsRootTransform,
            WeaponVfxState state,
            string ownerNameForLogs)
        {
            if (localEffectsRootTransform == null)
                return;

            NadaEffectInstanceAssembly.ReconcileInstancesOfType(
                localEffectsRootTransform,
                state,
                VfxEffectTypeIds.InnerFlames,
                ownerNameForLogs);

            if (state?.Effects == null)
                return;

            foreach (VfxEffectBlock block in
                     state.Effects)
            {
                if (block == null)
                    continue;

                if (!string.Equals(
                        block.TypeId,
                        VfxEffectTypeIds.InnerFlames,
                        System.StringComparison.Ordinal))
                {
                    continue;
                }

                NadaEffectInstance innerFlamesInstance =
                    NadaEffectInstanceAssembly.EnsureInstance(
                        localEffectsRootTransform,
                        block.InstanceId,
                        block.TypeId,
                        ownerNameForLogs);

                if (innerFlamesInstance == null ||
                    !innerFlamesInstance.IsValid)
                {
                    continue;
                }

                Transform innerFlamesTransform =
                    NadaFlamesRigAssembly.EnsureInnerFlamesBranch(
                        innerFlamesInstance.RootTransform,
                        ownerNameForLogs);

                if (innerFlamesTransform == null)
                    continue;

                NadaEffectBinder.BindInnerFlamesBlockEffect(
                    innerFlamesTransform,
                    block);

                innerFlamesTransform
                    .gameObject
                    .SetActive(true);
            }
        }

        private static void BindStrandsBlocks(
            Transform localEffectsRootTransform,
            WeaponVfxState state,
            string ownerNameForLogs)
        {
            if (localEffectsRootTransform == null)
                return;

            NadaEffectInstanceAssembly.ReconcileInstancesOfType(
                localEffectsRootTransform,
                state,
                VfxEffectTypeIds.Strands,
                ownerNameForLogs);

            if (state?.Effects == null)
                return;

            foreach (VfxEffectBlock block in
                     state.Effects)
            {
                if (block == null)
                    continue;

                if (!string.Equals(
                        block.TypeId,
                        VfxEffectTypeIds.Strands,
                        System.StringComparison.Ordinal))
                {
                    continue;
                }

                NadaEffectInstance strandsInstance =
                    NadaEffectInstanceAssembly.EnsureInstance(
                        localEffectsRootTransform,
                        block.InstanceId,
                        block.TypeId,
                        ownerNameForLogs);

                if (strandsInstance == null ||
                    !strandsInstance.IsValid)
                {
                    continue;
                }

                Transform strandsTransform =
                    NadaStrandsRigAssembly.EnsureStrandsBranch(
                        strandsInstance.RootTransform,
                        ownerNameForLogs);

                if (strandsTransform == null)
                    continue;

                NadaEffectBinder.BindStrandsBlockEffect(
                    strandsTransform,
                    block);

                strandsTransform
                    .gameObject
                    .SetActive(true);
            }
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

            foreach (VfxEffectBlock block in
                     state.Effects)
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

        private static void BindFlareBlocks(
            Transform localEffectsRootTransform,
            WeaponVfxState state,
            string ownerNameForLogs)
        {
            if (localEffectsRootTransform == null)
                return;

            NadaEffectInstanceAssembly.ReconcileInstancesOfType(
                localEffectsRootTransform,
                state,
                VfxEffectTypeIds.Flare,
                ownerNameForLogs);

            if (state?.Effects == null)
                return;

            foreach (VfxEffectBlock block in
                     state.Effects)
            {
                if (block == null)
                    continue;

                if (!string.Equals(
                        block.TypeId,
                        VfxEffectTypeIds.Flare,
                        System.StringComparison.Ordinal))
                {
                    continue;
                }

                NadaEffectInstance flareInstance =
                    NadaEffectInstanceAssembly.EnsureInstance(
                        localEffectsRootTransform,
                        block.InstanceId,
                        block.TypeId,
                        ownerNameForLogs);

                if (flareInstance == null ||
                    !flareInstance.IsValid)
                {
                    continue;
                }

                Transform flareTransform =
                    NadaFlareRigAssembly.EnsureFlareBranch(
                        flareInstance.RootTransform,
                        ownerNameForLogs);

                if (flareTransform == null)
                    continue;

                NadaEffectBinder.BindFlareBlockEffect(
                    flareTransform,
                    block);

                flareTransform
                    .gameObject
                    .SetActive(true);
            }
        }

        private static WeaponVfxState CreateMigratedPrototypeState(
            NadaWeaponRigContext context,
            string source)
        {
            WeaponVfxState innerFlamesState =
                CreateInnerFlamesPrototypeState(
                    context,
                    source);

            WeaponVfxState strandsState =
                CreateStrandsPrototypeState(
                    context,
                    source);

            WeaponVfxState sparksState =
                CreateSparksPrototypeState(
                    context,
                    source);

            WeaponVfxState flareState =
                CreateFlarePrototypeState(
                    context,
                    source);

            if (innerFlamesState?.Effects == null ||
                innerFlamesState.Effects.Count != 1 ||
                strandsState?.Effects == null ||
                strandsState.Effects.Count != 1 ||
                sparksState?.Effects == null ||
                sparksState.Effects.Count != 1 ||
                flareState?.Effects == null ||
                flareState.Effects.Count != 1)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [BlockPrototypeState] " +
                    $"Could not build combined Inner Flames + Strands + Sparks + Flare prototype.");

                return null;
            }

            innerFlamesState.Effects.Add(
                strandsState.Effects[0]);

            innerFlamesState.Effects.Add(
                sparksState.Effects[0]);

            innerFlamesState.Effects.Add(
                flareState.Effects[0]);

            NadaLogControl.Info(
                $"block-prototype-state:{source}:{context.Root.GetInstanceID()}",
                $"{Plugin.ModName}: [BlockPrototypeState] " +
                $"source='{source}' " +
                $"root='{context.Root.name}' " +
                $"effects={innerFlamesState.Effects.Count} " +
                $"order='[1:inner_flames,3:strands,4:sparks,5:flare]'");

            return innerFlamesState;
        }

        private static WeaponVfxState CreateInnerFlamesPrototypeState(
            NadaWeaponRigContext context,
            string source)
        {
            if (context == null ||
                context.Root == null)
            {
                return null;
            }

            WeaponVfxState blockState =
                LegacyVfxStateAdapter.CreateInnerFlamesPrototype(
                    context.State);

            if (blockState?.Effects == null ||
                blockState.Effects.Count != 1)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [InnerFlamesBlockPrototype] " +
                    $"Inner Flames prototype did not contain exactly one migrated effect block.");

                return null;
            }

            VfxEffectBlock block =
                blockState.Effects[0];

            if (block == null ||
                block.Transform == null ||
                block.Settings is not InnerFlamesVfxSettings inner)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [InnerFlamesBlockPrototype] " +
                    $"Inner Flames prototype contained invalid settings.");

                return null;
            }

            VfxState legacy =
                context.State;

            bool matchesLegacy =
                block.InstanceId ==
                    PrototypeLegacyInnerFlamesInstanceId &&
                block.TypeId ==
                    VfxEffectTypeIds.InnerFlames &&
                block.Enabled ==
                    legacy.InnerFlamesEnabled &&

                block.Transform.XOffset ==
                    legacy.InnerFlamesXOffset &&
                block.Transform.YOffset ==
                    legacy.InnerFlamesYOffset &&
                block.Transform.ZOffset ==
                    legacy.InnerFlamesZOffset &&

                block.Transform.XRotation ==
                    legacy.InnerFlamesXRotation &&
                block.Transform.YRotation ==
                    legacy.InnerFlamesYRotation &&
                block.Transform.ZRotation ==
                    legacy.InnerFlamesZRotation &&

                inner.WorldEnabled ==
                    legacy.InnerFlamesWorldEnabled &&
                inner.BlackEnabled ==
                    legacy.InnerFlamesBlackEnabled &&
                inner.WhiteEnabled ==
                    legacy.InnerFlamesWhiteEnabled &&

                inner.Energy ==
                    legacy.InnerFlamesEnergy &&
                inner.Scale ==
                    legacy.InnerFlamesScale &&
                inner.Luminance ==
                    legacy.InnerFlamesLuminance &&
                inner.Hue ==
                    legacy.InnerFlamesHue &&
                inner.Lifetime ==
                    legacy.InnerFlamesLifetime &&
                inner.SimulationSpeed ==
                    legacy.InnerFlamesSimulationSpeed &&
                inner.Length ==
                    legacy.InnerFlamesLength &&
                inner.Width ==
                    legacy.InnerFlamesWidth;

            NadaLogControl.Info(
                $"block-prototype:inner-flames:{source}:{context.Root.GetInstanceID()}",
                $"{Plugin.ModName}: [InnerFlamesBlockPrototype] " +
                $"source='{source}' " +
                $"root='{context.Root.name}' " +
                $"type='{block.TypeId}' " +
                $"id={block.InstanceId} " +
                $"enabled={block.Enabled} " +
                $"world={inner.WorldEnabled} " +
                $"black={inner.BlackEnabled} " +
                $"white={inner.WhiteEnabled} " +
                $"hue={inner.Hue} " +
                $"scale={inner.Scale} " +
                $"position=({block.Transform.XOffset}, " +
                $"{block.Transform.YOffset}, " +
                $"{block.Transform.ZOffset}) " +
                $"matchesLegacy={matchesLegacy}");

            return blockState;
        }

        private static WeaponVfxState CreateStrandsPrototypeState(
            NadaWeaponRigContext context,
            string source)
        {
            if (context == null ||
                context.Root == null)
            {
                return null;
            }

            WeaponVfxState blockState =
                LegacyVfxStateAdapter.CreateStrandsPrototype(
                    context.State);

            if (blockState?.Effects == null ||
                blockState.Effects.Count != 1)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [StrandsBlockPrototype] " +
                    $"Strands prototype did not contain exactly one migrated effect block.");

                return null;
            }

            VfxEffectBlock block =
                blockState.Effects[0];

            if (block == null ||
                block.Transform == null ||
                block.Settings is not StrandsVfxSettings strands)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [StrandsBlockPrototype] " +
                    $"Strands prototype contained invalid settings.");

                return null;
            }

            VfxState legacy =
                context.State;

            bool matchesLegacy =
                block.InstanceId ==
                    PrototypeLegacyStrandsInstanceId &&
                block.TypeId ==
                    VfxEffectTypeIds.Strands &&
                block.Enabled ==
                    legacy.StrandsEnabled &&

                block.Transform.XOffset ==
                    legacy.StrandsXOffset &&
                block.Transform.YOffset ==
                    legacy.StrandsYOffset &&
                block.Transform.ZOffset ==
                    legacy.StrandsZOffset &&

                block.Transform.XRotation ==
                    legacy.StrandsXRotation &&
                block.Transform.YRotation ==
                    legacy.StrandsYRotation &&
                block.Transform.ZRotation ==
                    legacy.StrandsZRotation &&

                strands.SpectrumEnabled ==
                    legacy.StrandsSpectrumEnabled &&
                strands.Energy ==
                    legacy.StrandsEnergy &&
                strands.ScaleWhole ==
                    legacy.StrandsScaleWhole &&
                strands.ScaleParts ==
                    legacy.StrandsScaleParts &&
                strands.Luminance ==
                    legacy.StrandsLuminance &&
                strands.Hue ==
                    legacy.StrandsHue &&
                strands.Lifetime ==
                    legacy.StrandsLifetime &&
                strands.Length ==
                    legacy.StrandsLength &&
                strands.SpectrumSpeed ==
                    legacy.StrandsSpectrumSpeed &&
                strands.Speed ==
                    legacy.StrandsSpeed &&
                strands.Radius ==
                    legacy.StrandsRadius &&
                strands.Drift ==
                    legacy.StrandsDrift;

            NadaLogControl.Info(
                $"block-prototype:strands:{source}:{context.Root.GetInstanceID()}",
                $"{Plugin.ModName}: [StrandsBlockPrototype] " +
                $"source='{source}' " +
                $"root='{context.Root.name}' " +
                $"type='{block.TypeId}' " +
                $"id={block.InstanceId} " +
                $"enabled={block.Enabled} " +
                $"spectrum={strands.SpectrumEnabled} " +
                $"hue={strands.Hue} " +
                $"scaleWhole={strands.ScaleWhole} " +
                $"scaleParts={strands.ScaleParts} " +
                $"position=({block.Transform.XOffset}, " +
                $"{block.Transform.YOffset}, " +
                $"{block.Transform.ZOffset}) " +
                $"matchesLegacy={matchesLegacy}");

            return blockState;
        }

        private static WeaponVfxState CreateSparksPrototypeState(
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
                    $"Sparks prototype did not contain exactly one migrated effect block.");

                return null;
            }

            VfxEffectBlock block =
                blockState.Effects[0];

            if (block == null ||
                block.Transform == null ||
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
                block.InstanceId ==
                    PrototypeLegacySparksInstanceId &&
                block.TypeId ==
                    VfxEffectTypeIds.Sparks &&
                block.Enabled ==
                    legacy.SparksEnabled &&

                block.Transform.XOffset ==
                    legacy.SparksXOffset &&
                block.Transform.YOffset ==
                    legacy.SparksYOffset &&
                block.Transform.ZOffset ==
                    legacy.SparksZOffset &&

                block.Transform.XRotation ==
                    legacy.SparksXRotation &&
                block.Transform.YRotation ==
                    legacy.SparksYRotation &&
                block.Transform.ZRotation ==
                    legacy.SparksZRotation &&

                sparks.Energy ==
                    legacy.SparksEnergy &&
                sparks.Scale ==
                    legacy.SparksScale &&
                sparks.Luminance ==
                    legacy.SparksLuminance &&
                sparks.Hue ==
                    legacy.SparksHue &&
                sparks.Lifetime ==
                    legacy.SparksLifetime &&
                sparks.SimulationSpeed ==
                    legacy.SparksSimulationSpeed &&
                sparks.Length ==
                    legacy.SparksLength &&
                sparks.Width ==
                    legacy.SparksWidth;

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

            return blockState;
        }

        private static WeaponVfxState CreateFlarePrototypeState(
            NadaWeaponRigContext context,
            string source)
        {
            if (context == null ||
                context.Root == null)
            {
                return null;
            }

            WeaponVfxState blockState =
                LegacyVfxStateAdapter.CreateFlarePrototype(
                    context.State);

            if (blockState?.Effects == null ||
                blockState.Effects.Count != 1)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [FlareBlockPrototype] " +
                    $"Flare prototype did not contain exactly one migrated effect block.");

                return null;
            }

            VfxEffectBlock block =
                blockState.Effects[0];

            if (block == null ||
                block.Transform == null ||
                block.Settings is not FlareVfxSettings flare)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [FlareBlockPrototype] " +
                    $"Flare prototype contained invalid settings.");

                return null;
            }

            VfxState legacy =
                context.State;

            bool matchesLegacy =
                block.InstanceId ==
                    PrototypeLegacyFlareInstanceId &&
                block.TypeId ==
                    VfxEffectTypeIds.Flare &&
                block.Enabled ==
                    legacy.FlareEnabled &&

                block.Transform.XOffset ==
                    legacy.FlareXOffset &&
                block.Transform.YOffset ==
                    legacy.FlareYOffset &&
                block.Transform.ZOffset ==
                    legacy.FlareZOffset &&

                block.Transform.XRotation == 0f &&
                block.Transform.YRotation == 0f &&
                block.Transform.ZRotation == 0f &&

                flare.Scale ==
                    legacy.FlareScale &&
                flare.Luminance ==
                    legacy.FlareLuminance &&
                flare.Hue ==
                    legacy.FlareHue;

            NadaLogControl.Info(
                $"block-prototype:flare:{source}:{context.Root.GetInstanceID()}",
                $"{Plugin.ModName}: [FlareBlockPrototype] " +
                $"source='{source}' " +
                $"root='{context.Root.name}' " +
                $"type='{block.TypeId}' " +
                $"id={block.InstanceId} " +
                $"enabled={block.Enabled} " +
                $"hue={flare.Hue} " +
                $"scale={flare.Scale} " +
                $"luminance={flare.Luminance} " +
                $"position=({block.Transform.XOffset}, " +
                $"{block.Transform.YOffset}, " +
                $"{block.Transform.ZOffset}) " +
                $"matchesLegacy={matchesLegacy}");

            return blockState;
        }
    }
}