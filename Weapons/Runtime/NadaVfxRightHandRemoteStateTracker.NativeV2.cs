using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NADA.VFX.Weapon.Core.Network;
using NADA.VFX.Weapon.Core.State;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Runtime.Structure;
using NADA.VFX.Weapon.Weapons.Targets;
using UnityEngine;

namespace NADA.VFX.Weapon.Weapons.Runtime
{
    // Native V2 is a separate source mode. The legacy tracker keeps its own
    // decoded state, but may not drive the same rig once V2 is authoritative.
    internal static partial class NadaVfxRightHandRemoteStateTracker
    {
        private const float GenerationCheckSeconds = 1f;
        private const float LegacyFallbackProbeSeconds = 120f;
        private const int NativeV2InitialProbeLimit = 3;

        private static readonly FieldInfo RemoteNViewField =
            AccessTools.Field(typeof(global::VisEquipment), "m_nview");

        // Targeted lookup for Player.Update. No scene scans or per-frame codec work.
        private static readonly Dictionary<int, long> PeerByPlayerId = new();

        private sealed partial class RemoteRightState
        {
            internal global::VisEquipment ObservedEquipment;
            internal uint ObservedGeneration;
            internal int ObservedVanillaHash;
            internal float NextGenerationCheck;
            internal bool NativeV2Authority;
            internal bool LegacyFallback;
            internal int NativeV2RequestAttempts;
            internal float NextNativeV2Request;
            internal bool HasNativeV2State;
            internal uint NativeV2Generation;
            internal uint NativeV2Revision;
            internal int NativeV2Hash;
            internal bool NativeV2Bound;
            internal WeaponVfxState NativeV2State;
            internal bool HasNativeV2Advertisement;
            internal uint NativeV2AdvertisedRevision;
            internal bool NativeV2AdvertisedBound;
            // Native V2 encodes legacy-bound items as native-unbound.
            // Resolve only the matching legacy V2 revision when needed.
            internal bool NativeV2LegacyResolved;
            internal bool NativeV2LegacyBound;
            internal VfxState NativeV2LegacyState;
            internal float NextLegacyResolutionRequest;
            internal int LegacyResolutionAttempts;
            internal bool AppliedNativeV2;
            internal uint AppliedGeneration;
        }

        private static void ResetNativeV2ForOwnerReplacement(RemoteRightState remote)
        {
            remote.ObservedGeneration = 0;
            remote.ObservedVanillaHash = 0;
            remote.NativeV2Authority = false;
            remote.LegacyFallback = false;
            remote.HasNativeV2State = false;
            remote.HasNativeV2Advertisement = false;
            remote.NativeV2LegacyResolved = false;
            remote.NativeV2LegacyBound = false;
            remote.NativeV2LegacyState = default;
            remote.NativeV2RequestAttempts = 0;
            remote.NextNativeV2Request = 0f;
            remote.RequestPending = false;
            ClearReceivedState(remote);
            ClearReceivedNativeState(remote);
            remote.HasAdvertisedState = false;
        }

        private static void RegisterObservedEquipment(
            RemoteRightState remote, global::VisEquipment equipment)
        {
            if (remote.ObservedPlayer != null)
                PeerByPlayerId[remote.ObservedPlayer.GetInstanceID()] = remote.PeerId;
            remote.ObservedEquipment = equipment;
        }

        private static void ForgetObservedPlayer(global::Player player)
        {
            if (player != null)
                PeerByPlayerId.Remove(player.GetInstanceID());
        }

        private static void ResetNativeV2Session()
        {
            PeerByPlayerId.Clear();
        }

