using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
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
    /// Blocks gameplay action getters while the editor is selected or typing.
    /// Clicking outside the window releases these actions without closing it.
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
                NadaVfxEditor.BlocksGameplayHotkeys;
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
    /// Inventory has its own input update. Block it only while the editor
    /// is selected, not merely because the editor window remains open.
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
                !NadaVfxEditor.BlocksGameplayHotkeys;
        }
    }

    /// <summary>
    /// The map has an independent UI input path; protect editor selection.
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
                !NadaVfxEditor.BlocksGameplayHotkeys;
        }
    }

    /// <summary>
    /// Prevent local sheath/draw actions while NADA owns editor input.
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
            return !NadaVfxEditor.BlocksGameplayHotkeys ||
                   humanoid == null ||
                   humanoid != global::Player.m_localPlayer ||
                   global::Player.m_localPlayer.IsDead();
        }
    }

    /// <summary>
    /// Selected editor input must not reach the player's gameplay action
    /// consumers. These three methods own the tested keyboard paths in
    /// Valheim 1.0.17: GP/AutoPickup, OpenRadial/OpenEmote, and Sit.
    ///
    /// Player.Update invokes the other two methods, but a transpiler on
    /// Update does not rewrite the called methods' own ZInput calls.
    /// Rewriting just these consumers avoids intercepting other mods or
    /// globally changing ZInput for unrelated interfaces.
    /// </summary>
    [HarmonyPatch]
    internal static class NadaVfxEditorGameplayButtonInputPatch
    {
        private static readonly string[] TargetMethodNames =
        {
            "Update",
            "CheckKeyboardRadialPressed",
            "HandleRadialInput"
        };

        private static readonly HashSet<string> ReportedInstallations =
            new HashSet<string>(StringComparer.Ordinal);

        private static readonly HashSet<string> ReportedBlockedQueries =
            new HashSet<string>(StringComparer.Ordinal);

        [HarmonyTargetMethods]
        private static IEnumerable<MethodBase> TargetMethods()
        {
            foreach (string name in TargetMethodNames)
            {
                MethodInfo method = AccessTools.Method(typeof(global::Player), name);
                if (method == null)
                {
                    Plugin.Log?.LogWarning(
                        $"{Plugin.ModName}: [EditorGameActionGuardUnavailable] " +
                        $"method='Player.{name}' not found.");
                    continue;
                }

                yield return method;
            }
        }

        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            MethodInfo getButtonDown = AccessTools.Method(
                typeof(global::ZInput), "GetButtonDown",
                new[] { typeof(string) });

            MethodInfo guardedButtonDown = AccessTools.Method(
                typeof(NadaVfxEditorGameplayButtonInputPatch),
                nameof(GetButtonDownEditorAware));

            int replacements = 0;
            foreach (CodeInstruction instruction in instructions)
            {
                if (getButtonDown != null && guardedButtonDown != null &&
                    instruction.Calls(getButtonDown))
                {
                    // Preserve Harmony labels and exception blocks.
                    CodeInstruction guarded = new CodeInstruction(instruction);
                    guarded.opcode = OpCodes.Call;
                    guarded.operand = guardedButtonDown;
                    replacements++;
                    yield return guarded;
                }
                else
                {
                    yield return instruction;
                }
            }

            string methodName = original?.Name ?? "<unknown>";
            if (replacements == 0)
            {
                if (ReportedInstallations.Add(methodName + ":missing"))
                    Plugin.Log?.LogWarning(
                        $"{Plugin.ModName}: [EditorGameActionGuardUnavailable] " +
                        $"method='Player.{methodName}' " +
                        "contains no ZInput.GetButtonDown(string) calls.");
            }
            else if (ReportedInstallations.Add(methodName))
            {
                Plugin.Log?.LogInfo(
                    $"{Plugin.ModName}: [EditorGameActionGuardInstalled] " +
                    $"method='Player.{methodName}' buttonDownCalls={replacements}.");
            }
        }

        private static bool GetButtonDownEditorAware(string action)
        {
            if (!NadaVfxEditor.BlocksGameplayHotkeys)
            {
                ReportedBlockedQueries.Clear();
                return global::ZInput.GetButtonDown(action);
            }

            // This indicates a guarded action *query*, not a keypress.
            // Log once per action while selected; never log every frame.
            if (IsTestedGameplayAction(action) &&
                ReportedBlockedQueries.Add(action))
            {
                Plugin.Log?.LogInfo(
                    $"{Plugin.ModName}: [EditorGameActionQueryBlocked] " +
                    $"action='{action}'.");
            }

            return false;
        }

        private static bool IsTestedGameplayAction(string action)
        {
            return string.Equals(action, "GP", StringComparison.Ordinal) ||
                   string.Equals(action, "AutoPickup", StringComparison.Ordinal) ||
                   string.Equals(action, "Sit", StringComparison.Ordinal) ||
                   string.Equals(action, "OpenRadial", StringComparison.Ordinal) ||
                   string.Equals(action, "OpenEmote", StringComparison.Ordinal);
        }
    }
}

// Player hotbar use has a separate path from PlayerController.TakeInput.
// Guard only that player-initiated path; do not block Humanoid.EquipItem,
// which other game systems can use for legitimate equipment/lifetime changes.
namespace NADA.VFX.Weapon.Editor.Patches
{
    [HarmonyPatch]
    internal static class NadaVfxEditorHotbarUsePatch
    {
        private static bool _reportedBlockedUse;

        [HarmonyTargetMethods]
        private static IEnumerable<MethodBase> TargetMethods()
        {
            bool found = false;
            foreach (MethodInfo method in
                     AccessTools.GetDeclaredMethods(typeof(global::Player)))
            {
                if (method.Name != "UseHotbarItem")
                    continue;

                found = true;
                yield return method;
            }

            if (!found)
                Plugin.Log?.LogWarning(
                    $"{Plugin.ModName}: [EditorHotbarGuard] " +
                    "Player.UseHotbarItem unavailable; hotbar fallback unverified.");
        }

        [HarmonyPrefix]
        private static bool Prefix(global::Player __instance)
        {
            if (__instance != global::Player.m_localPlayer ||
                __instance.IsDead())
                return true;

            if (!NadaVfxEditor.BlocksGameplayHotkeys)
            {
                _reportedBlockedUse = false;
                return true;
            }

            if (!_reportedBlockedUse)
            {
                _reportedBlockedUse = true;
                Plugin.Log?.LogInfo(
                    $"{Plugin.ModName}: [EditorHotbarUseBlocked] local hotbar input suppressed.");
            }

            return false;
        }
    }
}
