using UnityEngine;

namespace NADA.VFX.Weapon.Editor
{
    /// <summary>
    /// Built-in presentation skins for the NADA VFX editor.
    ///
    /// These are editor-only presentation choices. They never belong in
    /// WeaponVfxState, weapon persistence, or multiplayer state.
    /// </summary>
    internal enum NadaVfxEditorThemePreset
    {
        NadaClassic = 0,
        NadaDark = 1,
        NadaVal = 2,
        NadaDesert = 3,
        NadaWinter = 4,
        NadaNeo = 5
    }

    /// <summary>
    /// Lets individual skins eventually change the title treatment without
    /// putting theme-specific branches throughout the editor shell.
    /// </summary>
    internal enum NadaVfxEditorTitleVariant
    {
        PixelClassic = 0,
        PixelVal = 1,
        PixelNeo = 2,
        PixelDark = 3,
        PixelDesert = 4,
        PixelWinter = 5
    }

    /// <summary>
    /// Immutable-by-convention presentation data for one editor skin.
    ///
    /// Runtime UI owns the generated Texture2D objects. Themes only supply
    /// values used to build those objects.
    /// </summary>
    internal sealed class NadaVfxEditorTheme
    {
        internal string DisplayName;

        internal NadaVfxEditorTitleVariant TitleVariant;

        internal Color32 WindowBackground;
        internal Color32 TargetBackground;
        internal Color32 SettingsBackground;
        internal Color32 PanelBackground;
        internal Color32 PickerBackground;
        internal Color32 ReadOnlyBackground;

        internal Color32 BlockBackground;
        internal Color32 BlockHoverBackground;
        internal Color32 SelectedBlockBackground;

        internal Color32 ButtonBackground;
        internal Color32 ButtonHoverBackground;

        internal Color32 SegmentBackground;
        internal Color32 SegmentHoverBackground;
        internal Color32 SelectedSegmentBackground;

        internal Color32 ResetBackground;
        internal Color32 ResetHoverBackground;

        internal Color32 EffectOnBackground;
        internal Color32 EffectOnHoverBackground;

        internal Color32 EffectOffBackground;
        internal Color32 EffectOffHoverBackground;

        internal Color32 PrimaryText;
        internal Color32 MutedText;
        internal Color32 DimText;

        internal Color32 AccentPrimary;
        internal Color32 AccentSecondary;
        internal Color32 AccentTertiary;

        // Target/Effects/Edit headings can contrast without recoloring Styles/Settings.
        internal Color32 LowerSectionHeadingText;

        internal Color32 TargetText;

        internal Color32 SuccessText;
        internal Color32 OffText;
        internal Color32 WarningText;

        internal Color32 TitlePrimary;
        internal Color32 TitleShadow;
        internal Color32 TitleSubtitle;
    }

    // Choose readable presentation colors against the surface that actually
    // owns the label. This runs when styles are built, not every IMGUI event.
    internal static class NadaVfxEditorContrast
    {
        internal static Color32 TextOn(
            Color32 background, Color32 preferred,
            Color32 alternate, Color32 fallback,
            float minimumRatio = 4.5f)
        {
            if (Ratio(preferred, background) >= minimumRatio)
                return preferred;
            if (Ratio(alternate, background) >= minimumRatio)
                return alternate;
            if (Ratio(fallback, background) >= minimumRatio)
                return fallback;

            // Covers future theme palettes that do not have a readable accent.
            Color32 light = new Color32(255, 255, 255, 255);
            Color32 dark = new Color32(12, 15, 22, 255);
            return Ratio(light, background) >= Ratio(dark, background)
                ? light : dark;
        }

