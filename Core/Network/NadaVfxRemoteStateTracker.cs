using System;
using System.Collections.Generic;
using NADA.VFX.Weapon.Core.Debug;
using NADA.VFX.Weapon.Core.Network;
using NADA.VFX.Weapon.Core.State;
using NADA.VFX.Weapon.Runtime.Structure;
using UnityEngine;

namespace NADA.VFX.Weapon.Weapons.Runtime
{
    internal static class NadaVfxRemoteStateTracker
    {
        private static readonly NadaWeaponRigController
            WeaponRigController =
                new();

        private static readonly Dictionary<long, RemoteRightState>
            RemoteRightStates =
                new();

        private static bool _initialized;

        private sealed class RemoteRightState
        {
            internal long PeerId;

            internal int VisEquipmentId;

            internal GameObject RightInstance;

            internal int RightInstanceId;

            internal int ObservedItemHash;

            // The newest state revision the owner has told us exists.
            //
            // This is deliberately separate from ReceivedRevision. A change
            // notification can arrive while Valheim temporarily has no remote
            // weapon visual for us to attach to.
            internal bool HasAdvertisedState;

            internal int AdvertisedItemHash;

            internal uint AdvertisedRevision;

            internal bool AdvertisedBound;

            internal bool HasReceivedState;

            internal int ReceivedItemHash;

            internal uint ReceivedRevision;

            internal bool ReceivedBound;

            internal VfxState ReceivedState;

            internal GameObject AppliedInstance;

            internal int AppliedInstanceId;

            internal int AppliedItemHash;

            internal uint AppliedRevision;

            internal bool AppliedBound;

            internal bool RequestPending;

            internal int RequestedItemHash;

            internal int RequestAttempts;

            internal float NextRequestTime;
        }

        internal static void ObserveRight(
            long peerId,
            global::VisEquipment visEquipment,
            GameObject rightInstance,
            int itemHash)
        {
            EnsureInitialized();

            if (peerId == 0L ||
                visEquipment == null)
            {
                return;
            }

            if (!RemoteRightStates.TryGetValue(
                    peerId,
                    out RemoteRightState remoteState))
            {
                remoteState =
                    new RemoteRightState
                    {
                        PeerId = peerId
                    };

                RemoteRightStates[peerId] =
                    remoteState;
            }

            int visEquipmentId =
                visEquipment.GetInstanceID();

            int rightInstanceId =
                rightInstance != null
                    ? rightInstance.GetInstanceID()
                    : 0;

            bool visualOwnerChanged =
                remoteState.VisEquipmentId != 0 &&
                remoteState.VisEquipmentId !=
                    visEquipmentId;

            bool instanceChanged =
                remoteState.RightInstanceId !=
                    rightInstanceId;

            // Hash zero is commonly a transitional replication state. Do not
            // treat it as an authoritative weapon change.
            bool hasUsableItemHash =
                itemHash != 0;

            bool itemChanged =
                hasUsableItemHash &&
                remoteState.ObservedItemHash != 0 &&
                remoteState.ObservedItemHash !=
                    itemHash;

            if (visualOwnerChanged ||
                instanceChanged ||
                itemChanged)
            {
                RemovePreviouslyAppliedRigIfNeeded(
                    remoteState,
                    rightInstance,
                    rightInstanceId,
                    itemHash);

                ClearAppliedState(
                    remoteState);
            }

            remoteState.VisEquipmentId =
                visEquipmentId;

            remoteState.RightInstance =
                rightInstance;

            remoteState.RightInstanceId =
                rightInstanceId;

            if (!hasUsableItemHash)
            {
                return;
            }

            if (remoteState.ObservedItemHash !=
                itemHash)
            {
                remoteState.ObservedItemHash =
                    itemHash;

                remoteState.RequestPending =
                    false;

                remoteState.RequestedItemHash =
                    0;

                remoteState.RequestAttempts =
                    0;

                remoteState.NextRequestTime =
                    0f;

                if (remoteState.HasReceivedState &&
                    remoteState.ReceivedItemHash !=
                        itemHash)
                {
                    ClearReceivedState(
                        remoteState);
                }
            }

            if (rightInstance == null)
                return;

            // We may already have a usable cached state for this item, but if
            // the owner has advertised a newer revision we know that cache is
            // stale. Do not attach stale VFX to a newly-created visual while
            // we're waiting for the newer packet.
            if (HasNewerAdvertisedState(
                    remoteState,
                    itemHash))
            {
                RequestStateIfNeeded(
                    remoteState,
                    force: false);

                return;
            }

            if (remoteState.HasReceivedState &&
                remoteState.ReceivedItemHash ==
                    itemHash)
            {
                ApplyReceivedState(
                    remoteState);

                return;
            }

            RequestStateIfNeeded(
                remoteState,
                force: false);
        }

