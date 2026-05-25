using System.Collections.Generic;
using UnityEngine;

namespace NADA.VFX.Weapon.Modules.Motion
{
    internal static class NadaOrbitalsPath
    {
        // Config-facing defaults
        internal const float DefaultOrbitLengthMultiplier = 1.0f;
        internal const float DefaultTurnsPerOneWayPass = 2.0f;

        // Path shape defaults
        private const float DefaultTravelLength = 2.0f;
        private const float DefaultSpiralRadius = 0.35f;
        private const float DefaultBladeCenterOffset = 0.0f;

        // Cycle shape defaults
        private const float DefaultOneWayPassDuration = 5.0f;
        private const float DefaultTurnDuration = 0.80f;

        private const float DefaultCycleDurationSeconds =
            (DefaultOneWayPassDuration * 2f) +
            (DefaultTurnDuration * 2f);

        internal static Vector3 EvaluateLocalPositionAtCycleT(
            float cycleT,
            float radiusMultiplier,
            float orbitLengthMultiplier,
            float turnsPerOneWayPass,
            Vector3 baseLocalPosition)
        {
            cycleT = Mathf.Repeat(cycleT, 1f);

            float halfLength = (DefaultTravelLength * orbitLengthMultiplier) * 0.5f;
            float spiralRadius = DefaultSpiralRadius * radiusMultiplier;

            float upFraction = DefaultOneWayPassDuration / DefaultCycleDurationSeconds;
            float turnFraction = DefaultTurnDuration / DefaultCycleDurationSeconds;

            Vector3 localOffset =
                EvaluateCycleSegment(
                    cycleT,
                    halfLength,
                    spiralRadius,
                    turnsPerOneWayPass,
                    upFraction,
                    turnFraction);

            return baseLocalPosition + localOffset;
        }

        internal static float BuildArcLengthTable(
            float radiusMultiplier,
            float orbitLengthMultiplier,
            float turnsPerOneWayPass,
            int sampleCount,
            List<Vector3> sampledLocalPositions,
            List<float> sampledCumulativeLengths)
        {
            sampledLocalPositions.Clear();
            sampledCumulativeLengths.Clear();

            sampleCount = Mathf.Max(2, sampleCount);

            Vector3 previousPosition = EvaluateLocalPositionAtCycleT(
                0f,
                radiusMultiplier,
                orbitLengthMultiplier,
                turnsPerOneWayPass,
                Vector3.zero);

            sampledLocalPositions.Add(previousPosition);
            sampledCumulativeLengths.Add(0f);

            float totalLength = 0f;

            for (int sampleIndex = 1; sampleIndex < sampleCount; sampleIndex++)
            {
                float cycleT = sampleIndex / (float)(sampleCount - 1);

                Vector3 currentPosition = EvaluateLocalPositionAtCycleT(
                    cycleT,
                    radiusMultiplier,
                    orbitLengthMultiplier,
                    turnsPerOneWayPass,
                    Vector3.zero);

                totalLength += Vector3.Distance(previousPosition, currentPosition);

                sampledLocalPositions.Add(currentPosition);
                sampledCumulativeLengths.Add(totalLength);

                previousPosition = currentPosition;
            }

            return totalLength;
        }

        private static Vector3 EvaluateCycleSegment(
            float cycleT,
            float halfLength,
            float spiralRadius,
            float turnsPerOneWayPass,
            float upFraction,
            float turnFraction)
        {
            float downStart = upFraction + turnFraction;
            float bottomTurnStart = downStart + upFraction;

            if (cycleT < upFraction)
            {
                float segmentT = cycleT / upFraction;
                return EvaluateSpiralPass(
                    segmentT,
                    -halfLength,
                    halfLength,
                    0f,
                    turnsPerOneWayPass,
                    spiralRadius);
            }

            if (cycleT < downStart)
            {
                float segmentT = (cycleT - upFraction) / turnFraction;
                return EvaluateTurn(
                    segmentT,
                    halfLength,
                    turnsPerOneWayPass * Mathf.PI * 2f,
                    spiralRadius);
            }

            if (cycleT < bottomTurnStart)
            {
                float segmentT = (cycleT - downStart) / upFraction;
                return EvaluateSpiralPass(
                    segmentT,
                    halfLength,
                    -halfLength,
                    Mathf.PI,
                    turnsPerOneWayPass,
                    spiralRadius);
            }

            {
                float segmentT = (cycleT - bottomTurnStart) / turnFraction;
                return EvaluateTurn(
                    segmentT,
                    -halfLength,
                    Mathf.PI + (turnsPerOneWayPass * Mathf.PI * 2f),
                    spiralRadius);
            }
        }

        private static Vector3 EvaluateSpiralPass(
            float segmentT,
            float startY,
            float endY,
            float startAngle,
            float turnsPerOneWayPass,
            float spiralRadius)
        {
            float y = Mathf.Lerp(startY, endY, segmentT) + DefaultBladeCenterOffset;
            float angle = startAngle + (segmentT * turnsPerOneWayPass * Mathf.PI * 2f);

            return new Vector3(
                Mathf.Cos(angle) * spiralRadius,
                y,
                Mathf.Sin(angle) * spiralRadius);
        }

        private static Vector3 EvaluateTurn(
            float segmentT,
            float y,
            float startAngle,
            float spiralRadius)
        {
            float angle = startAngle + (segmentT * Mathf.PI);

            return new Vector3(
                Mathf.Cos(angle) * spiralRadius,
                y + DefaultBladeCenterOffset,
                Mathf.Sin(angle) * spiralRadius);
        }
    }
}