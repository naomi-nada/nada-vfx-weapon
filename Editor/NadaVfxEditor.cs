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
        private const int RestoreTabControlIdHint = 0x4E565254;
        private const float RestoreTabWidth = 150f;
        private const float RestoreTabHeight = 32f;
        private const float RestoreTabOpacity = 0.65f;
        private const float RestoreTabScreenMargin = 4f;
        private const float RestoreTabDragThreshold = 4f;

        private const string RenameControlName =
            "NadaVfxEditorRename";

        private const string NumericControlPrefix =
            "NadaVfxFloat_";

        private const float DefaultWindowWidth = 660f;
        private const float DefaultWindowHeight = 720f;

        private const float MinimumWindowWidth = 360f;
        // Match the window's 9px side/bottom inset between main cards.
        private const float MainCardGap = 9f;
        // The fixed header sits outside the editor scroll view. Keep its height
        // in one place so Edit fills the remaining viewport when resized.
        private const float EditorHeaderHeight = 56f;
        private const float TargetToolbarBottomSpace = 3f;
        // Effects determines the row height. This floor fits a collapsed
        // single-line Target without reserving extra empty panel space.
        private const float CompactTargetEffectsMinHeight = 116f;
        // Last Repaint measurement of the Effects card, not the scroll view's
        // spare height. Target follows this value on the next IMGUI layout.
        private static float _measuredEffectsHeight = CompactTargetEffectsMinHeight;
        // Settings/Styles share Edit's scroll region. Measure their actual
        // height so opening one never reduces Edit to a fixed 200px panel.
        private static float _measuredAuxiliaryPanelHeight;
        // Account for the scrollbar plus a small breathing gap beside the cards.
        private const float EditorScrollBarReserve = 24f;
        private const float MinimumWindowHeight = 360f;

        // Reuse measurement content; target names are drawn every IMGUI event.
        private static readonly GUIContent TargetNameMeasure = new GUIContent();

        private const float CompactLayoutBreakpoint = 500f;
        private const float LargeUiScale = 1.30f;
        private const float MinimumEditorOpacity = 0.35f;

        private static float UiScale =>
            NadaVfxEditorConfig.LargeUi?.Value == true ? LargeUiScale : 1f;

        // Bounds stay in logical IMGUI units; GUI.matrix projects them to pixels.
        private static float LogicalScreenWidth => Screen.width / UiScale;
        private static float LogicalScreenHeight => Screen.height / UiScale;


        private const float BlockHeight = 21f;
        private const float EffectChipWidth = 108f;
        private const float EffectChipGap = 4f;
        private const float EffectActionHeight = 23f;
        private const float EffectToggleWidth = 27f;

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

        private static Vector2 _editorScrollPosition;
        // Only a bounded set of encoded pre-edit snapshots is retained.
        // Encoding happens on input boundaries, never during Repaint/Layout
        // and never on every tick of a slider drag.
        private const int MaxUndoSteps = 20;

        private sealed class UndoEntry
        {
            internal byte[] Bytes;
            internal uint? SelectedEffectId;
        }

        private static readonly List<UndoEntry> UndoHistory = new();
        private static readonly List<UndoEntry> RedoHistory = new();
        private static global::ItemDrop.ItemData _undoOwner;
        private static UndoEntry _undoBeforeInput;
        private static bool _undoPointerGesture;
        private static bool _undoWasRestored;
        private static bool _pendingUndo;
        private static bool _pendingRedo;
        private static int _undoLastKeyboardControl;
        private static float _undoLastKeyboardTime;


        private static uint? _selectedInstanceId;
        private static uint? _renameInstanceId;
        private static uint? _resetConfirmInstanceId;
        private static uint? _deleteConfirmInstanceId;

        private static string _renameBuffer =
            string.Empty;

        private static bool _open;
        // Hidden is a temporary UI state, not a close: keep drafts and preview.
        private static bool _hidden;
        // The minimized NADA tab is positioned separately from the full editor.
        // Dragging it must never move or resize the editor window itself.
        private static bool _restoreTabPositionInitialized;
        private static Vector2 _restoreTabPosition;
        private static Vector2 _restoreTabPressMouse;
        private static Vector2 _restoreTabPressPosition;
        private static bool _restoreTabPointerDown;
        private static bool _restoreTabWasDragged;
        private static int _restoreTabControlId;
        private static bool _addPickerOpen;
        private static bool _positionInitialized;
        private static bool _targetDetailsExpanded;

        // True while this window owns editor interaction, not just a text field.
        // Left-clicking outside releases gameplay hotkeys without closing NADA.
        private static bool _editorSelected;
        private static bool _ownsKeyboardInput;
        private static bool _textInputClaimedThisDraw;
        private static bool _focusRenameOnNextDraw;
        private static uint? _pendingRenameInstanceId;
        private static string _pendingRenameDisplayName;
        private static global::ItemDrop.ItemData _pendingRenameItemData;

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
        private static Texture2D _neoKittenTexture;
        private static Texture2D _themeEmblemTexture;
        private static GUIStyle _opacityTrackStyle;
        private static GUIStyle _opacityThumbStyle;
        // GUI.Window can restore its own tint before invoking the content callback.
        // Hold the intended tint explicitly so the shell and children match.
        private static Color _windowContentTint = Color.white;
        // Negative means the slider is not being dragged/previewed.
        private static float _opacityPreview = -1f;
        private static float EditorOpacity => _opacityPreview >= 0f
            ? Mathf.Clamp(_opacityPreview, MinimumEditorOpacity, 1f)
            : Mathf.Clamp(NadaVfxEditorConfig.Opacity?.Value ?? 0.90f,
                MinimumEditorOpacity, 1f);
        private static GUIStyle _sizeTrackStyle;
        private static GUIStyle _sizeThumbStyle;
        private static GUIStyle _sizeLabelStyle;
        private static GUIStyle _sizeActiveLabelStyle;
        private static float _appliedUiScale = 1f;
        private static bool _settingsOpen;
        private static bool _clearAllConfirm;
        private static bool _pendingHandSwitch;
        private static bool _remotePlayerListExpanded;

        private enum ShortcutCapture { None, Toggle, Hide, Bind, Unbind }
        private static ShortcutCapture _shortcutCapture;
        private static string _shortcutError;

        private enum BindingAction { None, Bind, Unbind }
        private static BindingAction _pendingBindingAction;
        private static string _bindingError;
        private static string _bindingFeedback;
        private static float _bindingFeedbackUntil;
        private static global::ItemDrop.ItemData _bindingFeedbackItemData;

        private static uint? _pendingOrbitalSwapInstanceId;
        private static string _pendingOrbitalSwapTypeId;
        private static global::ItemDrop.ItemData _pendingOrbitalSwapItemData;
        private static uint? _orbitalVisualMenuInstanceId;
        private static Vector2 _orbitalVisualMenuScroll;

        private static NadaVfxEditorThemePreset
            _appliedThemePreset;

        private static bool
            _hasAppliedTheme;

        private static GUIStyle _windowStyle;
        private static GUIStyle _editorScrollStyle;
        private static GUISkin _editorScrollSkin;

        private static GUIStyle _brandSubtitleStyle;

        private static GUIStyle _smallHeaderStyle;
        private static GUIStyle _targetCardStyle;
        private static GUIStyle _settingsCardStyle;
        private static GUIStyle _targetTitleStyle;
        private static GUIStyle _targetNameWrappedStyle;
        private static GUIStyle _targetCaptionStyle;
        private static GUIStyle _detailsHeadingStyle;
        private static GUIStyle _detailLabelStyle;
        private static GUIStyle _detailValueStyle;
        private static string _hoveredTargetTooltip;
        private static GUIStyle _targetStatusStyle;
        private static GUIStyle _targetIconButtonStyle;
        private static GUIStyle _targetIconSelectedStyle;
        private static GUIStyle _hideGlyphStyle;
        private static GUIStyle _targetTooltipStyle;
        private static Texture2D[] _targetToolbarIcons;
        private static GUIStyle _panelStyle;
        private static GUIStyle _sectionHeaderStyle;
        private static GUIStyle _lowerSectionHeaderStyle;
        private static GUIStyle _settingsSubHeaderStyle;
        private static GUIStyle _blockStyle;
        private static GUIStyle _selectedBlockStyle;
        private static GUIStyle _blockLabelStyle;
        private static GUIStyle _disabledBlockLabelStyle;
        private static GUIStyle _inspectorTitleStyle;
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

        // Hiding minimizes only the UI. Keep the preview-camera/cursor session
        // alive so hiding the window never restores an old camera position.
        internal static bool IsOpen => _open;
        internal static bool IsVisible => _open && !_hidden;

        // Editor selection is the authority for gameplay hotkeys. A stale
        // IMGUI text control must not keep them blocked after click-away.
        internal static bool BlocksGameplayHotkeys =>
            IsVisible && _editorSelected;

        internal static bool OwnsKeyboardInput =>
            IsVisible && _editorSelected &&
            (_ownsKeyboardInput || _renameInstanceId.HasValue ||
             _shortcutCapture != ShortcutCapture.None);

        // Runtime input patches use this instead of opening normal
        // PlayerController input, which would also enable combat actions.
        internal static bool AllowsMovementWhileEditing =>
            IsOpen &&
            !BlocksGameplayHotkeys &&
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
                if (_hidden)
                    Show();
                else if (_open)
                    Close();
                else
                    Open();
                return;
            }

            // Hide/Show is optional and never intercepts active text entry.
            if (_open && _shortcutCapture == ShortcutCapture.None &&
                !OwnsKeyboardInput &&
                NadaVfxEditorConfig.HideHotkey != null &&
                NadaVfxEditorConfig.HideHotkey.Value.MainKey != KeyCode.None &&
                NadaVfxEditorConfig.HideHotkey.Value.IsDown())
            {
                if (_hidden)
                    Show();
                else
                    Hide();
                return;
            }

            if (!IsVisible)
            {
                // Hidden is a photo-preview state: retain a clickable restore
                // tab and do not resume the vanilla mouse-capture/camera path.
                EnforceCursorOwnership();
                return;
            }

            // Hotkeys share the button's deferred, target-validated action.
            // Never fire while entering text or capturing a new shortcut.
            if (_editorSelected && !OwnsKeyboardInput)
            {
                if (NadaVfxEditorConfig.BindHotkey != null &&
                    NadaVfxEditorConfig.BindHotkey.Value.MainKey != KeyCode.None &&
                    NadaVfxEditorConfig.BindHotkey.Value.IsDown() &&
                    !NadaVfxEditorWorkingState.IsReadOnly &&
                    NadaVfxEditorBindingController.HasBindableContent(
                        NadaVfxEditorWorkingState.State))
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
                return;

            EnsureStyles();
            EnsureWindowPosition();
            if (_hidden)
            {
                DrawRestoreTab();
                return;
            }

            float scale = UiScale;
            if (!Mathf.Approximately(_appliedUiScale, scale))
            {
                // Preserve the on-screen origin when changing modes.
                // Dimensions remain logical and therefore grow with the UI.
                _windowRect.x *= _appliedUiScale / scale;
                _windowRect.y *= _appliedUiScale / scale;
                _appliedUiScale = scale;
            }

            ClampWindowToScreen();
            UpdateWindowSelection();
            BeginUndoInput();

            Matrix4x4 previousMatrix = GUI.matrix;
            Color previousTint = GUI.color;
            try
            {
                // Window background draws here; GUI.Window may reset GUI.color
                // before drawing the callback, so DrawWindow reapplies this tint.
                _windowContentTint = new Color(previousTint.r, previousTint.g,
                    previousTint.b, previousTint.a * EditorOpacity);
                GUI.color = _windowContentTint;
                GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f)) * previousMatrix;
                _windowRect = GUI.Window(
                    WindowId,
                    _windowRect,
                    DrawWindow,
                    GUIContent.none,
                    _windowStyle);
            }
            finally
            {
                GUI.matrix = previousMatrix;
                GUI.color = previousTint;
            }

            // Only persist when the drag finishes; BepInEx may save on each
            // ConfigEntry change, so don't write on every MouseDrag repaint.
            if (_opacityPreview >= 0f && Event.current != null &&
                Event.current.rawType == EventType.MouseUp)
                CommitOpacityPreview();

            NadaVfxEditorStylesPanel.ProcessPendingDraftApply();
            ProcessPendingRenameRequest();
            ProcessPendingBindingAction();
            ProcessPendingOrbitalSwap();
            if (_pendingUndo)
            {
                _pendingUndo = false;
                UndoLastEdit();
            }
            else if (_pendingRedo)
            {
                _pendingRedo = false;
                RedoLastEdit();
            }
            if (_pendingHandSwitch)
            {
                _pendingHandSwitch = false;
                NadaVfxEditorTargetRegistry.SwitchHands();
                // SynchronizeTarget/preview transition on the next GUI draw;
                // changing the target inside GUI.Window risks IMGUI layout errors.
            }
            FinishUndoInput();
        }

        private static void UpdateWindowSelection()
        {
            Event current = Event.current;
            // Other IMGUI windows may consume MouseDown before our OnGUI.
            // rawType preserves the real click for selection ownership.
            if (current == null || current.rawType != EventType.MouseDown ||
                current.button != 0)
                return;

            bool selected = _windowRect.Contains(current.mousePosition / UiScale);
            if (_editorSelected == selected)
                return;

            _editorSelected = selected;
            if (!selected)
            {
                // Releasing the editor cancels unfinished transient entry,
                // without discarding the draft or closing the preview.
                if (_renameInstanceId.HasValue)
                    ClearTransientBlockActions();
                _shortcutCapture = ShortcutCapture.None;
                _ownsKeyboardInput = false;
                GUI.FocusControl(null);
            }

            Plugin.Log?.LogInfo(
                $"{Plugin.ModName}: [EditorInputSelection] " +
                $"selected={_editorSelected}");
        }

        // All editor text inputs claim the same keyboard owner on MouseDown.
        // This covers the click before IMGUI has committed its focused control.
        // The rect is in GUI.Window coordinates; never claim an outside click.
        internal static void ClaimTextInputClick(Rect fieldRect)
        {
            Event current = Event.current;
            if (!_open || !GUI.enabled || current == null ||
                current.rawType != EventType.MouseDown || current.button != 0 ||
                !fieldRect.Contains(current.mousePosition))
                return;

            if (!_editorSelected)
            {
                _editorSelected = true;
                Plugin.Log?.LogInfo(
                    $"{Plugin.ModName}: [EditorInputSelection] " +
                    "selected=true source=text-input-click");
            }

            _ownsKeyboardInput = true;
            _textInputClaimedThisDraw = true;
        }

        internal static void EnforceCursorOwnership()
        {
            if (!IsOpen)
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
            if (!IsVisible)
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
                    guiPosition / UiScale);
        }

        internal static void Close()
        {
            if (!_open)
                return;

            CommitOpacityPreview();
            SaveWindowBounds();

            _open =
                false;
            _hidden = false;
            ReleaseRestoreTabPointer();
            _editorSelected = false;

            _ownsKeyboardInput =
                false;
            _textInputClaimedThisDraw = false;

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
            _bindingFeedback = null;
            _bindingFeedbackItemData = null;
            _bindingFeedbackUntil = 0f;

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
            ResetUndoHistory(null);
        }

        private static void Open()
        {
            if (_open)
                return;

            _hidden = false;
            CaptureCursor();
            _cursorWindowStartTime = Time.unscaledTime;
            _cursorCorrectionsInWindow = 0;

            _open =
                true;
            _editorSelected = true;
            _bindingFeedback = null;
            _bindingFeedbackItemData = null;
            _bindingFeedbackUntil = 0f;

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
                _addEffectMenuScroll = Vector2.zero;

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

        private static void Hide()
        {
            if (!_open || _hidden)
                return;

            CommitOpacityPreview();
            _hidden = true;
            _editorSelected = false;
            _ownsKeyboardInput = false;
            _shortcutCapture = ShortcutCapture.None;
            _shortcutError = null;
            _resizing = false;
            _activeResizeEdges = ResizeEdges.None;
            ClearEffectDrag();
            // A hide-button MouseDown exits IMGUI before FinishUndoInput.
            // End that gesture without erasing previously committed history.
            _undoPointerGesture = false;
            _undoBeforeInput = null;
            if (Event.current != null)
                GUI.FocusControl(null);
            // Keep the preview camera and unlocked cursor owned until Close.
            // Restoring their opening-time state here caused a camera jump,
            // and the restored lock made the NADA tab impossible to click.
            EnforceCursorOwnership();
            Plugin.Log?.LogInfo($"{Plugin.ModName}: [Editor] hidden.");
        }

        private static void Show()
        {
            if (!_open || !_hidden)
                return;

            // The original cursor state is already captured by Open. Hiding
            // never releases ownership, so don't capture a second state.
            _hidden = false;
            ReleaseRestoreTabPointer();
            _editorSelected = true;
            _ownsKeyboardInput = false;
            _nextTargetRefreshTime = 0f;
            NadaVfxEditorTargetRegistry.RefreshFromRuntime();
            EnforceCursorOwnership();
            Plugin.Log?.LogInfo($"{Plugin.ModName}: [Editor] restored.");
        }

        private static Vector2 ClampRestoreTabPosition(Vector2 position)
        {
            return new Vector2(
                Mathf.Clamp(position.x, RestoreTabScreenMargin,
                    Mathf.Max(RestoreTabScreenMargin,
                        LogicalScreenWidth - RestoreTabWidth - RestoreTabScreenMargin)),
                Mathf.Clamp(position.y, RestoreTabScreenMargin,
                    Mathf.Max(RestoreTabScreenMargin,
                        LogicalScreenHeight - RestoreTabHeight - RestoreTabScreenMargin)));
        }

        private static void EnsureRestoreTabPosition()
        {
            if (!_restoreTabPositionInitialized)
            {
                int savedX = NadaVfxEditorConfig.RestoreTabX?.Value ?? -1;
                int savedY = NadaVfxEditorConfig.RestoreTabY?.Value ?? -1;
                _restoreTabPosition = new Vector2(
                    savedX >= 0 ? savedX : _windowRect.x + 6f,
                    savedY >= 0 ? savedY : _windowRect.y + 6f);
                _restoreTabPositionInitialized = true;
            }

            // Resolution or UI-size changes must never strand the restore tab.
            _restoreTabPosition = ClampRestoreTabPosition(_restoreTabPosition);
        }

        private static void SaveRestoreTabPosition()
        {
            // Preferences are stored once on release, never during MouseDrag.
            if (NadaVfxEditorConfig.RestoreTabX != null)
                NadaVfxEditorConfig.RestoreTabX.Value =
                    Mathf.RoundToInt(_restoreTabPosition.x);
            if (NadaVfxEditorConfig.RestoreTabY != null)
                NadaVfxEditorConfig.RestoreTabY.Value =
                    Mathf.RoundToInt(_restoreTabPosition.y);
        }

        private static void ReleaseRestoreTabPointer()
        {
            bool ownedPointer = _restoreTabPointerDown;
            _restoreTabPointerDown = false;
            _restoreTabWasDragged = false;
            // Do not release another IMGUI control after restoring the editor.
            if (ownedPointer && _restoreTabControlId != 0 &&
                GUIUtility.hotControl == _restoreTabControlId)
                GUIUtility.hotControl = 0;
        }

        private static void DrawRestoreTab()
        {
            // A click restores; a drag moves the minimized tab. No GUI.Window:
            // the tab has independent placement and does not move the editor.
            EnsureRestoreTabPosition();
            float scale = UiScale;
            Matrix4x4 previousMatrix = GUI.matrix;
            Color previousColor = GUI.color;
            try
            {
                Rect tabRect = new Rect(_restoreTabPosition.x, _restoreTabPosition.y,
                    RestoreTabWidth, RestoreTabHeight);
                // Stable control ID while the rectangle moves during a drag.
                int controlId = GUIUtility.GetControlID(
                    RestoreTabControlIdHint, FocusType.Passive);
                _restoreTabControlId = controlId;
                Event current = Event.current;
                // This is a top-level overlay. Coordinates in the editor are
                // logical units, while screen mouse coordinates are pixels.
                Vector2 mouse = current != null
                    ? current.mousePosition / scale : Vector2.zero;
                if (current != null)
                {
                    switch (current.GetTypeForControl(controlId))
                    {
                        case EventType.MouseDown:
                            if (current.button == 0 && tabRect.Contains(mouse))
                            {
                                GUIUtility.hotControl = controlId;
                                _restoreTabPointerDown = true;
                                _restoreTabWasDragged = false;
                                _restoreTabPressMouse = mouse;
                                _restoreTabPressPosition = _restoreTabPosition;
                                current.Use();
                            }
                            break;

                        case EventType.MouseDrag:
                            if (_restoreTabPointerDown && GUIUtility.hotControl == controlId)
                            {
                                Vector2 delta = mouse - _restoreTabPressMouse;
                                if (delta.sqrMagnitude >= RestoreTabDragThreshold * RestoreTabDragThreshold)
                                    _restoreTabWasDragged = true;
                                if (_restoreTabWasDragged)
                                    _restoreTabPosition = ClampRestoreTabPosition(
                                        _restoreTabPressPosition + delta);
                                current.Use();
                            }
                            break;

                        case EventType.MouseUp:
                            if (current.button == 0 && _restoreTabPointerDown &&
                                GUIUtility.hotControl == controlId)
                            {
                                bool wasDragged = _restoreTabWasDragged;
                                _restoreTabPointerDown = false;
                                _restoreTabWasDragged = false;
                                GUIUtility.hotControl = 0;
                                current.Use();
                                if (wasDragged)
                                    SaveRestoreTabPosition();
                                else
                                    Show();
                            }
                            break;
                    }
                }

                GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f)) * previousMatrix;
                GUI.color = new Color(previousColor.r, previousColor.g,
                    previousColor.b, previousColor.a * RestoreTabOpacity);
                // Labels do not take pointer ownership away from the drag control.
                Rect restoreRect = new Rect(_restoreTabPosition.x, _restoreTabPosition.y,
                    RestoreTabWidth, RestoreTabHeight);
                GUI.Label(restoreRect, GUIContent.none, _addButtonStyle);
                DrawRestoreTabBrand(restoreRect);
            }
            finally
            {
                GUI.matrix = previousMatrix;
                GUI.color = previousColor;
            }
        }

        private static void DrawRestoreTabBrand(Rect tabRect)
        {
            // Reuse the title's actual alpha-mask artwork and theme emblem.
            // No font-dependent emoji or new textures for the minimized view.
            EnsureBrandTexture();
            NadaVfxEditorTheme theme = NadaVfxEditorThemes.Current;
            bool stars = theme.TitleVariant == NadaVfxEditorTitleVariant.PixelClassic;
            Texture2D symbol = stars ? _wandTexture :
                theme.TitleVariant == NadaVfxEditorTitleVariant.PixelNeo
                    ? _neoKittenTexture : _themeEmblemTexture;
            float symbolSize = symbol == null ? 0f : stars ? 17f : 20f;
            const float gap = 4f;
            const float titleWidth = 107f;
            const float titleHeight = 23f;
            float totalWidth = titleWidth + (symbol == null ? 0f : gap + symbolSize);
            float x = tabRect.x + Mathf.Floor((tabRect.width - totalWidth) * .5f);
            float y = tabRect.y + Mathf.Floor((tabRect.height - titleHeight) * .5f);
            Rect titleRect = new Rect(x, y, titleWidth, titleHeight);
            Color original = GUI.color;
            try
            {
                if (_brandTitleTexture != null)
                {
                    Color shadow = theme.TitleShadow;
                    GUI.color = new Color(shadow.r, shadow.g, shadow.b,
                        shadow.a * original.a);
                    GUI.DrawTexture(new Rect(x + 1f, y + 1f, titleWidth, titleHeight),
                        _brandTitleTexture, ScaleMode.ScaleToFit, true);
                    Color primary = theme.TitlePrimary;
                    GUI.color = new Color(primary.r, primary.g, primary.b,
                        primary.a * original.a);
                    GUI.DrawTexture(titleRect, _brandTitleTexture,
                        ScaleMode.ScaleToFit, true);
                }
                else
                    GUI.Label(titleRect, "nada vfx", _targetTitleStyle);

                if (symbol != null)
                {
                    Color tint = stars ? theme.TitlePrimary : Color.white;
                    GUI.color = new Color(tint.r, tint.g, tint.b,
                        tint.a * original.a);
                    Rect symbolRect = new Rect(x + titleWidth + gap,
                        tabRect.y + Mathf.Floor((tabRect.height - symbolSize) * .5f),
                        symbolSize, symbolSize);
                    GUI.DrawTexture(symbolRect, symbol, ScaleMode.ScaleToFit, true);
                }
            }
            finally
            {
                GUI.color = original;
            }
        }

        private static void DrawWindow(int windowId)
        {
            Color callbackTint = GUI.color;
            try
            {
                // Unity draws the window shell and contents in different scopes.
                // Set the same absolute alpha here; never multiply twice.
                GUI.color = _windowContentTint;
                DrawWindowContents(windowId);
            }
            finally
            {
                GUI.color = callbackTint;
            }
        }

        private static void DrawWindowContents(int windowId)
        {
            // Tooltips are derived from the actual hovered button each draw.
            // Never reuse Unity's global GUI.tooltip, which may retain stale text.
            _hoveredTargetTooltip = null;
            _textInputClaimedThisDraw = false;

            // Capture before any focused text field can consume KeyDown.
            CaptureEditorShortcut();
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
                _addEffectMenuScroll = Vector2.zero;

                _targetDetailsExpanded =
                    false;
                _clearAllConfirm = false;
                NadaVfxEditorStylesPanel.OnTargetChanged();

                _editorScrollPosition =
                    Vector2.zero;
                _measuredEffectsHeight = CompactTargetEffectsMinHeight;
                _measuredAuxiliaryPanelHeight = 0f;

                ClearEffectDrag();
                ClearTransientBlockActions();

                ActivatePreview(
                    target);
            }

            // Keep the weapon identity and effect list visible while editing
            // long inspectors. Only Edit owns the scroll viewport.
            DrawTitleBar();
            GUILayout.Space(MainCardGap);

            EnsureSelection();
            DrawTargetAndEffects(target);
            GUILayout.Space(MainCardGap);

            // The scroll rail begins at Edit and ends at the window's bottom.
            // A private skin themes it without modifying vanilla GUI styles.
            GUISkin previousSkin = GUI.skin;
            if (_editorScrollSkin != null)
                GUI.skin = _editorScrollSkin;
            try
            {
                _editorScrollPosition = GUILayout.BeginScrollView(
                    _editorScrollPosition, false, true,
                    GUIStyle.none, GUI.skin.verticalScrollbar, _editorScrollStyle,
                    GUILayout.Width(FullEditorContentWidth),
                    GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true));

                // Settings/Styles share the lower scrollable workspace when
                // open. Target and Effects remain pinned and usable above.
                bool auxiliaryOpen = _settingsOpen || NadaVfxEditorStylesPanel.IsOpen;
                if (NadaVfxEditorStylesPanel.IsOpen)
                    NadaVfxEditorStylesPanel.Draw(target,
                        _settingsCardStyle, _sectionHeaderStyle, _bodyStyle,
                        _mutedStyle, _warningStyle, _addButtonStyle,
                        _themeOptionStyle, _styleTextFieldStyle, _styleTextAreaStyle,
                        EditorContentWidth);

                if (_settingsOpen)
                    DrawEditorSettings();

                if (auxiliaryOpen)
                {
                    // The panels end with their outer GUILayout group. Use
                    // their measured height on the next layout pass, just as
                    // we do for Effects, rather than a fixed Edit height.
                    if (Event.current.type == EventType.Repaint)
                    {
                        float height = GUILayoutUtility.GetLastRect().height;
                        if (height > 0f && !float.IsNaN(height) &&
                            !float.IsInfinity(height))
                            _measuredAuxiliaryPanelHeight = height;
                    }
                    GUILayout.Space(MainCardGap);
                }
                else
                    _measuredAuxiliaryPanelHeight = 0f;

                DrawInspectorPanel();
                GUILayout.EndScrollView();
            }
            finally
            {
                GUI.skin = previousSkin;
            }

            DrawResizeHandles();
            DrawTargetTooltip();

            UpdateKeyboardInputOwnership();

            if (Event.current.rawType ==
                EventType.MouseUp)
            {
                ClearEffectDrag();
            }

            if (!_resizing)
            {
                // Run last so buttons, inputs, effect drags and scrollbars
                // get first chance to consume MouseDown. Empty panel/window
                // space can move the editor without stealing those controls.
                // Keep the resize border separate from window dragging.
                GUI.DragWindow(new Rect(
                    ResizeEdgeThickness, ResizeEdgeThickness,
                    Mathf.Max(0f, _windowRect.width -
                        2f * ResizeEdgeThickness - EditorScrollBarReserve),
                    Mathf.Max(0f, _windowRect.height - 2f * ResizeEdgeThickness)));
            }
        }

        private static void ProcessPendingRenameRequest()
        {
            uint? instanceId = _pendingRenameInstanceId;
            string name = _pendingRenameDisplayName;
            global::ItemDrop.ItemData clickedItem = _pendingRenameItemData;

            _pendingRenameInstanceId = null;
            _pendingRenameDisplayName = null;
            _pendingRenameItemData = null;

            if (!instanceId.HasValue || clickedItem == null ||
                !object.ReferenceEquals(clickedItem,
                    NadaVfxEditorTargetRegistry.Current?.ItemData) ||
                NadaVfxEditorWorkingState.IsReadOnly ||
                _selectedInstanceId != instanceId)
                return;

            // Wait until GUI.Window has finished the click event. Showing the
            // new text field on that same event would change the layout after
            // Unity already assigned control IDs for its Layout pass.
            ClearTransientBlockActions();
            _renameInstanceId = instanceId;
            _renameBuffer = name ?? string.Empty;
            _focusRenameOnNextDraw = true;
            _ownsKeyboardInput = true;
        }

        private static void ProcessPendingOrbitalSwap()
        {
            uint? instanceId = _pendingOrbitalSwapInstanceId;
            string typeId = _pendingOrbitalSwapTypeId;
            global::ItemDrop.ItemData clickedItem = _pendingOrbitalSwapItemData;

            _pendingOrbitalSwapInstanceId = null;
            _pendingOrbitalSwapTypeId = null;
            _pendingOrbitalSwapItemData = null;

            if (!instanceId.HasValue || typeId == null || clickedItem == null ||
                !object.ReferenceEquals(clickedItem,
                    NadaVfxEditorTargetRegistry.Current?.ItemData) ||
                NadaVfxEditorWorkingState.IsReadOnly ||
                _selectedInstanceId != instanceId)
                return;

            if (!NadaVfxEditorWorkingState.TrySwapOrbitalType(
                    instanceId.Value, typeId))
                return;

            ClearTransientBlockActions();
            RefreshPreview();
        }

        private static void ProcessPendingBindingAction()
        {
            BindingAction requested = _pendingBindingAction;
            _pendingBindingAction = BindingAction.None;
            if (requested == BindingAction.None)
                return;

            _bindingFeedback = null;
            _bindingFeedbackItemData = null;

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

            // The vanilla center HUD can be obscured by other UI/messages.
            // Keep a short, item-scoped confirmation beside the editor action.
            _bindingFeedback = requested == BindingAction.Bind
                ? "Weapon VFX bound"
                : "Weapon VFX unbound";
            _bindingFeedbackItemData = liveTarget.ItemData;
            _bindingFeedbackUntil = Time.unscaledTime + 4f;

            global::MessageHud hud = global::MessageHud.instance;
            if (hud != null)
                hud.ShowMessage(global::MessageHud.MessageType.Center, _bindingFeedback);

            Plugin.Log?.LogInfo(
                $"{Plugin.ModName}: [EditorBindingFeedback] " +
                $"action={requested} hudAvailable={hud != null} " +
                $"item='{liveTarget.PrefabName}'.");
        }

        // Target/Effects use the full width; Edit alone reserves the scrollbar.
        private static float FullEditorContentWidth => Mathf.Max(0f,
            _windowRect.width - _windowStyle.padding.horizontal);

        private static float EditorContentWidth => Mathf.Max(0f,
            FullEditorContentWidth - EditorScrollBarReserve);

        private static float CompactTargetCardWidth =>
            Mathf.Clamp(EditorContentWidth * .255f, 94f, 116f);

        private static void DrawTitleBar()
        {
            EnsureBrandTexture();
            // The header isn't inside the scroll view, so it owns the full
            // padded window width rather than the scrollbar-reserved width.
            float width = _windowRect.width - _windowStyle.padding.horizontal;
            DrawBrandPanel(width, EditorHeaderHeight);
        }

        private static void DrawTargetAndEffects(NadaVfxEditorTarget target)
        {
            float contentWidth = FullEditorContentWidth;
            // Effects owns this row's height. Measure its real GUILayout card
            // after Repaint; the next layout uses that exact height for Target.
            // Never use an expanding empty rect here: scroll views can assign
            // their remaining viewport height to it, covering the Edit panel.
            float targetWidth = CompactTargetCardWidth;
            // This horizontal GUILayout group also inserts spacing between its
            // children. Reserve it so Effects ends at the same edge as Edit.
            float effectsWidth = contentWidth - targetWidth - 2f * MainCardGap;
            float minimumHeight = GetCompactTargetMinimumHeight(target, targetWidth);
            float targetHeight = Mathf.Max(minimumHeight, _measuredEffectsHeight);

            GUILayout.BeginHorizontal(GUIStyle.none,
                GUILayout.Width(contentWidth), GUILayout.MaxWidth(contentWidth),
                GUILayout.MinHeight(minimumHeight),
                GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
            GUILayout.BeginVertical(_targetCardStyle,
                GUILayout.Width(targetWidth), GUILayout.MaxWidth(targetWidth),
                GUILayout.Height(targetHeight), GUILayout.ExpandHeight(false));
            DrawTargetCardContents(target, targetWidth);
            GUILayout.EndVertical();
            GUILayout.Space(MainCardGap);
            DrawEffectsStrip(target, effectsWidth, true, minimumHeight);
            GUILayout.EndHorizontal();

            // Details are presentation only. Never enlarge Target or disrupt
            // the shared-height Target/Effects row to show long visual IDs.
            if (_targetDetailsExpanded)
                DrawTargetDetailsPanel(target, contentWidth);
        }

        private static void DrawBrandPanel(float width, float height)
        {
            // The wordmark is a brand anchor, while the actions belong to the
            // far right. Keep both in the non-scrolling header.
            const float outerPadding = 9f;
            const float toolbarWidth = 142f;
            float brandWidth = Mathf.Min(218f,
                Mathf.Max(140f, width - toolbarWidth - 2f * outerPadding - 10f));

            GUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height));
            GUILayout.Space(outerPadding);
            GUILayout.BeginVertical(GUILayout.Width(brandWidth));
            GUILayout.Space(6f);
            DrawBrandMark(brandWidth, true);
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical(GUILayout.Width(toolbarWidth));
            GUILayout.Space(8f);
            DrawBrandToolbar();
            GUILayout.EndVertical();
            GUILayout.Space(outerPadding);
            GUILayout.EndHorizontal();
        }

        private static void DrawBrandToolbar()
        {
            const float buttonSize = 28f;
            const float step = 38f;
            const float toolbarWidth = buttonSize + 3f * step;
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            Rect row = GUILayoutUtility.GetRect(toolbarWidth, buttonSize,
                GUILayout.Width(toolbarWidth), GUILayout.Height(buttonSize));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            Rect button = new Rect(row.x, row.y, buttonSize, buttonSize);

            if (DrawToolbarButton(button, 0, "Styles", NadaVfxEditorStylesPanel.IsOpen))
            {
                NadaVfxEditorStylesPanel.Toggle();
                if (NadaVfxEditorStylesPanel.IsOpen)
                {
                    _settingsOpen = false;
                    _themeDropdownOpen = false;
                }
            }

            button.x += step;
            if (DrawToolbarButton(button, 1, "Settings", _settingsOpen))
            {
                _settingsOpen = !_settingsOpen;
                if (_settingsOpen && NadaVfxEditorStylesPanel.IsOpen)
                    NadaVfxEditorStylesPanel.Toggle();
                if (!_settingsOpen)
                    _themeDropdownOpen = false;
            }

            button.x += step;
            if (DrawHideToolbarButton(button))
            {
                Hide();
                GUIUtility.ExitGUI();
            }

            button.x += step;
            if (DrawToolbarButton(button, 7, "Close", false))
            {
                Close();
                GUIUtility.ExitGUI();
            }
        }

        private static bool DrawHideToolbarButton(Rect rect)
        {
            if (rect.Contains(Event.current.mousePosition))
                _hoveredTargetTooltip = "Hide";
            bool clicked = GUI.Button(rect, GUIContent.none, _targetIconButtonStyle);
            GUI.Label(rect, "−", _hideGlyphStyle);
            return clicked;
        }

        private static void DrawBrandMark(float availableWidth, bool alignLeft = false)
        {
            const float titleHeight = 34f;
            NadaVfxEditorTheme titleTheme = NadaVfxEditorThemes.Current;
            bool stars = titleTheme.TitleVariant ==
                         NadaVfxEditorTitleVariant.PixelClassic;
            Texture2D symbol = stars ? _wandTexture :
                titleTheme.TitleVariant == NadaVfxEditorTitleVariant.PixelNeo
                    ? _neoKittenTexture : _themeEmblemTexture;
            float symbolSize = symbol == null ? 0f : stars ? 21f : 27f;
            float titleWidth = Mathf.Min(170f,
                Mathf.Max(100f, availableWidth - (symbol == null ? 0f : 4f + symbolSize)));

            GUILayout.BeginHorizontal(GUILayout.Height(titleHeight));
            if (!alignLeft) GUILayout.FlexibleSpace();
            Rect titleRect = GUILayoutUtility.GetRect(
                titleWidth, titleHeight,
                GUILayout.Width(titleWidth), GUILayout.Height(titleHeight));

            if (_brandTitleTexture != null)
            {
                Color original = GUI.color;
                try
                {
                    Color shadow = titleTheme.TitleShadow;
                    GUI.color = new Color(shadow.r, shadow.g, shadow.b,
                        shadow.a * original.a);
                    GUI.DrawTexture(new Rect(titleRect.x + 1f, titleRect.y + 1f,
                            titleRect.width, titleRect.height),
                        _brandTitleTexture, ScaleMode.ScaleToFit, true);
                    Color primary = titleTheme.TitlePrimary;
                    GUI.color = new Color(primary.r, primary.g, primary.b,
                        primary.a * original.a);
                    GUI.DrawTexture(titleRect, _brandTitleTexture,
                        ScaleMode.ScaleToFit, true);
                }
                finally { GUI.color = original; }
            }
            else
                GUI.Label(titleRect, "nada vfx", _targetTitleStyle);

            if (symbol != null)
            {
                GUILayout.Space(4f);
                Rect slot = GUILayoutUtility.GetRect(symbolSize, titleHeight,
                    GUILayout.Width(symbolSize), GUILayout.Height(titleHeight));
                Rect rect = new Rect(slot.x,
                    titleRect.y + Mathf.Floor((titleHeight - symbolSize) * .5f) + 1f,
                    symbolSize, symbolSize);
                Color previous = GUI.color;
                try
                {
                    Color tint = stars ? titleTheme.TitlePrimary : Color.white;
                    GUI.color = new Color(tint.r, tint.g, tint.b,
                        tint.a * previous.a);
                    GUI.DrawTexture(rect, symbol, ScaleMode.ScaleToFit, true);
                }
                finally { GUI.color = previous; }
            }
            if (!alignLeft) GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private static string GetTargetDisplayName(NadaVfxEditorTarget target,
            global::ItemDrop.ItemData item, string prefabName)
        {
            if (item == null)
                return "No equipped target";

            string name = target?.DisplayName;
            if (string.IsNullOrWhiteSpace(name))
            {
                name = item.m_shared?.m_name ?? prefabName;
                if (!string.IsNullOrEmpty(name) &&
                    name.StartsWith("$", StringComparison.Ordinal) &&
                    Localization.instance != null)
                    name = Localization.instance.Localize(name);
            }
            return name ?? string.Empty;
        }

        private static float GetCompactTargetMinimumHeight(
            NadaVfxEditorTarget target, float targetWidth)
        {
            global::ItemDrop.ItemData item =
                target?.ItemData ?? NadaVfxEditorTargetRegistry.DisplayOnlyHiddenItem();
            string prefabName = target?.PrefabName ??
                (item?.m_dropPrefab != null ? item.m_dropPrefab.name : string.Empty);
            TargetNameMeasure.text = GetTargetDisplayName(target, item, prefabName);
            float textWidth = Mathf.Max(1f,
                targetWidth - _targetCardStyle.padding.horizontal);
            float nameHeight = Mathf.Max(16f, Mathf.Ceil(
                _targetNameWrappedStyle.CalcHeight(TargetNameMeasure, textWidth)));

            // A wrapped name may need another line, but ordinary weapons keep
            // the tighter default. No guesswork based on the effect count.
            return CompactTargetEffectsMinHeight + Mathf.Max(0f, nameHeight - 16f);
        }

        private static void DrawTargetCardContents(NadaVfxEditorTarget target, float width)
        {
            global::ItemDrop.ItemData item =
                target?.ItemData ?? NadaVfxEditorTargetRegistry.DisplayOnlyHiddenItem();
            string prefabName = target?.PrefabName ??
                (item?.m_dropPrefab != null ? item.m_dropPrefab.name : string.Empty);
            string displayedName = GetTargetDisplayName(target, item, prefabName);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("TARGET", _targetCaptionStyle,
                GUILayout.Width(48f), GUILayout.Height(16f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // Both compact and expanded cards use their actual inside width.
            float nameWidth = Mathf.Max(1f,
                width - _targetCardStyle.padding.horizontal);
            TargetNameMeasure.text = displayedName;
            float nameHeight = Mathf.Max(16f, Mathf.Ceil(
                _targetNameWrappedStyle.CalcHeight(TargetNameMeasure, nameWidth)));
            GUILayout.Label(displayedName, _targetNameWrappedStyle,
                GUILayout.Width(nameWidth), GUILayout.Height(nameHeight));

            if (item != null)
            {
                // Balance the icon between the name and the actions without
                // letting the extra Effects row height spread these apart.
                // The sprite retains its transparent margins and aspect ratio.
                GUILayout.Space(2f);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                DrawTargetInventoryIcon(item, 32f);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(5f);
            }

            DrawTargetToolbarRow(target, item);

            GUILayout.Space(TargetToolbarBottomSpace);
        }

        private static void DrawTargetDetailsPanel(NadaVfxEditorTarget target, float availableWidth)
        {
            global::ItemDrop.ItemData item =
                target?.ItemData ?? NadaVfxEditorTargetRegistry.DisplayOnlyHiddenItem();
            if (item == null)
                return;

            string prefabName = target?.PrefabName ??
                (item.m_dropPrefab != null ? item.m_dropPrefab.name : string.Empty);
            NadaVfxEditorAppearance appearance =
                NadaVfxEditorVisualIdentity.Resolve(item, prefabName);

            // Full width like Edit; center the information, not just the title.
            GUILayout.Space(MainCardGap);
            GUILayout.BeginVertical(_targetCardStyle,
                GUILayout.Width(availableWidth), GUILayout.MaxWidth(availableWidth),
                GUILayout.ExpandWidth(false));
            GUILayout.Label("DETAILS", _detailsHeadingStyle,
                GUILayout.Height(18f));
            GUILayout.Space(3f);
            DrawCompactTargetInfo("Prefab ID", string.IsNullOrEmpty(prefabName)
                ? "Unknown" : prefabName, availableWidth);
            DrawCompactTargetInfo("Visual ID", appearance.Label, availableWidth);
            DrawCompactTargetInfo("Hand", NadaVfxEditorTargetRegistry.HandLabel,
                availableWidth);
            GUILayout.EndVertical();
        }

        private static void DrawCompactTargetInfo(string label, string value,
            float availableWidth)
        {
            const float labelWidth = 72f;
            const float gap = 10f;
            // Center the grid as a whole. Each column remains left-aligned so
            // labels and values share a consistent baseline and start edge.
            float valueWidth = Mathf.Min(248f, Mathf.Max(100f,
                availableWidth - _targetCardStyle.padding.horizontal - labelWidth - gap - 8f));
            // Measure a wrapping value instead of clipping long modded prefab IDs.
            var content = new GUIContent(value ?? "Unknown");
            float height = Mathf.Max(17f,
                _detailValueStyle.CalcHeight(content, valueWidth));
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(label, _detailLabelStyle,
                GUILayout.Width(labelWidth), GUILayout.Height(height));
            GUILayout.Space(gap);
            GUILayout.Label(content, _detailValueStyle,
                GUILayout.Width(valueWidth), GUILayout.Height(height));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private static void DrawTargetToolbarRow(
            NadaVfxEditorTarget target, global::ItemDrop.ItemData item)
        {
            const float buttonSize = 18f;
            const float step = 24f;
            const float toolbarWidth = buttonSize + 2f * step;
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            Rect row = GUILayoutUtility.GetRect(toolbarWidth, buttonSize,
                GUILayout.Width(toolbarWidth), GUILayout.Height(buttonSize));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            Rect button = new Rect(row.x, row.y, buttonSize, buttonSize);

            bool bound = target != null && target.IsReadOnly;
            bool invalid = target != null &&
                target.SourceKind == NadaWeaponLocalSourceKind.InvalidNative;
            int statusIcon = target == null ? 6 : bound ? 4 : 5;
            string status = item == null ? "No Target" : target == null
                ? "No Visual" : invalid ? "Invalid" :
                    bound ? "Bound" : "Unbound";
            DrawToolbarStatus(button, statusIcon, status);

            button.x += step;
            if (DrawToolbarButton(button, 2, "Details", _targetDetailsExpanded))
            {
                if (item != null)
                    _targetDetailsExpanded = !_targetDetailsExpanded;
            }

            button.x += step;
            if (DrawToolbarButton(button, 3, "Swap Hands", false))
                _pendingHandSwitch = true;
        }

        private static bool DrawToolbarButton(Rect rect, int iconIndex,
            string tooltip, bool selected)
        {
            // Hover is scoped to this exact rect, not GUI.tooltip, so it
            // clears immediately on mouse exit and doesn't leak between windows.
            if (rect.Contains(Event.current.mousePosition))
                _hoveredTargetTooltip = tooltip;
            bool clicked = GUI.Button(rect, GUIContent.none,
                selected ? _targetIconSelectedStyle : _targetIconButtonStyle);
            DrawToolbarArtwork(rect, iconIndex);
            return clicked;
        }

        private static void DrawToolbarStatus(Rect rect, int iconIndex, string tooltip)
        {
            if (rect.Contains(Event.current.mousePosition))
                _hoveredTargetTooltip = tooltip;
            GUI.Label(rect, GUIContent.none, _targetIconButtonStyle);
            DrawToolbarArtwork(rect, iconIndex);
        }

        private static void DrawToolbarArtwork(Rect rect, int iconIndex)
        {
            // Explicit scaling preserves the emoji artwork at 24px. Letting
            // GUIStyle lay out 64px textures can clip them inside small buttons.
            if (Event.current.type != EventType.Repaint)
                return;
            Rect imageRect = new Rect(rect.x + 2f, rect.y + 2f,
                rect.width - 4f, rect.height - 4f);
            Texture2D image = _targetToolbarIcons != null && iconIndex >= 0 &&
                              iconIndex < _targetToolbarIcons.Length
                ? _targetToolbarIcons[iconIndex] : null;
            if (image != null)
                GUI.DrawTexture(imageRect, image, ScaleMode.ScaleToFit, true);
            else
            {
                // Still usable if a resource wasn't embedded in the project.
                string[] fallback = { "S", "G", "D", "<> ", "L", "U", "?", "X" };
                GUI.Label(imageRect, fallback[iconIndex], _targetTitleStyle);
            }
        }

        private static void DrawTargetInventoryIcon(
            global::ItemDrop.ItemData item, float size)
        {
            Rect rect = GUILayoutUtility.GetRect(size, size,
                GUILayout.Width(size), GUILayout.Height(size));
            Sprite sprite = NadaVfxEditorTarget.GetOriginalInventoryIcon(item);
            if (sprite == null || sprite.texture == null ||
                Event.current.type != EventType.Repaint)
                return;

            try
            {
                // textureRect is trimmed by Unity; stretching it to the full
                // square distorts narrow icons (notably Dundr). Reconstruct
                // its position inside the original sprite rectangle instead.
                Rect source = sprite.textureRect;
                Rect full = sprite.rect;
                Vector2 offset = sprite.textureRectOffset;
                Texture2D texture = sprite.texture;
                if (full.width <= 0f || full.height <= 0f ||
                    source.width <= 0f || source.height <= 0f)
                    return;
                float fit = Mathf.Min(size / full.width, size / full.height);
                float fullWidth = full.width * fit;
                float fullHeight = full.height * fit;
                float left = rect.x + (size - fullWidth) * .5f;
                float top = rect.y + (size - fullHeight) * .5f;
                Rect draw = new Rect(left + offset.x * fit,
                    top + (full.height - offset.y - source.height) * fit,
                    source.width * fit, source.height * fit);
                Rect uv = new Rect(source.x / texture.width,
                    source.y / texture.height,
                    source.width / texture.width,
                    source.height / texture.height);
                GUI.DrawTextureWithTexCoords(draw, texture, uv, true);
            }
            catch (UnityException)
            {
                // Tight-packed sprites may not expose rectangular atlas data.
                // Never draw the full atlas as a fallback.
            }
        }

        private static void DrawTargetTooltip()
        {
            if (string.IsNullOrEmpty(_hoveredTargetTooltip) ||
                Event.current.type != EventType.Repaint)
                return;
            GUIContent content = new GUIContent(_hoveredTargetTooltip);
            float width = Mathf.Min(240f,
                _targetTooltipStyle.CalcSize(content).x + 6f);
            Vector2 mouse = Event.current.mousePosition;
            Rect rect = new Rect(
                Mathf.Clamp(mouse.x + 13f, 8f,
                    Mathf.Max(8f, _windowRect.width - width - 8f)),
                Mathf.Clamp(mouse.y + 17f, 8f,
                    Mathf.Max(8f, _windowRect.height - 32f)), width, 24f);
            GUI.Label(rect, content, _targetTooltipStyle);
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
            ResetUndoHistory(NadaVfxEditorTargetRegistry.Current?.ItemData);
            _selectedInstanceId = null;
            _clearAllConfirm = false;
            ClearTransientBlockActions();
            ClearEffectDrag();
            RefreshPreview();
        }

        private static bool _themeDropdownOpen;

        private static void DrawEditorSettings()
        {
            GUILayout.BeginVertical(_settingsCardStyle,
                GUILayout.Width(EditorContentWidth),
                GUILayout.MaxWidth(EditorContentWidth),
                GUILayout.ExpandWidth(false));

            GUILayout.Label("SETTINGS", _sectionHeaderStyle);

            GUILayout.Space(3f);
            GUILayout.Label("INTERFACE", _settingsSubHeaderStyle);
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

            GUILayout.Space(5f);
            DrawUiSizeSwitch();
            GUILayout.Space(10f);
            DrawEditorOpacitySlider();

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
            DrawShortcutSetting("Hide / Show Editor", NadaVfxEditorConfig.HideHotkey,
                ShortcutCapture.Hide);
            DrawShortcutSetting("Bind to Weapon", NadaVfxEditorConfig.BindHotkey,
                ShortcutCapture.Bind);
            DrawShortcutSetting("Unbind Weapon", NadaVfxEditorConfig.UnbindHotkey,
                ShortcutCapture.Unbind);
            if (!string.IsNullOrEmpty(_shortcutError))
                GUILayout.Label(_shortcutError, _warningStyle);

            GUILayout.EndVertical();
        }

        private static void DrawUiSizeSwitch()
        {
            if (NadaVfxEditorConfig.LargeUi == null)
                return;

            bool large = NadaVfxEditorConfig.LargeUi.Value;
            GUILayout.BeginHorizontal();
            GUILayout.Label("UI Size", _bodyStyle,
                GUILayout.Width(52f), GUILayout.Height(22f));

            Rect track = GUILayoutUtility.GetRect(178f, 22f,
                GUILayout.Width(178f), GUILayout.Height(22f));
            GUI.Box(track, GUIContent.none, _sizeTrackStyle);

            float halfWidth = (track.width - 4f) * 0.5f;
            Rect thumb = new Rect(
                track.x + (large ? halfWidth + 2f : 2f),
                track.y + 2f, halfWidth, track.height - 4f);
            GUI.Box(thumb, GUIContent.none, _sizeThumbStyle);

            GUI.Label(new Rect(track.x + 2f, track.y + 2f,
                    halfWidth, track.height - 4f),
                "Normal", large ? _sizeLabelStyle : _sizeActiveLabelStyle);
            GUI.Label(new Rect(track.x + halfWidth + 2f, track.y + 2f,
                    halfWidth, track.height - 4f),
                "Large", large ? _sizeActiveLabelStyle : _sizeLabelStyle);

            // The whole track is clickable; the thumb visibly moves between
            // the two positions. The new scale applies on the next OnGUI pass.
            if (GUI.Button(track, GUIContent.none, GUIStyle.none))
            {
                NadaVfxEditorConfig.LargeUi.Value = !large;
                Plugin.Log?.LogInfo(
                    $"{Plugin.ModName}: [EditorUiScale] " +
                    $"mode={(large ? "Normal" : "Large")} " +
                    $"scale={(large ? 1f : LargeUiScale):0.00}");
                GUI.FocusControl(null);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private static void DrawEditorOpacitySlider()
        {
            if (NadaVfxEditorConfig.Opacity == null)
                return;

            GUILayout.BeginHorizontal(GUILayout.Height(22f));
            GUILayout.Label("Opacity", _bodyStyle,
                GUILayout.Width(52f), GUILayout.Height(22f));

            Rect sliderArea = GUILayoutUtility.GetRect(178f, 22f,
                GUILayout.Width(178f), GUILayout.Height(22f));
            // IMGUI puts the short slider track at the top of its control rect.
            // Draw it centered independently, then use the full-height rect for
            // thumb alignment and drag hit testing.
            Rect track = new Rect(sliderArea.x, sliderArea.y + 8f,
                sliderArea.width, 6f);
            GUI.Box(track, GUIContent.none, _opacityTrackStyle);
            float next = GUI.HorizontalSlider(sliderArea, EditorOpacity,
                MinimumEditorOpacity, 1f, GUIStyle.none, _opacityThumbStyle);
            next = Mathf.Round(next * 100f) / 100f;
            if (!Mathf.Approximately(next, EditorOpacity))
                _opacityPreview = next;

            GUILayout.Space(8f);
            GUILayout.Label($"{Mathf.RoundToInt(EditorOpacity * 100f)}%",
                _mutedStyle, GUILayout.Width(40f), GUILayout.Height(22f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private static void CommitOpacityPreview()
        {
            if (_opacityPreview < 0f)
                return;

            float value = Mathf.Clamp(_opacityPreview, MinimumEditorOpacity, 1f);
            _opacityPreview = -1f;
            if (NadaVfxEditorConfig.Opacity != null &&
                !Mathf.Approximately(NadaVfxEditorConfig.Opacity.Value, value))
            {
                NadaVfxEditorConfig.Opacity.Value = value;
                Plugin.Log?.LogInfo(
                    $"{Plugin.ModName}: [EditorOpacity] value={value:0.00}");
            }
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
                        MatchesShortcut(defaultKey, NadaVfxEditorConfig.UnbindHotkey?.Value) ||
                        MatchesShortcut(defaultKey, NadaVfxEditorConfig.HideHotkey?.Value))
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
                (_shortcutCapture != ShortcutCapture.Hide &&
                 MatchesShortcut(candidate, NadaVfxEditorConfig.HideHotkey?.Value)) ||
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
                    : _shortcutCapture == ShortcutCapture.Hide
                        ? NadaVfxEditorConfig.HideHotkey
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
            GUILayout.Space(18f);
            GUILayout.Label("↳ Players", _mutedStyle,
                GUILayout.Width(100f), GUILayout.Height(20f));
            GUILayout.Space(6f);
            int hidden = NadaVfxRightHandRemoteStateTracker.HiddenPlayerCount;
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

            List<NadaVfxRightHandRemoteStateTracker.RemotePlayerChoice> players =
                NadaVfxRightHandRemoteStateTracker.GetVisiblePlayerChoices();
            GUILayout.BeginHorizontal();
            GUILayout.Space(142f);
            if (GUILayout.Button("Show All", _themeOptionStyle,
                    GUILayout.Width(92f), GUILayout.Height(20f)))
                NadaVfxRightHandRemoteStateTracker.ShowAllPlayers();
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
                GUILayout.Space(28f);
                GUILayout.Label(player.DisplayName, _bodyStyle,
                    GUILayout.Width(114f), GUILayout.Height(20f));
                if (GUILayout.Button(player.Hidden ? "Hidden" : "Visible",
                        _themeOptionStyle, GUILayout.Width(70f), GUILayout.Height(20f)))
                    NadaVfxRightHandRemoteStateTracker.SetPlayerHidden(player.PeerId, !player.Hidden);
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
            NadaVfxEditorTarget target, float width, bool compactRow = false,
            float minimumHeight = CompactTargetEffectsMinHeight)
        {
            // The compact row has one minimum height. Effects may increase it
            // as chips/pickers are added, and Target shares the resulting height.
            if (compactRow)
                GUILayout.BeginVertical(_panelStyle,
                    GUILayout.Width(width), GUILayout.MaxWidth(width),
                    GUILayout.MinHeight(minimumHeight),
                    GUILayout.ExpandHeight(false));
            else
                GUILayout.BeginVertical(_panelStyle,
                    GUILayout.Width(width), GUILayout.MaxWidth(width),
                    GUILayout.ExpandHeight(false));

            bool readOnly = NadaVfxEditorWorkingState.IsReadOnly;
            bool hasTarget = target?.ItemData != null;
            bool invalidNative = target?.SourceKind == NadaWeaponLocalSourceKind.InvalidNative;
            bool previousEnabled = GUI.enabled;

            if (readOnly)
            {
                _addPickerOpen = false;
                _clearAllConfirm = false;
            }

            _pendingReorderTargetInstanceId = null;
            const float actionWidth = 88f;
            const float columnGap = 5f;
            // Reserve the intrinsic IMGUI column spacing too; otherwise the
            // nested action row can force this card wider than the Edit card.
            float chipAreaWidth = width - _panelStyle.padding.horizontal -
                actionWidth - columnGap - 8f;
            WeaponVfxState state = NadaVfxEditorWorkingState.State;
            bool hasEffects = target != null && state?.Effects != null &&
                              state.Effects.Count > 0;
            int besideCount = 0;

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(
                GUILayout.Width(Mathf.Max(0f, chipAreaWidth)),
                GUILayout.MaxWidth(Mathf.Max(0f, chipAreaWidth)));
            GUILayout.Label("EFFECTS", _lowerSectionHeaderStyle,
                GUILayout.Height(18f));
            GUILayout.Space(2f);
            if (!hasEffects)
                GUILayout.Label(target == null ? "No target." : "No effects added.",
                    _mutedStyle);
            else if (chipAreaWidth >= EffectChipWidth)
            {
                // Only draw what fits beside the actions. Overflow continues
                // below across the complete width of the Effects panel.
                int perRow = Mathf.Max(1, Mathf.FloorToInt(
                    (chipAreaWidth + EffectChipGap) /
                    (EffectChipWidth + EffectChipGap)));
                besideCount = DrawEffectChipRange(state.Effects, 0,
                    perRow * 3, chipAreaWidth);
            }
            GUILayout.EndVertical();
            GUILayout.Space(columnGap);

            GUILayout.BeginVertical(GUILayout.Width(actionWidth));
            DrawEffectsAddButton(previousEnabled && hasTarget && !readOnly &&
                !invalidNative, actionWidth);
            GUILayout.Space(4f);
            DrawEffectsClearButton(previousEnabled && hasTarget && !readOnly &&
                !invalidNative && NadaVfxEditorWorkingState.EffectCount > 0,
                actionWidth);
            GUILayout.Space(4f);
            DrawEffectsBindingButton(previousEnabled && hasTarget &&
                !invalidNative && (readOnly ||
                NadaVfxEditorBindingController.HasBindableContent(
                    NadaVfxEditorWorkingState.State)), readOnly, actionWidth);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUI.enabled = previousEnabled;

            if (hasEffects)
            {
                int count = CountEffectChips(state.Effects);
                if (besideCount < count)
                {
                    GUILayout.Space(4f);
                    DrawEffectChipRange(state.Effects, besideCount,
                        count - besideCount,
                        width - _panelStyle.padding.horizontal);
                }
                ApplyPendingEffectReorder();
            }

            if (_addPickerOpen)
            {
                GUILayout.Space(4f);
                DrawCompactEffectPicker(target, width);
            }
            if (_clearAllConfirm && !readOnly)
            {
                GUILayout.Space(4f);
                DrawClearAllConfirmation();
            }
            // Binding confirmation already appears in the center HUD. Only
            // persistent read-only guidance belongs inside this card, tucked
            // against its lower-left edge rather than beneath the heading.
            if (readOnly || !string.IsNullOrEmpty(_bindingError))
            {
                GUILayout.FlexibleSpace();
                if (!string.IsNullOrEmpty(_bindingError))
                    GUILayout.Label(_bindingError, _warningStyle);
                else if (invalidNative)
                    GUILayout.Label("Saved native state is invalid. Editing is disabled.",
                        _warningStyle);
                else
                    GUILayout.Label("This weapon is bound. Unbind it to edit effects.",
                        _mutedStyle);
            }
            GUILayout.EndVertical();
            if (compactRow && Event.current.type == EventType.Repaint)
            {
                // After EndVertical, the last layout entry is the Effects card.
                // It includes chips, picker, warnings, and panel padding.
                float measuredHeight = GUILayoutUtility.GetLastRect().height;
                if (measuredHeight >= minimumHeight &&
                    !float.IsNaN(measuredHeight) &&
                    !float.IsInfinity(measuredHeight))
                    _measuredEffectsHeight = measuredHeight;
            }
        }

        private static void DrawEffectsAddButton(bool enabled, float width = 88f)
        {
            GUI.enabled = enabled;
            if (GUILayout.Button(_addPickerOpen ? "Close Add" : "+ Add Effect",
                    _addButtonStyle,
                    GUILayout.Width(width), GUILayout.Height(EffectActionHeight)))
            {
                ClearEffectDrag();
                _addPickerOpen = !_addPickerOpen;
                if (_addPickerOpen)
                    _addEffectMenuScroll = Vector2.zero;
            }
        }

        private static void DrawEffectsClearButton(bool enabled, float width = 74f)
        {
            GUI.enabled = enabled;
            if (GUILayout.Button("Clear All", _addButtonStyle,
                    GUILayout.Width(width), GUILayout.Height(EffectActionHeight)))
                _clearAllConfirm = true;
        }

        private static void DrawEffectsBindingButton(bool enabled, bool readOnly, float width = 106f)
        {
            GUI.enabled = enabled;
            if (GUILayout.Button(readOnly ? "Unbind Target" : "Bind to Target",
                    _addButtonStyle,
                    GUILayout.Width(width), GUILayout.Height(EffectActionHeight)))
            {
                _pendingBindingAction = readOnly ? BindingAction.Unbind : BindingAction.Bind;
            }
        }

        private static int CountEffectChips(System.Collections.Generic.List<VfxEffectBlock> effects)
        {
            int count = 0;
            foreach (VfxEffectBlock block in effects)
                if (block != null)
                    count++;
            return count;
        }

        // Renders a slice of the ordered effect list. The first slice lives
        // beside the action column; the remainder uses the whole panel width.
        // No arrays, state copies or extra allocation are needed for wrapping.
        private static int DrawEffectChipRange(
            System.Collections.Generic.List<VfxEffectBlock> effects,
            int startIndex, int maxCount, float availableWidth)
        {
            int perRow = Mathf.Max(1, Mathf.FloorToInt(
                (availableWidth + EffectChipGap) /
                (EffectChipWidth + EffectChipGap)));
            int seen = 0;
            int drawn = 0;
            int inRow = 0;
            bool rowOpen = false;

            foreach (VfxEffectBlock block in effects)
            {
                if (block == null)
                    continue;
                if (seen++ < startIndex)
                    continue;
                if (drawn >= maxCount)
                    break;

                if (!rowOpen)
                {
                    GUILayout.BeginHorizontal();
                    rowOpen = true;
                    inRow = 0;
                }
                DrawEffectChip(block);
                drawn++;
                inRow++;
                if (inRow >= perRow)
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    rowOpen = false;
                    if (drawn < maxCount)
                        GUILayout.Space(3f);
                }
                else
                    GUILayout.Space(EffectChipGap);
            }
            if (rowOpen)
            {
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            return drawn;
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
                    rowRect.y + 2f,
                    EffectToggleWidth,
                    rowRect.height - 4f);

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
                    rowRect.x + 3f,
                    rowRect.y + 2f,
                    10f,
                    rowRect.height - 4f);

            GUI.Label(
                gripRect,
                "::",
                _mutedStyle);

            Rect labelRect =
                new Rect(
                    rowRect.x + 12f,
                    rowRect.y + 2f,
                    Mathf.Max(0f, toggleRect.x - rowRect.x - 14f),
                    rowRect.height - 4f);

            string fullName = NadaVfxEditorWorkingState.GetDisplayName(block);
            GUI.Label(labelRect,
                GetEffectChipLabel(block, fullName, labelRect.width),
                block.Enabled ? _blockLabelStyle : _disabledBlockLabelStyle);

            // Chip width is intentionally compact. Expose the exact persisted
            // name on hover, especially for manually renamed effects.
            if (rowRect.Contains(Event.current.mousePosition))
                _hoveredTargetTooltip = fullName;

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

        private static readonly GUIContent EffectChipMeasure = new GUIContent();

        private static string GetEffectChipLabel(VfxEffectBlock block,
            string name, float availableWidth)
        {
            // Compact the generated label only. Never rename persisted blocks
            // or abbreviate a user's custom display name.
            NadaVfxEditorEffectDefinition definition =
                NadaVfxEditorEffectCatalog.Find(block.TypeId);
            string generatedBase = definition?.GeneratedNameBase;
            if (string.IsNullOrEmpty(generatedBase) ||
                string.IsNullOrEmpty(name) ||
                !name.StartsWith(generatedBase, StringComparison.Ordinal))
                return name;

            string ordinal = name.Substring(generatedBase.Length);
            if (ordinal.Length < 3)
                return name;
            foreach (char digit in ordinal)
                if (digit < '0' || digit > '9')
                    return name;

            string shortName;
            switch (generatedBase)
            {
                case "InnerFlames": shortName = "Inner"; break;
                case "OuterFlames": shortName = "Outer"; break;
                case "Strands": shortName = "Strands"; break;
                case "Sparks": shortName = "Sparks"; break;
                case "Appearance": shortName = "Appear"; break;
                case "Orbs": shortName = "Orbs"; break;
                case "Cores": shortName = "Cores"; break;
                case "Flames": shortName = "Flames"; break;
                case "Embers": shortName = "Embers"; break;
                default: shortName = generatedBase; break;
            }
            string compact = shortName + " " + ordinal;
            EffectChipMeasure.text = compact;
            if (_blockLabelStyle.CalcSize(EffectChipMeasure).x <= availableWidth)
                return compact;

            // At narrow widths, shorten the family instead of cutting off the
            // instance number. The full persisted name remains in the tooltip.
            switch (generatedBase)
            {
                case "InnerFlames": shortName = "In"; break;
                case "OuterFlames": shortName = "Out"; break;
                case "Strands": shortName = "Str"; break;
                case "Sparks": shortName = "Spk"; break;
                case "Appearance": shortName = "App"; break;
                case "Flames": shortName = "OrbFl"; break;
                case "Embers": shortName = "Emb"; break;
            }
            return shortName + " " + ordinal;
        }

        private static Vector2 _addEffectMenuScroll;

        private static void DrawCompactEffectPicker(
            NadaVfxEditorTarget target, float panelWidth)
        {
            if (target == null)
            {
                _addPickerOpen = false;
                return;
            }

            // A small File-menu-style list, pinned to the action column's
            // right edge. A single menu owns the vertical scrolling.
            float menuWidth = Mathf.Min(136f, Mathf.Max(100f,
                panelWidth - _panelStyle.padding.horizontal - 8f));
            // Respect both the panel's inner width and the picker's own
            // padding. Otherwise the scroll view can enlarge the Effects card
            // past its right edge when this menu is open.
            float innerWidth = panelWidth - _panelStyle.padding.horizontal;
            float scrollWidth = menuWidth - _pickerStyle.padding.horizontal;
            GUILayout.BeginHorizontal(GUIStyle.none, GUILayout.Width(innerWidth));
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical(_pickerStyle,
                GUILayout.Width(menuWidth), GUILayout.MaxWidth(menuWidth),
                GUILayout.ExpandWidth(false));
            _addEffectMenuScroll = GUILayout.BeginScrollView(_addEffectMenuScroll,
                false, true, GUIStyle.none, GUI.skin.verticalScrollbar,
                GUIStyle.none, GUILayout.Width(scrollWidth),
                GUILayout.MaxWidth(scrollWidth), GUILayout.Height(196f));
            DrawVerticalPickerGroup("STANDARD", false, scrollWidth - 18f);
            GUILayout.Space(5f);
            DrawVerticalPickerGroup("ORBITALS", true, scrollWidth - 18f);
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private static void DrawVerticalPickerGroup(
            string title, bool isOrbital, float availableWidth)
        {
            GUILayout.Label(title, _smallHeaderStyle, GUILayout.Height(15f));
            GUILayout.Space(1f);
            foreach (NadaVfxEditorEffectDefinition definition in
                     NadaVfxEditorEffectCatalog.All)
            {
                if (definition == null || definition.IsOrbital != isOrbital)
                    continue;
                DrawAddEffectButton(definition, availableWidth);
            }
        }

        private static void DrawAddEffectButton(
            NadaVfxEditorEffectDefinition definition,
            float buttonWidth)
        {
            if (definition == null)
                return;

            string pickerLabel = definition.PickerName;

            if (!GUILayout.Button(
                    pickerLabel,
                    _pickerButtonStyle,
                    GUILayout.Width(buttonWidth),
                    GUILayout.Height(19f)))
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

        private static void ResetUndoHistory(global::ItemDrop.ItemData owner)
        {
            UndoHistory.Clear();
            RedoHistory.Clear();
            _undoOwner = owner;
            _undoBeforeInput = null;
            _undoPointerGesture = false;
            _undoLastKeyboardControl = 0;
            _undoLastKeyboardTime = 0f;
            _pendingUndo = false;
            _pendingRedo = false;
        }

        private static bool TryCaptureUndo(out UndoEntry entry)
        {
            entry = null;
            global::ItemDrop.ItemData item = NadaVfxEditorTargetRegistry.Current?.ItemData;
            if (item == null || NadaVfxEditorWorkingState.IsReadOnly ||
                !NadaVfxEditorWorkingState.TryGetDraftForBinding(item,
                    out WeaponVfxState state, out uint cursor))
                return false;
            if (!WeaponVfxStateCodec.TryEncode(state, cursor,
                    out byte[] bytes, out string _))
                return false;
            entry = new UndoEntry { Bytes = bytes, SelectedEffectId = _selectedInstanceId };
            return true;
        }

        private static void BeginUndoInput()
        {
            global::ItemDrop.ItemData item = NadaVfxEditorTargetRegistry.Current?.ItemData;
            if (!object.ReferenceEquals(item, _undoOwner) ||
                (NadaVfxEditorWorkingState.IsReadOnly &&
                 (UndoHistory.Count > 0 || RedoHistory.Count > 0)))
                ResetUndoHistory(item);
            Event evt = Event.current;
            if (evt == null) return;
            if (evt.rawType == EventType.MouseDown)
            {
                _undoPointerGesture = TryCaptureUndo(out _undoBeforeInput);
            }
            else if (!_undoPointerGesture &&
                     (evt.rawType == EventType.KeyDown ||
                      evt.rawType == EventType.ScrollWheel))
            {
                TryCaptureUndo(out _undoBeforeInput);
            }
            _undoWasRestored = false;
        }

        private static void FinishUndoInput()
        {
            Event evt = Event.current;
            if (evt == null) return;
            bool pointerFinished = evt.rawType == EventType.MouseUp;
            bool keyboardFinished = evt.rawType == EventType.KeyDown;
            bool scrollFinished = evt.rawType == EventType.ScrollWheel;
            if (!pointerFinished && !keyboardFinished && !scrollFinished)
                return;
            // A slider drag remains one transaction even if typing or wheel
            // input occurs while the mouse is held down.
            if (_undoPointerGesture && !pointerFinished)
                return;

            UndoEntry before = _undoBeforeInput;
            if (!_undoWasRestored && before != null &&
                TryCaptureUndo(out UndoEntry after) &&
                !SameSnapshot(before.Bytes, after.Bytes))
            {
                // Any new authored change invalidates the redo branch, even
                // when rapid text input coalesces into one Undo entry.
                RedoHistory.Clear();
                bool coalesceText = keyboardFinished &&
                    GUIUtility.keyboardControl != 0 &&
                    GUIUtility.keyboardControl == _undoLastKeyboardControl &&
                    Time.unscaledTime - _undoLastKeyboardTime < 0.65f;
                if (!coalesceText)
                {
                    if (UndoHistory.Count == MaxUndoSteps)
                        UndoHistory.RemoveAt(0);
                    UndoHistory.Add(before);
                }
                _undoLastKeyboardControl = keyboardFinished ?
                    GUIUtility.keyboardControl : 0;
                _undoLastKeyboardTime = Time.unscaledTime;
            }
            if (pointerFinished)
            {
                _undoPointerGesture = false;
                _undoBeforeInput = null;
            }
            else if (!_undoPointerGesture)
                _undoBeforeInput = null;
        }

        private static bool SameSnapshot(byte[] first, byte[] second)
        {
            if (first == null || second == null || first.Length != second.Length)
                return false;
            for (int i = 0; i < first.Length; i++)
                if (first[i] != second[i]) return false;
            return true;
        }

        private static void PushHistory(List<UndoEntry> history, UndoEntry entry)
        {
            if (entry == null) return;
            if (history.Count == MaxUndoSteps)
                history.RemoveAt(0);
            history.Add(entry);
        }

        private static bool RestoreHistoryEntry(UndoEntry entry,
            UndoEntry current, string action)
        {
            global::ItemDrop.ItemData item =
                NadaVfxEditorTargetRegistry.Current?.ItemData;
            if (entry?.Bytes == null || current?.Bytes == null ||
                NadaVfxEditorWorkingState.IsReadOnly ||
                !object.ReferenceEquals(item, _undoOwner))
                return false;

            // Never roll the allocation cursor backwards. Undone IDs must
            // not become available for a future effect to accidentally reuse.
            if (!NadaVfxEditorWorkingState.TryGetDraftForBinding(item,
                    out WeaponVfxState _, out uint currentCursor))
                return false;
            if (!WeaponVfxStateCodec.TryDecode(entry.Bytes,
                    out WeaponVfxState restored, out uint cursor, out string reason) ||
                !NadaVfxEditorWorkingState.TryReplaceDraft(item,
                    restored, Math.Max(cursor, currentCursor), out reason))
            {
                Plugin.Log?.LogWarning($"{Plugin.ModName}: [Editor{action}] {reason}");
                return false;
            }

            _undoWasRestored = true;
            _undoBeforeInput = null;
            _undoPointerGesture = false;
            _undoLastKeyboardControl = 0;
            _selectedInstanceId = entry.SelectedEffectId;
            if (FindSelectedBlock() == null)
                _selectedInstanceId = null;
            ClearTransientBlockActions();
            ClearEffectDrag();
            RefreshPreview();
            return true;
        }

        private static void UndoLastEdit()
        {
            if (UndoHistory.Count == 0 || NadaVfxEditorWorkingState.IsReadOnly ||
                !object.ReferenceEquals(_undoOwner,
                    NadaVfxEditorTargetRegistry.Current?.ItemData) ||
                !TryCaptureUndo(out UndoEntry current))
                return;

            int index = UndoHistory.Count - 1;
            UndoEntry entry = UndoHistory[index];
            if (!RestoreHistoryEntry(entry, current, "Undo"))
                return;
            UndoHistory.RemoveAt(index);
            PushHistory(RedoHistory, current);
        }

        private static void RedoLastEdit()
        {
            if (RedoHistory.Count == 0 || NadaVfxEditorWorkingState.IsReadOnly ||
                !object.ReferenceEquals(_undoOwner,
                    NadaVfxEditorTargetRegistry.Current?.ItemData) ||
                !TryCaptureUndo(out UndoEntry current))
                return;

            int index = RedoHistory.Count - 1;
            UndoEntry entry = RedoHistory[index];
            if (!RestoreHistoryEntry(entry, current, "Redo"))
                return;
            RedoHistory.RemoveAt(index);
            PushHistory(UndoHistory, current);
        }

        private static void DrawInspectorPanel()
        {
            // Keep the empty Edit panel useful at normal window sizes, but
            // let effect inspectors grow naturally into the outer scroll view.
            // Match the scroll viewport exactly in the empty/default state.
            // A trailing subtraction here leaves a strip of scrollbar below Edit.
            // The header is outside the scroll view; subtract its *actual*
            // height, both gaps, and the shared Target/Effects row height.
            // Using an old header estimate left empty space below Edit whenever
            // the editor window was resized taller.
            float targetMinimum = GetCompactTargetMinimumHeight(
                NadaVfxEditorTargetRegistry.Current, CompactTargetCardWidth);
            float rowHeight = Mathf.Max(targetMinimum, _measuredEffectsHeight);
            float auxiliaryHeight = (_settingsOpen || NadaVfxEditorStylesPanel.IsOpen)
                ? _measuredAuxiliaryPanelHeight + MainCardGap : 0f;
            float minHeight = Mathf.Max(200f,
                _windowRect.height - _windowStyle.padding.vertical -
                EditorHeaderHeight - rowHeight - 2f * MainCardGap -
                auxiliaryHeight);
            GUILayout.BeginVertical(
                _panelStyle,
                GUILayout.Width(EditorContentWidth),
                GUILayout.MinHeight(minHeight),
                GUILayout.ExpandHeight(false));

            bool readOnly = NadaVfxEditorWorkingState.IsReadOnly;
            VfxEffectBlock selectedBlock = FindSelectedBlock();

            if (selectedBlock != null)
            {
                DrawSelectedEffectHeader(selectedBlock, readOnly);
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(readOnly ? "READ ONLY" : "EDIT",
                    readOnly ? _mutedStyle : _lowerSectionHeaderStyle);
                GUILayout.FlexibleSpace();
                if (NadaVfxEditorWorkingState.EffectCount > 0)
                {
                    DrawUndoButton();
                    GUILayout.Space(4f);
                    DrawRedoButton();
                }
                GUILayout.EndHorizontal();
            }

            if (readOnly &&
                !string.IsNullOrEmpty(NadaVfxEditorWorkingState.BoundViewError))
            {
                GUILayout.Label(NadaVfxEditorWorkingState.BoundViewError,
                    _warningStyle);
            }

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

            if (!readOnly)
                DrawTransientBlockControls(selectedBlock);

            // Keep a little separation between the action cluster and
            // the first inspector foldout, even with everything collapsed.
            GUILayout.Space(10f);

            NadaVfxEditorControls
                .SetCompactLayout(
                    IsCompactLayout);
            NadaVfxEditorControls.SetSectionLayout(
                CompactTargetCardWidth,
                EditorContentWidth - _panelStyle.padding.horizontal);

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

            GUILayout.EndVertical();
        }

        private static void DrawOrbitalTypeSelector(
            VfxEffectBlock block, bool readOnly)
        {
            NadaVfxEditorEffectDefinition selected =
                NadaVfxEditorEffectCatalog.Find(block.TypeId);
            if (selected?.IsOrbital != true)
            {
                _orbitalVisualMenuInstanceId = null;
                return;
            }

            const float menuWidth = 136f;
            const float rowHeight = 19f;
            bool menuOpen = _orbitalVisualMenuInstanceId == block.InstanceId;
            GUILayout.Space(5f);

            // Match the third action column exactly. FlexibleSpace plus
            // button margins can move this button beyond the grid edge.
            GUILayout.BeginHorizontal(GUILayout.Width(EditActionGroupWidth));
            GUILayout.Space(EditActionGroupWidth - EditActionButtonWidth);
            if (GUILayout.Button(selected.PickerName + (menuOpen ? " ▲" : " ▼"),
                    _headerActionButtonStyle,
                    GUILayout.Width(EditActionButtonWidth),
                    GUILayout.Height(EditActionButtonHeight)))
            {
                _orbitalVisualMenuInstanceId = menuOpen ? (uint?)null : block.InstanceId;
                _orbitalVisualMenuScroll = Vector2.zero;
                menuOpen = !menuOpen;
            }
            GUILayout.EndHorizontal();

            if (!menuOpen)
                return;

            int optionCount = 0;
            foreach (NadaVfxEditorEffectDefinition option in
                     NadaVfxEditorEffectCatalog.All)
                if (option != null && option.IsOrbital)
                    optionCount++;

            float listHeight = Mathf.Min(160f, optionCount * rowHeight + 8f);
            float scrollWidth = menuWidth - _pickerStyle.padding.horizontal;
            GUILayout.Space(3f);
            GUILayout.BeginHorizontal(GUILayout.Width(EditActionGroupWidth));
            GUILayout.Space(EditActionGroupWidth - menuWidth);
            GUILayout.BeginVertical(_pickerStyle, GUILayout.Width(menuWidth),
                GUILayout.MaxWidth(menuWidth), GUILayout.ExpandWidth(false));
            _orbitalVisualMenuScroll.x = 0f;
            _orbitalVisualMenuScroll = GUILayout.BeginScrollView(
                _orbitalVisualMenuScroll, false, true, GUIStyle.none,
                GUI.skin.verticalScrollbar, GUIStyle.none,
                GUILayout.Width(scrollWidth), GUILayout.MaxWidth(scrollWidth),
                GUILayout.Height(listHeight));

            bool previousEnabled = GUI.enabled;
            foreach (NadaVfxEditorEffectDefinition option in
                     NadaVfxEditorEffectCatalog.All)
            {
                if (option == null || !option.IsOrbital)
                    continue;
                bool isCurrent = option.TypeId == block.TypeId;
                GUI.enabled = previousEnabled && !readOnly && !isCurrent;
                if (GUILayout.Button(option.PickerName, _pickerButtonStyle,
                        GUILayout.Width(scrollWidth - 18f),
                        GUILayout.Height(rowHeight)))
                {
                    _pendingOrbitalSwapInstanceId = block.InstanceId;
                    _pendingOrbitalSwapTypeId = option.TypeId;
                    _pendingOrbitalSwapItemData =
                        NadaVfxEditorTargetRegistry.Current?.ItemData;
                    _orbitalVisualMenuInstanceId = null;
                }
            }
            GUI.enabled = previousEnabled;
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private static void DrawSelectedEffectHeader(VfxEffectBlock block, bool readOnly)
        {
            // Keep the action grid pinned to the top-right at *every* width.
            // The old compact layout stacked it after the title and left a
            // large blank band above the buttons when the window narrowed.
            float innerWidth = EditorContentWidth - _panelStyle.padding.horizontal;
            const float columnGap = 5f;
            float identityWidth = Mathf.Max(0f,
                innerWidth - EditActionGroupWidth - columnGap);

            GUILayout.BeginHorizontal(GUILayout.Width(innerWidth));
            GUILayout.BeginVertical(GUILayout.Width(identityWidth));
            GUILayout.Label(readOnly ? "READ ONLY" : "EDIT",
                readOnly ? _mutedStyle : _lowerSectionHeaderStyle,
                GUILayout.Width(identityWidth));
            GUILayout.Space(3f);
            DrawEffectIdentity(block, identityWidth);
            GUILayout.EndVertical();

            GUILayout.Space(columnGap);
            GUILayout.BeginVertical(GUILayout.Width(EditActionGroupWidth));
            DrawBlockActionButtons(block);
            DrawOrbitalTypeSelector(block, readOnly);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private static void DrawEffectIdentity(VfxEffectBlock block, float width)
        {
            // Constrain long user names to the identity column rather than
            // letting their GUILayout minimum push the action grid downward.
            GUILayout.Label(
                NadaVfxEditorWorkingState.GetDisplayName(block),
                _inspectorTitleStyle, GUILayout.Width(width));
            GUILayout.Label(
                GetFriendlyTypeName(block.TypeId),
                _mutedStyle, GUILayout.Width(width));
        }

        private const float EditActionButtonWidth = 64f;
        private const float EditActionButtonHeight = 23f;
        private const float EditActionGap = 4f;
        // Explicit button spacing makes the grid and selector share one
        // right edge regardless of IMGUI's layout margins or editor width.
        private const float EditActionGroupWidth =
            3f * EditActionButtonWidth + 2f * EditActionGap;

        private static void DrawUndoButton()
        {
            bool previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && !NadaVfxEditorWorkingState.IsReadOnly &&
                UndoHistory.Count > 0 &&
                object.ReferenceEquals(_undoOwner,
                    NadaVfxEditorTargetRegistry.Current?.ItemData);
            if (GUILayout.Button("Undo", _headerActionButtonStyle,
                    GUILayout.Width(EditActionButtonWidth),
                    GUILayout.Height(EditActionButtonHeight)))
                _pendingUndo = true;
            GUI.enabled = previousEnabled;
        }

        private static void DrawRedoButton()
        {
            bool previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && !NadaVfxEditorWorkingState.IsReadOnly &&
                RedoHistory.Count > 0 &&
                object.ReferenceEquals(_undoOwner,
                    NadaVfxEditorTargetRegistry.Current?.ItemData);
            if (GUILayout.Button("Redo", _headerActionButtonStyle,
                    GUILayout.Width(EditActionButtonWidth),
                    GUILayout.Height(EditActionButtonHeight)))
                _pendingRedo = true;
            GUI.enabled = previousEnabled;
        }

        private static void DrawBlockActionButtons(VfxEffectBlock block)
        {
            // Caller owns the fixed-width column shared with Orbitals Visual.
            GUILayout.BeginHorizontal(GUILayout.Width(EditActionGroupWidth));
            bool previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && !NadaVfxEditorWorkingState.IsReadOnly;
            try
            {
                if (GUILayout.Button("Duplicate", _headerActionButtonStyle,
                        GUILayout.Width(EditActionButtonWidth),
                        GUILayout.Height(EditActionButtonHeight)))
                {
                    if (NadaVfxEditorWorkingState.TryDuplicateEffect(block.InstanceId,
                            out VfxEffectBlock duplicate))
                    {
                        SelectBlock(duplicate.InstanceId);
                        RefreshPreview();
                    }
                }

                GUILayout.Space(EditActionGap);
                if (GUILayout.Button("Reset", _headerActionButtonStyle,
                        GUILayout.Width(EditActionButtonWidth),
                        GUILayout.Height(EditActionButtonHeight)))
                {
                    _resetConfirmInstanceId = block.InstanceId;
                    _renameInstanceId = null;
                    _renameBuffer = string.Empty;
                    _deleteConfirmInstanceId = null;
                }

                GUILayout.Space(EditActionGap);
                if (GUILayout.Button("Delete", _headerActionButtonStyle,
                        GUILayout.Width(EditActionButtonWidth),
                        GUILayout.Height(EditActionButtonHeight)))
                {
                    _deleteConfirmInstanceId = block.InstanceId;
                    _renameInstanceId = null;
                    _renameBuffer = string.Empty;
                    _resetConfirmInstanceId = null;
                }
            }
            finally
            {
                GUI.enabled = previousEnabled;
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(4f);
            GUILayout.BeginHorizontal();
            DrawUndoButton();
            GUILayout.Space(EditActionGap);
            DrawRedoButton();
            GUILayout.Space(EditActionGap);
            previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && !NadaVfxEditorWorkingState.IsReadOnly;
            if (GUILayout.Button("Rename", _headerActionButtonStyle,
                    GUILayout.Width(EditActionButtonWidth),
                    GUILayout.Height(EditActionButtonHeight)))
            {
                _pendingRenameInstanceId = block.InstanceId;
                _pendingRenameDisplayName =
                    NadaVfxEditorWorkingState.GetDisplayName(block);
                _pendingRenameItemData =
                    NadaVfxEditorTargetRegistry.Current?.ItemData;
            }
            GUI.enabled = previousEnabled;
            GUILayout.EndHorizontal();
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

                ClaimTextInputClick(GUILayoutUtility.GetLastRect());
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

            ClaimTextInputClick(GUILayoutUtility.GetLastRect());
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

            ClearTransientBlockActions();
        }

        private static void ClearTransientBlockActions()
        {
            _pendingRenameInstanceId = null;
            _pendingRenameDisplayName = null;
            _pendingRenameItemData = null;

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
            if (!_editorSelected)
            {
                // IMGUI may retain a named control until a later repaint.
                // It cannot reacquire input ownership while deselected.
                _ownsKeyboardInput = false;
                return;
            }

            // Don't clear a fresh click claim during the same GUI event.
            // On the next draw, focused-control detection takes over.
            if (_textInputClaimedThisDraw)
                return;

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
                        delta / UiScale);

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
                    LogicalScreenWidth);

            float minimumHeight =
                Mathf.Min(
                    MinimumWindowHeight,
                    LogicalScreenHeight);

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
                        LogicalScreenWidth);

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
                        LogicalScreenHeight);

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
                ? Mathf.Max(20f, LogicalScreenWidth - _windowRect.width - 24f)
                : x;
            _windowRect.y = y < 0 ? 64f : y;
            _appliedUiScale = UiScale;
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
            float maximumWidth = Mathf.Max(1f, LogicalScreenWidth);
            float maximumHeight = Mathf.Max(1f, LogicalScreenHeight);

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
                        LogicalScreenWidth -
                        _windowRect.width));

            _windowRect.y =
                Mathf.Clamp(
                    _windowRect.y,
                    0f,
                    Mathf.Max(
                        0f,
                        LogicalScreenHeight -
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

            _sizeTrackStyle = new GUIStyle(GUI.skin.box)
            {
                border = new RectOffset(7, 7, 7, 7),
                padding = new RectOffset(0, 0, 0, 0)
            };
            _sizeTrackStyle.normal.background =
                CreateRoundedOutlineTexture(theme.SegmentBackground, buttonStroke);
            _sizeThumbStyle = new GUIStyle(_sizeTrackStyle);
            _sizeThumbStyle.normal.background = CreateRoundedTexture(theme.AccentPrimary);
            _sizeLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 9, alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(0, 0, 0, 0)
            };
            _sizeLabelStyle.normal.textColor = theme.MutedText;
            _sizeActiveLabelStyle = new GUIStyle(_sizeLabelStyle);
            _sizeActiveLabelStyle.normal.textColor = theme.WindowBackground;

            _opacityTrackStyle = new GUIStyle(GUI.skin.horizontalSlider)
            {
                border = new RectOffset(7, 7, 7, 7),
                fixedHeight = 6f
            };
            _opacityTrackStyle.normal.background =
                CreateRoundedTexture(theme.SegmentBackground);
            _opacityThumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb)
            {
                border = new RectOffset(7, 7, 7, 7),
                fixedWidth = 13f,
                fixedHeight = 18f
            };
            _opacityThumbStyle.normal.background =
                CreateRoundedTexture(theme.AccentPrimary);
            _opacityThumbStyle.hover.background =
                CreateRoundedTexture(theme.AccentTertiary);

            if (theme.TitleVariant == NadaVfxEditorTitleVariant.PixelNeo)
                _neoKittenTexture = CreateNeoKittenTexture(theme);
            else if (theme.TitleVariant != NadaVfxEditorTitleVariant.PixelClassic)
                _themeEmblemTexture = CreateThemeEmblemTexture(theme);

            _windowStyle =
                new GUIStyle(
                    GUI.skin.window)
                {
                    padding =
                        new RectOffset(
                            9,
                            9,
                            10,
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

            _editorScrollStyle = new GUIStyle(GUI.skin.scrollView)
            {
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0)
            };
            _editorScrollStyle.normal.background = null;

            // GUISkin supplies the thumb / arrow styles that GUILayout's
            // built-in scroll view otherwise takes from the vanilla skin.
            // A private skin keeps this theming confined to our editor.
            _editorScrollSkin = UnityEngine.Object.Instantiate(GUI.skin);
            _editorScrollSkin.name = "NADA Editor Scroll Skin";
            _editorScrollSkin.hideFlags = HideFlags.HideAndDontSave;
            GUIStyle scrollbarTrack = new GUIStyle(GUI.skin.verticalScrollbar);
            scrollbarTrack.normal.background =
                CreateRoundedTexture(theme.PanelBackground, 4f);
            scrollbarTrack.hover.background = scrollbarTrack.normal.background;
            _editorScrollSkin.verticalScrollbar = scrollbarTrack;

            GUIStyle scrollbarThumb = new GUIStyle(GUI.skin.verticalScrollbarThumb);
            scrollbarThumb.normal.background =
                CreateRoundedTexture(theme.ButtonBackground, 4f);
            scrollbarThumb.hover.background =
                CreateRoundedTexture(theme.ButtonHoverBackground, 4f);
            scrollbarThumb.active.background = scrollbarThumb.hover.background;
            _editorScrollSkin.verticalScrollbarThumb = scrollbarThumb;

            GUIStyle scrollbarUp = new GUIStyle(GUI.skin.verticalScrollbarUpButton);
            scrollbarUp.normal.background =
                CreateRoundedTexture(theme.PanelBackground, 4f);
            scrollbarUp.hover.background = scrollbarThumb.hover.background;
            _editorScrollSkin.verticalScrollbarUpButton = scrollbarUp;
            GUIStyle scrollbarDown = new GUIStyle(GUI.skin.verticalScrollbarDownButton);
            scrollbarDown.normal.background = scrollbarUp.normal.background;
            scrollbarDown.hover.background = scrollbarThumb.hover.background;
            _editorScrollSkin.verticalScrollbarDownButton = scrollbarDown;

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
                    padding = new RectOffset(8, 8, 8, 8),
                    margin = new RectOffset(0, 0, 0, 0)
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

            // Settings is a separate surface so two theme colors stay distinct.
            // It inherits the same sizing/border as the target card.
            _targetToolbarIcons = CreateTargetToolbarIcons();
            _targetIconButtonStyle = new GUIStyle(_detailsButtonStyle ?? GUI.skin.button)
            {
                padding = new RectOffset(1, 1, 1, 1),
                alignment = TextAnchor.MiddleCenter,
                imagePosition = ImagePosition.ImageOnly
            };
            // Created after the button styles below are built; restyle there.
            _targetIconSelectedStyle = new GUIStyle(_targetIconButtonStyle);
            _targetTooltipStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 10,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(5, 5, 3, 3)
            };
            _targetTooltipStyle.normal.background =
                CreateRoundedTexture(theme.PickerBackground, 5f);
            _targetTooltipStyle.normal.textColor = theme.PrimaryText;

            _settingsCardStyle = new GUIStyle(_targetCardStyle);
            _settingsCardStyle.normal.background =
                CreateRoundedTexture(theme.SettingsBackground, 8f);

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
                NadaVfxEditorContrast.TextOn(theme.TargetBackground,
                    theme.TargetText, theme.AccentTertiary,
                    theme.PrimaryText);

            _hideGlyphStyle = new GUIStyle(_targetTitleStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 19,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(0, 0, 0, 1)
            };
            _hideGlyphStyle.normal.textColor = theme.PrimaryText;

            _targetNameWrappedStyle = new GUIStyle(_targetTitleStyle)
            {
                wordWrap = true,
                alignment = TextAnchor.UpperCenter
            };

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
                new GUIStyle(GUI.skin.box)
                {
                    padding = new RectOffset(7, 7, 7, 7),
                    margin = new RectOffset(0, 0, 0, 0)
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
                NadaVfxEditorContrast.TextOn(theme.SettingsBackground,
                    theme.AccentSecondary, theme.AccentTertiary,
                    theme.PrimaryText);

            // Distinguish headers on the sandstone/blue lower cards without
            // changing Settings, Styles or inspector subsection accents.
            _lowerSectionHeaderStyle = new GUIStyle(_sectionHeaderStyle);
            _lowerSectionHeaderStyle.normal.textColor =
                NadaVfxEditorContrast.TextOnBoth(theme.PanelBackground,
                    theme.TargetBackground, theme.LowerSectionHeadingText,
                    theme.AccentTertiary, theme.PrimaryText);

            // Compact label needs a clean text baseline at small font sizes.
            _targetCaptionStyle = new GUIStyle(_lowerSectionHeaderStyle)
            {
                padding = new RectOffset(0, 0, 0, 0),
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Overflow
            };

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

                    // Long effect names can wrap in the left identity column
                    // instead of forcing the top-right action grid to reflow.
                    wordWrap = true,

                    padding =
                        new RectOffset(
                            0,
                            0,
                            0,
                            0)
                };

            _inspectorTitleStyle.normal.textColor =
                NadaVfxEditorContrast.TextOn(theme.PanelBackground,
                    theme.AccentPrimary, theme.AccentTertiary,
                    theme.PrimaryText);

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
                NadaVfxEditorContrast.TextOn(theme.PanelBackground,
                    theme.MutedText, theme.AccentTertiary,
                    theme.PrimaryText);
            _detailsHeadingStyle = new GUIStyle(_mutedStyle)
            {
                fontSize = 10,
                alignment = TextAnchor.MiddleCenter
            };
            _detailLabelStyle = new GUIStyle(_mutedStyle)
            {
                fontSize = 10,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(0, 0, 0, 0)
            };
            _detailValueStyle = new GUIStyle(_bodyStyle)
            {
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(0, 0, 0, 0),
                wordWrap = true
            };


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
                new GUIStyle(GUI.skin.box)
                {
                    padding = new RectOffset(4, 4, 4, 4),
                    margin = new RectOffset(0, 0, 0, 0)
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
                new GUIStyle(GUI.skin.button)
                {
                    fontSize = 9,
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(6, 2, 0, 0),
                    margin = new RectOffset(0, 0, 0, 0),
                    clipping = TextClipping.Clip
                };

            // Menu items use a plain rectangular hover highlight, not
            // the rounded button texture used elsewhere in the editor.
            _pickerButtonStyle.border = new RectOffset(0, 0, 0, 0);
            _pickerButtonStyle.normal.background = null;
            _pickerButtonStyle.hover.background =
                CreateTexture(theme.ButtonHoverBackground);

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

            _targetIconButtonStyle.normal.background = roundedButtonBackground;
            _targetIconButtonStyle.hover.background = roundedButtonHover;
            _targetIconButtonStyle.active.background = roundedButtonHover;
            _targetIconButtonStyle.border = new RectOffset(7, 7, 7, 7);
            _targetIconSelectedStyle.normal.background =
                CreateRoundedTexture(theme.SelectedSegmentBackground, 6f);
            _targetIconSelectedStyle.hover.background = roundedButtonHover;
            _targetIconSelectedStyle.border = new RectOffset(7, 7, 7, 7);

            _headerActionButtonStyle = new GUIStyle(_addButtonStyle)
            {
                // All spacing is explicit in the action grid. Style margins
                // otherwise shift the Orbitals dropdown away from column 3.
                margin = new RectOffset(0, 0, 0, 0)
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

            Plugin.Log?.LogInfo(
                $"{Plugin.ModName}: [EditorTheme] {theme.DisplayName} palette-v8 applied.");
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
            if (_editorScrollSkin != null)
            {
                UnityEngine.Object.Destroy(_editorScrollSkin);
                _editorScrollSkin = null;
            }
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
            _neoKittenTexture = null;
            _themeEmblemTexture = null;
            _opacityTrackStyle = null;
            _opacityThumbStyle = null;
            _brandLoadAttempted = false;
            _sizeTrackStyle = null;
            _sizeThumbStyle = null;
            _sizeLabelStyle = null;
            _sizeActiveLabelStyle = null;

            _windowStyle = null;
            _editorScrollStyle = null;

            _brandSubtitleStyle = null;

            _smallHeaderStyle = null;
            _targetCardStyle = null;
            _settingsCardStyle = null;
            _targetTitleStyle = null;
            _targetNameWrappedStyle = null;
            _targetCaptionStyle = null;
            _detailsHeadingStyle = null;
            _detailLabelStyle = null;
            _detailValueStyle = null;
            _hoveredTargetTooltip = null;
            _targetStatusStyle = null;
            _targetIconButtonStyle = null;
            _targetIconSelectedStyle = null;
            _hideGlyphStyle = null;
            _targetTooltipStyle = null;
            _targetToolbarIcons = null;
            _panelStyle = null;
            _sectionHeaderStyle = null;
            _lowerSectionHeaderStyle = null;
            _settingsSubHeaderStyle = null;
            _blockStyle = null;
            _selectedBlockStyle = null;
            _blockLabelStyle = null;
            _disabledBlockLabelStyle = null;
            _inspectorTitleStyle = null;
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

        // Embedded full-color emoji-style controls render the same on both
        // macOS/Metal and Windows. Resources contain raw RGBA pixels (bottom-up),
        // avoiding platform emoji fonts, runtime image decoding, and shared assets.
        private static Texture2D[] CreateTargetToolbarIcons()
        {
            string[] names =
            {
                "palette", "gear", "details", "swap",
                "bound", "unbound", "unknown", "close"
            };
            var icons = new Texture2D[names.Length];
            Assembly assembly = typeof(NadaVfxEditor).Assembly;
            for (int i = 0; i < names.Length; i++)
            {
                string resource = "NADA.VFX.Weapon.Editor.Resources.nada-toolbar-" +
                                  names[i] + ".rgba";
                try
                {
                    using (Stream stream = assembly.GetManifestResourceStream(resource))
                    {
                        if (stream == null)
                        {
                            Plugin.Log?.LogWarning(
                                $"{Plugin.ModName}: [EditorToolbar] Missing resource: {resource}");
                            continue;
                        }
                        using (var reader = new BinaryReader(stream))
                        {
                            int width = reader.ReadUInt16();
                            int height = reader.ReadUInt16();
                            if (width != 64 || height != 64 ||
                                stream.Length - stream.Position != width * height * 4)
                                throw new InvalidDataException("Invalid toolbar image dimensions.");
                            byte[] rgba = reader.ReadBytes(width * height * 4);
                            var pixels = new Color32[width * height];
                            for (int p = 0, offset = 0; p < pixels.Length; p++, offset += 4)
                                pixels[p] = new Color32(rgba[offset], rgba[offset + 1],
                                    rgba[offset + 2], rgba[offset + 3]);
                            var texture = new Texture2D(width, height,
                                TextureFormat.RGBA32, false)
                            {
                                name = "NADA Toolbar " + names[i],
                                hideFlags = HideFlags.HideAndDontSave,
                                filterMode = FilterMode.Bilinear,
                                wrapMode = TextureWrapMode.Clamp
                            };
                            texture.SetPixels32(pixels);
                            texture.Apply(false, true);
                            OwnedTextures.Add(texture);
                            icons[i] = texture;
                        }
                    }
                }
                catch (Exception exception)
                {
                    Plugin.Log?.LogWarning($"{Plugin.ModName}: [EditorToolbar] " +
                        $"Couldn't load {names[i]}: {exception.Message}");
                }
            }
            return icons;
        }

        private static Texture2D CreateThemeEmblemTexture(NadaVfxEditorTheme theme)
        {
            const int size = 40;
            var pixels = new Color32[size * size];

            switch (theme.TitleVariant)
            {
                case NadaVfxEditorTitleVariant.PixelDark:
                    // Crescent moon with a small companion star.
                    for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        int moon = (x - 18) * (x - 18) + (y - 19) * (y - 19);
                        int cutout = (x - 25) * (x - 25) + (y - 25) * (y - 25);
                        if (moon <= 196 && cutout > 196)
                            DrawEmblemPixel(pixels, x, y, theme.TitlePrimary);
                    }
                    DrawEmblemStar(pixels, 32, 27, 4, theme.AccentTertiary);
                    DrawEmblemStar(pixels, 33, 13, 2, theme.MutedText);
                    break;

                case NadaVfxEditorTitleVariant.PixelVal:
                    // Evergreen on the left, a separate blue river on the right.
                    for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        bool canopy =
                            (y >= 22 && y <= 35 && Mathf.Abs(x - 15) <= (35 - y) * .75f + 1f) ||
                            (y >= 15 && y <= 28 && Mathf.Abs(x - 15) <= (28 - y) * .85f + 1f) ||
                            (y >= 9 && y <= 21 && Mathf.Abs(x - 15) <= (21 - y) * 1.0f + 1f);
                        if (canopy)
                            DrawEmblemPixel(pixels, x, y, theme.AccentPrimary);
                    }
                    for (int x = 13; x <= 17; x++)
                    for (int y = 5; y <= 11; y++)
                        DrawEmblemPixel(pixels, x, y, theme.AccentTertiary);
                    for (int x = 23; x < 39; x++)
                    {
                        int wave = Mathf.RoundToInt(9 + 2 * Mathf.Sin(x * .48f));
                        for (int d = 0; d < 3; d++)
                            DrawEmblemPixel(pixels, x, wave + d, theme.AccentSecondary);
                    }
                    break;

                case NadaVfxEditorTitleVariant.PixelDesert:
                    // Sandstone dunes beneath a sun: recognizable even at 27 px.
                    for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        float sun = (x - 15) * (x - 15) + (y - 26) * (y - 26);
                        if (sun <= 36)
                            DrawEmblemPixel(pixels, x, y, theme.AccentTertiary);
                        float rearDune = 12f + 5f * Mathf.Sin((x - 6) * .10f);
                        float frontDune = 7f + 5f * Mathf.Sin((x + 10) * .12f);
                        if (y <= rearDune)
                            DrawEmblemPixel(pixels, x, y, theme.AccentSecondary);
                        if (y <= frontDune)
                            DrawEmblemPixel(pixels, x, y, theme.AccentPrimary);
                    }
                    for (int ray = 0; ray < 8; ray++)
                    {
                        float a = ray * Mathf.PI / 4f;
                        DrawEmblemLine(pixels,
                            Mathf.RoundToInt(15f + 9f * Mathf.Cos(a)),
                            Mathf.RoundToInt(26f + 9f * Mathf.Sin(a)),
                            Mathf.RoundToInt(15f + 12f * Mathf.Cos(a)),
                            Mathf.RoundToInt(26f + 12f * Mathf.Sin(a)),
                            theme.AccentTertiary);
                    }
                    break;

                case NadaVfxEditorTitleVariant.PixelWinter:
                    // Six-sided snowflake with short barbs near each tip.
                    for (int arm = 0; arm < 6; arm++)
                    {
                        float a = Mathf.PI * arm / 3f;
                        DrawEmblemLine(pixels, 20, 20,
                            Mathf.RoundToInt(20f + 17f * Mathf.Cos(a)),
                            Mathf.RoundToInt(20f + 17f * Mathf.Sin(a)),
                            theme.AccentPrimary);
                        float branchX = 20f + 11f * Mathf.Cos(a);
                        float branchY = 20f + 11f * Mathf.Sin(a);
                        for (int side = -1; side <= 1; side += 2)
                        {
                            float b = a + side * Mathf.PI / 3f;
                            DrawEmblemLine(pixels,
                                Mathf.RoundToInt(branchX), Mathf.RoundToInt(branchY),
                                Mathf.RoundToInt(branchX - 5f * Mathf.Cos(b)),
                                Mathf.RoundToInt(branchY - 5f * Mathf.Sin(b)),
                                theme.TitlePrimary);
                        }
                    }
                    for (int y = 18; y <= 22; y++)
                    for (int x = 18; x <= 22; x++)
                        if (Mathf.Abs(x - 20) + Mathf.Abs(y - 20) <= 2)
                            DrawEmblemPixel(pixels, x, y, theme.TitlePrimary);
                    break;
            }

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = $"NADA {theme.DisplayName} Title Emblem",
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            OwnedTextures.Add(texture);
            return texture;
        }

        private static void DrawEmblemPixel(Color32[] pixels, int x, int y, Color32 color)
        {
            const int size = 40;
            if (x >= 0 && x < size && y >= 0 && y < size)
                pixels[y * size + x] = color;
        }

        private static void DrawEmblemStar(Color32[] pixels,
            int x, int y, int reach, Color32 color)
        {
            for (int d = -reach; d <= reach; d++)
            {
                DrawEmblemPixel(pixels, x + d, y, color);
                DrawEmblemPixel(pixels, x, y + d, color);
            }
        }

        private static void DrawEmblemLine(Color32[] pixels,
            int x0, int y0, int x1, int y1, Color32 color)
        {
            int dx = Mathf.Abs(x1 - x0);
            int dy = -Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int error = dx + dy;
            while (true)
            {
                DrawEmblemPixel(pixels, x0, y0, color);
                if (x0 == x1 && y0 == y1)
                    break;
                int doubled = 2 * error;
                if (doubled >= dy) { error += dy; x0 += sx; }
                if (doubled <= dx) { error += dx; y0 += sy; }
            }
        }

        private static Texture2D CreateNeoKittenTexture(NadaVfxEditorTheme theme)
        {
            const int size = 44;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "NADA Neo Purple Kitten",
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color32[size * size];

            // Saturated purple fur with lavender ears and pastel-blue eyes/outline.
            // These stay tied to Neo's palette when its colors are refined.
            Color32 fur = theme.ResetHoverBackground;
            Color32 violet = theme.AccentSecondary;
            Color32 blue = theme.AccentPrimary;
            Color32 nose = theme.AccentTertiary;

            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                bool inside = IsKittenPixel(x, y);
                if (!inside)
                {
                    bool touching = false;
                    bool near = false;
                    for (int dy = -2; dy <= 2; dy++)
                    for (int dx = -2; dx <= 2; dx++)
                    {
                        if (!IsKittenPixel(x + dx, y + dy))
                            continue;

                        near = true;
                        if (Mathf.Abs(dx) <= 1 && Mathf.Abs(dy) <= 1)
                            touching = true;
                    }

                    if (touching)
                        pixels[y * size + x] = blue;
                    else if (near)
                        pixels[y * size + x] = violet;
                    continue;
                }

                bool leftEye = (x - 16) * (x - 16) +
                               (y - 24) * (y - 24) <= 3;
                bool rightEye = (x - 28) * (x - 28) +
                                (y - 24) * (y - 24) <= 3;
                bool innerEar = y >= 33 && y <= 37 &&
                                ((x >= 13 && x <= 16) ||
                                 (x >= 28 && x <= 31));
                bool kittenNose = y == 19 && x >= 21 && x <= 23;

                pixels[y * size + x] = leftEye || rightEye ? blue :
                    kittenNose ? nose : innerEar ? violet : fur;
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            OwnedTextures.Add(texture);
            return texture;
        }

        private static bool IsKittenPixel(int x, int y)
        {
            float head = (x - 22f) * (x - 22f) / 196f +
                         (y - 23f) * (y - 23f) / 144f;
            float body = (x - 22f) * (x - 22f) / 116f +
                         (y - 11f) * (y - 11f) / 90f;
            bool leftEar = x >= 8 && x <= 20 && y >= 29 &&
                           y <= 41 - Mathf.Abs(x - 10) * 1.1f;
            bool rightEar = x >= 24 && x <= 36 && y >= 29 &&
                            y <= 41 - Mathf.Abs(x - 34) * 1.1f;
            return head <= 1f || body <= 1f || leftEar || rightEar;
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
