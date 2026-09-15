using System;
using HarmonyLib;
using NADA.VFX.Weapon.Core.Config;
using NADA.VFX.Weapon.Core.Debug;
using NADA.VFX.Weapon.Core.Network;
using NADA.VFX.Weapon.Core.State;
using NADA.VFX.Weapon.Weapons.Runtime;
using NADA.VFX.Weapon.Weapons.Targets;
using NADA.VFX.Weapon.Runtime.Structure;
using UnityEngine;

namespace NADA.VFX.Weapon.Weapons.Patches
{
    [HarmonyPatch(typeof(global::VisEquipment), "UpdateEquipmentVisuals")]
    internal static class VisEquipment
    {
        private static readonly System.Reflection.FieldInfo RightInstField =
            AccessTools.Field(
                typeof(global::VisEquipment),
                "m_rightItemInstance");

        private static readonly System.Reflection.FieldInfo LeftInstField =
            AccessTools.Field(
                typeof(global::VisEquipment),
                "m_leftItemInstance");

        private static readonly System.Reflection.FieldInfo NViewField =
            AccessTools.Field(
                typeof(global::VisEquipment),
                "m_nview");

        private const string RemoteRightPayloadKey =
            "nada.vfx.right.payload";

        private static readonly NadaWeaponRigController WeaponRigController =
            new();

        private static void Postfix(global::VisEquipment __instance)
        {
            try
            {
                if (__instance == null)
                    return;

                if (!IsAllowedRigOwner(__instance))
                {
                    HandleRemoteEquipment(__instance);
                    return;
                }

                if (Player.m_localPlayer == null &&
                    PluginConfig.CharacterSelectionVisibility.Value)
                {
                    NadaLogControl.Info(
                        $"char-select-probe:{__instance.GetInstanceID()}",
                        $"{Plugin.ModName}: [CharSelectProbe] " +
                        $"vis='{__instance.name}' " +
                        $"path='{NadaWeaponTargets.FullPath(__instance.transform)}'");
                }

                GameObject rightInstance =
                    SafeGetGameObject(
                        RightInstField,
                        __instance);

                GameObject leftInstance =
                    SafeGetGameObject(
                        LeftInstField,
                        __instance);

                global::ItemDrop.ItemData rightItem =
                    NadaEquippedItemResolver.ResolveRightHandItem();

                global::ItemDrop.ItemData leftItem =
                    NadaEquippedItemResolver.ResolveLeftHandItem();

                WriteLocalRightPayload(
                    __instance,
                    rightItem);

                TryApplyIfBound(
                    rightInstance,
                    rightItem);

                TryApplyIfBound(
                    leftInstance,
                    leftItem);
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: " +
                    $"VisEquipment.UpdateEquipmentVisuals postfix error: {e}");
            }
        }

