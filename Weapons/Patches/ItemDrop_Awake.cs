
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
            catch (Exception error)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: ItemDrop.Awake postfix error: {error}");
            }
        }

        private static IEnumerator CheckDroppedItemWhenReady(
            global::ItemDrop itemDrop)
        {
            if (itemDrop == null || itemDrop.gameObject == null)
                yield break;

            // Snapshot before yielding so changes during initialization
            // are still detected by the watcher.
            ZNetView initialView = itemDrop.GetComponent<ZNetView>();
            ZDO observedZdo = null;

            global::ItemDrop.ItemData observedItem = itemDrop.m_itemData;

            bool observedOwner = initialView != null &&
                                 initialView.IsValid() &&
                                 initialView.IsOwner();

            if (initialView != null && initialView.IsValid())
                observedZdo = initialView.GetZDO();

            ReadSignature(
                observedZdo,
                out int observedLength,
                out ulong observedFingerprint);

            yield return null;
            yield return null;

            if (itemDrop == null || itemDrop.gameObject == null)
                yield break;

            if (PluginConfig.DroppedItemVisibility?.Value == true)
                ApplyInitialDroppedItem(itemDrop);

            // Remote ZDO updates don't necessarily refresh the live
            // ItemData. Observe the packed record without writing to it
            // or claiming ownership.
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

                ReadSignature(
                    zdo,
                    out int currentLength,
                    out ulong currentFingerprint);

                global::ItemDrop.ItemData currentItem =
                    itemDrop.m_itemData;

                bool isOwner = view.IsOwner();

                bool ownershipChanged =
                    isOwner != observedOwner;

                bool packedChanged =
                    currentLength != observedLength ||
                    currentFingerprint != observedFingerprint;

                bool zdoChanged =
                    !ReferenceEquals(zdo, observedZdo);

                bool itemChanged =
                    !ReferenceEquals(currentItem, observedItem);

                if (!ownershipChanged &&
                    !packedChanged &&
                    !zdoChanged &&
                    !itemChanged)
                {
                    continue;
                }

                observedZdo = zdo;
                observedItem = currentItem;
                observedOwner = isOwner;
                observedLength = currentLength;
                observedFingerprint = currentFingerprint;

                // Preserve an existing owner's local ItemData. It may
                // contain changes that haven't been saved to the ZDO yet.
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
                    // Keep the authoritative refresh on ownership changes.
                    // The cached ItemData alone cannot establish whether
                    // the packed record contains a newer binding.
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

                    NadaWeaponLocalSourceSelection selection =
                        NadaWeaponLocalSourceResolver.Resolve(
                            observedItem);

                    if (selection.Kind ==
                        NadaWeaponLocalSourceKind.InvalidNative)
                    {
                        // A malformed native state must never leave the
                        // previous visual attached to this world drop.
                        RetireDroppedRig(itemDrop);

                        Plugin.Log.LogWarning(
                            $"{Plugin.ModName}: [DropLiveReloadRejected] " +
                            $"object={itemDrop.GetInstanceID()} " +
                            $"reason='invalid-native-state: " +
                            $"{selection.FailureReason}'.");

                        continue;
                    }

                    if (selection.Kind !=
                            NadaWeaponLocalSourceKind.NativeBound &&
                        selection.Kind !=
                            NadaWeaponLocalSourceKind.LegacyBound)
                    {
                        // Ordinary drops still refresh their ItemData,
                        // but don't need the NADA runtime or success logs.
                        // This also removes an old rig after unbinding.
                        RetireDroppedRig(itemDrop);
                        continue;
                    }

                    var controller = new NadaWeaponRigController();

                    bool applied = controller.TryApplyDroppedItem(
                        itemDrop.gameObject,
                        observedItem);

                    if (!applied)
                    {
                        RetireDroppedRig(itemDrop);

                        Plugin.Log.LogWarning(
                            $"{Plugin.ModName}: [DropLiveReloadRejected] " +
                            $"object={itemDrop.GetInstanceID()} " +
                            $"reason='bound-rig-apply-failed' " +
                            $"source={selection.Kind}.");

                        continue;
                    }

                    Plugin.Log.LogInfo(
                        $"{Plugin.ModName}: [DropLiveReload] " +
                        $"object={itemDrop.GetInstanceID()} " +
                        $"bytes={currentLength} " +
                        $"rigApplied=True " +
                        $"source={selection.Kind} " +
                        $"packedChanged={packedChanged} " +
                        $"ownershipChanged={ownershipChanged} " +
                        $"itemRefChanged={itemChanged} " +
                        $"zdoRefChanged={zdoChanged} " +
                        $"owner={isOwner}.");
                }
                catch (Exception error)
                {
                    // Failed reloads must not preserve stale VFX.
                    RetireDroppedRig(itemDrop);

                    Plugin.Log.LogWarning(
                        $"{Plugin.ModName}: [DropLiveReloadRejected] " +
                        $"object={itemDrop.GetInstanceID()} " +
                        $"reason='{error.GetBaseException().Message}'.");
                }
            }
        }

        private static void ReadSignature(
            ZDO zdo,
            out int length,
            out ulong fingerprint)
        {
            byte[] bytes = zdo?.GetByteArray(
                ZDOVars.s_itemData, null);

            length = bytes?.Length ?? 0;

            // Hash the packed record without keeping a second copy
            // of every dropped item's serialized data.
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

        private static void RetireDroppedRig(
            global::ItemDrop itemDrop)
        {
            if (itemDrop == null)
                return;

            Transform visual =
                NadaWeaponTargets.FindDroppedWeaponVisualRoot(
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

            if (selection.Kind ==
                NadaWeaponLocalSourceKind.InvalidNative)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [DropStateInvalid] " +
                    $"object='{itemDrop.gameObject.name}' " +
                    $"reason='{selection.FailureReason}'.");

                return;
            }

            if (selection.Kind !=
                    NadaWeaponLocalSourceKind.NativeBound &&
                selection.Kind !=
                    NadaWeaponLocalSourceKind.LegacyBound)
            {
                return;
            }

            var controller = new NadaWeaponRigController();

            bool applied = controller.TryApplyDroppedItem(
                itemDrop.gameObject,
                itemData);

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
