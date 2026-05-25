using System.Reflection;
using NADA.VFX.Weapon.Core.Config;
using NADA.VFX.Weapon.Core.Debug;
using NADA.VFX.Weapon.Core.State;
using NADA.VFX.Weapon.Runtime.Structure;
using NADA.VFX.Weapon.Weapons.Targets;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NADA.VFX.Weapon.Weapons.Runtime
{
    internal static class NadaRigVisibility
    {
        private static float _nextCharacterSelectionProbeTime;

        internal static void TickCharacterSelectionPreview()
        {
            if (!PluginConfig.CharacterSelectionVisibility.Value)
            {
                RemoveCharacterSelectionPreview();
                return;
            }

            if (Player.m_localPlayer != null)
                return;

            if (SceneManager.GetActiveScene().name != "start")
                return;

            if (Time.time < _nextCharacterSelectionProbeTime)
                return;

            _nextCharacterSelectionProbeTime = Time.time + 1f;

            GameObject[] roots = SceneManager
                .GetActiveScene()
                .GetRootGameObjects();

            var controller = new NadaWeaponRigController();

            foreach (GameObject root in roots)
            {
                if (root == null || !root.name.StartsWith("Player", System.StringComparison.Ordinal))
                    continue;

                bool appliedAny = false;

                appliedAny |= TryApplyCharacterSelectionHandPreview(
                    root.transform,
                    "RightHand_Attach",
                    "m_rightItem",
                    controller);

                appliedAny |= TryApplyCharacterSelectionHandPreview(
                    root.transform,
                    "LeftHand_Attach",
                    "m_leftItem",
                    controller);

                if (appliedAny)
                    return;
            }
        }

        internal static void RefreshDroppedItemVisibility()
        {
            var drops = Object.FindObjectsByType<global::ItemDrop>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            foreach (var itemDrop in drops)
            {
                if (itemDrop == null || itemDrop.gameObject == null)
                    continue;

                if (!VfxStateIO.IsBound(itemDrop.m_itemData))
                    continue;

                Transform attachTarget =
                    NadaWeaponTargets.FindVisualMeshRoot(itemDrop.transform)
                    ?? itemDrop.transform;

                Transform existingRig =
                    NadaRigPaths.FindDirectChild(
                        attachTarget,
                        Plugin.LocalWeaponRootName);

                if (!PluginConfig.DroppedItemVisibility.Value)
                {
                    if (existingRig != null)
                        Object.Destroy(existingRig.gameObject);

                    continue;
                }

                if (existingRig != null)
                    continue;

                var controller = new NadaWeaponRigController();
                controller.TryApplyDroppedItem(itemDrop.gameObject, itemDrop.m_itemData);
            }
        }

        private static bool TryApplyCharacterSelectionHandPreview(
            Transform previewRoot,
            string attachName,
            string itemFieldName,
            NadaWeaponRigController controller)
        {
            if (previewRoot == null || controller == null)
                return false;

            Transform handAttach =
                FindDescendantByName(previewRoot, attachName);

            if (handAttach == null)
                return false;

            global::ItemDrop.ItemData previewItem =
                ResolvePreviewItemData(previewRoot, itemFieldName);

            if (previewItem == null)
            {
                NadaLogControl.Info(
                    $"char-preview-null-item:{previewRoot.GetInstanceID()}:{attachName}",
                    $"{Plugin.ModName}: [CharSelectPreview] {attachName} has visual attach but preview item is null.");

                return false;
            }

            bool isBound = VfxStateIO.IsBound(previewItem);

            if (!isBound)
                return false;

            bool appliedAny = false;

            foreach (Transform childTransform in handAttach)
            {
                if (childTransform == null)
                    continue;

                Transform weaponVisualRootTransform =
                    NadaWeaponTargets.FindEquippedWeaponVisualRoot(childTransform);

                if (weaponVisualRootTransform == null)
                    continue;

                Transform attachTarget =
                    ResolveRigAttachTarget(weaponVisualRootTransform);

                if (attachTarget == null)
                    continue;

                Transform existingRig =
                    NadaRigPaths.FindDirectChild(
                        attachTarget,
                        Plugin.LocalWeaponRootName);

                if (existingRig != null)
                {
                    controller.TryApply(childTransform.gameObject, previewItem);
                    appliedAny = true;
                    continue;
                }

                bool applied =
                    controller.TryApply(childTransform.gameObject, previewItem);

                if (!applied)
                    continue;

                appliedAny = true;
            }

            return appliedAny;
        }

        private static void RemoveCharacterSelectionPreview()
        {
            if (Player.m_localPlayer != null)
                return;

            if (SceneManager.GetActiveScene().name != "start")
                return;

            GameObject[] roots = SceneManager
                .GetActiveScene()
                .GetRootGameObjects();

            foreach (GameObject root in roots)
            {
                if (root == null || !root.name.StartsWith("Player", System.StringComparison.Ordinal))
                    continue;

                RemoveCharacterSelectionHandPreview(root.transform, "RightHand_Attach");
                RemoveCharacterSelectionHandPreview(root.transform, "LeftHand_Attach");
            }
        }

        private static void RemoveCharacterSelectionHandPreview(
            Transform previewRoot,
            string attachName)
        {
            Transform handAttach =
                FindDescendantByName(previewRoot, attachName);

            if (handAttach == null)
                return;

            foreach (Transform childTransform in handAttach)
            {
                if (childTransform == null)
                    continue;

                Transform weaponVisualRootTransform =
                    NadaWeaponTargets.FindEquippedWeaponVisualRoot(childTransform);

                if (weaponVisualRootTransform == null)
                    continue;

                Transform attachTarget =
                    ResolveRigAttachTarget(weaponVisualRootTransform);

                if (attachTarget == null)
                    continue;

                Transform existingRig =
                    NadaRigPaths.FindDirectChild(
                        attachTarget,
                        Plugin.LocalWeaponRootName);

                if (existingRig == null)
                    continue;

                Object.Destroy(existingRig.gameObject);
            }
        }

        private static Transform ResolveRigAttachTarget(Transform weaponVisualRootTransform)
        {
            return weaponVisualRootTransform;
        }

        private static Transform FindDescendantByName(
            Transform rootTransform,
            string targetName)
        {
            if (rootTransform == null)
                return null;

            foreach (Transform childTransform in rootTransform.GetComponentsInChildren<Transform>(true))
            {
                if (childTransform != null && childTransform.name == targetName)
                    return childTransform;
            }
            return null;
        }

        private static global::ItemDrop.ItemData ResolvePreviewItemData(
            Transform previewRoot,
            string fieldName)
        {
            if (previewRoot == null || string.IsNullOrWhiteSpace(fieldName))
                return null;

            Humanoid humanoid = previewRoot.GetComponent<Humanoid>();
            if (humanoid == null)
                return null;

            FieldInfo itemField =
                typeof(Humanoid).GetField(
                    fieldName,
                    BindingFlags.Instance |
                    BindingFlags.NonPublic |
                    BindingFlags.Public);

            return itemField?.GetValue(humanoid) as global::ItemDrop.ItemData;
        }
    }
}