// File: Appliers/NadaScaleModule.cs
using System.Collections.Generic;
using NADA.VFX.Core.Config;
using NADA.VFX.Core.State;
using NADA.VFX.Runtime.Binding;
using UnityEngine;

namespace NADA.VFX.Modules.Properties
{
    internal sealed class NadaScaleModule : MonoBehaviour
    {
        private global::ItemDrop.ItemData _itemData;
        
        private Transform _effectsRoot;
        private Transform _outerFlamesRoot;
        private Transform _innerFlames;
        private Transform _flare;
        private RigGroups _groups;
        private NadaOrbsTargets _orbTargets;
        
        private bool _initialized;

        private Vector3 _baseFlareScale;
        private bool _hasBaseFlareScale;

        private Vector3 _baseInnerFlamesScale;
        private bool _hasBaseInnerFlamesScale;
        
        private Vector3 _orbBaseScale;
        private bool _hasOrbBaseScale;
        
        private bool _outerScaleBaselinesCached;
        
        private const float DefaultOrbBaselineScaleMult = 1.5f;

        private readonly Dictionary<int, ParticleSystem.MinMaxCurve> _baseOuterStartSize = new();
        private readonly Dictionary<int, ParticleSystem.MinMaxCurve> _baseOuterStartLifetime = new();
        private readonly Dictionary<int, float> _baseOuterSimulationSpeed = new();
        private readonly Dictionary<int, bool> _baseOuterSizeOverLifetimeEnabled = new();
        private readonly Dictionary<int, ParticleSystem.MinMaxCurve> _baseOuterSizeOverLifetime = new();

        private void Awake()
        {
            _effectsRoot = transform;
            InvokeRepeating(nameof(TickApply), 0f, 0.05f);
        }
        
        private void EnsureInitialized()
        {
            if (_initialized)
                return;

            if (_groups == null)
                return;

            _outerFlamesRoot = _groups.OuterRoot != null
                ? _groups.OuterRoot
                : NadaRigFinder.FindOuterFlames(_effectsRoot);

            _innerFlames = _groups.InnerRoot != null
                ? _groups.InnerRoot
                : NadaRigFinder.FindInnerFlames(_effectsRoot);

            _flare = _groups.FlareRoot != null
                ? _groups.FlareRoot
                : NadaRigFinder.FindFlare(_effectsRoot);

            CacheFlareBaseline();
            CacheInnerFlamesBaseline();

            _initialized = true;
        }

