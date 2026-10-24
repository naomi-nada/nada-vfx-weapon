using System;
using NADA.VFX.Weapon.Core.Persistence;
using NADA.VFX.Weapon.Core.State;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Blocks.Effects;
using NADA.VFX.Weapon.Core.State.Defaults;
using NADA.VFX.Weapon.Runtime.Structure;
using NADA.VFX.Weapon.Weapons.Targets;
using UnityEngine;

namespace NADA.VFX.Weapon.Weapons.Runtime
{
    // Temporary 94L test caller. Remove this before the public API ships.
    // It only works on an item that's unbound when the test starts.
    internal static class NadaWeaponStateTransitionProbe
    {
        private const uint TestInstanceId = 1;
        private const uint TestNextInstanceId = 2;

        private static global::ItemDrop.ItemData _testItem;

        internal static void Register()
        {
            try
            {
                new global::Terminal.ConsoleCommand(
                    "nada94l",
                    "Temporary NADA state transition test: apply, update, reject, remove, status",
                    Run,
                    isCheat: false,
                    isNetwork: false,
                    onlyServer: false,
                    isSecret: false,
                    allowInDevBuild: false,
                    hideBehindDevCommands: false,
                    optionsFetcher: null,
                    alwaysRefreshTabOptions: false,
                    remoteCommand: false,
                    onlyAdmin: false);
                Plugin.Log?.LogInfo($"{Plugin.ModName}: [94L] Console test registered.");
            }
            catch (Exception error)
            {
                // A test command shouldn't stop the mod from loading.
                Plugin.Log?.LogWarning(
                    $"{Plugin.ModName}: [94L] Couldn't register console test: {error}");
            }
        }

        private static void Run(global::Terminal.ConsoleEventArgs args)
        {
            if (args.Length < 2)
            {
                Report(args, "Use: nada94l apply | update | reject | remove | status");
                return;
            }

            try
            {
                switch (args[1].ToLowerInvariant())
                {
                    case "apply":
                        Apply(args);
                        break;
                    case "update":
                        Update(args);
                        break;
                    case "reject":
                        Reject(args);
                        break;
                    case "remove":
                        Remove(args);
                        break;
                    case "status":
                        Status(args);
                        break;
                    default:
                        Report(args, "Unknown operation. Use apply, update, reject, remove, or status.");
                        break;
                }
            }
            catch (Exception error)
            {
                Plugin.Log?.LogError($"{Plugin.ModName}: [94L FAIL] {error}");
                Report(args, $"FAIL: {error.GetType().Name}. Check LogOutput.log.");
            }
        }

        private static void Apply(global::Terminal.ConsoleEventArgs args)
        {
            if (_testItem != null)
            {
                Report(args, "Test already active. Finish with 'nada94l remove' first.");
                return;
            }

            if (NadaWeaponEditorPreviewState.IsActive)
            {
                Report(args, "Close the NADA editor first.");
                return;
            }

            global::ItemDrop.ItemData item =
                NadaEquippedItemResolver.ResolveRightHandItem();

            if (item == null ||
                object.ReferenceEquals(item, NadaEquippedItemResolver.ResolveLeftHandItem()))
            {
                Report(args, "Equip an unbound one-handed weapon in your right hand first.");
                return;
            }

            NadaWeaponLocalSourceSelection source =
                NadaWeaponLocalSourceResolver.Resolve(item);

            if (source.Kind != NadaWeaponLocalSourceKind.Unbound)
            {
                Report(args, $"Refusing to overwrite an existing item state ({source.Kind}). Use an unbound test weapon.");
                return;
            }

            VfxEffectBlock appearance = WeaponVfxBlockDefaults.Create(
                VfxEffectTypeIds.WeaponAppearance, TestInstanceId);
            if (appearance?.Settings is not WeaponAppearanceVfxSettings settings)
            {
                Report(args, "FAIL: Could not build the Appearance test block.");
                return;
            }

            appearance.DisplayName = "94L Target Appearance";
            settings.Visibility = 0.5f;

            var state = new WeaponVfxState();
            state.Effects.Add(appearance);

            if (!NadaWeaponNativeStateActions.TryApplyState(
                    item, state, TestNextInstanceId, out string reason))
            {
                Report(args, $"FAIL: Apply was rejected: {reason}");
                return;
            }

            _testItem = item;
            bool bound = WeaponVfxItemStore.Read(
                item, out WeaponVfxState stored, out uint cursor, out _) ==
                WeaponVfxStateReadStatus.Valid;
            Transform root = FindAppearanceRoot(item);

            Report(args,
                $"APPLY: bound={bound}, blocks={stored?.Effects?.Count ?? 0}, " +
                $"cursor={cursor}, instanceRoot={Id(root)}, visibility=0.50. " +
                "Weapon should fade immediately. If instanceRoot=missing, inspect the deferred visual.");
        }

