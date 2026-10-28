
using System;
using NADA.VFX.Weapon.Core.State;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Weapons.Runtime;

namespace NADA.VFX.Weapon.Core.Network
{
    internal sealed class NadaVfxRightHandPublishedState
    {
        internal readonly int ItemHash;
        internal readonly uint EquipmentGeneration;
        internal readonly uint Revision;
        internal readonly bool Bound;
        internal readonly int RecordCount;
        internal readonly byte[] PacketBytes;
        internal readonly bool NativeBound;
        internal readonly int NativeBlockCount;
        internal readonly byte[] NativePacketBytes;
        internal readonly string ItemName;

        internal NadaVfxRightHandPublishedState(
            int itemHash,
            uint equipmentGeneration,
            uint revision,
            bool bound,
            int recordCount,
            byte[] packetBytes,
            bool nativeBound,
            int nativeBlockCount,
            byte[] nativePacketBytes,
            string itemName)
        {
            ItemHash = itemHash;
            EquipmentGeneration = equipmentGeneration;
            Revision = revision;
            Bound = bound;
            RecordCount = recordCount;
            PacketBytes = packetBytes ?? Array.Empty<byte>();
            NativeBound = nativeBound;
            NativeBlockCount = nativeBlockCount;
            NativePacketBytes = nativePacketBytes ?? Array.Empty<byte>();
            ItemName = itemName ?? "<unknown>";
        }
    }

    internal static class NadaVfxRightHandStatePublisher
    {
        // A generation identifies one equipped ItemData reference.
        // The replicated ZDO carries this identity, not VFX data.
        internal const string EquipmentGenerationKey =
            "nada.vfx.weapon.right.equip_generation.v1";

        private static global::ItemDrop.ItemData _currentRightItem;
        private static int _currentRightItemHash;
        private static uint _equipmentGeneration;
        private static uint _nextEquipmentGeneration = 1;

        private static NadaVfxRightHandPublishedState
            _currentPublishedState;

        private static bool _currentStateDirty;
        private static uint _nextRevision = 1;

        internal static void ResetSession()
        {
            _currentRightItem = null;
            _currentRightItemHash = 0;
            _equipmentGeneration = 0;
            _nextEquipmentGeneration = 1;
            _currentPublishedState = null;
            _currentStateDirty = false;
            _nextRevision = 1;
        }

        internal static void ObserveRight(
            global::ItemDrop.ItemData itemData,
            int itemHash,
            global::ZDO zdo)
        {
            if (zdo == null)
                return;

            // Both missing means a genuinely empty hand. A mismatch
            // between ItemData and the vanilla hash is transitional.
            if (itemData == null && itemHash == 0)
            {
                _currentRightItem = null;
                _currentRightItemHash = 0;
                _equipmentGeneration = 0;
                _currentPublishedState = null;
                _currentStateDirty = false;

                if (zdo.GetInt(EquipmentGenerationKey, 0) != 0)
                    zdo.Set(EquipmentGenerationKey, 0);

                return;
            }

            if (itemData == null || itemHash == 0)
                return;

            bool itemChanged =
                !ReferenceEquals(_currentRightItem, itemData);

            bool hashChanged =
                _currentRightItemHash != itemHash;

            if (itemChanged || hashChanged)
            {
                _currentRightItem = itemData;
                _currentRightItemHash = itemHash;
                _currentPublishedState = null;
                _currentStateDirty = true;

                _equipmentGeneration = _nextEquipmentGeneration++;

                if (_nextEquipmentGeneration == 0 ||
                    _nextEquipmentGeneration > int.MaxValue)
                {
                    _nextEquipmentGeneration = 1;
                }

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [RightNativeEquipmentGeneration] " +
                    $"hash={itemHash} generation={_equipmentGeneration}");
            }

            // Write the equipment identity before announcing its VFX.
            if (zdo.GetInt(EquipmentGenerationKey, 0) !=
                unchecked((int)_equipmentGeneration))
            {
                zdo.Set(
                    EquipmentGenerationKey,
                    unchecked((int)_equipmentGeneration));
            }

            if (_currentStateDirty)
                PublishCurrent("dirty");
        }

        internal static void MarkDirty(
            global::ItemDrop.ItemData itemData)
        {
            if (itemData == null ||
                !ReferenceEquals(itemData, _currentRightItem))
            {
                return;
            }

            _currentStateDirty = true;

            if (_currentRightItemHash != 0)
                PublishCurrent("state-change");
        }

        internal static bool TryGetCurrent(
            out NadaVfxRightHandPublishedState publishedState)
        {
            publishedState = _currentPublishedState;
            return publishedState != null;
        }