        private void OnDestroy()
        {
            try { CancelInvoke(nameof(TickApply)); } catch { }
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

        private void TickApply()
        {
            EnsureInitialized();

            if (_effectsRoot == null) return;

            if (_outerFlamesRoot == null)
                _outerFlamesRoot = NadaRigFinder.FindOuterFlames(_effectsRoot);

            if (_innerFlames == null)
                _innerFlames = NadaRigFinder.FindInnerFlames(_effectsRoot);

            if (_flare == null)
                _flare = NadaRigFinder.FindFlare(_effectsRoot);

            var state = ResolveState();

            ApplyTransformsAndScales();
            ApplyOrbsScale(state);
        }

        private void ApplyTransformsAndScales()
        {
            ApplyOuterScale();
            ApplyInnerScale();
            ApplyFlareScale();
        }
        
        private void ApplyFlareScale()
        {
            if (_flare == null)
                _flare = NadaRigFinder.FindFlare(_effectsRoot);

            if (_flare == null)
                return;

            if (!_hasBaseFlareScale)
            {
                _baseFlareScale = _flare.localScale;
                _hasBaseFlareScale = true;
            }

            float flareMult = ClampScale(PluginConfig.FlareScaleMult.Value);
            _flare.localScale = _baseFlareScale * flareMult;
        }

        private void ApplyInnerScale()
        {
            if (_innerFlames == null)
                _innerFlames = NadaRigFinder.FindInnerFlames(_effectsRoot);

            if (_innerFlames == null)
                return;

            if (!_hasBaseInnerFlamesScale)
            {
                _baseInnerFlamesScale = _innerFlames.localScale;
                _hasBaseInnerFlamesScale = true;
            }

            float innerMult = ClampScale(PluginConfig.InnerFlamesScaleMult.Value);
            _innerFlames.localScale = _baseInnerFlamesScale * innerMult;
        }
        
        private void ApplyOuterScale()
        {
            if (!_outerScaleBaselinesCached)
            {
                if (_groups == null || _groups.OuterSystems == null)
                    return;

                CacheOuterScaleBaselines();
                _outerScaleBaselinesCached = true;
            }
            
            if (_groups == null || _groups.OuterSystems == null)
                return;

            float outerMult = ClampScale(PluginConfig.OuterFlamesScaleMult.Value);

            foreach (var ps in _groups.OuterSystems)
            {
                if (ps == null) continue;

                int id = ps.GetInstanceID();

                try
                {
                    var main = ps.main;

                    if (_baseOuterStartSize.TryGetValue(id, out var baseStartSize))
                        main.startSize = ScaleMinMaxCurve(baseStartSize, outerMult);

                    if (_baseOuterStartLifetime.TryGetValue(id, out var baseStartLifetime))
                        main.startLifetime = ScaleMinMaxCurve(baseStartLifetime, outerMult);

                    if (_baseOuterSimulationSpeed.TryGetValue(id, out var baseSimulationSpeed))
                    {
                        float simulationSpeedMult = outerMult < 1f ? outerMult : 1f;
                        main.simulationSpeed = baseSimulationSpeed * simulationSpeedMult;
                    }
                }
                catch { }

                try
                {
                    var sizeOverLifetime = ps.sizeOverLifetime;

                    if (_baseOuterSizeOverLifetimeEnabled.TryGetValue(id, out var wasEnabled))
                        sizeOverLifetime.enabled = wasEnabled;

                    if (wasEnabled && _baseOuterSizeOverLifetime.TryGetValue(id, out var baseSize))
                        sizeOverLifetime.size = ScaleMinMaxCurve(baseSize, outerMult);
                }
                catch { }
            }
        }

        private void ApplyOrbsScale(VfxState state)
        {
            if (_orbTargets == null || !_orbTargets.IsValid)
                return;

            CacheOrbBaseline();

            float scaleMult = ClampOrbScale(state.OrbitalsOrbsScale);

            _orbTargets.Root.localScale =
                _orbBaseScale * (DefaultOrbBaselineScaleMult * scaleMult);

            if (_orbTargets.Renderers != null)
            {
                foreach (var r in _orbTargets.Renderers)
                {
                    if (r == null) continue;
                    try { r.enabled = state.OrbitalsOrbsEnabled; } catch { }
                }
            }
        }
        
        private static float ClampOrbScale(float v)
        {
            if (float.IsNaN(v) || float.IsInfinity(v)) return 1f;
            return Mathf.Clamp(v, PluginConfig.MinOrbScaleMult, PluginConfig.MaxOrbScaleMult);
        }

        private void CacheOuterScaleBaselines()
        {
            if (_groups == null || _groups.OuterSystems == null)
                return;

            foreach (var ps in _groups.OuterSystems)
            {
                if (ps == null) continue;

                int id = ps.GetInstanceID();

                if (!_baseOuterStartSize.ContainsKey(id))
                {
                    try
                    {
                        var main = ps.main;
                        _baseOuterStartSize[id] = main.startSize;
                    }
                    catch { }
                }

                if (!_baseOuterStartLifetime.ContainsKey(id))
                {
                    try
                    {
                        var main = ps.main;
                        _baseOuterStartLifetime[id] = main.startLifetime;
                    }
                    catch { }
                }

                if (!_baseOuterSimulationSpeed.ContainsKey(id))
                {
                    try
                    {
                        var main = ps.main;
                        _baseOuterSimulationSpeed[id] = main.simulationSpeed;
                    }
                    catch { }
                }

                if (!_baseOuterSizeOverLifetimeEnabled.ContainsKey(id) ||
                    !_baseOuterSizeOverLifetime.ContainsKey(id))
                {
                    try
                    {
                        var sizeOverLifetime = ps.sizeOverLifetime;
                        _baseOuterSizeOverLifetimeEnabled[id] = sizeOverLifetime.enabled;
                        _baseOuterSizeOverLifetime[id] = sizeOverLifetime.size;
                    }
                    catch { }
                }
            }
        }

        private void CacheFlareBaseline()
        {
            if (_flare == null) return;

            _baseFlareScale = _flare.localScale;
            _hasBaseFlareScale = true;
        }
        
        private void CacheInnerFlamesBaseline()
        {
            if (_innerFlames == null) return;

            _baseInnerFlamesScale = _innerFlames.localScale;
            _hasBaseInnerFlamesScale = true;
        }
        
        private void CacheOrbBaseline()
        {
            if (_orbTargets == null || !_orbTargets.IsValid)
                return;

            if (!_hasOrbBaseScale)
            {
                _orbBaseScale = _orbTargets.Root.localScale;
                _hasOrbBaseScale = true;
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

        private static float ClampScale(float v)
        {
            if (float.IsNaN(v) || float.IsInfinity(v)) return 1f;
            return Mathf.Clamp(v, PluginConfig.MinScaleMult, PluginConfig.MaxScaleMult);
        }
    }
}