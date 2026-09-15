using System.Collections.Generic;
using NADA.VFX.Weapon.Core.State;
using NADA.VFX.Weapon.Runtime.Binding;
using UnityEngine;

namespace NADA.VFX.Weapon.Modules.Motion
{
    internal sealed class NadaOuterFlamesMotion :
        MonoBehaviour,
        INadaResolvedStateReceiver
    {
        private const float DefaultDragStrength = 0.015f;
        private const float MaxTrackedVelocity = 12f;
        private const float VelocitySmoothing = 12f;

        private global::ItemDrop.ItemData _itemData;
        private VfxState _resolvedState;
        private bool _hasResolvedState;

        private readonly List<CachedParticle> _particles = new();

        private Vector3 _lastWorldPosition;
        private bool _hasLastWorldPosition;
        private Vector3 _smoothedVelocity;
        private bool _initialized;

        private ParticleSystem.MinMaxCurve _xCurve = new(0f);
        private ParticleSystem.MinMaxCurve _yCurve = new(0f);
        private ParticleSystem.MinMaxCurve _zCurve = new(0f);

        private sealed class CachedParticle
        {
            public ParticleSystem System;
            public ParticleSystem.VelocityOverLifetimeModule Velocity;
        }

        internal void SetItemData(global::ItemDrop.ItemData itemData)
        {
            if (_itemData == itemData &&
                !_hasResolvedState)
            {
                return;
            }

            _itemData = itemData;
            _hasResolvedState = false;

            ResetVelocityTracking();
        }

        public void SetResolvedState(VfxState state)
        {
            _resolvedState = state;
            _hasResolvedState = true;
            _itemData = null;

            ResetVelocityTracking();
        }

        private void Awake()
        {
            RebuildParticleSystems();
            ResetVelocityTracking();
        }

        private void LateUpdate()
        {
            if (!_initialized)
                RebuildParticleSystems();

            if (_particles.Count == 0)
                return;

            Vector3 weaponVelocity =
                CalculateSmoothedWeaponVelocity();

            VfxState state =
                ResolveState();

            if (!state.OuterFlamesDragEnabled)
            {
                ApplyDragVelocity(
                    Vector3.zero,
                    0f);

                return;
            }

            ApplyDragVelocity(
                weaponVelocity,
                ResolveDragStrength());
        }

        private Vector3 CalculateSmoothedWeaponVelocity()
        {
            Vector3 currentWorldPosition =
                transform.position;

            Vector3 weaponVelocity =
                Vector3.zero;

            if (_hasLastWorldPosition)
            {
                float deltaTime =
                    Mathf.Max(
                        Time.deltaTime,
                        0.0001f);

                weaponVelocity =
                    (currentWorldPosition -
                     _lastWorldPosition) /
                    deltaTime;

                weaponVelocity =
                    Vector3.ClampMagnitude(
                        weaponVelocity,
                        MaxTrackedVelocity);
            }

            _lastWorldPosition =
                currentWorldPosition;

            _hasLastWorldPosition =
                true;

            _smoothedVelocity =
                Vector3.Lerp(
                    _smoothedVelocity,
                    weaponVelocity,
                    Time.deltaTime *
                    VelocitySmoothing);

            return _smoothedVelocity;
        }

        private void RebuildParticleSystems()
        {
            _particles.Clear();

            foreach (ParticleSystem particleSystem in
                     GetComponentsInChildren<ParticleSystem>(true))
            {
                if (particleSystem == null)
                    continue;

                try
                {
                    var velocityOverLifetime =
                        particleSystem.velocityOverLifetime;

                    velocityOverLifetime.enabled =
                        true;

                    velocityOverLifetime.space =
                        ParticleSystemSimulationSpace.World;

                    _particles.Add(
                        new CachedParticle
                        {
                            System = particleSystem,
                            Velocity = velocityOverLifetime
                        });
                }
                catch
                {
                }
            }

            _initialized =
                _particles.Count > 0;
        }

        private VfxState ResolveState()
        {
            if (_hasResolvedState)
                return _resolvedState;

            if (_itemData != null &&
                VfxStateIO.TryRead(
                    _itemData,
                    out VfxState itemState))
            {
                return itemState;
            }

            return VfxStateIO.FromConfig();
        }

        private void ResetVelocityTracking()
        {
            _lastWorldPosition =
                transform.position;

            _hasLastWorldPosition =
                true;

            _smoothedVelocity =
                Vector3.zero;
        }

        private float ResolveDragStrength()
        {
            float tunedValue =
                0.35f;

            if (float.IsNaN(tunedValue) ||
                float.IsInfinity(tunedValue))
            {
                return DefaultDragStrength;
            }

            return Mathf.Max(
                0f,
                tunedValue);
        }

        private void ApplyDragVelocity(
            Vector3 weaponVelocity,
            float dragStrength)
        {
            Vector3 dragVelocity =
                -weaponVelocity *
                dragStrength;

            _xCurve.constant =
                dragVelocity.x;

            _yCurve.constant =
                dragVelocity.y;

            _zCurve.constant =
                dragVelocity.z;

            foreach (CachedParticle particle in _particles)
            {
                if (particle == null ||
                    particle.System == null)
                {
                    continue;
                }

                try
                {
                    particle.Velocity.x =
                        _xCurve;

                    particle.Velocity.y =
                        _yCurve;

                    particle.Velocity.z =
                        _zCurve;
                }
                catch
                {
                }
            }
        }
    }
}