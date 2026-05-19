using NADA.VFX.Core.Config;
using UnityEngine;

namespace NADA.VFX.Core.Visuals
{
    internal static class NadaLuminanceUtility
    {
        private const float MinVisibleParticleLuminance = 0.55f;

        internal static float RemapParticleLuminance(float luminanceMultiplier)
        {
            float clamped = Mathf.Clamp(
                luminanceMultiplier,
                PluginConfig.MinLuminance,
                PluginConfig.MaxLuminance);

            float t = Mathf.InverseLerp(
                PluginConfig.MinLuminance,
                PluginConfig.MaxLuminance,
                clamped);

            return Mathf.Lerp(
                MinVisibleParticleLuminance,
                PluginConfig.MaxLuminance,
                t);
        }

        internal static ParticleSystem.MinMaxGradient ApplyToGradient(
            ParticleSystem.MinMaxGradient sourceGradient,
            float luminanceMultiplier)
        {
            switch (sourceGradient.mode)
            {
                case ParticleSystemGradientMode.Color:
                    return new ParticleSystem.MinMaxGradient(
                        ApplyToColor(sourceGradient.color, luminanceMultiplier));

                case ParticleSystemGradientMode.TwoColors:
                    return new ParticleSystem.MinMaxGradient(
                        ApplyToColor(sourceGradient.colorMin, luminanceMultiplier),
                        ApplyToColor(sourceGradient.colorMax, luminanceMultiplier));

                case ParticleSystemGradientMode.Gradient:
                    return new ParticleSystem.MinMaxGradient(
                        ApplyToUnityGradient(sourceGradient.gradient, luminanceMultiplier));

                case ParticleSystemGradientMode.TwoGradients:
                    return new ParticleSystem.MinMaxGradient(
                        ApplyToUnityGradient(sourceGradient.gradientMin, luminanceMultiplier),
                        ApplyToUnityGradient(sourceGradient.gradientMax, luminanceMultiplier));

                case ParticleSystemGradientMode.RandomColor:
                    return new ParticleSystem.MinMaxGradient(
                        ApplyToUnityGradient(sourceGradient.gradient, luminanceMultiplier));

                default:
                    return sourceGradient;
            }
        }

        internal static Gradient ApplyToUnityGradient(
            Gradient sourceGradient,
            float luminanceMultiplier)
        {
            if (sourceGradient == null)
                return null;

            GradientColorKey[] sourceColorKeys = sourceGradient.colorKeys;
            GradientAlphaKey[] sourceAlphaKeys = sourceGradient.alphaKeys;

            var colorKeys = new GradientColorKey[sourceColorKeys.Length];
            for (int i = 0; i < sourceColorKeys.Length; i++)
            {
                colorKeys[i] = new GradientColorKey(
                    ApplyToColor(sourceColorKeys[i].color, luminanceMultiplier),
                    sourceColorKeys[i].time);
            }

            var alphaKeys = new GradientAlphaKey[sourceAlphaKeys.Length];
            for (int i = 0; i < sourceAlphaKeys.Length; i++)
            {
                alphaKeys[i] = new GradientAlphaKey(
                    sourceAlphaKeys[i].alpha,
                    sourceAlphaKeys[i].time);
            }

            var result = new Gradient();
            result.SetKeys(colorKeys, alphaKeys);
            return result;
        }

        internal static Color ApplyToColor(Color color, float luminanceMultiplier)
        {
            Color.RGBToHSV(color, out float hue, out float saturation, out float value);

            float adjustedValue = Mathf.Clamp(
                value * luminanceMultiplier,
                0.35f,
                1.75f);

            Color result = Color.HSVToRGB(hue, saturation, adjustedValue, true);
            result.a = color.a;

            return result;
        }
    }
}