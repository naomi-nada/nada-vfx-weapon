using UnityEngine;
using NADA.VFX.Core.State;
using NADA.VFX.Weapons.Targets;
using NADA.VFX.Weapons.Runtime;
using NADA.VFX.Runtime.Binding;
using NADA.VFX.Runtime.Execution;

namespace NADA.VFX.Runtime
{
    internal static class NadaWeaponRigOrchestrator
    {
        internal static void Run(NadaWeaponRigContext context)
        {
            if (context == null || !context.IsValid) return;

            GameObject root = context.Root;
            global::ItemDrop.ItemData itemData = context.ItemData;

            if (!NadaRigCache.CacheReady) return;
            if (!NadaWeaponTargets.IsTargetOrAttachClone(root)) return;

            Transform sword15LavaTf = NadaWeaponTargets.FindSword15Lava(root.transform);
            if (sword15LavaTf == null) return;

            NadaRigMaintenance.DisableBrokenFlameRenderer(sword15LavaTf, root.name);

            Transform localWeaponTf = NadaRigCatalogAssembly.EnsureAttachedLocalWeaponBranch(sword15LavaTf, root.name);
            if (localWeaponTf == null) return;
            
            Transform worldWeaponTf = NadaRigCatalogAssembly.EnsureWorldWeaponBranch(sword15LavaTf, root.name);
            if (worldWeaponTf == null) return;
            
            Transform localEffectsTf = NadaRigPaths.FindLocalEffectsRoot(localWeaponTf);
            if (localEffectsTf == null) return;
            
            Transform localOrbsTf = NadaRigAssembly.EnsureLocalOrbsBranch(localWeaponTf, root.name);
            if (localOrbsTf == null) return;

            Transform worldOrbitalsTf = NadaRigAssembly.EnsureWorldOrbitalsBranch(worldWeaponTf, localOrbsTf, itemData, root.name);
            if (worldOrbitalsTf == null) return;
            
            Transform outerFlamesTf = NadaRigAssembly.EnsureLocalFlameBranchAndAlign(localWeaponTf, root.name);
            if (outerFlamesTf == null) return;
            
            NadaRigAssembly.FinalizeLocalOrbsBranch(localOrbsTf);
            
            NadaRigAssembly.EnsureLocalMirage(localEffectsTf, localOrbsTf, root.name);
            NadaRigAssembly.EnsureLocalSparks(localEffectsTf, localOrbsTf, root.name);
            
            NadaRigCatalog catalog = NadaRigCatalog.Build(localWeaponTf, worldWeaponTf);
            if (catalog == null || !catalog.IsValid) return;

            float orbOrbitAdherence = NadaMotionTuningResolver.GetOrbOrbitAdherence(itemData);

            // Preserve exact current behavior for now.
            // The tuning value is introduced here so motion selection becomes parameter-driven
            // instead of structure-driven, but it's not yet consumed by runtime motion code.
            _ = orbOrbitAdherence;
            
            NadaEffectBinder.BindOuterFlamesEffect(catalog.OuterFlames, itemData);
            
            NadaEffectBinder.BindInnerFlamesEffect(catalog.InnerFlames, itemData);
            
            NadaEffectBinder.BindFlareEffect(catalog.Flare, itemData);

            NadaEffectBinder.BindOrbitalsEffect(
                catalog.OrbitalsRoot ?? worldOrbitalsTf,
                catalog.OrbitalsOrbs ?? localOrbsTf,
                itemData);
            NadaMotionBinder.BindOrbsMotion(catalog.OrbitalsOrbs, itemData);
            NadaMotionBinder.BindOrbitalsRigFollow(catalog.OrbitalsRig, sword15LavaTf);

            NadaEffectBinder.BindMirageEffect(catalog.Mirage, itemData);

            Transform mirageMotionRoot =
                NadaMotionAnchorAssembly.EnsureStandaloneMirageMotionRoot(catalog.LocalEffectsRoot, catalog.Mirage);

            NadaMotionBinder.BindWorldFollow(mirageMotionRoot, catalog.Flare);
            NadaMotionBinder.BindWorldFollow(catalog.Mirage, mirageMotionRoot);

            NadaEffectBinder.BindSparksEffect(catalog.Sparks, itemData);

            Transform sparksMotionRoot =
                NadaMotionAnchorAssembly.EnsureStandaloneSparksMotionRoot(catalog.LocalEffectsRoot, catalog.Sparks);
            
            NadaMotionBinder.BindWorldFollow(sparksMotionRoot, catalog.Flare);
            NadaMotionBinder.BindWorldFollow(catalog.Sparks, sparksMotionRoot);

            if (itemData != null)
                VfxStateIO.EnsureInitializedFromConfig(itemData);

            NadaRigMaintenance.ApplyPickupFix(root);
        }
    }
}