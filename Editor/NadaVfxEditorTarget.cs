
using System;
using System.Collections.Generic;
using System.Globalization;
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
            ItemData = itemData;

            SourceRoot = sourceRoot;
            VisualRoot = visualRoot;

            DisplayName = displayName ?? string.Empty;
            ItemNameKey = itemNameKey ?? string.Empty;
            PrefabName = prefabName ?? string.Empty;

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
            VisualDisplayName =
                ResolveVisualDisplayName(visualRoot);

            VisualRootInstanceId =
                visualRoot != null
                    ? visualRoot.GetInstanceID()
                    : 0;

            VisualRootPath =
                visualRoot != null
                    ? NadaWeaponTargets.FullPath(visualRoot)
                    : string.Empty;

            HasRuntimeRig = hasRuntimeRig;
            RuntimeRigInstanceId = runtimeRigInstanceId;

            IsLegacyBound = isLegacyBound;

            SourceKind = sourceKind;
            BoundViewState = boundViewState;
            BoundViewError = boundViewError;

            RuntimeStateSource =
                runtimeStateSource ?? string.Empty;
        }

        private static string ResolveVisualDisplayName(Transform root)
        {
            if (root == null)
                return string.Empty;

            string rootName = root.name ?? string.Empty;

            if (!IsGenericVisualName(rootName))
                return rootName;

            foreach (Renderer renderer in
                     root.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null ||
                    renderer is ParticleSystemRenderer ||
                    renderer is TrailRenderer ||
                    renderer is LineRenderer)
                {
                    continue;
                }

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
                {
                    mesh = skinned.sharedMesh;
                }
                else if (renderer is MeshRenderer)
                {
                    MeshFilter filter =
                        renderer.GetComponent<MeshFilter>();

                    if (filter != null)
                        mesh = filter.sharedMesh;
                }

                string meshName =
                    mesh != null ? mesh.name : null;

                if (!string.IsNullOrWhiteSpace(meshName) &&
                    !IsGenericVisualName(meshName))
                {
                    return rootName + " (mesh: " + meshName + ")";
                }
            }

            // No authoritative appearance ID was available from the live
            // hierarchy. Don't replace it with the item's original prefab ID.
            return rootName;
        }

        private static bool IsGenericVisualName(string value) =>
            string.Equals(
                value, "model",
                StringComparison.OrdinalIgnoreCase) ||
            string.Equals(
                value, "default",
                StringComparison.OrdinalIgnoreCase) ||
            string.Equals(
                value, "mesh",
                StringComparison.OrdinalIgnoreCase);
    }

    internal static class NadaVfxEditorTargetRegistry
    {
        // Null uses the original right-first fallback. After a manual switch,
        // only the chosen hand is eligible until switched again.
        private static NadaEquippedWeaponTargetResolver.Hand? _selectedHand;

        private static NadaEquippedWeaponTargetResolver.Hand _resolvedHand =
            NadaEquippedWeaponTargetResolver.Hand.Right;

        // Temporary Stage 15 diagnostic. A visual instance is inspected
        // once, even if the editor refreshes its target every frame.
        private static readonly HashSet<int>
            ProbedVisualRootInstanceIds = new();

        private const int MaxProbedVisualRoots = 128;
        private const int MaxLoggedMeshRenderers = 24;
        private const int MaxLoggedMaterialSlotsPerRenderer = 8;

        internal static string HandLabel =>
            (_selectedHand ?? _resolvedHand) ==
                NadaEquippedWeaponTargetResolver.Hand.Left
                ? "Left"
                : "Right";

        internal static void SwitchHands()
        {
            var current = _selectedHand ?? _resolvedHand;

            _selectedHand =
                current == NadaEquippedWeaponTargetResolver.Hand.Right
                    ? NadaEquippedWeaponTargetResolver.Hand.Left
                    : NadaEquippedWeaponTargetResolver.Hand.Right;

            RefreshFromRuntime();

            Plugin.Log?.LogInfo(
                $"{Plugin.ModName}: [EditorTargetHand] " +
                $"selected={HandLabel} " +
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
                    _selectedHand.Value,
                    out resolvedTarget)
                : NadaEquippedWeaponTargetResolver.TryResolveFirst(
                    out resolvedTarget);

            if (!found)
            {
                Clear();
                return;
            }

            _resolvedHand = resolvedTarget.SelectedHand;

            Capture(resolvedTarget);
        }

        internal static void Clear()
        {
            Current = null;
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
                boundViewError =
                    source.FailureReason ??
                    "Native weapon state is invalid.";
            }

            string itemNameKey =
                itemData.m_shared?.m_name ?? string.Empty;

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

            ProbeWeaponRenderersOnce(Current);
        }

        private static void ProbeWeaponRenderersOnce(
            NadaVfxEditorTarget target)
        {
            // This is handy for weird weapon hierarchies, but it's too
            // noisy to run whenever someone switches weapons in the editor.
            if (Plugin.DebugLoggingEnabled?.Value != true ||
                target?.VisualRoot == null)
            {
                return;
            }

            int visualRootId = target.VisualRootInstanceId;

            if (ProbedVisualRootInstanceIds.Contains(visualRootId))
                return;

            // Keep the temporary diagnostic's lifetime bounded during
            // long sessions with many equipped weapon instances.
            if (ProbedVisualRootInstanceIds.Count >= MaxProbedVisualRoots)
                ProbedVisualRootInstanceIds.Clear();

            ProbedVisualRootInstanceIds.Add(visualRootId);

            try
            {
                LogWeaponRendererDetails(target);
            }
            catch (Exception exception)
            {
                // Inspection is presentation-only. A broken custom shader
                // must not interfere with editor targeting or VFX binding.
                Plugin.Log?.LogWarning(
                    $"{Plugin.ModName}: [WeaponOpacityProbeError] " +
                    $"visualId={visualRootId} error='{exception}'.");
            }
        }

        private static void LogWeaponRendererDetails(
            NadaVfxEditorTarget target)
        {
            Transform visualRoot = target.VisualRoot;

            Plugin.Log?.LogInfo(
                $"{Plugin.ModName}: [WeaponOpacityProbe] BEGIN " +
                $"prefab='{target.PrefabName}' " +
                $"source={target.SourceKind} " +
                $"visualId={target.VisualRootInstanceId} " +
                $"visualPath='{target.VisualRootPath}'.");

            Renderer[] renderers =
                visualRoot.GetComponentsInChildren<Renderer>(true);

            int eligibleMeshRendererCount = 0;
            int loggedMeshRendererCount = 0;
            int nadaRendererCount = 0;
            int otherRendererCount = 0;

            foreach (Renderer renderer in renderers)
            {
                if (renderer == null)
                    continue;

                // Scope opacity to actual weapon geometry. Particle, trail,
                // line and other VFX renderers are deliberately excluded.
                if (renderer is not MeshRenderer &&
                    renderer is not SkinnedMeshRenderer)
                {
                    otherRendererCount++;
                    continue;
                }

                if (IsUnderNadaRig(
                        renderer.transform,
                        visualRoot))
                {
                    nadaRendererCount++;
                    continue;
                }

                eligibleMeshRendererCount++;

                if (loggedMeshRendererCount >= MaxLoggedMeshRenderers)
                    continue;

                loggedMeshRendererCount++;

                Mesh mesh = null;

                if (renderer is SkinnedMeshRenderer skinned)
                {
                    mesh = skinned.sharedMesh;
                }
                else
                {
                    MeshFilter filter =
                        renderer.GetComponent<MeshFilter>();

                    if (filter != null)
                        mesh = filter.sharedMesh;
                }

                Material[] materials = renderer.sharedMaterials;

                string rendererPath =
                    NadaWeaponTargets.FullPath(renderer.transform);

                Plugin.Log?.LogInfo(
                    $"{Plugin.ModName}: [WeaponOpacityProbe] RENDERER " +
                    $"type={renderer.GetType().Name} " +
                    $"path='{rendererPath}' " +
                    $"mesh='{(mesh != null ? mesh.name : "<none>")}' " +
                    $"enabled={renderer.enabled} " +
                    $"shadowCasting={renderer.shadowCastingMode} " +
                    $"receiveShadows={renderer.receiveShadows} " +
                    $"materialSlots={materials.Length}.");

                int loggedSlots =
                    Mathf.Min(
                        materials.Length,
                        MaxLoggedMaterialSlotsPerRenderer);

                for (int slot = 0; slot < loggedSlots; slot++)
                {
                    Material material = materials[slot];

                    if (material == null)
                    {
                        Plugin.Log?.LogInfo(
                            $"{Plugin.ModName}: [WeaponOpacityProbe] MATERIAL " +
                            $"renderer='{renderer.name}' " +
                            $"slot={slot} material=<null>.");

                        continue;
                    }

                    string shaderName =
                        material.shader != null
                            ? material.shader.name
                            : "<missing>";

                    string renderType =
                        material.GetTag(
                            "RenderType",
                            false,
                            "<none>");

                    Plugin.Log?.LogInfo(
                        $"{Plugin.ModName}: [WeaponOpacityProbe] MATERIAL " +
                        $"renderer='{renderer.name}' " +
                        $"slot={slot} " +
                        $"name='{material.name}' " +
                        $"shader='{shaderName}' " +
                        $"renderQueue={material.renderQueue} " +
                        $"renderType='{renderType}' " +
                        $"colorAlpha={ReadMaterialAlpha(material, "_Color")} " +
                        $"baseColorAlpha={ReadMaterialAlpha(material, "_BaseColor")} " +
                        $"surface={ReadMaterialFloat(material, "_Surface")} " +
                        $"mode={ReadMaterialFloat(material, "_Mode")} " +
                        $"zWrite={ReadMaterialFloat(material, "_ZWrite")} " +
                        $"srcBlend={ReadMaterialFloat(material, "_SrcBlend")} " +
                        $"dstBlend={ReadMaterialFloat(material, "_DstBlend")} " +
                        $"alphaBlendKeyword={material.IsKeywordEnabled("_ALPHABLEND_ON")} " +
                        $"alphaPremultiplyKeyword={material.IsKeywordEnabled("_ALPHAPREMULTIPLY_ON")}.");
                }

                if (materials.Length > loggedSlots)
                {
                    Plugin.Log?.LogInfo(
                        $"{Plugin.ModName}: [WeaponOpacityProbe] " +
                        $"renderer='{renderer.name}' " +
                        $"additionalMaterialSlots={materials.Length - loggedSlots}.");
                }
            }

            Plugin.Log?.LogInfo(
                $"{Plugin.ModName}: [WeaponOpacityProbe] END " +
                $"prefab='{target.PrefabName}' " +
                $"eligibleMeshRenderers={eligibleMeshRendererCount} " +
                $"loggedMeshRenderers={loggedMeshRendererCount} " +
                $"nadaMeshRenderersSkipped={nadaRendererCount} " +
                $"otherRenderersSkipped={otherRendererCount}.");
        }

        private static bool IsUnderNadaRig(
            Transform rendererTransform,
            Transform visualRoot)
        {
            for (Transform ancestor = rendererTransform;
                 ancestor != null;
                 ancestor = ancestor.parent)
            {
                if (ancestor.name == Plugin.LocalWeaponRootName)
                    return true;

                if (ancestor == visualRoot)
                    break;
            }

            return false;
        }

        private static string ReadMaterialAlpha(
            Material material,
            string propertyName)
        {
            if (!material.HasProperty(propertyName))
                return "<absent>";

            return material.GetColor(propertyName).a.ToString(
                "0.###",
                CultureInfo.InvariantCulture);
        }

        private static string ReadMaterialFloat(
            Material material,
            string propertyName)
        {
            if (!material.HasProperty(propertyName))
                return "<absent>";

            return material.GetFloat(propertyName).ToString(
                "0.###",
                CultureInfo.InvariantCulture);
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
                result,
                legacy);

            return result;
        }

        private static string ResolveDisplayName(
            string itemNameKey,
            string prefabName)
        {
            if (!string.IsNullOrWhiteSpace(itemNameKey))
            {
                try
                {
                    if (Localization.instance != null)
                    {
                        string localized =
                            Localization.instance.Localize(itemNameKey);

                        if (!string.IsNullOrWhiteSpace(localized) &&
                            !string.Equals(
                                localized,
                                itemNameKey,
                                StringComparison.Ordinal))
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
                        StringComparison.Ordinal))
                {
                    return itemNameKey.Trim();
                }
            }

            if (!string.IsNullOrWhiteSpace(prefabName))
                return prefabName;

            return "Unknown Weapon";
        }
    }
}
