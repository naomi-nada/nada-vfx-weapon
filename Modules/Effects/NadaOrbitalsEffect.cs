using System.Collections.Generic;
using NADA.VFX.Weapon.Core.Config;
using NADA.VFX.Weapon.Core.State;
using NADA.VFX.Weapon.Core.Visuals;
using NADA.VFX.Weapon.Runtime.Binding;
using NADA.VFX.Weapon.Runtime.Structure;
using UnityEngine;

// Offset and Rotation lives in NadaOrbitalsMotion

namespace NADA.VFX.Weapon.Modules.Effects
{
    internal sealed class NadaOrbitalsEffect : MonoBehaviour, INadaItemDataReceiver
    {
        private global::ItemDrop.ItemData _itemData;

        // Orbs
        private Transform _orbsRootTransform;
        private Transform _localOrbsRootTransform;
        private Transform _orbsVisualTransform;
        private Transform _orbsPoolRootTransform;
        private bool _lastOrbsEnabled;
        private bool _hasLastOrbsEnabled;
        private Vector3 _orbsVisualBaseLocalScale;
        private bool _hasOrbsVisualBaseLocalScale;
        private readonly Dictionary<int, Vector3> _orbsPoolBaseLocalScaleByTransformId = new();

        private readonly List<ParticleSystem> _orbsParticleSystems = new();
        private readonly List<Renderer> _orbsRenderers = new();
        private readonly List<Light> _orbsLights = new();

        private const float DefaultOrbBaselineScaleMultiplier = 1.5f;
        private const float SnakeFollowerOrbScaleMultiplier = 0.66f;

        // Cores
        private Transform _coresRootTransform;
        private Transform _coresPoolRootTransform;
        private bool _lastCoresEnabled;
        private bool _hasLastCoresEnabled;

        private readonly List<Transform> _coresVisualTransforms = new();
        private readonly Dictionary<int, Vector3> _coresBaseLocalScaleByTransformId = new();

        private readonly List<ParticleSystem> _coresParticleSystems = new();
        private readonly List<Renderer> _coresRenderers = new();
        private readonly List<Light> _coresLights = new();

        private const float DefaultCoreBaselineScaleMultiplier = 0.1f;
        private const float CoreLuminanceBoost = 2.5f;

        // Flames
        private Transform _flamesRootTransform;
        private Transform _flamesPoolRootTransform;
        private bool _lastFlamesEnabled;
        private bool _hasLastFlamesEnabled;

        private readonly List<ParticleSystem> _flamesParticleSystems = new();
        private readonly List<Renderer> _flamesRenderers = new();
        private readonly List<Light> _flamesLights = new();

        private const float DefaultFlamesRateOverTime = 10f;
        private const float MaxFlamesRateOverTime = 100f;

        // Embers
        private Transform _embersRootTransform;
        private Transform _embersPoolRootTransform;
        private bool _lastEmbersEnabled;
        private bool _hasLastEmbersEnabled;

        private readonly List<ParticleSystem> _embersParticleSystems = new();
        private readonly List<Renderer> _embersRenderers = new();
        private readonly List<Light> _embersLights = new();

        private const float DefaultEmbersRateOverTime = 6f;
        private const float MaxEmbersRateOverTime = 60f;

        // Cache lifecycle
        private bool _componentCachesDirty = true;
        private bool _modifierBaselinesDirty = true;

        // Modifier baselines
        private readonly Dictionary<int, ParticleSystem.MinMaxCurve> _baseStartLifetimeByParticleSystemId = new();
        private readonly Dictionary<int, ParticleSystem.MinMaxCurve> _baseStartSizeByParticleSystemId = new();
        private readonly Dictionary<int, bool> _baseSizeOverLifetimeEnabledByParticleSystemId = new();
        private readonly Dictionary<int, ParticleSystem.MinMaxCurve> _baseSizeOverLifetimeByParticleSystemId = new();

        private readonly Dictionary<int, ParticleSystem.MinMaxGradient> _baseMainStartColorByParticleSystemId = new();
        private readonly Dictionary<int, bool> _baseColorOverLifetimeEnabledByParticleSystemId = new();
        private readonly Dictionary<int, ParticleSystem.MinMaxGradient> _baseColorOverLifetimeByParticleSystemId = new();
        
