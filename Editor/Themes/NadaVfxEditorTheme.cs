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
        NadaWinter = 4
    }

    /// <summary>
    /// Lets individual skins eventually change the title treatment without
    /// putting theme-specific branches throughout the editor shell.
    /// </summary>
    internal enum NadaVfxEditorTitleVariant
    {
        PixelClassic = 0,
        PixelVal = 1
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

        internal Color32 TargetText;

        internal Color32 SuccessText;
        internal Color32 OffText;
        internal Color32 WarningText;

        internal Color32 TitlePrimary;
        internal Color32 TitleShadow;
        internal Color32 TitleSubtitle;
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
                    C(8, 10, 16, 249),

                TargetBackground =
                    C(11, 14, 22, 248),

                PanelBackground =
                    C(14, 17, 26, 248),

                PickerBackground =
                    C(9, 12, 19),

                ReadOnlyBackground =
                    C(15, 17, 28),

                BlockBackground =
                    C(11, 14, 22),

                BlockHoverBackground =
                    C(19, 24, 38),

                SelectedBlockBackground =
                    C(38, 30, 58),

                ButtonBackground =
                    C(28, 32, 48),

                ButtonHoverBackground =
                    C(61, 45, 87),

                SegmentBackground =
                    C(11, 14, 22),

                SegmentHoverBackground =
                    C(27, 23, 43),

                SelectedSegmentBackground =
                    C(49, 38, 75),

                ResetBackground =
                    C(35, 29, 54),

                ResetHoverBackground =
                    C(78, 43, 97),

                EffectOnBackground =
                    C(24, 61, 47),

                EffectOnHoverBackground =
                    C(31, 78, 60),

                EffectOffBackground =
                    C(54, 24, 37),

                EffectOffHoverBackground =
                    C(72, 30, 48),

                PrimaryText =
                    C(237, 242, 247),

                MutedText =
                    C(144, 153, 171),

                DimText =
                    C(93, 102, 120),

                AccentPrimary =
                    C(50, 230, 255),

                AccentSecondary =
                    C(255, 79, 216),

                AccentTertiary =
                    C(169, 140, 255),

                TargetText =
                    C(105, 183, 255),

                SuccessText =
                    C(115, 245, 181),

                OffText =
                    C(198, 87, 117),

                WarningText =
                    C(255, 177, 91),

                TitlePrimary =
                    C(105, 183, 255),

                TitleShadow =
                    C(255, 79, 216, 105),

                TitleSubtitle =
                    C(255, 79, 216)
            };

        private static readonly NadaVfxEditorTheme Dark =
            new NadaVfxEditorTheme
            {
                DisplayName =
                    "nada-Dark",

                TitleVariant =
                    NadaVfxEditorTitleVariant.PixelClassic,

                WindowBackground =
                    C(5, 7, 10, 250),

                TargetBackground =
                    C(8, 10, 14, 249),

                PanelBackground =
                    C(9, 11, 16, 249),

                PickerBackground =
                    C(7, 9, 13),

                ReadOnlyBackground =
                    C(10, 12, 17),

                BlockBackground =
                    C(11, 13, 18),

                BlockHoverBackground =
                    C(17, 20, 28),

                SelectedBlockBackground =
                    C(22, 33, 40),

                ButtonBackground =
                    C(16, 19, 25),

                ButtonHoverBackground =
                    C(28, 37, 45),

                SegmentBackground =
                    C(9, 11, 16),

                SegmentHoverBackground =
                    C(18, 23, 29),

                SelectedSegmentBackground =
                    C(24, 37, 44),

                ResetBackground =
                    C(18, 25, 30),

                ResetHoverBackground =
                    C(28, 47, 52),

                EffectOnBackground =
                    C(19, 54, 44),

                EffectOnHoverBackground =
                    C(25, 71, 57),

                EffectOffBackground =
                    C(45, 25, 31),

                EffectOffHoverBackground =
                    C(61, 30, 39),

                PrimaryText =
                    C(228, 233, 238),

                MutedText =
                    C(127, 138, 149),

                DimText =
                    C(73, 83, 93),

                AccentPrimary =
                    C(88, 224, 220),

                AccentSecondary =
                    C(119, 174, 198),

                AccentTertiary =
                    C(157, 132, 255),

                TargetText =
                    C(124, 216, 255),

                SuccessText =
                    C(112, 235, 178),

                OffText =
                    C(179, 84, 100),

                WarningText =
                    C(243, 184, 100),

                TitlePrimary =
                    C(224, 234, 239),

                TitleShadow =
                    C(88, 224, 220, 95),

                TitleSubtitle =
                    C(119, 174, 198)
            };

        private static readonly NadaVfxEditorTheme Val =
            new NadaVfxEditorTheme
            {
                DisplayName =
                    "nada-Heim",

                TitleVariant =
                    NadaVfxEditorTitleVariant.PixelVal,

                WindowBackground =
                    C(10, 12, 10, 249),

                TargetBackground =
                    C(16, 18, 14, 248),

                PanelBackground =
                    C(13, 16, 13, 248),

                PickerBackground =
                    C(11, 14, 11),

                ReadOnlyBackground =
                    C(18, 19, 14),

                BlockBackground =
                    C(16, 18, 15),

                BlockHoverBackground =
                    C(29, 30, 21),

                SelectedBlockBackground =
                    C(51, 44, 28),

                ButtonBackground =
                    C(31, 31, 24),

                ButtonHoverBackground =
                    C(57, 49, 30),

                SegmentBackground =
                    C(17, 18, 14),

                SegmentHoverBackground =
                    C(34, 31, 20),

                SelectedSegmentBackground =
                    C(63, 49, 26),

                ResetBackground =
                    C(42, 35, 21),

                ResetHoverBackground =
                    C(76, 49, 25),

                EffectOnBackground =
                    C(29, 58, 36),

                EffectOnHoverBackground =
                    C(38, 76, 46),

                EffectOffBackground =
                    C(64, 31, 27),

                EffectOffHoverBackground =
                    C(88, 39, 31),

                PrimaryText =
                    C(226, 219, 197),

                MutedText =
                    C(145, 139, 120),

                DimText =
                    C(87, 85, 74),

                AccentPrimary =
                    C(236, 174, 75),

                AccentSecondary =
                    C(225, 77, 59),

                AccentTertiary =
                    C(112, 171, 176),

                TargetText =
                    C(223, 190, 112),

                SuccessText =
                    C(130, 191, 106),

                OffText =
                    C(203, 89, 72),

                WarningText =
                    C(240, 158, 67),

                TitlePrimary =
                    C(238, 219, 165),

                TitleShadow =
                    C(196, 67, 48, 115),

                TitleSubtitle =
                    C(225, 77, 59)
            };

        private static readonly NadaVfxEditorTheme Desert =
            new NadaVfxEditorTheme
            {
                DisplayName =
                    "nada-Desert",

                TitleVariant =
                    NadaVfxEditorTitleVariant.PixelClassic,

                WindowBackground =
                    C(19, 16, 13, 249),

                TargetBackground =
                    C(41, 34, 27, 248),

                PanelBackground =
                    C(29, 25, 20, 248),

                PickerBackground =
                    C(24, 20, 16),

                ReadOnlyBackground =
                    C(34, 29, 23),

                BlockBackground =
                    C(39, 32, 24),

                BlockHoverBackground =
                    C(57, 44, 29),

                SelectedBlockBackground =
                    C(110, 80, 38),

                ButtonBackground =
                    C(55, 44, 31),

                ButtonHoverBackground =
                    C(83, 61, 34),

                SegmentBackground =
                    C(37, 30, 23),

                SegmentHoverBackground =
                    C(62, 46, 29),

                SelectedSegmentBackground =
                    C(104, 70, 31),

                ResetBackground =
                    C(65, 48, 29),

                ResetHoverBackground =
                    C(103, 65, 31),

                EffectOnBackground =
                    C(40, 68, 43),

                EffectOnHoverBackground =
                    C(53, 88, 53),

                EffectOffBackground =
                    C(73, 38, 31),

                EffectOffHoverBackground =
                    C(96, 47, 35),

                PrimaryText =
                    C(240, 224, 196),

                MutedText =
                    C(166, 145, 119),

                DimText =
                    C(105, 90, 73),

                AccentPrimary =
                    C(243, 177, 73),

                AccentSecondary =
                    C(226, 112, 55),

                AccentTertiary =
                    C(193, 137, 78),

                TargetText =
                    C(244, 193, 104),

                SuccessText =
                    C(146, 204, 122),

                OffText =
                    C(211, 101, 77),

                WarningText =
                    C(255, 170, 69),

                TitlePrimary =
                    C(243, 177, 73),

                TitleShadow =
                    C(185, 72, 42, 115),

                TitleSubtitle =
                    C(226, 112, 55)
            };

        private static readonly NadaVfxEditorTheme Winter =
            new NadaVfxEditorTheme
            {
                DisplayName =
                    "nada-Winter",

                TitleVariant =
                    NadaVfxEditorTitleVariant.PixelClassic,

                WindowBackground =
                    C(41, 54, 71, 250),

                TargetBackground =
                    C(56, 72, 91, 249),

                PanelBackground =
                    C(48, 64, 83, 249),

                PickerBackground =
                    C(51, 66, 85),

                ReadOnlyBackground =
                    C(44, 58, 76),

                BlockBackground =
                    C(61, 80, 102),

                BlockHoverBackground =
                    C(76, 96, 119),

                SelectedBlockBackground =
                    C(81, 117, 151),

                ButtonBackground =
                    C(66, 84, 106),

                ButtonHoverBackground =
                    C(83, 110, 136),

                SegmentBackground =
                    C(52, 69, 88),

                SegmentHoverBackground =
                    C(74, 98, 121),

                SelectedSegmentBackground =
                    C(88, 132, 167),

                ResetBackground =
                    C(59, 85, 110),

                ResetHoverBackground =
                    C(82, 119, 148),

                EffectOnBackground =
                    C(45, 101, 88),

                EffectOnHoverBackground =
                    C(55, 126, 108),

                EffectOffBackground =
                    C(99, 52, 71),

                EffectOffHoverBackground =
                    C(124, 65, 86),

                PrimaryText =
                    C(235, 243, 250),

                MutedText =
                    C(182, 199, 217),

                DimText =
                    C(133, 155, 177),

                AccentPrimary =
                    C(127, 209, 244),

                AccentSecondary =
                    C(189, 164, 233),

                AccentTertiary =
                    C(149, 190, 227),

                TargetText =
                    C(167, 221, 255),

                SuccessText =
                    C(147, 225, 193),

                OffText =
                    C(241, 158, 176),

                WarningText =
                    C(255, 210, 152),

                TitlePrimary =
                    C(172, 223, 255),

                TitleShadow =
                    C(15, 36, 58, 165),

                TitleSubtitle =
                    C(190, 166, 233)
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

