using System.Collections.Generic;
using NADA.VFX.Weapon.Core.Debug;
using NADA.VFX.Weapon.Core.State;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Blocks.Effects;
using NADA.VFX.Weapon.Core.State.Migration;
using NADA.VFX.Weapon.Runtime.Binding;
using NADA.VFX.Weapon.Runtime.Formation;
using NADA.VFX.Weapon.Runtime.Structure;
using NADA.VFX.Weapon.Weapons.Targets;
using NADA.VFX.Weapon.Modules.Motion;
using UnityEngine;

namespace NADA.VFX.Weapon.Weapons.Runtime
{
    internal static partial class NadaWeaponRigOrchestrator
    {
        private const uint PrototypeLegacyInnerFlamesInstanceId = 1;
        private const uint PrototypeLegacyOuterFlamesInstanceId = 2;
        private const uint PrototypeLegacyStrandsInstanceId = 3;
        private const uint PrototypeLegacySparksInstanceId = 4;
        private const uint PrototypeLegacyFlareInstanceId = 5;
        private const uint PrototypeLegacyAuraInstanceId = 6;
        private const uint PrototypeLegacyOrbitalsOrbsInstanceId = 7;
        private const uint PrototypeLegacyOrbitalsCoresInstanceId = 8;
        private const uint PrototypeLegacyOrbitalsFlamesInstanceId = 9;
        private const uint PrototypeLegacyOrbitalsEmbersInstanceId = 10;

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

            NadaWeaponLocalSourceSelection localSource =
                NadaWeaponLocalSourceResolver.Resolve(itemData);

            bool hasPersistentBinding =
                localSource.Kind == NadaWeaponLocalSourceKind.NativeBound ||
                localSource.Kind == NadaWeaponLocalSourceKind.LegacyBound;

            bool invalidNative =
                localSource.Kind == NadaWeaponLocalSourceKind.InvalidNative;

            string nativeFailureReason = localSource.FailureReason;

            if (localSource.Kind == NadaWeaponLocalSourceKind.NativeBound)
            {
                // The binary codec validates structure; the existing formation
                // resolver validates cross-block Glue relationships.
                if (localSource.State == null)
                {
                    invalidNative = true;
                    nativeFailureReason = "Native state is missing.";
                }
                else
                {
                    bool formationAccepted =
                        OrbitalsFormationResolver.TryResolve(
                            localSource.State,
                            out OrbitalsFormationResolution formation);

                    if (!formationAccepted ||
                        formation == null ||
                        !formation.IsValid)
                    {
                        invalidNative = true;
                        nativeFailureReason =
                            formation?.FailureReason ??
                            "Native orbital relationships are invalid.";
                    }
                }
            }

            if (invalidNative)
            {
                // Fail closed before any structure assembly. Destroy is
                // deferred: deactivate first so stale effects cannot render.
                Transform staleRig = weaponVisualRootTransform != null
                    ? NadaRigPaths.FindDirectChild(
                        weaponVisualRootTransform,
                        Plugin.LocalWeaponRootName)
                    : null;

                if (staleRig != null)
                {
                    staleRig.gameObject.SetActive(false);
                    NadaWeaponRigRemoval.RemoveTrackedRig(staleRig);
                }

                NadaLogControl.Info(
                    $"native-local-invalid:{rootObject.GetInstanceID()}:{nativeFailureReason}",
                    $"{Plugin.ModName}: [NativeLocalStateRejected] " +
                    $"root='{rootObject.name}' " +
                    $"reason='{nativeFailureReason ?? "invalid-native-payload"}'");

                return;
            }

            if (!NadaRigCache.CacheReady)
            {
                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [OrchestratorSkip] cache not ready " +
                    $"root='{rootObject?.name}' " +
                    $"visual='{weaponVisualRootTransform?.name}' " +
                    $"item='{itemData?.m_shared?.m_name}' " +
                    $"bound={hasPersistentBinding}");

                return;
            }

            bool isEquippedTarget =
                NadaWeaponTargets.IsTargetOrAttachClone(
                    rootObject);

            bool isBoundDroppedItem =
                itemData != null &&
                hasPersistentBinding &&
                rootObject.GetComponent<global::ItemDrop>() != null;

            bool isBoundPreviewOrEquippedVisual =
                itemData != null &&
                hasPersistentBinding &&
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
                    $"bound={hasPersistentBinding} " +
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

            bool hasEditorPreview =
                localSource.Kind == NadaWeaponLocalSourceKind.EditorPreview;

            bool useBlockRuntime =
                localSource.Kind == NadaWeaponLocalSourceKind.EditorPreview ||
                hasPersistentBinding;

            bool hasNativeBlockSource =
                localSource.Kind == NadaWeaponLocalSourceKind.NativeBound ||
                localSource.Kind == NadaWeaponLocalSourceKind.EditorPreview;

