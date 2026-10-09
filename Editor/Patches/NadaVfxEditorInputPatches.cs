using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

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
    /// The gameplay camera owns mouse capture, independently of UpdateCamera.
    /// Do not let it re-lock/hide the pointer while the NADA editor owns input.
    /// Outside the editor the vanilla method runs unchanged.
    /// </summary>
    [HarmonyPatch(typeof(global::GameCamera), "UpdateMouseCapture")]
    internal static class NadaVfxEditorMouseCapturePatch
    {
        // The availability guard keeps older/different game builds from
        // failing PatchAll if the vanilla method was renamed or removed.
        [HarmonyPrepare]
        private static bool Prepare()
        {
            bool available = AccessTools.Method(
                typeof(global::GameCamera), "UpdateMouseCapture") != null;

            if (!available)
                Plugin.Log?.LogWarning(
                    $"{Plugin.ModName}: [EditorMouseCapturePatch] " +
                    "GameCamera.UpdateMouseCapture unavailable; cursor fallback only.");

            return available;
        }

        [HarmonyPrefix]
        private static bool Prefix()
        {
            if (!NadaVfxEditor.IsOpen)
                return true;

            NadaVfxEditor.EnforceCursorOwnership();
            return false;
        }
    }

    /// <summary>
    /// Optional movement preview. TakeInput stays blocked while the editor is
    /// open; this applies ONLY movement through vanilla player controls after
    /// the normal controller tick, leaving every gameplay action flag false.
    /// </summary>
    [HarmonyPatch(
        typeof(global::PlayerController),
        "FixedUpdate")]
    internal static class NadaVfxEditorMovementPatch
    {
        private static bool _movementWasApplied;
        private static bool _loggedThisSession;

        [HarmonyPostfix]
        private static void Postfix(
            global::PlayerController __instance)
        {
            if (__instance == null ||
                (!NadaVfxEditor.IsOpen && !_movementWasApplied))
            {
                return;
            }

            global::Player player =
                __instance.GetComponent<global::Player>();

            if (player == null ||
                player != global::Player.m_localPlayer)
            {
                return;
            }

            if (!NadaVfxEditor.IsOpen)
            {
                // Only a leftover movement command needs releasing here.
                if (_movementWasApplied)
                    ApplyMovement(player, Vector3.zero);

                _movementWasApplied = false;
                _loggedThisSession = false;
                return;
            }

            bool canMove =
                NadaVfxEditor.AllowsMovementWhileEditing &&
                !player.IsDead() &&
                !global::InventoryGui.IsVisible() &&
                !global::Minimap.IsOpen() &&
                !global::Menu.IsVisible() &&
                (global::Chat.instance == null ||
                 !global::Chat.instance.HasFocus());

            Vector3 movement =
                canMove
                    ? ReadMovementDirection()
                    : Vector3.zero;

            // The original TakeInput block remains active. Supply only movement
            // using Player.SetControls, never a gameplay action flag. Explicitly
            // send zero once when input is released to avoid a stale move dir.
            bool moving =
                movement.sqrMagnitude > 0.0001f;

            if (!moving && !_movementWasApplied)
                return;

            ApplyMovement(player, movement);
            _movementWasApplied = moving;

            if (moving && !_loggedThisSession)
            {
                _loggedThisSession = true;
                Plugin.Log?.LogInfo(
                    $"{Plugin.ModName}: [EditorMovement] movement preview active.");
            }
        }

        private static void ApplyMovement(
            global::Player player,
            Vector3 movement)
        {
            player.SetControls(
                movement,
                false, // attack
                false, // attack hold
                false, // secondary attack
                false, // secondary attack hold
                false, // block
                false, // block hold
                false, // jump
                false, // crouch
                false, // run
                false, // auto run
                false); // dodge
        }

        private static Vector3 ReadMovementDirection()
        {
            float x = 0f;
            float z = 0f;

            if (global::ZInput.GetButton("Left"))
                x -= 1f;
            if (global::ZInput.GetButton("Right"))
                x += 1f;
            if (global::ZInput.GetButton("Forward"))
                z += 1f;
            if (global::ZInput.GetButton("Backward"))
                z -= 1f;

            if (x == 0f && z == 0f)
                return Vector3.zero;

            Camera camera =
                Camera.main;

            if (camera == null)
                return Vector3.zero;

            Vector3 forward =
                camera.transform.forward;

            Vector3 right =
                camera.transform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            return Vector3.ClampMagnitude(
                right * x + forward * z,
                1f);
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
    /// Some third-party radial menus read Unity's legacy Input API directly,
    /// bypassing ZInput. While a NADA text field is focused, hide those raw
    /// key reads from other gameplay systems too. IMGUI text entry uses Event.
    /// Only managed getter wrappers can be patched; leave native externs alone.
    /// </summary>
    [HarmonyPatch]
    internal static class NadaVfxEditorRawKeyInputPatch
    {
        [HarmonyTargetMethods]
        private static IEnumerable<MethodBase> TargetMethods()
        {
            string[] names = { "GetKey", "GetKeyDown", "GetKeyUp" };
            System.Type[] argumentTypes = { typeof(KeyCode), typeof(string) };
            foreach (string name in names)
            foreach (System.Type argumentType in argumentTypes)
            {
                MethodInfo method = AccessTools.Method(
                    typeof(UnityEngine.Input), name,
                    new[] { argumentType });
                if (method == null)
                    continue;
                try
                {
                    if (method.GetMethodBody() == null)
                        continue;
                }
                catch (System.InvalidOperationException)
                {
                    continue;
                }
                catch (System.NotSupportedException)
                {
                    continue;
                }
                yield return method;
            }
        }

        [HarmonyPrefix]
        private static bool Prefix(ref bool __result)
        {
            if (!NadaVfxEditor.OwnsKeyboardInput)
                return true;
            __result = false;
            return false;
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

    /// <summary>
    /// Prevents a key typed into NADA from sheathing or drawing the local
    /// player's weapon, including callers that bypass the ZInput getters.
    /// </summary>
    [HarmonyPatch]
    internal static class NadaVfxEditorHandItemInputPatch
    {
        [HarmonyPatch(
            typeof(global::Humanoid),
            "HideHandItems",
            new System.Type[] { typeof(bool), typeof(bool) })]
        [HarmonyPrefix]
        private static bool HideHandItemsPrefix(
            global::Humanoid __instance)
        {
            return AllowHandItemAction(__instance);
        }

        [HarmonyPatch(
            typeof(global::Humanoid),
            "ShowHandItems",
            new System.Type[] { typeof(bool), typeof(bool) })]
        [HarmonyPrefix]
        private static bool ShowHandItemsPrefix(
            global::Humanoid __instance)
        {
            return AllowHandItemAction(__instance);
        }

        private static bool AllowHandItemAction(
            global::Humanoid humanoid)
        {
            return !NadaVfxEditor.OwnsKeyboardInput ||
                   humanoid == null ||
                   humanoid != global::Player.m_localPlayer;
        }
    }
}
