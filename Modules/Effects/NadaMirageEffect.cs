using System.Collections.Generic;
using NADA.VFX.Core.Color;
using NADA.VFX.Core.Config;
using NADA.VFX.Core.State;
using UnityEngine;

namespace NADA.VFX.Modules.Effects
{
    internal sealed class NadaMirageEffect : MonoBehaviour
    {
        // More scaffolding repeating the same process as I did for Sparks.
        private bool _loggedUnexpectedHost;
        private bool _loggedDiscovery;
        private int _discoveryAttempts;
        
        private global::ItemDrop.ItemData _itemData;

        private Transform _root;
        private Renderer[] _renderers;
        private Light[] _lights;
        private ParticleSystem[] _systems;

        private Vector3 _baseScale;
        private bool _hasBaseScale;

        private readonly Dictionary<int, ParticleSystem.MinMaxGradient> _baseMainStartColor = new();
        private readonly Dictionary<int, bool> _baseColOverLifetimeEnabled = new();
        private readonly Dictionary<int, ParticleSystem.MinMaxGradient> _baseColOverLifetime = new();

        private readonly Dictionary<int, bool> _baseCustomDataEnabled = new();
        private readonly Dictionary<int, ParticleSystemCustomDataMode> _baseCustom1Mode = new();
        private readonly Dictionary<int, ParticleSystemCustomDataMode> _baseCustom2Mode = new();
        private readonly Dictionary<int, ParticleSystem.MinMaxGradient> _baseCustom1Color = new();
        private readonly Dictionary<int, ParticleSystem.MinMaxGradient> _baseCustom2Color = new();

        private readonly Dictionary<int, MatBaseline> _baseMat = new();
        private readonly Dictionary<int, UnityEngine.Color> _baseLightColor = new();

        private bool _lastEnabled;
        private bool _hasLastEnabled;

        private sealed class MatBaseline
        {
            public UnityEngine.Color? Color;
            public UnityEngine.Color? BaseColor;
            public UnityEngine.Color? TintColor;
            public UnityEngine.Color? Emission;
        }

        internal void SetItemData(global::ItemDrop.ItemData itemData)
        {
            _itemData = itemData;
        }

        private void Awake()
        {
            _root = transform;
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
            if (_root == null) return;

            RebuildCaches();
            CacheBaselines();
            TryLogDiscovery();
            ValidateExpectedHosting();

            VfxState state = ResolveState();

            bool enabled = state.MirageEnabled;

            ApplyEnabled(enabled);
            ApplyScale(state.MirageScale);
            ApplyHueShift(state.MirageHue);

            if (!_hasLastEnabled || _lastEnabled != enabled)
            {
                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Mirage toggle applied on '{name}' (enabled={enabled}).");
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
            if (_root != null && !_hasBaseScale)
            {
                _baseScale = _root.localScale;
                _hasBaseScale = true;
            }

            CacheParticleBaselines();
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

        private void ApplyScale(float scale)
        {
            if (!_hasBaseScale)
                return;

            float clamped = ClampScale(scale);

            // Mirage default should be half its authored/runtime baseline size.
            float effective = clamped * 0.5f;

            _root.localScale = _baseScale * effective;
        }

        private void ApplyHueShift(float sliderValue)
        {
            float targetHue = NadaHueShiftUtility.SliderValueToTargetHue(sliderValue);

            ApplyParticleHueShift(targetHue);
            ApplyRendererHueShift(targetHue);
            ApplyLightHueShift(targetHue);
        }

        private void ApplyParticleHueShift(float targetHue)
        {
            if (_systems == null) return;

            foreach (var ps in _systems)
            {
                if (ps == null) continue;

                int id = ps.GetInstanceID();

                try
                {
                    if (_baseMainStartColor.TryGetValue(id, out var baseStart))
                    {
                        var main = ps.main;
                        main.startColor =
                            NadaHueShiftUtility.RetintMinMaxGradientToHueAllowGrayscale(baseStart, targetHue);
                    }
                }
                catch { }

                try
                {
                    var col = ps.colorOverLifetime;

                    if (_baseColOverLifetimeEnabled.TryGetValue(id, out var wasEnabled))
                        col.enabled = wasEnabled;

                    if (_baseColOverLifetime.TryGetValue(id, out var baseCol))
                    {
                        col.color =
                            NadaHueShiftUtility.RetintMinMaxGradientToHueAllowGrayscale(baseCol, targetHue);
                    }
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
                            NadaHueShiftUtility.RetintMinMaxGradientToHueAllowGrayscale(custom1Color, targetHue)
                        );
                    }

                    if (_baseCustom2Color.TryGetValue(id, out var custom2Color))
                    {
                        custom.SetColor(
                            ParticleSystemCustomData.Custom2,
                            NadaHueShiftUtility.RetintMinMaxGradientToHueAllowGrayscale(custom2Color, targetHue)
                        );
                    }
                }
                catch { }
            }
        }

