using System.Collections.Generic;
using NADA.VFX.Core.Config;
using NADA.VFX.Core.State;
using NADA.VFX.Core.Visuals;
using NADA.VFX.Runtime.Binding;
using NADA.VFX.Runtime.Structure;
using UnityEngine;

namespace NADA.VFX.Modules.Effects
{
    internal sealed class NadaOuterFlamesEffect : MonoBehaviour, INadaItemDataReceiver
    {
        private const float OuterFlamesVisualScaleMultiplier = 1.25f;

        private global::ItemDrop.ItemData _itemData;

        private Renderer[] _renderers;
        private Light[] _lights;
        private ParticleSystem[] _systems;

        private bool _componentCacheDirty = true;
        private bool _baselineCacheDirty = true;

        private Vector3 _baseLocalPosition;
        private Quaternion _baseLocalRotation;
        private bool _hasBasePlacement;

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

        private readonly Dictionary<int, EmissionBaseline> _baseEmissionByParticleSystemId = new();

        private readonly Dictionary<int, ParticleSystem.MinMaxCurve> _baseStartSize = new();
        private readonly Dictionary<int, ParticleSystem.MinMaxCurve> _baseStartLifetime = new();
        private readonly Dictionary<int, float> _baseSimulationSpeed = new();
        private readonly Dictionary<int, bool> _baseSizeOverLifetimeEnabled = new();
        private readonly Dictionary<int, ParticleSystem.MinMaxCurve> _baseSizeOverLifetime = new();

        private readonly Dictionary<int, Vector3> _baseShapeScale = new();
        private readonly Dictionary<int, Vector3> _baseShapePosition = new();

        private sealed class MaterialBaseline
        {
            public Color? Color;
            public Color? BaseColor;
            public Color? TintColor;
            public Color? EmissionColor;
        }

        private sealed class EmissionBaseline
        {
            public bool Enabled;
            public ParticleSystem.MinMaxCurve RateOverTime;
            public ParticleSystem.MinMaxCurve RateOverDistance;
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

            ApplyEnabled(state.OuterFlamesEnabled);
            ApplyEnergy(state.OuterFlamesEnergy);
            ApplyHueShift(state.OuterFlamesHue, state.OuterFlamesLuminance);
            ApplyScale(state.OuterFlamesScale);
            ApplyLength(state.OuterFlamesLength);
            ApplyLifetime(state.OuterFlamesLifetime);

            ApplyPlacement(
                state.OuterFlamesXOffset,
                state.OuterFlamesYOffset,
                state.OuterFlamesZOffset,
                state.OuterFlamesXRotation,
                state.OuterFlamesYRotation,
                state.OuterFlamesZRotation);
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

        private void RebuildComponentCache()
        {
            _renderers = GetComponentsInChildren<Renderer>(true);
            _lights = GetComponentsInChildren<Light>(true);
            _systems = GetComponentsInChildren<ParticleSystem>(true);
        }

