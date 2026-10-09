using System;
using System.Collections.Generic;
using NADA.VFX.Weapon.Core.Debug;
using NADA.VFX.Weapon.Editor;
using NADA.VFX.Weapon.Core.Network;
using NADA.VFX.Weapon.Core.State;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Runtime.Structure;
using NADA.VFX.Weapon.Weapons.Targets;
using UnityEngine;

namespace NADA.VFX.Weapon.Weapons.Runtime
{
    internal static class NadaVfxRemoteStateTracker
    {
        private const float FailedApplyRetrySeconds = 1f;

        private const float RemoteVisualRecheckSeconds = 1f;

        // A peer that hasn't answered any requests shouldn't restart the
        // fast retry sequence every time it equips another weapon.
        //
        // We still probe occasionally: silence doesn't prove the other
        // player lacks NADA, and a compatible peer may become responsive.
        private const int InitialUnansweredRequestLimit = 5;

        private const float UnansweredPeerProbeSeconds = 120f;

        private static readonly NadaWeaponRigController
            WeaponRigController =
                new();

        private static readonly Dictionary<long, RemoteRightState>
            RemoteRightStates =
                new();

        private static bool _initialized;

        // Local presentation preference only. Never clear decoded network state.
        // Peer IDs are session identifiers here, not long-lived accounts.
        private static readonly HashSet<long> HiddenPeers = new();

        internal sealed class RemotePlayerChoice
        {
            internal long PeerId;
            internal string DisplayName;
            internal bool Hidden;
        }

        internal static List<RemotePlayerChoice> GetVisiblePlayerChoices()
        {
            var result = new List<RemotePlayerChoice>();
            foreach (var entry in RemoteRightStates)
            {
                var player = entry.Value.ObservedPlayer;
                if (player == null)
                    continue;

                string name = player.GetPlayerName();
                result.Add(new RemotePlayerChoice
                {
                    PeerId = entry.Key,
                    DisplayName = string.IsNullOrEmpty(name) ? "Player" : name,
                    Hidden = HiddenPeers.Contains(entry.Key)
                });
            }
            result.Sort((a, b) =>
                string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase));
            return result;
        }

        internal static int HiddenPlayerCount => HiddenPeers.Count;

        internal static void SetPlayerHidden(long peerId, bool hidden)
        {
            if (peerId == 0L || !RemoteRightStates.ContainsKey(peerId))
                return;
            if (hidden ? HiddenPeers.Add(peerId) : HiddenPeers.Remove(peerId))
                RefreshPresentationVisibility();
        }

        internal static void ShowAllPlayers()
        {
            if (HiddenPeers.Count == 0)
                return;
            HiddenPeers.Clear();
            RefreshPresentationVisibility();
        }

        private static bool MayShowRig(RemoteRightState state) =>
            NadaVfxEditorConfig.MultiplayerVisibility?.Value != false &&
            !HiddenPeers.Contains(state.PeerId);

        private static void HideTrackedRig(RemoteRightState state)
        {
            // A cached root can become stale when Valheim replaces visuals.
            // Never change the visibility of a hierarchy we no longer own.
            if (state.AppliedRigRoot != null &&
                state.AppliedVisualRoot != null &&
                state.RightInstance != null &&
                state.AppliedRigRoot.parent == state.AppliedVisualRoot &&
                state.AppliedVisualRoot.parent == state.RightInstance.transform)
                state.AppliedRigRoot.gameObject.SetActive(false);
        }

        internal static void RefreshPresentationVisibility()
        {
            foreach (RemoteRightState state in RemoteRightStates.Values)
            {
                if (!MayShowRig(state))
                {
                    HideTrackedRig(state);
                    continue;
                }

                if (state.AppliedBound &&
                    state.AppliedInstance == state.RightInstance &&
                    state.AppliedItemHash != 0 &&
                    state.AppliedItemHash == state.ObservedItemHash &&
                    !HasNewerAdvertisedState(state, state.ObservedItemHash) &&
                    state.AppliedRigRoot != null &&
                    state.AppliedVisualRoot != null &&
                    state.RightInstance != null &&
                    state.AppliedRigRoot.parent == state.AppliedVisualRoot &&
                    state.AppliedVisualRoot.parent == state.RightInstance.transform)
                    state.AppliedRigRoot.gameObject.SetActive(true);

                // Reapply from cached decoded state if no rig exists yet.
                ApplyReceivedState(state);
            }
        }


