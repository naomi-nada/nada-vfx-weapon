using BepInEx.Configuration;

namespace NADA.VFX.Core.Config
{
    internal static class PluginConfig
    {
        internal static ConfigEntry<KeyboardShortcut> AttachHotkey;
        internal static ConfigEntry<KeyboardShortcut> BindHotkey;
        internal static ConfigEntry<bool> UnbindWeapon;

        internal static ConfigEntry<bool> CharacterSelectionVisibility;
        internal static ConfigEntry<bool> DroppedItemVisibility;

        internal static ConfigEntry<string> StyleName;
        internal static ConfigEntry<bool> SaveStyle;
        internal static ConfigEntry<string> LoadStyle;

        internal static ConfigEntry<float> RigRotation;
        internal static ConfigEntry<float> RigSideRotation;
        internal static ConfigEntry<float> RigLengthPosition;
        internal static ConfigEntry<float> RigSidePosition;

        internal static ConfigEntry<bool> ControlsBottomSpacer;
        internal static ConfigEntry<bool> VisibilityBottomSpacer;
        internal static ConfigEntry<bool> StylesBottomSpacer;
        internal static ConfigEntry<bool> PositionBottomSpacer;
        internal static ConfigEntry<bool> InnerFlamesBottomSpacer;
        internal static ConfigEntry<bool> OuterFlamesBottomSpacer;
        internal static ConfigEntry<bool> FlareBottomSpacer;
        internal static ConfigEntry<bool> OrbitalsOrbsBottomSpacer;
        internal static ConfigEntry<bool> OrbitalsFlamesBottomSpacer;

        internal static ConfigEntry<bool> InnerFlames = null;
        internal static ConfigEntry<float> InnerFlamesEnergy = null;
        internal static ConfigEntry<float> InnerFlamesScale = null;
        internal static ConfigEntry<float> InnerFlamesLength = null;
        internal static ConfigEntry<float> InnerFlamesHue = null;
        internal static ConfigEntry<float> InnerFlamesPosition = null;
        

        internal static ConfigEntry<bool> OuterFlames = null;
        internal static ConfigEntry<bool> OuterFlamesDragEnabled = null;
        internal static ConfigEntry<float> OuterFlamesEnergy = null;
        internal static ConfigEntry<float> OuterFlamesScale = null;
        internal static ConfigEntry<float> OuterFlamesLength = null;
        internal static ConfigEntry<float> OuterFlamesHue = null;
        internal static ConfigEntry<float> OuterFlamesPosition = null;

        internal static ConfigEntry<bool> Flare = null;
        internal static ConfigEntry<float> FlareScale = null;
        internal static ConfigEntry<float> FlareHue = null;

        internal static ConfigEntry<bool> OrbitalsOrbs = null;
        internal static ConfigEntry<float> OrbitalsOrbsCount = null;
        internal static ConfigEntry<float> OrbitalsOrbsDrift = null;
        internal static ConfigEntry<float> OrbitalsOrbsScale = null;
        internal static ConfigEntry<float> OrbitalsOrbsHue = null;
        internal static ConfigEntry<float> OrbitalsOrbsSpeed = null;
        internal static ConfigEntry<float> OrbitalsOrbsSpacing = null;
        internal static ConfigEntry<float> OrbitalsOrbsLength = null;
        internal static ConfigEntry<float> OrbitalsOrbsRadius = null;
        internal static ConfigEntry<float> OrbitalsOrbsCycles = null;

        internal static ConfigEntry<bool> OrbitalsFlames = null;
        internal static ConfigEntry<float> OrbitalsFlamesCount = null;
        internal static ConfigEntry<float> OrbitalsFlamesEnergy = null;
        internal static ConfigEntry<float> OrbitalsFlamesDrift = null;
        internal static ConfigEntry<float> OrbitalsFlamesHue = null;
        internal static ConfigEntry<float> OrbitalsFlamesSpeed = null;
        internal static ConfigEntry<float> OrbitalsFlamesSpacing = null;
        internal static ConfigEntry<float> OrbitalsFlamesLength = null;
        internal static ConfigEntry<float> OrbitalsFlamesRadius = null;
        internal static ConfigEntry<float> OrbitalsFlamesCycles = null;

