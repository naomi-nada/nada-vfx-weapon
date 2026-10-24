
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
        private static GUIStyle _compactSectionButtonStyle;
        private static float _sectionWidth;
        private static float _sectionLeftOffset;

        private static GUIStyle _modifierRowStyle;
        private static GUIStyle _sliderLabelStyle;
        private static GUIStyle _axisLabelStyle;
        private static GUIStyle _valueFieldStyle;
        private static GUIStyle _valueStyle;
        private static GUIStyle _placeholderStyle;
        private static GUIStyle _toggleStyle;
        private static GUIStyle _segmentButtonStyle;
        private static GUIStyle _selectedSegmentButtonStyle;
        private static GUIStyle _movementModeButtonStyle;
        private static GUIStyle _selectedMovementModeButtonStyle;
        private static GUIStyle _resetButtonStyle;

        // The open choice is UI-only; effect inspectors own the selected value.
        private static string _openDropdownKey;

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

        // Retain compact foldout widths, but center them inside Edit's
        // available content area rather than aligning with Target's column.
        internal static void SetSectionLayout(float targetWidth, float editInnerWidth)
        {
            _sectionWidth = Mathf.Max(1f, targetWidth * 0.8f);
            _sectionLeftOffset = Mathf.Max(0f,
                (editInnerWidth - _sectionWidth) * 0.5f);
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

                string caption = $"{marker}  {title}";
                if (_sectionWidth > 0f)
                {
                    GUIStyle style = _sectionWidth < 89f || title.Length > 10
                        ? _compactSectionButtonStyle : _sectionButtonStyle;

                    GUILayout.BeginHorizontal(GUIStyle.none);
                    GUILayout.Space(_sectionLeftOffset);
                    clicked = GUILayout.Button(caption, style,
                        GUILayout.Width(_sectionWidth), GUILayout.Height(19f));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
                else
                {
                    clicked = GUILayout.Button(caption, _sectionButtonStyle,
                        GUILayout.ExpandWidth(true), GUILayout.Height(19f));
                }
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

            // All inspector sections get the same breathing room before
            // the first control; changing sections never changes VFX state.
            if (expanded)
                GUILayout.Space(4f);
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

            NadaVfxEditor.ClaimTextInputClick(GUILayoutUtility.GetLastRect());

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

        // A compact, theme-aware single-choice menu for small fixed option sets.
        // No effect state or separate scroll view lives inside this control.
        internal static bool DropdownSelector(
            string key,
            string label,
            int selectedIndex,
            string[] options,
            out int nextSelectedIndex,
            int disabledOptionIndex = -1)
        {
            EnsureStyles();
            nextSelectedIndex = selectedIndex;
            if (options == null || options.Length == 0)
                return false;

            int safeIndex = Mathf.Clamp(selectedIndex, 0, options.Length - 1);
            bool open = _openDropdownKey == key;
            GUILayout.BeginHorizontal(_modifierRowStyle);
            GUILayout.Label(label, _sliderLabelStyle,
                GUILayout.Width(ModifierLabelWidth), GUILayout.Height(21f));
            GUILayout.Space(ComponentGap);
            if (GUILayout.Button((options[safeIndex] ?? string.Empty) +
                    (open ? " ▲" : " ▼"), _segmentButtonStyle,
                    GUILayout.Width(112f), GUILayout.Height(21f)))
            {
                open = !open;
                _openDropdownKey = open ? key : null;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (!open)
                return false;

            GUILayout.BeginHorizontal(_modifierRowStyle);
            GUILayout.Space(ModifierLabelWidth + ComponentGap);
            GUILayout.BeginVertical(GUILayout.Width(112f));
            bool changed = false;
            bool originalEnabled = GUI.enabled;
            for (int i = 0; i < options.Length; i++)
            {
                GUIStyle style = i == safeIndex
                    ? _selectedSegmentButtonStyle : _segmentButtonStyle;
                // Unsupported modes remain visible but cannot mutate state.
                GUI.enabled = originalEnabled && i != disabledOptionIndex;
                bool selected = GUILayout.Button(options[i] ?? string.Empty, style,
                    GUILayout.Width(112f), GUILayout.Height(20f));
                GUI.enabled = originalEnabled;
                if (!selected)
                    continue;

                _openDropdownKey = null;
                if (i != safeIndex)
                {
                    nextSelectedIndex = i;
                    changed = true;
                }
            }
            GUI.enabled = originalEnabled;
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            return changed;
        }

        // Compact, theme-aware movement choice shared by every orbital inspector.
        // The caller still owns the state change and Glue validation.
        internal static bool MovementModeSelector(
            string key,
            bool attached,
            out bool nextAttached,
            bool allowAttached = true)
        {
            EnsureStyles();
            nextAttached = attached;
            bool changed = false;
            bool previousEnabled = GUI.enabled;

            GUILayout.Space(3f);
            GUILayout.BeginHorizontal(_modifierRowStyle);
            GUILayout.FlexibleSpace();
            GUI.SetNextControlName($"NadaVfxMovement_{key}_Path");
            if (GUILayout.Button("Path",
                    attached ? _movementModeButtonStyle : _selectedMovementModeButtonStyle,
                    GUILayout.Width(96f), GUILayout.Height(24f)) && attached)
            {
                nextAttached = false;
                changed = true;
            }
            GUILayout.Space(10f);
            try
            {
                GUI.enabled = previousEnabled && allowAttached;
                GUI.SetNextControlName($"NadaVfxMovement_{key}_Attached");
                if (GUILayout.Button("Attached",
                        attached ? _selectedMovementModeButtonStyle : _movementModeButtonStyle,
                        GUILayout.Width(96f), GUILayout.Height(24f)) && !attached)
                {
                    nextAttached = true;
                    changed = true;
                }
            }
            finally
            {
                GUI.enabled = previousEnabled;
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            GUILayout.Space(3f);
            return changed;
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
            _openDropdownKey = null;

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

            NadaVfxEditor.ClaimTextInputClick(GUILayoutUtility.GetLastRect());

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

            Color32 subtleStroke = new Color32(
                theme.MutedText.r, theme.MutedText.g, theme.MutedText.b, 110);

            Color32 hoverStroke = new Color32(
                theme.MutedText.r, theme.MutedText.g, theme.MutedText.b, 175);

            // Section foldout frames should recede into the inspector,
            // unlike the brighter strokes used on actionable controls.
            Color32 sectionStroke = new Color32(
                theme.WindowBackground.r,
                theme.WindowBackground.g,
                theme.WindowBackground.b,
                255);

            Texture2D sectionBackground =
                CreateRoundedTexture(
                    theme.PanelBackground,
                    sectionStroke);

            Texture2D sectionHoverBackground =
                CreateRoundedTexture(
                    theme.SegmentBackground,
                    sectionStroke);

            Texture2D segmentBackground =
                CreateRoundedTexture(
                    theme.SegmentBackground,
                    subtleStroke);

            Texture2D segmentHoverBackground =
                CreateRoundedTexture(
                    theme.SegmentHoverBackground,
                    hoverStroke);

            Texture2D selectedSegmentBackground =
                CreateRoundedTexture(
                    theme.SelectedSegmentBackground);

            // Movement modes get their own stronger outlined surfaces.
            // Keep these separate from the ordinary small segments.
            Color32 movementIdleStroke = new Color32(
                theme.AccentSecondary.r,
                theme.AccentSecondary.g,
                theme.AccentSecondary.b,
                145);

            Color32 movementHoverStroke = new Color32(
                theme.AccentTertiary.r,
                theme.AccentTertiary.g,
                theme.AccentTertiary.b,
                215);

            Color32 movementSelectedStroke = new Color32(
                theme.AccentPrimary.r,
                theme.AccentPrimary.g,
                theme.AccentPrimary.b,
                255);

            Texture2D movementBackground =
                CreateRoundedTexture(
                    theme.SegmentBackground,
                    movementIdleStroke);

            Texture2D movementHoverBackground =
                CreateRoundedTexture(
                    theme.SegmentHoverBackground,
                    movementHoverStroke,
                    2);

            Texture2D movementSelectedBackground =
                CreateRoundedTexture(
                    theme.SelectedSegmentBackground,
                    movementSelectedStroke,
                    2);

            Texture2D resetBackground =
                CreateRoundedTexture(
                    theme.ResetBackground,
                    subtleStroke);

            Texture2D resetHoverBackground =
                CreateRoundedTexture(
                    theme.ResetHoverBackground,
                    hoverStroke);

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

            _sectionButtonStyle.border =
                new RectOffset(7, 7, 7, 7);

            _sectionButtonStyle.normal.background =
                sectionBackground;

            _sectionButtonStyle.hover.background =
                sectionHoverBackground;

            _sectionButtonStyle.active.background =
                sectionHoverBackground;

            // Desert's original orange-red caption was ~2:1 on sandstone.
            // Choose each text state against its real tab background, without
            // changing the theme's accent colors anywhere else.
            _sectionButtonStyle.normal.textColor =
                NadaVfxEditorContrast.TextOn(theme.PanelBackground,
                    theme.AccentSecondary, theme.AccentTertiary,
                    theme.PrimaryText, 5f);

            Color32 sectionHoverText = NadaVfxEditorContrast.TextOn(
                theme.SegmentBackground, theme.AccentTertiary,
                theme.AccentSecondary, theme.PrimaryText, 5f);
            _sectionButtonStyle.hover.textColor = sectionHoverText;
            _sectionButtonStyle.active.textColor = sectionHoverText;
            _sectionButtonStyle.alignment = TextAnchor.MiddleCenter;
            _sectionButtonStyle.clipping = TextClipping.Clip;

            _compactSectionButtonStyle = new GUIStyle(_sectionButtonStyle)
            {
                fontSize = 8,
                padding = new RectOffset(3, 3, 1, 1)
            };

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

            // Numeric editor fields share the same rounded geometry as the
            // shell inputs without modifying Unity's global GUI.skin.
            _valueFieldStyle.border =
                new RectOffset(7, 7, 7, 7);

            _valueFieldStyle.normal.background =
                CreateRoundedTexture(
                    theme.PickerBackground);

            _valueFieldStyle.hover.background =
                _valueFieldStyle.normal.background;

            _valueFieldStyle.focused.background =
                _valueFieldStyle.normal.background;

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

            _segmentButtonStyle.border =
                new RectOffset(7, 7, 7, 7);

            _segmentButtonStyle.normal.background =
                segmentBackground;

            _segmentButtonStyle.hover.background =
                segmentHoverBackground;

            _segmentButtonStyle.active.background =
                segmentHoverBackground;

            // Unselected mode labels still need to be readable on dark themes.
            _segmentButtonStyle.normal.textColor =
                Color.Lerp(
                    theme.MutedText,
                    theme.PrimaryText,
                    0.55f);

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

            // The large movement-mode buttons have their own geometry.
            // Their selected state uses a 2px accent stroke.
            _movementModeButtonStyle =
                new GUIStyle(
                    GUI.skin.button)
                {
                    fontSize = 10,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = false,
                    padding =
                        new RectOffset(
                            4,
                            4,
                            2,
                            2),
                    margin =
                        new RectOffset(
                            0,
                            0,
                            1,
                            1)
                };

            _movementModeButtonStyle.border =
                new RectOffset(7, 7, 7, 7);

            _movementModeButtonStyle.normal.background =
                movementBackground;

            _movementModeButtonStyle.hover.background =
                movementHoverBackground;

            _movementModeButtonStyle.active.background =
                movementHoverBackground;

            _movementModeButtonStyle.normal.textColor =
                theme.PrimaryText;

            _movementModeButtonStyle.hover.textColor =
                theme.AccentTertiary;

            _movementModeButtonStyle.active.textColor =
                theme.AccentTertiary;

            _selectedMovementModeButtonStyle =
                new GUIStyle(
                    _movementModeButtonStyle);

            _selectedMovementModeButtonStyle.normal.background =
                movementSelectedBackground;

            _selectedMovementModeButtonStyle.hover.background =
                movementSelectedBackground;

            _selectedMovementModeButtonStyle.active.background =
                movementSelectedBackground;

            _selectedMovementModeButtonStyle.normal.textColor =
                theme.AccentTertiary;

            _selectedMovementModeButtonStyle.hover.textColor =
                theme.AccentTertiary;

            _selectedMovementModeButtonStyle.active.textColor =
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

            _resetButtonStyle.border =
                new RectOffset(7, 7, 7, 7);

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
            _compactSectionButtonStyle = null;
            _modifierRowStyle = null;
            _sliderLabelStyle = null;
            _axisLabelStyle = null;
            _valueFieldStyle = null;
            _valueStyle = null;
            _placeholderStyle = null;
            _toggleStyle = null;
            _segmentButtonStyle = null;
            _selectedSegmentButtonStyle = null;
            _movementModeButtonStyle = null;
            _selectedMovementModeButtonStyle = null;
            _resetButtonStyle = null;

            _hasAppliedTheme =
                false;
        }

        private static Texture2D CreateRoundedTexture(
            Color32 color,
            Color32? outline = null,
            int strokeWidth = 1)
        {
            const int size = 20;
            const float radius = 6f;

            strokeWidth =
                Mathf.Clamp(strokeWidth, 1, 3);

            var texture =
                new Texture2D(
                    size,
                    size,
                    TextureFormat.RGBA32,
                    false)
                {
                    name = "NADA Editor Control Rounded UI",
                    hideFlags = HideFlags.HideAndDontSave,
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp
                };

            var pixels =
                new Color32[size * size];

            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float px = x + .5f;
                float py = y + .5f;

                float outer =
                    RoundedCoverage(
                        px,
                        py,
                        size,
                        radius);

                if (!outline.HasValue)
                {
                    pixels[y * size + x] =
                        new Color32(
                            color.r,
                            color.g,
                            color.b,
                            (byte)(color.a * outer));

                    continue;
                }

                float inner =
                    RoundedCoverage(
                        px - strokeWidth,
                        py - strokeWidth,
                        size - 2f * strokeWidth,
                        radius - strokeWidth);

                float rim =
                    Mathf.Clamp01(outer - inner);

                Color32 stroke =
                    outline.Value;

                float alpha =
                    inner * color.a +
                    rim * stroke.a;

                if (alpha <= 0f)
                    continue;

                float r =
                    (inner * color.r * color.a +
                     rim * stroke.r * stroke.a) / alpha;

                float g =
                    (inner * color.g * color.a +
                     rim * stroke.g * stroke.a) / alpha;

                float b =
                    (inner * color.b * color.a +
                     rim * stroke.b * stroke.a) / alpha;

                pixels[y * size + x] =
                    new Color32(
                        (byte)Mathf.Clamp(r, 0f, 255f),
                        (byte)Mathf.Clamp(g, 0f, 255f),
                        (byte)Mathf.Clamp(b, 0f, 255f),
                        (byte)Mathf.Clamp(alpha, 0f, 255f));
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);

            OwnedTextures.Add(texture);
            return texture;
        }

        private static float RoundedCoverage(
            float x,
            float y,
            float size,
            float radius)
        {
            float dx =
                Mathf.Max(radius - x, 0f);

            dx =
                Mathf.Max(
                    dx,
                    x - (size - radius));

            float dy =
                Mathf.Max(radius - y, 0f);

            dy =
                Mathf.Max(
                    dy,
                    y - (size - radius));

            return Mathf.Clamp01(
                radius + .5f -
                Mathf.Sqrt(dx * dx + dy * dy));
        }

        private static bool IsFinite(
            float value)
        {
            return
                !float.IsNaN(value) &&
                !float.IsInfinity(value);
        }

        private static string FormatFloat(
            float value,
            int decimals)
        {
            if (!IsFinite(value))
                return "0";

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
