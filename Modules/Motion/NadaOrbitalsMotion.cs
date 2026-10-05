using System.Collections.Generic;
using NADA.VFX.Weapon.Core.Config;
using NADA.VFX.Weapon.Core.State;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Blocks.Effects;
using NADA.VFX.Weapon.Core.Debug;
using UnityEngine;

namespace NADA.VFX.Weapon.Modules.Motion
{
    internal sealed class NadaOrbitalsMotion : MonoBehaviour
    {
        private const float LocalStateRefreshInterval = 0.05f;

        private VfxState _resolvedState;
        private bool _hasResolvedState;

        private VfxState _cachedLocalState;
        private bool _hasCachedLocalState;
        private bool _cachedLocalStateIsBound;
        private float _nextLocalStateRefreshTime;

        private OrbitalsOrbsMotionRuntimeState _orbsBlockState;
        private bool _hasOrbsBlockState;

        private const int MaxOrbitalsVisuals = 40;
        private const int ArcLengthSampleCount = 192;

        internal const float DefaultCycleProgressPerSecond = 0.10f;

        private const float HeadHistoryStepMultiplier = 0.25f;

        private const int ExtraHistoryPadding = 20;
        private const int MinHistoryStepPerFollower = 1;
        private const int MaxHistoryStepPerFollower = 36;

        private const int MaxParentHistorySamples =
            ((MaxOrbitalsVisuals - 1) * MaxHistoryStepPerFollower) +
            ExtraHistoryPadding;

        private const float HardLockAdherenceThreshold = 0.999f;
        private const float SnakeSpacingMultiplier = 0.55f;

        private static readonly Vector3 CoreSpinAxis =
            new Vector3(1f, 1f, 0f).normalized;

        private struct ParentWorldPoseSample
        {
            internal Vector3 Position;
            internal Quaternion Rotation;
        }

        private struct OrbitalsOrbsMotionRuntimeState
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
        }

        private NadaOrbitalsFamily _orbitalsFamily =
            NadaOrbitalsFamily.Orbs;

        private global::ItemDrop.ItemData _itemData;

        private Transform _headVisualTransform;
        private Transform _followerPoolRootTransform;

        private readonly List<Transform>
            _followerVisualTransforms = new();

        private bool _configuredVisualChain;
        private bool _visualChainDirty = true;
        private bool _initialized;

        private readonly List<Vector3>
            _sampledLocalPositions = new();

        private readonly List<float>
            _sampledCumulativeLengths = new();

        private float _cachedRadiusMultiplier = -1f;
        private float _cachedOrbitLengthMultiplier = -1f;
        private float _cachedTurnsPerOneWayPass = -1f;
        private float _cachedCycleLength;
        private bool _hasArcLengthCache;

        private bool _lastGlueEnabled;
        private bool _hasLastGlueEnabled;

        private NadaOrbitalsMotion _glueSourceMotion;
        private bool _hasExplicitGlueSourceMotion;

        private Vector3 _baseHeadLocalPosition;
        private Vector3 _currentLocalOffset;

        private Quaternion _currentLocalRotationOffset =
            Quaternion.identity;

        private Quaternion _currentVisualSpinRotation =
            Quaternion.identity;

        private float _currentCycleProgress01;
        private bool _hasCurrentCycleProgress01;

        private float _currentHistoryStepPerFollower;
        private bool _hasCurrentHistoryStepPerFollower;

        private ParentWorldPoseSample[] _parentWorldHistorySamples;
        private int _parentWorldHistoryStartIndex;
        private int _parentWorldHistoryCount;

        internal bool IsFamily(
            NadaOrbitalsFamily family)
        {
            return _orbitalsFamily ==
                   family;
        }

