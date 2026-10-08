using NADA.VFX.Weapon.Core.State;
using NADA.VFX.Weapon.Runtime.Structure;
using NADA.VFX.Weapon.Weapons.Targets;
using UnityEngine;

namespace NADA.VFX.Weapon.Editor
{
    internal sealed class NadaVfxEditorTarget
    {
        internal global::ItemDrop.ItemData ItemData { get; }

        internal GameObject SourceRoot { get; }
        internal Transform VisualRoot { get; }

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
            GameObject sourceRoot,
            Transform visualRoot,
            string displayName,
            string itemNameKey,
            string prefabName,
            bool hasRuntimeRig,
            int runtimeRigInstanceId,
            bool isLegacyBound,
            string runtimeStateSource)
        {
            ItemData =
                itemData;

            SourceRoot =
                sourceRoot;

            VisualRoot =
                visualRoot;

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
                sourceRoot != null
                    ? sourceRoot.name
                    : string.Empty;

            SourceRootInstanceId =
                sourceRoot != null
                    ? sourceRoot.GetInstanceID()
                    : 0;

            VisualRootName =
                visualRoot != null
                    ? visualRoot.name
                    : string.Empty;

            VisualRootInstanceId =
                visualRoot != null
                    ? visualRoot.GetInstanceID()
                    : 0;

            VisualRootPath =
                visualRoot != null
                    ? NadaWeaponTargets.FullPath(
                        visualRoot)
                    : string.Empty;

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
            if (!NadaEquippedWeaponTargetResolver.TryResolveFirst(
                    out NadaEquippedWeaponTargetResolver.ResolvedTarget
                        resolvedTarget))
            {
                Clear();

                return;
            }

            Capture(
                resolvedTarget);
        }

        internal static void Clear()
        {
            Current =
                null;
        }

        private static void Capture(
            NadaEquippedWeaponTargetResolver.ResolvedTarget resolvedTarget)
        {
            if (resolvedTarget == null ||
                resolvedTarget.ItemData == null ||
                resolvedTarget.Root == null ||
                resolvedTarget.VisualRoot == null)
            {
                Clear();

                return;
            }

            global::ItemDrop.ItemData itemData =
                resolvedTarget.ItemData;

            bool isLegacyBound =
                VfxStateIO.IsBound(
                    itemData);

            string itemNameKey =
                itemData.m_shared?.m_name ??
                string.Empty;

            string prefabName =
                itemData.m_dropPrefab != null
                    ? itemData.m_dropPrefab.name
                    : resolvedTarget.Root.name;

            string displayName =
                ResolveDisplayName(
                    itemNameKey,
                    prefabName);

            Transform runtimeRigTransform =
                NadaRigPaths.FindDirectChild(
                    resolvedTarget.VisualRoot,
                    Plugin.LocalWeaponRootName);

            bool hasRuntimeRig =
                runtimeRigTransform != null;

            string runtimeStateSource =
                isLegacyBound
                    ? "Legacy runtime: bound ItemData"
                    : "Editor/runtime defaults: unbound ItemData";

            Current =
                new NadaVfxEditorTarget(
                    itemData,
                    resolvedTarget.Root,
                    resolvedTarget.VisualRoot,
                    displayName,
                    itemNameKey,
                    prefabName,
                    hasRuntimeRig,
                    hasRuntimeRig
                        ? runtimeRigTransform.GetInstanceID()
                        : 0,
                    isLegacyBound,
                    runtimeStateSource);
        }

        private static string ResolveDisplayName(
            string itemNameKey,
            string prefabName)
        {
            if (!string.IsNullOrWhiteSpace(
                    itemNameKey))
            {
                try
                {
                    if (Localization.instance != null)
                    {
                        string localized =
                            Localization.instance.Localize(
                                itemNameKey);

                        if (!string.IsNullOrWhiteSpace(
                                localized) &&
                            !string.Equals(
                                localized,
                                itemNameKey,
                                System.StringComparison.Ordinal))
                        {
                            return localized.Trim();
                        }
                    }
                }
                catch
                {
                    // Localization is presentation-only. Falling back to the
                    // item's own literal name or prefab identity is safe.
                }

                if (!itemNameKey.StartsWith(
                        "$",
                        System.StringComparison.Ordinal))
                {
                    return itemNameKey.Trim();
                }
            }

            if (!string.IsNullOrWhiteSpace(
                    prefabName))
            {
                return prefabName;
            }

            return "Unknown Weapon";
        }
    }
}