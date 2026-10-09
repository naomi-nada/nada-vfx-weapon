using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NADA.VFX.Weapon.Core.Config;
using NADA.VFX.Weapon.Core.Persistence;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Editor.Inspectors;
using NADA.VFX.Weapon.Weapons.Runtime;
using UnityEngine;

namespace NADA.VFX.Weapon.Editor
{
    internal static class NadaVfxEditor
    {
        private const int WindowId = 0x4E414441;
        private const int ResizeControlIdHint = 0x4E565258;

        private const string RenameControlName =
            "NadaVfxEditorRename";

        private const string NumericControlPrefix =
            "NadaVfxFloat_";

        private const float DefaultWindowWidth = 660f;
        private const float DefaultWindowHeight = 720f;

        private const float MinimumWindowWidth = 360f;
        private const float MinimumWindowHeight = 360f;

        private const float CompactLayoutBreakpoint = 500f;

        private const float BlockHeight = 26f;
        private const float EffectChipWidth = 148f;
        private const float EffectChipGap = 4f;
        private const float EffectActionHeight = 23f;
        private const float EffectToggleWidth = 34f;

        private const float PickerButtonWidth = 88f;
        private const float PickerButtonGap = 3f;

        private const float ResizeEdgeThickness = 8f;
        private const float EffectDragThreshold = 5f;

        private const string WandResourceName =
            "NADA.VFX.Weapon.Editor.Resources.nada-vfx-wand.alpha";

        private const string BrandResourceName =
            "NADA.VFX.Weapon.Editor.Resources.nada-vfx-wordmark.alpha";

        [Flags]
        private enum ResizeEdges
        {
            None = 0,
            Left = 1,
            Right = 2,
            Top = 4,
            Bottom = 8
        }

        private static readonly List<Texture2D> OwnedTextures =
            new();

        private static Rect _windowRect =
            new Rect(
                0f,
                72f,
                DefaultWindowWidth,
                DefaultWindowHeight);

        private static Vector2 _inspectorScrollPosition;

        private static uint? _selectedInstanceId;
        private static uint? _renameInstanceId;
        private static uint? _resetConfirmInstanceId;
        private static uint? _deleteConfirmInstanceId;

        private static string _renameBuffer =
            string.Empty;

        private static bool _open;
        private static bool _addPickerOpen;
        private static bool _positionInitialized;
        private static bool _targetDetailsExpanded;

        private static bool _ownsKeyboardInput;
        private static bool _focusRenameOnNextDraw;

        private static bool _cursorStateCaptured;
        private static bool _previousCursorVisible;
        private static CursorLockMode _previousCursorLockMode;
        private static float _cursorWindowStartTime;
        private static int _cursorCorrectionsInWindow;

        private static bool _resizing;
        private static ResizeEdges _activeResizeEdges;
        private static Vector2 _resizeStartMouseScreenPosition;
        private static Rect _resizeStartRect;

        private static uint? _dragCandidateInstanceId;
        private static uint? _pendingReorderTargetInstanceId;

        private static bool _effectDragActive;

        private static Vector2 _effectDragStartMousePosition;

        private static float _nextTargetRefreshTime;

        private static Texture2D _brandTitleTexture;
        private static bool _brandLoadAttempted;
        private static Texture2D _wandTexture;
        private static bool _settingsOpen;
        private static bool _clearAllConfirm;
        private static bool _pendingHandSwitch;
        private static bool _remotePlayerListExpanded;

        private enum ShortcutCapture { None, Toggle, Bind, Unbind }
        private static ShortcutCapture _shortcutCapture;
        private static string _shortcutError;

        private enum BindingAction { None, Bind, Unbind }
        private static BindingAction _pendingBindingAction;
        private static string _bindingError;

        private static NadaVfxEditorThemePreset
            _appliedThemePreset;

        private static bool
            _hasAppliedTheme;

        private static GUIStyle _windowStyle;

        private static GUIStyle _brandSubtitleStyle;

        private static GUIStyle _smallHeaderStyle;
        private static GUIStyle _targetCardStyle;
        private static GUIStyle _targetTitleStyle;
        private static GUIStyle _targetStatusStyle;
        private static GUIStyle _panelStyle;
        private static GUIStyle _sectionHeaderStyle;
        private static GUIStyle _settingsSubHeaderStyle;
        private static GUIStyle _blockStyle;
        private static GUIStyle _selectedBlockStyle;
        private static GUIStyle _blockLabelStyle;
        private static GUIStyle _disabledBlockLabelStyle;
        private static GUIStyle _inspectorTitleStyle;
        private static GUIStyle _inspectorAccentStyle;
        private static GUIStyle _bodyStyle;
        private static GUIStyle _mutedStyle;
        private static GUIStyle _warningStyle;
        private static GUIStyle _closeButtonStyle;
        private static GUIStyle _settingsButtonStyle;
        private static GUIStyle _stylesButtonStyle;
        private static GUIStyle _styleTextFieldStyle;
        private static GUIStyle _styleTextAreaStyle;
        private static GUIStyle _themeOptionStyle;
        private static GUIStyle _addButtonStyle;
        private static GUIStyle _pickerStyle;
        private static GUIStyle _pickerButtonStyle;
        private static GUIStyle _readOnlyBoxStyle;
        private static GUIStyle _detailsButtonStyle;
        private static GUIStyle _headerActionButtonStyle;
        private static GUIStyle _effectOnStyle;
        private static GUIStyle _effectOffStyle;

        internal static bool IsOpen =>
            _open;

        internal static bool OwnsKeyboardInput =>
            _open &&
            (_ownsKeyboardInput || _renameInstanceId.HasValue ||
             _shortcutCapture != ShortcutCapture.None);

        // Runtime input patches use this instead of opening normal
        // PlayerController input, which would also enable combat actions.
        internal static bool AllowsMovementWhileEditing =>
            _open &&
            !OwnsKeyboardInput &&
            NadaVfxEditorConfig.AllowMovementWhileEditing?.Value == true;

        private static bool IsCompactLayout =>
            _windowRect.width <
            CompactLayoutBreakpoint;

        internal static void Tick()
        {
            if (NadaVfxEditorConfig.EditingEnabled == null ||
                NadaVfxEditorConfig.ToggleHotkey == null)
            {
                return;
            }

            if (!NadaVfxEditorConfig.EditingEnabled.Value)
            {
                if (_open)
                    Close();

                return;
            }

            if (_shortcutCapture == ShortcutCapture.None &&
                NadaVfxEditorConfig.ToggleHotkey.Value.IsDown())
            {
                if (_open)
                    Close();
                else
                    Open();
            }

            if (!_open)
                return;

            // Hotkeys share the button's deferred, target-validated action.
            // Never fire while entering text or capturing a new shortcut.
            if (!OwnsKeyboardInput)
            {
                if (NadaVfxEditorConfig.BindHotkey != null &&
                    NadaVfxEditorConfig.BindHotkey.Value.MainKey != KeyCode.None &&
                    NadaVfxEditorConfig.BindHotkey.Value.IsDown() &&
                    !NadaVfxEditorWorkingState.IsReadOnly)
                {
                    _pendingBindingAction = BindingAction.Bind;
                }
                else if (NadaVfxEditorConfig.UnbindHotkey != null &&
                         NadaVfxEditorConfig.UnbindHotkey.Value.MainKey != KeyCode.None &&
                         NadaVfxEditorConfig.UnbindHotkey.Value.IsDown() &&
                         NadaVfxEditorWorkingState.IsReadOnly)
                {
                    _pendingBindingAction = BindingAction.Unbind;
                }
            }

            EnforceCursorOwnership();

            if (Time.unscaledTime >=
                _nextTargetRefreshTime)
            {
                _nextTargetRefreshTime =
                    Time.unscaledTime +
                    1f;

                NadaVfxEditorTargetRegistry
                    .RefreshFromRuntime();
            }
        }

        internal static void Draw()
        {
            if (!_open ||
                NadaVfxEditorConfig.EditingEnabled == null ||
                !NadaVfxEditorConfig.EditingEnabled.Value)
            {
                return;
            }

            EnsureStyles();
            EnsureWindowPosition();
            ClampWindowToScreen();

            _windowRect =
                GUI.Window(
                    WindowId,
                    _windowRect,
                    DrawWindow,
                    GUIContent.none,
                    _windowStyle);

            NadaVfxEditorStylesPanel.ProcessPendingDraftApply();
            ProcessPendingBindingAction();
            if (_pendingHandSwitch)
            {
                _pendingHandSwitch = false;
                NadaVfxEditorTargetRegistry.SwitchHands();
                // SynchronizeTarget/preview transition on the next GUI draw;
                // changing the target inside GUI.Window risks IMGUI layout errors.
            }
        }

        internal static void EnforceCursorOwnership()
        {
            if (!_open)
                return;

            bool visibilityOverridden = !Cursor.visible;
            CursorLockMode previousLock = Cursor.lockState;
            bool lockOverridden = previousLock != CursorLockMode.None;
            if (!visibilityOverridden && !lockOverridden)
                return;

            if (lockOverridden)
                Cursor.lockState = CursorLockMode.None;
            if (visibilityOverridden)
                Cursor.visible = true;

            // Repeated corrections indicate another UI path is fighting for
            // cursor ownership. Sample only once per diagnostic window.
            _cursorCorrectionsInWindow++;
            if (Time.unscaledTime - _cursorWindowStartTime < 3f)
                return;

            if (_cursorCorrectionsInWindow >= 5)
            {
                Plugin.Log?.LogWarning(
                    $"{Plugin.ModName}: [EditorCursorOwnershipConflict] " +
                    $"corrections3s={_cursorCorrectionsInWindow} " +
                    $"visibilityOverridden={visibilityOverridden} " +
                    $"previousLock={previousLock}");
            }

            _cursorWindowStartTime = Time.unscaledTime;
            _cursorCorrectionsInWindow = 0;
        }

        internal static bool IsPointerOverWindow()
        {
            if (!_open)
                return false;

            Vector3 mousePosition =
                Input.mousePosition;

            Vector2 guiPosition =
                new Vector2(
                    mousePosition.x,
                    Screen.height -
                    mousePosition.y);

            return
                _windowRect.Contains(
                    guiPosition);
        }

        internal static void Close()
        {
            if (!_open)
                return;

            SaveWindowBounds();

            _open =
                false;

            _ownsKeyboardInput =
                false;

            _addPickerOpen =
                false;
            _settingsOpen = false;
            _clearAllConfirm = false;
            _pendingHandSwitch = false;
            _shortcutCapture = ShortcutCapture.None;
            _shortcutError = null;
            NadaVfxEditorStylesPanel.Close();
            _pendingBindingAction = BindingAction.None;
            _bindingError = null;

            if (_resizing &&
                GUIUtility.hotControl != 0)
            {
                GUIUtility.hotControl =
                    0;
            }

            _resizing =
                false;

            _activeResizeEdges =
                ResizeEdges.None;

            ClearEffectDrag();
            ClearTransientBlockActions();

            NadaWeaponEditorPreviewState
                .Deactivate();

            NadaVfxEditorPreviewRigSession
                .ReleaseOwnedRig();

            Plugin.Instance?
                .RefreshExistingEquippedRigsOnly();

            RestoreCursor();

            Plugin.Log?.LogInfo(
                $"{Plugin.ModName}: [Editor] closed.");
        }

        internal static void Shutdown()
        {
            Close();

            ResetVisualStyles();

            NadaVfxEditorControls
                .ResetUiState();

            NadaVfxEditorWorkingState
                .ClearDrafts();
        }