        internal void Configure(
            NadaOrbitalsFamily orbitalsFamily,
            global::ItemDrop.ItemData itemData)
        {
            bool preserveOrbsBlockState =
                orbitalsFamily ==
                    NadaOrbitalsFamily.Orbs &&
                _orbitalsFamily ==
                    NadaOrbitalsFamily.Orbs &&
                _hasOrbsBlockState &&
                _itemData == itemData &&
                itemData != null &&
                VfxStateIO.IsBound(itemData);

            if (_orbitalsFamily != orbitalsFamily)
            {
                _glueSourceMotion =
                    null;

                _hasExplicitGlueSourceMotion =
                    false;

                ResetOrbitStartPoint();
            }

            _orbitalsFamily =
                orbitalsFamily;

            _itemData =
                itemData;

            _hasResolvedState =
                false;

            if (!preserveOrbsBlockState)
            {
                _hasOrbsBlockState =
                    false;
            }

            InvalidateLocalStateCache();
        }

        internal void Configure(
            NadaOrbitalsFamily orbitalsFamily,
            VfxState state)
        {
            if (_orbitalsFamily != orbitalsFamily)
            {
                _glueSourceMotion =
                    null;

                _hasExplicitGlueSourceMotion =
                    false;

                ResetOrbitStartPoint();
            }

            _orbitalsFamily =
                orbitalsFamily;

            _resolvedState =
                state;

            _hasResolvedState =
                true;

            _itemData =
                null;

            // Remote/legacy resolved state remains authoritative until
            // that path is explicitly migrated.
            _hasOrbsBlockState =
                false;

            InvalidateLocalStateCache();
        }

        internal bool ConfigureOrbitalsOrbsBlock(
            VfxEffectBlock block)
        {
            if (_orbitalsFamily !=
                NadaOrbitalsFamily.Orbs)
            {
                ResetOrbitStartPoint();
            }

            _orbitalsFamily =
                NadaOrbitalsFamily.Orbs;

            _itemData =
                null;

            _hasResolvedState =
                false;

            _glueSourceMotion =
                null;

            _hasExplicitGlueSourceMotion =
                false;

            InvalidateLocalStateCache();

            return SetOrbitalsOrbsBlockState(
                block);
        }

        internal bool SetOrbitalsOrbsBlockState(
            VfxEffectBlock block)
        {
            OrbitalsOrbsVfxSettings orbsSettings =
                block?.Settings as OrbitalsOrbsVfxSettings;

            bool accepted =
                _orbitalsFamily ==
                    NadaOrbitalsFamily.Orbs &&
                block != null &&
                block.Transform != null &&
                block.TypeId ==
                    VfxEffectTypeIds.OrbitalsOrbs &&
                orbsSettings?.Path != null;

            OrbitalsOrbsMotionRuntimeState nextState =
                default;

            if (accepted)
            {
                nextState =
                    new OrbitalsOrbsMotionRuntimeState
                    {
                        Enabled =
                            block.Enabled,

                        SnakeEnabled =
                            orbsSettings.Path.SnakeEnabled,

                        Count =
                            orbsSettings.Path.Count,

                        Speed =
                            orbsSettings.Path.Speed,

                        Spacing =
                            orbsSettings.Path.Spacing,

                        Length =
                            orbsSettings.Path.Length,

                        Radius =
                            orbsSettings.Path.Radius,

                        Cycles =
                            orbsSettings.Path.Cycles,

                        Drift =
                            orbsSettings.Path.Drift,

                        XOffset =
                            block.Transform.XOffset,

                        YOffset =
                            block.Transform.YOffset,

                        ZOffset =
                            block.Transform.ZOffset,

                        XRotation =
                            block.Transform.XRotation,

                        YRotation =
                            block.Transform.YRotation,

                        ZRotation =
                            block.Transform.ZRotation
                    };
            }

            // Once Orbs motion has been handed to the block path,
            // malformed state is still authoritative and fails closed.
            // Do not silently fall back to local config or ItemData.
            _orbsBlockState =
                nextState;

            _hasOrbsBlockState =
                true;

            return accepted;
        }

        internal void SetGlueSourceMotion(
            NadaOrbitalsMotion glueSourceMotion)
        {
            if (_hasExplicitGlueSourceMotion &&
                _glueSourceMotion ==
                    glueSourceMotion)
            {
                return;
            }

            _glueSourceMotion =
                glueSourceMotion;

            _hasExplicitGlueSourceMotion =
                true;

            ResetOrbitStartPoint();
        }

