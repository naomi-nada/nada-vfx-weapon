using BepInEx.Configuration;

namespace NADA.VFX.Core.Config
{
    // Inner Flames, probably no Z rot needed
    // Outer Flames, probably no Z rot needed
    // Strands, probably no Y rot needed
    // Orbitals Orbs, probably no Y rot needed
    // Orbitals Cores, probably no Y rot needed
    // Orbitals Flames, probably no Y rot needed
    // Orbitals Embers, probably no Y rot needed

    internal static class PluginConfig
    {
        internal static ConfigEntry<bool> CharacterSelectionVisibility;
        internal static ConfigEntry<bool> DroppedItemVisibility;
        
        internal static ConfigEntry<KeyboardShortcut> AttachHotkey;
        internal static ConfigEntry<KeyboardShortcut> BindHotkey;
        internal static ConfigEntry<bool> UnbindWeapon;

        internal static ConfigEntry<bool> SaveStyle;
        internal static ConfigEntry<string> StyleName;
        internal static ConfigEntry<string> LoadStyle;

        internal static ConfigEntry<float> RigXOffset;
        internal static ConfigEntry<float> RigYOffset;
        internal static ConfigEntry<float> RigZOffset;
        internal static ConfigEntry<float> RigXRotation;
        internal static ConfigEntry<float> RigYRotation;
        internal static ConfigEntry<float> RigZRotation;

        internal static ConfigEntry<bool> ControlsBottomSpacer;
        internal static ConfigEntry<bool> VisibilityBottomSpacer;
        internal static ConfigEntry<bool> StylesBottomSpacer;
        internal static ConfigEntry<bool> RigTransformBottomSpacer;
        internal static ConfigEntry<bool> InnerFlamesBottomSpacer;
        internal static ConfigEntry<bool> OuterFlamesBottomSpacer;
        internal static ConfigEntry<bool> StrandsBottomSpacer;
        internal static ConfigEntry<bool> SparksBottomSpacer;
        internal static ConfigEntry<bool> FlareBottomSpacer;
        internal static ConfigEntry<bool> AuraBottomSpacer;
        internal static ConfigEntry<bool> OrbitalsOrbsBottomSpacer;
        internal static ConfigEntry<bool> OrbitalsCoresBottomSpacer;
        internal static ConfigEntry<bool> OrbitalsFlamesBottomSpacer;
        internal static ConfigEntry<bool> OrbitalsEmbersBottomSpacer;

        internal static ConfigEntry<bool> InnerFlames = null;
        internal static ConfigEntry<bool> InnerFlamesWorld = null;
        internal static ConfigEntry<bool> InnerFlamesBlack = null;
        internal static ConfigEntry<bool> InnerFlamesWhite = null;
        internal static ConfigEntry<float> InnerFlamesEnergy = null;
        internal static ConfigEntry<float> InnerFlamesScale = null;
        internal static ConfigEntry<float> InnerFlamesLuminance = null;
        internal static ConfigEntry<float> InnerFlamesHue = null;
        internal static ConfigEntry<float> InnerFlamesLifetime = null;
        internal static ConfigEntry<float> InnerFlamesLength = null;
        internal static ConfigEntry<float> InnerFlamesWidth = null;
        internal static ConfigEntry<float> InnerFlamesXOffset = null;
        internal static ConfigEntry<float> InnerFlamesYOffset = null;
        internal static ConfigEntry<float> InnerFlamesZOffset = null;
        internal static ConfigEntry<float> InnerFlamesXRotation = null;
        internal static ConfigEntry<float> InnerFlamesYRotation = null;
        internal static ConfigEntry<float> InnerFlamesZRotation = null;

        internal static ConfigEntry<bool> OuterFlames = null;
        internal static ConfigEntry<bool> OuterFlamesWorld = null;
        internal static ConfigEntry<bool> OuterFlamesBlack = null;
        internal static ConfigEntry<bool> OuterFlamesWhite = null;
        internal static ConfigEntry<bool> OuterFlamesDragEnabled = null;
        internal static ConfigEntry<float> OuterFlamesEnergy = null;
        internal static ConfigEntry<float> OuterFlamesScale = null;
        internal static ConfigEntry<float> OuterFlamesLuminance = null;
        internal static ConfigEntry<float> OuterFlamesHue = null;
        internal static ConfigEntry<float> OuterFlamesLifetime = null;
        internal static ConfigEntry<float> OuterFlamesLength = null;
        internal static ConfigEntry<float> OuterFlamesWidth = null;
        internal static ConfigEntry<float> OuterFlamesXOffset = null;
        internal static ConfigEntry<float> OuterFlamesYOffset = null;
        internal static ConfigEntry<float> OuterFlamesZOffset = null;
        internal static ConfigEntry<float> OuterFlamesXRotation = null;
        internal static ConfigEntry<float> OuterFlamesYRotation = null;
        internal static ConfigEntry<float> OuterFlamesZRotation = null;

        internal static ConfigEntry<bool> Strands = null;
        internal static ConfigEntry<bool> StrandsSpectrum = null;
        internal static ConfigEntry<float> StrandsEnergy = null;
        internal static ConfigEntry<float> StrandsScaleWhole = null;
        internal static ConfigEntry<float> StrandsScaleParts = null;
        internal static ConfigEntry<float> StrandsLuminance = null;
        internal static ConfigEntry<float> StrandsHue = null;
        internal static ConfigEntry<float> StrandsLifetime = null;
        internal static ConfigEntry<float> StrandsLength = null;
        internal static ConfigEntry<float> StrandsSpectrumSpeed = null;
        internal static ConfigEntry<float> StrandsSpeed = null;
        internal static ConfigEntry<float> StrandsRadius = null;
        internal static ConfigEntry<float> StrandsXOffset = null;
        internal static ConfigEntry<float> StrandsYOffset = null;
        internal static ConfigEntry<float> StrandsZOffset = null;
        internal static ConfigEntry<float> StrandsXRotation = null;
        internal static ConfigEntry<float> StrandsYRotation = null;
        internal static ConfigEntry<float> StrandsZRotation = null;
        internal static ConfigEntry<float> StrandsDrift = null;

        internal static ConfigEntry<bool> Sparks = null;
        internal static ConfigEntry<float> SparksEnergy = null;
        internal static ConfigEntry<float> SparksScale = null;
        internal static ConfigEntry<float> SparksLuminance = null;
        internal static ConfigEntry<float> SparksHue = null;
        internal static ConfigEntry<float> SparksLength = null;
        internal static ConfigEntry<float> SparksWidth = null;
        internal static ConfigEntry<float> SparksXOffset = null;
        internal static ConfigEntry<float> SparksYOffset = null;
        internal static ConfigEntry<float> SparksZOffset = null;
        internal static ConfigEntry<float> SparksXRotation = null;
        internal static ConfigEntry<float> SparksYRotation = null;
        internal static ConfigEntry<float> SparksZRotation = null;

        internal static ConfigEntry<bool> Flare = null;
        internal static ConfigEntry<float> FlareScale = null;
        internal static ConfigEntry<float> FlareLuminance = null;
        internal static ConfigEntry<float> FlareHue = null;
        internal static ConfigEntry<float> FlareXOffset = null;
        internal static ConfigEntry<float> FlareYOffset = null;
        internal static ConfigEntry<float> FlareZOffset = null;

        internal static ConfigEntry<bool> Aura = null;
        internal static ConfigEntry<float> AuraScale = null;
        internal static ConfigEntry<float> AuraLuminance = null;
        internal static ConfigEntry<float> AuraHue = null;
        internal static ConfigEntry<float> AuraXOffset = null;
        internal static ConfigEntry<float> AuraYOffset = null;
        internal static ConfigEntry<float> AuraZOffset = null;
        internal static ConfigEntry<float> AuraXRotation = null;
        internal static ConfigEntry<float> AuraYRotation = null;
        internal static ConfigEntry<float> AuraZRotation = null;

        internal static ConfigEntry<bool> OrbitalsOrbs = null;
        internal static ConfigEntry<bool> OrbitalsOrbsSnake = null;
        internal static ConfigEntry<bool> OrbitalsOrbsGlue = null;
        internal static ConfigEntry<bool> OrbitalsOrbsSync = null;
        internal static ConfigEntry<float> OrbitalsOrbsCount = null;
        internal static ConfigEntry<float> OrbitalsOrbsScale = null;
        internal static ConfigEntry<float> OrbitalsOrbsLuminance = null;
        internal static ConfigEntry<float> OrbitalsOrbsHue = null;
        internal static ConfigEntry<float> OrbitalsOrbsLength = null;
        internal static ConfigEntry<float> OrbitalsOrbsSpeed = null;
        internal static ConfigEntry<float> OrbitalsOrbsSpacing = null;
        internal static ConfigEntry<float> OrbitalsOrbsRadius = null;
        internal static ConfigEntry<float> OrbitalsOrbsCycles = null;
        internal static ConfigEntry<float> OrbitalsOrbsXOffset = null;
        internal static ConfigEntry<float> OrbitalsOrbsYOffset = null;
        internal static ConfigEntry<float> OrbitalsOrbsZOffset = null;
        internal static ConfigEntry<float> OrbitalsOrbsXRotation = null;
        internal static ConfigEntry<float> OrbitalsOrbsYRotation = null;
        internal static ConfigEntry<float> OrbitalsOrbsZRotation = null;
        internal static ConfigEntry<float> OrbitalsOrbsDrift = null;

        internal static ConfigEntry<bool> OrbitalsCores = null;
        internal static ConfigEntry<bool> OrbitalsCoresSnake = null;
        internal static ConfigEntry<bool> OrbitalsCoresGlue = null;
        internal static ConfigEntry<bool> OrbitalsCoresSync = null;
        internal static ConfigEntry<bool> OrbitalsCoresSpin = null;
        internal static ConfigEntry<float> OrbitalsCoresCount = null;
        internal static ConfigEntry<float> OrbitalsCoresScale = null;
        internal static ConfigEntry<float> OrbitalsCoresLuminance = null;
        internal static ConfigEntry<float> OrbitalsCoresHue = null;
        internal static ConfigEntry<float> OrbitalsCoresSpinSpeed = null;
        internal static ConfigEntry<float> OrbitalsCoresLength = null;
        internal static ConfigEntry<float> OrbitalsCoresSpeed = null;
        internal static ConfigEntry<float> OrbitalsCoresSpacing = null;
        internal static ConfigEntry<float> OrbitalsCoresRadius = null;
        internal static ConfigEntry<float> OrbitalsCoresCycles = null;
        internal static ConfigEntry<float> OrbitalsCoresXOffset = null;
        internal static ConfigEntry<float> OrbitalsCoresYOffset = null;
        internal static ConfigEntry<float> OrbitalsCoresZOffset = null;
        internal static ConfigEntry<float> OrbitalsCoresXRotation = null;
        internal static ConfigEntry<float> OrbitalsCoresYRotation = null;
        internal static ConfigEntry<float> OrbitalsCoresZRotation = null;
        internal static ConfigEntry<float> OrbitalsCoresDrift = null;

