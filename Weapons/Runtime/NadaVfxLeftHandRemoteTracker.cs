
using System;
using System.Collections.Generic;
using NADA.VFX.Weapon.Core.Network;
using NADA.VFX.Weapon.Editor;
using NADA.VFX.Weapon.Runtime.Structure;
using NADA.VFX.Weapon.Weapons.Targets;
using UnityEngine;

namespace NADA.VFX.Weapon.Weapons.Runtime
{
    // Left-hand native state is keyed by network peer and bound to the actual
    // left equipped wrapper. Never resolve another player's ItemData.
    internal static class NadaVfxLeftHandRemoteTracker
    {
        private const int FastRetryLimit = 5;
        private const float SlowProbeSeconds = 120f;
        private const float VisualRecheckSeconds = 1f;
        private const float ApplyRetrySeconds = 1f;

        private static readonly NadaWeaponRigController Controller = new();
        private static readonly Dictionary<long, RemoteState> States = new();
        private static readonly HashSet<long> HiddenPeers = new();
        private static bool _initialized;

        private sealed class RemoteState
        {
            internal long Peer;
            internal global::Player Player;
            internal int VisId;
            internal GameObject Wrapper;
            internal int WrapperId;
            internal int Hash;

            internal bool HasState;
            internal WeaponVfxNetworkStatePacket Packet;
            internal bool HasAdvertisement;
            internal int AdvertisedHash;
            internal uint AdvertisedRevision;

            internal int RequestAttempts;
            internal float NextRequest;
            internal int FailedWrapperId;
            internal uint FailedRevision;
            internal float NextApplyRetry;

            internal bool Applied;
            internal bool AppliedBound;
            internal int AppliedWrapperId;
            internal int AppliedHash;
            internal uint AppliedRevision;
            internal Transform AppliedVisual;
            internal Transform AppliedRig;
            internal float NextVisualRecheck;
        }

        private static void EnsureInitialized()
        {
            if (_initialized)
                return;
            _initialized = true;
            NadaVfxLeftHandNetworkTransport.StateReceived += Receive;
            NadaVfxLeftHandNetworkTransport.StateChanged += Changed;
            NadaVfxLeftHandNetworkTransport.SessionReset += Reset;
            SyncHiddenPeers();
        }

        private static bool IsNewer(uint incoming, uint current)
        {
            return incoming != current &&
                   unchecked((int)(incoming - current)) > 0;
        }

        private static bool CanShow(RemoteState state)
        {
            return NadaVfxEditorConfig.MultiplayerVisibility?.Value != false &&
                   !HiddenPeers.Contains(state.Peer);
        }

        // The right-hand editor already owns per-player visibility choices.
        // Read those choices on explicit UI refresh, never during every frame.
        private static void SyncHiddenPeers()
        {
            HiddenPeers.Clear();
            foreach (var choice in NadaVfxRemoteStateTracker.GetVisiblePlayerChoices())
                if (choice.Hidden)
                    HiddenPeers.Add(choice.PeerId);
        }

        internal static void RefreshPresentationVisibility()
        {
            SyncHiddenPeers();
            foreach (RemoteState state in States.Values)
            {
                if (!CanShow(state))
                {
                    if (OwnsRig(state))
                        state.AppliedRig.gameObject.SetActive(false);
                    continue;
                }

                if (Current(state) && state.AppliedBound && OwnsRig(state))
                {
                    state.AppliedRig.gameObject.SetActive(true);
                    continue;
                }
                Apply(state);
            }
        }

