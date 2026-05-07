using System.Collections.Generic;
using System.Globalization;
using NADA.VFX.Core.Config;

namespace NADA.VFX.Core.State
{
    internal static class VfxStateIO
    {
        internal static VfxState FromConfig()
        {
            return new VfxState
            {
                RigRotation = PluginConfig.RigRotation.Value,
                RigSideRotation = PluginConfig.RigSideRotation.Value,
                RigLengthPosition = PluginConfig.RigLengthPosition.Value,
                RigSidePosition = PluginConfig.RigSidePosition.Value,

                InnerFlamesEnabled = PluginConfig.InnerFlames.Value,
                InnerFlamesEnergy = PluginConfig.InnerFlamesEnergy.Value,
                InnerFlamesScale = PluginConfig.InnerFlamesScale.Value,
                InnerFlamesLength = PluginConfig.InnerFlamesLength.Value,
                InnerFlamesHue = PluginConfig.InnerFlamesHue.Value,
                InnerFlamesPosition = PluginConfig.InnerFlamesPosition.Value,

                OuterFlamesEnabled = PluginConfig.OuterFlames.Value,
                OuterFlamesDragEnabled = PluginConfig.OuterFlamesDragEnabled.Value,
                OuterFlamesEnergy = PluginConfig.OuterFlamesEnergy.Value,
                OuterFlamesScale = PluginConfig.OuterFlamesScale.Value,
                OuterFlamesLength = PluginConfig.OuterFlamesLength.Value,
                OuterFlamesHue = PluginConfig.OuterFlamesHue.Value,
                OuterFlamesPosition = PluginConfig.OuterFlamesPosition.Value,

                SparksEnabled = PluginConfig.Sparks.Value,
                SparksEnergy = PluginConfig.SparksEnergy.Value,
                SparksScale = PluginConfig.SparksScale.Value,
                SparksLength = PluginConfig.SparksLength.Value,
                SparksWidth = PluginConfig.SparksWidth.Value,
                SparksHue = PluginConfig.SparksHue.Value,
                SparksPosition = PluginConfig.SparksPosition.Value,

                FlareEnabled = PluginConfig.Flare.Value,
                FlareScale = PluginConfig.FlareScale.Value,
                FlareHue = PluginConfig.FlareHue.Value,
                FlarePosition = PluginConfig.FlarePosition.Value,

                AuraEnabled = PluginConfig.Aura.Value,
                AuraScale = PluginConfig.AuraScale.Value,
                AuraHue = PluginConfig.AuraHue.Value,

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

                OrbitalsEmbersEnabled = PluginConfig.OrbitalsEmbers.Value,
                OrbitalsEmbersCount = PluginConfig.OrbitalsEmbersCount.Value,
                OrbitalsEmbersEnergy = PluginConfig.OrbitalsEmbersEnergy.Value,
                OrbitalsEmbersDrift = PluginConfig.OrbitalsEmbersDrift.Value,
                OrbitalsEmbersHue = PluginConfig.OrbitalsEmbersHue.Value,
                OrbitalsEmbersSpeed = PluginConfig.OrbitalsEmbersSpeed.Value,
                OrbitalsEmbersSpacing = PluginConfig.OrbitalsEmbersSpacing.Value,
                OrbitalsEmbersLength = PluginConfig.OrbitalsEmbersLength.Value,
                OrbitalsEmbersRadius = PluginConfig.OrbitalsEmbersRadius.Value,
                OrbitalsEmbersCycles = PluginConfig.OrbitalsEmbersCycles.Value
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

            WriteFloat(customData, VfxStateKeys.RigRotation, state.RigRotation);
            WriteFloat(customData, VfxStateKeys.RigSideRotation, state.RigSideRotation);
            WriteFloat(customData, VfxStateKeys.RigLengthPosition, state.RigLengthPosition);
            WriteFloat(customData, VfxStateKeys.RigSidePosition, state.RigSidePosition);

            WriteBool(customData, VfxStateKeys.InnerFlamesEnabled, state.InnerFlamesEnabled);
            WriteFloat(customData, VfxStateKeys.InnerFlamesEnergy, state.InnerFlamesEnergy);
            WriteFloat(customData, VfxStateKeys.InnerFlamesScale, state.InnerFlamesScale);
            WriteFloat(customData, VfxStateKeys.InnerFlamesLength, state.InnerFlamesLength);
            WriteFloat(customData, VfxStateKeys.InnerFlamesHue, state.InnerFlamesHue);
            WriteFloat(customData, VfxStateKeys.InnerFlamesPosition, state.InnerFlamesPosition);

            WriteBool(customData, VfxStateKeys.OuterFlamesEnabled, state.OuterFlamesEnabled);
            WriteBool(customData, VfxStateKeys.OuterFlamesDragEnabled, state.OuterFlamesDragEnabled);
            WriteFloat(customData, VfxStateKeys.OuterFlamesEnergy, state.OuterFlamesEnergy);
            WriteFloat(customData, VfxStateKeys.OuterFlamesScale, state.OuterFlamesScale);
            WriteFloat(customData, VfxStateKeys.OuterFlamesLength, state.OuterFlamesLength);
            WriteFloat(customData, VfxStateKeys.OuterFlamesHue, state.OuterFlamesHue);
            WriteFloat(customData, VfxStateKeys.OuterFlamesPosition, state.OuterFlamesPosition);

            WriteBool(customData, VfxStateKeys.SparksEnabled, state.SparksEnabled);
            WriteFloat(customData, VfxStateKeys.SparksEnergy, state.SparksEnergy);
            WriteFloat(customData, VfxStateKeys.SparksScale, state.SparksScale);
            WriteFloat(customData, VfxStateKeys.SparksLength, state.SparksLength);
            WriteFloat(customData, VfxStateKeys.SparksWidth, state.SparksWidth);
            WriteFloat(customData, VfxStateKeys.SparksHue, state.SparksHue);
            WriteFloat(customData, VfxStateKeys.SparksPosition, state.SparksPosition);

            WriteBool(customData, VfxStateKeys.FlareEnabled, state.FlareEnabled);
            WriteFloat(customData, VfxStateKeys.FlareScale, state.FlareScale);
            WriteFloat(customData, VfxStateKeys.FlareHue, state.FlareHue);
            WriteFloat(customData, VfxStateKeys.FlarePosition, state.FlarePosition);

            WriteBool(customData, VfxStateKeys.AuraEnabled, state.AuraEnabled);
            WriteFloat(customData, VfxStateKeys.AuraScale, state.AuraScale);
            WriteFloat(customData, VfxStateKeys.AuraHue, state.AuraHue);

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
        }

        internal static bool TryRead(global::ItemDrop.ItemData item, out VfxState state)
        {
            state = default;
            if (item == null)
                return false;

            Dictionary<string, string> customData = item.m_customData;
            if (customData == null)
                return false;

            state.RigRotation = ReadFloat(customData, VfxStateKeys.RigRotation, PluginConfig.RigRotation.Value);
            state.RigSideRotation = ReadFloat(customData, VfxStateKeys.RigSideRotation, PluginConfig.RigSideRotation.Value);
            state.RigLengthPosition = ReadFloat(customData, VfxStateKeys.RigLengthPosition, PluginConfig.RigLengthPosition.Value);
            state.RigSidePosition = ReadFloat(customData, VfxStateKeys.RigSidePosition, PluginConfig.RigSidePosition.Value);

            state.InnerFlamesEnabled = ReadBool(customData, VfxStateKeys.InnerFlamesEnabled, PluginConfig.InnerFlames.Value);
            state.InnerFlamesEnergy = ReadFloat(customData, VfxStateKeys.InnerFlamesEnergy, PluginConfig.InnerFlamesEnergy.Value);
            state.InnerFlamesScale = ReadFloat(customData, VfxStateKeys.InnerFlamesScale, PluginConfig.InnerFlamesScale.Value);
            state.InnerFlamesLength = ReadFloat(customData, VfxStateKeys.InnerFlamesLength, PluginConfig.InnerFlamesLength.Value);
            state.InnerFlamesHue = ReadFloat(customData, VfxStateKeys.InnerFlamesHue, PluginConfig.InnerFlamesHue.Value);
            state.InnerFlamesPosition = ReadFloat(customData, VfxStateKeys.InnerFlamesPosition, PluginConfig.InnerFlamesPosition.Value);

            state.OuterFlamesEnabled = ReadBool(customData, VfxStateKeys.OuterFlamesEnabled, PluginConfig.OuterFlames.Value);
            state.OuterFlamesDragEnabled = ReadBool(customData, VfxStateKeys.OuterFlamesDragEnabled, PluginConfig.OuterFlamesDragEnabled.Value);
            state.OuterFlamesEnergy = ReadFloat(customData, VfxStateKeys.OuterFlamesEnergy, PluginConfig.OuterFlamesEnergy.Value);
            state.OuterFlamesScale = ReadFloat(customData, VfxStateKeys.OuterFlamesScale, PluginConfig.OuterFlamesScale.Value);
            state.OuterFlamesLength = ReadFloat(customData, VfxStateKeys.OuterFlamesLength, PluginConfig.OuterFlamesLength.Value);
            state.OuterFlamesHue = ReadFloat(customData, VfxStateKeys.OuterFlamesHue, PluginConfig.OuterFlamesHue.Value);
            state.OuterFlamesPosition = ReadFloat(customData, VfxStateKeys.OuterFlamesPosition, PluginConfig.OuterFlamesPosition.Value);

            state.SparksEnabled = ReadBool(customData, VfxStateKeys.SparksEnabled, PluginConfig.Sparks.Value);
            state.SparksEnergy = ReadFloat(customData, VfxStateKeys.SparksEnergy, PluginConfig.SparksEnergy.Value);
            state.SparksScale = ReadFloat(customData, VfxStateKeys.SparksScale, PluginConfig.SparksScale.Value);
            state.SparksLength = ReadFloat(customData, VfxStateKeys.SparksLength, PluginConfig.SparksLength.Value);
            state.SparksWidth = ReadFloat(customData, VfxStateKeys.SparksWidth, PluginConfig.SparksWidth.Value);
            state.SparksHue = ReadFloat(customData, VfxStateKeys.SparksHue, PluginConfig.SparksHue.Value);
            state.SparksPosition = ReadFloat(customData, VfxStateKeys.SparksPosition, PluginConfig.SparksPosition.Value);

            state.FlareEnabled = ReadBool(customData, VfxStateKeys.FlareEnabled, PluginConfig.Flare.Value);
            state.FlareScale = ReadFloat(customData, VfxStateKeys.FlareScale, PluginConfig.FlareScale.Value);
            state.FlareHue = ReadFloat(customData, VfxStateKeys.FlareHue, PluginConfig.FlareHue.Value);
            state.FlarePosition = ReadFloat(customData, VfxStateKeys.FlarePosition, PluginConfig.FlarePosition.Value);

            state.AuraEnabled = ReadBool(customData, VfxStateKeys.AuraEnabled, PluginConfig.Aura.Value);
            state.AuraScale = ReadFloat(customData, VfxStateKeys.AuraScale, PluginConfig.AuraScale.Value);
            state.AuraHue = ReadFloat(customData, VfxStateKeys.AuraHue, PluginConfig.AuraHue.Value);

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

            BackfillMissing(customData, VfxStateKeys.RigRotation, PluginConfig.RigRotation.Value);
            BackfillMissing(customData, VfxStateKeys.RigSideRotation, PluginConfig.RigSideRotation.Value);
            BackfillMissing(customData, VfxStateKeys.RigLengthPosition, PluginConfig.RigLengthPosition.Value);
            BackfillMissing(customData, VfxStateKeys.RigSidePosition, PluginConfig.RigSidePosition.Value);

            BackfillMissing(customData, VfxStateKeys.InnerFlamesEnabled, PluginConfig.InnerFlames.Value);
            BackfillMissing(customData, VfxStateKeys.InnerFlamesEnergy, PluginConfig.InnerFlamesEnergy.Value);
            BackfillMissing(customData, VfxStateKeys.InnerFlamesScale, PluginConfig.InnerFlamesScale.Value);
            BackfillMissing(customData, VfxStateKeys.InnerFlamesLength, PluginConfig.InnerFlamesLength.Value);
            BackfillMissing(customData, VfxStateKeys.InnerFlamesHue, PluginConfig.InnerFlamesHue.Value);
            BackfillMissing(customData, VfxStateKeys.InnerFlamesPosition, PluginConfig.InnerFlamesPosition.Value);

            BackfillMissing(customData, VfxStateKeys.OuterFlamesEnabled, PluginConfig.OuterFlames.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesDragEnabled, PluginConfig.OuterFlamesDragEnabled.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesEnergy, PluginConfig.OuterFlamesEnergy.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesScale, PluginConfig.OuterFlamesScale.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesLength, PluginConfig.OuterFlamesLength.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesHue, PluginConfig.OuterFlamesHue.Value);
            BackfillMissing(customData, VfxStateKeys.OuterFlamesPosition, PluginConfig.OuterFlamesPosition.Value);

            BackfillMissing(customData, VfxStateKeys.SparksEnabled, PluginConfig.Sparks.Value);
            BackfillMissing(customData, VfxStateKeys.SparksEnergy, PluginConfig.SparksEnergy.Value);
            BackfillMissing(customData, VfxStateKeys.SparksScale, PluginConfig.SparksScale.Value);
            BackfillMissing(customData, VfxStateKeys.SparksLength, PluginConfig.SparksLength.Value);
            BackfillMissing(customData, VfxStateKeys.SparksWidth, PluginConfig.SparksWidth.Value);
            BackfillMissing(customData, VfxStateKeys.SparksHue, PluginConfig.SparksHue.Value);
            BackfillMissing(customData, VfxStateKeys.SparksPosition, PluginConfig.SparksPosition.Value);

            BackfillMissing(customData, VfxStateKeys.FlareEnabled, PluginConfig.Flare.Value);
            BackfillMissing(customData, VfxStateKeys.FlareScale, PluginConfig.FlareScale.Value);
            BackfillMissing(customData, VfxStateKeys.FlareHue, PluginConfig.FlareHue.Value);
            BackfillMissing(customData, VfxStateKeys.FlarePosition, PluginConfig.FlarePosition.Value);

            BackfillMissing(customData, VfxStateKeys.AuraEnabled, PluginConfig.Aura.Value);
            BackfillMissing(customData, VfxStateKeys.AuraScale, PluginConfig.AuraScale.Value);
            BackfillMissing(customData, VfxStateKeys.AuraHue, PluginConfig.AuraHue.Value);

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
        }

        internal static void Clear(global::ItemDrop.ItemData item)
        {
            if (item?.m_customData == null)
                return;

            item.m_customData.Remove(VfxStateKeys.RigRotation);
            item.m_customData.Remove(VfxStateKeys.RigSideRotation);
            item.m_customData.Remove(VfxStateKeys.RigLengthPosition);
            item.m_customData.Remove(VfxStateKeys.RigSidePosition);

            item.m_customData.Remove(VfxStateKeys.Bound);

            item.m_customData.Remove(VfxStateKeys.InnerFlamesEnabled);
            item.m_customData.Remove(VfxStateKeys.InnerFlamesEnergy);
            item.m_customData.Remove(VfxStateKeys.InnerFlamesScale);
            item.m_customData.Remove(VfxStateKeys.InnerFlamesLength);
            item.m_customData.Remove(VfxStateKeys.InnerFlamesHue);
            item.m_customData.Remove(VfxStateKeys.InnerFlamesPosition);

            item.m_customData.Remove(VfxStateKeys.OuterFlamesEnabled);
            item.m_customData.Remove(VfxStateKeys.OuterFlamesDragEnabled);
            item.m_customData.Remove(VfxStateKeys.OuterFlamesEnergy);
            item.m_customData.Remove(VfxStateKeys.OuterFlamesScale);
            item.m_customData.Remove(VfxStateKeys.OuterFlamesLength);
            item.m_customData.Remove(VfxStateKeys.OuterFlamesHue);
            item.m_customData.Remove(VfxStateKeys.OuterFlamesPosition);

            item.m_customData.Remove(VfxStateKeys.SparksEnabled);
            item.m_customData.Remove(VfxStateKeys.SparksEnergy);
            item.m_customData.Remove(VfxStateKeys.SparksScale);
            item.m_customData.Remove(VfxStateKeys.SparksLength);
            item.m_customData.Remove(VfxStateKeys.SparksWidth);
            item.m_customData.Remove(VfxStateKeys.SparksHue);
            item.m_customData.Remove(VfxStateKeys.SparksPosition);

            item.m_customData.Remove(VfxStateKeys.FlareEnabled);
            item.m_customData.Remove(VfxStateKeys.FlareScale);
            item.m_customData.Remove(VfxStateKeys.FlareHue);
            item.m_customData.Remove(VfxStateKeys.FlarePosition);

            item.m_customData.Remove(VfxStateKeys.AuraEnabled);
            item.m_customData.Remove(VfxStateKeys.AuraScale);
            item.m_customData.Remove(VfxStateKeys.AuraHue);

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
        }

        internal static VfxState FromDefaults()
        {
            return new VfxState
            {
                RigRotation = PluginConfig.DefaultRigRotation,
                RigSideRotation = PluginConfig.DefaultRigSideRotation,
                RigLengthPosition = PluginConfig.DefaultRigLengthPosition,
                RigSidePosition = PluginConfig.DefaultRigSidePosition,

                InnerFlamesEnabled = true,
                InnerFlamesEnergy = PluginConfig.DefaultEnergy,
                InnerFlamesScale = 1f,
                InnerFlamesLength = PluginConfig.DefaultFlameLength,
                InnerFlamesHue = PluginConfig.DefaultHue,
                InnerFlamesPosition = PluginConfig.DefaultFlamePosition,

                OuterFlamesEnabled = true,
                OuterFlamesDragEnabled = false,
                OuterFlamesEnergy = PluginConfig.DefaultEnergy,
                OuterFlamesScale = 1f,
                OuterFlamesLength = PluginConfig.DefaultFlameLength,
                OuterFlamesHue = PluginConfig.DefaultHue,
                OuterFlamesPosition = PluginConfig.DefaultFlamePosition,

                SparksEnabled = true,
                SparksEnergy = PluginConfig.DefaultEnergy,
                SparksScale = 1f,
                SparksLength = PluginConfig.DefaultFlameLength,
                SparksWidth = PluginConfig.DefaultSparksWidth,
                SparksHue = PluginConfig.DefaultHue,
                SparksPosition = PluginConfig.DefaultFlamePosition,

                FlareEnabled = true,
                FlareScale = 1f,
                FlareHue = PluginConfig.DefaultHue,
                FlarePosition = PluginConfig.DefaultFlamePosition,

                AuraEnabled = true,
                AuraScale = 1f,
                AuraHue = PluginConfig.DefaultHue,

                OrbitalsOrbsEnabled = true,
                OrbitalsOrbsCount = PluginConfig.DefaultCountNormalized,
                OrbitalsOrbsDrift = PluginConfig.DefaultDrift,
                OrbitalsOrbsScale = 1f,
                OrbitalsOrbsHue = PluginConfig.DefaultHue,
                OrbitalsOrbsSpeed = PluginConfig.DefaultOrbitalsSpeed,
                OrbitalsOrbsSpacing = PluginConfig.DefaultOrbitalsSpacing,
                OrbitalsOrbsLength = PluginConfig.DefaultOrbitalsLengthMultiplier,
                OrbitalsOrbsRadius = PluginConfig.DefaultOrbitalsRadiusMultiplier,
                OrbitalsOrbsCycles = PluginConfig.DefaultOrbitalsCycles,

                OrbitalsFlamesEnabled = true,
                OrbitalsFlamesCount = PluginConfig.DefaultCountNormalized,
                OrbitalsFlamesEnergy = PluginConfig.DefaultEnergy,
                OrbitalsFlamesDrift = PluginConfig.DefaultDrift,
                OrbitalsFlamesHue = PluginConfig.DefaultHue,
                OrbitalsFlamesSpeed = PluginConfig.DefaultOrbitalsSpeed,
                OrbitalsFlamesSpacing = PluginConfig.DefaultOrbitalsSpacing,
                OrbitalsFlamesLength = PluginConfig.DefaultOrbitalsLengthMultiplier,
                OrbitalsFlamesRadius = PluginConfig.DefaultOrbitalsRadiusMultiplier,
                OrbitalsFlamesCycles = PluginConfig.DefaultOrbitalsCycles,

                OrbitalsEmbersEnabled = true,
                OrbitalsEmbersCount = PluginConfig.DefaultCountNormalized,
                OrbitalsEmbersEnergy = PluginConfig.DefaultEnergy,
                OrbitalsEmbersDrift = PluginConfig.DefaultDrift,
                OrbitalsEmbersHue = PluginConfig.DefaultHue,
                OrbitalsEmbersSpeed = PluginConfig.DefaultOrbitalsSpeed,
                OrbitalsEmbersSpacing = PluginConfig.DefaultOrbitalsSpacing,
                OrbitalsEmbersLength = PluginConfig.DefaultOrbitalsLengthMultiplier,
                OrbitalsEmbersRadius = PluginConfig.DefaultOrbitalsRadiusMultiplier,
                OrbitalsEmbersCycles = PluginConfig.DefaultOrbitalsCycles
            };
        }

        internal static void ApplyToConfig(VfxState state)
        {
            PluginConfig.RigRotation.Value = state.RigRotation;
            PluginConfig.RigSideRotation.Value = state.RigSideRotation;
            PluginConfig.RigLengthPosition.Value = state.RigLengthPosition;
            PluginConfig.RigSidePosition.Value = state.RigSidePosition;

            PluginConfig.InnerFlames.Value = state.InnerFlamesEnabled;
            PluginConfig.InnerFlamesEnergy.Value = state.InnerFlamesEnergy;
            PluginConfig.InnerFlamesScale.Value = state.InnerFlamesScale;
            PluginConfig.InnerFlamesLength.Value = state.InnerFlamesLength;
            PluginConfig.InnerFlamesHue.Value = state.InnerFlamesHue;
            PluginConfig.InnerFlamesPosition.Value = state.InnerFlamesPosition;

            PluginConfig.OuterFlames.Value = state.OuterFlamesEnabled;
            PluginConfig.OuterFlamesDragEnabled.Value = state.OuterFlamesDragEnabled;
            PluginConfig.OuterFlamesEnergy.Value = state.OuterFlamesEnergy;
            PluginConfig.OuterFlamesScale.Value = state.OuterFlamesScale;
            PluginConfig.OuterFlamesLength.Value = state.OuterFlamesLength;
            PluginConfig.OuterFlamesHue.Value = state.OuterFlamesHue;
            PluginConfig.OuterFlamesPosition.Value = state.OuterFlamesPosition;

            PluginConfig.Sparks.Value = state.SparksEnabled;
            PluginConfig.SparksEnergy.Value = state.SparksEnergy;
            PluginConfig.SparksScale.Value = state.SparksScale;
            PluginConfig.SparksLength.Value = state.SparksLength;
            PluginConfig.SparksWidth.Value = state.SparksWidth;
            PluginConfig.SparksHue.Value = state.SparksHue;
            PluginConfig.SparksPosition.Value = state.SparksPosition;

            PluginConfig.Flare.Value = state.FlareEnabled;
            PluginConfig.FlareScale.Value = state.FlareScale;
            PluginConfig.FlareHue.Value = state.FlareHue;
            PluginConfig.FlarePosition.Value = state.FlarePosition;

            PluginConfig.Aura.Value = state.AuraEnabled;
            PluginConfig.AuraScale.Value = state.AuraScale;
            PluginConfig.AuraHue.Value = state.AuraHue;

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
    }
}