        internal static ConfigEntry<bool> OrbitalsFlames = null;
        internal static ConfigEntry<float> OrbitalsFlamesCount = null;
        internal static ConfigEntry<float> OrbitalsFlamesEnergy = null;
        internal static ConfigEntry<float> OrbitalsFlamesScale = null;
        internal static ConfigEntry<float> OrbitalsFlamesLuminance = null;
        internal static ConfigEntry<float> OrbitalsFlamesHue = null;
        internal static ConfigEntry<float> OrbitalsFlamesLifetime = null;
        internal static ConfigEntry<float> OrbitalsFlamesLength = null;
        internal static ConfigEntry<float> OrbitalsFlamesSpeed = null;
        internal static ConfigEntry<float> OrbitalsFlamesSpacing = null;
        internal static ConfigEntry<float> OrbitalsFlamesRadius = null;
        internal static ConfigEntry<float> OrbitalsFlamesCycles = null;
        internal static ConfigEntry<float> OrbitalsFlamesXOffset = null;
        internal static ConfigEntry<float> OrbitalsFlamesYOffset = null;
        internal static ConfigEntry<float> OrbitalsFlamesZOffset = null;
        internal static ConfigEntry<float> OrbitalsFlamesXRotation = null;
        internal static ConfigEntry<float> OrbitalsFlamesYRotation = null;
        internal static ConfigEntry<float> OrbitalsFlamesZRotation = null;
        internal static ConfigEntry<float> OrbitalsFlamesDrift = null;

        internal static ConfigEntry<bool> OrbitalsEmbers = null;
        internal static ConfigEntry<float> OrbitalsEmbersCount = null;
        internal static ConfigEntry<float> OrbitalsEmbersEnergy = null;
        internal static ConfigEntry<float> OrbitalsEmbersScale = null;
        internal static ConfigEntry<float> OrbitalsEmbersLuminance = null;
        internal static ConfigEntry<float> OrbitalsEmbersHue = null;
        internal static ConfigEntry<float> OrbitalsEmbersLifetime = null;
        internal static ConfigEntry<float> OrbitalsEmbersLength = null;
        internal static ConfigEntry<float> OrbitalsEmbersSpeed = null;
        internal static ConfigEntry<float> OrbitalsEmbersSpacing = null;
        internal static ConfigEntry<float> OrbitalsEmbersRadius = null;
        internal static ConfigEntry<float> OrbitalsEmbersCycles = null;
        internal static ConfigEntry<float> OrbitalsEmbersXOffset = null;
        internal static ConfigEntry<float> OrbitalsEmbersYOffset = null;
        internal static ConfigEntry<float> OrbitalsEmbersZOffset = null;
        internal static ConfigEntry<float> OrbitalsEmbersXRotation = null;
        internal static ConfigEntry<float> OrbitalsEmbersYRotation = null;
        internal static ConfigEntry<float> OrbitalsEmbersZRotation = null;
        internal static ConfigEntry<float> OrbitalsEmbersDrift = null;

        internal const float MinEffectOffset = -2.0f;
        internal const float MaxEffectOffset = 2.0f;
        internal const float DefaultEffectOffset = 0f;

        internal const float MinEffectRotation = -180f;
        internal const float MaxEffectRotation = 180f;
        internal const float DefaultEffectRotation = 0f;

        internal const float defaultScaleMult = 1.0f;
        internal const float MinScaleMult = 0.25f;
        internal const float MaxScaleMult = 1.75f;

        internal const float DefaultLuminance = 1.0f;
        internal const float MinLuminance = 0.25f;
        internal const float MaxLuminance = 1.75f;

        internal const float MinHue = -0.50f;
        internal const float MaxHue = 0.50f;
        internal const float DefaultHue = 0.00f;

        internal const float MinSpectrumSpeed = 0.25f;
        internal const float MaxSpectrumSpeed = 1.75f;
        internal const float DefaultSpectrumSpeed = 1.00f;

        internal const float MinEnergy = 0.00f;
        internal const float MaxEnergy = 1.00f;
        internal const float DefaultEnergy = 0.00f;

        internal const float MinFlameLength = 0.10f;
        internal const float MaxFlameLength = 1.50f;
        internal const float DefaultFlameLength = 0.80f;

        internal const float MinFlameWidth = 0.10f;
        internal const float MaxFlameWidth = 5.10f;
        internal const float DefaultFlameWidth = 2.60f;

        internal const float MinSparksWidth = 0.25f;
        internal const float MaxSparksWidth = 1.75f;
        internal const float DefaultSparksWidth = 1.00f;

        internal const float MinAuraScale = 1.10f;
        internal const float MaxAuraScale = 2.10f;
        internal const float DefaultAuraScale = 1.60f;

        internal const float MinLifetime = 0.25f;
        internal const float MaxLifetime = 1.75f;
        internal const float DefaultLifetime = 1.00f;

        internal const float MinDrift = 0.00f;
        internal const float MaxDrift = 1.00f;
        internal const float DefaultDrift = 0.00f;

        internal const float MinOrbScaleMult = 0.20f;
        internal const float MaxOrbScaleMult = 1.80f;

        internal const float MinCoreScaleMult = 0.20f;
        internal const float MaxCoreScaleMult = 1.80f;

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

        internal const float MinOrbitalsLength = 0.25f;
        internal const float MaxOrbitalsLength = 1.75f;
        internal const float DefaultOrbitalsLength = 1.00f;

        internal const float MinOrbitalsCycles = 1.00f;
        internal const float MaxOrbitalsCycles = 5.00f;
        internal const float DefaultOrbitalsCycles = 3.00f;

        internal const float MinCoreSpinSpeed = 0.00f;
        internal const float MaxCoreSpinSpeed = 5.00f;
        internal const float DefaultCoreSpinSpeed = 1.00f;

        internal static void Bind(ConfigFile config)
        {
            const string visibilitySection = "VISIBILITY";
            const string hotkeysSection = "CONTROLS";
            const string stylesSection = "STYLES";
            const string rigTransformSection = "RIG TRANSFORM";
            const string innerFlamesSection = "INNER FLAMES";
            const string outerFlamesSection = "OUTER FLAMES";
            const string strandsSection = "STRANDS";
            const string sparksSection = "SPARKS";
            const string flareSection = "FLARE";
            const string auraSection = "AURA";
            const string orbitalsOrbsSection = "Orbitals: ORBS";
            const string orbitalsCoresSection = "Orbitals: CORES";
            const string orbitalsFlamesSection = "Orbitals: FLAMES";
            const string orbitalsEmbersSection = "Orbitals: EMBERS";

            Plugin.DebugLoggingEnabled = config.Bind(
                "Debug",
                "Enable Debug Logging",
                true,
                OrderedDescription(
                    "Enables verbose NADA VFX structure/discovery logs.",
                    10000,
                    isAdvanced: true));

            Plugin.EquipLoggingEnabled = config.Bind(
                "Debug",
                "Enable Equip Logging",
                true,
                OrderedDescription(
                    "Logs one concise NADA VFX line when an equipped item is processed.",
                    9900,
                    isAdvanced: true));

            CharacterSelectionVisibility = config.Bind(
                visibilitySection,
                "VFX Visibility on Character Selection",
                true,
                OrderedDescription(
                    "Show bound NADA VFX on character selection / start screen weapon previews.",
                    10000,
                    dispName: "Character Selection",
                    customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
                    hideSettingName: true));

            DroppedItemVisibility = config.Bind(
                visibilitySection,
                "VFX Visibility on Dropped Items",
                false,
                OrderedDescription(
                    "Show bound NADA VFX on dropped world item instances.",
                    9900,
                    dispName: "Dropped Items",
                    customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
                    hideSettingName: true));

            DroppedItemVisibility.SettingChanged += (_, __) =>
            {
                Plugin.Instance?.RefreshDroppedItemVisibility();
                config.Save();
            };

            VisibilityBottomSpacer = config.Bind(
                visibilitySection,
                "__Visibility Bottom Spacer",
                false,
                OrderedDescription(
                    "",
                    -999,
                    customDrawer: ConfigurationManagerDrawers.DrawSectionSpacer,
                    hideSettingName: true,
                    hideDefaultButton: true));

            AttachHotkey = config.Bind(
                hotkeysSection,
                "Attach to Weapon Hotkey",
                KeyboardShortcut.Empty,
                OrderedDescription(
                    "Press to attach NADA VFX to the equipped weapon.",
                    10000,
                    dispName: "Attach to Weapon Hotkey"));

            AttachHotkey.SettingChanged += (_, __) => config.Save();

            BindHotkey = config.Bind(
                hotkeysSection,
                "Bind to Weapon Hotkey",
                KeyboardShortcut.Empty,
                OrderedDescription(
                    "Press to bind the current NADA VFX rig/settings to the equipped weapon.",
                    9900,
                    dispName: "Bind to Weapon Hotkey"));

            BindHotkey.SettingChanged += (_, __) => config.Save();

            UnbindWeapon = config.Bind(
                hotkeysSection,
                "Unbind Current Weapon",
                false,
                OrderedDescription(
                    "Clears NADA VFX from the equipped weapon and refreshes it using current manager settings.",
                    9800,
                    dispName: "Unbind Current Weapon",
                    customDrawer: ConfigurationManagerDrawers.DrawUnbindWeaponButton,
                    hideSettingName: true,
                    hideDefaultButton: true));

            ControlsBottomSpacer = config.Bind(
                hotkeysSection,
                "__Controls Bottom Spacer",
                false,
                OrderedDescription(
                    "",
                    -999,
                    customDrawer: ConfigurationManagerDrawers.DrawSectionSpacer,
                    hideSettingName: true,
                    hideDefaultButton: true));

            StyleName = config.Bind(
                stylesSection,
                "Style Name",
                string.Empty,
                OrderedDescription(
                    "Name used when saving the current style.",
                    10000,
                    dispName: "Style Name",
                    isAdvanced: true,
                    browsable: false));

            LoadStyle = config.Bind(
                stylesSection,
                "Load Style",
                "Default",
                OrderedDescription(
                    "Load a saved style into the manager settings.",
                    9900,
                    dispName: "Choose Style",
                    customDrawer: ConfigurationManagerDrawers.DrawLoadStyleDropdown,
                    hideDefaultButton: true));

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
                    9800,
                    dispName: "Save Style",
                    customDrawer: ConfigurationManagerDrawers.DrawSaveStyleRow,
                    hideDefaultButton: true));

            StylesBottomSpacer = config.Bind(
                stylesSection,
                "__Styles Bottom Spacer",
                false,
                OrderedDescription(
                    "",
                    -999,
                    customDrawer: ConfigurationManagerDrawers.DrawSectionSpacer,
                    hideSettingName: true,
                    hideDefaultButton: true));

            RigXOffset = config.Bind(
                rigTransformSection,
                "X Offset",
                DefaultEffectOffset,
                OrderedDescription(
                    "Adjust the entire Rig's X axis offset.",
                    10000,
                    new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
                    dispName: "X Offset"));

            RigYOffset = config.Bind(
                rigTransformSection,
                "Y Offset",
                DefaultEffectOffset,
                OrderedDescription(
                    "Adjust the entire Rig's Y axis offset.",
                    9900,
                    new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
                    dispName: "Y Offset"));

            RigZOffset = config.Bind(
                rigTransformSection,
                "Z Offset",
                DefaultEffectOffset,
                OrderedDescription(
                    "Adjust the entire Rig's Z axis offset.",
                    9800,
                    new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
                    dispName: "Z Offset"));

            RigXRotation = config.Bind(
                rigTransformSection,
                "X Rotation",
                DefaultEffectRotation,
                OrderedDescription(
                    "Adjust the entire Rig's X axis rotation.",
                    9700,
                    new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
                    dispName: "X Rotation"));

            RigYRotation = config.Bind(
                rigTransformSection,
                "Y Rotation",
                DefaultEffectRotation,
                OrderedDescription(
                    "Adjust the entire Rig's Y axis rotation.",
                    9600,
                    new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
                    dispName: "Y Rotation"));

