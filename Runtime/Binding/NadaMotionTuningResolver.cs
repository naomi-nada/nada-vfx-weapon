using NADA.VFX.Core.State;
using UnityEngine;

namespace NADA.VFX.Runtime.Binding
{
    internal static class NadaMotionTuningResolver
    {
        // 0f = fully adhere to orbit path relative to moving host
        // 1f = free drift

        internal static float GetOrbsOrbitAdherence(global::ItemDrop.ItemData itemData)
        {
            VfxState state = ResolveState(itemData);
            return DriftToAdherence(state.OrbitalsOrbsDrift);
        }
        
        internal static float GetFlamesOrbitAdherence(global::ItemDrop.ItemData itemData)
        {
            VfxState state = ResolveState(itemData);
            return DriftToAdherence(state.OrbitalsFlamesDrift);
        }

        internal static float GetEmbersOrbitAdherence(global::ItemDrop.ItemData itemData)
        {
            VfxState state = ResolveState(itemData);
            return DriftToAdherence(state.OrbitalsEmbersDrift);
        }

        internal static bool GetOuterFlamesDragEnabled(global::ItemDrop.ItemData itemData)
        {
            VfxState state = ResolveState(itemData);
            return state.OuterFlamesDragEnabled;
        }

        internal static float GetOuterFlamesDrag(global::ItemDrop.ItemData itemData)
        {
            return 0.25f;
        }

        private static VfxState ResolveState(global::ItemDrop.ItemData itemData)
        {
            if (itemData != null)
            {
                if (!VfxStateIO.TryRead(itemData, out VfxState state))
                {
                    VfxStateIO.EnsureInitializedFromConfig(itemData);

                    if (!VfxStateIO.TryRead(itemData, out state))
                        state = VfxStateIO.FromConfig();
                }

                return state;
            }

            return VfxStateIO.FromConfig();
        }

        private static float DriftToAdherence(float drift)
        {
            if (float.IsNaN(drift) || float.IsInfinity(drift))
                drift = 0f;

            drift = Mathf.Clamp01(drift);
            return 1f - drift;
        }
    }
}