namespace NADA.VFX.Runtime.Binding
{
    internal static class NadaMotionTuningResolver
    {
        // 0f = preserve current drift behavior
        // 1f = fully locked to intended orbit path while host moves
        internal static float GetOrbOrbitAdherence(global::ItemDrop.ItemData itemData)
        {
            // Preserve exact current behavior for now.
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