        private static void EnsureInitialized()
        {
            if (_initialized)
                return;

            _initialized =
                true;

            NadaVfxNetworkTransport.StateReceived +=
                OnStateReceived;

            NadaVfxNetworkTransport.StateChangedReceived +=
                OnStateChangedReceived;

            NadaVfxNetworkTransport.SessionReset +=
                OnSessionReset;
        }

        private static void OnSessionReset()
        {
            RemoteRightStates.Clear();
        }

        private static void OnStateReceived(
            long sender,
            NadaVfxNetworkPacket packet,
            VfxState state)
        {
            if (sender == 0L ||
                packet == null)
            {
                return;
            }

            if (!RemoteRightStates.TryGetValue(
                    sender,
                    out RemoteRightState remoteState))
            {
                // We only consume state from peers that correspond to an
                // actually observed remote player.
                return;
            }

            if (remoteState.ObservedItemHash == 0 ||
                packet.ItemHash !=
                    remoteState.ObservedItemHash)
            {
                NadaLogControl.Info(
                    $"remote-v2-stale:" +
                    $"{sender}:" +
                    $"{packet.ItemHash}:" +
                    $"{remoteState.ObservedItemHash}:" +
                    $"{packet.Revision}",
                    $"{Plugin.ModName}: [RemoteNetworkState STALE] " +
                    $"peer={sender} " +
                    $"payloadHash={packet.ItemHash} " +
                    $"visibleHash={remoteState.ObservedItemHash} " +
                    $"revision={packet.Revision}");

                // Don't cancel a pending request just because an older response
                // arrived after Valheim changed the visible weapon.
                return;
            }

            // A StateChanged notification may have reached us before an older
            // outstanding response. If we already know revision 4 exists,
            // revision 3 must never become authoritative again.
            if (HasAdvertisedStateNewerThanPacket(
                    remoteState,
                    packet.ItemHash,
                    packet.Revision))
            {
                NadaLogControl.Info(
                    $"remote-v2-behind-advertised:" +
                    $"{sender}:" +
                    $"{packet.ItemHash}:" +
                    $"{packet.Revision}:" +
                    $"{remoteState.AdvertisedRevision}",
                    $"{Plugin.ModName}: [RemoteNetworkState OLD] " +
                    $"peer={sender} " +
                    $"hash={packet.ItemHash} " +
                    $"receivedRevision={packet.Revision} " +
                    $"advertisedRevision={remoteState.AdvertisedRevision}");

                return;
            }

            if (remoteState.HasReceivedState &&
                remoteState.ReceivedItemHash ==
                    packet.ItemHash)
            {
                if (packet.Revision ==
                    remoteState.ReceivedRevision)
                {
                    // Same state again. Keep it, but still allow a newly
                    // recreated visual instance to consume it below.
                }
                else if (!IsRevisionNewer(
                             packet.Revision,
                             remoteState.ReceivedRevision))
                {
                    NadaLogControl.Info(
                        $"remote-v2-old-revision:" +
                        $"{sender}:" +
                        $"{packet.ItemHash}:" +
                        $"{packet.Revision}",
                        $"{Plugin.ModName}: [RemoteNetworkState OLD] " +
                        $"peer={sender} " +
                        $"hash={packet.ItemHash} " +
                        $"receivedRevision={packet.Revision} " +
                        $"currentRevision={remoteState.ReceivedRevision}");

                    return;
                }
            }

            // This response is current enough to become authoritative.
            remoteState.RequestPending =
                false;

            remoteState.RequestedItemHash =
                0;

            remoteState.RequestAttempts =
                0;

            remoteState.NextRequestTime =
                0f;

            RememberAdvertisedState(
                remoteState,
                packet.ItemHash,
                packet.Revision,
                packet.Bound);

            remoteState.HasReceivedState =
                true;

            remoteState.ReceivedItemHash =
                packet.ItemHash;

            remoteState.ReceivedRevision =
                packet.Revision;

            remoteState.ReceivedBound =
                packet.Bound;

            remoteState.ReceivedState =
                state;

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [RemoteNetworkStateAccepted] " +
                $"peer={sender} " +
                $"hash={packet.ItemHash} " +
                $"revision={packet.Revision} " +
                $"bound={packet.Bound} " +
                $"records={packet.Records.Count}");