            RigZRotation = config.Bind(
                rigTransformSection,
                "Z Rotation",
                DefaultEffectRotation,
                OrderedDescription(
                    "Adjust the entire Rig's Z axis rotation.",
                    9500,
                    new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
                    dispName: "Z Rotation"));

            RigXOffset.SettingChanged += (_, __) => Plugin.Instance?.RefreshExistingUnboundEquippedRigsOnly();
            RigYOffset.SettingChanged += (_, __) => Plugin.Instance?.RefreshExistingUnboundEquippedRigsOnly();
            RigZOffset.SettingChanged += (_, __) => Plugin.Instance?.RefreshExistingUnboundEquippedRigsOnly();
            RigXRotation.SettingChanged += (_, __) => Plugin.Instance?.RefreshExistingUnboundEquippedRigsOnly();
            RigYRotation.SettingChanged += (_, __) => Plugin.Instance?.RefreshExistingUnboundEquippedRigsOnly();
            RigZRotation.SettingChanged += (_, __) => Plugin.Instance?.RefreshExistingUnboundEquippedRigsOnly();

            RigTransformBottomSpacer = config.Bind(
                rigTransformSection,
                "__Position Bottom Spacer",
                false,
                OrderedDescription(
                    "",
                    -999,
                    customDrawer: ConfigurationManagerDrawers.DrawSectionSpacer,
                    hideSettingName: true,
                    hideDefaultButton: true));

            InnerFlames = config.Bind(
                innerFlamesSection,
                "Enabled",
                true,
                OrderedDescription(
                    "Turn Inner Flames on or off.",
                    10000,
                    dispName: "Enabled",
                    customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
                    hideSettingName: true));
            
            InnerFlamesWorld = config.Bind(
                innerFlamesSection,
                "World",
                true,
                OrderedDescription(
                    "Emit Inner Flames particles in world space.",
                    9950,
                    dispName: "World",
                    customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
                    hideSettingName: true));

            InnerFlamesBlack = config.Bind(
                innerFlamesSection,
                "Black",
                false,
                OrderedDescription(
                    "Force Inner Flames to black. When enabled, Color is disabled.",
                    9900,
                    dispName: "Black (WIP)",
                    customDrawer: ConfigurationManagerDrawers.DrawInnerFlamesBlackCheckbox,
                    hideSettingName: true));

            InnerFlamesWhite = config.Bind(
                innerFlamesSection,
                "White",
                false,
                OrderedDescription(
                    "Force Inner Flames to white. When enabled, Color is disabled.",
                    9800,
                    dispName: "White",
                    customDrawer: ConfigurationManagerDrawers.DrawInnerFlamesWhiteCheckbox,
                    hideSettingName: true));

            InnerFlamesEnergy = config.Bind(
                innerFlamesSection,
                "Energy",
                DefaultEnergy,
                OrderedDescription(
                    "Adjust how intense Inner Flames feels.",
                    9700,
                    new AcceptableValueRange<float>(MinEnergy, MaxEnergy),
                    dispName: "Energy"));

            InnerFlamesScale = config.Bind(
                innerFlamesSection,
                "Scale",
                defaultScaleMult,
                OrderedDescription(
                    "Adjust the size of Inner Flames.",
                    9600,
                    new AcceptableValueRange<float>(MinScaleMult, MaxScaleMult),
                    dispName: "Scale"));

            InnerFlamesLuminance = config.Bind(
                innerFlamesSection,
                "Luminance",
                DefaultLuminance,
                OrderedDescription(
                    "Adjust how strongly Inner Flames glow.",
                    9500,
                    new AcceptableValueRange<float>(MinLuminance, MaxLuminance),
                    dispName: "Luminance"));

            InnerFlamesHue = config.Bind(
                innerFlamesSection,
                "Color",
                DefaultHue,
                OrderedDescription(
                    "Adjust the color of Inner Flames.",
                    9400,
                    new AcceptableValueRange<float>(MinHue, MaxHue),
                    dispName: "Color",
                    customDrawer: ConfigurationManagerDrawers.DrawInnerFlamesColorSlider));

            InnerFlamesLifetime = config.Bind(
                innerFlamesSection,
                "Lifetime",
                DefaultLifetime,
                OrderedDescription(
                    "Adjust how long emitted inner flames remain visible.",
                    9300,
                    new AcceptableValueRange<float>(MinLifetime, MaxLifetime),
                    dispName: "Lifetime"));

            InnerFlamesLength = config.Bind(
                innerFlamesSection,
                "Length",
                DefaultFlameLength,
                OrderedDescription(
                    "Adjust the distance Inner Flames emits along the spine of a weapon.",
                    9200,
                    new AcceptableValueRange<float>(MinFlameLength, MaxFlameLength),
                    dispName: "Length"));

            InnerFlamesWidth = config.Bind(
                innerFlamesSection,
                "Width",
                DefaultFlameWidth,
                OrderedDescription(
                    "Adjust the distance Inner Flames emits across the plane of a weapon.",
                    9100,
                    new AcceptableValueRange<float>(MinFlameWidth, MaxFlameWidth),
                    dispName: "Width"));

            InnerFlamesXOffset = config.Bind(
                innerFlamesSection,
                "X Offset",
                DefaultEffectOffset,
                OrderedDescription(
                    "Adjust the X axis offset of the whole Inner Flames effect.",
                    9000,
                    new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
                    dispName: "X Offset"));

            InnerFlamesYOffset = config.Bind(
                innerFlamesSection,
                "Y Offset",
                DefaultEffectOffset,
                OrderedDescription(
                    "Adjust the Y axis offset of the whole Inner Flames effect.",
                    8900,
                    new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
                    dispName: "Y Offset"));

            InnerFlamesZOffset = config.Bind(
                innerFlamesSection,
                "Z Offset",
                DefaultEffectOffset,
                OrderedDescription(
                    "Adjust the Z axis offset of the whole Inner Flames effect.",
                    8800,
                    new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
                    dispName: "Z Offset"));

            InnerFlamesXRotation = config.Bind(
                innerFlamesSection,
                "X Rotation",
                DefaultEffectRotation,
                OrderedDescription(
                    "Rotate the whole Inner Flames effect around its local X axis.",
                    8700,
                    new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
                    dispName: "X Rotation"));

            InnerFlamesYRotation = config.Bind(
                innerFlamesSection,
                "Y Rotation",
                DefaultEffectRotation,
                OrderedDescription(
                    "Rotate the whole Inner Flames effect around its local Y axis.",
                    8600,
                    new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
                    dispName: "Y Rotation"));

            InnerFlamesZRotation = config.Bind(
                innerFlamesSection,
                "Z Rotation",
                DefaultEffectRotation,
                OrderedDescription(
                    "Rotate the whole Inner Flames effect around its local Z axis.",
                    8500,
                    new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
                    dispName: "Z Rotation"));

            InnerFlamesBlack.SettingChanged += (_, __) =>
            {
                if (InnerFlamesBlack.Value && InnerFlamesWhite.Value)
                    InnerFlamesWhite.Value = false;

                config.Save();
            };

            InnerFlamesWhite.SettingChanged += (_, __) =>
            {
                if (InnerFlamesWhite.Value && InnerFlamesBlack.Value)
                    InnerFlamesBlack.Value = false;

                config.Save();
            };

            InnerFlamesBottomSpacer = config.Bind(
                innerFlamesSection,
                "__Inner Flames Bottom Spacer",
                false,
                OrderedDescription(
                    "",
                    -999,
                    customDrawer: ConfigurationManagerDrawers.DrawSectionSpacer,
                    hideSettingName: true,
                    hideDefaultButton: true));

            OuterFlames = config.Bind(
                outerFlamesSection,
                "Enabled",
                true,
                OrderedDescription(
                    "Turn Outer Flames on or off.",
                    10000,
                    dispName: "Enabled",
                    customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
                    hideSettingName: true));
            
            OuterFlamesWorld = config.Bind(
                outerFlamesSection,
                "World",
                true,
                OrderedDescription(
                    "Emit Outer Flames particles in world space.",
                    9950,
                    dispName: "World",
                    customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
                    hideSettingName: true));

            OuterFlamesBlack = config.Bind(
                outerFlamesSection,
                "Black",
                false,
                OrderedDescription(
                    "Force Outer Flames to black. When enabled, Color is disabled.",
                    9900,
                    dispName: "Black (WIP)",
                    customDrawer: ConfigurationManagerDrawers.DrawOuterFlamesBlackCheckbox,
                    hideSettingName: true));

            OuterFlamesWhite = config.Bind(
                outerFlamesSection,
                "White",
                false,
                OrderedDescription(
                    "Force Outer Flames to white. When enabled, Color is disabled.",
                    9800,
                    dispName: "White",
                    customDrawer: ConfigurationManagerDrawers.DrawOuterFlamesWhiteCheckbox,
                    hideSettingName: true));

            OuterFlamesDragEnabled = config.Bind(
                outerFlamesSection,
                "Drag Enabled",
                false,
                OrderedDescription(
                    "Enable or disable motion-based drag on Outer Flames.",
                    9700,
                    dispName: "Drag",
                    customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
                    hideSettingName: true));

            OuterFlamesEnergy = config.Bind(
                outerFlamesSection,
                "Energy",
                DefaultEnergy,
                OrderedDescription(
                    "Adjust how intense Outer Flames feels.",
                    9600,
                    new AcceptableValueRange<float>(MinEnergy, MaxEnergy),
                    dispName: "Energy"));

            OuterFlamesScale = config.Bind(
                outerFlamesSection,
                "Scale",
                defaultScaleMult,
                OrderedDescription(
                    "Adjust the size of Outer Flames.",
                    9500,
                    new AcceptableValueRange<float>(MinScaleMult, MaxScaleMult),
                    dispName: "Scale"));

            OuterFlamesLuminance = config.Bind(
                outerFlamesSection,
                "Luminance",
                DefaultLuminance,
                OrderedDescription(
                    "Adjust how strongly Outer Flames glow.",
                    9400,
                    new AcceptableValueRange<float>(MinLuminance, MaxLuminance),
                    dispName: "Luminance"));

            OuterFlamesHue = config.Bind(
                outerFlamesSection,
                "Color",
                DefaultHue,
                OrderedDescription(
                    "Adjust the color of Outer Flames.",
                    9300,
                    new AcceptableValueRange<float>(MinHue, MaxHue),
                    dispName: "Color",
                    customDrawer: ConfigurationManagerDrawers.DrawOuterFlamesColorSlider));

            OuterFlamesLifetime = config.Bind(
                outerFlamesSection,
                "Lifetime",
                DefaultLifetime,
                OrderedDescription(
                    "Adjust how long emitted outer flames remain visible.",
                    9200,
                    new AcceptableValueRange<float>(MinLifetime, MaxLifetime),
                    dispName: "Lifetime"));

            OuterFlamesLength = config.Bind(
                outerFlamesSection,
                "Length",
                DefaultFlameLength,
                OrderedDescription(
                    "Adjust how much of the blade emits flames.",
                    9100,
                    new AcceptableValueRange<float>(MinFlameLength, MaxFlameLength),
                    dispName: "Length"));

            OuterFlamesWidth = config.Bind(
                outerFlamesSection,
                "Width",
                DefaultFlameWidth,
                OrderedDescription(
                    "Adjust the distance Outer Flames emits across the plane of a weapon.",
                    9000,
                    new AcceptableValueRange<float>(MinFlameWidth, MaxFlameWidth),
                    dispName: "Width"));

