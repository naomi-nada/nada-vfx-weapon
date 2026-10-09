using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace NADA.VFX.Weapon.Editor
{
    internal static class NadaVfxEditorControls
    {
        private const float WideModifierLabelWidth = 88f;
        private const float CompactModifierLabelWidth = 68f;

        private const float WideNumericFieldWidth = 54f;
        private const float CompactNumericFieldWidth = 48f;

        private const float WideResetButtonWidth = 30f;
        private const float CompactResetButtonWidth = 28f;

        private const float WideAxisLabelWidth = 12f;
        private const float CompactAxisLabelWidth = 10f;

        private const float WideAxisFieldWidth = 52f;
        private const float CompactAxisFieldWidth = 40f;

        private const float WideSliderMinimumWidth = 70f;
        private const float CompactSliderMinimumWidth = 48f;

        private static readonly Dictionary<string, bool>
            SectionExpandedByKey =
                new();

        private static readonly Dictionary<string, string>
            NumericBufferByKey =
                new();

        private static readonly List<Texture2D>
            OwnedTextures =
                new();

        private static NadaVfxEditorThemePreset
            _appliedThemePreset;

        private static bool
            _hasAppliedTheme;

        private static bool
            _compactLayout;

        // Bound-state inspectors keep foldout navigation usable while their
        // actual value controls remain disabled by the editor shell.
        private static bool _readOnlyNavigation;

        private static GUIStyle _sectionButtonStyle;
        private static GUIStyle _modifierRowStyle;
        private static GUIStyle _sliderLabelStyle;
        private static GUIStyle _axisLabelStyle;
        private static GUIStyle _valueFieldStyle;
        private static GUIStyle _valueStyle;
        private static GUIStyle _placeholderStyle;
        private static GUIStyle _toggleStyle;
        private static GUIStyle _segmentButtonStyle;
        private static GUIStyle _selectedSegmentButtonStyle;
        private static GUIStyle _resetButtonStyle;

        private static float ModifierLabelWidth =>
            _compactLayout
                ? CompactModifierLabelWidth
                : WideModifierLabelWidth;

        private static float NumericFieldWidth =>
            _compactLayout
                ? CompactNumericFieldWidth
                : WideNumericFieldWidth;

        private static float ResetButtonWidth =>
            _compactLayout
                ? CompactResetButtonWidth
                : WideResetButtonWidth;

        private static float AxisLabelWidth =>
            _compactLayout
                ? CompactAxisLabelWidth
                : WideAxisLabelWidth;

        private static float AxisFieldWidth =>
            _compactLayout
                ? CompactAxisFieldWidth
                : WideAxisFieldWidth;

        private static float SliderMinimumWidth =>
            _compactLayout
                ? CompactSliderMinimumWidth
                : WideSliderMinimumWidth;

        private static float ComponentGap =>
            _compactLayout
                ? 3f
                : 5f;

        internal static void SetCompactLayout(
            bool compact)
        {
            _compactLayout =
                compact;
        }

        internal static void SetReadOnlyNavigation(bool enabled)
        {
            _readOnlyNavigation = enabled;
        }

        internal static bool Section(
            string key,
            string title,
            bool defaultExpanded = true)
        {
            EnsureStyles();

            if (string.IsNullOrWhiteSpace(
                    key))
            {
                key =
                    title ??
                    "section";
            }

            if (!SectionExpandedByKey.TryGetValue(
                    key,
                    out bool expanded))
            {
                expanded =
                    defaultExpanded;

                SectionExpandedByKey[
                    key] =
                    expanded;
            }

            string marker =
                expanded
                    ? "−"
                    : "+";

            bool previousEnabled = GUI.enabled;
            bool clicked;
            try
            {
                if (_readOnlyNavigation)
                    GUI.enabled = true;

                clicked = GUILayout.Button(
                    $"{marker}  {title}",
                    _sectionButtonStyle,
                    GUILayout.ExpandWidth(true),
                    GUILayout.Height(19f));
            }
            finally
            {
                GUI.enabled = previousEnabled;
            }

            if (clicked)
            {
                expanded = !expanded;
                SectionExpandedByKey[key] = expanded;
            }

            return expanded;
        }

        internal static bool Toggle(
            string key,
            string label,
            bool value,
            out bool nextValue,
            bool enabled = true,
            string disabledReason = null,
            string description = null)
        {
            EnsureStyles();

            nextValue =
                value;

            GUILayout.BeginHorizontal(
                _modifierRowStyle,
                GUILayout.Height(20f));

            bool previousEnabled =
                GUI.enabled;

            GUI.enabled =
                previousEnabled &&
                enabled;

            bool drawnValue =
                GUILayout.Toggle(
                    value,
                    label,
                    _toggleStyle,
                    GUILayout.Height(18f));

            GUI.enabled =
                previousEnabled;

            GUILayout.EndHorizontal();

            bool changed =
                enabled &&
                drawnValue != value;

            if (changed)
            {
                nextValue =
                    drawnValue;
            }

            return changed;
        }

        internal static bool FloatSlider(
            string key,
            string label,
            float value,
            float minimum,
            float maximum,
            out float nextValue,
            int decimals = 2,
            bool enabled = true,
            string disabledReason = null,
            string description = null,
            float? resetValue = null)
        {
            EnsureStyles();

            nextValue =
                value;

            if (minimum >
                maximum)
            {
                float temporary =
                    minimum;

                minimum =
                    maximum;

                maximum =
                    temporary;
            }

            float safeSliderValue =
                IsFinite(
                    value)
                    ? Mathf.Clamp(
                        value,
                        minimum,
                        maximum)
                    : Mathf.Clamp(
                        0f,
                        minimum,
                        maximum);

            string controlName =
                $"NadaVfxFloat_{key}";

            bool textFieldFocused =
                string.Equals(
                    GUI.GetNameOfFocusedControl(),
                    controlName,
                    System.StringComparison.Ordinal);

            if (!NumericBufferByKey.TryGetValue(
                    key,
                    out string numericBuffer) ||
                !textFieldFocused)
            {
                numericBuffer =
                    FormatFloat(
                        value,
                        decimals);

                NumericBufferByKey[
                    key] =
                    numericBuffer;
            }

            GUILayout.BeginHorizontal(
                _modifierRowStyle,
                GUILayout.Height(21f));

            GUILayout.Label(
                label,
                _sliderLabelStyle,
                GUILayout.Width(
                    ModifierLabelWidth),
                GUILayout.Height(19f));

            bool previousEnabled =
                GUI.enabled;

            GUI.enabled =
                previousEnabled &&
                enabled;

            Rect sliderSlot =
                GUILayoutUtility.GetRect(
                    SliderMinimumWidth,
                    19f,
                    GUILayout.MinWidth(
                        SliderMinimumWidth),
                    GUILayout.ExpandWidth(true));

            Rect sliderRect =
                new Rect(
                    sliderSlot.x,
                    sliderSlot.y + 3f,
                    sliderSlot.width,
                    13f);

            float sliderValue =
                GUI.HorizontalSlider(
                    sliderRect,
                    safeSliderValue,
                    minimum,
                    maximum);

            GUILayout.Space(
                ComponentGap);

            GUI.SetNextControlName(
                controlName);

            string typedValue =
                GUILayout.TextField(
                    numericBuffer ??
                    string.Empty,
                    14,
                    _valueFieldStyle,
                    GUILayout.Width(
                        NumericFieldWidth),
                    GUILayout.Height(19f));

            bool resetClicked =
                false;

            float clampedResetValue =
                value;

            if (resetValue.HasValue)
            {
                GUILayout.Space(
                    ComponentGap);

                clampedResetValue =
                    Mathf.Clamp(
                        resetValue.Value,
                        minimum,
                        maximum);

                bool canReset =
                    enabled &&
                    IsFinite(
                        resetValue.Value) &&
                    !Mathf.Approximately(
                        value,
                        clampedResetValue);

                GUI.enabled =
                    previousEnabled &&
                    canReset;

                resetClicked =
                    GUILayout.Button(
                        "↺",
                        _resetButtonStyle,
                        GUILayout.Width(
                            ResetButtonWidth),
                        GUILayout.Height(20f));
            }

            GUI.enabled =
                previousEnabled;

            GUILayout.EndHorizontal();

            bool changed =
                false;

            if (resetClicked)
            {
                nextValue =
                    clampedResetValue;

                NumericBufferByKey[
                    key] =
                    FormatFloat(
                        nextValue,
                        decimals);

                changed =
                    !Mathf.Approximately(
                        nextValue,
                        value);
            }
            else if (enabled &&
                     !Mathf.Approximately(
                         sliderValue,
                         safeSliderValue))
            {
                nextValue =
                    sliderValue;

                NumericBufferByKey[
                    key] =
                    FormatFloat(
                        nextValue,
                        decimals);

                changed =
                    !Mathf.Approximately(
                        nextValue,
                        value);
            }
            else
            {
                NumericBufferByKey[
                    key] =
                    typedValue;

                if (enabled &&
                    float.TryParse(
                        typedValue,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out float parsedValue))
                {
                    float clampedValue =
                        Mathf.Clamp(
                            parsedValue,
                            minimum,
                            maximum);

                    if (!Mathf.Approximately(
                            clampedValue,
                            value))
                    {
                        nextValue =
                            clampedValue;

                        changed =
                            true;
                    }

                    if (!Mathf.Approximately(
                            parsedValue,
                            clampedValue))
                    {
                        NumericBufferByKey[
                            key] =
                            FormatFloat(
                                clampedValue,
                                decimals);
                    }
                }
            }

            return changed;
        }

        internal static bool Vector3Fields(
            string key,
            string label,
            float x,
            float y,
            float z,
            float minimum,
            float maximum,
            out float nextX,
            out float nextY,
            out float nextZ,
            int decimals = 3,
            bool enabled = true,
            string disabledReason = null,
            string description = null,
            float? resetValue = null)
        {
            EnsureStyles();

            nextX =
                x;

            nextY =
                y;

            nextZ =
                z;

            if (minimum >
                maximum)
            {
                float temporary =
                    minimum;

                minimum =
                    maximum;

                maximum =
                    temporary;
            }

            GUILayout.BeginHorizontal(
                _modifierRowStyle,
                GUILayout.Height(21f));

            GUILayout.Label(
                label,
                _sliderLabelStyle,
                GUILayout.Width(
                    ModifierLabelWidth));

            bool previousEnabled =
                GUI.enabled;

            GUI.enabled =
                previousEnabled &&
                enabled;

            bool changed =
                false;

            changed |=
                DrawVectorComponent(
                    $"{key}:x",
                    "X",
                    x,
                    minimum,
                    maximum,
                    decimals,
                    out nextX);

            GUILayout.Space(
                ComponentGap);

            changed |=
                DrawVectorComponent(
                    $"{key}:y",
                    "Y",
                    y,
                    minimum,
                    maximum,
                    decimals,
                    out nextY);

            GUILayout.Space(
                ComponentGap);

            changed |=
                DrawVectorComponent(
                    $"{key}:z",
                    "Z",
                    z,
                    minimum,
                    maximum,
                    decimals,
                    out nextZ);

            if (resetValue.HasValue)
            {
                GUILayout.Space(
                    ComponentGap);

                float clampedReset =
                    Mathf.Clamp(
                        resetValue.Value,
                        minimum,
                        maximum);

                bool canReset =
                    enabled &&
                    IsFinite(
                        resetValue.Value) &&
                    (!Mathf.Approximately(
                         x,
                         clampedReset) ||
                     !Mathf.Approximately(
                         y,
                         clampedReset) ||
                     !Mathf.Approximately(
                         z,
                         clampedReset));

                GUI.enabled =
                    previousEnabled &&
                    canReset;

                if (GUILayout.Button(
                        "↺",
                        _resetButtonStyle,
                        GUILayout.Width(
                            ResetButtonWidth),
                        GUILayout.Height(20f)))
                {
                    nextX =
                        clampedReset;

                    nextY =
                        clampedReset;

                    nextZ =
                        clampedReset;

                    SetNumericBuffer(
                        $"{key}:x",
                        nextX,
                        decimals);

                    SetNumericBuffer(
                        $"{key}:y",
                        nextY,
                        decimals);

                    SetNumericBuffer(
                        $"{key}:z",
                        nextZ,
                        decimals);

                    changed =
                        true;
                }
            }

            GUI.enabled =
                previousEnabled;

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();

            return
                enabled &&
                changed;
        }

        internal static bool SegmentedSelector(
            string key,
            string label,
            int selectedIndex,
            string[] options,
            out int nextSelectedIndex,
            bool enabled = true,
            string disabledReason = null,
            string description = null)
        {
            EnsureStyles();

            nextSelectedIndex =
                selectedIndex;

            if (options == null ||
                options.Length == 0)
            {
                return false;
            }

            int safeSelectedIndex =
                Mathf.Clamp(
                    selectedIndex,
                    0,
                    options.Length - 1);

            GUILayout.BeginHorizontal(
                _modifierRowStyle,
                GUILayout.Height(21f));

            bool showLabel =
                !string.IsNullOrWhiteSpace(
                    label) &&
                !string.Equals(
                    label,
                    "Mode",
                    System.StringComparison.OrdinalIgnoreCase);

            if (showLabel)
            {
                GUILayout.Label(
                    label,
                    _sliderLabelStyle,
                    GUILayout.Width(
                        ModifierLabelWidth));
            }

            bool previousEnabled =
                GUI.enabled;

            GUI.enabled =
                previousEnabled &&
                enabled;

            bool changed =
                false;

            for (int i = 0;
                 i < options.Length;
                 i++)
            {
                string option =
                    options[i] ??
                    string.Empty;

                GUIStyle style =
                    i ==
                    safeSelectedIndex
                        ? _selectedSegmentButtonStyle
                        : _segmentButtonStyle;

                if (GUILayout.Button(
                        option,
                        style,
                        GUILayout.ExpandWidth(true),
                        GUILayout.Height(19f)))
                {
                    if (i !=
                        safeSelectedIndex)
                    {
                        nextSelectedIndex =
                            i;

                        changed =
                            true;
                    }
                }
            }

            GUI.enabled =
                previousEnabled;

            GUILayout.EndHorizontal();

            return
                enabled &&
                changed;
        }

        internal static void ReadOnlyFloat(
            string label,
            float value)
        {
            EnsureStyles();

            GUILayout.BeginHorizontal(
                _modifierRowStyle,
                GUILayout.Height(20f));

            GUILayout.Label(
                label,
                _sliderLabelStyle,
                GUILayout.Width(
                    ModifierLabelWidth));

            GUILayout.Label(
                FormatFloat(
                    value,
                    3),
                _valueStyle);

            GUILayout.EndHorizontal();
        }

        internal static void ReadOnlyBool(
            string label,
            bool value)
        {
            EnsureStyles();

            GUILayout.BeginHorizontal(
                _modifierRowStyle,
                GUILayout.Height(20f));

            GUILayout.Label(
                label,
                _sliderLabelStyle,
                GUILayout.Width(
                    ModifierLabelWidth));

            GUILayout.Label(
                value
                    ? "On"
                    : "Off",
                _valueStyle);

            GUILayout.EndHorizontal();
        }

        internal static void ReadOnlyVector3(
            string label,
            float x,
            float y,
            float z)
        {
            EnsureStyles();

            GUILayout.BeginHorizontal(
                _modifierRowStyle,
                GUILayout.Height(20f));

            GUILayout.Label(
                label,
                _sliderLabelStyle,
                GUILayout.Width(
                    ModifierLabelWidth));

            GUILayout.Label(
                $"X {x:0.###}   Y {y:0.###}   Z {z:0.###}",
                _valueStyle);

            GUILayout.EndHorizontal();
        }

        internal static void Hint(
            string message)
        {
            _ =
                message;
        }

        internal static void Placeholder(
            string message)
        {
            EnsureStyles();

            if (string.IsNullOrWhiteSpace(
                    message))
            {
                return;
            }

            GUILayout.Label(
                message,
                _placeholderStyle);
        }

        internal static void SpaceAfterSection()
        {
            GUILayout.Space(
                3f);
        }

        internal static void DrawTooltipOverlay(
            float availableWidth,
            float availableHeight)
        {
            _ =
                availableWidth;

            _ =
                availableHeight;
        }

        internal static void ResetUiState()
        {
            SectionExpandedByKey.Clear();
            NumericBufferByKey.Clear();

            _compactLayout =
                false;

            ResetVisualStyles();
        }

        private static bool DrawVectorComponent(
            string key,
            string axisLabel,
            float value,
            float minimum,
            float maximum,
            int decimals,
            out float nextValue)
        {
            nextValue =
                value;

            GUILayout.Label(
                axisLabel,
                _axisLabelStyle,
                GUILayout.Width(
                    AxisLabelWidth));

            string controlName =
                $"NadaVfxFloat_{key}";

            bool textFieldFocused =
                string.Equals(
                    GUI.GetNameOfFocusedControl(),
                    controlName,
                    System.StringComparison.Ordinal);

            if (!NumericBufferByKey.TryGetValue(
                    key,
                    out string numericBuffer) ||
                !textFieldFocused)
            {
                numericBuffer =
                    FormatFloat(
                        value,
                        decimals);

                NumericBufferByKey[
                    key] =
                    numericBuffer;
            }

            GUI.SetNextControlName(
                controlName);

            string typedValue =
                GUILayout.TextField(
                    numericBuffer ??
                    string.Empty,
                    14,
                    _valueFieldStyle,
                    GUILayout.Width(
                        AxisFieldWidth),
                    GUILayout.Height(19f));

            NumericBufferByKey[
                key] =
                    typedValue;

            if (!float.TryParse(
                    typedValue,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out float parsedValue))
            {
                return false;
            }

            float clampedValue =
                Mathf.Clamp(
                    parsedValue,
                    minimum,
                    maximum);

            if (!Mathf.Approximately(
                    parsedValue,
                    clampedValue))
            {
                NumericBufferByKey[
                    key] =
                    FormatFloat(
                        clampedValue,
                        decimals);
            }

            if (Mathf.Approximately(
                    clampedValue,
                    value))
            {
                return false;
            }

            nextValue =
                clampedValue;

            return true;
        }

        private static void SetNumericBuffer(
            string key,
            float value,
            int decimals)
        {
            NumericBufferByKey[
                key] =
                    FormatFloat(
                        value,
                        decimals);
        }

        private static void EnsureStyles()
        {
            NadaVfxEditorThemePreset currentPreset =
                NadaVfxEditorThemes.CurrentPreset;

            if (_sectionButtonStyle != null &&
                _hasAppliedTheme &&
                _appliedThemePreset ==
                currentPreset)
            {
                return;
            }

            ResetVisualStyles();

            _appliedThemePreset =
                currentPreset;

            _hasAppliedTheme =
                true;

            NadaVfxEditorTheme theme =
                NadaVfxEditorThemes.Get(
                    currentPreset);

            Texture2D sectionBackground =
                CreateTexture(
                    theme.PanelBackground);

            Texture2D sectionHoverBackground =
                CreateTexture(
                    theme.ButtonHoverBackground);

            Texture2D segmentBackground =
                CreateTexture(
                    theme.SegmentBackground);

            Texture2D segmentHoverBackground =
                CreateTexture(
                    theme.SegmentHoverBackground);

            Texture2D selectedSegmentBackground =
                CreateTexture(
                    theme.SelectedSegmentBackground);

            Texture2D resetBackground =
                CreateTexture(
                    theme.ResetBackground);

            Texture2D resetHoverBackground =
                CreateTexture(
                    theme.ResetHoverBackground);

            _sectionButtonStyle =
                new GUIStyle(
                    GUI.skin.button)
                {
                    fontSize = 9,
                    padding =
                        new RectOffset(
                            6,
                            6,
                            1,
                            1),
                    margin =
                        new RectOffset(
                            0,
                            0,
                            1,
                            1)
                };

            _sectionButtonStyle.normal.background =
                sectionBackground;

            _sectionButtonStyle.hover.background =
                sectionHoverBackground;

            _sectionButtonStyle.active.background =
                sectionHoverBackground;

            _sectionButtonStyle.normal.textColor =
                theme.AccentSecondary;

            _sectionButtonStyle.hover.textColor =
                theme.AccentTertiary;

            _sectionButtonStyle.active.textColor =
                theme.AccentTertiary;

            _modifierRowStyle =
                new GUIStyle(
                    GUIStyle.none)
                {
                    padding =
                        new RectOffset(
                            2,
                            2,
                            1,
                            1),
                    margin =
                        new RectOffset(
                            0,
                            0,
                            0,
                            0)
                };

            _sliderLabelStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    fontSize = 10,
                    wordWrap = false,
                    clipping =
                        TextClipping.Clip,
                    padding =
                        new RectOffset(
                            2,
                            2,
                            2,
                            0)
                };

            _sliderLabelStyle.normal.textColor =
                theme.PrimaryText;

            _axisLabelStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    fontSize = 9,
                    wordWrap = false,
                    padding =
                        new RectOffset(
                            0,
                            0,
                            2,
                            0)
                };

            _axisLabelStyle.normal.textColor =
                theme.MutedText;

            _valueFieldStyle =
                new GUIStyle(
                    GUI.skin.textField)
                {
                    fontSize = 9,
                    alignment =
                        TextAnchor.MiddleRight,
                    padding =
                        new RectOffset(
                            4,
                            4,
                            2,
                            2)
                };

            _valueFieldStyle.normal.textColor =
                theme.PrimaryText;

            _valueFieldStyle.focused.textColor =
                theme.PrimaryText;

            _valueFieldStyle.hover.textColor =
                theme.PrimaryText;

            _valueStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    fontSize = 10,
                    wordWrap = false,
                    padding =
                        new RectOffset(
                            2,
                            2,
                            1,
                            1)
                };

            _valueStyle.normal.textColor =
                theme.PrimaryText;

            _placeholderStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    fontSize = 9,
                    wordWrap = true,
                    padding =
                        new RectOffset(
                            3,
                            3,
                            2,
                            2)
                };

            _placeholderStyle.normal.textColor =
                theme.MutedText;

            _toggleStyle =
                new GUIStyle(
                    GUI.skin.toggle)
                {
                    fontSize = 10,
                    padding =
                        new RectOffset(
                            18,
                            2,
                            1,
                            1)
                };

            _toggleStyle.normal.textColor =
                theme.PrimaryText;

            _toggleStyle.hover.textColor =
                theme.PrimaryText;

            _segmentButtonStyle =
                new GUIStyle(
                    GUI.skin.button)
                {
                    fontSize = 9,
                    padding =
                        new RectOffset(
                            4,
                            4,
                            1,
                            1)
                };

            _segmentButtonStyle.normal.background =
                segmentBackground;

            _segmentButtonStyle.hover.background =
                segmentHoverBackground;

            _segmentButtonStyle.active.background =
                segmentHoverBackground;

            _segmentButtonStyle.normal.textColor =
                theme.MutedText;

            _segmentButtonStyle.hover.textColor =
                theme.PrimaryText;

            _selectedSegmentButtonStyle =
                new GUIStyle(
                    _segmentButtonStyle);

            _selectedSegmentButtonStyle.normal.background =
                selectedSegmentBackground;

            _selectedSegmentButtonStyle.hover.background =
                selectedSegmentBackground;

            _selectedSegmentButtonStyle.active.background =
                selectedSegmentBackground;

            _selectedSegmentButtonStyle.normal.textColor =
                theme.AccentTertiary;

            _selectedSegmentButtonStyle.hover.textColor =
                theme.AccentTertiary;

            _resetButtonStyle =
                new GUIStyle(
                    GUI.skin.button)
                {
                    fontSize = 17,
                    fontStyle =
                        FontStyle.Bold,
                    alignment =
                        TextAnchor.MiddleCenter,
                    padding =
                        new RectOffset(
                            0,
                            0,
                            0,
                            1)
                };

            _resetButtonStyle.normal.background =
                resetBackground;

            _resetButtonStyle.hover.background =
                resetHoverBackground;

            _resetButtonStyle.active.background =
                resetHoverBackground;

            _resetButtonStyle.normal.textColor =
                theme.AccentPrimary;

            _resetButtonStyle.hover.textColor =
                theme.AccentSecondary;
        }

        private static void ResetVisualStyles()
        {
            foreach (Texture2D texture in
                     OwnedTextures)
            {
                if (texture == null)
                    continue;

                Object.Destroy(
                    texture);
            }

            OwnedTextures.Clear();

            _sectionButtonStyle = null;
            _modifierRowStyle = null;
            _sliderLabelStyle = null;
            _axisLabelStyle = null;
            _valueFieldStyle = null;
            _valueStyle = null;
            _placeholderStyle = null;
            _toggleStyle = null;
            _segmentButtonStyle = null;
            _selectedSegmentButtonStyle = null;
            _resetButtonStyle = null;

            _hasAppliedTheme =
                false;
        }

        private static Texture2D CreateTexture(
            Color color)
        {
            var texture =
                new Texture2D(
                    1,
                    1,
                    TextureFormat.RGBA32,
                    false)
                {
                    name =
                        "NADA VFX Editor Control Runtime UI",

                    hideFlags =
                        HideFlags.HideAndDontSave,

                    filterMode =
                        FilterMode.Point,

                    wrapMode =
                        TextureWrapMode.Clamp
                };

            texture.SetPixel(
                0,
                0,
                color);

            texture.Apply(
                false,
                true);

            OwnedTextures.Add(
                texture);

            return texture;
        }

        private static bool IsFinite(
            float value)
        {
            return
                !float.IsNaN(
                    value) &&
                !float.IsInfinity(
                    value);
        }

        private static string FormatFloat(
            float value,
            int decimals)
        {
            if (!IsFinite(
                    value))
            {
                return "0";
            }

            decimals =
                Mathf.Clamp(
                    decimals,
                    0,
                    6);

            return value.ToString(
                $"F{decimals}",
                CultureInfo.InvariantCulture);
        }
    }
}