            ApplyReceivedState(
                remoteState);
        }

        private static void OnStateChangedReceived(
            long sender,
            int itemHash,
            uint revision,
            bool bound)
        {
            if (sender == 0L ||
                itemHash == 0)
            {
                return;
            }

            if (!RemoteRightStates.TryGetValue(
                    sender,
                    out RemoteRightState remoteState))
            {
                return;
            }

            // Remember the notification even when the remote weapon visual is
            // temporarily absent or Valheim hasn't replicated the new item hash
            // yet. That is the race the first multiplayer test exposed.
            if (!RememberAdvertisedState(
                    remoteState,
                    itemHash,
                    revision,
                    bound))
            {
                return;
            }

            if (remoteState.ObservedItemHash !=
                itemHash)
            {
                // Equipment replication has not caught up yet. Keep the
                // advertisement and wait for ObserveRight() to see this hash.
                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [RemoteStateRefreshDeferred] " +
                    $"peer={sender} " +
                    $"hash={itemHash} " +
                    $"revision={revision} " +
                    $"reason=visible-item-mismatch " +
                    $"visibleHash={remoteState.ObservedItemHash}");

                return;
            }

            if (remoteState.HasReceivedState &&
                remoteState.ReceivedItemHash ==
                    itemHash &&
                remoteState.ReceivedRevision ==
                    revision)
            {
                return;
            }

            remoteState.RequestPending =
                false;

            remoteState.RequestedItemHash =
                0;

