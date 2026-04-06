using System.Collections.Generic;
using NADA.VFX.Core.State;
using UnityEngine;

namespace NADA.VFX.Modules.Motion
{
    internal sealed class NadaOrbitalsChainMotion : MonoBehaviour
    {
        private const int ExtraHistoryPadding = 24;
        private const int MaxOrbitalsFollowers = 20;

        private global::ItemDrop.ItemData _itemData;
        private OrbitalsVisualKind _visualKind = OrbitalsVisualKind.Flames;

        private readonly List<Transform> _followerVisualTransforms = new();
        private readonly List<Vector3> _leadWorldPositionHistorySamples = new();
        private readonly Dictionary<int, bool> _wasFollowerActiveByInstanceId = new();

        private Transform _followerPoolRootTransform;
        private Transform _leadTransform;
        private bool _initialized;
        private bool _loggedStateOnce;

        internal bool IsInitialized => _initialized;

        internal void Initialize(
            OrbitalsVisualKind visualKind,
            Transform followerPoolRootTransform,
            Transform leadTransform,
            global::ItemDrop.ItemData itemData)
        {
            if (!_initialized)
            {
                _followerPoolRootTransform = followerPoolRootTransform;
                _leadTransform = leadTransform;
                _itemData = itemData;
                _visualKind = visualKind;

                _leadWorldPositionHistorySamples.Clear();
                _wasFollowerActiveByInstanceId.Clear();
                _loggedStateOnce = false;

                RebuildFollowerVisualTransforms();

                foreach (Transform followerVisualTransform in _followerVisualTransforms)
                {
                    if (followerVisualTransform == null)
                        continue;

                    try
                    {
                        StopAndClearAllParticles(followerVisualTransform);
                        followerVisualTransform.gameObject.SetActive(false);
                    }
                    catch { }
                }

                _initialized =
                    _followerPoolRootTransform != null &&
                    _leadTransform != null &&
                    _followerVisualTransforms.Count > 0;

                return;
            }

            _visualKind = visualKind;
            _followerPoolRootTransform = followerPoolRootTransform;
            _leadTransform = leadTransform;
            _itemData = itemData;
        }

        internal void UpdateKindLeadAndItemData(
            OrbitalsVisualKind visualKind,
            Transform leadTransform,
            global::ItemDrop.ItemData itemData)
        {
            _visualKind = visualKind;
            _leadTransform = leadTransform;
            _itemData = itemData;
        }

        private void LateUpdate()
        {
            if (!_initialized || _followerPoolRootTransform == null || _leadTransform == null)
                return;

            RebuildFollowerVisualTransforms();

            VfxState state = ResolveState();

            bool isEnabled = ResolveVisualFamilyEnabled(state);
            if (!isEnabled)
            {
                DisableAllFollowers();
                _leadWorldPositionHistorySamples.Clear();
                return;
            }

            int resolvedTotalVisualCount = ResolveVisualFamilyTotalCount(state);
            int resolvedFollowerCount = Mathf.Clamp(
                resolvedTotalVisualCount - 1,
                0,
                Mathf.Min(MaxOrbitalsFollowers, _followerVisualTransforms.Count));

            int resolvedHistorySpacing = ResolveVisualFamilyHistorySpacing(state);

            if (!_loggedStateOnce)
            {
                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Orbitals chain state on '{name}' " +
                    $"kind={_visualKind} enabled={isEnabled} resolvedTotal={resolvedTotalVisualCount} " +
                    $"resolvedFollowers={resolvedFollowerCount} resolvedSpacing={resolvedHistorySpacing} visuals={_followerVisualTransforms.Count}");

                _loggedStateOnce = true;
            }

            RecordLeadWorldHistory(
                _leadTransform.position,
                resolvedFollowerCount,
                resolvedHistorySpacing);

            ApplyFollowerPositions(
                resolvedFollowerCount,
                resolvedHistorySpacing);
        }

        private void DisableAllFollowers()
        {
            for (int followerIndex = 0; followerIndex < _followerVisualTransforms.Count; followerIndex++)
            {
                Transform followerVisualTransform = _followerVisualTransforms[followerIndex];
                if (followerVisualTransform == null)
                    continue;

                int instanceId = followerVisualTransform.GetInstanceID();

                if (followerVisualTransform.gameObject.activeSelf ||
                    (_wasFollowerActiveByInstanceId.TryGetValue(instanceId, out bool wasActive) && wasActive))
                {
                    StopAndClearAllParticles(followerVisualTransform);
                    followerVisualTransform.gameObject.SetActive(false);
                    _wasFollowerActiveByInstanceId[instanceId] = false;
                }
            }
        }

        private void RebuildFollowerVisualTransforms()
        {
            _followerVisualTransforms.Clear();

            if (_followerPoolRootTransform == null)
                return;

            foreach (Transform childTransform in _followerPoolRootTransform)
            {
                if (childTransform != null)
                    _followerVisualTransforms.Add(childTransform);
            }
        }