        private void ApplyRendererHueShift(float targetHue)
        {
            if (_renderers == null) return;

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
                        var c = NadaHueShiftUtility.RetintColorToHueAllowGrayscale(mb.Color.Value, targetHue);
                        if (m.HasProperty("_Color")) m.SetColor("_Color", c);
                        m.color = c;
                    }

                    if (mb.BaseColor.HasValue && m.HasProperty("_BaseColor"))
                    {
                        m.SetColor(
                            "_BaseColor",
                            NadaHueShiftUtility.RetintColorToHueAllowGrayscale(mb.BaseColor.Value, targetHue)
                        );
                    }

                    if (mb.TintColor.HasValue && m.HasProperty("_TintColor"))
                    {
                        m.SetColor(
                            "_TintColor",
                            NadaHueShiftUtility.RetintColorToHueAllowGrayscale(mb.TintColor.Value, targetHue)
                        );
                    }

                    if (mb.Emission.HasValue && m.HasProperty("_EmissionColor"))
                    {
                        m.SetColor(
                            "_EmissionColor",
                            NadaHueShiftUtility.RetintColorToHueAllowGrayscale(mb.Emission.Value, targetHue)
                        );
                    }
                }
                catch { }
            }
        }

        private void ApplyLightHueShift(float targetHue)
        {
            if (_lights == null) return;

            foreach (var l in _lights)
            {
                if (l == null) continue;

                int id = l.GetInstanceID();
                if (_baseLightColor.TryGetValue(id, out var baseColor))
                {
                    try
                    {
                        l.color = NadaHueShiftUtility.RetintColorToHueAllowGrayscale(baseColor, targetHue);
                    }
                    catch { }
                }
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

        private static float ClampScale(float v)
        {
            if (float.IsNaN(v) || float.IsInfinity(v)) return 1f;
            return Mathf.Clamp(v, PluginConfig.MinScaleMult, PluginConfig.MaxScaleMult);
        }
        
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
                (parentFollow != null || parentPath.Contains("Mirage Anchor"));

            if (!looksSettled && _discoveryAttempts < 10)
                return;

            _loggedDiscovery = true;

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [Mirage Discovery] root='{rootPath}', parent='{parentPath}', " +
                $"selfFollow={(selfFollow != null)}, childFollowCount={(childFollow?.Length ?? 0)}, " +
                $"parentFollow={(parentFollow != null ? GetSafePath(parentFollow.transform) : "<none>")}.");

            if (_systems == null || _systems.Length == 0)
            {
                Plugin.Log.LogInfo($"{Plugin.ModName}: [Mirage Discovery] no particle systems found.");
                return;
            }

            foreach (var ps in _systems)
            {
                if (ps == null) continue;

                try
                {
                    var main = ps.main;

                    Plugin.Log.LogInfo(
                        $"{Plugin.ModName}: [Mirage Discovery] ps='{ps.name}', path='{GetSafePath(ps.transform)}', " +
                        $"simSpace={main.simulationSpace}, playOnAwake={main.playOnAwake}, " +
                        $"loop={main.loop}, scalingMode={main.scalingMode}.");
                }
                catch
                {
                }
            }
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
                    $"{Plugin.ModName}: Mirage is not under the expected world branch. " +
                    $"Current path='{path}'. Mirage currently relies on world hosting/follow wiring.");
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
    }
}