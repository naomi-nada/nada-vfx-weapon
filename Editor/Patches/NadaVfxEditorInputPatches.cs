using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace NADA.VFX.Weapon.Editor.Patches
{
    /// <summary>
    /// Gives the editor temporary ownership of gameplay input while it is open.
    ///
    /// Player movement/combat and the normal character camera stay suppressed
    /// for the whole editor session. Global UI hotkeys are suppressed only
    /// while an actual NADA text field owns keyboard focus.
    /// </summary>
    [HarmonyPatch]
    internal static class NadaVfxEditorInputPatches
    {
        [HarmonyPatch(
            typeof(global::PlayerController),
            "TakeInput")]
        [HarmonyPrefix]
        private static bool PlayerControllerTakeInputPrefix(
            ref bool __result)
        {
            if (!NadaVfxEditor.IsOpen)
                return true;

            __result =
                false;

            return false;
        }

        [HarmonyPatch(
            typeof(global::GameCamera),
            "UpdateCamera")]
        [HarmonyPrefix]
        private static bool GameCameraUpdateCameraPrefix()
        {
            if (!NadaVfxEditor.IsOpen)
            {
                NadaVfxEditorPreviewCamera
                    .Deactivate();

                return true;
            }

            NadaVfxEditor.EnforceCursorOwnership();

            NadaVfxEditorPreviewCamera
                .Tick();

            return false;
        }
    }

    /// <summary>
    /// Blocks Valheim action getters only while an editor text field owns the
    /// keyboard.
    ///
    /// Keeping this focus-scoped is important: while the editor is open but
    /// not typing, vanilla UI controls can still be used for things such as
    /// opening inventory or changing equipment.
    /// </summary>
    [HarmonyPatch]
    internal static class NadaVfxEditorZInputPatches
    {
        [HarmonyTargetMethods]
        private static IEnumerable<MethodBase> TargetMethods()
        {
            foreach (MethodInfo method in
                     AccessTools.GetDeclaredMethods(
                         typeof(global::ZInput)))
            {
                if (!method.IsStatic ||
                    method.ReturnType !=
                    typeof(bool))
                {
                    continue;
                }

                if (!IsBlockedInputGetter(
                        method.Name))
                {
                    continue;
                }

                yield return method;
            }
        }

        [HarmonyPrefix]
        private static bool Prefix(
            ref bool __result)
        {
            if (!ShouldBlockEditorKeyboard())
                return true;

            __result =
                false;

            return false;
        }

        private static bool IsBlockedInputGetter(
            string methodName)
        {
            return
                methodName ==
                "GetButton" ||

                methodName ==
                "GetButtonDown" ||

                methodName ==
                "GetButtonUp" ||

                methodName ==
                "GetKey" ||

                methodName ==
                "GetKeyDown" ||

                methodName ==
                "GetKeyUp";
        }

        private static bool ShouldBlockEditorKeyboard()
        {
            return
                NadaVfxEditor.IsOpen &&
                NadaVfxEditor.OwnsKeyboardInput;
        }
    }

    /// <summary>
    /// Inventory has its own UI update path, so freeze that path while a NADA
    /// text field owns the keyboard. Outside text entry, inventory remains
    /// available while the editor is open.
    /// </summary>
    [HarmonyPatch(
        typeof(global::InventoryGui),
        "Update")]
    internal static class NadaVfxEditorInventoryInputPatch
    {
        [HarmonyPrefix]
        private static bool Prefix()
        {
            return
                !NadaVfxEditor.IsOpen ||
                !NadaVfxEditor.OwnsKeyboardInput;
        }
    }

    /// <summary>
    /// The map also processes input independently of PlayerController.
    /// Prevent map toggles/actions from stealing characters typed into NADA
    /// text fields.
    /// </summary>
    [HarmonyPatch(
        typeof(global::Minimap),
        "Update")]
    internal static class NadaVfxEditorMinimapInputPatch
    {
        [HarmonyPrefix]
        private static bool Prefix()
        {
            return
                !NadaVfxEditor.IsOpen ||
                !NadaVfxEditor.OwnsKeyboardInput;
        }
    }
}