using System;
using HarmonyLib;
using NADA.VFX.Weapon.Weapons.Runtime;

namespace NADA.VFX.Weapon.Weapons.Patches
{
    [HarmonyPatch(
        typeof(global::Player),
        "OnDestroy")]
    internal static class NadaVfxRemotePlayerLifecyclePatch
    {
        private static void Prefix(
            global::Player __instance)
        {
            try
            {
                NadaVfxRemoteStateTracker
                    .ReleasePlayerInstance(
                        __instance);
            }
            catch (Exception e)
            {
                // Never let NADA's cleanup interfere with vanilla player
                // destruction.
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [RemotePlayerRelease FAIL] {e}");
            }
        }
    }
}