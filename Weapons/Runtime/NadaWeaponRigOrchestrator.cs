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
        private const uint PrototypeLegacyOuterFlamesInstanceId = 2;
        private const uint PrototypeLegacyStrandsInstanceId = 3;
        private const uint PrototypeLegacySparksInstanceId = 4;
        private const uint PrototypeLegacyFlareInstanceId = 5;
        private const uint PrototypeLegacyAuraInstanceId = 6;
        private const uint PrototypeLegacyOrbitalsOrbsInstanceId = 7;

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

            bool hasBoundBlockState =
                itemData != null &&
                VfxStateIO.IsBound(itemData);

            if (hasBoundBlockState)
            {
                NadaOrbitalsRigAssembly
                    .EnsureLegacyOrbitalsRigWithoutOrbs(
                        localWeaponRootTransform,
                        itemData,
                        rootObject.name);
            }
            else
            {
                NadaOrbitalsRigAssembly.EnsureCompleteOrbitalsRig(
                    localWeaponRootTransform,
                    itemData,
                    rootObject.name);
            }

            if (hasBoundBlockState)
            {
                NadaFlamesRigAssembly.RemoveDirectOuterFlamesBranch(
                    localEffectsRootTransform,
                    rootObject.name);

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

                NadaAuraRigAssembly.RemoveDirectAuraBranch(
                    localEffectsRootTransform,
                    rootObject.name);
            }
            else
            {
                NadaFlamesRigAssembly.EnsureLocalFlameBranch(
                    localWeaponRootTransform,
                    rootObject.name);

                NadaStrandsRigAssembly.EnsureLocalStrandsBranch(
                    localWeaponRootTransform,
                    rootObject.name);

                NadaSparksRigAssembly.EnsureLocalSparksBranch(
                    localWeaponRootTransform,
                    rootObject.name);

                NadaFlareRigAssembly.EnsureFlareBranch(
                    localEffectsRootTransform,
                    rootObject.name);

                NadaAuraRigAssembly.EnsureLocalAuraBranch(
                    localWeaponRootTransform,
                    weaponVisualRootTransform,
                    rootObject.name);
            }

            NadaRigCatalog catalog =
                NadaRigCatalog.Build(
                    localWeaponRootTransform);

            if (catalog == null)
                return;

            WeaponVfxState blockState =
                null;

            if (hasBoundBlockState)
            {
                blockState =
                    CreateMigratedPrototypeState(
                        context,
                        "local");

                BindInnerFlamesBlocks(
                    localEffectsRootTransform,
                    blockState,
                    rootObject.name);

                BindOuterFlamesBlocks(
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

                BindAuraBlocks(
                    localEffectsRootTransform,
                    weaponVisualRootTransform,
                    blockState,
                    rootObject.name);

                BindOrbitalsOrbsBlocks(
                    localEffectsRootTransform,
                    localWeaponRootTransform,
                    catalog.OrbitalsRigRootTransform,
                    blockState,
                    rootObject.name);

                NadaEffectInstanceAssembly.ReconcileInstanceOrder(
                    localEffectsRootTransform,
                    blockState,
                    rootObject.name);
            }
            else
            {
                NadaEffectInstanceAssembly.ReconcileInstancesOfType(
                    localEffectsRootTransform,
                    null,
                    VfxEffectTypeIds.InnerFlames,
                    rootObject.name);

                NadaEffectInstanceAssembly.ReconcileInstancesOfType(
                    localEffectsRootTransform,
                    null,
                    VfxEffectTypeIds.OuterFlames,
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

                NadaEffectInstanceAssembly.ReconcileInstancesOfType(
                    localEffectsRootTransform,
                    null,
                    VfxEffectTypeIds.Aura,
                    rootObject.name);

                NadaEffectInstanceAssembly.ReconcileInstancesOfType(
                    localEffectsRootTransform,
                    null,
                    VfxEffectTypeIds.OrbitalsOrbs,
                    rootObject.name);

                NadaEffectBinder.BindOuterFlamesEffect(
                    catalog.OuterFlamesTransform,
                    itemData);

                NadaMotionBinder.BindOuterFlamesMotion(
                    catalog.OuterFlamesTransform,
                    itemData);

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

                NadaEffectBinder.BindAuraEffect(
                    catalog.AuraTransform,
                    itemData);

                ClearLegacyOrbitalsGlueSources(
                    catalog.OrbitalsRigRootTransform);
            }

            if (hasBoundBlockState)
            {
                NadaEffectBinder.BindOrbitalsEffect(
                    catalog.OrbitalsRootTransform,
                    null,
                    itemData);
            }
            else
            {
                NadaEffectBinder.BindOrbitalsEffect(
                    catalog.OrbitalsRootTransform,
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

            // 57E: remote Orbs now uses the same block-owned structure as
            // local bound Orbs. The shared hierarchy remains only for the
            // still-legacy Cores, Flames and Embers.
            NadaOrbitalsRigAssembly
                .EnsureLegacyOrbitalsRigWithoutOrbs(
                    localWeaponRootTransform,
                    context.State,
                    rootObject.name);

            Transform remoteEffectsRootTransform =
                NadaRigPaths.FindLocalEffectsRoot(
                    localWeaponRootTransform);

            if (remoteEffectsRootTransform == null)
                return false;

            NadaFlamesRigAssembly.RemoveDirectOuterFlamesBranch(
                remoteEffectsRootTransform,
                rootObject.name);

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

            NadaAuraRigAssembly.RemoveDirectAuraBranch(
                remoteEffectsRootTransform,
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

            BindOuterFlamesBlocks(
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

            BindAuraBlocks(
                remoteEffectsRootTransform,
                weaponVisualRootTransform,
                blockState,
                rootObject.name);

            // 57E: the remote ID-7 block now owns Orbs visual structure,
            // motion structure and pool exactly like the local path.
            BindOrbitalsOrbsBlocks(
                remoteEffectsRootTransform,
                localWeaponRootTransform,
                catalog.OrbitalsRigRootTransform,
                blockState,
                rootObject.name);

            NadaEffectInstanceAssembly.ReconcileInstanceOrder(
                remoteEffectsRootTransform,
                blockState,
                rootObject.name);

            // Shared remote Orbitals behavior now owns only the remaining
            // legacy Cores/Flames/Embers groups.
            if (catalog.OrbitalsRootTransform != null)
            {
                NadaEffectBinder.BindOrbitalsEffect(
                    catalog.OrbitalsRootTransform,
                    null,
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

        private static void BindAuraBlocks(
            Transform localEffectsRootTransform,
            Transform weaponVisualRootTransform,
            WeaponVfxState state,
            string ownerNameForLogs)
        {
            if (localEffectsRootTransform == null ||
                weaponVisualRootTransform == null)
            {
                return;
            }

            NadaEffectInstanceAssembly.ReconcileInstancesOfType(
                localEffectsRootTransform,
                state,
                VfxEffectTypeIds.Aura,
                ownerNameForLogs);

            if (state?.Effects == null)
                return;

            foreach (VfxEffectBlock block in state.Effects)
            {
                if (block == null)
                    continue;

                if (!string.Equals(
                        block.TypeId,
                        VfxEffectTypeIds.Aura,
                        System.StringComparison.Ordinal))
                {
                    continue;
                }

                NadaEffectInstance instance =
                    NadaEffectInstanceAssembly.EnsureInstance(
                        localEffectsRootTransform,
                        block.InstanceId,
                        block.TypeId,
                        ownerNameForLogs);

                if (instance == null ||
                    !instance.IsValid)
                {
                    continue;
                }

                Transform auraTransform =
                    NadaAuraRigAssembly.EnsureAuraBranch(
                        instance.RootTransform,
                        weaponVisualRootTransform,
                        ownerNameForLogs);

                if (auraTransform == null)
                    continue;

                NadaEffectBinder.BindAuraBlockEffect(
                    auraTransform,
                    block);

                auraTransform
                    .gameObject
                    .SetActive(true);
            }
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

            foreach (VfxEffectBlock block in state.Effects)
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

                NadaEffectInstance instance =
                    NadaEffectInstanceAssembly.EnsureInstance(
                        localEffectsRootTransform,
                        block.InstanceId,
                        block.TypeId,
                        ownerNameForLogs);

                if (instance == null ||
                    !instance.IsValid)
                {
                    continue;
                }

                Transform effectTransform =
                    NadaFlamesRigAssembly.EnsureInnerFlamesBranch(
                        instance.RootTransform,
                        ownerNameForLogs);

                if (effectTransform == null)
                    continue;

                NadaEffectBinder.BindInnerFlamesBlockEffect(
                    effectTransform,
                    block);

                effectTransform
                    .gameObject
                    .SetActive(true);
            }
        }

        private static void BindOuterFlamesBlocks(
            Transform localEffectsRootTransform,
            WeaponVfxState state,
            string ownerNameForLogs)
        {
            if (localEffectsRootTransform == null)
                return;

            NadaEffectInstanceAssembly.ReconcileInstancesOfType(
                localEffectsRootTransform,
                state,
                VfxEffectTypeIds.OuterFlames,
                ownerNameForLogs);

            if (state?.Effects == null)
                return;

            foreach (VfxEffectBlock block in state.Effects)
            {
                if (block == null)
                    continue;

                if (!string.Equals(
                        block.TypeId,
                        VfxEffectTypeIds.OuterFlames,
                        System.StringComparison.Ordinal))
                {
                    continue;
                }

                NadaEffectInstance instance =
                    NadaEffectInstanceAssembly.EnsureInstance(
                        localEffectsRootTransform,
                        block.InstanceId,
                        block.TypeId,
                        ownerNameForLogs);

                if (instance == null ||
                    !instance.IsValid)
                {
                    continue;
                }

                Transform effectTransform =
                    NadaFlamesRigAssembly.EnsureOuterFlamesBranch(
                        instance.RootTransform,
                        ownerNameForLogs);

                if (effectTransform == null)
                    continue;

                NadaEffectBinder.BindOuterFlamesBlockEffect(
                    effectTransform,
                    block);

                NadaMotionBinder.BindOuterFlamesBlockMotion(
                    effectTransform,
                    block);

                effectTransform
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

            foreach (VfxEffectBlock block in state.Effects)
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

                NadaEffectInstance instance =
                    NadaEffectInstanceAssembly.EnsureInstance(
                        localEffectsRootTransform,
                        block.InstanceId,
                        block.TypeId,
                        ownerNameForLogs);

                if (instance == null ||
                    !instance.IsValid)
                {
                    continue;
                }

                Transform effectTransform =
                    NadaStrandsRigAssembly.EnsureStrandsBranch(
                        instance.RootTransform,
                        ownerNameForLogs);

                if (effectTransform == null)
                    continue;

                NadaEffectBinder.BindStrandsBlockEffect(
                    effectTransform,
                    block);

                effectTransform
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

                NadaEffectInstance instance =
                    NadaEffectInstanceAssembly.EnsureInstance(
                        localEffectsRootTransform,
                        block.InstanceId,
                        block.TypeId,
                        ownerNameForLogs);

                if (instance == null ||
                    !instance.IsValid)
                {
                    continue;
                }

                Transform effectTransform =
                    NadaSparksRigAssembly.EnsureSparksBranch(
                        instance.RootTransform,
                        ownerNameForLogs);

                if (effectTransform == null)
                    continue;

                effectTransform
                    .gameObject
                    .SetActive(true);

                NadaEffectBinder.BindSparksBlockEffect(
                    effectTransform,
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

            foreach (VfxEffectBlock block in state.Effects)
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

                NadaEffectInstance instance =
                    NadaEffectInstanceAssembly.EnsureInstance(
                        localEffectsRootTransform,
                        block.InstanceId,
                        block.TypeId,
                        ownerNameForLogs);

                if (instance == null ||
                    !instance.IsValid)
                {
                    continue;
                }

                Transform effectTransform =
                    NadaFlareRigAssembly.EnsureFlareBranch(
                        instance.RootTransform,
                        ownerNameForLogs);

                if (effectTransform == null)
                    continue;

                NadaEffectBinder.BindFlareBlockEffect(
                    effectTransform,
                    block);

                effectTransform
                    .gameObject
                    .SetActive(true);
            }
        }

        private static void BindOrbitalsOrbsBlocks(
            Transform localEffectsRootTransform,
            Transform localWeaponRootTransform,
            Transform legacyOrbitalsRigRootTransform,
            WeaponVfxState state,
            string ownerNameForLogs)
        {
            if (localEffectsRootTransform == null ||
                localWeaponRootTransform == null)
            {
                return;
            }

            NadaEffectInstanceAssembly.ReconcileInstancesOfType(
                localEffectsRootTransform,
                state,
                VfxEffectTypeIds.OrbitalsOrbs,
                ownerNameForLogs);

            if (state?.Effects == null)
            {
                ClearLegacyOrbitalsGlueSources(
                    legacyOrbitalsRigRootTransform);

                return;
            }

            Transform legacyGlueSourceMotionRootTransform =
                null;

            foreach (VfxEffectBlock block in
                     state.Effects)
            {
                if (block == null)
                    continue;

                if (!string.Equals(
                        block.TypeId,
                        VfxEffectTypeIds.OrbitalsOrbs,
                        System.StringComparison.Ordinal))
                {
                    continue;
                }

                NadaEffectInstance instance =
                    NadaEffectInstanceAssembly.EnsureInstance(
                        localEffectsRootTransform,
                        block.InstanceId,
                        block.TypeId,
                        ownerNameForLogs);

                if (instance == null ||
                    !instance.IsValid)
                {
                    continue;
                }

                NadaOrbitalsRigAssembly
                    .OrbitalsOrbsInstanceStructure structure =
                    NadaOrbitalsRigAssembly
                        .EnsureOrbitalsOrbsInstanceStructure(
                            instance.RootTransform,
                            ownerNameForLogs);

                if (!structure.IsValid)
                    continue;

                NadaEffectBinder.BindOrbitalsOrbsBlockEffect(
                    instance.RootTransform,
                    structure.OrbsRootTransform,
                    block);

                NadaMotionBinder.BindOrbitalsOrbsBlockMotion(
                    structure.MotionRootTransform,
                    structure.HeadVisualTransform,
                    structure.PoolRootTransform,
                    block);

                NadaMotionBinder.BindOrbitalsRigFollow(
                    structure.RigRootTransform,
                    localWeaponRootTransform,
                    Vector3.zero,
                    Quaternion.Euler(
                        -90f,
                        0f,
                        0f));

                instance.RootTransform
                    .gameObject
                    .SetActive(true);

                if (block.InstanceId ==
                    PrototypeLegacyOrbitalsOrbsInstanceId)
                {
                    legacyGlueSourceMotionRootTransform =
                        structure.MotionRootTransform;
                }
            }

            if (legacyGlueSourceMotionRootTransform != null)
            {
                BindLegacyOrbitalsGlueSources(
                    legacyOrbitalsRigRootTransform,
                    legacyGlueSourceMotionRootTransform);
            }
            else
            {
                ClearLegacyOrbitalsGlueSources(
                    legacyOrbitalsRigRootTransform);
            }
        }

        private static void BindLegacyOrbitalsGlueSources(
            Transform legacyOrbitalsRigRootTransform,
            Transform orbsSourceMotionRootTransform)
        {
            if (legacyOrbitalsRigRootTransform == null ||
                orbsSourceMotionRootTransform == null)
            {
                return;
            }

            Transform motionRootsTransform =
                NadaRigPaths.FindDirectChild(
                    legacyOrbitalsRigRootTransform,
                    Plugin.OrbitalsMotionRootsName);

            if (motionRootsTransform == null)
                return;

            BindLegacyOrbitalsGlueSource(
                motionRootsTransform,
                Plugin.OrbitalsCoresMotionRootName,
                orbsSourceMotionRootTransform);

            BindLegacyOrbitalsGlueSource(
                motionRootsTransform,
                Plugin.OrbitalsFlamesMotionRootName,
                orbsSourceMotionRootTransform);

            BindLegacyOrbitalsGlueSource(
                motionRootsTransform,
                Plugin.OrbitalsEmbersMotionRootName,
                orbsSourceMotionRootTransform);
        }

        private static void BindLegacyOrbitalsGlueSource(
            Transform motionRootsTransform,
            string dependentMotionRootName,
            Transform orbsSourceMotionRootTransform)
        {
            Transform dependentMotionRootTransform =
                NadaRigPaths.FindDirectChild(
                    motionRootsTransform,
                    dependentMotionRootName);

            if (dependentMotionRootTransform == null)
                return;

            NadaMotionBinder.BindOrbitalsGlueSource(
                dependentMotionRootTransform,
                orbsSourceMotionRootTransform);
        }

        private static void ClearLegacyOrbitalsGlueSources(
            Transform legacyOrbitalsRigRootTransform)
        {
            if (legacyOrbitalsRigRootTransform == null)
                return;

            Transform motionRootsTransform =
                NadaRigPaths.FindDirectChild(
                    legacyOrbitalsRigRootTransform,
                    Plugin.OrbitalsMotionRootsName);

            if (motionRootsTransform == null)
                return;

            ClearLegacyOrbitalsGlueSource(
                motionRootsTransform,
                Plugin.OrbitalsCoresMotionRootName);

            ClearLegacyOrbitalsGlueSource(
                motionRootsTransform,
                Plugin.OrbitalsFlamesMotionRootName);

            ClearLegacyOrbitalsGlueSource(
                motionRootsTransform,
                Plugin.OrbitalsEmbersMotionRootName);
        }

        private static void ClearLegacyOrbitalsGlueSource(
            Transform motionRootsTransform,
            string dependentMotionRootName)
        {
            Transform dependentMotionRootTransform =
                NadaRigPaths.FindDirectChild(
                    motionRootsTransform,
                    dependentMotionRootName);

            if (dependentMotionRootTransform == null)
                return;

            NadaMotionBinder.ClearOrbitalsGlueSource(
                dependentMotionRootTransform);
        }

        private static WeaponVfxState CreateMigratedPrototypeState(
            NadaWeaponRigContext context,
            string source)
        {
            WeaponVfxState innerFlamesState =
                CreateInnerFlamesPrototypeState(
                    context,
                    source);

            WeaponVfxState outerFlamesState =
                CreateOuterFlamesPrototypeState(
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

            WeaponVfxState auraState =
                CreateAuraPrototypeState(
                    context,
                    source);

            WeaponVfxState orbitalsOrbsState =
                CreateOrbitalsOrbsPrototypeState(
                    context,
                    source);

            if (innerFlamesState?.Effects == null ||
                innerFlamesState.Effects.Count != 1 ||
                outerFlamesState?.Effects == null ||
                outerFlamesState.Effects.Count != 1 ||
                strandsState?.Effects == null ||
                strandsState.Effects.Count != 1 ||
                sparksState?.Effects == null ||
                sparksState.Effects.Count != 1 ||
                flareState?.Effects == null ||
                flareState.Effects.Count != 1 ||
                auraState?.Effects == null ||
                auraState.Effects.Count != 1 ||
                orbitalsOrbsState?.Effects == null ||
                orbitalsOrbsState.Effects.Count != 1)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [BlockPrototypeState] " +
                    $"Could not build combined migrated block prototype.");

                return null;
            }

            innerFlamesState.Effects.Add(
                outerFlamesState.Effects[0]);

            innerFlamesState.Effects.Add(
                strandsState.Effects[0]);

            innerFlamesState.Effects.Add(
                sparksState.Effects[0]);

            innerFlamesState.Effects.Add(
                flareState.Effects[0]);

            innerFlamesState.Effects.Add(
                auraState.Effects[0]);

            innerFlamesState.Effects.Add(
                orbitalsOrbsState.Effects[0]);

            NadaLogControl.Info(
                $"block-prototype-state:{source}:{context.Root.GetInstanceID()}",
                $"{Plugin.ModName}: [BlockPrototypeState] " +
                $"source='{source}' " +
                $"root='{context.Root.name}' " +
                $"effects={innerFlamesState.Effects.Count} " +
                $"order='[1:inner_flames,2:outer_flames,3:strands," +
                $"4:sparks,5:flare,6:aura,7:orbitals_orbs]'");

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

        private static WeaponVfxState CreateOuterFlamesPrototypeState(
            NadaWeaponRigContext context,
            string source)
        {
            if (context == null ||
                context.Root == null)
            {
                return null;
            }

            WeaponVfxState blockState =
                LegacyVfxStateAdapter.CreateOuterFlamesPrototype(
                    context.State);

            if (blockState?.Effects == null ||
                blockState.Effects.Count != 1)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [OuterFlamesBlockPrototype] " +
                    $"Outer Flames prototype did not contain exactly one migrated effect block.");

                return null;
            }

            VfxEffectBlock block =
                blockState.Effects[0];

            if (block == null ||
                block.Transform == null ||
                block.Settings is not OuterFlamesVfxSettings outer)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [OuterFlamesBlockPrototype] " +
                    $"Outer Flames prototype contained invalid settings.");

                return null;
            }

            VfxState legacy =
                context.State;

            bool matchesLegacy =
                block.InstanceId ==
                    PrototypeLegacyOuterFlamesInstanceId &&
                block.TypeId ==
                    VfxEffectTypeIds.OuterFlames &&
                block.Enabled ==
                    legacy.OuterFlamesEnabled &&

                block.Transform.XOffset ==
                    legacy.OuterFlamesXOffset &&
                block.Transform.YOffset ==
                    legacy.OuterFlamesYOffset &&
                block.Transform.ZOffset ==
                    legacy.OuterFlamesZOffset &&

                block.Transform.XRotation ==
                    legacy.OuterFlamesXRotation &&
                block.Transform.YRotation ==
                    legacy.OuterFlamesYRotation &&
                block.Transform.ZRotation ==
                    legacy.OuterFlamesZRotation &&

                outer.WorldEnabled ==
                    legacy.OuterFlamesWorldEnabled &&
                outer.BlackEnabled ==
                    legacy.OuterFlamesBlackEnabled &&
                outer.WhiteEnabled ==
                    legacy.OuterFlamesWhiteEnabled &&
                outer.DragEnabled ==
                    legacy.OuterFlamesDragEnabled &&

                outer.Energy ==
                    legacy.OuterFlamesEnergy &&
                outer.Scale ==
                    legacy.OuterFlamesScale &&
                outer.Luminance ==
                    legacy.OuterFlamesLuminance &&
                outer.Hue ==
                    legacy.OuterFlamesHue &&
                outer.Lifetime ==
                    legacy.OuterFlamesLifetime &&
                outer.SimulationSpeed ==
                    legacy.OuterFlamesSimulationSpeed &&
                outer.Length ==
                    legacy.OuterFlamesLength &&
                outer.Width ==
                    legacy.OuterFlamesWidth;

            NadaLogControl.Info(
                $"block-prototype:outer-flames:{source}:{context.Root.GetInstanceID()}",
                $"{Plugin.ModName}: [OuterFlamesBlockPrototype] " +
                $"source='{source}' " +
                $"root='{context.Root.name}' " +
                $"type='{block.TypeId}' " +
                $"id={block.InstanceId} " +
                $"enabled={block.Enabled} " +
                $"world={outer.WorldEnabled} " +
                $"black={outer.BlackEnabled} " +
                $"white={outer.WhiteEnabled} " +
                $"drag={outer.DragEnabled} " +
                $"hue={outer.Hue} " +
                $"scale={outer.Scale} " +
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

        private static WeaponVfxState CreateAuraPrototypeState(
            NadaWeaponRigContext context,
            string source)
        {
            if (context == null ||
                context.Root == null)
            {
                return null;
            }

            WeaponVfxState blockState =
                LegacyVfxStateAdapter.CreateAuraPrototype(
                    context.State);

            if (blockState?.Effects == null ||
                blockState.Effects.Count != 1)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [AuraBlockPrototype] " +
                    $"Aura prototype did not contain exactly one migrated effect block.");

                return null;
            }

            VfxEffectBlock block =
                blockState.Effects[0];

            if (block == null ||
                block.Transform == null ||
                block.Settings is not AuraVfxSettings aura)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [AuraBlockPrototype] " +
                    $"Aura prototype contained invalid settings.");

                return null;
            }

            VfxState legacy =
                context.State;

            bool matchesLegacy =
                block.InstanceId ==
                    PrototypeLegacyAuraInstanceId &&
                block.TypeId ==
                    VfxEffectTypeIds.Aura &&
                block.Enabled ==
                    legacy.AuraEnabled &&

                block.Transform.XOffset ==
                    legacy.AuraXOffset &&
                block.Transform.YOffset ==
                    legacy.AuraYOffset &&
                block.Transform.ZOffset ==
                    legacy.AuraZOffset &&

                block.Transform.XRotation ==
                    legacy.AuraXRotation &&
                block.Transform.YRotation ==
                    legacy.AuraYRotation &&
                block.Transform.ZRotation ==
                    legacy.AuraZRotation &&

                aura.Scale ==
                    legacy.AuraScale &&
                aura.Luminance ==
                    legacy.AuraLuminance &&
                aura.Hue ==
                    legacy.AuraHue;

            NadaLogControl.Info(
                $"block-prototype:aura:{source}:{context.Root.GetInstanceID()}",
                $"{Plugin.ModName}: [AuraBlockPrototype] " +
                $"source='{source}' " +
                $"root='{context.Root.name}' " +
                $"type='{block.TypeId}' " +
                $"id={block.InstanceId} " +
                $"enabled={block.Enabled} " +
                $"hue={aura.Hue} " +
                $"scale={aura.Scale} " +
                $"luminance={aura.Luminance} " +
                $"position=({block.Transform.XOffset}, " +
                $"{block.Transform.YOffset}, " +
                $"{block.Transform.ZOffset}) " +
                $"rotation=({block.Transform.XRotation}, " +
                $"{block.Transform.YRotation}, " +
                $"{block.Transform.ZRotation}) " +
                $"matchesLegacy={matchesLegacy}");

            return blockState;
        }

        private static WeaponVfxState CreateOrbitalsOrbsPrototypeState(
            NadaWeaponRigContext context,
            string source)
        {
            if (context == null ||
                context.Root == null)
            {
                return null;
            }

            WeaponVfxState blockState =
                LegacyVfxStateAdapter.CreateOrbitalsOrbsPrototype(
                    context.State);

            if (blockState?.Effects == null ||
                blockState.Effects.Count != 1)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [OrbitalsOrbsBlockPrototype] " +
                    $"Orbs prototype did not contain exactly one migrated effect block.");

                return null;
            }

            VfxEffectBlock block =
                blockState.Effects[0];

            if (block == null ||
                block.Transform == null ||
                block.Settings is not OrbitalsOrbsVfxSettings orbs ||
                orbs.Path == null)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [OrbitalsOrbsBlockPrototype] " +
                    $"Orbs prototype contained invalid settings.");

                return null;
            }

            VfxState legacy =
                context.State;

            bool matchesLegacy =
                block.InstanceId ==
                    PrototypeLegacyOrbitalsOrbsInstanceId &&
                block.TypeId ==
                    VfxEffectTypeIds.OrbitalsOrbs &&
                block.Enabled ==
                    legacy.OrbitalsOrbsEnabled &&

                block.Transform.XOffset ==
                    legacy.OrbitalsOrbsXOffset &&
                block.Transform.YOffset ==
                    legacy.OrbitalsOrbsYOffset &&
                block.Transform.ZOffset ==
                    legacy.OrbitalsOrbsZOffset &&

                block.Transform.XRotation ==
                    legacy.OrbitalsOrbsXRotation &&
                block.Transform.YRotation ==
                    legacy.OrbitalsOrbsYRotation &&
                block.Transform.ZRotation ==
                    legacy.OrbitalsOrbsZRotation &&

                orbs.Scale ==
                    legacy.OrbitalsOrbsScale &&
                orbs.Luminance ==
                    legacy.OrbitalsOrbsLuminance &&
                orbs.Hue ==
                    legacy.OrbitalsOrbsHue &&

                orbs.Path.SnakeEnabled ==
                    legacy.OrbitalsOrbsSnakeEnabled &&
                orbs.Path.Count ==
                    legacy.OrbitalsOrbsCount &&
                orbs.Path.Speed ==
                    legacy.OrbitalsOrbsSpeed &&
                orbs.Path.Spacing ==
                    legacy.OrbitalsOrbsSpacing &&
                orbs.Path.Length ==
                    legacy.OrbitalsOrbsLength &&
                orbs.Path.Radius ==
                    legacy.OrbitalsOrbsRadius &&
                orbs.Path.Cycles ==
                    legacy.OrbitalsOrbsCycles &&
                orbs.Path.Drift ==
                    legacy.OrbitalsOrbsDrift;

            NadaLogControl.Info(
                $"block-prototype:orbitals-orbs:{source}:{context.Root.GetInstanceID()}",
                $"{Plugin.ModName}: [OrbitalsOrbsBlockPrototype] " +
                $"source='{source}' " +
                $"root='{context.Root.name}' " +
                $"type='{block.TypeId}' " +
                $"id={block.InstanceId} " +
                $"enabled={block.Enabled} " +
                $"snake={orbs.Path.SnakeEnabled} " +
                $"count={orbs.Path.Count} " +
                $"hue={orbs.Hue} " +
                $"scale={orbs.Scale} " +
                $"speed={orbs.Path.Speed} " +
                $"spacing={orbs.Path.Spacing} " +
                $"length={orbs.Path.Length} " +
                $"radius={orbs.Path.Radius} " +
                $"cycles={orbs.Path.Cycles} " +
                $"drift={orbs.Path.Drift} " +
                $"position=({block.Transform.XOffset}, " +
                $"{block.Transform.YOffset}, " +
                $"{block.Transform.ZOffset}) " +
                $"rotation=({block.Transform.XRotation}, " +
                $"{block.Transform.YRotation}, " +
                $"{block.Transform.ZRotation}) " +
                $"matchesLegacy={matchesLegacy}");

            return blockState;
        }
    }
}