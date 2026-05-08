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

        // Sparks
        public bool SparksEnabled;
        public float SparksEnergy;
        public float SparksScale;
        public float SparksLength;
        public float SparksWidth;
        public float SparksHue;
        public float SparksPosition;

        // Flare
        public bool FlareEnabled;
        public float FlareScale;
        public float FlareHue;
        public float FlarePosition;

        // Aura
        public bool AuraEnabled;
        public float AuraScale;
        public float AuraHue;

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

        // Orbitals - Strands
        public bool OrbitalsStrandsEnabled;
        public bool OrbitalsStrandsSpectrumEnabled;
        public float OrbitalsStrandsEnergy;
        public float OrbitalsStrandsDrift;
        public float OrbitalsStrandsScaleWhole;
        public float OrbitalsStrandsScaleParts;
        public float OrbitalsStrandsHue;
        public float OrbitalsStrandsSpectrumSpeed;
        public float OrbitalsStrandsSpeed;
        public float OrbitalsStrandsLength;
        public float OrbitalsStrandsRadius;
        public float OrbitalsStrandsPosition;
        public float OrbitalsStrandsLifetime;
        
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
        public float OrbitalsEmbersLifetime;
    }
}