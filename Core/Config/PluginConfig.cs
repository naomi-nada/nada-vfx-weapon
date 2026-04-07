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
        internal static ConfigEntry<float> OrbitalsOrbsSpacing = null;
        internal static ConfigEntry<float> OrbitalsOrbsRadius = null;
        internal static ConfigEntry<float> OrbitalsOrbsDrift = null;

        internal static ConfigEntry<bool> OrbitalsFlames = null;
        internal static ConfigEntry<float> OrbitalsFlamesCount = null;
        internal static ConfigEntry<float> OrbitalsFlamesHue = null;
        internal static ConfigEntry<float> OrbitalsFlamesEnergy = null;
        internal static ConfigEntry<float> OrbitalsFlamesSpacing = null;
        internal static ConfigEntry<float> OrbitalsFlamesRadius = null;
        internal static ConfigEntry<float> OrbitalsFlamesDrift = null;

        internal static ConfigEntry<bool> OrbitalsEmbers = null;
        internal static ConfigEntry<float> OrbitalsEmbersCount = null;
        internal static ConfigEntry<float> OrbitalsEmbersHue = null;
        internal static ConfigEntry<float> OrbitalsEmbersEnergy = null;
        internal static ConfigEntry<float> OrbitalsEmbersSpacing = null;
        internal static ConfigEntry<float> OrbitalsEmbersRadius = null;
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

        internal const float MinOrbitalsSpacing = 0.00f;
        internal const float MaxOrbitalsSpacing = 1.00f;
        internal const float DefaultOrbitalsSpacing = 0.50f;
        
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
                    100,
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
                    99,
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
                    98,
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
                    97,
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
                    96,
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
                    100,
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
                    99,
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
                    98,
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
                    97,
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
                    96,
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
                    90,
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
                    89,
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
                    88,
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
                    85,
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
                    84,
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
                    83,
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
                    80,
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
                    79,
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
                    78,
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
                    77,
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
                    76,
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
                    75,
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
                    74,
                    new AcceptableValueRange<float>(MinHue, MaxHue),
                    dispName: "Color"
                )
            );

            OrbitalsOrbsSpacing = config.Bind(
                orbitalsOrbsSection,
                "Spacing",
                DefaultOrbitalsSpacing,
                OrderedDescription(
                    "Adjust how closely the orbs follow each other.",
                    73,
                    new AcceptableValueRange<float>(MinOrbitalsSpacing, MaxOrbitalsSpacing),
                    dispName: "Spacing",
                    showRangeAsPercent: true
                )
            );

            OrbitalsOrbsRadius = config.Bind(
                orbitalsOrbsSection,
                "Radius",
                1.00f,
                OrderedDescription(
                    "Adjust how wide the orbs wrap around the weapon.",
                    72,
                    new AcceptableValueRange<float>(0.50f, 1.50f),
                    dispName: "Radius"
                )
            );
            
            OrbitalsOrbsDrift = config.Bind(
                orbitalsOrbsSection,
                "Drift",
                DefaultDrift,
                OrderedDescription(
                    "Adjust how much the orbs drift away from their locked orbit path.",
                    71,
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
                    70,
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
                    69,
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
                    68,
                    new AcceptableValueRange<float>(MinHue, MaxHue),
                    dispName: "Color"
                )
            );
            
            OrbitalsFlamesSpacing = config.Bind(
                orbitalsFlamesSection,
                "Spacing",
                DefaultOrbitalsSpacing,
                OrderedDescription(
                    "Adjust how closely the flames follow each other.",
                    67,
                    new AcceptableValueRange<float>(MinOrbitalsSpacing, MaxOrbitalsSpacing),
                    dispName: "Spacing",
                    showRangeAsPercent: true
                )
            );

            OrbitalsFlamesRadius = config.Bind(
                orbitalsFlamesSection,
                "Radius",
                1.00f,
                OrderedDescription(
                    "Adjust how wide the flames wrap around the weapon.",
                    66,
                    new AcceptableValueRange<float>(0.50f, 1.50f),
                    dispName: "Radius"
                )
            );
            
            OrbitalsFlamesEnergy = config.Bind(
                orbitalsFlamesSection,
                "Energy",
                DefaultEnergy,
                OrderedDescription(
                    "Adjust how intense the flames feel.",
                    65,
                    new AcceptableValueRange<float>(MinEnergy, MaxEnergy),
                    dispName: "Energy"
                )
            );
            
            OrbitalsFlamesDrift = config.Bind(
                orbitalsFlamesSection,
                "Drift",
                DefaultDrift,
                OrderedDescription(
                    "Adjust how much the flames drift away from their locked orbit path.",
                    64,
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
                    60,
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
                    59,
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
                    58,
                    new AcceptableValueRange<float>(MinHue, MaxHue),
                    dispName: "Color"
                )
            );

            OrbitalsEmbersSpacing = config.Bind(
                orbitalsEmbersSection,
                "Spacing",
                DefaultOrbitalsSpacing,
                OrderedDescription(
                    "Adjust how closely the embers follow each other.",
                    57,
                    new AcceptableValueRange<float>(MinOrbitalsSpacing, MaxOrbitalsSpacing),
                    dispName: "Spacing",
                    showRangeAsPercent: true
                )
            );

            OrbitalsEmbersRadius = config.Bind(
                orbitalsEmbersSection,
                "Radius",
                1.00f,
                OrderedDescription(
                    "Adjust how wide the embers wrap around the weapon.",
                    56,
                    new AcceptableValueRange<float>(0.50f, 1.50f),
                    dispName: "Radius"
                )
            );
            
            OrbitalsEmbersEnergy = config.Bind(
                orbitalsEmbersSection,
                "Energy",
                DefaultEnergy,
                OrderedDescription(
                    "Adjust how intense the embers feel.",
                    55,
                    new AcceptableValueRange<float>(MinEnergy, MaxEnergy),
                    dispName: "Energy"
                )
            );
            
            OrbitalsEmbersDrift = config.Bind(
                orbitalsEmbersSection,
                "Drift",
                DefaultDrift,
                OrderedDescription(
                    "Adjust how much the embers drift away from their locked orbit path.",
                    54,
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