using System.Collections.Generic;
using NADA.VFX.Weapon.Core.Config;
using NADA.VFX.Weapon.Core.State;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Blocks.Effects;
using NADA.VFX.Weapon.Core.Visuals;
using NADA.VFX.Weapon.Runtime.Binding;
using NADA.VFX.Weapon.Runtime.Structure;
using UnityEngine;

namespace NADA.VFX.Weapon.Modules.Effects
{
    internal sealed class NadaAuraEffect :
        MonoBehaviour,
        INadaItemDataReceiver,
        INadaResolvedStateReceiver
    {
        private static readonly Color BaseAuraColor =
            new(0.925f, 0.157f, 0.953f, 0.02f);

        private global::ItemDrop.ItemData _itemData;

        private VfxState _resolvedState;
        private bool _hasResolvedState;

        private VfxState _cachedBoundState;
        private bool _hasCachedBoundState;

        private AuraRuntimeState _blockState;
        private bool _hasBlockState;

        private AuraRuntimeState _lastAppliedState;
        private bool _hasLastAppliedState;

        private readonly List<Renderer> _auraRenderers = new();
        private readonly List<NadaAuraShell> _auraShells = new();

        private bool _componentCacheDirty = true;

        private float _lastAppliedHue;
        private float _lastAppliedLuminance;
        private bool _hasAppliedColor;

        private Vector3 _baseLocalPosition;
        private Quaternion _baseLocalRotation;
        private bool _hasBasePlacement;

        private struct AuraRuntimeState
        {
            internal bool Enabled;

            internal float Scale;
            internal float Luminance;
            internal float Hue;

            internal float XOffset;
            internal float YOffset;
            internal float ZOffset;

            internal float XRotation;
            internal float YRotation;
            internal float ZRotation;
        }

        public void SetItemData(
            global::ItemDrop.ItemData itemData)
        {
            _itemData = itemData;

            _hasResolvedState = false;
            _hasBlockState = false;

            // An explicit reconfiguration can update the same item.
            // Don't keep its previous bound-state snapshot.
            _hasCachedBoundState = false;
        }

        public void SetResolvedState(
            VfxState state)
        {
            _resolvedState = state;

            _hasResolvedState = true;
            _hasBlockState = false;

            _itemData = null;
            _hasCachedBoundState = false;
        }

        internal bool SetBlockState(
            VfxEffectBlock block)
        {
            if (block == null ||
                block.Transform == null ||
                block.TypeId != VfxEffectTypeIds.Aura ||
                block.Settings is not AuraVfxSettings settings)
            {
                // Block ownership must fail closed. Once Aura is being
                // driven as a block, malformed state must not silently
                // fall back to this client's ItemData or config.
                _blockState = default;
                _hasBlockState = true;

                _itemData = null;
                _hasResolvedState = false;
                _hasCachedBoundState = false;

                return false;
            }

            _blockState =
                new AuraRuntimeState
                {
                    Enabled =
                        block.Enabled,

                    Scale =
                        settings.Scale,

                    Luminance =
                        settings.Luminance,

                    Hue =
                        settings.Hue,

                    XOffset =
                        block.Transform.XOffset,

                    YOffset =
                        block.Transform.YOffset,

                    ZOffset =
                        block.Transform.ZOffset,

                    XRotation =
                        block.Transform.XRotation,

                    YRotation =
                        block.Transform.YRotation,

                    ZRotation =
                        block.Transform.ZRotation
                };

            _hasBlockState = true;

            _itemData = null;
            _hasResolvedState = false;
            _hasCachedBoundState = false;

            return true;
        }

        private void Awake()
        {
            RebuildComponentCache();
            _componentCacheDirty = false;

            CacheBasePlacement();

            InvokeRepeating(
                nameof(TickApply),
                0f,
                0.05f);
        }

        private void OnDestroy()
        {
            try
            {
                CancelInvoke(
                    nameof(TickApply));
            }
            catch
            {
            }
        }

        private void TickApply()
        {
            bool forceApply = false;

            if (_componentCacheDirty)
            {
                RebuildComponentCache();
                _componentCacheDirty = false;

                forceApply = true;
            }

            AuraRuntimeState state =
                ResolveRuntimeState();

            // Keep polling for live config editing, but a stable Aura doesn't
            // need its renderers, shell scale, and placement rewritten 20x/sec.
            if (!forceApply &&
                _hasLastAppliedState &&
                AuraStateEquals(
                    state,
                    _lastAppliedState))
            {
                return;
            }

            ApplyEnabled(
                state.Enabled);

            ApplyColorIfChanged(
                state.Hue,
                state.Luminance);

            ApplyScale(
                state.Scale);

            ApplyPlacement(
                state.XOffset,
                state.YOffset,
                state.ZOffset,
                state.XRotation,
                state.YRotation,
                state.ZRotation);

            _lastAppliedState = state;
            _hasLastAppliedState = true;
        }

        private static bool AuraStateEquals(
            AuraRuntimeState left,
            AuraRuntimeState right)
        {
            return
                left.Enabled ==
                    right.Enabled &&

                FloatEquals(
                    left.Hue,
                    right.Hue) &&
                FloatEquals(
                    left.Luminance,
                    right.Luminance) &&
                FloatEquals(
                    left.Scale,
                    right.Scale) &&

                FloatEquals(
                    left.XOffset,
                    right.XOffset) &&
                FloatEquals(
                    left.YOffset,
                    right.YOffset) &&
                FloatEquals(
                    left.ZOffset,
                    right.ZOffset) &&

                FloatEquals(
                    left.XRotation,
                    right.XRotation) &&
                FloatEquals(
                    left.YRotation,
                    right.YRotation) &&
                FloatEquals(
                    left.ZRotation,
                    right.ZRotation);
        }

        private static bool FloatEquals(
            float left,
            float right)
        {
            return left.Equals(right);
        }

        private AuraRuntimeState ResolveRuntimeState()
        {
            if (_hasBlockState)
                return _blockState;

            if (_hasResolvedState)
            {
                return FromLegacyState(
                    _resolvedState);
            }

            if (_itemData != null &&
                VfxStateIO.IsBound(_itemData))
            {
                if (_hasCachedBoundState)
                {
                    return FromLegacyState(
                        _cachedBoundState);
                }

                if (VfxStateIO.TryRead(
                        _itemData,
                        out VfxState itemState))
                {
                    _cachedBoundState = itemState;
                    _hasCachedBoundState = true;

                    return FromLegacyState(
                        itemState);
                }
            }

            // An unbound item must return to live config editing.
            _hasCachedBoundState = false;

            return FromLegacyState(
                VfxStateIO.FromConfig());
        }

        private static AuraRuntimeState FromLegacyState(
            VfxState state)
        {
            return new AuraRuntimeState
            {
                Enabled =
                    state.AuraEnabled,

                Scale =
                    state.AuraScale,

                Luminance =
                    state.AuraLuminance,

                Hue =
                    state.AuraHue,

                XOffset =
                    state.AuraXOffset,

                YOffset =
                    state.AuraYOffset,

                ZOffset =
                    state.AuraZOffset,

                XRotation =
                    state.AuraXRotation,

                YRotation =
                    state.AuraYRotation,

                ZRotation =
                    state.AuraZRotation
            };
        }

        private void CacheBasePlacement()
        {
            if (_hasBasePlacement)
                return;

            _baseLocalPosition =
                transform.localPosition;

            _baseLocalRotation =
                transform.localRotation;

            _hasBasePlacement = true;
        }

        private void RebuildComponentCache()
        {
            _auraRenderers.Clear();
            _auraShells.Clear();

            // Aura shells belong to this branch. Don't search the
            // surrounding weapon hierarchy for renderers to modify.
            foreach (Renderer renderer in
                     GetComponentsInChildren<Renderer>(true))
            {
                if (renderer != null &&
                    IsAuraRenderer(renderer.transform))
                {
                    _auraRenderers.Add(
                        renderer);
                }
            }

            foreach (NadaAuraShell shell in
                     GetComponentsInChildren<NadaAuraShell>(true))
            {
                if (shell != null)
                {
                    _auraShells.Add(
                        shell);
                }
            }

            _hasAppliedColor = false;
        }

        private void ApplyEnabled(
            bool enabled)
        {
            foreach (Renderer renderer in _auraRenderers)
            {
                if (renderer == null)
                    continue;

                renderer.enabled =
                    enabled;
            }
        }

        private void ApplyColorIfChanged(
            float hue,
            float luminance)
        {
            if (_hasAppliedColor &&
                Mathf.Approximately(
                    _lastAppliedHue,
                    hue) &&
                Mathf.Approximately(
                    _lastAppliedLuminance,
                    luminance))
            {
                return;
            }

            ApplyColor(
                hue,
                luminance);

            _lastAppliedHue =
                hue;

            _lastAppliedLuminance =
                luminance;

            _hasAppliedColor =
                true;
        }

        private void ApplyColor(
            float hue,
            float luminance)
        {
            float targetHue =
                NadaHueShiftUtility
                    .SliderValueToTargetHue(
                        hue);

            float clampedLuminance =
                Mathf.Clamp(
                    luminance,
                    PluginConfig.MinLuminance,
                    PluginConfig.MaxLuminance);

            Color tintedColor =
                NadaLuminanceUtility
                    .ApplyToColor(
                        NadaHueShiftUtility
                            .RetintColorToHue(
                                BaseAuraColor,
                                targetHue),
                        clampedLuminance);

            foreach (Renderer renderer in _auraRenderers)
            {
                if (renderer == null)
                    continue;

                Material[] materials =
                    renderer.sharedMaterials;

                if (materials == null)
                    continue;

                foreach (Material material in materials)
                {
                    if (material == null)
                        continue;

                    if (material.HasProperty(
                            "_TintColor"))
                    {
                        material.SetColor(
                            "_TintColor",
                            tintedColor);
                    }

                    if (material.HasProperty(
                            "_EmissionColor"))
                    {
                        material.EnableKeyword(
                            "_EMISSION");

                        material.SetColor(
                            "_EmissionColor",
                            tintedColor *
                            clampedLuminance);
                    }
                }
            }
        }

        private void ApplyPlacement(
            float xOffset,
            float yOffset,
            float zOffset,
            float xRotation,
            float yRotation,
            float zRotation)
        {
            if (!_hasBasePlacement)
                return;

            NadaEffectTransformApplier.ApplyLocalPlacement(
                transform,
                _baseLocalPosition,
                _baseLocalRotation,
                ClampOffset(xOffset),
                ClampOffset(yOffset),
                ClampOffset(zOffset),
                ClampRotation(xRotation),
                ClampRotation(yRotation),
                ClampRotation(zRotation));
        }

        private void ApplyScale(
            float scale)
        {
            float clampedScale =
                Mathf.Clamp(
                    scale,
                    PluginConfig.MinAuraScale,
                    PluginConfig.MaxAuraScale);

            foreach (NadaAuraShell shell in _auraShells)
            {
                if (shell == null)
                    continue;

                ApplyShellScale(
                    shell,
                    clampedScale);
            }
        }

        private static void ApplyShellScale(
            NadaAuraShell shell,
            float clampedScale)
        {
            Transform scalePivot =
                shell.transform.parent;

            if (scalePivot != null &&
                scalePivot.name.StartsWith(
                    "Aura Shell",
                    System.StringComparison.Ordinal))
            {
                scalePivot.localScale =
                    shell.UsesReadableMesh
                        ? shell.BasePivotLocalScale
                        : BuildBoundsAwareUnreadableScale(
                            shell.BasePivotLocalScale,
                            shell.SourceBoundsSize,
                            clampedScale);
            }

            shell.transform.localScale =
                Vector3.one;

            if (shell.UsesReadableMesh)
            {
                shell.ApplyScale(
                    clampedScale);
            }
        }

        private static float ClampOffset(
            float value)
        {
            if (float.IsNaN(value) ||
                float.IsInfinity(value))
            {
                return PluginConfig.DefaultEffectOffset;
            }

            return Mathf.Clamp(
                value,
                PluginConfig.MinEffectOffset,
                PluginConfig.MaxEffectOffset);
        }

        private static float ClampRotation(
            float value)
        {
            if (float.IsNaN(value) ||
                float.IsInfinity(value))
            {
                return PluginConfig.DefaultEffectRotation;
            }

            return Mathf.Clamp(
                value,
                PluginConfig.MinEffectRotation,
                PluginConfig.MaxEffectRotation);
        }

        private static Vector3 BuildBoundsAwareUnreadableScale(
            Vector3 basePivotScale,
            Vector3 boundsSize,
            float scale)
        {
            float delta =
                scale - 1f;

            int longAxis =
                GetLargestAxis(
                    boundsSize);

            Vector3 weights =
                Vector3.one;

            weights[longAxis] =
                0.15f;

            return new Vector3(
                basePivotScale.x *
                (1f + delta * weights.x),
                basePivotScale.y *
                (1f + delta * weights.y),
                basePivotScale.z *
                (1f + delta * weights.z));
        }

        private static int GetLargestAxis(
            Vector3 value)
        {
            if (value.x >= value.y &&
                value.x >= value.z)
            {
                return 0;
            }

            if (value.y >= value.x &&
                value.y >= value.z)
            {
                return 1;
            }

            return 2;
        }

        private static bool IsAuraRenderer(
            Transform transform)
        {
            if (transform == null)
                return false;

            if (transform.name ==
                "Aura Mesh")
            {
                return true;
            }

            if (transform.name.StartsWith(
                    "Aura Shell",
                    System.StringComparison.Ordinal))
            {
                return true;
            }

            Transform parent =
                transform.parent;

            return
                parent != null &&
                parent.name.StartsWith(
                    "Aura Shell",
                    System.StringComparison.Ordinal);
        }
    }
}