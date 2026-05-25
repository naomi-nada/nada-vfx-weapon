using System;

namespace NADA.VFX.Weapon.Core.State
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
        public bool InnerFlamesWorldEnabled;
        public bool InnerFlamesBlackEnabled;
        public bool InnerFlamesWhiteEnabled;
        public float InnerFlamesEnergy;
        public float InnerFlamesScale;
        public float InnerFlamesLuminance;
        public float InnerFlamesHue;
        public float InnerFlamesLifetime;
        public float InnerFlamesSimulationSpeed;
        public float InnerFlamesLength;
        public float InnerFlamesWidth;
        public float InnerFlamesXOffset;
        public float InnerFlamesYOffset;
        public float InnerFlamesZOffset;
        public float InnerFlamesXRotation;
        public float InnerFlamesYRotation;
        public float InnerFlamesZRotation;

        // Outer Flames
        public bool OuterFlamesEnabled;
        public bool OuterFlamesWorldEnabled;
        public bool OuterFlamesBlackEnabled;
        public bool OuterFlamesWhiteEnabled;
        public bool OuterFlamesDragEnabled;
        public float OuterFlamesEnergy;
        public float OuterFlamesScale;
        public float OuterFlamesLuminance;
        public float OuterFlamesHue;
        public float OuterFlamesLifetime;
        public float OuterFlamesSimulationSpeed;
        public float OuterFlamesLength;
        public float OuterFlamesWidth;
        public float OuterFlamesXOffset;
        public float OuterFlamesYOffset;
        public float OuterFlamesZOffset;
        public float OuterFlamesXRotation;
        public float OuterFlamesYRotation;
        public float OuterFlamesZRotation;

        // Strands
        public bool StrandsEnabled;
        public bool StrandsSpectrumEnabled;
        public float StrandsEnergy;
        public float StrandsScaleWhole;
        public float StrandsScaleParts;
        public float StrandsLuminance;
        public float StrandsHue;
        public float StrandsLifetime;
        public float StrandsLength;
        public float StrandsSpectrumSpeed;
        public float StrandsSpeed;
        public float StrandsRadius;
        public float StrandsXOffset;
        public float StrandsYOffset;
        public float StrandsZOffset;
        public float StrandsXRotation;
        public float StrandsYRotation;
        public float StrandsZRotation;
        public float StrandsDrift;

        // Sparks
        public bool SparksEnabled;
        public float SparksEnergy;
        public float SparksScale;
        public float SparksLuminance;
        public float SparksHue;
        public float SparksLifetime;
        public float SparksSimulationSpeed;
        public float SparksLength;
        public float SparksWidth;
        public float SparksXOffset;
        public float SparksYOffset;
        public float SparksZOffset;
        public float SparksXRotation;
        public float SparksYRotation;
        public float SparksZRotation;

        // Flare
        public bool FlareEnabled;
        public float FlareScale;
        public float FlareLuminance;
        public float FlareHue;
        public float FlareXOffset;
        public float FlareYOffset;
        public float FlareZOffset;

        // Aura
        public bool AuraEnabled;
        public float AuraScale;
        public float AuraLuminance;
        public float AuraHue;
        public float AuraXOffset;
        public float AuraYOffset;
        public float AuraZOffset;
        public float AuraXRotation;
        public float AuraYRotation;
        public float AuraZRotation;

        // Orbitals - Orbs
        public bool OrbitalsOrbsEnabled;
        public bool OrbitalsOrbsSnakeEnabled;
        public bool OrbitalsOrbsGlueEnabled;
        public float OrbitalsOrbsCount;
        public float OrbitalsOrbsScale;
        public float OrbitalsOrbsLuminance;
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
        public float OrbitalsOrbsDrift;

        // Orbitals - Cores
        public bool OrbitalsCoresEnabled;
        public bool OrbitalsCoresSnakeEnabled;
        public bool OrbitalsCoresGlueEnabled;
        public bool OrbitalsCoresSpinEnabled;
        public float OrbitalsCoresCount;
        public float OrbitalsCoresScale;
        public float OrbitalsCoresLuminance;
        public float OrbitalsCoresHue;
        public float OrbitalsCoresSpinSpeed;
        public float OrbitalsCoresLength;
        public float OrbitalsCoresSpeed;
        public float OrbitalsCoresSpacing;
        public float OrbitalsCoresRadius;
        public float OrbitalsCoresCycles;
        public float OrbitalsCoresXOffset;
        public float OrbitalsCoresYOffset;
        public float OrbitalsCoresZOffset;
        public float OrbitalsCoresXRotation;
        public float OrbitalsCoresYRotation;
        public float OrbitalsCoresZRotation;
        public float OrbitalsCoresDrift;

        // Orbitals - Flames
        public bool OrbitalsFlamesEnabled;
        public float OrbitalsFlamesCount;
        public float OrbitalsFlamesEnergy;
        public float OrbitalsFlamesScale;
        public float OrbitalsFlamesLuminance;
        public float OrbitalsFlamesHue;
        public float OrbitalsFlamesLifetime;
        public float OrbitalsFlamesSimulationSpeed;
        public float OrbitalsFlamesLength;
        public float OrbitalsFlamesSpeed;
        public float OrbitalsFlamesSpacing;
        public float OrbitalsFlamesRadius;
        public float OrbitalsFlamesCycles;
        public float OrbitalsFlamesXOffset;
        public float OrbitalsFlamesYOffset;
        public float OrbitalsFlamesZOffset;
        public float OrbitalsFlamesXRotation;
        public float OrbitalsFlamesYRotation;
        public float OrbitalsFlamesZRotation;
        public float OrbitalsFlamesDrift;

        // Orbitals - Embers
        public bool OrbitalsEmbersEnabled;
        public float OrbitalsEmbersCount;
        public float OrbitalsEmbersEnergy;
        public float OrbitalsEmbersScale;
        public float OrbitalsEmbersLuminance;
        public float OrbitalsEmbersHue;
        public float OrbitalsEmbersLifetime;
        public float OrbitalsEmbersSimulationSpeed;
        public float OrbitalsEmbersLength;
        public float OrbitalsEmbersSpeed;
        public float OrbitalsEmbersSpacing;
        public float OrbitalsEmbersRadius;
        public float OrbitalsEmbersCycles;
        public float OrbitalsEmbersXOffset;
        public float OrbitalsEmbersYOffset;
        public float OrbitalsEmbersZOffset;
        public float OrbitalsEmbersXRotation;
        public float OrbitalsEmbersYRotation;
        public float OrbitalsEmbersZRotation;
        public float OrbitalsEmbersDrift;
    }
}