            if (hasNativeBlockSource)
            {
                NadaOrbitalsRigAssembly.EnsureNativeOrbitalsScaffold(
                    localWeaponRootTransform,
                    rootObject.name);
            }
            else if (useBlockRuntime)
            {
                // Legacy-bound items still use the transitional adapter path.
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

            if (useBlockRuntime)
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

            if (useBlockRuntime)
            {
                blockState =
                    localSource.Kind == NadaWeaponLocalSourceKind.NativeBound ||
                    localSource.Kind == NadaWeaponLocalSourceKind.EditorPreview
                        ? localSource.State
                        : CreateMigratedPrototypeState(
                            context,
                            "local");

                if (localSource.Kind == NadaWeaponLocalSourceKind.NativeBound)
                {
                    NadaLogControl.Info(
                        $"native-local-runtime:{rootObject.GetInstanceID()}",
                        $"{Plugin.ModName}: [NativeLocalRuntime] " +
                        $"root='{rootObject.name}' " +
                        $"effects={blockState.Effects.Count} " +
                        $"source='item-data'");
                }

                if (hasEditorPreview)
                {
                    NadaLogControl.Info(
                        $"editor-preview-runtime:{rootObject.GetInstanceID()}:" +
                        $"{blockState?.Effects?.Count ?? 0}",
                        $"{Plugin.ModName}: [EditorPreviewRuntime] " +
                        $"root='{rootObject.name}' " +
                        $"effects={blockState?.Effects?.Count ?? 0} " +
                        $"source='editor'");
                }

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

                Dictionary<uint, Transform> orbitalMotionRoots =
                    new Dictionary<uint, Transform>();

                BindOrbitalsCoresBlocks(
                    localEffectsRootTransform,
                    localWeaponRootTransform,
                    catalog.OrbitalsRootTransform,
                    catalog.OrbitalsRigRootTransform,
                    blockState,
                    orbitalMotionRoots,
                    rootObject.name);

                BindOrbitalsFlamesBlocks(
                    localEffectsRootTransform,
                    localWeaponRootTransform,
                    catalog.OrbitalsRootTransform,
                    catalog.OrbitalsRigRootTransform,
                    blockState,
                    orbitalMotionRoots,
                    rootObject.name);

                BindOrbitalsEmbersBlocks(
                    localEffectsRootTransform,
                    localWeaponRootTransform,
                    catalog.OrbitalsRootTransform,
                    catalog.OrbitalsRigRootTransform,
                    blockState,
                    orbitalMotionRoots,
                    rootObject.name);

                BindOrbitalsOrbsBlocks(
                    localEffectsRootTransform,
                    localWeaponRootTransform,
                    catalog.OrbitalsRigRootTransform,
                    blockState,
                    orbitalMotionRoots,
                    rootObject.name);

                BindResolvedOrbitalsTrajectorySources(
                    blockState,
                    orbitalMotionRoots,
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

                NadaEffectInstanceAssembly.ReconcileInstancesOfType(
                    localEffectsRootTransform,
                    null,
                    VfxEffectTypeIds.OrbitalsCores,
                    rootObject.name);

                NadaEffectInstanceAssembly.ReconcileInstancesOfType(
                    localEffectsRootTransform,
                    null,
                    VfxEffectTypeIds.OrbitalsFlames,
                    rootObject.name);

                NadaEffectInstanceAssembly.ReconcileInstancesOfType(
                    localEffectsRootTransform,
                    null,
                    VfxEffectTypeIds.OrbitalsEmbers,
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

            if (useBlockRuntime && !hasNativeBlockSource)
            {
                NadaEffectBinder.BindOrbitalsEffect(
                    catalog.OrbitalsRootTransform,
                    null,
                    itemData);
            }
            else if (!useBlockRuntime)
            {
                NadaEffectBinder.BindOrbitalsEffect(
                    catalog.OrbitalsRootTransform,
                    catalog.OrbitalsOrbsRootTransform,
                    itemData);
            }

            if (localSource.Kind == NadaWeaponLocalSourceKind.NativeBound ||
                localSource.Kind == NadaWeaponLocalSourceKind.EditorPreview)
            {
                NadaRigTransformApplier.Apply(
                    localWeaponRootTransform,
                    blockState.RigTransform);
            }
            else
            {
                NadaRigTransformApplier.Apply(
                    localWeaponRootTransform,
                    context.State);
            }

            if (!hasNativeBlockSource)
            {
                NadaMotionBinder.BindOrbitalsRigFollow(
                    catalog.OrbitalsRigRootTransform,
                    localWeaponRootTransform,
                    Vector3.zero,
                    Quaternion.Euler(
                        -90f,
                        0f,
                        0f));
            }

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

            Dictionary<uint, Transform> orbitalMotionRoots =
                new Dictionary<uint, Transform>();

            BindOrbitalsCoresBlocks(
                remoteEffectsRootTransform,
                localWeaponRootTransform,
                catalog.OrbitalsRootTransform,
                catalog.OrbitalsRigRootTransform,
                blockState,
                orbitalMotionRoots,
                rootObject.name);

            BindOrbitalsFlamesBlocks(
                remoteEffectsRootTransform,
                localWeaponRootTransform,
                catalog.OrbitalsRootTransform,
                catalog.OrbitalsRigRootTransform,
                blockState,
                orbitalMotionRoots,
                rootObject.name);

            BindOrbitalsEmbersBlocks(
                remoteEffectsRootTransform,
                localWeaponRootTransform,
                catalog.OrbitalsRootTransform,
                catalog.OrbitalsRigRootTransform,
                blockState,
                orbitalMotionRoots,
                rootObject.name);

            BindOrbitalsOrbsBlocks(
                remoteEffectsRootTransform,
                localWeaponRootTransform,
                catalog.OrbitalsRigRootTransform,
                blockState,
                orbitalMotionRoots,
                rootObject.name);

            BindResolvedOrbitalsTrajectorySources(
                blockState,
                orbitalMotionRoots,
                rootObject.name);

            NadaEffectInstanceAssembly.ReconcileInstanceOrder(
                remoteEffectsRootTransform,
                blockState,
                rootObject.name);

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

                // Don't build or leave active Aura shells when the block
                // contains invalid data. Valid state can reactivate this
                // same instance later.
                if (block.Transform == null ||
                    block.Settings is not AuraVfxSettings)
                {
                    instance.RootTransform
                        .gameObject
                        .SetActive(false);

                    NadaLogControl.Info(
                        $"aura-block-invalid:{instance.RootTransform.GetInstanceID()}",
                        $"{Plugin.ModName}: [AuraBlockRejected] " +
                        $"owner='{ownerNameForLogs}' " +
                        $"id={block.InstanceId} " +
                        $"reason='invalid-settings-or-transform'");

                    continue;
                }

                Transform auraTransform =
                    NadaAuraRigAssembly.EnsureAuraBranch(
                        instance.RootTransform,
                        weaponVisualRootTransform,
                        ownerNameForLogs);

                if (auraTransform == null)
                {
                    instance.RootTransform
                        .gameObject
                        .SetActive(false);

                    continue;
                }

                NadaEffectBinder.BindAuraBlockEffect(
                    auraTransform,
                    block);

                instance.RootTransform
                    .gameObject
                    .SetActive(block.Enabled);
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

        private static void BindOrbitalsCoresBlocks(
            Transform localEffectsRootTransform,
            Transform localWeaponRootTransform,
            Transform legacyOrbitalsRootTransform,
            Transform legacyOrbitalsRigRootTransform,
            WeaponVfxState state,
            Dictionary<uint, Transform> orbitalMotionRoots,
            string ownerNameForLogs)
        {
            if (localEffectsRootTransform == null ||
                localWeaponRootTransform == null)
            {
                return;
            }

            bool resolutionValid =
                OrbitalsFormationResolver.TryResolve(
                    state,
                    out OrbitalsFormationResolution resolution);

            if (!resolutionValid ||
                resolution == null ||
                !resolution.IsValid)
            {
                NadaOrbitalsCoresBlockStructure.RemoveLegacyRuntime(
                    legacyOrbitalsRootTransform,
                    legacyOrbitalsRigRootTransform,
                    ownerNameForLogs);

                NadaEffectInstanceAssembly.ReconcileInstancesOfType(
                    localEffectsRootTransform,
                    null,
                    VfxEffectTypeIds.OrbitalsCores,
                    ownerNameForLogs);

                return;
            }

            bool hasCoresBlocks =
                false;

            foreach (ResolvedOrbitalsFormation entry in
                     resolution.Entries)
            {
                if (entry != null &&
                    entry.TypeId ==
                        VfxEffectTypeIds.OrbitalsCores)
                {
                    hasCoresBlocks =
                        true;

                    break;
                }
            }

            if (!hasCoresBlocks)
            {
                NadaOrbitalsCoresBlockStructure.RemoveLegacyRuntime(
                    legacyOrbitalsRootTransform,
                    legacyOrbitalsRigRootTransform,
                    ownerNameForLogs);

                NadaEffectInstanceAssembly.ReconcileInstancesOfType(
                    localEffectsRootTransform,
                    null,
                    VfxEffectTypeIds.OrbitalsCores,
                    ownerNameForLogs);

                return;
            }

            NadaOrbitalsCoresBlockStructure.RemoveLegacyRuntime(
                legacyOrbitalsRootTransform,
                legacyOrbitalsRigRootTransform,
                ownerNameForLogs);

            NadaEffectInstanceAssembly.ReconcileInstancesOfType(
                localEffectsRootTransform,
                state,
                VfxEffectTypeIds.OrbitalsCores,
                ownerNameForLogs);

            foreach (VfxEffectBlock block in
                     state.Effects)
            {
                if (block == null ||
                    block.TypeId !=
                        VfxEffectTypeIds.OrbitalsCores)
                {
                    continue;
                }

                if (!resolution.TryGet(
                        block.InstanceId,
                        out ResolvedOrbitalsFormation resolved) ||
                    resolved == null)
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

                NadaOrbitalsCoresBlockStructure.Structure structure =
                    NadaOrbitalsCoresBlockStructure.Ensure(
                        instance.RootTransform,
                        ownerNameForLogs);

                if (!structure.IsValid)
                {
                    instance.RootTransform
                        .gameObject
                        .SetActive(false);

                    continue;
                }

                bool effectAccepted =
                    NadaOrbitalsBlockBinder.BindCoresEffect(
                        instance.RootTransform,
                        structure.CoresRootTransform,
                        structure.PoolRootTransform,
                        block);

                bool motionAccepted =
                    NadaOrbitalsBlockBinder.BindMotion(
                        structure.MotionRootTransform,
                        structure.HeadVisualTransform,
                        structure.PoolRootTransform,
                        block,
                        resolved);

                NadaMotionBinder.BindOrbitalsRigFollow(
                    structure.RigRootTransform,
                    localWeaponRootTransform,
                    Vector3.zero,
                    Quaternion.Euler(
                        -90f,
                        0f,
                        0f));

                bool active =
                    effectAccepted &&
                    motionAccepted;

                instance.RootTransform
                    .gameObject
                    .SetActive(
                        active);

                if (active &&
                    orbitalMotionRoots != null)
                {
                    orbitalMotionRoots[
                        block.InstanceId] =
                        structure.MotionRootTransform;
                }
            }
        }

        private static void BindOrbitalsFlamesBlocks(
            Transform localEffectsRootTransform,
            Transform localWeaponRootTransform,
            Transform legacyOrbitalsRootTransform,
            Transform legacyOrbitalsRigRootTransform,
            WeaponVfxState state,
            Dictionary<uint, Transform> orbitalMotionRoots,
            string ownerNameForLogs)
        {
            if (localEffectsRootTransform == null ||
                localWeaponRootTransform == null)
            {
                return;
            }

            bool resolutionValid =
                OrbitalsFormationResolver.TryResolve(
                    state,
                    out OrbitalsFormationResolution resolution);

            if (!resolutionValid ||
                resolution == null ||
                !resolution.IsValid)
            {
                NadaOrbitalsParticleBlockStructure.RemoveLegacyRuntime(
                    legacyOrbitalsRootTransform,
                    legacyOrbitalsRigRootTransform,
                    NadaOrbitalsFamily.Flames,
                    ownerNameForLogs);

                NadaEffectInstanceAssembly.ReconcileInstancesOfType(
                    localEffectsRootTransform,
                    null,
                    VfxEffectTypeIds.OrbitalsFlames,
                    ownerNameForLogs);

                return;
            }

            bool hasFlamesBlocks =
                false;

            foreach (ResolvedOrbitalsFormation entry in
                     resolution.Entries)
            {
                if (entry != null &&
                    entry.TypeId ==
                        VfxEffectTypeIds.OrbitalsFlames)
                {
                    hasFlamesBlocks =
                        true;

                    break;
                }
            }

            if (!hasFlamesBlocks)
            {
                NadaOrbitalsParticleBlockStructure.RemoveLegacyRuntime(
                    legacyOrbitalsRootTransform,
                    legacyOrbitalsRigRootTransform,
                    NadaOrbitalsFamily.Flames,
                    ownerNameForLogs);

                NadaEffectInstanceAssembly.ReconcileInstancesOfType(
                    localEffectsRootTransform,
                    null,
                    VfxEffectTypeIds.OrbitalsFlames,
                    ownerNameForLogs);

                return;
            }

            NadaOrbitalsParticleBlockStructure.RemoveLegacyRuntime(
                legacyOrbitalsRootTransform,
                legacyOrbitalsRigRootTransform,
                NadaOrbitalsFamily.Flames,
                ownerNameForLogs);

            NadaEffectInstanceAssembly.ReconcileInstancesOfType(
                localEffectsRootTransform,
                state,
                VfxEffectTypeIds.OrbitalsFlames,
                ownerNameForLogs);

            foreach (VfxEffectBlock block in
                     state.Effects)
            {
                if (block == null ||
                    block.TypeId !=
                        VfxEffectTypeIds.OrbitalsFlames)
                {
                    continue;
                }

                if (!resolution.TryGet(
                        block.InstanceId,
                        out ResolvedOrbitalsFormation resolved) ||
                    resolved == null)
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

                NadaOrbitalsParticleBlockStructure.Structure structure =
                    NadaOrbitalsParticleBlockStructure.Ensure(
                        instance.RootTransform,
                        NadaOrbitalsFamily.Flames,
                        ownerNameForLogs);

                if (!structure.IsValid)
                {
                    instance.RootTransform
                        .gameObject
                        .SetActive(false);

                    continue;
                }

                bool effectAccepted =
                    NadaOrbitalsBlockBinder.BindParticleEffect(
                        instance.RootTransform,
                        structure.VisualRootTransform,
                        structure.PoolRootTransform,
                        NadaOrbitalsFamily.Flames,
                        block);

                bool motionAccepted =
                    NadaOrbitalsBlockBinder.BindMotion(
                        structure.MotionRootTransform,
                        structure.HeadVisualTransform,
                        structure.PoolRootTransform,
                        block,
                        resolved);

                NadaMotionBinder.BindOrbitalsRigFollow(
                    structure.RigRootTransform,
                    localWeaponRootTransform,
                    Vector3.zero,
                    Quaternion.Euler(
                        -90f,
                        0f,
                        0f));

                bool active =
                    effectAccepted &&
                    motionAccepted;

                instance.RootTransform
                    .gameObject
                    .SetActive(
                        active);

                if (active &&
                    orbitalMotionRoots != null)
                {
                    orbitalMotionRoots[
                        block.InstanceId] =
                        structure.MotionRootTransform;
                }
            }
        }

        private static void BindOrbitalsEmbersBlocks(
            Transform localEffectsRootTransform,
            Transform localWeaponRootTransform,
            Transform legacyOrbitalsRootTransform,
            Transform legacyOrbitalsRigRootTransform,
            WeaponVfxState state,
            Dictionary<uint, Transform> orbitalMotionRoots,
            string ownerNameForLogs)
        {
            if (localEffectsRootTransform == null ||
                localWeaponRootTransform == null)
            {
                return;
            }

            bool resolutionValid =
                OrbitalsFormationResolver.TryResolve(
                    state,
                    out OrbitalsFormationResolution resolution);

            if (!resolutionValid ||
                resolution == null ||
                !resolution.IsValid)
            {
                NadaOrbitalsParticleBlockStructure.RemoveLegacyRuntime(
                    legacyOrbitalsRootTransform,
                    legacyOrbitalsRigRootTransform,
                    NadaOrbitalsFamily.Embers,
                    ownerNameForLogs);

                NadaEffectInstanceAssembly.ReconcileInstancesOfType(
                    localEffectsRootTransform,
                    null,
                    VfxEffectTypeIds.OrbitalsEmbers,
                    ownerNameForLogs);

                return;
            }

            bool hasEmbersBlocks =
                false;

            foreach (ResolvedOrbitalsFormation entry in
                     resolution.Entries)
            {
                if (entry != null &&
                    entry.TypeId ==
                        VfxEffectTypeIds.OrbitalsEmbers)
                {
                    hasEmbersBlocks =
                        true;

                    break;
                }
            }

            if (!hasEmbersBlocks)
            {
                NadaOrbitalsParticleBlockStructure.RemoveLegacyRuntime(
                    legacyOrbitalsRootTransform,
                    legacyOrbitalsRigRootTransform,
                    NadaOrbitalsFamily.Embers,
                    ownerNameForLogs);

                NadaEffectInstanceAssembly.ReconcileInstancesOfType(
                    localEffectsRootTransform,
                    null,
                    VfxEffectTypeIds.OrbitalsEmbers,
                    ownerNameForLogs);

                return;
            }

            NadaOrbitalsParticleBlockStructure.RemoveLegacyRuntime(
                legacyOrbitalsRootTransform,
                legacyOrbitalsRigRootTransform,
                NadaOrbitalsFamily.Embers,
                ownerNameForLogs);

            NadaEffectInstanceAssembly.ReconcileInstancesOfType(
                localEffectsRootTransform,
                state,
                VfxEffectTypeIds.OrbitalsEmbers,
                ownerNameForLogs);

            foreach (VfxEffectBlock block in
                     state.Effects)
            {
                if (block == null ||
                    block.TypeId !=
                        VfxEffectTypeIds.OrbitalsEmbers)
                {
                    continue;
                }

                if (!resolution.TryGet(
                        block.InstanceId,
                        out ResolvedOrbitalsFormation resolved) ||
                    resolved == null)
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

                NadaOrbitalsParticleBlockStructure.Structure structure =
                    NadaOrbitalsParticleBlockStructure.Ensure(
                        instance.RootTransform,
                        NadaOrbitalsFamily.Embers,
                        ownerNameForLogs);

                if (!structure.IsValid)
                {
                    instance.RootTransform
                        .gameObject
                        .SetActive(false);

                    continue;
                }

                bool effectAccepted =
                    NadaOrbitalsBlockBinder.BindParticleEffect(
                        instance.RootTransform,
                        structure.VisualRootTransform,
                        structure.PoolRootTransform,
                        NadaOrbitalsFamily.Embers,
                        block);

                bool motionAccepted =
                    NadaOrbitalsBlockBinder.BindMotion(
                        structure.MotionRootTransform,
                        structure.HeadVisualTransform,
                        structure.PoolRootTransform,
                        block,
                        resolved);

                NadaMotionBinder.BindOrbitalsRigFollow(
                    structure.RigRootTransform,
                    localWeaponRootTransform,
                    Vector3.zero,
                    Quaternion.Euler(
                        -90f,
                        0f,
                        0f));

                bool active =
                    effectAccepted &&
                    motionAccepted;

                instance.RootTransform
                    .gameObject
                    .SetActive(
                        active);

                if (active &&
                    orbitalMotionRoots != null)
                {
                    orbitalMotionRoots[
                        block.InstanceId] =
                        structure.MotionRootTransform;
                }
            }
        }

        
        private static void BindOrbitalsOrbsBlocks(
            Transform localEffectsRootTransform,
            Transform localWeaponRootTransform,
            Transform legacyOrbitalsRigRootTransform,
            WeaponVfxState state,
            Dictionary<uint, Transform> orbitalMotionRoots,
            string ownerNameForLogs)
        {
            if (localEffectsRootTransform == null ||
                localWeaponRootTransform == null)
            {
                return;
            }

            // The resolver owns trajectory relationships. An Orbs instance
            // must never silently run its own path when resolution fails.
            bool resolutionValid =
                OrbitalsFormationResolver.TryResolve(
                    state,
                    out OrbitalsFormationResolution resolution);

            if (!resolutionValid ||
                resolution == null ||
                !resolution.IsValid)
            {
                ClearLegacyOrbitalsGlueSources(
                    legacyOrbitalsRigRootTransform);

                NadaEffectInstanceAssembly.ReconcileInstancesOfType(
                    localEffectsRootTransform,
                    null,
                    VfxEffectTypeIds.OrbitalsOrbs,
                    ownerNameForLogs);

                return;
            }

            NadaEffectInstanceAssembly.ReconcileInstancesOfType(
                localEffectsRootTransform,
                state,
                VfxEffectTypeIds.OrbitalsOrbs,
                ownerNameForLogs);

            // The transitional shared rig must not retain an old Glue source.
            ClearLegacyOrbitalsGlueSources(
                legacyOrbitalsRigRootTransform);

            foreach (VfxEffectBlock block in state.Effects)
            {
                if (block == null ||
                    !string.Equals(
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

                if (!resolution.TryGet(
                        block.InstanceId,
                        out ResolvedOrbitalsFormation resolved) ||
                    resolved == null)
                {
                    instance.RootTransform
                        .gameObject
                        .SetActive(false);

                    continue;
                }

                NadaOrbitalsRigAssembly
                    .OrbitalsOrbsInstanceStructure structure =
                    NadaOrbitalsRigAssembly
                        .EnsureOrbitalsOrbsInstanceStructure(
                            instance.RootTransform,
                            ownerNameForLogs);

                if (!structure.IsValid)
                {
                    instance.RootTransform
                        .gameObject
                        .SetActive(false);

                    continue;
                }

                NadaEffectBinder.BindOrbitalsOrbsBlockEffect(
                    instance.RootTransform,
                    structure.OrbsRootTransform,
                    block);

                bool motionAccepted =
                    NadaMotionBinder.BindOrbitalsOrbsBlockMotion(
                        structure.MotionRootTransform,
                        structure.HeadVisualTransform,
                        structure.PoolRootTransform,
                        block,
                        resolved);

                NadaMotionBinder.BindOrbitalsRigFollow(
                    structure.RigRootTransform,
                    localWeaponRootTransform,
                    Vector3.zero,
                    Quaternion.Euler(
                        -90f,
                        0f,
                        0f));

                bool active =
                    block.Enabled &&
                    motionAccepted;

                instance.RootTransform
                    .gameObject
                    .SetActive(active);

                if (active &&
                    orbitalMotionRoots != null)
                {
                    orbitalMotionRoots[
                        block.InstanceId] =
                        structure.MotionRootTransform;
                }
            }
        }


        private static void BindResolvedOrbitalsTrajectorySources(
            WeaponVfxState state,
            Dictionary<uint, Transform> orbitalMotionRoots,
            string ownerNameForLogs)
        {
            if (state == null ||
                orbitalMotionRoots == null)
            {
                return;
            }

            bool resolutionValid =
                OrbitalsFormationResolver.TryResolve(
                    state,
                    out OrbitalsFormationResolution resolution);

            if (!resolutionValid ||
                resolution == null ||
                !resolution.IsValid)
            {
                NadaLogControl.Info(
                    $"orbitals-trajectory-pass-invalid:{ownerNameForLogs}",
                    $"{Plugin.ModName}: [OrbitalsTrajectoryPass] " +
                    $"owner='{ownerNameForLogs}' " +
                    $"accepted=False " +
                    $"reason='{resolution?.FailureReason ?? "invalid-resolution"}'");

                return;
            }

            foreach (ResolvedOrbitalsFormation entry in
                     resolution.Entries)
            {
                if (entry == null)
                    continue;

                if (!orbitalMotionRoots.TryGetValue(
                        entry.InstanceId,
                        out Transform followerMotionRootTransform) ||
                    followerMotionRootTransform == null)
                {
                    if (entry.Enabled &&
                        entry.IsFollower)
                    {
                        NadaLogControl.Info(
                            $"orbitals-trajectory-missing-follower:" +
                            $"{ownerNameForLogs}:{entry.InstanceId}",
                            $"{Plugin.ModName}: [OrbitalsTrajectorySourceResolve] " +
                            $"owner='{ownerNameForLogs}' " +
                            $"follower={entry.InstanceId} " +
                            $"source={entry.TrajectorySourceInstanceId} " +
                            $"accepted=False " +
                            $"reason='missing-follower-runtime'");
                    }

                    continue;
                }

                if (!entry.Enabled ||
                    !entry.IsFollower)
                {
                    NadaOrbitalsBlockBinder.ClearTrajectorySource(
                        followerMotionRootTransform);

                    continue;
                }

                orbitalMotionRoots.TryGetValue(
                    entry.TrajectorySourceInstanceId,
                    out Transform sourceMotionRootTransform);

                bool accepted =
                    NadaOrbitalsBlockBinder.BindTrajectorySource(
                        followerMotionRootTransform,
                        sourceMotionRootTransform);

                NadaLogControl.Info(
                    $"orbitals-trajectory-resolve:" +
                    $"{ownerNameForLogs}:{entry.InstanceId}",
                    $"{Plugin.ModName}: [OrbitalsTrajectorySourceResolve] " +
                    $"owner='{ownerNameForLogs}' " +
                    $"follower={entry.InstanceId} " +
                    $"source={entry.TrajectorySourceInstanceId} " +
                    $"sourceRuntime={(sourceMotionRootTransform != null)} " +
                    $"accepted={accepted}");
            }
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

            WeaponVfxState orbitalsCoresState =
                CreateOrbitalsCoresPrototypeState(
                    context,
                    source);

            WeaponVfxState orbitalsFlamesState =
                CreateOrbitalsFlamesPrototypeState(
                    context,
                    source);

            WeaponVfxState orbitalsEmbersState =
                CreateOrbitalsEmbersPrototypeState(
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
                orbitalsOrbsState.Effects.Count != 1 ||
                orbitalsCoresState?.Effects == null ||
                orbitalsCoresState.Effects.Count != 1 ||
                orbitalsFlamesState?.Effects == null ||
                orbitalsFlamesState.Effects.Count != 1 ||
                orbitalsEmbersState?.Effects == null ||
                orbitalsEmbersState.Effects.Count != 1)
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

            innerFlamesState.Effects.Add(
                orbitalsCoresState.Effects[0]);

            innerFlamesState.Effects.Add(
                orbitalsFlamesState.Effects[0]);

            innerFlamesState.Effects.Add(
                orbitalsEmbersState.Effects[0]);

            LegacyVfxStateAdapter.ApplyMigratedOrbitalsRelationships(
                innerFlamesState,
                context.State);

            LogMigratedOrbitalsRelationships(
                context,
                source,
                innerFlamesState);

            LogResolvedOrbitalsFormation(
                context,
                source,
                innerFlamesState);

            NadaLogControl.Info(
                $"block-prototype-state:{source}:{context.Root.GetInstanceID()}",
                $"{Plugin.ModName}: [BlockPrototypeState] " +
                $"source='{source}' " +
                $"root='{context.Root.name}' " +
                $"effects={innerFlamesState.Effects.Count} " +
                $"order='[1:inner_flames,2:outer_flames,3:strands," +
                $"4:sparks,5:flare,6:aura,7:orbitals_orbs," +
                $"8:orbitals_cores,9:orbitals_flames," +
                $"10:orbitals_embers]'");

            return innerFlamesState;
        }

        private static void LogMigratedOrbitalsRelationships(
            NadaWeaponRigContext context,
            string source,
            WeaponVfxState state)
        {
            if (context == null ||
                context.Root == null ||
                state?.Effects == null)
            {
                return;
            }

            OrbitalsOrbsVfxSettings orbs =
                null;

            foreach (VfxEffectBlock block in state.Effects)
            {
                if (block == null ||
                    block.InstanceId !=
                        PrototypeLegacyOrbitalsOrbsInstanceId ||
                    block.TypeId !=
                        VfxEffectTypeIds.OrbitalsOrbs)
                {
                    continue;
                }

                orbs =
                    block.Settings as OrbitalsOrbsVfxSettings;

                break;
            }

            if (orbs?.Formation == null)
                return;

            bool coresTargeted =
                orbs.Formation.GlueTargetInstanceIds != null &&
                orbs.Formation.GlueTargetInstanceIds.Contains(
                    PrototypeLegacyOrbitalsCoresInstanceId);

            bool flamesTargeted =
                orbs.Formation.GlueTargetInstanceIds != null &&
                orbs.Formation.GlueTargetInstanceIds.Contains(
                    PrototypeLegacyOrbitalsFlamesInstanceId);

            bool embersTargeted =
                orbs.Formation.GlueTargetInstanceIds != null &&
                orbs.Formation.GlueTargetInstanceIds.Contains(
                    PrototypeLegacyOrbitalsEmbersInstanceId);

            bool expectedLeaderEnabled =
                context.State.OrbitalsOrbsGlueEnabled ||
                context.State.OrbitalsCoresGlueEnabled;

            bool expectedCoresTargeted =
                context.State.OrbitalsCoresGlueEnabled;

            bool expectedFlamesTargeted =
                context.State.OrbitalsOrbsGlueEnabled;

            bool expectedEmbersTargeted =
                context.State.OrbitalsOrbsGlueEnabled;

            bool matchesLegacy =
                orbs.Formation.GlueLeaderEnabled ==
                    expectedLeaderEnabled &&
                coresTargeted ==
                    expectedCoresTargeted &&
                flamesTargeted ==
                    expectedFlamesTargeted &&
                embersTargeted ==
                    expectedEmbersTargeted;

            NadaLogControl.Info(
                $"block-prototype:orbitals-glue:{source}:{context.Root.GetInstanceID()}",
                $"{Plugin.ModName}: [OrbitalsGlueBlockPrototype] " +
                $"source='{source}' " +
                $"root='{context.Root.name}' " +
                $"leaderId={PrototypeLegacyOrbitalsOrbsInstanceId} " +
                $"leaderEnabled={orbs.Formation.GlueLeaderEnabled} " +
                $"coresTargeted={coresTargeted} " +
                $"flamesTargeted={flamesTargeted} " +
                $"embersTargeted={embersTargeted} " +
                $"legacyCoresGlue={context.State.OrbitalsCoresGlueEnabled} " +
                $"legacyOrbsGlue={context.State.OrbitalsOrbsGlueEnabled} " +
                $"matchesLegacy={matchesLegacy}");
        }

        private static void LogResolvedOrbitalsFormation(
            NadaWeaponRigContext context,
            string source,
            WeaponVfxState state)
        {
            if (context == null ||
                context.Root == null)
            {
                return;
            }

            bool valid =
                OrbitalsFormationResolver.TryResolve(
                    state,
                    out OrbitalsFormationResolution resolution);

            if (!valid ||
                resolution == null ||
                !resolution.IsValid)
            {
                string failureReason =
                    resolution?.FailureReason ??
                    "unknown";

                NadaLogControl.Info(
                    $"orbitals-formation-resolve-invalid:" +
                    $"{source}:{context.Root.GetInstanceID()}",
                    $"{Plugin.ModName}: [OrbitalsFormationResolve] " +
                    $"source='{source}' " +
                    $"root='{context.Root.name}' " +
                    $"valid=False " +
                    $"reason='{failureReason}'");

                return;
            }

            NadaLogControl.Info(
                $"orbitals-formation-resolve:" +
                $"{source}:{context.Root.GetInstanceID()}",
                $"{Plugin.ModName}: [OrbitalsFormationResolve] " +
                $"source='{source}' " +
                $"root='{context.Root.name}' " +
                $"valid=True " +
                $"orbitals={resolution.OrbitalsCount} " +
                $"leaders={resolution.LeaderCount} " +
                $"followers={resolution.FollowerCount}");

            foreach (ResolvedOrbitalsFormation entry in
                     resolution.Entries)
            {
                if (entry == null)
                    continue;

                string mode =
                    !entry.Enabled
                        ? "disabled"
                        : entry.IsFollower
                            ? "follower"
                            : entry.IsLeader
                                ? "leader"
                                : "independent";

                string leader =
                    entry.LeaderInstanceId.HasValue
                        ? entry.LeaderInstanceId.Value.ToString()
                        : "none";

                NadaLogControl.Info(
                    $"orbitals-formation-resolved:" +
                    $"{source}:" +
                    $"{context.Root.GetInstanceID()}:" +
                    $"{entry.InstanceId}",
                    $"{Plugin.ModName}: [OrbitalsFormationResolved] " +
                    $"source='{source}' " +
                    $"root='{context.Root.name}' " +
                    $"id={entry.InstanceId} " +
                    $"type='{entry.TypeId}' " +
                    $"enabled={entry.Enabled} " +
                    $"mode={mode} " +
                    $"leader={leader} " +
                    $"trajectorySource={entry.TrajectorySourceInstanceId} " +
                    $"count={entry.OwnCount} " +
                    $"snake={entry.EffectiveSnakeEnabled} " +
                    $"speed={entry.EffectiveSpeed} " +
                    $"spacing={entry.EffectiveSpacing} " +
                    $"length={entry.EffectiveLength} " +
                    $"radius={entry.EffectiveRadius} " +
                    $"cycles={entry.EffectiveCycles} " +
                    $"drift={entry.EffectiveDrift} " +
                    $"position=({entry.EffectiveXOffset}, " +
                    $"{entry.EffectiveYOffset}, " +
                    $"{entry.EffectiveZOffset}) " +
                    $"rotation=({entry.EffectiveXRotation}, " +
                    $"{entry.EffectiveYRotation}, " +
                    $"{entry.EffectiveZRotation})");
            }
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

        private static WeaponVfxState CreateOrbitalsCoresPrototypeState(
            NadaWeaponRigContext context,
            string source)
        {
            if (context == null ||
                context.Root == null)
            {
                return null;
            }

            WeaponVfxState blockState =
                LegacyVfxStateAdapter.CreateOrbitalsCoresPrototype(
                    context.State);

            if (blockState?.Effects == null ||
                blockState.Effects.Count != 1)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [OrbitalsCoresBlockPrototype] " +
                    $"Cores prototype did not contain exactly one migrated effect block.");

                return null;
            }

            VfxEffectBlock block =
                blockState.Effects[0];

            if (block == null ||
                block.Transform == null ||
                block.Settings is not OrbitalsCoresVfxSettings cores ||
                cores.Formation?.Path == null)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [OrbitalsCoresBlockPrototype] " +
                    $"Cores prototype contained invalid settings.");

                return null;
            }

            VfxState legacy =
                context.State;

            OrbitalsPathVfxSettings path =
                cores.Formation.Path;

            bool matchesLegacy =
                block.InstanceId ==
                    PrototypeLegacyOrbitalsCoresInstanceId &&
                block.TypeId ==
                    VfxEffectTypeIds.OrbitalsCores &&
                block.Enabled ==
                    legacy.OrbitalsCoresEnabled &&

                block.Transform.XOffset ==
                    legacy.OrbitalsCoresXOffset &&
                block.Transform.YOffset ==
                    legacy.OrbitalsCoresYOffset &&
                block.Transform.ZOffset ==
                    legacy.OrbitalsCoresZOffset &&

                block.Transform.XRotation ==
                    legacy.OrbitalsCoresXRotation &&
                block.Transform.YRotation ==
                    legacy.OrbitalsCoresYRotation &&
                block.Transform.ZRotation ==
                    legacy.OrbitalsCoresZRotation &&

                cores.Scale ==
                    legacy.OrbitalsCoresScale &&
                cores.Luminance ==
                    legacy.OrbitalsCoresLuminance &&
                cores.Hue ==
                    legacy.OrbitalsCoresHue &&
                cores.SpinEnabled ==
                    legacy.OrbitalsCoresSpinEnabled &&
                cores.SpinSpeed ==
                    legacy.OrbitalsCoresSpinSpeed &&

                path.SnakeEnabled ==
                    legacy.OrbitalsCoresSnakeEnabled &&
                path.Count ==
                    legacy.OrbitalsCoresCount &&
                path.Speed ==
                    legacy.OrbitalsCoresSpeed &&
                path.Spacing ==
                    legacy.OrbitalsCoresSpacing &&
                path.Length ==
                    legacy.OrbitalsCoresLength &&
                path.Radius ==
                    legacy.OrbitalsCoresRadius &&
                path.Cycles ==
                    legacy.OrbitalsCoresCycles &&
                path.Drift ==
                    legacy.OrbitalsCoresDrift &&

                !cores.Formation.GlueLeaderEnabled &&
                cores.Formation.GlueTargetInstanceIds != null &&
                cores.Formation.GlueTargetInstanceIds.Count == 0;

            NadaLogControl.Info(
                $"block-prototype:orbitals-cores:{source}:{context.Root.GetInstanceID()}",
                $"{Plugin.ModName}: [OrbitalsCoresBlockPrototype] " +
                $"source='{source}' " +
                $"root='{context.Root.name}' " +
                $"type='{block.TypeId}' " +
                $"id={block.InstanceId} " +
                $"enabled={block.Enabled} " +
                $"snake={path.SnakeEnabled} " +
                $"count={path.Count} " +
                $"hue={cores.Hue} " +
                $"scale={cores.Scale} " +
                $"spin={cores.SpinEnabled} " +
                $"spinSpeed={cores.SpinSpeed} " +
                $"speed={path.Speed} " +
                $"spacing={path.Spacing} " +
                $"length={path.Length} " +
                $"radius={path.Radius} " +
                $"cycles={path.Cycles} " +
                $"drift={path.Drift} " +
                $"position=({block.Transform.XOffset}, " +
                $"{block.Transform.YOffset}, " +
                $"{block.Transform.ZOffset}) " +
                $"rotation=({block.Transform.XRotation}, " +
                $"{block.Transform.YRotation}, " +
                $"{block.Transform.ZRotation}) " +
                $"matchesLegacy={matchesLegacy}");

            return blockState;
        }

        private static WeaponVfxState CreateOrbitalsFlamesPrototypeState(
            NadaWeaponRigContext context,
            string source)
        {
            if (context == null ||
                context.Root == null)
            {
                return null;
            }

            WeaponVfxState blockState =
                LegacyVfxStateAdapter.CreateOrbitalsFlamesPrototype(
                    context.State);

            if (blockState?.Effects == null ||
                blockState.Effects.Count != 1)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [OrbitalsFlamesBlockPrototype] " +
                    $"Flames prototype did not contain exactly one migrated effect block.");

                return null;
            }

            VfxEffectBlock block =
                blockState.Effects[0];

            if (block == null ||
                block.Transform == null ||
                block.Settings is not OrbitalsFlamesVfxSettings flames ||
                flames.Formation?.Path == null)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [OrbitalsFlamesBlockPrototype] " +
                    $"Flames prototype contained invalid settings.");

                return null;
            }

