using NADA.VFX.Weapon.Modules.Effects;
using UnityEngine;

namespace NADA.VFX.Weapon.Runtime.Binding
{
    internal static class NadaEffectBinder
    {
        internal static void BindInnerFlamesEffect(
            Transform innerFlamesTransform,
            global::ItemDrop.ItemData itemData)
        {
            BindItemDataEffect<NadaInnerFlamesEffect>(innerFlamesTransform, itemData);
        }

        internal static void BindOuterFlamesEffect(
            Transform outerFlamesTransform,
            global::ItemDrop.ItemData itemData)
        {
            BindItemDataEffect<NadaOuterFlamesEffect>(outerFlamesTransform, itemData);
        }
        
        internal static void BindSparksEffect(
            Transform sparksTransform,
            global::ItemDrop.ItemData itemData)
        {
            BindItemDataEffect<NadaSparksEffect>(sparksTransform, itemData);
        }

        internal static void BindFlareEffect(
            Transform flareTransform,
            global::ItemDrop.ItemData itemData)
        {
            BindItemDataEffect<NadaFlareEffect>(flareTransform, itemData);
        }
        
        internal static void BindAuraEffect(
            Transform auraTransform,
            global::ItemDrop.ItemData itemData)
        {
            BindItemDataEffect<NadaAuraEffect>(auraTransform, itemData);
        }

        internal static void BindOrbitalsEffect(
            Transform orbitalsRootTransform,
            Transform localOrbsRootTransform,
            global::ItemDrop.ItemData itemData)
        {
            if (orbitalsRootTransform == null)
                return;

            var orbitalsEffect = GetOrAddEffect<NadaOrbitalsEffect>(orbitalsRootTransform);
            orbitalsEffect.SetItemData(itemData);
            orbitalsEffect.SetLocalOrbsRootTransform(localOrbsRootTransform);
        }
        
        internal static void BindStrandsEffect(
            Transform strandsTransform,
            global::ItemDrop.ItemData itemData)
        {
            BindItemDataEffect<NadaStrandsEffect>(strandsTransform, itemData);
        }

        private static void BindItemDataEffect<TEffect>(
            Transform effectRootTransform,
            global::ItemDrop.ItemData itemData)
            where TEffect : MonoBehaviour, INadaItemDataReceiver
        {
            if (effectRootTransform == null)
                return;

            TEffect effect = GetOrAddEffect<TEffect>(effectRootTransform);
            effect.SetItemData(itemData);
        }

        private static TEffect GetOrAddEffect<TEffect>(Transform rootTransform)
            where TEffect : Component
        {
            TEffect effect = rootTransform.GetComponent<TEffect>();
            if (effect == null)
                effect = rootTransform.gameObject.AddComponent<TEffect>();

            return effect;
        }
    }
}