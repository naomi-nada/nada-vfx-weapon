using UnityEngine;
using NADA.VFX.Core.State;
using NADA.VFX.Weapons.Targets;
using NADA.VFX.Weapons.Runtime;
using NADA.VFX.Runtime.Binding;
using NADA.VFX.Runtime.Execution;
using NADA.VFX.Modules.Motion;

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

            Transform outerFlamesTf = NadaRigAssembly.EnsureLocalFlameBranchAndAlign(localWeaponTf, root.name);
            if (outerFlamesTf == null) return;

            Transform localOrbsTf = NadaRigAssembly.EnsureLocalOrbsBranch(localWeaponTf, root.name);
            if (localOrbsTf == null) return;

            Transform worldWeaponTf = NadaRigCatalogAssembly.EnsureWorldWeaponBranch(sword15LavaTf, root.name);
            if (worldWeaponTf == null) return;

            Transform worldOrbitalsTf = NadaRigAssembly.EnsureWorldOrbitalsBranch(worldWeaponTf, localOrbsTf, itemData, root.name);
            if (worldOrbitalsTf == null) return;

            NadaRigCatalog catalog = NadaRigCatalog.Build(localWeaponTf, worldWeaponTf);
            if (catalog == null || !catalog.IsValid) return;

            NadaRigAssembly.FinalizeLocalOrbsBranch(localOrbsTf);

            if (context.OrbTargets == null)
            {
                NadaOrbsTargets orbTargets = NadaOrbsTargets.Build(catalog.OrbitalsOrbs ?? localOrbsTf);
                if (orbTargets != null && orbTargets.IsValid)
                    context.SetOrbTargets(orbTargets);
            }

            float orbOrbitAdherence = NadaMotionTuningResolver.GetOrbOrbitAdherence(itemData);

            // Preserve exact current behavior for now.
            // The tuning value is introduced here so motion selection becomes parameter-driven
            // instead of structure-driven, but it is not yet consumed by runtime motion code.
            _ = orbOrbitAdherence;

            NadaMotionBinder.BindOrbsMotion(catalog.OrbitalsOrbs, itemData);
            
            NadaEffectBinder.BindOrbitalsEffect(catalog.OrbitalsRoot ?? worldOrbitalsTf, itemData);

            NadaMotionBinder.BindOrbitalsRigFollow(catalog.OrbitalsRig, sword15LavaTf);

            NadaEffectBinder.BindMirageEffect(catalog.Mirage, itemData);
            NadaMotionBinder.BindWorldFollow(catalog.Mirage, catalog.Flare);

            NadaEffectBinder.BindSparksEffect(catalog.Sparks, itemData);
            NadaMotionBinder.BindWorldFollow(catalog.Sparks, catalog.Flare);

            if (itemData != null)
                VfxStateIO.EnsureInitializedFromConfig(itemData);

            Transform effectsTf = catalog.LocalEffectsRoot;
            if (effectsTf == null) return;

            if (context.EffectsGroups == null)
            {
                RigGroups builtEffectsGroups = NadaRigFinder.BuildGroups(effectsTf);
                context.SetEffectsGroups(builtEffectsGroups);
            }

            RigGroups effectsGroups = context.EffectsGroups;
            if (effectsGroups == null)
                return;

            NadaModuleBinder.BindPropertyModules(
                effectsTf.gameObject,
                effectsGroups,
                context.OrbTargets,
                itemData
            );

            NadaRigMaintenance.ApplyPickupFix(root);
        }
    }
}