        private static void Update(global::Terminal.ConsoleEventArgs args)
        {
            if (!TryGetTestItem(args, out global::ItemDrop.ItemData item))
                return;

            if (WeaponVfxItemStore.Read(item, out WeaponVfxState state,
                    out uint cursor, out string reason) != WeaponVfxStateReadStatus.Valid ||
                state.Effects?.Count != 1 ||
                state.Effects[0].InstanceId != TestInstanceId ||
                state.Effects[0].Settings is not WeaponAppearanceVfxSettings settings)
            {
                Report(args, $"FAIL: Test item no longer has the expected block. {reason}");
                return;
            }

            Transform before = FindAppearanceRoot(item);
            int beforeId = Id(before);

            // Keep the same InstanceId. If reconciliation rebuilds it,
            // the before/after IDs will tell us.
            settings.Visibility = settings.Visibility > 0.3f ? 0.2f : 0.5f;
            float value = settings.Visibility;

            if (!NadaWeaponNativeStateActions.TryApplyState(
                    item, state, cursor, out reason))
            {
                Report(args, $"FAIL: Update was rejected: {reason}");
                return;
            }

            int afterId = Id(FindAppearanceRoot(item));
            bool isEquipped = object.ReferenceEquals(
                item, NadaEquippedItemResolver.ResolveRightHandItem());

            Report(args,
                $"UPDATE: visibility={value:0.00}, equipped={isEquipped}, " +
                $"instanceBefore={beforeId}, instanceAfter={afterId}, " +
                $"reused={(beforeId != 0 && beforeId == afterId)}. " +
                "If unequipped, only persisted state should change.");
        }

        private static void Reject(global::Terminal.ConsoleEventArgs args)
        {
            if (!TryGetTestItem(args, out global::ItemDrop.ItemData item))
                return;

            if (NadaWeaponEditorPreviewState.IsActive)
            {
                Report(args, "Close the NADA editor first.");
                return;
            }

            if (WeaponVfxItemStore.Read(item, out WeaponVfxState state,
                    out uint cursor, out string reason) != WeaponVfxStateReadStatus.Valid ||
                state.Effects?.Count != 1 ||
                state.Effects[0].Settings is not WeaponAppearanceVfxSettings settings)
            {
                Report(args, $"FAIL: Test state unavailable. {reason}");
                return;
            }

            // Codec rejection must leave ItemData untouched.
            string before = item.m_customData[WeaponVfxItemStore.PayloadKey];
            settings.Visibility = float.NaN;
            bool invalidAccepted = NadaWeaponNativeStateActions.TryApplyState(
                item, state, cursor, out string invalidReason);
            bool unchangedAfterInvalid =
                item.m_customData[WeaponVfxItemStore.PayloadKey] == before;

            // Simulate an active preview, then check the editor ownership guard.
            // The guard runs synchronously, so no preview rig is created here.
            settings.Visibility = 0.5f;
            bool previewAccepted;
            string previewReason;
            NadaWeaponEditorPreviewState.Activate(item, state);
            try
            {
                previewAccepted = NadaWeaponNativeStateActions.TryRemove(
                    item, out previewReason);
            }
            finally
            {
                NadaWeaponEditorPreviewState.Deactivate();
            }

            bool unchangedAfterPreview =
                item.m_customData.TryGetValue(WeaponVfxItemStore.PayloadKey,
                    out string after) && after == before;

            Report(args,
                $"REJECT: invalidRejected={!invalidAccepted}, " +
                $"invalidAtomic={unchangedAfterInvalid}, " +
                $"previewRejected={!previewAccepted}, " +
                $"previewAtomic={unchangedAfterPreview}. " +
                $"invalidReason='{invalidReason}', previewReason='{previewReason}'");
        }

