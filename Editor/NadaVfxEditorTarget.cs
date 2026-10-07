using NADA.VFX.Weapon.Core.State;
using NADA.VFX.Weapon.Runtime.Structure;
using NADA.VFX.Weapon.Weapons.Runtime;
using NADA.VFX.Weapon.Weapons.Targets;
using UnityEngine;

namespace NADA.VFX.Weapon.Editor
{
    internal sealed class NadaVfxEditorTarget
    {
        internal global::ItemDrop.ItemData ItemData { get; }

        internal string DisplayName { get; }
        internal string ItemNameKey { get; }
        internal string PrefabName { get; }

        internal string SourceRootName { get; }
        internal int SourceRootInstanceId { get; }

        internal string VisualRootName { get; }
        internal int VisualRootInstanceId { get; }
        internal string VisualRootPath { get; }

        internal bool HasRuntimeRig { get; }
        internal int RuntimeRigInstanceId { get; }

        internal bool IsLegacyBound { get; }
        internal string RuntimeStateSource { get; }

        internal NadaVfxEditorTarget(
            global::ItemDrop.ItemData itemData,
            string displayName,
            string itemNameKey,
            string prefabName,
            string sourceRootName,
            int sourceRootInstanceId,
            string visualRootName,
            int visualRootInstanceId,
            string visualRootPath,
            bool hasRuntimeRig,
            int runtimeRigInstanceId,
            bool isLegacyBound,
            string runtimeStateSource)
        {
            ItemData =
                itemData;

            DisplayName =
                displayName ??
                string.Empty;

            ItemNameKey =
                itemNameKey ??
                string.Empty;

            PrefabName =
                prefabName ??
                string.Empty;

            SourceRootName =
                sourceRootName ??
                string.Empty;

            SourceRootInstanceId =
                sourceRootInstanceId;

            VisualRootName =
                visualRootName ??
                string.Empty;

            VisualRootInstanceId =
                visualRootInstanceId;

            VisualRootPath =
                visualRootPath ??
                string.Empty;

            HasRuntimeRig =
                hasRuntimeRig;

            RuntimeRigInstanceId =
                runtimeRigInstanceId;

            IsLegacyBound =
                isLegacyBound;

            RuntimeStateSource =
                runtimeStateSource ??
                string.Empty;
        }
    }

    internal static class NadaVfxEditorTargetRegistry
    {
        internal static NadaVfxEditorTarget Current
        {
            get;
            private set;
        }

        internal static void RefreshFromRuntime()
        {
            NadaWeaponRigContext context =
                NadaWeaponRigController
                    .LastAppliedLocalContext;

            if (context == null ||
                !context.IsValid ||
                context.ItemData == null)
            {
                Clear();

                return;
            }

            global::ItemDrop.ItemData currentEquippedItem =
                NadaEquippedItemResolver
                    .ResolveFirstEquippedItem();

            if (currentEquippedItem == null ||
                !object.ReferenceEquals(
                    currentEquippedItem,
                    context.ItemData))
            {
                Clear();

                return;
            }

            Capture(
                context);
        }

        internal static void Clear()
        {
            Current =
                null;
        }

        private static void Capture(
            NadaWeaponRigContext context)
        {
            global::ItemDrop.ItemData itemData =
                context.ItemData;

            bool isLegacyBound =
                VfxStateIO.IsBound(
                    itemData);

            string itemNameKey =
                itemData?.m_shared?.m_name ??
                "<unknown>";

            string prefabName =
                itemData?.m_dropPrefab != null
                    ? itemData.m_dropPrefab.name
                    : context.Root?.name ??
                      "<unknown>";

            string displayName =
                !string.IsNullOrWhiteSpace(
                    prefabName)
                    ? prefabName
                    : itemNameKey;

            Transform runtimeRigTransform =
                context.WeaponVisualRoot != null
                    ? NadaRigPaths.FindDirectChild(
                        context.WeaponVisualRoot,
                        Plugin.LocalWeaponRootName)
                    : null;

            bool hasRuntimeRig =
                runtimeRigTransform != null;

            string runtimeStateSource =
                isLegacyBound
                    ? "Legacy runtime: bound ItemData"
                    : "Legacy runtime: manager defaults";

            Current =
                new NadaVfxEditorTarget(
                    itemData,
                    displayName,
                    itemNameKey,
                    prefabName,
                    context.Root != null
                        ? context.Root.name
                        : "<none>",
                    context.Root != null
                        ? context.Root.GetInstanceID()
                        : 0,
                    context.WeaponVisualRoot != null
                        ? context.WeaponVisualRoot.name
                        : "<none>",
                    context.WeaponVisualRoot != null
                        ? context.WeaponVisualRoot.GetInstanceID()
                        : 0,
                    context.WeaponVisualRoot != null
                        ? NadaWeaponTargets.FullPath(
                            context.WeaponVisualRoot)
                        : string.Empty,
                    hasRuntimeRig,
                    hasRuntimeRig
                        ? runtimeRigTransform.GetInstanceID()
                        : 0,
                    isLegacyBound,
                    runtimeStateSource);
        }
    }
}