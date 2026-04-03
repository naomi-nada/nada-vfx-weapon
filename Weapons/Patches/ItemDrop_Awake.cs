using System;
using HarmonyLib;
using NADA.VFX.Weapons.Targets;

namespace NADA.VFX.Weapons.Patches
{
    [HarmonyPatch(typeof(global::ItemDrop), "Awake")]
    internal static class ItemDrop_Awake
    {
        private static void Postfix(global::ItemDrop __instance)
        {
            try
            {
                if (__instance == null) return;
                var go = __instance.gameObject;
                if (go == null) return;
                if (!NadaWeaponTargets.IsTargetRoot(go)) return;
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning($"{Plugin.ModName}: ItemDrop.Awake postfix error: {e}");
            }
        }
    }
}