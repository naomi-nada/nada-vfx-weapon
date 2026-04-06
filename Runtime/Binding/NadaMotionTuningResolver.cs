namespace NADA.VFX.Runtime.Binding
{
    internal static class NadaMotionTuningResolver
    {
        // 0f = fully adhere to orbit path relative to moving host (current behavior)
        // 1f = free drift

        internal static float GetOrbsOrbitAdherence(global::ItemDrop.ItemData itemData)
        {
            // TODO: resolve from VfxState / config when drift is implemented
            return 0f;
        }

        internal static float GetFlamesOrbitAdherence(global::ItemDrop.ItemData itemData)
        {
            // TODO: resolve from VfxState / config when drift is implemented
            return 0f;
        }

        internal static float GetEmbersOrbitAdherence(global::ItemDrop.ItemData itemData)
        {
            // TODO: resolve from VfxState / config when drift is implemented
            return 0f;
        }
    }
}