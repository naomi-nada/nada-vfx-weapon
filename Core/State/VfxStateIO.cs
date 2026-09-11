using System.Collections.Generic;
using System.Globalization;
using System.Runtime.ExceptionServices;
using NADA.VFX.Weapon.Core.Config;

namespace NADA.VFX.Weapon.Core.State
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
                InnerFlamesWorldEnabled = PluginConfig.InnerFlamesWorld.Value,
                InnerFlamesBlackEnabled = PluginConfig.InnerFlamesBlack.Value,
                InnerFlamesWhiteEnabled = PluginConfig.InnerFlamesWhite.Value,
                InnerFlamesEnergy = PluginConfig.InnerFlamesEnergy.Value,
                InnerFlamesScale = PluginConfig.InnerFlamesScale.Value,
                InnerFlamesLuminance = PluginConfig.InnerFlamesLuminance.Value,
                InnerFlamesHue = PluginConfig.InnerFlamesHue.Value,
                InnerFlamesLifetime = PluginConfig.InnerFlamesLifetime.Value,
                InnerFlamesSimulationSpeed =  PluginConfig.InnerFlamesSimulationSpeed.Value,
                InnerFlamesLength = PluginConfig.InnerFlamesLength.Value,
                InnerFlamesWidth = PluginConfig.InnerFlamesWidth.Value,
                InnerFlamesXOffset = PluginConfig.InnerFlamesXOffset.Value,
                InnerFlamesYOffset = PluginConfig.InnerFlamesYOffset.Value,
                InnerFlamesZOffset = PluginConfig.InnerFlamesZOffset.Value,
                InnerFlamesXRotation = PluginConfig.InnerFlamesXRotation.Value,
                InnerFlamesYRotation = PluginConfig.InnerFlamesYRotation.Value,
                InnerFlamesZRotation = PluginConfig.InnerFlamesZRotation.Value,

                OuterFlamesEnabled = PluginConfig.OuterFlames.Value,
                OuterFlamesWorldEnabled = PluginConfig.OuterFlamesWorld.Value,
                OuterFlamesBlackEnabled = PluginConfig.OuterFlamesBlack.Value,
                OuterFlamesWhiteEnabled = PluginConfig.OuterFlamesWhite.Value,
                OuterFlamesDragEnabled = PluginConfig.OuterFlamesDragEnabled.Value,
                OuterFlamesEnergy = PluginConfig.OuterFlamesEnergy.Value,
                OuterFlamesScale = PluginConfig.OuterFlamesScale.Value,
                OuterFlamesLuminance = PluginConfig.OuterFlamesLuminance.Value,
                OuterFlamesHue = PluginConfig.OuterFlamesHue.Value,
                OuterFlamesLifetime = PluginConfig.OuterFlamesLifetime.Value,
                OuterFlamesSimulationSpeed = PluginConfig.OuterFlamesSimulationSpeed.Value,
                OuterFlamesLength = PluginConfig.OuterFlamesLength.Value,
                OuterFlamesWidth = PluginConfig.OuterFlamesWidth.Value,
                OuterFlamesXOffset = PluginConfig.OuterFlamesXOffset.Value,
                OuterFlamesYOffset = PluginConfig.OuterFlamesYOffset.Value,
                OuterFlamesZOffset = PluginConfig.OuterFlamesZOffset.Value,
                OuterFlamesXRotation = PluginConfig.OuterFlamesXRotation.Value,
                OuterFlamesYRotation = PluginConfig.OuterFlamesYRotation.Value,
                OuterFlamesZRotation = PluginConfig.OuterFlamesZRotation.Value,

                StrandsEnabled = PluginConfig.Strands.Value,
                StrandsSpectrumEnabled = PluginConfig.StrandsSpectrum.Value,
                StrandsEnergy = PluginConfig.StrandsEnergy.Value,
                StrandsScaleWhole = PluginConfig.StrandsScaleWhole.Value,
                StrandsScaleParts = PluginConfig.StrandsScaleParts.Value,
                StrandsLuminance = PluginConfig.StrandsLuminance.Value,
                StrandsHue = PluginConfig.StrandsHue.Value,
                StrandsLifetime = PluginConfig.StrandsLifetime.Value,
                StrandsLength = PluginConfig.StrandsLength.Value,
                StrandsSpectrumSpeed = PluginConfig.StrandsSpectrumSpeed.Value,
                StrandsSpeed = PluginConfig.StrandsSpeed.Value,
                StrandsRadius = PluginConfig.StrandsRadius.Value,
                StrandsXOffset = PluginConfig.StrandsXOffset.Value,
                StrandsYOffset = PluginConfig.StrandsYOffset.Value,
                StrandsZOffset = PluginConfig.StrandsZOffset.Value,
                StrandsXRotation = PluginConfig.StrandsXRotation.Value,
                StrandsYRotation = PluginConfig.StrandsYRotation.Value,
                StrandsZRotation = PluginConfig.StrandsZRotation.Value,
                StrandsDrift = PluginConfig.StrandsDrift.Value,

                SparksEnabled = PluginConfig.Sparks.Value,
                SparksEnergy = PluginConfig.SparksEnergy.Value,
                SparksScale = PluginConfig.SparksScale.Value,
                SparksLuminance = PluginConfig.SparksLuminance.Value,
                SparksHue = PluginConfig.SparksHue.Value,
                SparksLifetime = PluginConfig.SparksLifetime.Value,
                SparksSimulationSpeed =  PluginConfig.SparksSimulationSpeed.Value,
                SparksLength = PluginConfig.SparksLength.Value,
                SparksWidth = PluginConfig.SparksWidth.Value,
                SparksXOffset = PluginConfig.SparksXOffset.Value,
                SparksYOffset = PluginConfig.SparksYOffset.Value,
                SparksZOffset = PluginConfig.SparksZOffset.Value,
                SparksXRotation = PluginConfig.SparksXRotation.Value,
                SparksYRotation = PluginConfig.SparksYRotation.Value,
                SparksZRotation = PluginConfig.SparksZRotation.Value,

                FlareEnabled = PluginConfig.Flare.Value,
                FlareScale = PluginConfig.FlareScale.Value,
                FlareLuminance = PluginConfig.FlareLuminance.Value,
                FlareHue = PluginConfig.FlareHue.Value,
                FlareXOffset = PluginConfig.FlareXOffset.Value,
                FlareYOffset = PluginConfig.FlareYOffset.Value,
                FlareZOffset = PluginConfig.FlareZOffset.Value,

                AuraEnabled = PluginConfig.Aura.Value,
                AuraScale = PluginConfig.AuraScale.Value,
                AuraLuminance = PluginConfig.AuraLuminance.Value,
                AuraHue = PluginConfig.AuraHue.Value,
                AuraXOffset = PluginConfig.AuraXOffset.Value,
                AuraYOffset = PluginConfig.AuraYOffset.Value,
                AuraZOffset = PluginConfig.AuraZOffset.Value,
                AuraXRotation = PluginConfig.AuraXRotation.Value,
                AuraYRotation = PluginConfig.AuraYRotation.Value,
                AuraZRotation = PluginConfig.AuraZRotation.Value,

                OrbitalsOrbsEnabled = PluginConfig.OrbitalsOrbs.Value,
                OrbitalsOrbsSnakeEnabled = PluginConfig.OrbitalsOrbsSnake.Value,
                OrbitalsOrbsGlueEnabled = PluginConfig.OrbitalsOrbsGlue.Value,
                OrbitalsOrbsCount = PluginConfig.OrbitalsOrbsCount.Value,
                OrbitalsOrbsDrift = PluginConfig.OrbitalsOrbsDrift.Value,
                OrbitalsOrbsScale = PluginConfig.OrbitalsOrbsScale.Value,
                OrbitalsOrbsLuminance = PluginConfig.OrbitalsOrbsLuminance.Value,
                OrbitalsOrbsHue = PluginConfig.OrbitalsOrbsHue.Value,
                OrbitalsOrbsLength = PluginConfig.OrbitalsOrbsLength.Value,
                OrbitalsOrbsSpeed = PluginConfig.OrbitalsOrbsSpeed.Value,
                OrbitalsOrbsSpacing = PluginConfig.OrbitalsOrbsSpacing.Value,
                OrbitalsOrbsRadius = PluginConfig.OrbitalsOrbsRadius.Value,
                OrbitalsOrbsCycles = PluginConfig.OrbitalsOrbsCycles.Value,
                OrbitalsOrbsXOffset = PluginConfig.OrbitalsOrbsXOffset.Value,
                OrbitalsOrbsYOffset = PluginConfig.OrbitalsOrbsYOffset.Value,
                OrbitalsOrbsZOffset = PluginConfig.OrbitalsOrbsZOffset.Value,
                OrbitalsOrbsXRotation = PluginConfig.OrbitalsOrbsXRotation.Value,
                OrbitalsOrbsYRotation = PluginConfig.OrbitalsOrbsYRotation.Value,
                OrbitalsOrbsZRotation = PluginConfig.OrbitalsOrbsZRotation.Value,

                OrbitalsCoresEnabled = PluginConfig.OrbitalsCores.Value,
                OrbitalsCoresSnakeEnabled = PluginConfig.OrbitalsCoresSnake.Value,
                OrbitalsCoresGlueEnabled = PluginConfig.OrbitalsCoresGlue.Value,
                OrbitalsCoresSpinEnabled = PluginConfig.OrbitalsCoresSpin.Value,
                OrbitalsCoresCount = PluginConfig.OrbitalsCoresCount.Value,
                OrbitalsCoresScale = PluginConfig.OrbitalsCoresScale.Value,
                OrbitalsCoresLuminance = PluginConfig.OrbitalsCoresLuminance.Value,
                OrbitalsCoresHue = PluginConfig.OrbitalsCoresHue.Value,
                OrbitalsCoresSpinSpeed = PluginConfig.OrbitalsCoresSpinSpeed.Value,
                OrbitalsCoresLength = PluginConfig.OrbitalsCoresLength.Value,
                OrbitalsCoresSpeed = PluginConfig.OrbitalsCoresSpeed.Value,
                OrbitalsCoresSpacing = PluginConfig.OrbitalsCoresSpacing.Value,
                OrbitalsCoresRadius = PluginConfig.OrbitalsCoresRadius.Value,
                OrbitalsCoresCycles = PluginConfig.OrbitalsCoresCycles.Value,
                OrbitalsCoresXOffset = PluginConfig.OrbitalsCoresXOffset.Value,
                OrbitalsCoresYOffset = PluginConfig.OrbitalsCoresYOffset.Value,
                OrbitalsCoresZOffset = PluginConfig.OrbitalsCoresZOffset.Value,
                OrbitalsCoresXRotation = PluginConfig.OrbitalsCoresXRotation.Value,
                OrbitalsCoresYRotation = PluginConfig.OrbitalsCoresYRotation.Value,
                OrbitalsCoresZRotation = PluginConfig.OrbitalsCoresZRotation.Value,
                OrbitalsCoresDrift = PluginConfig.OrbitalsCoresDrift.Value,

                OrbitalsFlamesEnabled = PluginConfig.OrbitalsFlames.Value,
                OrbitalsFlamesCount = PluginConfig.OrbitalsFlamesCount.Value,
                OrbitalsFlamesEnergy = PluginConfig.OrbitalsFlamesEnergy.Value,
                OrbitalsFlamesScale = PluginConfig.OrbitalsFlamesScale.Value,
                OrbitalsFlamesLuminance = PluginConfig.OrbitalsFlamesLuminance.Value,
                OrbitalsFlamesHue = PluginConfig.OrbitalsFlamesHue.Value,
                OrbitalsFlamesLifetime = PluginConfig.OrbitalsFlamesLifetime.Value,
                OrbitalsFlamesSimulationSpeed =  PluginConfig.OrbitalsFlamesSimulationSpeed.Value,
                OrbitalsFlamesLength = PluginConfig.OrbitalsFlamesLength.Value,
                OrbitalsFlamesSpeed = PluginConfig.OrbitalsFlamesSpeed.Value,
                OrbitalsFlamesSpacing = PluginConfig.OrbitalsFlamesSpacing.Value,
                OrbitalsFlamesRadius = PluginConfig.OrbitalsFlamesRadius.Value,
                OrbitalsFlamesCycles = PluginConfig.OrbitalsFlamesCycles.Value,
                OrbitalsFlamesXOffset = PluginConfig.OrbitalsFlamesXOffset.Value,
                OrbitalsFlamesYOffset = PluginConfig.OrbitalsFlamesYOffset.Value,
                OrbitalsFlamesZOffset = PluginConfig.OrbitalsFlamesZOffset.Value,
                OrbitalsFlamesXRotation = PluginConfig.OrbitalsFlamesXRotation.Value,
                OrbitalsFlamesYRotation = PluginConfig.OrbitalsFlamesYRotation.Value,
                OrbitalsFlamesZRotation = PluginConfig.OrbitalsFlamesZRotation.Value,
                OrbitalsFlamesDrift = PluginConfig.OrbitalsFlamesDrift.Value,

                OrbitalsEmbersEnabled = PluginConfig.OrbitalsEmbers.Value,
                OrbitalsEmbersCount = PluginConfig.OrbitalsEmbersCount.Value,
                OrbitalsEmbersEnergy = PluginConfig.OrbitalsEmbersEnergy.Value,
                OrbitalsEmbersScale = PluginConfig.OrbitalsEmbersScale.Value,
                OrbitalsEmbersLuminance = PluginConfig.OrbitalsEmbersLuminance.Value,
                OrbitalsEmbersHue = PluginConfig.OrbitalsEmbersHue.Value,
                OrbitalsEmbersLifetime = PluginConfig.OrbitalsEmbersLifetime.Value,
                OrbitalsEmbersSimulationSpeed = PluginConfig.OrbitalsEmbersSimulationSpeed.Value,
                OrbitalsEmbersLength = PluginConfig.OrbitalsEmbersLength.Value,
                OrbitalsEmbersSpeed = PluginConfig.OrbitalsEmbersSpeed.Value,
                OrbitalsEmbersSpacing = PluginConfig.OrbitalsEmbersSpacing.Value,
                OrbitalsEmbersRadius = PluginConfig.OrbitalsEmbersRadius.Value,
                OrbitalsEmbersCycles = PluginConfig.OrbitalsEmbersCycles.Value,
                OrbitalsEmbersXOffset = PluginConfig.OrbitalsEmbersXOffset.Value,
                OrbitalsEmbersYOffset = PluginConfig.OrbitalsEmbersYOffset.Value,
                OrbitalsEmbersZOffset = PluginConfig.OrbitalsEmbersZOffset.Value,
                OrbitalsEmbersXRotation = PluginConfig.OrbitalsEmbersXRotation.Value,
                OrbitalsEmbersYRotation = PluginConfig.OrbitalsEmbersYRotation.Value,
                OrbitalsEmbersZRotation = PluginConfig.OrbitalsEmbersZRotation.Value,
                OrbitalsEmbersDrift = PluginConfig.OrbitalsEmbersDrift.Value
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
            WriteBool(customData, VfxStateKeys.InnerFlamesWorldEnabled, state.InnerFlamesWorldEnabled);
            WriteBool(customData, VfxStateKeys.InnerFlamesBlackEnabled, state.InnerFlamesBlackEnabled);
            WriteBool(customData, VfxStateKeys.InnerFlamesWhiteEnabled, state.InnerFlamesWhiteEnabled);
            WriteFloat(customData, VfxStateKeys.InnerFlamesEnergy, state.InnerFlamesEnergy);
            WriteFloat(customData, VfxStateKeys.InnerFlamesScale, state.InnerFlamesScale);
            WriteFloat(customData, VfxStateKeys.InnerFlamesLuminance, state.InnerFlamesLuminance);
            WriteFloat(customData, VfxStateKeys.InnerFlamesHue, state.InnerFlamesHue);
            WriteFloat(customData, VfxStateKeys.InnerFlamesLifetime, state.InnerFlamesLifetime);
            WriteFloat(customData, VfxStateKeys.InnerFlamesSimulationSpeed, state.InnerFlamesSimulationSpeed);
            WriteFloat(customData, VfxStateKeys.InnerFlamesLength, state.InnerFlamesLength);
            WriteFloat(customData, VfxStateKeys.InnerFlamesWidth, state.InnerFlamesWidth);
            WriteFloat(customData, VfxStateKeys.InnerFlamesXOffset, state.InnerFlamesXOffset);
            WriteFloat(customData, VfxStateKeys.InnerFlamesYOffset, state.InnerFlamesYOffset);
            WriteFloat(customData, VfxStateKeys.InnerFlamesZOffset, state.InnerFlamesZOffset);
            WriteFloat(customData, VfxStateKeys.InnerFlamesXRotation, state.InnerFlamesXRotation);
            WriteFloat(customData, VfxStateKeys.InnerFlamesYRotation, state.InnerFlamesYRotation);
            WriteFloat(customData, VfxStateKeys.InnerFlamesZRotation, state.InnerFlamesZRotation);

            WriteBool(customData, VfxStateKeys.OuterFlamesEnabled, state.OuterFlamesEnabled);
            WriteBool(customData, VfxStateKeys.OuterFlamesWorldEnabled, state.OuterFlamesWorldEnabled);
            WriteBool(customData, VfxStateKeys.OuterFlamesBlackEnabled, state.OuterFlamesBlackEnabled);
            WriteBool(customData, VfxStateKeys.OuterFlamesWhiteEnabled, state.OuterFlamesWhiteEnabled);
            WriteBool(customData, VfxStateKeys.OuterFlamesDragEnabled, state.OuterFlamesDragEnabled);
            WriteFloat(customData, VfxStateKeys.OuterFlamesEnergy, state.OuterFlamesEnergy);
            WriteFloat(customData, VfxStateKeys.OuterFlamesScale, state.OuterFlamesScale);
            WriteFloat(customData, VfxStateKeys.OuterFlamesLuminance, state.OuterFlamesLuminance);
            WriteFloat(customData, VfxStateKeys.OuterFlamesHue, state.OuterFlamesHue);
            WriteFloat(customData, VfxStateKeys.OuterFlamesLifetime, state.OuterFlamesLifetime);
            WriteFloat(customData, VfxStateKeys.OuterFlamesSimulationSpeed, state.OuterFlamesSimulationSpeed);
            WriteFloat(customData, VfxStateKeys.OuterFlamesLength, state.OuterFlamesLength);
            WriteFloat(customData, VfxStateKeys.OuterFlamesWidth, state.OuterFlamesWidth);
            WriteFloat(customData, VfxStateKeys.OuterFlamesXOffset, state.OuterFlamesXOffset);
            WriteFloat(customData, VfxStateKeys.OuterFlamesYOffset, state.OuterFlamesYOffset);
            WriteFloat(customData, VfxStateKeys.OuterFlamesZOffset, state.OuterFlamesZOffset);
            WriteFloat(customData, VfxStateKeys.OuterFlamesXRotation, state.OuterFlamesXRotation);
            WriteFloat(customData, VfxStateKeys.OuterFlamesYRotation, state.OuterFlamesYRotation);
            WriteFloat(customData, VfxStateKeys.OuterFlamesZRotation, state.OuterFlamesZRotation);

            WriteBool(customData, VfxStateKeys.StrandsEnabled, state.StrandsEnabled);
            WriteBool(customData, VfxStateKeys.StrandsSpectrumEnabled, state.StrandsSpectrumEnabled);
            WriteFloat(customData, VfxStateKeys.StrandsEnergy, state.StrandsEnergy);
            WriteFloat(customData, VfxStateKeys.StrandsScaleWhole, state.StrandsScaleWhole);
            WriteFloat(customData, VfxStateKeys.StrandsScaleParts, state.StrandsScaleParts);
            WriteFloat(customData, VfxStateKeys.StrandsLuminance, state.StrandsLuminance);
            WriteFloat(customData, VfxStateKeys.StrandsHue, state.StrandsHue);
            WriteFloat(customData, VfxStateKeys.StrandsLifetime, state.StrandsLifetime);
            WriteFloat(customData, VfxStateKeys.StrandsLength, state.StrandsLength);
            WriteFloat(customData, VfxStateKeys.StrandsSpectrumSpeed, state.StrandsSpectrumSpeed);
            WriteFloat(customData, VfxStateKeys.StrandsSpeed, state.StrandsSpeed);
            WriteFloat(customData, VfxStateKeys.StrandsRadius, state.StrandsRadius);
            WriteFloat(customData, VfxStateKeys.StrandsXOffset, state.StrandsXOffset);
            WriteFloat(customData, VfxStateKeys.StrandsYOffset, state.StrandsYOffset);
            WriteFloat(customData, VfxStateKeys.StrandsZOffset, state.StrandsZOffset);
            WriteFloat(customData, VfxStateKeys.StrandsXRotation, state.StrandsXRotation);
            WriteFloat(customData, VfxStateKeys.StrandsYRotation, state.StrandsYRotation);
            WriteFloat(customData, VfxStateKeys.StrandsZRotation, state.StrandsZRotation);
            WriteFloat(customData, VfxStateKeys.StrandsDrift, state.StrandsDrift);

            WriteBool(customData, VfxStateKeys.SparksEnabled, state.SparksEnabled);
            WriteFloat(customData, VfxStateKeys.SparksEnergy, state.SparksEnergy);
            WriteFloat(customData, VfxStateKeys.SparksScale, state.SparksScale);
            WriteFloat(customData, VfxStateKeys.SparksLuminance, state.SparksLuminance);
            WriteFloat(customData, VfxStateKeys.SparksHue, state.SparksHue);
            WriteFloat(customData, VfxStateKeys.SparksLifetime, state.SparksLifetime);
            WriteFloat(customData, VfxStateKeys.SparksSimulationSpeed, state.SparksSimulationSpeed);
            WriteFloat(customData, VfxStateKeys.SparksLength, state.SparksLength);
            WriteFloat(customData, VfxStateKeys.SparksWidth, state.SparksWidth);
            WriteFloat(customData, VfxStateKeys.SparksXOffset, state.SparksXOffset);
            WriteFloat(customData, VfxStateKeys.SparksYOffset, state.SparksYOffset);
            WriteFloat(customData, VfxStateKeys.SparksZOffset, state.SparksZOffset);
            WriteFloat(customData, VfxStateKeys.SparksXRotation, state.SparksXRotation);
            WriteFloat(customData, VfxStateKeys.SparksYRotation, state.SparksYRotation);
            WriteFloat(customData, VfxStateKeys.SparksZRotation, state.SparksZRotation);

            WriteBool(customData, VfxStateKeys.FlareEnabled, state.FlareEnabled);
            WriteFloat(customData, VfxStateKeys.FlareScale, state.FlareScale);
            WriteFloat(customData, VfxStateKeys.FlareLuminance, state.FlareLuminance);
            WriteFloat(customData, VfxStateKeys.FlareHue, state.FlareHue);
            WriteFloat(customData, VfxStateKeys.FlareXOffset, state.FlareXOffset);
            WriteFloat(customData, VfxStateKeys.FlareYOffset, state.FlareYOffset);
            WriteFloat(customData, VfxStateKeys.FlareZOffset, state.FlareZOffset);

            WriteBool(customData, VfxStateKeys.AuraEnabled, state.AuraEnabled);
            WriteFloat(customData, VfxStateKeys.AuraScale, state.AuraScale);
            WriteFloat(customData, VfxStateKeys.AuraLuminance, state.AuraLuminance);
            WriteFloat(customData, VfxStateKeys.AuraHue, state.AuraHue);
            WriteFloat(customData, VfxStateKeys.AuraXOffset, state.AuraXOffset);
            WriteFloat(customData, VfxStateKeys.AuraYOffset, state.AuraYOffset);
            WriteFloat(customData, VfxStateKeys.AuraZOffset, state.AuraZOffset);
            WriteFloat(customData, VfxStateKeys.AuraXRotation, state.AuraXRotation);
            WriteFloat(customData, VfxStateKeys.AuraYRotation, state.AuraYRotation);
            WriteFloat(customData, VfxStateKeys.AuraZRotation, state.AuraZRotation);

            WriteBool(customData, VfxStateKeys.OrbitalsOrbsEnabled, state.OrbitalsOrbsEnabled);
            WriteBool(customData, VfxStateKeys.OrbitalsOrbsGlueEnabled, state.OrbitalsOrbsGlueEnabled);
            WriteBool(customData, VfxStateKeys.OrbitalsOrbsSnakeEnabled, state.OrbitalsOrbsSnakeEnabled);
            WriteFloat(customData, VfxStateKeys.OrbitalsOrbsCount, state.OrbitalsOrbsCount);
            WriteFloat(customData, VfxStateKeys.OrbitalsOrbsScale, state.OrbitalsOrbsScale);
            WriteFloat(customData, VfxStateKeys.OrbitalsOrbsLuminance, state.OrbitalsOrbsLuminance);
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
            WriteFloat(customData, VfxStateKeys.OrbitalsOrbsDrift, state.OrbitalsOrbsDrift);

            WriteBool(customData, VfxStateKeys.OrbitalsCoresEnabled, state.OrbitalsCoresEnabled);
            WriteBool(customData, VfxStateKeys.OrbitalsCoresSnakeEnabled, state.OrbitalsCoresSnakeEnabled);
            WriteBool(customData, VfxStateKeys.OrbitalsCoresGlueEnabled, state.OrbitalsCoresGlueEnabled);
            WriteBool(customData, VfxStateKeys.OrbitalsCoresSpinEnabled, state.OrbitalsCoresSpinEnabled);
            WriteFloat(customData, VfxStateKeys.OrbitalsCoresCount, state.OrbitalsCoresCount);
            WriteFloat(customData, VfxStateKeys.OrbitalsCoresScale, state.OrbitalsCoresScale);
            WriteFloat(customData, VfxStateKeys.OrbitalsCoresLuminance, state.OrbitalsCoresLuminance);
            WriteFloat(customData, VfxStateKeys.OrbitalsCoresHue, state.OrbitalsCoresHue);
            WriteFloat(customData, VfxStateKeys.OrbitalsCoresSpinSpeed, state.OrbitalsCoresSpinSpeed);
            WriteFloat(customData, VfxStateKeys.OrbitalsCoresSpeed, state.OrbitalsCoresSpeed);
            WriteFloat(customData, VfxStateKeys.OrbitalsCoresSpacing, state.OrbitalsCoresSpacing);
            WriteFloat(customData, VfxStateKeys.OrbitalsCoresLength, state.OrbitalsCoresLength);
            WriteFloat(customData, VfxStateKeys.OrbitalsCoresRadius, state.OrbitalsCoresRadius);
            WriteFloat(customData, VfxStateKeys.OrbitalsCoresCycles, state.OrbitalsCoresCycles);
            WriteFloat(customData, VfxStateKeys.OrbitalsCoresXOffset, state.OrbitalsCoresXOffset);
            WriteFloat(customData, VfxStateKeys.OrbitalsCoresYOffset, state.OrbitalsCoresYOffset);
            WriteFloat(customData, VfxStateKeys.OrbitalsCoresZOffset, state.OrbitalsCoresZOffset);
            WriteFloat(customData, VfxStateKeys.OrbitalsCoresXRotation, state.OrbitalsCoresXRotation);
            WriteFloat(customData, VfxStateKeys.OrbitalsCoresYRotation, state.OrbitalsCoresYRotation);
            WriteFloat(customData, VfxStateKeys.OrbitalsCoresZRotation, state.OrbitalsCoresZRotation);
            WriteFloat(customData, VfxStateKeys.OrbitalsCoresDrift, state.OrbitalsCoresDrift);

            WriteBool(customData, VfxStateKeys.OrbitalsFlamesEnabled, state.OrbitalsFlamesEnabled);
            WriteFloat(customData, VfxStateKeys.OrbitalsFlamesCount, state.OrbitalsFlamesCount);
            WriteFloat(customData, VfxStateKeys.OrbitalsFlamesEnergy, state.OrbitalsFlamesEnergy);
            WriteFloat(customData, VfxStateKeys.OrbitalsFlamesScale, state.OrbitalsFlamesScale);
            WriteFloat(customData, VfxStateKeys.OrbitalsFlamesLuminance, state.OrbitalsFlamesLuminance);
            WriteFloat(customData, VfxStateKeys.OrbitalsFlamesHue, state.OrbitalsFlamesHue);
            WriteFloat(customData, VfxStateKeys.OrbitalsFlamesLifetime, state.OrbitalsFlamesLifetime);
            WriteFloat(customData, VfxStateKeys.OrbitalsFlamesSimulationSpeed, state.OrbitalsFlamesSimulationSpeed);
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
            WriteFloat(customData, VfxStateKeys.OrbitalsFlamesDrift, state.OrbitalsFlamesDrift);

            WriteBool(customData, VfxStateKeys.OrbitalsEmbersEnabled, state.OrbitalsEmbersEnabled);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersCount, state.OrbitalsEmbersCount);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersEnergy, state.OrbitalsEmbersEnergy);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersScale, state.OrbitalsEmbersScale);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersLuminance, state.OrbitalsEmbersLuminance);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersHue, state.OrbitalsEmbersHue);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersLifetime, state.OrbitalsEmbersLifetime);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersLength, state.OrbitalsEmbersLength);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersSpeed, state.OrbitalsEmbersSpeed);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersSpacing, state.OrbitalsEmbersSpacing);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersRadius, state.OrbitalsEmbersRadius);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersCycles, state.OrbitalsEmbersCycles);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersXOffset, state.OrbitalsEmbersXOffset);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersYOffset, state.OrbitalsEmbersYOffset);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersZOffset, state.OrbitalsEmbersZOffset);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersXRotation, state.OrbitalsEmbersXRotation);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersYRotation, state.OrbitalsEmbersYRotation);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersZRotation, state.OrbitalsEmbersZRotation);
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersDrift, state.OrbitalsEmbersDrift);
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
            state.InnerFlamesWorldEnabled = ReadBool(customData, VfxStateKeys.InnerFlamesWorldEnabled, PluginConfig.InnerFlamesWorld.Value);
            state.InnerFlamesBlackEnabled = ReadBool(customData, VfxStateKeys.InnerFlamesBlackEnabled, PluginConfig.InnerFlamesBlack.Value);
            state.InnerFlamesWhiteEnabled = ReadBool(customData, VfxStateKeys.InnerFlamesWhiteEnabled, PluginConfig.InnerFlamesWhite.Value);
            state.InnerFlamesEnergy = ReadFloat(customData, VfxStateKeys.InnerFlamesEnergy, PluginConfig.InnerFlamesEnergy.Value);
            state.InnerFlamesScale = ReadFloat(customData, VfxStateKeys.InnerFlamesScale, PluginConfig.InnerFlamesScale.Value);
            state.InnerFlamesLuminance = ReadFloat(customData, VfxStateKeys.InnerFlamesLuminance, PluginConfig.InnerFlamesLuminance.Value);
            state.InnerFlamesHue = ReadFloat(customData, VfxStateKeys.InnerFlamesHue, PluginConfig.InnerFlamesHue.Value);
            state.InnerFlamesLifetime = ReadFloat(customData, VfxStateKeys.InnerFlamesLifetime, PluginConfig.InnerFlamesLifetime.Value);
            state.InnerFlamesSimulationSpeed = ReadFloat(customData, VfxStateKeys.InnerFlamesSimulationSpeed, PluginConfig.InnerFlamesSimulationSpeed.Value);
            state.InnerFlamesLength = ReadFloat(customData, VfxStateKeys.InnerFlamesLength, PluginConfig.InnerFlamesLength.Value);
            state.InnerFlamesWidth = ReadFloat(customData, VfxStateKeys.InnerFlamesWidth, PluginConfig.InnerFlamesWidth.Value);
            state.InnerFlamesXOffset = ReadFloat(customData, VfxStateKeys.InnerFlamesXOffset, PluginConfig.InnerFlamesXOffset.Value);
            state.InnerFlamesYOffset = ReadFloat(customData, VfxStateKeys.InnerFlamesYOffset, PluginConfig.InnerFlamesYOffset.Value);
            state.InnerFlamesZOffset = ReadFloat(customData, VfxStateKeys.InnerFlamesZOffset, PluginConfig.InnerFlamesZOffset.Value);
            state.InnerFlamesXRotation = ReadFloat(customData, VfxStateKeys.InnerFlamesXRotation, PluginConfig.InnerFlamesXRotation.Value);
            state.InnerFlamesYRotation = ReadFloat(customData, VfxStateKeys.InnerFlamesYRotation, PluginConfig.InnerFlamesYRotation.Value);
            state.InnerFlamesZRotation = ReadFloat(customData, VfxStateKeys.InnerFlamesZRotation, PluginConfig.InnerFlamesZRotation.Value);

            state.OuterFlamesEnabled = ReadBool(customData, VfxStateKeys.OuterFlamesEnabled, PluginConfig.OuterFlames.Value);
            state.OuterFlamesWorldEnabled = ReadBool(customData, VfxStateKeys.OuterFlamesWorldEnabled, PluginConfig.OuterFlamesWorld.Value);
            state.OuterFlamesBlackEnabled = ReadBool(customData, VfxStateKeys.OuterFlamesBlackEnabled, PluginConfig.OuterFlamesBlack.Value);
            state.OuterFlamesWhiteEnabled = ReadBool(customData, VfxStateKeys.OuterFlamesWhiteEnabled, PluginConfig.OuterFlamesWhite.Value);
            state.OuterFlamesDragEnabled = ReadBool(customData, VfxStateKeys.OuterFlamesDragEnabled, PluginConfig.OuterFlamesDragEnabled.Value);
            state.OuterFlamesEnergy = ReadFloat(customData, VfxStateKeys.OuterFlamesEnergy, PluginConfig.OuterFlamesEnergy.Value);
            state.OuterFlamesScale = ReadFloat(customData, VfxStateKeys.OuterFlamesScale, PluginConfig.OuterFlamesScale.Value);
            state.OuterFlamesLuminance = ReadFloat(customData, VfxStateKeys.OuterFlamesLuminance, PluginConfig.OuterFlamesLuminance.Value);
            state.OuterFlamesHue = ReadFloat(customData, VfxStateKeys.OuterFlamesHue, PluginConfig.OuterFlamesHue.Value);
            state.OuterFlamesLifetime = ReadFloat(customData, VfxStateKeys.OuterFlamesLifetime, PluginConfig.OuterFlamesLifetime.Value);
            state.OuterFlamesSimulationSpeed = ReadFloat(customData, VfxStateKeys.OuterFlamesSimulationSpeed, PluginConfig.OuterFlamesSimulationSpeed.Value);
            state.OuterFlamesLength = ReadFloat(customData, VfxStateKeys.OuterFlamesLength, PluginConfig.OuterFlamesLength.Value);
            state.OuterFlamesWidth = ReadFloat(customData, VfxStateKeys.OuterFlamesWidth, PluginConfig.OuterFlamesWidth.Value);
            state.OuterFlamesXOffset = ReadFloat(customData, VfxStateKeys.OuterFlamesXOffset, PluginConfig.OuterFlamesXOffset.Value);
            state.OuterFlamesYOffset = ReadFloat(customData, VfxStateKeys.OuterFlamesYOffset, PluginConfig.OuterFlamesYOffset.Value);
            state.OuterFlamesZOffset = ReadFloat(customData, VfxStateKeys.OuterFlamesZOffset, PluginConfig.OuterFlamesZOffset.Value);
            state.OuterFlamesXRotation = ReadFloat(customData, VfxStateKeys.OuterFlamesXRotation, PluginConfig.OuterFlamesXRotation.Value);
            state.OuterFlamesYRotation = ReadFloat(customData, VfxStateKeys.OuterFlamesYRotation, PluginConfig.OuterFlamesYRotation.Value);
            state.OuterFlamesZRotation = ReadFloat(customData, VfxStateKeys.OuterFlamesZRotation, PluginConfig.OuterFlamesZRotation.Value);

            state.StrandsEnabled = ReadBool(customData, VfxStateKeys.StrandsEnabled, PluginConfig.Strands.Value);
            state.StrandsSpectrumEnabled = ReadBool(customData, VfxStateKeys.StrandsSpectrumEnabled, PluginConfig.StrandsSpectrum.Value);
            state.StrandsEnergy = ReadFloat(customData, VfxStateKeys.StrandsEnergy, PluginConfig.StrandsEnergy.Value);
            state.StrandsScaleWhole = ReadFloat(customData, VfxStateKeys.StrandsScaleWhole, PluginConfig.StrandsScaleWhole.Value);
            state.StrandsScaleParts = ReadFloat(customData, VfxStateKeys.StrandsScaleParts, PluginConfig.StrandsScaleParts.Value);
            state.StrandsLuminance = ReadFloat(customData, VfxStateKeys.StrandsLuminance, PluginConfig.StrandsLuminance.Value);
            state.StrandsHue = ReadFloat(customData, VfxStateKeys.StrandsHue, PluginConfig.StrandsHue.Value);
            state.StrandsLifetime = ReadFloat(customData, VfxStateKeys.StrandsLifetime, PluginConfig.StrandsLifetime.Value);
            state.StrandsLength = ReadFloat(customData, VfxStateKeys.StrandsLength, PluginConfig.StrandsLength.Value);
            state.StrandsSpectrumSpeed = ReadFloat(customData, VfxStateKeys.StrandsSpectrumSpeed, PluginConfig.StrandsSpectrumSpeed.Value);
            state.StrandsSpeed = ReadFloat(customData, VfxStateKeys.StrandsSpeed, PluginConfig.StrandsSpeed.Value);
            state.StrandsRadius = ReadFloat(customData, VfxStateKeys.StrandsRadius, PluginConfig.StrandsRadius.Value);
            state.StrandsXOffset = ReadFloat(customData, VfxStateKeys.StrandsXOffset, PluginConfig.StrandsXOffset.Value);
            state.StrandsYOffset = ReadFloat(customData, VfxStateKeys.StrandsYOffset, PluginConfig.StrandsYOffset.Value);
            state.StrandsZOffset = ReadFloat(customData, VfxStateKeys.StrandsZOffset, PluginConfig.StrandsZOffset.Value);
            state.StrandsXRotation = ReadFloat(customData, VfxStateKeys.StrandsXRotation, PluginConfig.StrandsXRotation.Value);
            state.StrandsYRotation = ReadFloat(customData, VfxStateKeys.StrandsYRotation, PluginConfig.StrandsYRotation.Value);
            state.StrandsZRotation = ReadFloat(customData, VfxStateKeys.StrandsZRotation, PluginConfig.StrandsZRotation.Value);
            state.StrandsDrift = ReadFloat(customData, VfxStateKeys.StrandsDrift, PluginConfig.StrandsDrift.Value);

            state.SparksEnabled = ReadBool(customData, VfxStateKeys.SparksEnabled, PluginConfig.Sparks.Value);
            state.SparksEnergy = ReadFloat(customData, VfxStateKeys.SparksEnergy, PluginConfig.SparksEnergy.Value);
            state.SparksScale = ReadFloat(customData, VfxStateKeys.SparksScale, PluginConfig.SparksScale.Value);
            state.SparksLuminance = ReadFloat(customData, VfxStateKeys.SparksLuminance, PluginConfig.SparksLuminance.Value);
            state.SparksHue = ReadFloat(customData, VfxStateKeys.SparksHue, PluginConfig.SparksHue.Value);
            state.SparksLifetime = ReadFloat(customData, VfxStateKeys.SparksLifetime, PluginConfig.SparksLifetime.Value);
            state.SparksSimulationSpeed = ReadFloat(customData, VfxStateKeys.SparksSimulationSpeed, PluginConfig.SparksSimulationSpeed.Value);
            state.SparksLength = ReadFloat(customData, VfxStateKeys.SparksLength, PluginConfig.SparksLength.Value);
            state.SparksWidth = ReadFloat(customData, VfxStateKeys.SparksWidth, PluginConfig.SparksWidth.Value);
            state.SparksXOffset = ReadFloat(customData, VfxStateKeys.SparksXOffset, PluginConfig.SparksXOffset.Value);
            state.SparksYOffset = ReadFloat(customData, VfxStateKeys.SparksYOffset, PluginConfig.SparksYOffset.Value);
            state.SparksZOffset = ReadFloat(customData, VfxStateKeys.SparksZOffset, PluginConfig.SparksZOffset.Value);
            state.SparksXRotation = ReadFloat(customData, VfxStateKeys.SparksXRotation, PluginConfig.SparksXRotation.Value);
            state.SparksYRotation = ReadFloat(customData, VfxStateKeys.SparksYRotation, PluginConfig.SparksYRotation.Value);
            state.SparksZRotation = ReadFloat(customData, VfxStateKeys.SparksZRotation, PluginConfig.SparksZRotation.Value);

            state.FlareEnabled = ReadBool(customData, VfxStateKeys.FlareEnabled, PluginConfig.Flare.Value);
            state.FlareScale = ReadFloat(customData, VfxStateKeys.FlareScale, PluginConfig.FlareScale.Value);
            state.FlareLuminance = ReadFloat(customData, VfxStateKeys.FlareLuminance, PluginConfig.FlareLuminance.Value);
            state.FlareHue = ReadFloat(customData, VfxStateKeys.FlareHue, PluginConfig.FlareHue.Value);
            state.FlareXOffset = ReadFloat(customData, VfxStateKeys.FlareXOffset, PluginConfig.FlareXOffset.Value);
            state.FlareYOffset = ReadFloat(customData, VfxStateKeys.FlareYOffset, PluginConfig.FlareYOffset.Value);
            state.FlareZOffset = ReadFloat(customData, VfxStateKeys.FlareZOffset, PluginConfig.FlareZOffset.Value);

            state.AuraEnabled = ReadBool(customData, VfxStateKeys.AuraEnabled, PluginConfig.Aura.Value);
            state.AuraScale = ReadFloat(customData, VfxStateKeys.AuraScale, PluginConfig.AuraScale.Value);
            state.AuraLuminance = ReadFloat(customData, VfxStateKeys.AuraLuminance, PluginConfig.AuraLuminance.Value);
            state.AuraHue = ReadFloat(customData, VfxStateKeys.AuraHue, PluginConfig.AuraHue.Value);
            state.AuraXOffset = ReadFloat(customData, VfxStateKeys.AuraXOffset, PluginConfig.AuraXOffset.Value);
            state.AuraYOffset = ReadFloat(customData, VfxStateKeys.AuraYOffset, PluginConfig.AuraYOffset.Value);
            state.AuraZOffset = ReadFloat(customData, VfxStateKeys.AuraZOffset, PluginConfig.AuraZOffset.Value);
            state.AuraXRotation = ReadFloat(customData, VfxStateKeys.AuraXRotation, PluginConfig.AuraXRotation.Value);
            state.AuraYRotation = ReadFloat(customData, VfxStateKeys.AuraYRotation, PluginConfig.AuraYRotation.Value);
            state.AuraZRotation = ReadFloat(customData, VfxStateKeys.AuraZRotation, PluginConfig.AuraZRotation.Value);

            state.OrbitalsOrbsEnabled = ReadBool(customData, VfxStateKeys.OrbitalsOrbsEnabled, PluginConfig.OrbitalsOrbs.Value);
            state.OrbitalsOrbsSnakeEnabled = ReadBool(customData, VfxStateKeys.OrbitalsOrbsSnakeEnabled, PluginConfig.OrbitalsOrbsSnake.Value);
            state.OrbitalsOrbsGlueEnabled = ReadBool(customData, VfxStateKeys.OrbitalsOrbsGlueEnabled, PluginConfig.OrbitalsOrbsGlue.Value);
            state.OrbitalsOrbsCount = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsCount, PluginConfig.OrbitalsOrbsCount.Value);
            state.OrbitalsOrbsScale = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsScale, PluginConfig.OrbitalsOrbsScale.Value);
            state.OrbitalsOrbsLuminance = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsLuminance, PluginConfig.OrbitalsOrbsLuminance.Value);
            state.OrbitalsOrbsHue = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsHue, PluginConfig.OrbitalsOrbsHue.Value);
            state.OrbitalsOrbsLength = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsLength, PluginConfig.OrbitalsOrbsLength.Value);
            state.OrbitalsOrbsSpeed = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsSpeed, PluginConfig.OrbitalsOrbsSpeed.Value);
            state.OrbitalsOrbsSpacing = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsSpacing, PluginConfig.OrbitalsOrbsSpacing.Value);
            state.OrbitalsOrbsRadius = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsRadius, PluginConfig.OrbitalsOrbsRadius.Value);
            state.OrbitalsOrbsCycles = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsCycles, PluginConfig.OrbitalsOrbsCycles.Value);
            state.OrbitalsOrbsXOffset = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsXOffset, PluginConfig.OrbitalsOrbsXOffset.Value);
            state.OrbitalsOrbsYOffset = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsYOffset, PluginConfig.OrbitalsOrbsYOffset.Value);
            state.OrbitalsOrbsZOffset = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsZOffset, PluginConfig.OrbitalsOrbsZOffset.Value);
            state.OrbitalsOrbsXRotation = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsXRotation, PluginConfig.OrbitalsOrbsXRotation.Value);
            state.OrbitalsOrbsYRotation = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsYRotation, PluginConfig.OrbitalsOrbsYRotation.Value);
            state.OrbitalsOrbsZRotation = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsZRotation, PluginConfig.OrbitalsOrbsZRotation.Value);
            state.OrbitalsOrbsDrift = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsDrift, PluginConfig.OrbitalsOrbsDrift.Value);

            state.OrbitalsCoresEnabled = ReadBool(customData, VfxStateKeys.OrbitalsCoresEnabled, PluginConfig.OrbitalsCores.Value);
            state.OrbitalsCoresSnakeEnabled = ReadBool(customData, VfxStateKeys.OrbitalsCoresSnakeEnabled, PluginConfig.OrbitalsCoresSnake.Value);
            state.OrbitalsCoresGlueEnabled = ReadBool(customData, VfxStateKeys.OrbitalsCoresGlueEnabled, PluginConfig.OrbitalsCoresGlue.Value);
            state.OrbitalsCoresSpinEnabled = ReadBool(customData, VfxStateKeys.OrbitalsCoresSpinEnabled, PluginConfig.OrbitalsCoresSpin.Value);
            state.OrbitalsCoresCount = ReadFloat(customData, VfxStateKeys.OrbitalsCoresCount, PluginConfig.OrbitalsCoresCount.Value);
            state.OrbitalsCoresScale = ReadFloat(customData, VfxStateKeys.OrbitalsCoresScale, PluginConfig.OrbitalsCoresScale.Value);
            state.OrbitalsCoresLuminance = ReadFloat(customData, VfxStateKeys.OrbitalsCoresLuminance, PluginConfig.OrbitalsCoresLuminance.Value);
            state.OrbitalsCoresHue = ReadFloat(customData, VfxStateKeys.OrbitalsCoresHue, PluginConfig.OrbitalsCoresHue.Value);
            state.OrbitalsCoresSpinSpeed = ReadFloat(customData, VfxStateKeys.OrbitalsCoresSpinSpeed, PluginConfig.OrbitalsCoresSpinSpeed.Value);
            state.OrbitalsCoresLength = ReadFloat(customData, VfxStateKeys.OrbitalsCoresLength, PluginConfig.OrbitalsCoresLength.Value);
            state.OrbitalsCoresSpeed = ReadFloat(customData, VfxStateKeys.OrbitalsCoresSpeed, PluginConfig.OrbitalsCoresSpeed.Value);
            state.OrbitalsCoresSpacing = ReadFloat(customData, VfxStateKeys.OrbitalsCoresSpacing, PluginConfig.OrbitalsCoresSpacing.Value);
            state.OrbitalsCoresRadius = ReadFloat(customData, VfxStateKeys.OrbitalsCoresRadius, PluginConfig.OrbitalsCoresRadius.Value);
            state.OrbitalsCoresCycles = ReadFloat(customData, VfxStateKeys.OrbitalsCoresCycles, PluginConfig.OrbitalsCoresCycles.Value);
            state.OrbitalsCoresXOffset = ReadFloat(customData, VfxStateKeys.OrbitalsCoresXOffset, PluginConfig.OrbitalsCoresXOffset.Value);
            state.OrbitalsCoresYOffset = ReadFloat(customData, VfxStateKeys.OrbitalsCoresYOffset, PluginConfig.OrbitalsCoresYOffset.Value);
            state.OrbitalsCoresZOffset = ReadFloat(customData, VfxStateKeys.OrbitalsCoresZOffset, PluginConfig.OrbitalsCoresZOffset.Value);
            state.OrbitalsCoresXRotation = ReadFloat(customData, VfxStateKeys.OrbitalsCoresXRotation, PluginConfig.OrbitalsCoresXRotation.Value);
            state.OrbitalsCoresYRotation = ReadFloat(customData, VfxStateKeys.OrbitalsCoresYRotation, PluginConfig.OrbitalsCoresYRotation.Value);
            state.OrbitalsCoresZRotation = ReadFloat(customData, VfxStateKeys.OrbitalsCoresZRotation, PluginConfig.OrbitalsCoresZRotation.Value);
            state.OrbitalsCoresDrift = ReadFloat(customData, VfxStateKeys.OrbitalsCoresDrift, PluginConfig.OrbitalsCoresDrift.Value);

            state.OrbitalsFlamesEnabled = ReadBool(customData, VfxStateKeys.OrbitalsFlamesEnabled, PluginConfig.OrbitalsFlames.Value);
            state.OrbitalsFlamesCount = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesCount, PluginConfig.OrbitalsFlamesCount.Value);
            state.OrbitalsFlamesEnergy = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesEnergy, PluginConfig.OrbitalsFlamesEnergy.Value);
            state.OrbitalsFlamesScale = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesScale, PluginConfig.OrbitalsFlamesScale.Value);
            state.OrbitalsFlamesLuminance = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesLuminance, PluginConfig.OrbitalsFlamesLuminance.Value);
            state.OrbitalsFlamesHue = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesHue, PluginConfig.OrbitalsFlamesHue.Value);
            state.OrbitalsFlamesLifetime = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesLifetime, PluginConfig.OrbitalsFlamesLifetime.Value);
            state.OrbitalsFlamesSimulationSpeed = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesSimulationSpeed, PluginConfig.OrbitalsFlamesSimulationSpeed.Value);
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
            state.OrbitalsFlamesDrift = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesDrift, PluginConfig.OrbitalsFlamesDrift.Value);

            state.OrbitalsEmbersEnabled = ReadBool(customData, VfxStateKeys.OrbitalsEmbersEnabled, PluginConfig.OrbitalsEmbers.Value);
            state.OrbitalsEmbersCount = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersCount, PluginConfig.OrbitalsEmbersCount.Value);
            state.OrbitalsEmbersEnergy = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersEnergy, PluginConfig.OrbitalsEmbersEnergy.Value);
            state.OrbitalsEmbersScale = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersScale, PluginConfig.OrbitalsEmbersScale.Value);
            state.OrbitalsEmbersLuminance = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersLuminance, PluginConfig.OrbitalsEmbersLuminance.Value);
            state.OrbitalsEmbersHue = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersHue, PluginConfig.OrbitalsEmbersHue.Value);
            state.OrbitalsEmbersLifetime = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersLifetime, PluginConfig.OrbitalsEmbersLifetime.Value);
            state.OrbitalsEmbersSimulationSpeed = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersSimulationSpeed, PluginConfig.OrbitalsEmbersSimulationSpeed.Value);
            state.OrbitalsEmbersLength = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersLength, PluginConfig.OrbitalsEmbersLength.Value);
            state.OrbitalsEmbersSpeed = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersSpeed, PluginConfig.OrbitalsEmbersSpeed.Value);
            state.OrbitalsEmbersSpacing = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersSpacing, PluginConfig.OrbitalsEmbersSpacing.Value);
            state.OrbitalsEmbersRadius = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersRadius, PluginConfig.OrbitalsEmbersRadius.Value);
            state.OrbitalsEmbersCycles = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersCycles, PluginConfig.OrbitalsEmbersCycles.Value);
            state.OrbitalsEmbersXOffset = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersXOffset, PluginConfig.OrbitalsEmbersXOffset.Value);
            state.OrbitalsEmbersYOffset = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersYOffset, PluginConfig.OrbitalsEmbersYOffset.Value);
            state.OrbitalsEmbersZOffset = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersZOffset, PluginConfig.OrbitalsEmbersZOffset.Value);
            state.OrbitalsEmbersXRotation = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersXRotation, PluginConfig.OrbitalsEmbersXRotation.Value);
            state.OrbitalsEmbersYRotation = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersYRotation, PluginConfig.OrbitalsEmbersYRotation.Value);
            state.OrbitalsEmbersZRotation = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersZRotation, PluginConfig.OrbitalsEmbersZRotation.Value);
            state.OrbitalsEmbersDrift = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersDrift, PluginConfig.OrbitalsEmbersDrift.Value);

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
            BackfillMissing(customData, VfxStateKeys.InnerFlamesWorldEnabled, PluginConfig.InnerFlamesWorld.Value);
            BackfillMissing(customData, VfxStateKeys.InnerFlamesBlackEnabled, PluginConfig.InnerFlamesBlack.Value);
            BackfillMissing(customData, VfxStateKeys.InnerFlamesWhiteEnabled, PluginConfig.InnerFlamesWhite.Value);
            BackfillMissing(customData, VfxStateKeys.InnerFlamesEnergy, PluginConfig.InnerFlamesEnergy.Value);
            BackfillMissing(customData, VfxStateKeys.InnerFlamesScale, PluginConfig.InnerFlamesScale.Value);
            BackfillMissing(customData, VfxStateKeys.InnerFlamesLuminance, PluginConfig.InnerFlamesLuminance.Value);
            BackfillMissing(customData, VfxStateKeys.InnerFlamesHue, PluginConfig.InnerFlamesHue.Value);
            BackfillMissing(customData, VfxStateKeys.InnerFlamesLifetime, PluginConfig.InnerFlamesLifetime.Value);
            BackfillMissing(customData, VfxStateKeys.InnerFlamesSimulationSpeed, PluginConfig.InnerFlamesSimulationSpeed.Value);
            BackfillMissing(customData, VfxStateKeys.InnerFlamesLength, PluginConfig.InnerFlamesLength.Value);
            BackfillMissing(customData, VfxStateKeys.InnerFlamesWidth, PluginConfig.InnerFlamesWidth.Value);
            BackfillMissing(customData, VfxStateKeys.InnerFlamesXOffset, PluginConfig.InnerFlamesXOffset.Value);
            BackfillMissing(customData, VfxStateKeys.InnerFlamesYOffset, PluginConfig.InnerFlamesYOffset.Value);
            BackfillMissing(customData, VfxStateKeys.InnerFlamesZOffset, PluginConfig.InnerFlamesZOffset.Value);
            BackfillMissing(customData, VfxStateKeys.InnerFlamesXRotation, PluginConfig.InnerFlamesXRotation.Value);
            BackfillMissing(customData, VfxStateKeys.InnerFlamesYRotation, PluginConfig.InnerFlamesYRotation.Value);
            BackfillMissing(customData, VfxStateKeys.InnerFlamesZRotation, PluginConfig.InnerFlamesZRotation.Value);

            BackfillMissing(customData, VfxStateKeys.OuterFlamesEnabled, PluginConfig.OuterFlames.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesWorldEnabled, PluginConfig.OuterFlamesWorld.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesBlackEnabled, PluginConfig.OuterFlamesBlack.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesWhiteEnabled, PluginConfig.OuterFlamesWhite.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesDragEnabled, PluginConfig.OuterFlamesDragEnabled.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesEnergy, PluginConfig.OuterFlamesEnergy.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesScale, PluginConfig.OuterFlamesScale.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesLuminance, PluginConfig.OuterFlamesLuminance.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesHue, PluginConfig.OuterFlamesHue.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesLifetime, PluginConfig.OuterFlamesLifetime.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesSimulationSpeed, PluginConfig.OuterFlamesSimulationSpeed.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesLength, PluginConfig.OuterFlamesLength.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesWidth, PluginConfig.OuterFlamesWidth.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesXOffset, PluginConfig.OuterFlamesXOffset.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesYOffset, PluginConfig.OuterFlamesYOffset.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesZOffset, PluginConfig.OuterFlamesZOffset.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesXRotation, PluginConfig.OuterFlamesXRotation.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesYRotation, PluginConfig.OuterFlamesYRotation.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesZRotation, PluginConfig.OuterFlamesZRotation.Value);

            BackfillMissing(customData, VfxStateKeys.StrandsEnabled, PluginConfig.Strands.Value);
            BackfillMissing(customData, VfxStateKeys.StrandsSpectrumEnabled, PluginConfig.StrandsSpectrum.Value);
            BackfillMissing(customData, VfxStateKeys.StrandsEnergy, PluginConfig.StrandsEnergy.Value);
            BackfillMissing(customData, VfxStateKeys.StrandsScaleWhole, PluginConfig.StrandsScaleWhole.Value);
            BackfillMissing(customData, VfxStateKeys.StrandsScaleParts, PluginConfig.StrandsScaleParts.Value);
            BackfillMissing(customData, VfxStateKeys.StrandsLuminance, PluginConfig.StrandsLuminance.Value);
            BackfillMissing(customData, VfxStateKeys.StrandsHue, PluginConfig.StrandsHue.Value);
            BackfillMissing(customData, VfxStateKeys.StrandsLifetime, PluginConfig.StrandsLifetime.Value);
            BackfillMissing(customData, VfxStateKeys.StrandsLength, PluginConfig.StrandsLength.Value);
            BackfillMissing(customData, VfxStateKeys.StrandsSpectrumSpeed, PluginConfig.StrandsSpectrumSpeed.Value);
            BackfillMissing(customData, VfxStateKeys.StrandsSpeed, PluginConfig.StrandsSpeed.Value);
            BackfillMissing(customData, VfxStateKeys.StrandsRadius, PluginConfig.StrandsRadius.Value);
            BackfillMissing(customData, VfxStateKeys.StrandsXOffset, PluginConfig.StrandsXOffset.Value);
            BackfillMissing(customData, VfxStateKeys.StrandsYOffset, PluginConfig.StrandsYOffset.Value);
            BackfillMissing(customData, VfxStateKeys.StrandsZOffset, PluginConfig.StrandsZOffset.Value);
            BackfillMissing(customData, VfxStateKeys.StrandsXRotation, PluginConfig.StrandsXRotation.Value);
            BackfillMissing(customData, VfxStateKeys.StrandsYRotation, PluginConfig.StrandsYRotation.Value);
            BackfillMissing(customData, VfxStateKeys.StrandsZRotation, PluginConfig.StrandsZRotation.Value);
            BackfillMissing(customData, VfxStateKeys.StrandsDrift, PluginConfig.StrandsDrift.Value);

            BackfillMissing(customData, VfxStateKeys.SparksEnabled, PluginConfig.Sparks.Value);
            BackfillMissing(customData, VfxStateKeys.SparksEnergy, PluginConfig.SparksEnergy.Value);
            BackfillMissing(customData, VfxStateKeys.SparksScale, PluginConfig.SparksScale.Value);
            BackfillMissing(customData, VfxStateKeys.SparksLuminance, PluginConfig.SparksLuminance.Value);
            BackfillMissing(customData, VfxStateKeys.SparksHue, PluginConfig.SparksHue.Value);
            BackfillMissing(customData, VfxStateKeys.SparksLifetime, PluginConfig.SparksLifetime.Value);
            BackfillMissing(customData, VfxStateKeys.SparksSimulationSpeed, PluginConfig.SparksSimulationSpeed.Value);
            BackfillMissing(customData, VfxStateKeys.SparksLength, PluginConfig.SparksLength.Value);
            BackfillMissing(customData, VfxStateKeys.SparksWidth, PluginConfig.SparksWidth.Value);
            BackfillMissing(customData, VfxStateKeys.SparksXOffset, PluginConfig.SparksXOffset.Value);
            BackfillMissing(customData, VfxStateKeys.SparksYOffset, PluginConfig.SparksYOffset.Value);
            BackfillMissing(customData, VfxStateKeys.SparksZOffset, PluginConfig.SparksZOffset.Value);
            BackfillMissing(customData, VfxStateKeys.SparksXRotation, PluginConfig.SparksXRotation.Value);
            BackfillMissing(customData, VfxStateKeys.SparksYRotation, PluginConfig.SparksYRotation.Value);
            BackfillMissing(customData, VfxStateKeys.SparksZRotation, PluginConfig.SparksZRotation.Value);

            BackfillMissing(customData, VfxStateKeys.FlareEnabled, PluginConfig.Flare.Value);
            BackfillMissing(customData, VfxStateKeys.FlareScale, PluginConfig.FlareScale.Value);
            BackfillMissing(customData, VfxStateKeys.FlareLuminance, PluginConfig.FlareLuminance.Value);
            BackfillMissing(customData, VfxStateKeys.FlareHue, PluginConfig.FlareHue.Value);
            BackfillMissing(customData, VfxStateKeys.FlareXOffset, PluginConfig.FlareXOffset.Value);
            BackfillMissing(customData, VfxStateKeys.FlareYOffset, PluginConfig.FlareYOffset.Value);
            BackfillMissing(customData, VfxStateKeys.FlareZOffset, PluginConfig.FlareZOffset.Value);

            BackfillMissing(customData, VfxStateKeys.AuraEnabled, PluginConfig.Aura.Value);
            BackfillMissing(customData, VfxStateKeys.AuraScale, PluginConfig.AuraScale.Value);
            BackfillMissing(customData, VfxStateKeys.AuraLuminance, PluginConfig.AuraLuminance.Value);
            BackfillMissing(customData, VfxStateKeys.AuraHue, PluginConfig.AuraHue.Value);
            BackfillMissing(customData, VfxStateKeys.AuraXOffset, PluginConfig.AuraXOffset.Value);
            BackfillMissing(customData, VfxStateKeys.AuraYOffset, PluginConfig.AuraYOffset.Value);
            BackfillMissing(customData, VfxStateKeys.AuraZOffset, PluginConfig.AuraZOffset.Value);
            BackfillMissing(customData, VfxStateKeys.AuraXRotation, PluginConfig.AuraXRotation.Value);
            BackfillMissing(customData, VfxStateKeys.AuraYRotation, PluginConfig.AuraYRotation.Value);
            BackfillMissing(customData, VfxStateKeys.AuraZRotation, PluginConfig.AuraZRotation.Value);

            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsEnabled, PluginConfig.OrbitalsOrbs.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsSnakeEnabled, PluginConfig.OrbitalsOrbsSnake.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsGlueEnabled, PluginConfig.OrbitalsOrbsGlue.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsCount, PluginConfig.OrbitalsOrbsCount.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsScale, PluginConfig.OrbitalsOrbsScale.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsLuminance, PluginConfig.OrbitalsOrbsLuminance.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsHue, PluginConfig.OrbitalsOrbsHue.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsLength, PluginConfig.OrbitalsOrbsLength.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsSpeed, PluginConfig.OrbitalsOrbsSpeed.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsSpacing, PluginConfig.OrbitalsOrbsSpacing.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsRadius, PluginConfig.OrbitalsOrbsRadius.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsCycles, PluginConfig.OrbitalsOrbsCycles.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsXOffset, PluginConfig.OrbitalsOrbsXOffset.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsYOffset, PluginConfig.OrbitalsOrbsYOffset.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsZOffset, PluginConfig.OrbitalsOrbsZOffset.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsXRotation, PluginConfig.OrbitalsOrbsXRotation.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsYRotation, PluginConfig.OrbitalsOrbsYRotation.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsZRotation, PluginConfig.OrbitalsOrbsZRotation.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsOrbsDrift, PluginConfig.OrbitalsOrbsDrift.Value);

            BackfillMissing(customData, VfxStateKeys.OrbitalsCoresEnabled, PluginConfig.OrbitalsCores.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsCoresSnakeEnabled, PluginConfig.OrbitalsCoresSnake.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsCoresGlueEnabled, PluginConfig.OrbitalsCoresGlue.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsCoresSpinEnabled, PluginConfig.OrbitalsCoresSpin.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsCoresCount, PluginConfig.OrbitalsCoresCount.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsCoresScale, PluginConfig.OrbitalsCoresScale.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsCoresLuminance, PluginConfig.OrbitalsCoresLuminance.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsCoresHue, PluginConfig.OrbitalsCoresHue.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsCoresSpinSpeed, PluginConfig.OrbitalsCoresSpinSpeed.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsCoresLength, PluginConfig.OrbitalsCoresLength.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsCoresSpeed, PluginConfig.OrbitalsCoresSpeed.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsCoresSpacing, PluginConfig.OrbitalsCoresSpacing.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsCoresRadius, PluginConfig.OrbitalsCoresRadius.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsCoresCycles, PluginConfig.OrbitalsCoresCycles.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsCoresXOffset, PluginConfig.OrbitalsCoresXOffset.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsCoresYOffset, PluginConfig.OrbitalsCoresYOffset.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsCoresZOffset, PluginConfig.OrbitalsCoresZOffset.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsCoresXRotation, PluginConfig.OrbitalsCoresXRotation.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsCoresYRotation, PluginConfig.OrbitalsCoresYRotation.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsCoresZRotation, PluginConfig.OrbitalsCoresZRotation.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsCoresDrift, PluginConfig.OrbitalsCoresDrift.Value);

            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesEnabled, PluginConfig.OrbitalsFlames.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesCount, PluginConfig.OrbitalsFlamesCount.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesEnergy, PluginConfig.OrbitalsFlamesEnergy.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesScale, PluginConfig.OrbitalsFlamesScale.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesLuminance, PluginConfig.OrbitalsFlamesLuminance.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesHue, PluginConfig.OrbitalsFlamesHue.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesLifetime, PluginConfig.OrbitalsFlamesLifetime.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesSimulationSpeed, PluginConfig.OrbitalsFlamesSimulationSpeed.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesLength, PluginConfig.OrbitalsFlamesLength.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesSpeed, PluginConfig.OrbitalsFlamesSpeed.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesSpacing, PluginConfig.OrbitalsFlamesSpacing.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesRadius, PluginConfig.OrbitalsFlamesRadius.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesCycles, PluginConfig.OrbitalsFlamesCycles.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesXOffset, PluginConfig.OrbitalsFlamesXOffset.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesYOffset, PluginConfig.OrbitalsFlamesYOffset.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesZOffset, PluginConfig.OrbitalsFlamesZOffset.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesXRotation, PluginConfig.OrbitalsFlamesXRotation.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesYRotation, PluginConfig.OrbitalsFlamesYRotation.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesZRotation, PluginConfig.OrbitalsFlamesZRotation.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsFlamesDrift, PluginConfig.OrbitalsFlamesDrift.Value);

            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersEnabled, PluginConfig.OrbitalsEmbers.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersCount, PluginConfig.OrbitalsEmbersCount.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersEnergy, PluginConfig.OrbitalsEmbersEnergy.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersScale, PluginConfig.OrbitalsEmbersScale.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersLuminance, PluginConfig.OrbitalsEmbersLuminance.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersHue, PluginConfig.OrbitalsEmbersHue.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersLifetime, PluginConfig.OrbitalsEmbersLifetime.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersSimulationSpeed, PluginConfig.OrbitalsEmbersSimulationSpeed.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersLength, PluginConfig.OrbitalsEmbersLength.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersSpeed, PluginConfig.OrbitalsEmbersSpeed.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersSpacing, PluginConfig.OrbitalsEmbersSpacing.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersRadius, PluginConfig.OrbitalsEmbersRadius.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersCycles, PluginConfig.OrbitalsEmbersCycles.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersXOffset, PluginConfig.OrbitalsEmbersXOffset.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersYOffset, PluginConfig.OrbitalsEmbersYOffset.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersZOffset, PluginConfig.OrbitalsEmbersZOffset.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersXRotation, PluginConfig.OrbitalsEmbersXRotation.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersYRotation, PluginConfig.OrbitalsEmbersYRotation.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersZRotation, PluginConfig.OrbitalsEmbersZRotation.Value);
            BackfillMissing(customData, VfxStateKeys.OrbitalsEmbersDrift, PluginConfig.OrbitalsEmbersDrift.Value);
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
            item.m_customData.Remove(VfxStateKeys.InnerFlamesWorldEnabled);
            item.m_customData.Remove(VfxStateKeys.InnerFlamesBlackEnabled);
            item.m_customData.Remove(VfxStateKeys.InnerFlamesWhiteEnabled);
            item.m_customData.Remove(VfxStateKeys.InnerFlamesEnergy);
            item.m_customData.Remove(VfxStateKeys.InnerFlamesScale);
            item.m_customData.Remove(VfxStateKeys.InnerFlamesLuminance);
            item.m_customData.Remove(VfxStateKeys.InnerFlamesHue);
            item.m_customData.Remove(VfxStateKeys.InnerFlamesLifetime);
            item.m_customData.Remove(VfxStateKeys.InnerFlamesSimulationSpeed);
            item.m_customData.Remove(VfxStateKeys.InnerFlamesLength);
            item.m_customData.Remove(VfxStateKeys.InnerFlamesWidth);
            item.m_customData.Remove(VfxStateKeys.InnerFlamesXOffset);
            item.m_customData.Remove(VfxStateKeys.InnerFlamesYOffset);
            item.m_customData.Remove(VfxStateKeys.InnerFlamesZOffset);
            item.m_customData.Remove(VfxStateKeys.InnerFlamesXRotation);
            item.m_customData.Remove(VfxStateKeys.InnerFlamesYRotation);
            item.m_customData.Remove(VfxStateKeys.InnerFlamesZRotation);

            item.m_customData.Remove(VfxStateKeys.OuterFlamesEnabled);
            item.m_customData.Remove(VfxStateKeys.OuterFlamesWorldEnabled);
            item.m_customData.Remove(VfxStateKeys.OuterFlamesBlackEnabled);
            item.m_customData.Remove(VfxStateKeys.OuterFlamesWhiteEnabled);
            item.m_customData.Remove(VfxStateKeys.OuterFlamesDragEnabled);
            item.m_customData.Remove(VfxStateKeys.OuterFlamesEnergy);
            item.m_customData.Remove(VfxStateKeys.OuterFlamesScale);
            item.m_customData.Remove(VfxStateKeys.OuterFlamesLuminance);
            item.m_customData.Remove(VfxStateKeys.OuterFlamesHue);
            item.m_customData.Remove(VfxStateKeys.OuterFlamesLifetime);
            item.m_customData.Remove(VfxStateKeys.OuterFlamesSimulationSpeed);
            item.m_customData.Remove(VfxStateKeys.OuterFlamesLength);
            item.m_customData.Remove(VfxStateKeys.OuterFlamesWidth);
            item.m_customData.Remove(VfxStateKeys.OuterFlamesXOffset);
            item.m_customData.Remove(VfxStateKeys.OuterFlamesYOffset);
            item.m_customData.Remove(VfxStateKeys.OuterFlamesZOffset);
            item.m_customData.Remove(VfxStateKeys.OuterFlamesXRotation);
            item.m_customData.Remove(VfxStateKeys.OuterFlamesYRotation);
            item.m_customData.Remove(VfxStateKeys.OuterFlamesZRotation);

            item.m_customData.Remove(VfxStateKeys.StrandsEnabled);
            item.m_customData.Remove(VfxStateKeys.StrandsSpectrumEnabled);
            item.m_customData.Remove(VfxStateKeys.StrandsEnergy);
            item.m_customData.Remove(VfxStateKeys.StrandsScaleWhole);
            item.m_customData.Remove(VfxStateKeys.StrandsScaleParts);
            item.m_customData.Remove(VfxStateKeys.StrandsLuminance);
            item.m_customData.Remove(VfxStateKeys.StrandsHue);
            item.m_customData.Remove(VfxStateKeys.StrandsLifetime);
            item.m_customData.Remove(VfxStateKeys.StrandsLength);
            item.m_customData.Remove(VfxStateKeys.StrandsSpectrumSpeed);
            item.m_customData.Remove(VfxStateKeys.StrandsSpeed);
            item.m_customData.Remove(VfxStateKeys.StrandsRadius);
            item.m_customData.Remove(VfxStateKeys.StrandsXOffset);
            item.m_customData.Remove(VfxStateKeys.StrandsYOffset);
            item.m_customData.Remove(VfxStateKeys.StrandsZOffset);
            item.m_customData.Remove(VfxStateKeys.StrandsXRotation);
            item.m_customData.Remove(VfxStateKeys.StrandsYRotation);
            item.m_customData.Remove(VfxStateKeys.StrandsZRotation);
            item.m_customData.Remove(VfxStateKeys.StrandsDrift);

            item.m_customData.Remove(VfxStateKeys.SparksEnabled);
            item.m_customData.Remove(VfxStateKeys.SparksEnergy);
            item.m_customData.Remove(VfxStateKeys.SparksScale);
            item.m_customData.Remove(VfxStateKeys.SparksLuminance);
            item.m_customData.Remove(VfxStateKeys.SparksHue);
            item.m_customData.Remove(VfxStateKeys.SparksLifetime);
            item.m_customData.Remove(VfxStateKeys.SparksSimulationSpeed);
            item.m_customData.Remove(VfxStateKeys.SparksLength);
            item.m_customData.Remove(VfxStateKeys.SparksWidth);
            item.m_customData.Remove(VfxStateKeys.SparksXOffset);
            item.m_customData.Remove(VfxStateKeys.SparksYOffset);
            item.m_customData.Remove(VfxStateKeys.SparksZOffset);
            item.m_customData.Remove(VfxStateKeys.SparksXRotation);
            item.m_customData.Remove(VfxStateKeys.SparksYRotation);
            item.m_customData.Remove(VfxStateKeys.SparksZRotation);

            item.m_customData.Remove(VfxStateKeys.FlareEnabled);
            item.m_customData.Remove(VfxStateKeys.FlareScale);
            item.m_customData.Remove(VfxStateKeys.FlareLuminance);
            item.m_customData.Remove(VfxStateKeys.FlareHue);
            item.m_customData.Remove(VfxStateKeys.FlareXOffset);
            item.m_customData.Remove(VfxStateKeys.FlareYOffset);
            item.m_customData.Remove(VfxStateKeys.FlareZOffset);

            item.m_customData.Remove(VfxStateKeys.AuraEnabled);
            item.m_customData.Remove(VfxStateKeys.AuraScale);
            item.m_customData.Remove(VfxStateKeys.AuraLuminance);
            item.m_customData.Remove(VfxStateKeys.AuraHue);
            item.m_customData.Remove(VfxStateKeys.AuraXOffset);
            item.m_customData.Remove(VfxStateKeys.AuraYOffset);
            item.m_customData.Remove(VfxStateKeys.AuraZOffset);
            item.m_customData.Remove(VfxStateKeys.AuraXRotation);
            item.m_customData.Remove(VfxStateKeys.AuraYRotation);
            item.m_customData.Remove(VfxStateKeys.AuraZRotation);

            item.m_customData.Remove(VfxStateKeys.OrbitalsOrbsEnabled);
            item.m_customData.Remove(VfxStateKeys.OrbitalsOrbsGlueEnabled);
            item.m_customData.Remove(VfxStateKeys.OrbitalsOrbsSnakeEnabled);
            item.m_customData.Remove(VfxStateKeys.OrbitalsOrbsCount);
            item.m_customData.Remove(VfxStateKeys.OrbitalsOrbsScale);
            item.m_customData.Remove(VfxStateKeys.OrbitalsOrbsLuminance);
            item.m_customData.Remove(VfxStateKeys.OrbitalsOrbsHue);
            item.m_customData.Remove(VfxStateKeys.OrbitalsOrbsLength);
            item.m_customData.Remove(VfxStateKeys.OrbitalsOrbsSpeed);
            item.m_customData.Remove(VfxStateKeys.OrbitalsOrbsSpacing);
            item.m_customData.Remove(VfxStateKeys.OrbitalsOrbsRadius);
            item.m_customData.Remove(VfxStateKeys.OrbitalsOrbsCycles);
            item.m_customData.Remove(VfxStateKeys.OrbitalsOrbsXOffset);
            item.m_customData.Remove(VfxStateKeys.OrbitalsOrbsYOffset);
            item.m_customData.Remove(VfxStateKeys.OrbitalsOrbsZOffset);
            item.m_customData.Remove(VfxStateKeys.OrbitalsOrbsXRotation);
            item.m_customData.Remove(VfxStateKeys.OrbitalsOrbsYRotation);
            item.m_customData.Remove(VfxStateKeys.OrbitalsOrbsZRotation);
            item.m_customData.Remove(VfxStateKeys.OrbitalsOrbsDrift);

            item.m_customData.Remove(VfxStateKeys.OrbitalsCoresEnabled);
            item.m_customData.Remove(VfxStateKeys.OrbitalsCoresSnakeEnabled);
            item.m_customData.Remove(VfxStateKeys.OrbitalsCoresGlueEnabled);
            item.m_customData.Remove(VfxStateKeys.OrbitalsCoresSpinEnabled);
            item.m_customData.Remove(VfxStateKeys.OrbitalsCoresCount);
            item.m_customData.Remove(VfxStateKeys.OrbitalsCoresScale);
            item.m_customData.Remove(VfxStateKeys.OrbitalsCoresLuminance);
            item.m_customData.Remove(VfxStateKeys.OrbitalsCoresHue);
            item.m_customData.Remove(VfxStateKeys.OrbitalsCoresSpinSpeed);
            item.m_customData.Remove(VfxStateKeys.OrbitalsCoresLength);
            item.m_customData.Remove(VfxStateKeys.OrbitalsCoresSpeed);
            item.m_customData.Remove(VfxStateKeys.OrbitalsCoresSpacing);
            item.m_customData.Remove(VfxStateKeys.OrbitalsCoresRadius);
            item.m_customData.Remove(VfxStateKeys.OrbitalsCoresCycles);
            item.m_customData.Remove(VfxStateKeys.OrbitalsCoresXOffset);
            item.m_customData.Remove(VfxStateKeys.OrbitalsCoresYOffset);
            item.m_customData.Remove(VfxStateKeys.OrbitalsCoresZOffset);
            item.m_customData.Remove(VfxStateKeys.OrbitalsCoresXRotation);
            item.m_customData.Remove(VfxStateKeys.OrbitalsCoresYRotation);
            item.m_customData.Remove(VfxStateKeys.OrbitalsCoresZRotation);
            item.m_customData.Remove(VfxStateKeys.OrbitalsCoresDrift);

            item.m_customData.Remove(VfxStateKeys.OrbitalsFlamesEnabled);
            item.m_customData.Remove(VfxStateKeys.OrbitalsFlamesCount);
            item.m_customData.Remove(VfxStateKeys.OrbitalsFlamesEnergy);
            item.m_customData.Remove(VfxStateKeys.OrbitalsFlamesScale);
            item.m_customData.Remove(VfxStateKeys.OrbitalsFlamesLuminance);
            item.m_customData.Remove(VfxStateKeys.OrbitalsFlamesHue);
            item.m_customData.Remove(VfxStateKeys.OrbitalsFlamesLifetime);
            item.m_customData.Remove(VfxStateKeys.OrbitalsFlamesSimulationSpeed);
            item.m_customData.Remove(VfxStateKeys.OrbitalsFlamesLength);
            item.m_customData.Remove(VfxStateKeys.OrbitalsFlamesSpeed);
            item.m_customData.Remove(VfxStateKeys.OrbitalsFlamesSpacing);
            item.m_customData.Remove(VfxStateKeys.OrbitalsFlamesRadius);
            item.m_customData.Remove(VfxStateKeys.OrbitalsFlamesCycles);
            item.m_customData.Remove(VfxStateKeys.OrbitalsFlamesXOffset);
            item.m_customData.Remove(VfxStateKeys.OrbitalsFlamesYOffset);
            item.m_customData.Remove(VfxStateKeys.OrbitalsFlamesZOffset);
            item.m_customData.Remove(VfxStateKeys.OrbitalsFlamesXRotation);
            item.m_customData.Remove(VfxStateKeys.OrbitalsFlamesYRotation);
            item.m_customData.Remove(VfxStateKeys.OrbitalsFlamesZRotation);
            item.m_customData.Remove(VfxStateKeys.OrbitalsFlamesDrift);

            item.m_customData.Remove(VfxStateKeys.OrbitalsEmbersEnabled);
            item.m_customData.Remove(VfxStateKeys.OrbitalsEmbersCount);
            item.m_customData.Remove(VfxStateKeys.OrbitalsEmbersEnergy);
            item.m_customData.Remove(VfxStateKeys.OrbitalsEmbersScale);
            item.m_customData.Remove(VfxStateKeys.OrbitalsEmbersLuminance);
            item.m_customData.Remove(VfxStateKeys.OrbitalsEmbersHue);
            item.m_customData.Remove(VfxStateKeys.OrbitalsEmbersLifetime);
            item.m_customData.Remove(VfxStateKeys.OrbitalsEmbersSimulationSpeed);
            item.m_customData.Remove(VfxStateKeys.OrbitalsEmbersLength);
            item.m_customData.Remove(VfxStateKeys.OrbitalsEmbersSpeed);
            item.m_customData.Remove(VfxStateKeys.OrbitalsEmbersSpacing);
            item.m_customData.Remove(VfxStateKeys.OrbitalsEmbersRadius);
            item.m_customData.Remove(VfxStateKeys.OrbitalsEmbersCycles);
            item.m_customData.Remove(VfxStateKeys.OrbitalsEmbersXOffset);
            item.m_customData.Remove(VfxStateKeys.OrbitalsEmbersYOffset);
            item.m_customData.Remove(VfxStateKeys.OrbitalsEmbersZOffset);
            item.m_customData.Remove(VfxStateKeys.OrbitalsEmbersXRotation);
            item.m_customData.Remove(VfxStateKeys.OrbitalsEmbersYRotation);
            item.m_customData.Remove(VfxStateKeys.OrbitalsEmbersZRotation);
            item.m_customData.Remove(VfxStateKeys.OrbitalsEmbersDrift);
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
                InnerFlamesWorldEnabled = false,
                InnerFlamesBlackEnabled = false,
                InnerFlamesWhiteEnabled = false,
                InnerFlamesEnergy = PluginConfig.DefaultEnergy,
                InnerFlamesScale = 1f,
                InnerFlamesLuminance = PluginConfig.DefaultLuminance,
                InnerFlamesHue = PluginConfig.DefaultHue,
                InnerFlamesLifetime = PluginConfig.DefaultLifetime,
                InnerFlamesSimulationSpeed =  PluginConfig.DefaultSimulationSpeed,
                InnerFlamesLength = PluginConfig.DefaultFlameLength,
                InnerFlamesWidth = PluginConfig.DefaultFlameWidth,
                InnerFlamesXOffset = PluginConfig.DefaultEffectOffset,
                InnerFlamesYOffset = PluginConfig.DefaultEffectOffset,
                InnerFlamesZOffset = PluginConfig.DefaultEffectOffset,
                InnerFlamesXRotation = PluginConfig.DefaultEffectRotation,
                InnerFlamesYRotation = PluginConfig.DefaultEffectRotation,
                InnerFlamesZRotation = PluginConfig.DefaultEffectRotation,

                OuterFlamesEnabled = true,
                OuterFlamesWorldEnabled = false,
                OuterFlamesBlackEnabled = false,
                OuterFlamesWhiteEnabled = false,
                OuterFlamesDragEnabled = false,
                OuterFlamesEnergy = PluginConfig.DefaultEnergy,
                OuterFlamesScale = 1f,
                OuterFlamesLuminance = PluginConfig.DefaultLuminance,
                OuterFlamesHue = PluginConfig.DefaultHue,
                OuterFlamesLifetime = PluginConfig.DefaultLifetime,
                OuterFlamesSimulationSpeed = PluginConfig.DefaultSimulationSpeed,
                OuterFlamesLength = PluginConfig.DefaultFlameLength,
                OuterFlamesWidth = PluginConfig.DefaultFlameWidth,
                OuterFlamesXOffset = PluginConfig.DefaultEffectOffset,
                OuterFlamesYOffset = PluginConfig.DefaultEffectOffset,
                OuterFlamesZOffset = PluginConfig.DefaultEffectOffset,
                OuterFlamesXRotation = PluginConfig.DefaultEffectRotation,
                OuterFlamesYRotation = PluginConfig.DefaultEffectRotation,
                OuterFlamesZRotation = PluginConfig.DefaultEffectRotation,

                StrandsEnabled = true,
                StrandsSpectrumEnabled = false,
                StrandsEnergy = PluginConfig.DefaultEnergy,
                StrandsScaleWhole = 0.50f,
                StrandsScaleParts = 1f,
                StrandsLuminance = PluginConfig.DefaultLuminance,
                StrandsHue = PluginConfig.DefaultHue,
                StrandsLifetime = PluginConfig.DefaultLifetime,
                StrandsLength = PluginConfig.DefaultOrbitalsLength,
                StrandsSpectrumSpeed = PluginConfig.DefaultSpectrumSpeed,
                StrandsSpeed = PluginConfig.DefaultOrbitalsSpeed,
                StrandsRadius = PluginConfig.DefaultOrbitalsRadiusMultiplier,
                StrandsXOffset = PluginConfig.DefaultEffectOffset,
                StrandsYOffset = PluginConfig.DefaultEffectOffset,
                StrandsZOffset = PluginConfig.DefaultEffectOffset,
                StrandsXRotation = PluginConfig.DefaultEffectRotation,
                StrandsYRotation = PluginConfig.DefaultEffectRotation,
                StrandsZRotation = PluginConfig.DefaultEffectRotation,
                StrandsDrift = PluginConfig.DefaultDrift,

                SparksEnabled = true,
                SparksEnergy = PluginConfig.DefaultEnergy,
                SparksScale = 1f,
                SparksLuminance = PluginConfig.DefaultLuminance,
                SparksHue = PluginConfig.DefaultHue,
                SparksLifetime = PluginConfig.DefaultLifetime,
                SparksSimulationSpeed =  PluginConfig.DefaultSimulationSpeed,
                SparksLength = PluginConfig.DefaultFlameLength,
                SparksWidth = PluginConfig.DefaultSparksWidth,
                SparksXOffset = PluginConfig.DefaultEffectOffset,
                SparksYOffset = PluginConfig.DefaultEffectOffset,
                SparksZOffset = PluginConfig.DefaultEffectOffset,
                SparksXRotation = PluginConfig.DefaultEffectRotation,
                SparksYRotation = PluginConfig.DefaultEffectRotation,
                SparksZRotation = PluginConfig.DefaultEffectRotation,

                FlareEnabled = true,
                FlareScale = 1f,
                FlareLuminance = PluginConfig.DefaultLuminance,
                FlareHue = PluginConfig.DefaultHue,
                FlareXOffset = PluginConfig.DefaultEffectOffset,
                FlareYOffset = PluginConfig.DefaultEffectOffset,
                FlareZOffset = PluginConfig.DefaultEffectOffset,

                AuraEnabled = true,
                AuraScale = PluginConfig.DefaultAuraScale,
                AuraLuminance = PluginConfig.DefaultLuminance,
                AuraHue = PluginConfig.DefaultHue,
                AuraXOffset = PluginConfig.DefaultEffectOffset,
                AuraYOffset = PluginConfig.DefaultEffectOffset,
                AuraZOffset = PluginConfig.DefaultEffectOffset,
                AuraXRotation = PluginConfig.DefaultEffectRotation,
                AuraYRotation = PluginConfig.DefaultEffectRotation,
                AuraZRotation = PluginConfig.DefaultEffectRotation,

                OrbitalsOrbsEnabled = true,
                OrbitalsOrbsGlueEnabled = false,
                OrbitalsOrbsSnakeEnabled = false,
                OrbitalsOrbsCount = PluginConfig.DefaultCountNormalized,
                OrbitalsOrbsScale = 1f,
                OrbitalsOrbsLuminance = PluginConfig.DefaultLuminance,
                OrbitalsOrbsHue = PluginConfig.DefaultHue,
                OrbitalsOrbsLength = PluginConfig.DefaultOrbitalsLength,
                OrbitalsOrbsSpeed = PluginConfig.DefaultOrbitalsSpeed,
                OrbitalsOrbsSpacing = PluginConfig.DefaultOrbitalsSpacing,
                OrbitalsOrbsRadius = PluginConfig.DefaultOrbitalsRadiusMultiplier,
                OrbitalsOrbsCycles = PluginConfig.DefaultOrbitalsCycles,
                OrbitalsOrbsXOffset = PluginConfig.DefaultEffectOffset,
                OrbitalsOrbsYOffset = PluginConfig.DefaultEffectOffset,
                OrbitalsOrbsZOffset = PluginConfig.DefaultEffectOffset,
                OrbitalsOrbsXRotation = PluginConfig.DefaultEffectRotation,
                OrbitalsOrbsYRotation = PluginConfig.DefaultEffectRotation,
                OrbitalsOrbsZRotation = PluginConfig.DefaultEffectRotation,
                OrbitalsOrbsDrift = PluginConfig.DefaultDrift,

                OrbitalsCoresEnabled = true,
                OrbitalsCoresSnakeEnabled =  false,
                OrbitalsCoresGlueEnabled = false,
                OrbitalsCoresSpinEnabled = true,
                OrbitalsCoresCount = PluginConfig.DefaultCountNormalized,
                OrbitalsCoresScale = 1f,
                OrbitalsCoresLuminance = PluginConfig.DefaultLuminance,
                OrbitalsCoresHue = PluginConfig.DefaultHue,
                OrbitalsCoresSpinSpeed = PluginConfig.DefaultCoreSpinSpeed,
                OrbitalsCoresLength = PluginConfig.DefaultOrbitalsLength,
                OrbitalsCoresSpeed = PluginConfig.DefaultOrbitalsSpeed,
                OrbitalsCoresSpacing = PluginConfig.DefaultOrbitalsSpacing,
                OrbitalsCoresRadius = PluginConfig.DefaultOrbitalsRadiusMultiplier,
                OrbitalsCoresCycles = PluginConfig.DefaultOrbitalsCycles,
                OrbitalsCoresXOffset = PluginConfig.DefaultEffectOffset,
                OrbitalsCoresYOffset = PluginConfig.DefaultEffectOffset,
                OrbitalsCoresZOffset = PluginConfig.DefaultEffectOffset,
                OrbitalsCoresXRotation = PluginConfig.DefaultEffectRotation,
                OrbitalsCoresYRotation = PluginConfig.DefaultEffectRotation,
                OrbitalsCoresZRotation = PluginConfig.DefaultEffectRotation,
                OrbitalsCoresDrift = PluginConfig.DefaultDrift,

                OrbitalsFlamesEnabled = true,
                OrbitalsFlamesCount = PluginConfig.DefaultCountNormalized,
                OrbitalsFlamesEnergy = PluginConfig.DefaultEnergy,
                OrbitalsFlamesScale = 1f,
                OrbitalsFlamesLuminance = PluginConfig.DefaultLuminance,
                OrbitalsFlamesHue = PluginConfig.DefaultHue,
                OrbitalsFlamesLifetime = PluginConfig.DefaultLifetime,
                OrbitalsFlamesSimulationSpeed =  PluginConfig.DefaultSimulationSpeed,
                OrbitalsFlamesLength = PluginConfig.DefaultOrbitalsLength,
                OrbitalsFlamesSpeed = PluginConfig.DefaultOrbitalsSpeed,
                OrbitalsFlamesSpacing = PluginConfig.DefaultOrbitalsSpacing,
                OrbitalsFlamesRadius = PluginConfig.DefaultOrbitalsRadiusMultiplier,
                OrbitalsFlamesCycles = PluginConfig.DefaultOrbitalsCycles,
                OrbitalsFlamesXOffset = PluginConfig.DefaultEffectOffset,
                OrbitalsFlamesYOffset = PluginConfig.DefaultEffectOffset,
                OrbitalsFlamesZOffset = PluginConfig.DefaultEffectOffset,
                OrbitalsFlamesXRotation = PluginConfig.DefaultEffectRotation,
                OrbitalsFlamesYRotation = PluginConfig.DefaultEffectRotation,
                OrbitalsFlamesZRotation = PluginConfig.DefaultEffectRotation,
                OrbitalsFlamesDrift = PluginConfig.DefaultDrift,

                OrbitalsEmbersEnabled = true,
                OrbitalsEmbersCount = PluginConfig.DefaultCountNormalized,
                OrbitalsEmbersEnergy = PluginConfig.DefaultEnergy,
                OrbitalsEmbersScale = 1f,
                OrbitalsEmbersLuminance = PluginConfig.DefaultLuminance,
                OrbitalsEmbersHue = PluginConfig.DefaultHue,
                OrbitalsEmbersLifetime = PluginConfig.DefaultLifetime,
                OrbitalsEmbersSimulationSpeed = PluginConfig.DefaultSimulationSpeed,
                OrbitalsEmbersLength = PluginConfig.DefaultOrbitalsLength,
                OrbitalsEmbersSpeed = PluginConfig.DefaultOrbitalsSpeed,
                OrbitalsEmbersSpacing = PluginConfig.DefaultOrbitalsSpacing,
                OrbitalsEmbersRadius = PluginConfig.DefaultOrbitalsRadiusMultiplier,
                OrbitalsEmbersCycles = PluginConfig.DefaultOrbitalsCycles,
                OrbitalsEmbersXOffset = PluginConfig.DefaultEffectOffset,
                OrbitalsEmbersYOffset = PluginConfig.DefaultEffectOffset,
                OrbitalsEmbersZOffset = PluginConfig.DefaultEffectOffset,
                OrbitalsEmbersXRotation = PluginConfig.DefaultEffectRotation,
                OrbitalsEmbersYRotation = PluginConfig.DefaultEffectRotation,
                OrbitalsEmbersZRotation = PluginConfig.DefaultEffectRotation,
                OrbitalsEmbersDrift = PluginConfig.DefaultDrift
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
            PluginConfig.InnerFlamesWorld.Value = state.InnerFlamesWorldEnabled;
            PluginConfig.InnerFlamesBlack.Value = state.InnerFlamesBlackEnabled;
            PluginConfig.InnerFlamesWhite.Value = state.InnerFlamesWhiteEnabled;
            PluginConfig.InnerFlamesEnergy.Value = state.InnerFlamesEnergy;
            PluginConfig.InnerFlamesScale.Value = state.InnerFlamesScale;
            PluginConfig.InnerFlamesLuminance.Value = state.InnerFlamesLuminance;
            PluginConfig.InnerFlamesHue.Value = state.InnerFlamesHue;
            PluginConfig.InnerFlamesLifetime.Value = state.InnerFlamesLifetime;
            PluginConfig.InnerFlamesSimulationSpeed.Value = state.InnerFlamesSimulationSpeed;
            PluginConfig.InnerFlamesLength.Value = state.InnerFlamesLength;
            PluginConfig.InnerFlamesWidth.Value = state.InnerFlamesWidth;
            PluginConfig.InnerFlamesXOffset.Value = state.InnerFlamesXOffset;
            PluginConfig.InnerFlamesYOffset.Value = state.InnerFlamesYOffset;
            PluginConfig.InnerFlamesZOffset.Value = state.InnerFlamesZOffset;
            PluginConfig.InnerFlamesXRotation.Value = state.InnerFlamesXRotation;
            PluginConfig.InnerFlamesYRotation.Value = state.InnerFlamesYRotation;
            PluginConfig.InnerFlamesZRotation.Value = state.InnerFlamesZRotation;

            PluginConfig.OuterFlames.Value = state.OuterFlamesEnabled;
            PluginConfig.OuterFlamesWorld.Value = state.OuterFlamesWorldEnabled;
            PluginConfig.OuterFlamesBlack.Value = state.OuterFlamesBlackEnabled;
            PluginConfig.OuterFlamesWhite.Value = state.OuterFlamesWhiteEnabled;
            PluginConfig.OuterFlamesDragEnabled.Value = state.OuterFlamesDragEnabled;
            PluginConfig.OuterFlamesEnergy.Value = state.OuterFlamesEnergy;
            PluginConfig.OuterFlamesScale.Value = state.OuterFlamesScale;
            PluginConfig.OuterFlamesLuminance.Value = state.OuterFlamesLuminance;
            PluginConfig.OuterFlamesHue.Value = state.OuterFlamesHue;
            PluginConfig.OuterFlamesLifetime.Value = state.OuterFlamesLifetime;
            PluginConfig.OuterFlamesSimulationSpeed.Value = state.OuterFlamesSimulationSpeed;
            PluginConfig.OuterFlamesLength.Value = state.OuterFlamesLength;
            PluginConfig.OuterFlamesWidth.Value = state.OuterFlamesWidth;
            PluginConfig.OuterFlamesXOffset.Value = state.OuterFlamesXOffset;
            PluginConfig.OuterFlamesYOffset.Value = state.OuterFlamesYOffset;
            PluginConfig.OuterFlamesZOffset.Value = state.OuterFlamesZOffset;
            PluginConfig.OuterFlamesXRotation.Value = state.OuterFlamesXRotation;
            PluginConfig.OuterFlamesYRotation.Value = state.OuterFlamesYRotation;
            PluginConfig.OuterFlamesZRotation.Value = state.OuterFlamesZRotation;

            PluginConfig.Strands.Value = state.StrandsEnabled;
            PluginConfig.StrandsSpectrum.Value = state.StrandsSpectrumEnabled;
            PluginConfig.StrandsEnergy.Value = state.StrandsEnergy;
            PluginConfig.StrandsScaleWhole.Value = state.StrandsScaleWhole;
            PluginConfig.StrandsScaleParts.Value = state.StrandsScaleParts;
            PluginConfig.StrandsLuminance.Value = state.StrandsLuminance;
            PluginConfig.StrandsHue.Value = state.StrandsHue;
            PluginConfig.StrandsLifetime.Value = state.StrandsLifetime;
            PluginConfig.StrandsLength.Value = state.StrandsLength;
            PluginConfig.StrandsSpectrumSpeed.Value = state.StrandsSpectrumSpeed;
            PluginConfig.StrandsSpeed.Value = state.StrandsSpeed;
            PluginConfig.StrandsRadius.Value = state.StrandsRadius;
            PluginConfig.StrandsXOffset.Value = state.StrandsXOffset;
            PluginConfig.StrandsYOffset.Value = state.StrandsYOffset;
            PluginConfig.StrandsZOffset.Value = state.StrandsZOffset;
            PluginConfig.StrandsXRotation.Value = state.StrandsXRotation;
            PluginConfig.StrandsYRotation.Value = state.StrandsYRotation;
            PluginConfig.StrandsZRotation.Value = state.StrandsZRotation;
            PluginConfig.StrandsDrift.Value = state.StrandsDrift;

            PluginConfig.Sparks.Value = state.SparksEnabled;
            PluginConfig.SparksEnergy.Value = state.SparksEnergy;
            PluginConfig.SparksScale.Value = state.SparksScale;
            PluginConfig.SparksLuminance.Value = state.SparksLuminance;
            PluginConfig.SparksHue.Value = state.SparksHue;
            PluginConfig.SparksLifetime.Value = state.SparksLifetime;
            PluginConfig.SparksSimulationSpeed.Value = state.SparksSimulationSpeed;
            PluginConfig.SparksLength.Value = state.SparksLength;
            PluginConfig.SparksWidth.Value = state.SparksWidth;
            PluginConfig.SparksXOffset.Value = state.SparksXOffset;
            PluginConfig.SparksYOffset.Value = state.SparksYOffset;
            PluginConfig.SparksZOffset.Value = state.SparksZOffset;
            PluginConfig.SparksXRotation.Value = state.SparksXRotation;
            PluginConfig.SparksYRotation.Value = state.SparksYRotation;
            PluginConfig.SparksZRotation.Value = state.SparksZRotation;

            PluginConfig.Flare.Value = state.FlareEnabled;
            PluginConfig.FlareScale.Value = state.FlareScale;
            PluginConfig.FlareLuminance.Value = state.FlareLuminance;
            PluginConfig.FlareHue.Value = state.FlareHue;
            PluginConfig.FlareXOffset.Value = state.FlareXOffset;
            PluginConfig.FlareYOffset.Value = state.FlareYOffset;
            PluginConfig.FlareZOffset.Value = state.FlareZOffset;

            PluginConfig.Aura.Value = state.AuraEnabled;
            PluginConfig.AuraScale.Value = state.AuraScale;
            PluginConfig.AuraHue.Value = state.AuraHue;
            PluginConfig.AuraLuminance.Value = state.AuraLuminance;
            PluginConfig.AuraXOffset.Value = state.AuraXOffset;
            PluginConfig.AuraYOffset.Value = state.AuraYOffset;
            PluginConfig.AuraZOffset.Value = state.AuraZOffset;
            PluginConfig.AuraXRotation.Value = state.AuraXRotation;
            PluginConfig.AuraYRotation.Value = state.AuraYRotation;
            PluginConfig.AuraZRotation.Value = state.AuraZRotation;

            PluginConfig.OrbitalsOrbs.Value = state.OrbitalsOrbsEnabled;
            PluginConfig.OrbitalsOrbsGlue.Value = state.OrbitalsOrbsGlueEnabled;
            PluginConfig.OrbitalsOrbsSnake.Value = state.OrbitalsOrbsSnakeEnabled;
            PluginConfig.OrbitalsOrbsCount.Value = state.OrbitalsOrbsCount;
            PluginConfig.OrbitalsOrbsScale.Value = state.OrbitalsOrbsScale;
            PluginConfig.OrbitalsOrbsLuminance.Value = state.OrbitalsOrbsLuminance;
            PluginConfig.OrbitalsOrbsHue.Value = state.OrbitalsOrbsHue;
            PluginConfig.OrbitalsOrbsLength.Value = state.OrbitalsOrbsLength;
            PluginConfig.OrbitalsOrbsSpeed.Value = state.OrbitalsOrbsSpeed;
            PluginConfig.OrbitalsOrbsSpacing.Value = state.OrbitalsOrbsSpacing;
            PluginConfig.OrbitalsOrbsRadius.Value = state.OrbitalsOrbsRadius;
            PluginConfig.OrbitalsOrbsCycles.Value = state.OrbitalsOrbsCycles;
            PluginConfig.OrbitalsOrbsXOffset.Value = state.OrbitalsOrbsXOffset;
            PluginConfig.OrbitalsOrbsYOffset.Value = state.OrbitalsOrbsYOffset;
            PluginConfig.OrbitalsOrbsZOffset.Value = state.OrbitalsOrbsZOffset;
            PluginConfig.OrbitalsOrbsXRotation.Value = state.OrbitalsOrbsXRotation;
            PluginConfig.OrbitalsOrbsYRotation.Value = state.OrbitalsOrbsYRotation;
            PluginConfig.OrbitalsOrbsZRotation.Value = state.OrbitalsOrbsZRotation;
            PluginConfig.OrbitalsOrbsDrift.Value = state.OrbitalsOrbsDrift;

            PluginConfig.OrbitalsCores.Value = state.OrbitalsCoresEnabled;
            PluginConfig.OrbitalsCoresSnake.Value = state.OrbitalsCoresSnakeEnabled;
            PluginConfig.OrbitalsCoresGlue.Value = state.OrbitalsCoresGlueEnabled;
            PluginConfig.OrbitalsCoresSpin.Value = state.OrbitalsCoresSpinEnabled;
            PluginConfig.OrbitalsCoresCount.Value = state.OrbitalsCoresCount;
            PluginConfig.OrbitalsCoresScale.Value = state.OrbitalsCoresScale;
            PluginConfig.OrbitalsCoresLuminance.Value = state.OrbitalsCoresLuminance;
            PluginConfig.OrbitalsCoresHue.Value = state.OrbitalsCoresHue;
            PluginConfig.OrbitalsCoresSpinSpeed.Value = state.OrbitalsCoresSpinSpeed;
            PluginConfig.OrbitalsCoresLength.Value = state.OrbitalsCoresLength;
            PluginConfig.OrbitalsCoresSpeed.Value = state.OrbitalsCoresSpeed;
            PluginConfig.OrbitalsCoresSpacing.Value = state.OrbitalsCoresSpacing;
            PluginConfig.OrbitalsCoresRadius.Value = state.OrbitalsCoresRadius;
            PluginConfig.OrbitalsCoresCycles.Value = state.OrbitalsCoresCycles;
            PluginConfig.OrbitalsCoresXOffset.Value = state.OrbitalsCoresXOffset;
            PluginConfig.OrbitalsCoresYOffset.Value = state.OrbitalsCoresYOffset;
            PluginConfig.OrbitalsCoresZOffset.Value = state.OrbitalsCoresZOffset;
            PluginConfig.OrbitalsCoresXRotation.Value = state.OrbitalsCoresXRotation;
            PluginConfig.OrbitalsCoresYRotation.Value = state.OrbitalsCoresYRotation;
            PluginConfig.OrbitalsCoresZRotation.Value = state.OrbitalsCoresZRotation;
            PluginConfig.OrbitalsCoresDrift.Value = state.OrbitalsCoresDrift;

            PluginConfig.OrbitalsFlames.Value = state.OrbitalsFlamesEnabled;
            PluginConfig.OrbitalsFlamesCount.Value = state.OrbitalsFlamesCount;
            PluginConfig.OrbitalsFlamesEnergy.Value = state.OrbitalsFlamesEnergy;
            PluginConfig.OrbitalsFlamesScale.Value = state.OrbitalsFlamesScale;
            PluginConfig.OrbitalsFlamesLuminance.Value = state.OrbitalsFlamesLuminance;
            PluginConfig.OrbitalsFlamesHue.Value = state.OrbitalsFlamesHue;
            PluginConfig.OrbitalsFlamesLifetime.Value = state.OrbitalsFlamesLifetime;
            PluginConfig.OrbitalsFlamesSimulationSpeed.Value = state.OrbitalsFlamesSimulationSpeed;
            PluginConfig.OrbitalsFlamesLength.Value = state.OrbitalsFlamesLength;
            PluginConfig.OrbitalsFlamesSpeed.Value = state.OrbitalsFlamesSpeed;
            PluginConfig.OrbitalsFlamesSpacing.Value = state.OrbitalsFlamesSpacing;
            PluginConfig.OrbitalsFlamesRadius.Value = state.OrbitalsFlamesRadius;
            PluginConfig.OrbitalsFlamesCycles.Value = state.OrbitalsFlamesCycles;
            PluginConfig.OrbitalsFlamesXOffset.Value = state.OrbitalsFlamesXOffset;
            PluginConfig.OrbitalsFlamesYOffset.Value = state.OrbitalsFlamesYOffset;
            PluginConfig.OrbitalsFlamesZOffset.Value = state.OrbitalsFlamesZOffset;
            PluginConfig.OrbitalsFlamesXRotation.Value = state.OrbitalsFlamesXRotation;
            PluginConfig.OrbitalsFlamesYRotation.Value = state.OrbitalsFlamesYRotation;
            PluginConfig.OrbitalsFlamesZRotation.Value = state.OrbitalsFlamesZRotation;
            PluginConfig.OrbitalsFlamesDrift.Value = state.OrbitalsFlamesDrift;

            PluginConfig.OrbitalsEmbers.Value = state.OrbitalsEmbersEnabled;
            PluginConfig.OrbitalsEmbersCount.Value = state.OrbitalsEmbersCount;
            PluginConfig.OrbitalsEmbersEnergy.Value = state.OrbitalsEmbersEnergy;
            PluginConfig.OrbitalsEmbersScale.Value = state.OrbitalsEmbersScale;
            PluginConfig.OrbitalsEmbersLuminance.Value = state.OrbitalsEmbersLuminance;
            PluginConfig.OrbitalsEmbersHue.Value = state.OrbitalsEmbersHue;
            PluginConfig.OrbitalsEmbersLifetime.Value = state.OrbitalsEmbersLifetime;
            PluginConfig.OrbitalsEmbersSimulationSpeed.Value = state.OrbitalsEmbersSimulationSpeed;
            PluginConfig.OrbitalsEmbersLength.Value = state.OrbitalsEmbersLength;
            PluginConfig.OrbitalsEmbersSpeed.Value = state.OrbitalsEmbersSpeed;
            PluginConfig.OrbitalsEmbersSpacing.Value = state.OrbitalsEmbersSpacing;
            PluginConfig.OrbitalsEmbersRadius.Value = state.OrbitalsEmbersRadius;
            PluginConfig.OrbitalsEmbersCycles.Value = state.OrbitalsEmbersCycles;
            PluginConfig.OrbitalsEmbersXOffset.Value = state.OrbitalsEmbersXOffset;
            PluginConfig.OrbitalsEmbersYOffset.Value = state.OrbitalsEmbersYOffset;
            PluginConfig.OrbitalsEmbersZOffset.Value = state.OrbitalsEmbersZOffset;
            PluginConfig.OrbitalsEmbersXRotation.Value = state.OrbitalsEmbersXRotation;
            PluginConfig.OrbitalsEmbersYRotation.Value = state.OrbitalsEmbersYRotation;
            PluginConfig.OrbitalsEmbersZRotation.Value = state.OrbitalsEmbersZRotation;
            PluginConfig.OrbitalsEmbersDrift.Value = state.OrbitalsEmbersDrift;
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
