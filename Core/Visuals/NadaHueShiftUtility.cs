using UnityEngine;

namespace NADA.VFX.Weapon.Core.Visuals
{
    internal static class NadaHueShiftUtility
    {
        private const float ClassicFlameHue = 0.08f;

        internal static float SliderValueToTargetHue(float sliderValue)
        {
            return WrapHue01(ClassicFlameHue + sliderValue);
        }

        private static float WrapHue01(float hue)
        {
            hue %= 1f;
            if (hue < 0f)
                hue += 1f;

            return hue;
        }

        internal static UnityEngine.Color RetintColorToHue(
            UnityEngine.Color inputColor,
            float targetHue)
        {
            UnityEngine.Color.RGBToHSV(inputColor, out _, out float saturation, out float value);

            if (saturation < 0.01f)
                return inputColor;

            UnityEngine.Color retintedColor =
                UnityEngine.Color.HSVToRGB(targetHue, saturation, value, true);

            retintedColor.a = inputColor.a;
            return retintedColor;
        }

        internal static UnityEngine.Color RetintColorToHueAllowGrayscale(
            UnityEngine.Color inputColor,
            float targetHue,
            float grayscaleSaturation = 0.85f,
            float valueBoost = 1.15f)
        {
            UnityEngine.Color.RGBToHSV(inputColor, out _, out float saturation, out float value);

            float outputSaturation = saturation < 0.01f ? grayscaleSaturation : saturation;

            // Boost brightness slightly but clamp to avoid blowout.
            float outputValue = Mathf.Clamp01(value * valueBoost);

            UnityEngine.Color retintedColor =
                UnityEngine.Color.HSVToRGB(targetHue, outputSaturation, outputValue, true);

            retintedColor.a = inputColor.a;
            return retintedColor;
        }

        internal static Gradient RetintGradientToHue(
            Gradient sourceGradient,
            float targetHue)
        {
            if (sourceGradient == null)
                return null;

            GradientColorKey[] sourceColorKeys = sourceGradient.colorKeys;
            GradientAlphaKey[] sourceAlphaKeys = sourceGradient.alphaKeys;

            var retintedColorKeys = new GradientColorKey[sourceColorKeys.Length];
            for (int colorKeyIndex = 0; colorKeyIndex < sourceColorKeys.Length; colorKeyIndex++)
            {
                retintedColorKeys[colorKeyIndex] = new GradientColorKey(
                    RetintColorToHue(sourceColorKeys[colorKeyIndex].color, targetHue),
                    sourceColorKeys[colorKeyIndex].time);
            }

            var copiedAlphaKeys = new GradientAlphaKey[sourceAlphaKeys.Length];
            for (int alphaKeyIndex = 0; alphaKeyIndex < sourceAlphaKeys.Length; alphaKeyIndex++)
            {
                copiedAlphaKeys[alphaKeyIndex] = new GradientAlphaKey(
                    sourceAlphaKeys[alphaKeyIndex].alpha,
                    sourceAlphaKeys[alphaKeyIndex].time);
            }

            var retintedGradient = new Gradient();
            retintedGradient.SetKeys(retintedColorKeys, copiedAlphaKeys);
            return retintedGradient;
        }

        internal static Gradient RetintGradientToHueAllowGrayscale(
            Gradient sourceGradient,
            float targetHue,
            float grayscaleSaturation = 0.85f)
        {
            if (sourceGradient == null)
                return null;

            GradientColorKey[] sourceColorKeys = sourceGradient.colorKeys;
            GradientAlphaKey[] sourceAlphaKeys = sourceGradient.alphaKeys;

            var retintedColorKeys = new GradientColorKey[sourceColorKeys.Length];
            for (int colorKeyIndex = 0; colorKeyIndex < sourceColorKeys.Length; colorKeyIndex++)
            {
                retintedColorKeys[colorKeyIndex] = new GradientColorKey(
                    RetintColorToHueAllowGrayscale(
                        sourceColorKeys[colorKeyIndex].color,
                        targetHue,
                        grayscaleSaturation),
                    sourceColorKeys[colorKeyIndex].time);
            }

            var copiedAlphaKeys = new GradientAlphaKey[sourceAlphaKeys.Length];
            for (int alphaKeyIndex = 0; alphaKeyIndex < sourceAlphaKeys.Length; alphaKeyIndex++)
            {
                copiedAlphaKeys[alphaKeyIndex] = new GradientAlphaKey(
                    sourceAlphaKeys[alphaKeyIndex].alpha,
                    sourceAlphaKeys[alphaKeyIndex].time);
            }

            var retintedGradient = new Gradient();
            retintedGradient.SetKeys(retintedColorKeys, copiedAlphaKeys);
            return retintedGradient;
        }

