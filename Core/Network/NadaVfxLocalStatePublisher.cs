using System;
using NADA.VFX.Weapon.Core.State;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Weapons.Runtime;

namespace NADA.VFX.Weapon.Core.Network
{
    internal sealed class NadaVfxLocalPublishedState
    {
        internal readonly int ItemHash;
        internal readonly uint Revision;
        internal readonly bool Bound;
        internal readonly int RecordCount;
        internal readonly byte[] PacketBytes;
        internal readonly bool NativeBound;
        internal readonly int NativeBlockCount;
        internal readonly byte[] NativePacketBytes;
        internal readonly string ItemName;

        internal NadaVfxLocalPublishedState(
            int itemHash,
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
            Revision = revision;
            Bound = bound;
            RecordCount = recordCount;

            PacketBytes =
                packetBytes ??
                Array.Empty<byte>();

            NativeBound = nativeBound;
            NativeBlockCount = nativeBlockCount;
            NativePacketBytes = nativePacketBytes ?? Array.Empty<byte>();

            ItemName =
                itemName ??
                "<unknown>";
        }
    }

    internal static class NadaVfxLocalStatePublisher
    {
        private static global::ItemDrop.ItemData _currentRightItem;

        private static int _currentRightItemHash;

        private static NadaVfxLocalPublishedState _currentPublishedState;

        private static bool _currentStateDirty;

        private static uint _nextRevision = 1;

        internal static void ResetSession()
        {
            _currentRightItem = null;
            _currentRightItemHash = 0;
            _currentPublishedState = null;
            _currentStateDirty = false;
            _nextRevision = 1;
        }

        internal static void ObserveRight(
            global::ItemDrop.ItemData itemData,
            int itemHash)
        {
            // Valheim can briefly expose incomplete equipment identity while
            // visuals are transitioning. Don't turn that into an authoritative
            // unbind or throw away the last known state.
            if (itemData == null ||
                itemHash == 0)
            {
                return;
            }

            bool itemChanged =
                !ReferenceEquals(
                    _currentRightItem,
                    itemData);

            bool hashChanged =
                _currentRightItemHash !=
                itemHash;

            if (itemChanged ||
                hashChanged)
            {
                _currentRightItem =
                    itemData;

                _currentRightItemHash =
                    itemHash;

                _currentPublishedState =
                    null;

                _currentStateDirty =
                    true;

                PublishCurrent(
                    "presentation-change");

                return;
            }

            if (_currentStateDirty)
            {
                PublishCurrent(
                    "dirty");
            }
        }

        internal static void MarkDirty(
            global::ItemDrop.ItemData itemData)
        {
            if (itemData == null)
                return;

            // State changes to inventory items that aren't currently presented
            // don't need network work yet. When that item is equipped,
            // ObserveRight() will resolve and publish its persisted state.
            if (!ReferenceEquals(
                    itemData,
                    _currentRightItem))
            {
                return;
            }

            _currentStateDirty =
                true;

            // If we already know the equipped item's vanilla identity, publish
            // immediately. This is what makes re-bind/unbind propagate without
            // waiting for some unrelated equipment refresh.
            if (_currentRightItemHash != 0)
            {
                PublishCurrent(
                    "state-change");
            }
        }

        internal static bool TryGetCurrent(
            out NadaVfxLocalPublishedState publishedState)
        {
            publishedState =
                _currentPublishedState;

            return
                publishedState != null;
        }

