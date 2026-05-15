using System.Collections.Generic;
using System.Globalization;
using NADA.VFX.Core.Config;
using UnityEngine;

namespace NADA.VFX.Core.State
{
    internal static class VfxStateIO
    {
        internal static VfxState FromConfig()
        {
            return new VfxState
            {
                RigXOffset = PluginConfig.RigXOffset.Value,
                RigYOffset = PluginConfig.RigYOffset.Value,
                RigZOffset = PluginConfig.RigZOffset.Value,
                RigXRotation = PluginConfig.RigXRotation.Value,
                RigYRotation = PluginConfig.RigYRotation.Value,
                RigZRotation = PluginConfig.RigZRotation.Value,

                InnerFlamesEnabled = PluginConfig.InnerFlames.Value,
                InnerFlamesEnergy = PluginConfig.InnerFlamesEnergy.Value,
                InnerFlamesScale = PluginConfig.InnerFlamesScale.Value,
                InnerFlamesLength = PluginConfig.InnerFlamesLength.Value,
                InnerFlamesHue = PluginConfig.InnerFlamesHue.Value,
                InnerFlamesXOffset =  PluginConfig.InnerFlamesXOffset.Value,
                InnerFlamesYOffset = PluginConfig.InnerFlamesYOffset.Value,
                InnerFlamesZOffset = PluginConfig.InnerFlamesZOffset.Value,
                InnerFlamesXRotation = PluginConfig.InnerFlamesXRotation.Value,
                InnerFlamesYRotation = PluginConfig.InnerFlamesYRotation.Value,
                InnerFlamesZRotation = PluginConfig.InnerFlamesZRotation.Value,
                
                OuterFlamesEnabled = PluginConfig.OuterFlames.Value,
                OuterFlamesDragEnabled = PluginConfig.OuterFlamesDragEnabled.Value,
                OuterFlamesEnergy = PluginConfig.OuterFlamesEnergy.Value,
                OuterFlamesScale = PluginConfig.OuterFlamesScale.Value,
                OuterFlamesLength = PluginConfig.OuterFlamesLength.Value,
                OuterFlamesHue = PluginConfig.OuterFlamesHue.Value,
                OuterFlamesXOffset = PluginConfig.OuterFlamesXOffset.Value,
                OuterFlamesYOffset = PluginConfig.OuterFlamesYOffset.Value,
                OuterFlamesZOffset = PluginConfig.OuterFlamesZOffset.Value,
                OuterFlamesXRotation = PluginConfig.OuterFlamesXRotation.Value,
                OuterFlamesYRotation = PluginConfig.OuterFlamesYRotation.Value,
                OuterFlamesZRotation = PluginConfig.OuterFlamesZRotation.Value,

                SparksEnabled = PluginConfig.Sparks.Value,
                SparksEnergy = PluginConfig.SparksEnergy.Value,
                SparksScale = PluginConfig.SparksScale.Value,
                SparksLength = PluginConfig.SparksLength.Value,
                SparksWidth = PluginConfig.SparksWidth.Value,
                SparksHue = PluginConfig.SparksHue.Value,
                SparksXOffset = PluginConfig.SparksXOffset.Value,
                SparksYOffset = PluginConfig.SparksYOffset.Value,
                SparksZOffset = PluginConfig.SparksZOffset.Value,
                SparksXRotation = PluginConfig.SparksXRotation.Value,
                SparksYRotation = PluginConfig.SparksYRotation.Value,
                SparksZRotation = PluginConfig.SparksZRotation.Value,

                FlareEnabled = PluginConfig.Flare.Value,
                FlareScale = PluginConfig.FlareScale.Value,
                FlareHue = PluginConfig.FlareHue.Value,
                FlareXOffset = PluginConfig.FlareXOffset.Value,
                FlareYOffset = PluginConfig.FlareYOffset.Value,
                FlareZOffset = PluginConfig.FlareZOffset.Value,

                AuraEnabled = PluginConfig.Aura.Value,
                AuraScale = PluginConfig.AuraScale.Value,
                AuraHue = PluginConfig.AuraHue.Value,
                AuraXOffset = PluginConfig.AuraXOffset.Value,
                AuraYOffset = PluginConfig.AuraYOffset.Value,
                AuraZOffset = PluginConfig.AuraZOffset.Value,
                AuraXRotation = PluginConfig.AuraXRotation.Value,
                AuraYRotation = PluginConfig.AuraYRotation.Value,
                AuraZRotation = PluginConfig.AuraZRotation.Value,

                OrbitalsOrbsEnabled = PluginConfig.OrbitalsOrbs.Value,
                OrbitalsOrbsCount = PluginConfig.OrbitalsOrbsCount.Value,
                OrbitalsOrbsDrift = PluginConfig.OrbitalsOrbsDrift.Value,
                OrbitalsOrbsScale = PluginConfig.OrbitalsOrbsScale.Value,
                OrbitalsOrbsHue = PluginConfig.OrbitalsOrbsHue.Value,
                OrbitalsOrbsSpeed = PluginConfig.OrbitalsOrbsSpeed.Value,
                OrbitalsOrbsSpacing = PluginConfig.OrbitalsOrbsSpacing.Value,
                OrbitalsOrbsLength = PluginConfig.OrbitalsOrbsLength.Value,
                OrbitalsOrbsRadius = PluginConfig.OrbitalsOrbsRadius.Value,
                OrbitalsOrbsCycles = PluginConfig.OrbitalsOrbsCycles.Value,
                OrbitalsOrbsXOffset =  PluginConfig.OrbitalsOrbsXOffset.Value,
                OrbitalsOrbsYOffset = PluginConfig.OrbitalsOrbsYOffset.Value,
                OrbitalsOrbsZOffset = PluginConfig.OrbitalsOrbsZOffset.Value,
                OrbitalsOrbsXRotation = PluginConfig.OrbitalsOrbsXRotation.Value,
                OrbitalsOrbsYRotation = PluginConfig.OrbitalsOrbsYRotation.Value,
                OrbitalsOrbsZRotation = PluginConfig.OrbitalsOrbsZRotation.Value,

                OrbitalsStrandsEnabled = PluginConfig.OrbitalsStrands.Value,
                OrbitalsStrandsSpectrumEnabled = PluginConfig.OrbitalsStrandsSpectrum.Value,
                OrbitalsStrandsEnergy = PluginConfig.OrbitalsStrandsEnergy.Value,
                OrbitalsStrandsDrift = PluginConfig.OrbitalsStrandsDrift.Value,
                OrbitalsStrandsScaleWhole = PluginConfig.OrbitalsStrandsScaleWhole.Value,
                OrbitalsStrandsScaleParts = PluginConfig.OrbitalsStrandsScaleParts.Value,
                OrbitalsStrandsHue = PluginConfig.OrbitalsStrandsHue.Value,
                OrbitalsStrandsSpectrumSpeed = PluginConfig.OrbitalsStrandsSpectrumSpeed.Value,
                OrbitalsStrandsSpeed = PluginConfig.OrbitalsStrandsSpeed.Value,
                OrbitalsStrandsLength = PluginConfig.OrbitalsStrandsLength.Value,
                OrbitalsStrandsRadius = PluginConfig.OrbitalsStrandsRadius.Value,
                OrbitalsStrandsLifetime = PluginConfig.OrbitalsStrandsLifetime.Value,
                OrbitalsStrandsXOffset = PluginConfig.OrbitalsStrandsXOffset.Value,
                OrbitalsStrandsYOffset = PluginConfig.OrbitalsStrandsYOffset.Value,
                OrbitalsStrandsZOffset = PluginConfig.OrbitalsStrandsZOffset.Value,
                OrbitalsStrandsXRotation = PluginConfig.OrbitalsStrandsXRotation.Value,
                OrbitalsStrandsYRotation = PluginConfig.OrbitalsStrandsYRotation.Value,
                OrbitalsStrandsZRotation = PluginConfig.OrbitalsStrandsZRotation.Value,

                OrbitalsFlamesEnabled = PluginConfig.OrbitalsFlames.Value,
                OrbitalsFlamesCount = PluginConfig.OrbitalsFlamesCount.Value,
                OrbitalsFlamesEnergy = PluginConfig.OrbitalsFlamesEnergy.Value,
                OrbitalsFlamesDrift = PluginConfig.OrbitalsFlamesDrift.Value,
                OrbitalsFlamesHue = PluginConfig.OrbitalsFlamesHue.Value,
                OrbitalsFlamesSpeed = PluginConfig.OrbitalsFlamesSpeed.Value,
                OrbitalsFlamesSpacing = PluginConfig.OrbitalsFlamesSpacing.Value,
                OrbitalsFlamesLength = PluginConfig.OrbitalsFlamesLength.Value,
                OrbitalsFlamesRadius = PluginConfig.OrbitalsFlamesRadius.Value,
                OrbitalsFlamesCycles = PluginConfig.OrbitalsFlamesCycles.Value,
                OrbitalsFlamesXOffset = PluginConfig.OrbitalsFlamesXOffset.Value,
                OrbitalsFlamesYOffset = PluginConfig.OrbitalsFlamesYOffset.Value,
                OrbitalsFlamesZOffset = PluginConfig.OrbitalsFlamesZOffset.Value,
                OrbitalsFlamesXRotation = PluginConfig.OrbitalsFlamesXRotation.Value,
                OrbitalsFlamesYRotation = PluginConfig.OrbitalsFlamesYRotation.Value,
                OrbitalsFlamesZRotation = PluginConfig.OrbitalsFlamesZRotation.Value,

                OrbitalsEmbersEnabled = PluginConfig.OrbitalsEmbers.Value,
                OrbitalsEmbersCount = PluginConfig.OrbitalsEmbersCount.Value,
                OrbitalsEmbersEnergy = PluginConfig.OrbitalsEmbersEnergy.Value,
                OrbitalsEmbersDrift = PluginConfig.OrbitalsEmbersDrift.Value,
                OrbitalsEmbersHue = PluginConfig.OrbitalsEmbersHue.Value,
                OrbitalsEmbersSpeed = PluginConfig.OrbitalsEmbersSpeed.Value,
                OrbitalsEmbersSpacing = PluginConfig.OrbitalsEmbersSpacing.Value,
                OrbitalsEmbersLength = PluginConfig.OrbitalsEmbersLength.Value,
                OrbitalsEmbersRadius = PluginConfig.OrbitalsEmbersRadius.Value,
                OrbitalsEmbersCycles = PluginConfig.OrbitalsEmbersCycles.Value,
                OrbitalsEmbersLifetime = PluginConfig.OrbitalsEmbersLifetime.Value,
                OrbitalsEmbersXOffset = PluginConfig.OrbitalsEmbersXOffset.Value,
                OrbitalsEmbersYOffset =  PluginConfig.OrbitalsEmbersYOffset.Value,
                OrbitalsEmbersZOffset =  PluginConfig.OrbitalsEmbersZOffset.Value,
                OrbitalsEmbersXRotation = PluginConfig.OrbitalsEmbersXRotation.Value,
                OrbitalsEmbersYRotation = PluginConfig.OrbitalsEmbersYRotation.Value,
                OrbitalsEmbersZRotation = PluginConfig.OrbitalsEmbersZRotation.Value
            };
        }

