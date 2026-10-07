using System.Collections.Generic;
using NADA.VFX.Weapon.Core.Config;
using NADA.VFX.Weapon.Core.Debug;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Blocks.Effects;
using NADA.VFX.Weapon.Runtime.Formation;
using UnityEngine;

namespace NADA.VFX.Weapon.Modules.Motion
{
    /// <summary>
    /// Runtime contract for an orbital motion instance that owns cycle phase.
    ///
    /// Followers consume only the phase from this contract. Their effective
    /// path, transform, count and visual settings still come from resolved
    /// WeaponVfxState.
    /// </summary>
    internal interface INadaOrbitalsPhaseSource
    {
        uint OrbitalsInstanceId { get; }

        bool TryGetCycleProgress01(
            out float cycleProgress01);
    }

    /// <summary>
    /// Motion behavior for one block-owned orbital instance.
    ///
    /// Formation relationships are resolved outside this component.
    /// Independent/leader instances own cycle phase. Followers consume the
    /// resolved trajectory plus phase from an explicitly bound source.
    /// </summary>
    internal sealed class NadaOrbitalsBlockMotion :
        MonoBehaviour,
        INadaOrbitalsPhaseSource
    {
        private const int MaxOrbitalsVisuals =
            40;

        private const int ArcLengthSampleCount =
            192;

        private const float HeadHistoryStepMultiplier =
            0.25f;

        private const int ExtraHistoryPadding =
            20;

        private const int MinHistoryStepPerFollower =
            1;

        private const int MaxHistoryStepPerFollower =
            36;

        private const int MaxParentHistorySamples =
            ((MaxOrbitalsVisuals - 1) *
             MaxHistoryStepPerFollower) +
            ExtraHistoryPadding;

        private const float HardLockAdherenceThreshold =
            0.999f;

        private const float SnakeSpacingMultiplier =
            0.55f;

        private static readonly Vector3 CoreSpinAxis =
            new Vector3(
                1f,
                1f,
                0f).normalized;

        private struct ParentWorldPoseSample
        {
            internal Vector3 Position;
            internal Quaternion Rotation;
        }

        private struct RuntimeState
        {
            internal bool Enabled;

            internal bool SnakeEnabled;

            internal float Count;
            internal float Speed;
            internal float Spacing;
            internal float Length;
            internal float Radius;
            internal float Cycles;
            internal float Drift;

            internal float XOffset;
            internal float YOffset;
            internal float ZOffset;

            internal float XRotation;
            internal float YRotation;
            internal float ZRotation;

            internal bool SpinEnabled;
            internal float SpinSpeed;
        }

        private RuntimeState _state;

        private bool _hasState;

        private uint _instanceId;

        private uint _trajectorySourceInstanceId;

        private bool _isFollower;

        private INadaOrbitalsPhaseSource
            _trajectoryPhaseSource;

        private string _typeId;

        private Transform _headVisualTransform;
        private Transform _followerPoolRootTransform;

        private readonly List<Transform>
            _followerVisualTransforms = new();

        private bool _visualChainDirty =
            true;

        private bool _initialized;

        private readonly List<Vector3>
            _sampledLocalPositions = new();

        private readonly List<float>
            _sampledCumulativeLengths = new();

        private float _cachedRadiusMultiplier =
            -1f;

        private float _cachedOrbitLengthMultiplier =
            -1f;

        private float _cachedTurnsPerOneWayPass =
            -1f;

        private float _cachedCycleLength;

        private bool _hasArcLengthCache;

        private Vector3 _baseHeadLocalPosition;

        private Vector3 _currentLocalOffset;

        private Quaternion _currentLocalRotationOffset =
            Quaternion.identity;

        private Quaternion _currentVisualSpinRotation =
            Quaternion.identity;

        private float _currentCycleProgress01;

        private bool _hasCurrentCycleProgress01;

        private int _lastCycleAdvanceFrame =
            -1;

        private float _currentHistoryStepPerFollower;

        private bool _hasCurrentHistoryStepPerFollower;

        private ParentWorldPoseSample[]
            _parentWorldHistorySamples;

        private int _parentWorldHistoryStartIndex;

        private int _parentWorldHistoryCount;

        uint INadaOrbitalsPhaseSource.OrbitalsInstanceId =>
            _instanceId;

        private void Awake()
        {
            NadaRuntimeDiagnostics
                .OrbitalsMotionCreated();

            _baseHeadLocalPosition =
                transform.localPosition;
        }

        private void OnDestroy()
        {
            NadaRuntimeDiagnostics
                .OrbitalsMotionDestroyed();
        }

        internal bool SetBlockState(
            VfxEffectBlock block,
            ResolvedOrbitalsFormation resolved,
            Transform headVisualTransform,
            Transform followerPoolRootTransform)
        {
            bool settingsValid =
                TryValidateSettings(
                    block,
                    out bool spinEnabled,
                    out float spinSpeed);

            bool accepted =
                settingsValid &&
                resolved != null &&
                block.InstanceId ==
                    resolved.InstanceId &&
                block.TypeId ==
                    resolved.TypeId &&
                block.Enabled ==
                    resolved.Enabled &&
                headVisualTransform != null &&
                followerPoolRootTransform != null;

            RuntimeState nextState =
                default;

            uint nextInstanceId =
                0;

            uint nextTrajectorySourceInstanceId =
                0;

            bool nextIsFollower =
                false;

            if (accepted)
            {
                nextInstanceId =
                    block.InstanceId;

                nextTrajectorySourceInstanceId =
                    resolved.TrajectorySourceInstanceId;

                nextIsFollower =
                    resolved.IsFollower;

                nextState =
                    new RuntimeState
                    {
                        Enabled =
                            block.Enabled,

                        SnakeEnabled =
                            resolved.EffectiveSnakeEnabled,

                        Count =
                            resolved.OwnCount,

                        Speed =
                            resolved.EffectiveSpeed,

                        Spacing =
                            resolved.EffectiveSpacing,

                        Length =
                            resolved.EffectiveLength,

                        Radius =
                            resolved.EffectiveRadius,

                        Cycles =
                            resolved.EffectiveCycles,

                        Drift =
                            resolved.EffectiveDrift,

                        XOffset =
                            resolved.EffectiveXOffset,

                        YOffset =
                            resolved.EffectiveYOffset,

                        ZOffset =
                            resolved.EffectiveZOffset,

                        XRotation =
                            resolved.EffectiveXRotation,

                        YRotation =
                            resolved.EffectiveYRotation,

                        ZRotation =
                            resolved.EffectiveZRotation,

                        SpinEnabled =
                            spinEnabled,

                        SpinSpeed =
                            spinSpeed
                    };
            }

            bool trajectoryIdentityChanged =
                _instanceId !=
                    nextInstanceId ||
                _trajectorySourceInstanceId !=
                    nextTrajectorySourceInstanceId ||
                _isFollower !=
                    nextIsFollower;

            _state =
                nextState;

            _hasState =
                true;

            _instanceId =
                nextInstanceId;

            _trajectorySourceInstanceId =
                nextTrajectorySourceInstanceId;

            _isFollower =
                nextIsFollower;

            _typeId =
                block?.TypeId;

            if (!accepted ||
                !_isFollower ||
                trajectoryIdentityChanged)
            {
                _trajectoryPhaseSource =
                    null;
            }

            SetExternalVisualChain(
                headVisualTransform,
                followerPoolRootTransform);

            if (!accepted)
            {
                DisableAllVisualsAndResetState();
            }

            return accepted;
        }

        internal bool BindTrajectorySource(
            INadaOrbitalsPhaseSource source)
        {
            if (!_hasState ||
                !_state.Enabled ||
                !_isFollower)
            {
                _trajectoryPhaseSource =
                    null;

                return false;
            }

            if (!IsPhaseSourceAlive(
                    source))
            {
                _trajectoryPhaseSource =
                    null;

                return false;
            }

            if (source.OrbitalsInstanceId ==
                    _instanceId ||
                source.OrbitalsInstanceId !=
                    _trajectorySourceInstanceId)
            {
                _trajectoryPhaseSource =
                    null;

                return false;
            }

            _trajectoryPhaseSource =
                source;

            return true;
        }

        internal void ClearTrajectorySource()
        {
            _trajectoryPhaseSource =
                null;
        }

        internal uint GetTrajectorySourceInstanceId()
        {
            return _trajectorySourceInstanceId;
        }

        internal bool IsFollower()
        {
            return
                _hasState &&
                _isFollower;
        }

        bool INadaOrbitalsPhaseSource.TryGetCycleProgress01(
            out float cycleProgress01)
        {
            return TryGetOwnedCycleProgress01(
                out cycleProgress01);
        }

        private bool TryGetOwnedCycleProgress01(
            out float cycleProgress01)
        {
            cycleProgress01 =
                0f;

            if (!_hasState ||
                !_state.Enabled ||
                _isFollower)
            {
                return false;
            }

            AdvanceOwnedCycleProgressForCurrentFrame();

            if (!_hasCurrentCycleProgress01)
                return false;

            cycleProgress01 =
                _currentCycleProgress01;

            return true;
        }

        private static bool IsPhaseSourceAlive(
            INadaOrbitalsPhaseSource source)
        {
            if (source == null)
                return false;

            if (source is Object unityObject &&
                unityObject == null)
            {
                return false;
            }

            return true;
        }

        private static bool TryValidateSettings(
            VfxEffectBlock block,
            out bool spinEnabled,
            out float spinSpeed)
        {
            spinEnabled =
                false;

            spinSpeed =
                0f;

            if (block == null ||
                block.Transform == null)
            {
                return false;
            }

            if (block.TypeId ==
                    VfxEffectTypeIds.OrbitalsOrbs &&
                block.Settings is OrbitalsOrbsVfxSettings orbs)
            {
                return
                    orbs.Formation?.Path != null;
            }

            if (block.TypeId ==
                    VfxEffectTypeIds.OrbitalsCores &&
                block.Settings is OrbitalsCoresVfxSettings cores)
            {
                if (cores.Formation?.Path == null)
                    return false;

                spinEnabled =
                    cores.SpinEnabled;

                spinSpeed =
                    cores.SpinSpeed;

                return true;
            }

            if (block.TypeId ==
                    VfxEffectTypeIds.OrbitalsFlames &&
                block.Settings is OrbitalsFlamesVfxSettings flames)
            {
                return
                    flames.Formation?.Path != null;
            }

            if (block.TypeId ==
                    VfxEffectTypeIds.OrbitalsEmbers &&
                block.Settings is OrbitalsEmbersVfxSettings embers)
            {
                return
                    embers.Formation?.Path != null;
            }

            return false;
        }

        private void SetExternalVisualChain(
            Transform headVisualTransform,
            Transform followerPoolRootTransform)
        {
            if (_headVisualTransform ==
                    headVisualTransform &&
                _followerPoolRootTransform ==
                    followerPoolRootTransform)
            {
                return;
            }

            _headVisualTransform =
                headVisualTransform;

            _followerPoolRootTransform =
                followerPoolRootTransform;

            _visualChainDirty =
                true;
        }

        private void LateUpdate()
        {
            if (_visualChainDirty)
            {
                ResolveVisualChain();

                _visualChainDirty =
                    false;
            }

            if (!_initialized)
                return;

            if (!_hasState ||
                !_state.Enabled)
            {
                DisableAllVisualsAndResetState();

                return;
            }

            ApplyMotion(
                _state);
        }

        private void ApplyMotion(
            RuntimeState state)
        {
            int desiredFollowerCount =
                ResolveDesiredFollowerCount(
                    state.Count);

            float radiusMultiplier =
                ClampOrbitalsRadius(
                    state.Radius);

            float orbitLengthMultiplier =
                ClampOrbitalsLength(
                    state.Length);

            float turnsPerOneWayPass =
                ClampOrbitalsCycles(
                    state.Cycles);

            _currentLocalOffset =
                new Vector3(
                    ClampOffset(
                        state.XOffset),
                    ClampOffset(
                        state.YOffset),
                    ClampOffset(
                        state.ZOffset));

            _currentLocalRotationOffset =
                Quaternion.Euler(
                    ClampRotation(
                        state.XRotation),
                    ClampRotation(
                        state.YRotation),
                    ClampRotation(
                        state.ZRotation));

            EnsureArcLengthCache(
                radiusMultiplier,
                orbitLengthMultiplier,
                turnsPerOneWayPass);

            float historyStepPerFollower =
                SmoothHistoryStepPerFollower(
                    ResolveHistoryStepPerFollowerFloat(
                        state.Spacing,
                        state.SnakeEnabled));

            float orbitAdherence =
                ResolveOrbitAdherenceFromDrift(
                    state.Drift);

            AdvanceVisualSpin(
                state);

            if (!TryResolveCycleProgressForCurrentFrame(
                    out float cycleProgress01))
            {
                DisableAllVisualsAndResetState();

                return;
            }

            ApplyHeadVisualEnabledState(
                true);

            ApplyFollowerVisualCount(
                desiredFollowerCount);

            float currentDistanceAlongCycle =
                _cachedCycleLength > 0f
                    ? cycleProgress01 *
                      _cachedCycleLength
                    : 0f;

            Vector3 currentHeadLocalPosition =
                EvaluateHeadLocalPositionAtDistance(
                    currentDistanceAlongCycle);

            transform.localPosition =
                currentHeadLocalPosition;

            RecordParentWorldHistory();

            ApplyHeadVisualPosition(
                currentDistanceAlongCycle,
                orbitAdherence,
                historyStepPerFollower);

            ApplyFollowerPositions(
                desiredFollowerCount,
                historyStepPerFollower,
                currentDistanceAlongCycle,
                orbitAdherence,
                state.SnakeEnabled);
        }

        private bool TryResolveCycleProgressForCurrentFrame(
            out float cycleProgress01)
        {
            cycleProgress01 =
                0f;

            if (!_hasState ||
                !_state.Enabled)
            {
                return false;
            }

            if (!_isFollower)
            {
                AdvanceOwnedCycleProgressForCurrentFrame();

                if (!_hasCurrentCycleProgress01)
                    return false;

                cycleProgress01 =
                    _currentCycleProgress01;

                return true;
            }

            if (!IsPhaseSourceAlive(
                    _trajectoryPhaseSource))
            {
                return false;
            }

            if (_trajectoryPhaseSource.OrbitalsInstanceId !=
                _trajectorySourceInstanceId)
            {
                return false;
            }

            if (!_trajectoryPhaseSource.TryGetCycleProgress01(
                    out float sourceCycleProgress01))
            {
                return false;
            }

            _currentCycleProgress01 =
                Mathf.Repeat(
                    sourceCycleProgress01,
                    1f);

            _hasCurrentCycleProgress01 =
                true;

            cycleProgress01 =
                _currentCycleProgress01;

            return true;
        }

        private void AdvanceOwnedCycleProgressForCurrentFrame()
        {
            int currentFrame =
                Time.frameCount;

            if (_lastCycleAdvanceFrame ==
                currentFrame)
            {
                return;
            }

            AdvanceCycleProgress(
                ClampOrbitalsSpeed(
                    _state.Speed));

            _lastCycleAdvanceFrame =
                currentFrame;
        }

        private void AdvanceVisualSpin(
            RuntimeState state)
        {
            if (_typeId !=
                    VfxEffectTypeIds.OrbitalsCores ||
                !state.SpinEnabled)
            {
                _currentVisualSpinRotation =
                    Quaternion.identity;

                return;
            }

            float spinSpeed =
                state.SpinSpeed;

            if (float.IsNaN(spinSpeed) ||
                float.IsInfinity(spinSpeed))
            {
                spinSpeed =
                    PluginConfig.DefaultCoreSpinSpeed;
            }

            float degreesThisFrame =
                spinSpeed *
                360f *
                Time.deltaTime;

            _currentVisualSpinRotation =
                Quaternion.AngleAxis(
                    degreesThisFrame,
                    CoreSpinAxis) *
                _currentVisualSpinRotation;
        }

        private void AdvanceCycleProgress(
            float cycleProgressPerSecond)
        {
            if (!_hasCurrentCycleProgress01)
            {
                _currentCycleProgress01 =
                    0f;

                _hasCurrentCycleProgress01 =
                    true;

                return;
            }

            _currentCycleProgress01 =
                Mathf.Repeat(
                    _currentCycleProgress01 +
                    cycleProgressPerSecond *
                    Time.deltaTime,
                    1f);
        }

        private void ResolveVisualChain()
        {
            _followerVisualTransforms.Clear();

            _initialized =
                false;

            if (_headVisualTransform == null ||
                _followerPoolRootTransform == null)
            {
                return;
            }

            foreach (Transform childTransform in
                     _followerPoolRootTransform)
            {
                if (childTransform != null)
                {
                    _followerVisualTransforms.Add(
                        childTransform);
                }
            }

            _initialized =
                true;
        }

        private void ApplyHeadVisualEnabledState(
            bool enabled)
        {
            if (_headVisualTransform != null &&
                _headVisualTransform.gameObject.activeSelf !=
                    enabled)
            {
                _headVisualTransform.gameObject.SetActive(
                    enabled);
            }
        }

        private void ApplyFollowerVisualCount(
            int desiredFollowerCount)
        {
            desiredFollowerCount =
                Mathf.Clamp(
                    desiredFollowerCount,
                    0,
                    MaxOrbitalsVisuals - 1);

            for (int visualIndex = 0;
                 visualIndex <
                 _followerVisualTransforms.Count;
                 visualIndex++)
            {
                Transform followerVisualTransform =
                    _followerVisualTransforms[
                        visualIndex];

                if (followerVisualTransform == null)
                    continue;

                bool shouldBeActive =
                    visualIndex <
                    desiredFollowerCount;

                if (followerVisualTransform.gameObject.activeSelf !=
                    shouldBeActive)
                {
                    followerVisualTransform.gameObject.SetActive(
                        shouldBeActive);
                }
            }
        }

        private void DisableAllVisualsAndResetState()
        {
            ApplyHeadVisualEnabledState(
                false);

            foreach (Transform followerVisualTransform in
                     _followerVisualTransforms)
            {
                if (followerVisualTransform != null &&
                    followerVisualTransform.gameObject.activeSelf)
                {
                    followerVisualTransform.gameObject.SetActive(
                        false);
                }
            }

            ResetOrbitStartPoint();
        }

        private static int ResolveDesiredFollowerCount(
            float normalizedCount)
        {
            normalizedCount =
                Mathf.Clamp01(
                    normalizedCount);

            int desiredTotalVisualCount =
                1 +
                Mathf.RoundToInt(
                    normalizedCount *
                    (MaxOrbitalsVisuals - 1));

            return Mathf.Clamp(
                desiredTotalVisualCount - 1,
                0,
                MaxOrbitalsVisuals - 1);
        }

        private static float ResolveHistoryStepPerFollowerFloat(
            float spacing,
            bool snakeSpacingEnabled)
        {
            if (snakeSpacingEnabled)
                return MinHistoryStepPerFollower;

            float spacingT =
                ClampOrbitalsSpacing(
                    spacing);

            return Mathf.Lerp(
                MinHistoryStepPerFollower,
                MaxHistoryStepPerFollower,
                spacingT);
        }

        private float SmoothHistoryStepPerFollower(
            float targetHistoryStepPerFollower)
        {
            if (!_hasCurrentHistoryStepPerFollower)
            {
                _currentHistoryStepPerFollower =
                    targetHistoryStepPerFollower;

                _hasCurrentHistoryStepPerFollower =
                    true;

                return _currentHistoryStepPerFollower;
            }

            float smoothingSpeed =
                targetHistoryStepPerFollower >
                _currentHistoryStepPerFollower
                    ? 4f
                    : 10f;

            _currentHistoryStepPerFollower =
                Mathf.Lerp(
                    _currentHistoryStepPerFollower,
                    targetHistoryStepPerFollower,
                    1f -
                    Mathf.Exp(
                        -smoothingSpeed *
                        Time.deltaTime));

            return _currentHistoryStepPerFollower;
        }

        private static float ClampOrbitalsSpacing(
            float value)
        {
            if (float.IsNaN(value) ||
                float.IsInfinity(value))
            {
                return PluginConfig.DefaultOrbitalsSpacing;
            }

            return Mathf.Clamp(
                value,
                PluginConfig.MinOrbitalsSpacing,
                PluginConfig.MaxOrbitalsSpacing);
        }

        private static float ClampOrbitalsRadius(
            float value)
        {
            if (float.IsNaN(value) ||
                float.IsInfinity(value))
            {
                return PluginConfig
                    .DefaultOrbitalsRadiusMultiplier;
            }

            return Mathf.Clamp(
                value,
                PluginConfig.MinOrbitalsRadiusMultiplier,
                PluginConfig.MaxOrbitalsRadiusMultiplier);
        }

        private static float ClampOrbitalsLength(
            float value)
        {
            if (float.IsNaN(value) ||
                float.IsInfinity(value))
            {
                return NadaOrbitalsPath
                    .DefaultOrbitLengthMultiplier;
            }

            return Mathf.Clamp(
                value,
                PluginConfig.MinOrbitalsLength,
                PluginConfig.MaxOrbitalsLength);
        }

        private static float ClampOrbitalsCycles(
            float value)
        {
            if (float.IsNaN(value) ||
                float.IsInfinity(value))
            {
                return NadaOrbitalsPath
                    .DefaultTurnsPerOneWayPass;
            }

            return Mathf.Clamp(
                value,
                PluginConfig.MinOrbitalsCycles,
                PluginConfig.MaxOrbitalsCycles);
        }

        private static float ClampOrbitalsSpeed(
            float value)
        {
            if (float.IsNaN(value) ||
                float.IsInfinity(value))
            {
                return NadaOrbitalsMotion
                    .DefaultCycleProgressPerSecond;
            }

            return Mathf.Clamp(
                value,
                PluginConfig.MinOrbitalsSpeed,
                PluginConfig.MaxOrbitalsSpeed);
        }

        private static float ResolveOrbitAdherenceFromDrift(
            float drift)
        {
            if (float.IsNaN(drift) ||
                float.IsInfinity(drift))
            {
                drift =
                    PluginConfig.DefaultDrift;
            }

            drift =
                Mathf.Clamp(
                    drift,
                    PluginConfig.MinDrift,
                    PluginConfig.MaxDrift);

            return 1f -
                   drift;
        }

        private void EnsureArcLengthCache(
            float radiusMultiplier,
            float orbitLengthMultiplier,
            float turnsPerOneWayPass)
        {
            if (_hasArcLengthCache &&
                Mathf.Approximately(
                    _cachedRadiusMultiplier,
                    radiusMultiplier) &&
                Mathf.Approximately(
                    _cachedOrbitLengthMultiplier,
                    orbitLengthMultiplier) &&
                Mathf.Approximately(
                    _cachedTurnsPerOneWayPass,
                    turnsPerOneWayPass))
            {
                return;
            }

            _cachedCycleLength =
                NadaOrbitalsPath.BuildArcLengthTable(
                    radiusMultiplier,
                    orbitLengthMultiplier,
                    turnsPerOneWayPass,
                    ArcLengthSampleCount,
                    _sampledLocalPositions,
                    _sampledCumulativeLengths);

            _cachedRadiusMultiplier =
                radiusMultiplier;

            _cachedOrbitLengthMultiplier =
                orbitLengthMultiplier;

            _cachedTurnsPerOneWayPass =
                turnsPerOneWayPass;

            _hasArcLengthCache =
                _cachedCycleLength > 0f;
        }

        private Vector3 EvaluateHeadLocalPositionAtDistance(
            float distanceAlongCycle)
        {
            Vector3 basePosition =
                _baseHeadLocalPosition +
                _currentLocalOffset;

            if (!_hasArcLengthCache ||
                _sampledLocalPositions.Count == 0 ||
                _sampledCumulativeLengths.Count == 0 ||
                _cachedCycleLength <= 0f)
            {
                return basePosition;
            }

            float wrappedDistance =
                Mathf.Repeat(
                    distanceAlongCycle,
                    _cachedCycleLength);

            int upperIndex =
                _sampledCumulativeLengths.BinarySearch(
                    wrappedDistance);

            if (upperIndex < 0)
                upperIndex = ~upperIndex;

            if (upperIndex <= 0)
            {
                return basePosition +
                       (_currentLocalRotationOffset *
                        _sampledLocalPositions[0]);
            }

            if (upperIndex >=
                _sampledCumulativeLengths.Count)
            {
                return basePosition +
                       (_currentLocalRotationOffset *
                        _sampledLocalPositions[
                            _sampledLocalPositions.Count - 1]);
            }

            int lowerIndex =
                upperIndex -
                1;

            float lowerDistance =
                _sampledCumulativeLengths[
                    lowerIndex];

            float upperDistance =
                _sampledCumulativeLengths[
                    upperIndex];

            float interpolationT =
                Mathf.Approximately(
                    lowerDistance,
                    upperDistance)
                    ? 0f
                    : Mathf.InverseLerp(
                        lowerDistance,
                        upperDistance,
                        wrappedDistance);

            Vector3 localPosition =
                Vector3.Lerp(
                    _sampledLocalPositions[
                        lowerIndex],
                    _sampledLocalPositions[
                        upperIndex],
                    interpolationT);

            return basePosition +
                   (_currentLocalRotationOffset *
                    localPosition);
        }

        private float EvaluateFollowerDistanceOffset(
            int followerIndex,
            float historyStepPerFollower,
            bool snakeSpacingEnabled)
        {
            float spacingT =
                Mathf.InverseLerp(
                    MinHistoryStepPerFollower,
                    MaxHistoryStepPerFollower,
                    historyStepPerFollower);

            float distancePerFollower =
                Mathf.Lerp(
                    0.06f,
                    0.32f,
                    spacingT);

            if (snakeSpacingEnabled)
            {
                distancePerFollower *=
                    SnakeSpacingMultiplier;
            }

            return followerIndex *
                   distancePerFollower;
        }

        private Vector3 EvaluateLockedFollowerWorldPosition(
            int followerIndex,
            float historyStepPerFollower,
            float currentDistanceAlongCycle,
            bool snakeSpacingEnabled)
        {
            float distanceOffset =
                EvaluateFollowerDistanceOffset(
                    followerIndex,
                    historyStepPerFollower,
                    snakeSpacingEnabled);

            return EvaluateHeadWorldPositionAtDistance(
                currentDistanceAlongCycle -
                distanceOffset);
        }

        private void ApplyHeadVisualPosition(
            float currentDistanceAlongCycle,
            float orbitAdherence,
            float historyStepPerFollower)
        {
            if (_headVisualTransform == null)
                return;

            Vector3 lockedHeadWorldPosition =
                transform.position;

            if (orbitAdherence >=
                HardLockAdherenceThreshold)
            {
                _headVisualTransform.position =
                    lockedHeadWorldPosition;

                _headVisualTransform.rotation =
                    _currentVisualSpinRotation;

                return;
            }

            float headHistorySampleIndex =
                Mathf.Max(
                    0.15f,
                    historyStepPerFollower *
                    HeadHistoryStepMultiplier);

            Vector3 driftedHeadWorldPosition =
                EvaluateDriftedWorldPosition(
                    currentDistanceAlongCycle,
                    headHistorySampleIndex);

            _headVisualTransform.position =
                Vector3.Lerp(
                    driftedHeadWorldPosition,
                    lockedHeadWorldPosition,
                    orbitAdherence);

            _headVisualTransform.rotation =
                _currentVisualSpinRotation;
        }

        private void ApplyFollowerPositions(
            int desiredFollowerCount,
            float historyStepPerFollower,
            float currentDistanceAlongCycle,
            float orbitAdherence,
            bool snakeSpacingEnabled)
        {
            desiredFollowerCount =
                Mathf.Clamp(
                    desiredFollowerCount,
                    0,
                    MaxOrbitalsVisuals - 1);

            int availableFollowerCount =
                Mathf.Min(
                    desiredFollowerCount,
                    _followerVisualTransforms.Count);

            for (int visibleIndex = 0;
                 visibleIndex <
                 availableFollowerCount;
                 visibleIndex++)
            {
                Transform followerVisualTransform =
                    _followerVisualTransforms[
                        visibleIndex];

                if (followerVisualTransform == null)
                    continue;

                int followerIndex =
                    visibleIndex +
                    1;

                Vector3 lockedWorldPosition =
                    EvaluateLockedFollowerWorldPosition(
                        followerIndex,
                        historyStepPerFollower,
                        currentDistanceAlongCycle,
                        snakeSpacingEnabled);

                if (orbitAdherence >=
                    HardLockAdherenceThreshold)
                {
                    followerVisualTransform.position =
                        lockedWorldPosition;

                    followerVisualTransform.rotation =
                        _currentVisualSpinRotation;

                    continue;
                }

                float followerDistanceOffset =
                    EvaluateFollowerDistanceOffset(
                        followerIndex,
                        historyStepPerFollower,
                        snakeSpacingEnabled);

                Vector3 driftedWorldPosition =
                    EvaluateDriftedWorldPosition(
                        currentDistanceAlongCycle -
                        followerDistanceOffset,
                        followerIndex *
                        historyStepPerFollower);

                followerVisualTransform.position =
                    Vector3.Lerp(
                        driftedWorldPosition,
                        lockedWorldPosition,
                        orbitAdherence);

                followerVisualTransform.rotation =
                    _currentVisualSpinRotation;
            }
        }

        private void ResetOrbitStartPoint()
        {
            _currentCycleProgress01 =
                0f;

            _hasCurrentCycleProgress01 =
                false;

            _lastCycleAdvanceFrame =
                -1;

            _currentHistoryStepPerFollower =
                0f;

            _hasCurrentHistoryStepPerFollower =
                false;

            ClearParentWorldHistory();
        }

        private void ClearParentWorldHistory()
        {
            _parentWorldHistoryStartIndex =
                0;

            _parentWorldHistoryCount =
                0;
        }

        private void EnsureParentWorldHistoryBuffer()
        {
            if (_parentWorldHistorySamples != null)
                return;

            _parentWorldHistorySamples =
                new ParentWorldPoseSample[
                    MaxParentHistorySamples];
        }

        private void RecordParentWorldHistory()
        {
            Transform parentTransform =
                transform.parent;

            if (parentTransform == null)
                return;

            EnsureParentWorldHistoryBuffer();

            _parentWorldHistoryStartIndex =
                (_parentWorldHistoryStartIndex -
                 1 +
                 MaxParentHistorySamples) %
                MaxParentHistorySamples;

            _parentWorldHistorySamples[
                _parentWorldHistoryStartIndex] =
                new ParentWorldPoseSample
                {
                    Position =
                        parentTransform.position,

                    Rotation =
                        parentTransform.rotation
                };

            if (_parentWorldHistoryCount <
                MaxParentHistorySamples)
            {
                _parentWorldHistoryCount++;
            }
        }

        private ParentWorldPoseSample
            GetParentWorldHistorySample(
                int logicalIndex)
        {
            logicalIndex =
                Mathf.Clamp(
                    logicalIndex,
                    0,
                    _parentWorldHistoryCount - 1);

            int physicalIndex =
                (_parentWorldHistoryStartIndex +
                 logicalIndex) %
                MaxParentHistorySamples;

            return _parentWorldHistorySamples[
                physicalIndex];
        }

        private Vector3 EvaluateHeadWorldPositionAtDistance(
            float distanceAlongCycle)
        {
            Vector3 localPosition =
                EvaluateHeadLocalPositionAtDistance(
                    distanceAlongCycle);

            Transform parentTransform =
                transform.parent;

            return parentTransform != null
                ? parentTransform.TransformPoint(
                    localPosition)
                : localPosition;
        }

        private Vector3 EvaluateDriftedWorldPosition(
            float distanceAlongCycle,
            float historySampleIndex)
        {
            Vector3 localPosition =
                EvaluateHeadLocalPositionAtDistance(
                    distanceAlongCycle);

            Transform parentTransform =
                transform.parent;

            if (parentTransform == null)
                return localPosition;

            Vector3 sampledParentPosition =
                SampleParentWorldPosition(
                    historySampleIndex,
                    parentTransform.position);

            Quaternion sampledParentRotation =
                SampleParentWorldRotation(
                    historySampleIndex,
                    parentTransform.rotation);

            return sampledParentPosition +
                   sampledParentRotation *
                   localPosition;
        }

        private Vector3 SampleParentWorldPosition(
            float sampleIndex,
            Vector3 fallbackPosition)
        {
            if (_parentWorldHistoryCount == 0)
                return fallbackPosition;

            int lowerIndex =
                Mathf.FloorToInt(
                    sampleIndex);

            int upperIndex =
                Mathf.CeilToInt(
                    sampleIndex);

            if (lowerIndex >=
                _parentWorldHistoryCount)
            {
                return GetParentWorldHistorySample(
                        _parentWorldHistoryCount - 1)
                    .Position;
            }

            if (upperIndex >=
                _parentWorldHistoryCount)
            {
                return GetParentWorldHistorySample(
                        _parentWorldHistoryCount - 1)
                    .Position;
            }

            if (lowerIndex == upperIndex)
            {
                return GetParentWorldHistorySample(
                        lowerIndex)
                    .Position;
            }

            float interpolationT =
                sampleIndex -
                lowerIndex;

            return Vector3.Lerp(
                GetParentWorldHistorySample(
                        lowerIndex)
                    .Position,
                GetParentWorldHistorySample(
                        upperIndex)
                    .Position,
                interpolationT);
        }

        private Quaternion SampleParentWorldRotation(
            float sampleIndex,
            Quaternion fallbackRotation)
        {
            if (_parentWorldHistoryCount == 0)
                return fallbackRotation;

            int lowerIndex =
                Mathf.FloorToInt(
                    sampleIndex);

            int upperIndex =
                Mathf.CeilToInt(
                    sampleIndex);

            if (lowerIndex >=
                _parentWorldHistoryCount)
            {
                return GetParentWorldHistorySample(
                        _parentWorldHistoryCount - 1)
                    .Rotation;
            }

            if (upperIndex >=
                _parentWorldHistoryCount)
            {
                return GetParentWorldHistorySample(
                        _parentWorldHistoryCount - 1)
                    .Rotation;
            }

            if (lowerIndex == upperIndex)
            {
                return GetParentWorldHistorySample(
                        lowerIndex)
                    .Rotation;
            }

            float interpolationT =
                sampleIndex -
                lowerIndex;

            return Quaternion.Slerp(
                GetParentWorldHistorySample(
                        lowerIndex)
                    .Rotation,
                GetParentWorldHistorySample(
                        upperIndex)
                    .Rotation,
                interpolationT);
        }

        private static float ClampOffset(
            float value)
        {
            if (float.IsNaN(value) ||
                float.IsInfinity(value))
            {
                return PluginConfig.DefaultEffectOffset;
            }

            return Mathf.Clamp(
                value,
                PluginConfig.MinEffectOffset,
                PluginConfig.MaxEffectOffset);
        }

        private static float ClampRotation(
            float value)
        {
            if (float.IsNaN(value) ||
                float.IsInfinity(value))
            {
                return PluginConfig.DefaultEffectRotation;
            }

            return Mathf.Clamp(
                value,
                PluginConfig.MinEffectRotation,
                PluginConfig.MaxEffectRotation);
        }
    }
}