using System.Collections.Generic;
using NADA.VFX.Core.Config;
using NADA.VFX.Core.State;
using NADA.VFX.Core.Visuals;
using NADA.VFX.Runtime.Binding;
using NADA.VFX.Runtime.Structure;
using UnityEngine;

namespace NADA.VFX.Modules.Effects
{
    internal sealed class NadaFlareEffect : MonoBehaviour, INadaItemDataReceiver
    {
        private global::ItemDrop.ItemData _itemData;

        private Renderer[] _renderers;
        private Light[] _lights;
        private ParticleSystem[] _systems;

        private Vector3 _baseScale;
        private bool _hasBaseScale;

        private Vector3 _baseLocalPosition;
        private Quaternion _baseLocalRotation;
        private bool _hasBasePlacement;

        private float _lastHue;
        private bool _hasLastHue;

        private bool _componentCacheDirty = true;
        private bool _baselineCacheDirty = true;

        private readonly Dictionary<int, ParticleSystem.MinMaxGradient> _baseMainStartColor = new();
        private readonly Dictionary<int, bool> _baseColorOverLifetimeEnabled = new();
        private readonly Dictionary<int, ParticleSystem.MinMaxGradient> _baseColorOverLifetime = new();

        private readonly Dictionary<int, bool> _baseCustomDataEnabled = new();
        private readonly Dictionary<int, ParticleSystemCustomDataMode> _baseCustom1Mode = new();
        private readonly Dictionary<int, ParticleSystemCustomDataMode> _baseCustom2Mode = new();
        private readonly Dictionary<int, ParticleSystem.MinMaxGradient> _baseCustom1Color = new();
        private readonly Dictionary<int, ParticleSystem.MinMaxGradient> _baseCustom2Color = new();

        private readonly Dictionary<int, MaterialBaseline> _baseMaterialByRendererId = new();
        private readonly Dictionary<int, Color> _baseLightColorByLightId = new();

        private sealed class MaterialBaseline
        {
            public Color? Color;
            public Color? BaseColor;
            public Color? TintColor;
            public Color? EmissionColor;
        }

        public void SetItemData(global::ItemDrop.ItemData itemData)
        {
            _itemData = itemData;
        }

        private void Awake()
        {
            RebuildComponentCache();
            CacheBaselines();

            _componentCacheDirty = false;
            _baselineCacheDirty = false;

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
                _baselineCacheDirty = true;
            }

            if (_baselineCacheDirty)
            {
                CacheBaselines();
                _baselineCacheDirty = false;
            }

            VfxState state = ResolveState();

            ApplyEnabled(state.FlareEnabled);
            ApplyScale(state.FlareScale);
            ApplyHueShift(state.FlareHue);

            ApplyPlacement(
                state.FlareXOffset,
                state.FlareYOffset,
                state.FlareZOffset);

            RestartSystemsIfHueChanged(state.FlareHue);
        }

        private VfxState ResolveState()
        {
            if (_itemData != null)
            {
                if (VfxStateIO.TryRead(_itemData, out var itemState))
                    return itemState;

                VfxStateIO.EnsureInitializedFromConfig(_itemData);

                if (VfxStateIO.TryRead(_itemData, out itemState))
                    return itemState;
            }

            return VfxStateIO.FromConfig();
        }

        // Cache / baselines

        private void RebuildComponentCache()
        {
            _renderers = GetComponentsInChildren<Renderer>(true);
            _lights = GetComponentsInChildren<Light>(true);
            _systems = GetComponentsInChildren<ParticleSystem>(true);
        }

        private void CacheBaselines()
        {
            if (!_hasBaseScale)
            {
                _baseScale = transform.localScale;
                _hasBaseScale = true;
            }

            CacheBasePlacement();

            CacheParticleBaselines();
            CacheRendererBaselines();
            CacheLightBaselines();
        }

        private void CacheBasePlacement()
        {
            if (_hasBasePlacement)
                return;

            _baseLocalPosition = transform.localPosition;
            _baseLocalRotation = transform.localRotation;
            _hasBasePlacement = true;
        }

        private void CacheParticleBaselines()
        {
            if (_systems == null)
                return;

            foreach (ParticleSystem particleSystem in _systems)
            {
                if (particleSystem == null)
                    continue;

                int particleSystemId = particleSystem.GetInstanceID();

                if (!_baseMainStartColor.ContainsKey(particleSystemId))
                {
                    try
                    {
                        var main = particleSystem.main;
                        _baseMainStartColor[particleSystemId] = main.startColor;
                    }
                    catch { }
                }

                if (!_baseColorOverLifetimeEnabled.ContainsKey(particleSystemId) ||
                    !_baseColorOverLifetime.ContainsKey(particleSystemId))
                {
                    try
                    {
                        var colorOverLifetime = particleSystem.colorOverLifetime;
                        _baseColorOverLifetimeEnabled[particleSystemId] = colorOverLifetime.enabled;
                        _baseColorOverLifetime[particleSystemId] = colorOverLifetime.color;
                    }
                    catch { }
                }

                if (!_baseCustomDataEnabled.ContainsKey(particleSystemId))
                {
                    try
                    {
                        var customData = particleSystem.customData;

                        _baseCustomDataEnabled[particleSystemId] = customData.enabled;
                        _baseCustom1Mode[particleSystemId] =
                            customData.GetMode(ParticleSystemCustomData.Custom1);
                        _baseCustom2Mode[particleSystemId] =
                            customData.GetMode(ParticleSystemCustomData.Custom2);
                        _baseCustom1Color[particleSystemId] =
                            customData.GetColor(ParticleSystemCustomData.Custom1);
                        _baseCustom2Color[particleSystemId] =
                            customData.GetColor(ParticleSystemCustomData.Custom2);
                    }
                    catch { }
                }
            }
        }

