using System;

namespace NADA.VFX.Core.State
{
    [Serializable]
    internal struct VfxState
    {
        // Rig Position
        public float RigXOffset;
        public float RigYOffset;
        public float RigZOffset;
        public float RigXRotation;
        public float RigYRotation;
        public float RigZRotation;

        // Inner Flames
        public bool InnerFlamesEnabled;
        public float InnerFlamesEnergy;
        public float InnerFlamesScale;
        public float InnerFlamesLength;
        public float InnerFlamesHue;
        public float InnerFlamesXOffset;
        public float InnerFlamesYOffset;
        public float InnerFlamesZOffset;
        public float InnerFlamesXRotation;
        public float InnerFlamesYRotation;
        public float InnerFlamesZRotation;

        // Outer Flames
        public bool OuterFlamesEnabled;
        public bool OuterFlamesDragEnabled;
        public float OuterFlamesEnergy;
        public float OuterFlamesScale;
        public float OuterFlamesLength;
        public float OuterFlamesHue;
        public float OuterFlamesXOffset;
        public float OuterFlamesYOffset;
        public float OuterFlamesZOffset;
        public float OuterFlamesXRotation;
        public float OuterFlamesYRotation;
        public float OuterFlamesZRotation;

        // Sparks
        public bool SparksEnabled;
        public float SparksEnergy;
        public float SparksScale;
        public float SparksLength;
        public float SparksWidth;
        public float SparksHue;
        public float SparksXOffset;
        public float SparksYOffset;
        public float SparksZOffset;
        public float SparksXRotation;
        public float SparksYRotation;
        public float SparksZRotation;

        // Flare
        public bool FlareEnabled;
        public float FlareScale;
        public float FlareHue;
        public float FlareXOffset;
        public float FlareYOffset;
        public float FlareZOffset;

        // Aura
        public bool AuraEnabled;
        public float AuraScale;
        public float AuraHue;
        public float AuraXOffset;
        public float AuraYOffset;
        public float AuraZOffset;
        public float AuraXRotation;
        public float AuraYRotation;
        public float AuraZRotation;

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
        public float OrbitalsOrbsXOffset;
        public float OrbitalsOrbsYOffset;
        public float OrbitalsOrbsZOffset;
        public float OrbitalsOrbsXRotation;
        public float OrbitalsOrbsYRotation;
        public float OrbitalsOrbsZRotation;

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
        public float OrbitalsStrandsLifetime;
        public float OrbitalsStrandsXOffset;
        public float OrbitalsStrandsYOffset;
        public float OrbitalsStrandsZOffset;
        public float OrbitalsStrandsXRotation;
        public float OrbitalsStrandsYRotation;
        public float OrbitalsStrandsZRotation;

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
        public float OrbitalsFlamesXOffset;
        public float OrbitalsFlamesYOffset;
        public float OrbitalsFlamesZOffset;
        public float OrbitalsFlamesXRotation;
        public float OrbitalsFlamesYRotation;
        public float OrbitalsFlamesZRotation;

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
        public float OrbitalsEmbersXOffset;
        public float OrbitalsEmbersYOffset;
        public float OrbitalsEmbersZOffset;
        public float OrbitalsEmbersXRotation;
        public float OrbitalsEmbersYRotation;
        public float OrbitalsEmbersZRotation;
    }
}