using System;
using System.Collections;
using HarmonyLib;
using NADA.VFX.Weapon.Core.Config;
using NADA.VFX.Weapon.Core.State;
using NADA.VFX.Weapon.Weapons.Targets;

namespace NADA.VFX.Weapon.Weapons.Patches
{
    [HarmonyPatch(typeof(global::ItemDrop), "Awake")]
    internal static class ItemDrop_Awake
    {
        private static void Postfix(global::ItemDrop __instance)
        {
            try
            {
                if (__instance == null)
                    return;

                if (!PluginConfig.DroppedItemVisibility.Value)
                    return;

                if (Plugin.Instance == null)
                    return;

                Plugin.Instance.StartCoroutine(CheckDroppedItemWhenReady(__instance));
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning($"{Plugin.ModName}: ItemDrop.Awake postfix error: {e}");
            }
        }

        private static IEnumerator CheckDroppedItemWhenReady(global::ItemDrop itemDrop)
        {
            yield return null;
            yield return null;

            if (itemDrop == null || itemDrop.gameObject == null)
                yield break;

            var itemData = itemDrop.m_itemData;
            if (itemData == null)
                yield break;

            bool isBound = VfxStateIO.IsBound(itemData);
            if (!isBound)
                yield break;

            var controller = new Runtime.NadaWeaponRigController();

            bool applied =
                controller.TryApplyDroppedItem(itemDrop.gameObject, itemData);

            if (!applied)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [DropApply FAIL] " +
                    $"object='{itemDrop.gameObject.name}' " +
                    $"item='{itemData.m_shared?.m_name}' " +
                    $"path='{NadaWeaponTargets.FullPath(itemDrop.transform)}'");
                yield break;
            }

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [DropApply OK] " +
                $"object='{itemDrop.gameObject.name}' " +
                $"item='{itemData.m_shared?.m_name}' " +
                $"path='{NadaWeaponTargets.FullPath(itemDrop.transform)}'");
        }
    }
}