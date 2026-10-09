using System;
using NADA.VFX.Weapon.Core.Persistence;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Runtime.Formation;
using NADA.VFX.Weapon.Weapons.Runtime;
using UnityEngine;

namespace NADA.VFX.Weapon.Editor
{
    // Styles are snapshots, not a second state owner. Disk and codec work
    // happen only on explicit actions; IMGUI repaint only reads cached names.
    internal static class NadaVfxEditorStylesPanel
    {
        private const string StyleNameControlName = "NadaVfxStyleName";
        private const string ShareInputControlName = "NadaVfxShareInput";
        private const int MaxShareCodeCharacters = 90000;
        private const float LabelWidth = 84f;
        private const float ActionWidth = 84f;
        private const float ControlHeight = 23f;
        private const float RowGap = 4f;

        private static bool _open;
        private static bool _styleDropdownOpen;
        private static string _selectedStyle = string.Empty;
        private static string _saveName = string.Empty;
        private static string _shareInput = string.Empty;
        private static string _message;
        private static string _confirmOverwrite;
        private static string _confirmDelete;
        private static WeaponVfxState _pendingImport;
        private static uint _pendingImportCursor;
        private static global::ItemDrop.ItemData _pendingImportItem;
        private static WeaponVfxState _queuedApplyState;
        private static uint _queuedApplyCursor;
        private static global::ItemDrop.ItemData _queuedApplyItem;
        private static string _queuedApplyLabel;
        private static Vector2 _styleListScroll;

        internal static bool IsOpen => _open;

        internal static void Toggle()
        {
            _open = !_open;
            OnTargetChanged();
            if (_open)
                WeaponVfxStyleStore.RefreshNames();
        }

        internal static void OnTargetChanged()
        {
            _styleDropdownOpen = false;
            _confirmOverwrite = null;
            _confirmDelete = null;
            ClearPendingImport();
            _queuedApplyState = null;
            _queuedApplyCursor = 0;
            _queuedApplyItem = null;
            _queuedApplyLabel = null;
            _message = null;
        }

        internal static void Close()
        {
            _open = false;
            OnTargetChanged();
            _shareInput = string.Empty;
        }

        internal static bool IsTextInputFocused(string focusedControl) =>
            string.Equals(focusedControl, StyleNameControlName, StringComparison.Ordinal) ||
            string.Equals(focusedControl, ShareInputControlName, StringComparison.Ordinal);

        private static void ClearPendingImport()
        {
            _pendingImport = null;
            _pendingImportCursor = 0;
            _pendingImportItem = null;
        }

        // Applying a draft can rebuild preview hierarchy. Defer it until the
        // containing GUI.Window has finished to keep IMGUI layout balanced.
        private static void QueueDraftApply(NadaVfxEditorTarget target,
            WeaponVfxState state, uint cursor, string label)
        {
            _queuedApplyState = state;
            _queuedApplyCursor = cursor;
            _queuedApplyItem = target?.ItemData;
            _queuedApplyLabel = label;
        }

        internal static void ProcessPendingDraftApply()
        {
            if (_queuedApplyState == null)
                return;

            WeaponVfxState state = _queuedApplyState;
            uint cursor = _queuedApplyCursor;
            global::ItemDrop.ItemData item = _queuedApplyItem;
            string label = _queuedApplyLabel;
            _queuedApplyState = null;
            _queuedApplyItem = null;
            _queuedApplyCursor = 0;
            _queuedApplyLabel = null;

            NadaVfxEditorTarget target = NadaVfxEditorTargetRegistry.Current;
            if (target?.ItemData == null ||
                !ReferenceEquals(item, target.ItemData))
            {
                _message = "Equipped weapon changed. Try again.";
                return;
            }
            ApplyToDraft(target, state, cursor, label);
        }

        private static bool TryCurrentSnapshot(NadaVfxEditorTarget target,
            out WeaponVfxState state, out uint cursor)
        {
            state = null;
            cursor = 0;
            if (target?.ItemData == null)
            {
                _message = "No equipped weapon.";
                return false;
            }

            NadaVfxEditorTargetRegistry.RefreshFromRuntime();
            if (!ReferenceEquals(target.ItemData,
                    NadaVfxEditorTargetRegistry.Current?.ItemData))
            {
                _message = "Equipped weapon changed. Try again.";
                return false;
            }

            if (!NadaVfxEditorWorkingState.TryGetSnapshotForExport(
                    target.ItemData, out state, out cursor, out string reason))
            {
                _message = reason ?? "Cannot read current VFX state.";
                return false;
            }

            if (!OrbitalsFormationResolver.TryResolve(state,
                    out OrbitalsFormationResolution formation) ||
                formation == null || !formation.IsValid)
            {
                _message = formation?.FailureReason ?? "Invalid Orbital formation.";
                return false;
            }
            return true;
        }