            OuterFlamesXOffset = config.Bind(
                outerFlamesSection,
                "X Offset",
                DefaultEffectOffset,
                OrderedDescription(
                    "Adjust the X axis offset of the whole Outer Flames effect.",
                    8900,
                    new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
                    dispName: "X Offset"));

            OuterFlamesYOffset = config.Bind(
                outerFlamesSection,
                "Y Offset",
                DefaultEffectOffset,
                OrderedDescription(
                    "Adjust the Y axis offset of the whole Outer Flames effect.",
                    8800,
                    new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
                    dispName: "Y Offset"));

            OuterFlamesZOffset = config.Bind(
                outerFlamesSection,
                "Z Offset",
                DefaultEffectOffset,
                OrderedDescription(
                    "Adjust the Z axis offset of the whole Outer Flames effect.",
                    8700,
                    new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
                    dispName: "Z Offset"));

            OuterFlamesXRotation = config.Bind(
                outerFlamesSection,
                "X Rotation",
                DefaultEffectRotation,
                OrderedDescription(
                    "Rotate the whole Outer Flames effect around its local X axis.",
                    8600,
                    new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
                    dispName: "X Rotation"));

            OuterFlamesYRotation = config.Bind(
                outerFlamesSection,
                "Y Rotation",
                DefaultEffectRotation,
                OrderedDescription(
                    "Rotate the whole Outer Flames effect around its local Y axis.",
                    8500,
                    new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
                    dispName: "Y Rotation"));

            OuterFlamesZRotation = config.Bind(
                outerFlamesSection,
                "Z Rotation",
                DefaultEffectRotation,
                OrderedDescription(
                    "Rotate the whole Outer Flames effect around its local Z axis.",
                    8400,
                    new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
                    dispName: "Z Rotation"));

            OuterFlamesBlack.SettingChanged += (_, __) =>
            {
                if (OuterFlamesBlack.Value && OuterFlamesWhite.Value)
                    OuterFlamesWhite.Value = false;

                config.Save();
            };

            OuterFlamesWhite.SettingChanged += (_, __) =>
            {
                if (OuterFlamesWhite.Value && OuterFlamesBlack.Value)
                    OuterFlamesBlack.Value = false;

                config.Save();
            };

            OuterFlamesBottomSpacer = config.Bind(
                outerFlamesSection,
                "__Outer Flames Bottom Spacer",
                false,
                OrderedDescription(
                    "",
                    -999,
                    customDrawer: ConfigurationManagerDrawers.DrawSectionSpacer,
                    hideSettingName: true,
                    hideDefaultButton: true));

            Strands = config.Bind(
                strandsSection,
                "Enabled",
                true,
                OrderedDescription(
                    "Turn Strands on or off.",
                    10000,
                    dispName: "Enabled",
                    customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
                    hideSettingName: true));

            StrandsSpectrum = config.Bind(
                strandsSection,
                "Spectrum",
                false,
                OrderedDescription(
                    "Continuously cycle Strands through the full color spectrum.",
                    9900,
                    dispName: "Spectrum",
                    customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
                    hideSettingName: true));

            StrandsEnergy = config.Bind(
                strandsSection,
                "Energy",
                DefaultEnergy,
                OrderedDescription(
                    "Adjust the emission intensity of the strands.",
                    9800,
                    new AcceptableValueRange<float>(MinEnergy, MaxEnergy),
                    dispName: "Energy"));

            StrandsScaleWhole = config.Bind(
                strandsSection,
                "Scale Whole",
                defaultScaleMult,
                OrderedDescription(
                    "Adjust the size of the strands.",
                    9700,
                    new AcceptableValueRange<float>(MinScaleMult, MaxScaleMult),
                    dispName: "Scale Whole"));

            StrandsScaleParts = config.Bind(
                strandsSection,
                "Scale Parts",
                1.00f,
                OrderedDescription(
                    "Adjust the size of the individual strand particles.",
                    9600,
                    new AcceptableValueRange<float>(MinScaleMult, MaxScaleMult),
                    dispName: "Scale Parts"));

            StrandsLuminance = config.Bind(
                strandsSection,
                "Luminance",
                DefaultLuminance,
                OrderedDescription(
                    "Adjust how strongly the strands glow.",
                    9500,
                    new AcceptableValueRange<float>(MinLuminance, MaxLuminance),
                    dispName: "Luminance"));

            StrandsHue = config.Bind(
                strandsSection,
                "Color",
                DefaultHue,
                OrderedDescription(
                    "Adjust the color of the strands.",
                    9400,
                    new AcceptableValueRange<float>(MinHue, MaxHue),
                    dispName: "Color",
                    customDrawer: ConfigurationManagerDrawers.DrawStrandsColorSlider));

            StrandsLifetime = config.Bind(
                strandsSection,
                "Lifetime",
                DefaultLifetime,
                OrderedDescription(
                    "Adjust the length of time strands exist.",
                    9300,
                    new AcceptableValueRange<float>(MinLifetime, MaxLifetime),
                    dispName: "Lifetime"));

            StrandsLength = config.Bind(
                strandsSection,
                "Length",
                DefaultOrbitalsLength,
                OrderedDescription(
                    "Adjust how far the strands stretch along the weapon.",
                    9200,
                    new AcceptableValueRange<float>(MinOrbitalsLength, MaxOrbitalsLength),
                    dispName: "Length"));

            StrandsSpectrumSpeed = config.Bind(
                strandsSection,
                "Spectrum Speed",
                DefaultSpectrumSpeed,
                OrderedDescription(
                    "Adjust how quickly Strands cycle through the color spectrum.",
                    9100,
                    new AcceptableValueRange<float>(MinSpectrumSpeed, MaxSpectrumSpeed),
                    dispName: "Spectrum Speed",
                    customDrawer: ConfigurationManagerDrawers.DrawStrandsSpectrumSpeedSlider));

            StrandsSpeed = config.Bind(
                strandsSection,
                "Speed",
                DefaultOrbitalsSpeed,
                OrderedDescription(
                    "Adjust how quickly the strands animate.",
                    9000,
                    new AcceptableValueRange<float>(MinOrbitalsSpeed, MaxOrbitalsSpeed),
                    dispName: "Speed"));

            StrandsRadius = config.Bind(
                strandsSection,
                "Radius",
                DefaultOrbitalsRadiusMultiplier,
                OrderedDescription(
                    "Adjust how much the strands wrap.",
                    8900,
                    new AcceptableValueRange<float>(MinOrbitalsRadiusMultiplier, MaxOrbitalsRadiusMultiplier),
                    dispName: "Radius"));

            StrandsXOffset = config.Bind(
                strandsSection,
                "X Offset",
                DefaultEffectOffset,
                OrderedDescription(
                    "Adjust the X axis offset of the whole Strands effect.",
                    8800,
                    new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
                    dispName: "X Offset"));

            StrandsYOffset = config.Bind(
                strandsSection,
                "Y Offset",
                DefaultEffectOffset,
                OrderedDescription(
                    "Adjust the Y axis offset of the whole Strands effect.",
                    8700,
                    new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
                    dispName: "Y Offset"));

            StrandsZOffset = config.Bind(
                strandsSection,
                "Z Offset",
                DefaultEffectOffset,
                OrderedDescription(
                    "Adjust the Z axis offset of the whole Strands effect.",
                    8600,
                    new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
                    dispName: "Z Offset"));

            StrandsXRotation = config.Bind(
                strandsSection,
                "X Rotation",
                DefaultEffectRotation,
                OrderedDescription(
                    "Rotate the whole Strands effect around its local X axis.",
                    8500,
                    new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
                    dispName: "X Rotation"));

            StrandsYRotation = config.Bind(
                strandsSection,
                "Y Rotation",
                DefaultEffectRotation,
                OrderedDescription(
                    "Rotate the whole Strands effect around its local Y axis.",
                    8400,
                    new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
                    dispName: "Y Rotation"));

            StrandsZRotation = config.Bind(
                strandsSection,
                "Z Rotation",
                DefaultEffectRotation,
                OrderedDescription(
                    "Rotate the whole Strands effect around its local Z axis.",
                    8300,
                    new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
                    dispName: "Z Rotation"));

            StrandsDrift = config.Bind(
                strandsSection,
                "Drift",
                DefaultDrift,
                OrderedDescription(
                    "Adjust how much the strands adhere to their motion path.",
                    8200,
                    new AcceptableValueRange<float>(MinDrift, MaxDrift),
                    dispName: "Drift"));

            StrandsBottomSpacer = config.Bind(
                strandsSection,
                "__Strands Bottom Spacer",
                false,
                OrderedDescription(
                    "",
                    -999,
                    customDrawer: ConfigurationManagerDrawers.DrawSectionSpacer,
                    hideSettingName: true,
                    hideDefaultButton: true));

            Sparks = config.Bind(
                sparksSection,
                "Enabled",
                true,
                OrderedDescription(
                    "Turn Sparks on or off.",
                    10000,
                    dispName: "Enabled",
                    customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
                    hideSettingName: true));

            SparksEnergy = config.Bind(
                sparksSection,
                "Energy",
                DefaultEnergy,
                OrderedDescription(
                    "Adjust how intense Sparks feels.",
                    9900,
                    new AcceptableValueRange<float>(MinEnergy, MaxEnergy),
                    dispName: "Energy"));

            SparksScale = config.Bind(
                sparksSection,
                "Scale",
                defaultScaleMult,
                OrderedDescription(
                    "Adjust the size of Sparks.",
                    9800,
                    new AcceptableValueRange<float>(MinScaleMult, MaxScaleMult),
                    dispName: "Scale"));

            SparksLuminance = config.Bind(
                sparksSection,
                "Luminance",
                DefaultLuminance,
                OrderedDescription(
                    "Adjust how strongly Sparks glow.",
                    9700,
                    new AcceptableValueRange<float>(MinLuminance, MaxLuminance),
                    dispName: "Luminance"));

            SparksHue = config.Bind(
                sparksSection,
                "Color",
                DefaultHue,
                OrderedDescription(
                    "Adjust the color of Sparks.",
                    9600,
                    new AcceptableValueRange<float>(MinHue, MaxHue),
                    dispName: "Color"));

            SparksLength = config.Bind(
                sparksSection,
                "Length",
                DefaultFlameLength,
                OrderedDescription(
                    "Adjust how much of the weapon emits Sparks.",
                    9500,
                    new AcceptableValueRange<float>(MinFlameLength, MaxFlameLength),
                    dispName: "Length"));

            SparksWidth = config.Bind(
                sparksSection,
                "Width",
                DefaultSparksWidth,
                OrderedDescription(
                    "Pull Sparks inward or push them wider around the weapon.",
                    9400,
                    new AcceptableValueRange<float>(MinSparksWidth, MaxSparksWidth),
                    dispName: "Width"));

            SparksXOffset = config.Bind(
                sparksSection,
                "X Offset",
                DefaultEffectOffset,
                OrderedDescription(
                    "Adjust the X axis offset of the whole Sparks effect.",
                    9300,
                    new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
                    dispName: "X Offset"));

            SparksYOffset = config.Bind(
                sparksSection,
                "Y Offset",
                DefaultEffectOffset,
                OrderedDescription(
                    "Adjust the Y axis offset of the whole Sparks effect.",
                    9200,
                    new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
                    dispName: "Y Offset"));

            SparksZOffset = config.Bind(
                sparksSection,
                "Z Offset",
                DefaultEffectOffset,
                OrderedDescription(
                    "Adjust the Z axis offset of the whole Sparks effect.",
                    9100,
                    new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
                    dispName: "Z Offset"));