        private void CacheBaselines()
        {
            CacheBasePlacement();
            CacheParticleColorBaselines();
            CacheParticleScaleBaselines();
            CacheEmissionBaselines();
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

        private void CacheParticleColorBaselines()
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

        private void CacheParticleScaleBaselines()
        {
            if (_systems == null)
                return;

            foreach (ParticleSystem particleSystem in _systems)
            {
                if (particleSystem == null)
                    continue;

                int particleSystemId = particleSystem.GetInstanceID();

                if (!_baseStartSize.ContainsKey(particleSystemId))
                {
                    try
                    {
                        var main = particleSystem.main;
                        _baseStartSize[particleSystemId] = main.startSize;
                    }
                    catch { }
                }

                if (!_baseStartLifetime.ContainsKey(particleSystemId))
                {
                    try
                    {
                        var main = particleSystem.main;
                        _baseStartLifetime[particleSystemId] = main.startLifetime;
                    }
                    catch { }
                }

                if (!_baseSimulationSpeed.ContainsKey(particleSystemId))
                {
                    try
                    {
                        var main = particleSystem.main;
                        _baseSimulationSpeed[particleSystemId] = main.simulationSpeed;
                    }
                    catch { }
                }

                if (!_baseSizeOverLifetimeEnabled.ContainsKey(particleSystemId) ||
                    !_baseSizeOverLifetime.ContainsKey(particleSystemId))
                {
                    try
                    {
                        var sizeOverLifetime = particleSystem.sizeOverLifetime;
                        _baseSizeOverLifetimeEnabled[particleSystemId] = sizeOverLifetime.enabled;
                        _baseSizeOverLifetime[particleSystemId] = sizeOverLifetime.size;
                    }
                    catch { }
                }

                if (!_baseShapeScale.ContainsKey(particleSystemId))
                {
                    try
                    {
                        var shape = particleSystem.shape;
                        _baseShapeScale[particleSystemId] = shape.scale;
                        _baseShapePosition[particleSystemId] = shape.position;
                    }
                    catch { }
                }
            }
        }

        private void CacheEmissionBaselines()
        {
            if (_systems == null)
                return;

            foreach (ParticleSystem particleSystem in _systems)
            {
                if (particleSystem == null)
                    continue;

                int particleSystemId = particleSystem.GetInstanceID();
                if (_baseEmissionByParticleSystemId.ContainsKey(particleSystemId))
                    continue;

                try
                {
                    var emission = particleSystem.emission;

                    _baseEmissionByParticleSystemId[particleSystemId] = new EmissionBaseline
                    {
                        Enabled = emission.enabled,
                        RateOverTime = emission.rateOverTime,
                        RateOverDistance = emission.rateOverDistance
                    };
                }
                catch { }
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
            if (_systems == null)
                return;

            float scaleMultiplier =
                ClampScale(scale) * OuterFlamesVisualScaleMultiplier;

            foreach (ParticleSystem particleSystem in _systems)
            {
                if (particleSystem == null)
                    continue;

                ApplyParticleScale(particleSystem, scaleMultiplier);
            }
        }

        private void ApplyParticleScale(
            ParticleSystem particleSystem,
            float scaleMultiplier)
        {
            int particleSystemId = particleSystem.GetInstanceID();

            try
            {
                var main = particleSystem.main;

                if (_baseStartSize.TryGetValue(particleSystemId, out var baseStartSize))
                    main.startSize = ScaleMinMaxCurve(baseStartSize, scaleMultiplier);

                if (_baseSimulationSpeed.TryGetValue(particleSystemId, out float baseSimulationSpeed))
                {
                    float simulationSpeedMultiplier =
                        scaleMultiplier < 1f ? scaleMultiplier : 1f;

                    main.simulationSpeed =
                        baseSimulationSpeed * simulationSpeedMultiplier;
                }
            }
            catch { }

            try
            {
                var sizeOverLifetime = particleSystem.sizeOverLifetime;

                if (_baseSizeOverLifetimeEnabled.TryGetValue(
                        particleSystemId,
                        out bool wasSizeOverLifetimeEnabled))
                {
                    sizeOverLifetime.enabled = wasSizeOverLifetimeEnabled;

                    if (wasSizeOverLifetimeEnabled &&
                        _baseSizeOverLifetime.TryGetValue(particleSystemId, out var baseSize))
                    {
                        sizeOverLifetime.size =
                            ScaleMinMaxCurve(baseSize, scaleMultiplier);
                    }
                }
            }
            catch { }
        }

        private void ApplyLength(float length)
        {
            if (_systems == null)
                return;

            float clampedLength = Mathf.Clamp(
                length,
                PluginConfig.MinFlameLength,
                PluginConfig.MaxFlameLength);

            foreach (ParticleSystem particleSystem in _systems)
            {
                if (particleSystem == null)
                    continue;

                ApplyParticleLength(
                    particleSystem,
                    clampedLength);
            }
        }

        private void ApplyParticleLength(
            ParticleSystem particleSystem,
            float clampedLength)
        {
            int particleSystemId = particleSystem.GetInstanceID();

            if (!_baseShapeScale.TryGetValue(particleSystemId, out Vector3 baseScale) ||
                !_baseShapePosition.TryGetValue(particleSystemId, out Vector3 basePosition))
            {
                return;
            }

            try
            {
                var shape = particleSystem.shape;

                Vector3 nextScale = baseScale;
                nextScale.z = baseScale.z * clampedLength;

                Vector3 nextPosition = basePosition;
                nextPosition.z =
                    basePosition.z +
                    ((baseScale.z - nextScale.z) * 0.5f);

                shape.scale = nextScale;
                shape.position = nextPosition;
            }
            catch { }
        }

        private void ApplyLifetime(float lifetime)
        {
            if (_systems == null)
                return;

            float clampedLifetime = Mathf.Clamp(
                lifetime,
                PluginConfig.MinLifetime,
                PluginConfig.MaxLifetime);

            foreach (ParticleSystem particleSystem in _systems)
            {
                if (particleSystem == null)
                    continue;

                int particleSystemId = particleSystem.GetInstanceID();

                if (!_baseStartLifetime.TryGetValue(
                        particleSystemId,
                        out var baseLifetime))
                    continue;

                try
                {
                    var main = particleSystem.main;
                    main.startLifetime = ScaleMinMaxCurve(baseLifetime, clampedLifetime);
                }
                catch { }
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

        private void ApplyHueShift(float sliderValue, float luminanceMultiplier)
        {
            float targetHue =
                NadaHueShiftUtility.SliderValueToTargetHue(sliderValue);

            float rendererLuminanceMultiplier = Mathf.Clamp(
                luminanceMultiplier,
                PluginConfig.MinLuminance,
                PluginConfig.MaxLuminance);

            ApplyParticleHueShift(targetHue, rendererLuminanceMultiplier);
            ApplyRendererHueShift(targetHue, rendererLuminanceMultiplier);
            ApplyLightHueShift(targetHue, rendererLuminanceMultiplier);
        }

        private void ApplyParticleHueShift(float targetHue, float luminanceMultiplier)
        {
            if (_systems == null)
                return;

            float particleLuminance =
                NadaLuminanceUtility.RemapParticleLuminance(luminanceMultiplier);

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
                            NadaLuminanceUtility.ApplyToGradient(
                                NadaHueShiftUtility.RetintMinMaxGradientToHue(baseStartColor, targetHue),
                                particleLuminance);
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
                            NadaLuminanceUtility.ApplyToGradient(
                                NadaHueShiftUtility.RetintMinMaxGradientToHue(baseColorOverLifetime, targetHue),
                                particleLuminance);
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
                            NadaLuminanceUtility.ApplyToGradient(
                                NadaHueShiftUtility.RetintMinMaxGradientToHue(custom1Color, targetHue),
                                particleLuminance));
                    }

                    if (_baseCustom2Color.TryGetValue(particleSystemId, out var custom2Color))
                    {
                        customData.SetColor(
                            ParticleSystemCustomData.Custom2,
                            NadaLuminanceUtility.ApplyToGradient(
                                NadaHueShiftUtility.RetintMinMaxGradientToHue(custom2Color, targetHue),
                                particleLuminance));
                    }
                }
                catch { }
            }
        }

