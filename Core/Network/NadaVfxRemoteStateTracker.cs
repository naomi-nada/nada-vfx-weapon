using System;
using System.Collections.Generic;
using NADA.VFX.Weapon.Core.Debug;
using NADA.VFX.Weapon.Core.Network;
using NADA.VFX.Weapon.Core.State;
using NADA.VFX.Weapon.Runtime.Structure;
using NADA.VFX.Weapon.Weapons.Targets;
using UnityEngine;

namespace NADA.VFX.Weapon.Weapons.Runtime
{
    internal static class NadaVfxRemoteStateTracker
    {
        private const float FailedApplyRetrySeconds = 1f;

        private const float RemoteVisualRecheckSeconds = 1f;

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

            internal GameObject AppliedInstance;

            internal int AppliedInstanceId;

            internal int AppliedItemHash;

            internal uint AppliedRevision;

            internal bool AppliedBound;

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

            internal float NextApplyRetryTime;

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
            if (remoteState.FailedApplyInstanceId ==
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
            bool matchesAppliedState =
                remoteState.AppliedInstanceId ==
                    rightInstanceId &&
                remoteState.AppliedItemHash ==
                    remoteState.ReceivedItemHash &&
                remoteState.AppliedRevision ==
                    remoteState.ReceivedRevision &&
                remoteState.AppliedBound ==
                    remoteState.ReceivedBound;

            if (!matchesAppliedState)
                return false;

            // An unbound state has no rig to validate.
            if (!remoteState.ReceivedBound)
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