        private static void CheckObservedGeneration(
            RemoteRightState remote, bool force)
        {
            if (remote == null || remote.ObservedEquipment == null)
                return;

            float now = Time.unscaledTime;
            if (!force && now < remote.NextGenerationCheck)
                return;
            remote.NextGenerationCheck = now + GenerationCheckSeconds;

            if (RemoteNViewField == null)
                return;

            global::ZNetView nview =
                RemoteNViewField.GetValue(remote.ObservedEquipment) as global::ZNetView;
            if (nview == null || !nview.IsValid())
                return;

            global::ZDO zdo = nview.GetZDO();
            if (zdo == null)
                return;

            int vanillaHash = zdo.GetInt(ZDOVars.s_rightItem);
            remote.ObservedVanillaHash = vanillaHash;
            uint generation = unchecked((uint)zdo.GetInt(
                NadaVfxRightHandStatePublisher.EquipmentGenerationKey, 0));

            // Both keys must refer to the same current visual identity.
            // A transient hash mismatch is not permission to adopt a token.
            if (vanillaHash != 0 && remote.ObservedItemHash != 0 &&
                vanillaHash != remote.ObservedItemHash)
                return;

            if (vanillaHash == 0 && remote.AppliedBound)
            {
                RemoveTrackedAppliedRig(remote);
                ClearAppliedState(remote);
            }

            if (generation == remote.ObservedGeneration)
                return;

            uint previous = remote.ObservedGeneration;
            remote.ObservedGeneration = generation;

            // A new equipped ItemData can reuse the exact same prefab and
            // wrapper. Retire its previous rig before considering new packets.
            if (remote.AppliedBound)
                RemoveTrackedAppliedRig(remote);
            ClearAppliedState(remote);
            ClearFailedApply(remote);
            ClearReceivedState(remote);
            ClearReceivedNativeState(remote);
            remote.HasAdvertisedState = false;
            remote.HasNativeV2State = false;
            remote.NativeV2State = null;
            remote.HasNativeV2Advertisement = false;
            remote.NativeV2LegacyResolved = false;
            remote.NativeV2LegacyBound = false;
            remote.NativeV2LegacyState = default;
            remote.LegacyResolutionAttempts = 0;
            remote.NextLegacyResolutionRequest = 0f;
            remote.NativeV2RequestAttempts = 0;
            remote.NextNativeV2Request = 0f;
            remote.LegacyFallback = false;
            remote.RequestPending = false;
            remote.RequestedItemHash = 0;
            remote.NextRequestTime = 0f;

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [RightNativeV2GenerationChanged] " +
                $"peer={remote.PeerId} hash={remote.ObservedItemHash} " +
                $"previous={previous} generation={generation}");
        }

        // No old-format response carries an equipment generation. During
        // negotiation it must fail closed, unless V2 genuinely times out.
        private static bool IgnoreLegacy(RemoteRightState remote)
        {
            return remote != null && remote.ObservedGeneration != 0 &&
                   (remote.NativeV2Authority || !remote.LegacyFallback);
        }

        private static bool HasNewerNativeV2Advertisement(RemoteRightState remote)
        {
            return remote.NativeV2Authority &&
                   remote.HasNativeV2Advertisement &&
                   (!remote.HasNativeV2State ||
                    IsRevisionNewer(remote.NativeV2AdvertisedRevision,
                                    remote.NativeV2Revision));
        }

        private static bool HasCurrentNativeV2State(RemoteRightState remote)
        {
            return remote.NativeV2Authority && remote.HasNativeV2State &&
                   remote.NativeV2Hash == remote.ObservedItemHash &&
                   remote.NativeV2Generation == remote.ObservedGeneration &&
                   !HasNewerNativeV2Advertisement(remote);
        }

        // Returns true when the Native V2 negotiation owns the request.
        // Returns false only when the old format may be used as a fallback.
        private static bool RequestNativeV2OrFallback(
            RemoteRightState remote, bool force)
        {
            if (remote.ObservedGeneration == 0)
                return false;
            if (remote.ObservedVanillaHash != remote.ObservedItemHash)
                return true; // Visible identity has not caught up yet.

            float now = Time.unscaledTime;
            if (HasCurrentNativeV2State(remote))
                return true;

            if (remote.LegacyFallback && !remote.NativeV2Authority)
            {
                // Keep a very occasional V2 probe for peers that upgrade
                // during a session, without blocking old-format visuals.
                if (now >= remote.NextNativeV2Request)
                {
                    NadaVfxRightHandNativeV2Transport.Request(
                        remote.PeerId, remote.ObservedItemHash,
                        remote.ObservedGeneration, 0);
                    remote.NextNativeV2Request = now + LegacyFallbackProbeSeconds;
                }
                return false;
            }

            if (!force && now < remote.NextNativeV2Request)
                return true;

            if (!remote.NativeV2Authority &&
                remote.NativeV2RequestAttempts >= NativeV2InitialProbeLimit)
            {
                remote.LegacyFallback = true;
                remote.NextNativeV2Request = now + LegacyFallbackProbeSeconds;
                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [RightNativeV2Fallback] " +
                    $"peer={remote.PeerId} generation={remote.ObservedGeneration}");
                return false;
            }

