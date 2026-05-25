using System;
using HarmonyLib;
using NADA.VFX.Weapon.Core.Config;
using NADA.VFX.Weapon.Core.Debug;
using NADA.VFX.Weapon.Core.State;
using NADA.VFX.Weapon.Weapons.Runtime;
using NADA.VFX.Weapon.Weapons.Targets;
using UnityEngine;

namespace NADA.VFX.Weapon.Weapons.Patches
{
    [HarmonyPatch(typeof(global::VisEquipment), "UpdateEquipmentVisuals")]
    internal static class VisEquipment
    {
        private static readonly System.Reflection.FieldInfo RightInstField =
            AccessTools.Field(typeof(global::VisEquipment), "m_rightItemInstance");

        private static readonly System.Reflection.FieldInfo LeftInstField =
            AccessTools.Field(typeof(global::VisEquipment), "m_leftItemInstance");

        private static readonly NadaWeaponRigController WeaponRigController = new();

        private static void Postfix(global::VisEquipment __instance)
        {
            try
            {
                if (__instance == null)
                    return;
                
                if (!IsAllowedRigOwner(__instance))
                    return;
                
                if (Player.m_localPlayer == null &&
                    PluginConfig.CharacterSelectionVisibility.Value)
                {
                    NadaLogControl.Info(
                        $"char-select-probe:{__instance.GetInstanceID()}",
                        $"{Plugin.ModName}: [CharSelectProbe] " +
                        $"vis='{__instance.name}' " +
                        $"path='{NadaWeaponTargets.FullPath(__instance.transform)}'");
                }

                GameObject rightInstance = SafeGetGameObject(RightInstField, __instance);
                GameObject leftInstance = SafeGetGameObject(LeftInstField, __instance);

                global::ItemDrop.ItemData rightItem =
                    NadaEquippedItemResolver.ResolveRightHandItem();

                global::ItemDrop.ItemData leftItem =
                    NadaEquippedItemResolver.ResolveLeftHandItem();

                TryApplyIfBound(rightInstance, rightItem);
                TryApplyIfBound(leftInstance, leftItem);
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning($"{Plugin.ModName}: VisEquipment.UpdateEquipmentVisuals postfix error: {e}");
            }
        }

        private static void TryApplyIfBound(
            GameObject itemInstance,
            global::ItemDrop.ItemData itemData)
        {
            if (itemInstance == null || itemData == null)
                return;

            if (!VfxStateIO.IsBound(itemData))
                return;

            WeaponRigController.TryApply(itemInstance, itemData);
        }

        private static GameObject SafeGetGameObject(
            System.Reflection.FieldInfo field,
            global::VisEquipment ve)
        {
            try
            {
                if (field == null || ve == null)
                    return null;

                return field.GetValue(ve) as GameObject;
            }
            catch
            {
                return null;
            }
        }
        
        private static bool IsAllowedRigOwner(global::VisEquipment visEquipment)
        {
            if (visEquipment == null)
                return false;

            if (Player.m_localPlayer != null)
                return visEquipment.transform.IsChildOf(Player.m_localPlayer.transform);

            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "start")
                return false;

            Transform root = visEquipment.transform;

            while (root.parent != null)
                root = root.parent;

            return root.name.StartsWith("Player", System.StringComparison.Ordinal);
        }
    }
}