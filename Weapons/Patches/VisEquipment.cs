using System;
using HarmonyLib;
using NADA.VFX.Weapons.Runtime;
using UnityEngine;

namespace NADA.VFX.Weapons.Patches
{
    internal static class VisEquipment
    {
        private static readonly AccessTools.FieldRef<global::VisEquipment, GameObject> RightInst =
            AccessTools.FieldRefAccess<global::VisEquipment, GameObject>("m_rightItemInstance");

        private static readonly AccessTools.FieldRef<global::VisEquipment, GameObject> LeftInst =
            AccessTools.FieldRefAccess<global::VisEquipment, GameObject>("m_leftItemInstance");

        private static readonly System.Reflection.FieldInfo RightItemField =
            AccessTools.Field(typeof(global::VisEquipment), "m_rightItem");

        private static readonly System.Reflection.FieldInfo LeftItemField =
            AccessTools.Field(typeof(global::VisEquipment), "m_leftItem");

        private static readonly NadaWeaponRigController WeaponRigController = new();

        private static void Postfix(global::VisEquipment __instance)
        {
            try
            {
                if (__instance == null) return;

                var rightGo = RightInst(__instance);
                var leftGo = LeftInst(__instance);

                var rightItem = SafeGetItem(RightItemField, __instance);
                var leftItem = SafeGetItem(LeftItemField, __instance);

                if (rightGo != null)
                    WeaponRigController.TryApply(rightGo, rightItem);

                if (leftGo != null)
                    WeaponRigController.TryApply(leftGo, leftItem);
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning($"{Plugin.ModName}: VisEquipment.UpdateEquipmentVisuals postfix error: {e}");
            }
        }

        private static global::ItemDrop.ItemData SafeGetItem(System.Reflection.FieldInfo field, global::VisEquipment ve)
        {
            try
            {
                if (field == null || ve == null) return null;
                return field.GetValue(ve) as global::ItemDrop.ItemData;
            }
            catch
            {
                return null;
            }
        }
    }
}