using NADA.VFX.Modules.Effects;
using UnityEngine;

namespace NADA.VFX.Runtime.Execution
{
    internal static class NadaEffectBinder
    {
        public static void BindOrbitalsEffect(
            Transform orbitalsRoot,
            global::ItemDrop.ItemData itemData)
        {
            if (orbitalsRoot == null) return;

            var effect = orbitalsRoot.GetComponent<NadaOrbitalsEffect>();
            if (effect == null)
                effect = orbitalsRoot.gameObject.AddComponent<NadaOrbitalsEffect>();

            effect.SetItemData(itemData);
        }

        public static void BindMirageEffect(
            Transform mirageTf,
            global::ItemDrop.ItemData itemData)
        {
            if (mirageTf == null) return;

            var effect = mirageTf.GetComponent<NadaMirageEffect>();
            if (effect == null)
                effect = mirageTf.gameObject.AddComponent<NadaMirageEffect>();

            effect.SetItemData(itemData);
        }

        public static void BindSparksEffect(
            Transform sparksTf,
            global::ItemDrop.ItemData itemData)
        {
            if (sparksTf == null) return;

            var effect = sparksTf.GetComponent<NadaSparksEffect>();
            if (effect == null)
                effect = sparksTf.gameObject.AddComponent<NadaSparksEffect>();

            effect.SetItemData(itemData);
        }
    }
}