        internal static ConfigEntry<bool> OrbitalsEmbers = null;
        internal static ConfigEntry<float> OrbitalsEmbersCount = null;
        internal static ConfigEntry<float> OrbitalsEmbersEnergy = null;
        internal static ConfigEntry<float> OrbitalsEmbersDrift = null;
        internal static ConfigEntry<float> OrbitalsEmbersHue = null;
        internal static ConfigEntry<float> OrbitalsEmbersSpeed = null;
        internal static ConfigEntry<float> OrbitalsEmbersSpacing = null;
        internal static ConfigEntry<float> OrbitalsEmbersLength = null;
        internal static ConfigEntry<float> OrbitalsEmbersRadius = null;
        internal static ConfigEntry<float> OrbitalsEmbersCycles = null;

        internal const float MinRigRotation = -180f;
        internal const float MaxRigRotation = 180f;
        internal const float DefaultRigRotation = 0f;
        
        internal const float MinRigSideRotation = -180f;
        internal const float MaxRigSideRotation = 180f;
        internal const float DefaultRigSideRotation = 0f;

        internal const float MinRigLengthPosition = -1.00f;
        internal const float MaxRigLengthPosition = 1.00f;
        internal const float DefaultRigLengthPosition = 0f;

        internal const float MinRigSidePosition = -1.00f;
        internal const float MaxRigSidePosition = 1.00f;
        internal const float DefaultRigSidePosition = 0f;
        
        internal const float MinFlamePosition = -1.50f;
        internal const float MaxFlamePosition = 1.50f;
        internal const float DefaultFlamePosition = 0f;

        internal const float MaxScaleMult = 1.50f;
        internal const float MinScaleMult = 0.50f;

        internal const float MinHue = -0.50f;
        internal const float MaxHue = 0.50f;
        internal const float DefaultHue = 0.00f;

        internal const float MinEnergy = 0.00f;
        internal const float MaxEnergy = 1.00f;
        internal const float DefaultEnergy = 0.00f;
        
        internal const float MinFlameLength = 0.10f;
        internal const float MaxFlameLength = 1.50f;
        internal const float DefaultFlameLength = 0.80f;

        internal const float MinOrbScaleMult = 0.2f;
        internal const float MaxOrbScaleMult = 1.8f;

        internal const float MinCountNormalized = 0.00f;
        internal const float MaxCountNormalized = 1.00f;
        internal const float DefaultCountNormalized = 0.00f;

        internal const float MinOrbitalsSpeed = 0.00f;
        internal const float MaxOrbitalsSpeed = 0.16f;
        internal const float DefaultOrbitalsSpeed = 0.08f;

        internal const float MinOrbitalsSpacing = 0.00f;
        internal const float MaxOrbitalsSpacing = 1.00f;
        internal const float DefaultOrbitalsSpacing = 0.50f;

        internal const float MinOrbitalsRadiusMultiplier = 0.20f;
        internal const float MaxOrbitalsRadiusMultiplier = 1.80f;
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
            const string hotkeysSection = "CONTROLS";
            const string visibilitySection = "VISIBILITY";
            const string stylesSection = "STYLES";
            const string positionSection = "POSITIONING";
            const string innerFlamesSection = "INNER FLAMES";
            const string outerFlamesSection = "OUTER FLAMES";
            const string flareSection = "FLARE";
            const string orbitalsOrbsSection = "Orbitals: ORBS";
            const string orbitalsFlamesSection = "Orbitals: FLAMES";
            const string orbitalsEmbersSection = "Orbitals: EMBERS";

            Plugin.DebugLoggingEnabled = config.Bind(
                "Debug",
                "Enable Debug Logging",
                true,
                OrderedDescription("Enables verbose NADA VFX structure/discovery logs.", 500, isAdvanced: true)
            );

