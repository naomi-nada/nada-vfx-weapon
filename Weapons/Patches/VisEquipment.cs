using System;
using System.Collections.Generic;
using HarmonyLib;
using NADA.VFX.Weapon.Core.Config;
using NADA.VFX.Weapon.Core.Debug;
using NADA.VFX.Weapon.Core.Network;
using NADA.VFX.Weapon.Core.State;
using NADA.VFX.Weapon.Runtime.Structure;
using NADA.VFX.Weapon.Weapons.Runtime;
using NADA.VFX.Weapon.Weapons.Targets;
using UnityEngine;

namespace NADA.VFX.Weapon.Weapons.Patches
{
    [HarmonyPatch(
        typeof(global::VisEquipment),
        "UpdateEquipmentVisuals")]
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

        private const string LegacyRemoteRightPayloadKey =
            "nada.vfx.right.payload";

        private static readonly NadaWeaponRigController WeaponRigController =
            new();

        private static readonly HashSet<int>
            LegacyPayloadCheckedVisEquipment =
                new();

        private const float LocalVisualRecheckSeconds = 1f;

        private static readonly LocalApplyCache RightLocalApplyCache =
            new();

        private static readonly LocalApplyCache LeftLocalApplyCache =
            new();

        private sealed class LocalApplyCache
        {
            internal global::VisEquipment Owner;
            internal GameObject ItemInstance;
            internal global::ItemDrop.ItemData ItemData;
            internal Transform VisualRoot;
            internal Transform RigRoot;
            internal float NextVisualRecheckTime;

            internal global::VisEquipment LastUnboundOwner;
            internal GameObject LastUnboundInstance;
            internal global::ItemDrop.ItemData LastUnboundItemData;

            internal void Clear()
            {
                Owner = null;
                ItemInstance = null;
                ItemData = null;
                VisualRoot = null;
                RigRoot = null;
                NextVisualRecheckTime = 0f;

                LastUnboundOwner = null;
                LastUnboundInstance = null;
                LastUnboundItemData = null;
            }
        }

        private static void Postfix(
            global::VisEquipment __instance)
        {
            try
            {
                if (__instance == null)
                    return;

                if (!IsAllowedRigOwner(
                        __instance))
                {
                    if (IsRemotePlayer(
                            __instance))
                    {
                        HandleRemoteEquipment(
                            __instance);
                    }

                    return;
                }

                bool isLocalPlayer =
                    Player.m_localPlayer != null;

                if (!isLocalPlayer &&
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
                    NadaEquippedItemResolver
                        .ResolveRightHandItem();

                global::ItemDrop.ItemData leftItem =
                    NadaEquippedItemResolver
                        .ResolveLeftHandItem();

                if (isLocalPlayer)
                {
                    ClearLegacyLocalRightPayloadOnce(
                        __instance);

                    int rightItemHash =
                        ReadRightItemHash(
                            __instance);

                    NadaVfxLocalStatePublisher
                        .ObserveRight(
                            rightItem,
                            rightItemHash);
                }

                TryApplyIfBound(
                    __instance,
                    rightInstance,
                    rightItem,
                    isLocalPlayer ? RightLocalApplyCache : null);

                TryApplyIfBound(
                    __instance,
                    leftInstance,
                    leftItem,
                    isLocalPlayer ? LeftLocalApplyCache : null);
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: " +
                    $"VisEquipment.UpdateEquipmentVisuals postfix error: {e}");
            }
        }

        private static void HandleRemoteEquipment(
            global::VisEquipment visEquipment)
        {
            if (!IsRemotePlayer(
                    visEquipment))
            {
                return;
            }

            GameObject rightInstance =
                SafeGetGameObject(
                    RightInstField,
                    visEquipment);

            long peerId =
                ReadPeerId(
                    visEquipment);

            int rightItemHash =
                ReadRightItemHash(
                    visEquipment);

            NadaVfxRemoteStateTracker
                .ObserveRight(
                    peerId,
                    visEquipment,
                    rightInstance,
                    rightItemHash);
        }

