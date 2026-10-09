using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
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
        private static readonly FieldInfo RightPreviewItemField =
            AccessTools.Field(typeof(Humanoid), "m_rightItem");

        private static readonly FieldInfo LeftPreviewItemField =
            AccessTools.Field(typeof(Humanoid), "m_leftItem");

        // VisEquipment.m_rightItem / m_leftItem identify the actual rendered
        // prefab. On Valheim 1.0.17 these are decimal prefab-hash strings,
        // unlike m_current*ItemHash (which is not a prefab identity).
        private static readonly FieldInfo RightVisualItemField =
            AccessTools.Field(typeof(global::VisEquipment), "m_rightItem");

        private static readonly FieldInfo LeftVisualItemField =
            AccessTools.Field(typeof(global::VisEquipment), "m_leftItem");

        private static float _nextCharacterSelectionProbeTime;
        private static bool _characterSelectionPreviewClearedWhileDisabled;

        internal static void TickCharacterSelectionPreview()
        {
            if (Player.m_localPlayer != null ||
                SceneManager.GetActiveScene().name != "start")
            {
                _characterSelectionPreviewClearedWhileDisabled = false;
                return;
            }

            if (PluginConfig.CharacterSelectionVisibility?.Value != true)
            {
                if (_characterSelectionPreviewClearedWhileDisabled)
                    return;

                RemoveCharacterSelectionPreview();
                _characterSelectionPreviewClearedWhileDisabled = true;
                return;
            }

            _characterSelectionPreviewClearedWhileDisabled = false;

            if (Time.time < _nextCharacterSelectionProbeTime)
                return;

            _nextCharacterSelectionProbeTime = Time.time + 1f;

            GameObject[] roots =
                SceneManager.GetActiveScene().GetRootGameObjects();

            var controller = new NadaWeaponRigController();

            foreach (GameObject root in roots)
            {
                if (root == null ||
                    !root.name.StartsWith("Player", StringComparison.Ordinal))
                {
                    continue;
                }

                bool appliedAny = false;

                appliedAny |= TryApplyCharacterSelectionHandPreview(
                    root.transform,
                    "RightHand_Attach",
                    RightPreviewItemField,
                    RightVisualItemField,
                    controller);

                appliedAny |= TryApplyCharacterSelectionHandPreview(
                    root.transform,
                    "LeftHand_Attach",
                    LeftPreviewItemField,
                    LeftVisualItemField,
                    controller);

                if (appliedAny)
                    return;
            }
        }

        internal static void RefreshDroppedItemVisibility()
        {
            // Only called for explicit visibility changes, not every frame.
            var drops = UnityEngine.Object.FindObjectsByType<global::ItemDrop>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            foreach (global::ItemDrop itemDrop in drops)
            {
                if (itemDrop == null || itemDrop.gameObject == null)
                    continue;

                Transform attachTarget =
                    NadaWeaponTargets.FindDroppedWeaponVisualRoot(
                        itemDrop.transform);

                if (attachTarget == null)
                    continue;

                Transform existingRig = NadaRigPaths.FindDirectChild(
                    attachTarget,
                    Plugin.LocalWeaponRootName);

                NadaWeaponLocalSourceSelection selection =
                    NadaWeaponLocalSourceResolver.Resolve(itemDrop.m_itemData);

                bool isBound =
                    selection.Kind == NadaWeaponLocalSourceKind.NativeBound ||
                    selection.Kind == NadaWeaponLocalSourceKind.LegacyBound;

                if (PluginConfig.DroppedItemVisibility?.Value != true || !isBound)
                {
                    if (existingRig != null)
                        RemoveVisibleRig(existingRig);
                    continue;
                }

                if (existingRig != null)
                    continue;

                var controller = new NadaWeaponRigController();
                controller.TryApplyDroppedItem(
                    itemDrop.gameObject, itemDrop.m_itemData);
            }
        }

        private static bool TryApplyCharacterSelectionHandPreview(
            Transform previewRoot,
            string attachName,
            FieldInfo itemField,
            FieldInfo visualItemField,
            NadaWeaponRigController controller)
        {
            if (previewRoot == null || controller == null)
                return false;

            Transform handAttach = FindDescendantByName(previewRoot, attachName);
            if (handAttach == null)
                return false;

            string visibleItemIdentity = ReadPreviewVisibleItemIdentity(
                previewRoot, visualItemField);

            global::ItemDrop.ItemData previewItem = ResolvePreviewItemData(
                previewRoot, itemField, visibleItemIdentity, attachName);

            if (previewItem == null)
            {
                RemoveCharacterSelectionHandPreview(previewRoot, attachName);
                return false;
            }

            NadaWeaponLocalSourceSelection selection =
                NadaWeaponLocalSourceResolver.Resolve(previewItem);

            if (selection.Kind != NadaWeaponLocalSourceKind.NativeBound &&
                selection.Kind != NadaWeaponLocalSourceKind.LegacyBound)
            {
                RemoveCharacterSelectionHandPreview(previewRoot, attachName);
                return false;
            }

            bool appliedAny = false;

            foreach (Transform childTransform in handAttach)
            {
                if (childTransform == null)
                    continue;

                Transform visualRoot =
                    NadaWeaponTargets.FindEquippedWeaponVisualRoot(childTransform);

                if (visualRoot == null)
                    continue;

                bool applied = controller.TryApply(
                    childTransform.gameObject, previewItem);

                if (applied)
                    appliedAny = true;
            }

            string logKey =
                $"{previewRoot.GetInstanceID()}:{attachName}:{visibleItemIdentity}";

            if (appliedAny)
            {
                NadaLogControl.Info(
                    $"char-preview-applied:{logKey}",
                    $"{Plugin.ModName}: [CharSelectApply OK] " +
                    $"hand='{attachName}' " +
                    $"item='{previewItem.m_dropPrefab?.name}' " +
                    $"identity='{visibleItemIdentity}' source={selection.Kind}.");
            }
            else
            {
                NadaLogControl.Info(
                    $"char-preview-apply-failed:{logKey}",
                    $"{Plugin.ModName}: [CharSelectApply FAIL] " +
                    $"hand='{attachName}' " +
                    $"item='{previewItem.m_dropPrefab?.name}' " +
                    $"identity='{visibleItemIdentity}' source={selection.Kind} " +
                    "reason='no-applicable-visual-or-controller-rejected'.");
            }

            return appliedAny;
        }

        private static global::ItemDrop.ItemData ResolvePreviewItemData(
            Transform previewRoot,
            FieldInfo itemField,
            string visibleItemIdentity,
            string handName)
        {
            // Character select owns a separate Player and ItemData instances.
            // Never borrow gameplay ItemData or use an unverified inventory item.
            Player previewPlayer = previewRoot.GetComponent<Player>();
            if (previewPlayer == null ||
                string.IsNullOrWhiteSpace(visibleItemIdentity))
            {
                LogUnresolved(previewRoot, handName, visibleItemIdentity,
                    "missing-preview-player-or-visible-item-identity");
                return null;
            }

            // Vanilla Humanoid already knows which particular item is in this
            // hand. Verify that the visible equipment matches its prefab before
            // trusting the reference (including its persisted NADA state).
            global::ItemDrop.ItemData directItem =
                itemField?.GetValue(previewPlayer) as global::ItemDrop.ItemData;

            if (directItem != null &&
                directItem.m_equipped &&
                MatchesVisibleItemIdentity(directItem, visibleItemIdentity))
            {
                return directItem;
            }

            // Transitional fallback when the Humanoid hand field is not yet
            // populated: only accept a unique EQUIPPED matching inventory item.
            // Multiple copies of a prefab must never inherit each other's VFX.
            Inventory inventory = previewPlayer.GetInventory();
            List<global::ItemDrop.ItemData> items = inventory?.GetAllItems();
            if (items == null)
            {
                LogUnresolved(previewRoot, handName, visibleItemIdentity,
                    "preview-inventory-missing");
                return null;
            }

            int equippedMatches = 0;
            global::ItemDrop.ItemData uniqueEquippedMatch = null;

            foreach (global::ItemDrop.ItemData item in items)
            {
                if (item == null || !item.m_equipped ||
                    !MatchesVisibleItemIdentity(item, visibleItemIdentity))
                {
                    continue;
                }

                equippedMatches++;
                uniqueEquippedMatch = item;
            }

            if (equippedMatches == 1)
                return uniqueEquippedMatch;

            LogUnresolved(previewRoot, handName, visibleItemIdentity,
                $"equipped-identity-matches={equippedMatches} " +
                $"direct='{directItem?.m_dropPrefab?.name ?? "<null>"}'");
            return null;
        }

        private static bool MatchesVisibleItemIdentity(
            global::ItemDrop.ItemData item,
            string visibleIdentity)
        {
            GameObject prefab = item?.m_dropPrefab;
            if (prefab == null || string.IsNullOrWhiteSpace(visibleIdentity))
                return false;

            string prefabName = prefab.name;
            if (string.Equals(prefabName, visibleIdentity, StringComparison.Ordinal))
                return true;

            // Vanilla's preview VisEquipment stores the decimal stable hash
            // as a string on this Valheim build. This is the *prefab* hash,
            // not m_currentRightItemHash / m_currentLeftItemHash.
            int prefabHash = StringExtensionMethods.GetStableHashCode(prefabName);
            return string.Equals(
                prefabHash.ToString(System.Globalization.CultureInfo.InvariantCulture),
                visibleIdentity,
                StringComparison.Ordinal);
        }

        private static string ReadPreviewVisibleItemIdentity(
            Transform previewRoot,
            FieldInfo visualItemField)
        {
            if (previewRoot == null || visualItemField == null)
                return null;

            global::VisEquipment equipment =
                previewRoot.GetComponentInChildren<global::VisEquipment>(true);
            if (equipment == null)
                return null;

            try
            {
                return visualItemField.GetValue(equipment)?.ToString();
            }
            catch (Exception exception)
            {
                NadaLogControl.Info(
                    $"char-preview-visual-field-error:{previewRoot.GetInstanceID()}:{visualItemField.Name}",
                    $"{Plugin.ModName}: [CharSelectPreview] " +
                    $"visual identity lookup failed: {exception.GetType().Name}.");
                return null;
            }
        }

        private static void LogUnresolved(
            Transform previewRoot,
            string handName,
            string visibleIdentity,
            string reason)
        {
            NadaLogControl.Info(
                $"char-preview-item:{previewRoot.GetInstanceID()}:{handName}:{visibleIdentity}:{reason}",
                $"{Plugin.ModName}: [CharSelectPreview] " +
                $"hand='{handName}' identity='{visibleIdentity}' reason='{reason}'.");
        }

        private static void RemoveCharacterSelectionPreview()
        {
            if (Player.m_localPlayer != null ||
                SceneManager.GetActiveScene().name != "start")
            {
                return;
            }

            foreach (GameObject root in
                     SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (root == null ||
                    !root.name.StartsWith("Player", StringComparison.Ordinal))
                {
                    continue;
                }

                RemoveCharacterSelectionHandPreview(
                    root.transform, "RightHand_Attach");
                RemoveCharacterSelectionHandPreview(
                    root.transform, "LeftHand_Attach");
            }
        }

        private static void RemoveCharacterSelectionHandPreview(
            Transform previewRoot,
            string attachName)
        {
            Transform handAttach = FindDescendantByName(
                previewRoot, attachName);

            if (handAttach == null)
                return;

            foreach (Transform child in handAttach)
            {
                if (child == null)
                    continue;

                Transform weaponVisualRoot =
                    NadaWeaponTargets.FindEquippedWeaponVisualRoot(child);

                if (weaponVisualRoot == null)
                    continue;

                Transform rig = NadaRigPaths.FindDirectChild(
                    weaponVisualRoot,
                    Plugin.LocalWeaponRootName);

                if (rig != null)
                    RemoveVisibleRig(rig);
            }
        }

        private static void RemoveVisibleRig(Transform rig)
        {
            if (rig == null)
                return;

            // Unity destruction is deferred. Hide and detach the known NADA
            // hierarchy first so subsequent discovery cannot reuse stale VFX.
            rig.gameObject.SetActive(false);
            rig.SetParent(null, false);
            UnityEngine.Object.Destroy(rig.gameObject);
        }

        private static Transform FindDescendantByName(
            Transform root,
            string targetName)
        {
            if (root == null)
                return null;

            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child != null && child.name == targetName)
                    return child;
            }

            return null;
        }
    }
}