            SparksXRotation = config.Bind(
                sparksSection,
                "X Rotation",
                DefaultEffectRotation,
                OrderedDescription(
                    "Rotate the whole Sparks effect around its local X axis.",
                    9000,
                    new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
                    dispName: "X Rotation"));

            SparksYRotation = config.Bind(
                sparksSection,
                "Y Rotation",
                DefaultEffectRotation,
                OrderedDescription(
                    "Rotate the whole Sparks effect around its local Y axis.",
                    8900,
                    new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
                    dispName: "Y Rotation"));

            SparksZRotation = config.Bind(
                sparksSection,
                "Z Rotation",
                DefaultEffectRotation,
                OrderedDescription(
                    "Rotate the whole Sparks effect around its local Z axis.",
                    8800,
                    new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
                    dispName: "Z Rotation"));

            SparksBottomSpacer = config.Bind(
                sparksSection,
                "__Sparks Bottom Spacer",
                false,
                OrderedDescription(
                    "",
                    -999,
                    customDrawer: ConfigurationManagerDrawers.DrawSectionSpacer,
                    hideSettingName: true,
                    hideDefaultButton: true));

            Flare = config.Bind(
                flareSection,
                "Enabled",
                true,
                OrderedDescription(
                    "Turn Flare on or off.",
                    10000,
                    dispName: "Enabled",
                    customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
                    hideSettingName: true));

            FlareScale = config.Bind(
                flareSection,
                "Scale",
                defaultScaleMult,
                OrderedDescription(
                    "Adjust the size of Flare.",
                    9900,
                    new AcceptableValueRange<float>(MinScaleMult, MaxScaleMult),
                    dispName: "Scale"));

            FlareLuminance = config.Bind(
                flareSection,
                "Luminance",
                DefaultLuminance,
                OrderedDescription(
                    "Adjust how strongly the flare glows.",
                    9800,
                    new AcceptableValueRange<float>(MinLuminance, MaxLuminance),
                    dispName: "Luminance"));

            FlareHue = config.Bind(
                flareSection,
                "Color",
                DefaultHue,
                OrderedDescription(
                    "Adjust the color of Flare.",
                    9700,
                    new AcceptableValueRange<float>(MinHue, MaxHue),
                    dispName: "Color"));

            FlareXOffset = config.Bind(
                flareSection,
                "X Offset",
                DefaultEffectOffset,
                OrderedDescription(
                    "Adjust the X axis offset of the whole Flare effect.",
                    9600,
                    new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
                    dispName: "X Offset"));

            FlareYOffset = config.Bind(
                flareSection,
                "Y Offset",
                DefaultEffectOffset,
                OrderedDescription(
                    "Adjust the Y axis offset of the whole Flare effect.",
                    9500,
                    new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
                    dispName: "Y Offset"));

            FlareZOffset = config.Bind(
                flareSection,
                "Z Offset",
                DefaultEffectOffset,
                OrderedDescription(
                    "Adjust the Z axis offset of the whole Flare effect.",
                    9400,
                    new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
                    dispName: "Z Offset"));

            FlareBottomSpacer = config.Bind(
                flareSection,
                "__Flare Bottom Spacer",
                false,
                OrderedDescription(
                    "",
                    -999,
                    customDrawer: ConfigurationManagerDrawers.DrawSectionSpacer,
                    hideSettingName: true,
                    hideDefaultButton: true));

            Aura = config.Bind(
                auraSection,
                "Enabled",
                true,
                OrderedDescription(
                    "Turn Aura on or off.",
                    10000,
                    dispName: "Enabled",
                    customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
                    hideSettingName: true));

            AuraScale = config.Bind(
                auraSection,
                "Scale",
                DefaultAuraScale,
                OrderedDescription(
                    "Adjust the size of Aura.",
                    9900,
                    new AcceptableValueRange<float>(MinAuraScale, MaxAuraScale),
                    dispName: "Scale"));

            AuraLuminance = config.Bind(
                auraSection,
                "Luminance",
                DefaultLuminance,
                OrderedDescription(
                    "Adjust how strongly Aura glows.",
                    9800,
                    new AcceptableValueRange<float>(MinLuminance, MaxLuminance),
                    dispName: "Luminance"));

            AuraHue = config.Bind(
                auraSection,
                "Color",
                DefaultHue,
                OrderedDescription(
                    "Adjust the color of Aura.",
                    9700,
                    new AcceptableValueRange<float>(MinHue, MaxHue),
                    dispName: "Color"));

            AuraXOffset = config.Bind(
                auraSection,
                "X Offset",
                DefaultEffectOffset,
                OrderedDescription(
                    "Adjust the X axis offset of the whole Aura effect.",
                    9600,
                    new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
                    dispName: "X Offset"));

            AuraYOffset = config.Bind(
                auraSection,
                "Y Offset",
                DefaultEffectOffset,
                OrderedDescription(
                    "Adjust the Y axis offset of the whole Aura effect.",
                    9500,
                    new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
                    dispName: "Y Offset"));

            AuraZOffset = config.Bind(
                auraSection,
                "Z Offset",
                DefaultEffectOffset,
                OrderedDescription(
                    "Adjust the Z axis offset of the whole Aura effect.",
                    9400,
                    new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
                    dispName: "Z Offset"));

            AuraXRotation = config.Bind(
                auraSection,
                "X Rotation",
                DefaultEffectRotation,
                OrderedDescription(
                    "Rotate the whole Aura effect around its local X axis.",
                    9300,
                    new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
                    dispName: "X Rotation"));

            AuraYRotation = config.Bind(
                auraSection,
                "Y Rotation",
                DefaultEffectRotation,
                OrderedDescription(
                    "Rotate the whole Aura effect around its local Y axis.",
                    9200,
                    new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
                    dispName: "Y Rotation"));

            AuraZRotation = config.Bind(
                auraSection,
                "Z Rotation",
                DefaultEffectRotation,
                OrderedDescription(
                    "Rotate the whole Aura effect around its local Z axis.",
                    9100,
                    new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
                    dispName: "Z Rotation"));

            AuraBottomSpacer = config.Bind(
                auraSection,
                "__Aura Bottom Spacer",
                false,
                OrderedDescription(
                    "",
                    -999,
                    customDrawer: ConfigurationManagerDrawers.DrawSectionSpacer,
                    hideSettingName: true,
                    hideDefaultButton: true));

            OrbitalsOrbs = config.Bind(
                orbitalsOrbsSection,
                "Enabled",
                true,
                OrderedDescription(
                    "Turn Orbs on or off.",
                    10000,
                    dispName: "Enabled",
                    customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
                    hideSettingName: true));

            OrbitalsOrbsSnake = config.Bind(
                orbitalsOrbsSection,
                "Snake",
                false,
                OrderedDescription(
                    "Turn Snake Mode on or off for Orbs.",
                    9900,
                    dispName: "Snake",
                    customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
                    hideSettingName: true));

            OrbitalsOrbsGlue = config.Bind(
                orbitalsOrbsSection,
                "Glue",
                false,
                OrderedDescription(
                    "Keep Flames and Embers locked to Orbs while Orbs are enabled.",
                    9800,
                    dispName: "Glue",
                    customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
                    hideSettingName: true));
            
            OrbitalsOrbsGlue.SettingChanged += (_, __) =>
            {
                if (OrbitalsOrbsGlue.Value && OrbitalsCoresGlue != null)
                    OrbitalsCoresGlue.Value = false;

                config.Save();
            };

            OrbitalsOrbsSync = config.Bind(
                orbitalsOrbsSection,
                "Sync",
                false,
                OrderedDescription(
                    "Align Cores, Flames, and Embers with Orbs.",
                    9700,
                    dispName: "Sync",
                    customDrawer: ConfigurationManagerDrawers.DrawOrbitalsOrbsSyncButton,
                    hideSettingName: true,
                    hideDefaultButton: true));

            OrbitalsOrbsCount = config.Bind(
                orbitalsOrbsSection,
                "Count",
                DefaultCountNormalized,
                OrderedDescription(
                    "Adjust how many orbs are active.",
                    9600,
                    new AcceptableValueRange<float>(MinCountNormalized, MaxCountNormalized),
                    dispName: "Count",
                    showRangeAsPercent: true));

            OrbitalsOrbsScale = config.Bind(
                orbitalsOrbsSection,
                "Scale",
                defaultScaleMult,
                OrderedDescription(
                    "Adjust the size of the orbs.",
                    9500,
                    new AcceptableValueRange<float>(MinOrbScaleMult, MaxOrbScaleMult),
                    dispName: "Scale"));

            OrbitalsOrbsLuminance = config.Bind(
                orbitalsOrbsSection,
                "Luminance",
                DefaultLuminance,
                OrderedDescription(
                    "Adjust how strongly the orbs glow.",
                    9400,
                    new AcceptableValueRange<float>(MinLuminance, MaxLuminance),
                    dispName: "Luminance"));

            OrbitalsOrbsHue = config.Bind(
                orbitalsOrbsSection,
                "Color",
                DefaultHue,
                OrderedDescription(
                    "Adjust the color of the orbs.",
                    9300,
                    new AcceptableValueRange<float>(MinHue, MaxHue),
                    dispName: "Color"));

            OrbitalsOrbsLength = config.Bind(
                orbitalsOrbsSection,
                "Orbit Length",
                DefaultOrbitalsLength,
                OrderedDescription(
                    "Adjust how far the orbs travel along the weapon before turning around.",
                    9200,
                    new AcceptableValueRange<float>(MinOrbitalsLength, MaxOrbitalsLength),
                    dispName: "Length"));

            OrbitalsOrbsSpeed = config.Bind(
                orbitalsOrbsSection,
                "Speed",
                DefaultOrbitalsSpeed,
                OrderedDescription(
                    "Adjust how quickly the orbs travel through their orbit.",
                    9100,
                    new AcceptableValueRange<float>(MinOrbitalsSpeed, MaxOrbitalsSpeed),
                    dispName: "Speed"));

            OrbitalsOrbsSpacing = config.Bind(
                orbitalsOrbsSection,
                "Spacing",
                DefaultOrbitalsSpacing,
                OrderedDescription(
                    "Adjust how closely the orbs follow each other.",
                    9000,
                    new AcceptableValueRange<float>(MinOrbitalsSpacing, MaxOrbitalsSpacing),
                    dispName: "Spacing",
                    showRangeAsPercent: true,
                    customDrawer: ConfigurationManagerDrawers.DrawOrbsSpacingSlider));

            OrbitalsOrbsRadius = config.Bind(
                orbitalsOrbsSection,
                "Radius",
                DefaultOrbitalsRadiusMultiplier,
                OrderedDescription(
                    "Adjust how wide the orbs wrap around the weapon.",
                    8900,
                    new AcceptableValueRange<float>(MinOrbitalsRadiusMultiplier, MaxOrbitalsRadiusMultiplier),
                    dispName: "Radius"));

            OrbitalsOrbsCycles = config.Bind(
                orbitalsOrbsSection,
                "Cycles",
                DefaultOrbitalsCycles,
                OrderedDescription(
                    "Adjust how many turns the orbs make before reversing direction.",
                    8800,
                    new AcceptableValueRange<float>(MinOrbitalsCycles, MaxOrbitalsCycles),
                    dispName: "Cycles"));

            OrbitalsOrbsXOffset = config.Bind(
                orbitalsOrbsSection,
                "X Offset",
                DefaultEffectOffset,
                OrderedDescription(
                    "Adjust the X axis offset of the whole Orbitals Orbs effect.",
                    8700,
                    new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
                    dispName: "X Offset"));