        private readonly Dictionary<int, float> _baseSimulationSpeedByParticleSystemId = new();

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
            if (localOrbsRootTransform == null)
                return;

            if (_localOrbsRootTransform == localOrbsRootTransform)
                return;

            _localOrbsRootTransform = localOrbsRootTransform;
            _componentCachesDirty = true;
            _modifierBaselinesDirty = true;
        }

        private void Awake()
        {
            RebuildOrbitalsComponentCaches();
            CacheModifierBaselines();

            _componentCachesDirty = false;
            _modifierBaselinesDirty = false;

            InvokeRepeating(nameof(TickApply), 0f, 0.05f);
        }

        private void OnDestroy()
        {
            try { CancelInvoke(nameof(TickApply)); } catch { }
        }

        private void TickApply()
        {
            if (_componentCachesDirty)
            {
                RebuildOrbitalsComponentCaches();
                _componentCachesDirty = false;
                _modifierBaselinesDirty = true;
            }

            if (_modifierBaselinesDirty)
            {
                CacheModifierBaselines();
                _modifierBaselinesDirty = false;
            }

            VfxState state = ResolveState();

            ApplyOrbs(state);
            ApplyCores(state);
            ApplyFlames(state);
            ApplyEmbers(state);
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

        // Orbs

        private void ApplyOrbs(VfxState state)
        {
            ApplyGroupEnabledState(
                _orbsParticleSystems,
                _orbsRenderers,
                _orbsLights,
                state.OrbitalsOrbsEnabled);

            if (_orbsRootTransform != null)
                NadaRigTransforms.DisableRootVisualContent(_orbsRootTransform);

            EnsureParticleSystemsPlayingIfEnabled(
                _orbsParticleSystems,
                state.OrbitalsOrbsEnabled);

            ApplyOrbsScale(
                state.OrbitalsOrbsScale,
                state.OrbitalsOrbsSnakeEnabled);

            ApplyHueShift(
                _orbsParticleSystems,
                _orbsRenderers,
                _orbsLights,
                state.OrbitalsOrbsHue,
                state.OrbitalsOrbsLuminance);

            LogToggleStateIfChanged(
                "Orbitals Orbs",
                state.OrbitalsOrbsEnabled,
                ref _lastOrbsEnabled,
                ref _hasLastOrbsEnabled);
        }

        private void CacheOrbsBaselineScale()
        {
            if (_orbsVisualTransform != null && !_hasOrbsVisualBaseLocalScale)
            {
                _orbsVisualBaseLocalScale = NormalizeUniformScale(_orbsVisualTransform.localScale);
                _hasOrbsVisualBaseLocalScale = true;
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

                _orbsPoolBaseLocalScaleByTransformId[transformId] =
                    NormalizeUniformScale(pooledOrbTransform.localScale);
            }
        }

        private void ApplyOrbsScale(float scale, bool snake)
        {
            float clampedScale = ClampVisualScale(scale);
            float headScaleMultiplier =
                DefaultOrbBaselineScaleMultiplier * clampedScale;

            float followerScaleMultiplier =
                headScaleMultiplier *
                (snake ? SnakeFollowerOrbScaleMultiplier : 1f);

            if (_orbsRootTransform != null)
                _orbsRootTransform.localScale = Vector3.one;

            if (_orbsVisualTransform != null && _hasOrbsVisualBaseLocalScale)
            {
                _orbsVisualTransform.localScale =
                    _orbsVisualBaseLocalScale * headScaleMultiplier;

                NadaRigTransforms.ForceUniformWorldScale(_orbsVisualTransform);
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

                pooledOrbTransform.localScale = baseLocalScale * followerScaleMultiplier;
                NadaRigTransforms.ForceUniformWorldScale(pooledOrbTransform);
            }
        }

        // Cores

        private void ApplyCoresScale(float scale, bool snake)
        {
            float clampedScale = ClampVisualScale(scale);

            float sharedScaleMultiplier =
                DefaultCoreBaselineScaleMultiplier * clampedScale;

            if (_coresRootTransform != null &&
                _coresBaseLocalScaleByTransformId.TryGetValue(
                    _coresRootTransform.GetInstanceID(),
                    out Vector3 rootBaseScale))
            {
                _coresRootTransform.localScale = rootBaseScale * sharedScaleMultiplier;
                NadaRigTransforms.ForceUniformWorldScale(_coresRootTransform);
            }

            if (_coresPoolRootTransform == null)
                return;

            foreach (Transform pooledCoreTransform in _coresPoolRootTransform)
            {
                if (pooledCoreTransform == null)
                    continue;

                int transformId = pooledCoreTransform.GetInstanceID();
                if (!_coresBaseLocalScaleByTransformId.TryGetValue(transformId, out Vector3 baseLocalScale))
                    continue;

                pooledCoreTransform.localScale = baseLocalScale * sharedScaleMultiplier;
                NadaRigTransforms.ForceUniformWorldScale(pooledCoreTransform);
            }
        }
        
        private void ApplyCores(VfxState state)
        {
            SetRootActive(_coresRootTransform, state.OrbitalsCoresEnabled);
            SetRootActive(_coresPoolRootTransform, state.OrbitalsCoresEnabled);

            ApplyGroupEnabledState(
                _coresParticleSystems,
                _coresRenderers,
                _coresLights,
                state.OrbitalsCoresEnabled);

            EnsureParticleSystemsPlayingIfEnabled(
                _coresParticleSystems,
                state.OrbitalsCoresEnabled);

            ApplyCoresScale(
                state.OrbitalsCoresScale,
                state.OrbitalsCoresSnakeEnabled);

            ApplyHueShift(
                _coresParticleSystems,
                _coresRenderers,
                _coresLights,
                state.OrbitalsCoresHue,
                RemapCoreLuminance(state.OrbitalsCoresLuminance));

            LogToggleStateIfChanged(
                "Orbitals Cores",
                state.OrbitalsCoresEnabled,
                ref _lastCoresEnabled,
                ref _hasLastCoresEnabled);
        }

        private void CacheCoresBaselineScale()
        {
            AddCoreVisualTransform(_coresRootTransform);

            if (_coresPoolRootTransform == null)
                return;

            foreach (Transform pooledCoreTransform in _coresPoolRootTransform)
                AddCoreVisualTransform(pooledCoreTransform);
        }

        private void AddCoreVisualTransform(Transform coreTransform)
        {
            if (coreTransform == null)
                return;

            if (!_coresVisualTransforms.Contains(coreTransform))
                _coresVisualTransforms.Add(coreTransform);

            int transformId = coreTransform.GetInstanceID();
            if (_coresBaseLocalScaleByTransformId.ContainsKey(transformId))
                return;

            _coresBaseLocalScaleByTransformId[transformId] =
                NormalizeUniformScale(coreTransform.localScale);
        }
        

        // Flames

        private void ApplyFlames(VfxState state)
        {
            ApplyGroupEnabledState(
                _flamesParticleSystems,
                _flamesRenderers,
                _flamesLights,
                state.OrbitalsFlamesEnabled);

            EnsureParticleSystemsPlayingIfEnabled(
                _flamesParticleSystems,
                state.OrbitalsFlamesEnabled);

            ApplyHueShift(
                _flamesParticleSystems,
                _flamesRenderers,
                _flamesLights,
                state.OrbitalsFlamesHue,
                state.OrbitalsFlamesLuminance);

            ApplyFlamesEnergy(
                _flamesParticleSystems,
                state.OrbitalsFlamesEnergy);
            
            ApplySimulationSpeed(
                _flamesParticleSystems,
                state.OrbitalsFlamesSimulationSpeed);

            ApplyFlamesScale(
                _flamesParticleSystems,
                state.OrbitalsFlamesScale);

            ApplyFlamesLifetime(
                _flamesParticleSystems,
                state.OrbitalsFlamesLifetime);

            LogToggleStateIfChanged(
                "Orbitals Flames",
                state.OrbitalsFlamesEnabled,
                ref _lastFlamesEnabled,
                ref _hasLastFlamesEnabled);
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
                    continue;

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

        private void ApplyFlamesScale(List<ParticleSystem> particleSystems, float scale)
        {
            if (particleSystems == null)
                return;

            float scaleMultiplier = ClampScale(scale);

            foreach (var particleSystem in particleSystems)
            {
                if (particleSystem == null)
                    continue;

                int particleSystemId = particleSystem.GetInstanceID();

                try
                {
                    var main = particleSystem.main;

                    if (_baseStartSizeByParticleSystemId.TryGetValue(particleSystemId, out var baseStartSize))
                        main.startSize = ScaleMinMaxCurve(baseStartSize, scaleMultiplier);
                }
                catch { }

                try
                {
                    var sizeOverLifetime = particleSystem.sizeOverLifetime;

                    if (_baseSizeOverLifetimeEnabledByParticleSystemId.TryGetValue(
                            particleSystemId,
                            out bool wasSizeOverLifetimeEnabled))
                    {
                        sizeOverLifetime.enabled = wasSizeOverLifetimeEnabled;

                        if (wasSizeOverLifetimeEnabled &&
                            _baseSizeOverLifetimeByParticleSystemId.TryGetValue(
                                particleSystemId,
                                out var baseSizeOverLifetime))
                        {
                            sizeOverLifetime.size =
                                ScaleMinMaxCurve(baseSizeOverLifetime, scaleMultiplier);
                        }
                    }
                }
                catch { }
            }
        }

        private void ApplyFlamesLifetime(List<ParticleSystem> particleSystems, float lifetime)
        {
            if (particleSystems == null)
                return;

            float clampedLifetime = Mathf.Clamp(
                lifetime,
                PluginConfig.MinLifetime,
                PluginConfig.MaxLifetime);

            foreach (var particleSystem in particleSystems)
            {
                if (particleSystem == null)
                    continue;

                int particleSystemId = particleSystem.GetInstanceID();

                if (!_baseStartLifetimeByParticleSystemId.TryGetValue(
                        particleSystemId,
                        out var baseLifetime))
                    continue;

                try
                {
                    var main = particleSystem.main;
                    main.startLifetime = MultiplyCurve(baseLifetime, clampedLifetime);
                }
                catch { }
            }
        }

        // Embers

        private void ApplyEmbers(VfxState state)
        {
            ApplyGroupEnabledState(
                _embersParticleSystems,
                _embersRenderers,
                _embersLights,
                state.OrbitalsEmbersEnabled);

            EnsureParticleSystemsPlayingIfEnabled(
                _embersParticleSystems,
                state.OrbitalsEmbersEnabled);

            ApplyHueShift(
                _embersParticleSystems,
                _embersRenderers,
                _embersLights,
                state.OrbitalsEmbersHue,
                state.OrbitalsEmbersLuminance);

            ApplyEmbersEnergy(
                _embersParticleSystems,
                state.OrbitalsEmbersEnergy);

            ApplyEmbersScale(
                _embersParticleSystems,
                state.OrbitalsEmbersScale);
            
            ApplySimulationSpeed(
                _flamesParticleSystems,
                state.OrbitalsFlamesSimulationSpeed);

            ApplyEmbersLifetime(
                _embersParticleSystems,
                state.OrbitalsEmbersLifetime);

            LogToggleStateIfChanged(
                "Orbitals Embers",
                state.OrbitalsEmbersEnabled,
                ref _lastEmbersEnabled,
                ref _hasLastEmbersEnabled);
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
                    continue;

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
        
        private void ApplyEmbersScale(List<ParticleSystem> particleSystems, float scale)
        {
            if (particleSystems == null)
                return;

            float scaleMultiplier = ClampScale(scale);

            foreach (var particleSystem in particleSystems)
            {
                if (particleSystem == null)
                    continue;

                int particleSystemId = particleSystem.GetInstanceID();

                try
                {
                    var main = particleSystem.main;

                    if (_baseStartSizeByParticleSystemId.TryGetValue(particleSystemId, out var baseStartSize))
                        main.startSize = ScaleMinMaxCurve(baseStartSize, scaleMultiplier);
                }
                catch { }

                try
                {
                    var sizeOverLifetime = particleSystem.sizeOverLifetime;

                    if (_baseSizeOverLifetimeEnabledByParticleSystemId.TryGetValue(
                            particleSystemId,
                            out bool wasSizeOverLifetimeEnabled))
                    {
                        sizeOverLifetime.enabled = wasSizeOverLifetimeEnabled;

                        if (wasSizeOverLifetimeEnabled &&
                            _baseSizeOverLifetimeByParticleSystemId.TryGetValue(
                                particleSystemId,
                                out var baseSizeOverLifetime))
                        {
                            sizeOverLifetime.size =
                                ScaleMinMaxCurve(baseSizeOverLifetime, scaleMultiplier);
                        }
                    }
                }
                catch { }
            }
        }

        private void ApplyEmbersLifetime(List<ParticleSystem> particleSystems, float lifetime)
        {
            if (particleSystems == null)
                return;

            float clamped = Mathf.Clamp(
                lifetime,
                PluginConfig.MinLifetime,
                PluginConfig.MaxLifetime);

            foreach (var particleSystem in particleSystems)
            {
                if (particleSystem == null)
                    continue;

                int particleSystemId = particleSystem.GetInstanceID();

                if (!_baseStartLifetimeByParticleSystemId.TryGetValue(
                        particleSystemId,
                        out var baseLifetime))
                    continue;

                try
                {
                    var main = particleSystem.main;
                    main.startLifetime = MultiplyCurve(baseLifetime, clamped);
                }
                catch { }
            }
        }

        // Cache discovery

        private void RebuildOrbitalsComponentCaches()
        {
            ClearComponentCaches();

            _orbsRootTransform = _localOrbsRootTransform != null
                ? _localOrbsRootTransform
                : NadaRigPaths.FindDirectChild(transform, Plugin.OrbitalsOrbsName);

            _orbsVisualTransform =
                NadaRigPaths.FindDirectChild(_orbsRootTransform, "Orb_00");

            _coresRootTransform =
                NadaRigPaths.FindDirectChild(transform, Plugin.OrbitalsCoresName);

            _flamesRootTransform =
                NadaRigPaths.FindDirectChild(transform, Plugin.OrbitalsFlamesName);

            _embersRootTransform =
                NadaRigPaths.FindDirectChild(transform, Plugin.OrbitalsEmbersName);

            ResolvePoolRoots();

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
                _coresRootTransform,
                _coresParticleSystems,
                _coresRenderers,
                _coresLights);

            AppendUniqueGroupComponents(
                _coresPoolRootTransform,
                _coresParticleSystems,
                _coresRenderers,
                _coresLights);

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

        private void ClearComponentCaches()
        {
            _orbsParticleSystems.Clear();
            _orbsRenderers.Clear();
            _orbsLights.Clear();

            _coresVisualTransforms.Clear();
            _coresParticleSystems.Clear();
            _coresRenderers.Clear();
            _coresLights.Clear();

            _flamesParticleSystems.Clear();
            _flamesRenderers.Clear();
            _flamesLights.Clear();

            _embersParticleSystems.Clear();
            _embersRenderers.Clear();
            _embersLights.Clear();
        }

        private void ResolvePoolRoots()
        {
            _orbsPoolRootTransform = null;
            _coresPoolRootTransform = null;
            _flamesPoolRootTransform = null;
            _embersPoolRootTransform = null;

            Transform orbitalsRigRootTransform =
                NadaRigPaths.FindDirectChild(transform, Plugin.OrbitalsRigRootName);

            Transform orbitalsPoolsRootTransform =
                NadaRigPaths.FindDirectChild(orbitalsRigRootTransform, Plugin.OrbitalsPoolsRootName);

            if (orbitalsPoolsRootTransform == null)
                return;

            _orbsPoolRootTransform =
                NadaRigPaths.FindDirectChild(orbitalsPoolsRootTransform, Plugin.OrbitalsOrbsPoolName);

            _coresPoolRootTransform =
                NadaRigPaths.FindDirectChild(orbitalsPoolsRootTransform, Plugin.OrbitalsCoresPoolName);

            _flamesPoolRootTransform =
                NadaRigPaths.FindDirectChild(orbitalsPoolsRootTransform, Plugin.OrbitalsFlamesPoolName);

            _embersPoolRootTransform =
                NadaRigPaths.FindDirectChild(orbitalsPoolsRootTransform, Plugin.OrbitalsEmbersPoolName);
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

        // Baselines

        private void CacheModifierBaselines()
        {
            CacheOrbsBaselines();
            CacheCoresBaselines();
            CacheFlamesBaselines();
            CacheEmbersBaselines();
        }

        private void CacheOrbsBaselines()
        {
            CacheParticleBaselines(_orbsParticleSystems);
            CacheRendererBaselines(_orbsRenderers);
            CacheLightBaselines(_orbsLights);
            CacheOrbsBaselineScale();
        }

        private void CacheCoresBaselines()
        {
            CacheParticleBaselines(_coresParticleSystems);
            CacheEmissionBaselines(_coresParticleSystems);
            CacheRendererBaselines(_coresRenderers);
            CacheLightBaselines(_coresLights);
            CacheCoresBaselineScale();
        }

        private void CacheFlamesBaselines()
        {
            CacheParticleBaselines(_flamesParticleSystems);
            CacheEmissionBaselines(_flamesParticleSystems);
            CacheRendererBaselines(_flamesRenderers);
            CacheLightBaselines(_flamesLights);
        }

        private void CacheEmbersBaselines()
                {
                    CacheParticleBaselines(_embersParticleSystems);
                    CacheEmissionBaselines(_embersParticleSystems);
                    CacheRendererBaselines(_embersRenderers);
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
                        _baseColorOverLifetimeEnabledByParticleSystemId[particleSystemId] =
                            colorOverLifetime.enabled;
                        _baseColorOverLifetimeByParticleSystemId[particleSystemId] =
                            colorOverLifetime.color;
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

                if (!_baseStartLifetimeByParticleSystemId.ContainsKey(particleSystemId))
                {
                    try
                    {
                        var main = particleSystem.main;
                        _baseStartLifetimeByParticleSystemId[particleSystemId] = main.startLifetime;
                    }
                    catch { }
                }

                if (!_baseStartSizeByParticleSystemId.ContainsKey(particleSystemId))
                {
                    try
                    {
                        var main = particleSystem.main;
                        _baseStartSizeByParticleSystemId[particleSystemId] = main.startSize;
                    }
                    catch { }
                }

                if (!_baseSimulationSpeedByParticleSystemId.ContainsKey(particleSystemId))
                {
                    try
                    {
                        var main = particleSystem.main;
                        _baseSimulationSpeedByParticleSystemId[particleSystemId] = main.simulationSpeed;
                    }
                    catch { }
                }

                if (!_baseSizeOverLifetimeEnabledByParticleSystemId.ContainsKey(particleSystemId) ||
                    !_baseSizeOverLifetimeByParticleSystemId.ContainsKey(particleSystemId))
                {
                    try
                    {
                        var sizeOverLifetime = particleSystem.sizeOverLifetime;
                        _baseSizeOverLifetimeEnabledByParticleSystemId[particleSystemId] =
                            sizeOverLifetime.enabled;
                        _baseSizeOverLifetimeByParticleSystemId[particleSystemId] =
                            sizeOverLifetime.size;
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

        // Shared apply helpers

        private static void SetRootActive(Transform rootTransform, bool enabled)
        {
            if (rootTransform == null)
                return;

            if (rootTransform.gameObject.activeSelf != enabled)
                rootTransform.gameObject.SetActive(enabled);
        }

        private static void ApplyGroupEnabledState(
            List<ParticleSystem> particleSystems,
            List<Renderer> renderers,
            List<Light> lights,
            bool enabled)
        {
            if (particleSystems != null)
            {
                foreach (var particleSystem in particleSystems)
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
            }

            if (renderers != null)
            {
                foreach (var renderer in renderers)
                {
                    if (renderer == null)
                        continue;

                    try { renderer.enabled = enabled; } catch { }
                }
            }

            if (lights != null)
            {
                foreach (var light in lights)
                {
                    if (light == null)
                        continue;

                    try { light.enabled = enabled; } catch { }
                }
            }
        }
        
        private void ApplySimulationSpeed(
            List<ParticleSystem> particleSystems,
            float simulationSpeed)
        {
            if (particleSystems == null)
                return;

            float clampedSimulationSpeed = ClampSimulationSpeed(simulationSpeed);

            foreach (var particleSystem in particleSystems)
            {
                if (particleSystem == null)
                    continue;

                int particleSystemId = particleSystem.GetInstanceID();

                if (!_baseSimulationSpeedByParticleSystemId.TryGetValue(
                        particleSystemId,
                        out float baseSimulationSpeed))
                {
                    continue;
                }

                try
                {
                    var main = particleSystem.main;
                    main.simulationSpeed = baseSimulationSpeed * clampedSimulationSpeed;
                }
                catch { }
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
            float sliderValue,
            float rendererEmissionMultiplier = 1f)
        {
            float targetHue = NadaHueShiftUtility.SliderValueToTargetHue(sliderValue);

            ApplyParticleHueShift(particleSystems, targetHue, rendererEmissionMultiplier);
            ApplyRendererHueShift(renderers, targetHue, rendererEmissionMultiplier);
            ApplyLightHueShift(lights, targetHue);
        }

        private void ApplyParticleHueShift(
            List<ParticleSystem> particleSystems,
            float targetHue,
            float luminanceMultiplier = 1f)
        {
            if (particleSystems == null)
                return;

            float clampedLuminanceMultiplier =
                NadaLuminanceUtility.RemapParticleLuminance(luminanceMultiplier);

            foreach (var particleSystem in particleSystems)
            {
                if (particleSystem == null)
                    continue;

                int particleSystemId = particleSystem.GetInstanceID();

                try
                {
                    if (_baseMainStartColorByParticleSystemId.TryGetValue(
                            particleSystemId,
                            out var baseStartColor))
                    {
                        var main = particleSystem.main;

                        main.startColor =
                            NadaLuminanceUtility.ApplyToGradient(
                                NadaHueShiftUtility.RetintMinMaxGradientToHue(baseStartColor, targetHue),
                                clampedLuminanceMultiplier);
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
                            NadaLuminanceUtility.ApplyToGradient(
                                NadaHueShiftUtility.RetintMinMaxGradientToHue(baseColorOverLifetime, targetHue),
                                clampedLuminanceMultiplier);
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

        private void ApplyRendererHueShift(
            List<Renderer> renderers,
            float targetHue,
            float emissionMultiplier = 1f)
        {
            if (renderers == null)
                return;

            float clampedEmissionMultiplier = Mathf.Clamp(
                emissionMultiplier,
                PluginConfig.MinLuminance,
                PluginConfig.MaxLuminance);

            foreach (var renderer in renderers)
            {
                if (renderer == null)
                    continue;

                int rendererId = renderer.GetInstanceID();
                if (!_baseMaterialByRendererId.TryGetValue(rendererId, out var materialBaseline) ||
                    materialBaseline == null)
                    continue;

                try
                {
                    var material = renderer.material;
                    if (material == null)
                        continue;

                    material.renderQueue = 3100;

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
                        Color retintedEmission =
                            NadaHueShiftUtility.RetintColorToHue(materialBaseline.EmissionColor.Value, targetHue);

                        material.EnableKeyword("_EMISSION");
                        material.SetColor(
                            "_EmissionColor",
                            retintedEmission * clampedEmissionMultiplier);
                    }
                }
                catch { }
            }
        }

        private void ApplyLightHueShift(
            List<Light> lights,
            float targetHue)
        {
            if (lights == null)
                return;

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

        private static float RemapCoreLuminance(float luminance)
        {
            if (float.IsNaN(luminance) || float.IsInfinity(luminance))
                return PluginConfig.DefaultLuminance;

            luminance = Mathf.Clamp(
                luminance,
                PluginConfig.MinLuminance,
                PluginConfig.MaxLuminance);

            return luminance * CoreLuminanceBoost;
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

        private static ParticleSystem.MinMaxCurve MultiplyCurve(
            ParticleSystem.MinMaxCurve source,
            float multiplier)
        {
            ParticleSystem.MinMaxCurve result = source;

            result.constant *= multiplier;
            result.constantMin *= multiplier;
            result.constantMax *= multiplier;
            result.curveMultiplier *= multiplier;

            return result;
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

        private static float ClampScale(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                return 1f;

            return Mathf.Clamp(
                value,
                PluginConfig.MinScaleMult,
                PluginConfig.MaxScaleMult);
        }
        
        private static float ClampSimulationSpeed(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                return PluginConfig.DefaultSimulationSpeed;

            return Mathf.Clamp(
                value,
                PluginConfig.MinSimulationSpeed,
                PluginConfig.MaxSimulationSpeed);
        }

        private static Vector3 NormalizeUniformScale(Vector3 scale)
        {
            float uniform = Mathf.Max(
                Mathf.Abs(scale.x),
                Mathf.Abs(scale.y),
                Mathf.Abs(scale.z));

            if (uniform <= 0.0001f)
                uniform = 1f;

            return Vector3.one * uniform;
        }

        private static float ClampVisualScale(float value)
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

            lastEnabledValue = enabled;
            hasLastEnabledValue = true;
        }
    }
}