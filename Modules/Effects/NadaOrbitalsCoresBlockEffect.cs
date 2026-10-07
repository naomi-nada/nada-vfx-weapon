using System.Collections.Generic;
using NADA.VFX.Weapon.Core.Config;
using NADA.VFX.Weapon.Core.Debug;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Blocks.Effects;
using NADA.VFX.Weapon.Core.Visuals;
using NADA.VFX.Weapon.Runtime.Structure;
using UnityEngine;

namespace NADA.VFX.Weapon.Modules.Effects
{
    /// <summary>
    /// Visual behavior for one block-owned orbital Cores instance.
    ///
    /// State comes only from its VfxEffectBlock. There is no ItemData,
    /// config, or legacy VfxState fallback here.
    /// </summary>
    internal sealed class NadaOrbitalsCoresBlockEffect :
        MonoBehaviour
    {
        private const float DefaultCoreBaselineScaleMultiplier =
            0.1f;

        private const float CoreLuminanceBoost =
            2.5f;

        private Transform _coresRootTransform;
        private Transform _poolRootTransform;

        private readonly List<ParticleSystem>
            _particleSystems = new();

        private readonly List<Renderer>
            _renderers = new();

        private readonly List<Light>
            _lights = new();

        private readonly Dictionary<int, Vector3>
            _baseLocalScaleByTransformId = new();

        private readonly Dictionary<int, ParticleSystem.MinMaxGradient>
            _baseMainStartColorByParticleSystemId = new();

        private readonly Dictionary<int, bool>
            _baseColorOverLifetimeEnabledByParticleSystemId = new();

        private readonly Dictionary<int, ParticleSystem.MinMaxGradient>
            _baseColorOverLifetimeByParticleSystemId = new();

        private readonly Dictionary<int, MaterialBaseline>
            _baseMaterialByRendererId = new();

        private readonly Dictionary<int, Color>
            _baseLightColorByLightId = new();

        private readonly HashSet<Material>
            _ownedRendererMaterials = new();

        private bool _hasCachedBaselines;

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
        }

        private void OnDestroy()
        {
            NadaRuntimeDiagnostics
                .OrbitalsEffectDestroyed();

            DestroyOwnedMaterials();
        }

        internal bool SetBlockState(
            Transform coresRootTransform,
            Transform poolRootTransform,
            VfxEffectBlock block)
        {
            OrbitalsCoresVfxSettings settings =
                block?.Settings as OrbitalsCoresVfxSettings;

            bool accepted =
                block != null &&
                block.Transform != null &&
                block.TypeId ==
                    VfxEffectTypeIds.OrbitalsCores &&
                settings?.Formation?.Path != null &&
                coresRootTransform != null &&
                poolRootTransform != null;

            if (!accepted)
            {
                DisableVisuals();

                return false;
            }

            bool rootsChanged =
                _coresRootTransform !=
                    coresRootTransform ||
                _poolRootTransform !=
                    poolRootTransform;

            if (rootsChanged)
            {
                DestroyOwnedMaterials();
                ClearCaches();

                _coresRootTransform =
                    coresRootTransform;

                _poolRootTransform =
                    poolRootTransform;

                RebuildCaches();
                CacheBaselines();

                _hasCachedBaselines =
                    true;
            }
            else if (!_hasCachedBaselines)
            {
                RebuildCaches();
                CacheBaselines();

                _hasCachedBaselines =
                    true;
            }

            ApplyEnabledState(
                block.Enabled);

            if (!block.Enabled)
                return true;

            ApplyScale(
                settings.Scale);

            ApplyHueShift(
                settings.Hue,
                settings.Luminance);

            EnsureParticleSystemsPlaying();

            return true;
        }

        private void ApplyEnabledState(
            bool enabled)
        {
            if (!enabled)
            {
                ApplyGroupEnabledState(
                    false);

                SetRootActive(
                    _coresRootTransform,
                    false);

                SetRootActive(
                    _poolRootTransform,
                    false);

                return;
            }

            SetRootActive(
                _coresRootTransform,
                true);

            SetRootActive(
                _poolRootTransform,
                true);

            ApplyGroupEnabledState(
                true);
        }

        private void DisableVisuals()
        {
            ApplyEnabledState(
                false);
        }

        private void RebuildCaches()
        {
            ClearCaches();

            AppendComponents(
                _coresRootTransform);

            AppendComponents(
                _poolRootTransform);

            AddScaleTransform(
                _coresRootTransform);

            if (_poolRootTransform != null)
            {
                foreach (Transform pooledTransform in
                         _poolRootTransform)
                {
                    AddScaleTransform(
                        pooledTransform);
                }
            }
        }

        private void ClearCaches()
        {
            _particleSystems.Clear();
            _renderers.Clear();
            _lights.Clear();

            _baseLocalScaleByTransformId.Clear();

            _baseMainStartColorByParticleSystemId.Clear();
            _baseColorOverLifetimeEnabledByParticleSystemId.Clear();
            _baseColorOverLifetimeByParticleSystemId.Clear();

            _baseMaterialByRendererId.Clear();
            _baseLightColorByLightId.Clear();

            _hasCachedBaselines =
                false;
        }

        private void AppendComponents(
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

        private void AddScaleTransform(
            Transform targetTransform)
        {
            if (targetTransform == null)
                return;

            int transformId =
                targetTransform.GetInstanceID();

            if (_baseLocalScaleByTransformId.ContainsKey(
                    transformId))
            {
                return;
            }

            _baseLocalScaleByTransformId[
                    transformId] =
                NormalizeUniformScale(
                    targetTransform.localScale);
        }

        private void CacheBaselines()
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

                    _baseMainStartColorByParticleSystemId[
                            particleSystemId] =
                        main.startColor;
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
            }