            OrbitalsOrbsYOffset = config.Bind(
                orbitalsOrbsSection,
                "Y Offset",
                DefaultEffectOffset,
                OrderedDescription(
                    "Adjust the Y axis offset of the whole Orbitals Orbs effect.",
                    8600,
                    new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
                    dispName: "Y Offset"));

            OrbitalsOrbsZOffset = config.Bind(
                orbitalsOrbsSection,
                "Z Offset",
                DefaultEffectOffset,
                OrderedDescription(
                    "Adjust the Z axis offset of the whole Orbitals Orbs effect.",
                    8500,
                    new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
                    dispName: "Z Offset"));

            OrbitalsOrbsXRotation = config.Bind(
                orbitalsOrbsSection,
                "X Rotation",
                DefaultEffectRotation,
                OrderedDescription(
                    "Rotate the whole Orbitals Orbs effect around its local X axis.",
                    8400,
                    new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
                    dispName: "X Rotation"));

            OrbitalsOrbsYRotation = config.Bind(
                orbitalsOrbsSection,
                "Y Rotation",
                DefaultEffectRotation,
                OrderedDescription(
                    "Rotate the whole Orbitals Orbs effect around its local Y axis.",
                    8300,
                    new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
                    dispName: "Y Rotation"));

            OrbitalsOrbsZRotation = config.Bind(
                orbitalsOrbsSection,
                "Z Rotation",
                DefaultEffectRotation,
                OrderedDescription(
                    "Rotate the whole Orbitals Orbs effect around its local Z axis.",
                    8200,
                    new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
                    dispName: "Z Rotation"));

            OrbitalsOrbsDrift = config.Bind(
                orbitalsOrbsSection,
                "Drift",
                DefaultDrift,
                OrderedDescription(
                    "Adjust how much the orbs drift away from their locked orbit path.",
                    8100,
                    new AcceptableValueRange<float>(MinDrift, MaxDrift),
                    dispName: "Drift",
                    showRangeAsPercent: true));

            OrbitalsOrbsBottomSpacer = config.Bind(
                orbitalsOrbsSection,
                "__Orbitals Orbs Bottom Spacer",
                false,
                OrderedDescription(
                    "",
                    -999,
                    customDrawer: ConfigurationManagerDrawers.DrawSectionSpacer,
                    hideSettingName: true,
                    hideDefaultButton: true));

            OrbitalsCores = config.Bind(
                orbitalsCoresSection,
                "Enabled",
                true,
                OrderedDescription(
                    "Turn Cores on or off.",
                    8000,
                    dispName: "Enabled",
                    customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
                    hideSettingName: true));
            
            OrbitalsCoresSnake = config.Bind(
                orbitalsCoresSection,
                "Snake",
                false,
                OrderedDescription(
                    "Turn Snake Mode on or off for Cores.",
                    7900,
                    dispName: "Snake",
                    customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
                    hideSettingName: true));

            OrbitalsCoresGlue = config.Bind(
                orbitalsCoresSection,
                "Glue",
                false,
                OrderedDescription(
                    "Keep Orbs, Flames and Embers locked to Cores while Cores are enabled.",
                    7800,
                    dispName: "Glue",
                    customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
                    hideSettingName: true));

            OrbitalsCoresGlue.SettingChanged += (_, __) =>
            {
                if (OrbitalsCoresGlue.Value && OrbitalsOrbsGlue != null)
                    OrbitalsOrbsGlue.Value = false;

                config.Save();
            };
            
            OrbitalsCoresSpin = config.Bind(
                orbitalsCoresSection,
                "Spin",
                true,
                OrderedDescription(
                    "Spin Cores while they move through the air.",
                    7700,
                    dispName: "Spin",
                    customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
                    hideSettingName: true));

            OrbitalsCoresSync = config.Bind(
                orbitalsCoresSection,
                "Sync",
                false,
                OrderedDescription(
                    "Align Orbs, Flames, and Embers with Cores.",
                    7600,
                    dispName: "Sync",
                    customDrawer: ConfigurationManagerDrawers.DrawOrbitalsCoresSyncButton,
                    hideSettingName: true,
                    hideDefaultButton: true));

            OrbitalsCoresCount = config.Bind(
                orbitalsCoresSection,
                "Count",
                DefaultCountNormalized,
                OrderedDescription(
                    "Adjust how many cores are active.",
                    7500,
                    new AcceptableValueRange<float>(MinCountNormalized, MaxCountNormalized),
                    dispName: "Count",
                    showRangeAsPercent: true));

            OrbitalsCoresScale = config.Bind(
                orbitalsCoresSection,
                "Scale",
                defaultScaleMult,
                OrderedDescription(
                    "Adjust the size of the cores.",
                    7400,
                    new AcceptableValueRange<float>(MinCoreScaleMult, MaxCoreScaleMult),
                    dispName: "Scale"));

            OrbitalsCoresLuminance = config.Bind(
                orbitalsCoresSection,
                "Luminance",
                DefaultLuminance,
                OrderedDescription(
                    "Adjust how strongly the cores glow.",
                    7300,
                    new AcceptableValueRange<float>(MinLuminance, MaxLuminance),
                    dispName: "Luminance"));

            OrbitalsCoresHue = config.Bind(
                orbitalsCoresSection,
                "Color",
                DefaultHue,
                OrderedDescription(
                    "Adjust the color of the cores.",
                    7200,
                    new AcceptableValueRange<float>(MinHue, MaxHue),
                    dispName: "Color"));

            OrbitalsCoresSpinSpeed = config.Bind(
                orbitalsCoresSection,
                "Spin Speed",
                DefaultCoreSpinSpeed,
                OrderedDescription(
                    "Adjust how quickly the cores spin.",
                    7100,
                    new AcceptableValueRange<float>(MinCoreSpinSpeed, MaxCoreSpinSpeed),
                    dispName: "Spin Speed",
                    customDrawer: ConfigurationManagerDrawers.DrawOrbitalsCoresSpinSpeedSlider));

            OrbitalsCoresLength = config.Bind(
                orbitalsCoresSection,
                "Orbit Length",
                DefaultOrbitalsLength,
                OrderedDescription(
                    "Adjust how far the cores travel along the weapon before turning around.",
                    7000,
                    new AcceptableValueRange<float>(MinOrbitalsLength, MaxOrbitalsLength),
                    dispName: "Length"));

            OrbitalsCoresSpeed = config.Bind(
                orbitalsCoresSection,
                "Speed",
                DefaultOrbitalsSpeed,
                OrderedDescription(
                    "Adjust how quickly the cores travel through their orbit.",
                    6900,
                    new AcceptableValueRange<float>(MinOrbitalsSpeed, MaxOrbitalsSpeed),
                    dispName: "Speed"));

            OrbitalsCoresSpacing = config.Bind(
                orbitalsCoresSection,
                "Spacing",
                DefaultOrbitalsSpacing,
                OrderedDescription(
                    "Adjust how closely the cores follow each other.",
                    6800,
                    new AcceptableValueRange<float>(MinOrbitalsSpacing, MaxOrbitalsSpacing),
                    dispName: "Spacing",
                    showRangeAsPercent: true));

            OrbitalsCoresRadius = config.Bind(
                orbitalsCoresSection,
                "Radius",
                DefaultOrbitalsRadiusMultiplier,
                OrderedDescription(
                    "Adjust how wide the cores wrap around the weapon.",
                    6700,
                    new AcceptableValueRange<float>(MinOrbitalsRadiusMultiplier, MaxOrbitalsRadiusMultiplier),
                    dispName: "Radius"));

            OrbitalsCoresCycles = config.Bind(
                orbitalsCoresSection,
                "Cycles",
                DefaultOrbitalsCycles,
                OrderedDescription(
                    "Adjust how many turns the cores make before reversing direction.",
                    6600,
                    new AcceptableValueRange<float>(MinOrbitalsCycles, MaxOrbitalsCycles),
                    dispName: "Cycles"));

            OrbitalsCoresXOffset = config.Bind(
                orbitalsCoresSection,
                "X Offset",
                DefaultEffectOffset,
                OrderedDescription(
                    "Adjust the X axis offset of the whole Orbitals Cores effect.",
                    6500,
                    new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
                    dispName: "X Offset"));

            OrbitalsCoresYOffset = config.Bind(
                orbitalsCoresSection,
                "Y Offset",
                DefaultEffectOffset,
                OrderedDescription(
                    "Adjust the Y axis offset of the whole Orbitals Cores effect.",
                    6400,
                    new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
                    dispName: "Y Offset"));

            OrbitalsCoresZOffset = config.Bind(
                orbitalsCoresSection,
                "Z Offset",
                DefaultEffectOffset,
                OrderedDescription(
                    "Adjust the Z axis offset of the whole Orbitals Cores effect.",
                    6300,
                    new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
                    dispName: "Z Offset"));

            OrbitalsCoresXRotation = config.Bind(
                orbitalsCoresSection,
                "X Rotation",
                DefaultEffectRotation,
                OrderedDescription(
                    "Rotate the whole Orbitals Cores effect around its local X axis.",
                    6200,
                    new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
                    dispName: "X Rotation"));

            OrbitalsCoresYRotation = config.Bind(
                orbitalsCoresSection,
                "Y Rotation",
                DefaultEffectRotation,
                OrderedDescription(
                    "Rotate the whole Orbitals Cores effect around its local Y axis.",
                    6100,
                    new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
                    dispName: "Y Rotation"));

            OrbitalsCoresZRotation = config.Bind(
                orbitalsCoresSection,
                "Z Rotation",
                DefaultEffectRotation,
                OrderedDescription(
                    "Rotate the whole Orbitals Cores effect around its local Z axis.",
                    6000,
                    new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
                    dispName: "Z Rotation"));

            OrbitalsCoresDrift = config.Bind(
                orbitalsCoresSection,
                "Drift",
                DefaultDrift,
                OrderedDescription(
                    "Adjust how much the cores drift away from their locked orbit path.",
                    5900,
                    new AcceptableValueRange<float>(MinDrift, MaxDrift),
                    dispName: "Drift",
                    showRangeAsPercent: true));

            OrbitalsCoresBottomSpacer = config.Bind(
                orbitalsCoresSection,
                "__Orbitals Cores Bottom Spacer",
                false,
                OrderedDescription(
                    "",
                    -999,
                    customDrawer: ConfigurationManagerDrawers.DrawSectionSpacer,
                    hideSettingName: true,
                    hideDefaultButton: true));

            OrbitalsFlames = config.Bind(
                orbitalsFlamesSection,
                "Enabled",
                true,
                OrderedDescription(
                    "Turn Flames on or off.",
                    10000,
                    dispName: "Enabled",
                    customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
                    hideSettingName: true));

            OrbitalsFlamesCount = config.Bind(
                orbitalsFlamesSection,
                "Count",
                DefaultCountNormalized,
                OrderedDescription(
                    "Adjust how many flames are active.",
                    9900,
                    new AcceptableValueRange<float>(MinCountNormalized, MaxCountNormalized),
                    dispName: "Count",
                    showRangeAsPercent: true));

            OrbitalsFlamesEnergy = config.Bind(
                orbitalsFlamesSection,
                "Energy",
                DefaultEnergy,
                OrderedDescription(
                    "Adjust how intense the flames feel.",
                    9800,
                    new AcceptableValueRange<float>(MinEnergy, MaxEnergy),
                    dispName: "Energy"));