        private sealed class RemoteRightState
        {
            internal long PeerId;

            internal int VisEquipmentId;

            // Keep the exact player instance that owns this observation.
            // An old instance must not clear a replacement for the same peer.
            internal global::Player ObservedPlayer;

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

            // Native and V2 packets share an owner's publication revision,
            // but neither is allowed to overwrite the other format's data.
            internal bool HasReceivedNative;
            internal int NativeItemHash;
            internal uint NativeRevision;
            internal bool NativeBound;
            internal WeaponVfxState NativeState;

            internal GameObject AppliedInstance;

            internal int AppliedInstanceId;

            internal int AppliedItemHash;

            internal uint AppliedRevision;

            internal bool AppliedBound;
            internal bool AppliedNative;

            // These are the actual roots that received the remote state.
            // Their existence alone isn't enough: the visual must still
            // belong to the equipped wrapper.
            internal Transform AppliedVisualRoot;

            internal Transform AppliedRigRoot;

            internal float NextVisualRecheckTime;

            // A failed application should not rebuild on every equipment
            // callback. These fields identify the exact failed attempt.
            internal int FailedApplyInstanceId;

            internal int FailedApplyItemHash;

            internal uint FailedApplyRevision;
            internal bool FailedApplyNative;

            internal float NextApplyRetryTime;

            internal bool RequestPending;

            internal int RequestedItemHash;

            // Count unanswered requests across weapon changes. A response
            // or valid change notification resets this peer's retry history.
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

            // Resolve the Player only when the observed equipment owner
            // changes, not on every vanilla equipment callback.
            if (remoteState.VisEquipmentId !=
                visEquipmentId)
            {
                remoteState.ObservedPlayer =
                    visEquipment
                        .GetComponentInParent<global::Player>();
            }

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

                ClearFailedApply(
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

                // A different weapon deserves an immediate attempt while
                // we're still in the normal retry window.
                //
                // Once a peer has repeatedly gone unanswered, preserve its
                // backoff across weapon swaps. Otherwise an unmodded player
                // can restart our fast requests just by changing equipment.
                if (remoteState.RequestAttempts <
                    InitialUnansweredRequestLimit)
                {
                    remoteState.NextRequestTime =
                        0f;
                }

                if (remoteState.HasReceivedState &&
                    remoteState.ReceivedItemHash != itemHash)
                    ClearReceivedState(remoteState);

                if (remoteState.HasReceivedNative &&
                    remoteState.NativeItemHash != itemHash)
                    ClearReceivedNativeState(remoteState);
            }

            if (!MayShowRig(remoteState))
            {
                HideTrackedRig(remoteState);
                return;
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

            if ((remoteState.HasReceivedState &&
                 remoteState.ReceivedItemHash == itemHash) ||
                (remoteState.HasReceivedNative &&
                 remoteState.NativeItemHash == itemHash))
            {
                ApplyReceivedState(remoteState);
                return;
            }

            RequestStateIfNeeded(
                remoteState,
                force: false);
        }

        // Player.OnDestroy is about this GameObject's lifetime, not proof
        // that its network peer disconnected.
        internal static void ReleasePlayerInstance(
            global::Player player)
        {
            if (ReferenceEquals(
                    player,
                    null))
            {
                return;
            }

            // Gather keys first so we never modify the dictionary while
            // enumerating it. A peer's replacement Player might already
            // exist, so match the actual object reference.
            List<long> peersToRelease =
                null;

            foreach (KeyValuePair<long, RemoteRightState> entry
                     in RemoteRightStates)
            {
                if (!ReferenceEquals(
                        entry.Value.ObservedPlayer,
                        player))
                {
                    continue;
                }

                peersToRelease ??=
                    new List<long>();

                peersToRelease.Add(
                    entry.Key);
            }

            if (peersToRelease == null)
                return;

            foreach (long peerId in peersToRelease)
            {
                if (!RemoteRightStates.TryGetValue(
                        peerId,
                        out RemoteRightState remoteState) ||
                    !ReferenceEquals(
                        remoteState.ObservedPlayer,
                        player))
                {
                    continue;
                }

                int visEquipmentId =
                    remoteState.VisEquipmentId;

                // Remove the record even if its old rig was already
                // destroyed along with the player's hierarchy.
                RemoteRightStates.Remove(
                    peerId);
                HiddenPeers.Remove(peerId);

                bool removedRig =
                    RemoveTrackedAppliedRig(
                        remoteState);

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [RemotePlayerReleased] " +
                    $"peer={peerId} " +
                    $"vis={visEquipmentId} " +
                    $"removedRig={removedRig}");
            }
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