        private bool ResolveVisualFamilyEnabled(VfxState state)
        {
            return _visualKind == OrbitalsVisualKind.Embers
                ? state.OrbitalsEmbersEnabled
                : state.OrbitalsFlamesEnabled;
        }

        private int ResolveVisualFamilyTotalCount(VfxState state)
        {
            float normalizedCount = _visualKind == OrbitalsVisualKind.Embers
                ? state.OrbitalsEmbersCount
                : state.OrbitalsFlamesCount;

            normalizedCount = Mathf.Clamp01(normalizedCount);
            return 1 + Mathf.RoundToInt(normalizedCount * (MaxOrbitalsFollowers - 1));
        }

        private int ResolveVisualFamilyHistorySpacing(VfxState state)
        {
            float spacingT = _visualKind == OrbitalsVisualKind.Embers
                ? state.OrbitalsEmbersSpacing
                : state.OrbitalsFlamesSpacing;

            spacingT = Mathf.Clamp01(spacingT);

            const int minHistoryStep = 2;
            const int maxHistoryStep = 35;

            return Mathf.RoundToInt(Mathf.Lerp(minHistoryStep, maxHistoryStep, spacingT));
        }

        private void RecordLeadWorldHistory(
            Vector3 leadWorldPosition,
            int followerCount,
            int historySpacing)
        {
            _leadWorldPositionHistorySamples.Insert(0, leadWorldPosition);

            int maxHistorySamples =
                Mathf.Max(1, (followerCount + 1) * historySpacing + ExtraHistoryPadding);

            if (_leadWorldPositionHistorySamples.Count > maxHistorySamples)
            {
                _leadWorldPositionHistorySamples.RemoveRange(
                    maxHistorySamples,
                    _leadWorldPositionHistorySamples.Count - maxHistorySamples);
            }
        }

        private void ApplyFollowerPositions(
            int followerCount,
            int historySpacing)
        {
            for (int followerIndex = 0; followerIndex < _followerVisualTransforms.Count; followerIndex++)
            {
                Transform followerVisualTransform = _followerVisualTransforms[followerIndex];
                if (followerVisualTransform == null)
                    continue;

                bool shouldBeActive = followerIndex < followerCount;
                int instanceId = followerVisualTransform.GetInstanceID();

                _wasFollowerActiveByInstanceId.TryGetValue(instanceId, out bool wasActive);

                if (!shouldBeActive)
                {
                    if (followerVisualTransform.gameObject.activeSelf || wasActive)
                    {
                        StopAndClearAllParticles(followerVisualTransform);
                        followerVisualTransform.gameObject.SetActive(false);
                        _wasFollowerActiveByInstanceId[instanceId] = false;
                    }

                    continue;
                }

                int historySampleIndex = (followerIndex + 1) * historySpacing;
                if (historySampleIndex >= _leadWorldPositionHistorySamples.Count)
                {
                    if (followerVisualTransform.gameObject.activeSelf || wasActive)
                    {
                        StopAndClearAllParticles(followerVisualTransform);
                        followerVisualTransform.gameObject.SetActive(false);
                        _wasFollowerActiveByInstanceId[instanceId] = false;
                    }

                    continue;
                }

                Vector3 sampledFollowerWorldPosition =
                    _leadWorldPositionHistorySamples[historySampleIndex];

                if (!followerVisualTransform.gameObject.activeSelf)
                    followerVisualTransform.gameObject.SetActive(true);

                followerVisualTransform.position = sampledFollowerWorldPosition;
                followerVisualTransform.rotation = Quaternion.identity;

                if (!wasActive)
                {
                    HardResetAndPlayAllParticles(followerVisualTransform);
                    _wasFollowerActiveByInstanceId[instanceId] = true;
                }
            }
        }

        private static void HardResetAndPlayAllParticles(Transform rootTransform)
        {
            if (rootTransform == null)
                return;

            foreach (ParticleSystem particleSystem in rootTransform.GetComponentsInChildren<ParticleSystem>(true))
            {
                if (particleSystem == null)
                    continue;

                try
                {
                    particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    particleSystem.Clear(true);
                    particleSystem.Play(true);
                }
                catch { }
            }

            foreach (Renderer renderer in rootTransform.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null)
                    continue;

                try { renderer.enabled = true; } catch { }
            }

            foreach (Light light in rootTransform.GetComponentsInChildren<Light>(true))
            {
                if (light == null)
                    continue;

                try { light.enabled = true; } catch { }
            }
        }

        private static void StopAndClearAllParticles(Transform rootTransform)
        {
            if (rootTransform == null)
                return;

            foreach (ParticleSystem particleSystem in rootTransform.GetComponentsInChildren<ParticleSystem>(true))
            {
                if (particleSystem == null)
                    continue;

                try
                {
                    particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    particleSystem.Clear(true);
                }
                catch { }
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
    }
}