using System.Collections.Generic;
using UnityEngine;

namespace NADA.VFX.Modules.Motion
{
    internal static class NadaOrbitalsPath
    {
        internal const float DefaultTravelLength = 2.0f;
        internal const float DefaultSpiralRadius = 0.35f;
        internal const float DefaultBladeCenterOffset = 0.0f;

        internal const float DefaultOneWayPassDuration = 5.0f;
        internal const float DefaultTurnDuration = 0.80f;
        internal const float DefaultTurnsPerOneWayPass = 2.0f;

        internal const float DefaultOrbitLengthMultiplier = 1.0f;

        internal const float DefaultCycleDurationSeconds =
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

            float totalDuration = DefaultCycleDurationSeconds;

            float upFraction = DefaultOneWayPassDuration / totalDuration;
            float topTurnFraction = DefaultTurnDuration / totalDuration;
            float downFraction = DefaultOneWayPassDuration / totalDuration;
            float bottomTurnFraction = DefaultTurnDuration / totalDuration;

            Vector3 localOffset;

            if (cycleT < upFraction)
            {
                float segmentT = cycleT / upFraction;

                float y = Mathf.Lerp(-halfLength, halfLength, segmentT) + DefaultBladeCenterOffset;
                float angle = segmentT * turnsPerOneWayPass * Mathf.PI * 2f;

                float x = Mathf.Cos(angle) * spiralRadius;
                float z = Mathf.Sin(angle) * spiralRadius;

                localOffset = new Vector3(x, y, z);
            }
            else if (cycleT < upFraction + topTurnFraction)
            {
                float segmentT = (cycleT - upFraction) / topTurnFraction;

                float startAngle = turnsPerOneWayPass * Mathf.PI * 2f;
                float angle = startAngle + (segmentT * Mathf.PI);

                float x = Mathf.Cos(angle) * spiralRadius;
                float z = Mathf.Sin(angle) * spiralRadius;
                float y = halfLength + DefaultBladeCenterOffset;

                localOffset = new Vector3(x, y, z);
            }
            else if (cycleT < upFraction + topTurnFraction + downFraction)
            {
                float segmentT = (cycleT - upFraction - topTurnFraction) / downFraction;

                float y = Mathf.Lerp(halfLength, -halfLength, segmentT) + DefaultBladeCenterOffset;
                float angle = Mathf.PI + (segmentT * turnsPerOneWayPass * Mathf.PI * 2f);

                float x = Mathf.Cos(angle) * spiralRadius;
                float z = Mathf.Sin(angle) * spiralRadius;

                localOffset = new Vector3(x, y, z);
            }
            else
            {
                float segmentT = (cycleT - upFraction - topTurnFraction - downFraction) / bottomTurnFraction;

                float startAngle = Mathf.PI + (turnsPerOneWayPass * Mathf.PI * 2f);
                float angle = startAngle + (segmentT * Mathf.PI);

                float x = Mathf.Cos(angle) * spiralRadius;
                float z = Mathf.Sin(angle) * spiralRadius;
                float y = -halfLength + DefaultBladeCenterOffset;

                localOffset = new Vector3(x, y, z);
            }

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
    }
}