        private void ApplyRendererHueShift(float targetHue, float luminanceMultiplier)
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
                            ScaleColorRgb(
                                NadaHueShiftUtility.RetintColorToHue(
                                    materialBaseline.Color.Value,
                                    targetHue),
                                luminanceMultiplier);

                        if (material.HasProperty("_Color"))
                            material.SetColor("_Color", color);

                        material.color = color;
                    }

                    if (materialBaseline.BaseColor.HasValue && material.HasProperty("_BaseColor"))
                    {
                        material.SetColor(
                            "_BaseColor",
                            ScaleColorRgb(
                                NadaHueShiftUtility.RetintColorToHue(
                                    materialBaseline.BaseColor.Value,
                                    targetHue),
                                luminanceMultiplier));
                    }

                    if (materialBaseline.TintColor.HasValue && material.HasProperty("_TintColor"))
                    {
                        material.SetColor(
                            "_TintColor",
                            ScaleColorRgb(
                                NadaHueShiftUtility.RetintColorToHue(
                                    materialBaseline.TintColor.Value,
                                    targetHue),
                                luminanceMultiplier));
                    }

                    if (materialBaseline.EmissionColor.HasValue && material.HasProperty("_EmissionColor"))
                    {
                        material.SetColor(
                            "_EmissionColor",
                            ScaleColorRgb(
                                NadaHueShiftUtility.RetintColorToHue(
                                    materialBaseline.EmissionColor.Value,
                                    targetHue),
                                luminanceMultiplier));
                    }
                }
                catch { }
            }
        }

        private void ApplyLightHueShift(float targetHue, float luminanceMultiplier)
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
                            NadaLuminanceUtility.ApplyToColor(
                                NadaHueShiftUtility.RetintColorToHue(baseColor, targetHue),
                                luminanceMultiplier);
                    }
                    catch { }
                }
            }
        }

        private void ApplyEnergy(float energy)
        {
            if (_systems == null)
                return;

            float energyT = Mathf.Clamp01(energy);
            float emissionMultiplier = Mathf.Lerp(1f, 10f, energyT);

            foreach (ParticleSystem particleSystem in _systems)
            {
                if (particleSystem == null)
                    continue;

                ApplyParticleEnergy(
                    particleSystem,
                    emissionMultiplier);
            }
        }

        private void ApplyParticleEnergy(
            ParticleSystem particleSystem,
            float emissionMultiplier)
        {
            int particleSystemId = particleSystem.GetInstanceID();

            if (!_baseEmissionByParticleSystemId.TryGetValue(
                    particleSystemId,
                    out EmissionBaseline baseline) ||
                baseline == null)
            {
                return;
            }

            try
            {
                var emission = particleSystem.emission;

                emission.enabled = baseline.Enabled;
                if (!baseline.Enabled)
                    return;

                ParticleSystem.MinMaxCurve outerBaseRateOverTime =
                    OverrideConstantBaseline(baseline.RateOverTime, 10f);

                emission.rateOverTime =
                    ScaleMinMaxCurve(outerBaseRateOverTime, emissionMultiplier);

                emission.rateOverDistance =
                    ScaleMinMaxCurve(baseline.RateOverDistance, emissionMultiplier);
            }
            catch { }
        }

        private static Color ScaleColorRgb(Color color, float multiplier)
        {
            color.r *= multiplier;
            color.g *= multiplier;
            color.b *= multiplier;
            return color;
        }

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

        private static float ClampRotation(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                return PluginConfig.DefaultEffectRotation;

            return Mathf.Clamp(
                value,
                PluginConfig.MinEffectRotation,
                PluginConfig.MaxEffectRotation);
        }

        private static ParticleSystem.MinMaxCurve OverrideConstantBaseline(
            ParticleSystem.MinMaxCurve source,
            float constantValue)
        {
            switch (source.mode)
            {
                case ParticleSystemCurveMode.Constant:
                    return new ParticleSystem.MinMaxCurve(constantValue);

                case ParticleSystemCurveMode.TwoConstants:
                    return new ParticleSystem.MinMaxCurve(constantValue, constantValue);

                default:
                    return source;
            }
        }

        private static ParticleSystem.MinMaxCurve ScaleMinMaxCurve(
            ParticleSystem.MinMaxCurve source,
            float multiplier)
        {
            switch (source.mode)
            {
                case ParticleSystemCurveMode.Constant:
                    return new ParticleSystem.MinMaxCurve(source.constant * multiplier);

                case ParticleSystemCurveMode.TwoConstants:
                    return new ParticleSystem.MinMaxCurve(
                        source.constantMin * multiplier,
                        source.constantMax * multiplier);

                case ParticleSystemCurveMode.Curve:
                    return new ParticleSystem.MinMaxCurve(
                        source.curveMultiplier * multiplier,
                        source.curve);

                case ParticleSystemCurveMode.TwoCurves:
                    return new ParticleSystem.MinMaxCurve(
                        source.curveMultiplier * multiplier,
                        source.curveMin,
                        source.curveMax);

                default:
                    return source;
            }
        }
    }
}