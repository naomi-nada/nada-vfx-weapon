using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Blocks.Effects;
using NADA.VFX.Weapon.Core.State.Defaults;
using NADA.VFX.Weapon.Runtime.Formation;

namespace NADA.VFX.Weapon.Editor
{
    internal static class NadaVfxEditorWorkingState
    {
        private const int MaxDisplayNameLength = 64;

        private sealed class EditorDraft
        {
            internal WeaponVfxState State { get; } =
                new();

            internal Dictionary<string, int>
                NextNameOrdinalByTypeId { get; } =
                    new();

            // Keep IDs retired for the lifetime of this weapon draft.
            // Scanning only surviving effects would reuse a deleted ID.
            internal uint NextInstanceId { get; set; } = 1;
        }

        private sealed class ItemDataReferenceComparer :
            IEqualityComparer<global::ItemDrop.ItemData>
        {
            internal static readonly
                ItemDataReferenceComparer Instance =
                    new();

            public bool Equals(
                global::ItemDrop.ItemData left,
                global::ItemDrop.ItemData right)
            {
                return object.ReferenceEquals(
                    left,
                    right);
            }

            public int GetHashCode(
                global::ItemDrop.ItemData itemData)
            {
                return itemData == null
                    ? 0
                    : RuntimeHelpers.GetHashCode(
                        itemData);
            }
        }

        private static readonly Dictionary<
            global::ItemDrop.ItemData,
            EditorDraft>
            DraftByItemData =
                new(
                    ItemDataReferenceComparer.Instance);

        private static global::ItemDrop.ItemData _targetItemData;

        private static EditorDraft _activeDraft;

        private static WeaponVfxState _state =
            new();

        private static Dictionary<string, int>
            _nextNameOrdinalByTypeId =
                new();

        internal static WeaponVfxState State =>
            _state;

        internal static int EffectCount =>
            _state?.Effects?.Count ??
            0;

        /// <summary>
        /// Selects the temporary editor draft owned by the current ItemData.
        ///
        /// Drafts are retained for the lifetime of the editor session, so
        /// switching away from a weapon and returning to it does not discard
        /// unsaved authoring work.
        /// </summary>
        internal static bool SynchronizeTarget(
            NadaVfxEditorTarget target)
        {
            global::ItemDrop.ItemData nextItemData =
                target?.ItemData;

            if (object.ReferenceEquals(
                    nextItemData,
                    _targetItemData))
            {
                return false;
            }

            _targetItemData =
                nextItemData;

            if (_targetItemData == null)
            {
                _activeDraft =
                    null;

                _state =
                    new WeaponVfxState();

                _nextNameOrdinalByTypeId =
                    new Dictionary<string, int>();

                return true;
            }

            if (DraftByItemData.TryGetValue(
                    _targetItemData,
                    out EditorDraft existingDraft))
            {
                _activeDraft =
                    existingDraft;

                _state =
                    existingDraft.State;

                _nextNameOrdinalByTypeId =
                    existingDraft.NextNameOrdinalByTypeId;

                Plugin.Log?.LogInfo(
                    $"{Plugin.ModName}: [EditorWorkingState] " +
                    $"restored in-memory draft " +
                    $"for '{target?.PrefabName ?? "<unknown>"}' " +
                    $"effects={_state.Effects?.Count ?? 0}.");

                return true;
            }

            var newDraft =
                new EditorDraft();

            DraftByItemData.Add(
                _targetItemData,
                newDraft);

            _activeDraft =
                newDraft;

            _state =
                newDraft.State;

            _nextNameOrdinalByTypeId =
                newDraft.NextNameOrdinalByTypeId;

            Plugin.Log?.LogInfo(
                $"{Plugin.ModName}: [EditorWorkingState] " +
                $"created in-memory draft " +
                $"for '{target?.PrefabName ?? "<unknown>"}'.");

            return true;
        }

        internal static void ClearDrafts()
        {
            DraftByItemData.Clear();

            _targetItemData =
                null;

            _activeDraft =
                null;

            _state =
                new WeaponVfxState();

            _nextNameOrdinalByTypeId =
                new Dictionary<string, int>();
        }