        private static void WriteLocalRightPayload(
            global::VisEquipment visEquipment,
            global::ItemDrop.ItemData rightItem)
        {
            if (visEquipment == null)
                return;

            if (Player.m_localPlayer == null)
                return;

            if (!visEquipment.transform.IsChildOf(
                    Player.m_localPlayer.transform))
            {
                return;
            }

            ZNetView nview =
                SafeGetZNetView(visEquipment);

            if (nview == null ||
                !nview.IsValid() ||
                !nview.IsOwner())
            {
                return;
            }

            ZDO zdo =
                nview.GetZDO();

            if (zdo == null)
                return;

            int rightItemHash =
                ReadRightItemHash(visEquipment);

            string currentPayload =
                zdo.GetString(
                    RemoteRightPayloadKey,
                    string.Empty);

            // Valheim can briefly report no usable right-hand identity while
            // equipment visuals are transitioning. That is not an unbind.
            //
            // Keep the last payload until we can resolve a real item again.
            // The remote hash check will fail closed if the visible weapon
            // changes before the new NADA state arrives.
            if (rightItem == null ||
                rightItemHash == 0)
            {
                return;
            }

            bool isBound =
                VfxStateIO.IsBound(rightItem);

            string payload;

            if (isBound)
            {
                if (!VfxStateIO.TryRead(
                        rightItem,
                        out VfxState state))
                {
                    return;
                }

                payload =
                    NadaVfxNetworkCodec.Serialize(
                        rightItemHash,
                        true,
                        state);
            }
            else
            {
                // A resolved item that is explicitly unbound is authoritative.
                payload =
                    string.Empty;
            }

            if (currentPayload == payload)
                return;

            zdo.Set(
                RemoteRightPayloadKey,
                payload);

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [NetworkStateWrite] " +
                $"rightHash={rightItemHash} " +
                $"bound={isBound} " +
                $"chars={payload.Length}");
        }

        private static void HandleRemoteEquipment(
            global::VisEquipment visEquipment)
        {
            if (visEquipment == null)
                return;

            if (Player.m_localPlayer == null)
                return;

            if (visEquipment.transform.IsChildOf(
                    Player.m_localPlayer.transform))
            {
                return;
            }

            GameObject rightInstance =
                SafeGetGameObject(
                    RightInstField,
                    visEquipment);

            GameObject leftInstance =
                SafeGetGameObject(
                    LeftInstField,
                    visEquipment);

            Transform rightVisualRoot =
                rightInstance != null
                    ? NadaWeaponTargets.FindEquippedWeaponVisualRoot(
                        rightInstance.transform)
                    : null;

            Transform leftVisualRoot =
                leftInstance != null
                    ? NadaWeaponTargets.FindEquippedWeaponVisualRoot(
                        leftInstance.transform)
                    : null;

            NadaLogControl.Info(
                $"remote-state:" +
                $"{visEquipment.GetInstanceID()}:" +
                $"{rightInstance?.GetInstanceID() ?? 0}:" +
                $"{leftInstance?.GetInstanceID() ?? 0}",
                $"{Plugin.ModName}: [RemoteEquipProbe] " +
                $"vis='{visEquipment.name}' " +
                $"path='{NadaWeaponTargets.FullPath(visEquipment.transform)}' " +
                $"rightInstance='{rightInstance?.name ?? "<null>"}' " +
                $"rightVisual='{rightVisualRoot?.name ?? "<null>"}' " +
                $"rightPath='{NadaWeaponTargets.FullPath(rightVisualRoot)}' " +
                $"leftInstance='{leftInstance?.name ?? "<null>"}' " +
                $"leftVisual='{leftVisualRoot?.name ?? "<null>"}' " +
                $"leftPath='{NadaWeaponTargets.FullPath(leftVisualRoot)}'");

            TryApplyRemoteRightWeaponState(
                visEquipment,
                rightInstance);
        }

        private static void TryApplyRemoteRightWeaponState(
            global::VisEquipment visEquipment,
            GameObject rightInstance)
        {
            if (visEquipment == null ||
                rightInstance == null)
            {
                return;
            }

            ZNetView nview =
                SafeGetZNetView(visEquipment);

            if (nview == null ||
                !nview.IsValid())
            {
                return;
            }

            ZDO zdo =
                nview.GetZDO();

            if (zdo == null)
                return;

            string payload =
                zdo.GetString(
                    RemoteRightPayloadKey,
                    string.Empty);

            if (string.IsNullOrWhiteSpace(payload))
            {
                NadaWeaponRigRemoval.RemoveFromEquippedRoot(
                    rightInstance);

                return;
            }

            if (!NadaVfxNetworkCodec.TryDeserialize(
                    payload,
                    out NadaVfxNetworkSnapshot snapshot,
                    out VfxState state))
            {
                NadaLogControl.Info(
                    $"remote-network-fail:" +
                    $"{visEquipment.GetInstanceID()}:" +
                    $"{payload.GetHashCode()}",
                    $"{Plugin.ModName}: [RemoteNetworkState FAIL] " +
                    $"payload could not be decoded.");

                NadaWeaponRigRemoval.RemoveFromEquippedRoot(
                    rightInstance);

                return;
            }

            if (!snapshot.Bound)
            {
                NadaLogControl.Info(
                    $"remote-network-unbound:" +
                    $"{visEquipment.GetInstanceID()}:" +
                    $"{snapshot.ItemHash}",
                    $"{Plugin.ModName}: [RemoteNetworkState UNBOUND] " +
                    $"hash={snapshot.ItemHash}");

                NadaWeaponRigRemoval.RemoveFromEquippedRoot(
                    rightInstance);

                return;
            }

            int rightItemHash =
                ReadRightItemHash(visEquipment);

            if (rightItemHash == 0 ||
                snapshot.ItemHash != rightItemHash)
            {
                NadaLogControl.Info(
                    $"remote-network-stale:" +
                    $"{visEquipment.GetInstanceID()}:" +
                    $"{snapshot.ItemHash}:" +
                    $"{rightItemHash}",
                    $"{Plugin.ModName}: [RemoteNetworkState STALE] " +
                    $"payloadHash={snapshot.ItemHash} " +
                    $"visibleHash={rightItemHash}");

                return;
            }

            NadaLogControl.Info(
                $"remote-network-ok:" +
                $"{visEquipment.GetInstanceID()}:" +
                $"{snapshot.ItemHash}:" +
                $"{payload.GetHashCode()}",
                $"{Plugin.ModName}: [RemoteNetworkState OK] " +
                $"hash={snapshot.ItemHash} " +
                $"bound={snapshot.Bound} " +
                $"entries={snapshot.Entries?.Count ?? 0} " +
                $"chars={payload.Length} " +
                $"innerHue={state.InnerFlamesHue:F3} " +
                $"innerScale={state.InnerFlamesScale:F3} " +
                $"outer={state.OuterFlamesEnabled} " +
                $"auraHue={state.AuraHue:F3} " +
                $"orbsCount={state.OrbitalsOrbsCount:F3} " +
                $"rigX={state.RigXOffset:F3}");

            bool applied =
                WeaponRigController
                    .TryApplyResolvedState(
                        rightInstance,
                        snapshot.ItemHash,
                        state);

            NadaLogControl.Info(
                $"remote-state-result:" +
                $"{visEquipment.GetInstanceID()}:" +
                $"{rightInstance.GetInstanceID()}:" +
                $"{payload.GetHashCode()}",
                $"{Plugin.ModName}: [RemoteStateResult] " +
                $"hash={rightItemHash} " +
                $"applied={applied}");
        }

        private static void TryApplyIfBound(
            GameObject itemInstance,
            global::ItemDrop.ItemData itemData)
        {
            if (itemInstance == null ||
                itemData == null)
            {
                return;
            }

            if (!VfxStateIO.IsBound(itemData))
                return;

            WeaponRigController.TryApply(
                itemInstance,
                itemData);
        }

        private static GameObject SafeGetGameObject(
            System.Reflection.FieldInfo field,
            global::VisEquipment visEquipment)
        {
            try
            {
                if (field == null ||
                    visEquipment == null)
                {
                    return null;
                }

                return field.GetValue(
                    visEquipment) as GameObject;
            }
            catch
            {
                return null;
            }
        }

        private static ZNetView SafeGetZNetView(
            global::VisEquipment visEquipment)
        {
            try
            {
                if (NViewField == null ||
                    visEquipment == null)
                {
                    return null;
                }

                return NViewField.GetValue(
                    visEquipment) as ZNetView;
            }
            catch
            {
                return null;
            }
        }

        private static int ReadRightItemHash(
            global::VisEquipment visEquipment)
        {
            ZNetView nview =
                SafeGetZNetView(visEquipment);

            if (nview == null ||
                !nview.IsValid())
            {
                return 0;
            }

            ZDO zdo =
                nview.GetZDO();

            if (zdo == null)
                return 0;

            return zdo.GetInt(
                ZDOVars.s_rightItem);
        }

        private static bool IsAllowedRigOwner(
            global::VisEquipment visEquipment)
        {
            if (visEquipment == null)
                return false;

            if (Player.m_localPlayer != null)
            {
                return visEquipment.transform.IsChildOf(
                    Player.m_localPlayer.transform);
            }

            if (UnityEngine.SceneManagement.SceneManager
                    .GetActiveScene()
                    .name != "start")
            {
                return false;
            }

            Transform root =
                visEquipment.transform;

            while (root.parent != null)
                root = root.parent;

            return root.name.StartsWith(
                "Player",
                StringComparison.Ordinal);
        }
    }
}