        internal void ClearExplicitGlueSourceMotion()
        {
            if (!_hasExplicitGlueSourceMotion &&
                _glueSourceMotion == null)
            {
                return;
            }

            _glueSourceMotion =
                null;

            _hasExplicitGlueSourceMotion =
                false;

            ResetOrbitStartPoint();
        }

        private void InvalidateLocalStateCache()
        {
            _hasCachedLocalState =
                false;

            _cachedLocalStateIsBound =
                false;

            _nextLocalStateRefreshTime =
                0f;
        }

        private VfxState ResolveState()
        {
            if (_hasResolvedState)
                return _resolvedState;

            float now =
                Time.unscaledTime;

            if (_hasCachedLocalState &&
                now < _nextLocalStateRefreshTime)
            {
                return _cachedLocalState;
            }

            // Bound state is fixed until we're explicitly reconfigured.
            // Keep checking the binding marker so an unbind cannot leave
            // an old snapshot active.
            if (_hasCachedLocalState &&
                _cachedLocalStateIsBound &&
                _itemData != null &&
                VfxStateIO.IsBound(_itemData))
            {
                _nextLocalStateRefreshTime =
                    now +
                    LocalStateRefreshInterval;

                return _cachedLocalState;
            }

            _cachedLocalState =
                ResolveLocalState();

            _hasCachedLocalState =
                true;

            _nextLocalStateRefreshTime =
                now +
                LocalStateRefreshInterval;

            return _cachedLocalState;
        }

        private VfxState ResolveLocalState()
        {
            _cachedLocalStateIsBound =
                false;

            if (_itemData != null &&
                VfxStateIO.IsBound(_itemData))
            {
                if (VfxStateIO.TryRead(
                        _itemData,
                        out VfxState itemState))
                {
                    _cachedLocalStateIsBound =
                        true;

                    return itemState;
                }
            }

            return VfxStateIO.FromConfig();
        }

        internal void SetExternalVisualChain(
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

            _configuredVisualChain =
                headVisualTransform != null &&
                followerPoolRootTransform != null;

            _visualChainDirty =
                true;
        }

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

            if (_orbitalsFamily ==
                    NadaOrbitalsFamily.Orbs &&
                _hasOrbsBlockState)
            {
                if (!_orbsBlockState.Enabled)
                {
                    DisableAllVisualsAndResetState();
                    return;
                }

                ApplyOrbitalsOrbsBlockMotion(
                    _orbsBlockState);

                return;
            }

            VfxState state =
                ResolveState();

            if (!ResolveFamilyEnabled(
                    state))
            {
                DisableAllVisualsAndResetState();
                return;
            }

