using System.Collections.Generic;
using UnityEngine;
using UColor = UnityEngine.Color;
using NADA.VFX.Core.State;
using NADA.VFX.Runtime.Binding;
using NADA.VFX.Runtime.Persistence;
using NADA.VFX.Runtime;

namespace NADA.VFX.Modules.Properties
{
    internal sealed class NadaColorModule : MonoBehaviour
    {
        private global::ItemDrop.ItemData _itemData;
        private RigGroups _groups;
        private NadaOrbsTargets _orbTargets;

        private readonly Dictionary<int, ParticleSystem.MinMaxGradient> _baseMainStartColor = new();
        private readonly Dictionary<int, bool> _baseColOverLifetimeEnabled = new();
        private readonly Dictionary<int, ParticleSystem.MinMaxGradient> _baseColOverLifetime = new();

        private readonly Dictionary<int, bool> _baseCustomDataEnabled = new();
        private readonly Dictionary<int, ParticleSystemCustomDataMode> _baseCustom1Mode = new();
        private readonly Dictionary<int, ParticleSystemCustomDataMode> _baseCustom2Mode = new();
        private readonly Dictionary<int, ParticleSystem.MinMaxGradient> _baseCustom1Color = new();
        private readonly Dictionary<int, ParticleSystem.MinMaxGradient> _baseCustom2Color = new();

        private readonly Dictionary<int, MatBaseline> _baseMat = new();
        private readonly Dictionary<int, UColor> _baseLightColor = new();

        private float _lastFlareHueShift;
        private bool _hasLastFlareHueShift;

        private const float ClassicFlameHue = 0.08f;
        private const float ClassicOrbHue = 0.08f;

        private sealed class MatBaseline
        {
            public UColor? Color;
            public UColor? BaseColor;
            public UColor? TintColor;
            public UColor? Emission;
        }

        public void SetItemData(global::ItemDrop.ItemData itemData)
        {
            _itemData = itemData;
        }
        
        public void SetRigGroups(RigGroups groups)
        {
            if (groups != null)
                _groups = groups;
        }
        
        public void SetOrbTargets(NadaOrbsTargets targets)
        {
            if (targets != null && targets.IsValid)
                _orbTargets = targets;
        }

        private void Awake()
        {
            InvokeRepeating(nameof(TickApply), 0f, 0.05f);
        }

        private void OnDestroy()
        {
            try { CancelInvoke(nameof(TickApply)); } catch { }
        }

        private void TickApply()
        {
            NadaRigMaintenance.ApplyEffectEnabledStates(transform);

            if (_groups == null)
                return;

            CacheAllBaselines();
            CacheOrbBaselines();

            VfxState state = NadaWeaponStateResolver.Resolve(_itemData);

            ApplyHueShiftSlider(
                _groups.InnerSystems,
                _groups.InnerRenderers,
                _groups.InnerLights,
                state.Inner
            );

            ApplyHueShiftSlider(
                _groups.OuterSystems,
                _groups.OuterRenderers,
                _groups.OuterLights,
                state.Outer
            );

            ApplyHueShiftSlider(
                _groups.FlareSystems,
                _groups.FlareRenderers,
                _groups.FlareLights,
                state.Flare
            );

            ApplyOrbHueShift(state.OrbitalsOrbs);

            bool flareHueChanged =
                !_hasLastFlareHueShift ||
                Mathf.Abs(state.Flare - _lastFlareHueShift) > 0.0001f;

            if (flareHueChanged)
            {
                RestartSystems(_groups.FlareSystems);

                _lastFlareHueShift = state.Flare;
                _hasLastFlareHueShift = true;
            }
        }

        private void CacheAllBaselines()
        {
            CacheSystemBaselines(_groups.InnerSystems);
            CacheSystemBaselines(_groups.OuterSystems);
            CacheSystemBaselines(_groups.FlareSystems);

            CacheRendererBaselines(_groups.InnerRenderers);
            CacheRendererBaselines(_groups.OuterRenderers);
            CacheRendererBaselines(_groups.FlareRenderers);

            CacheLightBaselines(_groups.InnerLights);
            CacheLightBaselines(_groups.OuterLights);
            CacheLightBaselines(_groups.FlareLights);
        }
        