        // Some headings appear on both Effects and Edit surfaces.
        internal static Color32 TextOnBoth(
            Color32 firstBackground, Color32 secondBackground,
            Color32 preferred, Color32 alternate, Color32 fallback)
        {
            if (Ratio(preferred, firstBackground) >= 4.5f &&
                Ratio(preferred, secondBackground) >= 4.5f)
                return preferred;
            if (Ratio(alternate, firstBackground) >= 4.5f &&
                Ratio(alternate, secondBackground) >= 4.5f)
                return alternate;
            if (Ratio(fallback, firstBackground) >= 4.5f &&
                Ratio(fallback, secondBackground) >= 4.5f)
                return fallback;

            Color32 light = new Color32(255, 255, 255, 255);
            Color32 dark = new Color32(12, 15, 22, 255);
            float lightMinimum = Mathf.Min(Ratio(light, firstBackground),
                Ratio(light, secondBackground));
            float darkMinimum = Mathf.Min(Ratio(dark, firstBackground),
                Ratio(dark, secondBackground));
            return lightMinimum >= darkMinimum ? light : dark;
        }

        internal static float Ratio(Color32 a, Color32 b)
        {
            float la = Luminance(a);
            float lb = Luminance(b);
            return (Mathf.Max(la, lb) + 0.05f) /
                   (Mathf.Min(la, lb) + 0.05f);
        }

        private static float Luminance(Color32 c)
        {
            return 0.2126f * Channel(c.r) +
                   0.7152f * Channel(c.g) +
                   0.0722f * Channel(c.b);
        }

        private static float Channel(byte channel)
        {
            float v = channel / 255f;
            return v <= 0.04045f
                ? v / 12.92f
                : Mathf.Pow((v + 0.055f) / 1.055f, 2.4f);
        }
    }

    internal static class NadaVfxEditorThemes
    {
        private static readonly NadaVfxEditorTheme Classic =
            new NadaVfxEditorTheme
            {
                DisplayName =
                    "nada-Classic",

                TitleVariant =
                    NadaVfxEditorTitleVariant.PixelClassic,

                WindowBackground =
                    C(10, 15, 32, 251),

                TargetBackground =
                    C(19, 45, 86, 250),

                SettingsBackground =
                    C(72, 48, 96, 250),

                PanelBackground =
                    C(13, 34, 71, 250),

                PickerBackground =
                    C(10, 20, 40),

                ReadOnlyBackground =
                    C(21, 31, 53),

                BlockBackground =
                    C(21, 38, 69),

                BlockHoverBackground =
                    C(32, 56, 91),

                SelectedBlockBackground =
                    C(43, 73, 123),

                ButtonBackground =
                    C(30, 45, 75),

                ButtonHoverBackground =
                    C(48, 76, 115),

                SegmentBackground =
                    C(19, 32, 55),

                SegmentHoverBackground =
                    C(37, 56, 91),

                SelectedSegmentBackground =
                    C(65, 86, 132),

                ResetBackground =
                    C(44, 36, 80),

                ResetHoverBackground =
                    C(82, 49, 118),

                EffectOnBackground =
                    C(20, 73, 62),

                EffectOnHoverBackground =
                    C(30, 98, 81),

                EffectOffBackground =
                    C(87, 26, 57),

                EffectOffHoverBackground =
                    C(114, 33, 77),

                PrimaryText =
                    C(250, 241, 255),

                MutedText =
                    C(181, 164, 206),

                DimText =
                    C(117, 92, 153),

                AccentPrimary =
                    C(73, 236, 255),

                AccentSecondary =
                    C(255, 88, 211),

                AccentTertiary =
                    C(183, 123, 255),

                LowerSectionHeadingText =
                    C(255, 88, 211),

                TargetText =
                    C(126, 197, 255),

                SuccessText =
                    C(103, 238, 179),

                OffText =
                    C(249, 118, 152),

                WarningText =
                    C(255, 189, 99),

                TitlePrimary =
                    C(115, 206, 255),

                TitleShadow =
                    C(255, 86, 202, 155),

                TitleSubtitle =
                    C(255, 98, 219)
            };