            OrbitalsFlamesScale = config.Bind(
                orbitalsFlamesSection,
                "Scale",
                defaultScaleMult,
                OrderedDescription(
                    "Adjust the size of the flames.",
                    9700,
                    new AcceptableValueRange<float>(MinScaleMult, MaxScaleMult),
                    dispName: "Scale"));

            OrbitalsFlamesLuminance = config.Bind(
                orbitalsFlamesSection,
                "Luminance",
                DefaultLuminance,
                OrderedDescription(
                    "Adjust how strongly the flames glow.",
                    9600,
                    new AcceptableValueRange<float>(MinLuminance, MaxLuminance),
                    dispName: "Luminance"));

            OrbitalsFlamesHue = config.Bind(
                orbitalsFlamesSection,
                "Color",
                DefaultHue,
                OrderedDescription(
                    "Adjust the color of the flames.",
                    9500,
                    new AcceptableValueRange<float>(MinHue, MaxHue),
                    dispName: "Color"));

            OrbitalsFlamesLifetime = config.Bind(
                orbitalsFlamesSection,
                "Lifetime",
                DefaultLifetime,
                OrderedDescription(
                    "Adjust how long emitted flames remain visible.",
                    9400,
                    new AcceptableValueRange<float>(MinLifetime, MaxLifetime),
                    dispName: "Lifetime"));

            OrbitalsFlamesLength = config.Bind(
                orbitalsFlamesSection,
                "Orbit Length",
                DefaultOrbitalsLength,
                OrderedDescription(
                    "Adjust how far the flames travel along the weapon before turning around.",
                    9300,
                    new AcceptableValueRange<float>(MinOrbitalsLength, MaxOrbitalsLength),
                    customDrawer: ConfigurationManagerDrawers.DrawGlueLockedFloatSlider,
                    dispName: "Length"));

            OrbitalsFlamesSpeed = config.Bind(
                orbitalsFlamesSection,
                "Speed",
                DefaultOrbitalsSpeed,
                OrderedDescription(
                    "Adjust how quickly the flames travel through their orbit.",
                    9200,
                    new AcceptableValueRange<float>(MinOrbitalsSpeed, MaxOrbitalsSpeed),
                    customDrawer: ConfigurationManagerDrawers.DrawGlueLockedFloatSlider,
                    dispName: "Speed"));

            OrbitalsFlamesSpacing = config.Bind(
                orbitalsFlamesSection,
                "Spacing",
                DefaultOrbitalsSpacing,
                OrderedDescription(
                    "Adjust how closely the flames follow each other.",
                    9100,
                    new AcceptableValueRange<float>(MinOrbitalsSpacing, MaxOrbitalsSpacing),
                    customDrawer: ConfigurationManagerDrawers.DrawGlueLockedFloatSlider,
                    dispName: "Spacing",
                    showRangeAsPercent: true));

            OrbitalsFlamesRadius = config.Bind(
                orbitalsFlamesSection,
                "Radius",
                DefaultOrbitalsRadiusMultiplier,
                OrderedDescription(
                    "Adjust how wide the flames wrap around the weapon.",
                    9000,
                    new AcceptableValueRange<float>(MinOrbitalsRadiusMultiplier, MaxOrbitalsRadiusMultiplier),
                    customDrawer: ConfigurationManagerDrawers.DrawGlueLockedFloatSlider,
                    dispName: "Radius"));

            OrbitalsFlamesCycles = config.Bind(
                orbitalsFlamesSection,
                "Cycles",
                DefaultOrbitalsCycles,
                OrderedDescription(
                    "Adjust how many turns the flames make before reversing direction.",
                    8900,
                    new AcceptableValueRange<float>(MinOrbitalsCycles, MaxOrbitalsCycles),
                    customDrawer: ConfigurationManagerDrawers.DrawGlueLockedFloatSlider,
                    dispName: "Cycles"));

            OrbitalsFlamesXOffset = config.Bind(
                orbitalsFlamesSection,
                "X Offset",
                DefaultEffectOffset,
                OrderedDescription(
                    "Adjust the X axis offset of the whole Orbitals Flames effect.",
                    8800,
                    new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
                    customDrawer: ConfigurationManagerDrawers.DrawGlueLockedFloatSlider,
                    dispName: "X Offset"));

            OrbitalsFlamesYOffset = config.Bind(
                orbitalsFlamesSection,
                "Y Offset",
                DefaultEffectOffset,
                OrderedDescription(
                    "Adjust the Y axis offset of the whole Orbitals Flames effect.",
                    8700,
                    new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
                    customDrawer: ConfigurationManagerDrawers.DrawGlueLockedFloatSlider,
                    dispName: "Y Offset"));

            OrbitalsFlamesZOffset = config.Bind(
                orbitalsFlamesSection,
                "Z Offset",
                DefaultEffectOffset,
                OrderedDescription(
                    "Adjust the Z axis offset of the whole Orbitals Flames effect.",
                    8600,
                    new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
                    customDrawer: ConfigurationManagerDrawers.DrawGlueLockedFloatSlider,
                    dispName: "Z Offset"));

            OrbitalsFlamesXRotation = config.Bind(
                orbitalsFlamesSection,
                "X Rotation",
                DefaultEffectRotation,
                OrderedDescription(
                    "Rotate the whole Orbitals Flames effect around its local X axis.",
                    8500,
                    new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
                    customDrawer: ConfigurationManagerDrawers.DrawGlueLockedFloatSlider,
                    dispName: "X Rotation"));

            OrbitalsFlamesYRotation = config.Bind(
                orbitalsFlamesSection,
                "Y Rotation",
                DefaultEffectRotation,
                OrderedDescription(
                    "Rotate the whole Orbitals Flames effect around its local Y axis.",
                    8400,
                    new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
                    customDrawer: ConfigurationManagerDrawers.DrawGlueLockedFloatSlider,
                    dispName: "Y Rotation"));

            OrbitalsFlamesZRotation = config.Bind(
                orbitalsFlamesSection,
                "Z Rotation",
                DefaultEffectRotation,
                OrderedDescription(
                    "Rotate the whole Orbitals Flames effect around its local Z axis.",
                    8300,
                    new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
                    customDrawer: ConfigurationManagerDrawers.DrawGlueLockedFloatSlider,
                    dispName: "Z Rotation"));

            OrbitalsFlamesDrift = config.Bind(
                orbitalsFlamesSection,
                "Drift",
                DefaultDrift,
                OrderedDescription(
                    "Adjust how much the flames drift away from their locked orbit path.",
                    8200,
                    new AcceptableValueRange<float>(MinDrift, MaxDrift),
                    customDrawer: ConfigurationManagerDrawers.DrawGlueLockedFloatSlider,
                    dispName: "Drift",
                    showRangeAsPercent: true));

            OrbitalsFlamesBottomSpacer = config.Bind(
                orbitalsFlamesSection,
                "__Orbitals Flames Bottom Spacer",
                false,
                OrderedDescription(
                    "",
                    -999,
                    customDrawer: ConfigurationManagerDrawers.DrawSectionSpacer,
                    hideSettingName: true,
                    hideDefaultButton: true));

            OrbitalsEmbers = config.Bind(
                orbitalsEmbersSection,
                "Enabled",
                true,
                OrderedDescription(
                    "Turn Embers on or off.",
                    10000,
                    dispName: "Enabled",
                    customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
                    hideSettingName: true));

            OrbitalsEmbersCount = config.Bind(
                orbitalsEmbersSection,
                "Count",
                DefaultCountNormalized,
                OrderedDescription(
                    "Adjust how many embers are active.",
                    9900,
                    new AcceptableValueRange<float>(MinCountNormalized, MaxCountNormalized),
                    dispName: "Count",
                    showRangeAsPercent: true));

            OrbitalsEmbersEnergy = config.Bind(
                orbitalsEmbersSection,
                "Energy",
                DefaultEnergy,
                OrderedDescription(
                    "Adjust how intense the embers feel.",
                    9800,
                    new AcceptableValueRange<float>(MinEnergy, MaxEnergy),
                    dispName: "Energy"));

            OrbitalsEmbersScale = config.Bind(
                orbitalsEmbersSection,
                "Scale",
                defaultScaleMult,
                OrderedDescription(
                    "Adjust the size of the embers.",
                    9700,
                    new AcceptableValueRange<float>(MinScaleMult, MaxScaleMult),
                    dispName: "Scale"));

            OrbitalsEmbersLuminance = config.Bind(
                orbitalsEmbersSection,
                "Luminance",
                DefaultLuminance,
                OrderedDescription(
                    "Adjust how strongly the embers glow.",
                    9600,
                    new AcceptableValueRange<float>(MinLuminance, MaxLuminance),
                    dispName: "Luminance"));

            OrbitalsEmbersHue = config.Bind(
                orbitalsEmbersSection,
                "Color",
                DefaultHue,
                OrderedDescription(
                    "Adjust the color of the embers.",
                    9500,
                    new AcceptableValueRange<float>(MinHue, MaxHue),
                    dispName: "Color"));

            OrbitalsEmbersLifetime = config.Bind(
                orbitalsEmbersSection,
                "Lifetime",
                DefaultLifetime,
                OrderedDescription(
                    "Adjust how long emitted embers remain visible.",
                    9400,
                    new AcceptableValueRange<float>(MinLifetime, MaxLifetime),
                    dispName: "Lifetime"));

            OrbitalsEmbersLength = config.Bind(
                orbitalsEmbersSection,
                "Orbit Length",
                DefaultOrbitalsLength,
                OrderedDescription(
                    "Adjust how far the embers travel along the weapon before turning around.",
                    9300,
                    new AcceptableValueRange<float>(MinOrbitalsLength, MaxOrbitalsLength),
                    customDrawer: ConfigurationManagerDrawers.DrawGlueLockedFloatSlider,
                    dispName: "Length"));

            OrbitalsEmbersSpeed = config.Bind(
                orbitalsEmbersSection,
                "Speed",
                DefaultOrbitalsSpeed,
                OrderedDescription(
                    "Adjust how quickly the embers travel through their orbit.",
                    9200,
                    new AcceptableValueRange<float>(MinOrbitalsSpeed, MaxOrbitalsSpeed),
                    customDrawer: ConfigurationManagerDrawers.DrawGlueLockedFloatSlider,
                    dispName: "Speed"));

            OrbitalsEmbersSpacing = config.Bind(
                orbitalsEmbersSection,
                "Spacing",
                DefaultOrbitalsSpacing,
                OrderedDescription(
                    "Adjust how closely the embers follow each other.",
                    9100,
                    new AcceptableValueRange<float>(MinOrbitalsSpacing, MaxOrbitalsSpacing),
                    customDrawer: ConfigurationManagerDrawers.DrawGlueLockedFloatSlider,
                    dispName: "Spacing",
                    showRangeAsPercent: true));

            OrbitalsEmbersRadius = config.Bind(
                orbitalsEmbersSection,
                "Radius",
                DefaultOrbitalsRadiusMultiplier,
                OrderedDescription(
                    "Adjust how wide the embers wrap around the weapon.",
                    9000,
                    new AcceptableValueRange<float>(MinOrbitalsRadiusMultiplier, MaxOrbitalsRadiusMultiplier),
                    customDrawer: ConfigurationManagerDrawers.DrawGlueLockedFloatSlider,
                    dispName: "Radius"));

