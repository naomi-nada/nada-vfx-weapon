namespace NADA.VFX.Core.State
{
    internal struct VfxState
    {
        // Inner Flames
        public bool InnerFlamesEnabled;
        public float InnerFlamesScale;
        public float InnerFlamesHue;
        public float InnerFlamesEnergy;
        public float InnerFlamesChaos;

        // Outer Flames
        public bool OuterFlamesEnabled;
        public float OuterFlamesScale;
        public float OuterFlamesHue;
        public float OuterFlamesEnergy;
        public float OuterFlamesChaos;

        // Flare
        public bool FlareEnabled;
        public float FlareScale;
        public float FlareHue;

        // Sparks
        public bool SparksEnabled;
        public float SparksHue;
        public float SparksEnergy;

        // Mirage
        public bool MirageEnabled;
        public float MirageScale;
        public float MirageHue;

        // Orbitals - Orbs
        public bool OrbitalsOrbsEnabled;
        public float OrbitalsOrbsCount;
        public float OrbitalsOrbsScale;
        public float OrbitalsOrbsHue;
        public float OrbitalsOrbsSpacing;
        public float OrbitalsOrbsRadius;

        // Orbitals - Flames
        public bool OrbitalsFlamesEnabled;
        public float OrbitalsFlamesCount;
        public float OrbitalsFlamesHue;
        public float OrbitalsFlamesEnergy;
        public float OrbitalsFlamesSpacing;
        public float OrbitalsFlamesRadius;

        // Orbitals - Embers
        public bool OrbitalsEmbersEnabled;
        public float OrbitalsEmbersCount;
        public float OrbitalsEmbersHue;
        public float OrbitalsEmbersEnergy;
        public float OrbitalsEmbersSpacing;
        public float OrbitalsEmbersRadius;
    }
}