        private static bool ApplyToDraft(NadaVfxEditorTarget target,
            WeaponVfxState state, uint cursor, string label)
        {
            NadaVfxEditorTargetRegistry.RefreshFromRuntime();
            if (target?.ItemData == null ||
                !ReferenceEquals(target.ItemData,
                    NadaVfxEditorTargetRegistry.Current?.ItemData) ||
                NadaVfxEditorWorkingState.IsReadOnly)
            {
                _message = "Equip an unbound weapon to apply a style.";
                return false;
            }

            if (!NadaVfxEditorWorkingState.TryReplaceDraft(
                    target.ItemData, state, cursor, out string reason))
            {
                _message = reason ?? "Could not replace draft.";
                return false;
            }

            NadaVfxEditor.OnStylesDraftReplaced();
            _message = label + " applied (" + state.Effects.Count +
                       " effects). Preview updated; not bound.";
            return true;
        }

        private static void ChooseStyle(NadaVfxEditorTarget target, string name)
        {
            _styleDropdownOpen = false;
            _confirmDelete = null;
            _selectedStyle = name;

            // Selection IS the load action. There is no hidden second button.
            if (!WeaponVfxStyleStore.TryLoad(name,
                    out WeaponVfxState loaded, out uint cursor,
                    out string reason))
            {
                _message = reason ?? "Could not load selected style.";
                return;
            }

            QueueDraftApply(target, loaded, cursor, "Style '" + name + "'");
        }

        private static void SaveStyle(NadaVfxEditorTarget target, string name)
        {
            _confirmOverwrite = null;
            if (!TryCurrentSnapshot(target, out WeaponVfxState state,
                    out uint cursor))
                return;
            if (!WeaponVfxStyleStore.TrySave(name, state, cursor, out string reason))
            {
                _message = reason ?? "Could not save style.";
                return;
            }
            _selectedStyle = name;
            _message = "Saved '" + name + "' (" + state.Effects.Count + " effects).";
            Plugin.Log?.LogInfo($"{Plugin.ModName}: [EditorStyleSaved] " +
                                $"name='{name}' effects={state.Effects.Count}.");
        }