            ApplyOrbitalsMotion(
                state);
        }

        private void ApplyOrbitalsOrbsBlockMotion(
            OrbitalsOrbsMotionRuntimeState state)
        {
            const bool glueEnabled =
                false;

            bool snakeSpacingEnabled =
                state.SnakeEnabled;

            UpdateGlueState(
                glueEnabled);

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
                        snakeSpacingEnabled));

            float orbitAdherence =
                ResolveOrbitAdherenceFromDrift(
                    state.Drift);

            // Orbs do not have their own visual spin behavior.
            _currentVisualSpinRotation =
                Quaternion.identity;

            ApplyHeadVisualEnabledState(
                true);

            ApplyFollowerVisualCount(
                desiredFollowerCount);

            AdvanceCycleProgress(
                ClampOrbitalsSpeed(
                    state.Speed));

            float currentDistanceAlongCycle =
                _cachedCycleLength > 0f
                    ? _currentCycleProgress01 *
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
                snakeSpacingEnabled);
        }

        private void ApplyOrbitalsMotion(
            VfxState state)
        {
            bool glueEnabled =
                ResolveGlueEnabled(
                    state);

            bool snakeSpacingEnabled =
                ResolveSnakeSpacingEnabled(
                    state,
                    glueEnabled);

            UpdateGlueState(
                glueEnabled);

            int desiredFollowerCount =
                ResolveDesiredFollowerCount(
                    state);

            float radiusMultiplier =
                ResolveRadiusMultiplier(
                    state);

            float orbitLengthMultiplier =
                ResolveOrbitLengthMultiplier(
                    state);

            float turnsPerOneWayPass =
                ResolveTurnsPerOneWayPass(
                    state);

            _currentLocalOffset =
                ResolveLocalOffset(
                    state);

            _currentLocalRotationOffset =
                ResolveLocalRotationOffset(
                    state);

            EnsureArcLengthCache(
                radiusMultiplier,
                orbitLengthMultiplier,
                turnsPerOneWayPass);

            float historyStepPerFollower =
                SmoothHistoryStepPerFollower(
                    ResolveHistoryStepPerFollowerFloat(
                        state,
                        snakeSpacingEnabled));

            float orbitAdherence =
                Mathf.Clamp01(
                    ResolveOrbitAdherence(
                        state));

            AdvanceVisualSpin(
                state);

            ApplyHeadVisualEnabledState(
                true);

            ApplyFollowerVisualCount(
                desiredFollowerCount);

            AdvanceCycleProgress(
                ResolveCycleProgressPerSecond(
                    state));

            if (glueEnabled)
            {
                NadaOrbitalsMotion glueSourceMotion =
                    ResolveGlueSourceMotion();

                if (glueSourceMotion != null &&
                    glueSourceMotion
                        ._hasCurrentCycleProgress01)
                {
                    _currentCycleProgress01 =
                        glueSourceMotion
                            ._currentCycleProgress01;

                    _hasCurrentCycleProgress01 =
                        true;
                }
            }

            float currentDistanceAlongCycle =
                _cachedCycleLength > 0f
                    ? _currentCycleProgress01 *
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
                snakeSpacingEnabled);
        }

        private void UpdateGlueState(
            bool glueEnabled)
        {
            if (_hasLastGlueEnabled &&
                _lastGlueEnabled ==
                    glueEnabled)
            {
                return;
            }

            ResetOrbitStartPoint();

            _lastGlueEnabled =
                glueEnabled;

            _hasLastGlueEnabled =
                true;
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

        private void AdvanceVisualSpin(
            VfxState state)
        {
            if (_orbitalsFamily !=
                NadaOrbitalsFamily.Cores)
            {
                _currentVisualSpinRotation =
                    Quaternion.identity;

                return;
            }

            if (!state.OrbitalsCoresSpinEnabled)
            {
                _currentVisualSpinRotation =
                    Quaternion.identity;

                return;
            }

            float spinSpeed =
                state.OrbitalsCoresSpinSpeed;

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

        private void ResolveVisualChain()
        {
            _followerVisualTransforms.Clear();

            _initialized =
                false;

            if (!_configuredVisualChain ||
                _headVisualTransform == null ||
                _followerPoolRootTransform == null)
            {
                return;
            }

            foreach (Transform childTransform in
                     _followerPoolRootTransform)
            {
                if (childTransform == null)
                    continue;

                // The head owns its own slot. Followers should never
                // quietly include it.
                if (childTransform ==
                        _headVisualTransform ||
                    _headVisualTransform.IsChildOf(
                        childTransform) ||
                    childTransform.IsChildOf(
                        _headVisualTransform) ||
                    childTransform.name ==
                        _headVisualTransform.name)
                {
                    continue;
                }

                _followerVisualTransforms.Add(
                    childTransform);
            }

            _initialized =
                true;
        }

        private void ApplyHeadVisualEnabledState(
            bool enabled)
        {
            if (_headVisualTransform == null)
                return;

            if (_headVisualTransform.gameObject.activeSelf !=
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

        private bool ResolveFamilyEnabled(
            VfxState state)
        {
            return _orbitalsFamily switch
            {
                NadaOrbitalsFamily.Orbs =>
                    state.OrbitalsOrbsEnabled,

                NadaOrbitalsFamily.Cores =>
                    state.OrbitalsCoresEnabled,

                NadaOrbitalsFamily.Flames =>
                    state.OrbitalsFlamesEnabled,

                NadaOrbitalsFamily.Embers =>
                    state.OrbitalsEmbersEnabled,

                _ => false
            };
        }

        private bool ResolveGlueEnabled(
            VfxState state)
        {
            if (_orbitalsFamily ==
                NadaOrbitalsFamily.Orbs)
            {
                return false;
            }

            if (!state.OrbitalsOrbsEnabled)
                return false;

            return _orbitalsFamily switch
            {
                NadaOrbitalsFamily.Cores =>
                    state.OrbitalsCoresGlueEnabled,

                NadaOrbitalsFamily.Flames =>
                    state.OrbitalsOrbsGlueEnabled,

                NadaOrbitalsFamily.Embers =>
                    state.OrbitalsOrbsGlueEnabled,

                _ => false
            };
        }

        private bool ResolveSnakeSpacingEnabled(
            VfxState state,
            bool glueEnabled)
        {
            if (glueEnabled)
                return state.OrbitalsOrbsSnakeEnabled;

            return _orbitalsFamily switch
            {
                NadaOrbitalsFamily.Orbs =>
                    state.OrbitalsOrbsSnakeEnabled,

                NadaOrbitalsFamily.Cores =>
                    state.OrbitalsCoresSnakeEnabled,

                _ => false
            };
        }

        private int ResolveDesiredFollowerCount(
            VfxState state)
        {
            float normalizedCount =
                _orbitalsFamily switch
                {
                    NadaOrbitalsFamily.Orbs =>
                        state.OrbitalsOrbsCount,

                    NadaOrbitalsFamily.Cores =>
                        state.OrbitalsCoresCount,

                    NadaOrbitalsFamily.Flames =>
                        state.OrbitalsFlamesCount,

                    NadaOrbitalsFamily.Embers =>
                        state.OrbitalsEmbersCount,

                    _ => 0f
                };

            return ResolveDesiredFollowerCount(
                normalizedCount);
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

        private float ResolveHistoryStepPerFollowerFloat(
            VfxState state,
            bool snakeSpacingEnabled)
        {
            if (snakeSpacingEnabled)
                return MinHistoryStepPerFollower;

            bool glueEnabled =
                ResolveGlueEnabled(
                    state);

            float spacing =
                _orbitalsFamily switch
                {
                    NadaOrbitalsFamily.Orbs =>
                        state.OrbitalsOrbsSpacing,

                    NadaOrbitalsFamily.Cores =>
                        glueEnabled
                            ? state.OrbitalsOrbsSpacing
                            : state.OrbitalsCoresSpacing,

                    NadaOrbitalsFamily.Flames =>
                        glueEnabled
                            ? state.OrbitalsOrbsSpacing
                            : state.OrbitalsFlamesSpacing,

                    NadaOrbitalsFamily.Embers =>
                        glueEnabled
                            ? state.OrbitalsOrbsSpacing
                            : state.OrbitalsEmbersSpacing,

                    _ => 0f
                };

            return ResolveHistoryStepPerFollowerFloat(
                spacing,
                false);
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

        private float ResolveRadiusMultiplier(
            VfxState state)
        {
            bool glueEnabled =
                ResolveGlueEnabled(
                    state);

            float value =
                _orbitalsFamily switch
                {
                    NadaOrbitalsFamily.Orbs =>
                        state.OrbitalsOrbsRadius,

                    NadaOrbitalsFamily.Cores =>
                        glueEnabled
                            ? state.OrbitalsOrbsRadius
                            : state.OrbitalsCoresRadius,

                    NadaOrbitalsFamily.Flames =>
                        glueEnabled
                            ? state.OrbitalsOrbsRadius
                            : state.OrbitalsFlamesRadius,

                    NadaOrbitalsFamily.Embers =>
                        glueEnabled
                            ? state.OrbitalsOrbsRadius
                            : state.OrbitalsEmbersRadius,

                    _ =>
                        PluginConfig
                            .DefaultOrbitalsRadiusMultiplier
                };

            return ClampOrbitalsRadius(
                value);
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

        private float ResolveOrbitLengthMultiplier(
            VfxState state)
        {
            bool glueEnabled =
                ResolveGlueEnabled(
                    state);

            float value =
                _orbitalsFamily switch
                {
                    NadaOrbitalsFamily.Orbs =>
                        state.OrbitalsOrbsLength,

                    NadaOrbitalsFamily.Cores =>
                        glueEnabled
                            ? state.OrbitalsOrbsLength
                            : state.OrbitalsCoresLength,

                    NadaOrbitalsFamily.Flames =>
                        glueEnabled
                            ? state.OrbitalsOrbsLength
                            : state.OrbitalsFlamesLength,

                    NadaOrbitalsFamily.Embers =>
                        glueEnabled
                            ? state.OrbitalsOrbsLength
                            : state.OrbitalsEmbersLength,

                    _ =>
                        NadaOrbitalsPath
                            .DefaultOrbitLengthMultiplier
                };

            return ClampOrbitalsLength(
                value);
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

        private float ResolveTurnsPerOneWayPass(
            VfxState state)
        {
            bool glueEnabled =
                ResolveGlueEnabled(
                    state);

            float value =
                _orbitalsFamily switch
                {
                    NadaOrbitalsFamily.Orbs =>
                        state.OrbitalsOrbsCycles,

                    NadaOrbitalsFamily.Cores =>
                        glueEnabled
                            ? state.OrbitalsOrbsCycles
                            : state.OrbitalsCoresCycles,

                    NadaOrbitalsFamily.Flames =>
                        glueEnabled
                            ? state.OrbitalsOrbsCycles
                            : state.OrbitalsFlamesCycles,

                    NadaOrbitalsFamily.Embers =>
                        glueEnabled
                            ? state.OrbitalsOrbsCycles
                            : state.OrbitalsEmbersCycles,

                    _ =>
                        NadaOrbitalsPath
                            .DefaultTurnsPerOneWayPass
                };

            return ClampOrbitalsCycles(
                value);
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

        private float ResolveCycleProgressPerSecond(
            VfxState state)
        {
            bool glueEnabled =
                ResolveGlueEnabled(
                    state);

            float value =
                _orbitalsFamily switch
                {
                    NadaOrbitalsFamily.Orbs =>
                        state.OrbitalsOrbsSpeed,

                    NadaOrbitalsFamily.Cores =>
                        glueEnabled
                            ? state.OrbitalsOrbsSpeed
                            : state.OrbitalsCoresSpeed,

                    NadaOrbitalsFamily.Flames =>
                        glueEnabled
                            ? state.OrbitalsOrbsSpeed
                            : state.OrbitalsFlamesSpeed,

                    NadaOrbitalsFamily.Embers =>
                        glueEnabled
                            ? state.OrbitalsOrbsSpeed
                            : state.OrbitalsEmbersSpeed,

                    _ =>
                        DefaultCycleProgressPerSecond
                };

            return ClampOrbitalsSpeed(
                value);
        }

        private static float ClampOrbitalsSpeed(
            float value)
        {
            if (float.IsNaN(value) ||
                float.IsInfinity(value))
            {
                return DefaultCycleProgressPerSecond;
            }

            return Mathf.Clamp(
                value,
                PluginConfig.MinOrbitalsSpeed,
                PluginConfig.MaxOrbitalsSpeed);
        }

        private float ResolveOrbitAdherence(
            VfxState state)
        {
            bool glueEnabled =
                ResolveGlueEnabled(
                    state);

            float drift =
                _orbitalsFamily switch
                {
                    NadaOrbitalsFamily.Orbs =>
                        state.OrbitalsOrbsDrift,

                    NadaOrbitalsFamily.Cores =>
                        glueEnabled
                            ? state.OrbitalsOrbsDrift
                            : state.OrbitalsCoresDrift,

                    NadaOrbitalsFamily.Flames =>
                        glueEnabled
                            ? state.OrbitalsOrbsDrift
                            : state.OrbitalsFlamesDrift,

                    NadaOrbitalsFamily.Embers =>
                        glueEnabled
                            ? state.OrbitalsOrbsDrift
                            : state.OrbitalsEmbersDrift,

                    _ =>
                        PluginConfig.DefaultDrift
                };

            return ResolveOrbitAdherenceFromDrift(
                drift);
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

        internal void ResetOrbitStartPoint()
        {
            _currentCycleProgress01 =
                0f;

            _hasCurrentCycleProgress01 =
                false;

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

            _parentWorldHistoryStartIndex =
                0;

            _parentWorldHistoryCount =
                0;
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

            if (sampleIndex <= 0f)
            {
                return GetParentWorldHistorySample(
                        0)
                    .Position;
            }

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

            if (lowerIndex ==
                upperIndex)
            {
                return GetParentWorldHistorySample(
                        lowerIndex)
                    .Position;
            }

            float interpolationT =
                sampleIndex -
                lowerIndex;

            Vector3 lowerPosition =
                GetParentWorldHistorySample(
                        lowerIndex)
                    .Position;

            Vector3 upperPosition =
                GetParentWorldHistorySample(
                        upperIndex)
                    .Position;

            return Vector3.Lerp(
                lowerPosition,
                upperPosition,
                interpolationT);
        }

        private Quaternion SampleParentWorldRotation(
            float sampleIndex,
            Quaternion fallbackRotation)
        {
            if (_parentWorldHistoryCount == 0)
                return fallbackRotation;

            if (sampleIndex <= 0f)
            {
                return GetParentWorldHistorySample(
                        0)
                    .Rotation;
            }

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

            if (lowerIndex ==
                upperIndex)
            {
                return GetParentWorldHistorySample(
                        lowerIndex)
                    .Rotation;
            }

            float interpolationT =
                sampleIndex -
                lowerIndex;

            Quaternion lowerRotation =
                GetParentWorldHistorySample(
                        lowerIndex)
                    .Rotation;

            Quaternion upperRotation =
                GetParentWorldHistorySample(
                        upperIndex)
                    .Rotation;

            return Quaternion.Slerp(
                lowerRotation,
                upperRotation,
                interpolationT);
        }

        private NadaOrbitalsMotion ResolveGlueSourceMotion()
        {
            // Explicit binding wins. If the explicitly bound source has been
            // destroyed, fail closed rather than silently pairing this motion
            // with some other Orbs instance in the hierarchy.
            if (_hasExplicitGlueSourceMotion)
                return _glueSourceMotion;

            // Transitional legacy behavior. While all four families still
            // occupy one shared Orbitals hierarchy, retain the existing
            // discovery path until structural migration removes it.
            if (_glueSourceMotion != null)
                return _glueSourceMotion;

            Transform root =
                transform.parent;

            if (root == null)
                return null;

            foreach (NadaOrbitalsMotion motion in
                     root.GetComponentsInChildren<NadaOrbitalsMotion>(
                         true))
            {
                if (motion != null &&
                    motion != this &&
                    motion._orbitalsFamily ==
                        NadaOrbitalsFamily.Orbs)
                {
                    _glueSourceMotion =
                        motion;

                    return _glueSourceMotion;
                }
            }

            return null;
        }

        private Vector3 ResolveLocalOffset(
            VfxState state)
        {
            bool glueEnabled =
                ResolveGlueEnabled(
                    state);

            float x =
                _orbitalsFamily switch
                {
                    NadaOrbitalsFamily.Orbs =>
                        state.OrbitalsOrbsXOffset,

                    NadaOrbitalsFamily.Cores =>
                        glueEnabled
                            ? state.OrbitalsOrbsXOffset
                            : state.OrbitalsCoresXOffset,

                    NadaOrbitalsFamily.Flames =>
                        glueEnabled
                            ? state.OrbitalsOrbsXOffset
                            : state.OrbitalsFlamesXOffset,

                    NadaOrbitalsFamily.Embers =>
                        glueEnabled
                            ? state.OrbitalsOrbsXOffset
                            : state.OrbitalsEmbersXOffset,

                    _ =>
                        PluginConfig.DefaultEffectOffset
                };

            float y =
                _orbitalsFamily switch
                {
                    NadaOrbitalsFamily.Orbs =>
                        state.OrbitalsOrbsYOffset,

                    NadaOrbitalsFamily.Cores =>
                        glueEnabled
                            ? state.OrbitalsOrbsYOffset
                            : state.OrbitalsCoresYOffset,

                    NadaOrbitalsFamily.Flames =>
                        glueEnabled
                            ? state.OrbitalsOrbsYOffset
                            : state.OrbitalsFlamesYOffset,

                    NadaOrbitalsFamily.Embers =>
                        glueEnabled
                            ? state.OrbitalsOrbsYOffset
                            : state.OrbitalsEmbersYOffset,

                    _ =>
                        PluginConfig.DefaultEffectOffset
                };

            float z =
                _orbitalsFamily switch
                {
                    NadaOrbitalsFamily.Orbs =>
                        state.OrbitalsOrbsZOffset,

                    NadaOrbitalsFamily.Cores =>
                        glueEnabled
                            ? state.OrbitalsOrbsZOffset
                            : state.OrbitalsCoresZOffset,

                    NadaOrbitalsFamily.Flames =>
                        glueEnabled
                            ? state.OrbitalsOrbsZOffset
                            : state.OrbitalsFlamesZOffset,

                    NadaOrbitalsFamily.Embers =>
                        glueEnabled
                            ? state.OrbitalsOrbsZOffset
                            : state.OrbitalsEmbersZOffset,

                    _ =>
                        PluginConfig.DefaultEffectOffset
                };

            return new Vector3(
                ClampOffset(
                    x),
                ClampOffset(
                    y),
                ClampOffset(
                    z));
        }

        private Quaternion ResolveLocalRotationOffset(
            VfxState state)
        {
            bool glueEnabled =
                ResolveGlueEnabled(
                    state);

            float x =
                _orbitalsFamily switch
                {
                    NadaOrbitalsFamily.Orbs =>
                        state.OrbitalsOrbsXRotation,

                    NadaOrbitalsFamily.Cores =>
                        glueEnabled
                            ? state.OrbitalsOrbsXRotation
                            : state.OrbitalsCoresXRotation,

                    NadaOrbitalsFamily.Flames =>
                        glueEnabled
                            ? state.OrbitalsOrbsXRotation
                            : state.OrbitalsFlamesXRotation,

                    NadaOrbitalsFamily.Embers =>
                        glueEnabled
                            ? state.OrbitalsOrbsXRotation
                            : state.OrbitalsEmbersXRotation,

                    _ =>
                        PluginConfig.DefaultEffectRotation
                };

            float y =
                _orbitalsFamily switch
                {
                    NadaOrbitalsFamily.Orbs =>
                        state.OrbitalsOrbsYRotation,

                    NadaOrbitalsFamily.Cores =>
                        glueEnabled
                            ? state.OrbitalsOrbsYRotation
                            : state.OrbitalsCoresYRotation,

                    NadaOrbitalsFamily.Flames =>
                        glueEnabled
                            ? state.OrbitalsOrbsYRotation
                            : state.OrbitalsFlamesYRotation,

                    NadaOrbitalsFamily.Embers =>
                        glueEnabled
                            ? state.OrbitalsOrbsYRotation
                            : state.OrbitalsEmbersYRotation,

                    _ =>
                        PluginConfig.DefaultEffectRotation
                };

            float z =
                _orbitalsFamily switch
                {
                    NadaOrbitalsFamily.Orbs =>
                        state.OrbitalsOrbsZRotation,

                    NadaOrbitalsFamily.Cores =>
                        glueEnabled
                            ? state.OrbitalsOrbsZRotation
                            : state.OrbitalsCoresZRotation,

                    NadaOrbitalsFamily.Flames =>
                        glueEnabled
                            ? state.OrbitalsOrbsZRotation
                            : state.OrbitalsFlamesZRotation,

                    NadaOrbitalsFamily.Embers =>
                        glueEnabled
                            ? state.OrbitalsOrbsZRotation
                            : state.OrbitalsEmbersZRotation,

                    _ =>
                        PluginConfig.DefaultEffectRotation
                };

            return Quaternion.Euler(
                ClampRotation(
                    x),
                ClampRotation(
                    y),
                ClampRotation(
                    z));
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