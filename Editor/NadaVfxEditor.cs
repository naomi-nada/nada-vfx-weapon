using System;
using System.Collections.Generic;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Editor.Inspectors;
using NADA.VFX.Weapon.Weapons.Runtime;
using UnityEngine;

namespace NADA.VFX.Weapon.Editor
{
    internal static class NadaVfxEditor
    {
        private const int WindowId = 0x4E414441;

        private const float DefaultWindowWidth = 760f;
        private const float DefaultWindowHeight = 560f;

        private const float MinimumWindowWidth = 540f;
        private const float MinimumWindowHeight = 460f;

        private const float EffectsPanelWidth = 220f;
        private const float BlockHeight = 30f;

        private const float ResizeGripSize = 20f;
        private const float EffectDragThreshold = 5f;

        private static readonly List<Texture2D> OwnedTextures =
            new();

        private static Rect _windowRect =
            new Rect(
                0f,
                72f,
                DefaultWindowWidth,
                DefaultWindowHeight);

        private static Vector2 _effectsScrollPosition;
        private static Vector2 _pickerScrollPosition;
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

        private static bool _cursorStateCaptured;
        private static bool _previousCursorVisible;
        private static CursorLockMode _previousCursorLockMode;

        private static bool _resizing;
        private static Vector2 _resizeStartMouseScreenPosition;
        private static Vector2 _resizeStartSize;

        private static uint? _dragCandidateInstanceId;
        private static uint? _pendingReorderTargetInstanceId;

        private static bool _effectDragActive;

        private static Vector2 _effectDragStartMousePosition;

        private static float _nextTargetRefreshTime;

        private static GUIStyle _windowStyle;
        private static GUIStyle _titleStyle;
        private static GUIStyle _smallHeaderStyle;
        private static GUIStyle _targetCardStyle;
        private static GUIStyle _targetTitleStyle;
        private static GUIStyle _panelStyle;
        private static GUIStyle _sectionHeaderStyle;
        private static GUIStyle _blockStyle;
        private static GUIStyle _selectedBlockStyle;
        private static GUIStyle _blockLabelStyle;
        private static GUIStyle _disabledBlockLabelStyle;
        private static GUIStyle _inspectorTitleStyle;
        private static GUIStyle _bodyStyle;
        private static GUIStyle _mutedStyle;
        private static GUIStyle _warningStyle;
        private static GUIStyle _closeButtonStyle;
        private static GUIStyle _addButtonStyle;
        private static GUIStyle _pickerStyle;
        private static GUIStyle _pickerButtonStyle;
        private static GUIStyle _readOnlyBoxStyle;
        private static GUIStyle _compactToggleStyle;
        private static GUIStyle _detailsButtonStyle;

        internal static bool IsOpen =>
            _open;

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

            if (NadaVfxEditorConfig.ToggleHotkey.Value.IsDown())
            {
                if (_open)
                {
                    Close();
                }
                else
                {
                    Open();
                }
            }

            if (!_open)
                return;

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

            EnforceCursorOwnership();

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

            EnforceCursorOwnership();
        }

        internal static void EnforceCursorOwnership()
        {
            if (!_open)
                return;

            Cursor.visible =
                true;

            if (Cursor.lockState !=
                CursorLockMode.None)
            {
                Cursor.lockState =
                    CursorLockMode.None;
            }
        }

        internal static void Close()
        {
            if (!_open)
                return;

            _open =
                false;

            _addPickerOpen =
                false;

            if (_resizing &&
                GUIUtility.hotControl != 0)
            {
                GUIUtility.hotControl =
                    0;
            }

            _resizing =
                false;

            ClearEffectDrag();
            ClearTransientBlockActions();

            NadaWeaponEditorPreviewState
                .Deactivate();

            Plugin.Instance?
                .RefreshExistingEquippedRigsOnly();

            RestoreCursor();

            Plugin.Log?.LogInfo(
                $"{Plugin.ModName}: [Editor] closed.");
        }

