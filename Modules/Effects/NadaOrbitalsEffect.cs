using System.Collections.Generic;
using NADA.VFX.Core.Color;
using NADA.VFX.Core.State;
using UnityEngine;

namespace NADA.VFX.Modules.Effects
{
    internal sealed class NadaOrbitalsEffect : MonoBehaviour
    {
        private global::ItemDrop.ItemData _itemData;

        private Transform _flamesRoot;
        private Transform _flamesPoolRoot;
        private Transform _embersRoot;
        private Transform _embersPoolRoot;
        
        private bool _lastFlamesEnabled;
        private bool _hasLastFlamesEnabled;

        private bool _lastEmbersEnabled;
        private bool _hasLastEmbersEnabled;
        
        private const float DefaultEnergyRateOverTime = 10f;
        private const float MaxEnergyRateOverTime = 100f;

        private const float DefaultEmbersRateOverTime = 6f;
        private const float MaxEmbersRateOverTime = 60f;

        private readonly List<ParticleSystem> _flamesSystems = new();
        private readonly List<Renderer> _flamesRenderers = new();
        private readonly List<Light> _flamesLights = new();

        private readonly List<ParticleSystem> _embersSystems = new();
        private readonly List<Renderer> _embersRenderers = new();
        private readonly List<Light> _embersLights = new();

        private readonly Dictionary<int, ParticleSystem.MinMaxGradient> _baseMainStartColor = new();
        private readonly Dictionary<int, bool> _baseColOverLifetimeEnabled = new();
        private readonly Dictionary<int, ParticleSystem.MinMaxGradient> _baseColOverLifetime = new();

        private readonly Dictionary<int, bool> _baseCustomDataEnabled = new();
        private readonly Dictionary<int, ParticleSystemCustomDataMode> _baseCustom1Mode = new();
        private readonly Dictionary<int, ParticleSystemCustomDataMode> _baseCustom2Mode = new();
        private readonly Dictionary<int, ParticleSystem.MinMaxGradient> _baseCustom1Color = new();
        private readonly Dictionary<int, ParticleSystem.MinMaxGradient> _baseCustom2Color = new();

        private readonly Dictionary<int, EmissionBaseline> _baseEmission = new();

        private readonly Dictionary<int, MatBaseline> _baseMat = new();
        private readonly Dictionary<int, UnityEngine.Color> _baseLightColor = new();

        private sealed class MatBaseline
        {
            public UnityEngine.Color? Color;
            public UnityEngine.Color? BaseColor;
            public UnityEngine.Color? TintColor;
            public UnityEngine.Color? Emission;
        }

        private sealed class EmissionBaseline
        {
            public bool Enabled;
            public ParticleSystem.MinMaxCurve RateOverTime;
            public ParticleSystem.MinMaxCurve RateOverDistance;
        }

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

            // Live lead roots are controlled here.
            ApplyRootEnabled(_flamesRoot, state.OrbitalsFlamesEnabled);
            ApplyRootEnabled(_embersRoot, state.OrbitalsEmbersEnabled);
            
            if (state.OrbitalsFlamesEnabled)
            {
                foreach (var ps in _flamesSystems)
                {
                    if (ps != null && !ps.isPlaying)
                        ps.Play(true);
                }
            }

            if (state.OrbitalsEmbersEnabled)
            {
                foreach (var ps in _embersSystems)
                {
                    if (ps != null && !ps.isPlaying)
                        ps.Play(true);
                }
            }

            ApplyHueShift(_flamesSystems, _flamesRenderers, _flamesLights, state.OrbitalsFlamesHue);
            ApplyHueShift(_embersSystems, _embersRenderers, _embersLights, state.OrbitalsEmbersHue);

            ApplyEnergy(_flamesSystems, state.OrbitalsFlamesEnergy);
            ApplyEmbersEnergy(_embersSystems, state.OrbitalsEmbersEnergy);

            if (!_hasLastFlamesEnabled || _lastFlamesEnabled != state.OrbitalsFlamesEnabled)
            {
                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Orbitals Flames toggle applied on '{name}' (enabled={state.OrbitalsFlamesEnabled}).");
                _lastFlamesEnabled = state.OrbitalsFlamesEnabled;
                _hasLastFlamesEnabled = true;
            }

