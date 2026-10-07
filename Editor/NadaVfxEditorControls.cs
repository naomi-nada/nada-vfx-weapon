using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace NADA.VFX.Weapon.Editor
{
    /// <summary>
    /// Reusable IMGUI controls for the NADA VFX editor.
    ///
    /// This owns presentation and transient UI state only. It never resolves
    /// effect behavior or owns WeaponVfxState.
    /// </summary>
    internal static class NadaVfxEditorControls
    {
        private static readonly Dictionary<string, bool>
            SectionExpandedByKey =
                new();

        private static readonly Dictionary<string, string>
            NumericBufferByKey =
                new();

        private static readonly List<Texture2D>
            OwnedTextures =
                new();

        private static GUIStyle _sectionButtonStyle;
        private static GUIStyle _modifierRowStyle;
        private static GUIStyle _sliderLabelStyle;
        private static GUIStyle _valueFieldStyle;
        private static GUIStyle _valueStyle;
        private static GUIStyle _hintStyle;
        private static GUIStyle _placeholderStyle;
        private static GUIStyle _disabledReasonStyle;
        private static GUIStyle _toggleStyle;
        private static GUIStyle _segmentButtonStyle;
        private static GUIStyle _selectedSegmentButtonStyle;

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
                    ? "[-]"
                    : "[+]";

            if (GUILayout.Button(
                    $"{marker}  {title}",
                    _sectionButtonStyle,
                    GUILayout.ExpandWidth(true),
                    GUILayout.Height(25f)))
            {
                expanded =
                    !expanded;

                SectionExpandedByKey[
                    key] =
                    expanded;
            }

            return expanded;
        }

        internal static bool Toggle(
            string key,
            string label,
            bool value,
            out bool nextValue,
            bool enabled = true,
            string disabledReason = null)
        {
            EnsureStyles();

            nextValue =
                value;

            GUILayout.BeginVertical(
                _modifierRowStyle);

            bool previousEnabled =
                GUI.enabled;

            GUI.enabled =
                previousEnabled &&
                enabled;

            bool drawnValue =
                GUILayout.Toggle(
                    value,
                    label,
                    _toggleStyle);

            GUI.enabled =
                previousEnabled;

            bool changed =
                enabled &&
                drawnValue != value;

            if (changed)
            {
                nextValue =
                    drawnValue;
            }

            if (!enabled &&
                !string.IsNullOrWhiteSpace(
                    disabledReason))
            {
                GUILayout.Label(
                    disabledReason,
                    _disabledReasonStyle);
            }

            GUILayout.EndVertical();

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
            string disabledReason = null)
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

            GUILayout.BeginVertical(
                _modifierRowStyle);

            GUILayout.BeginHorizontal();

            GUILayout.Label(
                label,
                _sliderLabelStyle,
                GUILayout.Width(116f));

            bool previousEnabled =
                GUI.enabled;

            GUI.enabled =
                previousEnabled &&
                enabled;

            float sliderValue =
                GUILayout.HorizontalSlider(
                    safeSliderValue,
                    minimum,
                    maximum,
                    GUILayout.MinWidth(100f),
                    GUILayout.ExpandWidth(true));

            GUILayout.Space(
                7f);

            GUI.SetNextControlName(
                controlName);

            string typedValue =
                GUILayout.TextField(
                    numericBuffer ?? string.Empty,
                    14,
                    _valueFieldStyle,
                    GUILayout.Width(66f));

            GUI.enabled =
                previousEnabled;

            GUILayout.EndHorizontal();

            bool changed =
                false;

            if (enabled &&
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

            if (!enabled &&
                !string.IsNullOrWhiteSpace(
                    disabledReason))
            {
                GUILayout.Label(
                    disabledReason,
                    _disabledReasonStyle);
            }

            GUILayout.EndVertical();

            return changed;
        }

        internal static bool SegmentedSelector(
            string key,
            string label,
            int selectedIndex,
            string[] options,
            out int nextSelectedIndex,
            bool enabled = true,
            string disabledReason = null)
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

            GUILayout.BeginVertical(
                _modifierRowStyle);

            if (!string.IsNullOrWhiteSpace(
                    label))
            {
                GUILayout.Label(
                    label,
                    _sliderLabelStyle);
            }

            GUILayout.BeginHorizontal();

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
                        GUILayout.Height(24f)))
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

            if (!enabled &&
                !string.IsNullOrWhiteSpace(
                    disabledReason))
            {
                GUILayout.Label(
                    disabledReason,
                    _disabledReasonStyle);
            }

            GUILayout.EndVertical();

            return
                enabled &&
                changed;
        }

        internal static void ReadOnlyFloat(
            string label,
            float value)
        {
            EnsureStyles();

            GUILayout.BeginVertical(
                _modifierRowStyle);

            GUILayout.BeginHorizontal();

            GUILayout.Label(
                label,
                _sliderLabelStyle,
                GUILayout.Width(116f));

            GUILayout.Label(
                FormatFloat(
                    value,
                    3),
                _valueStyle);

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        internal static void ReadOnlyBool(
            string label,
            bool value)
        {
            EnsureStyles();

            GUILayout.BeginVertical(
                _modifierRowStyle);

            GUILayout.BeginHorizontal();

            GUILayout.Label(
                label,
                _sliderLabelStyle,
                GUILayout.Width(116f));

            GUILayout.Label(
                value
                    ? "On"
                    : "Off",
                _valueStyle);

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        internal static void ReadOnlyVector3(
            string label,
            float x,
            float y,
            float z)
        {
            EnsureStyles();

            GUILayout.BeginVertical(
                _modifierRowStyle);

            GUILayout.BeginHorizontal();

            GUILayout.Label(
                label,
                _sliderLabelStyle,
                GUILayout.Width(116f));

            GUILayout.Label(
                $"X {x:0.###}   Y {y:0.###}   Z {z:0.###}",
                _valueStyle);

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        internal static void Hint(
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
                _hintStyle);
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
                7f);
        }

        internal static void ResetUiState()
        {
            SectionExpandedByKey.Clear();
            NumericBufferByKey.Clear();

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
            _valueFieldStyle = null;
            _valueStyle = null;
            _hintStyle = null;
            _placeholderStyle = null;
            _disabledReasonStyle = null;
            _toggleStyle = null;
            _segmentButtonStyle = null;
            _selectedSegmentButtonStyle = null;
        }

        private static void EnsureStyles()
        {
            if (_sectionButtonStyle != null)
                return;

            Texture2D sectionBackground =
                CreateTexture(
                    new Color(
                        0.20f,
                        0.16f,
                        0.105f,
                        1f));

            Texture2D sectionHoverBackground =
                CreateTexture(
                    new Color(
                        0.265f,
                        0.205f,
                        0.12f,
                        1f));

            Texture2D modifierBackground =
                CreateTexture(
                    new Color(
                        0.095f,
                        0.083f,
                        0.068f,
                        0.72f));

            Texture2D segmentBackground =
                CreateTexture(
                    new Color(
                        0.145f,
                        0.125f,
                        0.10f,
                        1f));

            Texture2D segmentHoverBackground =
                CreateTexture(
                    new Color(
                        0.22f,
                        0.18f,
                        0.12f,
                        1f));

            Texture2D selectedSegmentBackground =
                CreateTexture(
                    new Color(
                        0.43f,
                        0.315f,
                        0.15f,
                        1f));

            Color primaryText =
                new Color(
                    0.93f,
                    0.90f,
                    0.82f,
                    1f);

            Color mutedText =
                new Color(
                    0.63f,
                    0.60f,
                    0.53f,
                    1f);

            Color accentText =
                new Color(
                    0.91f,
                    0.70f,
                    0.34f,
                    1f);

            Color disabledText =
                new Color(
                    0.50f,
                    0.47f,
                    0.42f,
                    1f);

            _sectionButtonStyle =
                new GUIStyle(
                    GUI.skin.button)
                {
                    fontSize = 10,
                    padding =
                        new RectOffset(
                            8,
                            8,
                            4,
                            4)
                };

            _sectionButtonStyle.normal.background =
                sectionBackground;

            _sectionButtonStyle.hover.background =
                sectionHoverBackground;

            _sectionButtonStyle.active.background =
                sectionHoverBackground;

            _sectionButtonStyle.normal.textColor =
                accentText;

            _sectionButtonStyle.hover.textColor =
                primaryText;

            _modifierRowStyle =
                new GUIStyle(
                    GUI.skin.box)
                {
                    padding =
                        new RectOffset(
                            7,
                            7,
                            5,
                            5),
                    margin =
                        new RectOffset(
                            0,
                            0,
                            2,
                            2)
                };

            _modifierRowStyle.normal.background =
                modifierBackground;

            _sliderLabelStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    fontSize = 10,
                    wordWrap = false
                };

            _sliderLabelStyle.normal.textColor =
                primaryText;

            _valueFieldStyle =
                new GUIStyle(
                    GUI.skin.textField)
                {
                    fontSize = 10,
                    padding =
                        new RectOffset(
                            5,
                            5,
                            3,
                            3)
                };

            _valueFieldStyle.normal.textColor =
                primaryText;

            _valueStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    fontSize = 10,
                    wordWrap = false
                };

            _valueStyle.normal.textColor =
                primaryText;

            _hintStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    fontSize = 9,
                    wordWrap = true,
                    padding =
                        new RectOffset(
                            4,
                            4,
                            2,
                            2)
                };

            _hintStyle.normal.textColor =
                mutedText;

            _placeholderStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    fontSize = 10,
                    wordWrap = true,
                    padding =
                        new RectOffset(
                            5,
                            5,
                            4,
                            4)
                };

            _placeholderStyle.normal.textColor =
                mutedText;

            _disabledReasonStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    fontSize = 9,
                    wordWrap = true,
                    padding =
                        new RectOffset(
                            116,
                            4,
                            0,
                            1)
                };

            _disabledReasonStyle.normal.textColor =
                disabledText;

            _toggleStyle =
                new GUIStyle(
                    GUI.skin.toggle)
                {
                    fontSize = 10,
                    padding =
                        new RectOffset(
                            18,
                            4,
                            2,
                            2)
                };

            _toggleStyle.normal.textColor =
                primaryText;

            _segmentButtonStyle =
                new GUIStyle(
                    GUI.skin.button)
                {
                    fontSize = 10,
                    padding =
                        new RectOffset(
                            6,
                            6,
                            3,
                            3)
                };

            _segmentButtonStyle.normal.background =
                segmentBackground;

            _segmentButtonStyle.hover.background =
                segmentHoverBackground;

            _segmentButtonStyle.active.background =
                segmentHoverBackground;

            _segmentButtonStyle.normal.textColor =
                primaryText;

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
                primaryText;
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