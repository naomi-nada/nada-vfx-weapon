using System;
using HarmonyLib;

namespace NADA.VFX.Weapon.Weapons.Patches
{
    [HarmonyPatch(typeof(ZNetScene), "Awake")]
    internal static class ZNetScene_Awake
    {
        private static void Postfix()
        {
            try
            {
                Plugin.Log.LogInfo($"{Plugin.ModName}: ZNetScene.Awake postfix fired.");
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning($"{Plugin.ModName}: ZNetScene.Awake postfix error: {e}");
            }
        }
    }
}