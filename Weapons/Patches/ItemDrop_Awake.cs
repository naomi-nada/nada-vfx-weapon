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
                // Reserved for dropped-item / pickup-side runtime fixes.
                // This remains the intended entry point for dropped instance
                // shader repair and visual rig application work.
                if (__instance == null) return;

                var itemDropObject = __instance.gameObject;
                if (itemDropObject == null) return;
                if (!NadaWeaponTargets.IsTargetRoot(itemDropObject)) return;
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning($"{Plugin.ModName}: ItemDrop.Awake postfix error: {e}");
            }
        }
    }
}