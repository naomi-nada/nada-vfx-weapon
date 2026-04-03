namespace NADA.VFX.Runtime.Binding
{
    internal static class NadaMotionTuningResolver
    {
        // 0f = maintain current drift behavior
        // 1f = fully locked to orbit path while host moves
        internal static float GetOrbOrbitAdherence(global::ItemDrop.ItemData itemData)
        {
            return 0f;
        }

        internal static float GetFlamesOrbitAdherence(global::ItemDrop.ItemData itemData)
        {
            return 0f;
        }

        internal static float GetEmbersOrbitAdherence(global::ItemDrop.ItemData itemData)
        {
            return 0f;
        }
    }
}