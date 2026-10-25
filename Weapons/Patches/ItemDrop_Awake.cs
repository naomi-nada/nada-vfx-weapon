
using System;
using System.Collections;
using HarmonyLib;
using NADA.VFX.Weapon.Core.Config;
using NADA.VFX.Weapon.Runtime.Structure;
using NADA.VFX.Weapon.Weapons.Runtime;
using NADA.VFX.Weapon.Weapons.Targets;
using UnityEngine;

namespace NADA.VFX.Weapon.Weapons.Patches
{
    [HarmonyPatch(typeof(global::ItemDrop), "Awake")]
    internal static class ItemDrop_Awake
    {
        private static readonly WaitForSeconds ReloadInterval =
            new WaitForSeconds(1f);

        private static void Postfix(global::ItemDrop __instance)
        {
            try
            {
                if (__instance == null ||
                    PluginConfig.DroppedItemVisibility?.Value != true ||
                    Plugin.Instance == null)
                {
                    return;
                }

                Plugin.Instance.StartCoroutine(
                    CheckDroppedItemWhenReady(__instance));
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: ItemDrop.Awake postfix error: {e}");
            }
        }

        private static IEnumerator CheckDroppedItemWhenReady(
            global::ItemDrop itemDrop)
        {
            // Record the initial ZDO bytes before yielding. A remote update
            // received during initial setup still needs a live reload.
            ZDO observedZdo = null;
            global::ItemDrop.ItemData observedItem = itemDrop.m_itemData;
            ZNetView initialView = itemDrop.GetComponent<ZNetView>();
            bool observedOwner = initialView != null &&
                                 initialView.IsValid() && initialView.IsOwner();
            if (initialView != null && initialView.IsValid())
                observedZdo = initialView.GetZDO();

            ReadSignature(observedZdo,
                out int observedLength, out ulong observedFingerprint);

            yield return null;
            yield return null;

            if (itemDrop == null || itemDrop.gameObject == null)
                yield break;

            if (PluginConfig.DroppedItemVisibility?.Value == true)
                ApplyInitialDroppedItem(itemDrop);

            // ItemDrop.Load() runs when a drop is created, not when an
            // already-live remote ZDO receives a new packed item record.
            // Only read a changed record, never write or claim ownership.
            while (itemDrop != null && itemDrop.gameObject != null)
            {
                yield return ReloadInterval;

                if (itemDrop == null || itemDrop.gameObject == null)
                    yield break;

                if (PluginConfig.DroppedItemVisibility?.Value != true)
                    continue;

                ZNetView view = itemDrop.GetComponent<ZNetView>();
                if (view == null || !view.IsValid())
                    continue;

                ZDO zdo = view.GetZDO();
                if (zdo == null)
                    continue;

                ReadSignature(zdo,
                    out int currentLength, out ulong currentFingerprint);

                global::ItemDrop.ItemData currentItem = itemDrop.m_itemData;
                bool isOwner = view.IsOwner();
                bool ownershipChanged = isOwner != observedOwner;
                if (!ownershipChanged &&
                    ReferenceEquals(zdo, observedZdo) &&
                    ReferenceEquals(currentItem, observedItem) &&
                    currentLength == observedLength &&
                    currentFingerprint == observedFingerprint)
                {
                    continue;
                }

                observedZdo = zdo;
                observedItem = currentItem;
                observedOwner = isOwner;
                observedLength = currentLength;
                observedFingerprint = currentFingerprint;

                // Never overwrite an owner's pending local change. When
                // ownership just transferred, first reload the authoritative
                // item record instead of inheriting another peer's cache.
                if (isOwner && !ownershipChanged)
                    continue;

                if (currentItem == null || currentLength == 0)
                {
                    RetireDroppedRig(itemDrop);
                    Plugin.Log.LogWarning(
                        $"{Plugin.ModName}: [DropLiveReloadRejected] " +
                        $"object={itemDrop.GetInstanceID()} " +
                        "reason='missing-item-or-packed-data'.");
                    continue;
                }

                try
                {
                    // Vanilla owns the ItemData format. Reload its actual
                    // live instance, then reuse NADA's block reconciler.
                    itemDrop.Load();
                    observedItem = itemDrop.m_itemData;
                    if (observedItem == null)
                    {
                        RetireDroppedRig(itemDrop);
                        Plugin.Log.LogWarning(
                            $"{Plugin.ModName}: [DropLiveReloadRejected] " +
                            $"object={itemDrop.GetInstanceID()} " +
                            "reason='loaded-item-is-null'.");
                        continue;
                    }

                    var controller = new NadaWeaponRigController();
                    bool applied = controller.TryApplyDroppedItem(
                        itemDrop.gameObject, itemDrop.m_itemData);

                    Plugin.Log.LogInfo(
                        $"{Plugin.ModName}: [DropLiveReload] " +
                        $"object={itemDrop.GetInstanceID()} " +
                        $"bytes={currentLength} rigApplied={applied}.");
                }
                catch (Exception error)
                {
                    // A malformed replicated record must not leave an old
                    // transparent/material-modified rig on the dropped item.
                    RetireDroppedRig(itemDrop);
                    Plugin.Log.LogWarning(
                        $"{Plugin.ModName}: [DropLiveReloadRejected] " +
                        $"object={itemDrop.GetInstanceID()} " +
                        $"reason='{error.GetBaseException().Message}'.");
                }
            }
        }