        private static void Remove(global::Terminal.ConsoleEventArgs args)
        {
            if (!TryGetTestItem(args, out global::ItemDrop.ItemData item))
                return;

            if (!NadaWeaponNativeStateActions.TryRemove(item, out string reason))
            {
                Report(args, $"FAIL: Remove was rejected: {reason}");
                return;
            }

            NadaWeaponLocalSourceSelection source =
                NadaWeaponLocalSourceResolver.Resolve(item);
            bool nativeMissing = WeaponVfxItemStore.Read(
                item, out _, out _, out _) == WeaponVfxStateReadStatus.Missing;
            bool legacyMissing = !VfxStateIO.IsBound(item);
            Transform remaining = FindAppearanceRoot(item);
            bool removeAgain = NadaWeaponNativeStateActions.TryRemove(item, out string againReason);

            Report(args,
                $"REMOVE: source={source.Kind}, nativeMissing={nativeMissing}, " +
                $"legacyMissing={legacyMissing}, instanceRoot={Id(remaining)}, " +
                $"idempotent={removeAgain}. " +
                (removeAgain ? "Normal unbound behavior should return." :
                    $"Second remove failed: {againReason}"));

            if (source.Kind == NadaWeaponLocalSourceKind.Unbound &&
                nativeMissing && legacyMissing && removeAgain)
            {
                _testItem = null;
            }
        }

        private static void Status(global::Terminal.ConsoleEventArgs args)
        {
            global::ItemDrop.ItemData item = _testItem ??
                NadaEquippedItemResolver.ResolveRightHandItem();
            if (item == null)
            {
                Report(args, "No test item or right-hand weapon.");
                return;
            }

            NadaWeaponLocalSourceSelection source =
                NadaWeaponLocalSourceResolver.Resolve(item);
            bool equipped = object.ReferenceEquals(
                item, NadaEquippedItemResolver.ResolveRightHandItem());
            Report(args,
                $"STATUS: item='{item.m_dropPrefab?.name ?? "<unknown>"}', " +
                $"tracked={object.ReferenceEquals(item, _testItem)}, " +
                $"equipped={equipped}, source={source.Kind}, " +
                $"blocks={source.State?.Effects?.Count ?? 0}, " +
                $"instanceRoot={Id(FindAppearanceRoot(item))}.");
        }

        private static bool TryGetTestItem(
            global::Terminal.ConsoleEventArgs args,
            out global::ItemDrop.ItemData item)
        {
            item = _testItem;
            if (item == null)
            {
                Report(args, "No test active. Start on an unbound weapon with 'nada94l apply'.");
                return false;
            }

            if (NadaWeaponEditorPreviewState.IsActive)
            {
                Report(args, "Close the NADA editor before continuing this test.");
                return false;
            }

            return true;
        }

        private static Transform FindAppearanceRoot(global::ItemDrop.ItemData item)
        {
            if (!object.ReferenceEquals(item,
                    NadaEquippedItemResolver.ResolveRightHandItem()))
                return null;

            // Read only the current right-hand instance; no scene searches.
            Player player = Player.m_localPlayer;
            global::VisEquipment owner =
                player != null ? player.GetComponentInChildren<global::VisEquipment>(true) : null;
            var field = HarmonyLib.AccessTools.Field(
                typeof(global::VisEquipment), "m_rightItemInstance");
            GameObject instance = owner != null
                ? field?.GetValue(owner) as GameObject
                : null;

            Transform visual = instance != null
                ? NadaWeaponTargets.FindEquippedWeaponVisualRoot(instance.transform)
                : null;
            Transform rig = NadaRigPaths.FindDirectChild(
                visual, Plugin.LocalWeaponRootName);
            Transform instances = NadaRigPaths.FindLocalEffectInstancesRoot(rig);
            return NadaRigPaths.FindDirectChild(
                instances, $"Effect_{TestInstanceId:D8}_{VfxEffectTypeIds.WeaponAppearance}");
        }

        private static int Id(Transform transform) =>
            transform != null ? transform.GetInstanceID() : 0;

        private static void Report(global::Terminal.ConsoleEventArgs args, string message)
        {
            string line = $"[94L] {message}";
            args.Context.AddString(line);
            Plugin.Log?.LogInfo($"{Plugin.ModName}: {line}");
        }
    }
}