            VfxState legacy =
                context.State;

            OrbitalsPathVfxSettings path =
                flames.Formation.Path;

            bool matchesLegacy =
                block.InstanceId ==
                    PrototypeLegacyOrbitalsFlamesInstanceId &&
                block.TypeId ==
                    VfxEffectTypeIds.OrbitalsFlames &&
                block.Enabled ==
                    legacy.OrbitalsFlamesEnabled &&

                block.Transform.XOffset ==
                    legacy.OrbitalsFlamesXOffset &&
                block.Transform.YOffset ==
                    legacy.OrbitalsFlamesYOffset &&
                block.Transform.ZOffset ==
                    legacy.OrbitalsFlamesZOffset &&

                block.Transform.XRotation ==
                    legacy.OrbitalsFlamesXRotation &&
                block.Transform.YRotation ==
                    legacy.OrbitalsFlamesYRotation &&
                block.Transform.ZRotation ==
                    legacy.OrbitalsFlamesZRotation &&

                flames.Energy ==
                    legacy.OrbitalsFlamesEnergy &&
                flames.Scale ==
                    legacy.OrbitalsFlamesScale &&
                flames.Luminance ==
                    legacy.OrbitalsFlamesLuminance &&
                flames.Hue ==
                    legacy.OrbitalsFlamesHue &&
                flames.Lifetime ==
                    legacy.OrbitalsFlamesLifetime &&
                flames.SimulationSpeed ==
                    legacy.OrbitalsFlamesSimulationSpeed &&

