using NADA.VFX.Weapon.Core.Debug;
using NADA.VFX.Weapon.Core.State;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Runtime.Structure;
using NADA.VFX.Weapon.Weapons.Targets;
using UnityEngine;

namespace NADA.VFX.Weapon.Weapons.Runtime
{
    internal sealed class NadaWeaponRigController
    {
        // Read-only observation point for tools such as the editor.
        // Runtime still owns resolving and applying the actual weapon target.
        internal static NadaWeaponRigContext LastAppliedLocalContext
        {
            get;
            private set;
        }

        public bool TryApply(
            GameObject root,
            global::ItemDrop.ItemData itemData = null)
        {
            if (root == null)
                return false;

            if (!NadaWeaponTargets.IsTargetOrAttachClone(
                    root))
            {
                return false;
            }

            Transform weaponVisualRootTransform =
                NadaWeaponTargets.FindEquippedWeaponVisualRoot(
                    root.transform);

            if (weaponVisualRootTransform == null)
                return false;

            Transform attachTarget =
                weaponVisualRootTransform;

            itemData ??=
                ResolveItemData(
                    root);

            // Transitional context data. The orchestrator independently
            // selects the native block state for native-bound weapons.
            VfxState state =
                NadaWeaponStateResolver.Resolve(
                    itemData);

            return TryApplyInternal(
                root,
                itemData,
                state,
                attachTarget);
        }

        public bool TryApplyResolvedState(
            GameObject root,
            int itemHash,
            VfxState state)
        {
            if (root == null)
                return false;

            if (!NadaWeaponTargets.IsTargetOrAttachClone(
                    root))
            {
                return false;
            }

            Transform weaponVisualRootTransform =
                NadaWeaponTargets.FindEquippedWeaponVisualRoot(
                    root.transform);

            if (weaponVisualRootTransform == null)
                return false;

            NadaLogControl.Info(
                $"remote-rig-target:{root.GetInstanceID()}",
                $"{Plugin.ModName}: [RemoteRigTarget] " +
                $"root='{root.name}' " +
                $"visualRoot='{weaponVisualRootTransform.name}' " +
                $"itemHash={itemHash} " +
                $"path='{NadaWeaponTargets.FullPath(weaponVisualRootTransform)}'");

            NadaWeaponMetadata metadata =
                NadaWeaponMetadataResolver.FromItemHash(
                    itemHash);

            var context =
                new NadaWeaponRigContext(
                    root,
                    itemData: null,
                    state,
                    weaponVisualRootTransform);

            if (!context.IsValid)
                return false;

            return NadaWeaponRigOrchestrator.RunRemote(
                context,
                metadata);
        }

        // Remote native state is already resolved by the network codec.
        // Never manufacture an ItemData or project blocks into legacy state.
        public bool TryApplyResolvedBlockState(
            GameObject root,
            int itemHash,
            WeaponVfxState blocks)
        {
            if (root == null || itemHash == 0 || blocks == null ||
                !NadaWeaponTargets.IsTargetOrAttachClone(root))
                return false;

            Transform visual = NadaWeaponTargets.FindEquippedWeaponVisualRoot(
                root.transform);
            if (visual == null)
                return false;

            NadaWeaponMetadata metadata =
                NadaWeaponMetadataResolver.FromItemHash(itemHash);

            // Explicitly empty transitional context: native rendering must
            // not consume local configuration or any legacy VfxState fields.
            var context = new NadaWeaponRigContext(
                root, itemData: null, state: default(VfxState), visual);
            if (!context.IsValid)
                return false;

            return NadaWeaponRigOrchestrator.RunRemoteNative(
                context, metadata, blocks);
        }

