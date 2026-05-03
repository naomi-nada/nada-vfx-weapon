using System;

namespace NADA.VFX.Core.State
{
    [Serializable]
    internal struct VfxState
    {
        // Rig Position
        public float RigRotation;
        public float RigSideRotation;
        public float RigLengthPosition;
        public float RigSidePosition;
        
        // Inner Flames
        public bool InnerFlamesEnabled;
        public float InnerFlamesEnergy;
        public float InnerFlamesScale;
        public float InnerFlamesLength;
        public float InnerFlamesHue;
        public float InnerFlamesPosition;

        // Outer Flames
        public bool OuterFlamesEnabled;
        public bool OuterFlamesDragEnabled;
        public float OuterFlamesEnergy;
        public float OuterFlamesScale;
        public float OuterFlamesLength;
        public float OuterFlamesHue;
        public float OuterFlamesPosition;

        // Flare
        public bool FlareEnabled;
        public float FlareScale;
        public float FlareHue;

        // Orbitals - Orbs
        public bool OrbitalsOrbsEnabled;
        public float OrbitalsOrbsCount;
        public float OrbitalsOrbsDrift;
        public float OrbitalsOrbsScale;
        public float OrbitalsOrbsHue;
        public float OrbitalsOrbsSpeed;
        public float OrbitalsOrbsSpacing;
        public float OrbitalsOrbsLength;
        public float OrbitalsOrbsRadius;
        public float OrbitalsOrbsCycles;

        // Orbitals - Flames
        public bool OrbitalsFlamesEnabled;
        public float OrbitalsFlamesCount;
        public float OrbitalsFlamesEnergy;
        public float OrbitalsFlamesDrift;
        public float OrbitalsFlamesHue;
        public float OrbitalsFlamesSpeed;
        public float OrbitalsFlamesSpacing;
        public float OrbitalsFlamesLength;
        public float OrbitalsFlamesRadius;
        public float OrbitalsFlamesCycles;

        // Orbitals - Embers
        public bool OrbitalsEmbersEnabled;
        public float OrbitalsEmbersCount;
        public float OrbitalsEmbersEnergy;
        public float OrbitalsEmbersDrift;
        public float OrbitalsEmbersHue;
        public float OrbitalsEmbersSpeed;
        public float OrbitalsEmbersSpacing;
        public float OrbitalsEmbersLength;
        public float OrbitalsEmbersRadius;
        public float OrbitalsEmbersCycles;
    }
}