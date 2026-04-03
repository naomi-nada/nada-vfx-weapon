using System.Collections.Generic;
using UnityEngine;

namespace NADA.VFX.Core.Color
{
    internal static class NadaVfxPainter
    {
        private static readonly HashSet<int> PaintedInstanceIds = new HashSet<int>();

        internal static bool TryApplyPalette(GameObject rigRoot, NadaVfxPalette palette, string ownerLabel = null)
        {
            if (rigRoot == null)
            {
                Plugin.Log.LogWarning($"{Plugin.ModName}: NadaVfxPainter aborted (rigRoot null).");
                return false;
            }

            if (palette == null)
            {
                Plugin.Log.LogWarning($"{Plugin.ModName}: NadaVfxPainter aborted (palette null).");
                return false;
            }

            var flamePs = FindPrimaryFlameSystem(rigRoot);
            if (flamePs == null)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: NadaVfxPainter could not find primary flame under '{GetSafePath(rigRoot.transform)}'.");
                return false;
            }

            var id = flamePs.gameObject.GetInstanceID();
            if (PaintedInstanceIds.Contains(id))
                return true;

            ApplyToParticleSystem(flamePs, palette);

            var pointLight = FindPrimaryPointLight(rigRoot);
            if (pointLight != null && palette.PointLightColor.HasValue)
            {
                pointLight.color = palette.PointLightColor.Value;
            }

            PaintedInstanceIds.Add(id);

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: Applied flame palette '{palette.Name}' to '{flamePs.name}' " +
                $"under '{GetSafePath(rigRoot.transform)}' (owner='{ownerLabel ?? "unknown"}').");

            return true;
        }

        private static void ApplyToParticleSystem(ParticleSystem ps, NadaVfxPalette palette)
        {
            if (ps == null || palette == null)
                return;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(palette.ColorOverLifetime);

            var customData = ps.customData;
            customData.enabled = true;

            customData.SetMode(ParticleSystemCustomData.Custom1, ParticleSystemCustomDataMode.Color);
            customData.SetColor(ParticleSystemCustomData.Custom1, new ParticleSystem.MinMaxGradient(palette.Custom1));

            customData.SetMode(ParticleSystemCustomData.Custom2, ParticleSystemCustomDataMode.Color);
            customData.SetColor(ParticleSystemCustomData.Custom2, new ParticleSystem.MinMaxGradient(palette.Custom2));
        }

        private static ParticleSystem FindPrimaryFlameSystem(GameObject root)
        {
            if (root == null) return null;

            var systems = root.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in systems)
            {
                if (ps == null) continue;

                var n = ps.name;
                if (n == "fx_Torch_Basic" || n == "fx_Torch_Blue" || n == "fx_Torch_Green")
                    return ps;
            }

            return null;
        }

        private static Light FindPrimaryPointLight(GameObject root)
        {
            if (root == null) return null;

            var lights = root.GetComponentsInChildren<Light>(true);
            foreach (var l in lights)
            {
                if (l == null) continue;
                if (l.name == "Point light")
                    return l;
            }

            return null;
        }

        private static string GetSafePath(Transform t)
        {
            if (t == null) return "<null>";

            var parts = new List<string>();
            while (t != null)
            {
                parts.Add(t.name);
                t = t.parent;
            }

            parts.Reverse();
            return string.Join("/", parts);
        }
    }
}