// File: Rig/NadaRigFinder.cs
using System.Collections.Generic;
using UnityEngine;

namespace NADA.VFX.Runtime.Binding
{
    internal sealed class RigGroups
    {
        public Transform InnerRoot;
        public Transform OuterRoot;
        public Transform FlareRoot;

        public readonly List<ParticleSystem> InnerSystems = new();
        public readonly List<ParticleSystemRenderer> InnerRenderers = new();
        public readonly List<Light> InnerLights = new();

        public readonly List<ParticleSystem> OuterSystems = new();
        public readonly List<ParticleSystemRenderer> OuterRenderers = new();
        public readonly List<Light> OuterLights = new();

        public readonly List<ParticleSystem> FlareSystems = new();
        public readonly List<ParticleSystemRenderer> FlareRenderers = new();
        public readonly List<Light> FlareLights = new();
    }

    internal static class NadaRigFinder
    {
        internal static Transform FindOuterFlames(Transform rootTf)
        {
            if (rootTf == null) return null;

            foreach (Transform t in rootTf.GetComponentsInChildren<Transform>(true))
            {
                if (t != null && t.name == Plugin.OuterFlamesName)
                    return t;
            }

            return null;
        }

        internal static Transform FindInnerFlames(Transform rootTf)
        {
            if (rootTf == null) return null;

            foreach (Transform t in rootTf.GetComponentsInChildren<Transform>(true))
            {
                if (t != null && t.name == Plugin.InnerFlamesName)
                    return t;
            }

            return null;
        }

        internal static Transform FindFlare(Transform rootTf)
        {
            if (rootTf == null) return null;

            foreach (Transform t in rootTf.GetComponentsInChildren<Transform>(true))
            {
                if (t != null && t.name == Plugin.FlareName)
                    return t;
            }

            return null;
        }

        private static bool IsDescendantOf(Transform t, Transform ancestor)
        {
            if (t == null || ancestor == null) return false;

            var p = t;
            while (p != null)
            {
                if (p == ancestor) return true;
                p = p.parent;
            }

            return false;
        }

        internal static RigGroups BuildGroups(Transform rigTf)
        {
            var g = new RigGroups();
            if (rigTf == null) return g;

            g.OuterRoot = FindOuterFlames(rigTf);
            g.InnerRoot = FindInnerFlames(rigTf);
            g.FlareRoot = FindFlare(rigTf);

            var systems = rigTf.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
            foreach (var ps in systems)
            {
                if (ps == null) continue;

                var t = ps.transform;
                if (t == null) continue;

                if (g.InnerRoot != null && IsDescendantOf(t, g.InnerRoot))
                {
                    g.InnerSystems.Add(ps);
                }
                else if (g.FlareRoot != null && IsDescendantOf(t, g.FlareRoot))
                {
                    g.FlareSystems.Add(ps);
                }
                else if (g.OuterRoot != null && IsDescendantOf(t, g.OuterRoot))
                {
                    g.OuterSystems.Add(ps);
                }
            }

            var renderers = rigTf.GetComponentsInChildren<ParticleSystemRenderer>(includeInactive: true);
            foreach (var r in renderers)
            {
                if (r == null) continue;

                var t = r.transform;
                if (t == null) continue;

                if (g.InnerRoot != null && IsDescendantOf(t, g.InnerRoot))
                {
                    g.InnerRenderers.Add(r);
                }
                else if (g.FlareRoot != null && IsDescendantOf(t, g.FlareRoot))
                {
                    g.FlareRenderers.Add(r);
                }
                else if (g.OuterRoot != null && IsDescendantOf(t, g.OuterRoot))
                {
                    g.OuterRenderers.Add(r);
                }
            }

            var lights = rigTf.GetComponentsInChildren<Light>(includeInactive: true);
            foreach (var l in lights)
            {
                if (l == null) continue;

                var t = l.transform;
                if (t == null) continue;

                if (g.InnerRoot != null && IsDescendantOf(t, g.InnerRoot))
                {
                    g.InnerLights.Add(l);
                }
                else if (g.FlareRoot != null && IsDescendantOf(t, g.FlareRoot))
                {
                    g.FlareLights.Add(l);
                }
                else if (g.OuterRoot != null && IsDescendantOf(t, g.OuterRoot))
                {
                    g.OuterLights.Add(l);
                }
            }

            return g;
        }
    }
}