using BepInEx.Configuration;

namespace NADA.VFX.Core.Config
{
    internal static class PluginConfig
    {
        internal static ConfigEntry<bool> InnerFlames = null;
        internal static ConfigEntry<float> InnerFlamesScale = null;
        internal static ConfigEntry<float> InnerFlamesHue = null;
        internal static ConfigEntry<float> InnerFlamesEnergy = null;
        internal static ConfigEntry<float> InnerFlamesChaos = null;
        
        internal static ConfigEntry<bool> OuterFlames = null;
        internal static ConfigEntry<float> OuterFlamesScale = null;
        internal static ConfigEntry<float> OuterFlamesHue = null;
        internal static ConfigEntry<float> OuterFlamesEnergy = null;
        internal static ConfigEntry<float> OuterFlamesChaos = null;
        
        internal static ConfigEntry<bool> Flare = null;
        internal static ConfigEntry<float> FlareScale = null;
        internal static ConfigEntry<float> FlareHue = null;
        
        internal static ConfigEntry<bool> Sparks = null;
        internal static ConfigEntry<float> SparksHue = null;
        internal static ConfigEntry<float> SparksEnergy = null;
        
        internal static ConfigEntry<bool> Mirage = null;
        internal static ConfigEntry<float> MirageScale = null;
        internal static ConfigEntry<float> MirageHue = null;

        internal static ConfigEntry<bool> OrbitalsOrbs = null;
        internal static ConfigEntry<float> OrbitalsOrbsCount = null;
        internal static ConfigEntry<float> OrbitalsOrbsScale = null;
        internal static ConfigEntry<float> OrbitalsOrbsHue = null;
        internal static ConfigEntry<float> OrbitalsOrbsSpeed = null;
        internal static ConfigEntry<float> OrbitalsOrbsSpacing = null;
        internal static ConfigEntry<float> OrbitalsOrbsLength = null;
        internal static ConfigEntry<float> OrbitalsOrbsRadius = null;
        internal static ConfigEntry<float> OrbitalsOrbsCycles = null;
        internal static ConfigEntry<float> OrbitalsOrbsDrift = null;

        internal static ConfigEntry<bool> OrbitalsFlames = null;
        internal static ConfigEntry<float> OrbitalsFlamesCount = null;
        internal static ConfigEntry<float> OrbitalsFlamesHue = null;
        internal static ConfigEntry<float> OrbitalsFlamesEnergy = null;
        internal static ConfigEntry<float> OrbitalsFlamesSpeed = null;
        internal static ConfigEntry<float> OrbitalsFlamesSpacing = null;
        internal static ConfigEntry<float> OrbitalsFlamesLength = null;
        internal static ConfigEntry<float> OrbitalsFlamesRadius = null;
        internal static ConfigEntry<float> OrbitalsFlamesCycles = null;
        internal static ConfigEntry<float> OrbitalsFlamesDrift = null;
        

        internal static ConfigEntry<bool> OrbitalsEmbers = null;
        internal static ConfigEntry<float> OrbitalsEmbersCount = null;
        internal static ConfigEntry<float> OrbitalsEmbersHue = null;
        internal static ConfigEntry<float> OrbitalsEmbersEnergy = null;
        internal static ConfigEntry<float> OrbitalsEmbersSpeed = null;
        internal static ConfigEntry<float> OrbitalsEmbersSpacing = null;
        internal static ConfigEntry<float> OrbitalsEmbersLength = null;
        internal static ConfigEntry<float> OrbitalsEmbersRadius = null;
        internal static ConfigEntry<float> OrbitalsEmbersCycles = null;
        internal static ConfigEntry<float> OrbitalsEmbersDrift = null;
        
        internal const float MaxScaleMult = 1.50f;
        internal const float MinScaleMult = 0.50f;

        internal const float MinHue = -0.50f;
        internal const float MaxHue = 0.50f;
        internal const float DefaultHue = 0.00f;

        internal const float MinEnergy = 0.00f;
        internal const float MaxEnergy = 1.00f;
        internal const float DefaultEnergy = 0.00f;