        private void CacheOrbBaselines()
        {
            if (_orbTargets == null || !_orbTargets.IsValid)
                return;

            CacheOrbRendererBaselines(_orbTargets.Renderers);
            CacheOrbLightBaselines(_orbTargets.Lights);
            CacheOrbSystemBaselines(_orbTargets.Systems);
        }

        private void CacheOrbRendererBaselines(Renderer[] renderers)
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

        private void CacheOrbLightBaselines(Light[] lights)
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

        private void CacheOrbSystemBaselines(ParticleSystem[] systems)
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
            }
        }

        private void CacheSystemBaselines(List<ParticleSystem> systems)
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

        private void CacheRendererBaselines(List<ParticleSystemRenderer> renderers)
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

        private void ApplyHueShiftSlider(
            List<ParticleSystem> systems,
            List<ParticleSystemRenderer> renderers,
            List<Light> lights,
            float sliderValue)
        {
            float targetHue = SliderValueToTargetHue(sliderValue);

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
                            main.startColor = RetintMinMaxGradientToHue(baseStart, targetHue);
                        }
                    }
                    catch { }

                    try
                    {
                        var col = ps.colorOverLifetime;

                        if (_baseColOverLifetimeEnabled.TryGetValue(id, out var wasEnabled))
                            col.enabled = wasEnabled;

                        if (_baseColOverLifetime.TryGetValue(id, out var baseCol))
                            col.color = RetintMinMaxGradientToHue(baseCol, targetHue);
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
                                RetintMinMaxGradientToHue(custom1Color, targetHue)
                            );
                        }

                        if (_baseCustom2Color.TryGetValue(id, out var custom2Color))
                        {
                            custom.SetColor(
                                ParticleSystemCustomData.Custom2,
                                RetintMinMaxGradientToHue(custom2Color, targetHue)
                            );
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
                            var c = RetintColorToHue(mb.Color.Value, targetHue);
                            if (m.HasProperty("_Color")) m.SetColor("_Color", c);
                            m.color = c;
                        }

                        if (mb.BaseColor.HasValue && m.HasProperty("_BaseColor"))
                            m.SetColor("_BaseColor", RetintColorToHue(mb.BaseColor.Value, targetHue));

                        if (mb.TintColor.HasValue && m.HasProperty("_TintColor"))
                            m.SetColor("_TintColor", RetintColorToHue(mb.TintColor.Value, targetHue));

                        if (mb.Emission.HasValue && m.HasProperty("_EmissionColor"))
                            m.SetColor("_EmissionColor", RetintColorToHue(mb.Emission.Value, targetHue));
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
                        try { l.color = RetintColorToHue(baseColor, targetHue); } catch { }
                    }
                }
            }
        }
        
        private void ApplyOrbHueShift(float sliderValue)
        {
            if (_orbTargets == null || !_orbTargets.IsValid)
                return;

            float targetHue = WrapHue01(ClassicOrbHue + sliderValue);

            if (_orbTargets.Renderers != null)
            {
                foreach (var r in _orbTargets.Renderers)
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
                            var c = RetintColorToHue(mb.Color.Value, targetHue);
                            if (m.HasProperty("_Color")) m.SetColor("_Color", c);
                            m.color = c;
                        }

                        if (mb.BaseColor.HasValue && m.HasProperty("_BaseColor"))
                            m.SetColor("_BaseColor", RetintColorToHue(mb.BaseColor.Value, targetHue));

                        if (mb.TintColor.HasValue && m.HasProperty("_TintColor"))
                            m.SetColor("_TintColor", RetintColorToHue(mb.TintColor.Value, targetHue));

                        if (mb.Emission.HasValue && m.HasProperty("_EmissionColor"))
                            m.SetColor("_EmissionColor", RetintColorToHue(mb.Emission.Value, targetHue));
                    }
                    catch { }
                }
            }

            if (_orbTargets.Lights != null)
            {
                foreach (var l in _orbTargets.Lights)
                {
                    if (l == null) continue;

                    int id = l.GetInstanceID();
                    if (_baseLightColor.TryGetValue(id, out var baseColor))
                    {
                        try { l.color = RetintColorToHue(baseColor, targetHue); } catch { }
                    }
                }
            }

            if (_orbTargets.Systems != null)
            {
                foreach (var ps in _orbTargets.Systems)
                {
                    if (ps == null) continue;

                    int id = ps.GetInstanceID();
                    if (!_baseMainStartColor.TryGetValue(id, out var baseStart))
                        continue;

                    try
                    {
                        var main = ps.main;
                        main.startColor = RetintMinMaxGradientToHue(baseStart, targetHue);
                    }
                    catch { }
                }
            }
        }

        private static float SliderValueToTargetHue(float sliderValue)
        {
            return WrapHue01(ClassicFlameHue + sliderValue);
        }

        private static float WrapHue01(float h)
        {
            h %= 1f;
            if (h < 0f) h += 1f;
            return h;
        }

        private static UColor RetintColorToHue(UColor input, float targetHue)
        {
            UColor.RGBToHSV(input, out _, out float s, out float v);

            if (s < 0.01f)
            {
                var unchanged = input;
                unchanged.a = input.a;
                return unchanged;
            }

            var tinted = UColor.HSVToRGB(targetHue, s, v, true);
            tinted.a = input.a;
            return tinted;
        }

        private static Gradient RetintGradientToHue(Gradient source, float targetHue)
        {
            if (source == null) return null;

            var srcColorKeys = source.colorKeys;
            var srcAlphaKeys = source.alphaKeys;

            var newColorKeys = new GradientColorKey[srcColorKeys.Length];
            for (int i = 0; i < srcColorKeys.Length; i++)
            {
                newColorKeys[i] = new GradientColorKey(
                    RetintColorToHue(srcColorKeys[i].color, targetHue),
                    srcColorKeys[i].time
                );
            }

            var newAlphaKeys = new GradientAlphaKey[srcAlphaKeys.Length];
            for (int i = 0; i < srcAlphaKeys.Length; i++)
            {
                newAlphaKeys[i] = new GradientAlphaKey(
                    srcAlphaKeys[i].alpha,
                    srcAlphaKeys[i].time
                );
            }

            var g = new Gradient();
            g.SetKeys(newColorKeys, newAlphaKeys);
            return g;
        }

        private static ParticleSystem.MinMaxGradient RetintMinMaxGradientToHue(
            ParticleSystem.MinMaxGradient source,
            float targetHue)
        {
            switch (source.mode)
            {
                case ParticleSystemGradientMode.Color:
                    return new ParticleSystem.MinMaxGradient(
                        RetintColorToHue(source.color, targetHue)
                    );

                case ParticleSystemGradientMode.TwoColors:
                    return new ParticleSystem.MinMaxGradient(
                        RetintColorToHue(source.colorMin, targetHue),
                        RetintColorToHue(source.colorMax, targetHue)
                    );

                case ParticleSystemGradientMode.Gradient:
                    return new ParticleSystem.MinMaxGradient(
                        RetintGradientToHue(source.gradient, targetHue)
                    );

                case ParticleSystemGradientMode.TwoGradients:
                    return new ParticleSystem.MinMaxGradient(
                        RetintGradientToHue(source.gradientMin, targetHue),
                        RetintGradientToHue(source.gradientMax, targetHue)
                    );

                case ParticleSystemGradientMode.RandomColor:
                    return new ParticleSystem.MinMaxGradient(
                        RetintGradientToHue(source.gradient, targetHue)
                    );

                default:
                    return source;
            }
        }
        
        private static void RestartSystems(List<ParticleSystem> systems)
        {
            if (systems == null) return;

            foreach (var ps in systems)
            {
                if (ps == null) continue;

                try
                {
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    ps.Play(true);
                }
                catch { }
            }
        }
    }
}