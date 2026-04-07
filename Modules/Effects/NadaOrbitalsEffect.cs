using System.Collections.Generic;
using NADA.VFX.Core.Color;
using NADA.VFX.Core.State;
using NADA.VFX.Runtime.Binding;
using NADA.VFX.Runtime.Structure;
using UnityEngine;

namespace NADA.VFX.Modules.Effects
{
    internal sealed class NadaOrbitalsEffect : MonoBehaviour, INadaItemDataReceiver
    {
        private global::ItemDrop.ItemData _itemData;

        private Transform _orbsRootTransform;
        private Transform _localOrbsRootTransform;
        private Transform _orbsPoolRootTransform;
        private bool _lastOrbsEnabled;
        private bool _hasLastOrbsEnabled;
        private Vector3 _orbsBaseLocalScale;
        private bool _hasOrbsBaseLocalScale;
        private readonly Dictionary<int, Vector3> _orbsPoolBaseLocalScaleByTransformId = new();

        private Transform _flamesRootTransform;
        private Transform _flamesPoolRootTransform;
        private bool _lastFlamesEnabled;
        private bool _hasLastFlamesEnabled;

        private Transform _embersRootTransform;
        private Transform _embersPoolRootTransform;
        private bool _lastEmbersEnabled;
        private bool _hasLastEmbersEnabled;

        private const float DefaultOrbBaselineScaleMultiplier = 1.5f;

        private const float DefaultFlamesRateOverTime = 10f;
        private const float MaxFlamesRateOverTime = 100f;

        private const float DefaultEmbersRateOverTime = 6f;
        private const float MaxEmbersRateOverTime = 60f;

        private readonly List<ParticleSystem> _orbsParticleSystems = new();
        private readonly List<Renderer> _orbsRenderers = new();
        private readonly List<Light> _orbsLights = new();

        private readonly List<ParticleSystem> _flamesParticleSystems = new();
        private readonly List<Renderer> _flamesRenderers = new();
        private readonly List<Light> _flamesLights = new();

        private readonly List<ParticleSystem> _embersParticleSystems = new();
        private readonly List<Renderer> _embersRenderers = new();
        private readonly List<Light> _embersLights = new();

        private readonly Dictionary<int, ParticleSystem.MinMaxGradient> _baseMainStartColorByParticleSystemId = new();
        private readonly Dictionary<int, bool> _baseColorOverLifetimeEnabledByParticleSystemId = new();
        private readonly Dictionary<int, ParticleSystem.MinMaxGradient> _baseColorOverLifetimeByParticleSystemId = new();

        private readonly Dictionary<int, bool> _baseCustomDataEnabledByParticleSystemId = new();
        private readonly Dictionary<int, ParticleSystemCustomDataMode> _baseCustom1ModeByParticleSystemId = new();
        private readonly Dictionary<int, ParticleSystemCustomDataMode> _baseCustom2ModeByParticleSystemId = new();
        private readonly Dictionary<int, ParticleSystem.MinMaxGradient> _baseCustom1ColorByParticleSystemId = new();
        private readonly Dictionary<int, ParticleSystem.MinMaxGradient> _baseCustom2ColorByParticleSystemId = new();

        private readonly Dictionary<int, EmissionBaseline> _baseEmissionByParticleSystemId = new();
        private readonly Dictionary<int, MaterialBaseline> _baseMaterialByRendererId = new();
        private readonly Dictionary<int, Color> _baseLightColorByLightId = new();

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

        internal void SetLocalOrbsRootTransform(Transform localOrbsRootTransform)
        {
            if (localOrbsRootTransform != null)
                _localOrbsRootTransform = localOrbsRootTransform;
        }

        private void Awake()
        {
            RebuildOrbitalsComponentCaches();
            CacheModifierBaselines();
            InvokeRepeating(nameof(TickApply), 0f, 0.05f);
        }

        private void OnDestroy()
        {
            try { CancelInvoke(nameof(TickApply)); } catch { }
        }

