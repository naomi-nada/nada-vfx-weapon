using System.Collections.Generic;
using NADA.VFX.Weapon.Core.Config;
using NADA.VFX.Weapon.Core.State;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Blocks.Effects;
using NADA.VFX.Weapon.Core.Visuals;
using NADA.VFX.Weapon.Runtime.Binding;
using NADA.VFX.Weapon.Runtime.Structure;
using UnityEngine;

namespace NADA.VFX.Weapon.Modules.Effects
{
    internal sealed class NadaStrandsEffect :
        MonoBehaviour,
        INadaItemDataReceiver,
        INadaResolvedStateReceiver
    {
        private const float AuthoredScaleMultiplier = 0.50f;

        private global::ItemDrop.ItemData _itemData;

        private VfxState _resolvedState;
        private bool _hasResolvedState;

        private VfxState _cachedBoundState;
        private bool _hasCachedBoundState;

        private StrandsRuntimeState _blockState;
        private bool _hasBlockState;

        private StrandsRuntimeState _lastAppliedStaticState;
        private bool _hasLastAppliedStaticState;

        private ParticleSystem[] _systems;
        private Renderer[] _renderers;

        private bool _componentCacheDirty = true;
        private bool _baselineCacheDirty = true;

        private Vector3 _baseLocalPosition;
        private Quaternion _baseLocalRotation;
        private bool _hasBasePlacement;

        private bool _hasDriftWorldPose;
        private Vector3 _driftWorldPosition;
        private Quaternion _driftWorldRotation;

        private readonly HashSet<Material> _ownedRendererMaterials = new();

        private readonly Dictionary<int, ParticleSystem.MinMaxGradient> _baseStartColors = new();
        private readonly Dictionary<int, ParticleSystem.MinMaxGradient> _baseColorOverLifetime = new();
        private readonly Dictionary<int, bool> _baseColorOverLifetimeEnabled = new();

        private readonly Dictionary<int, float> _baseEmissionRates = new();
        private readonly Dictionary<int, float> _baseStartLifetimes = new();

        private readonly Dictionary<int, MaterialBaseline> _baseMaterials = new();
        private readonly Dictionary<int, float> _baseShapeRadius = new();
        private readonly Dictionary<int, Vector3> _baseShapeScale = new();

        private readonly Dictionary<int, ParticleSystem.MinMaxCurve> _baseStartSizeX = new();
        private readonly Dictionary<int, ParticleSystem.MinMaxCurve> _baseStartSizeY = new();
        private readonly Dictionary<int, ParticleSystem.MinMaxCurve> _baseStartSizeZ = new();

        private struct StrandsRuntimeState
        {
            internal bool Enabled;
            internal bool SpectrumEnabled;

            internal float Energy;

            internal float ScaleWhole;
            internal float ScaleParts;

            internal float Luminance;
            internal float Hue;

            internal float Lifetime;
            internal float Length;

            internal float SpectrumSpeed;
            internal float Speed;
            internal float Radius;
            internal float Drift;

            internal float XOffset;
            internal float YOffset;
            internal float ZOffset;

            internal float XRotation;
            internal float YRotation;
            internal float ZRotation;
        }

        private sealed class MaterialBaseline
        {
            public Color? Color;
            public Color? TintColor;
            public Color? EmissionColor;
        }

        public void SetItemData(global::ItemDrop.ItemData itemData)
        {
            _itemData = itemData;

            _hasResolvedState = false;
            _hasBlockState = false;

            // The same item may have been explicitly rebound.
            // Don't keep its previous bound-state snapshot.
            _hasCachedBoundState = false;
        }

        public void SetResolvedState(VfxState state)
        {
            _resolvedState = state;

            _hasResolvedState = true;
            _hasBlockState = false;

            _itemData = null;
            _hasCachedBoundState = false;
        }

        internal bool SetBlockState(
            VfxEffectBlock block)
        {
            if (block == null ||
                block.Transform == null ||
                block.TypeId != VfxEffectTypeIds.Strands ||
                block.Settings is not StrandsVfxSettings settings)
            {
                // A malformed block must not silently fall back to local
                // config or legacy ItemData. Block mode fails closed.
                _blockState = default;
                _hasBlockState = true;

                _itemData = null;
                _hasResolvedState = false;
                _hasCachedBoundState = false;

                _hasDriftWorldPose = false;

                return false;
            }

            _blockState =
                new StrandsRuntimeState
                {
                    Enabled =
                        block.Enabled,

                    SpectrumEnabled =
                        settings.SpectrumEnabled,

                    Energy =
                        settings.Energy,

                    ScaleWhole =
                        settings.ScaleWhole,

                    ScaleParts =
                        settings.ScaleParts,

                    Luminance =
                        settings.Luminance,

                    Hue =
                        settings.Hue,

                    Lifetime =
                        settings.Lifetime,

                    Length =
                        settings.Length,

                    SpectrumSpeed =
                        settings.SpectrumSpeed,

                    Speed =
                        settings.Speed,

                    Radius =
                        settings.Radius,

                    Drift =
                        settings.Drift,

                    XOffset =
                        block.Transform.XOffset,

                    YOffset =
                        block.Transform.YOffset,

                    ZOffset =
                        block.Transform.ZOffset,

                    XRotation =
                        block.Transform.XRotation,

                    YRotation =
                        block.Transform.YRotation,

                    ZRotation =
                        block.Transform.ZRotation
                };

            _hasBlockState = true;

            _itemData = null;
            _hasResolvedState = false;
            _hasCachedBoundState = false;

            return true;
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
            try
            {
                CancelInvoke(nameof(TickApply));
            }
            catch
            {
            }

            foreach (Material material in _ownedRendererMaterials)
            {
                if (material != null)
                {
                    Object.Destroy(material);
                }
            }

            _ownedRendererMaterials.Clear();
        }

        private void TickApply()
        {
            bool forceStaticApply = false;

            if (_componentCacheDirty)
            {
                RebuildComponentCache();
                _componentCacheDirty = false;
                _baselineCacheDirty = true;

                forceStaticApply = true;
            }

            if (_baselineCacheDirty)
            {
                CacheBaselines();
                _baselineCacheDirty = false;

                forceStaticApply = true;
            }

            StrandsRuntimeState state =
                ResolveRuntimeState();

            bool staticStateChanged =
                forceStaticApply ||
                !_hasLastAppliedStaticState ||
                !StrandsStaticStateEquals(
                    state,
                    _lastAppliedStaticState);

            if (staticStateChanged)
            {
                ApplyEnabled(state.Enabled);
                ApplyEnergy(state.Energy);
            }

            // Drift is actual runtime motion, so it still needs to run
            // continuously even when the underlying state is unchanged.
            ApplyDrift(state.Drift);

            if (staticStateChanged)
            {
                ApplyScaleWhole(state.ScaleWhole);
                ApplyScaleParts(state.ScaleParts);
            }

            // Spectrum is time-driven. Static hue only needs to be
            // reapplied when its actual state changes.
            if (state.SpectrumEnabled)
            {
                ApplySpectrum(
                    state.SpectrumSpeed,
                    state.Luminance);
            }
            else if (staticStateChanged)
            {
                ApplyHue(
                    state.Hue,
                    state.Luminance);
            }

            if (staticStateChanged)
            {
                ApplySpeed(state.Speed);
                ApplyLength(state.Length);
                ApplyRadius(state.Radius);
                ApplyLifetime(state.Lifetime);
            }

            // Keep placement on the existing continuous path for now.
            // Drift and placement interact, so we'll inspect that properly
            // during the motion audit instead of changing the feel here.
            ApplyPlacement(
                state.XOffset,
                state.YOffset,
                state.ZOffset,
                state.XRotation,
                state.YRotation,
                state.ZRotation);

            if (staticStateChanged)
            {
                _lastAppliedStaticState = state;
                _hasLastAppliedStaticState = true;
            }
        }

        private static bool StrandsStaticStateEquals(
            StrandsRuntimeState left,
            StrandsRuntimeState right)
        {
            return
                left.Enabled ==
                    right.Enabled &&
                left.SpectrumEnabled ==
                    right.SpectrumEnabled &&

                FloatEquals(
                    left.Energy,
                    right.Energy) &&
                FloatEquals(
                    left.ScaleWhole,
                    right.ScaleWhole) &&
                FloatEquals(
                    left.ScaleParts,
                    right.ScaleParts) &&

                FloatEquals(
                    left.Hue,
                    right.Hue) &&
                FloatEquals(
                    left.Luminance,
                    right.Luminance) &&

                FloatEquals(
                    left.Speed,
                    right.Speed) &&
                FloatEquals(
                    left.Length,
                    right.Length) &&
                FloatEquals(
                    left.Radius,
                    right.Radius) &&
                FloatEquals(
                    left.Lifetime,
                    right.Lifetime);
        }

        private static bool FloatEquals(
            float left,
            float right)
        {
            return left.Equals(right);
        }

        private StrandsRuntimeState ResolveRuntimeState()
        {
            if (_hasBlockState)
                return _blockState;

            if (_hasResolvedState)
            {
                return FromLegacyState(
                    _resolvedState);
            }

            if (_itemData != null &&
                VfxStateIO.IsBound(_itemData))
            {
                if (_hasCachedBoundState)
                {
                    return FromLegacyState(
                        _cachedBoundState);
                }

                if (VfxStateIO.TryRead(
                        _itemData,
                        out VfxState itemState))
                {
                    _cachedBoundState = itemState;
                    _hasCachedBoundState = true;

                    return FromLegacyState(
                        itemState);
                }
            }

            // Unbinding must discard the previous snapshot.
            _hasCachedBoundState = false;

            return FromLegacyState(
                VfxStateIO.FromConfig());
        }

        private static StrandsRuntimeState FromLegacyState(
            VfxState state)
        {
            return new StrandsRuntimeState
            {
                Enabled =
                    state.StrandsEnabled,

                SpectrumEnabled =
                    state.StrandsSpectrumEnabled,

                Energy =
                    state.StrandsEnergy,

                ScaleWhole =
                    state.StrandsScaleWhole,

                ScaleParts =
                    state.StrandsScaleParts,

                Luminance =
                    state.StrandsLuminance,

                Hue =
                    state.StrandsHue,

                Lifetime =
                    state.StrandsLifetime,

                Length =
                    state.StrandsLength,

                SpectrumSpeed =
                    state.StrandsSpectrumSpeed,

                Speed =
                    state.StrandsSpeed,

                Radius =
                    state.StrandsRadius,

                Drift =
                    state.StrandsDrift,

                XOffset =
                    state.StrandsXOffset,

                YOffset =
                    state.StrandsYOffset,

                ZOffset =
                    state.StrandsZOffset,

                XRotation =
                    state.StrandsXRotation,

                YRotation =
                    state.StrandsYRotation,

                ZRotation =
                    state.StrandsZRotation
            };
        }

        // Cache / baselines

        private void RebuildComponentCache()
        {
            _systems = GetComponentsInChildren<ParticleSystem>(true);
            _renderers = GetComponentsInChildren<Renderer>(true);
        }

        private void CacheBaselines()
        {
            CacheBasePlacement();
            CacheParticleBaselines();
            CacheRendererBaselines();
        }

        private void CacheBasePlacement()
        {
            if (_hasBasePlacement)
                return;

            _baseLocalPosition = transform.localPosition;
            _baseLocalRotation = transform.localRotation;
            _hasBasePlacement = true;
        }

        private void CacheParticleBaselines()
        {
            if (_systems == null)
                return;

            foreach (ParticleSystem system in _systems)
            {
                if (system == null)
                    continue;

                var mainModule = system.main;
                mainModule.loop = true;
                mainModule.playOnAwake = true;

                int systemId = system.GetInstanceID();

                var shape = system.shape;

                if (!_baseShapeRadius.ContainsKey(systemId))
                    _baseShapeRadius[systemId] = shape.radius;

                if (!_baseShapeScale.ContainsKey(systemId))
                    _baseShapeScale[systemId] = shape.scale;

                if (!_baseStartSizeX.ContainsKey(systemId))
                    _baseStartSizeX[systemId] = mainModule.startSize;

                if (!_baseStartSizeY.ContainsKey(systemId))
                    _baseStartSizeY[systemId] = mainModule.startSizeY;

                if (!_baseStartSizeZ.ContainsKey(systemId))
                    _baseStartSizeZ[systemId] = mainModule.startSizeZ;

                if (!_baseStartColors.ContainsKey(systemId))
                    _baseStartColors[systemId] = mainModule.startColor;

                if (!_baseEmissionRates.ContainsKey(systemId))
                {
                    var emission = system.emission;
                    _baseEmissionRates[systemId] = emission.rateOverTime.constant;
                }

                if (!_baseStartLifetimes.ContainsKey(systemId))
                    _baseStartLifetimes[systemId] = mainModule.startLifetime.constant;

                if (!_baseColorOverLifetime.ContainsKey(systemId))
                {
                    var colorOverLifetime = system.colorOverLifetime;

                    _baseColorOverLifetimeEnabled[systemId] = colorOverLifetime.enabled;
                    _baseColorOverLifetime[systemId] = colorOverLifetime.color;
                }
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

                int rendererId = renderer.GetInstanceID();

                if (_baseMaterials.ContainsKey(rendererId))
                    continue;

                Material previousMaterial;
                Material material;

                try
                {
                    previousMaterial = renderer.sharedMaterial;
                    material = renderer.material;
                }
                catch
                {
                    continue;
                }

                if (material == null)
                    continue;

                // Only own an instance created by this access.
                // The original donor material remains borrowed.
                if (material != previousMaterial)
                {
                    _ownedRendererMaterials.Add(material);
                }

                var baseline = new MaterialBaseline();

                if (material.HasProperty("_Color"))
                    baseline.Color = material.GetColor("_Color");

                if (material.HasProperty("_TintColor"))
                    baseline.TintColor = material.GetColor("_TintColor");

                if (material.HasProperty("_EmissionColor"))
                    baseline.EmissionColor = material.GetColor("_EmissionColor");

                _baseMaterials[rendererId] = baseline;
            }
        }

        // Enabled / energy

        private void ApplyEnabled(bool enabled)
        {
            ApplyParticleSystemEnabledState(enabled);
            ApplyRendererEnabledState(enabled);
        }

        private void ApplyParticleSystemEnabledState(bool enabled)
        {
            if (_systems == null)
                return;

            foreach (ParticleSystem system in _systems)
            {
                if (system == null)
                    continue;

                try
                {
                    if (enabled)
                    {
                        if (!system.gameObject.activeSelf)
                            system.gameObject.SetActive(true);

                        if (!system.isPlaying)
                            system.Play(true);
                    }
                    else
                    {
                        if (system.isPlaying)
                            system.Stop(
                                true,
                                ParticleSystemStopBehavior.StopEmittingAndClear);
                    }
                }
                catch
                {
                }
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

                try
                {
                    renderer.enabled = enabled;
                }
                catch
                {
                }
            }
        }

        private void ApplyEnergy(float energy)
        {
            float clamped = Mathf.Clamp01(energy);
            float multiplier = Mathf.Lerp(0.25f, 2.5f, clamped);

            if (_systems == null)
                return;

            foreach (ParticleSystem system in _systems)
            {
                if (system == null)
                    continue;

                int systemId = system.GetInstanceID();

                if (!_baseEmissionRates.TryGetValue(
                        systemId,
                        out float baseRate))
                {
                    baseRate = 10f;
                }

                try
                {
                    var emission = system.emission;
                    emission.enabled = true;

                    emission.rateOverTime =
                        new ParticleSystem.MinMaxCurve(
                            baseRate * multiplier);
                }
                catch
                {
                }
            }
        }

        // Drift / placement

        private void ApplyDrift(float drift)
        {
            float clampedDrift = Mathf.Clamp01(drift);
            bool lockedToWeapon = clampedDrift <= 0.001f;

            ApplySimulationSpace(
                lockedToWeapon
                    ? ParticleSystemSimulationSpace.Local
                    : ParticleSystemSimulationSpace.World);

            if (lockedToWeapon)
            {
                _hasDriftWorldPose = false;
                return;
            }

            Transform parentTransform = transform.parent;

            if (parentTransform == null)
            {
                _hasDriftWorldPose = false;
                return;
            }

            Vector3 targetWorldPosition = transform.position;
            Quaternion targetWorldRotation = transform.rotation;

            if (!_hasDriftWorldPose)
            {
                _driftWorldPosition = targetWorldPosition;
                _driftWorldRotation = targetWorldRotation;
                _hasDriftWorldPose = true;
            }

            float driftT =
                Mathf.Pow(
                    clampedDrift,
                    0.35f);

            float followSpeed =
                Mathf.Lerp(
                    30f,
                    0.15f,
                    driftT);

            float t =
                1f -
                Mathf.Exp(
                    -followSpeed * Time.deltaTime);

            _driftWorldPosition =
                Vector3.Lerp(
                    _driftWorldPosition,
                    targetWorldPosition,
                    t);

            _driftWorldRotation =
                Quaternion.Slerp(
                    _driftWorldRotation,
                    targetWorldRotation,
                    t);

            transform.position =
                _driftWorldPosition;

            transform.rotation =
                _driftWorldRotation;
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

        private void ApplySimulationSpace(
            ParticleSystemSimulationSpace simulationSpace)
        {
            if (_systems == null)
                return;

            foreach (ParticleSystem system in _systems)
            {
                if (system == null)
                    continue;

                try
                {
                    var mainModule = system.main;

                    if (mainModule.simulationSpace == simulationSpace)
                        continue;

                    mainModule.simulationSpace = simulationSpace;

                    system.Clear(true);
                    system.Play(true);
                }
                catch
                {
                }
            }
        }

        // Color / spectrum

        private void ApplyHue(
            float hue,
            float luminance)
        {
            float targetHue =
                NadaHueShiftUtility.SliderValueToTargetHue(
                    hue);

            ApplyHueFromNormalizedHue(
                targetHue,
                luminance);
        }

        private void ApplySpectrum(
            float speed,
            float luminance)
        {
            float clampedSpeed =
                Mathf.Clamp(
                    speed,
                    PluginConfig.MinSpectrumSpeed,
                    PluginConfig.MaxSpectrumSpeed);

            float hue =
                Mathf.Repeat(
                    Time.time * clampedSpeed,
                    1f);

            ApplyHueFromNormalizedHue(
                hue,
                luminance);
        }

        private void ApplyHueFromNormalizedHue(
            float targetHue,
            float luminance)
        {
            float particleLuminance =
                NadaLuminanceUtility.RemapParticleLuminance(
                    luminance);

            float rendererEmissionMultiplier =
                Mathf.Clamp(
                    luminance,
                    PluginConfig.MinLuminance,
                    PluginConfig.MaxLuminance);

            ApplyParticleHue(
                targetHue,
                particleLuminance);

            ApplyRendererHue(
                targetHue,
                rendererEmissionMultiplier);
        }

        private void ApplyParticleHue(
            float targetHue,
            float particleLuminance)
        {
            if (_systems == null)
                return;

            foreach (ParticleSystem system in _systems)
            {
                if (system == null)
                    continue;

                int systemId = system.GetInstanceID();

                try
                {
                    if (_baseStartColors.TryGetValue(
                            systemId,
                            out var baseStartColor))
                    {
                        var mainModule = system.main;

                        mainModule.startColor =
                            NadaLuminanceUtility.ApplyToGradient(
                                NadaHueShiftUtility.RetintMinMaxGradientToHue(
                                    baseStartColor,
                                    targetHue),
                                particleLuminance);
                    }
                }
                catch
                {
                }

                try
                {
                    if (_baseColorOverLifetime.TryGetValue(
                            systemId,
                            out var baseColor))
                    {
                        var colorOverLifetime =
                            system.colorOverLifetime;

                        if (_baseColorOverLifetimeEnabled.TryGetValue(
                                systemId,
                                out bool wasEnabled))
                        {
                            colorOverLifetime.enabled =
                                wasEnabled;
                        }

                        colorOverLifetime.color =
                            NadaLuminanceUtility.ApplyToGradient(
                                NadaHueShiftUtility.RetintMinMaxGradientToHue(
                                    baseColor,
                                    targetHue),
                                particleLuminance);
                    }
                }
                catch
                {
                }
            }
        }

        private void ApplyRendererHue(
            float targetHue,
            float rendererEmissionMultiplier)
        {
            if (_renderers == null)
                return;

            foreach (Renderer renderer in _renderers)
            {
                if (renderer == null)
                    continue;

                int rendererId = renderer.GetInstanceID();

                if (!_baseMaterials.TryGetValue(
                        rendererId,
                        out MaterialBaseline baseline) ||
                    baseline == null)
                {
                    continue;
                }

                Material material;

                try
                {
                    material = renderer.material;
                }
                catch
                {
                    continue;
                }

                if (material == null)
                    continue;

                try
                {
                    if (baseline.Color.HasValue &&
                        material.HasProperty("_Color"))
                    {
                        material.SetColor(
                            "_Color",
                            NadaLuminanceUtility.ApplyToColor(
                                NadaHueShiftUtility.RetintColorToHue(
                                    baseline.Color.Value,
                                    targetHue),
                                rendererEmissionMultiplier));
                    }

                    if (baseline.TintColor.HasValue &&
                        material.HasProperty("_TintColor"))
                    {
                        material.SetColor(
                            "_TintColor",
                            NadaLuminanceUtility.ApplyToColor(
                                NadaHueShiftUtility.RetintColorToHue(
                                    baseline.TintColor.Value,
                                    targetHue),
                                rendererEmissionMultiplier));
                    }

                    if (baseline.EmissionColor.HasValue &&
                        material.HasProperty("_EmissionColor"))
                    {
                        material.EnableKeyword("_EMISSION");

                        material.SetColor(
                            "_EmissionColor",
                            NadaHueShiftUtility.RetintColorToHue(
                                baseline.EmissionColor.Value,
                                targetHue) *
                            rendererEmissionMultiplier);
                    }
                }
                catch
                {
                }
            }
        }

        // Scale / shape

        private void ApplyScaleWhole(float scale)
        {
            float clamped =
                Mathf.Clamp(
                    scale,
                    PluginConfig.MinScaleMult,
                    PluginConfig.MaxScaleMult);

            transform.localScale =
                Vector3.one *
                (clamped * AuthoredScaleMultiplier);
        }

        private void ApplyScaleParts(float scale)
        {
            float clamped =
                Mathf.Clamp(
                    scale,
                    PluginConfig.MinScaleMult,
                    PluginConfig.MaxScaleMult);

            if (_systems == null)
                return;

            foreach (ParticleSystem system in _systems)
            {
                if (system == null)
                    continue;

                int systemId = system.GetInstanceID();

                if (!_baseStartSizeX.TryGetValue(
                        systemId,
                        out ParticleSystem.MinMaxCurve baseSizeX))
                {
                    continue;
                }

                if (!_baseStartSizeY.TryGetValue(
                        systemId,
                        out ParticleSystem.MinMaxCurve baseSizeY))
                {
                    baseSizeY = baseSizeX;
                }

                if (!_baseStartSizeZ.TryGetValue(
                        systemId,
                        out ParticleSystem.MinMaxCurve baseSizeZ))
                {
                    baseSizeZ = baseSizeX;
                }

                try
                {
                    var mainModule = system.main;

                    mainModule.startSize3D = true;

                    mainModule.startSizeX =
                        MultiplyCurve(
                            baseSizeX,
                            clamped);

                    mainModule.startSizeY =
                        MultiplyCurve(
                            baseSizeY,
                            clamped);

                    mainModule.startSizeZ =
                        MultiplyCurve(
                            baseSizeZ,
                            clamped);
                }
                catch
                {
                }
            }
        }

        private void ApplyLength(float length)
        {
            float clamped =
                Mathf.Clamp(
                    length,
                    PluginConfig.MinOrbitalsLength,
                    PluginConfig.MaxOrbitalsLength);

            if (_systems == null)
                return;

            foreach (ParticleSystem system in _systems)
            {
                if (system == null)
                    continue;

                int systemId = system.GetInstanceID();

                if (!_baseShapeScale.TryGetValue(
                        systemId,
                        out Vector3 baseScale))
                {
                    baseScale = Vector3.one;
                }

                try
                {
                    var shape = system.shape;

                    shape.enabled = true;

                    shape.scale =
                        new Vector3(
                            baseScale.x,
                            baseScale.y * clamped,
                            baseScale.z);
                }
                catch
                {
                }
            }
        }

        private void ApplyRadius(float radius)
        {
            float clamped =
                Mathf.Clamp(
                    radius,
                    PluginConfig.MinOrbitalsRadiusMultiplier,
                    PluginConfig.MaxOrbitalsRadiusMultiplier);

            if (_systems == null)
                return;

            foreach (ParticleSystem system in _systems)
            {
                if (system == null)
                    continue;

                int systemId = system.GetInstanceID();

                if (!_baseShapeRadius.TryGetValue(
                        systemId,
                        out float baseRadius))
                {
                    baseRadius = 0.5f;
                }

                try
                {
                    var shape = system.shape;

                    shape.enabled = true;
                    shape.radius =
                        baseRadius * clamped;
                }
                catch
                {
                }
            }
        }

        // Timing

        private void ApplySpeed(float speed)
        {
            float normalized =
                Mathf.InverseLerp(
                    PluginConfig.MinOrbitalsSpeed,
                    PluginConfig.MaxOrbitalsSpeed,
                    speed);

            float speedMultiplier =
                Mathf.Lerp(
                    0.25f,
                    2.5f,
                    normalized);

            if (_systems == null)
                return;

            foreach (ParticleSystem system in _systems)
            {
                if (system == null)
                    continue;

                try
                {
                    var mainModule = system.main;
                    mainModule.simulationSpeed =
                        speedMultiplier;
                }
                catch
                {
                }
            }
        }

        private void ApplyLifetime(float lifetime)
        {
            float clamped =
                Mathf.Clamp(
                    lifetime,
                    PluginConfig.MinLifetime,
                    PluginConfig.MaxLifetime);

            if (_systems == null)
                return;

            foreach (ParticleSystem system in _systems)
            {
                if (system == null)
                    continue;

                int systemId = system.GetInstanceID();

                if (!_baseStartLifetimes.TryGetValue(
                        systemId,
                        out float baseLifetime))
                {
                    baseLifetime = 2f;
                }

                try
                {
                    var mainModule = system.main;

                    mainModule.startLifetime =
                        new ParticleSystem.MinMaxCurve(
                            baseLifetime * clamped);
                }
                catch
                {
                }
            }
        }

        // Helpers

        private static float ClampOffset(float value)
        {
            if (float.IsNaN(value) ||
                float.IsInfinity(value))
            {
                return PluginConfig.DefaultEffectOffset;
            }

            return Mathf.Clamp(
                value,
                PluginConfig.MinEffectOffset,
                PluginConfig.MaxEffectOffset);
        }

        private static float ClampRotation(float value)
        {
            if (float.IsNaN(value) ||
                float.IsInfinity(value))
            {
                return PluginConfig.DefaultEffectRotation;
            }

            return Mathf.Clamp(
                value,
                PluginConfig.MinEffectRotation,
                PluginConfig.MaxEffectRotation);
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
    }
}