        private static void PublishCurrent(string reason)
        {
            if (_currentRightItem == null ||
                _currentRightItemHash == 0 ||
                _equipmentGeneration == 0)
            {
                return;
            }

            NadaWeaponLocalSourceSelection source =
                NadaWeaponLocalSourceResolver.Resolve(
                    _currentRightItem);

            // Legacy V2 intentionally remains unbound when native
            // block state is authoritative.
            bool bound =
                source.Kind == NadaWeaponLocalSourceKind.LegacyBound;

            VfxState state = default;

            if (bound &&
                !VfxStateIO.TryRead(_currentRightItem, out state))
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [NetworkLocalState FAIL-CLOSED] " +
                    $"item='{GetItemName(_currentRightItem)}' " +
                    $"hash={_currentRightItemHash} " +
                    $"reason=persisted-state-read-failed");

                bound = false;
                state = default;
            }

            uint revision = TakeNextRevision();

            NadaVfxNetworkPacket packet;
            byte[] packetBytes;

            try
            {
                packet =
                    NadaVfxNetworkCodecV2.BuildPacketFromState(
                        _currentRightItemHash,
                        revision,
                        bound,
                        state);

                packetBytes =
                    NadaVfxNetworkCodecV2.SerializePacket(packet);
            }
            catch (Exception error)
            {
                _currentPublishedState = null;
                _currentStateDirty = true;

                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [NetworkLocalState FAIL] " +
                    $"item='{GetItemName(_currentRightItem)}' " +
                    $"hash={_currentRightItemHash} " +
                    $"reason={reason} error={error}");

                return;
            }

            bool nativeBound =
                source.Kind == NadaWeaponLocalSourceKind.NativeBound;

            WeaponVfxState nativeState =
                nativeBound ? source.State : null;

            uint nextInstanceId =
                nativeBound ? source.NextInstanceId : 0;

            if (!WeaponVfxNetworkCodec.TryEncode(
                    _currentRightItemHash,
                    revision,
                    nativeBound,
                    nativeState,
                    nextInstanceId,
                    out byte[] nativePacketBytes,
                    out string nativeReason))
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [NetworkNativePublish FAIL-CLOSED] " +
                    $"item='{GetItemName(_currentRightItem)}' " +
                    $"hash={_currentRightItemHash} revision={revision} " +
                    $"reason={nativeReason}");

                nativeBound = false;
                nativeState = null;

                if (!WeaponVfxNetworkCodec.TryEncode(
                        _currentRightItemHash,
                        revision,
                        false,
                        null,
                        0,
                        out nativePacketBytes,
                        out nativeReason))
                {
                    _currentPublishedState = null;
                    _currentStateDirty = true;

                    Plugin.Log.LogWarning(
                        $"{Plugin.ModName}: [NetworkNativePublish FAIL] " +
                        $"reason={nativeReason}");

                    return;
                }
            }

            if (source.Kind == NadaWeaponLocalSourceKind.InvalidNative)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [NetworkNativePublish FAIL-CLOSED] " +
                    $"item='{GetItemName(_currentRightItem)}' " +
                    $"hash={_currentRightItemHash} revision={revision} " +
                    $"reason={source.FailureReason ?? "invalid-native-item-state"}");
            }

            int nativeBlockCount =
                nativeBound ? nativeState.Effects.Count : 0;

            string itemName = GetItemName(_currentRightItem);

            _currentPublishedState =
                new NadaVfxRightHandPublishedState(
                    _currentRightItemHash,
                    _equipmentGeneration,
                    revision,
                    bound,
                    packet.Records.Count,
                    packetBytes,
                    nativeBound,
                    nativeBlockCount,
                    nativePacketBytes,
                    itemName);

            _currentStateDirty = false;

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [NetworkLocalState] " +
                $"item='{itemName}' " +
                $"hash={_currentRightItemHash} " +
                $"generation={_equipmentGeneration} " +
                $"revision={revision} bound={bound} " +
                $"records={packet.Records.Count} " +
                $"bytes={packetBytes.Length} reason={reason}");

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [NetworkNativeLocalState] " +
                $"item='{itemName}' " +
                $"hash={_currentRightItemHash} " +
                $"generation={_equipmentGeneration} " +
                $"revision={revision} source={source.Kind} " +
                $"bound={nativeBound} blocks={nativeBlockCount} " +
                $"bytes={nativePacketBytes.Length} reason={reason}");

            // Keep the compatibility pipeline untouched during migration.
            NadaVfxRightHandNetworkTransport
                .NotifyLocalStateChanged(_currentPublishedState);

            // Native V2 sends notifications only to V2 subscribers.
            // Until the remote tracker is connected, this has no recipients.
            NadaVfxRightHandNativeV2Transport
                .NotifyChanged(_currentPublishedState);
        }

        private static uint TakeNextRevision()
        {
            uint revision = _nextRevision++;

            // Revision zero means "no known state" in requests.
            if (revision == 0)
                revision = _nextRevision++;

            if (_nextRevision == 0)
                _nextRevision = 1;

            return revision;
        }

        private static string GetItemName(
            global::ItemDrop.ItemData itemData)
        {
            return itemData?.m_shared?.m_name ?? "<unknown>";
        }
    }
}
