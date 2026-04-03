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
        private OrbitalsVisualKind _kind = OrbitalsVisualKind.Flames;

        private readonly List<Transform> _visuals = new();
        private readonly List<Vector3> _history = new();
        private readonly Dictionary<int, bool> _lastActiveByInstanceId = new();

        private Transform _poolRoot;
        private Transform _leadTransform;
        private bool _initialized;
        private bool _loggedStateOnce;

        internal bool IsInitialized => _initialized;

        internal void Initialize(
            OrbitalsVisualKind kind,
            Transform poolRoot,
            Transform leadTransform,
            global::ItemDrop.ItemData itemData)
        {
            if (!_initialized)
            {
                _poolRoot = poolRoot;
                _leadTransform = leadTransform;
                _itemData = itemData;
                _kind = kind;

                _history.Clear();
                _lastActiveByInstanceId.Clear();
                _loggedStateOnce = false;

                CacheVisuals();

                foreach (Transform visual in _visuals)
                {
                    if (visual == null) continue;

                    try
                    {
                        StopAndClearAllParticles(visual);
                        visual.gameObject.SetActive(false);
                    }
                    catch { }
                }

                _initialized = _poolRoot != null && _leadTransform != null && _visuals.Count > 0;
                return;
            }

            _kind = kind;
            _poolRoot = poolRoot;
            _leadTransform = leadTransform;
            _itemData = itemData;
        }

        internal void SetKindLeadAndItemData(
            OrbitalsVisualKind kind,
            Transform leadTransform,
            global::ItemDrop.ItemData itemData)
        {
            _kind = kind;
            _leadTransform = leadTransform;
            _itemData = itemData;
        }

        private void LateUpdate()
        {
            if (!_initialized || _poolRoot == null || _leadTransform == null)
                return;

            CacheVisuals();

            VfxState state = ResolveState();

            bool enabled = ResolveEnabled(state);
            if (!enabled)
            {
                DisableAllFollowers();
                _history.Clear();
                return;
            }

            int totalCount = ResolveTotalCount(state);
            int followerCount = Mathf.Clamp(totalCount - 1, 0, Mathf.Min(MaxOrbitalsFollowers, _visuals.Count));
            int spacing = ResolveSpacing(state);

            if (!_loggedStateOnce)
            {
                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Orbitals chain state on '{name}' " +
                    $"kind={_kind} enabled={enabled} resolvedTotal={totalCount} " +
                    $"resolvedFollowers={followerCount} resolvedSpacing={spacing} visuals={_visuals.Count}");

                _loggedStateOnce = true;
            }

            RecordHistory(_leadTransform.position, followerCount, spacing);
            ApplyFollowers(followerCount, spacing);
        }

        private void DisableAllFollowers()
        {
            for (int i = 0; i < _visuals.Count; i++)
            {
                Transform visual = _visuals[i];
                if (visual == null) continue;

                int instanceId = visual.GetInstanceID();

                if (visual.gameObject.activeSelf ||
                    (_lastActiveByInstanceId.TryGetValue(instanceId, out bool wasActive) && wasActive))
                {
                    StopAndClearAllParticles(visual);
                    visual.gameObject.SetActive(false);
                    _lastActiveByInstanceId[instanceId] = false;
                }
            }
        }

        private void CacheVisuals()
        {
            _visuals.Clear();

            if (_poolRoot == null)
                return;

            foreach (Transform child in _poolRoot)
            {
                if (child != null)
                    _visuals.Add(child);
            }
        }

        private bool ResolveEnabled(VfxState state)
        {
            return _kind == OrbitalsVisualKind.Embers
                ? state.OrbitalsEmbersEnabled
                : state.OrbitalsFlamesEnabled;
        }

        private int ResolveTotalCount(VfxState state)
        {
            float normalized = _kind == OrbitalsVisualKind.Embers
                ? state.OrbitalsEmbersCount
                : state.OrbitalsFlamesCount;

            normalized = Mathf.Clamp01(normalized);
            return 1 + Mathf.RoundToInt(normalized * 19f);
        }

        private int ResolveSpacing(VfxState state)
        {
            float spacingT = _kind == OrbitalsVisualKind.Embers
                ? state.OrbitalsEmbersSpacing
                : state.OrbitalsFlamesSpacing;

            spacingT = Mathf.Clamp01(spacingT);

            const int min = 2;
            const int max = 35;

            return Mathf.RoundToInt(Mathf.Lerp(min, max, spacingT));
        }

        private void RecordHistory(Vector3 leadWorldPosition, int followerCount, int spacing)
        {
            _history.Insert(0, leadWorldPosition);

            int maxHistory = Mathf.Max(1, (followerCount + 1) * spacing + ExtraHistoryPadding);
            if (_history.Count > maxHistory)
                _history.RemoveRange(maxHistory, _history.Count - maxHistory);
        }

        private void ApplyFollowers(int followerCount, int spacing)
        {
            for (int i = 0; i < _visuals.Count; i++)
            {
                Transform visual = _visuals[i];
                if (visual == null) continue;

                bool shouldBeActive = i < followerCount;
                int instanceId = visual.GetInstanceID();

                _lastActiveByInstanceId.TryGetValue(instanceId, out bool wasActive);

                if (!shouldBeActive)
                {
                    if (visual.gameObject.activeSelf || wasActive)
                    {
                        StopAndClearAllParticles(visual);
                        visual.gameObject.SetActive(false);
                        _lastActiveByInstanceId[instanceId] = false;
                    }

                    continue;
                }

                int historyIndex = (i + 1) * spacing;
                if (historyIndex >= _history.Count)
                {
                    if (visual.gameObject.activeSelf || wasActive)
                    {
                        StopAndClearAllParticles(visual);
                        visual.gameObject.SetActive(false);
                        _lastActiveByInstanceId[instanceId] = false;
                    }

                    continue;
                }

                Vector3 targetWorldPosition = _history[historyIndex];

                if (!visual.gameObject.activeSelf)
                    visual.gameObject.SetActive(true);

                visual.position = targetWorldPosition;
                visual.rotation = Quaternion.identity;

                if (!wasActive)
                {
                    HardResetAndPlayAllParticles(visual);
                    _lastActiveByInstanceId[instanceId] = true;
                }
            }
        }

        private static void HardResetAndPlayAllParticles(Transform root)
        {
            if (root == null) return;

            foreach (ParticleSystem ps in root.GetComponentsInChildren<ParticleSystem>(true))
            {
                if (ps == null) continue;

                try
                {
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    ps.Clear(true);
                    ps.Play(true);
                }
                catch { }
            }

            foreach (Renderer r in root.GetComponentsInChildren<Renderer>(true))
            {
                if (r == null) continue;
                try { r.enabled = true; } catch { }
            }

            foreach (Light l in root.GetComponentsInChildren<Light>(true))
            {
                if (l == null) continue;
                try { l.enabled = true; } catch { }
            }
        }

        private static void StopAndClearAllParticles(Transform root)
        {
            if (root == null) return;

            foreach (ParticleSystem ps in root.GetComponentsInChildren<ParticleSystem>(true))
            {
                if (ps == null) continue;

                try
                {
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    ps.Clear(true);
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