                !path.SnakeEnabled &&
                path.Count ==
                    legacy.OrbitalsFlamesCount &&
                path.Speed ==
                    legacy.OrbitalsFlamesSpeed &&
                path.Spacing ==
                    legacy.OrbitalsFlamesSpacing &&
                path.Length ==
                    legacy.OrbitalsFlamesLength &&
                path.Radius ==
                    legacy.OrbitalsFlamesRadius &&
                path.Cycles ==
                    legacy.OrbitalsFlamesCycles &&
                path.Drift ==
                    legacy.OrbitalsFlamesDrift &&

                !flames.Formation.GlueLeaderEnabled &&
                flames.Formation.GlueTargetInstanceIds != null &&
                flames.Formation.GlueTargetInstanceIds.Count == 0;

            NadaLogControl.Info(
                $"block-prototype:orbitals-flames:{source}:{context.Root.GetInstanceID()}",
                $"{Plugin.ModName}: [OrbitalsFlamesBlockPrototype] " +
                $"source='{source}' " +
                $"root='{context.Root.name}' " +
                $"type='{block.TypeId}' " +
                $"id={block.InstanceId} " +
                $"enabled={block.Enabled} " +
                $"snake={path.SnakeEnabled} " +
                $"count={path.Count} " +
                $"hue={flames.Hue} " +
                $"scale={flames.Scale} " +
                $"energy={flames.Energy} " +
                $"lifetime={flames.Lifetime} " +
                $"simulationSpeed={flames.SimulationSpeed} " +
                $"speed={path.Speed} " +
                $"spacing={path.Spacing} " +
                $"length={path.Length} " +
                $"radius={path.Radius} " +
                $"cycles={path.Cycles} " +
                $"drift={path.Drift} " +
                $"position=({block.Transform.XOffset}, " +
                $"{block.Transform.YOffset}, " +
                $"{block.Transform.ZOffset}) " +
                $"rotation=({block.Transform.XRotation}, " +
                $"{block.Transform.YRotation}, " +
                $"{block.Transform.ZRotation}) " +
                $"matchesLegacy={matchesLegacy}");