        private static readonly NadaVfxEditorTheme Dark =
            new NadaVfxEditorTheme
            {
                DisplayName =
                    "nada-Dark",

                TitleVariant =
                    NadaVfxEditorTitleVariant.PixelDark,

                WindowBackground =
                    C(5, 7, 10, 253),

                TargetBackground =
                    C(12, 16, 20, 251),

                SettingsBackground =
                    C(73, 75, 82, 251),

                PanelBackground =
                    C(9, 13, 18, 252),

                PickerBackground =
                    C(5, 9, 13),

                ReadOnlyBackground =
                    C(16, 20, 25),

                BlockBackground =
                    C(17, 23, 29),

                BlockHoverBackground =
                    C(28, 38, 44),

                SelectedBlockBackground =
                    C(44, 63, 68),

                ButtonBackground =
                    C(27, 34, 39),

                ButtonHoverBackground =
                    C(41, 53, 59),

                SegmentBackground =
                    C(12, 17, 22),

                SegmentHoverBackground =
                    C(24, 34, 40),

                SelectedSegmentBackground =
                    C(46, 70, 76),

                ResetBackground =
                    C(25, 37, 43),

                ResetHoverBackground =
                    C(44, 65, 71),

                EffectOnBackground =
                    C(22, 66, 56),

                EffectOnHoverBackground =
                    C(32, 86, 72),

                EffectOffBackground =
                    C(66, 29, 36),

                EffectOffHoverBackground =
                    C(82, 37, 46),

                PrimaryText =
                    C(243, 249, 250),

                MutedText =
                    C(159, 178, 185),

                DimText =
                    C(104, 127, 139),

                AccentPrimary =
                    C(164, 239, 229),

                AccentSecondary =
                    C(132, 187, 204),

                AccentTertiary =
                    C(204, 227, 230),

                LowerSectionHeadingText =
                    C(132, 187, 204),

                TargetText =
                    C(189, 226, 240),

                SuccessText =
                    C(147, 234, 196),

                OffText =
                    C(220, 111, 130),

                WarningText =
                    C(248, 195, 125),

                TitlePrimary =
                    C(234, 245, 248),

                TitleShadow =
                    C(119, 218, 211, 100),

                TitleSubtitle =
                    C(143, 195, 209)
            };

        private static readonly NadaVfxEditorTheme Val =
            new NadaVfxEditorTheme
            {
                DisplayName =
                    "nada-Heim",

                TitleVariant =
                    NadaVfxEditorTitleVariant.PixelVal,

                WindowBackground =
                    C(8, 26, 20, 251),

                TargetBackground =
                    C(19, 73, 126, 250),

                SettingsBackground =
                    C(18, 58, 44, 250),

                PanelBackground =
                    C(14, 57, 101, 251),

                PickerBackground =
                    C(7, 32, 27),

                ReadOnlyBackground =
                    C(19, 52, 41),

                BlockBackground =
                    C(24, 66, 49),

                BlockHoverBackground =
                    C(35, 89, 64),

                SelectedBlockBackground =
                    C(53, 111, 77),

                ButtonBackground =
                    C(31, 78, 59),

                ButtonHoverBackground =
                    C(47, 111, 81),

                SegmentBackground =
                    C(14, 49, 39),

                SegmentHoverBackground =
                    C(32, 82, 65),

                SelectedSegmentBackground =
                    C(54, 121, 92),

                ResetBackground =
                    C(36, 66, 44),

                ResetHoverBackground =
                    C(69, 112, 68),

                EffectOnBackground =
                    C(35, 90, 52),

                EffectOnHoverBackground =
                    C(52, 117, 67),

                EffectOffBackground =
                    C(83, 39, 29),

                EffectOffHoverBackground =
                    C(111, 47, 35),

                PrimaryText =
                    C(237, 246, 224),

                MutedText =
                    C(172, 204, 172),

                DimText =
                    C(112, 149, 128),

                AccentPrimary =
                    C(146, 229, 115),

                AccentSecondary =
                    C(103, 199, 255),

                AccentTertiary =
                    C(228, 194, 125),

                LowerSectionHeadingText =
                    C(103, 199, 255),

                TargetText =
                    C(165, 221, 255),

                SuccessText =
                    C(173, 230, 136),

                OffText =
                    C(247, 119, 101),

                WarningText =
                    C(255, 208, 111),

                TitlePrimary =
                    C(225, 233, 169),

                TitleShadow =
                    C(52, 173, 130, 140),

                TitleSubtitle =
                    C(104, 198, 229)
            };

