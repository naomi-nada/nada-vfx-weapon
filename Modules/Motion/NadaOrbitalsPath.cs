using UnityEngine;

namespace NADA.VFX.Modules.Motion
{
    internal static class NadaOrbitalsPath
    {
        internal const float DefaultTravelLength = 2.0f;
        internal const float DefaultSpiralRadius = 0.35f;
        internal const float DefaultBladeCenterOffset = 0.85f;

        internal const float DefaultOneWayPassDuration = 5.0f;
        internal const float DefaultTurnDuration = 0.80f;
        internal const float DefaultTurnsPerOneWayPass = 2.0f;

        internal static Vector3 EvaluateLocalPosition(
            float timeValue,
            float radiusMultiplier,
            Vector3 baseLocalPosition)
        {
            float halfLength = DefaultTravelLength * 0.5f;
            float spiralRadius = DefaultSpiralRadius * radiusMultiplier;

            float upTime = DefaultOneWayPassDuration;
            float turnTime = DefaultTurnDuration;
            float downTime = DefaultOneWayPassDuration;
            float bottomTurnTime = DefaultTurnDuration;

            float cycleDuration = upTime + turnTime + downTime + bottomTurnTime;
            float cycleTime = Mathf.Repeat(timeValue, cycleDuration);

            Vector3 localOffset;

            if (cycleTime < upTime)
            {
                float normalizedSegmentTime = cycleTime / upTime;
                float y = Mathf.Lerp(-halfLength, halfLength, normalizedSegmentTime) + DefaultBladeCenterOffset;

                float angle = normalizedSegmentTime * DefaultTurnsPerOneWayPass * Mathf.PI * 2f;
                float x = Mathf.Cos(angle) * spiralRadius;
                float z = Mathf.Sin(angle) * spiralRadius;

                localOffset = new Vector3(x, y, z);
            }
            else if (cycleTime < upTime + turnTime)
            {
                float normalizedSegmentTime = (cycleTime - upTime) / turnTime;

                float startAngle = DefaultTurnsPerOneWayPass * Mathf.PI * 2f;
                float angle = startAngle + (normalizedSegmentTime * Mathf.PI);

                float x = Mathf.Cos(angle) * spiralRadius;
                float z = Mathf.Sin(angle) * spiralRadius;
                float y = halfLength + DefaultBladeCenterOffset;

                localOffset = new Vector3(x, y, z);
            }
            else if (cycleTime < upTime + turnTime + downTime)
            {
                float normalizedSegmentTime = (cycleTime - upTime - turnTime) / downTime;
                float y = Mathf.Lerp(halfLength, -halfLength, normalizedSegmentTime) + DefaultBladeCenterOffset;

                float angle = Mathf.PI + (normalizedSegmentTime * DefaultTurnsPerOneWayPass * Mathf.PI * 2f);
                float x = Mathf.Cos(angle) * spiralRadius;
                float z = Mathf.Sin(angle) * spiralRadius;

                localOffset = new Vector3(x, y, z);
            }
            else
            {
                float normalizedSegmentTime = (cycleTime - upTime - turnTime - downTime) / bottomTurnTime;

                float startAngle = Mathf.PI + (DefaultTurnsPerOneWayPass * Mathf.PI * 2f);
                float angle = startAngle + (normalizedSegmentTime * Mathf.PI);

                float x = Mathf.Cos(angle) * spiralRadius;
                float z = Mathf.Sin(angle) * spiralRadius;
                float y = -halfLength + DefaultBladeCenterOffset;

                localOffset = new Vector3(x, y, z);
            }

            return baseLocalPosition + localOffset;
        }

        internal static float EvaluateFollowerTemporalOffsetSeconds(
            int followerIndex,
            float historyStepPerFollower,
            float minHistoryStepPerFollower,
            float maxHistoryStepPerFollower)
        {
            float cycleDuration =
                DefaultOneWayPassDuration +
                DefaultTurnDuration +
                DefaultOneWayPassDuration +
                DefaultTurnDuration;

            float spacingT = Mathf.InverseLerp(
                minHistoryStepPerFollower,
                maxHistoryStepPerFollower,
                historyStepPerFollower);

            float minSecondsPerFollower = cycleDuration * 0.01f;
            float maxSecondsPerFollower = cycleDuration * 0.04f;

            float secondsPerFollower = Mathf.Lerp(
                minSecondsPerFollower,
                maxSecondsPerFollower,
                spacingT);

            return followerIndex * secondsPerFollower;
        }
    }
}