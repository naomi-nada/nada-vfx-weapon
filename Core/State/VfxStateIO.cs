using System.Collections.Generic;
using System.Globalization;
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
                InnerFlamesSimulationSpeed = PluginConfig.InnerFlamesSimulationSpeed.Value,
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
                SparksSimulationSpeed = PluginConfig.SparksSimulationSpeed.Value,
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
                OrbitalsFlamesSimulationSpeed = PluginConfig.OrbitalsFlamesSimulationSpeed.Value,
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

            Write(
                item.m_customData,
                state,
                bound);
        }

        private static void Write(
            Dictionary<string, string> customData,
            VfxState state,
            bool bound)
        {
            if (customData == null)
                return;

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
            WriteFloat(customData, VfxStateKeys.OrbitalsEmbersSimulationSpeed, state.OrbitalsEmbersSimulationSpeed);
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

            if (item?.m_customData == null)
                return false;

            Dictionary<string, string> customData =
                item.m_customData;

            // Bound is the marker that tells us this ItemData actually contains a persisted NADA state.
            // Other mods use customData too, so the dictionary existing by itself isn't enough.
            if (!customData.ContainsKey(VfxStateKeys.Bound))
                return false;

            return TryRead(
                customData,
                FromConfig(),
                out state);
        }

        private static bool TryRead(
            Dictionary<string, string> customData,
            VfxState fallbackState,
            out VfxState state)
        {
            state = fallbackState;

            if (customData == null)
                return false;

            state.RigXOffset = ReadFloat(customData, VfxStateKeys.RigXOffset, state.RigXOffset);
            state.RigYOffset = ReadFloat(customData, VfxStateKeys.RigYOffset, state.RigYOffset);
            state.RigZOffset = ReadFloat(customData, VfxStateKeys.RigZOffset, state.RigZOffset);
            state.RigXRotation = ReadFloat(customData, VfxStateKeys.RigXRotation, state.RigXRotation);
            state.RigYRotation = ReadFloat(customData, VfxStateKeys.RigYRotation, state.RigYRotation);
            state.RigZRotation = ReadFloat(customData, VfxStateKeys.RigZRotation, state.RigZRotation);

            state.InnerFlamesEnabled = ReadBool(customData, VfxStateKeys.InnerFlamesEnabled, state.InnerFlamesEnabled);
            state.InnerFlamesWorldEnabled = ReadBool(customData, VfxStateKeys.InnerFlamesWorldEnabled, state.InnerFlamesWorldEnabled);
            state.InnerFlamesBlackEnabled = ReadBool(customData, VfxStateKeys.InnerFlamesBlackEnabled, state.InnerFlamesBlackEnabled);
            state.InnerFlamesWhiteEnabled = ReadBool(customData, VfxStateKeys.InnerFlamesWhiteEnabled, state.InnerFlamesWhiteEnabled);
            state.InnerFlamesEnergy = ReadFloat(customData, VfxStateKeys.InnerFlamesEnergy, state.InnerFlamesEnergy);
            state.InnerFlamesScale = ReadFloat(customData, VfxStateKeys.InnerFlamesScale, state.InnerFlamesScale);
            state.InnerFlamesLuminance = ReadFloat(customData, VfxStateKeys.InnerFlamesLuminance, state.InnerFlamesLuminance);
            state.InnerFlamesHue = ReadFloat(customData, VfxStateKeys.InnerFlamesHue, state.InnerFlamesHue);
            state.InnerFlamesLifetime = ReadFloat(customData, VfxStateKeys.InnerFlamesLifetime, state.InnerFlamesLifetime);
            state.InnerFlamesSimulationSpeed = ReadFloat(customData, VfxStateKeys.InnerFlamesSimulationSpeed, state.InnerFlamesSimulationSpeed);
            state.InnerFlamesLength = ReadFloat(customData, VfxStateKeys.InnerFlamesLength, state.InnerFlamesLength);
            state.InnerFlamesWidth = ReadFloat(customData, VfxStateKeys.InnerFlamesWidth, state.InnerFlamesWidth);
            state.InnerFlamesXOffset = ReadFloat(customData, VfxStateKeys.InnerFlamesXOffset, state.InnerFlamesXOffset);
            state.InnerFlamesYOffset = ReadFloat(customData, VfxStateKeys.InnerFlamesYOffset, state.InnerFlamesYOffset);
            state.InnerFlamesZOffset = ReadFloat(customData, VfxStateKeys.InnerFlamesZOffset, state.InnerFlamesZOffset);
            state.InnerFlamesXRotation = ReadFloat(customData, VfxStateKeys.InnerFlamesXRotation, state.InnerFlamesXRotation);
            state.InnerFlamesYRotation = ReadFloat(customData, VfxStateKeys.InnerFlamesYRotation, state.InnerFlamesYRotation);
            state.InnerFlamesZRotation = ReadFloat(customData, VfxStateKeys.InnerFlamesZRotation, state.InnerFlamesZRotation);

            state.OuterFlamesEnabled = ReadBool(customData, VfxStateKeys.OuterFlamesEnabled, state.OuterFlamesEnabled);
            state.OuterFlamesWorldEnabled = ReadBool(customData, VfxStateKeys.OuterFlamesWorldEnabled, state.OuterFlamesWorldEnabled);
            state.OuterFlamesBlackEnabled = ReadBool(customData, VfxStateKeys.OuterFlamesBlackEnabled, state.OuterFlamesBlackEnabled);
            state.OuterFlamesWhiteEnabled = ReadBool(customData, VfxStateKeys.OuterFlamesWhiteEnabled, state.OuterFlamesWhiteEnabled);
            state.OuterFlamesDragEnabled = ReadBool(customData, VfxStateKeys.OuterFlamesDragEnabled, state.OuterFlamesDragEnabled);
            state.OuterFlamesEnergy = ReadFloat(customData, VfxStateKeys.OuterFlamesEnergy, state.OuterFlamesEnergy);
            state.OuterFlamesScale = ReadFloat(customData, VfxStateKeys.OuterFlamesScale, state.OuterFlamesScale);
            state.OuterFlamesLuminance = ReadFloat(customData, VfxStateKeys.OuterFlamesLuminance, state.OuterFlamesLuminance);
            state.OuterFlamesHue = ReadFloat(customData, VfxStateKeys.OuterFlamesHue, state.OuterFlamesHue);
            state.OuterFlamesLifetime = ReadFloat(customData, VfxStateKeys.OuterFlamesLifetime, state.OuterFlamesLifetime);
            state.OuterFlamesSimulationSpeed = ReadFloat(customData, VfxStateKeys.OuterFlamesSimulationSpeed, state.OuterFlamesSimulationSpeed);
            state.OuterFlamesLength = ReadFloat(customData, VfxStateKeys.OuterFlamesLength, state.OuterFlamesLength);
            state.OuterFlamesWidth = ReadFloat(customData, VfxStateKeys.OuterFlamesWidth, state.OuterFlamesWidth);
            state.OuterFlamesXOffset = ReadFloat(customData, VfxStateKeys.OuterFlamesXOffset, state.OuterFlamesXOffset);
            state.OuterFlamesYOffset = ReadFloat(customData, VfxStateKeys.OuterFlamesYOffset, state.OuterFlamesYOffset);
            state.OuterFlamesZOffset = ReadFloat(customData, VfxStateKeys.OuterFlamesZOffset, state.OuterFlamesZOffset);
            state.OuterFlamesXRotation = ReadFloat(customData, VfxStateKeys.OuterFlamesXRotation, state.OuterFlamesXRotation);
            state.OuterFlamesYRotation = ReadFloat(customData, VfxStateKeys.OuterFlamesYRotation, state.OuterFlamesYRotation);
            state.OuterFlamesZRotation = ReadFloat(customData, VfxStateKeys.OuterFlamesZRotation, state.OuterFlamesZRotation);

            state.StrandsEnabled = ReadBool(customData, VfxStateKeys.StrandsEnabled, state.StrandsEnabled);
            state.StrandsSpectrumEnabled = ReadBool(customData, VfxStateKeys.StrandsSpectrumEnabled, state.StrandsSpectrumEnabled);
            state.StrandsEnergy = ReadFloat(customData, VfxStateKeys.StrandsEnergy, state.StrandsEnergy);
            state.StrandsScaleWhole = ReadFloat(customData, VfxStateKeys.StrandsScaleWhole, state.StrandsScaleWhole);
            state.StrandsScaleParts = ReadFloat(customData, VfxStateKeys.StrandsScaleParts, state.StrandsScaleParts);
            state.StrandsLuminance = ReadFloat(customData, VfxStateKeys.StrandsLuminance, state.StrandsLuminance);
            state.StrandsHue = ReadFloat(customData, VfxStateKeys.StrandsHue, state.StrandsHue);
            state.StrandsLifetime = ReadFloat(customData, VfxStateKeys.StrandsLifetime, state.StrandsLifetime);
            state.StrandsLength = ReadFloat(customData, VfxStateKeys.StrandsLength, state.StrandsLength);
            state.StrandsSpectrumSpeed = ReadFloat(customData, VfxStateKeys.StrandsSpectrumSpeed, state.StrandsSpectrumSpeed);
            state.StrandsSpeed = ReadFloat(customData, VfxStateKeys.StrandsSpeed, state.StrandsSpeed);
            state.StrandsRadius = ReadFloat(customData, VfxStateKeys.StrandsRadius, state.StrandsRadius);
            state.StrandsXOffset = ReadFloat(customData, VfxStateKeys.StrandsXOffset, state.StrandsXOffset);
            state.StrandsYOffset = ReadFloat(customData, VfxStateKeys.StrandsYOffset, state.StrandsYOffset);
            state.StrandsZOffset = ReadFloat(customData, VfxStateKeys.StrandsZOffset, state.StrandsZOffset);
            state.StrandsXRotation = ReadFloat(customData, VfxStateKeys.StrandsXRotation, state.StrandsXRotation);
            state.StrandsYRotation = ReadFloat(customData, VfxStateKeys.StrandsYRotation, state.StrandsYRotation);
            state.StrandsZRotation = ReadFloat(customData, VfxStateKeys.StrandsZRotation, state.StrandsZRotation);
            state.StrandsDrift = ReadFloat(customData, VfxStateKeys.StrandsDrift, state.StrandsDrift);

            state.SparksEnabled = ReadBool(customData, VfxStateKeys.SparksEnabled, state.SparksEnabled);
            state.SparksEnergy = ReadFloat(customData, VfxStateKeys.SparksEnergy, state.SparksEnergy);
            state.SparksScale = ReadFloat(customData, VfxStateKeys.SparksScale, state.SparksScale);
            state.SparksLuminance = ReadFloat(customData, VfxStateKeys.SparksLuminance, state.SparksLuminance);
            state.SparksHue = ReadFloat(customData, VfxStateKeys.SparksHue, state.SparksHue);
            state.SparksLifetime = ReadFloat(customData, VfxStateKeys.SparksLifetime, state.SparksLifetime);
            state.SparksSimulationSpeed = ReadFloat(customData, VfxStateKeys.SparksSimulationSpeed, state.SparksSimulationSpeed);
            state.SparksLength = ReadFloat(customData, VfxStateKeys.SparksLength, state.SparksLength);
            state.SparksWidth = ReadFloat(customData, VfxStateKeys.SparksWidth, state.SparksWidth);
            state.SparksXOffset = ReadFloat(customData, VfxStateKeys.SparksXOffset, state.SparksXOffset);
            state.SparksYOffset = ReadFloat(customData, VfxStateKeys.SparksYOffset, state.SparksYOffset);
            state.SparksZOffset = ReadFloat(customData, VfxStateKeys.SparksZOffset, state.SparksZOffset);
            state.SparksXRotation = ReadFloat(customData, VfxStateKeys.SparksXRotation, state.SparksXRotation);
            state.SparksYRotation = ReadFloat(customData, VfxStateKeys.SparksYRotation, state.SparksYRotation);
            state.SparksZRotation = ReadFloat(customData, VfxStateKeys.SparksZRotation, state.SparksZRotation);

            state.FlareEnabled = ReadBool(customData, VfxStateKeys.FlareEnabled, state.FlareEnabled);
            state.FlareScale = ReadFloat(customData, VfxStateKeys.FlareScale, state.FlareScale);
            state.FlareLuminance = ReadFloat(customData, VfxStateKeys.FlareLuminance, state.FlareLuminance);
            state.FlareHue = ReadFloat(customData, VfxStateKeys.FlareHue, state.FlareHue);
            state.FlareXOffset = ReadFloat(customData, VfxStateKeys.FlareXOffset, state.FlareXOffset);
            state.FlareYOffset = ReadFloat(customData, VfxStateKeys.FlareYOffset, state.FlareYOffset);
            state.FlareZOffset = ReadFloat(customData, VfxStateKeys.FlareZOffset, state.FlareZOffset);

            state.AuraEnabled = ReadBool(customData, VfxStateKeys.AuraEnabled, state.AuraEnabled);
            state.AuraScale = ReadFloat(customData, VfxStateKeys.AuraScale, state.AuraScale);
            state.AuraLuminance = ReadFloat(customData, VfxStateKeys.AuraLuminance, state.AuraLuminance);
            state.AuraHue = ReadFloat(customData, VfxStateKeys.AuraHue, state.AuraHue);
            state.AuraXOffset = ReadFloat(customData, VfxStateKeys.AuraXOffset, state.AuraXOffset);
            state.AuraYOffset = ReadFloat(customData, VfxStateKeys.AuraYOffset, state.AuraYOffset);
            state.AuraZOffset = ReadFloat(customData, VfxStateKeys.AuraZOffset, state.AuraZOffset);
            state.AuraXRotation = ReadFloat(customData, VfxStateKeys.AuraXRotation, state.AuraXRotation);
            state.AuraYRotation = ReadFloat(customData, VfxStateKeys.AuraYRotation, state.AuraYRotation);
            state.AuraZRotation = ReadFloat(customData, VfxStateKeys.AuraZRotation, state.AuraZRotation);

            state.OrbitalsOrbsEnabled = ReadBool(customData, VfxStateKeys.OrbitalsOrbsEnabled, state.OrbitalsOrbsEnabled);
            state.OrbitalsOrbsSnakeEnabled = ReadBool(customData, VfxStateKeys.OrbitalsOrbsSnakeEnabled, state.OrbitalsOrbsSnakeEnabled);
            state.OrbitalsOrbsGlueEnabled = ReadBool(customData, VfxStateKeys.OrbitalsOrbsGlueEnabled, state.OrbitalsOrbsGlueEnabled);
            state.OrbitalsOrbsCount = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsCount, state.OrbitalsOrbsCount);
            state.OrbitalsOrbsScale = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsScale, state.OrbitalsOrbsScale);
            state.OrbitalsOrbsLuminance = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsLuminance, state.OrbitalsOrbsLuminance);
            state.OrbitalsOrbsHue = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsHue, state.OrbitalsOrbsHue);
            state.OrbitalsOrbsLength = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsLength, state.OrbitalsOrbsLength);
            state.OrbitalsOrbsSpeed = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsSpeed, state.OrbitalsOrbsSpeed);
            state.OrbitalsOrbsSpacing = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsSpacing, state.OrbitalsOrbsSpacing);
            state.OrbitalsOrbsRadius = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsRadius, state.OrbitalsOrbsRadius);
            state.OrbitalsOrbsCycles = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsCycles, state.OrbitalsOrbsCycles);
            state.OrbitalsOrbsXOffset = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsXOffset, state.OrbitalsOrbsXOffset);
            state.OrbitalsOrbsYOffset = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsYOffset, state.OrbitalsOrbsYOffset);
            state.OrbitalsOrbsZOffset = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsZOffset, state.OrbitalsOrbsZOffset);
            state.OrbitalsOrbsXRotation = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsXRotation, state.OrbitalsOrbsXRotation);
            state.OrbitalsOrbsYRotation = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsYRotation, state.OrbitalsOrbsYRotation);
            state.OrbitalsOrbsZRotation = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsZRotation, state.OrbitalsOrbsZRotation);
            state.OrbitalsOrbsDrift = ReadFloat(customData, VfxStateKeys.OrbitalsOrbsDrift, state.OrbitalsOrbsDrift);

            state.OrbitalsCoresEnabled = ReadBool(customData, VfxStateKeys.OrbitalsCoresEnabled, state.OrbitalsCoresEnabled);
            state.OrbitalsCoresSnakeEnabled = ReadBool(customData, VfxStateKeys.OrbitalsCoresSnakeEnabled, state.OrbitalsCoresSnakeEnabled);
            state.OrbitalsCoresGlueEnabled = ReadBool(customData, VfxStateKeys.OrbitalsCoresGlueEnabled, state.OrbitalsCoresGlueEnabled);
            state.OrbitalsCoresSpinEnabled = ReadBool(customData, VfxStateKeys.OrbitalsCoresSpinEnabled, state.OrbitalsCoresSpinEnabled);
            state.OrbitalsCoresCount = ReadFloat(customData, VfxStateKeys.OrbitalsCoresCount, state.OrbitalsCoresCount);
            state.OrbitalsCoresScale = ReadFloat(customData, VfxStateKeys.OrbitalsCoresScale, state.OrbitalsCoresScale);
            state.OrbitalsCoresLuminance = ReadFloat(customData, VfxStateKeys.OrbitalsCoresLuminance, state.OrbitalsCoresLuminance);
            state.OrbitalsCoresHue = ReadFloat(customData, VfxStateKeys.OrbitalsCoresHue, state.OrbitalsCoresHue);
            state.OrbitalsCoresSpinSpeed = ReadFloat(customData, VfxStateKeys.OrbitalsCoresSpinSpeed, state.OrbitalsCoresSpinSpeed);
            state.OrbitalsCoresLength = ReadFloat(customData, VfxStateKeys.OrbitalsCoresLength, state.OrbitalsCoresLength);
            state.OrbitalsCoresSpeed = ReadFloat(customData, VfxStateKeys.OrbitalsCoresSpeed, state.OrbitalsCoresSpeed);
            state.OrbitalsCoresSpacing = ReadFloat(customData, VfxStateKeys.OrbitalsCoresSpacing, state.OrbitalsCoresSpacing);
            state.OrbitalsCoresRadius = ReadFloat(customData, VfxStateKeys.OrbitalsCoresRadius, state.OrbitalsCoresRadius);
            state.OrbitalsCoresCycles = ReadFloat(customData, VfxStateKeys.OrbitalsCoresCycles, state.OrbitalsCoresCycles);
            state.OrbitalsCoresXOffset = ReadFloat(customData, VfxStateKeys.OrbitalsCoresXOffset, state.OrbitalsCoresXOffset);
            state.OrbitalsCoresYOffset = ReadFloat(customData, VfxStateKeys.OrbitalsCoresYOffset, state.OrbitalsCoresYOffset);
            state.OrbitalsCoresZOffset = ReadFloat(customData, VfxStateKeys.OrbitalsCoresZOffset, state.OrbitalsCoresZOffset);
            state.OrbitalsCoresXRotation = ReadFloat(customData, VfxStateKeys.OrbitalsCoresXRotation, state.OrbitalsCoresXRotation);
            state.OrbitalsCoresYRotation = ReadFloat(customData, VfxStateKeys.OrbitalsCoresYRotation, state.OrbitalsCoresYRotation);
            state.OrbitalsCoresZRotation = ReadFloat(customData, VfxStateKeys.OrbitalsCoresZRotation, state.OrbitalsCoresZRotation);
            state.OrbitalsCoresDrift = ReadFloat(customData, VfxStateKeys.OrbitalsCoresDrift, state.OrbitalsCoresDrift);

            state.OrbitalsFlamesEnabled = ReadBool(customData, VfxStateKeys.OrbitalsFlamesEnabled, state.OrbitalsFlamesEnabled);
            state.OrbitalsFlamesCount = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesCount, state.OrbitalsFlamesCount);
            state.OrbitalsFlamesEnergy = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesEnergy, state.OrbitalsFlamesEnergy);
            state.OrbitalsFlamesScale = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesScale, state.OrbitalsFlamesScale);
            state.OrbitalsFlamesLuminance = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesLuminance, state.OrbitalsFlamesLuminance);
            state.OrbitalsFlamesHue = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesHue, state.OrbitalsFlamesHue);
            state.OrbitalsFlamesLifetime = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesLifetime, state.OrbitalsFlamesLifetime);
            state.OrbitalsFlamesSimulationSpeed = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesSimulationSpeed, state.OrbitalsFlamesSimulationSpeed);
            state.OrbitalsFlamesSpeed = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesSpeed, state.OrbitalsFlamesSpeed);
            state.OrbitalsFlamesSpacing = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesSpacing, state.OrbitalsFlamesSpacing);
            state.OrbitalsFlamesLength = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesLength, state.OrbitalsFlamesLength);
            state.OrbitalsFlamesRadius = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesRadius, state.OrbitalsFlamesRadius);
            state.OrbitalsFlamesCycles = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesCycles, state.OrbitalsFlamesCycles);
            state.OrbitalsFlamesXOffset = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesXOffset, state.OrbitalsFlamesXOffset);
            state.OrbitalsFlamesYOffset = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesYOffset, state.OrbitalsFlamesYOffset);
            state.OrbitalsFlamesZOffset = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesZOffset, state.OrbitalsFlamesZOffset);
            state.OrbitalsFlamesXRotation = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesXRotation, state.OrbitalsFlamesXRotation);
            state.OrbitalsFlamesYRotation = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesYRotation, state.OrbitalsFlamesYRotation);
            state.OrbitalsFlamesZRotation = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesZRotation, state.OrbitalsFlamesZRotation);
            state.OrbitalsFlamesDrift = ReadFloat(customData, VfxStateKeys.OrbitalsFlamesDrift, state.OrbitalsFlamesDrift);

            state.OrbitalsEmbersEnabled = ReadBool(customData, VfxStateKeys.OrbitalsEmbersEnabled, state.OrbitalsEmbersEnabled);
            state.OrbitalsEmbersCount = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersCount, state.OrbitalsEmbersCount);
            state.OrbitalsEmbersEnergy = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersEnergy, state.OrbitalsEmbersEnergy);
            state.OrbitalsEmbersScale = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersScale, state.OrbitalsEmbersScale);
            state.OrbitalsEmbersLuminance = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersLuminance, state.OrbitalsEmbersLuminance);
            state.OrbitalsEmbersHue = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersHue, state.OrbitalsEmbersHue);
            state.OrbitalsEmbersLifetime = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersLifetime, state.OrbitalsEmbersLifetime);
            state.OrbitalsEmbersSimulationSpeed = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersSimulationSpeed, state.OrbitalsEmbersSimulationSpeed);
            state.OrbitalsEmbersLength = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersLength, state.OrbitalsEmbersLength);
            state.OrbitalsEmbersSpeed = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersSpeed, state.OrbitalsEmbersSpeed);
            state.OrbitalsEmbersSpacing = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersSpacing, state.OrbitalsEmbersSpacing);
            state.OrbitalsEmbersRadius = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersRadius, state.OrbitalsEmbersRadius);
            state.OrbitalsEmbersCycles = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersCycles, state.OrbitalsEmbersCycles);
            state.OrbitalsEmbersXOffset = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersXOffset, state.OrbitalsEmbersXOffset);
            state.OrbitalsEmbersYOffset = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersYOffset, state.OrbitalsEmbersYOffset);
            state.OrbitalsEmbersZOffset = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersZOffset, state.OrbitalsEmbersZOffset);
            state.OrbitalsEmbersXRotation = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersXRotation, state.OrbitalsEmbersXRotation);
            state.OrbitalsEmbersYRotation = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersYRotation, state.OrbitalsEmbersYRotation);
            state.OrbitalsEmbersZRotation = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersZRotation, state.OrbitalsEmbersZRotation);
            state.OrbitalsEmbersDrift = ReadFloat(customData, VfxStateKeys.OrbitalsEmbersDrift, state.OrbitalsEmbersDrift);

            return true;
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
                InnerFlamesSimulationSpeed = PluginConfig.DefaultSimulationSpeed,
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
                SparksSimulationSpeed = PluginConfig.DefaultSimulationSpeed,
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
                OrbitalsCoresSnakeEnabled = false,
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
                OrbitalsFlamesSimulationSpeed = PluginConfig.DefaultSimulationSpeed,
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

        private static float ReadFloat(
            Dictionary<string, string> customData,
            string key,
            float fallback)
        {
            if (customData == null)
                return fallback;

            if (!customData.TryGetValue(key, out string stringValue) ||
                string.IsNullOrWhiteSpace(stringValue))
            {
                return fallback;
            }

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

        private static bool ReadBool(
            Dictionary<string, string> customData,
            string key,
            bool fallback)
        {
            if (customData == null)
                return fallback;

            if (!customData.TryGetValue(key, out string stringValue) ||
                string.IsNullOrWhiteSpace(stringValue))
            {
                return fallback;
            }

            if (bool.TryParse(
                    stringValue,
                    out bool parsedValue))
            {
                return parsedValue;
            }

            return fallback;
        }

        private static void WriteFloat(
            Dictionary<string, string> customData,
            string key,
            float value)
        {
            if (customData == null)
                return;

            customData[key] =
                value.ToString(
                    CultureInfo.InvariantCulture);
        }

        private static void WriteBool(
            Dictionary<string, string> customData,
            string key,
            bool value)
        {
            if (customData == null)
                return;

            customData[key] =
                value
                    ? "true"
                    : "false";
        }

        private static void BackfillMissing(
            Dictionary<string, string> customData,
            string key,
            float value)
        {
            if (customData == null ||
                customData.ContainsKey(key))
            {
                return;
            }

            customData[key] =
                value.ToString(
                    CultureInfo.InvariantCulture);
        }

        private static void BackfillMissing(
            Dictionary<string, string> customData,
            string key,
            bool value)
        {
            if (customData == null ||
                customData.ContainsKey(key))
            {
                return;
            }

            customData[key] =
                value
                    ? "true"
                    : "false";
        }

        internal static bool IsBound(
            global::ItemDrop.ItemData item)
        {
            if (item?.m_customData == null)
                return false;

            return ReadBool(
                item.m_customData,
                VfxStateKeys.Bound,
                false);
        }

        internal static VfxStyleSave ToStyleSave(
            string styleName,
            VfxState state)
        {
            Dictionary<string, string> entries =
                ToStateEntries(state);

            var save =
                new VfxStyleSave
                {
                    Version = 1,
                    Name = styleName
                };

            foreach (var pair in entries)
            {
                save.Entries.Add(
                    new VfxStyleEntry(
                        pair.Key,
                        pair.Value));
            }

            return save;
        }

        internal static VfxState FromStyleSave(
            VfxStyleSave save)
        {
            if (save == null ||
                save.Entries == null)
            {
                return FromDefaults();
            }

            var entries =
                new Dictionary<string, string>();

            foreach (VfxStyleEntry entry in save.Entries)
            {
                if (entry == null)
                    continue;

                if (string.IsNullOrWhiteSpace(entry.Key))
                    continue;

                entries[entry.Key] =
                    entry.Value;
            }

            return FromStateEntries(entries);
        }

        internal static Dictionary<string, string> ToStateEntries(
            VfxState state)
        {
            var entries =
                new Dictionary<string, string>();

            Write(
                entries,
                state,
                bound: false);

            entries.Remove(
                VfxStateKeys.Bound);

            return entries;
        }

        internal static VfxState FromStateEntries(
            Dictionary<string, string> entries)
        {
            VfxState fallbackState =
                FromDefaults();

            if (entries == null)
                return fallbackState;

            if (TryRead(
                    entries,
                    fallbackState,
                    out VfxState loadedState))
            {
                return loadedState;
            }

            return fallbackState;
        }
    }
}