            Plugin.EquipLoggingEnabled = config.Bind(
                "Debug",
                "Enable Equip Logging",
                true,
                OrderedDescription("Logs one concise NADA VFX line when an equipped item is processed.", 450, isAdvanced: true)
            );
            
            CharacterSelectionVisibility = config.Bind(
                visibilitySection,
                "VFX Visibility on Character Selection",
                true,
                OrderedDescription(
                    "Show bound NADA VFX on character selection / start screen weapon previews.",
                    300,
                    dispName: "Character Selection",
                    customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
                    hideSettingName: true
                )
            );

            DroppedItemVisibility = config.Bind(
                visibilitySection,
                "VFX Visibility on Dropped Items",
                false,
                OrderedDescription(
                    "Show bound NADA VFX on dropped world item instances.",
                    295,
                    dispName: "Dropped Items",
                    customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
                    hideSettingName: true
                )
            );
            
            DroppedItemVisibility.SettingChanged += (_, __) =>
            {
                Plugin.Instance?.RefreshDroppedItemVisibility();
                config.Save();
            };

            VisibilityBottomSpacer = config.Bind(
                visibilitySection,
                "__Visibility Bottom Spacer",
                false,
                OrderedDescription("", 
                    -999, 
                    customDrawer: ConfigurationManagerDrawers.DrawSectionSpacer, 
                    hideSettingName: true, 
                    hideDefaultButton: true)
            );

            AttachHotkey = config.Bind(
                hotkeysSection,
                "Attach to Weapon Hotkey",
                KeyboardShortcut.Empty,
                OrderedDescription("Press to attach NADA VFX to the equipped weapon.", 
                    300, 
                    dispName: "Attach to Weapon Hotkey")
            );

            AttachHotkey.SettingChanged += (_, __) => config.Save();

            BindHotkey = config.Bind(
                hotkeysSection,
                "Bind to Weapon Hotkey",
                KeyboardShortcut.Empty,
                OrderedDescription("Press to bind the current NADA VFX rig/settings to the equipped weapon.", 
                    275,
                    dispName: "Bind to Weapon Hotkey")
            );

            BindHotkey.SettingChanged += (_, __) => config.Save();

            UnbindWeapon = config.Bind(
                hotkeysSection,
                "Unbind Current Weapon",
                false,
                OrderedDescription(
                    "Clears NADA VFX from the equipped weapon and refreshes it using current manager settings.",
                    260,
                    dispName: "Unbind Current Weapon",
                    customDrawer: ConfigurationManagerDrawers.DrawUnbindWeaponButton,
                    hideSettingName: true,
                    hideDefaultButton: true
                )
            );

            ControlsBottomSpacer = config.Bind(
                hotkeysSection,
                "__Controls Bottom Spacer",
                false,
                OrderedDescription("", -999, customDrawer: ConfigurationManagerDrawers.DrawSectionSpacer, hideSettingName: true, hideDefaultButton: true)
            );

            StyleName = config.Bind(
                stylesSection,
                "Style Name",
                string.Empty,
                OrderedDescription("Name used when saving the current style.", 240, dispName: "Style Name", isAdvanced: true, browsable: false)
            );

            LoadStyle = config.Bind(
                stylesSection,
                "Load Style",
                "Default",
                OrderedDescription(
                    "Load a saved style into the manager settings.",
                    230,
                    dispName: "Choose Style",
                    customDrawer: ConfigurationManagerDrawers.DrawLoadStyleDropdown,
                    hideDefaultButton: true
                )
            );

            LoadStyle.SettingChanged += (_, __) =>
            {
                if (Plugin.Instance != null)
                    Plugin.Instance.LoadStyleIntoManager(LoadStyle.Value);

                config.Save();
            };

            SaveStyle = config.Bind(
                stylesSection,
                "Save Style",
                false,
                OrderedDescription(
                    "Save the current manager settings as a named style.",
                    220,
                    dispName: "Save Style",
                    customDrawer: ConfigurationManagerDrawers.DrawSaveStyleRow,
                    hideDefaultButton: true
                )
            );

