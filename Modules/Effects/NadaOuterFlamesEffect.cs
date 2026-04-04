using UnityEngine;
using System.Collections.Generic;
using NADA.VFX.Core.State;

namespace NADA.VFX.Modules.Effects
{
    internal sealed class NadaOuterFlamesEffect : MonoBehaviour
    {
        private global::ItemDrop.ItemData _itemData;

        private Renderer[] _renderers;
        private Light[] _lights;
        private ParticleSystem[] _systems;
        
        private readonly Dictionary<int, ParticleSystem.MinMaxGradient> _baseMainStartColor = new();
        private readonly Dictionary<int, bool> _baseColOverLifetimeEnabled = new();
        private readonly Dictionary<int, ParticleSystem.MinMaxGradient> _baseColOverLifetime = new();

        private readonly Dictionary<int, bool> _baseCustomDataEnabled = new();
        private readonly Dictionary<int, ParticleSystemCustomDataMode> _baseCustom1Mode = new();
        private readonly Dictionary<int, ParticleSystemCustomDataMode> _baseCustom2Mode = new();
        private readonly Dictionary<int, ParticleSystem.MinMaxGradient> _baseCustom1Color = new();
        private readonly Dictionary<int, ParticleSystem.MinMaxGradient> _baseCustom2Color = new();

        private readonly Dictionary<int, MatBaseline> _baseMat = new();
        private readonly Dictionary<int, Color> _baseLightColor = new();

        private readonly Dictionary<int, EmissionBaseline> _baseEmission = new();

        private readonly Dictionary<int, ParticleSystem.MinMaxCurve> _baseStartSize = new();
        private readonly Dictionary<int, ParticleSystem.MinMaxCurve> _baseStartLifetime = new();
        private readonly Dictionary<int, float> _baseSimulationSpeed = new();
        private readonly Dictionary<int, bool> _baseSizeOverLifetimeEnabled = new();
        private readonly Dictionary<int, ParticleSystem.MinMaxCurve> _baseSizeOverLifetime = new();

        private sealed class MatBaseline
        {
            public Color? Color;
            public Color? BaseColor;
            public Color? TintColor;
            public Color? Emission;
        }

        private sealed class EmissionBaseline
        {
            public bool Enabled;
            public ParticleSystem.MinMaxCurve RateOverTime;
            public ParticleSystem.MinMaxCurve RateOverDistance;
        }

        private bool _lastEnabled;
        private bool _hasLastEnabled;

        internal void SetItemData(global::ItemDrop.ItemData itemData)
        {
            _itemData = itemData;
        }

        private void Awake()
        {
            RebuildCaches();
            CacheBaselines();
            InvokeRepeating(nameof(TickApply), 0f, 0.05f);
        }

        private void OnDestroy()
        {
            try { CancelInvoke(nameof(TickApply)); } catch { }
        }

