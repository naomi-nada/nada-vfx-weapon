using NADA.VFX.Modules.Properties;
using NADA.VFX.Runtime.Binding;
using UnityEngine;

namespace NADA.VFX.Runtime.Execution
{
    internal static class NadaModuleBinder
    {
        public static void BindPropertyModules(
            GameObject effectsRoot,
            RigGroups effectsGroups,
            global::ItemDrop.ItemData itemData)
        {
            if (effectsRoot == null) return;

            BindColorModule(effectsRoot, effectsGroups, itemData);
            BindEnergyModule(effectsRoot, effectsGroups, itemData);
            BindScaleModule(effectsRoot, effectsGroups, itemData);
        }

        private static void BindColorModule(
            GameObject effectsRoot,
            RigGroups effectsGroups,
            global::ItemDrop.ItemData itemData)
        {
            var colorModule = effectsRoot.GetComponent<NadaColorModule>();
            if (colorModule == null)
                colorModule = effectsRoot.AddComponent<NadaColorModule>();

            colorModule.SetItemData(itemData);
            colorModule.SetRigGroups(effectsGroups);
        }

        private static void BindEnergyModule(
            GameObject effectsRoot,
            RigGroups effectsGroups,
            global::ItemDrop.ItemData itemData)
        {
            var energyModule = effectsRoot.GetComponent<NadaEnergyModule>();
            if (energyModule == null)
                energyModule = effectsRoot.AddComponent<NadaEnergyModule>();

            energyModule.SetItemData(itemData);
            energyModule.SetRigGroups(effectsGroups);
        }

        private static void BindScaleModule(
            GameObject effectsRoot,
            RigGroups effectsGroups,
            global::ItemDrop.ItemData itemData)
        {
            var scaleModule = effectsRoot.GetComponent<NadaScaleModule>();
            if (scaleModule == null)
                scaleModule = effectsRoot.AddComponent<NadaScaleModule>();

            scaleModule.SetItemData(itemData);
            scaleModule.SetRigGroups(effectsGroups);
        }
    }
}