            StylesBottomSpacer = config.Bind(
                stylesSection,
                "__Styles Bottom Spacer",
                false,
                OrderedDescription("", -999, customDrawer: ConfigurationManagerDrawers.DrawSectionSpacer, hideSettingName: true, hideDefaultButton: true)
            );

            RigRotation = config.Bind(
                positionSection,
                "Rotation",
                DefaultRigRotation,
                OrderedDescription(
                    "Rotate the whole NADA rig around the weapon.",
                    205,
                    new AcceptableValueRange<float>(MinRigRotation, MaxRigRotation),
                    dispName: "Forward Tilt"
                )
            );
            
            RigSideRotation = config.Bind(
                positionSection,
                "Side Rotation",
                DefaultRigSideRotation,
                OrderedDescription(
                    "Rotate the whole NADA rig left or right around the weapon.",
                    204,
                    new AcceptableValueRange<float>(MinRigSideRotation, MaxRigSideRotation),
                    dispName: "Side Tilt"
                )
            );

            RigLengthPosition = config.Bind(
                positionSection,
                "Length Position",
                DefaultRigLengthPosition,
                OrderedDescription(
                    "Slide the whole NADA rig up or down the weapon length.",
                    203,
                    new AcceptableValueRange<float>(MinRigLengthPosition, MaxRigLengthPosition),
                    dispName: "Forward Position"
                )
            );
            
            RigSidePosition = config.Bind(
                positionSection,
                "Side Position",
                DefaultRigSidePosition,
                OrderedDescription(
                    "Slide the whole NADA rig left or right across the weapon.",
                    202,
                    new AcceptableValueRange<float>(MinRigSidePosition, MaxRigSidePosition),
                    dispName: "Side Position"
                )
            );
            
            RigSidePosition.SettingChanged += (_, __) =>
                Plugin.Instance?.RefreshExistingUnboundEquippedRigsOnly();

            RigLengthPosition.SettingChanged += (_, __) =>
                Plugin.Instance?.RefreshExistingUnboundEquippedRigsOnly();

            RigSideRotation.SettingChanged += (_, __) =>
                Plugin.Instance?.RefreshExistingUnboundEquippedRigsOnly();

            RigRotation.SettingChanged += (_, __) =>
                Plugin.Instance?.RefreshExistingUnboundEquippedRigsOnly();

            PositionBottomSpacer = config.Bind(
                positionSection,
                "__Position Bottom Spacer",
                false,
                OrderedDescription("", -999, customDrawer: ConfigurationManagerDrawers.DrawSectionSpacer, hideSettingName: true, hideDefaultButton: true)
            );

            InnerFlames = config.Bind(
                innerFlamesSection,
                "Enabled",
                true,
                OrderedDescription("Turn Inner Flames on or off.", 200, dispName: "Enabled", customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel, hideSettingName: true)
            );

            InnerFlamesEnergy = config.Bind(
                innerFlamesSection,
                "Energy",
                DefaultEnergy,
                OrderedDescription("Adjust how intense Inner Flames feels.", 199, new AcceptableValueRange<float>(MinEnergy, MaxEnergy), dispName: "Energy")
            );

            InnerFlamesScale = config.Bind(
                innerFlamesSection,
                "Scale",
                1.00f,
                OrderedDescription("Adjust the size of Inner Flames.", 198, new AcceptableValueRange<float>(MinScaleMult, MaxScaleMult), dispName: "Scale")
            );

            InnerFlamesLength = config.Bind(
                innerFlamesSection,
                "Length",
                DefaultFlameLength,
                OrderedDescription(
                    "Adjust how much of the blade emits inner flames.",
                    197,
                    new AcceptableValueRange<float>(MinFlameLength, MaxFlameLength),
                    dispName: "Length"
                )
            );
            
            InnerFlamesHue = config.Bind(
                innerFlamesSection,
                "Color",
                DefaultHue,
                OrderedDescription("Adjust the color of Inner Flames.", 196, new AcceptableValueRange<float>(MinHue, MaxHue), dispName: "Color")
            );
            