        public bool TryApplyDroppedItem(
            GameObject root,
            global::ItemDrop.ItemData itemData)
        {
            if (root == null ||
                itemData == null)
            {
                return false;
            }

            NadaWeaponLocalSourceSelection source =
                NadaWeaponLocalSourceResolver.Resolve(
                    itemData);

            // Only persisted bindings belong on dropped weapons.
            // An invalid native payload cannot fall through to legacy.
            if (source.Kind != NadaWeaponLocalSourceKind.NativeBound &&
                source.Kind != NadaWeaponLocalSourceKind.LegacyBound)
            {
                if (source.Kind == NadaWeaponLocalSourceKind.InvalidNative)
                {
                    Transform invalidVisual =
                        NadaWeaponTargets.FindDroppedWeaponVisualRoot(
                            root.transform);

                    if (invalidVisual != null)
                    {
                        Transform staleRig =
                            NadaRigPaths.FindDirectChild(
                                invalidVisual,
                                Plugin.LocalWeaponRootName);

                        if (staleRig != null)
                        {
                            staleRig.gameObject.SetActive(false);
                            NadaWeaponRigRemoval.RemoveTrackedRig(staleRig);
                        }
                    }

                    NadaLogControl.Info(
                        $"drop-native-invalid:{root.GetInstanceID()}:{source.FailureReason}",
                        $"{Plugin.ModName}: [DropNativeStateRejected] " +
                        $"root='{root.name}' " +
                        $"reason='{source.FailureReason}'");
                }

                return false;
            }

            // Dropped items have their own wrapper/root, so attach the rig
            // to the actual weapon visual instead. Using the ItemDrop root
            // here can put the rig in a different local coordinate space.
            Transform attachTarget =
                NadaWeaponTargets.FindDroppedWeaponVisualRoot(
                    root.transform);

            if (attachTarget == null)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [DropApply] could not resolve visual root " +
                    $"for '{root.name}'.");

                return false;
            }

            NadaLogControl.Info(
                $"drop-target:{root.GetInstanceID()}",
                $"{Plugin.ModName}: [DropTarget] " +
                $"root='{root.name}' " +
                $"visualRoot='{attachTarget.name}' " +
                $"path='{NadaWeaponTargets.FullPath(attachTarget)}'");

            Transform existingRoot =
                NadaRigPaths.FindDirectChild(
                    attachTarget,
                    Plugin.LocalWeaponRootName);

            // Preserve the existing drop probe behavior for this pass.
            if (existingRoot != null)
                return existingRoot.gameObject.activeSelf;

            VfxState state =
                NadaWeaponStateResolver.Resolve(
                    itemData);

            var context =
                new NadaWeaponRigContext(
                    root,
                    itemData,
                    state,
                    attachTarget);

            if (!context.IsValid)
                return false;

            NadaWeaponRigOrchestrator.Run(
                context);

            Transform refreshedRoot =
                NadaRigPaths.FindDirectChild(
                    attachTarget,
                    Plugin.LocalWeaponRootName);

            return refreshedRoot != null &&
                   refreshedRoot.gameObject.activeSelf;
        }

        private static bool TryApplyInternal(
            GameObject root,
            global::ItemDrop.ItemData itemData,
            VfxState state,
            Transform attachTarget)
        {
            if (root == null ||
                attachTarget == null)
            {
                return false;
            }

            Transform existingRoot =
                NadaRigPaths.FindDirectChild(
                    attachTarget,
                    Plugin.LocalWeaponRootName);

            if (existingRoot != null)
            {
                NadaLogControl.Equip(
                    $"refresh:{existingRoot.GetInstanceID()}",
                    $"{Plugin.ModName}: [Attach] refreshing existing rig on '{attachTarget.name}'.");
            }
            else
            {
                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [AttachTarget] " +
                    $"root='{root.name}' " +
                    $"visualRoot='{attachTarget.name}' " +
                    $"attachTarget='{attachTarget.name}' " +
                    $"path='{NadaWeaponTargets.FullPath(attachTarget)}'");
            }

            var context =
                new NadaWeaponRigContext(
                    root,
                    itemData,
                    state,
                    attachTarget);

            if (!context.IsValid)
                return false;

            NadaWeaponRigOrchestrator.Run(
                context);

            Transform refreshedRoot =
                NadaRigPaths.FindDirectChild(
                    attachTarget,
                    Plugin.LocalWeaponRootName);

            // Destroy is deferred. Reject a disabled stale shell left while
            // an invalid native binding is being retired.
            bool applied =
                refreshedRoot != null &&
                refreshedRoot.gameObject.activeSelf;

            if (applied)
            {
                LastAppliedLocalContext =
                    context;
            }

            return applied;
        }

        private static global::ItemDrop.ItemData ResolveItemData(
            GameObject root)
        {
            var itemDrop =
                root.GetComponent<global::ItemDrop>();

            if (itemDrop != null)
                return itemDrop.m_itemData;

            return NadaEquippedItemResolver
                .ResolveFirstEquippedItem();
        }
    }
}