        internal static void Observe(long peer, global::VisEquipment vis,
            GameObject leftWrapper, int leftHash)
        {
            EnsureInitialized();
            if (peer == 0 || vis == null)
                return;

            if (!States.TryGetValue(peer, out RemoteState state))
            {
                state = new RemoteState { Peer = peer };
                States.Add(peer, state);
            }

            int visId = vis.GetInstanceID();
            int wrapperId = leftWrapper != null ? leftWrapper.GetInstanceID() : 0;
            bool playerReplaced = state.VisId != 0 && state.VisId != visId;
            bool wrapperChanged = state.WrapperId != wrapperId;
            bool itemChanged = leftHash != 0 && state.Hash != 0 &&
                               leftHash != state.Hash;

            if (playerReplaced || wrapperChanged || itemChanged)
            {
                Retire(state);
                ClearApplied(state);
                // A new instance can hold an entirely different item with
                // the same prefab hash. Do not reuse the previous state.
                ClearReceived(state);
                state.RequestAttempts = 0;
                state.NextRequest = 0f;
            }

            if (state.VisId != visId)
                state.Player = vis.GetComponentInParent<global::Player>();
            state.VisId = visId;
            state.Wrapper = leftWrapper;
            state.WrapperId = wrapperId;

            // Zero can be a transient vanilla identity. A missing wrapper
            // means there is no target to which any old state may remain bound.
            if (leftHash == 0)
            {
                if (leftWrapper == null)
                {
                    state.Hash = 0;
                    ClearReceived(state);
                }
                return;
            }

            if (state.Hash != leftHash)
            {
                state.Hash = leftHash;
                ClearReceived(state);
                state.RequestAttempts = 0;
                state.NextRequest = 0f;
            }

            if (leftWrapper == null)
                return;

            if (state.HasState && state.Packet.ItemHash == leftHash &&
                !WaitingForNewer(state))
            {
                Apply(state);
                return;
            }

            RequestIfNeeded(state, false);
        }

        private static void Receive(long sender,
            WeaponVfxNetworkStatePacket packet)
        {
            if (sender == 0 || packet == null || packet.ItemHash == 0 ||
                packet.Revision == 0 || (packet.Bound && packet.State == null) ||
                !States.TryGetValue(sender, out RemoteState state))
                return;

            if (state.Hash == 0 || packet.ItemHash != state.Hash)
            {
                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [LeftNativeStateStale] " +
                    $"peer={sender} received={packet.ItemHash} visible={state.Hash}");
                return;
            }

            if (WaitingForNewerThan(state, packet.ItemHash, packet.Revision))
                return;

            if (state.HasState && state.Packet.ItemHash == packet.ItemHash &&
                packet.Revision != state.Packet.Revision &&
                !IsNewer(packet.Revision, state.Packet.Revision))
                return;

