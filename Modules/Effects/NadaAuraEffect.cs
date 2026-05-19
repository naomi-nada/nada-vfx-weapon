using System.Collections.Generic;
using NADA.VFX.Core.Config;
using NADA.VFX.Core.State;
using NADA.VFX.Core.Visuals;
using NADA.VFX.Runtime.Binding;
using NADA.VFX.Runtime.Structure;
using UnityEngine;

namespace NADA.VFX.Modules.Effects
{
    internal sealed class NadaAuraEffect : MonoBehaviour, INadaItemDataReceiver
    {
        private static readonly Color BaseAuraColor =
            new(0.925f, 0.157f, 0.953f, 0.02f);

        private global::ItemDrop.ItemData _itemData;

        private readonly List<Renderer> _auraRenderers = new();
        private readonly List<NadaAuraShell> _auraShells = new();

        private bool _componentCacheDirty = true;

        private Vector3 _baseLocalPosition;
        private Quaternion _baseLocalRotation;
        private bool _hasBasePlacement;

        public void SetItemData(global::ItemDrop.ItemData itemData)
        {
            _itemData = itemData;
        }

        private void Awake()
        {
            RebuildComponentCache();
            _componentCacheDirty = false;

            CacheBasePlacement();

            InvokeRepeating(nameof(TickApply), 0f, 0.05f);
        }

        private void OnDestroy()
        {
            try { CancelInvoke(nameof(TickApply)); } catch { }
        }

        private void TickApply()
        {
            if (_componentCacheDirty)
            {
                RebuildComponentCache();
                _componentCacheDirty = false;
            }

            VfxState state = ResolveState();

            ApplyEnabled(state.AuraEnabled);
            ApplyColor(state.AuraHue, state.AuraLuminance);
            ApplyScale(state.AuraScale);

            ApplyPlacement(
                state.AuraXOffset,
                state.AuraYOffset,
                state.AuraZOffset,
                state.AuraXRotation,
                state.AuraYRotation,
                state.AuraZRotation);
        }

        private VfxState ResolveState()
        {
            if (_itemData != null && VfxStateIO.IsBound(_itemData))
            {
                if (VfxStateIO.TryRead(_itemData, out var itemState))
                    return itemState;
            }

            return VfxStateIO.FromConfig();
        }

        private void CacheBasePlacement()
        {
            if (_hasBasePlacement)
                return;

            _baseLocalPosition = transform.localPosition;
            _baseLocalRotation = transform.localRotation;
            _hasBasePlacement = true;
        }

        private void RebuildComponentCache()
        {
            _auraRenderers.Clear();
            _auraShells.Clear();

            Transform searchRoot = ResolveAuraSearchRoot();

            foreach (Renderer renderer in searchRoot.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer != null && IsAuraRenderer(renderer.transform))
                    _auraRenderers.Add(renderer);
            }

            foreach (NadaAuraShell shell in searchRoot.GetComponentsInChildren<NadaAuraShell>(true))
            {
                if (shell != null)
                    _auraShells.Add(shell);
            }
        }

        private Transform ResolveAuraSearchRoot()
        {
            Transform current = transform;

            while (current != null)
            {
                if (current.name == Plugin.LocalWeaponRootName)
                    return current.parent != null ? current.parent : transform;

                current = current.parent;
            }

            return transform;
        }

        private void ApplyEnabled(bool enabled)
        {
            foreach (Renderer renderer in _auraRenderers)
            {
                if (renderer == null)
                    continue;

                renderer.enabled = enabled;
            }
        }

        private void ApplyColor(float hue, float luminance)
        {
            float targetHue = NadaHueShiftUtility.SliderValueToTargetHue(hue);

            float clampedLuminance = Mathf.Clamp(
                luminance,
                PluginConfig.MinLuminance,
                PluginConfig.MaxLuminance);

            Color tintedColor =
                NadaLuminanceUtility.ApplyToColor(
                    NadaHueShiftUtility.RetintColorToHue(BaseAuraColor, targetHue),
                    clampedLuminance);

            foreach (Renderer renderer in _auraRenderers)
            {
                if (renderer == null)
                    continue;

                Material[] materials = renderer.materials;
                if (materials == null)
                    continue;

                foreach (Material material in materials)
                {
                    if (material == null)
                        continue;

                    if (material.HasProperty("_TintColor"))
                        material.SetColor("_TintColor", tintedColor);

                    if (material.HasProperty("_EmissionColor"))
                    {
                        material.EnableKeyword("_EMISSION");
                        material.SetColor("_EmissionColor", tintedColor * clampedLuminance);
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

        private void ApplyScale(float scale)
        {
            float clampedScale = Mathf.Clamp(
                scale,
                PluginConfig.MinAuraScale,
                PluginConfig.MaxAuraScale);

            foreach (NadaAuraShell shell in _auraShells)
            {
                if (shell == null)
                    continue;

                ApplyShellScale(shell, clampedScale);
            }
        }

        private static void ApplyShellScale(
            NadaAuraShell shell,
            float clampedScale)
        {
            Transform scalePivot = shell.transform.parent;

            if (scalePivot != null &&
                scalePivot.name.StartsWith("Aura Shell", System.StringComparison.Ordinal))
            {
                scalePivot.localScale = shell.UsesReadableMesh
                    ? shell.BasePivotLocalScale
                    : BuildBoundsAwareUnreadableScale(
                        shell.BasePivotLocalScale,
                        shell.SourceBoundsSize,
                        clampedScale);
            }

            shell.transform.localScale = Vector3.one;

            if (shell.UsesReadableMesh)
                shell.ApplyScale(clampedScale);
        }

        private static float ClampOffset(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                return PluginConfig.DefaultEffectOffset;

            return Mathf.Clamp(
                value,
                PluginConfig.MinEffectOffset,
                PluginConfig.MaxEffectOffset);
        }
        
        private static float ClampRotation(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                return PluginConfig.DefaultEffectRotation;

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
            float delta = scale - 1f;

            int longAxis = GetLargestAxis(boundsSize);

            Vector3 weights = Vector3.one;
            weights[longAxis] = 0.15f;

            return new Vector3(
                basePivotScale.x * (1f + delta * weights.x),
                basePivotScale.y * (1f + delta * weights.y),
                basePivotScale.z * (1f + delta * weights.z));
        }

        private static int GetLargestAxis(Vector3 value)
        {
            if (value.x >= value.y && value.x >= value.z)
                return 0;

            if (value.y >= value.x && value.y >= value.z)
                return 1;

            return 2;
        }

        private static bool IsAuraRenderer(Transform transform)
        {
            if (transform == null)
                return false;

            if (transform.name == "Aura Mesh")
                return true;

            if (transform.name.StartsWith("Aura Shell", System.StringComparison.Ordinal))
                return true;

            Transform parent = transform.parent;

            return parent != null &&
                   parent.name.StartsWith("Aura Shell", System.StringComparison.Ordinal);
        }
    }
}