        internal static ParticleSystem.MinMaxGradient RetintMinMaxGradientToHue(
            ParticleSystem.MinMaxGradient sourceGradient,
            float targetHue)
        {
            switch (sourceGradient.mode)
            {
                case ParticleSystemGradientMode.Color:
                    return new ParticleSystem.MinMaxGradient(
                        RetintColorToHue(sourceGradient.color, targetHue));

                case ParticleSystemGradientMode.TwoColors:
                    return new ParticleSystem.MinMaxGradient(
                        RetintColorToHue(sourceGradient.colorMin, targetHue),
                        RetintColorToHue(sourceGradient.colorMax, targetHue));

                case ParticleSystemGradientMode.Gradient:
                    return new ParticleSystem.MinMaxGradient(
                        RetintGradientToHue(sourceGradient.gradient, targetHue));

                case ParticleSystemGradientMode.TwoGradients:
                    return new ParticleSystem.MinMaxGradient(
                        RetintGradientToHue(sourceGradient.gradientMin, targetHue),
                        RetintGradientToHue(sourceGradient.gradientMax, targetHue));

                case ParticleSystemGradientMode.RandomColor:
                    return new ParticleSystem.MinMaxGradient(
                        RetintGradientToHue(sourceGradient.gradient, targetHue));

                default:
                    return sourceGradient;
            }
        }

        internal static ParticleSystem.MinMaxGradient RetintMinMaxGradientToHueAllowGrayscale(
            ParticleSystem.MinMaxGradient sourceGradient,
            float targetHue,
            float grayscaleSaturation = 0.85f)
        {
            switch (sourceGradient.mode)
            {
                case ParticleSystemGradientMode.Color:
                    return new ParticleSystem.MinMaxGradient(
                        RetintColorToHueAllowGrayscale(
                            sourceGradient.color,
                            targetHue,
                            grayscaleSaturation));

                case ParticleSystemGradientMode.TwoColors:
                    return new ParticleSystem.MinMaxGradient(
                        RetintColorToHueAllowGrayscale(
                            sourceGradient.colorMin,
                            targetHue,
                            grayscaleSaturation),
                        RetintColorToHueAllowGrayscale(
                            sourceGradient.colorMax,
                            targetHue,
                            grayscaleSaturation));

                case ParticleSystemGradientMode.Gradient:
                    return new ParticleSystem.MinMaxGradient(
                        RetintGradientToHueAllowGrayscale(
                            sourceGradient.gradient,
                            targetHue,
                            grayscaleSaturation));

                case ParticleSystemGradientMode.TwoGradients:
                    return new ParticleSystem.MinMaxGradient(
                        RetintGradientToHueAllowGrayscale(
                            sourceGradient.gradientMin,
                            targetHue,
                            grayscaleSaturation),
                        RetintGradientToHueAllowGrayscale(
                            sourceGradient.gradientMax,
                            targetHue,
                            grayscaleSaturation));

                case ParticleSystemGradientMode.RandomColor:
                    return new ParticleSystem.MinMaxGradient(
                        RetintGradientToHueAllowGrayscale(
                            sourceGradient.gradient,
                            targetHue,
                            grayscaleSaturation));

                default:
                    return sourceGradient;
            }
        }
    }
}