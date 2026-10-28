
using System;
using HarmonyLib;
using NADA.VFX.Weapon.Core.Network;
using NADA.VFX.Weapon.Weapons.Runtime;
using UnityEngine;

namespace NADA.VFX.Weapon.Weapons.Patches
{
    // Left-hand native transport uses the same equipped visual lifecycle
    // as the existing right-hand runtime, but never borrows its state.
    [HarmonyPatch(typeof(global::VisEquipment), "UpdateEquipmentVisuals")]
    internal static class NadaVfxLeftHandNetworkVisualPatch
    {
        private static readonly System.Reflection.FieldInfo NViewField =
            AccessTools.Field(typeof(global::VisEquipment), "m_nview");

        private static readonly System.Reflection.FieldInfo LeftInstanceField =
            AccessTools.Field(typeof(global::VisEquipment), "m_leftItemInstance");

        private static void Postfix(global::VisEquipment __instance)
        {
            try
            {
                if (__instance == null || Player.m_localPlayer == null)
                    return;

                global::Player player = __instance
                    .GetComponentInParent<global::Player>();

                if (player == null)
                    return;

                ZNetView view = NViewField?.GetValue(__instance) as ZNetView;

                if (view == null || !view.IsValid())
                    return;

                ZDO zdo = view.GetZDO();

                if (zdo == null)
                    return;

                int hash = zdo.GetInt(ZDOVars.s_leftItem, 0);

                if (ReferenceEquals(player, Player.m_localPlayer))
                {
                    if (!view.IsOwner())
                        return;

                    NadaVfxLeftHandStatePublisher.Observe(
                        NadaEquippedItemResolver.ResolveLeftHandItem(),
                        hash,
                        zdo);
                }
                else
                {
                    var wrapper = LeftInstanceField?.GetValue(__instance)
                        as GameObject;

                    uint generation = unchecked((uint)zdo.GetInt(
                        NadaVfxLeftHandStatePublisher.EquipmentGenerationKey,
                        0));

                    NadaVfxLeftHandRemoteTracker.Observe(
                        zdo.GetOwner(),
                        __instance,
                        wrapper,
                        hash,
                        generation,
                        zdo);
                }
            }
            catch (Exception error)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [LeftNativeVisualPatchFailed] " +
                    error.GetBaseException().Message);
            }
        }
    }

    [HarmonyPatch(typeof(NadaVfxRightHandStatePublisher), "MarkDirty")]
    internal static class NadaVfxLeftHandNetworkDirtyPatch
    {
        private static void Postfix(global::ItemDrop.ItemData itemData)
        {
            try
            {
                NadaVfxLeftHandStatePublisher.MarkDirty(itemData);
            }
            catch (Exception error)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [LeftNativeDirtyFailed] " +
                    error.GetBaseException().Message);
            }
        }
    }

    [HarmonyPatch(typeof(NadaVfxRightHandRemoteStateTracker),
        "RefreshPresentationVisibility")]
    internal static class NadaVfxLeftHandNetworkVisibilityPatch
    {
        private static void Postfix()
        {
            try
            {
                NadaVfxLeftHandRemoteTracker.RefreshPresentationVisibility();
            }
            catch (Exception error)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [LeftNativeVisibilityFailed] " +
                    error.GetBaseException().Message);
            }
        }
    }

    [HarmonyPatch(typeof(global::Player), "OnDestroy")]
    internal static class NadaVfxLeftHandNetworkPlayerLifetimePatch
    {
        private static void Prefix(global::Player __instance)
        {
            try
            {
                NadaVfxLeftHandRemoteTracker.ReleasePlayerInstance(__instance);
            }
            catch (Exception error)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [LeftNativePlayerReleaseFailed] " +
                    error.GetBaseException().Message);
            }
        }
    }
}
