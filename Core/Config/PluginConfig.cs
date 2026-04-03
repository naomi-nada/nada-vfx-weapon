using BepInEx.Configuration;

namespace NADA.VFX.Core.Config
{
    internal static class PluginConfig
    {
        internal const float MaxScaleMult = 1.50f;
        internal const float MinScaleMult = 0.50f;

        internal const float MinOrbScaleMult = 0.2f;
        internal const float MaxOrbScaleMult = 1.8f;

        internal const float MinHueShift = -0.50f;
        internal const float MaxHueShift = 0.50f;
        internal const float DefaultHueShift = 0.00f;

        internal const float MinEnergy = 0.00f;
        internal const float MaxEnergy = 1.00f;
        internal const float DefaultEnergy = 0.00f;

        internal const float MinChaos = 0.00f;
        internal const float MaxChaos = 1.00f;
        internal const float DefaultChaos = 0.00f;

        internal const float MinCountNormalized = 0.00f;
        internal const float MaxCountNormalized = 1.00f;
        internal const float DefaultCountNormalized = 0.00f;

        internal const float MinOrbSpacing = 0.00f;
        internal const float MaxOrbSpacing = 1.00f;
        internal const float DefaultOrbSpacing = 0.50f;

        internal static ConfigEntry<bool> InnerFlames = null;
        internal static ConfigEntry<bool> OuterFlames = null;
        internal static ConfigEntry<bool> Flare = null;

        internal static ConfigEntry<float> InnerFlamesScaleMult = null;
        internal static ConfigEntry<float> OuterFlamesScaleMult = null;
        internal static ConfigEntry<float> FlareScaleMult = null;

        internal static ConfigEntry<float> InnerFlamesHueShift = null;
        internal static ConfigEntry<float> OuterFlamesHueShift = null;
        internal static ConfigEntry<float> FlareHueShift = null;

        internal static ConfigEntry<float> InnerFlamesEnergy = null;
        internal static ConfigEntry<float> OuterFlamesEnergy = null;

        internal static ConfigEntry<float> InnerFlamesChaos = null;
        internal static ConfigEntry<float> OuterFlamesChaos = null;

        internal static ConfigEntry<bool> OrbitalsOrbs = null;
        internal static ConfigEntry<float> OrbitalsOrbsCount = null;
        internal static ConfigEntry<float> OrbitalsOrbsScale = null;
        internal static ConfigEntry<float> OrbitalsOrbsHueShift = null;
        internal static ConfigEntry<float> OrbitalsOrbsSpacing = null;
        internal static ConfigEntry<float> OrbitalsOrbsRadius = null;

        internal static ConfigEntry<bool> Mirage = null;
        internal static ConfigEntry<float> MirageScaleMult = null;
        internal static ConfigEntry<float> MirageHueShift = null;

        internal static ConfigEntry<bool> Sparks = null;
        internal static ConfigEntry<float> SparksHueShift = null;
        internal static ConfigEntry<float> SparksEnergy = null;

        internal static ConfigEntry<bool> OrbitalsFlames = null;
        internal static ConfigEntry<float> OrbitalsFlamesCount = null;
        internal static ConfigEntry<float> OrbitalsFlamesHueShift = null;
        internal static ConfigEntry<float> OrbitalsFlamesEnergy = null;
        internal static ConfigEntry<float> OrbitalsFlamesSpacing = null;
        internal static ConfigEntry<float> OrbitalsFlamesRadius = null;

        internal static ConfigEntry<bool> OrbitalsEmbers = null;
        internal static ConfigEntry<float> OrbitalsEmbersCount = null;
        internal static ConfigEntry<float> OrbitalsEmbersHueShift = null;
        internal static ConfigEntry<float> OrbitalsEmbersEnergy = null;
        internal static ConfigEntry<float> OrbitalsEmbersSpacing = null;
        internal static ConfigEntry<float> OrbitalsEmbersRadius = null;

