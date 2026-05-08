using System.Collections.Generic;
using UnityEngine;
using NADA.VFX.Core.Config;
using NADA.VFX.Core.State;
using NADA.VFX.Core.Visuals;
using NADA.VFX.Runtime.Binding;

namespace NADA.VFX.Modules.Effects
{
    internal sealed class NadaOrbitalsStrandsEffect : MonoBehaviour, INadaItemDataReceiver
    {
        private global::ItemDrop.ItemData _itemData;

        private ParticleSystem[] _systems;
        private Renderer[] _renderers;

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
        
        private bool _hasDriftWorldPose;
        private Vector3 _driftWorldPosition;
        private Quaternion _driftWorldRotation;
        
        private const float AuthoredScaleMultiplier = 0.50f;

        private sealed class MaterialBaseline
        {
            public Color? Color;
            public Color? TintColor;
            public Color? EmissionColor;
        }

        public void SetItemData(global::ItemDrop.ItemData itemData)
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

            ApplyEnabled(state.OrbitalsStrandsEnabled);
            ApplyEnergy(state.OrbitalsStrandsEnergy);
            ApplyDrift(
                state.OrbitalsStrandsDrift,
                state.OrbitalsStrandsPosition);
            ApplyScaleWhole(state.OrbitalsStrandsScaleWhole);
            ApplyScaleParts(state.OrbitalsStrandsScaleParts);
            ApplyHue(state.OrbitalsStrandsHue);
            ApplySpeed(state.OrbitalsStrandsSpeed);
            ApplyLength(state.OrbitalsStrandsLength);
            ApplyRadius(state.OrbitalsStrandsRadius);
            ApplyLifetime(state.OrbitalsStrandsLifetime);
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
            _systems = GetComponentsInChildren<ParticleSystem>(true);
            _renderers = GetComponentsInChildren<Renderer>(true);
        }

        private void CacheBaselines()
        {
            if (_systems != null)
            {
                foreach (ParticleSystem system in _systems)
                {
                    if (system == null)
                        continue;

                    var mainModule = system.main;
                    mainModule.loop = true;
                    mainModule.playOnAwake = true;

                    int id = system.GetInstanceID();
                    
                    var shape = system.shape;

                    if (!_baseShapeRadius.ContainsKey(id))
                        _baseShapeRadius[id] = shape.radius;

                    if (!_baseShapeScale.ContainsKey(id))
                        _baseShapeScale[id] = shape.scale;
                    
                    if (!_baseStartSizeX.ContainsKey(id))
                        _baseStartSizeX[id] = mainModule.startSize;

                    if (!_baseStartSizeY.ContainsKey(id))
                        _baseStartSizeY[id] = mainModule.startSizeY;

                    if (!_baseStartSizeZ.ContainsKey(id))
                        _baseStartSizeZ[id] = mainModule.startSizeZ;

                    if (!_baseStartColors.ContainsKey(id))
                        _baseStartColors[id] = mainModule.startColor;

                    if (!_baseEmissionRates.ContainsKey(id))
                    {
                        var emission = system.emission;
                        _baseEmissionRates[id] = emission.rateOverTime.constant;
                    }

                    if (!_baseStartLifetimes.ContainsKey(id))
                        _baseStartLifetimes[id] = mainModule.startLifetime.constant;

                    if (!_baseColorOverLifetime.ContainsKey(id))
                    {
                        var color = system.colorOverLifetime;
                        _baseColorOverLifetimeEnabled[id] = color.enabled;
                        _baseColorOverLifetime[id] = color.color;
                    }
                }
            }

            if (_renderers != null)
            {
                foreach (Renderer renderer in _renderers)
                {
                    if (renderer == null)
                        continue;

                    int id = renderer.GetInstanceID();
                    if (_baseMaterials.ContainsKey(id))
                        continue;

                    Material material = renderer.material;
                    if (material == null)
                        continue;

                    var baseline = new MaterialBaseline();

                    if (material.HasProperty("_Color"))
                        baseline.Color = material.GetColor("_Color");

                    if (material.HasProperty("_TintColor"))
                        baseline.TintColor = material.GetColor("_TintColor");

                    if (material.HasProperty("_EmissionColor"))
                        baseline.EmissionColor = material.GetColor("_EmissionColor");

                    _baseMaterials[id] = baseline;
                }
            }
        }

        private void ApplyEnabled(bool enabled)
        {
            if (_systems != null)
            {
                foreach (ParticleSystem system in _systems)
                {
                    if (system == null)
                        continue;

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
                            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    }
                }
            }

