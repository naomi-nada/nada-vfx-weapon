using System.Collections.Generic;
using UnityEngine;
using NADA.VFX.Runtime.Binding;
using NADA.VFX.Runtime.Persistence;
using NADA.VFX.Core.State;

namespace NADA.VFX.Modules.Properties
{
    internal sealed class NadaEnergyModule : MonoBehaviour
    {
        private global::ItemDrop.ItemData _itemData;
        private RigGroups _groups;

        private sealed class EmissionBaseline
        {
            public bool Enabled;
            public ParticleSystem.MinMaxCurve RateOverTime;
            public ParticleSystem.MinMaxCurve RateOverDistance;
        }

        private readonly Dictionary<int, EmissionBaseline> _baseEmission = new();

        public void SetItemData(global::ItemDrop.ItemData itemData)
        {
            _itemData = itemData;
        }
        
        public void SetRigGroups(RigGroups groups)
        {
            if (groups != null)
                _groups = groups;
        }

        private void Awake()
        {
            InvokeRepeating(nameof(TickApply), 0f, 0.05f);
        }

        private void OnDestroy()
        {
            try { CancelInvoke(nameof(TickApply)); } catch { }
        }

        private void TickApply()
        {
            if (_groups == null)
                return;

            CacheAllBaselines();

            VfxState state = NadaWeaponStateResolver.Resolve(_itemData);
            
            ApplyOuterEnergy(_groups.OuterSystems, state.OuterFlamesEnergy);
        }

        private void CacheAllBaselines()
        {
            CacheEmissionBaselines(_groups.OuterSystems);
        }

        private void CacheEmissionBaselines(List<ParticleSystem> systems)
        {
            if (systems == null) return;

            foreach (var ps in systems)
            {
                if (ps == null) continue;

                int id = ps.GetInstanceID();
                if (_baseEmission.ContainsKey(id))
                    continue;

                try
                {
                    var emission = ps.emission;

                    _baseEmission[id] = new EmissionBaseline
                    {
                        Enabled = emission.enabled,
                        RateOverTime = emission.rateOverTime,
                        RateOverDistance = emission.rateOverDistance
                    };
                }
                catch { }
            }
        }
        
        private void ApplyOuterEnergy(List<ParticleSystem> systems, float energy)
        {
            if (systems == null) return;

            float t = Mathf.Clamp01(energy);

            foreach (var ps in systems)
            {
                if (ps == null) continue;

                int id = ps.GetInstanceID();
                if (!_baseEmission.TryGetValue(id, out var baseline) || baseline == null)
                    continue;

                try
                {
                    var emission = ps.emission;

                    emission.enabled = baseline.Enabled;
                    if (!baseline.Enabled)
                        continue;

                    float emissionMult = Mathf.Lerp(1f, 10f, t);
                    var outerBaseRateOverTime = OverrideConstantBaseline(baseline.RateOverTime, 10f);

                    emission.rateOverTime = ScaleMinMaxCurve(outerBaseRateOverTime, emissionMult);
                    emission.rateOverDistance = ScaleMinMaxCurve(baseline.RateOverDistance, emissionMult);
                }
                catch { }
            }
        }

        private static ParticleSystem.MinMaxCurve OverrideConstantBaseline(
            ParticleSystem.MinMaxCurve source,
            float constantValue)
        {
            switch (source.mode)
            {
                case ParticleSystemCurveMode.Constant:
                    return new ParticleSystem.MinMaxCurve(constantValue);

                case ParticleSystemCurveMode.TwoConstants:
                    return new ParticleSystem.MinMaxCurve(constantValue, constantValue);

                default:
                    return source;
            }
        }

        private static ParticleSystem.MinMaxCurve ScaleMinMaxCurve(
            ParticleSystem.MinMaxCurve source,
            float multiplier)
        {
            switch (source.mode)
            {
                case ParticleSystemCurveMode.Constant:
                    return new ParticleSystem.MinMaxCurve(source.constant * multiplier);

                case ParticleSystemCurveMode.TwoConstants:
                    return new ParticleSystem.MinMaxCurve(
                        source.constantMin * multiplier,
                        source.constantMax * multiplier
                    );

                case ParticleSystemCurveMode.Curve:
                    return new ParticleSystem.MinMaxCurve(
                        source.curveMultiplier * multiplier,
                        source.curve
                    );

                case ParticleSystemCurveMode.TwoCurves:
                    return new ParticleSystem.MinMaxCurve(
                        source.curveMultiplier * multiplier,
                        source.curveMin,
                        source.curveMax
                    );

                default:
                    return source;
            }
        }
    }
}