        private static void Open()
        {
            if (_open)
                return;

            CaptureCursor();
            _cursorWindowStartTime = Time.unscaledTime;
            _cursorCorrectionsInWindow = 0;

            _open =
                true;

            _ownsKeyboardInput =
                false;

            EnforceCursorOwnership();

            NadaVfxEditorTargetRegistry
                .RefreshFromRuntime();

            NadaVfxEditorTarget target =
                NadaVfxEditorTargetRegistry.Current;

            bool targetChanged =
                NadaVfxEditorWorkingState
                    .SynchronizeTarget(
                        target);

            if (targetChanged)
            {
                _bindingError = null;
                _selectedInstanceId =
                    null;

                _addPickerOpen =
                    false;

                _targetDetailsExpanded =
                    false;
                _clearAllConfirm = false;
                NadaVfxEditorStylesPanel.OnTargetChanged();

                ClearEffectDrag();
                ClearTransientBlockActions();
            }

            ActivatePreview(
                target);

            _nextTargetRefreshTime =
                Time.unscaledTime +
                1f;

            Plugin.Log?.LogInfo(
                $"{Plugin.ModName}: [Editor] opened.");
        }

        private static void DrawWindow(
            int windowId)
        {
            // Capture before any focused text field can consume KeyDown.
            CaptureEditorShortcut();
            DrawTitleBar();

            GUILayout.Space(
                4f);

            NadaVfxEditorTarget target =
                NadaVfxEditorTargetRegistry.Current;

            bool targetChanged =
                NadaVfxEditorWorkingState
                    .SynchronizeTarget(
                        target);

            if (targetChanged)
            {
                _bindingError = null;
                _selectedInstanceId =
                    null;

                _addPickerOpen =
                    false;

                _targetDetailsExpanded =
                    false;
                _clearAllConfirm = false;
                NadaVfxEditorStylesPanel.OnTargetChanged();

                _inspectorScrollPosition =
                    Vector2.zero;

                ClearEffectDrag();
                ClearTransientBlockActions();

                ActivatePreview(
                    target);
            }

            if (NadaVfxEditorStylesPanel.IsOpen)
                NadaVfxEditorStylesPanel.Draw(target,
                    _targetCardStyle, _sectionHeaderStyle, _bodyStyle,
                    _mutedStyle, _warningStyle, _addButtonStyle,
                    _themeOptionStyle, _styleTextFieldStyle, _styleTextAreaStyle);

            if (_settingsOpen)
                DrawEditorSettings();

            DrawTargetCard(target);

            GUILayout.Space(
                5f);

            EnsureSelection();

            DrawEffectsStrip(
                target);

            GUILayout.Space(
                5f);

            DrawInspectorPanel();

            DrawResizeHandles();

            UpdateKeyboardInputOwnership();

            if (Event.current.rawType ==
                EventType.MouseUp)
            {
                ClearEffectDrag();
            }

            if (!_resizing)
            {
                GUI.DragWindow(
                    new Rect(
                        0f,
                        ResizeEdgeThickness,
                        _windowRect.width - 34f,
                        38f - ResizeEdgeThickness));
            }
        }

        private static void ProcessPendingBindingAction()
        {
            BindingAction requested = _pendingBindingAction;
            _pendingBindingAction = BindingAction.None;
            if (requested == BindingAction.None)
                return;

            // Re-resolve at the user's click. A one-second registry snapshot
            // may no longer describe the currently equipped weapon.
            global::ItemDrop.ItemData clickedItem =
                NadaVfxEditorTargetRegistry.Current?.ItemData;
            NadaVfxEditorTargetRegistry.RefreshFromRuntime();
            NadaVfxEditorTarget liveTarget = NadaVfxEditorTargetRegistry.Current;
            if (clickedItem == null ||
                !object.ReferenceEquals(clickedItem, liveTarget?.ItemData))
            {
                _bindingError = "Equipped weapon changed. Try again.";
                return;
            }

            bool success = requested == BindingAction.Bind
                ? NadaVfxEditorBindingController.TryBind(liveTarget, out _bindingError)
                : NadaVfxEditorBindingController.TryUnbind(liveTarget, out _bindingError);
            if (!success)
            {
                Plugin.Log?.LogWarning(
                    $"{Plugin.ModName}: [EditorBindingRejected] " +
                    $"action={requested} reason='{_bindingError}'.");
                return;
            }

            _bindingError = null;
            if (global::MessageHud.instance != null)
            {
                global::MessageHud.instance.ShowMessage(
                    global::MessageHud.MessageType.Center,
                    requested == BindingAction.Bind
                        ? "Weapon VFX bound"
                        : "Weapon VFX unbound");
            }
            _addPickerOpen = false;
            _clearAllConfirm = false;
            NadaVfxEditorStylesPanel.OnTargetChanged();
            ClearEffectDrag();
            ClearTransientBlockActions();

            if (requested == BindingAction.Bind)
            {
                NadaWeaponEditorPreviewState.Deactivate();
                NadaVfxEditorPreviewRigSession.ReleaseOwnedRig();
            }

            NadaVfxEditorTargetRegistry.RefreshFromRuntime();
            NadaVfxEditorTarget refreshed = NadaVfxEditorTargetRegistry.Current;
            NadaVfxEditorWorkingState.SynchronizeTarget(refreshed);
            if (requested == BindingAction.Unbind)
                ActivatePreview(refreshed);

            Plugin.Instance?.RefreshExistingEquippedRigsOnly();
        }

        private static void DrawTitleBar()
        {
            EnsureBrandTexture();

            GUILayout.BeginHorizontal(
                GUILayout.Height(34f));

            const float titleHeight = 29f;
            const float titleWidth = 154f;
            const float wandSize = 18f;

            Rect titleRect =
                GUILayoutUtility.GetRect(
                    titleWidth,
                    titleHeight,
                    GUILayout.Width(titleWidth),
                    GUILayout.Height(titleHeight));

            if (_brandTitleTexture != null)
            {
                NadaVfxEditorTheme theme =
                    NadaVfxEditorThemes.Current;

                Color previousColor = GUI.color;

                try
                {
                    GUI.color = theme.TitleShadow;
                    GUI.DrawTexture(
                        new Rect(
                            titleRect.x + 1f,
                            titleRect.y + 1f,
                            titleRect.width,
                            titleRect.height),
                        _brandTitleTexture,
                        ScaleMode.ScaleToFit,
                        true);

                    GUI.color = theme.TitlePrimary;
                    GUI.DrawTexture(
                        titleRect,
                        _brandTitleTexture,
                        ScaleMode.ScaleToFit,
                        true);
                }
                finally
                {
                    GUI.color = previousColor;
                }
            }
            else
            {
                GUI.Label(
                    titleRect,
                    "nada vfx",
                    _targetTitleStyle);
            }

            GUILayout.Space(4f);

            if (_wandTexture != null)
            {
                Rect wandSlot = GUILayoutUtility.GetRect(
                    wandSize,
                    titleHeight,
                    GUILayout.Width(wandSize),
                    GUILayout.Height(titleHeight));

                Rect wandRect = new Rect(
                    wandSlot.x,
                    titleRect.y + Mathf.Floor((titleRect.height - wandSize) * 0.5f) + 1f,
                    wandSize,
                    wandSize);

                NadaVfxEditorTheme theme = NadaVfxEditorThemes.Current;
                Color previousColor = GUI.color;
                try
                {
                    GUI.color = theme.TitlePrimary;
                    GUI.DrawTexture(
                        wandRect,
                        _wandTexture,
                        ScaleMode.ScaleToFit,
                        true);
                }
                finally
                {
                    GUI.color = previousColor;
                }
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Styles",
                    NadaVfxEditorStylesPanel.IsOpen ? _stylesButtonStyle : _settingsButtonStyle,
                    GUILayout.Width(70f), GUILayout.Height(20f)))
            {
                NadaVfxEditorStylesPanel.Toggle();
                if (NadaVfxEditorStylesPanel.IsOpen)
                {
                    _settingsOpen = false;
                    _themeDropdownOpen = false;
                }
            }
            GUILayout.Space(4f);

            if (GUILayout.Button("Settings",
                    _settingsOpen ? _stylesButtonStyle : _settingsButtonStyle,
                    GUILayout.Width(84f), GUILayout.Height(20f)))
            {
                _settingsOpen = !_settingsOpen;
                if (_settingsOpen && NadaVfxEditorStylesPanel.IsOpen)
                    NadaVfxEditorStylesPanel.Toggle();
                if (!_settingsOpen)
                    _themeDropdownOpen = false;
            }

            if (GUILayout.Button(
                    "X",
                    _closeButtonStyle,
                    GUILayout.Width(22f),
                    GUILayout.Height(20f)))
            {
                Close();
                GUIUtility.ExitGUI();
            }

            GUILayout.EndHorizontal();
        }

        private static void DrawTargetCard(
            NadaVfxEditorTarget target)
        {
            GUILayout.BeginVertical(
                _targetCardStyle);

            GUILayout.BeginHorizontal();

            GUILayout.Label(
                "TARGET",
                _sectionHeaderStyle,
                GUILayout.Width(46f));

            if (target == null)
            {
                GUILayout.Label(
                    "NO EDITABLE TARGET",
                    _targetTitleStyle);

                GUILayout.FlexibleSpace();
                if (!IsCompactLayout)
                    DrawSwitchHandsButton();

                GUILayout.EndHorizontal();
                if (IsCompactLayout)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    DrawSwitchHandsButton();
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();

                return;
            }

            string title =
                string.IsNullOrWhiteSpace(
                    target.DisplayName)
                    ? target.PrefabName
                    : target.DisplayName;

            // Reserve space for the status and Details before measuring the name.
            // A short name never gets a large invisible layout slot.
            float availableNameWidth =
                Mathf.Max(24f, _windowRect.width -
                    (IsCompactLayout ? 185f : 304f));

            float measuredNameWidth =
                _targetTitleStyle.CalcSize(
                    new GUIContent(title)).x;

            float targetNameWidth =
                Mathf.Min(measuredNameWidth, availableNameWidth);

            GUILayout.Label(
                title,
                _targetTitleStyle,
                GUILayout.Width(targetNameWidth),
                GUILayout.Height(18f));

            GUILayout.Space(
                6f);

            GUILayout.Label(
                target.SourceKind == NadaWeaponLocalSourceKind.InvalidNative
                    ? "Invalid"
                    : target.IsReadOnly
                        ? "Bound"
                        : "Unbound",
                _targetStatusStyle,
                GUILayout.Width(48f),
                GUILayout.Height(18f));

            GUILayout.FlexibleSpace();

            if (!IsCompactLayout)
            {
                DrawSwitchHandsButton();
                GUILayout.Space(4f);
            }

            if (GUILayout.Button(
                    _targetDetailsExpanded
                        ? "Hide"
                        : "Details",
                    _detailsButtonStyle,
                    GUILayout.Width(52f),
                    GUILayout.Height(18f)))
            {
                _targetDetailsExpanded =
                    !_targetDetailsExpanded;
            }

            GUILayout.EndHorizontal();
            if (IsCompactLayout)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                DrawSwitchHandsButton();
                GUILayout.EndHorizontal();
            }

            if (_targetDetailsExpanded)
            {
                GUILayout.Space(
                    4f);

                DrawSingleInfoRow(
                    "Prefab ID",
                    string.IsNullOrWhiteSpace(
                        target.PrefabName)
                        ? "Unknown"
                        : target.PrefabName);

                DrawSingleInfoRow(
                    "Visual Root",
                    string.IsNullOrWhiteSpace(target.VisualDisplayName)
                        ? "Unknown"
                        : target.VisualDisplayName);

                DrawSingleInfoRow("Hand", NadaVfxEditorTargetRegistry.HandLabel);
            }

