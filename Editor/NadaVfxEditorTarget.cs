using NADA.VFX.Weapon.Runtime.Formation;
using NADA.VFX.Weapon.Weapons.Runtime;
using NADA.VFX.Weapon.Core.State.Migration;
using NADA.VFX.Weapon.Core.State.Blocks;
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
        internal string VisualDisplayName { get; }
        internal int VisualRootInstanceId { get; }
        internal string VisualRootPath { get; }

        internal bool HasRuntimeRig { get; }
        internal int RuntimeRigInstanceId { get; }

        internal bool IsLegacyBound { get; }
        internal NadaWeaponLocalSourceKind SourceKind { get; }
        internal WeaponVfxState BoundViewState { get; }
        internal string BoundViewError { get; }
        internal bool IsReadOnly =>
            SourceKind == NadaWeaponLocalSourceKind.NativeBound ||
            SourceKind == NadaWeaponLocalSourceKind.LegacyBound ||
            SourceKind == NadaWeaponLocalSourceKind.InvalidNative;
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
            string runtimeStateSource,
            NadaWeaponLocalSourceKind sourceKind,
            WeaponVfxState boundViewState,
            string boundViewError)
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

            // Generic wrapper names such as 'model' do not identify a skin.
            // If a render mesh carries a useful name, show it as a mesh
            // identity, not as a claimed transmog/prefab ID.
            VisualDisplayName = ResolveVisualDisplayName(visualRoot);

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

            SourceKind = sourceKind;
            BoundViewState = boundViewState;
            BoundViewError = boundViewError;

            RuntimeStateSource =
                runtimeStateSource ??
                string.Empty;
        }

        private static string ResolveVisualDisplayName(Transform root)
        {
            if (root == null)
                return string.Empty;

            string rootName = root.name ?? string.Empty;
            if (!IsGenericVisualName(rootName))
                return rootName;

            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null ||
                    renderer is ParticleSystemRenderer ||
                    renderer is TrailRenderer ||
                    renderer is LineRenderer)
                    continue;

                // The native effects live beneath their own weapon root.
                // Ignore those renderers when identifying the original visual.
                bool nadaOwned = false;
                for (Transform ancestor = renderer.transform;
                     ancestor != null && ancestor != root;
                     ancestor = ancestor.parent)
                {
                    if (ancestor.name == Plugin.LocalWeaponRootName)
                    {
                        nadaOwned = true;
                        break;
                    }
                }
                if (nadaOwned)
                    continue;

                Mesh mesh = null;
                if (renderer is SkinnedMeshRenderer skinned)
                    mesh = skinned.sharedMesh;
                else if (renderer is MeshRenderer)
                {
                    MeshFilter filter = renderer.GetComponent<MeshFilter>();
                    if (filter != null)
                        mesh = filter.sharedMesh;
                }

                string meshName = mesh != null ? mesh.name : null;
                if (!string.IsNullOrWhiteSpace(meshName) &&
                    !IsGenericVisualName(meshName))
                    return rootName + " (mesh: " + meshName + ")";
            }

            // No authoritative appearance ID was available from the live
            // hierarchy. Don't replace it with the item's original prefab ID.
            return rootName;
        }

        private static bool IsGenericVisualName(string value) =>
            string.Equals(value, "model", System.StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "default", System.StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "mesh", System.StringComparison.OrdinalIgnoreCase);
    }

    internal static class NadaVfxEditorTargetRegistry
    {
        // Null uses the original right-first fallback. After a manual switch,
        // only the chosen hand is eligible until switched again.
        private static NadaEquippedWeaponTargetResolver.Hand? _selectedHand;
        private static NadaEquippedWeaponTargetResolver.Hand _resolvedHand =
            NadaEquippedWeaponTargetResolver.Hand.Right;

        internal static string HandLabel =>
            (_selectedHand ?? _resolvedHand) ==
                NadaEquippedWeaponTargetResolver.Hand.Left ? "Left" : "Right";

        internal static void SwitchHands()
        {
            var current = _selectedHand ?? _resolvedHand;
            _selectedHand = current == NadaEquippedWeaponTargetResolver.Hand.Right
                ? NadaEquippedWeaponTargetResolver.Hand.Left
                : NadaEquippedWeaponTargetResolver.Hand.Right;
            RefreshFromRuntime();
            Plugin.Log?.LogInfo($"{Plugin.ModName}: [EditorTargetHand] selected={HandLabel} " +
                $"item='{Current?.PrefabName ?? "<none>"}'.");
        }

        internal static NadaVfxEditorTarget Current
        {
            get;
            private set;
        }

        internal static void RefreshFromRuntime()
        {
            NadaEquippedWeaponTargetResolver.ResolvedTarget resolvedTarget;
            bool found = _selectedHand.HasValue
                ? NadaEquippedWeaponTargetResolver.TryResolveSelected(
                    _selectedHand.Value, out resolvedTarget)
                : NadaEquippedWeaponTargetResolver.TryResolveFirst(
                    out resolvedTarget);
            if (!found)
            {
                Clear();

                return;
            }

            _resolvedHand = resolvedTarget.SelectedHand;
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

            NadaWeaponLocalSourceSelection source =
                NadaWeaponLocalSourceResolver.Resolve(itemData);

            bool isLegacyBound =
                source.Kind == NadaWeaponLocalSourceKind.LegacyBound;

            WeaponVfxState boundViewState = null;
            string boundViewError = null;

            if (source.Kind == NadaWeaponLocalSourceKind.NativeBound)
            {
                // The decoded state is a detached data snapshot. It never
                // becomes the runtime's mutable state or an editor draft.
                boundViewState = source.State;

                if (boundViewState == null ||
                    !OrbitalsFormationResolver.TryResolve(
                        boundViewState,
                        out OrbitalsFormationResolution formation) ||
                    formation == null ||
                    !formation.IsValid)
                {
                    boundViewState = null;
                    boundViewError = "Native block formation is invalid.";
                }
            }
            else if (isLegacyBound)
            {
                if (VfxStateIO.TryRead(itemData, out VfxState legacy))
                {
                    boundViewState =
                        BuildLegacyBoundView(legacy);
                }
                else
                {
                    boundViewError =
                        "Legacy binding could not be read.";
                }
            }
            else if (source.Kind == NadaWeaponLocalSourceKind.InvalidNative)
            {
                boundViewError = source.FailureReason ??
                    "Native weapon state is invalid.";
            }

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
                source.Kind == NadaWeaponLocalSourceKind.NativeBound
                    ? "Native runtime: bound ItemData"
                    : isLegacyBound
                        ? "Legacy runtime: bound ItemData"
                        : source.Kind == NadaWeaponLocalSourceKind.InvalidNative
                            ? "Invalid native binding: rendering blocked"
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
                    runtimeStateSource,
                    source.Kind,
                    boundViewState,
                    boundViewError);
        }

        // The legacy adapter exists for migration and read-only presentation.
        // The editor never writes this view back through legacy persistence.
        private static WeaponVfxState BuildLegacyBoundView(VfxState legacy)
        {
            WeaponVfxState result =
                LegacyVfxStateAdapter.CreateInnerFlamesPrototype(legacy);

            result.Effects.AddRange(
                LegacyVfxStateAdapter.CreateOuterFlamesPrototype(legacy).Effects);
            result.Effects.AddRange(
                LegacyVfxStateAdapter.CreateStrandsPrototype(legacy).Effects);
            result.Effects.AddRange(
                LegacyVfxStateAdapter.CreateSparksPrototype(legacy).Effects);
            result.Effects.AddRange(
                LegacyVfxStateAdapter.CreateFlarePrototype(legacy).Effects);
            result.Effects.AddRange(
                LegacyVfxStateAdapter.CreateAuraPrototype(legacy).Effects);
            result.Effects.AddRange(
                LegacyVfxStateAdapter.CreateOrbitalsOrbsPrototype(legacy).Effects);
            result.Effects.AddRange(
                LegacyVfxStateAdapter.CreateOrbitalsCoresPrototype(legacy).Effects);
            result.Effects.AddRange(
                LegacyVfxStateAdapter.CreateOrbitalsFlamesPrototype(legacy).Effects);
            result.Effects.AddRange(
                LegacyVfxStateAdapter.CreateOrbitalsEmbersPrototype(legacy).Effects);

            LegacyVfxStateAdapter.ApplyMigratedOrbitalsRelationships(
                result, legacy);

            return result;
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