        private void TickApply()
        {
            RebuildOrbitalsComponentCaches();
            CacheModifierBaselines();

            VfxState state = ResolveState();

            ApplyEffectRootEnabledState(_orbsRootTransform, state.OrbitalsOrbsEnabled);
            if (_orbsRootTransform != null)
                NadaRigTransforms.DisableRootVisualContent(_orbsRootTransform);

            ApplyEffectRootEnabledState(_flamesRootTransform, state.OrbitalsFlamesEnabled);
            ApplyEffectRootEnabledState(_embersRootTransform, state.OrbitalsEmbersEnabled);

            EnsureParticleSystemsPlayingIfEnabled(_orbsParticleSystems, state.OrbitalsOrbsEnabled);
            EnsureParticleSystemsPlayingIfEnabled(_flamesParticleSystems, state.OrbitalsFlamesEnabled);
            EnsureParticleSystemsPlayingIfEnabled(_embersParticleSystems, state.OrbitalsEmbersEnabled);

            ApplyOrbsScale(state.OrbitalsOrbsScale);
            ApplyHueShift(_orbsParticleSystems, _orbsRenderers, _orbsLights, state.OrbitalsOrbsHue);
            ApplyHueShift(_flamesParticleSystems, _flamesRenderers, _flamesLights, state.OrbitalsFlamesHue);
            ApplyHueShift(_embersParticleSystems, _embersRenderers, _embersLights, state.OrbitalsEmbersHue);

            ApplyFlamesEnergy(_flamesParticleSystems, state.OrbitalsFlamesEnergy);
            ApplyEmbersEnergy(_embersParticleSystems, state.OrbitalsEmbersEnergy);

            LogToggleStateIfChanged(
                "Orbitals Orbs",
                state.OrbitalsOrbsEnabled,
                ref _lastOrbsEnabled,
                ref _hasLastOrbsEnabled);

            LogToggleStateIfChanged(
                "Orbitals Flames",
                state.OrbitalsFlamesEnabled,
                ref _lastFlamesEnabled,
                ref _hasLastFlamesEnabled);

            LogToggleStateIfChanged(
                "Orbitals Embers",
                state.OrbitalsEmbersEnabled,
                ref _lastEmbersEnabled,
                ref _hasLastEmbersEnabled);
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

        private void RebuildOrbitalsComponentCaches()
        {
            _orbsParticleSystems.Clear();
            _orbsRenderers.Clear();
            _orbsLights.Clear();

            _flamesParticleSystems.Clear();
            _flamesRenderers.Clear();
            _flamesLights.Clear();

            _embersParticleSystems.Clear();
            _embersRenderers.Clear();
            _embersLights.Clear();

            _orbsRootTransform = _localOrbsRootTransform != null
                ? _localOrbsRootTransform
                : NadaRigPaths.FindDirectChild(transform, Plugin.OrbitalsOrbsName);

            _flamesRootTransform = NadaRigPaths.FindDirectChild(transform, Plugin.OrbitalsFlamesName);
            _embersRootTransform = NadaRigPaths.FindDirectChild(transform, Plugin.OrbitalsEmbersName);

            _orbsPoolRootTransform = null;
            _flamesPoolRootTransform = null;
            _embersPoolRootTransform = null;

            Transform orbitalsRigRootTransform =
                NadaRigPaths.FindDirectChild(transform, Plugin.OrbitalsRigRootName);

            Transform orbitalsPoolsRootTransform =
                NadaRigPaths.FindDirectChild(orbitalsRigRootTransform, Plugin.OrbitalsPoolsRootName);

            if (orbitalsPoolsRootTransform != null)
            {
                _orbsPoolRootTransform =
                    NadaRigPaths.FindDirectChild(orbitalsPoolsRootTransform, Plugin.OrbitalsOrbsPoolName);

                _flamesPoolRootTransform =
                    NadaRigPaths.FindDirectChild(orbitalsPoolsRootTransform, Plugin.OrbitalsFlamesPoolName);

                _embersPoolRootTransform =
                    NadaRigPaths.FindDirectChild(orbitalsPoolsRootTransform, Plugin.OrbitalsEmbersPoolName);
            }

            AppendUniqueGroupComponents(
                _orbsRootTransform,
                _orbsParticleSystems,
                _orbsRenderers,
                _orbsLights);

            AppendUniqueGroupComponents(
                _orbsPoolRootTransform,
                _orbsParticleSystems,
                _orbsRenderers,
                _orbsLights);

            AppendUniqueGroupComponents(
                _flamesRootTransform,
                _flamesParticleSystems,
                _flamesRenderers,
                _flamesLights);

            AppendUniqueGroupComponents(
                _flamesPoolRootTransform,
                _flamesParticleSystems,
                _flamesRenderers,
                _flamesLights);

            AppendUniqueGroupComponents(
                _embersRootTransform,
                _embersParticleSystems,
                _embersRenderers,
                _embersLights);

            AppendUniqueGroupComponents(
                _embersPoolRootTransform,
                _embersParticleSystems,
                _embersRenderers,
                _embersLights);
        }

        private void CacheModifierBaselines()
        {
            CacheParticleBaselines(_orbsParticleSystems);
            CacheRendererBaselines(_orbsRenderers);
            CacheLightBaselines(_orbsLights);
            CacheOrbsBaselineScale();

            CacheParticleBaselines(_flamesParticleSystems);
            CacheParticleBaselines(_embersParticleSystems);

            CacheEmissionBaselines(_flamesParticleSystems);
            CacheEmissionBaselines(_embersParticleSystems);

            CacheRendererBaselines(_flamesRenderers);
            CacheRendererBaselines(_embersRenderers);

            CacheLightBaselines(_flamesLights);
            CacheLightBaselines(_embersLights);
        }

        private void CacheParticleBaselines(List<ParticleSystem> particleSystems)
        {
            if (particleSystems == null)
                return;

            foreach (var particleSystem in particleSystems)
            {
                if (particleSystem == null)
                    continue;

                int particleSystemId = particleSystem.GetInstanceID();

                if (!_baseMainStartColorByParticleSystemId.ContainsKey(particleSystemId))
                {
                    try
                    {
                        var main = particleSystem.main;
                        _baseMainStartColorByParticleSystemId[particleSystemId] = main.startColor;
                    }
                    catch { }
                }

                if (!_baseColorOverLifetimeEnabledByParticleSystemId.ContainsKey(particleSystemId) ||
                    !_baseColorOverLifetimeByParticleSystemId.ContainsKey(particleSystemId))
                {
                    try
                    {
                        var colorOverLifetime = particleSystem.colorOverLifetime;
                        _baseColorOverLifetimeEnabledByParticleSystemId[particleSystemId] = colorOverLifetime.enabled;
                        _baseColorOverLifetimeByParticleSystemId[particleSystemId] = colorOverLifetime.color;
                    }
                    catch { }
                }

                if (!_baseCustomDataEnabledByParticleSystemId.ContainsKey(particleSystemId))
                {
                    try
                    {
                        var customData = particleSystem.customData;
                        _baseCustomDataEnabledByParticleSystemId[particleSystemId] = customData.enabled;
                        _baseCustom1ModeByParticleSystemId[particleSystemId] =
                            customData.GetMode(ParticleSystemCustomData.Custom1);
                        _baseCustom2ModeByParticleSystemId[particleSystemId] =
                            customData.GetMode(ParticleSystemCustomData.Custom2);
                        _baseCustom1ColorByParticleSystemId[particleSystemId] =
                            customData.GetColor(ParticleSystemCustomData.Custom1);
                        _baseCustom2ColorByParticleSystemId[particleSystemId] =
                            customData.GetColor(ParticleSystemCustomData.Custom2);
                    }
                    catch { }
                }
            }
        }

        private void CacheEmissionBaselines(List<ParticleSystem> particleSystems)
        {
            if (particleSystems == null)
                return;

            foreach (var particleSystem in particleSystems)
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

        private void CacheRendererBaselines(List<Renderer> renderers)
        {
            if (renderers == null)
                return;

            foreach (var renderer in renderers)
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

        private void CacheLightBaselines(List<Light> lights)
        {
            if (lights == null)
                return;

            foreach (var light in lights)
            {
                if (light == null)
                    continue;

                int lightId = light.GetInstanceID();
                if (_baseLightColorByLightId.ContainsKey(lightId))
                    continue;

                _baseLightColorByLightId[lightId] = light.color;
            }
        }

        private static void ApplyEffectRootEnabledState(
            Transform effectRootTransform,
            bool enabled)
        {
            if (effectRootTransform == null)
                return;

            foreach (var particleSystem in effectRootTransform.GetComponentsInChildren<ParticleSystem>(true))
            {
                if (particleSystem == null)
                    continue;

                try
                {
                    if (enabled)
                    {
                        if (!particleSystem.gameObject.activeSelf)
                            particleSystem.gameObject.SetActive(true);

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

            foreach (var renderer in effectRootTransform.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null)
                    continue;

                try { renderer.enabled = enabled; } catch { }
            }

            foreach (var light in effectRootTransform.GetComponentsInChildren<Light>(true))
            {
                if (light == null)
                    continue;

                try { light.enabled = enabled; } catch { }
            }
        }

        private static void EnsureParticleSystemsPlayingIfEnabled(
            List<ParticleSystem> particleSystems,
            bool enabled)
        {
            if (!enabled || particleSystems == null)
                return;

            foreach (var particleSystem in particleSystems)
            {
                if (particleSystem != null && !particleSystem.isPlaying)
                    particleSystem.Play(true);
            }
        }

        private void ApplyHueShift(
            List<ParticleSystem> particleSystems,
            List<Renderer> renderers,
            List<Light> lights,
            float sliderValue)
        {
            float targetHue = NadaHueShiftUtility.SliderValueToTargetHue(sliderValue);

            if (particleSystems != null)
            {
                foreach (var particleSystem in particleSystems)
                {
                    if (particleSystem == null)
                        continue;

                    int particleSystemId = particleSystem.GetInstanceID();

                    try
                    {
                        if (_baseMainStartColorByParticleSystemId.TryGetValue(particleSystemId, out var baseStartColor))
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

                        if (_baseColorOverLifetimeEnabledByParticleSystemId.TryGetValue(
                                particleSystemId,
                                out var wasColorOverLifetimeEnabled))
                        {
                            colorOverLifetime.enabled = wasColorOverLifetimeEnabled;
                        }

                        if (_baseColorOverLifetimeByParticleSystemId.TryGetValue(
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

                        if (_baseCustomDataEnabledByParticleSystemId.TryGetValue(
                                particleSystemId,
                                out var wasCustomDataEnabled))
                        {
                            customData.enabled = wasCustomDataEnabled;
                        }

                        if (_baseCustom1ModeByParticleSystemId.TryGetValue(
                                particleSystemId,
                                out var baseCustom1Mode))
                        {
                            customData.SetMode(ParticleSystemCustomData.Custom1, baseCustom1Mode);
                        }

                        if (_baseCustom2ModeByParticleSystemId.TryGetValue(
                                particleSystemId,
                                out var baseCustom2Mode))
                        {
                            customData.SetMode(ParticleSystemCustomData.Custom2, baseCustom2Mode);
                        }

                        if (_baseCustom1ColorByParticleSystemId.TryGetValue(
                                particleSystemId,
                                out var baseCustom1Color))
                        {
                            customData.SetColor(
                                ParticleSystemCustomData.Custom1,
                                NadaHueShiftUtility.RetintMinMaxGradientToHue(baseCustom1Color, targetHue));
                        }

                        if (_baseCustom2ColorByParticleSystemId.TryGetValue(
                                particleSystemId,
                                out var baseCustom2Color))
                        {
                            customData.SetColor(
                                ParticleSystemCustomData.Custom2,
                                NadaHueShiftUtility.RetintMinMaxGradientToHue(baseCustom2Color, targetHue));
                        }
                    }
                    catch { }
                }
            }

            if (renderers != null)
            {
                foreach (var renderer in renderers)
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
                        var material = renderer.material;
                        if (material == null)
                            continue;

                        if (materialBaseline.Color.HasValue)
                        {
                            var retintedColor =
                                NadaHueShiftUtility.RetintColorToHue(materialBaseline.Color.Value, targetHue);

                            if (material.HasProperty("_Color"))
                                material.SetColor("_Color", retintedColor);

                            material.color = retintedColor;
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

            if (lights != null)
            {
                foreach (var light in lights)
                {
                    if (light == null)
                        continue;

                    int lightId = light.GetInstanceID();
                    if (_baseLightColorByLightId.TryGetValue(lightId, out var baseLightColor))
                    {
                        try
                        {
                            light.color = NadaHueShiftUtility.RetintColorToHue(baseLightColor, targetHue);
                        }
                        catch { }
                    }
                }
            }
        }

        private void ApplyFlamesEnergy(List<ParticleSystem> particleSystems, float energy)
        {
            if (particleSystems == null)
                return;

            float energyT = Mathf.Clamp01(energy);
            float targetRateOverTime =
                Mathf.Lerp(DefaultFlamesRateOverTime, MaxFlamesRateOverTime, energyT);

            foreach (var particleSystem in particleSystems)
            {
                if (particleSystem == null)
                    continue;

                int particleSystemId = particleSystem.GetInstanceID();
                if (!_baseEmissionByParticleSystemId.TryGetValue(particleSystemId, out var emissionBaseline) ||
                    emissionBaseline == null)
                {
                    continue;
                }

                try
                {
                    var emission = particleSystem.emission;
                    emission.enabled = emissionBaseline.Enabled;

                    if (!emissionBaseline.Enabled)
                        continue;

                    emission.rateOverTime =
                        OverrideConstantBaseline(emissionBaseline.RateOverTime, targetRateOverTime);

                    emission.rateOverDistance = emissionBaseline.RateOverDistance;
                }
                catch { }
            }
        }

        private void ApplyEmbersEnergy(List<ParticleSystem> particleSystems, float energy)
        {
            if (particleSystems == null)
                return;

            float energyT = Mathf.Clamp01(energy);
            float targetRateOverTime =
                Mathf.Lerp(DefaultEmbersRateOverTime, MaxEmbersRateOverTime, energyT);

            foreach (var particleSystem in particleSystems)
            {
                if (particleSystem == null)
                    continue;

                int particleSystemId = particleSystem.GetInstanceID();
                if (!_baseEmissionByParticleSystemId.TryGetValue(particleSystemId, out var emissionBaseline) ||
                    emissionBaseline == null)
                {
                    continue;
                }

                try
                {
                    var emission = particleSystem.emission;
                    emission.enabled = true;
                    emission.rateOverTime = new ParticleSystem.MinMaxCurve(targetRateOverTime);
                    emission.rateOverDistance = emissionBaseline.RateOverDistance;

                    if (!particleSystem.isPlaying)
                        particleSystem.Play(true);
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

        private static void AppendUniqueGroupComponents(
            Transform rootTransform,
            List<ParticleSystem> particleSystems,
            List<Renderer> renderers,
            List<Light> lights)
        {
            if (rootTransform == null)
                return;

            var seenParticleSystemIds = new HashSet<int>();
            foreach (var particleSystem in particleSystems)
            {
                if (particleSystem != null)
                    seenParticleSystemIds.Add(particleSystem.GetInstanceID());
            }

            var seenRendererIds = new HashSet<int>();
            foreach (var renderer in renderers)
            {
                if (renderer != null)
                    seenRendererIds.Add(renderer.GetInstanceID());
            }

            var seenLightIds = new HashSet<int>();
            foreach (var light in lights)
            {
                if (light != null)
                    seenLightIds.Add(light.GetInstanceID());
            }

            foreach (var particleSystem in rootTransform.GetComponentsInChildren<ParticleSystem>(true))
            {
                if (particleSystem == null)
                    continue;

                if (seenParticleSystemIds.Add(particleSystem.GetInstanceID()))
                    particleSystems.Add(particleSystem);
            }

            foreach (var renderer in rootTransform.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null)
                    continue;

                if (seenRendererIds.Add(renderer.GetInstanceID()))
                    renderers.Add(renderer);
            }

            foreach (var light in rootTransform.GetComponentsInChildren<Light>(true))
            {
                if (light == null)
                    continue;

                if (seenLightIds.Add(light.GetInstanceID()))
                    lights.Add(light);
            }
        }

        private void CacheOrbsBaselineScale()
        {
            if (_orbsRootTransform != null && !_hasOrbsBaseLocalScale)
            {
                _orbsBaseLocalScale = _orbsRootTransform.localScale;
                _hasOrbsBaseLocalScale = true;
            }

            if (_orbsPoolRootTransform == null)
                return;

            foreach (Transform pooledOrbTransform in _orbsPoolRootTransform)
            {
                if (pooledOrbTransform == null)
                    continue;

                int transformId = pooledOrbTransform.GetInstanceID();
                if (_orbsPoolBaseLocalScaleByTransformId.ContainsKey(transformId))
                    continue;

                _orbsPoolBaseLocalScaleByTransformId[transformId] = pooledOrbTransform.localScale;
            }
        }

        private void ApplyOrbsScale(float scale)
        {
            float clampedScale = ClampOrbScale(scale);
            float scaleMultiplier = DefaultOrbBaselineScaleMultiplier * clampedScale;

            if (_orbsRootTransform != null && _hasOrbsBaseLocalScale)
            {
                _orbsRootTransform.localScale =
                    _orbsBaseLocalScale * scaleMultiplier;
            }

            if (_orbsPoolRootTransform == null)
                return;

            foreach (Transform pooledOrbTransform in _orbsPoolRootTransform)
            {
                if (pooledOrbTransform == null)
                    continue;

                int transformId = pooledOrbTransform.GetInstanceID();
                if (!_orbsPoolBaseLocalScaleByTransformId.TryGetValue(transformId, out var baseLocalScale))
                    continue;

                pooledOrbTransform.localScale = baseLocalScale * scaleMultiplier;
            }
        }

        private static float ClampOrbScale(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                return 1f;

            return Mathf.Clamp(value, 0.2f, 1.8f);
        }

        private void LogToggleStateIfChanged(
            string effectDisplayName,
            bool enabled,
            ref bool lastEnabledValue,
            ref bool hasLastEnabledValue)
        {
            if (hasLastEnabledValue && lastEnabledValue == enabled)
                return;

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: {effectDisplayName} toggle applied on '{name}' (enabled={enabled}).");

            lastEnabledValue = enabled;
            hasLastEnabledValue = true;
        }
    }
}