            InnerFlamesPosition = config.Bind(
                innerFlamesSection,
                "Position",
                DefaultFlamePosition,
                OrderedDescription(
                    "Move Inner Flames forward or backward along the blade.",
                    195,
                    new AcceptableValueRange<float>(MinFlamePosition, MaxFlamePosition),
                    dispName: "Position"
                )
            );

            InnerFlamesBottomSpacer = config.Bind(
                innerFlamesSection,
                "__Inner Flames Bottom Spacer",
                false,
                OrderedDescription("", -998, customDrawer: ConfigurationManagerDrawers.DrawSectionSpacer, hideSettingName: true, hideDefaultButton: true)
            );

            OuterFlames = config.Bind(
                outerFlamesSection,
                "Enabled",
                true,
                OrderedDescription("Turn Outer Flames on or off.", 
                    190, 
                    dispName: "Enabled", 
                    customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel, 
                    hideSettingName: true)
            );

            OuterFlamesDragEnabled = config.Bind(
                outerFlamesSection,
                "Drag Enabled",
                false,
                OrderedDescription
                    ("Enable or disable motion-based drag on Outer Flames.", 
                        189, 
                        dispName: "Drag", 
                        customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel, 
                        hideSettingName: true)
            );

            OuterFlamesEnergy = config.Bind(
                outerFlamesSection,
                "Energy",
                DefaultEnergy,
                OrderedDescription("Adjust how intense Outer Flames feels.", 
                    188, 
                    new AcceptableValueRange<float>(MinEnergy, MaxEnergy), 
                    dispName: "Energy")
            );

            OuterFlamesScale = config.Bind(
                outerFlamesSection,
                "Scale",
                1.00f,
                OrderedDescription("Adjust the size of Outer Flames.", 
                    187, 
                    new AcceptableValueRange<float>(MinScaleMult, MaxScaleMult), 
                    dispName: "Scale")
            );
            
            OuterFlamesLength = config.Bind(
                outerFlamesSection,
                "Length",
                DefaultFlameLength,
                OrderedDescription(
                    "Adjust how much of the blade emits flames.",
                    186,
                    new AcceptableValueRange<float>(MinFlameLength, MaxFlameLength),
                    dispName: "Length"
                )
            );

            OuterFlamesHue = config.Bind(
                outerFlamesSection,
                "Color",
                DefaultHue,
                OrderedDescription("Adjust the color of Outer Flames.", 
                    185, 
                    new AcceptableValueRange<float>(MinHue, MaxHue), 
                    dispName: "Color")
            );
            
            OuterFlamesPosition = config.Bind(
                outerFlamesSection,
                "Position",
                DefaultFlamePosition,
                OrderedDescription(
                    "Move Outer Flames forward or backward along the blade.",
                    184,
                    new AcceptableValueRange<float>(MinFlamePosition, MaxFlamePosition),
                    dispName: "Position"
                )
            );

            OuterFlamesBottomSpacer = config.Bind(
                outerFlamesSection,
                "__Outer Flames Bottom Spacer",
                false,
                OrderedDescription("", -999, customDrawer: ConfigurationManagerDrawers.DrawSectionSpacer, hideSettingName: true, hideDefaultButton: true)
            );

            Flare = config.Bind(
                flareSection,
                "Enabled",
                true,
                OrderedDescription("Turn Flare on or off.", 180, dispName: "Enabled", customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel, hideSettingName: true)
            );

            FlareScale = config.Bind(
                flareSection,
                "Scale",
                1.00f,
                OrderedDescription("Adjust the size of Flare.", 179, new AcceptableValueRange<float>(MinScaleMult, MaxScaleMult), dispName: "Scale")
            );

            FlareHue = config.Bind(
                flareSection,
                "Color",
                DefaultHue,
                OrderedDescription("Adjust the color of Flare.", 178, new AcceptableValueRange<float>(MinHue, MaxHue), dispName: "Color")
            );