        internal static bool TryAddEffect(
            string typeId,
            out VfxEffectBlock block)
        {
            block =
                null;

            if (!CanEdit())
                return false;

            uint instanceId =
                AllocateInstanceId();

            if (instanceId == 0)
                return false;

            VfxEffectBlock created =
                WeaponVfxBlockDefaults.Create(
                    typeId,
                    instanceId);

            if (created == null)
            {
                Plugin.Log?.LogWarning(
                    $"{Plugin.ModName}: [EditorBlockAdd] " +
                    $"could not create block " +
                    $"type='{typeId ?? "<null>"}'.");

                return false;
            }

            created.DisplayName =
                AllocateDisplayName(
                    created.TypeId);

            _state.Effects.Add(
                created);

            CommitAllocatedInstanceId(
                instanceId);

            block =
                created;

            Plugin.Log?.LogInfo(
                $"{Plugin.ModName}: [EditorBlockAdd] " +
                $"type='{created.TypeId}' " +
                $"id={created.InstanceId} " +
                $"name='{created.DisplayName}' " +
                $"effects={_state.Effects.Count} " +
                $"previewPending=True persisted=False.");

            return true;
        }

        internal static bool TrySetEnabled(
            uint instanceId,
            bool enabled)
        {
            VfxEffectBlock block =
                FindBlock(
                    instanceId);

            if (block == null)
                return false;

            if (block.Enabled == enabled)
                return true;

            // Enabling a dormant orbital leader can reactivate Glue
            // relationships that now conflict with another active leader.
            // Validate the proposed state before allowing the preview to
            // consume it. Disabling always remains available.
            block.Enabled =
                enabled;

            if (enabled && IsOrbitalType(block.TypeId))
            {
                bool valid =
                    OrbitalsFormationResolver.TryResolve(
                        _state,
                        out OrbitalsFormationResolution resolution);

                if (!valid || resolution == null || !resolution.IsValid)
                {
                    block.Enabled =
                        false;

                    string reason =
                        resolution?.FailureReason ??
                        "formation-resolution-failed";

                    Plugin.Log?.LogWarning(
                        $"{Plugin.ModName}: [EditorBlockEnableRejected] " +
                        $"id={block.InstanceId} " +
                        $"type='{block.TypeId}' " +
                        $"reason='{reason}'.");

                    return false;
                }
            }

            Plugin.Log?.LogInfo(
                $"{Plugin.ModName}: [EditorBlockEnabled] " +
                $"id={block.InstanceId} " +
                $"type='{block.TypeId}' " +
                $"enabled={block.Enabled}.");

            return true;
        }

        internal static bool TryRenameEffect(
            uint instanceId,
            string displayName)
        {
            VfxEffectBlock block =
                FindBlock(
                    instanceId);

            if (block == null)
                return false;

            string normalized =
                NormalizeDisplayName(
                    displayName);

            if (string.IsNullOrWhiteSpace(
                    normalized))
            {
                return false;
            }

            if (string.Equals(
                    block.DisplayName,
                    normalized,
                    StringComparison.Ordinal))
            {
                return true;
            }

            string previous =
                GetDisplayName(
                    block);

            block.DisplayName =
                normalized;

            Plugin.Log?.LogInfo(
                $"{Plugin.ModName}: [EditorBlockRename] " +
                $"id={block.InstanceId} " +
                $"type='{block.TypeId}' " +
                $"from='{previous}' " +
                $"to='{block.DisplayName}'.");

            return true;
        }

        internal static bool TryResetEffect(
            uint instanceId)
        {
            if (!CanEdit())
                return false;

            VfxEffectBlock block =
                FindBlock(
                    instanceId);

            if (block == null)
                return false;

            VfxEffectBlock defaultBlock =
                WeaponVfxBlockDefaults.Create(
                    block.TypeId,
                    block.InstanceId);

            if (defaultBlock == null ||
                defaultBlock.Transform == null ||
                defaultBlock.Settings == null)
            {
                Plugin.Log?.LogWarning(
                    $"{Plugin.ModName}: [EditorBlockReset] " +
                    $"could not create defaults " +
                    $"id={block.InstanceId} " +
                    $"type='{block.TypeId ?? "<null>"}'.");

                return false;
            }

            block.Transform =
                defaultBlock.Transform;

            block.Settings =
                defaultBlock.Settings;

            Plugin.Log?.LogInfo(
                $"{Plugin.ModName}: [EditorBlockReset] " +
                $"id={block.InstanceId} " +
                $"type='{block.TypeId}' " +
                $"name='{GetDisplayName(block)}' " +
                $"enabled={block.Enabled} " +
                $"identityPreserved=True.");

            return true;
        }