        internal static void Bind(ConfigFile config)
        {
            const string innerSection = "Weapon / INNER FLAMES";
            const string outerSection = "Weapon / OUTER FLAMES";
            const string flareSection = "Weapon / FLARE";
            const string mirageSection = "Weapon / MIRAGE";
            const string sparksSection = "Weapon / SPARKS";
            const string OrbitalsOrbsSection = "Weapon / Orbitals / ORBS";
            const string OrbitalsFlamesSection = "Weapon / Orbitals / FLAMES";
            const string OrbitalsEmbersSection = "Weapon / Orbitals / EMBERS";

            InnerFlames = config.Bind(
                innerSection,
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

            InnerFlamesScaleMult = config.Bind(
                innerSection,
                "Scale",
                1.00f,
                OrderedDescription(
                    "Adjust the size of Inner Flames.",
                    99,
                    new AcceptableValueRange<float>(MinScaleMult, MaxScaleMult),
                    dispName: "Scale"
                )
            );

            InnerFlamesHueShift = config.Bind(
                innerSection,
                "Color",
                DefaultHueShift,
                OrderedDescription(
                    "Adjust the color of Inner Flames.",
                    98,
                    new AcceptableValueRange<float>(MinHueShift, MaxHueShift),
                    dispName: "Color"
                )
            );

            InnerFlamesEnergy = config.Bind(
                innerSection,
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
                innerSection,
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
                outerSection,
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

            OuterFlamesScaleMult = config.Bind(
                outerSection,
                "Scale",
                1.00f,
                OrderedDescription(
                    "Adjust the size of Outer Flames.",
                    99,
                    new AcceptableValueRange<float>(MinScaleMult, MaxScaleMult),
                    dispName: "Scale"
                )
            );

            OuterFlamesHueShift = config.Bind(
                outerSection,
                "Color",
                DefaultHueShift,
                OrderedDescription(
                    "Adjust the color of Outer Flames.",
                    98,
                    new AcceptableValueRange<float>(MinHueShift, MaxHueShift),
                    dispName: "Color"
                )
            );

            OuterFlamesEnergy = config.Bind(
                outerSection,
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
                outerSection,
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

            FlareScaleMult = config.Bind(
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

            FlareHueShift = config.Bind(
                flareSection,
                "Color",
                DefaultHueShift,
                OrderedDescription(
                    "Adjust the color of Flare.",
                    88,
                    new AcceptableValueRange<float>(MinHueShift, MaxHueShift),
                    dispName: "Color"
                )
            );

            Mirage = config.Bind(
                mirageSection,
                "Enabled",
                true,
                OrderedDescription(
                    "Turn Mirage on or off.",
                    85,
                    dispName: "Enabled",
                    customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
                    hideSettingName: true
                )
            );

            MirageScaleMult = config.Bind(
                mirageSection,
                "Scale",
                1.00f,
                OrderedDescription(
                    "Adjust the size of Mirage.",
                    84,
                    new AcceptableValueRange<float>(MinScaleMult, MaxScaleMult),
                    dispName: "Scale"
                )
            );

            MirageHueShift = config.Bind(
                mirageSection,
                "Color",
                DefaultHueShift,
                OrderedDescription(
                    "Adjust the color of Mirage.",
                    83,
                    new AcceptableValueRange<float>(MinHueShift, MaxHueShift),
                    dispName: "Color"
                )
            );

            Sparks = config.Bind(
                sparksSection,
                "Enabled",
                true,
                OrderedDescription(
                    "Turn Sparks on or off.",
                    80,
                    dispName: "Enabled",
                    customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
                    hideSettingName: true
                )
            );

            SparksHueShift = config.Bind(
                sparksSection,
                "Color",
                DefaultHueShift,
                OrderedDescription(
                    "Adjust the color of Sparks.",
                    79,
                    new AcceptableValueRange<float>(MinHueShift, MaxHueShift),
                    dispName: "Color"
                )
            );

            SparksEnergy = config.Bind(
                sparksSection,
                "Energy",
                DefaultEnergy,
                OrderedDescription(
                    "Adjust how intense Sparks feels.",
                    78,
                    new AcceptableValueRange<float>(MinEnergy, MaxEnergy),
                    dispName: "Energy"
                )
            );

            OrbitalsOrbs = config.Bind(
                OrbitalsOrbsSection,
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
                OrbitalsOrbsSection,
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
                OrbitalsOrbsSection,
                "Scale",
                1f,
                OrderedDescription(
                    "Adjust the size of the orbs.",
                    75,
                    new AcceptableValueRange<float>(MinOrbScaleMult, MaxOrbScaleMult),
                    dispName: "Scale"
                )
            );

            OrbitalsOrbsHueShift = config.Bind(
                OrbitalsOrbsSection,
                "Color",
                DefaultHueShift,
                OrderedDescription(
                    "Adjust the color of the orbs.",
                    74,
                    new AcceptableValueRange<float>(MinHueShift, MaxHueShift),
                    dispName: "Color"
                )
            );

            OrbitalsOrbsSpacing = config.Bind(
                OrbitalsOrbsSection,
                "Spacing",
                DefaultOrbSpacing,
                OrderedDescription(
                    "Adjust how closely the orbs follow each other.",
                    73,
                    new AcceptableValueRange<float>(MinOrbSpacing, MaxOrbSpacing),
                    dispName: "Spacing",
                    showRangeAsPercent: true
                )
            );

            OrbitalsOrbsRadius = config.Bind(
                OrbitalsOrbsSection,
                "Radius",
                1.00f,
                OrderedDescription(
                    "Adjust how wide the orbs wrap around the weapon.",
                    72,
                    new AcceptableValueRange<float>(0.50f, 1.50f),
                    dispName: "Radius"
                )
            );
            
            OrbitalsFlames = config.Bind(
                OrbitalsFlamesSection,
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
                OrbitalsFlamesSection,
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

            OrbitalsFlamesHueShift = config.Bind(
                OrbitalsFlamesSection,
                "Color",
                DefaultHueShift,
                OrderedDescription(
                    "Adjust the color of the flames.",
                    68,
                    new AcceptableValueRange<float>(MinHueShift, MaxHueShift),
                    dispName: "Color"
                )
            );
            
            OrbitalsFlamesSpacing = config.Bind(
                OrbitalsFlamesSection,
                "Spacing",
                DefaultOrbSpacing,
                OrderedDescription(
                    "Adjust how closely the flames follow each other.",
                    67,
                    new AcceptableValueRange<float>(MinOrbSpacing, MaxOrbSpacing),
                    dispName: "Spacing",
                    showRangeAsPercent: true
                )
            );

            OrbitalsFlamesRadius = config.Bind(
                OrbitalsFlamesSection,
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
                OrbitalsFlamesSection,
                "Energy",
                DefaultEnergy,
                OrderedDescription(
                    "Adjust how intense the flames feel.",
                    65,
                    new AcceptableValueRange<float>(MinEnergy, MaxEnergy),
                    dispName: "Energy"
                )
            );

            OrbitalsEmbers = config.Bind(
                OrbitalsEmbersSection,
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
                OrbitalsEmbersSection,
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

            OrbitalsEmbersHueShift = config.Bind(
                OrbitalsEmbersSection,
                "Color",
                DefaultHueShift,
                OrderedDescription(
                    "Adjust the color of the embers.",
                    58,
                    new AcceptableValueRange<float>(MinHueShift, MaxHueShift),
                    dispName: "Color"
                )
            );

            OrbitalsEmbersSpacing = config.Bind(
                OrbitalsEmbersSection,
                "Spacing",
                DefaultOrbSpacing,
                OrderedDescription(
                    "Adjust how closely the embers follow each other.",
                    57,
                    new AcceptableValueRange<float>(MinOrbSpacing, MaxOrbSpacing),
                    dispName: "Spacing",
                    showRangeAsPercent: true
                )
            );

            OrbitalsEmbersRadius = config.Bind(
                OrbitalsEmbersSection,
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
                OrbitalsEmbersSection,
                "Energy",
                DefaultEnergy,
                OrderedDescription(
                    "Adjust how intense the embers feel.",
                    55,
                    new AcceptableValueRange<float>(MinEnergy, MaxEnergy),
                    dispName: "Energy"
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