            FlareBottomSpacer = config.Bind(
                flareSection,
                "__Flare Bottom Spacer",
                false,
                OrderedDescription("", -999, customDrawer: ConfigurationManagerDrawers.DrawSectionSpacer, hideSettingName: true, hideDefaultButton: true)
            );

            OrbitalsOrbs = config.Bind(
                orbitalsOrbsSection,
                "Enabled",
                true,
                OrderedDescription("Turn Orbs on or off.", 170, dispName: "Enabled", customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel, hideSettingName: true)
            );

            OrbitalsOrbsCount = config.Bind(
                orbitalsOrbsSection,
                "Count",
                DefaultCountNormalized,
                OrderedDescription("Adjust how many orbs are active.", 169, new AcceptableValueRange<float>(MinCountNormalized, MaxCountNormalized), dispName: "Count", showRangeAsPercent: true)
            );

            OrbitalsOrbsDrift = config.Bind(
                orbitalsOrbsSection,
                "Drift",
                DefaultDrift,
                OrderedDescription("Adjust how much the orbs drift away from their locked orbit path.", 168, new AcceptableValueRange<float>(MinDrift, MaxDrift), dispName: "Drift", showRangeAsPercent: true)
            );

            OrbitalsOrbsScale = config.Bind(
                orbitalsOrbsSection,
                "Scale",
                1f,
                OrderedDescription("Adjust the size of the orbs.", 167, new AcceptableValueRange<float>(MinOrbScaleMult, MaxOrbScaleMult), dispName: "Scale")
            );

            OrbitalsOrbsHue = config.Bind(
                orbitalsOrbsSection,
                "Color",
                DefaultHue,
                OrderedDescription("Adjust the color of the orbs.", 166, new AcceptableValueRange<float>(MinHue, MaxHue), dispName: "Color")
            );

            OrbitalsOrbsSpeed = config.Bind(
                orbitalsOrbsSection,
                "Speed",
                DefaultOrbitalsSpeed,
                OrderedDescription("Adjust how quickly the orbs travel through their orbit.", 165, new AcceptableValueRange<float>(MinOrbitalsSpeed, MaxOrbitalsSpeed), dispName: "Speed")
            );

            OrbitalsOrbsSpacing = config.Bind(
                orbitalsOrbsSection,
                "Spacing",
                DefaultOrbitalsSpacing,
                OrderedDescription("Adjust how closely the orbs follow each other.", 164, new AcceptableValueRange<float>(MinOrbitalsSpacing, MaxOrbitalsSpacing), dispName: "Spacing", showRangeAsPercent: true)
            );

            OrbitalsOrbsLength = config.Bind(
                orbitalsOrbsSection,
                "Orbit Length",
                DefaultOrbitalsLengthMultiplier,
                OrderedDescription("Adjust how far the orbs travel along the weapon before turning around.", 163, new AcceptableValueRange<float>(MinOrbitalsLengthMultiplier, MaxOrbitalsLengthMultiplier), dispName: "Length")
            );

            OrbitalsOrbsRadius = config.Bind(
                orbitalsOrbsSection,
                "Radius",
                DefaultOrbitalsRadiusMultiplier,
                OrderedDescription("Adjust how wide the orbs wrap around the weapon.", 162, new AcceptableValueRange<float>(MinOrbitalsRadiusMultiplier, MaxOrbitalsRadiusMultiplier), dispName: "Radius")
            );

            OrbitalsOrbsCycles = config.Bind(
                orbitalsOrbsSection,
                "Cycles",
                DefaultOrbitalsCycles,
                OrderedDescription("Adjust how many turns the orbs make before reversing direction.", 161, new AcceptableValueRange<float>(MinOrbitalsCycles, MaxOrbitalsCycles), dispName: "Cycles")
            );

            OrbitalsOrbsBottomSpacer = config.Bind(
                orbitalsOrbsSection,
                "__Orbitals Orbs Bottom Spacer",
                false,
                OrderedDescription("", -999, customDrawer: ConfigurationManagerDrawers.DrawSectionSpacer, hideSettingName: true, hideDefaultButton: true)
            );

