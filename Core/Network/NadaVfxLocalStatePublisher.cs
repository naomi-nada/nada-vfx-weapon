using System;
using NADA.VFX.Weapon.Core.State;

namespace NADA.VFX.Weapon.Core.Network
{
    internal sealed class NadaVfxLocalPublishedState
    {
        internal readonly int ItemHash;
        internal readonly uint Revision;
        internal readonly bool Bound;
        internal readonly int RecordCount;
        internal readonly byte[] PacketBytes;
        internal readonly string ItemName;

        internal NadaVfxLocalPublishedState(
            int itemHash,
            uint revision,
            bool bound,
            int recordCount,
            byte[] packetBytes,
            string itemName)
        {
            ItemHash = itemHash;
            Revision = revision;
            Bound = bound;
            RecordCount = recordCount;

            PacketBytes =
                packetBytes ??
                Array.Empty<byte>();

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

            bool bound =
                VfxStateIO.IsBound(
                    _currentRightItem);

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