        internal static void Draw(NadaVfxEditorTarget target,
            GUIStyle card, GUIStyle heading, GUIStyle body,
            GUIStyle muted, GUIStyle warning, GUIStyle button,
            GUIStyle dropdown, GUIStyle textField, GUIStyle textArea)
        {
            bool hasTarget = target?.ItemData != null;
            bool editable = hasTarget && !NadaVfxEditorWorkingState.IsReadOnly &&
                            target.SourceKind != NadaWeaponLocalSourceKind.InvalidNative;
            bool canExport = hasTarget &&
                target.SourceKind != NadaWeaponLocalSourceKind.InvalidNative;

            GUILayout.BeginVertical(card);
            GUILayout.Label("STYLES", heading);
            GUILayout.Space(3f);

            // Row 1: choosing a saved style immediately loads it into the draft.
            GUILayout.BeginHorizontal(GUILayout.Height(ControlHeight));
            GUILayout.Label("Choose Style", body, GUILayout.Width(LabelWidth));
            bool previous = GUI.enabled;
            GUI.enabled = previous && editable;
            string caption = string.IsNullOrEmpty(_selectedStyle)
                ? "Select a saved style  \u25be"
                : _selectedStyle + "  \u25be";
            if (GUILayout.Button(caption, dropdown, GUILayout.Height(ControlHeight),
                    GUILayout.MinWidth(120f), GUILayout.ExpandWidth(true)))
            {
                _styleDropdownOpen = !_styleDropdownOpen;
                _confirmDelete = null;
            }
            GUI.enabled = previous && !string.IsNullOrEmpty(_selectedStyle);
            if (GUILayout.Button("Delete", button,
                    GUILayout.Width(ActionWidth), GUILayout.Height(ControlHeight)))
            {
                _confirmDelete = _selectedStyle;
                _styleDropdownOpen = false;
            }
            GUI.enabled = previous;
            GUILayout.EndHorizontal();

            if (_styleDropdownOpen)
            {
                var names = WeaponVfxStyleStore.GetNames();
                _styleListScroll = GUILayout.BeginScrollView(_styleListScroll,
                    GUILayout.Height(Mathf.Min(116f, Math.Max(29f, (names.Count - 1) * 23f + 4f))));
                int choices = 0;
                foreach (string name in names)
                {
                    // 'Default' was a legacy config operation, not a saved style.
                    if (string.Equals(name, "Default", StringComparison.OrdinalIgnoreCase))
                        continue;
                    choices++;
                    if (GUILayout.Button(name, dropdown, GUILayout.Height(21f)))
                        ChooseStyle(target, name);
                }
                if (choices == 0)
                    GUILayout.Label("No saved styles yet.", muted);
                GUILayout.EndScrollView();
            }

            if (_confirmDelete != null)
            {
                GUILayout.BeginHorizontal(GUILayout.Height(ControlHeight));
                GUILayout.Space(LabelWidth + RowGap);
                GUILayout.Label("Delete style?", warning,
                    GUILayout.MinWidth(66f), GUILayout.ExpandWidth(true),
                    GUILayout.Height(ControlHeight));
                if (GUILayout.Button("Delete", button,
                        GUILayout.Width(ActionWidth), GUILayout.Height(ControlHeight)))
                {
                    string name = _confirmDelete;
                    _confirmDelete = null;
                    if (WeaponVfxStyleStore.TryDelete(name, out string reason))
                    {
                        if (string.Equals(_selectedStyle, name,
                                StringComparison.OrdinalIgnoreCase))
                            _selectedStyle = string.Empty;
                        _message = "Deleted '" + name + "'.";
                    }
                    else
                        _message = reason ?? "Could not delete style.";
                }
                if (GUILayout.Button("Cancel", button,
                        GUILayout.Width(ActionWidth), GUILayout.Height(ControlHeight)))
                    _confirmDelete = null;
                GUILayout.EndHorizontal();
            }

            // Row 2: saving is independent of the chosen style.
            GUILayout.BeginHorizontal(GUILayout.Height(ControlHeight));
            GUILayout.Label("Save Style", body, GUILayout.Width(LabelWidth));
            GUI.SetNextControlName(StyleNameControlName);
            string editedName = GUILayout.TextField(_saveName, 64,
                textField, GUILayout.Height(ControlHeight),
                GUILayout.MinWidth(120f), GUILayout.ExpandWidth(true));
            if (!string.Equals(editedName, _saveName, StringComparison.Ordinal))
            {
                _saveName = editedName;
                _confirmOverwrite = null;
            }
            previous = GUI.enabled;
            GUI.enabled = previous && canExport &&
                          !string.IsNullOrWhiteSpace(_saveName);
            if (GUILayout.Button("Save", button,
                    GUILayout.Width(ActionWidth), GUILayout.Height(ControlHeight)))
            {
                string name = _saveName.Trim();
                if (!WeaponVfxStyleCodec.TryNormalizeName(name, out _))
                    _message = "Style names must be 1–64 characters (not Default).";
                else
                {
                    bool exists = false;
                    foreach (string saved in WeaponVfxStyleStore.GetNames())
                    {
                        if (!string.Equals(saved, name,
                                StringComparison.OrdinalIgnoreCase))
                            continue;
                        exists = true;
                        break;
                    }
                    if (exists)
                        _confirmOverwrite = name;
                    else
                        SaveStyle(target, name);
                }
            }
            GUI.enabled = previous;
            GUILayout.EndHorizontal();

            if (_confirmOverwrite != null)
            {
                GUILayout.BeginHorizontal(GUILayout.Height(ControlHeight));
                GUILayout.Space(LabelWidth + RowGap);
                GUILayout.Label("Overwrite style?", warning,
                    GUILayout.MinWidth(66f), GUILayout.ExpandWidth(true),
                    GUILayout.Height(ControlHeight));
                previous = GUI.enabled;
                GUI.enabled = previous && canExport;
                if (GUILayout.Button("Overwrite", button,
                        GUILayout.Width(ActionWidth), GUILayout.Height(ControlHeight)))
                    SaveStyle(target, _confirmOverwrite);
                GUI.enabled = previous;
                if (GUILayout.Button("Cancel", button,
                        GUILayout.Width(ActionWidth), GUILayout.Height(ControlHeight)))
                    _confirmOverwrite = null;
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(7f);
            GUILayout.Label("SHARING", heading);
            GUILayout.BeginHorizontal(GUILayout.Height(ControlHeight));
            GUILayout.Label("Export", body, GUILayout.Width(LabelWidth));
            previous = GUI.enabled;
            GUI.enabled = previous && canExport;
            if (GUILayout.Button("Copy Code", button,
                    GUILayout.Width(ActionWidth), GUILayout.Height(ControlHeight)))
            {
                if (TryCurrentSnapshot(target,
                        out WeaponVfxState state, out uint cursor))
                {
                    if (WeaponVfxShareCode.TryEncode(state, cursor,
                            out string code, out string reason))
                    {
                        GUIUtility.systemCopyBuffer = code;
                        _message = "Copied " + code.Length + "-character share code.";
                        Plugin.Log?.LogInfo($"{Plugin.ModName}: [EditorShareCodeCopied] " +
                                            $"effects={state.Effects.Count} chars={code.Length}.");
                    }
                    else
                        _message = reason ?? "Cannot encode share code.";
                }
            }
            GUI.enabled = previous;
            GUILayout.Label("Current editor state", muted);
            GUILayout.EndHorizontal();

            GUILayout.Space(2f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Import", body, GUILayout.Width(LabelWidth));
            GUI.SetNextControlName(ShareInputControlName);
            string editedCode = GUILayout.TextArea(_shareInput,
                MaxShareCodeCharacters, textArea,
                GUILayout.MinHeight(46f), GUILayout.MaxHeight(46f),
                GUILayout.ExpandWidth(true));
            if (!string.Equals(editedCode, _shareInput, StringComparison.Ordinal))
            {
                _shareInput = editedCode;
                ClearPendingImport();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(GUILayout.Height(ControlHeight));
            GUILayout.Space(LabelWidth + RowGap);
            if (GUILayout.Button("Paste", button,
                    GUILayout.Width(ActionWidth), GUILayout.Height(ControlHeight)))
            {
                string clipboard = GUIUtility.systemCopyBuffer ?? string.Empty;
                if (clipboard.Length <= MaxShareCodeCharacters)
                {
                    _shareInput = clipboard;
                    ClearPendingImport();
                }
                else
                    _message = "Clipboard code exceeds size limit.";
            }
            previous = GUI.enabled;
            GUI.enabled = previous && editable &&
                          !string.IsNullOrWhiteSpace(_shareInput);
            GUILayout.Space(RowGap);
            if (GUILayout.Button("Import Code", button,
                    GUILayout.Width(ActionWidth + 12f), GUILayout.Height(ControlHeight)))
            {
                ClearPendingImport();
                if (WeaponVfxShareCode.TryDecode(_shareInput,
                        out WeaponVfxState imported, out uint cursor,
                        out string reason))
                {
                    _pendingImport = imported;
                    _pendingImportCursor = cursor;
                    _pendingImportItem = target.ItemData;
                    _message = "Code ready: " + imported.Effects.Count +
                               " effects. Confirm draft replacement below.";
                }
                else
                    _message = reason ?? "Invalid share code.";
            }
            GUI.enabled = previous;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (_pendingImport != null)
            {
                GUILayout.BeginHorizontal(GUILayout.Height(ControlHeight));
                GUILayout.Space(LabelWidth + RowGap);
                GUILayout.Label("Replace draft with " + _pendingImport.Effects.Count + " effects?",
                    warning, GUILayout.MinWidth(66f), GUILayout.ExpandWidth(true),
                    GUILayout.Height(ControlHeight));
                previous = GUI.enabled;
                GUI.enabled = previous && editable &&
                    ReferenceEquals(target?.ItemData, _pendingImportItem);
                if (GUILayout.Button("Replace Draft", button,
                        GUILayout.Width(ActionWidth + 12f), GUILayout.Height(ControlHeight)))
                {
                    QueueDraftApply(target, _pendingImport,
                        _pendingImportCursor, "Share code");
                    ClearPendingImport();
                }
                GUI.enabled = previous;
                if (GUILayout.Button("Cancel", button,
                        GUILayout.Width(ActionWidth), GUILayout.Height(ControlHeight)))
                    ClearPendingImport();
                GUILayout.EndHorizontal();
            }

            if (!string.IsNullOrEmpty(_message))
            {
                GUILayout.Space(3f);
                GUILayout.Label(_message, muted);
            }
            GUILayout.EndVertical();
        }
    }
}