            OrbitalsFlames = config.Bind(
                orbitalsFlamesSection,
                "Enabled",
                true,
                OrderedDescription("Turn Flames on or off.", 160, dispName: "Enabled", customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel, hideSettingName: true)
            );

            OrbitalsFlamesCount = config.Bind(
                orbitalsFlamesSection,
                "Count",
                DefaultCountNormalized,
                OrderedDescription("Adjust how many flames are active.", 159, new AcceptableValueRange<float>(MinCountNormalized, MaxCountNormalized), dispName: "Count", showRangeAsPercent: true)
            );

            OrbitalsFlamesEnergy = config.Bind(
                orbitalsFlamesSection,
                "Energy",
                DefaultEnergy,
                OrderedDescription("Adjust how intense the flames feel.", 158, new AcceptableValueRange<float>(MinEnergy, MaxEnergy), dispName: "Energy")
            );

            OrbitalsFlamesDrift = config.Bind(
                orbitalsFlamesSection,
                "Drift",
                DefaultDrift,
                OrderedDescription("Adjust how much the flames drift away from their locked orbit path.", 157, new AcceptableValueRange<float>(MinDrift, MaxDrift), dispName: "Drift", showRangeAsPercent: true)
            );

            OrbitalsFlamesHue = config.Bind(
                orbitalsFlamesSection,
                "Color",
                DefaultHue,
                OrderedDescription("Adjust the color of the flames.", 156, new AcceptableValueRange<float>(MinHue, MaxHue), dispName: "Color")
            );

            OrbitalsFlamesSpeed = config.Bind(
                orbitalsFlamesSection,
                "Speed",
                DefaultOrbitalsSpeed,
                OrderedDescription("Adjust how quickly the flames travel through their orbit.", 155, new AcceptableValueRange<float>(MinOrbitalsSpeed, MaxOrbitalsSpeed), dispName: "Speed")
            );

            OrbitalsFlamesSpacing = config.Bind(
                orbitalsFlamesSection,
                "Spacing",
                DefaultOrbitalsSpacing,
                OrderedDescription("Adjust how closely the flames follow each other.", 154, new AcceptableValueRange<float>(MinOrbitalsSpacing, MaxOrbitalsSpacing), dispName: "Spacing", showRangeAsPercent: true)
            );

            OrbitalsFlamesLength = config.Bind(
                orbitalsFlamesSection,
                "Orbit Length",
                DefaultOrbitalsLengthMultiplier,
                OrderedDescription("Adjust how far the flames travel along the weapon before turning around.", 153, new AcceptableValueRange<float>(MinOrbitalsLengthMultiplier, MaxOrbitalsLengthMultiplier), dispName: "Length")
            );

            OrbitalsFlamesRadius = config.Bind(
                orbitalsFlamesSection,
                "Radius",
                DefaultOrbitalsRadiusMultiplier,
                OrderedDescription("Adjust how wide the flames wrap around the weapon.", 152, new AcceptableValueRange<float>(MinOrbitalsRadiusMultiplier, MaxOrbitalsRadiusMultiplier), dispName: "Radius")
            );

            OrbitalsFlamesCycles = config.Bind(
                orbitalsFlamesSection,
                "Cycles",
                DefaultOrbitalsCycles,
                OrderedDescription("Adjust how many turns the flames make before reversing direction.", 151, new AcceptableValueRange<float>(MinOrbitalsCycles, MaxOrbitalsCycles), dispName: "Cycles")
            );

            OrbitalsFlamesBottomSpacer = config.Bind(
                orbitalsFlamesSection,
                "__Orbitals Flames Bottom Spacer",
                false,
                OrderedDescription("", -999, customDrawer: ConfigurationManagerDrawers.DrawSectionSpacer, hideSettingName: true, hideDefaultButton: true)
            );