            if (!_hasLastEmbersEnabled || _lastEmbersEnabled != state.OrbitalsEmbersEnabled)
            {
                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Orbitals Embers toggle applied on '{name}' (enabled={state.OrbitalsEmbersEnabled}).");
                _lastEmbersEnabled = state.OrbitalsEmbersEnabled;
                _hasLastEmbersEnabled = true;
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
            _flamesSystems.Clear();
            _flamesRenderers.Clear();
            _flamesLights.Clear();

            _embersSystems.Clear();
            _embersRenderers.Clear();
            _embersLights.Clear();

            _flamesRoot = FindDirectChild(transform, Plugin.OrbitalsFlamesName);
            _embersRoot = FindDirectChild(transform, Plugin.OrbitalsEmbersName);

            _flamesPoolRoot = null;
            _embersPoolRoot = null;

            Transform effectsRoot = transform.parent;
            Transform orbitalsRigRoot = FindDirectChild(effectsRoot, Plugin.OrbitalsRigRootName);
            Transform orbitalsPoolsRoot = FindDirectChild(orbitalsRigRoot, Plugin.OrbitalsPoolsRootName);

            if (orbitalsPoolsRoot != null)
            {
                _flamesPoolRoot = FindDirectChild(orbitalsPoolsRoot, Plugin.OrbitalsFlamesPoolName);
                _embersPoolRoot = FindDirectChild(orbitalsPoolsRoot, Plugin.OrbitalsEmbersPoolName);
            }

            // Cache both live roots and pool roots for color/energy application.
            CacheGroupDedup(_flamesRoot, _flamesSystems, _flamesRenderers, _flamesLights);
            CacheGroupDedup(_flamesPoolRoot, _flamesSystems, _flamesRenderers, _flamesLights);

            CacheGroupDedup(_embersRoot, _embersSystems, _embersRenderers, _embersLights);
            CacheGroupDedup(_embersPoolRoot, _embersSystems, _embersRenderers, _embersLights);
        }

        private void CacheBaselines()
        {
            CacheParticleBaselines(_flamesSystems);
            CacheParticleBaselines(_embersSystems);

            CacheEmissionBaselines(_flamesSystems);
            CacheEmissionBaselines(_embersSystems);

            CacheRendererBaselines(_flamesRenderers);
            CacheRendererBaselines(_embersRenderers);

            CacheLightBaselines(_flamesLights);
            CacheLightBaselines(_embersLights);
        }

        private void CacheParticleBaselines(List<ParticleSystem> systems)
        {
            if (systems == null) return;

            foreach (var ps in systems)
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

        private void CacheEmissionBaselines(List<ParticleSystem> systems)
        {
            if (systems == null) return;

            foreach (var ps in systems)
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

        private void CacheRendererBaselines(List<Renderer> renderers)
        {
            if (renderers == null) return;

            foreach (var r in renderers)
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

        private void CacheLightBaselines(List<Light> lights)
        {
            if (lights == null) return;

            foreach (var l in lights)
            {
                if (l == null) continue;

                int id = l.GetInstanceID();
                if (_baseLightColor.ContainsKey(id)) continue;

                _baseLightColor[id] = l.color;
            }
        }

        private static void ApplyRootEnabled(Transform root, bool enabled)
        {
            if (root == null) return;

            var systems = root.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in systems)
            {
                if (ps == null) continue;

                try
                {
                    if (enabled)
                    {
                        if (!ps.gameObject.activeSelf)
                            ps.gameObject.SetActive(true);

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

            var renderers = root.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                if (r == null) continue;
                try { r.enabled = enabled; } catch { }
            }

            var lights = root.GetComponentsInChildren<Light>(true);
            foreach (var l in lights)
            {
                if (l == null) continue;
                try { l.enabled = enabled; } catch { }
            }
        }

        private void ApplyHueShift(
            List<ParticleSystem> systems,
            List<Renderer> renderers,
            List<Light> lights,
            float sliderValue)
        {
            float targetHue = NadaHueShiftUtility.SliderValueToTargetHue(sliderValue);

            if (systems != null)
            {
                foreach (var ps in systems)
                {
                    if (ps == null) continue;

                    int id = ps.GetInstanceID();

                    try
                    {
                        if (_baseMainStartColor.TryGetValue(id, out var baseStart))
                        {
                            var main = ps.main;
                            main.startColor = NadaHueShiftUtility.RetintMinMaxGradientToHue(baseStart, targetHue);
                        }
                    }
                    catch { }

                    try
                    {
                        var col = ps.colorOverLifetime;

                        if (_baseColOverLifetimeEnabled.TryGetValue(id, out var wasEnabled))
                            col.enabled = wasEnabled;

                        if (_baseColOverLifetime.TryGetValue(id, out var baseCol))
                            col.color = NadaHueShiftUtility.RetintMinMaxGradientToHue(baseCol, targetHue);
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
                                NadaHueShiftUtility.RetintMinMaxGradientToHue(custom1Color, targetHue));
                        }

                        if (_baseCustom2Color.TryGetValue(id, out var custom2Color))
                        {
                            custom.SetColor(
                                ParticleSystemCustomData.Custom2,
                                NadaHueShiftUtility.RetintMinMaxGradientToHue(custom2Color, targetHue));
                        }
                    }
                    catch { }
                }
            }

            if (renderers != null)
            {
                foreach (var r in renderers)
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
                            var c = NadaHueShiftUtility.RetintColorToHue(mb.Color.Value, targetHue);
                            if (m.HasProperty("_Color")) m.SetColor("_Color", c);
                            m.color = c;
                        }

                        if (mb.BaseColor.HasValue && m.HasProperty("_BaseColor"))
                            m.SetColor("_BaseColor", NadaHueShiftUtility.RetintColorToHue(mb.BaseColor.Value, targetHue));

                        if (mb.TintColor.HasValue && m.HasProperty("_TintColor"))
                            m.SetColor("_TintColor", NadaHueShiftUtility.RetintColorToHue(mb.TintColor.Value, targetHue));

                        if (mb.Emission.HasValue && m.HasProperty("_EmissionColor"))
                            m.SetColor("_EmissionColor", NadaHueShiftUtility.RetintColorToHue(mb.Emission.Value, targetHue));
                    }
                    catch { }
                }
            }

