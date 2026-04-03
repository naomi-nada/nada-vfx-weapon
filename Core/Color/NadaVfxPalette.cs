// File: VFX/NadaVfxPalette.cs
using UnityEngine;
using UColor = UnityEngine.Color;

namespace NADA.VFX.Core.Color
{
    internal sealed class NadaVfxPalette
    {
        internal string Name;
        internal Gradient ColorOverLifetime;
        internal Gradient Custom1;
        internal Gradient Custom2;
        internal UColor? PointLightColor;

        internal static NadaVfxPalette BlueTorchTest()
        {
            return new NadaVfxPalette
            {
                Name = "BlueTorchTest",

                // From your blue ground torch dump:
                // ColorOverLifetime: white -> white
                ColorOverLifetime = MakeGradient(
                    new GradientColorKey(new UColor(1f, 1f, 1f, 1f), 0f),
                    new GradientColorKey(new UColor(1f, 1f, 1f, 1f), 1f)
                ),

                // Custom1:
                // (0:RGBA(0.778, 0.876, 1.320, 1.000))
                // (1:RGBA(1.000, 1.097, 4.000, 1.000))
                Custom1 = MakeGradient(
                    new GradientColorKey(new UColor(0.778f, 0.876f, 1.320f, 1f), 0f),
                    new GradientColorKey(new UColor(1.000f, 1.097f, 4.000f, 1f), 1f)
                ),

                // Custom2:
                // (0:RGBA(0.000, 0.055, 0.749, 1.000))
                // (1:RGBA(0.000, 0.786, 1.000, 1.000))
                Custom2 = MakeGradient(
                    new GradientColorKey(new UColor(0.000f, 0.055f, 0.749f, 1f), 0f),
                    new GradientColorKey(new UColor(0.000f, 0.786f, 1.000f, 1f), 1f)
                ),

                // Blue torch point light
                PointLightColor = new UColor(0.482f, 0.771f, 1.000f, 1f)
            };
        }

        private static Gradient MakeGradient(params GradientColorKey[] colorKeys)
        {
            var g = new Gradient();

            var alphaKeys = new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            };

            g.SetKeys(colorKeys, alphaKeys);
            return g;
        }
    }
}