        private static void ClearLegacyLocalRightPayloadOnce(
            global::VisEquipment visEquipment)
        {
            if (visEquipment == null ||
                Player.m_localPlayer == null)
            {
                return;
            }

            if (!visEquipment.transform.IsChildOf(
                    Player.m_localPlayer.transform))
            {
                return;
            }

            int visEquipmentId =
                visEquipment.GetInstanceID();

            if (LegacyPayloadCheckedVisEquipment.Contains(
                    visEquipmentId))
            {
                return;
            }

            ZNetView nview =
                SafeGetZNetView(
                    visEquipment);

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

            LegacyPayloadCheckedVisEquipment.Add(
                visEquipmentId);

            string legacyPayload =
                zdo.GetString(
                    LegacyRemoteRightPayloadKey,
                    string.Empty);

            if (string.IsNullOrEmpty(
                    legacyPayload))
            {
                return;
            }

            zdo.Set(
                LegacyRemoteRightPayloadKey,
                string.Empty);

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [LegacyNetworkPayloadCleared] " +
                $"vis={visEquipmentId} " +
                $"chars={legacyPayload.Length}");
        }

        private static void TryApplyIfBound(
            global::VisEquipment visEquipment,
            GameObject itemInstance,
            global::ItemDrop.ItemData itemData,
            LocalApplyCache applyCache)
        {
            if (itemInstance == null ||
                itemData == null)
            {
                applyCache?.Clear();
                return;
            }

            bool isBound =
                VfxStateIO.IsBound(
                    itemData);

            bool hasEditorPreview =
                NadaWeaponEditorPreviewState.TryGet(
                    itemData,
                    out _);

            if (!isBound &&
                !hasEditorPreview)
            {
                if (applyCache == null)
                    return;

                if (applyCache.LastUnboundOwner == visEquipment &&
                    applyCache.LastUnboundInstance == itemInstance &&
                    ReferenceEquals(
                        applyCache.LastUnboundItemData,
                        itemData))
                {
                    return;
                }

                bool removedTracked =
                    applyCache.Owner == visEquipment &&
                    NadaWeaponRigRemoval.RemoveTrackedRig(
                        applyCache.RigRoot);

                if (!removedTracked)
                {
                    NadaWeaponRigRemoval.RemoveFromEquippedRoot(
                        itemInstance);
                }

                applyCache.Clear();

                applyCache.LastUnboundOwner =
                    visEquipment;

                applyCache.LastUnboundInstance =
                    itemInstance;

                applyCache.LastUnboundItemData =
                    itemData;

                return;
            }

            if (CanSkipLocalApply(
                    applyCache,
                    visEquipment,
                    itemInstance,
                    itemData))
            {
                return;
            }

            applyCache?.Clear();

            bool applied =
                WeaponRigController.TryApply(
                    itemInstance,
                    itemData);

            if (!applied ||
                applyCache == null)
            {
                return;
            }

            Transform visualRoot =
                NadaWeaponTargets.FindEquippedWeaponVisualRoot(
                    itemInstance.transform);

            if (visualRoot == null)
                return;

            Transform rigRoot =
                NadaRigPaths.FindDirectChild(
                    visualRoot,
                    Plugin.LocalWeaponRootName);

            if (rigRoot == null)
                return;

            applyCache.Owner =
                visEquipment;

            applyCache.ItemInstance =
                itemInstance;

            applyCache.ItemData =
                itemData;

            applyCache.VisualRoot =
                visualRoot;

            applyCache.RigRoot =
                rigRoot;

            applyCache.NextVisualRecheckTime =
                Time.realtimeSinceStartup +
                LocalVisualRecheckSeconds;
        }

        private static bool CanSkipLocalApply(
            LocalApplyCache applyCache,
            global::VisEquipment visEquipment,
            GameObject itemInstance,
            global::ItemDrop.ItemData itemData)
        {
            if (applyCache == null)
                return false;

            if (applyCache.Owner != visEquipment ||
                applyCache.ItemInstance != itemInstance ||
                !ReferenceEquals(
                    applyCache.ItemData,
                    itemData))
            {
                return false;
            }

            Transform visualRoot =
                applyCache.VisualRoot;

            Transform rigRoot =
                applyCache.RigRoot;

            if (visualRoot == null ||
                rigRoot == null)
            {
                return false;
            }

            if (visualRoot.parent !=
                    itemInstance.transform ||
                !visualRoot.gameObject.activeInHierarchy ||
                rigRoot.parent !=
                    visualRoot ||
                !rigRoot.gameObject.activeInHierarchy)
            {
                return false;
            }

            float now =
                Time.realtimeSinceStartup;

            if (now >=
                applyCache.NextVisualRecheckTime)
            {
                Transform currentVisualRoot =
                    NadaWeaponTargets.FindEquippedWeaponVisualRoot(
                        itemInstance.transform);

                if (currentVisualRoot !=
                    visualRoot)
                {
                    return false;
                }

                applyCache.NextVisualRecheckTime =
                    now +
                    LocalVisualRecheckSeconds;
            }

            return true;
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
                SafeGetZNetView(
                    visEquipment);

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

        private static long ReadPeerId(
            global::VisEquipment visEquipment)
        {
            ZNetView nview =
                SafeGetZNetView(
                    visEquipment);

            if (nview == null ||
                !nview.IsValid())
            {
                return 0L;
            }

            ZDO zdo =
                nview.GetZDO();

            if (zdo == null)
                return 0L;

            return zdo.GetOwner();
        }

        private static bool IsRemotePlayer(
            global::VisEquipment visEquipment)
        {
            if (visEquipment == null ||
                Player.m_localPlayer == null)
            {
                return false;
            }

            global::Player player =
                visEquipment
                    .GetComponentInParent<global::Player>();

            if (player == null)
                return false;

            return
                player !=
                    Player.m_localPlayer &&
                !visEquipment.transform.IsChildOf(
                    Player.m_localPlayer.transform);
        }

        private static bool IsAllowedRigOwner(
            global::VisEquipment visEquipment)
        {
            if (visEquipment == null)
                return false;

            if (Player.m_localPlayer != null)
            {
                return
                    visEquipment.transform.IsChildOf(
                        Player.m_localPlayer.transform);
            }

            if (UnityEngine.SceneManagement
                    .SceneManager
                    .GetActiveScene()
                    .name != "start")
            {
                return false;
            }

            Transform root =
                visEquipment.transform;

            while (root.parent != null)
            {
                root =
                    root.parent;
            }

            return root.name.StartsWith(
                "Player",
                StringComparison.Ordinal);
        }
    }
}