            if (lights != null)
            {
                foreach (var l in lights)
                {
                    if (l == null) continue;

                    int id = l.GetInstanceID();
                    if (_baseLightColor.TryGetValue(id, out var baseColor))
                    {
                        try { l.color = NadaHueShiftUtility.RetintColorToHue(baseColor, targetHue); } catch { }
                    }
                }
            }
        }

        private void ApplyEnergy(List<ParticleSystem> systems, float energy)
        {
            if (systems == null) return;

            float t = Mathf.Clamp01(energy);
            float targetRateOverTime = Mathf.Lerp(DefaultEnergyRateOverTime, MaxEnergyRateOverTime, t);

            foreach (var ps in systems)
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

                    emission.rateOverTime = OverrideConstantBaseline(baseline.RateOverTime, targetRateOverTime);
                    emission.rateOverDistance = baseline.RateOverDistance;
                }
                catch { }
            }
        }

        private void ApplyEmbersEnergy(List<ParticleSystem> systems, float energy)
        {
            if (systems == null) return;

            float t = Mathf.Clamp01(energy);
            float targetRateOverTime = Mathf.Lerp(DefaultEmbersRateOverTime, MaxEmbersRateOverTime, t);

            foreach (var ps in systems)
            {
                if (ps == null) continue;

                int id = ps.GetInstanceID();
                if (!_baseEmission.TryGetValue(id, out var baseline) || baseline == null)
                    continue;

                try
                {
                    var emission = ps.emission;
                    emission.enabled = true;
                    emission.rateOverTime = new ParticleSystem.MinMaxCurve(targetRateOverTime);
                    emission.rateOverDistance = baseline.RateOverDistance;

                    if (!ps.isPlaying)
                        ps.Play(true);
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

        private static void CacheGroupDedup(
            Transform root,
            List<ParticleSystem> systems,
            List<Renderer> renderers,
            List<Light> lights)
        {
            if (root == null) return;

            var seenPs = new HashSet<int>();
            foreach (var ps in systems)
            {
                if (ps != null)
                    seenPs.Add(ps.GetInstanceID());
            }

            var seenRenderers = new HashSet<int>();
            foreach (var r in renderers)
            {
                if (r != null)
                    seenRenderers.Add(r.GetInstanceID());
            }

            var seenLights = new HashSet<int>();
            foreach (var l in lights)
            {
                if (l != null)
                    seenLights.Add(l.GetInstanceID());
            }

            var rootSystems = root.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in rootSystems)
            {
                if (ps == null) continue;
                if (seenPs.Add(ps.GetInstanceID()))
                    systems.Add(ps);
            }

            var rootRenderers = root.GetComponentsInChildren<Renderer>(true);
            foreach (var r in rootRenderers)
            {
                if (r == null) continue;
                if (seenRenderers.Add(r.GetInstanceID()))
                    renderers.Add(r);
            }

            var rootLights = root.GetComponentsInChildren<Light>(true);
            foreach (var l in rootLights)
            {
                if (l == null) continue;
                if (seenLights.Add(l.GetInstanceID()))
                    lights.Add(l);
            }
        }

        private static Transform FindDirectChild(Transform parent, string name)
        {
            if (parent == null) return null;

            foreach (Transform child in parent)
            {
                if (child != null && child.name == name)
                    return child;
            }

            return null;
        }
    }
}