            return blockState;
        }

        private static WeaponVfxState CreateOrbitalsEmbersPrototypeState(
            NadaWeaponRigContext context,
            string source)
        {
            if (context == null ||
                context.Root == null)
            {
                return null;
            }

            WeaponVfxState blockState =
                LegacyVfxStateAdapter.CreateOrbitalsEmbersPrototype(
                    context.State);

            if (blockState?.Effects == null ||
                blockState.Effects.Count != 1)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [OrbitalsEmbersBlockPrototype] " +
                    $"Embers prototype did not contain exactly one migrated effect block.");

                return null;
            }

            VfxEffectBlock block =
                blockState.Effects[0];

            if (block == null ||
                block.Transform == null ||
                block.Settings is not OrbitalsEmbersVfxSettings embers ||
                embers.Formation?.Path == null)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [OrbitalsEmbersBlockPrototype] " +
                    $"Embers prototype contained invalid settings.");

                return null;
            }

            VfxState legacy =
                context.State;

            OrbitalsPathVfxSettings path =
                embers.Formation.Path;

            bool matchesLegacy =
                block.InstanceId ==
                    PrototypeLegacyOrbitalsEmbersInstanceId &&
                block.TypeId ==
                    VfxEffectTypeIds.OrbitalsEmbers &&
                block.Enabled ==
                    legacy.OrbitalsEmbersEnabled &&

                block.Transform.XOffset ==
                    legacy.OrbitalsEmbersXOffset &&
                block.Transform.YOffset ==
                    legacy.OrbitalsEmbersYOffset &&
                block.Transform.ZOffset ==
                    legacy.OrbitalsEmbersZOffset &&

                block.Transform.XRotation ==
                    legacy.OrbitalsEmbersXRotation &&
                block.Transform.YRotation ==
                    legacy.OrbitalsEmbersYRotation &&
                block.Transform.ZRotation ==
                    legacy.OrbitalsEmbersZRotation &&

                embers.Energy ==
                    legacy.OrbitalsEmbersEnergy &&
                embers.Scale ==
                    legacy.OrbitalsEmbersScale &&
                embers.Luminance ==
                    legacy.OrbitalsEmbersLuminance &&
                embers.Hue ==
                    legacy.OrbitalsEmbersHue &&
                embers.Lifetime ==
                    legacy.OrbitalsEmbersLifetime &&
                embers.SimulationSpeed ==
                    legacy.OrbitalsEmbersSimulationSpeed &&

                !path.SnakeEnabled &&
                path.Count ==
                    legacy.OrbitalsEmbersCount &&
                path.Speed ==
                    legacy.OrbitalsEmbersSpeed &&
                path.Spacing ==
                    legacy.OrbitalsEmbersSpacing &&
                path.Length ==
                    legacy.OrbitalsEmbersLength &&
                path.Radius ==
                    legacy.OrbitalsEmbersRadius &&
                path.Cycles ==
                    legacy.OrbitalsEmbersCycles &&
                path.Drift ==
                    legacy.OrbitalsEmbersDrift &&

                !embers.Formation.GlueLeaderEnabled &&
                embers.Formation.GlueTargetInstanceIds != null &&
                embers.Formation.GlueTargetInstanceIds.Count == 0;

            NadaLogControl.Info(
                $"block-prototype:orbitals-embers:{source}:{context.Root.GetInstanceID()}",
                $"{Plugin.ModName}: [OrbitalsEmbersBlockPrototype] " +
                $"source='{source}' " +
                $"root='{context.Root.name}' " +
                $"type='{block.TypeId}' " +
                $"id={block.InstanceId} " +
                $"enabled={block.Enabled} " +
                $"snake={path.SnakeEnabled} " +
                $"count={path.Count} " +
                $"hue={embers.Hue} " +
                $"scale={embers.Scale} " +
                $"energy={embers.Energy} " +
                $"lifetime={embers.Lifetime} " +
                $"simulationSpeed={embers.SimulationSpeed} " +
                $"speed={path.Speed} " +
                $"spacing={path.Spacing} " +
                $"length={path.Length} " +
                $"radius={path.Radius} " +
                $"cycles={path.Cycles} " +
                $"drift={path.Drift} " +
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