            uint knownRevision = remote.HasNativeV2State &&
                remote.NativeV2Generation == remote.ObservedGeneration
                    ? remote.NativeV2Revision : 0;

            if (!NadaVfxRightHandNativeV2Transport.Request(
                    remote.PeerId, remote.ObservedItemHash,
                    remote.ObservedGeneration, knownRevision))
            {
                return true; // Fail closed: don't accept an unverified old packet.
            }

            remote.NativeV2RequestAttempts = Math.Min(
                remote.NativeV2RequestAttempts + 1,
                InitialUnansweredRequestLimit + 1);
            remote.NextNativeV2Request = now +
                GetRetryDelay(remote.NativeV2RequestAttempts);
            return true;
        }

        private static void OnNativeV2StateReceived(
            long sender, uint generation, WeaponVfxNetworkStatePacket packet)
        {
            if (sender == 0 || generation == 0 || packet == null ||
                packet.ItemHash == 0 || packet.Revision == 0 ||
                (packet.Bound && packet.State == null) ||
                !RemoteRightStates.TryGetValue(sender, out RemoteRightState remote))
                return;

            CheckObservedGeneration(remote, true);
            if (remote.ObservedItemHash != packet.ItemHash ||
                remote.ObservedVanillaHash != packet.ItemHash ||
                remote.ObservedGeneration != generation)
            {
                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [RightNativeV2StaleRejected] " +
                    $"peer={sender} hash={packet.ItemHash} " +
                    $"packetGeneration={generation} " +
                    $"visibleGeneration={remote.ObservedGeneration}");
                return;
            }

            if (remote.HasNativeV2Advertisement &&
                IsRevisionNewer(remote.NativeV2AdvertisedRevision,
                                packet.Revision))
                return;

            if (remote.HasNativeV2State &&
                remote.NativeV2Generation == generation &&
                packet.Revision != remote.NativeV2Revision &&
                !IsRevisionNewer(packet.Revision, remote.NativeV2Revision))
                return;

            bool changedRevision = !remote.HasNativeV2State ||
                remote.NativeV2Generation != generation ||
                remote.NativeV2Revision != packet.Revision;
            remote.NativeV2Authority = true;
            remote.LegacyFallback = false;
            remote.HasNativeV2State = true;
            remote.NativeV2Generation = generation;
            remote.NativeV2Hash = packet.ItemHash;
            remote.NativeV2Revision = packet.Revision;
            remote.NativeV2Bound = packet.Bound;
            remote.NativeV2State = packet.State;
            if (changedRevision)
            {
                remote.NativeV2LegacyResolved = false;
                remote.NativeV2LegacyBound = false;
                remote.NativeV2LegacyState = default;
                remote.LegacyResolutionAttempts = 0;
                remote.NextLegacyResolutionRequest = 0f;
            }
            remote.NativeV2RequestAttempts = 0;
            remote.NextNativeV2Request = 0f;
            remote.RequestPending = false;
            remote.RequestedItemHash = 0;
            remote.RequestAttempts = 0;
            if (!remote.HasNativeV2Advertisement ||
                IsRevisionNewer(packet.Revision,
                                remote.NativeV2AdvertisedRevision))
            {
                remote.HasNativeV2Advertisement = true;
                remote.NativeV2AdvertisedRevision = packet.Revision;
                remote.NativeV2AdvertisedBound = packet.Bound;
            }

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [RightNativeV2StateAccepted] " +
                $"peer={sender} hash={packet.ItemHash} " +
                $"generation={generation} revision={packet.Revision} " +
                $"bound={packet.Bound} blocks={packet.State?.Effects?.Count ?? 0}");

            ApplyNativeV2State(remote);
        }

        private static void OnNativeV2StateChanged(
            long sender, int itemHash, uint generation,
            uint revision, bool bound)
        {
            if (sender == 0 || itemHash == 0 || generation == 0 ||
                revision == 0 ||
                !RemoteRightStates.TryGetValue(sender, out RemoteRightState remote))
                return;

            CheckObservedGeneration(remote, true);
            if (remote.ObservedGeneration != generation ||
                remote.ObservedItemHash != itemHash ||
                remote.ObservedVanillaHash != itemHash)
                return; // ZDO may catch up later; the periodic check re-requests.

            if (remote.HasNativeV2Advertisement &&
                revision != remote.NativeV2AdvertisedRevision &&
                !IsRevisionNewer(revision, remote.NativeV2AdvertisedRevision))
                return;

            remote.NativeV2Authority = true;
            remote.LegacyFallback = false;
            remote.HasNativeV2Advertisement = true;
            remote.NativeV2AdvertisedRevision = revision;
            remote.NativeV2AdvertisedBound = bound;

            // A same-item settings update must not destroy its surviving
            // effect instances. Only a confirmed unbind removes immediately.
            if (!bound && remote.AppliedBound && remote.AppliedNativeV2 &&
                remote.AppliedGeneration == generation &&
                IsRevisionNewer(revision, remote.AppliedRevision))
            {
                HideTrackedRig(remote);
            }

            RequestStateIfNeeded(remote, force: true);
        }

        private static bool IsNativeV2AppliedCurrent(
            RemoteRightState remote, int visualId)
        {
            if (!remote.AppliedNativeV2 ||
                remote.AppliedGeneration != remote.ObservedGeneration ||
                remote.AppliedInstanceId != visualId ||
                remote.AppliedItemHash != remote.NativeV2Hash ||
                remote.AppliedRevision != remote.NativeV2Revision ||
                remote.AppliedBound !=
                    (remote.NativeV2Bound ||
                     (remote.NativeV2LegacyResolved && remote.NativeV2LegacyBound)))
                return false;

            if (!remote.AppliedBound)
                return true;

            if (remote.AppliedVisualRoot == null ||
                remote.AppliedRigRoot == null || remote.RightInstance == null ||
                remote.AppliedVisualRoot.parent != remote.RightInstance.transform ||
                remote.AppliedRigRoot.parent != remote.AppliedVisualRoot)
                return false;

            if (Time.unscaledTime >= remote.NextVisualRecheckTime)
            {
                remote.NextVisualRecheckTime =
                    Time.unscaledTime + RemoteVisualRecheckSeconds;
                if (NadaWeaponTargets.FindEquippedWeaponVisualRoot(
                        remote.RightInstance.transform) != remote.AppliedVisualRoot)
                    return false;
            }
            return true;
        }

        private static void ApplyNativeV2State(RemoteRightState remote)
        {
            if (remote == null || !remote.NativeV2Authority ||
                !remote.HasNativeV2State ||
                remote.ObservedVanillaHash != remote.ObservedItemHash ||
                remote.NativeV2Generation != remote.ObservedGeneration ||
                remote.NativeV2Hash != remote.ObservedItemHash ||
                HasNewerNativeV2Advertisement(remote) ||
                remote.RightInstance == null)
                return;

            if (!MayShowRig(remote))
            {
                HideTrackedRig(remote);
                return;
            }

            int visualId = remote.RightInstance.GetInstanceID();
            if (IsNativeV2AppliedCurrent(remote, visualId))
                return;

            if (!remote.NativeV2Bound && !remote.NativeV2LegacyResolved)
            {
                // Fail closed visually while deciding whether this is a
                // genuinely unbound item or an older legacy-bound style.
                HideTrackedRig(remote);
                RequestNativeV2LegacyResolution(remote);
                return;
            }

            if (!remote.NativeV2Bound && remote.NativeV2LegacyBound)
            {
                ApplyNativeV2LegacyState(remote, visualId);
                return;
            }

            if (!remote.NativeV2Bound)
            {
                if (!RemoveTrackedAppliedRig(remote))
                    NadaWeaponRigRemoval.RemoveFromEquippedRoot(remote.RightInstance);
                ClearAppliedState(remote);
                ClearFailedApply(remote);
                remote.AppliedNativeV2 = true;
                remote.AppliedGeneration = remote.ObservedGeneration;
                remote.AppliedInstance = remote.RightInstance;
                remote.AppliedInstanceId = visualId;
                remote.AppliedItemHash = remote.NativeV2Hash;
                remote.AppliedRevision = remote.NativeV2Revision;
                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [RightNativeV2RigRemoved] " +
                    $"peer={remote.PeerId} generation={remote.ObservedGeneration} " +
                    $"revision={remote.NativeV2Revision}");
                return;
            }

            if (remote.NativeV2State == null)
                return;

            if (remote.FailedApplyNative &&
                remote.FailedApplyInstanceId == visualId &&
                remote.FailedApplyItemHash == remote.NativeV2Hash &&
                remote.FailedApplyRevision == remote.NativeV2Revision &&
                Time.unscaledTime < remote.NextApplyRetryTime)
                return;

            RetireObsoleteAppliedRigIfNeeded(remote);

            bool applied = WeaponRigController.TryApplyResolvedBlockState(
                remote.RightInstance, remote.NativeV2Hash, remote.NativeV2State);
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
                remote.FailedApplyItemHash = remote.NativeV2Hash;
                remote.FailedApplyRevision = remote.NativeV2Revision;
                remote.NextApplyRetryTime =
                    Time.unscaledTime + FailedApplyRetrySeconds;
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [RightNativeV2ApplyFailed] " +
                    $"peer={remote.PeerId} generation={remote.ObservedGeneration} " +
                    $"revision={remote.NativeV2Revision}");
                return;
            }

            ClearFailedApply(remote);
            remote.AppliedInstance = remote.RightInstance;
            remote.AppliedInstanceId = visualId;
            remote.AppliedItemHash = remote.NativeV2Hash;
            remote.AppliedRevision = remote.NativeV2Revision;
            remote.AppliedBound = true;
            remote.AppliedNative = true;
            remote.AppliedNativeV2 = true;
            remote.AppliedGeneration = remote.ObservedGeneration;
            remote.AppliedVisualRoot = visual;
            remote.AppliedRigRoot = rig;
            remote.NextVisualRecheckTime =
                Time.unscaledTime + RemoteVisualRecheckSeconds;

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [RightNativeV2RigApplied] " +
                $"peer={remote.PeerId} hash={remote.NativeV2Hash} " +
                $"generation={remote.ObservedGeneration} " +
                $"revision={remote.NativeV2Revision} " +
                $"blocks={remote.NativeV2State.Effects.Count} " +
                $"instance={visualId} rig={rig.GetInstanceID()}");
        }

        // Used only for native-unbound packets, which are ambiguous until the
        // legacy V2 packet for that same revision is decoded. The old native V1
        // companion response stays ignored; it cannot claim authority.
        private static void RequestNativeV2LegacyResolution(RemoteRightState remote)
        {
            if (remote.NativeV2LegacyResolved ||
                Time.unscaledTime < remote.NextLegacyResolutionRequest)
                return;

            if (remote.LegacyResolutionAttempts >= 5)
            {
                // No response means no verified legacy binding. Remove the
                // stale rig, but never resurrect it from local settings.
                remote.NativeV2LegacyResolved = true;
                remote.NativeV2LegacyBound = false;
                ApplyNativeV2State(remote);
                return;
            }

            if (NadaVfxRightHandNetworkTransport.RequestState(
                    remote.PeerId, remote.NativeV2Hash, remote.NativeV2Revision))
            {
                remote.LegacyResolutionAttempts++;
                remote.NextLegacyResolutionRequest =
                    Time.unscaledTime + GetRetryDelay(remote.LegacyResolutionAttempts);
            }
        }

        private static bool TryAcceptNativeV2LegacyResponse(
            RemoteRightState remote, NadaVfxNetworkPacket packet,
            VfxState state)
        {
            if (remote == null || packet == null ||
                !remote.NativeV2Authority || !remote.HasNativeV2State ||
                remote.NativeV2Bound ||
                remote.NativeV2Generation == 0 ||
                remote.NativeV2Generation != remote.ObservedGeneration ||
                packet.ItemHash != remote.NativeV2Hash ||
                packet.Revision != remote.NativeV2Revision)
                return false;

            remote.NativeV2LegacyResolved = true;
            remote.NativeV2LegacyBound = packet.Bound;
            remote.NativeV2LegacyState = state;
            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [RightNativeV2LegacyResolved] " +
                $"peer={remote.PeerId} generation={remote.ObservedGeneration} " +
                $"revision={packet.Revision} bound={packet.Bound}");
            ApplyNativeV2State(remote);
            return true;
        }

        private static void ApplyNativeV2LegacyState(
            RemoteRightState remote, int visualId)
        {
            if (!remote.NativeV2LegacyResolved || !remote.NativeV2LegacyBound)
                return;

            if (remote.FailedApplyInstanceId == visualId &&
                remote.FailedApplyItemHash == remote.NativeV2Hash &&
                remote.FailedApplyRevision == remote.NativeV2Revision &&
                Time.unscaledTime < remote.NextApplyRetryTime)
                return;

            RetireObsoleteAppliedRigIfNeeded(remote);
            bool applied = WeaponRigController.TryApplyResolvedState(
                remote.RightInstance, remote.NativeV2Hash,
                remote.NativeV2LegacyState);
            Transform visual = null;
            Transform rig = null;
            if (applied)
            {
                visual = NadaWeaponTargets.FindEquippedWeaponVisualRoot(
                    remote.RightInstance.transform);
                if (visual != null)
                    rig = NadaRigPaths.FindDirectChild(visual,
                        Plugin.LocalWeaponRootName);
                applied = rig != null && rig.gameObject.activeSelf;
            }

            if (!applied)
            {
                remote.FailedApplyNative = false;
                remote.FailedApplyInstanceId = visualId;
                remote.FailedApplyItemHash = remote.NativeV2Hash;
                remote.FailedApplyRevision = remote.NativeV2Revision;
                remote.NextApplyRetryTime =
                    Time.unscaledTime + FailedApplyRetrySeconds;
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [RightNativeV2LegacyApplyFailed] " +
                    $"peer={remote.PeerId} revision={remote.NativeV2Revision}");
                return;
            }

            ClearFailedApply(remote);
            remote.AppliedInstance = remote.RightInstance;
            remote.AppliedInstanceId = visualId;
            remote.AppliedItemHash = remote.NativeV2Hash;
            remote.AppliedRevision = remote.NativeV2Revision;
            remote.AppliedBound = true;
            remote.AppliedNative = false;
            remote.AppliedNativeV2 = true;
            remote.AppliedGeneration = remote.ObservedGeneration;
            remote.AppliedVisualRoot = visual;
            remote.AppliedRigRoot = rig;
            remote.NextVisualRecheckTime =
                Time.unscaledTime + RemoteVisualRecheckSeconds;
            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [RightNativeV2LegacyRigApplied] " +
                $"peer={remote.PeerId} generation={remote.ObservedGeneration} " +
                $"revision={remote.NativeV2Revision}");
        }

        [HarmonyPatch(typeof(global::Player), "Update")]
        private static class NativeV2RemoteGenerationPatch
        {
            private static void Postfix(global::Player __instance)
            {
                try
                {
                    if (__instance == null ||
                        __instance == global::Player.m_localPlayer ||
                        !PeerByPlayerId.TryGetValue(
                            __instance.GetInstanceID(), out long peer) ||
                        !RemoteRightStates.TryGetValue(
                            peer, out RemoteRightState remote) ||
                        !ReferenceEquals(remote.ObservedPlayer, __instance))
                        return;

                    if (Time.unscaledTime < remote.NextGenerationCheck)
                        return;

                    CheckObservedGeneration(remote, true);
                    if (remote.RightInstance == null ||
                        remote.ObservedItemHash == 0)
                        return;

                    if (remote.ObservedGeneration != 0 &&
                        !remote.LegacyFallback &&
                        (!HasCurrentNativeV2State(remote) ||
                         HasNewerNativeV2Advertisement(remote)))
                        RequestStateIfNeeded(remote, force: false);
                    else if (remote.LegacyFallback &&
                             !remote.NativeV2Authority &&
                             !remote.HasReceivedNative &&
                             !remote.HasReceivedState)
                        RequestStateIfNeeded(remote, force: false);

                    ApplyReceivedState(remote);
                }
                catch (Exception error)
                {
                    Plugin.Log.LogWarning(
                        $"{Plugin.ModName}: [RightNativeV2GenerationCheckFailed] " +
                        error.Message);
                }
            }
        }
    }
}
