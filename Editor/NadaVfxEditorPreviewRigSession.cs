using NADA.VFX.Weapon.Core.State;
using NADA.VFX.Weapon.Runtime.Structure;
using NADA.VFX.Weapon.Weapons.Runtime;
using UnityEngine;

namespace NADA.VFX.Weapon.Editor
{
    /// <summary>
    /// Owns temporary runtime rigs created solely so an unbound equipped
    /// weapon can display the editor's in-memory preview state.
    ///
    /// Existing runtime rigs are borrowed, never owned here.
    /// </summary>
    internal static class NadaVfxEditorPreviewRigSession
    {
        private static global::ItemDrop.ItemData _ownedItemData;
        private static Transform _ownedRigRoot;

        internal static bool Apply(
            NadaVfxEditorTarget target)
        {
            if (target?.ItemData == null ||
                target.SourceRoot == null ||
                target.VisualRoot == null)
            {
                ReleaseOwnedRig();

                return false;
            }

            if (_ownedItemData != null &&
                !object.ReferenceEquals(
                    _ownedItemData,
                    target.ItemData))
            {
                ReleaseOwnedRig();
            }

            Transform existingRig =
                NadaRigPaths.FindDirectChild(
                    target.VisualRoot,
                    Plugin.LocalWeaponRootName);

            bool rigExistedBeforeApply =
                existingRig != null;

            var controller =
                new NadaWeaponRigController();

            bool applied =
                controller.TryApply(
                    target.SourceRoot,
                    target.ItemData);

            if (!applied)
                return false;

            Transform refreshedRig =
                NadaRigPaths.FindDirectChild(
                    target.VisualRoot,
                    Plugin.LocalWeaponRootName);

            if (refreshedRig == null)
                return false;

            if (!rigExistedBeforeApply &&
                !VfxStateIO.IsBound(
                    target.ItemData))
            {
                _ownedItemData =
                    target.ItemData;

                _ownedRigRoot =
                    refreshedRig;

                Plugin.Log?.LogInfo(
                    $"{Plugin.ModName}: [EditorPreviewRig] " +
                    $"created temporary rig " +
                    $"item='{target.ItemNameKey}' " +
                    $"rig={refreshedRig.GetInstanceID()}.");
            }

            return true;
        }

        internal static void ReleaseOwnedRig()
        {
            if (_ownedItemData == null &&
                _ownedRigRoot == null)
            {
                return;
            }

            global::ItemDrop.ItemData itemData =
                _ownedItemData;

            Transform rigRoot =
                _ownedRigRoot;

            _ownedItemData =
                null;

            _ownedRigRoot =
                null;

            if (rigRoot == null)
                return;

            if (itemData != null &&
                VfxStateIO.IsBound(
                    itemData))
            {
                Plugin.Log?.LogInfo(
                    $"{Plugin.ModName}: [EditorPreviewRig] " +
                    $"temporary rig became persistent because item is now bound; " +
                    $"rig={rigRoot.GetInstanceID()}.");

                return;
            }

            // Destroy is deferred by Unity. Disable immediately so another
            // runtime/editor pass cannot rediscover this as an active rig.
            try
            {
                rigRoot.gameObject.SetActive(
                    false);
            }
            catch
            {
            }

            NadaWeaponRigRemoval.RemoveTrackedRig(
                rigRoot);

            Plugin.Log?.LogInfo(
                $"{Plugin.ModName}: [EditorPreviewRig] " +
                $"removed temporary rig.");
        }
    }
}