            state.Packet = packet;
            state.HasState = true;
            state.RequestAttempts = 0;
            state.NextRequest = 0f;
            state.HasAdvertisement = true;
            state.AdvertisedHash = packet.ItemHash;
            state.AdvertisedRevision = packet.Revision;

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [LeftNativeStateAccepted] " +
                $"peer={sender} hash={packet.ItemHash} " +
                $"revision={packet.Revision} bound={packet.Bound} " +
                $"blocks={packet.State?.Effects?.Count ?? 0}");
            Apply(state);
        }

        private static void Changed(long sender, int hash, uint revision,
            bool bound)
        {
            if (sender == 0 || hash == 0 || revision == 0 ||
                !States.TryGetValue(sender, out RemoteState state))
                return;

            if (state.HasAdvertisement && state.AdvertisedHash == hash &&
                !IsNewer(revision, state.AdvertisedRevision))
                return;

            state.HasAdvertisement = true;
            state.AdvertisedHash = hash;
            state.AdvertisedRevision = revision;
            if (state.Hash != hash)
                return;
            if (state.HasState && state.Packet.ItemHash == hash &&
                !IsNewer(revision, state.Packet.Revision))
                return;

            // A newer state is now authoritative. Hide the old visuals
            // until the packet arrives, but keep their runtime root so an
            // in-place revision can reconcile instead of rebuilding it.
            if (OwnsRig(state))
                state.AppliedRig.gameObject.SetActive(false);
            state.NextRequest = 0f;
            if (state.Wrapper != null)
                RequestIfNeeded(state, true);
        }

        private static bool WaitingForNewer(RemoteState state)
        {
            return state.HasState && WaitingForNewerThan(state,
                state.Packet.ItemHash, state.Packet.Revision);
        }

        private static bool WaitingForNewerThan(RemoteState state,
            int hash, uint revision)
        {
            return state.HasAdvertisement &&
                   state.AdvertisedHash == hash &&
                   IsNewer(state.AdvertisedRevision, revision);
        }

        private static void RequestIfNeeded(RemoteState state, bool force)
        {
            if (state.Peer == 0 || state.Hash == 0 || state.Wrapper == null)
                return;

            float now = Time.unscaledTime;
            if (!force && now < state.NextRequest)
                return;

            uint knownRevision = state.HasState &&
                state.Packet.ItemHash == state.Hash
                    ? state.Packet.Revision : 0u;

            if (!NadaVfxLeftHandNetworkTransport.Request(
                    state.Peer, state.Hash, knownRevision))
                return;

            state.RequestAttempts = Math.Min(
                state.RequestAttempts + 1, FastRetryLimit + 1);
            float delay;
            switch (state.RequestAttempts)
            {
                case 1: delay = 1f; break;
                case 2: delay = 2f; break;
                case 3: delay = 5f; break;
                case 4: delay = 10f; break;
                case 5: delay = 30f; break;
                default: delay = SlowProbeSeconds; break;
            }
            state.NextRequest = now + delay;
            if (state.RequestAttempts == FastRetryLimit + 1)
                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [LeftNativePeerBackoff] " +
                    $"peer={state.Peer} retrySeconds={SlowProbeSeconds}");
        }

        private static bool OwnsRig(RemoteState state)
        {
            return state.AppliedRig != null &&
                   state.AppliedVisual != null &&
                   state.Wrapper != null &&
                   state.AppliedVisual.parent == state.Wrapper.transform &&
                   state.AppliedRig.parent == state.AppliedVisual;
        }

        private static bool Current(RemoteState state)
        {
            if (!state.Applied || !state.HasState || state.Wrapper == null ||
                state.AppliedWrapperId != state.WrapperId ||
                state.AppliedHash != state.Hash ||
                state.AppliedRevision != state.Packet.Revision ||
                state.AppliedBound != state.Packet.Bound || WaitingForNewer(state))
                return false;

            if (!state.AppliedBound)
                return true;

            if (!OwnsRig(state))
                return false;

            if (Time.unscaledTime >= state.NextVisualRecheck)
            {
                state.NextVisualRecheck = Time.unscaledTime +
                                          VisualRecheckSeconds;
                Transform current = NadaWeaponTargets
                    .FindEquippedWeaponVisualRoot(state.Wrapper.transform);
                if (current != state.AppliedVisual)
                    return false;
            }
            return true;
        }

        private static void Apply(RemoteState state)
        {
            if (state.Wrapper == null || state.Hash == 0 ||
                !state.HasState || state.Packet.ItemHash != state.Hash ||
                WaitingForNewer(state))
                return;

            if (!CanShow(state))
            {
                if (OwnsRig(state))
                    state.AppliedRig.gameObject.SetActive(false);
                return;
            }

            if (Current(state))
            {
                if (state.AppliedBound && OwnsRig(state))
                    state.AppliedRig.gameObject.SetActive(true);
                return;
            }

            var packet = state.Packet;
            if (!packet.Bound)
            {
                if (!Retire(state))
                    NadaWeaponRigRemoval.RemoveFromEquippedRoot(state.Wrapper);
                RememberApplied(state, null, null);
                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [LeftNativeRigRemoved] " +
                    $"peer={state.Peer} hash={state.Hash} " +
                    $"revision={packet.Revision}");
                return;
            }

            if (packet.State == null)
                return;

            if (state.FailedWrapperId == state.WrapperId &&
                state.FailedRevision == packet.Revision &&
                Time.unscaledTime < state.NextApplyRetry)
                return;

            // A same-wrapper update reuses the existing runtime root.
            // Retire only when the visual identity actually changed.
            Transform targetVisual = NadaWeaponTargets
                .FindEquippedWeaponVisualRoot(state.Wrapper.transform);
            bool canReuse = OwnsRig(state) &&
                            targetVisual == state.AppliedVisual;
            if (canReuse)
                state.AppliedRig.gameObject.SetActive(true);
            else
            {
                Retire(state);
                ClearApplied(state);
            }

            bool applied = Controller.TryApplyResolvedBlockState(
                state.Wrapper, state.Hash, packet.State);

            Transform visual = null;
            Transform rig = null;
            if (applied)
            {
                visual = NadaWeaponTargets.FindEquippedWeaponVisualRoot(
                    state.Wrapper.transform);
                if (visual != null)
                    rig = NadaRigPaths.FindDirectChild(
                        visual, Plugin.LocalWeaponRootName);
                applied = rig != null && rig.gameObject.activeSelf &&
                          visual.parent == state.Wrapper.transform;
            }

            if (!applied)
            {
                // A partially assembled rig must not remain visible if
                // reconciliation or the target lookup failed.
                if (!Retire(state))
                    NadaWeaponRigRemoval.RemoveFromEquippedRoot(state.Wrapper);
                state.Applied = false;
                state.AppliedRig = null;
                state.AppliedVisual = null;
                state.FailedWrapperId = state.WrapperId;
                state.FailedRevision = packet.Revision;
                state.NextApplyRetry = Time.unscaledTime + ApplyRetrySeconds;
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [LeftNativeRigApplyFailed] " +
                    $"peer={state.Peer} hash={state.Hash} " +
                    $"revision={packet.Revision}");
                return;
            }

            state.FailedWrapperId = 0;
            state.FailedRevision = 0;
            RememberApplied(state, visual, rig);
            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [LeftNativeRigApplied] " +
                $"peer={state.Peer} hash={state.Hash} " +
                $"revision={packet.Revision} " +
                $"blocks={packet.State.Effects.Count} " +
                $"wrapper={state.WrapperId} rig={rig.GetInstanceID()}");
        }

        private static void RememberApplied(RemoteState state,
            Transform visual, Transform rig)
        {
            state.Applied = true;
            state.AppliedBound = state.Packet.Bound;
            state.AppliedWrapperId = state.WrapperId;
            state.AppliedHash = state.Packet.ItemHash;
            state.AppliedRevision = state.Packet.Revision;
            state.AppliedVisual = visual;
            state.AppliedRig = rig;
            state.NextVisualRecheck = Time.unscaledTime +
                                      VisualRecheckSeconds;
        }

        private static bool Retire(RemoteState state)
        {
            if (state.AppliedRig == null)
                return false;
            // Destroy is deferred by Unity, so immediately hide the old
            // hierarchy before a later observer can rediscover it.
            state.AppliedRig.gameObject.SetActive(false);
            return NadaWeaponRigRemoval.RemoveTrackedRig(state.AppliedRig);
        }

        private static void ClearApplied(RemoteState state)
        {
            state.Applied = false;
            state.AppliedBound = false;
            state.AppliedWrapperId = 0;
            state.AppliedHash = 0;
            state.AppliedRevision = 0;
            state.AppliedVisual = null;
            state.AppliedRig = null;
            state.FailedWrapperId = 0;
            state.FailedRevision = 0;
            state.NextApplyRetry = 0f;
        }

        private static void ClearReceived(RemoteState state)
        {
            state.HasState = false;
            state.Packet = null;
            state.HasAdvertisement = false;
            state.AdvertisedHash = 0;
            state.AdvertisedRevision = 0;
        }

        internal static void ReleasePlayerInstance(global::Player player)
        {
            if (ReferenceEquals(player, null))
                return;
            var remove = new List<long>();
            foreach (var pair in States)
                if (ReferenceEquals(pair.Value.Player, player))
                    remove.Add(pair.Key);

            foreach (long peer in remove)
            {
                RemoteState state = States[peer];
                Retire(state);
                States.Remove(peer);
                HiddenPeers.Remove(peer);
            }
        }

        private static void Reset()
        {
            foreach (RemoteState state in States.Values)
                Retire(state);
            States.Clear();
            HiddenPeers.Clear();
        }
    }
}