        internal const float MinChaos = 0.00f;
        internal const float MaxChaos = 1.00f;
        internal const float DefaultChaos = 0.00f;
        
        internal const float MinOrbScaleMult = 0.2f;
        internal const float MaxOrbScaleMult = 1.8f;
        
        internal const float MinCountNormalized = 0.00f;
        internal const float MaxCountNormalized = 1.00f;
        internal const float DefaultCountNormalized = 0.00f;
        
        internal const float MinOrbitalsSpeed = 0.02f;
        internal const float MaxOrbitalsSpeed = 0.18f;
        internal const float DefaultOrbitalsSpeed = 0.10f;

        internal const float MinOrbitalsSpacing = 0.00f;
        internal const float MaxOrbitalsSpacing = 1.00f;
        internal const float DefaultOrbitalsSpacing = 0.50f;
        
        internal const float MinOrbitalsRadiusMultiplier = 0.25f;
        internal const float MaxOrbitalsRadiusMultiplier = 1.75f;
        internal const float DefaultOrbitalsRadiusMultiplier = 1.00f;

        internal const float MinOrbitalsLengthMultiplier = 0.50f;
        internal const float MaxOrbitalsLengthMultiplier = 1.50f;
        internal const float DefaultOrbitalsLengthMultiplier = 1.00f;
        
        internal const float MinOrbitalsCycles = 1.00f;
        internal const float MaxOrbitalsCycles = 5.00f;
        internal const float DefaultOrbitalsCycles = 3.00f;
        
        internal const float MinDrift = 0.00f;
        internal const float MaxDrift = 1.00f;
        internal const float DefaultDrift = 0.00f;

