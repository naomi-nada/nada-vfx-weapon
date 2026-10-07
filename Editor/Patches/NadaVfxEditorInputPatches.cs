using HarmonyLib;

namespace NADA.VFX.Weapon.Editor.Patches
{
    /// <summary>
    /// Gives the editor temporary ownership of gameplay input while it is open.
    ///
    /// The player and normal character camera must not react to mouse/keyboard
    /// input while the user is interacting with editor UI. The preview-camera
    /// controller owns camera presentation during that interval.
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
}