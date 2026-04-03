using System.Collections.Generic;
using NADA.VFX.Core.Color;
using NADA.VFX.Core.State;
using UnityEngine;

namespace NADA.VFX.Modules.Effects
{
    internal sealed class NadaSparksEffect : MonoBehaviour
    {
        // Sparks currently behaves as a world-hosted effect because it's living on the world branch
        // and using world simulation space. I did this to just quickly give it the motion behavior I
        // wanted but my goal now is to give it its own explicit motion ownership (same with Mirage,
        // Flames, and Embers). To that end, some of the scaffolding here is transitional.
        private bool _loggedUnexpectedHost;
        private bool _loggedDiscovery;
        private int _discoveryAttempts;
        
        private global::ItemDrop.ItemData _itemData;

        private Renderer[] _renderers;
        private Light[] _lights;
        private ParticleSystem[] _systems;
        
        private readonly Dictionary<int, MatBaseline> _baseMat = new();
        private readonly Dictionary<int, UnityEngine.Color> _baseLightColor = new();

        private readonly Dictionary<int, bool> _baseColOverLifetimeEnabled = new();
        private readonly Dictionary<int, bool> _baseCustomDataEnabled = new();
        
        private readonly Dictionary<int, ParticleSystemCustomDataMode> _baseCustom1Mode = new();
        private readonly Dictionary<int, ParticleSystemCustomDataMode> _baseCustom2Mode = new();
        
        private readonly Dictionary<int, ParticleSystem.MinMaxGradient> _baseMainStartColor = new();
        private readonly Dictionary<int, ParticleSystem.MinMaxGradient> _baseColOverLifetime = new();
        private readonly Dictionary<int, ParticleSystem.MinMaxGradient> _baseCustom1Color = new();
        private readonly Dictionary<int, ParticleSystem.MinMaxGradient> _baseCustom2Color = new();
        
        private readonly Dictionary<int, EmissionBaseline> _baseEmission = new();
        private const float DefaultEnergyRateOverTime = 10f;
        private const float MaxEnergyRateOverTime = 100f;

        private bool _lastEnabled;
        private bool _hasLastEnabled;

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
            TryLogDiscovery();
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
            TryLogDiscovery();
            ValidateExpectedHosting();

            VfxState state = ResolveState();
            bool enabled = state.SparksEnabled;

            ApplyEnabled(enabled);
            ApplyHueShift(state.Sparks);
            ApplyEnergy(state.SparksEnergy);

            if (!_hasLastEnabled || _lastEnabled != enabled)
            {
                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Sparks toggle applied on '{name}' (enabled={enabled}).");
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

        private void ApplyHueShift(float sliderValue)
        {
            float targetHue = NadaHueShiftUtility.SliderValueToTargetHue(sliderValue);

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

            if (_lights != null)
            {
                foreach (var l in _lights)
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

        private void ApplyEnergy(float energy)
        {
            if (_systems == null) return;

            float t = Mathf.Clamp01(energy);
            float targetRateOverTime = Mathf.Lerp(DefaultEnergyRateOverTime, MaxEnergyRateOverTime, t);

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

                    emission.rateOverTime = OverrideConstantBaseline(baseline.RateOverTime, targetRateOverTime);
                    
                    emission.rateOverDistance = baseline.RateOverDistance;
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
        
        // Temporary discovery logging while Sparks is still world hosted.
        // Will remove it once Sparks has explicit working motion ownership.
        private void TryLogDiscovery()
        {
            if (_loggedDiscovery)
                return;

            _discoveryAttempts++;

            string rootPath = GetSafePath(transform);
            string parentPath = transform.parent != null ? GetSafePath(transform.parent) : "<no parent>";

            var selfFollow = GetComponent<NADA.VFX.Modules.Motion.NadaWorldFollowMotion>();
            var childFollow = GetComponentsInChildren<NADA.VFX.Modules.Motion.NadaWorldFollowMotion>(true);
            var parentFollow = GetComponentInParent<NADA.VFX.Modules.Motion.NadaWorldFollowMotion>();

            bool looksSettled =
                transform.parent != null &&
                (parentFollow != null || parentPath.Contains("Sparks Anchor"));

            if (!looksSettled && _discoveryAttempts < 10)
                return;

            _loggedDiscovery = true;

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [Sparks Discovery] root='{rootPath}', parent='{parentPath}', " +
                $"selfFollow={(selfFollow != null)}, childFollowCount={(childFollow?.Length ?? 0)}, " +
                $"parentFollow={(parentFollow != null ? GetSafePath(parentFollow.transform) : "<none>")}.");

            if (_systems == null || _systems.Length == 0)
            {
                Plugin.Log.LogInfo($"{Plugin.ModName}: [Sparks Discovery] no particle systems found.");
                return;
            }

            foreach (var ps in _systems)
            {
                if (ps == null) continue;

                try
                {
                    var main = ps.main;

                    Plugin.Log.LogInfo(
                        $"{Plugin.ModName}: [Sparks Discovery] ps='{ps.name}', path='{GetSafePath(ps.transform)}', " +
                        $"simSpace={main.simulationSpace}, playOnAwake={main.playOnAwake}, " +
                        $"loop={main.loop}, scalingMode={main.scalingMode}.");
                }
                catch
                {
                }
            }
        }

        private static string GetSafePath(Transform t)
        {
            if (t == null) return "<null>";

            var parts = new System.Collections.Generic.List<string>();
            while (t != null)
            {
                parts.Add(t.name);
                t = t.parent;
            }

            parts.Reverse();
            return string.Join("/", parts);
        }
        
        private void ValidateExpectedHosting()
        {
            if (_loggedUnexpectedHost)
                return;

            string path = GetSafePath(transform);

            bool isUnderWorldBranch = path.Contains("NADA VFX World");

            if (!isUnderWorldBranch)
            {
                _loggedUnexpectedHost = true;
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: Sparks is not under the expected world branch. " +
                    $"Current path='{path}'. Sparks currently relies on world hosting + World sim space.");
            }
        }
    }
}