            if (_renderers != null)
            {
                foreach (Renderer renderer in _renderers)
                {
                    if (renderer != null)
                        renderer.enabled = enabled;
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

                int id = system.GetInstanceID();

                if (!_baseEmissionRates.TryGetValue(id, out float baseRate))
                    baseRate = 10f;

                var emission = system.emission;
                emission.enabled = true;
                emission.rateOverTime = new ParticleSystem.MinMaxCurve(baseRate * multiplier);
            }
        }
        
        private void ApplyDrift(float drift, float position)
        {
            float clampedDrift = Mathf.Clamp01(drift);

            Vector3 desiredLocalPosition = ResolveDesiredLocalPosition(position);
            Quaternion desiredLocalRotation = Quaternion.Euler(90f, 0f, 0f);

            bool lockedToWeapon = clampedDrift <= 0.001f;

            ApplySimulationSpace(lockedToWeapon
                ? ParticleSystemSimulationSpace.Local
                : ParticleSystemSimulationSpace.World);

            Transform parentTransform = transform.parent;

            if (parentTransform == null || lockedToWeapon)
            {
                transform.localPosition = desiredLocalPosition;
                transform.localRotation = desiredLocalRotation;
                _hasDriftWorldPose = false;
                return;
            }

            Vector3 targetWorldPosition = parentTransform.TransformPoint(desiredLocalPosition);
            Quaternion targetWorldRotation = parentTransform.rotation * desiredLocalRotation;

            if (!_hasDriftWorldPose)
            {
                _driftWorldPosition = targetWorldPosition;
                _driftWorldRotation = targetWorldRotation;
                _hasDriftWorldPose = true;
            }

            float driftT = clampedDrift * clampedDrift;
            float followSpeed = Mathf.Lerp(30f, 2f, driftT);
            float t = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);

            _driftWorldPosition = Vector3.Lerp(
                _driftWorldPosition,
                targetWorldPosition,
                t);

            _driftWorldRotation = Quaternion.Slerp(
                _driftWorldRotation,
                targetWorldRotation,
                t);