        private static void ReadSignature(
            ZDO zdo, out int length, out ulong fingerprint)
        {
            byte[] bytes = zdo?.GetByteArray(ZDOVars.s_itemData, null);
            length = bytes?.Length ?? 0;

            // Keep a compact signature rather than cloning the entire
            // packed item record for every live drop.
            const ulong offset = 14695981039346656037UL;
            const ulong prime = 1099511628211UL;
            ulong hash = offset;
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                    hash = unchecked((hash ^ bytes[i]) * prime);
            }

            fingerprint = hash;
        }

        private static void RetireDroppedRig(global::ItemDrop itemDrop)
        {
            if (itemDrop == null)
                return;

            Transform visual = NadaWeaponTargets.FindDroppedWeaponVisualRoot(
                itemDrop.transform);
            Transform rig = visual != null
                ? NadaRigPaths.FindDirectChild(
                    visual, Plugin.LocalWeaponRootName)
                : null;

            if (rig != null)
                NadaWeaponRigRemoval.RemoveTrackedRig(rig);
        }

        private static void ApplyInitialDroppedItem(
            global::ItemDrop itemDrop)
        {
            global::ItemDrop.ItemData itemData = itemDrop.m_itemData;
            if (itemData == null)
                return;

            NadaWeaponLocalSourceSelection selection =
                NadaWeaponLocalSourceResolver.Resolve(itemData);

            if (selection.Kind == NadaWeaponLocalSourceKind.InvalidNative)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [DropStateInvalid] " +
                    $"object='{itemDrop.gameObject.name}' " +
                    $"reason='{selection.FailureReason}'.");
                return;
            }

            if (selection.Kind != NadaWeaponLocalSourceKind.NativeBound &&
                selection.Kind != NadaWeaponLocalSourceKind.LegacyBound)
                return;

            var controller = new NadaWeaponRigController();
            bool applied = controller.TryApplyDroppedItem(
                itemDrop.gameObject, itemData);

            if (!applied)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [DropApply FAIL] " +
                    $"object='{itemDrop.gameObject.name}' " +
                    $"item='{itemData.m_shared?.m_name}' " +
                    $"source={selection.Kind} " +
                    $"path='{NadaWeaponTargets.FullPath(itemDrop.transform)}'");
                return;
            }

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [DropApply OK] " +
                $"object='{itemDrop.gameObject.name}' " +
                $"item='{itemData.m_shared?.m_name}' " +
                $"source={selection.Kind} " +
                $"path='{NadaWeaponTargets.FullPath(itemDrop.transform)}'");
        }
    }
}