        private static void PublishCurrent(
            string reason)
        {
            if (_currentRightItem == null ||
                _currentRightItemHash == 0)
            {
                return;
            }

            NadaWeaponLocalSourceSelection source =
                NadaWeaponLocalSourceResolver.Resolve(
                    _currentRightItem);

            // V2 transport only represents legacy singleton VfxState.
            // A native-bound item must not publish stale legacy visuals.
            // Native state has its own packet; V2 remains unbound so an
            // older receiver cannot resurrect stale legacy effects.
            bool bound =
                source.Kind == NadaWeaponLocalSourceKind.LegacyBound;


            VfxState state =
                default;

            if (bound)
            {
                if (!VfxStateIO.TryRead(
                        _currentRightItem,
                        out state))
                {
                    // A bound item whose state cannot be decoded is not safe to
                    // render remotely. Publish a fail-closed unbound packet
                    // instead of leaving another client on stale VFX forever.
                    Plugin.Log.LogWarning(
                        $"{Plugin.ModName}: [NetworkLocalState FAIL-CLOSED] " +
                        $"item='{GetItemName(_currentRightItem)}' " +
                        $"hash={_currentRightItemHash} " +
                        $"reason=persisted-state-read-failed");

                    bound =
                        false;

                    state =
                        default;
                }
            }

            uint revision =
                TakeNextRevision();

            NadaVfxNetworkPacket packet;
            byte[] packetBytes;

            try
            {
                packet =
                    NadaVfxNetworkCodecV2
                        .BuildPacketFromState(
                            _currentRightItemHash,
                            revision,
                            bound,
                            state);

                packetBytes =
                    NadaVfxNetworkCodecV2
                        .SerializePacket(
                            packet);
            }
            catch (Exception e)
            {
                // Never leave a previous item masquerading as the current one.
                _currentPublishedState =
                    null;

                _currentStateDirty =
                    true;

                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [NetworkLocalState FAIL] " +
                    $"item='{GetItemName(_currentRightItem)}' " +
                    $"hash={_currentRightItemHash} " +
                    $"reason={reason} " +
                    $"error={e}");

                return;
            }

            // Both protocols use the same revision and vanilla item hash.
            // Transport never reads ItemData: it only ships prepared bytes.
            bool nativeBound =
                source.Kind == NadaWeaponLocalSourceKind.NativeBound;

            WeaponVfxState nativeState =
                nativeBound ? source.State : null;

            uint nextInstanceId =
                nativeBound ? source.NextInstanceId : 0;

            byte[] nativePacketBytes;
            if (!WeaponVfxNetworkCodec.TryEncode(
                    _currentRightItemHash,
                    revision,
                    nativeBound,
                    nativeState,
                    nextInstanceId,
                    out nativePacketBytes,
                    out string nativeReason))
            {
                // Invalid/oversized native state must not be replaced with
                // legacy state or partly transmitted. Publish a native unbind.
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [NetworkNativePublish FAIL-CLOSED] " +
                    $"item='{GetItemName(_currentRightItem)}' " +
                    $"hash={_currentRightItemHash} " +
                    $"revision={revision} " +
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
                    $"hash={_currentRightItemHash} " +
                    $"revision={revision} " +
                    $"reason={source.FailureReason ?? "invalid-native-item-state"}");
            }

            int nativeBlockCount = nativeBound
                ? nativeState.Effects.Count
                : 0;

            string itemName =
                GetItemName(
                    _currentRightItem);

            _currentPublishedState =
                new NadaVfxLocalPublishedState(
                    _currentRightItemHash,
                    revision,
                    bound,
                    packet.Records.Count,
                    packetBytes,
                    nativeBound,
                    nativeBlockCount,
                    nativePacketBytes,
                    itemName);

            _currentStateDirty =
                false;

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [NetworkLocalState] " +
                $"item='{itemName}' " +
                $"hash={_currentRightItemHash} " +
                $"revision={revision} " +
                $"bound={bound} " +
                $"records={packet.Records.Count} " +
                $"bytes={packetBytes.Length} " +
                $"reason={reason}");

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [NetworkNativeLocalState] " +
                $"item='{itemName}' " +
                $"hash={_currentRightItemHash} " +
                $"revision={revision} " +
                $"source={source.Kind} " +
                $"bound={nativeBound} " +
                $"blocks={nativeBlockCount} " +
                $"bytes={nativePacketBytes.Length} " +
                $"reason={reason}");

            NadaVfxNetworkTransport.NotifyLocalStateChanged(
                _currentPublishedState);
        }

        private static uint TakeNextRevision()
        {
            uint revision =
                _nextRevision++;

            // Revision zero is reserved as "I don't have a known revision yet"
            // in request messages.
            if (revision == 0)
            {
                revision =
                    _nextRevision++;
            }

            if (_nextRevision == 0)
            {
                _nextRevision =
                    1;
            }

            return revision;
        }

        private static string GetItemName(
            global::ItemDrop.ItemData itemData)
        {
            return
                itemData?.m_shared?.m_name ??
                "<unknown>";
        }
    }
}