            transform.position = _driftWorldPosition;
            transform.rotation = _driftWorldRotation;
        }

        private void ApplyHue(float hue)
        {
            float targetHue = NadaHueShiftUtility.SliderValueToTargetHue(hue);

            if (_systems != null)
            {
                foreach (ParticleSystem system in _systems)
                {
                    if (system == null)
                        continue;

                    int id = system.GetInstanceID();

                    if (_baseStartColors.TryGetValue(id, out var baseStartColor))
                    {
                        var mainModule = system.main;
                        mainModule.startColor =
                            NadaHueShiftUtility.RetintMinMaxGradientToHue(baseStartColor, targetHue);
                    }

                    if (_baseColorOverLifetime.TryGetValue(id, out var baseColor))
                    {
                        var color = system.colorOverLifetime;

                        if (_baseColorOverLifetimeEnabled.TryGetValue(id, out bool wasEnabled))
                            color.enabled = wasEnabled;

                        color.color =
                            NadaHueShiftUtility.RetintMinMaxGradientToHue(baseColor, targetHue);
                    }
                }
            }

            if (_renderers != null)
            {
                foreach (Renderer renderer in _renderers)
                {
                    if (renderer == null)
                        continue;

                    int id = renderer.GetInstanceID();
                    if (!_baseMaterials.TryGetValue(id, out MaterialBaseline baseline) || baseline == null)
                        continue;

                    Material material = renderer.material;
                    if (material == null)
                        continue;

                    if (baseline.Color.HasValue && material.HasProperty("_Color"))
                    {
                        material.SetColor(
                            "_Color",
                            NadaHueShiftUtility.RetintColorToHue(baseline.Color.Value, targetHue));
                    }

                    if (baseline.TintColor.HasValue && material.HasProperty("_TintColor"))
                    {
                        material.SetColor(
                            "_TintColor",
                            NadaHueShiftUtility.RetintColorToHue(baseline.TintColor.Value, targetHue));
                    }

                    if (baseline.EmissionColor.HasValue && material.HasProperty("_EmissionColor"))
                    {
                        material.SetColor(
                            "_EmissionColor",
                            NadaHueShiftUtility.RetintColorToHue(baseline.EmissionColor.Value, targetHue));
                    }
                }
            }
        }

        private void ApplyScaleWhole(float scale)
        {
            float clamped = Mathf.Clamp(
                scale,
                PluginConfig.MinScaleMult,
                PluginConfig.MaxScaleMult);

            transform.localScale = Vector3.one * (clamped * AuthoredScaleMultiplier);
        }
        
        private void ApplyScaleParts(float scale)
        {
            float clamped = Mathf.Clamp(
                scale,
                PluginConfig.MinScaleMult,
                PluginConfig.MaxScaleMult);

            if (_systems == null)
                return;

            foreach (ParticleSystem system in _systems)
            {
                if (system == null)
                    continue;

                int id = system.GetInstanceID();

                if (!_baseStartSizeX.TryGetValue(id, out ParticleSystem.MinMaxCurve baseSizeX))
                    continue;

                if (!_baseStartSizeY.TryGetValue(id, out ParticleSystem.MinMaxCurve baseSizeY))
                    baseSizeY = baseSizeX;

                if (!_baseStartSizeZ.TryGetValue(id, out ParticleSystem.MinMaxCurve baseSizeZ))
                    baseSizeZ = baseSizeX;

                var mainModule = system.main;
                mainModule.startSize3D = true;
                mainModule.startSize = MultiplyCurve(baseSizeX, clamped);
                mainModule.startSizeY = MultiplyCurve(baseSizeY, clamped);
                mainModule.startSizeZ = MultiplyCurve(baseSizeZ, clamped);
            }
        }

        private void ApplySpeed(float speed)
        {
            float normalized = Mathf.InverseLerp(
                PluginConfig.MinOrbitalsSpeed,
                PluginConfig.MaxOrbitalsSpeed,
                speed);

            float speedMultiplier = Mathf.Lerp(0.25f, 2.5f, normalized);

            if (_systems == null)
                return;

            foreach (ParticleSystem system in _systems)
            {
                if (system == null)
                    continue;

                var mainModule = system.main;
                mainModule.simulationSpeed = speedMultiplier;
            }
        }
        
        private void ApplyLength(float length)
        {
            float clamped = Mathf.Clamp(
                length,
                PluginConfig.MinOrbitalsLength,
                PluginConfig.MaxOrbitalsLength);

            if (_systems == null)
                return;

            foreach (ParticleSystem system in _systems)
            {
                if (system == null)
                    continue;

                int id = system.GetInstanceID();

                if (!_baseShapeScale.TryGetValue(id, out Vector3 baseScale))
                    baseScale = Vector3.one;

                var shape = system.shape;
                shape.enabled = true;
                shape.scale = new Vector3(
                    baseScale.x,
                    baseScale.y * clamped,
                    baseScale.z);
            }
        }

        private void ApplyLifetime(float lifetime)
        {
            float clamped = Mathf.Clamp(
                lifetime,
                PluginConfig.MinStrandsLifetime,
                PluginConfig.MaxStrandsLifetime);

            if (_systems == null)
                return;

            foreach (ParticleSystem system in _systems)
            {
                if (system == null)
                    continue;

                int id = system.GetInstanceID();

                if (!_baseStartLifetimes.TryGetValue(id, out float baseLifetime))
                    baseLifetime = 2f;

                var mainModule = system.main;
                mainModule.startLifetime = new ParticleSystem.MinMaxCurve(baseLifetime * clamped);
            }
        }

        private void ApplyRadius(float radius)
        {
            float clamped = Mathf.Clamp(
                radius,
                PluginConfig.MinOrbitalsRadiusMultiplier,
                PluginConfig.MaxOrbitalsRadiusMultiplier);

            if (_systems == null)
                return;

            foreach (ParticleSystem system in _systems)
            {
                if (system == null)
                    continue;

                int id = system.GetInstanceID();

                if (!_baseShapeRadius.TryGetValue(id, out float baseRadius))
                    baseRadius = 0.5f;

                var shape = system.shape;
                shape.enabled = true;
                shape.radius = baseRadius * clamped;
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
        
        private Vector3 ResolveDesiredLocalPosition(float position)
        {
            float clamped = Mathf.Clamp(
                position,
                PluginConfig.MinFlamePosition,
                PluginConfig.MaxFlamePosition);

            return new Vector3(0f, 0f, clamped);
        }
        
        private void ApplySimulationSpace(ParticleSystemSimulationSpace simulationSpace)
        {
            if (_systems == null)
                return;

            foreach (ParticleSystem system in _systems)
            {
                if (system == null)
                    continue;

                var mainModule = system.main;

                if (mainModule.simulationSpace == simulationSpace)
                    continue;

                mainModule.simulationSpace = simulationSpace;

                system.Clear(true);
                system.Play(true);
            }
        }
    }
}