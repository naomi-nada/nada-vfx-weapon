using NADA.VFX.Weapon.Core.State.Blocks;

namespace NADA.VFX.Weapon.Weapons.Runtime
{
    /// <summary>
    /// Temporary local presentation override used by the in-game editor.
    ///
    /// This does not persist state, write ItemData, touch config, or own
    /// runtime objects. It only tells the local orchestrator which resolved
    /// WeaponVfxState should drive presentation while editing.
    /// </summary>
    internal static class NadaWeaponEditorPreviewState
    {
        private static global::ItemDrop.ItemData _itemData;
        private static WeaponVfxState _state;

        internal static bool IsActive =>
            _itemData != null &&
            _state != null;

        internal static void Activate(
            global::ItemDrop.ItemData itemData,
            WeaponVfxState state)
        {
            if (itemData == null ||
                state == null)
            {
                Deactivate();
                return;
            }

            bool changed =
                !object.ReferenceEquals(
                    _itemData,
                    itemData) ||
                !object.ReferenceEquals(
                    _state,
                    state);

            _itemData =
                itemData;

            _state =
                state;

            if (!changed)
                return;

            Plugin.Log?.LogInfo(
                $"{Plugin.ModName}: [EditorPreview] " +
                $"activated item='{itemData.m_shared?.m_name ?? "<unknown>"}' " +
                $"effects={state.Effects?.Count ?? 0}.");
        }

        internal static void Deactivate()
        {
            if (_itemData == null &&
                _state == null)
            {
                return;
            }

            string itemName =
                _itemData?.m_shared?.m_name ??
                "<unknown>";

            _itemData =
                null;

            _state =
                null;

            Plugin.Log?.LogInfo(
                $"{Plugin.ModName}: [EditorPreview] " +
                $"deactivated item='{itemName}'.");
        }

        internal static bool TryGet(
            global::ItemDrop.ItemData itemData,
            out WeaponVfxState state)
        {
            state =
                null;

            if (itemData == null ||
                _itemData == null ||
                _state == null)
            {
                return false;
            }

            if (!object.ReferenceEquals(
                    itemData,
                    _itemData))
            {
                return false;
            }

            state =
                _state;

            return true;
        }
    }
}