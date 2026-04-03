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
            // instead of structure-driven, but it's not yet consumed by runtime motion code.
            _ = orbOrbitAdherence;

            NadaMotionBinder.BindOrbsMotion(catalog.OrbitalsOrbs, itemData);
            
            NadaEffectBinder.BindOrbitalsEffect(catalog.OrbitalsRoot ?? worldOrbitalsTf, itemData);

            NadaMotionBinder.BindOrbitalsRigFollow(catalog.OrbitalsRig, sword15LavaTf);

            NadaEffectBinder.BindMirageEffect(catalog.Mirage, itemData);

            Transform mirageMotionRoot = EnsureMirageAnchor(catalog.WorldEffectsRoot, catalog.Mirage);
            NadaMotionBinder.BindWorldFollow(mirageMotionRoot, catalog.Flare);

            NadaEffectBinder.BindSparksEffect(catalog.Sparks, itemData);

            Transform sparksMotionRoot = EnsureSparksAnchor(catalog.WorldEffectsRoot, catalog.Sparks);
            NadaMotionBinder.BindWorldFollow(sparksMotionRoot, catalog.Flare);

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
        
        // Mirage still lives on the world branch for now, but its follow behavior should belong
        // to a dedicated motion root instead of the visual object itself.
        private static Transform EnsureMirageAnchor(Transform worldEffectsRoot, Transform mirageTf)
        {
            if (mirageTf == null)
                return null;

            if (worldEffectsRoot == null)
                return mirageTf;

            Transform anchorTf = NadaRigTransforms.EnsureChild(worldEffectsRoot, "Mirage Anchor");
            if (anchorTf == null)
                return mirageTf;

            if (mirageTf.parent != anchorTf)
            {
                mirageTf.SetParent(anchorTf, true);

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Reparented Mirage under '{NadaWeaponTargets.FullPath(anchorTf)}'.");
            }

            return anchorTf;
        }
        
        // Sparks still lives on the world branch for now, but its follow behavior should belong
        // to a dedicated motion root instead of the visual object itself.
        private static Transform EnsureSparksAnchor(Transform worldEffectsRoot, Transform sparksTf)
        {
            if (sparksTf == null)
                return null;

            if (worldEffectsRoot == null)
                return sparksTf;

            Transform anchorTf = NadaRigTransforms.EnsureChild(worldEffectsRoot, "Sparks Anchor");
            if (anchorTf == null)
                return sparksTf;

            if (sparksTf.parent != anchorTf)
            {
                sparksTf.SetParent(anchorTf, true);

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Reparented Sparks under '{NadaWeaponTargets.FullPath(anchorTf)}'.");
            }

            return anchorTf;
        }
    }
}