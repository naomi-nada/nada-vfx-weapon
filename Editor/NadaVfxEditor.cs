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
        private const float EffectsPanelWidth = 220f;
        private const float BlockHeight = 30f;

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
        private static uint? _deleteConfirmInstanceId;

        private static string _renameBuffer =
            string.Empty;

        private static bool _open;
        private static bool _addPickerOpen;
        private static bool _positionInitialized;

        private static bool _cursorStateCaptured;
        private static bool _previousCursorVisible;
        private static CursorLockMode _previousCursorLockMode;

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

            Cursor.visible =
                true;

            Cursor.lockState =
                CursorLockMode.None;

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
        }

        internal static void Close()
        {
            if (!_open)
                return;

            _open =
                false;

            _addPickerOpen =
                false;

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
        }

        private static void Open()
        {
            if (_open)
                return;

            CaptureCursor();

            _open =
                true;

            Cursor.visible =
                true;

            Cursor.lockState =
                CursorLockMode.None;

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

                _effectsScrollPosition =
                    Vector2.zero;

                _pickerScrollPosition =
                    Vector2.zero;

                _inspectorScrollPosition =
                    Vector2.zero;

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

            GUI.DragWindow(
                new Rect(
                    0f,
                    0f,
                    _windowRect.width - 44f,
                    30f));
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
                "BLOCK EDITOR / 0E-B",
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

            if (target == null)
            {
                GUILayout.Label(
                    "NO EDITABLE WEAPON",
                    _targetTitleStyle);

                GUILayout.Label(
                    "Equip a supported weapon with a resolved local NADA rig.",
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
                title.ToUpperInvariant(),
                _targetTitleStyle);

            if (!string.IsNullOrWhiteSpace(
                    target.ItemNameKey))
            {
                GUILayout.Label(
                    target.ItemNameKey,
                    _mutedStyle);
            }

            GUILayout.Space(
                5f);

            DrawInfoRow(
                "Prefab",
                target.PrefabName,
                "Legacy State",
                target.IsLegacyBound
                    ? "Bound"
                    : "Unbound");

            DrawInfoRow(
                "Source Root",
                $"{target.SourceRootName}  #{target.SourceRootInstanceId}",
                "Visual Root",
                $"{target.VisualRootName}  #{target.VisualRootInstanceId}");

            DrawInfoRow(
                "Runtime Rig",
                target.HasRuntimeRig
                    ? $"NADA Weapon  #{target.RuntimeRigInstanceId}"
                    : "Not Present",
                "Editor Blocks",
                NadaVfxEditorWorkingState
                    .EffectCount
                    .ToString());

            GUILayout.Space(
                3f);

            GUILayout.Label(
                target.RuntimeStateSource,
                _mutedStyle);

            GUILayout.Label(
                "Editor state: live preview / memory only / not persisted",
                _mutedStyle);

            if (!string.IsNullOrWhiteSpace(
                    target.VisualRootPath))
            {
                GUILayout.Label(
                    target.VisualRootPath,
                    _mutedStyle);
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
            GUILayout.Label(
                "EFFECTS",
                _sectionHeaderStyle);

            GUILayout.Space(
                5f);

            _effectsScrollPosition =
                GUILayout.BeginScrollView(
                    _effectsScrollPosition,
                    GUILayout.ExpandHeight(true));

            WeaponVfxState state =
                NadaVfxEditorWorkingState.State;

            if (target == null)
            {
                GUILayout.Label(
                    "No weapon target.",
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
                _sectionHeaderStyle);

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

            GUIStyle style =
                selected
                    ? _selectedBlockStyle
                    : _blockStyle;

            Rect rowRect =
                GUILayoutUtility.GetRect(
                    GUIContent.none,
                    style,
                    GUILayout.Height(BlockHeight),
                    GUILayout.ExpandWidth(true));

            if (GUI.Button(
                    rowRect,
                    GUIContent.none,
                    style))
            {
                SelectBlock(
                    block.InstanceId);
            }

            Rect labelRect =
                new Rect(
                    rowRect.x + 10f,
                    rowRect.y + 5f,
                    Mathf.Max(
                        0f,
                        rowRect.width - 20f),
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

        private static void DrawInspectorPanel()
        {
            GUILayout.BeginVertical(
                _panelStyle,
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true));

            VfxEffectBlock selectedBlock =
                FindSelectedBlock();

            if (selectedBlock == null)
            {
                GUILayout.Label(
                    "SELECT AN EFFECT",
                    _inspectorTitleStyle);

                GUILayout.Space(
                    4f);

                GUILayout.Label(
                    "Add an effect on the left, then select its block to edit it here.",
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
                $"{GetFriendlyTypeName(selectedBlock.TypeId)}  /  Instance #{selectedBlock.InstanceId}",
                _mutedStyle);

            GUILayout.Space(
                10f);

            DrawBlockLifecycleControls(
                selectedBlock);

            GUILayout.Space(
                10f);

            bool inspectorChanged =
                NadaVfxBlockInspectorRegistry.Draw(
                    selectedBlock);

            if (inspectorChanged)
            {
                RefreshPreview();
            }

            GUILayout.Space(
                5f);

            GUILayout.BeginVertical(
                _readOnlyBoxStyle);

            GUILayout.Label(
                "WORKING STATE",
                _sectionHeaderStyle);

            GUILayout.Label(
                "This block is driving the live local preview. " +
                "The editor state is still memory-only and is not persisted yet.",
                _mutedStyle);

            GUILayout.EndVertical();

            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }

        private static void DrawBlockLifecycleControls(
            VfxEffectBlock block)
        {
            GUILayout.Label(
                "BLOCK",
                _sectionHeaderStyle);

            if (NadaVfxEditorControls.Toggle(
                    $"block:{block.InstanceId}:enabled",
                    "Enabled",
                    block.Enabled,
                    out bool nextEnabled))
            {
                if (NadaVfxEditorWorkingState.TrySetEnabled(
                        block.InstanceId,
                        nextEnabled))
                {
                    RefreshPreview();
                }
            }

            GUILayout.Space(
                3f);

            DrawSingleInfoRow(
                "Type ID",
                block.TypeId);

            DrawSingleInfoRow(
                "Instance ID",
                block.InstanceId.ToString());

            GUILayout.Space(
                6f);

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

            bool previousEnabled =
                GUI.enabled;

            GUI.enabled =
                previousEnabled &&
                NadaVfxEditorWorkingState.CanMoveUp(
                    block.InstanceId);

            if (GUILayout.Button(
                    "Up",
                    GUILayout.Width(38f)))
            {
                if (NadaVfxEditorWorkingState.TryMoveEffect(
                        block.InstanceId,
                        -1))
                {
                    RefreshPreview();
                }
            }

            GUI.enabled =
                previousEnabled &&
                NadaVfxEditorWorkingState.CanMoveDown(
                    block.InstanceId);

            if (GUILayout.Button(
                    "Down",
                    GUILayout.Width(46f)))
            {
                if (NadaVfxEditorWorkingState.TryMoveEffect(
                        block.InstanceId,
                        1))
                {
                    RefreshPreview();
                }
            }

            GUI.enabled =
                previousEnabled;

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

            GUILayout.Label(
                "Renaming changes authored metadata only. " +
                "InstanceId and runtime identity stay unchanged.",
                _mutedStyle);
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

                    _renameInstanceId =
                        null;

                    _renameBuffer =
                        string.Empty;

                    _inspectorScrollPosition =
                        Vector2.zero;

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

        private static void DrawInfoRow(
            string leftLabel,
            string leftValue,
            string rightLabel,
            string rightValue)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label(
                leftLabel,
                _mutedStyle,
                GUILayout.Width(82f));

            GUILayout.Label(
                leftValue,
                _bodyStyle,
                GUILayout.MinWidth(170f));

            GUILayout.Space(
                12f);

            GUILayout.Label(
                rightLabel,
                _mutedStyle,
                GUILayout.Width(82f));

            GUILayout.Label(
                rightValue,
                _bodyStyle);

            GUILayout.EndHorizontal();
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

        private static void ClampWindowToScreen()
        {
            float maxWidth =
                Mathf.Max(
                    620f,
                    Screen.width - 40f);

            float maxHeight =
                Mathf.Max(
                    460f,
                    Screen.height - 80f);

            _windowRect.width =
                Mathf.Min(
                    DefaultWindowWidth,
                    maxWidth);

            _windowRect.height =
                Mathf.Min(
                    DefaultWindowHeight,
                    maxHeight);

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
                            9,
                            9)
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