            foreach (Renderer renderer in
                     _renderers)
            {
                CacheRendererBaseline(
                    renderer);
            }

            foreach (Light light in
                     _lights)
            {
                if (light == null)
                    continue;

                _baseLightColorByLightId[
                        light.GetInstanceID()] =
                    light.color;
            }
        }

        private void CacheRendererBaseline(
            Renderer renderer)
        {
            if (renderer == null)
                return;

            int rendererId =
                renderer.GetInstanceID();

            if (_baseMaterialByRendererId.ContainsKey(
                    rendererId))
            {
                return;
            }

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
                return;
            }

            if (material == null)
                return;

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

        private void ApplyScale(
            float scale)
        {
            float clampedScale =
                ClampVisualScale(
                    scale);

            float scaleMultiplier =
                DefaultCoreBaselineScaleMultiplier *
                clampedScale;

            ApplyTransformScale(
                _coresRootTransform,
                scaleMultiplier);

            if (_poolRootTransform == null)
                return;

            foreach (Transform pooledTransform in
                     _poolRootTransform)
            {
                ApplyTransformScale(
                    pooledTransform,
                    scaleMultiplier);
            }
        }

        private void ApplyTransformScale(
            Transform targetTransform,
            float multiplier)
        {
            if (targetTransform == null)
                return;

            if (!_baseLocalScaleByTransformId.TryGetValue(
                    targetTransform.GetInstanceID(),
                    out Vector3 baseScale))
            {
                return;
            }

            targetTransform.localScale =
                baseScale *
                multiplier;

            NadaRigTransforms.ForceUniformWorldScale(
                targetTransform);
        }

        private void ApplyHueShift(
            float sliderValue,
            float luminance)
        {
            float targetHue =
                NadaHueShiftUtility
                    .SliderValueToTargetHue(
                        sliderValue);

            float luminanceMultiplier =
                RemapCoreLuminance(
                    luminance);

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
                    if (_baseMainStartColorByParticleSystemId
                        .TryGetValue(
                            particleSystemId,
                            out ParticleSystem.MinMaxGradient baseStartColor))
                    {
                        var main =
                            particleSystem.main;

                        main.startColor =
                            NadaLuminanceUtility.ApplyToGradient(
                                NadaHueShiftUtility
                                    .RetintMinMaxGradientToHue(
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
                    if (_baseColorOverLifetimeByParticleSystemId
                        .TryGetValue(
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
                            NadaHueShiftUtility.RetintColorToHue(
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
                            NadaHueShiftUtility.RetintColorToHue(
                                baseline.BaseColor.Value,
                                targetHue));
                    }

                    if (baseline.TintColor.HasValue &&
                        material.HasProperty(
                            "_TintColor"))
                    {
                        material.SetColor(
                            "_TintColor",
                            NadaHueShiftUtility.RetintColorToHue(
                                baseline.TintColor.Value,
                                targetHue));
                    }

                    if (baseline.EmissionColor.HasValue &&
                        material.HasProperty(
                            "_EmissionColor"))
                    {
                        Color emission =
                            NadaHueShiftUtility.RetintColorToHue(
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
            foreach (Light light in _lights)
            {
                if (light == null)
                    continue;

                if (!_baseLightColorByLightId.TryGetValue(
                        light.GetInstanceID(),
                        out Color baseColor))
                {
                    continue;
                }

                try
                {
                    light.color =
                        NadaHueShiftUtility.RetintColorToHue(
                            baseColor,
                            targetHue);
                }
                catch
                {
                }
            }
        }

        private void ApplyGroupEnabledState(
            bool enabled)
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
                    if (enabled)
                    {
                        if (!particleSystem.isPlaying)
                            particleSystem.Play(true);
                    }
                    else if (particleSystem.isPlaying)
                    {
                        particleSystem.Stop(
                            true,
                            ParticleSystemStopBehavior
                                .StopEmittingAndClear);
                    }
                }
                catch
                {
                }
            }

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
            bool active)
        {
            if (rootTransform != null &&
                rootTransform.gameObject.activeSelf != active)
            {
                rootTransform.gameObject.SetActive(
                    active);
            }
        }

        private static float RemapCoreLuminance(
            float luminance)
        {
            if (float.IsNaN(luminance) ||
                float.IsInfinity(luminance))
            {
                return PluginConfig.DefaultLuminance;
            }

            luminance =
                Mathf.Clamp(
                    luminance,
                    PluginConfig.MinLuminance,
                    PluginConfig.MaxLuminance);

            return luminance *
                   CoreLuminanceBoost;
        }

        private static float ClampVisualScale(
            float value)
        {
            if (float.IsNaN(value) ||
                float.IsInfinity(value))
            {
                return 1f;
            }

            return Mathf.Clamp(
                value,
                0.2f,
                1.8f);
        }

        private static Vector3 NormalizeUniformScale(
            Vector3 scale)
        {
            float uniform =
                Mathf.Max(
                    Mathf.Abs(scale.x),
                    Mathf.Abs(scale.y),
                    Mathf.Abs(scale.z));

            if (uniform <= 0.0001f)
                uniform = 1f;

            return Vector3.one *
                   uniform;
        }
    }
}