using UnityEngine;

namespace NADA.VFX.Core.Color
{
    internal static class NadaHueShiftUtility
    {
        private const float ClassicFlameHue = 0.08f;

        internal static float SliderValueToTargetHue(float sliderValue)
        {
            return WrapHue01(ClassicFlameHue + sliderValue);
        }

        private static float WrapHue01(float h)
        {
            h %= 1f;
            if (h < 0f) h += 1f;
            return h;
        }

        internal static UnityEngine.Color RetintColorToHue(UnityEngine.Color input, float targetHue)
        {
            UnityEngine.Color.RGBToHSV(input, out _, out float s, out float v);

            if (s < 0.01f)
            {
                var unchanged = input;
                unchanged.a = input.a;
                return unchanged;
            }

            var tinted = UnityEngine.Color.HSVToRGB(targetHue, s, v, true);
            tinted.a = input.a;
            return tinted;
        }

        internal static UnityEngine.Color RetintColorToHueAllowGrayscale(
            UnityEngine.Color input,
            float targetHue,
            float grayscaleSaturation = 0.85f,
            float valueBoost = 1.15f)
        {
            UnityEngine.Color.RGBToHSV(input, out _, out float s, out float v);

            float outS = s < 0.01f ? grayscaleSaturation : s;

            // Boost brightness slightly but clamp to avoid blowout
            float outV = Mathf.Clamp01(v * valueBoost);

            var tinted = UnityEngine.Color.HSVToRGB(targetHue, outS, outV, true);
            tinted.a = input.a;
            return tinted;
        }

        internal static Gradient RetintGradientToHue(Gradient source, float targetHue)
        {
            if (source == null) return null;

            var srcColorKeys = source.colorKeys;
            var srcAlphaKeys = source.alphaKeys;

            var newColorKeys = new GradientColorKey[srcColorKeys.Length];
            for (int i = 0; i < srcColorKeys.Length; i++)
            {
                newColorKeys[i] = new GradientColorKey(
                    RetintColorToHue(srcColorKeys[i].color, targetHue),
                    srcColorKeys[i].time
                );
            }

            var newAlphaKeys = new GradientAlphaKey[srcAlphaKeys.Length];
            for (int i = 0; i < srcAlphaKeys.Length; i++)
            {
                newAlphaKeys[i] = new GradientAlphaKey(
                    srcAlphaKeys[i].alpha,
                    srcAlphaKeys[i].time
                );
            }

            var g = new Gradient();
            g.SetKeys(newColorKeys, newAlphaKeys);
            return g;
        }

        internal static Gradient RetintGradientToHueAllowGrayscale(
            Gradient source,
            float targetHue,
            float grayscaleSaturation = 0.85f)
        {
            if (source == null) return null;

            var srcColorKeys = source.colorKeys;
            var srcAlphaKeys = source.alphaKeys;

            var newColorKeys = new GradientColorKey[srcColorKeys.Length];
            for (int i = 0; i < srcColorKeys.Length; i++)
            {
                newColorKeys[i] = new GradientColorKey(
                    RetintColorToHueAllowGrayscale(srcColorKeys[i].color, targetHue, grayscaleSaturation),
                    srcColorKeys[i].time
                );
            }

            var newAlphaKeys = new GradientAlphaKey[srcAlphaKeys.Length];
            for (int i = 0; i < srcAlphaKeys.Length; i++)
            {
                newAlphaKeys[i] = new GradientAlphaKey(
                    srcAlphaKeys[i].alpha,
                    srcAlphaKeys[i].time
                );
            }

            var g = new Gradient();
            g.SetKeys(newColorKeys, newAlphaKeys);
            return g;
        }

        internal static ParticleSystem.MinMaxGradient RetintMinMaxGradientToHue(
            ParticleSystem.MinMaxGradient source,
            float targetHue)
        {
            switch (source.mode)
            {
                case ParticleSystemGradientMode.Color:
                    return new ParticleSystem.MinMaxGradient(
                        RetintColorToHue(source.color, targetHue)
                    );

                case ParticleSystemGradientMode.TwoColors:
                    return new ParticleSystem.MinMaxGradient(
                        RetintColorToHue(source.colorMin, targetHue),
                        RetintColorToHue(source.colorMax, targetHue)
                    );

                case ParticleSystemGradientMode.Gradient:
                    return new ParticleSystem.MinMaxGradient(
                        RetintGradientToHue(source.gradient, targetHue)
                    );

                case ParticleSystemGradientMode.TwoGradients:
                    return new ParticleSystem.MinMaxGradient(
                        RetintGradientToHue(source.gradientMin, targetHue),
                        RetintGradientToHue(source.gradientMax, targetHue)
                    );

                case ParticleSystemGradientMode.RandomColor:
                    return new ParticleSystem.MinMaxGradient(
                        RetintGradientToHue(source.gradient, targetHue)
                    );

                default:
                    return source;
            }
        }

        internal static ParticleSystem.MinMaxGradient RetintMinMaxGradientToHueAllowGrayscale(
            ParticleSystem.MinMaxGradient source,
            float targetHue,
            float grayscaleSaturation = 0.85f)
        {
            switch (source.mode)
            {
                case ParticleSystemGradientMode.Color:
                    return new ParticleSystem.MinMaxGradient(
                        RetintColorToHueAllowGrayscale(source.color, targetHue, grayscaleSaturation)
                    );

                case ParticleSystemGradientMode.TwoColors:
                    return new ParticleSystem.MinMaxGradient(
                        RetintColorToHueAllowGrayscale(source.colorMin, targetHue, grayscaleSaturation),
                        RetintColorToHueAllowGrayscale(source.colorMax, targetHue, grayscaleSaturation)
                    );

                case ParticleSystemGradientMode.Gradient:
                    return new ParticleSystem.MinMaxGradient(
                        RetintGradientToHueAllowGrayscale(source.gradient, targetHue, grayscaleSaturation)
                    );

                case ParticleSystemGradientMode.TwoGradients:
                    return new ParticleSystem.MinMaxGradient(
                        RetintGradientToHueAllowGrayscale(source.gradientMin, targetHue, grayscaleSaturation),
                        RetintGradientToHueAllowGrayscale(source.gradientMax, targetHue, grayscaleSaturation)
                    );

                case ParticleSystemGradientMode.RandomColor:
                    return new ParticleSystem.MinMaxGradient(
                        RetintGradientToHueAllowGrayscale(source.gradient, targetHue, grayscaleSaturation)
                    );

                default:
                    return source;
            }
        }
    }
}