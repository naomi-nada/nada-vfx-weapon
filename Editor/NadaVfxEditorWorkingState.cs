using System;
using System.Collections.Generic;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Blocks.Effects;
using NADA.VFX.Weapon.Core.State.Defaults;

namespace NADA.VFX.Weapon.Editor
{
    internal static class NadaVfxEditorWorkingState
    {
        private const int MaxDisplayNameLength = 64;

        private static global::ItemDrop.ItemData _targetItemData;

        private static WeaponVfxState _state =
            new();

        private static readonly Dictionary<string, int>
            NextNameOrdinalByTypeId =
                new();

        internal static WeaponVfxState State =>
            _state;

        internal static int EffectCount =>
            _state?.Effects?.Count ??
            0;

        /// <summary>
        /// Makes the current equipped item the owner of this temporary editor
        /// working state.
        ///
        /// Returning true means the editor target changed and the previous
        /// in-memory state was discarded.
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

            ResetWorkingState();

            if (_targetItemData != null)
            {
                Plugin.Log?.LogInfo(
                    $"{Plugin.ModName}: [EditorWorkingState] " +
                    $"target changed; created empty in-memory block state " +
                    $"for '{target?.PrefabName ?? "<unknown>"}'.");
            }

            return true;
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

            block.Enabled =
                enabled;

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

            // Reset authored modifier data without replacing the logical block.
            //
            // Identity, name, enabled state, and list position remain owned by
            // the existing VfxEffectBlock. Runtime reconciliation should
            // therefore update the existing instance rather than create a new one.
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

        private static void ResetWorkingState()
        {
            _state =
                new WeaponVfxState();

            NextNameOrdinalByTypeId.Clear();
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
            uint highest =
                0;

            foreach (VfxEffectBlock block in
                     _state.Effects)
            {
                if (block == null)
                    continue;

                if (block.InstanceId >
                    highest)
                {
                    highest =
                        block.InstanceId;
                }
            }

            if (highest ==
                uint.MaxValue)
            {
                return 0;
            }

            uint next =
                highest + 1;

            if (next == 0)
                return 0;

            return next;
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

            if (NextNameOrdinalByTypeId.TryGetValue(
                    typeId,
                    out int nextOrdinal))
            {
                ordinal =
                    nextOrdinal;
            }

            NextNameOrdinalByTypeId[
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