        internal static void Bind(ConfigFile config)
        {
            const string innerFlamesSection = "INNER FLAMES";
            const string outerFlamesSection = "OUTER FLAMES";
            const string flareSection = "FLARE";
            const string sparksSection = "SPARKS";
            const string mirageSection = "MIRAGE";
            const string orbitalsOrbsSection = "Orbitals: ORBS";
            const string orbitalsFlamesSection = "Orbitals: FLAMES";
            const string orbitalsEmbersSection = "Orbitals: EMBERS";

            InnerFlames = config.Bind(
                innerFlamesSection,
                "Enabled",
                true,
                OrderedDescription(
                    "Turn Inner Flames on or off.",
                    200,
                    dispName: "Enabled",
                    customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
                    hideSettingName: true
                )
            );

            InnerFlamesScale = config.Bind(
                innerFlamesSection,
                "Scale",
                1.00f,
                OrderedDescription(
                    "Adjust the size of Inner Flames.",
                    199,
                    new AcceptableValueRange<float>(MinScaleMult, MaxScaleMult),
                    dispName: "Scale"
                )
            );

            InnerFlamesHue = config.Bind(
                innerFlamesSection,
                "Color",
                DefaultHue,
                OrderedDescription(
                    "Adjust the color of Inner Flames.",
                    198,
                    new AcceptableValueRange<float>(MinHue, MaxHue),
                    dispName: "Color"
                )
            );

            InnerFlamesEnergy = config.Bind(
                innerFlamesSection,
                "Energy",
                DefaultEnergy,
                OrderedDescription(
                    "Adjust how intense Inner Flames feels.",
                    197,
                    new AcceptableValueRange<float>(MinEnergy, MaxEnergy),
                    dispName: "Energy"
                )
            );

            InnerFlamesChaos = config.Bind(
                innerFlamesSection,
                "Chaos (Coming Soon)",
                DefaultChaos,
                OrderedDescription(
                    "Adjust how wild and chaotic Inner Flames feels.",
                    196,
                    new AcceptableValueRange<float>(MinChaos, MaxChaos),
                    dispName: "Chaos (Coming Soon)",
                    customDrawer: ConfigurationManagerDrawers.DrawDisabledSlider
                )
            );

            OuterFlames = config.Bind(
                outerFlamesSection,
                "Enabled",
                true,
                OrderedDescription(
                    "Turn Outer Flames on or off.",
                    190,
                    dispName: "Enabled",
                    customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
                    hideSettingName: true
                )
            );

            OuterFlamesScale = config.Bind(
                outerFlamesSection,
                "Scale",
                1.00f,
                OrderedDescription(
                    "Adjust the size of Outer Flames.",
                    189,
                    new AcceptableValueRange<float>(MinScaleMult, MaxScaleMult),
                    dispName: "Scale"
                )
            );

            OuterFlamesHue = config.Bind(
                outerFlamesSection,
                "Color",
                DefaultHue,
                OrderedDescription(
                    "Adjust the color of Outer Flames.",
                    188,
                    new AcceptableValueRange<float>(MinHue, MaxHue),
                    dispName: "Color"
                )
            );

            OuterFlamesEnergy = config.Bind(
                outerFlamesSection,
                "Energy",
                DefaultEnergy,
                OrderedDescription(
                    "Adjust how intense Outer Flames feels.",
                    187,
                    new AcceptableValueRange<float>(MinEnergy, MaxEnergy),
                    dispName: "Energy"
                )
            );

            OuterFlamesChaos = config.Bind(
                outerFlamesSection,
                "Chaos (Coming Soon)",
                DefaultChaos,
                OrderedDescription(
                    "Adjust how wild and chaotic Outer Flames feels.",
                    186,
                    new AcceptableValueRange<float>(MinChaos, MaxChaos),
                    dispName: "Chaos (Coming Soon)",
                    customDrawer: ConfigurationManagerDrawers.DrawDisabledSlider
                )
            );
            
            Flare = config.Bind(
                flareSection,
                "Enabled",
                true,
                OrderedDescription(
                    "Turn Flare on or off.",
                    180,
                    dispName: "Enabled",
                    customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
                    hideSettingName: true
                )
            );

            FlareScale = config.Bind(
                flareSection,
                "Scale",
                1.00f,
                OrderedDescription(
                    "Adjust the size of Flare.",
                    179,
                    new AcceptableValueRange<float>(MinScaleMult, MaxScaleMult),
                    dispName: "Scale"
                )
            );

            FlareHue = config.Bind(
                flareSection,
                "Color",
                DefaultHue,
                OrderedDescription(
                    "Adjust the color of Flare.",
                    178,
                    new AcceptableValueRange<float>(MinHue, MaxHue),
                    dispName: "Color"
                )
            );

            Sparks = config.Bind(
                sparksSection,
                "Enabled",
                true,
                OrderedDescription(
                    "Turn Sparks on or off.",
                    170,
                    dispName: "Enabled",
                    customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
                    hideSettingName: true
                )
            );

            SparksHue = config.Bind(
                sparksSection,
                "Color",
                DefaultHue,
                OrderedDescription(
                    "Adjust the color of Sparks.",
                    169,
                    new AcceptableValueRange<float>(MinHue, MaxHue),
                    dispName: "Color"
                )
            );

            SparksEnergy = config.Bind(
                sparksSection,
                "Energy",
                DefaultEnergy,
                OrderedDescription(
                    "Adjust how intense Sparks feels.",
                    168,
                    new AcceptableValueRange<float>(MinEnergy, MaxEnergy),
                    dispName: "Energy"
                )
            );
            
            Mirage = config.Bind(
                mirageSection,
                "Enabled",
                true,
                OrderedDescription(
                    "Turn Mirage on or off.",
                    160,
                    dispName: "Enabled",
                    customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
                    hideSettingName: true
                )
            );

            MirageScale = config.Bind(
                mirageSection,
                "Scale",
                1.00f,
                OrderedDescription(
                    "Adjust the size of Mirage.",
                    159,
                    new AcceptableValueRange<float>(MinScaleMult, MaxScaleMult),
                    dispName: "Scale"
                )
            );

            MirageHue = config.Bind(
                mirageSection,
                "Color",
                DefaultHue,
                OrderedDescription(
                    "Adjust the color of Mirage.",
                    158,
                    new AcceptableValueRange<float>(MinHue, MaxHue),
                    dispName: "Color"
                )
            );

            OrbitalsOrbs = config.Bind(
                orbitalsOrbsSection,
                "Enabled",
                true,
                OrderedDescription(
                    "Turn Orbs on or off.",
                    150,
                    dispName: "Enabled",
                    customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
                    hideSettingName: true
                )
            );

            OrbitalsOrbsCount = config.Bind(
                orbitalsOrbsSection,
                "Count",
                DefaultCountNormalized,
                OrderedDescription(
                    "Adjust how many orbs are active.",
                    149,
                    new AcceptableValueRange<float>(MinCountNormalized, MaxCountNormalized),
                    dispName: "Count",
                    showRangeAsPercent: true
                )
            );

            OrbitalsOrbsScale = config.Bind(
                orbitalsOrbsSection,
                "Scale",
                1f,
                OrderedDescription(
                    "Adjust the size of the orbs.",
                    148,
                    new AcceptableValueRange<float>(MinOrbScaleMult, MaxOrbScaleMult),
                    dispName: "Scale"
                )
            );

            OrbitalsOrbsHue = config.Bind(
                orbitalsOrbsSection,
                "Color",
                DefaultHue,
                OrderedDescription(
                    "Adjust the color of the orbs.",
                    147,
                    new AcceptableValueRange<float>(MinHue, MaxHue),
                    dispName: "Color"
                )
            );
            
            OrbitalsOrbsSpeed = config.Bind(
                orbitalsOrbsSection,
                "Speed",
                DefaultOrbitalsSpeed,
                OrderedDescription(
                    "Adjust how quickly the orbs travel through their orbit.",
                    146,
                    new AcceptableValueRange<float>(MinOrbitalsSpeed, MaxOrbitalsSpeed),
                    dispName: "Speed"
                )
            );

            OrbitalsOrbsSpacing = config.Bind(
                orbitalsOrbsSection,
                "Spacing",
                DefaultOrbitalsSpacing,
                OrderedDescription(
                    "Adjust how closely the orbs follow each other.",
                    145,
                    new AcceptableValueRange<float>(MinOrbitalsSpacing, MaxOrbitalsSpacing),
                    dispName: "Spacing",
                    showRangeAsPercent: true
                )
            );
            
            OrbitalsOrbsLength = config.Bind(
                orbitalsOrbsSection,
                "Orbit Length",
                DefaultOrbitalsLengthMultiplier,
                OrderedDescription(
                    "Adjust how far the orbs travel along the weapon before turning around.",
                    144,
                    new AcceptableValueRange<float>(MinOrbitalsLengthMultiplier, MaxOrbitalsLengthMultiplier),
                    dispName: "Length"
                )
            );

            OrbitalsOrbsRadius = config.Bind(
                orbitalsOrbsSection,
                "Radius",
                DefaultOrbitalsRadiusMultiplier,
                OrderedDescription(
                    "Adjust how wide the orbs wrap around the weapon.",
                    143,
                    new AcceptableValueRange<float>(MinOrbitalsRadiusMultiplier, MaxOrbitalsRadiusMultiplier),
                    dispName: "Radius"
                )
            );
            
            OrbitalsOrbsCycles = config.Bind(
                orbitalsOrbsSection,
                "Cycles",
                DefaultOrbitalsCycles,
                OrderedDescription(
                    "Adjust how many turns the orbs make before reversing direction.",
                    142,
                    new AcceptableValueRange<float>(MinOrbitalsCycles, MaxOrbitalsCycles),
                    dispName: "Cycles"
                )
            );
            
            OrbitalsOrbsDrift = config.Bind(
                orbitalsOrbsSection,
                "Drift",
                DefaultDrift,
                OrderedDescription(
                    "Adjust how much the orbs drift away from their locked orbit path.",
                    141,
                    new AcceptableValueRange<float>(MinDrift, MaxDrift),
                    dispName: "Drift",
                    showRangeAsPercent: true
                )
            );
            
            OrbitalsFlames = config.Bind(
                orbitalsFlamesSection,
                "Enabled",
                true,
                OrderedDescription(
                    "Turn Flames on or off.",
                    140,
                    dispName: "Enabled",
                    customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
                    hideSettingName: true
                )
            );
            
            OrbitalsFlamesCount = config.Bind(
                orbitalsFlamesSection,
                "Count",
                DefaultCountNormalized,
                OrderedDescription(
                    "Adjust how many flames are active.",
                    139,
                    new AcceptableValueRange<float>(MinCountNormalized, MaxCountNormalized),
                    dispName: "Count",
                    showRangeAsPercent: true
                )
            );

            OrbitalsFlamesHue = config.Bind(
                orbitalsFlamesSection,
                "Color",
                DefaultHue,
                OrderedDescription(
                    "Adjust the color of the flames.",
                    138,
                    new AcceptableValueRange<float>(MinHue, MaxHue),
                    dispName: "Color"
                )
            );
            
            OrbitalsFlamesSpeed = config.Bind(
                orbitalsFlamesSection,
                "Speed",
                DefaultOrbitalsSpeed,
                OrderedDescription(
                    "Adjust how quickly the flames travel through their orbit.",
                    137,
                    new AcceptableValueRange<float>(MinOrbitalsSpeed, MaxOrbitalsSpeed),
                    dispName: "Speed"
                )
            );
            
            OrbitalsFlamesSpacing = config.Bind(
                orbitalsFlamesSection,
                "Spacing",
                DefaultOrbitalsSpacing,
                OrderedDescription(
                    "Adjust how closely the flames follow each other.",
                    136,
                    new AcceptableValueRange<float>(MinOrbitalsSpacing, MaxOrbitalsSpacing),
                    dispName: "Spacing",
                    showRangeAsPercent: true
                )
            );

            OrbitalsFlamesLength = config.Bind(
                orbitalsFlamesSection,
                "Orbit Length",
                DefaultOrbitalsLengthMultiplier,
                OrderedDescription(
                    "Adjust how far the flames travel along the weapon before turning around.",
                    135,
                    new AcceptableValueRange<float>(MinOrbitalsLengthMultiplier, MaxOrbitalsLengthMultiplier),
                    dispName: "Length"
                )
            );
            
            OrbitalsFlamesRadius = config.Bind(
                orbitalsFlamesSection,
                "Radius",
                DefaultOrbitalsRadiusMultiplier,
                OrderedDescription(
                    "Adjust how wide the flames wrap around the weapon.",
                    134,
                    new AcceptableValueRange<float>(MinOrbitalsRadiusMultiplier, MaxOrbitalsRadiusMultiplier),
                    dispName: "Radius"
                )
            );
            
            OrbitalsFlamesEnergy = config.Bind(
                orbitalsFlamesSection,
                "Energy",
                DefaultEnergy,
                OrderedDescription(
                    "Adjust how intense the flames feel.",
                    133,
                    new AcceptableValueRange<float>(MinEnergy, MaxEnergy),
                    dispName: "Energy"
                )
            );
            
            OrbitalsFlamesCycles = config.Bind(
                orbitalsFlamesSection,
                "Cycles",
                DefaultOrbitalsCycles,
                OrderedDescription(
                    "Adjust how many turns the flames make before reversing direction.",
                    132,
                    new AcceptableValueRange<float>(MinOrbitalsCycles, MaxOrbitalsCycles),
                    dispName: "Cycles"
                )
            );
            
            OrbitalsFlamesDrift = config.Bind(
                orbitalsFlamesSection,
                "Drift",
                DefaultDrift,
                OrderedDescription(
                    "Adjust how much the flames drift away from their locked orbit path.",
                    131,
                    new AcceptableValueRange<float>(MinDrift, MaxDrift),
                    dispName: "Drift",
                    showRangeAsPercent: true
                )
            );

            OrbitalsEmbers = config.Bind(
                orbitalsEmbersSection,
                "Enabled",
                true,
                OrderedDescription(
                    "Turn Embers on or off.",
                    130,
                    dispName: "Enabled",
                    customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
                    hideSettingName: true
                )
            );
            
            OrbitalsEmbersCount = config.Bind(
                orbitalsEmbersSection,
                "Count",
                DefaultCountNormalized,
                OrderedDescription(
                    "Adjust how many embers are active.",
                    129,
                    new AcceptableValueRange<float>(MinCountNormalized, MaxCountNormalized),
                    dispName: "Count",
                    showRangeAsPercent: true
                )
            );

            OrbitalsEmbersHue = config.Bind(
                orbitalsEmbersSection,
                "Color",
                DefaultHue,
                OrderedDescription(
                    "Adjust the color of the embers.",
                    128,
                    new AcceptableValueRange<float>(MinHue, MaxHue),
                    dispName: "Color"
                )
            );

            OrbitalsEmbersSpeed = config.Bind(
                orbitalsEmbersSection,
                "Speed",
                DefaultOrbitalsSpeed,
                OrderedDescription(
                    "Adjust how quickly the embers travel through their orbit.",
                    127,
                    new AcceptableValueRange<float>(MinOrbitalsSpeed, MaxOrbitalsSpeed),
                    dispName: "Speed"
                )
            );
            
            OrbitalsEmbersSpacing = config.Bind(
                orbitalsEmbersSection,
                "Spacing",
                DefaultOrbitalsSpacing,
                OrderedDescription(
                    "Adjust how closely the embers follow each other.",
                    126,
                    new AcceptableValueRange<float>(MinOrbitalsSpacing, MaxOrbitalsSpacing),
                    dispName: "Spacing",
                    showRangeAsPercent: true
                )
            );
            
            OrbitalsEmbersLength = config.Bind(
                orbitalsEmbersSection,
                "Orbit Length",
                DefaultOrbitalsLengthMultiplier,
                OrderedDescription(
                    "Adjust how far the embers travel along the weapon before turning around.",
                    125,
                    new AcceptableValueRange<float>(MinOrbitalsLengthMultiplier, MaxOrbitalsLengthMultiplier),
                    dispName: "Length"
                )
            );

            OrbitalsEmbersRadius = config.Bind(
                orbitalsEmbersSection,
                "Radius",
                DefaultOrbitalsRadiusMultiplier,
                OrderedDescription(
                    "Adjust how wide the embers wrap around the weapon.",
                    124,
                    new AcceptableValueRange<float>(MinOrbitalsRadiusMultiplier, MaxOrbitalsRadiusMultiplier),
                    dispName: "Radius"
                )
            );
            
            OrbitalsEmbersEnergy = config.Bind(
                orbitalsEmbersSection,
                "Energy",
                DefaultEnergy,
                OrderedDescription(
                    "Adjust how intense the embers feel.",
                    123,
                    new AcceptableValueRange<float>(MinEnergy, MaxEnergy),
                    dispName: "Energy"
                )
            );
            
            OrbitalsEmbersCycles = config.Bind(
                orbitalsEmbersSection,
                "Cycles",
                DefaultOrbitalsCycles,
                OrderedDescription(
                    "Adjust how many turns the embers make before reversing direction.",
                    122,
                    new AcceptableValueRange<float>(MinOrbitalsCycles, MaxOrbitalsCycles),
                    dispName: "Cycles"
                )
            );
            
            OrbitalsEmbersDrift = config.Bind(
                orbitalsEmbersSection,
                "Drift",
                DefaultDrift,
                OrderedDescription(
                    "Adjust how much the embers drift away from their locked orbit path.",
                    121,
                    new AcceptableValueRange<float>(MinDrift, MaxDrift),
                    dispName: "Drift",
                    showRangeAsPercent: true
                )
            );

            Plugin.Log.LogInfo($"{Plugin.ModName}: Config bound.");
        }

        private static ConfigDescription OrderedDescription(
            string description,
            int order,
            AcceptableValueBase acceptableValues = null,
            string dispName = null,
            System.Action<ConfigEntryBase> customDrawer = null,
            bool? showRangeAsPercent = null,
            bool? isAdvanced = null,
            bool? hideSettingName = null)
        {
            return new ConfigDescription(
                description,
                acceptableValues,
                new ConfigurationManagerAttributes
                {
                    Order = order,
                    DispName = dispName,
                    CustomDrawer = customDrawer,
                    ShowRangeAsPercent = showRangeAsPercent,
                    IsAdvanced = isAdvanced,
                    HideSettingName = hideSettingName
                }
            );
        }
    }
}