        internal static void Write(global::ItemDrop.ItemData item, VfxState state, bool bound)
        {
            if (item == null)
                return;

            if (item.m_customData == null)
                item.m_customData = new Dictionary<string, string>();

            Dictionary<string, string> customData = item.m_customData;

            WriteBool(customData, VfxStateKeys.Bound, bound);

            WriteFloat(customData, VfxStateKeys.RigXOffset, state.RigXOffset);
            WriteFloat(customData, VfxStateKeys.RigYOffset, state.RigYOffset);
            WriteFloat(customData, VfxStateKeys.RigZOffset, state.RigZOffset);
            WriteFloat(customData, VfxStateKeys.RigXRotation, state.RigXRotation);
            WriteFloat(customData, VfxStateKeys.RigYRotation, state.RigYRotation);
            WriteFloat(customData, VfxStateKeys.RigZRotation, state.RigZRotation);

            WriteBool(customData, VfxStateKeys.InnerFlamesEnabled, state.InnerFlamesEnabled);
            WriteFloat(customData, VfxStateKeys.InnerFlamesEnergy, state.InnerFlamesEnergy);
            WriteFloat(customData, VfxStateKeys.InnerFlamesScale, state.InnerFlamesScale);
            WriteFloat(customData, VfxStateKeys.InnerFlamesLength, state.InnerFlamesLength);
            WriteFloat(customData, VfxStateKeys.InnerFlamesHue, state.InnerFlamesHue);
            WriteFloat(customData, VfxStateKeys.InnerFlamesXOffset, state.InnerFlamesXOffset);
            WriteFloat(customData, VfxStateKeys.InnerFlamesYOffset, state.InnerFlamesYOffset);
            WriteFloat(customData, VfxStateKeys.InnerFlamesZOffset, state.InnerFlamesZOffset);
            WriteFloat(customData, VfxStateKeys.InnerFlamesXRotation, state.InnerFlamesXRotation);
            WriteFloat(customData, VfxStateKeys.InnerFlamesYRotation, state.InnerFlamesYRotation);
            WriteFloat(customData, VfxStateKeys.InnerFlamesZRotation, state.InnerFlamesZRotation);

            WriteBool(customData, VfxStateKeys.OuterFlamesEnabled, state.OuterFlamesEnabled);
            WriteBool(customData, VfxStateKeys.OuterFlamesDragEnabled, state.OuterFlamesDragEnabled);
            WriteFloat(customData, VfxStateKeys.OuterFlamesEnergy, state.OuterFlamesEnergy);
            WriteFloat(customData, VfxStateKeys.OuterFlamesScale, state.OuterFlamesScale);
            WriteFloat(customData, VfxStateKeys.OuterFlamesLength, state.OuterFlamesLength);
            WriteFloat(customData, VfxStateKeys.OuterFlamesHue, state.OuterFlamesHue);
            WriteFloat(customData, VfxStateKeys.OuterFlamesXOffset, state.OuterFlamesXOffset);
            WriteFloat(customData, VfxStateKeys.OuterFlamesYOffset, state.OuterFlamesYOffset);
            WriteFloat(customData, VfxStateKeys.OuterFlamesZOffset, state.OuterFlamesZOffset);
            WriteFloat(customData, VfxStateKeys.OuterFlamesXRotation, state.OuterFlamesXRotation);
            WriteFloat(customData, VfxStateKeys.OuterFlamesYRotation, state.OuterFlamesYRotation);
            WriteFloat(customData, VfxStateKeys.OuterFlamesZRotation, state.OuterFlamesZRotation);

            WriteBool(customData, VfxStateKeys.SparksEnabled, state.SparksEnabled);
            WriteFloat(customData, VfxStateKeys.SparksEnergy, state.SparksEnergy);
            WriteFloat(customData, VfxStateKeys.SparksScale, state.SparksScale);
            WriteFloat(customData, VfxStateKeys.SparksLength, state.SparksLength);
            WriteFloat(customData, VfxStateKeys.SparksWidth, state.SparksWidth);
            WriteFloat(customData, VfxStateKeys.SparksHue, state.SparksHue);
            WriteFloat(customData, VfxStateKeys.SparksXOffset, state.SparksXOffset);
            WriteFloat(customData, VfxStateKeys.SparksYOffset, state.SparksYOffset);
            WriteFloat(customData, VfxStateKeys.SparksZOffset, state.SparksZOffset);
            WriteFloat(customData, VfxStateKeys.SparksXRotation, state.SparksXRotation);
            WriteFloat(customData, VfxStateKeys.SparksYRotation, state.SparksYRotation);
            WriteFloat(customData, VfxStateKeys.SparksZRotation, state.SparksZRotation);
            
            WriteBool(customData, VfxStateKeys.FlareEnabled, state.FlareEnabled);
            WriteFloat(customData, VfxStateKeys.FlareScale, state.FlareScale);
            WriteFloat(customData, VfxStateKeys.FlareHue, state.FlareHue);
            WriteFloat(customData, VfxStateKeys.FlareXOffset, state.FlareXOffset);
            WriteFloat(customData, VfxStateKeys.FlareYOffset, state.FlareYOffset);
            WriteFloat(customData, VfxStateKeys.FlareZOffset, state.FlareZOffset);

            WriteBool(customData, VfxStateKeys.AuraEnabled, state.AuraEnabled);
            WriteFloat(customData, VfxStateKeys.AuraScale, state.AuraScale);
            WriteFloat(customData, VfxStateKeys.AuraHue, state.AuraHue);
            WriteFloat(customData, VfxStateKeys.AuraXOffset, state.AuraXOffset);
            WriteFloat(customData, VfxStateKeys.AuraYOffset, state.AuraYOffset);
            WriteFloat(customData, VfxStateKeys.AuraZOffset, state.AuraZOffset);
            WriteFloat(customData, VfxStateKeys.AuraXRotation, state.AuraXRotation);
            WriteFloat(customData, VfxStateKeys.AuraYRotation, state.AuraYRotation);
            WriteFloat(customData, VfxStateKeys.AuraZRotation, state.AuraZRotation);

            WriteBool(customData, VfxStateKeys.OrbitalsOrbsEnabled, state.OrbitalsOrbsEnabled);
            WriteFloat(customData, VfxStateKeys.OrbitalsOrbsCount, state.OrbitalsOrbsCount);
            WriteFloat(customData, VfxStateKeys.OrbitalsOrbsDrift, state.OrbitalsOrbsDrift);
            WriteFloat(customData, VfxStateKeys.OrbitalsOrbsScale, state.OrbitalsOrbsScale);
            WriteFloat(customData, VfxStateKeys.OrbitalsOrbsHue, state.OrbitalsOrbsHue);
            WriteFloat(customData, VfxStateKeys.OrbitalsOrbsSpeed, state.OrbitalsOrbsSpeed);
            WriteFloat(customData, VfxStateKeys.OrbitalsOrbsSpacing, state.OrbitalsOrbsSpacing);
            WriteFloat(customData, VfxStateKeys.OrbitalsOrbsLength, state.OrbitalsOrbsLength);
            WriteFloat(customData, VfxStateKeys.OrbitalsOrbsRadius, state.OrbitalsOrbsRadius);
            WriteFloat(customData, VfxStateKeys.OrbitalsOrbsCycles, state.OrbitalsOrbsCycles);
            WriteFloat(customData, VfxStateKeys.OrbitalsOrbsXOffset, state.OrbitalsOrbsXOffset);
            WriteFloat(customData, VfxStateKeys.OrbitalsOrbsYOffset, state.OrbitalsOrbsYOffset);
            WriteFloat(customData, VfxStateKeys.OrbitalsOrbsZOffset, state.OrbitalsOrbsZOffset);
            WriteFloat(customData, VfxStateKeys.OrbitalsOrbsXRotation, state.OrbitalsOrbsXRotation);
            WriteFloat(customData, VfxStateKeys.OrbitalsOrbsYRotation, state.OrbitalsOrbsYRotation);
            WriteFloat(customData, VfxStateKeys.OrbitalsOrbsZRotation, state.OrbitalsOrbsZRotation);

            WriteBool(customData, VfxStateKeys.OrbitalsStrandsEnabled, state.OrbitalsStrandsEnabled);
            WriteBool(customData, VfxStateKeys.OrbitalsStrandsSpectrumEnabled, state.OrbitalsStrandsSpectrumEnabled);
            WriteFloat(customData, VfxStateKeys.OrbitalsStrandsEnergy, state.OrbitalsStrandsEnergy);
            WriteFloat(customData, VfxStateKeys.OrbitalsStrandsDrift, state.OrbitalsStrandsDrift);
            WriteFloat(customData, VfxStateKeys.OrbitalsStrandsScaleWhole, state.OrbitalsStrandsScaleWhole);
            WriteFloat(customData, VfxStateKeys.OrbitalsStrandsScaleParts, state.OrbitalsStrandsScaleParts);
            WriteFloat(customData, VfxStateKeys.OrbitalsStrandsHue, state.OrbitalsStrandsHue);
            WriteFloat(customData, VfxStateKeys.OrbitalsStrandsSpectrumSpeed, state.OrbitalsStrandsSpectrumSpeed);
            WriteFloat(customData, VfxStateKeys.OrbitalsStrandsSpeed, state.OrbitalsStrandsSpeed);
            WriteFloat(customData, VfxStateKeys.OrbitalsStrandsLength, state.OrbitalsStrandsLength);
            WriteFloat(customData, VfxStateKeys.OrbitalsStrandsRadius, state.OrbitalsStrandsRadius);
            WriteFloat(customData, VfxStateKeys.OrbitalsStrandsLifetime, state.OrbitalsStrandsLifetime);
            WriteFloat(customData, VfxStateKeys.OrbitalsStrandsXOffset, state.OrbitalsStrandsXOffset);
            WriteFloat(customData, VfxStateKeys.OrbitalsStrandsYOffset, state.OrbitalsStrandsYOffset);
            WriteFloat(customData, VfxStateKeys.OrbitalsStrandsZOffset, state.OrbitalsStrandsZOffset);
            WriteFloat(customData, VfxStateKeys.OrbitalsStrandsXRotation, state.OrbitalsStrandsXRotation);
            WriteFloat(customData, VfxStateKeys.OrbitalsStrandsYRotation, state.OrbitalsStrandsYRotation);
            WriteFloat(customData, VfxStateKeys.OrbitalsStrandsZRotation, state.OrbitalsStrandsZRotation);

            WriteBool(customData, VfxStateKeys.OrbitalsFlamesEnabled, state.OrbitalsFlamesEnabled);
            WriteFloat(customData, VfxStateKeys.OrbitalsFlamesCount, state.OrbitalsFlamesCount);
            WriteFloat(customData, VfxStateKeys.OrbitalsFlamesEnergy, state.OrbitalsFlamesEnergy);
            WriteFloat(customData, VfxStateKeys.OrbitalsFlamesDrift, state.OrbitalsFlamesDrift);
            WriteFloat(customData, VfxStateKeys.OrbitalsFlamesHue, state.OrbitalsFlamesHue);
            WriteFloat(customData, VfxStateKeys.OrbitalsFlamesSpeed, state.OrbitalsFlamesSpeed);
            WriteFloat(customData, VfxStateKeys.OrbitalsFlamesSpacing, state.OrbitalsFlamesSpacing);
            WriteFloat(customData, VfxStateKeys.OrbitalsFlamesLength, state.OrbitalsFlamesLength);
            WriteFloat(customData, VfxStateKeys.OrbitalsFlamesRadius, state.OrbitalsFlamesRadius);
            WriteFloat(customData, VfxStateKeys.OrbitalsFlamesCycles, state.OrbitalsFlamesCycles);
            WriteFloat(customData, VfxStateKeys.OrbitalsFlamesXOffset, state.OrbitalsFlamesXOffset);
            WriteFloat(customData, VfxStateKeys.OrbitalsFlamesYOffset, state.OrbitalsFlamesYOffset);
            WriteFloat(customData, VfxStateKeys.OrbitalsFlamesZOffset, state.OrbitalsFlamesZOffset);
            WriteFloat(customData, VfxStateKeys.OrbitalsFlamesXRotation, state.OrbitalsFlamesXRotation);
            WriteFloat(customData, VfxStateKeys.OrbitalsFlamesYRotation, state.OrbitalsFlamesYRotation);
            WriteFloat(customData, VfxStateKeys.OrbitalsFlamesZRotation, state.OrbitalsFlamesZRotation);

            WriteBool(customData, VfxStateKeys.OrbitalsEmbersEnabled, state.OrbitalsEmbersEnabled);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersCount, state.OrbitalsEmbersCount);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersEnergy, state.OrbitalsEmbersEnergy);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersDrift, state.OrbitalsEmbersDrift);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersHue, state.OrbitalsEmbersHue);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersSpeed, state.OrbitalsEmbersSpeed);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersSpacing, state.OrbitalsEmbersSpacing);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersLength, state.OrbitalsEmbersLength);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersRadius, state.OrbitalsEmbersRadius);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersCycles, state.OrbitalsEmbersCycles);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersLifetime, state.OrbitalsEmbersLifetime);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersXOffset, state.OrbitalsEmbersXOffset);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersYOffset, state.OrbitalsEmbersYOffset);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersZOffset, state.OrbitalsEmbersZOffset);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersXRotation, state.OrbitalsEmbersXRotation);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersYRotation, state.OrbitalsEmbersYRotation);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersZRotation, state.OrbitalsEmbersZRotation);
        }

        internal static bool TryRead(global::ItemDrop.ItemData item, out VfxState state)
        {
            state = default;
            if (item == null)
                return false;

            Dictionary<string, string> customData = item.m_customData;
            if (customData == null)
                return false;

            state.RigXOffset = ReadFloat(customData, VfxStateKeys.RigXOffset, PluginConfig.RigXOffset.Value);
            state.RigYOffset = ReadFloat(customData, VfxStateKeys.RigYOffset, PluginConfig.RigYOffset.Value);
            state.RigZOffset = ReadFloat(customData, VfxStateKeys.RigZOffset, PluginConfig.RigZOffset.Value);
            state.RigXRotation = ReadFloat(customData, VfxStateKeys.RigXRotation, PluginConfig.RigXRotation.Value);
            state.RigYRotation = ReadFloat(customData, VfxStateKeys.RigYRotation, PluginConfig.RigYRotation.Value);
            state.RigZRotation = ReadFloat(customData, VfxStateKeys.RigZRotation, PluginConfig.RigZRotation.Value);

            state.InnerFlamesEnabled = ReadBool(customData, VfxStateKeys.InnerFlamesEnabled, PluginConfig.InnerFlames.Value);
            state.InnerFlamesEnergy = ReadFloat(customData, VfxStateKeys.InnerFlamesEnergy, PluginConfig.InnerFlamesEnergy.Value);
            state.InnerFlamesScale = ReadFloat(customData, VfxStateKeys.InnerFlamesScale, PluginConfig.InnerFlamesScale.Value);
            state.InnerFlamesLength = ReadFloat(customData, VfxStateKeys.InnerFlamesLength, PluginConfig.InnerFlamesLength.Value);
            state.InnerFlamesHue = ReadFloat(customData, VfxStateKeys.InnerFlamesHue, PluginConfig.InnerFlamesHue.Value);
            state.InnerFlamesXOffset = ReadFloat(customData, VfxStateKeys.InnerFlamesXOffset, PluginConfig.InnerFlamesXOffset.Value);
            state.InnerFlamesYOffset = ReadFloat(customData, VfxStateKeys.InnerFlamesYOffset, PluginConfig.InnerFlamesYOffset.Value);
            state.InnerFlamesZOffset = ReadFloat(customData, VfxStateKeys.InnerFlamesZOffset, PluginConfig.InnerFlamesZOffset.Value);
            state.InnerFlamesXRotation = ReadFloat(customData, VfxStateKeys.InnerFlamesXRotation, PluginConfig.InnerFlamesXRotation.Value);
            state.InnerFlamesYRotation = ReadFloat(customData, VfxStateKeys.InnerFlamesYRotation, PluginConfig.InnerFlamesYRotation.Value);
            state.InnerFlamesZRotation = ReadFloat(customData, VfxStateKeys.InnerFlamesZRotation, PluginConfig.InnerFlamesZRotation.Value);
            state.InnerFlamesXRotation = ReadFloat(customData, VfxStateKeys.InnerFlamesXRotation, PluginConfig.InnerFlamesXRotation.Value);
            
            state.OuterFlamesEnabled = ReadBool(customData, VfxStateKeys.OuterFlamesEnabled, PluginConfig.OuterFlames.Value);
            state.OuterFlamesDragEnabled = ReadBool(customData, VfxStateKeys.OuterFlamesDragEnabled, PluginConfig.OuterFlamesDragEnabled.Value);
            state.OuterFlamesEnergy = ReadFloat(customData, VfxStateKeys.OuterFlamesEnergy, PluginConfig.OuterFlamesEnergy.Value);
            state.OuterFlamesScale = ReadFloat(customData, VfxStateKeys.OuterFlamesScale, PluginConfig.OuterFlamesScale.Value);
            state.OuterFlamesLength = ReadFloat(customData, VfxStateKeys.OuterFlamesLength, PluginConfig.OuterFlamesLength.Value);
            state.OuterFlamesHue = ReadFloat(customData, VfxStateKeys.OuterFlamesHue, PluginConfig.OuterFlamesHue.Value);
            state.OuterFlamesXOffset = ReadFloat(customData, VfxStateKeys.OuterFlamesXOffset, PluginConfig.OuterFlamesXOffset.Value);
            state.OuterFlamesYOffset = ReadFloat(customData, VfxStateKeys.OuterFlamesYOffset, PluginConfig.OuterFlamesYOffset.Value);
            state.OuterFlamesZOffset = ReadFloat(customData, VfxStateKeys.OuterFlamesZOffset, PluginConfig.OuterFlamesZOffset.Value);
            state.OuterFlamesYRotation = ReadFloat(customData, VfxStateKeys.OuterFlamesYRotation, PluginConfig.OuterFlamesYRotation.Value);
            state.OuterFlamesZRotation = ReadFloat(customData, VfxStateKeys.OuterFlamesZRotation, PluginConfig.OuterFlamesZRotation.Value);
            state.OuterFlamesXRotation = ReadFloat(customData, VfxStateKeys.OuterFlamesXRotation, PluginConfig.OuterFlamesXRotation.Value);

            state.SparksEnabled = ReadBool(customData, VfxStateKeys.SparksEnabled, PluginConfig.Sparks.Value);
            state.SparksEnergy = ReadFloat(customData, VfxStateKeys.SparksEnergy, PluginConfig.SparksEnergy.Value);
            state.SparksScale = ReadFloat(customData, VfxStateKeys.SparksScale, PluginConfig.SparksScale.Value);
            state.SparksLength = ReadFloat(customData, VfxStateKeys.SparksLength, PluginConfig.SparksLength.Value);
            state.SparksWidth = ReadFloat(customData, VfxStateKeys.SparksWidth, PluginConfig.SparksWidth.Value);
            state.SparksHue = ReadFloat(customData, VfxStateKeys.SparksHue, PluginConfig.SparksHue.Value);
            state.SparksXOffset = ReadFloat(customData, VfxStateKeys.SparksXOffset, PluginConfig.SparksXOffset.Value);
            state.SparksYOffset = ReadFloat(customData, VfxStateKeys.SparksYOffset, PluginConfig.SparksYOffset.Value);
            state.SparksZOffset = ReadFloat(customData, VfxStateKeys.SparksZOffset, PluginConfig.SparksZOffset.Value);
            state.SparksXRotation = ReadFloat(customData, VfxStateKeys.SparksXRotation, PluginConfig.SparksXRotation.Value);
            state.SparksYRotation = ReadFloat(customData, VfxStateKeys.SparksYRotation, PluginConfig.SparksYRotation.Value);
            state.SparksZRotation = ReadFloat(customData, VfxStateKeys.SparksZRotation, PluginConfig.SparksZRotation.Value);
            
            state.FlareEnabled = ReadBool(customData, VfxStateKeys.FlareEnabled, PluginConfig.Flare.Value);
            state.FlareScale = ReadFloat(customData, VfxStateKeys.FlareScale, PluginConfig.FlareScale.Value);
            state.FlareHue = ReadFloat(customData, VfxStateKeys.FlareHue, PluginConfig.FlareHue.Value);
            state.FlareXOffset = ReadFloat(customData, VfxStateKeys.FlareXOffset, PluginConfig.FlareXOffset.Value);
            state.FlareYOffset = ReadFloat(customData, VfxStateKeys.FlareYOffset, PluginConfig.FlareYOffset.Value);
            state.FlareZOffset = ReadFloat(customData, VfxStateKeys.FlareZOffset, PluginConfig.FlareZOffset.Value);

            state.AuraEnabled = ReadBool(customData, VfxStateKeys.AuraEnabled, PluginConfig.Aura.Value);
            state.AuraScale = ReadFloat(customData, VfxStateKeys.AuraScale, PluginConfig.AuraScale.Value);
            state.AuraHue = ReadFloat(customData, VfxStateKeys.AuraHue, PluginConfig.AuraHue.Value);
            state.AuraXOffset = ReadFloat(customData, VfxStateKeys.AuraXOffset, PluginConfig.AuraXOffset.Value);
            state.AuraYOffset = ReadFloat(customData, VfxStateKeys.AuraYOffset, PluginConfig.AuraYOffset.Value);
            state.AuraZOffset = ReadFloat(customData, VfxStateKeys.AuraZOffset, PluginConfig.AuraZOffset.Value);
            state.AuraXRotation = ReadFloat(customData, VfxStateKeys.AuraXRotation, PluginConfig.AuraXRotation.Value);
            state.AuraYRotation = ReadFloat(customData, VfxStateKeys.AuraYRotation, PluginConfig.AuraYRotation.Value);
            state.AuraZRotation = ReadFloat(customData, VfxStateKeys.AuraZRotation, PluginConfig.AuraZRotation.Value);

            state.OrbitalsOrbsEnabled = ReadBool(customData, VfxStateKeys.OrbitalsOrbsEnabled, PluginConfig.OrbitalsOrbs.Value);
            state.OrbitalsOrbsCount = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsCount, PluginConfig.OrbitalsOrbsCount.Value);
            state.OrbitalsOrbsDrift = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsDrift, PluginConfig.OrbitalsOrbsDrift.Value);
            state.OrbitalsOrbsScale = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsScale, PluginConfig.OrbitalsOrbsScale.Value);
            state.OrbitalsOrbsHue = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsHue, PluginConfig.OrbitalsOrbsHue.Value);
            state.OrbitalsOrbsSpeed = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsSpeed, PluginConfig.OrbitalsOrbsSpeed.Value);
            state.OrbitalsOrbsSpacing = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsSpacing, PluginConfig.OrbitalsOrbsSpacing.Value);
            state.OrbitalsOrbsLength = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsLength, PluginConfig.OrbitalsOrbsLength.Value);
            state.OrbitalsOrbsRadius = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsRadius, PluginConfig.OrbitalsOrbsRadius.Value);
            state.OrbitalsOrbsCycles = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsCycles, PluginConfig.OrbitalsOrbsCycles.Value);
            state.OrbitalsOrbsXOffset = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsXOffset, PluginConfig.OrbitalsOrbsXOffset.Value);
            state.OrbitalsOrbsYOffset = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsYOffset, PluginConfig.OrbitalsOrbsYOffset.Value);
            state.OrbitalsOrbsZOffset = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsZOffset, PluginConfig.OrbitalsOrbsZOffset.Value);
            state.OrbitalsOrbsXRotation = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsXRotation, PluginConfig.OrbitalsOrbsXRotation.Value);
            state.OrbitalsOrbsYRotation = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsYRotation, PluginConfig.OrbitalsOrbsYRotation.Value);
            state.OrbitalsOrbsZRotation = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsZRotation, PluginConfig.OrbitalsOrbsZRotation.Value);
            
            state.OrbitalsStrandsEnabled = ReadBool(customData, VfxStateKeys.OrbitalsStrandsEnabled, PluginConfig.OrbitalsStrands.Value);
            state.OrbitalsStrandsSpectrumEnabled = ReadBool(customData, VfxStateKeys.OrbitalsStrandsSpectrumEnabled, PluginConfig.OrbitalsStrandsSpectrum.Value);
            state.OrbitalsStrandsEnergy = ReadFloat(customData, VfxStateKeys.OrbitalsStrandsEnergy, PluginConfig.OrbitalsStrandsEnergy.Value);
            state.OrbitalsStrandsDrift = ReadFloat(customData, VfxStateKeys.OrbitalsStrandsDrift, PluginConfig.OrbitalsStrandsDrift.Value);
            state.OrbitalsStrandsScaleWhole = ReadFloat(customData, VfxStateKeys.OrbitalsStrandsScaleWhole, PluginConfig.OrbitalsStrandsScaleWhole.Value);
            state.OrbitalsStrandsScaleParts = ReadFloat(customData, VfxStateKeys.OrbitalsStrandsScaleParts, PluginConfig.OrbitalsStrandsScaleParts.Value);
            state.OrbitalsStrandsHue = ReadFloat(customData, VfxStateKeys.OrbitalsStrandsHue, PluginConfig.OrbitalsStrandsHue.Value);
            state.OrbitalsStrandsSpectrumSpeed = ReadFloat(customData, VfxStateKeys.OrbitalsStrandsSpectrumSpeed, PluginConfig.OrbitalsStrandsSpectrumSpeed.Value);
            state.OrbitalsStrandsSpeed = ReadFloat(customData, VfxStateKeys.OrbitalsStrandsSpeed, PluginConfig.OrbitalsStrandsSpeed.Value);
            state.OrbitalsStrandsLength = ReadFloat(customData, VfxStateKeys.OrbitalsStrandsLength, PluginConfig.OrbitalsStrandsLength.Value);
            state.OrbitalsStrandsRadius = ReadFloat(customData, VfxStateKeys.OrbitalsStrandsRadius, PluginConfig.OrbitalsStrandsRadius.Value);
            state.OrbitalsStrandsLifetime = ReadFloat(customData, VfxStateKeys.OrbitalsStrandsLifetime, PluginConfig.OrbitalsStrandsLifetime.Value);
            state.OrbitalsStrandsXOffset = ReadFloat(customData, VfxStateKeys.OrbitalsStrandsXOffset,  PluginConfig.OrbitalsStrandsXOffset.Value);
            state.OrbitalsStrandsYOffset = ReadFloat(customData, VfxStateKeys.OrbitalsStrandsYOffset,  PluginConfig.OrbitalsStrandsYOffset.Value);
            state.OrbitalsStrandsZOffset = ReadFloat(customData, VfxStateKeys.OrbitalsStrandsZOffset,  PluginConfig.OrbitalsStrandsZOffset.Value);
            state.OrbitalsStrandsXRotation = ReadFloat(customData, VfxStateKeys.OrbitalsStrandsXRotation, PluginConfig.OrbitalsStrandsXRotation.Value);
            state.OrbitalsStrandsYRotation = ReadFloat(customData, VfxStateKeys.OrbitalsStrandsYRotation, PluginConfig.OrbitalsStrandsYRotation.Value);
            state.OrbitalsStrandsZRotation = ReadFloat(customData, VfxStateKeys.OrbitalsStrandsZRotation, PluginConfig.OrbitalsStrandsZRotation.Value);

            state.OrbitalsFlamesEnabled = ReadBool(customData, VfxStateKeys.OrbitalsFlamesEnabled, PluginConfig.OrbitalsFlames.Value);
            state.OrbitalsFlamesCount = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesCount, PluginConfig.OrbitalsFlamesCount.Value);
            state.OrbitalsFlamesEnergy = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesEnergy, PluginConfig.OrbitalsFlamesEnergy.Value);
            state.OrbitalsFlamesDrift = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesDrift, PluginConfig.OrbitalsFlamesDrift.Value);
            state.OrbitalsFlamesHue = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesHue, PluginConfig.OrbitalsFlamesHue.Value);
            state.OrbitalsFlamesSpeed = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesSpeed, PluginConfig.OrbitalsFlamesSpeed.Value);
            state.OrbitalsFlamesSpacing = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesSpacing, PluginConfig.OrbitalsFlamesSpacing.Value);
            state.OrbitalsFlamesLength = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesLength, PluginConfig.OrbitalsFlamesLength.Value);
            state.OrbitalsFlamesRadius = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesRadius, PluginConfig.OrbitalsFlamesRadius.Value);
            state.OrbitalsFlamesCycles = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesCycles, PluginConfig.OrbitalsFlamesCycles.Value);
            state.OrbitalsFlamesXOffset = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesXOffset, PluginConfig.OrbitalsFlamesXOffset.Value);
            state.OrbitalsFlamesYOffset = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesYOffset, PluginConfig.OrbitalsFlamesYOffset.Value);
            state.OrbitalsFlamesZOffset = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesZOffset, PluginConfig.OrbitalsFlamesZOffset.Value);
            state.OrbitalsFlamesXRotation = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesXRotation, PluginConfig.OrbitalsFlamesXRotation.Value);
            state.OrbitalsFlamesYRotation = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesYRotation, PluginConfig.OrbitalsFlamesYRotation.Value);
            state.OrbitalsFlamesZRotation = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesZRotation, PluginConfig.OrbitalsFlamesZRotation.Value);

            state.OrbitalsEmbersEnabled = ReadBool(customData, VfxStateKeys.OrbitalsEmbersEnabled, PluginConfig.OrbitalsEmbers.Value);
            state.OrbitalsEmbersCount = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersCount, PluginConfig.OrbitalsEmbersCount.Value);
            state.OrbitalsEmbersEnergy = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersEnergy, PluginConfig.OrbitalsEmbersEnergy.Value);
            state.OrbitalsEmbersDrift = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersDrift, PluginConfig.OrbitalsEmbersDrift.Value);
            state.OrbitalsEmbersHue = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersHue, PluginConfig.OrbitalsEmbersHue.Value);
            state.OrbitalsEmbersSpeed = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersSpeed, PluginConfig.OrbitalsEmbersSpeed.Value);
            state.OrbitalsEmbersSpacing = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersSpacing, PluginConfig.OrbitalsEmbersSpacing.Value);
            state.OrbitalsEmbersLength = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersLength, PluginConfig.OrbitalsEmbersLength.Value);
            state.OrbitalsEmbersRadius = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersRadius, PluginConfig.OrbitalsEmbersRadius.Value);
            state.OrbitalsEmbersCycles = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersCycles, PluginConfig.OrbitalsEmbersCycles.Value);
            state.OrbitalsEmbersLifetime = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersLifetime, PluginConfig.OrbitalsEmbersLifetime.Value);
            state.OrbitalsEmbersXOffset = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersXOffset, PluginConfig.OrbitalsEmbersXOffset.Value);
            state.OrbitalsEmbersYOffset = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersYOffset, PluginConfig.OrbitalsEmbersYOffset.Value);
            state.OrbitalsEmbersZOffset = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersZOffset, PluginConfig.OrbitalsEmbersZOffset.Value);
            state.OrbitalsEmbersXRotation = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersXRotation, PluginConfig.OrbitalsEmbersXRotation.Value);
            state.OrbitalsEmbersYRotation = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersYRotation, PluginConfig.OrbitalsEmbersYRotation.Value);
            state.OrbitalsEmbersZRotation = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersZRotation, PluginConfig.OrbitalsEmbersZRotation.Value);
            
            return true;
        }

        internal static void EnsureInitializedFromConfig(global::ItemDrop.ItemData item)
        {
            if (item == null)
                return;

            if (IsBound(item))
                return;

            if (item.m_customData == null)
                item.m_customData = new Dictionary<string, string>();

            Dictionary<string, string> customData = item.m_customData;

            BackfillMissing(customData, VfxStateKeys.RigXOffset, PluginConfig.RigXOffset.Value);
            BackfillMissing(customData, VfxStateKeys.RigYOffset, PluginConfig.RigYOffset.Value);
            BackfillMissing(customData, VfxStateKeys.RigZOffset, PluginConfig.RigZOffset.Value);
            BackfillMissing(customData, VfxStateKeys.RigXRotation, PluginConfig.RigXRotation.Value);
            BackfillMissing(customData, VfxStateKeys.RigYRotation, PluginConfig.RigYRotation.Value);
            BackfillMissing(customData, VfxStateKeys.RigZRotation, PluginConfig.RigZRotation.Value);

            BackfillMissing(customData, VfxStateKeys.InnerFlamesEnabled, PluginConfig.InnerFlames.Value);
            BackfillMissing(customData, VfxStateKeys.InnerFlamesEnergy, PluginConfig.InnerFlamesEnergy.Value);
            BackfillMissing(customData, VfxStateKeys.InnerFlamesScale, PluginConfig.InnerFlamesScale.Value);
            BackfillMissing(customData, VfxStateKeys.InnerFlamesLength, PluginConfig.InnerFlamesLength.Value);
            BackfillMissing(customData, VfxStateKeys.InnerFlamesHue, PluginConfig.InnerFlamesHue.Value);
            BackfillMissing(customData, VfxStateKeys.InnerFlamesXOffset, PluginConfig.InnerFlamesXOffset.Value);
            BackfillMissing(customData, VfxStateKeys.InnerFlamesYOffset, PluginConfig.InnerFlamesYOffset.Value);
            BackfillMissing(customData, VfxStateKeys.InnerFlamesZOffset, PluginConfig.InnerFlamesZOffset.Value);
            BackfillMissing(customData, VfxStateKeys.InnerFlamesXRotation, PluginConfig.InnerFlamesXRotation.Value);
            BackfillMissing(customData, VfxStateKeys.InnerFlamesYRotation, PluginConfig.InnerFlamesYRotation.Value);
            BackfillMissing(customData, VfxStateKeys.InnerFlamesZRotation, PluginConfig.InnerFlamesZRotation.Value);

            BackfillMissing(customData, VfxStateKeys.OuterFlamesEnabled, PluginConfig.OuterFlames.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesDragEnabled, PluginConfig.OuterFlamesDragEnabled.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesEnergy, PluginConfig.OuterFlamesEnergy.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesScale, PluginConfig.OuterFlamesScale.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesLength, PluginConfig.OuterFlamesLength.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesHue, PluginConfig.OuterFlamesHue.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesXOffset, PluginConfig.OuterFlamesXOffset.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesYOffset, PluginConfig.OuterFlamesYOffset.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesZOffset, PluginConfig.OuterFlamesZOffset.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesXRotation, PluginConfig.OuterFlamesXRotation.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesYRotation, PluginConfig.OuterFlamesYRotation.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesZRotation, PluginConfig.OuterFlamesZRotation.Value);

            BackfillMissing(customData, VfxStateKeys.SparksEnabled, PluginConfig.Sparks.Value);
            BackfillMissing(customData, VfxStateKeys.SparksEnergy, PluginConfig.SparksEnergy.Value);
            BackfillMissing(customData, VfxStateKeys.SparksScale, PluginConfig.SparksScale.Value);
            BackfillMissing(customData, VfxStateKeys.SparksLength, PluginConfig.SparksLength.Value);
            BackfillMissing(customData, VfxStateKeys.SparksWidth, PluginConfig.SparksWidth.Value);
            BackfillMissing(customData, VfxStateKeys.SparksHue, PluginConfig.SparksHue.Value);
            BackfillMissing(customData, VfxStateKeys.SparksXOffset, PluginConfig.SparksXOffset.Value);
            BackfillMissing(customData, VfxStateKeys.SparksYOffset, PluginConfig.SparksYOffset.Value);
            BackfillMissing(customData, VfxStateKeys.SparksZOffset, PluginConfig.SparksZOffset.Value);
            BackfillMissing(customData, VfxStateKeys.SparksXRotation, PluginConfig.SparksXRotation.Value);
            BackfillMissing(customData, VfxStateKeys.SparksYRotation, PluginConfig.SparksYRotation.Value);
            BackfillMissing(customData, VfxStateKeys.SparksZRotation, PluginConfig.SparksZRotation.Value);

            BackfillMissing(customData, VfxStateKeys.FlareEnabled, PluginConfig.Flare.Value);
            BackfillMissing(customData, VfxStateKeys.FlareScale, PluginConfig.FlareScale.Value);
            BackfillMissing(customData, VfxStateKeys.FlareHue, PluginConfig.FlareHue.Value);
            BackfillMissing(customData, VfxStateKeys.FlareXOffset, PluginConfig.FlareXOffset.Value);
            BackfillMissing(customData, VfxStateKeys.FlareYOffset, PluginConfig.FlareYOffset.Value);
            BackfillMissing(customData, VfxStateKeys.FlareZOffset, PluginConfig.FlareZOffset.Value);

            BackfillMissing(customData, VfxStateKeys.AuraEnabled, PluginConfig.Aura.Value);
            BackfillMissing(customData, VfxStateKeys.AuraScale, PluginConfig.AuraScale.Value);
            BackfillMissing(customData, VfxStateKeys.AuraHue, PluginConfig.AuraHue.Value);
            BackfillMissing(customData, VfxStateKeys.AuraXOffset, PluginConfig.AuraXOffset.Value);
            BackfillMissing(customData, VfxStateKeys.AuraYOffset, PluginConfig.AuraYOffset.Value);
            BackfillMissing(customData, VfxStateKeys.AuraZOffset, PluginConfig.AuraZOffset.Value);
            BackfillMissing(customData, VfxStateKeys.AuraXRotation, PluginConfig.AuraXRotation.Value);
            BackfillMissing(customData, VfxStateKeys.AuraYRotation, PluginConfig.AuraYRotation.Value);
            BackfillMissing(customData, VfxStateKeys.AuraZRotation, PluginConfig.AuraZRotation.Value);

            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsEnabled, PluginConfig.OrbitalsOrbs.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsCount, PluginConfig.OrbitalsOrbsCount.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsDrift, PluginConfig.OrbitalsOrbsDrift.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsScale, PluginConfig.OrbitalsOrbsScale.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsHue, PluginConfig.OrbitalsOrbsHue.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsSpeed, PluginConfig.OrbitalsOrbsSpeed.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsSpacing, PluginConfig.OrbitalsOrbsSpacing.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsLength, PluginConfig.OrbitalsOrbsLength.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsRadius, PluginConfig.OrbitalsOrbsRadius.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsCycles, PluginConfig.OrbitalsOrbsCycles.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsXOffset, PluginConfig.OrbitalsOrbsXOffset.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsYOffset, PluginConfig.OrbitalsOrbsYOffset.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsZOffset, PluginConfig.OrbitalsOrbsZOffset.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsXRotation, PluginConfig.OrbitalsOrbsXRotation.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsYRotation, PluginConfig.OrbitalsOrbsYRotation.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsZRotation, PluginConfig.OrbitalsOrbsZRotation.Value);

            BackfillMissing(customData, VfxStateKeys.OrbitalsStrandsEnabled, PluginConfig.OrbitalsStrands.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsStrandsSpectrumEnabled, PluginConfig.OrbitalsStrandsSpectrum.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsStrandsEnergy, PluginConfig.OrbitalsStrandsEnergy.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsStrandsDrift, PluginConfig.OrbitalsStrandsDrift.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsStrandsScaleWhole, PluginConfig.OrbitalsStrandsScaleWhole.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsStrandsScaleParts, PluginConfig.OrbitalsStrandsScaleParts.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsStrandsHue, PluginConfig.OrbitalsStrandsHue.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsStrandsSpectrumSpeed, PluginConfig.OrbitalsStrandsSpectrumSpeed.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsStrandsSpeed, PluginConfig.OrbitalsStrandsSpeed.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsStrandsLength, PluginConfig.OrbitalsStrandsLength.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsStrandsRadius, PluginConfig.OrbitalsStrandsRadius.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsStrandsLifetime, PluginConfig.OrbitalsStrandsLifetime.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsStrandsXOffset, PluginConfig.OrbitalsStrandsXOffset.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsStrandsYOffset, PluginConfig.OrbitalsStrandsYOffset.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsStrandsZOffset, PluginConfig.OrbitalsStrandsZOffset.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsStrandsXRotation, PluginConfig.OrbitalsStrandsXRotation.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsStrandsYRotation, PluginConfig.OrbitalsStrandsYRotation.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsStrandsZRotation, PluginConfig.OrbitalsStrandsZRotation.Value);

            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesEnabled, PluginConfig.OrbitalsFlames.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesCount, PluginConfig.OrbitalsFlamesCount.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesEnergy, PluginConfig.OrbitalsFlamesEnergy.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesDrift, PluginConfig.OrbitalsFlamesDrift.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesHue, PluginConfig.OrbitalsFlamesHue.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesSpeed, PluginConfig.OrbitalsFlamesSpeed.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesSpacing, PluginConfig.OrbitalsFlamesSpacing.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesLength, PluginConfig.OrbitalsFlamesLength.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesRadius, PluginConfig.OrbitalsFlamesRadius.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesCycles, PluginConfig.OrbitalsFlamesCycles.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesXOffset, PluginConfig.OrbitalsFlamesXOffset.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesYOffset, PluginConfig.OrbitalsFlamesYOffset.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesZOffset, PluginConfig.OrbitalsFlamesZOffset.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesXRotation, PluginConfig.OrbitalsFlamesXRotation.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesYRotation, PluginConfig.OrbitalsFlamesYRotation.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesZRotation, PluginConfig.OrbitalsFlamesZRotation.Value);

            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersEnabled, PluginConfig.OrbitalsEmbers.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersCount, PluginConfig.OrbitalsEmbersCount.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersEnergy, PluginConfig.OrbitalsEmbersEnergy.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersDrift, PluginConfig.OrbitalsEmbersDrift.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersHue, PluginConfig.OrbitalsEmbersHue.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersSpeed, PluginConfig.OrbitalsEmbersSpeed.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersSpacing, PluginConfig.OrbitalsEmbersSpacing.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersLength, PluginConfig.OrbitalsEmbersLength.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersRadius, PluginConfig.OrbitalsEmbersRadius.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersCycles, PluginConfig.OrbitalsEmbersCycles.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersLifetime, PluginConfig.OrbitalsEmbersLifetime.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersXOffset, PluginConfig.OrbitalsEmbersXOffset.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersYOffset, PluginConfig.OrbitalsEmbersYOffset.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersZOffset, PluginConfig.OrbitalsEmbersZOffset.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersXRotation, PluginConfig.OrbitalsEmbersXRotation.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersYRotation, PluginConfig.OrbitalsEmbersYRotation.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersZRotation, PluginConfig.OrbitalsEmbersZRotation.Value);
        }

        internal static void Clear(global::ItemDrop.ItemData item)
        {
            if (item?.m_customData == null)
                return;
            
            item.m_customData.Remove(VfxStateKeys.Bound);

            item.m_customData.Remove(VfxStateKeys.RigXOffset);
            item.m_customData.Remove(VfxStateKeys.RigYOffset);
            item.m_customData.Remove(VfxStateKeys.RigZOffset);
            item.m_customData.Remove(VfxStateKeys.RigXRotation);
            item.m_customData.Remove(VfxStateKeys.RigYRotation);
            item.m_customData.Remove(VfxStateKeys.RigZRotation);

            item.m_customData.Remove(VfxStateKeys.InnerFlamesEnabled);
            item.m_customData.Remove(VfxStateKeys.InnerFlamesEnergy);
            item.m_customData.Remove(VfxStateKeys.InnerFlamesScale);
            item.m_customData.Remove(VfxStateKeys.InnerFlamesLength);
            item.m_customData.Remove(VfxStateKeys.InnerFlamesHue);
            item.m_customData.Remove(VfxStateKeys.InnerFlamesXOffset);
            item.m_customData.Remove(VfxStateKeys.InnerFlamesYOffset);
            item.m_customData.Remove(VfxStateKeys.InnerFlamesZOffset);
            item.m_customData.Remove(VfxStateKeys.InnerFlamesXRotation);
            item.m_customData.Remove(VfxStateKeys.InnerFlamesYRotation);
            item.m_customData.Remove(VfxStateKeys.InnerFlamesZRotation);

            item.m_customData.Remove(VfxStateKeys.OuterFlamesEnabled);
            item.m_customData.Remove(VfxStateKeys.OuterFlamesDragEnabled);
            item.m_customData.Remove(VfxStateKeys.OuterFlamesEnergy);
            item.m_customData.Remove(VfxStateKeys.OuterFlamesScale);
            item.m_customData.Remove(VfxStateKeys.OuterFlamesLength);
            item.m_customData.Remove(VfxStateKeys.OuterFlamesHue);
            item.m_customData.Remove(VfxStateKeys.OuterFlamesXOffset);
            item.m_customData.Remove(VfxStateKeys.OuterFlamesYOffset);
            item.m_customData.Remove(VfxStateKeys.OuterFlamesZOffset);
            item.m_customData.Remove(VfxStateKeys.OuterFlamesXRotation);
            item.m_customData.Remove(VfxStateKeys.OuterFlamesYRotation);
            item.m_customData.Remove(VfxStateKeys.OuterFlamesZRotation);

            item.m_customData.Remove(VfxStateKeys.SparksEnabled);
            item.m_customData.Remove(VfxStateKeys.SparksEnergy);
            item.m_customData.Remove(VfxStateKeys.SparksScale);
            item.m_customData.Remove(VfxStateKeys.SparksLength);
            item.m_customData.Remove(VfxStateKeys.SparksWidth);
            item.m_customData.Remove(VfxStateKeys.SparksHue);
            item.m_customData.Remove(VfxStateKeys.SparksXOffset);
            item.m_customData.Remove(VfxStateKeys.SparksYOffset);
            item.m_customData.Remove(VfxStateKeys.SparksZOffset);
            item.m_customData.Remove(VfxStateKeys.SparksXRotation);
            item.m_customData.Remove(VfxStateKeys.SparksYRotation);
            item.m_customData.Remove(VfxStateKeys.SparksZRotation);

            item.m_customData.Remove(VfxStateKeys.FlareEnabled);
            item.m_customData.Remove(VfxStateKeys.FlareScale);
            item.m_customData.Remove(VfxStateKeys.FlareHue);
            item.m_customData.Remove(VfxStateKeys.FlareXOffset);
            item.m_customData.Remove(VfxStateKeys.FlareYOffset);
            item.m_customData.Remove(VfxStateKeys.FlareZOffset);

            item.m_customData.Remove(VfxStateKeys.AuraEnabled);
            item.m_customData.Remove(VfxStateKeys.AuraScale);
            item.m_customData.Remove(VfxStateKeys.AuraHue);
            item.m_customData.Remove(VfxStateKeys.AuraXOffset);
            item.m_customData.Remove(VfxStateKeys.AuraYOffset);
            item.m_customData.Remove(VfxStateKeys.AuraZOffset);
            item.m_customData.Remove(VfxStateKeys.AuraXRotation);
            item.m_customData.Remove(VfxStateKeys.AuraYRotation);
            item.m_customData.Remove(VfxStateKeys.AuraZRotation);

            item.m_customData.Remove(VfxStateKeys.OrbitalsOrbsEnabled);
            item.m_customData.Remove(VfxStateKeys.OrbitalsOrbsCount);
            item.m_customData.Remove(VfxStateKeys.OrbitalsOrbsDrift);
            item.m_customData.Remove(VfxStateKeys.OrbitalsOrbsScale);
            item.m_customData.Remove(VfxStateKeys.OrbitalsOrbsHue);
            item.m_customData.Remove(VfxStateKeys.OrbitalsOrbsSpeed);
            item.m_customData.Remove(VfxStateKeys.OrbitalsOrbsSpacing);
            item.m_customData.Remove(VfxStateKeys.OrbitalsOrbsLength);
            item.m_customData.Remove(VfxStateKeys.OrbitalsOrbsRadius);
            item.m_customData.Remove(VfxStateKeys.OrbitalsOrbsCycles);
            item.m_customData.Remove(VfxStateKeys.OrbitalsOrbsXOffset);
            item.m_customData.Remove(VfxStateKeys.OrbitalsOrbsYOffset);
            item.m_customData.Remove(VfxStateKeys.OrbitalsOrbsZOffset);
            item.m_customData.Remove(VfxStateKeys.OrbitalsOrbsXRotation);
            item.m_customData.Remove(VfxStateKeys.OrbitalsOrbsYRotation);
            item.m_customData.Remove(VfxStateKeys.OrbitalsOrbsZRotation);

            item.m_customData.Remove(VfxStateKeys.OrbitalsStrandsEnabled);
            item.m_customData.Remove(VfxStateKeys.OrbitalsStrandsSpectrumEnabled);
            item.m_customData.Remove(VfxStateKeys.OrbitalsStrandsEnergy);
            item.m_customData.Remove(VfxStateKeys.OrbitalsStrandsDrift);
            item.m_customData.Remove(VfxStateKeys.OrbitalsStrandsScaleWhole);
            item.m_customData.Remove(VfxStateKeys.OrbitalsStrandsScaleParts);
            item.m_customData.Remove(VfxStateKeys.OrbitalsStrandsHue);
            item.m_customData.Remove(VfxStateKeys.OrbitalsStrandsSpectrumSpeed);
            item.m_customData.Remove(VfxStateKeys.OrbitalsStrandsSpeed);
            item.m_customData.Remove(VfxStateKeys.OrbitalsStrandsLength);
            item.m_customData.Remove(VfxStateKeys.OrbitalsStrandsRadius);
            item.m_customData.Remove(VfxStateKeys.OrbitalsStrandsLifetime);
            item.m_customData.Remove(VfxStateKeys.OrbitalsStrandsXOffset);
            item.m_customData.Remove(VfxStateKeys.OrbitalsStrandsYOffset);
            item.m_customData.Remove(VfxStateKeys.OrbitalsStrandsZOffset);
            item.m_customData.Remove(VfxStateKeys.OrbitalsStrandsXRotation);
            item.m_customData.Remove(VfxStateKeys.OrbitalsStrandsYRotation);
            item.m_customData.Remove(VfxStateKeys.OrbitalsStrandsZRotation);

            item.m_customData.Remove(VfxStateKeys.OrbitalsFlamesEnabled);
            item.m_customData.Remove(VfxStateKeys.OrbitalsFlamesCount);
            item.m_customData.Remove(VfxStateKeys.OrbitalsFlamesEnergy);
            item.m_customData.Remove(VfxStateKeys.OrbitalsFlamesDrift);
            item.m_customData.Remove(VfxStateKeys.OrbitalsFlamesHue);
            item.m_customData.Remove(VfxStateKeys.OrbitalsFlamesSpeed);
            item.m_customData.Remove(VfxStateKeys.OrbitalsFlamesSpacing);
            item.m_customData.Remove(VfxStateKeys.OrbitalsFlamesLength);
            item.m_customData.Remove(VfxStateKeys.OrbitalsFlamesRadius);
            item.m_customData.Remove(VfxStateKeys.OrbitalsFlamesCycles);
            item.m_customData.Remove(VfxStateKeys.OrbitalsFlamesXOffset);
            item.m_customData.Remove(VfxStateKeys.OrbitalsFlamesYOffset);
            item.m_customData.Remove(VfxStateKeys.OrbitalsFlamesZOffset);
            item.m_customData.Remove(VfxStateKeys.OrbitalsFlamesXRotation);
            item.m_customData.Remove(VfxStateKeys.OrbitalsFlamesYRotation);
            item.m_customData.Remove(VfxStateKeys.OrbitalsFlamesZRotation);

            item.m_customData.Remove(VfxStateKeys.OrbitalsEmbersEnabled);
            item.m_customData.Remove(VfxStateKeys.OrbitalsEmbersCount);
            item.m_customData.Remove(VfxStateKeys.OrbitalsEmbersEnergy);
            item.m_customData.Remove(VfxStateKeys.OrbitalsEmbersDrift);
            item.m_customData.Remove(VfxStateKeys.OrbitalsEmbersHue);
            item.m_customData.Remove(VfxStateKeys.OrbitalsEmbersSpeed);
            item.m_customData.Remove(VfxStateKeys.OrbitalsEmbersSpacing);
            item.m_customData.Remove(VfxStateKeys.OrbitalsEmbersLength);
            item.m_customData.Remove(VfxStateKeys.OrbitalsEmbersRadius);
            item.m_customData.Remove(VfxStateKeys.OrbitalsEmbersCycles);
            item.m_customData.Remove(VfxStateKeys.OrbitalsEmbersLifetime);
            item.m_customData.Remove(VfxStateKeys.OrbitalsEmbersXOffset);
            item.m_customData.Remove(VfxStateKeys.OrbitalsEmbersYOffset);
            item.m_customData.Remove(VfxStateKeys.OrbitalsEmbersZOffset);
            item.m_customData.Remove(VfxStateKeys.OrbitalsEmbersXRotation);
            item.m_customData.Remove(VfxStateKeys.OrbitalsEmbersYRotation);
            item.m_customData.Remove(VfxStateKeys.OrbitalsEmbersZRotation);
        }

        internal static VfxState FromDefaults()
        {
            return new VfxState
            {
                RigXOffset = PluginConfig.DefaultEffectOffset,
                RigYOffset = PluginConfig.DefaultEffectOffset,
                RigZOffset = PluginConfig.DefaultEffectOffset,
                RigXRotation = PluginConfig.DefaultEffectRotation,
                RigYRotation = PluginConfig.DefaultEffectRotation,
                RigZRotation = PluginConfig.DefaultEffectRotation,

                InnerFlamesEnabled = true,
                InnerFlamesEnergy = PluginConfig.DefaultEnergy,
                InnerFlamesScale = 1f,
                InnerFlamesLength = PluginConfig.DefaultFlameLength,
                InnerFlamesHue = PluginConfig.DefaultHue,
                InnerFlamesXOffset = PluginConfig.DefaultEffectOffset,
                InnerFlamesYOffset = PluginConfig.DefaultEffectOffset,
                InnerFlamesZOffset = PluginConfig.DefaultEffectOffset,
                InnerFlamesXRotation = PluginConfig.DefaultEffectRotation,
                InnerFlamesYRotation = PluginConfig.DefaultEffectRotation,
                InnerFlamesZRotation = PluginConfig.DefaultEffectRotation,

                OuterFlamesEnabled = true,
                OuterFlamesDragEnabled = false,
                OuterFlamesEnergy = PluginConfig.DefaultEnergy,
                OuterFlamesScale = 1f,
                OuterFlamesLength = PluginConfig.DefaultFlameLength,
                OuterFlamesHue = PluginConfig.DefaultHue,
                OuterFlamesXOffset = PluginConfig.DefaultEffectOffset,
                OuterFlamesYOffset = PluginConfig.DefaultEffectOffset,
                OuterFlamesZOffset = PluginConfig.DefaultEffectOffset,
                OuterFlamesXRotation = PluginConfig.DefaultEffectRotation,
                OuterFlamesYRotation = PluginConfig.DefaultEffectRotation,
                OuterFlamesZRotation = PluginConfig.DefaultEffectRotation,

                SparksEnabled = true,
                SparksEnergy = PluginConfig.DefaultEnergy,
                SparksScale = 1f,
                SparksLength = PluginConfig.DefaultFlameLength,
                SparksWidth = PluginConfig.DefaultSparksWidth,
                SparksHue = PluginConfig.DefaultHue,
                SparksXOffset = PluginConfig.DefaultEffectOffset,
                SparksYOffset = PluginConfig.DefaultEffectOffset,
                SparksZOffset = PluginConfig.DefaultEffectOffset,
                SparksXRotation = PluginConfig.DefaultEffectRotation,
                SparksYRotation = PluginConfig.DefaultEffectRotation,
                SparksZRotation = PluginConfig.DefaultEffectRotation,
                
                FlareEnabled = true,
                FlareScale = 1f,
                FlareHue = PluginConfig.DefaultHue,
                FlareXOffset = PluginConfig.DefaultEffectOffset,
                FlareYOffset = PluginConfig.DefaultEffectOffset,
                FlareZOffset = PluginConfig.DefaultEffectOffset,

                AuraEnabled = true,
                AuraScale = PluginConfig.DefaultAuraScale,
                AuraHue = PluginConfig.DefaultHue,
                AuraXOffset = PluginConfig.DefaultEffectOffset,
                AuraYOffset = PluginConfig.DefaultEffectOffset,
                AuraZOffset = PluginConfig.DefaultEffectOffset,
                AuraXRotation = PluginConfig.DefaultEffectRotation,
                AuraYRotation = PluginConfig.DefaultEffectRotation,
                AuraZRotation = PluginConfig.DefaultEffectRotation,

                OrbitalsOrbsEnabled = true,
                OrbitalsOrbsCount = PluginConfig.DefaultCountNormalized,
                OrbitalsOrbsDrift = PluginConfig.DefaultDrift,
                OrbitalsOrbsScale = 1f,
                OrbitalsOrbsHue = PluginConfig.DefaultHue,
                OrbitalsOrbsSpeed = PluginConfig.DefaultOrbitalsSpeed,
                OrbitalsOrbsSpacing = PluginConfig.DefaultOrbitalsSpacing,
                OrbitalsOrbsLength = PluginConfig.DefaultOrbitalsLength,
                OrbitalsOrbsRadius = PluginConfig.DefaultOrbitalsRadiusMultiplier,
                OrbitalsOrbsCycles = PluginConfig.DefaultOrbitalsCycles,
                OrbitalsOrbsXOffset = PluginConfig.DefaultEffectOffset,
                OrbitalsOrbsYOffset = PluginConfig.DefaultEffectOffset,
                OrbitalsOrbsZOffset = PluginConfig.DefaultEffectOffset,
                OrbitalsOrbsXRotation = PluginConfig.DefaultEffectRotation,
                OrbitalsOrbsYRotation = PluginConfig.DefaultEffectRotation,
                OrbitalsOrbsZRotation = PluginConfig.DefaultEffectRotation,

                OrbitalsStrandsEnabled = true,
                OrbitalsStrandsSpectrumEnabled = false,
                OrbitalsStrandsEnergy = PluginConfig.DefaultEnergy,
                OrbitalsStrandsDrift = PluginConfig.DefaultDrift,
                OrbitalsStrandsScaleWhole = 0.50f,
                OrbitalsStrandsScaleParts = 1f,
                OrbitalsStrandsHue = PluginConfig.DefaultHue,
                OrbitalsStrandsSpectrumSpeed = PluginConfig.DefaultSpectrumSpeed,
                OrbitalsStrandsSpeed = PluginConfig.DefaultOrbitalsSpeed,
                OrbitalsStrandsLength = PluginConfig.DefaultOrbitalsLength,
                OrbitalsStrandsRadius = PluginConfig.DefaultOrbitalsRadiusMultiplier,
                OrbitalsStrandsLifetime = PluginConfig.DefaultLifetime,
                OrbitalsStrandsXOffset = PluginConfig.DefaultEffectOffset,
                OrbitalsStrandsYOffset = PluginConfig.DefaultEffectOffset,
                OrbitalsStrandsZOffset = PluginConfig.DefaultEffectOffset,
                OrbitalsStrandsXRotation = PluginConfig.DefaultEffectRotation,
                OrbitalsStrandsYRotation = PluginConfig.DefaultEffectRotation,
                OrbitalsStrandsZRotation = PluginConfig.DefaultEffectRotation,

                OrbitalsFlamesEnabled = true,
                OrbitalsFlamesCount = PluginConfig.DefaultCountNormalized,
                OrbitalsFlamesEnergy = PluginConfig.DefaultEnergy,
                OrbitalsFlamesDrift = PluginConfig.DefaultDrift,
                OrbitalsFlamesHue = PluginConfig.DefaultHue,
                OrbitalsFlamesSpeed = PluginConfig.DefaultOrbitalsSpeed,
                OrbitalsFlamesSpacing = PluginConfig.DefaultOrbitalsSpacing,
                OrbitalsFlamesLength = PluginConfig.DefaultOrbitalsLength,
                OrbitalsFlamesRadius = PluginConfig.DefaultOrbitalsRadiusMultiplier,
                OrbitalsFlamesCycles = PluginConfig.DefaultOrbitalsCycles,
                OrbitalsFlamesXOffset = PluginConfig.DefaultEffectOffset,
                OrbitalsFlamesYOffset = PluginConfig.DefaultEffectOffset,
                OrbitalsFlamesZOffset = PluginConfig.DefaultEffectOffset,
                OrbitalsFlamesXRotation = PluginConfig.DefaultEffectRotation,
                OrbitalsFlamesYRotation = PluginConfig.DefaultEffectRotation,
                OrbitalsFlamesZRotation = PluginConfig.DefaultEffectRotation,
                
                OrbitalsEmbersEnabled = true,
                OrbitalsEmbersCount = PluginConfig.DefaultCountNormalized,
                OrbitalsEmbersEnergy = PluginConfig.DefaultEnergy,
                OrbitalsEmbersDrift = PluginConfig.DefaultDrift,
                OrbitalsEmbersHue = PluginConfig.DefaultHue,
                OrbitalsEmbersSpeed = PluginConfig.DefaultOrbitalsSpeed,
                OrbitalsEmbersSpacing = PluginConfig.DefaultOrbitalsSpacing,
                OrbitalsEmbersLength = PluginConfig.DefaultOrbitalsLength,
                OrbitalsEmbersRadius = PluginConfig.DefaultOrbitalsRadiusMultiplier,
                OrbitalsEmbersCycles = PluginConfig.DefaultOrbitalsCycles,
                OrbitalsEmbersLifetime = PluginConfig.DefaultLifetime,
                OrbitalsEmbersXOffset = PluginConfig.DefaultEffectOffset,
                OrbitalsEmbersYOffset = PluginConfig.DefaultEffectOffset,
                OrbitalsEmbersZOffset = PluginConfig.DefaultEffectOffset,
                OrbitalsEmbersXRotation = PluginConfig.DefaultEffectRotation,
                OrbitalsEmbersYRotation = PluginConfig.DefaultEffectRotation,
                OrbitalsEmbersZRotation = PluginConfig.DefaultEffectRotation,
            };
        }

        internal static void ApplyToConfig(VfxState state)
        {
            PluginConfig.RigXOffset.Value = state.RigXOffset;
            PluginConfig.RigYOffset.Value = state.RigYOffset;
            PluginConfig.RigZOffset.Value = state.RigZOffset;
            PluginConfig.RigXRotation.Value = state.RigXRotation;
            PluginConfig.RigYRotation.Value = state.RigYRotation;
            PluginConfig.RigZRotation.Value = state.RigZRotation;

            PluginConfig.InnerFlames.Value = state.InnerFlamesEnabled;
            PluginConfig.InnerFlamesEnergy.Value = state.InnerFlamesEnergy;
            PluginConfig.InnerFlamesScale.Value = state.InnerFlamesScale;
            PluginConfig.InnerFlamesLength.Value = state.InnerFlamesLength;
            PluginConfig.InnerFlamesHue.Value = state.InnerFlamesHue;
            PluginConfig.InnerFlamesXOffset.Value = state.InnerFlamesXOffset;
            PluginConfig.InnerFlamesYOffset.Value = state.InnerFlamesYOffset;
            PluginConfig.InnerFlamesZOffset.Value = state.InnerFlamesZOffset;
            PluginConfig.InnerFlamesXRotation.Value = state.InnerFlamesXRotation;
            PluginConfig.InnerFlamesYRotation.Value = state.InnerFlamesYRotation;
            PluginConfig.InnerFlamesZRotation.Value = state.InnerFlamesZRotation;

            PluginConfig.OuterFlames.Value = state.OuterFlamesEnabled;
            PluginConfig.OuterFlamesDragEnabled.Value = state.OuterFlamesDragEnabled;
            PluginConfig.OuterFlamesEnergy.Value = state.OuterFlamesEnergy;
            PluginConfig.OuterFlamesScale.Value = state.OuterFlamesScale;
            PluginConfig.OuterFlamesLength.Value = state.OuterFlamesLength;
            PluginConfig.OuterFlamesHue.Value = state.OuterFlamesHue;
            PluginConfig.OuterFlamesXOffset.Value = state.OuterFlamesXOffset;
            PluginConfig.OuterFlamesYOffset.Value = state.OuterFlamesYOffset;
            PluginConfig.OuterFlamesZOffset.Value = state.OuterFlamesZOffset;
            PluginConfig.OuterFlamesXRotation.Value = state.OuterFlamesXRotation;
            PluginConfig.OuterFlamesYRotation.Value = state.OuterFlamesYRotation;
            PluginConfig.OuterFlamesZRotation.Value = state.OuterFlamesZRotation;

            PluginConfig.Sparks.Value = state.SparksEnabled;
            PluginConfig.SparksEnergy.Value = state.SparksEnergy;
            PluginConfig.SparksScale.Value = state.SparksScale;
            PluginConfig.SparksLength.Value = state.SparksLength;
            PluginConfig.SparksWidth.Value = state.SparksWidth;
            PluginConfig.SparksHue.Value = state.SparksHue;
            PluginConfig.SparksXOffset.Value = state.SparksXOffset;
            PluginConfig.SparksYOffset.Value = state.SparksYOffset;
            PluginConfig.SparksZOffset.Value = state.SparksZOffset;
            PluginConfig.SparksXRotation.Value = state.SparksXRotation;
            PluginConfig.SparksYRotation.Value = state.SparksYRotation;
            PluginConfig.SparksZRotation.Value = state.SparksZRotation;

            PluginConfig.Flare.Value = state.FlareEnabled;
            PluginConfig.FlareScale.Value = state.FlareScale;
            PluginConfig.FlareHue.Value = state.FlareHue;
            PluginConfig.FlareXOffset.Value = state.FlareXOffset;
            PluginConfig.FlareYOffset.Value = state.FlareYOffset;
            PluginConfig.FlareZOffset.Value = state.FlareZOffset;

            PluginConfig.Aura.Value = state.AuraEnabled;
            PluginConfig.AuraScale.Value = state.AuraScale;
            PluginConfig.AuraHue.Value = state.AuraHue;
            PluginConfig.AuraXOffset.Value = state.AuraXOffset;
            PluginConfig.AuraYOffset.Value = state.AuraYOffset;
            PluginConfig.AuraZOffset.Value = state.AuraZOffset;
            PluginConfig.AuraXRotation.Value = state.AuraXRotation;
            PluginConfig.AuraYRotation.Value = state.AuraYRotation;
            PluginConfig.AuraZRotation.Value = state.AuraZRotation;

            PluginConfig.OrbitalsOrbs.Value = state.OrbitalsOrbsEnabled;
            PluginConfig.OrbitalsOrbsCount.Value = state.OrbitalsOrbsCount;
            PluginConfig.OrbitalsOrbsDrift.Value = state.OrbitalsOrbsDrift;
            PluginConfig.OrbitalsOrbsScale.Value = state.OrbitalsOrbsScale;
            PluginConfig.OrbitalsOrbsHue.Value = state.OrbitalsOrbsHue;
            PluginConfig.OrbitalsOrbsSpeed.Value = state.OrbitalsOrbsSpeed;
            PluginConfig.OrbitalsOrbsSpacing.Value = state.OrbitalsOrbsSpacing;
            PluginConfig.OrbitalsOrbsLength.Value = state.OrbitalsOrbsLength;
            PluginConfig.OrbitalsOrbsRadius.Value = state.OrbitalsOrbsRadius;
            PluginConfig.OrbitalsOrbsCycles.Value = state.OrbitalsOrbsCycles;
            PluginConfig.OrbitalsOrbsXOffset.Value = state.OrbitalsOrbsXOffset;
            PluginConfig.OrbitalsOrbsYOffset.Value = state.OrbitalsOrbsYOffset;
            PluginConfig.OrbitalsOrbsZOffset.Value = state.OrbitalsOrbsZOffset;
            PluginConfig.OrbitalsOrbsXRotation.Value = state.OrbitalsOrbsXRotation;
            PluginConfig.OrbitalsOrbsYRotation.Value = state.OrbitalsOrbsYRotation;
            PluginConfig.OrbitalsOrbsZRotation.Value = state.OrbitalsOrbsZRotation;

            PluginConfig.OrbitalsStrands.Value = state.OrbitalsStrandsEnabled;
            PluginConfig.OrbitalsStrandsSpectrum.Value = state.OrbitalsStrandsSpectrumEnabled;
            PluginConfig.OrbitalsStrandsEnergy.Value = state.OrbitalsStrandsEnergy;
            PluginConfig.OrbitalsStrandsDrift.Value = state.OrbitalsStrandsDrift;
            PluginConfig.OrbitalsStrandsScaleWhole.Value = state.OrbitalsStrandsScaleWhole;
            PluginConfig.OrbitalsStrandsScaleParts.Value = state.OrbitalsStrandsScaleParts;
            PluginConfig.OrbitalsStrandsHue.Value = state.OrbitalsStrandsHue;
            PluginConfig.OrbitalsStrandsSpectrumSpeed.Value = state.OrbitalsStrandsSpectrumSpeed;
            PluginConfig.OrbitalsStrandsSpeed.Value = state.OrbitalsStrandsSpeed;
            PluginConfig.OrbitalsStrandsLength.Value = state.OrbitalsStrandsLength;
            PluginConfig.OrbitalsStrandsRadius.Value = state.OrbitalsStrandsRadius;
            PluginConfig.OrbitalsStrandsLifetime.Value = state.OrbitalsStrandsLifetime;
            PluginConfig.OrbitalsStrandsXOffset.Value = state.OrbitalsStrandsXOffset;
            PluginConfig.OrbitalsStrandsYOffset.Value = state.OrbitalsStrandsYOffset;
            PluginConfig.OrbitalsStrandsZOffset.Value = state.OrbitalsStrandsZOffset;
            PluginConfig.OrbitalsStrandsXRotation.Value = state.OrbitalsStrandsXRotation;
            PluginConfig.OrbitalsStrandsYRotation.Value = state.OrbitalsStrandsYRotation;
            PluginConfig.OrbitalsStrandsZRotation.Value = state.OrbitalsStrandsZRotation;

            PluginConfig.OrbitalsFlames.Value = state.OrbitalsFlamesEnabled;
            PluginConfig.OrbitalsFlamesCount.Value = state.OrbitalsFlamesCount;
            PluginConfig.OrbitalsFlamesEnergy.Value = state.OrbitalsFlamesEnergy;
            PluginConfig.OrbitalsFlamesDrift.Value = state.OrbitalsFlamesDrift;
            PluginConfig.OrbitalsFlamesHue.Value = state.OrbitalsFlamesHue;
            PluginConfig.OrbitalsFlamesSpeed.Value = state.OrbitalsFlamesSpeed;
            PluginConfig.OrbitalsFlamesSpacing.Value = state.OrbitalsFlamesSpacing;
            PluginConfig.OrbitalsFlamesLength.Value = state.OrbitalsFlamesLength;
            PluginConfig.OrbitalsFlamesRadius.Value = state.OrbitalsFlamesRadius;
            PluginConfig.OrbitalsFlamesCycles.Value = state.OrbitalsFlamesCycles;
            PluginConfig.OrbitalsFlamesXOffset.Value = state.OrbitalsFlamesXOffset;
            PluginConfig.OrbitalsFlamesYOffset.Value = state.OrbitalsFlamesYOffset;
            PluginConfig.OrbitalsFlamesZOffset.Value = state.OrbitalsFlamesZOffset;
            PluginConfig.OrbitalsFlamesXRotation.Value = state.OrbitalsFlamesXRotation;
            PluginConfig.OrbitalsFlamesYRotation.Value = state.OrbitalsFlamesYRotation;
            PluginConfig.OrbitalsFlamesZRotation.Value = state.OrbitalsFlamesZRotation;

            PluginConfig.OrbitalsEmbers.Value = state.OrbitalsEmbersEnabled;
            PluginConfig.OrbitalsEmbersCount.Value = state.OrbitalsEmbersCount;
            PluginConfig.OrbitalsEmbersEnergy.Value = state.OrbitalsEmbersEnergy;
            PluginConfig.OrbitalsEmbersDrift.Value = state.OrbitalsEmbersDrift;
            PluginConfig.OrbitalsEmbersHue.Value = state.OrbitalsEmbersHue;
            PluginConfig.OrbitalsEmbersSpeed.Value = state.OrbitalsEmbersSpeed;
            PluginConfig.OrbitalsEmbersSpacing.Value = state.OrbitalsEmbersSpacing;
            PluginConfig.OrbitalsEmbersLength.Value = state.OrbitalsEmbersLength;
            PluginConfig.OrbitalsEmbersRadius.Value = state.OrbitalsEmbersRadius;
            PluginConfig.OrbitalsEmbersCycles.Value = state.OrbitalsEmbersCycles;
            PluginConfig.OrbitalsEmbersLifetime.Value = state.OrbitalsEmbersLifetime;
            PluginConfig.OrbitalsEmbersXOffset.Value = state.OrbitalsEmbersXOffset;
            PluginConfig.OrbitalsEmbersYOffset.Value = state.OrbitalsEmbersYOffset;
            PluginConfig.OrbitalsEmbersZOffset.Value = state.OrbitalsEmbersZOffset;
            PluginConfig.OrbitalsEmbersXRotation.Value = state.OrbitalsEmbersXRotation;
            PluginConfig.OrbitalsEmbersYRotation.Value = state.OrbitalsEmbersYRotation;
            PluginConfig.OrbitalsEmbersZRotation.Value = state.OrbitalsEmbersZRotation;
        }

        private static float ReadFloat(Dictionary<string, string> customData, string key, float fallback)
        {
            if (customData == null)
                return fallback;

            if (!customData.TryGetValue(key, out string stringValue) || string.IsNullOrWhiteSpace(stringValue))
                return fallback;

            if (float.TryParse(
                    stringValue,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out float parsedValue))
            {
                return parsedValue;
            }

            return fallback;
        }

        private static bool ReadBool(Dictionary<string, string> customData, string key, bool fallback)
        {
            if (customData == null)
                return fallback;

            if (!customData.TryGetValue(key, out string stringValue) || string.IsNullOrWhiteSpace(stringValue))
                return fallback;

            if (bool.TryParse(stringValue, out bool parsedValue))
                return parsedValue;

            return fallback;
        }

        private static void WriteFloat(Dictionary<string, string> customData, string key, float value)
        {
            if (customData == null)
                return;

            customData[key] = value.ToString(CultureInfo.InvariantCulture);
        }

        private static void WriteBool(Dictionary<string, string> customData, string key, bool value)
        {
            if (customData == null)
                return;

            customData[key] = value ? "true" : "false";
        }

        private static void BackfillMissing(Dictionary<string, string> customData, string key, float value)
        {
            if (customData == null || customData.ContainsKey(key))
                return;

            customData[key] = value.ToString(CultureInfo.InvariantCulture);
        }

        private static void BackfillMissing(Dictionary<string, string> customData, string key, bool value)
        {
            if (customData == null || customData.ContainsKey(key))
                return;

            customData[key] = value ? "true" : "false";
        }

        internal static bool IsBound(global::ItemDrop.ItemData item)
        {
            if (item?.m_customData == null)
                return false;

            return ReadBool(
                item.m_customData,
                VfxStateKeys.Bound,
                false);
        }
        
        internal static VfxStyleSave ToStyleSave(string styleName, VfxState state)
        {
            var tempItem = new global::ItemDrop.ItemData
            {
                m_customData = new Dictionary<string, string>()
            };

            Write(tempItem, state, bound: false);

            tempItem.m_customData.Remove(VfxStateKeys.Bound);

            var save = new VfxStyleSave
            {
                Version = 1,
                Name = styleName
            };

            foreach (var pair in tempItem.m_customData)
            {
                save.Entries.Add(new VfxStyleEntry(pair.Key, pair.Value));
            }

            return save;
        }

        internal static VfxState FromStyleSave(VfxStyleSave save)
        {
            VfxState fallbackState = FromDefaults();

            if (save == null || save.Entries == null)
                return fallbackState;

            var tempItem = new global::ItemDrop.ItemData
            {
                m_customData = new Dictionary<string, string>()
            };

            // First write defaults so missing/new fields are safe.
            Write(tempItem, fallbackState, bound: false);

            // Then overwrite only the fields this style actually saved.
            foreach (VfxStyleEntry entry in save.Entries)
            {
                if (entry == null)
                    continue;

                if (string.IsNullOrWhiteSpace(entry.Key))
                    continue;

                tempItem.m_customData[entry.Key] = entry.Value;
            }

            if (TryRead(tempItem, out VfxState loadedState))
                return loadedState;

            return fallbackState;
        }
    }
}