        private void TickApply()
        {
            RebuildCaches();
            CacheBaselines();

            VfxState state = ResolveState();
            bool enabled = state.OuterFlamesEnabled;

            ApplyEnabled(enabled);
            ApplyHueShift(state.OuterFlamesHue);
            ApplyScale(state.OuterFlamesScale);
            ApplyEnergy(state.OuterFlamesEnergy);

            if (!_hasLastEnabled || _lastEnabled != enabled)
            {
                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Outer Flames toggle applied on '{name}' (enabled={enabled}).");
                _lastEnabled = enabled;
                _hasLastEnabled = true;
            }
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

        private void RebuildCaches()
        {
            _renderers = GetComponentsInChildren<Renderer>(true);
            _lights = GetComponentsInChildren<Light>(true);
            _systems = GetComponentsInChildren<ParticleSystem>(true);
        }
        
        private void CacheBaselines()
        {
            CacheParticleBaselines();
            CacheEmissionBaselines();
            CacheRendererBaselines();
            CacheLightBaselines();
            CacheScaleBaselines();
        }
        
        private void CacheParticleBaselines()
        {
            if (_systems == null) return;

            foreach (var ps in _systems)
            {
                if (ps == null) continue;

                int id = ps.GetInstanceID();

                if (!_baseMainStartColor.ContainsKey(id))
                {
                    try
                    {
                        var main = ps.main;
                        _baseMainStartColor[id] = main.startColor;
                    }
                    catch { }
                }

                if (!_baseColOverLifetimeEnabled.ContainsKey(id) || !_baseColOverLifetime.ContainsKey(id))
                {
                    try
                    {
                        var col = ps.colorOverLifetime;
                        _baseColOverLifetimeEnabled[id] = col.enabled;
                        _baseColOverLifetime[id] = col.color;
                    }
                    catch { }
                }

                if (!_baseCustomDataEnabled.ContainsKey(id))
                {
                    try
                    {
                        var custom = ps.customData;
                        _baseCustomDataEnabled[id] = custom.enabled;
                        _baseCustom1Mode[id] = custom.GetMode(ParticleSystemCustomData.Custom1);
                        _baseCustom2Mode[id] = custom.GetMode(ParticleSystemCustomData.Custom2);
                        _baseCustom1Color[id] = custom.GetColor(ParticleSystemCustomData.Custom1);
                        _baseCustom2Color[id] = custom.GetColor(ParticleSystemCustomData.Custom2);
                    }
                    catch { }
                }
            }
        }
        
        private void CacheScaleBaselines()
        {
            if (_systems == null) return;

            foreach (var ps in _systems)
            {
                if (ps == null) continue;

                int id = ps.GetInstanceID();

                if (!_baseStartSize.ContainsKey(id))
                {
                    try
                    {
                        var main = ps.main;
                        _baseStartSize[id] = main.startSize;
                    }
                    catch { }
                }

                if (!_baseStartLifetime.ContainsKey(id))
                {
                    try
                    {
                        var main = ps.main;
                        _baseStartLifetime[id] = main.startLifetime;
                    }
                    catch { }
                }

                if (!_baseSimulationSpeed.ContainsKey(id))
                {
                    try
                    {
                        var main = ps.main;
                        _baseSimulationSpeed[id] = main.simulationSpeed;
                    }
                    catch { }
                }

                if (!_baseSizeOverLifetimeEnabled.ContainsKey(id) ||
                    !_baseSizeOverLifetime.ContainsKey(id))
                {
                    try
                    {
                        var sizeOverLifetime = ps.sizeOverLifetime;
                        _baseSizeOverLifetimeEnabled[id] = sizeOverLifetime.enabled;
                        _baseSizeOverLifetime[id] = sizeOverLifetime.size;
                    }
                    catch { }
                }
            }
        }
        
        private void CacheEmissionBaselines()
        {
            if (_systems == null) return;

            foreach (var ps in _systems)
            {
                if (ps == null) continue;

                int id = ps.GetInstanceID();
                if (_baseEmission.ContainsKey(id))
                    continue;

                try
                {
                    var emission = ps.emission;

                    _baseEmission[id] = new EmissionBaseline
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
            if (_renderers == null) return;

            foreach (var r in _renderers)
            {
                if (r == null) continue;

                Material m;
                try { m = r.material; } catch { continue; }
                if (m == null) continue;

                int rid = r.GetInstanceID();
                if (_baseMat.ContainsKey(rid)) continue;

                var mb = new MatBaseline();

                try
                {
                    if (m.HasProperty("_Color")) mb.Color = m.GetColor("_Color");
                    if (m.HasProperty("_BaseColor")) mb.BaseColor = m.GetColor("_BaseColor");
                    if (m.HasProperty("_TintColor")) mb.TintColor = m.GetColor("_TintColor");
                    if (m.HasProperty("_EmissionColor")) mb.Emission = m.GetColor("_EmissionColor");
                }
                catch { }

                _baseMat[rid] = mb;
            }
        }

        private void CacheLightBaselines()
        {
            if (_lights == null) return;

            foreach (var l in _lights)
            {
                if (l == null) continue;

                int id = l.GetInstanceID();
                if (_baseLightColor.ContainsKey(id)) continue;

                _baseLightColor[id] = l.color;
            }
        }

        private void ApplyEnabled(bool enabled)
        {
            if (_systems != null)
            {
                foreach (var ps in _systems)
                {
                    if (ps == null) continue;

                    try
                    {
                        if (enabled)
                        {
                            if (!ps.isPlaying)
                                ps.Play(true);
                        }
                        else
                        {
                            if (ps.isPlaying)
                                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                        }
                    }
                    catch { }
                }
            }

            if (_renderers != null)
            {
                foreach (var r in _renderers)
                {
                    if (r == null) continue;
                    try { r.enabled = enabled; } catch { }
                }
            }

            if (_lights != null)
            {
                foreach (var l in _lights)
                {
                    if (l == null) continue;
                    try { l.enabled = enabled; } catch { }
                }
            }
        }
        
        private void ApplyScale(float scale)
        {
            if (_systems == null) return;

            float outerMult = ClampScale(scale);

            foreach (var ps in _systems)
            {
                if (ps == null) continue;

                int id = ps.GetInstanceID();

                try
                {
                    var main = ps.main;

                    if (_baseStartSize.TryGetValue(id, out var baseStartSize))
                        main.startSize = ScaleMinMaxCurve(baseStartSize, outerMult);

                    if (_baseStartLifetime.TryGetValue(id, out var baseStartLifetime))
                        main.startLifetime = ScaleMinMaxCurve(baseStartLifetime, outerMult);

                    if (_baseSimulationSpeed.TryGetValue(id, out var baseSimulationSpeed))
                    {
                        float simulationSpeedMult = outerMult < 1f ? outerMult : 1f;
                        main.simulationSpeed = baseSimulationSpeed * simulationSpeedMult;
                    }
                }
                catch { }

                try
                {
                    var sizeOverLifetime = ps.sizeOverLifetime;

                    if (_baseSizeOverLifetimeEnabled.TryGetValue(id, out var wasEnabled))
                        sizeOverLifetime.enabled = wasEnabled;

                    if (wasEnabled && _baseSizeOverLifetime.TryGetValue(id, out var baseSize))
                        sizeOverLifetime.size = ScaleMinMaxCurve(baseSize, outerMult);
                }
                catch { }
            }
        }

        private static float ClampScale(float v)
        {
            if (float.IsNaN(v) || float.IsInfinity(v)) return 1f;
            return Mathf.Clamp(v, NADA.VFX.Core.Config.PluginConfig.MinScaleMult, NADA.VFX.Core.Config.PluginConfig.MaxScaleMult);
        }
        
        private void ApplyHueShift(float sliderValue)
        {
            float targetHue = NADA.VFX.Core.Color.NadaHueShiftUtility.SliderValueToTargetHue(sliderValue);

            if (_systems != null)
            {
                foreach (var ps in _systems)
                {
                    if (ps == null) continue;

                    int id = ps.GetInstanceID();

                    try
                    {
                        if (_baseMainStartColor.TryGetValue(id, out var baseStart))
                        {
                            var main = ps.main;
                            main.startColor = NADA.VFX.Core.Color.NadaHueShiftUtility.RetintMinMaxGradientToHue(baseStart, targetHue);
                        }
                    }
                    catch { }

                    try
                    {
                        var col = ps.colorOverLifetime;

                        if (_baseColOverLifetimeEnabled.TryGetValue(id, out var wasEnabled))
                            col.enabled = wasEnabled;

                        if (_baseColOverLifetime.TryGetValue(id, out var baseCol))
                            col.color = NADA.VFX.Core.Color.NadaHueShiftUtility.RetintMinMaxGradientToHue(baseCol, targetHue);
                    }
                    catch { }

                    try
                    {
                        var custom = ps.customData;

                        if (_baseCustomDataEnabled.TryGetValue(id, out var customEnabled))
                            custom.enabled = customEnabled;

                        if (_baseCustom1Mode.TryGetValue(id, out var custom1Mode))
                            custom.SetMode(ParticleSystemCustomData.Custom1, custom1Mode);

                        if (_baseCustom2Mode.TryGetValue(id, out var custom2Mode))
                            custom.SetMode(ParticleSystemCustomData.Custom2, custom2Mode);

                        if (_baseCustom1Color.TryGetValue(id, out var custom1Color))
                        {
                            custom.SetColor(
                                ParticleSystemCustomData.Custom1,
                                NADA.VFX.Core.Color.NadaHueShiftUtility.RetintMinMaxGradientToHue(custom1Color, targetHue));
                        }

                        if (_baseCustom2Color.TryGetValue(id, out var custom2Color))
                        {
                            custom.SetColor(
                                ParticleSystemCustomData.Custom2,
                                NADA.VFX.Core.Color.NadaHueShiftUtility.RetintMinMaxGradientToHue(custom2Color, targetHue));
                        }
                    }
                    catch { }
                }
            }

            if (_renderers != null)
            {
                foreach (var r in _renderers)
                {
                    if (r == null) continue;

                    int rid = r.GetInstanceID();
                    if (!_baseMat.TryGetValue(rid, out var mb) || mb == null)
                        continue;

                    try
                    {
                        var m = r.material;
                        if (m == null) continue;

                        if (mb.Color.HasValue)
                        {
                            var c = NADA.VFX.Core.Color.NadaHueShiftUtility.RetintColorToHue(mb.Color.Value, targetHue);
                            if (m.HasProperty("_Color")) m.SetColor("_Color", c);
                            m.color = c;
                        }

                        if (mb.BaseColor.HasValue && m.HasProperty("_BaseColor"))
                            m.SetColor("_BaseColor", NADA.VFX.Core.Color.NadaHueShiftUtility.RetintColorToHue(mb.BaseColor.Value, targetHue));

                        if (mb.TintColor.HasValue && m.HasProperty("_TintColor"))
                            m.SetColor("_TintColor", NADA.VFX.Core.Color.NadaHueShiftUtility.RetintColorToHue(mb.TintColor.Value, targetHue));

                        if (mb.Emission.HasValue && m.HasProperty("_EmissionColor"))
                            m.SetColor("_EmissionColor", NADA.VFX.Core.Color.NadaHueShiftUtility.RetintColorToHue(mb.Emission.Value, targetHue));
                    }
                    catch { }
                }
            }

            if (_lights != null)
            {
                foreach (var l in _lights)
                {
                    if (l == null) continue;

                    int id = l.GetInstanceID();
                    if (_baseLightColor.TryGetValue(id, out var baseColor))
                    {
                        try { l.color = NADA.VFX.Core.Color.NadaHueShiftUtility.RetintColorToHue(baseColor, targetHue); } catch { }
                    }
                }
            }
        }
        
        private void ApplyEnergy(float energy)
        {
            if (_systems == null) return;

            float t = Mathf.Clamp01(energy);

            foreach (var ps in _systems)
            {
                if (ps == null) continue;

                int id = ps.GetInstanceID();
                if (!_baseEmission.TryGetValue(id, out var baseline) || baseline == null)
                    continue;

                try
                {
                    var emission = ps.emission;

                    emission.enabled = baseline.Enabled;
                    if (!baseline.Enabled)
                        continue;

                    float emissionMult = Mathf.Lerp(1f, 10f, t);
                    var outerBaseRateOverTime = OverrideConstantBaseline(baseline.RateOverTime, 10f);

                    emission.rateOverTime = ScaleMinMaxCurve(outerBaseRateOverTime, emissionMult);
                    emission.rateOverDistance = ScaleMinMaxCurve(baseline.RateOverDistance, emissionMult);
                }
                catch { }
            }
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
                        source.constantMax * multiplier
                    );

                case ParticleSystemCurveMode.Curve:
                    return new ParticleSystem.MinMaxCurve(
                        source.curveMultiplier * multiplier,
                        source.curve
                    );

                case ParticleSystemCurveMode.TwoCurves:
                    return new ParticleSystem.MinMaxCurve(
                        source.curveMultiplier * multiplier,
                        source.curveMin,
                        source.curveMax
                    );

                default:
                    return source;
            }
        }
    }
}