            remoteState.NextRequestTime =
                0f;

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [RemoteStateRefreshNeeded] " +
                $"peer={sender} " +
                $"hash={itemHash} " +
                $"revision={revision} " +
                $"bound={bound}");

            if (remoteState.RightInstance == null)
            {
                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [RemoteStateRefreshDeferred] " +
                    $"peer={sender} " +
                    $"hash={itemHash} " +
                    $"revision={revision} " +
                    $"reason=no-right-instance");

                return;
            }

            RequestStateIfNeeded(
                remoteState,
                force: true);
        }

        private static void RequestStateIfNeeded(
            RemoteRightState remoteState,
            bool force)
        {
            if (remoteState == null ||
                remoteState.PeerId == 0L ||
                remoteState.ObservedItemHash == 0 ||
                remoteState.RightInstance == null)
            {
                return;
            }

            float now =
                Time.unscaledTime;

            if (!force)
            {
                if (remoteState.RequestPending &&
                    now <
                        remoteState.NextRequestTime)
                {
                    return;
                }

                if (now <
                    remoteState.NextRequestTime)
                {
                    return;
                }
            }

            uint knownRevision =
                remoteState.HasReceivedState &&
                remoteState.ReceivedItemHash ==
                    remoteState.ObservedItemHash
                    ? remoteState.ReceivedRevision
                    : 0u;

            bool sent =
                NadaVfxNetworkTransport.RequestState(
                    remoteState.PeerId,
                    remoteState.ObservedItemHash,
                    knownRevision);

            if (!sent)
                return;

            remoteState.RequestPending =
                true;

            remoteState.RequestedItemHash =
                remoteState.ObservedItemHash;

            remoteState.RequestAttempts++;

            remoteState.NextRequestTime =
                now +
                GetRetryDelay(
                    remoteState.RequestAttempts);
        }

        private static float GetRetryDelay(
            int attempt)
        {
            if (attempt <= 1)
                return 1f;

            if (attempt == 2)
                return 2f;

            if (attempt == 3)
                return 5f;

            if (attempt == 4)
                return 10f;

            return 30f;
        }

        private static void ApplyReceivedState(
            RemoteRightState remoteState)
        {
            if (remoteState == null ||
                !remoteState.HasReceivedState ||
                remoteState.RightInstance == null ||
                remoteState.ObservedItemHash == 0 ||
                remoteState.ReceivedItemHash !=
                    remoteState.ObservedItemHash)
            {
                return;
            }

            // Once we know a newer authoritative revision exists, the cached
            // state is no longer eligible to be attached to a new visual.
            if (HasNewerAdvertisedState(
                    remoteState,
                    remoteState.ReceivedItemHash))
            {
                return;
            }

            int rightInstanceId =
                remoteState.RightInstance.GetInstanceID();

            if (IsAppliedStateCurrent(
                    remoteState,
                    rightInstanceId))
            {
                return;
            }

            if (!remoteState.ReceivedBound)
            {
                NadaWeaponRigRemoval
                    .RemoveFromEquippedRoot(
                        remoteState.RightInstance);

                RememberAppliedState(
                    remoteState,
                    rightInstanceId);

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [RemoteStateRemoved] " +
                    $"peer={remoteState.PeerId} " +
                    $"hash={remoteState.ReceivedItemHash} " +
                    $"revision={remoteState.ReceivedRevision} " +
                    $"reason=unbound");

                return;
            }

            bool applied =
                WeaponRigController
                    .TryApplyResolvedState(
                        remoteState.RightInstance,
                        remoteState.ReceivedItemHash,
                        remoteState.ReceivedState);

            if (!applied)
            {
                NadaLogControl.Info(
                    $"remote-v2-apply-fail:" +
                    $"{remoteState.PeerId}:" +
                    $"{rightInstanceId}:" +
                    $"{remoteState.ReceivedItemHash}:" +
                    $"{remoteState.ReceivedRevision}",
                    $"{Plugin.ModName}: [RemoteStateApply FAIL] " +
                    $"peer={remoteState.PeerId} " +
                    $"hash={remoteState.ReceivedItemHash} " +
                    $"revision={remoteState.ReceivedRevision} " +
                    $"instance={rightInstanceId}");

                return;
            }

            RememberAppliedState(
                remoteState,
                rightInstanceId);

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [RemoteStateApplied] " +
                $"peer={remoteState.PeerId} " +
                $"hash={remoteState.ReceivedItemHash} " +
                $"revision={remoteState.ReceivedRevision} " +
                $"instance={rightInstanceId}");
        }

        private static bool HasNewerAdvertisedState(
            RemoteRightState remoteState,
            int itemHash)
        {
            if (remoteState == null ||
                !remoteState.HasAdvertisedState ||
                remoteState.AdvertisedItemHash !=
                    itemHash)
            {
                return false;
            }

            if (!remoteState.HasReceivedState ||
                remoteState.ReceivedItemHash !=
                    itemHash)
            {
                return true;
            }

            return
                IsRevisionNewer(
                    remoteState.AdvertisedRevision,
                    remoteState.ReceivedRevision);
        }

        private static bool HasAdvertisedStateNewerThanPacket(
            RemoteRightState remoteState,
            int itemHash,
            uint packetRevision)
        {
            if (remoteState == null ||
                !remoteState.HasAdvertisedState ||
                remoteState.AdvertisedItemHash !=
                    itemHash)
            {
                return false;
            }

            return
                IsRevisionNewer(
                    remoteState.AdvertisedRevision,
                    packetRevision);
        }

        private static bool RememberAdvertisedState(
            RemoteRightState remoteState,
            int itemHash,
            uint revision,
            bool bound)
        {
            if (remoteState == null ||
                itemHash == 0 ||
                revision == 0)
            {
                return false;
            }

            if (remoteState.HasAdvertisedState)
            {
                if (revision ==
                    remoteState.AdvertisedRevision)
                {
                    return
                        remoteState.AdvertisedItemHash ==
                        itemHash;
                }

                // Revisions belong to the owner's right-hand publication stream,
                // so they remain ordered even when the equipped item changes.
                if (!IsRevisionNewer(
                        revision,
                        remoteState.AdvertisedRevision))
                {
                    return false;
                }
            }

            remoteState.HasAdvertisedState =
                true;

            remoteState.AdvertisedItemHash =
                itemHash;

            remoteState.AdvertisedRevision =
                revision;

            remoteState.AdvertisedBound =
                bound;

            return true;
        }

        private static bool IsAppliedStateCurrent(
            RemoteRightState remoteState,
            int rightInstanceId)
        {
            return
                remoteState.AppliedInstanceId ==
                    rightInstanceId &&
                remoteState.AppliedItemHash ==
                    remoteState.ReceivedItemHash &&
                remoteState.AppliedRevision ==
                    remoteState.ReceivedRevision &&
                remoteState.AppliedBound ==
                    remoteState.ReceivedBound;
        }

        private static void RememberAppliedState(
            RemoteRightState remoteState,
            int rightInstanceId)
        {
            remoteState.AppliedInstance =
                remoteState.RightInstance;

            remoteState.AppliedInstanceId =
                rightInstanceId;

            remoteState.AppliedItemHash =
                remoteState.ReceivedItemHash;

            remoteState.AppliedRevision =
                remoteState.ReceivedRevision;

            remoteState.AppliedBound =
                remoteState.ReceivedBound;
        }

        private static void RemovePreviouslyAppliedRigIfNeeded(
            RemoteRightState remoteState,
            GameObject newRightInstance,
            int newRightInstanceId,
            int newItemHash)
        {
            if (remoteState == null ||
                remoteState.AppliedInstance == null)
            {
                return;
            }

            bool sameInstance =
                remoteState.AppliedInstanceId ==
                    newRightInstanceId;

            bool sameItem =
                newItemHash == 0 ||
                remoteState.AppliedItemHash ==
                    newItemHash;

            if (sameInstance &&
                sameItem)
            {
                return;
            }

            NadaWeaponRigRemoval
                .RemoveFromEquippedRoot(
                    remoteState.AppliedInstance);
        }

        private static void ClearReceivedState(
            RemoteRightState remoteState)
        {
            remoteState.HasReceivedState =
                false;

            remoteState.ReceivedItemHash =
                0;

            remoteState.ReceivedRevision =
                0;

            remoteState.ReceivedBound =
                false;

            remoteState.ReceivedState =
                default;
        }

        private static void ClearAppliedState(
            RemoteRightState remoteState)
        {
            remoteState.AppliedInstance =
                null;

            remoteState.AppliedInstanceId =
                0;

            remoteState.AppliedItemHash =
                0;

            remoteState.AppliedRevision =
                0;

            remoteState.AppliedBound =
                false;
        }

        private static bool IsRevisionNewer(
            uint incoming,
            uint current)
        {
            if (incoming == current)
                return false;

            return
                unchecked(
                    (int)(
                        incoming -
                        current)) > 0;
        }
    }
}