        private static readonly NadaVfxEditorTheme Desert =
            new NadaVfxEditorTheme
            {
                DisplayName =
                    "nada-Desert",

                TitleVariant =
                    NadaVfxEditorTitleVariant.PixelDesert,

                WindowBackground =
                    C(51, 28, 16, 251),

                TargetBackground =
                    C(148, 88, 34, 250),

                SettingsBackground =
                    C(111, 44, 28, 250),

                PanelBackground =
                    C(138, 80, 34, 251),

                PickerBackground =
                    C(67, 35, 16),

                ReadOnlyBackground =
                    C(113, 65, 29),

                BlockBackground =
                    C(145, 87, 38),

                BlockHoverBackground =
                    C(161, 99, 43),

                SelectedBlockBackground =
                    C(181, 112, 50),

                ButtonBackground =
                    C(110, 61, 23),

                ButtonHoverBackground =
                    C(135, 75, 30),

                SegmentBackground =
                    C(105, 57, 23),

                SegmentHoverBackground =
                    C(141, 80, 32),

                SelectedSegmentBackground =
                    C(171, 102, 42),

                ResetBackground =
                    C(98, 47, 22),

                ResetHoverBackground =
                    C(168, 85, 28),

                EffectOnBackground =
                    C(53, 92, 48),

                EffectOnHoverBackground =
                    C(72, 115, 59),

                EffectOffBackground =
                    C(99, 40, 31),

                EffectOffHoverBackground =
                    C(132, 49, 33),

                PrimaryText =
                    C(255, 234, 196),

                MutedText =
                    C(222, 182, 133),

                DimText =
                    C(154, 113, 76),

                AccentPrimary =
                    C(255, 174, 60),

                AccentSecondary =
                    C(235, 101, 44),

                AccentTertiary =
                    C(255, 218, 133),

                LowerSectionHeadingText =
                    C(255, 235, 194),

                TargetText =
                    C(255, 203, 112),

                SuccessText =
                    C(191, 231, 128),

                OffText =
                    C(248, 123, 99),

                WarningText =
                    C(255, 213, 106),

                TitlePrimary =
                    C(255, 187, 83),

                TitleShadow =
                    C(203, 91, 40, 135),

                TitleSubtitle =
                    C(255, 143, 61)
            };

        private static readonly NadaVfxEditorTheme Winter =
            new NadaVfxEditorTheme
            {
                DisplayName =
                    "nada-Winter",

                TitleVariant =
                    NadaVfxEditorTitleVariant.PixelWinter,

                WindowBackground =
                    C(34, 53, 76, 253),

                TargetBackground =
                    C(57, 96, 129, 252),

                SettingsBackground =
                    C(31, 68, 110, 252),

                PanelBackground =
                    C(51, 88, 123, 253),

                PickerBackground =
                    C(43, 69, 97),

                ReadOnlyBackground =
                    C(47, 66, 89),

                BlockBackground =
                    C(71, 102, 133),

                BlockHoverBackground =
                    C(91, 126, 158),

                SelectedBlockBackground =
                    C(100, 150, 190),

                ButtonBackground =
                    C(76, 107, 139),

                ButtonHoverBackground =
                    C(94, 135, 169),

                SegmentBackground =
                    C(55, 81, 110),

                SegmentHoverBackground =
                    C(85, 125, 158),

                SelectedSegmentBackground =
                    C(110, 157, 194),

                ResetBackground =
                    C(64, 94, 129),

                ResetHoverBackground =
                    C(91, 138, 174),

                EffectOnBackground =
                    C(48, 103, 99),

                EffectOnHoverBackground =
                    C(60, 133, 124),

                EffectOffBackground =
                    C(116, 65, 90),

                EffectOffHoverBackground =
                    C(149, 81, 105),

                PrimaryText =
                    C(246, 250, 255),

                MutedText =
                    C(207, 227, 244),

                DimText =
                    C(157, 187, 215),

                AccentPrimary =
                    C(169, 232, 255),

                AccentSecondary =
                    C(202, 182, 255),

                AccentTertiary =
                    C(199, 228, 255),

                LowerSectionHeadingText =
                    C(202, 182, 255),

                TargetText =
                    C(195, 238, 255),

                SuccessText =
                    C(154, 242, 202),

                OffText =
                    C(255, 171, 193),

                WarningText =
                    C(255, 225, 162),

                TitlePrimary =
                    C(215, 246, 255),

                TitleShadow =
                    C(38, 75, 115, 190),

                TitleSubtitle =
                    C(210, 194, 255)
            };