            NadaVfxNetworkTransport.NativeStateReceived +=
                OnNativeStateReceived;

            NadaVfxNetworkTransport.NativeStateChangedReceived +=
                OnNativeStateChangedReceived;

            NadaVfxNetworkTransport.SessionReset +=
                OnSessionReset;
        }

        private static void OnSessionReset()
        {
            RemoteRightStates.Clear();
            HiddenPeers.Clear();
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

            // A decoded NADA packet proves this peer can answer us, even
            // if Valheim switched its visible weapon before it arrived.
            // This only resets retry history; the item/revision guards
            // below still decide whether the packet can be applied.
            remoteState.RequestAttempts =
                0;

            if (remoteState.RequestedItemHash !=
                remoteState.ObservedItemHash)
            {
                remoteState.NextRequestTime =
                    0f;
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

        private static void OnNativeStateReceived(
            long sender,
            WeaponVfxNetworkStatePacket packet)
        {
            if (sender == 0L || packet == null || packet.ItemHash == 0 ||
                packet.Revision == 0 || (packet.Bound && packet.State == null))
                return;

            if (!RemoteRightStates.TryGetValue(sender, out RemoteRightState remote))
                return;

            remote.RequestAttempts = 0;

            if (remote.ObservedItemHash == 0 ||
                packet.ItemHash != remote.ObservedItemHash)
            {
                NadaLogControl.Info(
                    $"remote-native-stale:{sender}:{packet.ItemHash}:{packet.Revision}",
                    $"{Plugin.ModName}: [RemoteNativeState STALE] " +
                    $"peer={sender} receivedHash={packet.ItemHash} " +
                    $"visibleHash={remote.ObservedItemHash} revision={packet.Revision}");
                return;
            }

            if (HasAdvertisedStateNewerThanPacket(
                    remote, packet.ItemHash, packet.Revision))
                return;

            if (remote.HasReceivedNative && remote.NativeItemHash == packet.ItemHash &&
                packet.Revision != remote.NativeRevision &&
                !IsRevisionNewer(packet.Revision, remote.NativeRevision))
                return;

            // A newer V2 state cannot be replaced by an older native response.
            if (remote.HasReceivedState && remote.ReceivedItemHash == packet.ItemHash &&
                IsRevisionNewer(remote.ReceivedRevision, packet.Revision))
                return;

            RememberAdvertisedState(remote, packet.ItemHash, packet.Revision, packet.Bound);
            remote.HasReceivedNative = true;
            remote.NativeItemHash = packet.ItemHash;
            remote.NativeRevision = packet.Revision;
            remote.NativeBound = packet.Bound;
            remote.NativeState = packet.State;
            remote.RequestPending = false;
            remote.RequestedItemHash = 0;
            remote.NextRequestTime = 0f;

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [RemoteNativeStateAccepted] " +
                $"peer={sender} hash={packet.ItemHash} revision={packet.Revision} " +
                $"bound={packet.Bound} blocks={packet.State?.Effects?.Count ?? 0}");

            ApplyReceivedState(remote);
        }

        private static void OnNativeStateChangedReceived(
            long sender,
            int itemHash,
            uint revision,
            bool bound)
        {
            if (sender == 0L || itemHash == 0 || revision == 0 ||
                !RemoteRightStates.TryGetValue(sender, out RemoteRightState remote))
                return;

            if (!RememberAdvertisedState(remote, itemHash, revision, bound))
                return;

            remote.RequestAttempts = 0;
            if (remote.ObservedItemHash != itemHash)
                return;

            if (remote.HasReceivedNative && remote.NativeItemHash == itemHash &&
                remote.NativeRevision == revision)
                return;

            // The owner has a newer snapshot. Stop displaying the old one
            // while waiting for the packet, regardless of the old source.
            if (remote.AppliedBound && remote.AppliedItemHash == itemHash &&
                IsRevisionNewer(revision, remote.AppliedRevision))
            {
                if (RemoveTrackedAppliedRig(remote))
                    ClearAppliedState(remote);
            }

            remote.RequestPending = false;
            remote.RequestedItemHash = 0;
            remote.NextRequestTime = 0f;
            if (remote.RightInstance != null)
                RequestStateIfNeeded(remote, force: true);
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

            // A valid advertisement also confirms that this peer speaks
            // NADA's protocol. Restore its normal request budget.
            remoteState.RequestAttempts =
                0;

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

            // A newer unbind notification is enough to stop rendering the
            // previous revision. We still request the full packet below so
            // the received state can become authoritative.
            if (!bound &&
                remoteState.AppliedBound &&
                remoteState.AppliedItemHash == itemHash &&
                IsRevisionNewer(
                    revision,
                    remoteState.AppliedRevision))
            {
                bool removedRig =
                    RemoveTrackedAppliedRig(
                        remoteState);

                if (removedRig)
                {
                    ClearAppliedState(
                        remoteState);
                }

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [RemoteUnbindAdvertised] " +
                    $"peer={sender} " +
                    $"hash={itemHash} " +
                    $"revision={revision} " +
                    $"removedRig={removedRig}");
            }

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

            int previousAttempts =
                remoteState.RequestAttempts;

            // Saturate the counter once the slow-probe schedule starts.
            // This is a long-lived per-peer record; it doesn't need to
            // count unanswered requests forever.
            remoteState.RequestAttempts =
                Math.Min(
                    previousAttempts + 1,
                    InitialUnansweredRequestLimit + 1);

            remoteState.NextRequestTime =
                now +
                GetRetryDelay(
                    remoteState.RequestAttempts);

            if (previousAttempts ==
                InitialUnansweredRequestLimit)
            {
                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [RemotePeerRequestBackoff] " +
                    $"peer={remoteState.PeerId} " +
                    $"unanswered={remoteState.RequestAttempts} " +
                    $"probeSeconds={UnansweredPeerProbeSeconds}");
            }
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

