using System.Collections.Generic;
using NADA.VFX.Weapon.Core.Config;
using NADA.VFX.Weapon.Core.Debug;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Blocks.Effects;
using NADA.VFX.Weapon.Core.Visuals;
using NADA.VFX.Weapon.Modules.Motion;
using UnityEngine;

namespace NADA.VFX.Weapon.Modules.Effects
{
    /// <summary>
    /// Visual behavior for one block-owned Flames or Embers orbital instance.
    ///
    /// The block is the only state authority. There is no ItemData,
    /// config, or legacy VfxState fallback here.
    /// </summary>
    internal sealed class NadaOrbitalsParticleBlockEffect :
        MonoBehaviour
    {
        private const float DefaultFlamesRateOverTime =
            10f;

        private const float MaxFlamesRateOverTime =
            100f;

        private const float DefaultEmbersRateOverTime =
            6f;

        private const float MaxEmbersRateOverTime =
            60f;

        private struct RuntimeState
        {
            internal bool Enabled;

            internal float Energy;
            internal float Scale;
            internal float Luminance;
            internal float Hue;
            internal float Lifetime;
            internal float SimulationSpeed;
        }

        private NadaOrbitalsFamily _family;

        private Transform _visualRootTransform;
        private Transform _poolRootTransform;

        private RuntimeState _state;
        private bool _hasState;

        private readonly List<ParticleSystem>
            _particleSystems = new();

        private readonly List<Renderer>
            _renderers = new();

        private readonly List<Light>
            _lights = new();

        private readonly Dictionary<int, ParticleSystem.MinMaxCurve>
            _baseStartLifetimeByParticleSystemId = new();

        private readonly Dictionary<int, ParticleSystem.MinMaxCurve>
            _baseStartSizeByParticleSystemId = new();

        private readonly Dictionary<int, bool>
            _baseSizeOverLifetimeEnabledByParticleSystemId = new();

        private readonly Dictionary<int, ParticleSystem.MinMaxCurve>
            _baseSizeOverLifetimeByParticleSystemId = new();

        private readonly Dictionary<int, ParticleSystem.MinMaxGradient>
            _baseMainStartColorByParticleSystemId = new();

        private readonly Dictionary<int, bool>
            _baseColorOverLifetimeEnabledByParticleSystemId = new();

        private readonly Dictionary<int, ParticleSystem.MinMaxGradient>
            _baseColorOverLifetimeByParticleSystemId = new();

        private readonly Dictionary<int, float>
            _baseSimulationSpeedByParticleSystemId = new();

        private readonly Dictionary<int, bool>
            _baseCustomDataEnabledByParticleSystemId = new();

        private readonly Dictionary<int, ParticleSystemCustomDataMode>
            _baseCustom1ModeByParticleSystemId = new();

        private readonly Dictionary<int, ParticleSystemCustomDataMode>
            _baseCustom2ModeByParticleSystemId = new();

        private readonly Dictionary<int, ParticleSystem.MinMaxGradient>
            _baseCustom1ColorByParticleSystemId = new();

        private readonly Dictionary<int, ParticleSystem.MinMaxGradient>
            _baseCustom2ColorByParticleSystemId = new();

        private readonly Dictionary<int, EmissionBaseline>
            _baseEmissionByParticleSystemId = new();

        private readonly Dictionary<int, MaterialBaseline>
            _baseMaterialByRendererId = new();

        private readonly Dictionary<int, Color>
            _baseLightColorByLightId = new();

        private readonly HashSet<Material>
            _ownedRendererMaterials = new();

        private sealed class EmissionBaseline
        {
            internal bool Enabled;
            internal ParticleSystem.MinMaxCurve RateOverTime;
            internal ParticleSystem.MinMaxCurve RateOverDistance;
        }

        private sealed class MaterialBaseline
        {
            internal Color? Color;
            internal Color? BaseColor;
            internal Color? TintColor;
            internal Color? EmissionColor;
        }

        private void Awake()
        {
            NadaRuntimeDiagnostics
                .OrbitalsEffectCreated();

            InvokeRepeating(
                nameof(TickLiveness),
                0f,
                0.05f);
        }

        private void OnDestroy()
        {
            NadaRuntimeDiagnostics
                .OrbitalsEffectDestroyed();

            try
            {
                CancelInvoke(
                    nameof(TickLiveness));
            }
            catch
            {
            }

            DestroyOwnedMaterials();
        }

        internal bool SetBlockState(
            NadaOrbitalsFamily family,
            Transform visualRootTransform,
            Transform poolRootTransform,
            VfxEffectBlock block)
        {
            bool supportedFamily =
                family ==
                    NadaOrbitalsFamily.Flames ||
                family ==
                    NadaOrbitalsFamily.Embers;
            
            RuntimeState nextState =
                default;

            bool accepted =
                supportedFamily &&
                block != null &&
                block.Transform != null &&
                visualRootTransform != null &&
                poolRootTransform != null &&
                TryBuildRuntimeState(
                    family,
                    block,
                    out nextState);

            if (!accepted)
            {
                DisableVisuals();

                _hasState =
                    false;

                return false;
            }

            bool rootsChanged =
                _family != family ||
                _visualRootTransform !=
                    visualRootTransform ||
                _poolRootTransform !=
                    poolRootTransform;

            if (rootsChanged)
            {
                DestroyOwnedMaterials();
                ClearCaches();

                _family =
                    family;

                _visualRootTransform =
                    visualRootTransform;

                _poolRootTransform =
                    poolRootTransform;

                RebuildCaches();
                CacheBaselines();
            }

            bool stateChanged =
                !_hasState ||
                !RuntimeStateEquals(
                    _state,
                    nextState);

            _state =
                nextState;

            _hasState =
                true;

            ApplyEnabledState(
                _state.Enabled);

            if (!_state.Enabled)
                return true;

            if (rootsChanged ||
                stateChanged)
            {
                ApplyStaticState(
                    _state);
            }

            EnsureParticleSystemsPlaying();

            return true;
        }

        private static bool TryBuildRuntimeState(
            NadaOrbitalsFamily family,
            VfxEffectBlock block,
            out RuntimeState state)
        {
            state =
                default;

            if (family ==
                NadaOrbitalsFamily.Flames)
            {
                OrbitalsFlamesVfxSettings settings =
                    block.Settings as OrbitalsFlamesVfxSettings;

                if (block.TypeId !=
                        VfxEffectTypeIds.OrbitalsFlames ||
                    settings?.Formation?.Path == null)
                {
                    return false;
                }

                state =
                    new RuntimeState
                    {
                        Enabled =
                            block.Enabled,

                        Energy =
                            settings.Energy,

                        Scale =
                            settings.Scale,

                        Luminance =
                            settings.Luminance,

                        Hue =
                            settings.Hue,

                        Lifetime =
                            settings.Lifetime,

                        SimulationSpeed =
                            settings.SimulationSpeed
                    };

                return true;
            }

            if (family ==
                NadaOrbitalsFamily.Embers)
            {
                OrbitalsEmbersVfxSettings settings =
                    block.Settings as OrbitalsEmbersVfxSettings;

                if (block.TypeId !=
                        VfxEffectTypeIds.OrbitalsEmbers ||
                    settings?.Formation?.Path == null)
                {
                    return false;
                }

                state =
                    new RuntimeState
                    {
                        Enabled =
                            block.Enabled,

                        Energy =
                            settings.Energy,

                        Scale =
                            settings.Scale,

                        Luminance =
                            settings.Luminance,

                        Hue =
                            settings.Hue,

                        Lifetime =
                            settings.Lifetime,

                        SimulationSpeed =
                            settings.SimulationSpeed
                    };

                return true;
            }

            return false;
        }

        private void ApplyStaticState(
            RuntimeState state)
        {
            ApplyHueShift(
                state.Hue,
                state.Luminance);

            ApplyEnergy(
                state.Energy);

            ApplySimulationSpeed(
                state.SimulationSpeed);

            ApplyScale(
                state.Scale);

            ApplyLifetime(
                state.Lifetime);
        }

        private void ApplyEnabledState(
            bool enabled)
        {
            SetRootActive(
                _visualRootTransform,
                enabled);

            SetRootActive(
                _poolRootTransform,
                enabled);

            ApplyGroupEnabledState(
                enabled);
        }

        private void DisableVisuals()
        {
            ApplyEnabledState(
                false);
        }

        private void TickLiveness()
        {
            if (!_hasState ||
                !_state.Enabled)
            {
                return;
            }

            EnsureParticleSystemsPlaying();
        }

        private void RebuildCaches()
        {
            ClearCaches();

            AppendUniqueGroupComponents(
                _visualRootTransform);

            AppendUniqueGroupComponents(
                _poolRootTransform);
        }

        private void ClearCaches()
        {
            _particleSystems.Clear();
            _renderers.Clear();
            _lights.Clear();

            _baseStartLifetimeByParticleSystemId.Clear();
            _baseStartSizeByParticleSystemId.Clear();

            _baseSizeOverLifetimeEnabledByParticleSystemId.Clear();
            _baseSizeOverLifetimeByParticleSystemId.Clear();

            _baseMainStartColorByParticleSystemId.Clear();

            _baseColorOverLifetimeEnabledByParticleSystemId.Clear();
            _baseColorOverLifetimeByParticleSystemId.Clear();

            _baseSimulationSpeedByParticleSystemId.Clear();

            _baseCustomDataEnabledByParticleSystemId.Clear();
            _baseCustom1ModeByParticleSystemId.Clear();
            _baseCustom2ModeByParticleSystemId.Clear();
            _baseCustom1ColorByParticleSystemId.Clear();
            _baseCustom2ColorByParticleSystemId.Clear();

            _baseEmissionByParticleSystemId.Clear();
            _baseMaterialByRendererId.Clear();
            _baseLightColorByLightId.Clear();
        }

        private void AppendUniqueGroupComponents(
            Transform rootTransform)
        {
            if (rootTransform == null)
                return;

            foreach (ParticleSystem particleSystem in
                     rootTransform.GetComponentsInChildren<ParticleSystem>(
                         true))
            {
                if (particleSystem != null &&
                    !_particleSystems.Contains(
                        particleSystem))
                {
                    _particleSystems.Add(
                        particleSystem);
                }
            }

            foreach (Renderer renderer in
                     rootTransform.GetComponentsInChildren<Renderer>(
                         true))
            {
                if (renderer != null &&
                    !_renderers.Contains(
                        renderer))
                {
                    _renderers.Add(
                        renderer);
                }
            }

            foreach (Light light in
                     rootTransform.GetComponentsInChildren<Light>(
                         true))
            {
                if (light != null &&
                    !_lights.Contains(
                        light))
                {
                    _lights.Add(
                        light);
                }
            }
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
            foreach (ParticleSystem particleSystem in
                     _particleSystems)
            {
                if (particleSystem == null)
                    continue;

                int particleSystemId =
                    particleSystem.GetInstanceID();

                try
                {
                    var main =
                        particleSystem.main;

                    _baseStartLifetimeByParticleSystemId[
                            particleSystemId] =
                        main.startLifetime;

                    _baseStartSizeByParticleSystemId[
                            particleSystemId] =
                        main.startSize;

                    _baseMainStartColorByParticleSystemId[
                            particleSystemId] =
                        main.startColor;

                    _baseSimulationSpeedByParticleSystemId[
                            particleSystemId] =
                        main.simulationSpeed;
                }
                catch
                {
                }

                try
                {
                    var sizeOverLifetime =
                        particleSystem.sizeOverLifetime;

                    _baseSizeOverLifetimeEnabledByParticleSystemId[
                            particleSystemId] =
                        sizeOverLifetime.enabled;

                    _baseSizeOverLifetimeByParticleSystemId[
                            particleSystemId] =
                        sizeOverLifetime.size;
                }
                catch
                {
                }

                try
                {
                    var colorOverLifetime =
                        particleSystem.colorOverLifetime;

                    _baseColorOverLifetimeEnabledByParticleSystemId[
                            particleSystemId] =
                        colorOverLifetime.enabled;

                    _baseColorOverLifetimeByParticleSystemId[
                            particleSystemId] =
                        colorOverLifetime.color;
                }
                catch
                {
                }

                try
                {
                    var customData =
                        particleSystem.customData;

                    _baseCustomDataEnabledByParticleSystemId[
                            particleSystemId] =
                        customData.enabled;

                    _baseCustom1ModeByParticleSystemId[
                            particleSystemId] =
                        customData.GetMode(
                            ParticleSystemCustomData.Custom1);

                    _baseCustom2ModeByParticleSystemId[
                            particleSystemId] =
                        customData.GetMode(
                            ParticleSystemCustomData.Custom2);

                    _baseCustom1ColorByParticleSystemId[
                            particleSystemId] =
                        customData.GetColor(
                            ParticleSystemCustomData.Custom1);

                    _baseCustom2ColorByParticleSystemId[
                            particleSystemId] =
                        customData.GetColor(
                            ParticleSystemCustomData.Custom2);
                }
                catch
                {
                }
            }
        }

        private void CacheEmissionBaselines()
        {
            foreach (ParticleSystem particleSystem in
                     _particleSystems)
            {
                if (particleSystem == null)
                    continue;

                int particleSystemId =
                    particleSystem.GetInstanceID();

                try
                {
                    var emission =
                        particleSystem.emission;

                    _baseEmissionByParticleSystemId[
                            particleSystemId] =
                        new EmissionBaseline
                        {
                            Enabled =
                                emission.enabled,

                            RateOverTime =
                                emission.rateOverTime,

                            RateOverDistance =
                                emission.rateOverDistance
                        };
                }
                catch
                {
                }
            }
        }

        private void CacheRendererBaselines()
        {
            foreach (Renderer renderer in
                     _renderers)
            {
                if (renderer == null)
                    continue;

                int rendererId =
                    renderer.GetInstanceID();

                Material previousMaterial;
                Material material;

                try
                {
                    previousMaterial =
                        renderer.sharedMaterial;

                    material =
                        renderer.material;
                }
                catch
                {
                    continue;
                }

                if (material == null)
                    continue;

                if (material != previousMaterial)
                {
                    _ownedRendererMaterials.Add(
                        material);
                }

                var baseline =
                    new MaterialBaseline();

                try
                {
                    if (material.HasProperty(
                            "_Color"))
                    {
                        baseline.Color =
                            material.GetColor(
                                "_Color");
                    }

                    if (material.HasProperty(
                            "_BaseColor"))
                    {
                        baseline.BaseColor =
                            material.GetColor(
                                "_BaseColor");
                    }

                    if (material.HasProperty(
                            "_TintColor"))
                    {
                        baseline.TintColor =
                            material.GetColor(
                                "_TintColor");
                    }

                    if (material.HasProperty(
                            "_EmissionColor"))
                    {
                        baseline.EmissionColor =
                            material.GetColor(
                                "_EmissionColor");
                    }
                }
                catch
                {
                }

                _baseMaterialByRendererId[
                        rendererId] =
                    baseline;
            }
        }

        private void CacheLightBaselines()
        {
            foreach (Light light in
                     _lights)
            {
                if (light != null)
                {
                    _baseLightColorByLightId[
                            light.GetInstanceID()] =
                        light.color;
                }
            }
        }

        private void ApplyEnergy(
            float energy)
        {
            float energyT =
                Mathf.Clamp01(
                    energy);

            float targetRateOverTime =
                _family ==
                    NadaOrbitalsFamily.Flames
                    ? Mathf.Lerp(
                        DefaultFlamesRateOverTime,
                        MaxFlamesRateOverTime,
                        energyT)
                    : Mathf.Lerp(
                        DefaultEmbersRateOverTime,
                        MaxEmbersRateOverTime,
                        energyT);

            foreach (ParticleSystem particleSystem in
                     _particleSystems)
            {
                if (particleSystem == null)
                    continue;

                int particleSystemId =
                    particleSystem.GetInstanceID();

                if (!_baseEmissionByParticleSystemId.TryGetValue(
                        particleSystemId,
                        out EmissionBaseline baseline) ||
                    baseline == null)
                {
                    continue;
                }

                try
                {
                    var emission =
                        particleSystem.emission;

                    if (_family ==
                        NadaOrbitalsFamily.Flames)
                    {
                        emission.enabled =
                            baseline.Enabled;

                        if (!baseline.Enabled)
                            continue;

                        emission.rateOverTime =
                            OverrideConstantBaseline(
                                baseline.RateOverTime,
                                targetRateOverTime);
                    }
                    else
                    {
                        emission.enabled =
                            true;

                        emission.rateOverTime =
                            new ParticleSystem.MinMaxCurve(
                                targetRateOverTime);
                    }

                    emission.rateOverDistance =
                        baseline.RateOverDistance;
                }
                catch
                {
                }
            }
        }

        private void ApplyScale(
            float scale)
        {
            float scaleMultiplier =
                ClampScale(
                    scale);

            foreach (ParticleSystem particleSystem in
                     _particleSystems)
            {
                if (particleSystem == null)
                    continue;

                int particleSystemId =
                    particleSystem.GetInstanceID();

                try
                {
                    if (_baseStartSizeByParticleSystemId.TryGetValue(
                            particleSystemId,
                            out ParticleSystem.MinMaxCurve baseStartSize))
                    {
                        var main =
                            particleSystem.main;

                        main.startSize =
                            ScaleMinMaxCurve(
                                baseStartSize,
                                scaleMultiplier);
                    }
                }
                catch
                {
                }

                try
                {
                    if (_baseSizeOverLifetimeEnabledByParticleSystemId
                        .TryGetValue(
                            particleSystemId,
                            out bool wasEnabled))
                    {
                        var sizeOverLifetime =
                            particleSystem.sizeOverLifetime;

                        sizeOverLifetime.enabled =
                            wasEnabled;

                        if (wasEnabled &&
                            _baseSizeOverLifetimeByParticleSystemId
                                .TryGetValue(
                                    particleSystemId,
                                    out ParticleSystem.MinMaxCurve baseCurve))
                        {
                            sizeOverLifetime.size =
                                ScaleMinMaxCurve(
                                    baseCurve,
                                    scaleMultiplier);
                        }
                    }
                }
                catch
                {
                }
            }
        }

        private void ApplyLifetime(
            float lifetime)
        {
            float clampedLifetime =
                Mathf.Clamp(
                    lifetime,
                    PluginConfig.MinLifetime,
                    PluginConfig.MaxLifetime);

            foreach (ParticleSystem particleSystem in
                     _particleSystems)
            {
                if (particleSystem == null)
                    continue;

                int particleSystemId =
                    particleSystem.GetInstanceID();

                if (!_baseStartLifetimeByParticleSystemId.TryGetValue(
                        particleSystemId,
                        out ParticleSystem.MinMaxCurve baseline))
                {
                    continue;
                }

                try
                {
                    var main =
                        particleSystem.main;

                    main.startLifetime =
                        MultiplyCurve(
                            baseline,
                            clampedLifetime);
                }
                catch
                {
                }
            }
        }

        private void ApplySimulationSpeed(
            float simulationSpeed)
        {
            float clampedSpeed =
                ClampSimulationSpeed(
                    simulationSpeed);

            foreach (ParticleSystem particleSystem in
                     _particleSystems)
            {
                if (particleSystem == null)
                    continue;

                int particleSystemId =
                    particleSystem.GetInstanceID();

                if (!_baseSimulationSpeedByParticleSystemId.TryGetValue(
                        particleSystemId,
                        out float baseline))
                {
                    continue;
                }

                try
                {
                    var main =
                        particleSystem.main;

                    main.simulationSpeed =
                        baseline *
                        clampedSpeed;
                }
                catch
                {
                }
            }
        }

        private void ApplyHueShift(
            float sliderValue,
            float luminanceMultiplier)
        {
            float targetHue =
                NadaHueShiftUtility
                    .SliderValueToTargetHue(
                        sliderValue);

            ApplyParticleHueShift(
                targetHue,
                luminanceMultiplier);

            ApplyRendererHueShift(
                targetHue,
                luminanceMultiplier);

            ApplyLightHueShift(
                targetHue);
        }

        private void ApplyParticleHueShift(
            float targetHue,
            float luminanceMultiplier)
        {
            float particleLuminance =
                NadaLuminanceUtility
                    .RemapParticleLuminance(
                        luminanceMultiplier);

            foreach (ParticleSystem particleSystem in
                     _particleSystems)
            {
                if (particleSystem == null)
                    continue;

                int particleSystemId =
                    particleSystem.GetInstanceID();

                try
                {
                    if (_baseMainStartColorByParticleSystemId.TryGetValue(
                            particleSystemId,
                            out ParticleSystem.MinMaxGradient baseColor))
                    {
                        var main =
                            particleSystem.main;

                        main.startColor =
                            NadaLuminanceUtility.ApplyToGradient(
                                NadaHueShiftUtility
                                    .RetintMinMaxGradientToHue(
                                        baseColor,
                                        targetHue),
                                particleLuminance);
                    }
                }
                catch
                {
                }

                try
                {
                    if (_baseColorOverLifetimeByParticleSystemId.TryGetValue(
                            particleSystemId,
                            out ParticleSystem.MinMaxGradient baseColor))
                    {
                        var colorOverLifetime =
                            particleSystem.colorOverLifetime;

                        if (_baseColorOverLifetimeEnabledByParticleSystemId
                            .TryGetValue(
                                particleSystemId,
                                out bool wasEnabled))
                        {
                            colorOverLifetime.enabled =
                                wasEnabled;
                        }

                        colorOverLifetime.color =
                            NadaLuminanceUtility.ApplyToGradient(
                                NadaHueShiftUtility
                                    .RetintMinMaxGradientToHue(
                                        baseColor,
                                        targetHue),
                                particleLuminance);
                    }
                }
                catch
                {
                }

                try
                {
                    var customData =
                        particleSystem.customData;

                    if (_baseCustomDataEnabledByParticleSystemId.TryGetValue(
                            particleSystemId,
                            out bool wasEnabled))
                    {
                        customData.enabled =
                            wasEnabled;
                    }

                    if (_baseCustom1ModeByParticleSystemId.TryGetValue(
                            particleSystemId,
                            out ParticleSystemCustomDataMode mode1))
                    {
                        customData.SetMode(
                            ParticleSystemCustomData.Custom1,
                            mode1);
                    }

                    if (_baseCustom2ModeByParticleSystemId.TryGetValue(
                            particleSystemId,
                            out ParticleSystemCustomDataMode mode2))
                    {
                        customData.SetMode(
                            ParticleSystemCustomData.Custom2,
                            mode2);
                    }

                    if (_baseCustom1ColorByParticleSystemId.TryGetValue(
                            particleSystemId,
                            out ParticleSystem.MinMaxGradient color1))
                    {
                        customData.SetColor(
                            ParticleSystemCustomData.Custom1,
                            NadaHueShiftUtility
                                .RetintMinMaxGradientToHue(
                                    color1,
                                    targetHue));
                    }

                    if (_baseCustom2ColorByParticleSystemId.TryGetValue(
                            particleSystemId,
                            out ParticleSystem.MinMaxGradient color2))
                    {
                        customData.SetColor(
                            ParticleSystemCustomData.Custom2,
                            NadaHueShiftUtility
                                .RetintMinMaxGradientToHue(
                                    color2,
                                    targetHue));
                    }
                }
                catch
                {
                }
            }
        }

        private void ApplyRendererHueShift(
            float targetHue,
            float luminanceMultiplier)
        {
            float clampedLuminance =
                Mathf.Clamp(
                    luminanceMultiplier,
                    PluginConfig.MinLuminance,
                    PluginConfig.MaxLuminance);

            foreach (Renderer renderer in
                     _renderers)
            {
                if (renderer == null)
                    continue;

                if (!_baseMaterialByRendererId.TryGetValue(
                        renderer.GetInstanceID(),
                        out MaterialBaseline baseline) ||
                    baseline == null)
                {
                    continue;
                }

                try
                {
                    Material material =
                        renderer.material;

                    if (material == null)
                        continue;

                    material.renderQueue =
                        3100;

                    if (baseline.Color.HasValue)
                    {
                        Color color =
                            NadaHueShiftUtility
                                .RetintColorToHue(
                                    baseline.Color.Value,
                                    targetHue);

                        if (material.HasProperty(
                                "_Color"))
                        {
                            material.SetColor(
                                "_Color",
                                color);
                        }

                        material.color =
                            color;
                    }

                    if (baseline.BaseColor.HasValue &&
                        material.HasProperty(
                            "_BaseColor"))
                    {
                        material.SetColor(
                            "_BaseColor",
                            NadaHueShiftUtility
                                .RetintColorToHue(
                                    baseline.BaseColor.Value,
                                    targetHue));
                    }

                    if (baseline.TintColor.HasValue &&
                        material.HasProperty(
                            "_TintColor"))
                    {
                        material.SetColor(
                            "_TintColor",
                            NadaHueShiftUtility
                                .RetintColorToHue(
                                    baseline.TintColor.Value,
                                    targetHue));
                    }

                    if (baseline.EmissionColor.HasValue &&
                        material.HasProperty(
                            "_EmissionColor"))
                    {
                        Color emission =
                            NadaHueShiftUtility
                                .RetintColorToHue(
                                    baseline.EmissionColor.Value,
                                    targetHue);

                        material.EnableKeyword(
                            "_EMISSION");

                        material.SetColor(
                            "_EmissionColor",
                            emission *
                            clampedLuminance);
                    }
                }
                catch
                {
                }
            }
        }

        private void ApplyLightHueShift(
            float targetHue)
        {
            foreach (Light light in
                     _lights)
            {
                if (light == null)
                    continue;

                if (_baseLightColorByLightId.TryGetValue(
                        light.GetInstanceID(),
                        out Color baseline))
                {
                    light.color =
                        NadaHueShiftUtility
                            .RetintColorToHue(
                                baseline,
                                targetHue);
                }
            }
        }

        private void ApplyGroupEnabledState(
            bool enabled)
        {
            foreach (Renderer renderer in
                     _renderers)
            {
                if (renderer != null &&
                    renderer.gameObject.activeInHierarchy)
                {
                    renderer.enabled =
                        enabled;
                }
            }

            foreach (Light light in
                     _lights)
            {
                if (light != null &&
                    light.gameObject.activeInHierarchy)
                {
                    light.enabled =
                        enabled;
                }
            }

            if (!enabled)
            {
                foreach (ParticleSystem particleSystem in
                         _particleSystems)
                {
                    if (particleSystem == null ||
                        !particleSystem.gameObject.activeInHierarchy)
                    {
                        continue;
                    }

                    try
                    {
                        particleSystem.Stop(
                            true,
                            ParticleSystemStopBehavior
                                .StopEmittingAndClear);
                    }
                    catch
                    {
                    }
                }
            }
        }

        private void EnsureParticleSystemsPlaying()
        {
            foreach (ParticleSystem particleSystem in
                     _particleSystems)
            {
                if (particleSystem != null &&
                    particleSystem.gameObject.activeInHierarchy &&
                    !particleSystem.isPlaying)
                {
                    particleSystem.Play(
                        true);
                }
            }
        }

        private void DestroyOwnedMaterials()
        {
            foreach (Material material in
                     _ownedRendererMaterials)
            {
                if (material != null)
                {
                    Object.Destroy(
                        material);
                }
            }

            _ownedRendererMaterials.Clear();
        }

        private static void SetRootActive(
            Transform rootTransform,
            bool enabled)
        {
            if (rootTransform != null &&
                rootTransform.gameObject.activeSelf !=
                    enabled)
            {
                rootTransform.gameObject.SetActive(
                    enabled);
            }
        }

        private static bool RuntimeStateEquals(
            RuntimeState left,
            RuntimeState right)
        {
            return
                left.Enabled ==
                    right.Enabled &&

                left.Energy.Equals(
                    right.Energy) &&

                left.Scale.Equals(
                    right.Scale) &&

                left.Luminance.Equals(
                    right.Luminance) &&

                left.Hue.Equals(
                    right.Hue) &&

                left.Lifetime.Equals(
                    right.Lifetime) &&

                left.SimulationSpeed.Equals(
                    right.SimulationSpeed);
        }

        private static ParticleSystem.MinMaxCurve
            OverrideConstantBaseline(
                ParticleSystem.MinMaxCurve source,
                float constantValue)
        {
            switch (source.mode)
            {
                case ParticleSystemCurveMode.Constant:
                    return new ParticleSystem.MinMaxCurve(
                        constantValue);

                case ParticleSystemCurveMode.TwoConstants:
                    return new ParticleSystem.MinMaxCurve(
                        constantValue,
                        constantValue);

                default:
                    return source;
            }
        }

        private static ParticleSystem.MinMaxCurve MultiplyCurve(
            ParticleSystem.MinMaxCurve source,
            float multiplier)
        {
            ParticleSystem.MinMaxCurve result =
                source;

            result.constant *=
                multiplier;

            result.constantMin *=
                multiplier;

            result.constantMax *=
                multiplier;

            result.curveMultiplier *=
                multiplier;

            return result;
        }

        private static ParticleSystem.MinMaxCurve ScaleMinMaxCurve(
            ParticleSystem.MinMaxCurve source,
            float multiplier)
        {
            switch (source.mode)
            {
                case ParticleSystemCurveMode.Constant:
                    return new ParticleSystem.MinMaxCurve(
                        source.constant *
                        multiplier);

                case ParticleSystemCurveMode.TwoConstants:
                    return new ParticleSystem.MinMaxCurve(
                        source.constantMin *
                        multiplier,
                        source.constantMax *
                        multiplier);

                case ParticleSystemCurveMode.Curve:
                    return new ParticleSystem.MinMaxCurve(
                        source.curveMultiplier *
                        multiplier,
                        source.curve);

                case ParticleSystemCurveMode.TwoCurves:
                    return new ParticleSystem.MinMaxCurve(
                        source.curveMultiplier *
                        multiplier,
                        source.curveMin,
                        source.curveMax);

                default:
                    return source;
            }
        }

        private static float ClampScale(
            float value)
        {
            if (float.IsNaN(value) ||
                float.IsInfinity(value))
            {
                return 1f;
            }

            return Mathf.Clamp(
                value,
                PluginConfig.MinScaleMult,
                PluginConfig.MaxScaleMult);
        }

        private static float ClampSimulationSpeed(
            float value)
        {
            if (float.IsNaN(value) ||
                float.IsInfinity(value))
            {
                return PluginConfig.DefaultSimulationSpeed;
            }

            return Mathf.Clamp(
                value,
                PluginConfig.MinSimulationSpeed,
                PluginConfig.MaxSimulationSpeed);
        }
    }
}