        private static readonly NadaVfxEditorTheme Neo =
            new NadaVfxEditorTheme
            {
                DisplayName =
                    "nada-Neo",

                TitleVariant =
                    NadaVfxEditorTitleVariant.PixelNeo,

                WindowBackground =
                    C(24, 12, 52, 252),

                TargetBackground =
                    C(28, 70, 133, 251),

                SettingsBackground =
                    C(52, 30, 103, 251),

                PanelBackground =
                    C(26, 56, 119, 251),

                PickerBackground =
                    C(31, 15, 69),

                ReadOnlyBackground =
                    C(36, 50, 110),

                BlockBackground =
                    C(51, 78, 159),

                BlockHoverBackground =
                    C(75, 108, 193),

                SelectedBlockBackground =
                    C(114, 145, 217),

                ButtonBackground =
                    C(57, 82, 155),

                ButtonHoverBackground =
                    C(83, 115, 193),

                SegmentBackground =
                    C(47, 36, 110),

                SegmentHoverBackground =
                    C(76, 58, 151),

                SelectedSegmentBackground =
                    C(99, 112, 193),

                ResetBackground =
                    C(75, 40, 125),

                ResetHoverBackground =
                    C(132, 75, 186),

                EffectOnBackground =
                    C(35, 92, 100),

                EffectOnHoverBackground =
                    C(48, 123, 134),

                EffectOffBackground =
                    C(94, 37, 87),

                EffectOffHoverBackground =
                    C(127, 44, 107),

                PrimaryText =
                    C(247, 239, 255),

                MutedText =
                    C(198, 174, 234),

                DimText =
                    C(143, 117, 194),

                AccentPrimary =
                    C(162, 222, 255),

                AccentSecondary =
                    C(206, 148, 255),

                AccentTertiary =
                    C(172, 179, 255),

                LowerSectionHeadingText =
                    C(206, 148, 255),

                TargetText =
                    C(197, 232, 255),

                SuccessText =
                    C(154, 241, 214),

                OffText =
                    C(245, 139, 193),

                WarningText =
                    C(255, 211, 159),

                TitlePrimary =
                    C(201, 163, 255),

                TitleShadow =
                    C(139, 210, 255, 165),

                TitleSubtitle =
                    C(163, 218, 255)
            };

        internal static NadaVfxEditorThemePreset CurrentPreset
        {
            get
            {
                if (NadaVfxEditorConfig.Theme == null)
                {
                    return
                        NadaVfxEditorThemePreset
                            .NadaClassic;
                }

                return
                    NadaVfxEditorConfig
                        .Theme
                        .Value;
            }
        }

        internal static NadaVfxEditorTheme Current =>
            Get(
                CurrentPreset);

        internal static NadaVfxEditorTheme Get(
            NadaVfxEditorThemePreset preset)
        {
            return preset switch
            {
                NadaVfxEditorThemePreset.NadaDark =>
                    Dark,

                NadaVfxEditorThemePreset.NadaVal =>
                    Val,

                NadaVfxEditorThemePreset.NadaDesert =>
                    Desert,

                NadaVfxEditorThemePreset.NadaWinter =>
                    Winter,

                NadaVfxEditorThemePreset.NadaNeo =>
                    Neo,

                _ =>
                    Classic
            };
        }

        internal static string GetDisplayName(
            NadaVfxEditorThemePreset preset)
        {
            return
                Get(
                    preset).DisplayName;
        }

        private static Color32 C(
            byte red,
            byte green,
            byte blue,
            byte alpha = 255)
        {
            return
                new Color32(
                    red,
                    green,
                    blue,
                    alpha);
        }
    }
}

