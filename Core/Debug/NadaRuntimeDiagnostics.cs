using UnityEngine;

namespace NADA.VFX.Weapon.Core.Debug
{
    /// <summary>
    /// Temporary runtime health counters for tracking down multiplayer slowdown.
    /// Keep this cheap. We're trying to observe the problem, not become part of it. ^-^
    /// </summary>
    internal static class NadaRuntimeDiagnostics
    {
        private const float ReportIntervalSeconds = 10f;

        private static int _liveInnerFlamesEffects;
        private static int _liveOrbitalsEffects;
        private static int _liveOrbitalsMotions;

        private static int _remoteAppliesSinceLastReport;
        private static long _remoteAppliesTotal;

        private static float _nextReportTime;

        private static int _lastReportedInnerFlamesEffects = -1;
        private static int _lastReportedOrbitalsEffects = -1;
        private static int _lastReportedOrbitalsMotions = -1;

        internal static void InnerFlamesCreated()
        {
            _liveInnerFlamesEffects++;
        }

        internal static void InnerFlamesDestroyed()
        {
            if (_liveInnerFlamesEffects > 0)
                _liveInnerFlamesEffects--;
        }

        internal static void OrbitalsEffectCreated()
        {
            _liveOrbitalsEffects++;
        }

        internal static void OrbitalsEffectDestroyed()
        {
            if (_liveOrbitalsEffects > 0)
                _liveOrbitalsEffects--;
        }

        internal static void OrbitalsMotionCreated()
        {
            _liveOrbitalsMotions++;
        }

        internal static void OrbitalsMotionDestroyed()
        {
            if (_liveOrbitalsMotions > 0)
                _liveOrbitalsMotions--;
        }

        internal static void RecordRemoteApply()
        {
            _remoteAppliesSinceLastReport++;
            _remoteAppliesTotal++;
        }

        internal static void Tick()
        {
            if (!Plugin.DebugLoggingEnabled.Value)
                return;

            if (Time.unscaledTime < _nextReportTime)
                return;

            _nextReportTime = Time.unscaledTime + ReportIntervalSeconds;

            int remoteAppliesThisInterval = _remoteAppliesSinceLastReport;
            _remoteAppliesSinceLastReport = 0;

            bool componentCountsChanged =
                _liveInnerFlamesEffects != _lastReportedInnerFlamesEffects ||
                _liveOrbitalsEffects != _lastReportedOrbitalsEffects ||
                _liveOrbitalsMotions != _lastReportedOrbitalsMotions;

            if (!componentCountsChanged &&
                remoteAppliesThisInterval == 0)
            {
                return;
            }

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [RuntimeHealth] " +
                $"inner={_liveInnerFlamesEffects} " +
                $"orbitalsFx={_liveOrbitalsEffects} " +
                $"orbitalsMotion={_liveOrbitalsMotions} " +
                $"remoteApplies10s={remoteAppliesThisInterval} " +
                $"remoteAppliesTotal={_remoteAppliesTotal}");

            _lastReportedInnerFlamesEffects = _liveInnerFlamesEffects;
            _lastReportedOrbitalsEffects = _liveOrbitalsEffects;
            _lastReportedOrbitalsMotions = _liveOrbitalsMotions;
        }
    }
}