        internal static bool TryDuplicateEffect(
            uint sourceInstanceId,
            out VfxEffectBlock duplicate)
        {
            duplicate =
                null;

            if (!CanEdit())
                return false;

            int sourceIndex =
                FindBlockIndex(
                    sourceInstanceId);

            if (sourceIndex < 0)
                return false;

            VfxEffectBlock source =
                _state.Effects[
                    sourceIndex];

            if (source == null)
                return false;

            uint instanceId =
                AllocateInstanceId();

            if (instanceId == 0)
                return false;

            string displayName =
                AllocateDisplayName(
                    source.TypeId);

            if (!VfxEffectBlockDuplicator.TryDuplicate(
                    source,
                    instanceId,
                    displayName,
                    out VfxEffectBlock created))
            {
                Plugin.Log?.LogWarning(
                    $"{Plugin.ModName}: [EditorBlockDuplicate] " +
                    $"could not duplicate " +
                    $"id={source.InstanceId} " +
                    $"type='{source.TypeId}'.");

                return false;
            }

            int insertIndex =
                Math.Min(
                    sourceIndex + 1,
                    _state.Effects.Count);

            _state.Effects.Insert(
                insertIndex,
                created);

            CommitAllocatedInstanceId(
                instanceId);

            duplicate =
                created;

            Plugin.Log?.LogInfo(
                $"{Plugin.ModName}: [EditorBlockDuplicate] " +
                $"sourceId={source.InstanceId} " +
                $"newId={created.InstanceId} " +
                $"type='{created.TypeId}' " +
                $"name='{created.DisplayName}' " +
                $"index={insertIndex} " +
                $"glueRelationshipsCopied=False.");

            return true;
        }

        internal static bool TryDeleteEffect(
            uint instanceId,
            out uint? nextSelectionInstanceId)
        {
            nextSelectionInstanceId =
                null;

            if (!CanEdit())
                return false;

            int index =
                FindBlockIndex(
                    instanceId);

            if (index < 0)
                return false;

            VfxEffectBlock block =
                _state.Effects[
                    index];

            string typeId =
                block?.TypeId ??
                "<unknown>";

            RemoveGlueReferencesTo(
                instanceId);

            _state.Effects.RemoveAt(
                index);

            nextSelectionInstanceId =
                FindNearestSelection(
                    index);

            Plugin.Log?.LogInfo(
                $"{Plugin.ModName}: [EditorBlockDelete] " +
                $"id={instanceId} " +
                $"type='{typeId}' " +
                $"effects={_state.Effects.Count}.");

            return true;
        }

        internal static bool TryMoveEffect(
            uint instanceId,
            int offset)
        {
            if (!CanEdit() ||
                offset == 0)
            {
                return false;
            }

            int oldIndex =
                FindBlockIndex(
                    instanceId);

            if (oldIndex < 0)
                return false;

            int newIndex =
                oldIndex + offset;

            if (newIndex < 0 ||
                newIndex >= _state.Effects.Count)
            {
                return false;
            }

            VfxEffectBlock block =
                _state.Effects[
                    oldIndex];

            _state.Effects.RemoveAt(
                oldIndex);

            _state.Effects.Insert(
                newIndex,
                block);

            Plugin.Log?.LogInfo(
                $"{Plugin.ModName}: [EditorBlockReorder] " +
                $"id={instanceId} " +
                $"from={oldIndex} " +
                $"to={newIndex}.");

            return true;
        }

        internal static bool CanMoveUp(
            uint instanceId)
        {
            return
                FindBlockIndex(
                    instanceId) > 0;
        }

        internal static bool CanMoveDown(
            uint instanceId)
        {
            int index =
                FindBlockIndex(
                    instanceId);

            return
                index >= 0 &&
                _state?.Effects != null &&
                index <
                _state.Effects.Count - 1;
        }

        internal static string GetDisplayName(
            VfxEffectBlock block)
        {
            if (block == null)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(
                    block.DisplayName))
            {
                return block.DisplayName;
            }

            NadaVfxEditorEffectDefinition definition =
                NadaVfxEditorEffectCatalog.Find(
                    block.TypeId);

            string baseName =
                definition?.GeneratedNameBase ??
                "Effect";

            return
                $"{baseName}{block.InstanceId:000}";
        }

        private static bool CanEdit()
        {
            return
                _targetItemData != null &&
                _state != null &&
                _state.Effects != null;
        }

        private static VfxEffectBlock FindBlock(
            uint instanceId)
        {
            int index =
                FindBlockIndex(
                    instanceId);

            if (index < 0)
                return null;

            return
                _state.Effects[
                    index];
        }