        internal static void Shutdown()
        {
            Close();

            foreach (Texture2D texture in
                     OwnedTextures)
            {
                if (texture == null)
                    continue;

                UnityEngine.Object.Destroy(
                    texture);
            }

            OwnedTextures.Clear();

            NadaVfxEditorControls
                .ResetUiState();

            _windowStyle = null;
            _titleStyle = null;
            _smallHeaderStyle = null;
            _targetCardStyle = null;
            _targetTitleStyle = null;
            _panelStyle = null;
            _sectionHeaderStyle = null;
            _blockStyle = null;
            _selectedBlockStyle = null;
            _blockLabelStyle = null;
            _disabledBlockLabelStyle = null;
            _inspectorTitleStyle = null;
            _bodyStyle = null;
            _mutedStyle = null;
            _warningStyle = null;
            _closeButtonStyle = null;
            _addButtonStyle = null;
            _pickerStyle = null;
            _pickerButtonStyle = null;
            _readOnlyBoxStyle = null;
            _compactToggleStyle = null;
            _detailsButtonStyle = null;
        }

        private static void Open()
        {
            if (_open)
                return;

            CaptureCursor();

            _open =
                true;

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
                _selectedInstanceId =
                    null;

                _addPickerOpen =
                    false;

                _targetDetailsExpanded =
                    false;

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
            DrawTitleBar();

            GUILayout.Space(
                6f);

            NadaVfxEditorTarget target =
                NadaVfxEditorTargetRegistry.Current;

            bool targetChanged =
                NadaVfxEditorWorkingState
                    .SynchronizeTarget(
                        target);

            if (targetChanged)
            {
                _selectedInstanceId =
                    null;

                _addPickerOpen =
                    false;

                _targetDetailsExpanded =
                    false;

                _effectsScrollPosition =
                    Vector2.zero;

                _pickerScrollPosition =
                    Vector2.zero;

                _inspectorScrollPosition =
                    Vector2.zero;

                ClearEffectDrag();
                ClearTransientBlockActions();

                ActivatePreview(
                    target);
            }

            DrawTargetCard(
                target);

            GUILayout.Space(
                8f);

            EnsureSelection();

            GUILayout.BeginHorizontal(
                GUILayout.ExpandHeight(true));

            DrawEffectsPanel(
                target);

            GUILayout.Space(
                8f);

            DrawInspectorPanel();

            GUILayout.EndHorizontal();

            DrawResizeGrip();

            NadaVfxEditorControls
                .DrawTooltipOverlay(
                    _windowRect.width,
                    _windowRect.height);

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
                        0f,
                        _windowRect.width - 44f,
                        30f));
            }
        }

        private static void DrawTitleBar()
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label(
                "NADA VFX",
                _titleStyle);

            GUILayout.Space(
                8f);

            GUILayout.Label(
                "EDITOR / 0H-D",
                _smallHeaderStyle);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(
                    "X",
                    _closeButtonStyle,
                    GUILayout.Width(26f),
                    GUILayout.Height(22f)))
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
                _sectionHeaderStyle);

            GUILayout.FlexibleSpace();

            if (target != null &&
                GUILayout.Button(
                    _targetDetailsExpanded
                        ? "Hide Details"
                        : "Details",
                    _detailsButtonStyle,
                    GUILayout.Width(82f),
                    GUILayout.Height(20f)))
            {
                _targetDetailsExpanded =
                    !_targetDetailsExpanded;
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(
                3f);

            if (target == null)
            {
                GUILayout.Label(
                    "NO EDITABLE TARGET",
                    _targetTitleStyle);

                GUILayout.Label(
                    "Equip or select a supported target.",
                    _mutedStyle);

                GUILayout.EndVertical();

                return;
            }

            string title =
                string.IsNullOrWhiteSpace(
                    target.DisplayName)
                    ? target.PrefabName
                    : target.DisplayName;

            GUILayout.Label(
                title,
                _targetTitleStyle);

            string bindingStatus =
                target.IsLegacyBound
                    ? "Bound"
                    : "Unbound";

            GUILayout.Label(
                $"{bindingStatus}  ·  Live Preview",
                _mutedStyle);

            if (_targetDetailsExpanded)
            {
                GUILayout.Space(
                    7f);

                DrawSingleInfoRow(
                    "Prefab",
                    string.IsNullOrWhiteSpace(
                        target.PrefabName)
                        ? "Unknown"
                        : target.PrefabName);

                if (!string.IsNullOrWhiteSpace(
                        target.ItemNameKey))
                {
                    DrawSingleInfoRow(
                        "Item Key",
                        target.ItemNameKey);
                }

                DrawSingleInfoRow(
                    "Visual Target",
                    string.IsNullOrWhiteSpace(
                        target.VisualRootName)
                        ? "Unknown"
                        : target.VisualRootName);
            }

            GUILayout.EndVertical();
        }

        private static void DrawEffectsPanel(
            NadaVfxEditorTarget target)
        {
            GUILayout.BeginVertical(
                _panelStyle,
                GUILayout.Width(EffectsPanelWidth),
                GUILayout.ExpandHeight(true));

            GUILayout.Label(
                "EFFECTS",
                _sectionHeaderStyle);

            GUILayout.Space(
                5f);

            if (_addPickerOpen)
            {
                DrawEffectPickerPanel(
                    target);
            }
            else
            {
                DrawEffectListPanel(
                    target);
            }

            GUILayout.EndVertical();
        }

        private static void DrawEffectListPanel(
            NadaVfxEditorTarget target)
        {
            _pendingReorderTargetInstanceId =
                null;

            _effectsScrollPosition =
                GUILayout.BeginScrollView(
                    _effectsScrollPosition,
                    GUILayout.ExpandHeight(true));

            WeaponVfxState state =
                NadaVfxEditorWorkingState.State;

            if (target == null)
            {
                GUILayout.Label(
                    "No target.",
                    _mutedStyle);
            }
            else if (state?.Effects == null ||
                     state.Effects.Count == 0)
            {
                GUILayout.Label(
                    "No effects added.",
                    _mutedStyle);
            }
            else
            {
                foreach (VfxEffectBlock block in
                         state.Effects)
                {
                    if (block == null)
                        continue;

                    DrawBlockBand(
                        block);
                }
            }

            GUILayout.EndScrollView();

            ApplyPendingEffectReorder();

            GUILayout.Space(
                6f);

            bool previousEnabled =
                GUI.enabled;

            GUI.enabled =
                previousEnabled &&
                target != null;

            if (GUILayout.Button(
                    "+ Add Effect",
                    _addButtonStyle,
                    GUILayout.Height(BlockHeight)))
            {
                ClearEffectDrag();

                _addPickerOpen =
                    true;

                _pickerScrollPosition =
                    Vector2.zero;
            }

            GUI.enabled =
                previousEnabled;
        }

        private static void DrawEffectPickerPanel(
            NadaVfxEditorTarget target)
        {
            GUILayout.Label(
                "ADD EFFECT",
                _smallHeaderStyle);

            GUILayout.Space(
                5f);

            _pickerScrollPosition =
                GUILayout.BeginScrollView(
                    _pickerScrollPosition,
                    GUILayout.ExpandHeight(true));

            DrawAddEffectPicker(
                target);

            GUILayout.EndScrollView();

            GUILayout.Space(
                6f);

            if (GUILayout.Button(
                    "< Back",
                    _addButtonStyle,
                    GUILayout.Height(BlockHeight)))
            {
                _addPickerOpen =
                    false;
            }
        }

        private static void DrawAddEffectPicker(
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

            GUILayout.Label(
                "STANDARD",
                _sectionHeaderStyle);

            GUILayout.Space(
                3f);

            foreach (NadaVfxEditorEffectDefinition definition in
                     NadaVfxEditorEffectCatalog.All)
            {
                if (definition.IsOrbital)
                    continue;

                DrawAddEffectButton(
                    definition);
            }

            GUILayout.Space(
                8f);

            GUILayout.Label(
                "ORBITALS",
                _sectionHeaderStyle);

            GUILayout.Space(
                3f);

            foreach (NadaVfxEditorEffectDefinition definition in
                     NadaVfxEditorEffectCatalog.All)
            {
                if (!definition.IsOrbital)
                    continue;

                DrawAddEffectButton(
                    definition);
            }

            GUILayout.EndVertical();
        }

        private static void DrawAddEffectButton(
            NadaVfxEditorEffectDefinition definition)
        {
            if (definition == null)
                return;

            if (!GUILayout.Button(
                    definition.PickerName,
                    _pickerButtonStyle,
                    GUILayout.Height(27f)))
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

            _effectsScrollPosition =
                Vector2.zero;

            _pickerScrollPosition =
                Vector2.zero;

            RefreshPreview();
        }

        private static void DrawBlockBand(
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
                    GUILayout.Height(BlockHeight),
                    GUILayout.ExpandWidth(true));

            GUI.Box(
                rowRect,
                GUIContent.none,
                style);

            HandleEffectRowMouse(
                block,
                rowRect);

            Rect gripRect =
                new Rect(
                    rowRect.x + 7f,
                    rowRect.y + 5f,
                    18f,
                    rowRect.height - 10f);

            GUI.Label(
                gripRect,
                "::",
                _mutedStyle);

            Rect labelRect =
                new Rect(
                    rowRect.x + 27f,
                    rowRect.y + 5f,
                    Mathf.Max(
                        0f,
                        rowRect.width - 35f),
                    rowRect.height - 10f);

            string label =
                NadaVfxEditorWorkingState.GetDisplayName(
                    block);

            if (!block.Enabled)
            {
                label +=
                    "  (Off)";
            }

            GUI.Label(
                labelRect,
                label,
                block.Enabled
                    ? _blockLabelStyle
                    : _disabledBlockLabelStyle);
        }

        private static void HandleEffectRowMouse(
            VfxEffectBlock block,
            Rect rowRect)
        {
            Event current =
                Event.current;

            bool mouseOver =
                rowRect.Contains(
                    current.mousePosition);

            if (current.type ==
                    EventType.MouseDown &&
                current.button ==
                    0 &&
                mouseOver)
            {
                SelectBlock(
                    block.InstanceId);

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

            GUILayout.Label(
                "EDIT",
                _sectionHeaderStyle);

            GUILayout.Space(
                5f);

            VfxEffectBlock selectedBlock =
                FindSelectedBlock();

            if (selectedBlock == null)
            {
                GUILayout.Label(
                    "Add an effect on the left, then select it to edit.",
                    _mutedStyle);

                GUILayout.EndVertical();

                return;
            }

            _inspectorScrollPosition =
                GUILayout.BeginScrollView(
                    _inspectorScrollPosition,
                    GUILayout.ExpandHeight(true));

            string displayName =
                NadaVfxEditorWorkingState
                    .GetDisplayName(
                        selectedBlock);

            GUILayout.Label(
                displayName,
                _inspectorTitleStyle);

            GUILayout.Label(
                GetFriendlyTypeName(
                    selectedBlock.TypeId),
                _mutedStyle);

            GUILayout.Space(
                7f);

            DrawBlockLifecycleControls(
                selectedBlock);

            GUILayout.Space(
                8f);

            bool inspectorChanged =
                NadaVfxBlockInspectorRegistry.Draw(
                    selectedBlock);

            if (inspectorChanged)
            {
                RefreshPreview();
            }

            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }

        private static void DrawBlockLifecycleControls(
            VfxEffectBlock block)
        {
            bool nextEnabled =
                GUILayout.Toggle(
                    block.Enabled,
                    "Enabled",
                    _compactToggleStyle,
                    GUILayout.Width(78f));

            if (nextEnabled !=
                block.Enabled)
            {
                if (NadaVfxEditorWorkingState.TrySetEnabled(
                        block.InstanceId,
                        nextEnabled))
                {
                    RefreshPreview();
                }
            }

            GUILayout.Space(
                4f);

            if (_renameInstanceId.HasValue &&
                _renameInstanceId.Value ==
                block.InstanceId)
            {
                DrawRenameControls(
                    block);
            }
            else
            {
                DrawBlockActionButtons(
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

        private static void DrawBlockActionButtons(
            VfxEffectBlock block)
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(
                    "Rename",
                    GUILayout.Width(66f)))
            {
                _renameInstanceId =
                    block.InstanceId;

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
                    GUILayout.Width(72f)))
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
                    GUILayout.Width(50f)))
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
                    GUILayout.Width(54f)))
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

            GUILayout.EndHorizontal();
        }

        private static void DrawRenameControls(
            VfxEffectBlock block)
        {
            GUILayout.Label(
                "Name",
                _mutedStyle);

            _renameBuffer =
                GUILayout.TextField(
                    _renameBuffer ?? string.Empty,
                    64);

            GUILayout.BeginHorizontal();

            bool canSave =
                !string.IsNullOrWhiteSpace(
                    _renameBuffer);

            bool previousEnabled =
                GUI.enabled;

            GUI.enabled =
                previousEnabled &&
                canSave;

            if (GUILayout.Button(
                    "Save Name",
                    GUILayout.Width(82f)))
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
                    GUILayout.Width(58f)))
            {
                _renameInstanceId =
                    null;

                _renameBuffer =
                    string.Empty;
            }

            GUILayout.EndHorizontal();
        }

        private static void DrawResetConfirmation(
            VfxEffectBlock block)
        {
            GUILayout.Space(
                5f);

            GUILayout.BeginVertical(
                _readOnlyBoxStyle);

            GUILayout.Label(
                $"Reset '{NadaVfxEditorWorkingState.GetDisplayName(block)}'?",
                _warningStyle);

            GUILayout.Label(
                "All effect modifiers and transform values return to their defaults. " +
                "Name, Enabled state, identity, and list position stay unchanged.",
                _mutedStyle);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button(
                    "Confirm Reset",
                    GUILayout.Width(96f)))
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
                    GUILayout.Width(58f)))
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
                5f);

            GUILayout.BeginVertical(
                _readOnlyBoxStyle);

            GUILayout.Label(
                $"Delete '{NadaVfxEditorWorkingState.GetDisplayName(block)}'?",
                _warningStyle);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button(
                    "Confirm Delete",
                    GUILayout.Width(104f)))
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
                    GUILayout.Width(58f)))
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

            _renameBuffer =
                string.Empty;

            _resetConfirmInstanceId =
                null;

            _deleteConfirmInstanceId =
                null;
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
                GUILayout.Width(100f));

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

        private static void DrawResizeGrip()
        {
            Rect gripRect =
                new Rect(
                    _windowRect.width -
                    ResizeGripSize -
                    3f,
                    _windowRect.height -
                    ResizeGripSize -
                    3f,
                    ResizeGripSize,
                    ResizeGripSize);

            Event current =
                Event.current;

            int controlId =
                GUIUtility.GetControlID(
                    FocusType.Passive);

            EventType controlEvent =
                current.GetTypeForControl(
                    controlId);

            switch (controlEvent)
            {
                case EventType.MouseDown:
                {
                    if (current.button != 0 ||
                        !gripRect.Contains(
                            current.mousePosition))
                    {
                        break;
                    }

                    GUIUtility.hotControl =
                        controlId;

                    _resizing =
                        true;

                    _resizeStartMouseScreenPosition =
                        GUIUtility.GUIToScreenPoint(
                            current.mousePosition);

                    _resizeStartSize =
                        new Vector2(
                            _windowRect.width,
                            _windowRect.height);

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

                    _windowRect.width =
                        _resizeStartSize.x +
                        delta.x;

                    _windowRect.height =
                        _resizeStartSize.y +
                        delta.y;

                    ClampWindowSize();

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

                    GUIUtility.hotControl =
                        0;

                    current.Use();

                    break;
                }
            }
        }

        private static void EnsureWindowPosition()
        {
            if (_positionInitialized)
                return;

            _windowRect.x =
                Mathf.Max(
                    20f,
                    Screen.width -
                    _windowRect.width -
                    24f);

            _windowRect.y =
                64f;

            _positionInitialized =
                true;
        }

        private static void ClampWindowSize()
        {
            float maximumWidth =
                Mathf.Max(
                    320f,
                    Screen.width - 20f);

            float maximumHeight =
                Mathf.Max(
                    300f,
                    Screen.height - 20f);

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
            if (_windowStyle != null)
                return;

            Texture2D windowBackground =
                CreateTexture(
                    new Color(
                        0.075f,
                        0.065f,
                        0.055f,
                        0.97f));

            Texture2D targetBackground =
                CreateTexture(
                    new Color(
                        0.16f,
                        0.135f,
                        0.105f,
                        0.96f));

            Texture2D panelBackground =
                CreateTexture(
                    new Color(
                        0.115f,
                        0.10f,
                        0.08f,
                        0.96f));

            Texture2D blockBackground =
                CreateTexture(
                    new Color(
                        0.18f,
                        0.15f,
                        0.115f,
                        1f));

            Texture2D blockHoverBackground =
                CreateTexture(
                    new Color(
                        0.235f,
                        0.19f,
                        0.135f,
                        1f));

            Texture2D selectedBlockBackground =
                CreateTexture(
                    new Color(
                        0.43f,
                        0.315f,
                        0.15f,
                        1f));

            Texture2D pickerBackground =
                CreateTexture(
                    new Color(
                        0.105f,
                        0.09f,
                        0.07f,
                        1f));

            Texture2D readOnlyBackground =
                CreateTexture(
                    new Color(
                        0.13f,
                        0.115f,
                        0.095f,
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

            Color disabledText =
                new Color(
                    0.48f,
                    0.46f,
                    0.42f,
                    1f);

            Color accentText =
                new Color(
                    0.91f,
                    0.70f,
                    0.34f,
                    1f);

            Color warningText =
                new Color(
                    0.95f,
                    0.62f,
                    0.38f,
                    1f);

            _windowStyle =
                new GUIStyle(
                    GUI.skin.window)
                {
                    padding =
                        new RectOffset(
                            12,
                            12,
                            8,
                            12)
                };

            _windowStyle.normal.background =
                windowBackground;

            _windowStyle.normal.textColor =
                primaryText;

            _titleStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    fontSize = 16
                };

            _titleStyle.normal.textColor =
                primaryText;

            _smallHeaderStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    fontSize = 10
                };

            _smallHeaderStyle.normal.textColor =
                accentText;

            _targetCardStyle =
                new GUIStyle(
                    GUI.skin.box)
                {
                    padding =
                        new RectOffset(
                            12,
                            12,
                            8,
                            8)
                };

            _targetCardStyle.normal.background =
                targetBackground;

            _targetTitleStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    fontSize = 14
                };

            _targetTitleStyle.normal.textColor =
                primaryText;

            _panelStyle =
                new GUIStyle(
                    GUI.skin.box)
                {
                    padding =
                        new RectOffset(
                            10,
                            10,
                            10,
                            10)
                };

            _panelStyle.normal.background =
                panelBackground;

            _sectionHeaderStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    fontSize = 10
                };

            _sectionHeaderStyle.normal.textColor =
                accentText;

            _blockStyle =
                new GUIStyle(
                    GUI.skin.button)
                {
                    padding =
                        new RectOffset(
                            8,
                            8,
                            4,
                            4),
                    fontSize = 12
                };

            _blockStyle.normal.background =
                blockBackground;

            _blockStyle.hover.background =
                blockHoverBackground;

            _blockStyle.active.background =
                blockHoverBackground;

            _selectedBlockStyle =
                new GUIStyle(
                    _blockStyle);

            _selectedBlockStyle.normal.background =
                selectedBlockBackground;

            _selectedBlockStyle.hover.background =
                selectedBlockBackground;

            _selectedBlockStyle.active.background =
                selectedBlockBackground;

            _blockLabelStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    fontSize = 12
                };

            _blockLabelStyle.normal.textColor =
                primaryText;

            _disabledBlockLabelStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    fontSize = 12
                };

            _disabledBlockLabelStyle.normal.textColor =
                disabledText;

            _inspectorTitleStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    fontSize = 15
                };

            _inspectorTitleStyle.normal.textColor =
                primaryText;

            _bodyStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    fontSize = 11,
                    wordWrap = false
                };

            _bodyStyle.normal.textColor =
                primaryText;

            _mutedStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    fontSize = 10,
                    wordWrap = true
                };

            _mutedStyle.normal.textColor =
                mutedText;

            _warningStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    fontSize = 11,
                    wordWrap = true
                };

            _warningStyle.normal.textColor =
                warningText;

            _closeButtonStyle =
                new GUIStyle(
                    GUI.skin.button)
                {
                    fontSize = 11
                };

            _closeButtonStyle.normal.textColor =
                primaryText;

            _addButtonStyle =
                new GUIStyle(
                    _blockStyle);

            _pickerStyle =
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

            _pickerStyle.normal.background =
                pickerBackground;

            _pickerButtonStyle =
                new GUIStyle(
                    GUI.skin.button)
                {
                    fontSize = 11,
                    padding =
                        new RectOffset(
                            6,
                            6,
                            3,
                            3)
                };

            _pickerButtonStyle.normal.textColor =
                primaryText;

            _readOnlyBoxStyle =
                new GUIStyle(
                    GUI.skin.box)
                {
                    padding =
                        new RectOffset(
                            10,
                            10,
                            8,
                            8)
                };

            _readOnlyBoxStyle.normal.background =
                readOnlyBackground;

            _compactToggleStyle =
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

            _compactToggleStyle.normal.textColor =
                primaryText;

            _detailsButtonStyle =
                new GUIStyle(
                    GUI.skin.button)
                {
                    fontSize = 9,
                    padding =
                        new RectOffset(
                            5,
                            5,
                            2,
                            2)
                };

            _detailsButtonStyle.normal.textColor =
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
                    name = "NADA VFX Editor Runtime UI",
                    hideFlags = HideFlags.HideAndDontSave,
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp
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
            if (!_open ||
                target?.ItemData == null)
            {
                NadaWeaponEditorPreviewState
                    .Deactivate();

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

                Plugin.Instance?
                    .RefreshExistingEquippedRigsOnly();

                return;
            }

            NadaWeaponEditorPreviewState
                .Activate(
                    target.ItemData,
                    state);

            Plugin.Instance?
                .RefreshExistingEquippedRigsOnly();
        }

        private static void RefreshPreview()
        {
            if (!_open)
                return;

            NadaVfxEditorTarget target =
                NadaVfxEditorTargetRegistry.Current;

            if (target?.ItemData == null)
                return;

            WeaponVfxState state =
                NadaVfxEditorWorkingState.State;

            if (state == null)
                return;

            NadaWeaponEditorPreviewState
                .Activate(
                    target.ItemData,
                    state);

            Plugin.Instance?
                .RefreshExistingEquippedRigsOnly();
        }
    }
}