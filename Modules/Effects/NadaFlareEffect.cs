using UnityEngine;
using NADA.VFX.Core.State;

namespace NADA.VFX.Modules.Effects
{
    internal sealed class NadaFlareEffect : MonoBehaviour
    {
        private global::ItemDrop.ItemData _itemData;

        private Renderer[] _renderers;
        private Light[] _lights;
        private ParticleSystem[] _systems;

        private bool _lastEnabled;
        private bool _hasLastEnabled;

        internal void SetItemData(global::ItemDrop.ItemData itemData)
        {
            _itemData = itemData;
        }

        private void Awake()
        {
            RebuildCaches();
            InvokeRepeating(nameof(TickApply), 0f, 0.05f);
        }

        private void OnDestroy()
        {
            try { CancelInvoke(nameof(TickApply)); } catch { }
        }

        private void TickApply()
        {
            RebuildCaches();

            VfxState state = ResolveState();
            bool enabled = state.FlareEnabled;

            ApplyEnabled(enabled);

            if (!_hasLastEnabled || _lastEnabled != enabled)
            {
                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Flare toggle applied on '{name}' (enabled={enabled}).");
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
    }
}