        private static int FindBlockIndex(
            uint instanceId)
        {
            if (_state?.Effects == null)
                return -1;

            for (int i = 0;
                 i < _state.Effects.Count;
                 i++)
            {
                VfxEffectBlock block =
                    _state.Effects[i];

                if (block != null &&
                    block.InstanceId ==
                    instanceId)
                {
                    return i;
                }
            }

            return -1;
        }

        private static uint? FindNearestSelection(
            int removedIndex)
        {
            if (_state?.Effects == null ||
                _state.Effects.Count == 0)
            {
                return null;
            }

            int start =
                Math.Min(
                    removedIndex,
                    _state.Effects.Count - 1);

            for (int i = start;
                 i < _state.Effects.Count;
                 i++)
            {
                VfxEffectBlock block =
                    _state.Effects[i];

                if (block != null)
                    return block.InstanceId;
            }

            for (int i = start - 1;
                 i >= 0;
                 i--)
            {
                VfxEffectBlock block =
                    _state.Effects[i];

                if (block != null)
                    return block.InstanceId;
            }

            return null;
        }

        private static void RemoveGlueReferencesTo(
            uint removedInstanceId)
        {
            if (_state?.Effects == null)
                return;

            foreach (VfxEffectBlock block in
                     _state.Effects)
            {
                if (block == null ||
                    block.InstanceId ==
                    removedInstanceId)
                {
                    continue;
                }

                OrbitalsFormationVfxSettings formation =
                    GetFormation(
                        block);

                if (formation?.GlueTargetInstanceIds == null)
                    continue;

                formation.GlueTargetInstanceIds.RemoveAll(
                    id =>
                        id ==
                        removedInstanceId);
            }
        }

        private static bool IsOrbitalType(string typeId)
        {
            return
                typeId == VfxEffectTypeIds.OrbitalsOrbs ||
                typeId == VfxEffectTypeIds.OrbitalsCores ||
                typeId == VfxEffectTypeIds.OrbitalsFlames ||
                typeId == VfxEffectTypeIds.OrbitalsEmbers;
        }

        private static OrbitalsFormationVfxSettings GetFormation(
            VfxEffectBlock block)
        {
            if (block?.Settings == null)
                return null;

            return block.Settings switch
            {
                OrbitalsOrbsVfxSettings orbs =>
                    orbs.Formation,

                OrbitalsCoresVfxSettings cores =>
                    cores.Formation,

                OrbitalsFlamesVfxSettings flames =>
                    flames.Formation,

                OrbitalsEmbersVfxSettings embers =>
                    embers.Formation,

                _ =>
                    null
            };
        }

        private static uint AllocateInstanceId()
        {
            if (_activeDraft == null ||
                _state?.Effects == null ||
                _activeDraft.NextInstanceId == 0)
            {
                return 0;
            }

            uint next =
                _activeDraft.NextInstanceId;

            // Allow future draft initialization from existing state without
            // ever reusing IDs retired during this editor session.
            foreach (VfxEffectBlock block in
                     _state.Effects)
            {
                if (block == null ||
                    block.InstanceId < next)
                {
                    continue;
                }

                if (block.InstanceId == uint.MaxValue)
                    return 0;

                next =
                    block.InstanceId + 1;
            }

            return next;
        }

        private static void CommitAllocatedInstanceId(
            uint instanceId)
        {
            if (_activeDraft == null ||
                instanceId == 0)
            {
                return;
            }

            // Zero is reserved as the exhaustion sentinel; never wrap to 1.
            _activeDraft.NextInstanceId =
                instanceId == uint.MaxValue
                    ? 0
                    : instanceId + 1;
        }

        private static string AllocateDisplayName(
            string typeId)
        {
            NadaVfxEditorEffectDefinition definition =
                NadaVfxEditorEffectCatalog.Find(
                    typeId);

            string baseName =
                definition?.GeneratedNameBase ??
                "Effect";

            int ordinal =
                1;

            if (_nextNameOrdinalByTypeId.TryGetValue(
                    typeId,
                    out int nextOrdinal))
            {
                ordinal =
                    nextOrdinal;
            }

            _nextNameOrdinalByTypeId[
                typeId] =
                ordinal + 1;

            return
                $"{baseName}{ordinal:000}";
        }

        private static string NormalizeDisplayName(
            string value)
        {
            if (string.IsNullOrWhiteSpace(
                    value))
            {
                return string.Empty;
            }

            string normalized =
                value.Trim();

            if (normalized.Length >
                MaxDisplayNameLength)
            {
                normalized =
                    normalized.Substring(
                        0,
                        MaxDisplayNameLength);
            }

            return normalized;
        }
    }
}