            OrbitalsEmbersCycles = config.Bind(
                orbitalsEmbersSection,
                "Cycles",
                DefaultOrbitalsCycles,
                OrderedDescription(
                    "Adjust how many turns the embers make before reversing direction.",
                    8900,
                    new AcceptableValueRange<float>(MinOrbitalsCycles, MaxOrbitalsCycles),
                    customDrawer: ConfigurationManagerDrawers.DrawGlueLockedFloatSlider,
                    dispName: "Cycles"));

            OrbitalsEmbersXOffset = config.Bind(
                orbitalsEmbersSection,
                "X Offset",
                DefaultEffectOffset,
                OrderedDescription(
                    "Adjust the X axis offset of the whole Orbitals Embers effect.",
                    8800,
                    new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
                    customDrawer: ConfigurationManagerDrawers.DrawGlueLockedFloatSlider,
                    dispName: "X Offset"));

            OrbitalsEmbersYOffset = config.Bind(
                orbitalsEmbersSection,
                "Y Offset",
                DefaultEffectOffset,
                OrderedDescription(
                    "Adjust the Y axis offset of the whole Orbitals Embers effect.",
                    8700,
                    new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
                    customDrawer: ConfigurationManagerDrawers.DrawGlueLockedFloatSlider,
                    dispName: "Y Offset"));

            OrbitalsEmbersZOffset = config.Bind(
                orbitalsEmbersSection,
                "Z Offset",
                DefaultEffectOffset,
                OrderedDescription(
                    "Adjust the Z axis offset of the whole Orbitals Embers effect.",
                    8600,
                    new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
                    customDrawer: ConfigurationManagerDrawers.DrawGlueLockedFloatSlider,
                    dispName: "Z Offset"));

            OrbitalsEmbersXRotation = config.Bind(
                orbitalsEmbersSection,
                "X Rotation",
                DefaultEffectRotation,
                OrderedDescription(
                    "Rotate the whole Orbitals Embers effect around its local X axis.",
                    8500,
                    new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
                    customDrawer: ConfigurationManagerDrawers.DrawGlueLockedFloatSlider,
                    dispName: "X Rotation"));

            OrbitalsEmbersYRotation = config.Bind(
                orbitalsEmbersSection,
                "Y Rotation",
                DefaultEffectRotation,
                OrderedDescription(
                    "Rotate the whole Orbitals Embers effect around its local Y axis.",
                    8400,
                    new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
                    customDrawer: ConfigurationManagerDrawers.DrawGlueLockedFloatSlider,
                    dispName: "Y Rotation"));

            OrbitalsEmbersZRotation = config.Bind(
                orbitalsEmbersSection,
                "Z Rotation",
                DefaultEffectRotation,
                OrderedDescription(
                    "Rotate the whole Orbitals Embers effect around its local Z axis.",
                    8300,
                    new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
                    customDrawer: ConfigurationManagerDrawers.DrawGlueLockedFloatSlider,
                    dispName: "Z Rotation"));

            OrbitalsEmbersDrift = config.Bind(
                orbitalsEmbersSection,
                "Drift",
                DefaultDrift,
                OrderedDescription(
                    "Adjust how much the embers drift away from their locked orbit path.",
                    8200,
                    new AcceptableValueRange<float>(MinDrift, MaxDrift),
                    customDrawer: ConfigurationManagerDrawers.DrawGlueLockedFloatSlider,
                    dispName: "Drift",
                    showRangeAsPercent: true));

            OrbitalsEmbersBottomSpacer = config.Bind(
                orbitalsEmbersSection,
                "__Orbitals Embers Bottom Spacer",
                false,
                OrderedDescription(
                    "",
                    -999,
                    customDrawer: ConfigurationManagerDrawers.DrawSectionSpacer,
                    hideSettingName: true,
                    hideDefaultButton: true));

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
                });
        }
        
                private static void CopyOrbitalsPositionFromOrbsToCores()
        {
            OrbitalsCoresDrift.Value = OrbitalsOrbsDrift.Value;
            OrbitalsCoresSpeed.Value = OrbitalsOrbsSpeed.Value;
            OrbitalsCoresSpacing.Value = OrbitalsOrbsSpacing.Value;
            OrbitalsCoresLength.Value = OrbitalsOrbsLength.Value;
            OrbitalsCoresRadius.Value = OrbitalsOrbsRadius.Value;
            OrbitalsCoresCycles.Value = OrbitalsOrbsCycles.Value;
            OrbitalsCoresXOffset.Value = OrbitalsOrbsXOffset.Value;
            OrbitalsCoresYOffset.Value = OrbitalsOrbsYOffset.Value;
            OrbitalsCoresZOffset.Value = OrbitalsOrbsZOffset.Value;
            OrbitalsCoresXRotation.Value = OrbitalsOrbsXRotation.Value;
            OrbitalsCoresYRotation.Value = OrbitalsOrbsYRotation.Value;
            OrbitalsCoresZRotation.Value = OrbitalsOrbsZRotation.Value;
        }

        private static void CopyOrbitalsPositionFromOrbsToFlames()
        {
            OrbitalsFlamesDrift.Value = OrbitalsOrbsDrift.Value;
            OrbitalsFlamesSpeed.Value = OrbitalsOrbsSpeed.Value;
            OrbitalsFlamesSpacing.Value = OrbitalsOrbsSpacing.Value;
            OrbitalsFlamesLength.Value = OrbitalsOrbsLength.Value;
            OrbitalsFlamesRadius.Value = OrbitalsOrbsRadius.Value;
            OrbitalsFlamesCycles.Value = OrbitalsOrbsCycles.Value;
            OrbitalsFlamesXOffset.Value = OrbitalsOrbsXOffset.Value;
            OrbitalsFlamesYOffset.Value = OrbitalsOrbsYOffset.Value;
            OrbitalsFlamesZOffset.Value = OrbitalsOrbsZOffset.Value;
            OrbitalsFlamesXRotation.Value = OrbitalsOrbsXRotation.Value;
            OrbitalsFlamesYRotation.Value = OrbitalsOrbsYRotation.Value;
            OrbitalsFlamesZRotation.Value = OrbitalsOrbsZRotation.Value;
        }

        private static void CopyOrbitalsPositionFromOrbsToEmbers()
        {
            OrbitalsEmbersDrift.Value = OrbitalsOrbsDrift.Value;
            OrbitalsEmbersSpeed.Value = OrbitalsOrbsSpeed.Value;
            OrbitalsEmbersSpacing.Value = OrbitalsOrbsSpacing.Value;
            OrbitalsEmbersLength.Value = OrbitalsOrbsLength.Value;
            OrbitalsEmbersRadius.Value = OrbitalsOrbsRadius.Value;
            OrbitalsEmbersCycles.Value = OrbitalsOrbsCycles.Value;
            OrbitalsEmbersXOffset.Value = OrbitalsOrbsXOffset.Value;
            OrbitalsEmbersYOffset.Value = OrbitalsOrbsYOffset.Value;
            OrbitalsEmbersZOffset.Value = OrbitalsOrbsZOffset.Value;
            OrbitalsEmbersXRotation.Value = OrbitalsOrbsXRotation.Value;
            OrbitalsEmbersYRotation.Value = OrbitalsOrbsYRotation.Value;
            OrbitalsEmbersZRotation.Value = OrbitalsOrbsZRotation.Value;
        }
        
        private static void CopyOrbitalsPositionFromCoresToOrbs()
        {
            OrbitalsOrbsDrift.Value = OrbitalsCoresDrift.Value;
            OrbitalsOrbsSpeed.Value = OrbitalsCoresSpeed.Value;
            OrbitalsOrbsSpacing.Value = OrbitalsCoresSpacing.Value;
            OrbitalsOrbsLength.Value = OrbitalsCoresLength.Value;
            OrbitalsOrbsRadius.Value = OrbitalsCoresRadius.Value;
            OrbitalsOrbsCycles.Value = OrbitalsCoresCycles.Value;
            OrbitalsOrbsXOffset.Value = OrbitalsCoresXOffset.Value;
            OrbitalsOrbsYOffset.Value = OrbitalsCoresYOffset.Value;
            OrbitalsOrbsZOffset.Value = OrbitalsCoresZOffset.Value;
            OrbitalsOrbsXRotation.Value = OrbitalsCoresXRotation.Value;
            OrbitalsOrbsYRotation.Value = OrbitalsCoresYRotation.Value;
            OrbitalsOrbsZRotation.Value = OrbitalsCoresZRotation.Value;
        }

        private static void CopyOrbitalsPositionFromCoresToFlames()
        {
            OrbitalsFlamesDrift.Value = OrbitalsCoresDrift.Value;
            OrbitalsFlamesSpeed.Value = OrbitalsCoresSpeed.Value;
            OrbitalsFlamesSpacing.Value = OrbitalsCoresSpacing.Value;
            OrbitalsFlamesLength.Value = OrbitalsCoresLength.Value;
            OrbitalsFlamesRadius.Value = OrbitalsCoresRadius.Value;
            OrbitalsFlamesCycles.Value = OrbitalsCoresCycles.Value;
            OrbitalsFlamesXOffset.Value = OrbitalsCoresXOffset.Value;
            OrbitalsFlamesYOffset.Value = OrbitalsCoresYOffset.Value;
            OrbitalsFlamesZOffset.Value = OrbitalsCoresZOffset.Value;
            OrbitalsFlamesXRotation.Value = OrbitalsCoresXRotation.Value;
            OrbitalsFlamesYRotation.Value = OrbitalsCoresYRotation.Value;
            OrbitalsFlamesZRotation.Value = OrbitalsCoresZRotation.Value;
        }

        private static void CopyOrbitalsPositionFromCoresToEmbers()
        {
            OrbitalsEmbersDrift.Value = OrbitalsCoresDrift.Value;
            OrbitalsEmbersSpeed.Value = OrbitalsCoresSpeed.Value;
            OrbitalsEmbersSpacing.Value = OrbitalsCoresSpacing.Value;
            OrbitalsEmbersLength.Value = OrbitalsCoresLength.Value;
            OrbitalsEmbersRadius.Value = OrbitalsCoresRadius.Value;
            OrbitalsEmbersCycles.Value = OrbitalsCoresCycles.Value;
            OrbitalsEmbersXOffset.Value = OrbitalsCoresXOffset.Value;
            OrbitalsEmbersYOffset.Value = OrbitalsCoresYOffset.Value;
            OrbitalsEmbersZOffset.Value = OrbitalsCoresZOffset.Value;
            OrbitalsEmbersXRotation.Value = OrbitalsCoresXRotation.Value;
            OrbitalsEmbersYRotation.Value = OrbitalsCoresYRotation.Value;
            OrbitalsEmbersZRotation.Value = OrbitalsCoresZRotation.Value;
        }

        internal static void SyncOrbitalsToOrbs()
        {
            if (OrbitalsOrbsCount == null)
                return;

            CopyOrbitalsPositionFromOrbsToCores();
            CopyOrbitalsPositionFromOrbsToFlames();
            CopyOrbitalsPositionFromOrbsToEmbers();

            Plugin.Instance?.ResetOrbitalsStartPoints();
            Plugin.Instance?.RefreshExistingUnboundEquippedRigsOnly();
        }
        
        internal static void SyncOrbitalsToCores()
        {
            if (OrbitalsCoresCount == null)
                return;

            CopyOrbitalsPositionFromCoresToOrbs();
            CopyOrbitalsPositionFromCoresToFlames();
            CopyOrbitalsPositionFromCoresToEmbers();

            Plugin.Instance?.ResetOrbitalsStartPoints();
            Plugin.Instance?.RefreshExistingUnboundEquippedRigsOnly();
        }
    }
}