        private void CacheRendererBaselines()
        {
            if (_renderers == null)
                return;

            foreach (Renderer renderer in _renderers)
            {
                if (renderer == null)
                    continue;

                Material material;
                try { material = renderer.material; } catch { continue; }

                if (material == null)
                    continue;

                int rendererId = renderer.GetInstanceID();
                if (_baseMaterialByRendererId.ContainsKey(rendererId))
                    continue;

                var materialBaseline = new MaterialBaseline();

                try
                {
                    if (material.HasProperty("_Color"))
                        materialBaseline.Color = material.GetColor("_Color");

                    if (material.HasProperty("_BaseColor"))
                        materialBaseline.BaseColor = material.GetColor("_BaseColor");

                    if (material.HasProperty("_TintColor"))
                        materialBaseline.TintColor = material.GetColor("_TintColor");

                    if (material.HasProperty("_EmissionColor"))
                        materialBaseline.EmissionColor = material.GetColor("_EmissionColor");
                }
                catch { }

                _baseMaterialByRendererId[rendererId] = materialBaseline;
            }
        }

        private void CacheLightBaselines()
        {
            if (_lights == null)
                return;

            foreach (Light light in _lights)
            {
                if (light == null)
                    continue;

                int lightId = light.GetInstanceID();
                if (_baseLightColorByLightId.ContainsKey(lightId))
                    continue;

                _baseLightColorByLightId[lightId] = light.color;
            }
        }

        // Config-facing apply path

        private void ApplyEnabled(bool enabled)
        {
            ApplyParticleSystemEnabledState(enabled);
            ApplyRendererEnabledState(enabled);
            ApplyLightEnabledState(enabled);
        }

        private void ApplyParticleSystemEnabledState(bool enabled)
        {
            if (_systems == null)
                return;

            foreach (ParticleSystem particleSystem in _systems)
            {
                if (particleSystem == null)
                    continue;

                try
                {
                    if (enabled)
                    {
                        if (!particleSystem.isPlaying)
                            particleSystem.Play(true);
                    }
                    else
                    {
                        if (particleSystem.isPlaying)
                            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    }
                }
                catch { }
            }
        }

        private void ApplyRendererEnabledState(bool enabled)
        {
            if (_renderers == null)
                return;

            foreach (Renderer renderer in _renderers)
            {
                if (renderer == null)
                    continue;

                try { renderer.enabled = enabled; } catch { }
            }
        }

        private void ApplyLightEnabledState(bool enabled)
        {
            if (_lights == null)
                return;

            foreach (Light light in _lights)
            {
                if (light == null)
                    continue;

                try { light.enabled = enabled; } catch { }
            }
        }

        private void ApplyScale(float scale)
        {
            if (!_hasBaseScale)
                return;

            transform.localScale =
                _baseScale * ClampScale(scale);
        }

        private void ApplyPlacement(
            float xOffset,
            float yOffset,
            float zOffset)
        {
            if (!_hasBasePlacement)
                return;

            float clampedXOffset = ClampOffset(xOffset);
            float clampedYOffset = ClampOffset(yOffset);
            float clampedZOffset = ClampOffset(zOffset);

            NadaEffectPlacement.ApplyLocalPlacement(
                transform,
                _baseLocalPosition,
                _baseLocalRotation,
                clampedXOffset,
                clampedYOffset,
                clampedZOffset,
                0f);
        }

        private void ApplyHueShift(float sliderValue)
        {
            float targetHue =
                NadaHueShiftUtility.SliderValueToTargetHue(sliderValue);

            ApplyParticleHueShift(targetHue);
            ApplyRendererHueShift(targetHue);
            ApplyLightHueShift(targetHue);
        }