            GUILayout.EndVertical();
        }

        private static void DrawSwitchHandsButton()
        {
            if (GUILayout.Button("Switch Hands", _detailsButtonStyle,
                    GUILayout.Width(94f), GUILayout.Height(18f)))
                _pendingHandSwitch = true;
        }

        private static void DrawClearAllConfirmation()
        {
            WeaponVfxState state = NadaVfxEditorWorkingState.State;
            int count = state?.Effects?.Count ?? 0;
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Clear all {count} effects?", _warningStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Confirm", _addButtonStyle,
                    GUILayout.Width(76f), GUILayout.Height(22f)))
            {
                // Use the draft mutation API so runtime reconciliation sees
                // the same change as deleting blocks individually.
                var ids = new List<uint>();
                if (state?.Effects != null)
                    foreach (VfxEffectBlock block in state.Effects)
                        if (block != null)
                            ids.Add(block.InstanceId);

                bool changed = false;
                foreach (uint id in ids)
                    if (NadaVfxEditorWorkingState.TryDeleteEffect(id, out uint? _))
                        changed = true;
                _clearAllConfirm = false;
                if (changed)
                {
                    _selectedInstanceId = null;
                    ClearTransientBlockActions();
                    ClearEffectDrag();
                    RefreshPreview();
                }
            }
            if (GUILayout.Button("Cancel", _addButtonStyle,
                    GUILayout.Width(60f), GUILayout.Height(22f)))
                _clearAllConfirm = false;
            GUILayout.EndHorizontal();
        }

        // Called by the panel after WorkingState atomically replaces a draft.
        internal static void OnStylesDraftReplaced()
        {
            _selectedInstanceId = null;
            _inspectorScrollPosition = Vector2.zero;
            _clearAllConfirm = false;
            ClearTransientBlockActions();
            ClearEffectDrag();
            RefreshPreview();
        }

        private static bool _themeDropdownOpen;

        private static void DrawEditorSettings()
        {
            GUILayout.BeginVertical(_targetCardStyle);

            GUILayout.Label("SETTINGS", _sectionHeaderStyle);

            GUILayout.Space(3f);
            GUILayout.BeginHorizontal();

            GUILayout.Label(
                "Theme",
                _bodyStyle,
                GUILayout.Width(52f),
                GUILayout.Height(21f));

            if (NadaVfxEditorConfig.Theme != null)
            {
                NadaVfxEditorThemePreset current =
                    NadaVfxEditorThemes.CurrentPreset;

                if (GUILayout.Button(
                        _themeDropdownOpen
                            ? "Select Theme"
                            : "Select Theme",
                        _themeOptionStyle,
                        GUILayout.Width(116f),
                        GUILayout.Height(21f)))
                {
                    _themeDropdownOpen = !_themeDropdownOpen;
                }

                GUILayout.Space(5f);
                GUILayout.Label(
                    "Current: " + NadaVfxEditorThemes.GetDisplayName(current),
                    _mutedStyle,
                    GUILayout.Height(21f));
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (_themeDropdownOpen &&
                NadaVfxEditorConfig.Theme != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(52f);
                GUILayout.BeginVertical(GUILayout.Width(156f));

                foreach (NadaVfxEditorThemePreset preset in
                         (NadaVfxEditorThemePreset[])Enum.GetValues(
                             typeof(NadaVfxEditorThemePreset)))
                {
                    if (GUILayout.Button(
                            NadaVfxEditorThemes.GetDisplayName(preset),
                            _themeOptionStyle,
                            GUILayout.Height(20f)))
                    {
                        NadaVfxEditorConfig.Theme.Value = preset;
                        _themeDropdownOpen = false;
                    }
                }

                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(7f);
            GUILayout.Label("VISIBILITY", _settingsSubHeaderStyle);
            GUILayout.Space(2f);

            DrawVisibilitySetting(
                "Character Selection",
                PluginConfig.CharacterSelectionVisibility);

            DrawVisibilitySetting(
                "Dropped Items",
                PluginConfig.DroppedItemVisibility);

            DrawVisibilitySetting(
                "Multiplayer",
                NadaVfxEditorConfig.MultiplayerVisibility);

            if (NadaVfxEditorConfig.MultiplayerVisibility?.Value == true)
                DrawRemotePlayerVisibility();

            GUILayout.Space(7f);
            GUILayout.Label("CONTROLS", _settingsSubHeaderStyle);
            GUILayout.Space(2f);

            DrawVisibilitySetting(
                "Allow Movement",
                NadaVfxEditorConfig.AllowMovementWhileEditing);

            GUILayout.Space(3f);
            DrawShortcutSetting("Open / Close Editor", NadaVfxEditorConfig.ToggleHotkey,
                ShortcutCapture.Toggle);
            DrawShortcutSetting("Bind to Weapon", NadaVfxEditorConfig.BindHotkey,
                ShortcutCapture.Bind);
            DrawShortcutSetting("Unbind Weapon", NadaVfxEditorConfig.UnbindHotkey,
                ShortcutCapture.Unbind);
            if (!string.IsNullOrEmpty(_shortcutError))
                GUILayout.Label(_shortcutError, _warningStyle);

            GUILayout.EndVertical();
        }

        private static void DrawShortcutSetting(
            string label,
            BepInEx.Configuration.ConfigEntry<BepInEx.Configuration.KeyboardShortcut> setting,
            ShortcutCapture capture)
        {
            if (setting == null)
                return;

            GUILayout.BeginHorizontal(GUILayout.Height(22f));
            GUILayout.Label(label, _bodyStyle,
                GUILayout.Width(118f), GUILayout.Height(20f));
            GUILayout.Space(6f);

            bool recording = _shortcutCapture == capture;
            string current = setting.Value.MainKey == KeyCode.None
                ? "Set Key"
                : setting.Value.ToString();
            if (GUILayout.Button(recording ? "Press a key..." : current,
                    _themeOptionStyle,
                    GUILayout.Width(126f), GUILayout.Height(20f)))
            {
                _shortcutCapture = recording ? ShortcutCapture.None : capture;
                _shortcutError = null;
            }

            GUILayout.Space(4f);
            bool enabled = GUI.enabled;
            bool isToggle = capture == ShortcutCapture.Toggle;
            GUI.enabled = enabled &&
                (isToggle || setting.Value.MainKey != KeyCode.None);
            if (GUILayout.Button(isToggle ? "Reset" : "Clear", _themeOptionStyle,
                    GUILayout.Width(48f), GUILayout.Height(20f)))
            {
                if (isToggle)
                {
                    var defaultKey = new BepInEx.Configuration.KeyboardShortcut(KeyCode.F6);
                    if (MatchesShortcut(defaultKey, NadaVfxEditorConfig.BindHotkey?.Value) ||
                        MatchesShortcut(defaultKey, NadaVfxEditorConfig.UnbindHotkey?.Value))
                    {
                        _shortcutError = "F6 is already assigned to another editor shortcut.";
                    }
                    else
                    {
                        setting.Value = defaultKey;
                        _shortcutError = null;
                    }
                }
                else
                {
                    setting.Value = BepInEx.Configuration.KeyboardShortcut.Empty;
                    _shortcutError = null;
                }
                if (recording)
                    _shortcutCapture = ShortcutCapture.None;
            }
            GUI.enabled = enabled;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private static void CaptureEditorShortcut()
        {
            if (_shortcutCapture == ShortcutCapture.None ||
                Event.current.type != EventType.KeyDown)
                return;

            Event current = Event.current;
            KeyCode key = current.keyCode;
            if (key == KeyCode.Escape)
            {
                _shortcutCapture = ShortcutCapture.None;
                _shortcutError = null;
                current.Use();
                return;
            }

            // Modifier key presses alone are not valid shortcuts.
            if (key == KeyCode.None || IsModifierKey(key))
            {
                current.Use();
                return;
            }

            var modifiers = new List<KeyCode>();
            if (current.shift)
                modifiers.Add(KeyCode.LeftShift);
            if (current.control)
                modifiers.Add(KeyCode.LeftControl);
            if (current.alt)
                modifiers.Add(KeyCode.LeftAlt);
            if (current.command)
                modifiers.Add(KeyCode.LeftCommand);

            var candidate = new BepInEx.Configuration.KeyboardShortcut(
                key, modifiers.ToArray());
            bool duplicate =
                (_shortcutCapture != ShortcutCapture.Toggle &&
                 MatchesShortcut(candidate, NadaVfxEditorConfig.ToggleHotkey?.Value)) ||
                (_shortcutCapture != ShortcutCapture.Bind &&
                 MatchesShortcut(candidate, NadaVfxEditorConfig.BindHotkey?.Value)) ||
                (_shortcutCapture != ShortcutCapture.Unbind &&
                 MatchesShortcut(candidate, NadaVfxEditorConfig.UnbindHotkey?.Value));

            if (duplicate)
            {
                _shortcutError = "This shortcut is already used by the editor.";
            }
            else
            {
                var destination = _shortcutCapture == ShortcutCapture.Toggle
                    ? NadaVfxEditorConfig.ToggleHotkey
                    : _shortcutCapture == ShortcutCapture.Bind
                        ? NadaVfxEditorConfig.BindHotkey
                        : NadaVfxEditorConfig.UnbindHotkey;
                if (destination != null)
                    destination.Value = candidate;
                _shortcutError = null;
            }

            _shortcutCapture = ShortcutCapture.None;
            current.Use();
        }

        private static bool MatchesShortcut(
            BepInEx.Configuration.KeyboardShortcut a,
            BepInEx.Configuration.KeyboardShortcut? b)
        {
            return b.HasValue &&
                   a.MainKey == b.Value.MainKey &&
                   string.Equals(a.ToString(), b.Value.ToString(),
                       StringComparison.Ordinal);
        }

        private static bool IsModifierKey(KeyCode key)
        {
            return key == KeyCode.LeftShift || key == KeyCode.RightShift ||
                   key == KeyCode.LeftControl || key == KeyCode.RightControl ||
                   key == KeyCode.LeftAlt || key == KeyCode.RightAlt ||
                   key == KeyCode.LeftCommand || key == KeyCode.RightCommand;
        }

        private static void DrawRemotePlayerVisibility()
        {
            GUILayout.BeginHorizontal(GUILayout.Height(22f));
            GUILayout.Label("Players", _bodyStyle,
                GUILayout.Width(118f), GUILayout.Height(20f));
            GUILayout.Space(6f);
            int hidden = NadaVfxRemoteStateTracker.HiddenPlayerCount;
            string caption = _remotePlayerListExpanded
                ? "Hide List"
                : hidden > 0 ? $"Hide Selected ({hidden})" : "All";
            if (GUILayout.Button(caption, _themeOptionStyle,
                    GUILayout.Width(122f), GUILayout.Height(20f)))
                _remotePlayerListExpanded = !_remotePlayerListExpanded;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (!_remotePlayerListExpanded)
                return;

            List<NadaVfxRemoteStateTracker.RemotePlayerChoice> players =
                NadaVfxRemoteStateTracker.GetVisiblePlayerChoices();
            GUILayout.BeginHorizontal();
            GUILayout.Space(124f);
            if (GUILayout.Button("Show All", _themeOptionStyle,
                    GUILayout.Width(92f), GUILayout.Height(20f)))
                NadaVfxRemoteStateTracker.ShowAllPlayers();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (players.Count == 0)
            {
                GUILayout.Label("No other tracked players in this session.", _mutedStyle);
                return;
            }

            foreach (var player in players)
            {
                GUILayout.BeginHorizontal(GUILayout.Height(21f));
                GUILayout.Space(10f);
                GUILayout.Label(player.DisplayName, _bodyStyle,
                    GUILayout.Width(114f), GUILayout.Height(20f));
                if (GUILayout.Button(player.Hidden ? "Hidden" : "Visible",
                        _themeOptionStyle, GUILayout.Width(70f), GUILayout.Height(20f)))
                    NadaVfxRemoteStateTracker.SetPlayerHidden(player.PeerId, !player.Hidden);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            GUILayout.Label("Player choices reset when you leave the server.", _mutedStyle);
        }

        private static void DrawVisibilitySetting(
            string label,
            BepInEx.Configuration.ConfigEntry<bool> setting)
        {
            if (setting == null)
                return;

            GUILayout.BeginHorizontal(GUILayout.Height(22f));

            GUILayout.Label(
                label,
                _bodyStyle,
                GUILayout.Width(118f),
                GUILayout.Height(20f));

            GUILayout.Space(6f);

            bool enabled = setting.Value;
            if (GUILayout.Button(
                    enabled ? "ON" : "OFF",
                    _themeOptionStyle,
                    GUILayout.Width(52f),
                    GUILayout.Height(20f)))
            {
                // Use the existing config entry so its runtime change handlers
                // remain the single owner of visibility updates.
                setting.Value = !enabled;
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private static void DrawEffectsStrip(
            NadaVfxEditorTarget target)
        {
            GUILayout.BeginVertical(_panelStyle);

            bool readOnly = NadaVfxEditorWorkingState.IsReadOnly;
            bool hasTarget = target?.ItemData != null;
            bool invalidNative = target?.SourceKind == NadaWeaponLocalSourceKind.InvalidNative;
            bool previousEnabled = GUI.enabled;

            if (readOnly)
            {
                _addPickerOpen = false;
                _clearAllConfirm = false;
            }

            // One horizontal action row leaves all available width to the
            // effect chips beneath it, including when the window is compact.
            GUILayout.BeginHorizontal();
            GUILayout.Label("EFFECTS", _sectionHeaderStyle,
                GUILayout.Width(52f), GUILayout.Height(EffectActionHeight));
            GUILayout.FlexibleSpace();

            GUI.enabled = previousEnabled && hasTarget && !readOnly && !invalidNative;
            if (GUILayout.Button(_addPickerOpen ? "Close Add" : "+ Add Effect",
                    _addButtonStyle,
                    GUILayout.Width(88f), GUILayout.Height(EffectActionHeight)))
            {
                ClearEffectDrag();
                _addPickerOpen = !_addPickerOpen;
            }

            GUILayout.Space(3f);
            GUI.enabled = previousEnabled && hasTarget && !readOnly && !invalidNative &&
                          NadaVfxEditorWorkingState.EffectCount > 0;
            if (GUILayout.Button("Clear All", _addButtonStyle,
                    GUILayout.Width(74f), GUILayout.Height(EffectActionHeight)))
                _clearAllConfirm = true;

            GUILayout.Space(3f);
            GUI.enabled = previousEnabled && hasTarget && !invalidNative;
            if (GUILayout.Button(readOnly ? "Unbind Weapon" : "Bind to Weapon",
                    _addButtonStyle,
                    GUILayout.Width(106f), GUILayout.Height(EffectActionHeight)))
            {
                _pendingBindingAction = readOnly ? BindingAction.Unbind : BindingAction.Bind;
            }

            GUI.enabled = previousEnabled;
            GUILayout.EndHorizontal();

            if (readOnly && invalidNative)
                GUILayout.Label("Saved native state is invalid. Editing is disabled.",
                    _warningStyle);
            else if (readOnly)
                GUILayout.Label("This weapon is bound. Unbind it to edit effects.",
                    _mutedStyle);

            GUILayout.Space(3f);
            DrawWrappedEffects(target);
            if (_addPickerOpen)
            {
                GUILayout.Space(4f);
                DrawCompactEffectPicker(target);
            }

            if (_clearAllConfirm && !readOnly)
            {
                GUILayout.Space(4f);
                DrawClearAllConfirmation();
            }
            if (!string.IsNullOrEmpty(_bindingError))
                GUILayout.Label(_bindingError, _warningStyle);

            GUILayout.EndVertical();
        }

        private static void DrawWrappedEffects(
            NadaVfxEditorTarget target)
        {
            _pendingReorderTargetInstanceId =
                null;

            WeaponVfxState state =
                NadaVfxEditorWorkingState.State;

            if (target == null)
            {
                GUILayout.Label(
                    "No target.",
                    _mutedStyle);

                return;
            }

            if (state?.Effects == null ||
                state.Effects.Count == 0)
            {
                GUILayout.Label(
                    "No effects added.",
                    _mutedStyle);

                return;
            }

            float availableWidth =
                Mathf.Max(
                    EffectChipWidth,
                    _windowRect.width - 38f);

            int effectsPerRow =
                Mathf.Max(
                    1,
                    Mathf.FloorToInt(
                        (availableWidth +
                         EffectChipGap) /
                        (EffectChipWidth +
                         EffectChipGap)));

            int effectsInCurrentRow =
                0;

            bool rowOpen =
                false;

            foreach (VfxEffectBlock block in
                     state.Effects)
            {
                if (block == null)
                    continue;

                if (!rowOpen)
                {
                    GUILayout.BeginHorizontal();

                    rowOpen =
                        true;

                    effectsInCurrentRow =
                        0;
                }

                DrawEffectChip(
                    block);

                effectsInCurrentRow++;

                if (effectsInCurrentRow <
                    effectsPerRow)
                {
                    GUILayout.Space(
                        EffectChipGap);
                }

                if (effectsInCurrentRow >=
                    effectsPerRow)
                {
                    GUILayout.FlexibleSpace();

                    GUILayout.EndHorizontal();

                    rowOpen =
                        false;

                    GUILayout.Space(
                        3f);
                }
            }

            if (rowOpen)
            {
                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();
            }

            ApplyPendingEffectReorder();
        }

        private static void DrawEffectChip(
            VfxEffectBlock block)
        {
            bool selected =
                _selectedInstanceId.HasValue &&
                _selectedInstanceId.Value ==
                block.InstanceId;

            bool draggingThisBlock =
                _effectDragActive &&
                _dragCandidateInstanceId.HasValue &&
                _dragCandidateInstanceId.Value ==
                block.InstanceId;

            GUIStyle style =
                selected ||
                draggingThisBlock
                    ? _selectedBlockStyle
                    : _blockStyle;

            Rect rowRect =
                GUILayoutUtility.GetRect(
                    GUIContent.none,
                    style,
                    GUILayout.Width(
                        EffectChipWidth),
                    GUILayout.Height(
                        BlockHeight));

            GUI.Box(
                rowRect,
                GUIContent.none,
                style);

            Rect toggleRect =
                new Rect(
                    rowRect.xMax -
                    EffectToggleWidth -
                    5f,
                    rowRect.y + 3f,
                    EffectToggleWidth,
                    rowRect.height - 6f);

            Rect interactionRect =
                new Rect(
                    rowRect.x,
                    rowRect.y,
                    Mathf.Max(
                        0f,
                        toggleRect.x -
                        rowRect.x -
                        3f),
                    rowRect.height);

            HandleEffectRowMouse(
                block,
                interactionRect);

            Rect gripRect =
                new Rect(
                    rowRect.x + 6f,
                    rowRect.y + 3f,
                    14f,
                    rowRect.height - 6f);

            GUI.Label(
                gripRect,
                "::",
                _mutedStyle);

            Rect labelRect =
                new Rect(
                    rowRect.x + 21f,
                    rowRect.y + 3f,
                    Mathf.Max(
                        0f,
                        toggleRect.x -
                        rowRect.x -
                        25f),
                    rowRect.height - 6f);

            GUI.Label(
                labelRect,
                NadaVfxEditorWorkingState.GetDisplayName(
                    block),
                block.Enabled
                    ? _blockLabelStyle
                    : _disabledBlockLabelStyle);

            GUIStyle toggleStyle =
                block.Enabled
                    ? _effectOnStyle
                    : _effectOffStyle;

            bool previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled &&
                          !NadaVfxEditorWorkingState.IsReadOnly;

            if (GUI.Button(
                    toggleRect,
                    block.Enabled
                        ? "ON"
                        : "OFF",
                    toggleStyle))
            {
                if (NadaVfxEditorWorkingState.TrySetEnabled(
                        block.InstanceId,
                        !block.Enabled))
                {
                    RefreshPreview();
                }
            }

            GUI.enabled = previousEnabled;
        }

        private static void DrawCompactEffectPicker(
            NadaVfxEditorTarget target)
        {
            if (target == null)
            {
                _addPickerOpen =
                    false;

                return;
            }

            GUILayout.BeginVertical(
                _pickerStyle);

            DrawWrappedPickerGroup(
                "STANDARD",
                isOrbital: false);

            GUILayout.Space(
                5f);

            DrawWrappedPickerGroup(
                "ORBITALS",
                isOrbital: true);

            GUILayout.EndVertical();
        }

        private static void DrawWrappedPickerGroup(
            string title,
            bool isOrbital)
        {
            GUILayout.Label(
                title,
                _smallHeaderStyle);

            GUILayout.Space(
                2f);

            var definitions =
                new List<NadaVfxEditorEffectDefinition>();

            foreach (NadaVfxEditorEffectDefinition definition in
                     NadaVfxEditorEffectCatalog.All)
            {
                if (definition == null ||
                    definition.IsOrbital !=
                    isOrbital)
                {
                    continue;
                }

                definitions.Add(
                    definition);
            }

            if (definitions.Count == 0)
                return;

            float availableWidth =
                Mathf.Max(
                    PickerButtonWidth,
                    _windowRect.width - 50f);

            int buttonsPerRow =
                Mathf.Max(
                    1,
                    Mathf.FloorToInt(
                        (availableWidth +
                         PickerButtonGap) /
                        (PickerButtonWidth +
                         PickerButtonGap)));

            int index =
                0;

            while (index <
                   definitions.Count)
            {
                GUILayout.BeginHorizontal();

                int drawn =
                    0;

                while (index <
                       definitions.Count &&
                       drawn <
                       buttonsPerRow)
                {
                    DrawAddEffectButton(
                        definitions[index]);

                    index++;
                    drawn++;

                    if (drawn <
                        buttonsPerRow &&
                        index <
                        definitions.Count)
                    {
                        GUILayout.Space(
                            PickerButtonGap);
                    }
                }

                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();

                if (index <
                    definitions.Count)
                {
                    GUILayout.Space(
                        3f);
                }
            }
        }

        private static void DrawAddEffectButton(
            NadaVfxEditorEffectDefinition definition)
        {
            if (definition == null)
                return;

            if (!GUILayout.Button(
                    $"+ {definition.PickerName}",
                    _pickerButtonStyle,
                    GUILayout.Width(
                        PickerButtonWidth),
                    GUILayout.Height(22f)))
            {
                return;
            }

            if (!NadaVfxEditorWorkingState.TryAddEffect(
                    definition.TypeId,
                    out VfxEffectBlock block))
            {
                return;
            }

            SelectBlock(
                block.InstanceId);

            _addPickerOpen =
                false;

            RefreshPreview();
        }

        private static void HandleEffectRowMouse(
            VfxEffectBlock block,
            Rect interactionRect)
        {
            Event current =
                Event.current;

            bool mouseOver =
                interactionRect.Contains(
                    current.mousePosition);

            if (current.type ==
                    EventType.MouseDown &&
                current.button ==
                    0 &&
                mouseOver)
            {
                SelectBlock(
                    block.InstanceId);

                if (NadaVfxEditorWorkingState.IsReadOnly)
                {
                    ClearEffectDrag();
                    return;
                }

                _dragCandidateInstanceId =
                    block.InstanceId;

                _effectDragStartMousePosition =
                    current.mousePosition;

                _effectDragActive =
                    false;
            }

            if (!_dragCandidateInstanceId.HasValue ||
                current.type !=
                EventType.MouseDrag)
            {
                return;
            }

            if (!_effectDragActive)
            {
                float dragDistance =
                    Vector2.Distance(
                        _effectDragStartMousePosition,
                        current.mousePosition);

                if (dragDistance >=
                    EffectDragThreshold)
                {
                    _effectDragActive =
                        true;
                }
            }

            if (!_effectDragActive ||
                !mouseOver ||
                _dragCandidateInstanceId.Value ==
                block.InstanceId)
            {
                return;
            }

            _pendingReorderTargetInstanceId =
                block.InstanceId;
        }

        private static void ApplyPendingEffectReorder()
        {
            if (NadaVfxEditorWorkingState.IsReadOnly)
                return;

            if (!_effectDragActive ||
                !_dragCandidateInstanceId.HasValue ||
                !_pendingReorderTargetInstanceId.HasValue)
            {
                return;
            }

            uint draggedInstanceId =
                _dragCandidateInstanceId.Value;

            uint targetInstanceId =
                _pendingReorderTargetInstanceId.Value;

            if (draggedInstanceId ==
                targetInstanceId)
            {
                return;
            }

            WeaponVfxState state =
                NadaVfxEditorWorkingState.State;

            if (state?.Effects == null)
                return;

            int draggedIndex =
                FindEffectIndex(
                    state,
                    draggedInstanceId);

            int targetIndex =
                FindEffectIndex(
                    state,
                    targetInstanceId);

            if (draggedIndex < 0 ||
                targetIndex < 0 ||
                draggedIndex ==
                targetIndex)
            {
                return;
            }

            int direction =
                targetIndex >
                draggedIndex
                    ? 1
                    : -1;

            bool moved =
                false;

            while (draggedIndex !=
                   targetIndex)
            {
                if (!NadaVfxEditorWorkingState.TryMoveEffect(
                        draggedInstanceId,
                        direction))
                {
                    break;
                }

                draggedIndex +=
                    direction;

                moved =
                    true;
            }

            if (moved)
            {
                RefreshPreview();
            }
        }

        private static int FindEffectIndex(
            WeaponVfxState state,
            uint instanceId)
        {
            if (state?.Effects == null)
                return -1;

            for (int i = 0;
                 i < state.Effects.Count;
                 i++)
            {
                VfxEffectBlock block =
                    state.Effects[i];

                if (block != null &&
                    block.InstanceId ==
                    instanceId)
                {
                    return i;
                }
            }

            return -1;
        }

        private static void ClearEffectDrag()
        {
            _dragCandidateInstanceId =
                null;

            _pendingReorderTargetInstanceId =
                null;

            _effectDragActive =
                false;

            _effectDragStartMousePosition =
                Vector2.zero;
        }

        private static void DrawInspectorPanel()
        {
            GUILayout.BeginVertical(
                _panelStyle,
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true));

            bool readOnly = NadaVfxEditorWorkingState.IsReadOnly;

            GUILayout.Label(
                readOnly ? "READ ONLY" : "EDIT",
                readOnly ? _mutedStyle : _sectionHeaderStyle);

            if (readOnly &&
                !string.IsNullOrEmpty(NadaVfxEditorWorkingState.BoundViewError))
            {
                GUILayout.Label(
                    NadaVfxEditorWorkingState.BoundViewError,
                    _warningStyle);
            }

            GUILayout.Space(
                2f);

            VfxEffectBlock selectedBlock =
                FindSelectedBlock();

            if (selectedBlock == null)
            {
                GUILayout.Label(
                    readOnly
                        ? "No saved effects to inspect."
                        : "Add an effect, then select it to edit.",
                    _mutedStyle);

                GUILayout.EndVertical();

                return;
            }

            _inspectorScrollPosition =
                GUILayout.BeginScrollView(
                    _inspectorScrollPosition,
                    GUILayout.ExpandHeight(true));

            DrawSelectedEffectHeader(
                selectedBlock);

            if (!readOnly)
                DrawTransientBlockControls(selectedBlock);

            GUILayout.Space(
                4f);

            NadaVfxEditorControls
                .SetCompactLayout(
                    IsCompactLayout);

            // Inspectors contain direct typed setters. Disable their input,
            // and render only a detached read-only snapshot when bound.
            bool previousEnabled = GUI.enabled;
            bool inspectorChanged;
            try
            {
                NadaVfxEditorControls.SetReadOnlyNavigation(readOnly);
                GUI.enabled = previousEnabled && !readOnly;
                inspectorChanged =
                    NadaVfxBlockInspectorRegistry.Draw(selectedBlock);
            }
            finally
            {
                GUI.enabled = previousEnabled;
                NadaVfxEditorControls.SetReadOnlyNavigation(false);
            }

            if (inspectorChanged && !readOnly)
                RefreshPreview();

            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }

        private static void DrawSelectedEffectHeader(
            VfxEffectBlock block)
        {
            if (IsCompactLayout)
            {
                DrawEffectIdentity(
                    block);

                GUILayout.Space(
                    4f);

                GUILayout.BeginHorizontal();

                DrawBlockActionButtons(
                    block);

                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();

                return;
            }

            GUILayout.BeginHorizontal();

            DrawEffectIdentity(
                block);

            GUILayout.FlexibleSpace();

            DrawBlockActionButtons(
                block);

            GUILayout.EndHorizontal();
        }

        private static void DrawEffectIdentity(
            VfxEffectBlock block)
        {
            GUILayout.BeginHorizontal(
                GUILayout.ExpandWidth(true));

            GUILayout.Box(
                GUIContent.none,
                _inspectorAccentStyle,
                GUILayout.Width(3f),
                GUILayout.Height(32f));

            GUILayout.Space(
                6f);

            GUILayout.BeginVertical(
                GUILayout.ExpandWidth(true));

            GUILayout.Label(
                NadaVfxEditorWorkingState.GetDisplayName(
                    block),
                _inspectorTitleStyle);

            GUILayout.Label(
                GetFriendlyTypeName(
                    block.TypeId),
                _mutedStyle);

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        private static void DrawBlockActionButtons(
            VfxEffectBlock block)
        {
            bool previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled &&
                          !NadaVfxEditorWorkingState.IsReadOnly;

            try
            {
                if (GUILayout.Button(
                        "Rename",
                        _headerActionButtonStyle,
                        GUILayout.Width(54f),
                        GUILayout.Height(23f)))
                {
                    _renameInstanceId =
                        block.InstanceId;

                    _focusRenameOnNextDraw = true;
                    _ownsKeyboardInput = true;

                    _renameBuffer =
                        NadaVfxEditorWorkingState
                            .GetDisplayName(
                                block);

                    _resetConfirmInstanceId =
                        null;

                    _deleteConfirmInstanceId =
                        null;
                }

                if (GUILayout.Button(
                        "Duplicate",
                        _headerActionButtonStyle,
                        GUILayout.Width(66f),
                        GUILayout.Height(23f)))
                {
                    if (NadaVfxEditorWorkingState.TryDuplicateEffect(
                            block.InstanceId,
                            out VfxEffectBlock duplicate))
                    {
                        SelectBlock(
                            duplicate.InstanceId);

                        RefreshPreview();
                    }
                }

                if (GUILayout.Button(
                        "Reset",
                        _headerActionButtonStyle,
                        GUILayout.Width(48f),
                        GUILayout.Height(23f)))
                {
                    _resetConfirmInstanceId =
                        block.InstanceId;

                    _renameInstanceId =
                        null;

                    _renameBuffer =
                        string.Empty;

                    _deleteConfirmInstanceId =
                        null;
                }

                if (GUILayout.Button(
                        "Delete",
                        _headerActionButtonStyle,
                        GUILayout.Width(48f),
                        GUILayout.Height(23f)))
                {
                    _deleteConfirmInstanceId =
                        block.InstanceId;

                    _renameInstanceId =
                        null;

                    _renameBuffer =
                        string.Empty;

                    _resetConfirmInstanceId =
                        null;
                }
            }
            finally
            {
                GUI.enabled = previousEnabled;
            }
        }

        private static void DrawTransientBlockControls(
            VfxEffectBlock block)
        {
            if (_renameInstanceId.HasValue &&
                _renameInstanceId.Value ==
                block.InstanceId)
            {
                DrawRenameControls(
                    block);
            }

            if (_resetConfirmInstanceId.HasValue &&
                _resetConfirmInstanceId.Value ==
                block.InstanceId)
            {
                DrawResetConfirmation(
                    block);
            }

            if (_deleteConfirmInstanceId.HasValue &&
                _deleteConfirmInstanceId.Value ==
                block.InstanceId)
            {
                DrawDeleteConfirmation(
                    block);
            }
        }

        private static void DrawRenameControls(
            VfxEffectBlock block)
        {
            GUILayout.Space(
                3f);

            if (IsCompactLayout)
            {
                GUILayout.Label(
                    "Name",
                    _mutedStyle);

                GUI.SetNextControlName(
                    RenameControlName);

                _renameBuffer =
                    GUILayout.TextField(
                        _renameBuffer ??
                        string.Empty,
                        64,
                        GUILayout.ExpandWidth(true),
                        GUILayout.Height(20f));

                FocusRenameFieldIfRequested();

                GUILayout.Space(
                    3f);

                GUILayout.BeginHorizontal();

                DrawRenameActionButtons(
                    block);

                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();

                return;
            }

            GUILayout.BeginHorizontal();

            GUILayout.Label(
                "Name",
                _mutedStyle,
                GUILayout.Width(34f));

            GUI.SetNextControlName(
                RenameControlName);

            _renameBuffer =
                GUILayout.TextField(
                    _renameBuffer ??
                    string.Empty,
                    64,
                    GUILayout.ExpandWidth(true),
                    GUILayout.Height(20f));

            FocusRenameFieldIfRequested();

            DrawRenameActionButtons(
                block);

            GUILayout.EndHorizontal();
        }

        private static void FocusRenameFieldIfRequested()
        {
            if (!_focusRenameOnNextDraw ||
                Event.current.type != EventType.Repaint)
                return;

            GUI.FocusControl(RenameControlName);
            _focusRenameOnNextDraw = false;
        }

        private static void DrawRenameActionButtons(
            VfxEffectBlock block)
        {
            bool canSave =
                !string.IsNullOrWhiteSpace(
                    _renameBuffer);

            bool previousEnabled =
                GUI.enabled;

            GUI.enabled =
                previousEnabled &&
                canSave;

            if (GUILayout.Button(
                    "Save",
                    _headerActionButtonStyle,
                    GUILayout.Width(44f),
                    GUILayout.Height(22f)))
            {
                if (NadaVfxEditorWorkingState.TryRenameEffect(
                        block.InstanceId,
                        _renameBuffer))
                {
                    _renameInstanceId =
                        null;

                    _renameBuffer =
                        string.Empty;
                }
            }

            GUI.enabled =
                previousEnabled;

            if (GUILayout.Button(
                    "Cancel",
                    _headerActionButtonStyle,
                    GUILayout.Width(50f),
                    GUILayout.Height(22f)))
            {
                _renameInstanceId =
                    null;

                _renameBuffer =
                    string.Empty;
            }
        }

        private static void DrawResetConfirmation(
            VfxEffectBlock block)
        {
            GUILayout.Space(
                3f);

            GUILayout.BeginVertical(
                _readOnlyBoxStyle);

            GUILayout.Label(
                $"Reset '{NadaVfxEditorWorkingState.GetDisplayName(block)}'?",
                _warningStyle);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button(
                    "Confirm",
                    _headerActionButtonStyle,
                    GUILayout.Width(60f),
                    GUILayout.Height(22f)))
            {
                if (NadaVfxEditorWorkingState.TryResetEffect(
                        block.InstanceId))
                {
                    _resetConfirmInstanceId =
                        null;

                    RefreshPreview();
                }
            }

            if (GUILayout.Button(
                    "Cancel",
                    _headerActionButtonStyle,
                    GUILayout.Width(50f),
                    GUILayout.Height(22f)))
            {
                _resetConfirmInstanceId =
                    null;
            }

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private static void DrawDeleteConfirmation(
            VfxEffectBlock block)
        {
            GUILayout.Space(
                3f);

            GUILayout.BeginVertical(
                _readOnlyBoxStyle);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(
                $"Delete '{NadaVfxEditorWorkingState.GetDisplayName(block)}'?",
                _warningStyle);
            GUILayout.Space(8f);

            if (GUILayout.Button(
                    "Delete",
                    _headerActionButtonStyle,
                    GUILayout.Width(50f),
                    GUILayout.Height(22f)))
            {
                uint deletedInstanceId =
                    block.InstanceId;

                if (NadaVfxEditorWorkingState.TryDeleteEffect(
                        deletedInstanceId,
                        out uint? nextSelection))
                {
                    _selectedInstanceId =
                        nextSelection;

                    _deleteConfirmInstanceId =
                        null;

                    _resetConfirmInstanceId =
                        null;

                    _renameInstanceId =
                        null;

                    _renameBuffer =
                        string.Empty;

                    _inspectorScrollPosition =
                        Vector2.zero;

                    ClearEffectDrag();

                    RefreshPreview();
                }
            }

            if (GUILayout.Button(
                    "Cancel",
                    _headerActionButtonStyle,
                    GUILayout.Width(50f),
                    GUILayout.Height(22f)))
            {
                _deleteConfirmInstanceId =
                    null;
            }

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private static void SelectBlock(
            uint instanceId)
        {
            if (_selectedInstanceId.HasValue &&
                _selectedInstanceId.Value ==
                instanceId)
            {
                return;
            }

            _selectedInstanceId =
                instanceId;

            _inspectorScrollPosition =
                Vector2.zero;

            ClearTransientBlockActions();
        }

        private static void ClearTransientBlockActions()
        {
            _renameInstanceId =
                null;

            _focusRenameOnNextDraw = false;

            _renameBuffer =
                string.Empty;

            _resetConfirmInstanceId =
                null;

            _deleteConfirmInstanceId =
                null;
        }

        private static void UpdateKeyboardInputOwnership()
        {
            string focusedControl =
                GUI.GetNameOfFocusedControl();

            if (string.IsNullOrEmpty(
                    focusedControl))
            {
                _ownsKeyboardInput =
                    false;

                return;
            }

            _ownsKeyboardInput =
                string.Equals(
                    focusedControl,
                    RenameControlName,
                    StringComparison.Ordinal) ||

                focusedControl.StartsWith(
                    NumericControlPrefix,
                    StringComparison.Ordinal) ||
                NadaVfxEditorStylesPanel.IsTextInputFocused(focusedControl);
        }

        private static string GetFriendlyTypeName(
            string typeId)
        {
            NadaVfxEditorEffectDefinition definition =
                NadaVfxEditorEffectCatalog.Find(
                    typeId);

            return
                definition?.PickerName ??
                typeId ??
                "Unknown";
        }

        private static void DrawSingleInfoRow(
            string label,
            string value)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label(
                label,
                _mutedStyle,
                GUILayout.Width(72f));

            GUILayout.Label(
                value,
                _bodyStyle);

            GUILayout.EndHorizontal();
        }

        private static void EnsureSelection()
        {
            WeaponVfxState state =
                NadaVfxEditorWorkingState.State;

            if (state?.Effects == null ||
                state.Effects.Count == 0)
            {
                _selectedInstanceId =
                    null;

                ClearEffectDrag();
                ClearTransientBlockActions();

                return;
            }

            if (_selectedInstanceId.HasValue)
            {
                foreach (VfxEffectBlock block in
                         state.Effects)
                {
                    if (block != null &&
                        block.InstanceId ==
                        _selectedInstanceId.Value)
                    {
                        return;
                    }
                }
            }

            VfxEffectBlock firstValid =
                null;

            foreach (VfxEffectBlock block in
                     state.Effects)
            {
                if (block == null)
                    continue;

                firstValid =
                    block;

                break;
            }

            _selectedInstanceId =
                firstValid?.InstanceId;

            ClearEffectDrag();
            ClearTransientBlockActions();
        }

        private static VfxEffectBlock FindSelectedBlock()
        {
            WeaponVfxState state =
                NadaVfxEditorWorkingState.State;

            if (state?.Effects == null ||
                !_selectedInstanceId.HasValue)
            {
                return null;
            }

            foreach (VfxEffectBlock block in
                     state.Effects)
            {
                if (block != null &&
                    block.InstanceId ==
                    _selectedInstanceId.Value)
                {
                    return block;
                }
            }

            return null;
        }

        private static void DrawResizeHandles()
        {
            Event current =
                Event.current;

            int controlId =
                GUIUtility.GetControlID(
                    ResizeControlIdHint,
                    FocusType.Passive);

            EventType controlEvent =
                current.GetTypeForControl(
                    controlId);

            switch (controlEvent)
            {
                case EventType.MouseDown:
                {
                    if (current.button != 0)
                        break;

                    ResizeEdges edges =
                        GetResizeEdgesAt(
                            current.mousePosition);

                    if (edges ==
                        ResizeEdges.None)
                    {
                        break;
                    }

                    GUIUtility.hotControl =
                        controlId;

                    _resizing =
                        true;

                    _activeResizeEdges =
                        edges;

                    _resizeStartMouseScreenPosition =
                        GUIUtility.GUIToScreenPoint(
                            current.mousePosition);

                    _resizeStartRect =
                        _windowRect;

                    current.Use();

                    break;
                }

                case EventType.MouseDrag:
                {
                    if (!_resizing ||
                        GUIUtility.hotControl !=
                        controlId)
                    {
                        break;
                    }

                    Vector2 currentMouseScreenPosition =
                        GUIUtility.GUIToScreenPoint(
                            current.mousePosition);

                    Vector2 delta =
                        currentMouseScreenPosition -
                        _resizeStartMouseScreenPosition;

                    ApplyResizeDelta(
                        delta);

                    current.Use();

                    break;
                }

                case EventType.MouseUp:
                {
                    if (GUIUtility.hotControl !=
                        controlId)
                    {
                        break;
                    }

                    _resizing =
                        false;

                    _activeResizeEdges =
                        ResizeEdges.None;

                    GUIUtility.hotControl =
                        0;

                    current.Use();

                    break;
                }
            }
        }

        private static ResizeEdges GetResizeEdgesAt(
            Vector2 mousePosition)
        {
            ResizeEdges edges =
                ResizeEdges.None;

            if (mousePosition.x >= 0f &&
                mousePosition.x <=
                ResizeEdgeThickness)
            {
                edges |=
                    ResizeEdges.Left;
            }
            else if (mousePosition.x <=
                         _windowRect.width &&
                     mousePosition.x >=
                         _windowRect.width -
                         ResizeEdgeThickness)
            {
                edges |=
                    ResizeEdges.Right;
            }

            if (mousePosition.y >= 0f &&
                mousePosition.y <=
                ResizeEdgeThickness)
            {
                edges |=
                    ResizeEdges.Top;
            }
            else if (mousePosition.y <=
                         _windowRect.height &&
                     mousePosition.y >=
                         _windowRect.height -
                         ResizeEdgeThickness)
            {
                edges |=
                    ResizeEdges.Bottom;
            }

            return edges;
        }

        private static void ApplyResizeDelta(
            Vector2 delta)
        {
            Rect start =
                _resizeStartRect;

            Rect resized =
                start;

            float minimumWidth =
                Mathf.Min(
                    MinimumWindowWidth,
                    Screen.width);

            float minimumHeight =
                Mathf.Min(
                    MinimumWindowHeight,
                    Screen.height);

            if ((_activeResizeEdges &
                 ResizeEdges.Left) != 0)
            {
                float fixedRight =
                    start.xMax;

                float maximumLeft =
                    fixedRight -
                    minimumWidth;

                float nextLeft =
                    Mathf.Clamp(
                        start.x + delta.x,
                        0f,
                        Mathf.Max(
                            0f,
                            maximumLeft));

                resized.x =
                    nextLeft;

                resized.width =
                    fixedRight -
                    nextLeft;
            }
            else if ((_activeResizeEdges &
                      ResizeEdges.Right) != 0)
            {
                float minimumRight =
                    start.x +
                    minimumWidth;

                float nextRight =
                    Mathf.Clamp(
                        start.xMax + delta.x,
                        minimumRight,
                        Screen.width);

                resized.width =
                    nextRight -
                    start.x;
            }

            if ((_activeResizeEdges &
                 ResizeEdges.Top) != 0)
            {
                float fixedBottom =
                    start.yMax;

                float maximumTop =
                    fixedBottom -
                    minimumHeight;

                float nextTop =
                    Mathf.Clamp(
                        start.y + delta.y,
                        0f,
                        Mathf.Max(
                            0f,
                            maximumTop));

                resized.y =
                    nextTop;

                resized.height =
                    fixedBottom -
                    nextTop;
            }
            else if ((_activeResizeEdges &
                      ResizeEdges.Bottom) != 0)
            {
                float minimumBottom =
                    start.y +
                    minimumHeight;

                float nextBottom =
                    Mathf.Clamp(
                        start.yMax + delta.y,
                        minimumBottom,
                        Screen.height);

                resized.height =
                    nextBottom -
                    start.y;
            }

            _windowRect =
                resized;
        }

        private static void EnsureWindowPosition()
        {
            if (_positionInitialized)
                return;

            _windowRect.width = NadaVfxEditorConfig.WindowWidth?.Value
                ?? DefaultWindowWidth;
            _windowRect.height = NadaVfxEditorConfig.WindowHeight?.Value
                ?? DefaultWindowHeight;
            ClampWindowSize();

            int x = NadaVfxEditorConfig.WindowX?.Value ?? -1;
            int y = NadaVfxEditorConfig.WindowY?.Value ?? -1;
            _windowRect.x = x < 0
                ? Mathf.Max(20f, Screen.width - _windowRect.width - 24f)
                : x;
            _windowRect.y = y < 0 ? 64f : y;
            _positionInitialized = true;
            ClampWindowToScreen();
        }

        // Persist only at close, not on every IMGUI resize or drag event.
        private static void SaveWindowBounds()
        {
            if (!_positionInitialized)
                return;

            if (NadaVfxEditorConfig.WindowX != null)
                NadaVfxEditorConfig.WindowX.Value = Mathf.RoundToInt(_windowRect.x);
            if (NadaVfxEditorConfig.WindowY != null)
                NadaVfxEditorConfig.WindowY.Value = Mathf.RoundToInt(_windowRect.y);
            if (NadaVfxEditorConfig.WindowWidth != null)
                NadaVfxEditorConfig.WindowWidth.Value = Mathf.RoundToInt(_windowRect.width);
            if (NadaVfxEditorConfig.WindowHeight != null)
                NadaVfxEditorConfig.WindowHeight.Value = Mathf.RoundToInt(_windowRect.height);
        }

        private static void ClampWindowSize()
        {
            float maximumWidth =
                Mathf.Max(
                    MinimumWindowWidth,
                    Screen.width);

            float maximumHeight =
                Mathf.Max(
                    MinimumWindowHeight,
                    Screen.height);

            float minimumWidth =
                Mathf.Min(
                    MinimumWindowWidth,
                    maximumWidth);

            float minimumHeight =
                Mathf.Min(
                    MinimumWindowHeight,
                    maximumHeight);

            _windowRect.width =
                Mathf.Clamp(
                    _windowRect.width,
                    minimumWidth,
                    maximumWidth);

            _windowRect.height =
                Mathf.Clamp(
                    _windowRect.height,
                    minimumHeight,
                    maximumHeight);
        }

        private static void ClampWindowToScreen()
        {
            ClampWindowSize();

            _windowRect.x =
                Mathf.Clamp(
                    _windowRect.x,
                    0f,
                    Mathf.Max(
                        0f,
                        Screen.width -
                        _windowRect.width));

            _windowRect.y =
                Mathf.Clamp(
                    _windowRect.y,
                    0f,
                    Mathf.Max(
                        0f,
                        Screen.height -
                        _windowRect.height));
        }

        private static void CaptureCursor()
        {
            if (_cursorStateCaptured)
                return;

            _previousCursorVisible =
                Cursor.visible;

            _previousCursorLockMode =
                Cursor.lockState;

            _cursorStateCaptured =
                true;
        }

        private static void RestoreCursor()
        {
            if (!_cursorStateCaptured)
                return;

            Cursor.visible =
                _previousCursorVisible;

            Cursor.lockState =
                _previousCursorLockMode;

            _cursorStateCaptured =
                false;
        }

        private static void EnsureStyles()
        {
            NadaVfxEditorThemePreset currentPreset =
                NadaVfxEditorThemes.CurrentPreset;

            if (_windowStyle != null &&
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

            // Neutral one-pixel strokes distinguish ordinary controls from
            // active panel tabs, which retain the brighter accent outline.
            Color32 buttonStroke = new Color32(
                theme.MutedText.r, theme.MutedText.g, theme.MutedText.b, 110);
            Color32 buttonHoverStroke = new Color32(
                theme.MutedText.r, theme.MutedText.g, theme.MutedText.b, 175);
            Texture2D roundedButtonBackground =
                CreateRoundedOutlineTexture(theme.ButtonBackground, buttonStroke);
            Texture2D roundedButtonHover =
                CreateRoundedOutlineTexture(theme.ButtonHoverBackground, buttonHoverStroke);

            Texture2D inspectorAccentBackground =
                CreateTexture(
                    theme.AccentPrimary);

            _windowStyle =
                new GUIStyle(
                    GUI.skin.window)
                {
                    padding =
                        new RectOffset(
                            9,
                            9,
                            6,
                            9)
                };

            _windowStyle.border =
                new RectOffset(
                    10,
                    10,
                    10,
                    10);

            _windowStyle.normal.background =
                CreateRoundedTexture(
                    theme.WindowBackground,
                    8f);

            _windowStyle.normal.textColor =
                theme.PrimaryText;

            _brandSubtitleStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
fontSize = 12,

                    fontStyle =
                        FontStyle.Bold,

                    alignment =
                        TextAnchor.LowerLeft,

                    clipping =
                        TextClipping.Clip,

                    padding =
                        new RectOffset(
                            0,
                            0,
                            0,
                            0)
                };

            _brandSubtitleStyle.normal.textColor =
                theme.TitleSubtitle;

            _smallHeaderStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    fontSize = 9,
                    fontStyle =
                        FontStyle.Bold
                };

            _smallHeaderStyle.normal.textColor =
                theme.AccentTertiary;

            _targetCardStyle =
                new GUIStyle(
                    GUI.skin.box)
                {
                    padding =
                        new RectOffset(
                            8,
                            8,
                            5,
                            5)
                };

            _targetCardStyle.border =
                new RectOffset(
                    10,
                    10,
                    10,
                    10);

            _targetCardStyle.normal.background =
                CreateRoundedTexture(
                    theme.TargetBackground,
                    8f);

            _targetTitleStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    fontSize = 12,

                    wordWrap = false,

                    clipping =
                        TextClipping.Clip,

                    padding =
                        new RectOffset(
                            0,
                            0,
                            1,
                            0)
                };

            _targetTitleStyle.normal.textColor =
                theme.TargetText;

            _targetStatusStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    fontSize = 9,

                    wordWrap = false,

                    alignment =
                        TextAnchor.MiddleLeft,

                    padding =
                        new RectOffset(
                            0,
                            0,
                            1,
                            0)
                };

            _targetStatusStyle.normal.textColor =
                theme.MutedText;

            _panelStyle =
                new GUIStyle(
                    GUI.skin.box)
                {
                    padding =
                        new RectOffset(
                            7,
                            7,
                            7,
                            7)
                };

            _panelStyle.border =
                new RectOffset(
                    10,
                    10,
                    10,
                    10);

            _panelStyle.normal.background =
                CreateRoundedTexture(
                    theme.PanelBackground,
                    8f);

            _sectionHeaderStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    fontSize = 9,
                    fontStyle =
                        FontStyle.Bold
                };

            _sectionHeaderStyle.normal.textColor =
                theme.AccentSecondary;

            _blockStyle =
                new GUIStyle(
                    GUI.skin.button)
                {
                    padding =
                        new RectOffset(
                            6,
                            6,
                            3,
                            3),

                    fontSize = 10
                };

            _blockStyle.border = new RectOffset(7, 7, 7, 7);
            _blockStyle.normal.background =
                CreateRoundedTexture(theme.BlockBackground);

            _blockStyle.hover.background =
                CreateRoundedTexture(theme.BlockHoverBackground);

            _blockStyle.active.background =
                _blockStyle.hover.background;

            _selectedBlockStyle =
                new GUIStyle(
                    _blockStyle);

            _selectedBlockStyle.normal.background =
                CreateRoundedTexture(theme.SelectedBlockBackground);

            _selectedBlockStyle.hover.background =
                CreateRoundedTexture(theme.SelectedBlockBackground);

            _selectedBlockStyle.active.background =
                CreateRoundedTexture(theme.SelectedBlockBackground);

            _blockLabelStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    fontSize = 10,

                    fontStyle =
                        FontStyle.Bold,

                    clipping =
                        TextClipping.Clip
                };

            _blockLabelStyle.normal.textColor =
                theme.PrimaryText;

            _disabledBlockLabelStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    fontSize = 10,

                    fontStyle =
                        FontStyle.Bold,

                    clipping =
                        TextClipping.Clip
                };

            _disabledBlockLabelStyle.normal.textColor =
                theme.DimText;

            _inspectorTitleStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    fontSize = 15,

                    fontStyle =
                        FontStyle.Bold,

                    clipping =
                        TextClipping.Clip,

                    padding =
                        new RectOffset(
                            0,
                            0,
                            0,
                            0)
                };

            _inspectorTitleStyle.normal.textColor =
                theme.AccentPrimary;

            _inspectorAccentStyle =
                new GUIStyle(
                    GUI.skin.box)
                {
                    margin =
                        new RectOffset(
                            0,
                            0,
                            1,
                            1),

                    padding =
                        new RectOffset(
                            0,
                            0,
                            0,
                            0)
                };

            _inspectorAccentStyle.normal.background =
                inspectorAccentBackground;

            _bodyStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    fontSize = 10,

                    wordWrap = false,

                    clipping =
                        TextClipping.Clip
                };

            _bodyStyle.normal.textColor =
                theme.PrimaryText;

            _mutedStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    fontSize = 9,
                    wordWrap = true
                };

            _mutedStyle.normal.textColor =
                theme.MutedText;

            _settingsSubHeaderStyle = new GUIStyle(_sectionHeaderStyle)
            {
                fontSize = 9,
                fontStyle = FontStyle.Bold
            };
            _settingsSubHeaderStyle.normal.textColor = theme.MutedText;

            _warningStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    fontSize = 10,
                    wordWrap = true
                };

            _warningStyle.normal.textColor =
                theme.WarningText;

            _closeButtonStyle =
                new GUIStyle(
                    GUI.skin.button)
                {
                    fontSize = 10,

                    padding =
                        new RectOffset(
                            2,
                            2,
                            1,
                            1)
                };

            _closeButtonStyle.border = new RectOffset(7, 7, 7, 7);
            _closeButtonStyle.normal.background = roundedButtonBackground;
            _closeButtonStyle.hover.background = roundedButtonHover;
            _closeButtonStyle.active.background = roundedButtonHover;

            _closeButtonStyle.normal.textColor =
                theme.MutedText;

            _closeButtonStyle.hover.textColor =
                theme.AccentSecondary;

            // Match the close button's margins and height in the title bar.
            _settingsButtonStyle =
                new GUIStyle(_closeButtonStyle);

            _settingsButtonStyle.normal.textColor =
                theme.PrimaryText;

            _settingsButtonStyle.hover.textColor =
                theme.AccentPrimary;
            _settingsButtonStyle.border = new RectOffset(7, 7, 7, 7);
            _settingsButtonStyle.normal.background = roundedButtonBackground;
            _settingsButtonStyle.hover.background = roundedButtonHover;
            _settingsButtonStyle.active.background =
                _settingsButtonStyle.hover.background;

            // Both idle tabs use the same neutral style. The shared selected
            // style only appears while the corresponding panel is open.
            _stylesButtonStyle = new GUIStyle(_settingsButtonStyle);
            _stylesButtonStyle.normal.textColor = theme.AccentPrimary;
            _stylesButtonStyle.hover.textColor = theme.PrimaryText;
            _stylesButtonStyle.normal.background =
                CreateRoundedOutlineTexture(theme.ButtonBackground, theme.AccentPrimary);
            _stylesButtonStyle.hover.background =
                CreateRoundedOutlineTexture(theme.ButtonHoverBackground, theme.AccentPrimary);
            _stylesButtonStyle.active.background = _stylesButtonStyle.hover.background;

            _themeOptionStyle =
                new GUIStyle(_settingsButtonStyle);

            _themeOptionStyle.border =
                new RectOffset(7, 7, 7, 7);

            _themeOptionStyle.normal.background = roundedButtonBackground;
            _themeOptionStyle.hover.background = roundedButtonHover;

            _themeOptionStyle.active.background =
                _themeOptionStyle.hover.background;

            _addButtonStyle =
                new GUIStyle(
                    GUI.skin.button)
                {
                    fontSize = 10,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    padding =
                        new RectOffset(
                            5,
                            5,
                            2,
                            2)
                };

            _addButtonStyle.border = new RectOffset(7, 7, 7, 7);
            _addButtonStyle.normal.background = roundedButtonBackground;
            _addButtonStyle.hover.background = roundedButtonHover;
            _addButtonStyle.active.background = roundedButtonHover;

            _addButtonStyle.normal.textColor =
                theme.AccentPrimary;

            _addButtonStyle.hover.textColor =
                theme.PrimaryText;

            _pickerStyle =
                new GUIStyle(
                    GUI.skin.box)
                {
                    padding =
                        new RectOffset(
                            6,
                            6,
                            5,
                            5)
                };

            _pickerStyle.border =
                new RectOffset(
                    10,
                    10,
                    10,
                    10);

            _pickerStyle.normal.background =
                CreateRoundedTexture(
                    theme.PickerBackground,
                    8f);

            _pickerButtonStyle =
                new GUIStyle(
                    GUI.skin.button)
                {
                    fontSize = 9,

                    padding =
                        new RectOffset(
                            4,
                            4,
                            2,
                            2),

                    clipping =
                        TextClipping.Clip
                };

            _pickerButtonStyle.border =
                new RectOffset(
                    7,
                    7,
                    7,
                    7);

            _pickerButtonStyle.normal.background = roundedButtonBackground;
            _pickerButtonStyle.hover.background = roundedButtonHover;

            _pickerButtonStyle.active.background =
                _pickerButtonStyle.hover.background;

            _pickerButtonStyle.normal.textColor =
                theme.PrimaryText;

            _pickerButtonStyle.hover.textColor =
                theme.AccentPrimary;

            _readOnlyBoxStyle =
                new GUIStyle(
                    GUI.skin.box)
                {
                    padding =
                        new RectOffset(
                            7,
                            7,
                            5,
                            5)
                };

            _readOnlyBoxStyle.border =
                new RectOffset(
                    10,
                    10,
                    10,
                    10);

            _readOnlyBoxStyle.normal.background =
                CreateRoundedTexture(
                    theme.ReadOnlyBackground,
                    8f);

            _detailsButtonStyle =
                new GUIStyle(
                    GUI.skin.button)
                {
                    fontSize = 8,

                    padding =
                        new RectOffset(
                            4,
                            4,
                            1,
                            1)
                };

            _detailsButtonStyle.border = new RectOffset(7, 7, 7, 7);
            _detailsButtonStyle.normal.background = roundedButtonBackground;
            _detailsButtonStyle.hover.background = roundedButtonHover;
            _detailsButtonStyle.active.background = roundedButtonHover;

            _detailsButtonStyle.normal.textColor =
                theme.MutedText;

            _detailsButtonStyle.hover.textColor =
                theme.TargetText;

            _headerActionButtonStyle = new GUIStyle(_addButtonStyle)
            {
                margin = new RectOffset(2, 2, 0, 0)
            };

            Texture2D inputBackground = CreateRoundedTexture(theme.PickerBackground, 6f);
            Texture2D inputFocusedBackground =
                CreateRoundedOutlineTexture(theme.PickerBackground, theme.AccentPrimary);
            _styleTextFieldStyle = new GUIStyle(GUI.skin.textField);
            _styleTextFieldStyle.border = new RectOffset(7, 7, 7, 7);
            _styleTextFieldStyle.normal.background = inputBackground;
            _styleTextFieldStyle.focused.background = inputFocusedBackground;
            _styleTextFieldStyle.hover.background = inputBackground;
            _styleTextFieldStyle.normal.textColor = theme.PrimaryText;
            _styleTextFieldStyle.focused.textColor = theme.PrimaryText;
            _styleTextFieldStyle.hover.textColor = theme.PrimaryText;

            _styleTextAreaStyle = new GUIStyle(GUI.skin.textArea);
            _styleTextAreaStyle.border = new RectOffset(7, 7, 7, 7);
            _styleTextAreaStyle.normal.background = inputBackground;
            _styleTextAreaStyle.focused.background = inputFocusedBackground;
            _styleTextAreaStyle.hover.background = inputBackground;
            _styleTextAreaStyle.normal.textColor = theme.PrimaryText;
            _styleTextAreaStyle.focused.textColor = theme.PrimaryText;
            _styleTextAreaStyle.hover.textColor = theme.PrimaryText;

            _effectOnStyle =
                new GUIStyle(
                    GUI.skin.button)
                {
                    fontSize = 8,

                    fontStyle =
                        FontStyle.Bold,

                    alignment =
                        TextAnchor.MiddleCenter,

                    padding =
                        new RectOffset(
                            1,
                            1,
                            0,
                            0)
                };

            _effectOnStyle.border = new RectOffset(7, 7, 7, 7);
            _effectOnStyle.normal.background =
                CreateRoundedTexture(theme.EffectOnBackground);

            _effectOnStyle.hover.background =
                CreateRoundedTexture(theme.EffectOnHoverBackground);

            _effectOnStyle.active.background =
                _effectOnStyle.hover.background;

            _effectOnStyle.normal.textColor =
                theme.SuccessText;

            _effectOnStyle.hover.textColor =
                theme.PrimaryText;

            _effectOffStyle =
                new GUIStyle(
                    _effectOnStyle);

            _effectOffStyle.normal.background =
                CreateRoundedTexture(theme.EffectOffBackground);

            _effectOffStyle.hover.background =
                CreateRoundedTexture(theme.EffectOffHoverBackground);

            _effectOffStyle.active.background =
                _effectOffStyle.hover.background;

            _effectOffStyle.normal.textColor =
                theme.OffText;

            _effectOffStyle.hover.textColor =
                theme.PrimaryText;
        }

        private static void EnsureBrandTexture()
        {
            if (_brandLoadAttempted) return;
            _brandLoadAttempted = true;
            _brandTitleTexture = LoadAlphaMask(BrandResourceName, "NADA VFX Wordmark");
            _wandTexture = LoadAlphaMask(WandResourceName, "NADA VFX Stars");
        }

        private static Texture2D LoadAlphaMask(string resourceName, string textureName)
        {

            try
            {
                Assembly assembly = typeof(NadaVfxEditor).Assembly;

                using (Stream stream =
                       assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        Plugin.Log?.LogWarning(
                            $"{Plugin.ModName}: [EditorBrand] " +
                            $"embedded resource missing: {resourceName}");
                        return null;
                    }

                    using (var reader = new BinaryReader(stream))
                    {
                        int width = reader.ReadUInt16();
                        int height = reader.ReadUInt16();

                        if (width <= 0 || height <= 0 ||
                            width > 2048 || height > 2048)
                        {
                            throw new InvalidDataException(
                                "Invalid embedded wordmark dimensions.");
                        }

                        var pixels = new Color32[width * height];
                        int index = 0;

                        while (index < pixels.Length)
                        {
                            int runLength = reader.ReadByte();
                            byte alpha = reader.ReadByte();

                            if (runLength == 0 ||
                                runLength > pixels.Length - index)
                            {
                                throw new InvalidDataException(
                                    "Invalid embedded wordmark mask.");
                            }

                            for (int i = 0; i < runLength; i++)
                            {
                                int sourceIndex = index++;
                                int x = sourceIndex % width;
                                int y = sourceIndex / width;
                                int destinationIndex =
                                    (height - 1 - y) * width + x;

                                pixels[destinationIndex] =
                                    new Color32(255, 255, 255, alpha);
                            }
                        }

                        var texture = new Texture2D(
                            width, height, TextureFormat.RGBA32, false)
                        {
                            name = textureName,
                            hideFlags = HideFlags.HideAndDontSave,
                            filterMode = textureName == "NADA VFX Stars"
                                ? FilterMode.Point
                                : FilterMode.Bilinear,
                            wrapMode = TextureWrapMode.Clamp
                        };

                        texture.SetPixels32(pixels);
                        texture.Apply(false, true);

                        OwnedTextures.Add(texture);
                        Plugin.Log?.LogInfo(
                            $"{Plugin.ModName}: [EditorBrand] " +
                            $"embedded texture loaded: {textureName}.");
                        return texture;
                    }
                }
            }
            catch (Exception exception)
            {
                Plugin.Log?.LogWarning(
                    $"{Plugin.ModName}: [EditorBrand] " +
                    $"resource load failed: {exception.Message}");
            }
            return null;
        }

        private static void ResetVisualStyles()
        {
            foreach (Texture2D texture in
                     OwnedTextures)
            {
                if (texture == null)
                    continue;

                UnityEngine.Object.Destroy(
                    texture);
            }

            OwnedTextures.Clear();

            _brandTitleTexture = null;
            _wandTexture = null;
            _brandLoadAttempted = false;

            _windowStyle = null;

            _brandSubtitleStyle = null;

            _smallHeaderStyle = null;
            _targetCardStyle = null;
            _targetTitleStyle = null;
            _targetStatusStyle = null;
            _panelStyle = null;
            _sectionHeaderStyle = null;
            _settingsSubHeaderStyle = null;
            _blockStyle = null;
            _selectedBlockStyle = null;
            _blockLabelStyle = null;
            _disabledBlockLabelStyle = null;
            _inspectorTitleStyle = null;
            _inspectorAccentStyle = null;
            _bodyStyle = null;
            _mutedStyle = null;
            _warningStyle = null;
            _closeButtonStyle = null;
            _settingsButtonStyle = null;
            _stylesButtonStyle = null;
            _styleTextFieldStyle = null;
            _styleTextAreaStyle = null;
            _themeOptionStyle = null;
            _addButtonStyle = null;
            _pickerStyle = null;
            _pickerButtonStyle = null;
            _readOnlyBoxStyle = null;
            _detailsButtonStyle = null;
            _headerActionButtonStyle = null;
            _effectOnStyle = null;
            _effectOffStyle = null;

            _hasAppliedTheme =
                false;
        }

        private static Texture2D CreateRoundedTexture(
            Color32 color,
            float radius = 6f)
        {
            const int size = 20;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "NADA Rounded UI",
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(radius - (x + .5f), 0f);
                dx = Mathf.Max(dx, x + .5f - (size - radius));
                float dy = Mathf.Max(radius - (y + .5f), 0f);
                dy = Mathf.Max(dy, y + .5f - (size - radius));
                float coverage = Mathf.Clamp01(radius + .5f - Mathf.Sqrt(dx * dx + dy * dy));
                pixels[y * size + x] = new Color32(color.r, color.g, color.b,
                    (byte)(color.a * coverage));
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            OwnedTextures.Add(texture);
            return texture;
        }

        private static Texture2D CreateRoundedOutlineTexture(
            Color32 fill, Color32 stroke)
        {
            const int size = 20;
            const float radius = 6f;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "NADA Outlined Rounded UI",
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float outer = RoundedCoverage(x + .5f, y + .5f, size, radius);
                float inner = RoundedCoverage(x - .5f, y - .5f, size - 2, radius - 1f);
                float rim = Mathf.Clamp01(outer - inner);
                float alpha = inner * fill.a + rim * stroke.a;
                if (alpha <= 0f)
                    continue;
                // Premultiplied contribution gives a soft 1px outline.
                float r = (inner * fill.r * fill.a + rim * stroke.r * stroke.a) / alpha;
                float g = (inner * fill.g * fill.a + rim * stroke.g * stroke.a) / alpha;
                float b = (inner * fill.b * fill.a + rim * stroke.b * stroke.a) / alpha;
                pixels[y * size + x] = new Color32(
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

        private static float RoundedCoverage(float x, float y, int size, float radius)
        {
            float dx = Mathf.Max(radius - x, 0f);
            dx = Mathf.Max(dx, x - (size - radius));
            float dy = Mathf.Max(radius - y, 0f);
            dy = Mathf.Max(dy, y - (size - radius));
            return Mathf.Clamp01(radius + .5f - Mathf.Sqrt(dx * dx + dy * dy));
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
                        "NADA VFX Editor Runtime UI",

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

        private static void ActivatePreview(
            NadaVfxEditorTarget target)
        {
            if (NadaVfxEditorWorkingState.IsReadOnly)
            {
                NadaWeaponEditorPreviewState.Deactivate();
                NadaVfxEditorPreviewRigSession.ReleaseOwnedRig();
                return;
            }

            if (!_open ||
                target?.ItemData == null ||
                target.SourceRoot == null ||
                target.VisualRoot == null)
            {
                NadaWeaponEditorPreviewState
                    .Deactivate();

                NadaVfxEditorPreviewRigSession
                    .ReleaseOwnedRig();

                Plugin.Instance?
                    .RefreshExistingEquippedRigsOnly();

                return;
            }

            WeaponVfxState state =
                NadaVfxEditorWorkingState.State;

            if (state == null)
            {
                NadaWeaponEditorPreviewState
                    .Deactivate();

                NadaVfxEditorPreviewRigSession
                    .ReleaseOwnedRig();

                Plugin.Instance?
                    .RefreshExistingEquippedRigsOnly();

                return;
            }

            NadaWeaponEditorPreviewState
                .Activate(
                    target.ItemData,
                    state);

            if (!NadaVfxEditorPreviewRigSession.Apply(
                    target))
            {
                Plugin.Log?.LogWarning(
                    $"{Plugin.ModName}: [EditorPreview] " +
                    $"could not apply preview rig " +
                    $"for '{target.PrefabName}'.");
            }
        }

        private static void RefreshPreview()
        {
            if (!_open || NadaVfxEditorWorkingState.IsReadOnly)
                return;

            NadaVfxEditorTarget target =
                NadaVfxEditorTargetRegistry.Current;

            if (target?.ItemData == null ||
                target.SourceRoot == null ||
                target.VisualRoot == null)
            {
                return;
            }

            WeaponVfxState state =
                NadaVfxEditorWorkingState.State;

            if (state == null)
                return;

            NadaWeaponEditorPreviewState
                .Activate(
                    target.ItemData,
                    state);

            if (!NadaVfxEditorPreviewRigSession.Apply(
                    target))
            {
                Plugin.Log?.LogWarning(
                    $"{Plugin.ModName}: [EditorPreview] " +
                    $"could not refresh preview rig " +
                    $"for '{target.PrefabName}'.");
            }
        }
    }
}
