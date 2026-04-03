using System;
using HarmonyLib;
using NADA.VFX.Runtime.Binding;

namespace NADA.VFX.Weapons.Patches
{
    [HarmonyPatch(typeof(ZNetScene), "Awake")]
    internal static class ZNetScene_Awake
    {
        private static void Postfix()
        {
            try
            {
                Plugin.Log.LogInfo($"{Plugin.ModName}: ZNetScene.Awake postfix fired.");
                
                NadaRigCatalogAssembly.DestroyCatalogRoots();
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning($"{Plugin.ModName}: ZNetScene.Awake postfix error: {e}");
            }
        }
    }
}