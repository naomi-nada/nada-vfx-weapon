using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Blocks.Effects;
using UnityEngine;

namespace NADA.VFX.Weapon.Runtime.Structure
{
    // Appearance blocks still have stable instance roots. Their visual
    // contribution is resolved separately and applied once to the weapon mesh.
    internal static class NadaWeaponAppearanceBlockAssembly
    {
        internal static void Reconcile(
            Transform effectsRoot,
            WeaponVfxState state,
            string ownerName)
        {
            if (effectsRoot == null)
                return;

            NadaEffectInstanceAssembly.ReconcileInstancesOfType(
                effectsRoot, state, VfxEffectTypeIds.WeaponAppearance,
                ownerName);

            if (state?.Effects == null)
                return;

            foreach (VfxEffectBlock block in state.Effects)
            {
                if (block?.TypeId != VfxEffectTypeIds.WeaponAppearance)
                    continue;

                if (block.Transform == null ||
                    block.Settings is not WeaponAppearanceVfxSettings)
                {
                    Transform instances = NadaRigPaths.FindDirectChild(
                        effectsRoot, NadaRigPaths.EffectInstancesRootName);
                    if (instances != null)
                        NadaEffectInstanceAssembly.RemoveInstance(
                            instances, block.InstanceId);
                    continue;
                }

                NadaEffectInstance instance =
                    NadaEffectInstanceAssembly.EnsureInstance(
                        effectsRoot, block.InstanceId, block.TypeId, ownerName);
                if (instance?.IsValid == true)
                    instance.RootObject.SetActive(block.Enabled);
            }
        }

    }
}