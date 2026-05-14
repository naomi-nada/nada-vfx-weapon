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
        internal static ConfigEntry<float> RigYRotation;
        internal static ConfigEntry<float> RigLengthPosition;
        internal static ConfigEntry<float> RigYPosition;

        internal static ConfigEntry<bool> ControlsBottomSpacer;
        internal static ConfigEntry<bool> VisibilityBottomSpacer;
        internal static ConfigEntry<bool> StylesBottomSpacer;
        internal static ConfigEntry<bool> PositionBottomSpacer;
        internal static ConfigEntry<bool> InnerFlamesBottomSpacer;
        internal static ConfigEntry<bool> OuterFlamesBottomSpacer;
        internal static ConfigEntry<bool> SparksBottomSpacer;
        internal static ConfigEntry<bool> FlareBottomSpacer;
        internal static ConfigEntry<bool> AuraBottomSpacer;
        internal static ConfigEntry<bool> OrbitalsOrbsBottomSpacer;
        internal static ConfigEntry<bool> OrbitalsStrandsBottomSpacer;
        internal static ConfigEntry<bool> OrbitalsFlamesBottomSpacer;

        internal static ConfigEntry<bool> InnerFlames = null;
        internal static ConfigEntry<float> InnerFlamesEnergy = null;
        internal static ConfigEntry<float> InnerFlamesScale = null;
        internal static ConfigEntry<float> InnerFlamesLength = null;
        internal static ConfigEntry<float> InnerFlamesHue = null;
        internal static ConfigEntry<float> InnerFlamesXOffset = null;
        internal static ConfigEntry<float> InnerFlamesYOffset = null;
        internal static ConfigEntry<float> InnerFlamesZOffset = null;
        internal static ConfigEntry<float> InnerFlamesXRotation = null;
        internal static ConfigEntry<float> InnerFlamesYRotation = null;
        internal static ConfigEntry<float> InnerFlamesZRotation = null;

        internal static ConfigEntry<bool> OuterFlames = null;
        internal static ConfigEntry<bool> OuterFlamesDragEnabled = null;
        internal static ConfigEntry<float> OuterFlamesEnergy = null;
        internal static ConfigEntry<float> OuterFlamesScale = null;
        internal static ConfigEntry<float> OuterFlamesLength = null;
        internal static ConfigEntry<float> OuterFlamesHue = null;
        internal static ConfigEntry<float> OuterFlamesXOffset = null;
        internal static ConfigEntry<float> OuterFlamesYOffset = null;
        internal static ConfigEntry<float> OuterFlamesZOffset = null;
        internal static ConfigEntry<float> OuterFlamesXRotation = null;
        internal static ConfigEntry<float> OuterFlamesYRotation = null;
        internal static ConfigEntry<float> OuterFlamesZRotation = null;

        internal static ConfigEntry<bool> Sparks = null;
        internal static ConfigEntry<float> SparksEnergy = null;
        internal static ConfigEntry<float> SparksScale = null;
        internal static ConfigEntry<float> SparksLength = null;
        internal static ConfigEntry<float> SparksWidth = null;
        internal static ConfigEntry<float> SparksHue = null;
        internal static ConfigEntry<float> SparksXOffset = null;
        internal static ConfigEntry<float> SparksYOffset = null;
        internal static ConfigEntry<float> SparksZOffset = null;
        internal static ConfigEntry<float> SparksXRotation = null;
        internal static ConfigEntry<float> SparksYRotation = null;
        internal static ConfigEntry<float> SparksZRotation = null;

        internal static ConfigEntry<bool> Flare = null;
        internal static ConfigEntry<float> FlareScale = null;
        internal static ConfigEntry<float> FlareHue = null;
        internal static ConfigEntry<float> FlareXOffset = null;
        internal static ConfigEntry<float> FlareYOffset = null;
        internal static ConfigEntry<float> FlareZOffset = null;
        
        internal static ConfigEntry<bool> Aura = null;
        internal static ConfigEntry<float> AuraScale = null;
        internal static ConfigEntry<float> AuraHue = null;
        internal static ConfigEntry<float> AuraXOffset = null;
        internal static ConfigEntry<float> AuraYOffset = null;
        internal static ConfigEntry<float> AuraZOffset = null;
        internal static ConfigEntry<float> AuraXRotation = null;
        internal static ConfigEntry<float> AuraYRotation = null;
        internal static ConfigEntry<float> AuraZRotation = null;
        
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
        internal static ConfigEntry<float> OrbitalsOrbsXOffset = null;
        internal static ConfigEntry<float> OrbitalsOrbsYOffset = null;
        internal static ConfigEntry<float> OrbitalsOrbsZOffset = null;
        internal static ConfigEntry<float> OrbitalsOrbsXRotation = null;
        internal static ConfigEntry<float> OrbitalsOrbsYRotation = null;
        internal static ConfigEntry<float> OrbitalsOrbsZRotation = null;

        internal static ConfigEntry<bool> OrbitalsStrands = null;
        internal static ConfigEntry<bool> OrbitalsStrandsSpectrum = null;
        internal static ConfigEntry<float> OrbitalsStrandsEnergy = null;
        internal static ConfigEntry<float> OrbitalsStrandsDrift = null;
        internal static ConfigEntry<float> OrbitalsStrandsScaleWhole = null;
        internal static ConfigEntry<float> OrbitalsStrandsScaleParts = null;
        internal static ConfigEntry<float> OrbitalsStrandsHue = null;
        internal static ConfigEntry<float> OrbitalsStrandsSpectrumSpeed = null;
        internal static ConfigEntry<float> OrbitalsStrandsSpeed = null;
        internal static ConfigEntry<float> OrbitalsStrandsLength = null;
        internal static ConfigEntry<float> OrbitalsStrandsRadius = null;
        internal static ConfigEntry<float> OrbitalsStrandsLifetime = null;
        internal static ConfigEntry<float> OrbitalsStrandsXOffset = null;
        internal static ConfigEntry<float> OrbitalsStrandsYOffset = null;
        internal static ConfigEntry<float> OrbitalsStrandsZOffset = null;
        internal static ConfigEntry<float> OrbitalsStrandsXRotation = null;
        internal static ConfigEntry<float> OrbitalsStrandsYRotation = null;
        internal static ConfigEntry<float> OrbitalsStrandsZRotation = null;

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
        internal static ConfigEntry<float> OrbitalsFlamesXOffset = null;
        internal static ConfigEntry<float> OrbitalsFlamesYOffset = null;
        internal static ConfigEntry<float> OrbitalsFlamesZOffset = null;
        internal static ConfigEntry<float> OrbitalsFlamesXRotation = null;
        internal static ConfigEntry<float> OrbitalsFlamesYRotation = null;
        internal static ConfigEntry<float> OrbitalsFlamesZRotation = null;

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
        internal static ConfigEntry<float> OrbitalsEmbersLifetime = null;
        internal static ConfigEntry<float> OrbitalsEmbersXOffset = null;
        internal static ConfigEntry<float> OrbitalsEmbersYOffset = null;
        internal static ConfigEntry<float> OrbitalsEmbersZOffset = null;
        internal static ConfigEntry<float> OrbitalsEmbersXRotation = null;
        internal static ConfigEntry<float> OrbitalsEmbersYRotation = null;
        internal static ConfigEntry<float> OrbitalsEmbersZRotation = null;

        internal const float MinRigRotation = -180f;
        internal const float MaxRigRotation = 180f;
        internal const float DefaultRigRotation = 0f;

        internal const float MinRigYRotation = -180f;
        internal const float MaxRigYRotation = 180f;
        internal const float DefaultRigYRotation = 0f;

        internal const float MinRigLengthPosition = -1.00f;
        internal const float MaxRigLengthPosition = 1.00f;
        internal const float DefaultRigLengthPosition = 0f;

        internal const float MinRigYPosition = -1.00f;
        internal const float MaxRigYPosition = 1.00f;
        internal const float DefaultRigYPosition = 0f;
        
        internal const float MinEffectOffset = -2.0f;
        internal const float MaxEffectOffset = 2.0f;
        internal const float DefaultEffectOffset = 0f;
        
        internal const float MinEffectRotation = -180f;
        internal const float MaxEffectRotation = 180f;
        internal const float DefaultEffectRotation = 0f;

        internal const float MaxScaleMult = 1.75f;
        internal const float MinScaleMult = 0.25f;

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

        internal const float MinSparksWidth = 0.25f;
        internal const float MaxSparksWidth = 1.75f;
        internal const float DefaultSparksWidth = 1.00f;
        
        internal const float MinAuraScale = 1.10f;
        internal const float MaxAuraScale = 2.10f;
        internal const float DefaultAuraScale = 1.6f;
        
        internal const float MinLifetime = 0.25f;
        internal const float MaxLifetime = 1.75f;
        internal const float DefaultLifetime = 1.00f;

        internal const float MinDrift = 0.00f;
        internal const float MaxDrift = 1.00f;
        internal const float DefaultDrift = 0.00f;

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
        
        internal const float MinOrbitalsLength = 0.25f;
        internal const float MaxOrbitalsLength = 1.75f;
        internal const float DefaultOrbitalsLength = 1.00f;

        internal const float MinOrbitalsCycles = 1.00f;
        internal const float MaxOrbitalsCycles = 5.00f;
        internal const float DefaultOrbitalsCycles = 3.00f;

        internal static void Bind(ConfigFile config)
{
    const string hotkeysSection = "CONTROLS";
    const string visibilitySection = "VISIBILITY";
    const string stylesSection = "STYLES";
    const string positionSection = "POSITIONING";
    const string innerFlamesSection = "INNER FLAMES";
    const string outerFlamesSection = "OUTER FLAMES";
    const string sparksSection = "SPARKS";
    const string flareSection = "FLARE";
    const string auraSection = "AURA";
    const string orbitalsOrbsSection = "Orbitals: ORBS";
    const string orbitalsStrandsSection = "Orbitals: STRANDS";
    const string orbitalsFlamesSection = "Orbitals: FLAMES";
    const string orbitalsEmbersSection = "Orbitals: EMBERS";

    Plugin.DebugLoggingEnabled = config.Bind(
        "Debug",
        "Enable Debug Logging",
        true,
        OrderedDescription(
            "Enables verbose NADA VFX structure/discovery logs.",
            500,
            isAdvanced: true)
    );

    Plugin.EquipLoggingEnabled = config.Bind(
        "Debug",
        "Enable Equip Logging",
        true,
        OrderedDescription(
            "Logs one concise NADA VFX line when an equipped item is processed.",
            450,
            isAdvanced: true)
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
            hideSettingName: true)
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
            hideSettingName: true)
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
        OrderedDescription(
            "",
            -999,
            customDrawer: ConfigurationManagerDrawers.DrawSectionSpacer,
            hideSettingName: true,
            hideDefaultButton: true)
    );

    AttachHotkey = config.Bind(
        hotkeysSection,
        "Attach to Weapon Hotkey",
        KeyboardShortcut.Empty,
        OrderedDescription(
            "Press to attach NADA VFX to the equipped weapon.",
            300,
            dispName: "Attach to Weapon Hotkey")
    );

    AttachHotkey.SettingChanged += (_, __) => config.Save();

    BindHotkey = config.Bind(
        hotkeysSection,
        "Bind to Weapon Hotkey",
        KeyboardShortcut.Empty,
        OrderedDescription(
            "Press to bind the current NADA VFX rig/settings to the equipped weapon.",
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
            hideDefaultButton: true)
    );

    ControlsBottomSpacer = config.Bind(
        hotkeysSection,
        "__Controls Bottom Spacer",
        false,
        OrderedDescription(
            "",
            -999,
            customDrawer: ConfigurationManagerDrawers.DrawSectionSpacer,
            hideSettingName: true,
            hideDefaultButton: true)
    );

    StyleName = config.Bind(
        stylesSection,
        "Style Name",
        string.Empty,
        OrderedDescription(
            "Name used when saving the current style.",
            240,
            dispName: "Style Name",
            isAdvanced: true,
            browsable: false)
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
            hideDefaultButton: true)
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
            hideDefaultButton: true)
    );

    StylesBottomSpacer = config.Bind(
        stylesSection,
        "__Styles Bottom Spacer",
        false,
        OrderedDescription(
            "",
            -999,
            customDrawer: ConfigurationManagerDrawers.DrawSectionSpacer,
            hideSettingName: true,
            hideDefaultButton: true)
    );

    RigRotation = config.Bind(
        positionSection,
        "Rotation",
        DefaultRigRotation,
        OrderedDescription(
            "Rotate the whole NADA rig around the weapon.",
            205,
            new AcceptableValueRange<float>(MinRigRotation, MaxRigRotation),
            dispName: "X Tilt")
    );

    RigYRotation = config.Bind(
        positionSection,
        "Y Rotation",
        DefaultRigYRotation,
        OrderedDescription(
            "Rotate the whole NADA rig left or right around the weapon.",
            204,
            new AcceptableValueRange<float>(MinRigYRotation, MaxRigYRotation),
            dispName: "Y Tilt")
    );

    RigLengthPosition = config.Bind(
        positionSection,
        "Length Position",
        DefaultRigLengthPosition,
        OrderedDescription(
            "Slide the whole NADA rig up or down the weapon length.",
            203,
            new AcceptableValueRange<float>(MinRigLengthPosition, MaxRigLengthPosition),
            dispName: "X Position")
    );

    RigYPosition = config.Bind(
        positionSection,
        "Y Position",
        DefaultRigYPosition,
        OrderedDescription(
            "Slide the whole NADA rig left or right across the weapon.",
            202,
            new AcceptableValueRange<float>(MinRigYPosition, MaxRigYPosition),
            dispName: "Y Position")
    );

    RigYPosition.SettingChanged += (_, __) =>
        Plugin.Instance?.RefreshExistingUnboundEquippedRigsOnly();

    RigLengthPosition.SettingChanged += (_, __) =>
        Plugin.Instance?.RefreshExistingUnboundEquippedRigsOnly();

    RigYRotation.SettingChanged += (_, __) =>
        Plugin.Instance?.RefreshExistingUnboundEquippedRigsOnly();

    RigRotation.SettingChanged += (_, __) =>
        Plugin.Instance?.RefreshExistingUnboundEquippedRigsOnly();

    PositionBottomSpacer = config.Bind(
        positionSection,
        "__Position Bottom Spacer",
        false,
        OrderedDescription(
            "",
            -999,
            customDrawer: ConfigurationManagerDrawers.DrawSectionSpacer,
            hideSettingName: true,
            hideDefaultButton: true)
    );

    InnerFlames = config.Bind(
        innerFlamesSection,
        "Enabled",
        true,
        OrderedDescription(
            "Turn Inner Flames on or off.",
            200,
            dispName: "Enabled",
            customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
            hideSettingName: true)
    );

    InnerFlamesEnergy = config.Bind(
        innerFlamesSection,
        "Energy",
        DefaultEnergy,
        OrderedDescription(
            "Adjust how intense Inner Flames feels.",
            199,
            new AcceptableValueRange<float>(MinEnergy, MaxEnergy),
            dispName: "Energy")
    );

    InnerFlamesScale = config.Bind(
        innerFlamesSection,
        "Scale",
        1.00f,
        OrderedDescription(
            "Adjust the size of Inner Flames.",
            198,
            new AcceptableValueRange<float>(MinScaleMult, MaxScaleMult),
            dispName: "Scale")
    );

    InnerFlamesLength = config.Bind(
        innerFlamesSection,
        "Length",
        DefaultFlameLength,
        OrderedDescription(
            "Adjust how much of the blade emits inner flames.",
            197,
            new AcceptableValueRange<float>(MinFlameLength, MaxFlameLength),
            dispName: "Length")
    );

    InnerFlamesHue = config.Bind(
        innerFlamesSection,
        "Color",
        DefaultHue,
        OrderedDescription(
            "Adjust the color of Inner Flames.",
            196,
            new AcceptableValueRange<float>(MinHue, MaxHue),
            dispName: "Color")
    );

    InnerFlamesXOffset = config.Bind(
        innerFlamesSection,
        "X Offset",
        DefaultEffectOffset,
        OrderedDescription(
            "Adjust the X axis offset of the whole Inner Flames effect.",
            195,
            new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
            dispName: "X Offset")
    );

    InnerFlamesYOffset = config.Bind(
        innerFlamesSection,
        "Y Offset",
        DefaultEffectOffset,
        OrderedDescription(
            "Adjust the Y axis offset of the whole Inner Flames effect.",
            194,
            new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
            dispName: "Y Offset")
    );

    InnerFlamesZOffset = config.Bind(
        innerFlamesSection,
        "Z Offset",
        DefaultEffectOffset,
        OrderedDescription(
            "Adjust the Z axis offset of the whole Inner Flames effect.",
            193,
            new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
            dispName: "Z Offset")
    );

    InnerFlamesXRotation = config.Bind(
        innerFlamesSection,
        "X Rotation",
        DefaultEffectRotation,
        OrderedDescription(
            "Rotate the whole Inner Flames effect around its local X axis.",
            192,
            new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
            dispName: "X Rotation")
    );

    InnerFlamesYRotation = config.Bind(
        innerFlamesSection,
        "Y Rotation",
        DefaultEffectRotation,
        OrderedDescription(
            "Rotate the whole Inner Flames effect around its local Y axis.",
            191,
            new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
            dispName: "Y Rotation")
    );

    InnerFlamesZRotation = config.Bind(
        innerFlamesSection,
        "Z Rotation",
        DefaultEffectRotation,
        OrderedDescription(
            "Rotate the whole Inner Flames effect around its local Z axis.",
            190,
            new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
            dispName: "Z Rotation")
    );

    InnerFlamesBottomSpacer = config.Bind(
        innerFlamesSection,
        "__Inner Flames Bottom Spacer",
        false,
        OrderedDescription(
            "",
            -998,
            customDrawer: ConfigurationManagerDrawers.DrawSectionSpacer,
            hideSettingName: true,
            hideDefaultButton: true)
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
            hideSettingName: true)
    );

    OuterFlamesDragEnabled = config.Bind(
        outerFlamesSection,
        "Drag Enabled",
        false,
        OrderedDescription(
            "Enable or disable motion-based drag on Outer Flames.",
            189,
            dispName: "Drag",
            customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
            hideSettingName: true)
    );

    OuterFlamesEnergy = config.Bind(
        outerFlamesSection,
        "Energy",
        DefaultEnergy,
        OrderedDescription(
            "Adjust how intense Outer Flames feels.",
            188,
            new AcceptableValueRange<float>(MinEnergy, MaxEnergy),
            dispName: "Energy")
    );

    OuterFlamesScale = config.Bind(
        outerFlamesSection,
        "Scale",
        1.00f,
        OrderedDescription(
            "Adjust the size of Outer Flames.",
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
            dispName: "Length")
    );

    OuterFlamesHue = config.Bind(
        outerFlamesSection,
        "Color",
        DefaultHue,
        OrderedDescription(
            "Adjust the color of Outer Flames.",
            185,
            new AcceptableValueRange<float>(MinHue, MaxHue),
            dispName: "Color")
    );

    OuterFlamesXOffset = config.Bind(
        outerFlamesSection,
        "X Offset",
        DefaultEffectOffset,
        OrderedDescription(
            "Adjust the X axis offset of the whole Outer Flames effect.",
            184,
            new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
            dispName: "X Offset")
    );

    OuterFlamesYOffset = config.Bind(
        outerFlamesSection,
        "Y Offset",
        DefaultEffectOffset,
        OrderedDescription(
            "Adjust the Y axis offset of the whole Outer Flames effect.",
            183,
            new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
            dispName: "Y Offset")
    );

    OuterFlamesZOffset = config.Bind(
        outerFlamesSection,
        "Z Offset",
        DefaultEffectOffset,
        OrderedDescription(
            "Adjust the Z axis offset of the whole Outer Flames effect.",
            182,
            new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
            dispName: "Z Offset")
    );

    OuterFlamesXRotation = config.Bind(
        outerFlamesSection,
        "X Rotation",
        DefaultEffectRotation,
        OrderedDescription(
            "Rotate the whole Outer Flames effect around its local X axis.",
            181,
            new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
            dispName: "X Rotation")
    );

    OuterFlamesYRotation = config.Bind(
        outerFlamesSection,
        "Y Rotation",
        DefaultEffectRotation,
        OrderedDescription(
            "Rotate the whole Outer Flames effect around its local Y axis.",
            180,
            new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
            dispName: "Y Rotation")
    );

    OuterFlamesZRotation = config.Bind(
        outerFlamesSection,
        "Z Rotation",
        DefaultEffectRotation,
        OrderedDescription(
            "Rotate the whole Outer Flames effect around its local Z axis.",
            179,
            new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
            dispName: "Z Rotation")
    );

    OuterFlamesBottomSpacer = config.Bind(
        outerFlamesSection,
        "__Outer Flames Bottom Spacer",
        false,
        OrderedDescription(
            "",
            -999,
            customDrawer: ConfigurationManagerDrawers.DrawSectionSpacer,
            hideSettingName: true,
            hideDefaultButton: true)
    );

    Sparks = config.Bind(
        sparksSection,
        "Enabled",
        true,
        OrderedDescription(
            "Turn Sparks on or off.",
            180,
            dispName: "Enabled",
            customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
            hideSettingName: true)
    );

    SparksEnergy = config.Bind(
        sparksSection,
        "Energy",
        DefaultEnergy,
        OrderedDescription(
            "Adjust how intense Sparks feels.",
            179,
            new AcceptableValueRange<float>(MinEnergy, MaxEnergy),
            dispName: "Energy")
    );

    SparksScale = config.Bind(
        sparksSection,
        "Scale",
        1.00f,
        OrderedDescription(
            "Adjust the size of Sparks.",
            178,
            new AcceptableValueRange<float>(MinScaleMult, MaxScaleMult),
            dispName: "Scale")
    );

    SparksLength = config.Bind(
        sparksSection,
        "Length",
        DefaultFlameLength,
        OrderedDescription(
            "Adjust how much of the weapon emits Sparks.",
            177,
            new AcceptableValueRange<float>(MinFlameLength, MaxFlameLength),
            dispName: "Length")
    );

    SparksWidth = config.Bind(
        sparksSection,
        "Width",
        DefaultSparksWidth,
        OrderedDescription(
            "Pull Sparks inward or push them wider around the weapon.",
            176,
            new AcceptableValueRange<float>(MinSparksWidth, MaxSparksWidth),
            dispName: "Width")
    );

    SparksHue = config.Bind(
        sparksSection,
        "Color",
        DefaultHue,
        OrderedDescription(
            "Adjust the color of Sparks.",
            175,
            new AcceptableValueRange<float>(MinHue, MaxHue),
            dispName: "Color")
    );

    SparksXOffset = config.Bind(
        sparksSection,
        "X Offset",
        DefaultEffectOffset,
        OrderedDescription(
            "Adjust the X axis offset of the whole Sparks effect.",
            174,
            new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
            dispName: "X Offset")
    );

    SparksYOffset = config.Bind(
        sparksSection,
        "Y Offset",
        DefaultEffectOffset,
        OrderedDescription(
            "Adjust the Y axis offset of the whole Sparks effect.",
            173,
            new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
            dispName: "Y Offset")
    );

    SparksZOffset = config.Bind(
        sparksSection,
        "Z Offset",
        DefaultEffectOffset,
        OrderedDescription(
            "Adjust the Z axis offset of the whole Sparks effect.",
            172,
            new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
            dispName: "Z Offset")
    );

    SparksXRotation = config.Bind(
        sparksSection,
        "X Rotation",
        DefaultEffectRotation,
        OrderedDescription(
            "Rotate the whole Sparks effect around its local X axis.",
            171,
            new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
            dispName: "X Rotation")
    );

    SparksYRotation = config.Bind(
        sparksSection,
        "Y Rotation",
        DefaultEffectRotation,
        OrderedDescription(
            "Rotate the whole Sparks effect around its local Y axis.",
            170,
            new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
            dispName: "Y Rotation")
    );

    SparksZRotation = config.Bind(
        sparksSection,
        "Z Rotation",
        DefaultEffectRotation,
        OrderedDescription(
            "Rotate the whole Sparks effect around its local Z axis.",
            169,
            new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
            dispName: "Z Rotation")
    );

    SparksBottomSpacer = config.Bind(
        sparksSection,
        "__Sparks Bottom Spacer",
        false,
        OrderedDescription(
            "",
            -999,
            customDrawer: ConfigurationManagerDrawers.DrawSectionSpacer,
            hideSettingName: true,
            hideDefaultButton: true)
    );

    Flare = config.Bind(
        flareSection,
        "Enabled",
        true,
        OrderedDescription(
            "Turn Flare on or off.",
            170,
            dispName: "Enabled",
            customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
            hideSettingName: true)
    );

    FlareScale = config.Bind(
        flareSection,
        "Scale",
        1.00f,
        OrderedDescription(
            "Adjust the size of Flare.",
            169,
            new AcceptableValueRange<float>(MinScaleMult, MaxScaleMult),
            dispName: "Scale")
    );

    FlareHue = config.Bind(
        flareSection,
        "Color",
        DefaultHue,
        OrderedDescription(
            "Adjust the color of Flare.",
            168,
            new AcceptableValueRange<float>(MinHue, MaxHue),
            dispName: "Color")
    );

    FlareXOffset = config.Bind(
        flareSection,
        "X Offset",
        DefaultEffectOffset,
        OrderedDescription(
            "Adjust the X axis offset of the whole Flare effect.",
            167,
            new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
            dispName: "X Offset")
    );

    FlareYOffset = config.Bind(
        flareSection,
        "Y Offset",
        DefaultEffectOffset,
        OrderedDescription(
            "Adjust the Y axis offset of the whole Flare effect.",
            166,
            new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
            dispName: "Y Offset")
    );

    FlareZOffset = config.Bind(
        flareSection,
        "Z Offset",
        DefaultEffectOffset,
        OrderedDescription(
            "Adjust the Z axis offset of the whole Flare effect.",
            165,
            new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
            dispName: "Z Offset")
    );

    FlareBottomSpacer = config.Bind(
        flareSection,
        "__Flare Bottom Spacer",
        false,
        OrderedDescription(
            "",
            -999,
            customDrawer: ConfigurationManagerDrawers.DrawSectionSpacer,
            hideSettingName: true,
            hideDefaultButton: true)
    );

    Aura = config.Bind(
        auraSection,
        "Enabled",
        true,
        OrderedDescription(
            "Turn Aura on or off.",
            165,
            dispName: "Enabled",
            customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
            hideSettingName: true)
    );

    AuraScale = config.Bind(
        auraSection,
        "Scale",
        DefaultAuraScale,
        OrderedDescription(
            "Adjust the size of Aura.",
            164,
            new AcceptableValueRange<float>(MinAuraScale, MaxAuraScale),
            dispName: "Scale")
    );

    AuraHue = config.Bind(
        auraSection,
        "Color",
        DefaultHue,
        OrderedDescription(
            "Adjust the color of Aura.",
            163,
            new AcceptableValueRange<float>(MinHue, MaxHue),
            dispName: "Color")
    );

    AuraXOffset = config.Bind(
        auraSection,
        "X Offset",
        DefaultEffectOffset,
        OrderedDescription(
            "Adjust the X axis offset of the whole Aura effect.",
            162,
            new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
            dispName: "X Offset")
    );

    AuraYOffset = config.Bind(
        auraSection,
        "Y Offset",
        DefaultEffectOffset,
        OrderedDescription(
            "Adjust the Y axis offset of the whole Aura effect.",
            161,
            new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
            dispName: "Y Offset")
    );

    AuraZOffset = config.Bind(
        auraSection,
        "Z Offset",
        DefaultEffectOffset,
        OrderedDescription(
            "Adjust the Z axis offset of the whole Aura effect.",
            160,
            new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
            dispName: "Z Offset")
    );

    AuraXRotation = config.Bind(
        auraSection,
        "X Rotation",
        DefaultEffectRotation,
        OrderedDescription(
            "Rotate the whole Aura effect around its local X axis.",
            159,
            new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
            dispName: "X Rotation")
    );

    AuraYRotation = config.Bind(
        auraSection,
        "Y Rotation",
        DefaultEffectRotation,
        OrderedDescription(
            "Rotate the whole Aura effect around its local Y axis.",
            158,
            new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
            dispName: "Y Rotation")
    );

    AuraZRotation = config.Bind(
        auraSection,
        "Z Rotation",
        DefaultEffectRotation,
        OrderedDescription(
            "Rotate the whole Aura effect around its local Z axis.",
            157,
            new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
            dispName: "Z Rotation")
    );

    AuraBottomSpacer = config.Bind(
        auraSection,
        "__Aura Bottom Spacer",
        false,
        OrderedDescription(
            "",
            -999,
            customDrawer: ConfigurationManagerDrawers.DrawSectionSpacer,
            hideSettingName: true,
            hideDefaultButton: true)
    );

    OrbitalsOrbs = config.Bind(
        orbitalsOrbsSection,
        "Enabled",
        true,
        OrderedDescription(
            "Turn Orbs on or off.",
            155,
            dispName: "Enabled",
            customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
            hideSettingName: true)
    );

    OrbitalsOrbsCount = config.Bind(
        orbitalsOrbsSection,
        "Count",
        DefaultCountNormalized,
        OrderedDescription(
            "Adjust how many orbs are active.",
            154,
            new AcceptableValueRange<float>(MinCountNormalized, MaxCountNormalized),
            dispName: "Count",
            showRangeAsPercent: true)
    );

    OrbitalsOrbsDrift = config.Bind(
        orbitalsOrbsSection,
        "Drift",
        DefaultDrift,
        OrderedDescription(
            "Adjust how much the orbs drift away from their locked orbit path.",
            153,
            new AcceptableValueRange<float>(MinDrift, MaxDrift),
            dispName: "Drift",
            showRangeAsPercent: true)
    );

    OrbitalsOrbsScale = config.Bind(
        orbitalsOrbsSection,
        "Scale",
        1f,
        OrderedDescription(
            "Adjust the size of the orbs.",
            152,
            new AcceptableValueRange<float>(MinOrbScaleMult, MaxOrbScaleMult),
            dispName: "Scale")
    );

    OrbitalsOrbsHue = config.Bind(
        orbitalsOrbsSection,
        "Color",
        DefaultHue,
        OrderedDescription(
            "Adjust the color of the orbs.",
            151,
            new AcceptableValueRange<float>(MinHue, MaxHue),
            dispName: "Color")
    );

    OrbitalsOrbsSpeed = config.Bind(
        orbitalsOrbsSection,
        "Speed",
        DefaultOrbitalsSpeed,
        OrderedDescription(
            "Adjust how quickly the orbs travel through their orbit.",
            150,
            new AcceptableValueRange<float>(MinOrbitalsSpeed, MaxOrbitalsSpeed),
            dispName: "Speed")
    );

    OrbitalsOrbsSpacing = config.Bind(
        orbitalsOrbsSection,
        "Spacing",
        DefaultOrbitalsSpacing,
        OrderedDescription(
            "Adjust how closely the orbs follow each other.",
            149,
            new AcceptableValueRange<float>(MinOrbitalsSpacing, MaxOrbitalsSpacing),
            dispName: "Spacing",
            showRangeAsPercent: true)
    );

    OrbitalsOrbsLength = config.Bind(
        orbitalsOrbsSection,
        "Orbit Length",
        DefaultOrbitalsLength,
        OrderedDescription(
            "Adjust how far the orbs travel along the weapon before turning around.",
            148,
            new AcceptableValueRange<float>(MinOrbitalsLength, MaxOrbitalsLength),
            dispName: "Length")
    );

    OrbitalsOrbsRadius = config.Bind(
        orbitalsOrbsSection,
        "Radius",
        DefaultOrbitalsRadiusMultiplier,
        OrderedDescription(
            "Adjust how wide the orbs wrap around the weapon.",
            147,
            new AcceptableValueRange<float>(MinOrbitalsRadiusMultiplier, MaxOrbitalsRadiusMultiplier),
            dispName: "Radius")
    );

    OrbitalsOrbsCycles = config.Bind(
        orbitalsOrbsSection,
        "Cycles",
        DefaultOrbitalsCycles,
        OrderedDescription(
            "Adjust how many turns the orbs make before reversing direction.",
            146,
            new AcceptableValueRange<float>(MinOrbitalsCycles, MaxOrbitalsCycles),
            dispName: "Cycles")
    );

    OrbitalsOrbsXOffset = config.Bind(
        orbitalsOrbsSection,
        "X Offset",
        DefaultEffectOffset,
        OrderedDescription(
            "Adjust the X axis offset of the whole Orbitals Orbs effect.",
            145,
            new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
            dispName: "X Offset")
    );

    OrbitalsOrbsYOffset = config.Bind(
        orbitalsOrbsSection,
        "Y Offset",
        DefaultEffectOffset,
        OrderedDescription(
            "Adjust the Y axis offset of the whole Orbitals Orbs effect.",
            144,
            new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
            dispName: "Y Offset")
    );

    OrbitalsOrbsZOffset = config.Bind(
        orbitalsOrbsSection,
        "Z Offset",
        DefaultEffectOffset,
        OrderedDescription(
            "Adjust the Z axis offset of the whole Orbitals Orbs effect.",
            143,
            new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
            dispName: "Z Offset")
    );

    OrbitalsOrbsXRotation = config.Bind(
        orbitalsOrbsSection,
        "X Rotation",
        DefaultEffectRotation,
        OrderedDescription(
            "Rotate the whole Orbitals Orbs effect around its local X axis.",
            142,
            new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
            dispName: "X Rotation")
    );

    OrbitalsOrbsYRotation = config.Bind(
        orbitalsOrbsSection,
        "Y Rotation",
        DefaultEffectRotation,
        OrderedDescription(
            "Rotate the whole Orbitals Orbs effect around its local Y axis.",
            141,
            new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
            dispName: "Y Rotation")
    );

    OrbitalsOrbsZRotation = config.Bind(
        orbitalsOrbsSection,
        "Z Rotation",
        DefaultEffectRotation,
        OrderedDescription(
            "Rotate the whole Orbitals Orbs effect around its local Z axis.",
            140,
            new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
            dispName: "Z Rotation")
    );

    OrbitalsOrbsBottomSpacer = config.Bind(
        orbitalsOrbsSection,
        "__Orbitals Orbs Bottom Spacer",
        false,
        OrderedDescription(
            "",
            -999,
            customDrawer: ConfigurationManagerDrawers.DrawSectionSpacer,
            hideSettingName: true,
            hideDefaultButton: true)
    );

    OrbitalsStrands = config.Bind(
        orbitalsStrandsSection,
        "Enabled",
        true,
        OrderedDescription(
            "Turn Strands on or off.",
            140,
            dispName: "Enabled",
            customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
            hideSettingName: true)
    );

    OrbitalsStrandsSpectrum = config.Bind(
        orbitalsStrandsSection,
        "Spectrum",
        false,
        OrderedDescription(
            "Continuously cycle Strands through the full color spectrum.",
            139,
            dispName: "Spectrum",
            customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
            hideSettingName: true)
    );

    OrbitalsStrandsEnergy = config.Bind(
        orbitalsStrandsSection,
        "Energy",
        DefaultEnergy,
        OrderedDescription(
            "Adjust how intense the strands feel.",
            138,
            new AcceptableValueRange<float>(MinEnergy, MaxEnergy),
            dispName: "Energy")
    );

    OrbitalsStrandsDrift = config.Bind(
        orbitalsStrandsSection,
        "Drift",
        DefaultDrift,
        OrderedDescription(
            "Adjust how much the strands adhere to their orbit path.",
            137,
            new AcceptableValueRange<float>(MinDrift, MaxDrift),
            dispName: "Drift")
    );

    OrbitalsStrandsScaleWhole = config.Bind(
        orbitalsStrandsSection,
        "Scale Whole",
        1.0f,
        OrderedDescription(
            "Adjust the overall size of the Strands effect.",
            136,
            new AcceptableValueRange<float>(MinScaleMult, MaxScaleMult),
            dispName: "Scale Whole")
    );

    OrbitalsStrandsScaleParts = config.Bind(
        orbitalsStrandsSection,
        "Scale Parts",
        1.00f,
        OrderedDescription(
            "Adjust the size of the individual strand particles.",
            135,
            new AcceptableValueRange<float>(MinScaleMult, MaxScaleMult),
            dispName: "Scale Parts")
    );

    OrbitalsStrandsHue = config.Bind(
        orbitalsStrandsSection,
        "Color",
        DefaultHue,
        OrderedDescription(
            "Adjust the color of the strands.",
            134,
            new AcceptableValueRange<float>(MinHue, MaxHue),
            dispName: "Color",
            customDrawer: ConfigurationManagerDrawers.DrawStrandsColorSlider)
    );

    OrbitalsStrandsSpectrumSpeed = config.Bind(
        orbitalsStrandsSection,
        "Spectrum Speed",
        DefaultSpectrumSpeed,
        OrderedDescription(
            "Adjust how quickly Strands cycle through the color spectrum.",
            133,
            new AcceptableValueRange<float>(MinSpectrumSpeed, MaxSpectrumSpeed),
            dispName: "Spectrum Speed",
            customDrawer: ConfigurationManagerDrawers.DrawStrandsSpectrumSpeedSlider)
    );

    OrbitalsStrandsSpeed = config.Bind(
        orbitalsStrandsSection,
        "Speed",
        DefaultOrbitalsSpeed,
        OrderedDescription(
            "Adjust how quickly the strands animate.",
            132,
            new AcceptableValueRange<float>(MinOrbitalsSpeed, MaxOrbitalsSpeed),
            dispName: "Speed")
    );

    OrbitalsStrandsLength = config.Bind(
        orbitalsStrandsSection,
        "Orbit Length",
        DefaultOrbitalsLength,
        OrderedDescription(
            "Adjust how far the strands stretch along the weapon.",
            131,
            new AcceptableValueRange<float>(MinOrbitalsLength, MaxOrbitalsLength),
            dispName: "Length")
    );

    OrbitalsStrandsRadius = config.Bind(
        orbitalsStrandsSection,
        "Radius",
        DefaultOrbitalsRadiusMultiplier,
        OrderedDescription(
            "Adjust how wide the strands wrap around the weapon.",
            130,
            new AcceptableValueRange<float>(MinOrbitalsRadiusMultiplier, MaxOrbitalsRadiusMultiplier),
            dispName: "Radius")
    );

    OrbitalsStrandsLifetime = config.Bind(
        orbitalsStrandsSection,
        "Lifetime",
        DefaultLifetime,
        OrderedDescription(
            "Adjust the length of time strands exist.",
            129,
            new AcceptableValueRange<float>(MinLifetime, MaxLifetime),
            dispName: "Lifetime")
    );

    OrbitalsStrandsXOffset = config.Bind(
        orbitalsStrandsSection,
        "X Offset",
        DefaultEffectOffset,
        OrderedDescription(
            "Adjust the X axis offset of the whole Orbitals Strands effect.",
            128,
            new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
            dispName: "X Offset")
    );

    OrbitalsStrandsYOffset = config.Bind(
        orbitalsStrandsSection,
        "Y Offset",
        DefaultEffectOffset,
        OrderedDescription(
            "Adjust the Y axis offset of the whole Orbitals Strands effect.",
            127,
            new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
            dispName: "Y Offset")
    );

    OrbitalsStrandsZOffset = config.Bind(
        orbitalsStrandsSection,
        "Z Offset",
        DefaultEffectOffset,
        OrderedDescription(
            "Adjust the Z axis offset of the whole Orbitals Strands effect.",
            126,
            new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
            dispName: "Z Offset")
    );

    OrbitalsStrandsXRotation = config.Bind(
        orbitalsStrandsSection,
        "X Rotation",
        DefaultEffectRotation,
        OrderedDescription(
            "Rotate the whole Orbitals Strands effect around its local X axis.",
            125,
            new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
            dispName: "X Rotation")
    );

    OrbitalsStrandsYRotation = config.Bind(
        orbitalsStrandsSection,
        "Y Rotation",
        DefaultEffectRotation,
        OrderedDescription(
            "Rotate the whole Orbitals Strands effect around its local Y axis.",
            124,
            new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
            dispName: "Y Rotation")
    );

    OrbitalsStrandsZRotation = config.Bind(
        orbitalsStrandsSection,
        "Z Rotation",
        DefaultEffectRotation,
        OrderedDescription(
            "Rotate the whole Orbitals Strands effect around its local Z axis.",
            123,
            new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
            dispName: "Z Rotation")
    );

    OrbitalsStrandsBottomSpacer = config.Bind(
        orbitalsStrandsSection,
        "__Orbitals Strands Bottom Spacer",
        false,
        OrderedDescription(
            "",
            -999,
            customDrawer: ConfigurationManagerDrawers.DrawSectionSpacer,
            hideSettingName: true,
            hideDefaultButton: true)
    );

    OrbitalsFlames = config.Bind(
        orbitalsFlamesSection,
        "Enabled",
        true,
        OrderedDescription(
            "Turn Flames on or off.",
            120,
            dispName: "Enabled",
            customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
            hideSettingName: true)
    );

    OrbitalsFlamesCount = config.Bind(
        orbitalsFlamesSection,
        "Count",
        DefaultCountNormalized,
        OrderedDescription(
            "Adjust how many flames are active.",
            119,
            new AcceptableValueRange<float>(MinCountNormalized, MaxCountNormalized),
            dispName: "Count",
            showRangeAsPercent: true)
    );

    OrbitalsFlamesEnergy = config.Bind(
        orbitalsFlamesSection,
        "Energy",
        DefaultEnergy,
        OrderedDescription(
            "Adjust how intense the flames feel.",
            118,
            new AcceptableValueRange<float>(MinEnergy, MaxEnergy),
            dispName: "Energy")
    );

    OrbitalsFlamesDrift = config.Bind(
        orbitalsFlamesSection,
        "Drift",
        DefaultDrift,
        OrderedDescription(
            "Adjust how much the flames drift away from their locked orbit path.",
            117,
            new AcceptableValueRange<float>(MinDrift, MaxDrift),
            dispName: "Drift",
            showRangeAsPercent: true)
    );

    OrbitalsFlamesHue = config.Bind(
        orbitalsFlamesSection,
        "Color",
        DefaultHue,
        OrderedDescription(
            "Adjust the color of the flames.",
            116,
            new AcceptableValueRange<float>(MinHue, MaxHue),
            dispName: "Color")
    );

    OrbitalsFlamesSpeed = config.Bind(
        orbitalsFlamesSection,
        "Speed",
        DefaultOrbitalsSpeed,
        OrderedDescription(
            "Adjust how quickly the flames travel through their orbit.",
            115,
            new AcceptableValueRange<float>(MinOrbitalsSpeed, MaxOrbitalsSpeed),
            dispName: "Speed")
    );

    OrbitalsFlamesSpacing = config.Bind(
        orbitalsFlamesSection,
        "Spacing",
        DefaultOrbitalsSpacing,
        OrderedDescription(
            "Adjust how closely the flames follow each other.",
            114,
            new AcceptableValueRange<float>(MinOrbitalsSpacing, MaxOrbitalsSpacing),
            dispName: "Spacing",
            showRangeAsPercent: true)
    );

    OrbitalsFlamesLength = config.Bind(
        orbitalsFlamesSection,
        "Orbit Length",
        DefaultOrbitalsLength,
        OrderedDescription(
            "Adjust how far the flames travel along the weapon before turning around.",
            113,
            new AcceptableValueRange<float>(MinOrbitalsLength, MaxOrbitalsLength),
            dispName: "Length")
    );

    OrbitalsFlamesRadius = config.Bind(
        orbitalsFlamesSection,
        "Radius",
        DefaultOrbitalsRadiusMultiplier,
        OrderedDescription(
            "Adjust how wide the flames wrap around the weapon.",
            112,
            new AcceptableValueRange<float>(MinOrbitalsRadiusMultiplier, MaxOrbitalsRadiusMultiplier),
            dispName: "Radius")
    );

    OrbitalsFlamesCycles = config.Bind(
        orbitalsFlamesSection,
        "Cycles",
        DefaultOrbitalsCycles,
        OrderedDescription(
            "Adjust how many turns the flames make before reversing direction.",
            111,
            new AcceptableValueRange<float>(MinOrbitalsCycles, MaxOrbitalsCycles),
            dispName: "Cycles")
    );

    OrbitalsFlamesXOffset = config.Bind(
        orbitalsFlamesSection,
        "X Offset",
        DefaultEffectOffset,
        OrderedDescription(
            "Adjust the X axis offset of the whole Orbitals Flames effect.",
            110,
            new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
            dispName: "X Offset")
    );

    OrbitalsFlamesYOffset = config.Bind(
        orbitalsFlamesSection,
        "Y Offset",
        DefaultEffectOffset,
        OrderedDescription(
            "Adjust the Y axis offset of the whole Orbitals Flames effect.",
            109,
            new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
            dispName: "Y Offset")
    );

    OrbitalsFlamesZOffset = config.Bind(
        orbitalsFlamesSection,
        "Z Offset",
        DefaultEffectOffset,
        OrderedDescription(
            "Adjust the Z axis offset of the whole Orbitals Flames effect.",
            108,
            new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
            dispName: "Z Offset")
    );

    OrbitalsFlamesXRotation = config.Bind(
        orbitalsFlamesSection,
        "X Rotation",
        DefaultEffectRotation,
        OrderedDescription(
            "Rotate the whole Orbitals Flames effect around its local X axis.",
            107,
            new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
            dispName: "X Rotation")
    );

    OrbitalsFlamesYRotation = config.Bind(
        orbitalsFlamesSection,
        "Y Rotation",
        DefaultEffectRotation,
        OrderedDescription(
            "Rotate the whole Orbitals Flames effect around its local Y axis.",
            106,
            new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
            dispName: "Y Rotation")
    );

    OrbitalsFlamesZRotation = config.Bind(
        orbitalsFlamesSection,
        "Z Rotation",
        DefaultEffectRotation,
        OrderedDescription(
            "Rotate the whole Orbitals Flames effect around its local Z axis.",
            105,
            new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
            dispName: "Z Rotation")
    );

    OrbitalsFlamesBottomSpacer = config.Bind(
        orbitalsFlamesSection,
        "__Orbitals Flames Bottom Spacer",
        false,
        OrderedDescription(
            "",
            -999,
            customDrawer: ConfigurationManagerDrawers.DrawSectionSpacer,
            hideSettingName: true,
            hideDefaultButton: true)
    );

    OrbitalsEmbers = config.Bind(
        orbitalsEmbersSection,
        "Enabled",
        true,
        OrderedDescription(
            "Turn Embers on or off.",
            100,
            dispName: "Enabled",
            customDrawer: ConfigurationManagerDrawers.DrawEnabledCheckboxWithLabel,
            hideSettingName: true)
    );

    OrbitalsEmbersCount = config.Bind(
        orbitalsEmbersSection,
        "Count",
        DefaultCountNormalized,
        OrderedDescription(
            "Adjust how many embers are active.",
            99,
            new AcceptableValueRange<float>(MinCountNormalized, MaxCountNormalized),
            dispName: "Count",
            showRangeAsPercent: true)
    );

    OrbitalsEmbersEnergy = config.Bind(
        orbitalsEmbersSection,
        "Energy",
        DefaultEnergy,
        OrderedDescription(
            "Adjust how intense the embers feel.",
            98,
            new AcceptableValueRange<float>(MinEnergy, MaxEnergy),
            dispName: "Energy")
    );

    OrbitalsEmbersDrift = config.Bind(
        orbitalsEmbersSection,
        "Drift",
        DefaultDrift,
        OrderedDescription(
            "Adjust how much the embers drift away from their locked orbit path.",
            97,
            new AcceptableValueRange<float>(MinDrift, MaxDrift),
            dispName: "Drift",
            showRangeAsPercent: true)
    );

    OrbitalsEmbersHue = config.Bind(
        orbitalsEmbersSection,
        "Color",
        DefaultHue,
        OrderedDescription(
            "Adjust the color of the embers.",
            96,
            new AcceptableValueRange<float>(MinHue, MaxHue),
            dispName: "Color")
    );

    OrbitalsEmbersSpeed = config.Bind(
        orbitalsEmbersSection,
        "Speed",
        DefaultOrbitalsSpeed,
        OrderedDescription(
            "Adjust how quickly the embers travel through their orbit.",
            95,
            new AcceptableValueRange<float>(MinOrbitalsSpeed, MaxOrbitalsSpeed),
            dispName: "Speed")
    );

    OrbitalsEmbersSpacing = config.Bind(
        orbitalsEmbersSection,
        "Spacing",
        DefaultOrbitalsSpacing,
        OrderedDescription(
            "Adjust how closely the embers follow each other.",
            94,
            new AcceptableValueRange<float>(MinOrbitalsSpacing, MaxOrbitalsSpacing),
            dispName: "Spacing",
            showRangeAsPercent: true)
    );

    OrbitalsEmbersLength = config.Bind(
        orbitalsEmbersSection,
        "Orbit Length",
        DefaultOrbitalsLength,
        OrderedDescription(
            "Adjust how far the embers travel along the weapon before turning around.",
            93,
            new AcceptableValueRange<float>(MinOrbitalsLength, MaxOrbitalsLength),
            dispName: "Length")
    );

    OrbitalsEmbersRadius = config.Bind(
        orbitalsEmbersSection,
        "Radius",
        DefaultOrbitalsRadiusMultiplier,
        OrderedDescription(
            "Adjust how wide the embers wrap around the weapon.",
            92,
            new AcceptableValueRange<float>(MinOrbitalsRadiusMultiplier, MaxOrbitalsRadiusMultiplier),
            dispName: "Radius")
    );

    OrbitalsEmbersCycles = config.Bind(
        orbitalsEmbersSection,
        "Cycles",
        DefaultOrbitalsCycles,
        OrderedDescription(
            "Adjust how many turns the embers make before reversing direction.",
            91,
            new AcceptableValueRange<float>(MinOrbitalsCycles, MaxOrbitalsCycles),
            dispName: "Cycles")
    );

    OrbitalsEmbersLifetime = config.Bind(
        orbitalsEmbersSection,
        "Lifetime",
        DefaultLifetime,
        OrderedDescription(
            "Adjust how long emitted embers remain visible.",
            90,
            new AcceptableValueRange<float>(MinLifetime, MaxLifetime),
            dispName: "Lifetime")
    );

    OrbitalsEmbersXOffset = config.Bind(
        orbitalsEmbersSection,
        "X Offset",
        DefaultEffectOffset,
        OrderedDescription(
            "Adjust the X axis offset of the whole Orbitals Embers effect.",
            89,
            new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
            dispName: "X Offset")
    );

    OrbitalsEmbersYOffset = config.Bind(
        orbitalsEmbersSection,
        "Y Offset",
        DefaultEffectOffset,
        OrderedDescription(
            "Adjust the Y axis offset of the whole Orbitals Embers effect.",
            88,
            new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
            dispName: "Y Offset")
    );

    OrbitalsEmbersZOffset = config.Bind(
        orbitalsEmbersSection,
        "Z Offset",
        DefaultEffectOffset,
        OrderedDescription(
            "Adjust the Z axis offset of the whole Orbitals Embers effect.",
            87,
            new AcceptableValueRange<float>(MinEffectOffset, MaxEffectOffset),
            dispName: "Z Offset")
    );

    OrbitalsEmbersXRotation = config.Bind(
        orbitalsEmbersSection,
        "X Rotation",
        DefaultEffectRotation,
        OrderedDescription(
            "Rotate the whole Orbitals Embers effect around its local X axis.",
            86,
            new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
            dispName: "X Rotation")
    );

    OrbitalsEmbersYRotation = config.Bind(
        orbitalsEmbersSection,
        "Y Rotation",
        DefaultEffectRotation,
        OrderedDescription(
            "Rotate the whole Orbitals Embers effect around its local Y axis.",
            85,
            new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
            dispName: "Y Rotation")
    );

    OrbitalsEmbersZRotation = config.Bind(
        orbitalsEmbersSection,
        "Z Rotation",
        DefaultEffectRotation,
        OrderedDescription(
            "Rotate the whole Orbitals Embers effect around its local Z axis.",
            84,
            new AcceptableValueRange<float>(MinEffectRotation, MaxEffectRotation),
            dispName: "Z Rotation")
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