        private void ApplyParticleHueShift(float targetHue)
        {
            if (_systems == null)
                return;

            foreach (ParticleSystem particleSystem in _systems)
            {
                if (particleSystem == null)
                    continue;

                int particleSystemId = particleSystem.GetInstanceID();

                try
                {
                    if (_baseMainStartColor.TryGetValue(particleSystemId, out var baseStartColor))
                    {
                        var main = particleSystem.main;
                        main.startColor =
                            NadaHueShiftUtility.RetintMinMaxGradientToHue(baseStartColor, targetHue);
                    }
                }
                catch { }

                try
                {
                    var colorOverLifetime = particleSystem.colorOverLifetime;

                    if (_baseColorOverLifetimeEnabled.TryGetValue(
                            particleSystemId,
                            out bool wasColorOverLifetimeEnabled))
                    {
                        colorOverLifetime.enabled = wasColorOverLifetimeEnabled;
                    }

                    if (_baseColorOverLifetime.TryGetValue(
                            particleSystemId,
                            out var baseColorOverLifetime))
                    {
                        colorOverLifetime.color =
                            NadaHueShiftUtility.RetintMinMaxGradientToHue(baseColorOverLifetime, targetHue);
                    }
                }
                catch { }

                try
                {
                    var customData = particleSystem.customData;

                    if (_baseCustomDataEnabled.TryGetValue(
                            particleSystemId,
                            out bool wasCustomDataEnabled))
                    {
                        customData.enabled = wasCustomDataEnabled;
                    }

                    if (_baseCustom1Mode.TryGetValue(particleSystemId, out var custom1Mode))
                        customData.SetMode(ParticleSystemCustomData.Custom1, custom1Mode);

                    if (_baseCustom2Mode.TryGetValue(particleSystemId, out var custom2Mode))
                        customData.SetMode(ParticleSystemCustomData.Custom2, custom2Mode);

                    if (_baseCustom1Color.TryGetValue(particleSystemId, out var custom1Color))
                    {
                        customData.SetColor(
                            ParticleSystemCustomData.Custom1,
                            NadaHueShiftUtility.RetintMinMaxGradientToHue(custom1Color, targetHue));
                    }

                    if (_baseCustom2Color.TryGetValue(particleSystemId, out var custom2Color))
                    {
                        customData.SetColor(
                            ParticleSystemCustomData.Custom2,
                            NadaHueShiftUtility.RetintMinMaxGradientToHue(custom2Color, targetHue));
                    }
                }
                catch { }
            }
        }

        private void ApplyRendererHueShift(float targetHue)
        {
            if (_renderers == null)
                return;

            foreach (Renderer renderer in _renderers)
            {
                if (renderer == null)
                    continue;

                int rendererId = renderer.GetInstanceID();

                if (!_baseMaterialByRendererId.TryGetValue(rendererId, out var materialBaseline) ||
                    materialBaseline == null)
                {
                    continue;
                }

                try
                {
                    Material material = renderer.material;
                    if (material == null)
                        continue;

                    if (materialBaseline.Color.HasValue)
                    {
                        Color color =
                            NadaHueShiftUtility.RetintColorToHue(materialBaseline.Color.Value, targetHue);

                        if (material.HasProperty("_Color"))
                            material.SetColor("_Color", color);

                        material.color = color;
                    }

                    if (materialBaseline.BaseColor.HasValue && material.HasProperty("_BaseColor"))
                    {
                        material.SetColor(
                            "_BaseColor",
                            NadaHueShiftUtility.RetintColorToHue(materialBaseline.BaseColor.Value, targetHue));
                    }

                    if (materialBaseline.TintColor.HasValue && material.HasProperty("_TintColor"))
                    {
                        material.SetColor(
                            "_TintColor",
                            NadaHueShiftUtility.RetintColorToHue(materialBaseline.TintColor.Value, targetHue));
                    }

                    if (materialBaseline.EmissionColor.HasValue && material.HasProperty("_EmissionColor"))
                    {
                        material.SetColor(
                            "_EmissionColor",
                            NadaHueShiftUtility.RetintColorToHue(materialBaseline.EmissionColor.Value, targetHue));
                    }
                }
                catch { }
            }
        }

        private void ApplyLightHueShift(float targetHue)
        {
            if (_lights == null)
                return;

            foreach (Light light in _lights)
            {
                if (light == null)
                    continue;

                int lightId = light.GetInstanceID();

                if (_baseLightColorByLightId.TryGetValue(lightId, out Color baseColor))
                {
                    try
                    {
                        light.color =
                            NadaHueShiftUtility.RetintColorToHue(baseColor, targetHue);
                    }
                    catch { }
                }
            }
        }

        private void RestartSystemsIfHueChanged(float hue)
        {
            bool hueChanged =
                !_hasLastHue ||
                Mathf.Abs(hue - _lastHue) > 0.0001f;

            if (!hueChanged)
                return;

            RestartSystems();

            _lastHue = hue;
            _hasLastHue = true;
        }

        private void RestartSystems()
        {
            if (_systems == null)
                return;

            foreach (ParticleSystem particleSystem in _systems)
            {
                if (particleSystem == null)
                    continue;

                try
                {
                    particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    particleSystem.Play(true);
                }
                catch { }
            }
        }

        // Helpers

        private static float ClampScale(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                return 1f;

            return Mathf.Clamp(
                value,
                PluginConfig.MinScaleMult,
                PluginConfig.MaxScaleMult);
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
    }
}