using System.Collections.Generic;
using NADA.VFX.Weapon.Core.Debug;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Runtime.Binding;
using NADA.VFX.Weapon.Runtime.Formation;
using NADA.VFX.Weapon.Runtime.Structure;
using NADA.VFX.Weapon.Weapons.Targets;
using UnityEngine;

namespace NADA.VFX.Weapon.Weapons.Runtime
{
    // This native entry shares the existing private block binders. It must
    // never derive remote state from ItemData, local config, or legacy VfxState.
    internal static partial class NadaWeaponRigOrchestrator
    {
        internal static bool RunRemoteNative(
            NadaWeaponRigContext context,
            NadaWeaponMetadata metadata,
            WeaponVfxState blockState)
        {
            if (context == null || !context.IsValid ||
                context.Root == null || context.WeaponVisualRoot == null ||
                blockState?.Effects == null || blockState.RigTransform == null)
            {
                return false;
            }

            GameObject root = context.Root;
            Transform visual = context.WeaponVisualRoot;

            // Codec validation is necessary, but Glue requires cross-block
            // validation before anything is assembled on the remote weapon.
            if (!OrbitalsFormationResolver.TryResolve(
                    blockState, out OrbitalsFormationResolution formation) ||
                formation == null || !formation.IsValid)
            {
                Transform stale = NadaRigPaths.FindDirectChild(
                    visual, Plugin.LocalWeaponRootName);
                if (stale != null)
                {
                    stale.gameObject.SetActive(false);
                    NadaWeaponRigRemoval.RemoveTrackedRig(stale);
                }

                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [RemoteNativeRigRejected] " +
                    $"root='{root.name}' reason='invalid-formation' " +
                    $"detail='{formation?.FailureReason ?? "resolution-failed"}'");
                return false;
            }

            if (!NadaRigCache.CacheReady ||
                !NadaWeaponTargets.IsTargetOrAttachClone(root))
            {
                return false;
            }

            NadaRuntimeDiagnostics.RecordRemoteApply();

            NadaWeaponRigAlignment alignment =
                NadaWeaponRigAlignmentResolver.Resolve(metadata, visual);

            Transform rig = NadaRigRootAssembly.EnsureAttachedLocalWeaponBranch(
                visual, root.name, alignment);
            if (rig == null)
                return false;

            // Native blocks build their own visuals, motion, and pools.
            // Do not construct shared legacy effects or resolve legacy state.
            NadaOrbitalsRigAssembly.EnsureNativeOrbitalsScaffold(
                rig, root.name);

            Transform effects = NadaRigPaths.FindLocalEffectsRoot(rig);
            if (effects == null)
                return false;

            NadaFlamesRigAssembly.RemoveDirectOuterFlamesBranch(effects, root.name);
            NadaFlamesRigAssembly.RemoveDirectInnerFlamesBranch(effects, root.name);
            NadaStrandsRigAssembly.RemoveDirectStrandsBranch(effects, root.name);
            NadaSparksRigAssembly.RemoveDirectSparksBranch(effects, root.name);
            NadaFlareRigAssembly.RemoveDirectFlareBranch(effects, root.name);
            NadaAuraRigAssembly.RemoveDirectAuraBranch(effects, root.name);

            NadaRigCatalog catalog = NadaRigCatalog.Build(rig);
            if (catalog == null || !catalog.IsValid)
                return false;

            BindInnerFlamesBlocks(effects, blockState, root.name);
            BindOuterFlamesBlocks(effects, blockState, root.name);
            BindStrandsBlocks(effects, blockState, root.name);
            BindSparksBlocks(effects, blockState, root.name);
            BindFlareBlocks(effects, blockState, root.name);
            BindAuraBlocks(effects, visual, blockState, root.name);

            var motionRoots = new Dictionary<uint, Transform>();

            BindOrbitalsCoresBlocks(
                effects, rig, catalog.OrbitalsRootTransform,
                catalog.OrbitalsRigRootTransform, blockState,
                motionRoots, root.name);
            BindOrbitalsFlamesBlocks(
                effects, rig, catalog.OrbitalsRootTransform,
                catalog.OrbitalsRigRootTransform, blockState,
                motionRoots, root.name);
            BindOrbitalsEmbersBlocks(
                effects, rig, catalog.OrbitalsRootTransform,
                catalog.OrbitalsRigRootTransform, blockState,
                motionRoots, root.name);
            BindOrbitalsOrbsBlocks(
                effects, rig, catalog.OrbitalsRigRootTransform,
                blockState, motionRoots, root.name);

            BindResolvedOrbitalsTrajectorySources(
                blockState, motionRoots, root.name);
            NadaEffectInstanceAssembly.ReconcileInstanceOrder(
                effects, blockState, root.name);

            NadaRigTransformApplier.Apply(rig, blockState.RigTransform);
            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [RemoteNativeRigApplied] " +
                $"root='{root.name}' visual='{visual.name}' " +
                $"item='{metadata.ItemName}' blocks={blockState.Effects.Count}");

            return true;
        }
    }
}