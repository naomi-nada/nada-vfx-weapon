using System.Collections.Generic;
using UnityEngine;
using NADA.VFX.Core.Config;
using NADA.VFX.Weapons.Targets;

namespace NADA.VFX.Runtime.Binding
{
    internal static class NadaRigMaintenance
    {
        private sealed class ToggleStateCache
        {
            public bool HasState;
            public bool Flare;
            public bool Inner;
            public bool Outer;
        }

        private static readonly Dictionary<int, ToggleStateCache> ToggleStatesByRigInstanceId = new();

        internal static void DisableBrokenFlameRenderer(Transform sword15LavaTf, string ownerNameForLogs)
        {
            if (sword15LavaTf == null) return;

            var psChild = sword15LavaTf.Find("Particle System");
            if (psChild == null) return;

            var psr = psChild.GetComponent<ParticleSystemRenderer>();
            if (psr != null && psr.enabled)
            {
                psr.enabled = false;
                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Disabled broken flame renderer at '{NadaWeaponTargets.FullPath(psChild)}' (owner='{ownerNameForLogs}').");
            }

            var ps = psChild.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                try { ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); } catch { }
            }
        }

        internal static void ApplyPickupFix(GameObject go)
        {
            if (!NadaRigCache.CacheReady) return;
            if (NadaRigCache.PickupMat == null) return;
            if (!NadaWeaponTargets.IsTargetRoot(go)) return;

            var psr = go.GetComponent<ParticleSystemRenderer>();
            if (psr == null) return;
            if (psr.sharedMaterial == NadaRigCache.PickupMat) return;

            psr.sharedMaterial = NadaRigCache.PickupMat;
            Plugin.Log.LogInfo($"{Plugin.ModName}: Fixed ItemDrop pickup particle material on '{go.name}' (root PSR).");
        }

        internal static void ApplyEffectEnabledStates(Transform rigTf)
        {
            if (rigTf == null) return;

            var groups = NadaRigFinder.BuildGroups(rigTf);

            bool flareOn = PluginConfig.Flare.Value;
            bool innerOn = PluginConfig.InnerFlames.Value;
            bool outerOn = PluginConfig.OuterFlames.Value;

            ApplyGroupEnabled(groups.FlareSystems, groups.FlareRenderers, groups.FlareLights, flareOn);
            ApplyGroupEnabled(groups.InnerSystems, groups.InnerRenderers, groups.InnerLights, innerOn);
            ApplyGroupEnabled(groups.OuterSystems, groups.OuterRenderers, groups.OuterLights, outerOn);

            LogToggleStateIfChanged(rigTf, flareOn, innerOn, outerOn);
        }

        private static void ApplyGroupEnabled(
            List<ParticleSystem> systems,
            List<ParticleSystemRenderer> renderers,
            List<Light> lights,
            bool enabled)
        {
            if (systems != null)
            {
                foreach (var ps in systems)
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

            if (renderers != null)
            {
                foreach (var r in renderers)
                {
                    if (r == null) continue;
                    try { r.enabled = enabled; } catch { }
                }
            }

            if (lights != null)
            {
                foreach (var l in lights)
                {
                    if (l == null) continue;
                    try { l.enabled = enabled; } catch { }
                }
            }
        }

        private static void LogToggleStateIfChanged(Transform rigTf, bool flareOn, bool innerOn, bool outerOn)
        {
            if (rigTf == null) return;

            int rigId = rigTf.GetInstanceID();

            if (!ToggleStatesByRigInstanceId.TryGetValue(rigId, out var cache))
            {
                cache = new ToggleStateCache();
                ToggleStatesByRigInstanceId[rigId] = cache;
            }

            bool changed =
                !cache.HasState ||
                cache.Flare != flareOn ||
                cache.Inner != innerOn ||
                cache.Outer != outerOn;

            if (!changed) return;

            cache.HasState = true;
            cache.Flare = flareOn;
            cache.Inner = innerOn;
            cache.Outer = outerOn;

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: Effect toggles applied on '{NadaWeaponTargets.FullPath(rigTf)}' " +
                $"(id={rigTf.GetInstanceID()}, flare={flareOn}, inner={innerOn}, outer={outerOn}).");
        }
    }
}