            if (attempt == 5)
                return 30f;

            return UnansweredPeerProbeSeconds;
        }

        private static void ApplyReceivedState(
            RemoteRightState remoteState)
        {
            if (remoteState == null ||
                remoteState.RightInstance == null ||
                remoteState.ObservedItemHash == 0 ||
                (remoteState.ReceivedItemHash != remoteState.ObservedItemHash &&
                 remoteState.NativeItemHash != remoteState.ObservedItemHash))
            {
                return;
            }

            if (!MayShowRig(remoteState))
            {
                HideTrackedRig(remoteState);
                return;
            }

            if (ShouldUseNative(remoteState))
            {
                ApplyReceivedNativeState(remoteState);
                return;
            }

            if (!remoteState.HasReceivedState ||
                remoteState.ReceivedItemHash != remoteState.ObservedItemHash)
                return;

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
                ClearFailedApply(
                    remoteState);

                // The last rig may be on a visual that has since detached.
                // Prefer removing the rig we actually applied, not whichever
                // visual the equipped wrapper resolves to today.
                if (!RemoveTrackedAppliedRig(
                        remoteState))
                {
                    NadaWeaponRigRemoval
                        .RemoveFromEquippedRoot(
                            remoteState.RightInstance);
                }

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

            // A failed build may be observed hundreds of times per second.
            // Retry the same instance/revision at most once per second, but
            // let a different weapon or a newer revision try immediately.
            if (!remoteState.FailedApplyNative &&
                remoteState.FailedApplyInstanceId ==
                    rightInstanceId &&
                remoteState.FailedApplyItemHash ==
                    remoteState.ReceivedItemHash &&
                remoteState.FailedApplyRevision ==
                    remoteState.ReceivedRevision &&
                Time.unscaledTime <
                    remoteState.NextApplyRetryTime)
            {
                return;
            }

            // A rig on an old visual must go away before we try to attach
            // state to the current visual. This runs only when an application
            // is actually needed, not on every equipment callback.
            RetireObsoleteAppliedRigIfNeeded(
                remoteState);

            bool applied =
                WeaponRigController
                    .TryApplyResolvedState(
                        remoteState.RightInstance,
                        remoteState.ReceivedItemHash,
                        remoteState.ReceivedState);

            Transform appliedVisualRoot = null;

            Transform appliedRigRoot = null;

            if (applied)
            {
                // RunRemote() succeeded structurally. Before caching that
                // success, capture the actual rig it left on the weapon.
                // Otherwise a missing root could become a permanent success.
                appliedVisualRoot =
                    NadaWeaponTargets.FindEquippedWeaponVisualRoot(
                        remoteState.RightInstance.transform);

                if (appliedVisualRoot != null)
                {
                    appliedRigRoot =
                        NadaRigPaths.FindDirectChild(
                            appliedVisualRoot,
                            Plugin.LocalWeaponRootName);
                }

                if (appliedRigRoot == null)
                    applied = false;
            }

            if (!applied)
            {
                remoteState.FailedApplyNative = false;
                remoteState.FailedApplyInstanceId =
                    rightInstanceId;

                remoteState.FailedApplyItemHash =
                    remoteState.ReceivedItemHash;

                remoteState.FailedApplyRevision =
                    remoteState.ReceivedRevision;

                remoteState.NextApplyRetryTime =
                    Time.unscaledTime +
                    FailedApplyRetrySeconds;

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

            ClearFailedApply(
                remoteState);

            RememberAppliedState(
                remoteState,
                rightInstanceId,
                appliedVisualRoot,
                appliedRigRoot);

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [RemoteStateApplied] " +
                $"peer={remoteState.PeerId} " +
                $"hash={remoteState.ReceivedItemHash} " +
                $"revision={remoteState.ReceivedRevision} " +
                $"instance={rightInstanceId}");
        }

        // Native wins at an equal revision only when it is bound. This lets
        // legacy-bound weapons keep using V2 while their native packet says
        // unbound, and prevents the V2 compatibility unbind removing native VFX.
        private static bool ShouldUseNative(RemoteRightState remote)
        {
            if (!remote.HasReceivedNative ||
                remote.NativeItemHash != remote.ObservedItemHash)
                return false;

            if (!remote.HasReceivedState ||
                remote.ReceivedItemHash != remote.ObservedItemHash)
                return true;

            if (IsRevisionNewer(remote.NativeRevision, remote.ReceivedRevision))
                return true;
            if (IsRevisionNewer(remote.ReceivedRevision, remote.NativeRevision))
                return false;
            return remote.NativeBound;
        }

        private static void ApplyReceivedNativeState(RemoteRightState remote)
        {
            if (!MayShowRig(remote))
            {
                HideTrackedRig(remote);
                return;
            }
            if (remote.RightInstance == null || remote.ObservedItemHash == 0 ||
                !remote.HasReceivedNative ||
                remote.NativeItemHash != remote.ObservedItemHash ||
                HasNewerAdvertisedState(remote, remote.NativeItemHash))
                return;

            int visualId = remote.RightInstance.GetInstanceID();
            if (IsAppliedStateCurrent(remote, visualId))
                return;

            if (!remote.NativeBound)
            {
                ClearFailedApply(remote);
                if (!RemoveTrackedAppliedRig(remote))
                    NadaWeaponRigRemoval.RemoveFromEquippedRoot(remote.RightInstance);
                RememberAppliedState(remote, visualId);
                remote.AppliedNative = true;
                remote.AppliedRevision = remote.NativeRevision;
                remote.AppliedItemHash = remote.NativeItemHash;
                remote.AppliedBound = false;
                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [RemoteNativeStateRemoved] " +
                    $"peer={remote.PeerId} hash={remote.NativeItemHash} " +
                    $"revision={remote.NativeRevision}");
                return;
            }

            if (remote.NativeState == null)
                return;

            if (remote.FailedApplyNative &&
                remote.FailedApplyInstanceId == visualId &&
                remote.FailedApplyItemHash == remote.NativeItemHash &&
                remote.FailedApplyRevision == remote.NativeRevision &&
                Time.unscaledTime < remote.NextApplyRetryTime)
                return;

            RetireObsoleteAppliedRigIfNeeded(remote);

            bool applied = WeaponRigController.TryApplyResolvedBlockState(
                remote.RightInstance, remote.NativeItemHash, remote.NativeState);

            Transform visual = null;
            Transform rig = null;
            if (applied)
            {
                visual = NadaWeaponTargets.FindEquippedWeaponVisualRoot(
                    remote.RightInstance.transform);
                if (visual != null)
                    rig = NadaRigPaths.FindDirectChild(
                        visual, Plugin.LocalWeaponRootName);
                applied = rig != null && rig.gameObject.activeSelf;
            }

            if (!applied)
            {
                remote.FailedApplyNative = true;
                remote.FailedApplyInstanceId = visualId;
                remote.FailedApplyItemHash = remote.NativeItemHash;
                remote.FailedApplyRevision = remote.NativeRevision;
                remote.NextApplyRetryTime = Time.unscaledTime + FailedApplyRetrySeconds;
                NadaLogControl.Info(
                    $"remote-native-apply-fail:{remote.PeerId}:{remote.NativeRevision}:{visualId}",
                    $"{Plugin.ModName}: [RemoteNativeStateApply FAIL] " +
                    $"peer={remote.PeerId} hash={remote.NativeItemHash} " +
                    $"revision={remote.NativeRevision}");
                return;
            }

            ClearFailedApply(remote);
            RememberAppliedState(remote, visualId, visual, rig);
            remote.AppliedNative = true;
            remote.AppliedRevision = remote.NativeRevision;
            remote.AppliedItemHash = remote.NativeItemHash;
            remote.AppliedBound = true;
            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [RemoteNativeStateApplied] " +
                $"peer={remote.PeerId} hash={remote.NativeItemHash} " +
                $"revision={remote.NativeRevision} " +
                $"blocks={remote.NativeState.Effects.Count} instance={visualId}");
        }

        private static void RetireObsoleteAppliedRigIfNeeded(
            RemoteRightState remoteState)
        {
            if (remoteState == null ||
                !remoteState.AppliedBound ||
                remoteState.AppliedRigRoot == null ||
                remoteState.RightInstance == null)
            {
                return;
            }

            Transform previousVisual =
                remoteState.AppliedVisualRoot;

            Transform previousRig =
                remoteState.AppliedRigRoot;

            Transform currentVisual =
                NadaWeaponTargets.FindEquippedWeaponVisualRoot(
                    remoteState.RightInstance.transform);

            bool previousVisualStillOwned =
                previousVisual != null &&
                previousVisual.parent ==
                    remoteState.RightInstance.transform;

            bool previousRigStillOwned =
                previousVisual != null &&
                previousRig.parent ==
                    previousVisual;

            if (previousVisualStillOwned &&
                previousRigStillOwned &&
                currentVisual == previousVisual)
            {
                // Only the state changed. The rig is still on the actual
                // weapon visual, so the normal refresh path can reuse it.
                return;
            }

            int oldRigId =
                previousRig.GetInstanceID();

            if (!NadaWeaponRigRemoval.RemoveTrackedRig(
                    previousRig))
            {
                return;
            }

            NadaLogControl.Info(
                $"remote-rig-retired:{remoteState.PeerId}:{oldRigId}",
                $"{Plugin.ModName}: [RemoteRigRetired] " +
                $"peer={remoteState.PeerId} " +
                $"oldRigId={oldRigId} " +
                $"oldVisual='{(previousVisual != null ? previousVisual.name : "<destroyed>")}' " +
                $"currentVisual='{(currentVisual != null ? currentVisual.name : "<none>")}'");

            // This was a rendering failure, not a network-state change.
            // Keep ReceivedState so it can be applied to a valid visual.
            ClearAppliedState(
                remoteState);
        }

        private static bool RemoveTrackedAppliedRig(
            RemoteRightState remoteState)
        {
            if (remoteState == null ||
                !remoteState.AppliedBound ||
                remoteState.AppliedRigRoot == null)
            {
                return false;
            }

            return NadaWeaponRigRemoval.RemoveTrackedRig(
                remoteState.AppliedRigRoot);
        }

        private static bool HasNewerAdvertisedState(
            RemoteRightState remote,
            int itemHash)
        {
            if (remote == null || !remote.HasAdvertisedState ||
                remote.AdvertisedItemHash != itemHash)
                return false;

            bool hasRevision = false;
            uint newest = 0;
            if (remote.HasReceivedState && remote.ReceivedItemHash == itemHash)
            {
                newest = remote.ReceivedRevision;
                hasRevision = true;
            }
            if (remote.HasReceivedNative && remote.NativeItemHash == itemHash &&
                (!hasRevision || IsRevisionNewer(remote.NativeRevision, newest)))
            {
                newest = remote.NativeRevision;
                hasRevision = true;
            }

            return !hasRevision || IsRevisionNewer(remote.AdvertisedRevision, newest);
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
            bool usingNative = ShouldUseNative(remoteState);
            int expectedHash = usingNative
                ? remoteState.NativeItemHash : remoteState.ReceivedItemHash;
            uint expectedRevision = usingNative
                ? remoteState.NativeRevision : remoteState.ReceivedRevision;
            bool expectedBound = usingNative
                ? remoteState.NativeBound : remoteState.ReceivedBound;

            bool matchesAppliedState =
                remoteState.AppliedNative == usingNative &&
                remoteState.AppliedInstanceId ==
                    rightInstanceId &&
                remoteState.AppliedItemHash == expectedHash &&
                remoteState.AppliedRevision == expectedRevision &&
                remoteState.AppliedBound == expectedBound;

            if (!matchesAppliedState)
                return false;

            // An unbound state has no rig to validate.
            if (!expectedBound)
                return true;

            // Unity's null check also detects a destroyed GameObject even
            // when we're still holding its old C# Transform reference.
            if (remoteState.AppliedVisualRoot == null ||
                remoteState.AppliedRigRoot == null)
            {
                return false;
            }

            // P3.1 checked that the rig still existed. P3.2 also checks
            // whether the visual is still owned by this equipped wrapper.
            if (remoteState.AppliedVisualRoot.parent !=
                    remoteState.RightInstance.transform ||
                remoteState.AppliedRigRoot.parent !=
                    remoteState.AppliedVisualRoot)
            {
                return false;
            }

            float now =
                Time.unscaledTime;

            if (now >= remoteState.NextVisualRecheckTime)
            {
                // Two visuals can coexist briefly under a wrapper. Check the
                // resolver's current choice at most once per second instead
                // of searching renderer hierarchies on every callback.
                remoteState.NextVisualRecheckTime =
                    now + RemoteVisualRecheckSeconds;

                Transform currentVisual =
                    NadaWeaponTargets.FindEquippedWeaponVisualRoot(
                        remoteState.RightInstance.transform);

                if (currentVisual !=
                    remoteState.AppliedVisualRoot)
                {
                    return false;
                }
            }

            return true;
        }

        private static void RememberAppliedState(
            RemoteRightState remoteState,
            int rightInstanceId,
            Transform appliedVisualRoot = null,
            Transform appliedRigRoot = null)
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

            remoteState.AppliedNative = false;

            remoteState.AppliedVisualRoot =
                appliedVisualRoot;

            remoteState.AppliedRigRoot =
                appliedRigRoot;

            remoteState.NextVisualRecheckTime =
                Time.unscaledTime +
                RemoteVisualRecheckSeconds;
        }

        private static void RemovePreviouslyAppliedRigIfNeeded(
            RemoteRightState remoteState,
            GameObject newRightInstance,
            int newRightInstanceId,
            int newItemHash)
        {
            if (remoteState == null ||
                remoteState.AppliedInstanceId == 0 ||
                !remoteState.AppliedBound)
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

            // The former wrapper might already be destroyed, or its old
            // visual might have detached. Use our tracked rig first.
            if (RemoveTrackedAppliedRig(
                    remoteState))
            {
                return;
            }

            // Preserve the old fallback for applications that have no
            // surviving tracked rig reference.
            if (remoteState.AppliedInstance != null)
            {
                NadaWeaponRigRemoval
                    .RemoveFromEquippedRoot(
                        remoteState.AppliedInstance);
            }
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

        private static void ClearReceivedNativeState(RemoteRightState remote)
        {
            remote.HasReceivedNative = false;
            remote.NativeItemHash = 0;
            remote.NativeRevision = 0;
            remote.NativeBound = false;
            remote.NativeState = null;
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

            remoteState.AppliedNative = false;

            remoteState.AppliedVisualRoot =
                null;

            remoteState.AppliedRigRoot =
                null;

            remoteState.NextVisualRecheckTime =
                0f;
        }

        private static void ClearFailedApply(
            RemoteRightState remoteState)
        {
            remoteState.FailedApplyInstanceId =
                0;

            remoteState.FailedApplyItemHash =
                0;

            remoteState.FailedApplyRevision =
                0;
            remoteState.FailedApplyNative = false;

            remoteState.NextApplyRetryTime =
                0f;
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