            OrbitalsEmbers = config.Bind(
                orbitalsEmbersSection,
                "Enabled",
                true,
                OrderedDescription("Turn Embers on or off.", 150, dispName: "Enabled", customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel, hideSettingName: true)
            );

            OrbitalsEmbersCount = config.Bind(
                orbitalsEmbersSection,
                "Count",
                DefaultCountNormalized,
                OrderedDescription("Adjust how many embers are active.", 149, new AcceptableValueRange<float>(MinCountNormalized, MaxCountNormalized), dispName: "Count", showRangeAsPercent: true)
            );

            OrbitalsEmbersEnergy = config.Bind(
                orbitalsEmbersSection,
                "Energy",
                DefaultEnergy,
                OrderedDescription("Adjust how intense the embers feel.", 148, new AcceptableValueRange<float>(MinEnergy, MaxEnergy), dispName: "Energy")
            );

            OrbitalsEmbersDrift = config.Bind(
                orbitalsEmbersSection,
                "Drift",
                DefaultDrift,
                OrderedDescription("Adjust how much the embers drift away from their locked orbit path.", 147, new AcceptableValueRange<float>(MinDrift, MaxDrift), dispName: "Drift", showRangeAsPercent: true)
            );

            OrbitalsEmbersHue = config.Bind(
                orbitalsEmbersSection,
                "Color",
                DefaultHue,
                OrderedDescription("Adjust the color of the embers.", 146, new AcceptableValueRange<float>(MinHue, MaxHue), dispName: "Color")
            );

            OrbitalsEmbersSpeed = config.Bind(
                orbitalsEmbersSection,
                "Speed",
                DefaultOrbitalsSpeed,
                OrderedDescription("Adjust how quickly the embers travel through their orbit.", 145, new AcceptableValueRange<float>(MinOrbitalsSpeed, MaxOrbitalsSpeed), dispName: "Speed")
            );

            OrbitalsEmbersSpacing = config.Bind(
                orbitalsEmbersSection,
                "Spacing",
                DefaultOrbitalsSpacing,
                OrderedDescription("Adjust how closely the embers follow each other.", 144, new AcceptableValueRange<float>(MinOrbitalsSpacing, MaxOrbitalsSpacing), dispName: "Spacing", showRangeAsPercent: true)
            );

            OrbitalsEmbersLength = config.Bind(
                orbitalsEmbersSection,
                "Orbit Length",
                DefaultOrbitalsLengthMultiplier,
                OrderedDescription("Adjust how far the embers travel along the weapon before turning around.", 143, new AcceptableValueRange<float>(MinOrbitalsLengthMultiplier, MaxOrbitalsLengthMultiplier), dispName: "Length")
            );

            OrbitalsEmbersRadius = config.Bind(
                orbitalsEmbersSection,
                "Radius",
                DefaultOrbitalsRadiusMultiplier,
                OrderedDescription("Adjust how wide the embers wrap around the weapon.", 142, new AcceptableValueRange<float>(MinOrbitalsRadiusMultiplier, MaxOrbitalsRadiusMultiplier), dispName: "Radius")
            );

            OrbitalsEmbersCycles = config.Bind(
                orbitalsEmbersSection,
                "Cycles",
                DefaultOrbitalsCycles,
                OrderedDescription("Adjust how many turns the embers make before reversing direction.", 141, new AcceptableValueRange<float>(MinOrbitalsCycles, MaxOrbitalsCycles), dispName: "Cycles")
            );

            config.Save();
        }

        private static ConfigDescription OrderedDescription(
            string description,
            int order,
            AcceptableValueBase acceptableValues = null,
            string dispName = null,
            System.Action<ConfigEntryBase> customDrawer = null,
            bool? showRangeAsPercent = null,
            bool? isAdvanced = null,
            bool? hideSettingName = null,
            bool? browsable = null,
            bool? hideDefaultButton = null)
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
                    HideSettingName = hideSettingName,
                    Browsable = browsable,
                    HideDefaultButton = hideDefaultButton
                }
            );
        }
    }
}