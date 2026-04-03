using System.Collections.Generic;
using UnityEngine;

namespace NADA.VFX.Runtime.Binding
{
    internal sealed class NadaOrbsTargets
    {
        public Transform Root { get; }
        public Renderer[] Renderers { get; }
        public Light[] Lights { get; }
        public ParticleSystem[] Systems { get; }

        public bool IsValid =>
            Root != null &&
            Renderers != null &&
            Lights != null &&
            Systems != null;

        public NadaOrbsTargets(
            Transform root,
            Renderer[] renderers,
            Light[] lights,
            ParticleSystem[] systems)
        {
            Root = root;
            Renderers = renderers ?? System.Array.Empty<Renderer>();
            Lights = lights ?? System.Array.Empty<Light>();
            Systems = systems ?? System.Array.Empty<ParticleSystem>();
        }

        internal static NadaOrbsTargets Build(Transform root)
        {
            if (root == null) return null;

            Renderer[] renderers = GetChildRenderersOnly(root);
            Light[] lights = root.GetComponentsInChildren<Light>(true);
            ParticleSystem[] systems = root.GetComponentsInChildren<ParticleSystem>(true);

            return new NadaOrbsTargets(root, renderers, lights, systems);
        }

        private static Renderer[] GetChildRenderersOnly(Transform root)
        {
            if (root == null) return System.Array.Empty<Renderer>();

            var all = root.GetComponentsInChildren<Renderer>(true);
            var list = new List<Renderer>(all.Length);

            foreach (var r in all)
            {
                if (r == null) continue;
                if (r.transform == root) continue;
                list.Add(r);
            }

            return list.ToArray();
        }
    }
}