
using System;
using System.Collections.Generic;
using HarmonyLib;

namespace NADA.VFX.Weapon.Core.Network
{
    // Generation-aware native right-hand transport.
    //
    // The older right-hand V2/native V1 RPCs remain available during
    // migration. Never reuse their RPC names with a different packet layout.
    //
    // This class transports prepared state only. It does not resolve
    // ItemData, own remote rigs, or decide which state may be rendered.
    internal static class NadaVfxRightHandNativeV2Transport
    {
        private const string RequestRpc =
            "naomi.nada.vfx.weapon.right_request_native_v2";

        private const string ResponseRpc =
            "naomi.nada.vfx.weapon.right_response_native_v2";

        private const string ChangedRpc =
            "naomi.nada.vfx.weapon.right_changed_native_v2";

        private const int Version = 2;
        private const int MaxControlBytes = 128;

        private const int MaxResponseBytes =
            WeaponVfxNetworkCodec.MaxPacketBytes + 128;

        // The publisher uses generations 1 through int.MaxValue,
        // then wraps to 1. Zero means no equipped identity.
        private const uint MaxEquipmentGeneration = int.MaxValue;

        private static global::ZRoutedRpc _registered;

        private sealed class Subscription
        {
            internal int ItemHash;
            internal uint Generation;
            internal string PlatformId;
        }

        // An observer subscribes by requesting a state. Keep requests
        // subscribed even if publication has not happened yet.
        private static readonly Dictionary<long, Subscription>
            InterestedPeers = new();

        internal static event Action<
            long,
            uint,
            WeaponVfxNetworkStatePacket> StateReceived;

        internal static event Action<
            long,
            int,
            uint,
            uint,
            bool> StateChanged;

        internal static event Action SessionReset;

        internal static void TryRegister()
        {
            global::ZRoutedRpc rpc =
                global::ZRoutedRpc.instance;

            if (rpc == null ||
                ReferenceEquals(rpc, _registered))
            {
                return;
            }

            _registered = rpc;
            InterestedPeers.Clear();

            rpc.Register(
                RequestRpc,
                new Action<long, global::ZPackage>(OnRequest));

            rpc.Register(
                ResponseRpc,
                new Action<long, global::ZPackage>(OnResponse));

            rpc.Register(
                ChangedRpc,
                new Action<long, global::ZPackage>(OnChanged));

            if (SubscriptionOrderingContractPasses())
            {
                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: " +
                    "[RightNativeV2SubscriptionTests OK] tests=9");
            }
            else
            {
                Plugin.Log.LogError(
                    $"{Plugin.ModName}: " +
                    "[RightNativeV2SubscriptionTests FAIL]");
            }

            SessionReset?.Invoke();

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [RightNativeV2TransportRegistered]");
        }

        internal static bool Request(
            long peer,
            int itemHash,
            uint generation,
            uint knownRevision)
        {
            if (peer == 0 ||
                itemHash == 0 ||
                generation == 0 ||
                generation > MaxEquipmentGeneration)
            {
                return false;
            }

            global::ZRoutedRpc rpc =
                global::ZRoutedRpc.instance;

            if (rpc == null ||
                !ReferenceEquals(rpc, _registered))
            {
                return false;
            }

            try
            {
                var package = new global::ZPackage();

                package.Write(Version);
                package.Write(itemHash);
                package.Write(unchecked((int)generation));
                package.Write(unchecked((int)knownRevision));

                rpc.InvokeRoutedRPC(
                    peer,
                    RequestRpc,
                    package);

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [RightNativeV2RequestSent] " +
                    $"peer={peer} hash={itemHash} " +
                    $"generation={generation} known={knownRevision}");

                return true;
            }
            catch (Exception error)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [RightNativeV2RequestFailed] " +
                    $"peer={peer} hash={itemHash} " +
                    $"error={error.Message}");

                return false;
            }
        }

        internal static void NotifyChanged(
            NadaVfxRightHandPublishedState state)
        {
            global::ZRoutedRpc rpc =
                global::ZRoutedRpc.instance;

            if (state == null ||
                rpc == null ||
                !ReferenceEquals(rpc, _registered) ||
                InterestedPeers.Count == 0)
            {
                return;
            }

            // Notify subscribers for this prefab even when equipment
            // generation changes. The receiver needs to invalidate the
            // previous item rather than silently keep its old effects.
            var recipients = new List<long>();

            foreach (KeyValuePair<long, Subscription> entry
                     in InterestedPeers)
            {
                if (entry.Key != 0 &&
                    entry.Value.ItemHash == state.ItemHash)
                {
                    recipients.Add(entry.Key);
                }
            }

            foreach (long peer in recipients)
            {
                try
                {
                    var package = new global::ZPackage();

                    package.Write(Version);
                    package.Write(state.ItemHash);
                    package.Write(
                        unchecked((int)state.EquipmentGeneration));
                    package.Write(unchecked((int)state.Revision));
                    package.Write(state.NativeBound ? 1 : 0);

                    rpc.InvokeRoutedRPC(
                        peer,
                        ChangedRpc,
                        package);

                    Plugin.Log.LogInfo(
                        $"{Plugin.ModName}: [RightNativeV2ChangedSent] " +
                        $"peer={peer} hash={state.ItemHash} " +
                        $"generation={state.EquipmentGeneration} " +
                        $"revision={state.Revision} " +
                        $"bound={state.NativeBound}");
                }
                catch (Exception error)
                {
                    Plugin.Log.LogWarning(
                        $"{Plugin.ModName}: [RightNativeV2ChangedFailed] " +
                        $"peer={peer} error={error.Message}");
                }
            }
        }

        private static void OnRequest(
            long sender,
            global::ZPackage package)
        {
            try
            {
                if (sender == 0 ||
                    package == null ||
                    package.Size() <= 0 ||
                    package.Size() > MaxControlBytes)
                {
                    return;
                }

                int version = package.ReadInt();
                int itemHash = package.ReadInt();

                uint generation =
                    unchecked((uint)package.ReadInt());

                uint knownRevision =
                    unchecked((uint)package.ReadInt());

                if (version != Version ||
                    itemHash == 0 ||
                    generation == 0 ||
                    generation > MaxEquipmentGeneration)
                {
                    return;
                }

                // Read the owner's current publication before deciding
                // whether an existing subscription may be replaced.
                NadaVfxRightHandStatePublisher.TryGetCurrent(
                    out NadaVfxRightHandPublishedState state);

                bool matchesPublished =
                    state != null &&
                    state.ItemHash == itemHash &&
                    state.EquipmentGeneration == generation;

                if (!InterestedPeers.TryGetValue(
                        sender,
                        out Subscription interest))
                {
                    interest = new Subscription();
                    InterestedPeers.Add(sender, interest);
                }
                else if (!CanReplaceSubscription(
                             interest,
                             itemHash,
                             generation,
                             matchesPublished))
                {
                    string rejectionReason =
                        generation == interest.Generation
                            ? "same-generation-item-mismatch"
                            : "older-generation";

                    Plugin.Log.LogInfo(
                        $"{Plugin.ModName}: " +
                        "[RightNativeV2SubscriptionRequestIgnored] " +
                        $"peer={sender} " +
                        $"requestedHash={itemHash} " +
                        $"requestedGeneration={generation} " +
                        $"subscribedHash={interest.ItemHash} " +
                        $"subscribedGeneration={interest.Generation} " +
                        $"reason={rejectionReason}");

                    return;
                }

                interest.ItemHash = itemHash;
                interest.Generation = generation;

                if (string.IsNullOrEmpty(interest.PlatformId))
                    TryLinkSubscriber(sender, interest);

                // Pending subscriptions are retained. Equipment
                // replication and local publication may not be ready
                // when the observer first requests a generation.
                if (!matchesPublished)
                    return;

                SendResponse(sender, state);

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [RightNativeV2RequestHandled] " +
                    $"peer={sender} hash={itemHash} " +
                    $"generation={generation} " +
                    $"known={knownRevision} revision={state.Revision}");
            }
            catch (Exception error)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [RightNativeV2RequestReceiveFailed] " +
                    error.Message);
            }
        }

        // The subscription belongs to an equipment generation, not merely
        // a prefab hash. An older request cannot displace newer interest.
        private static bool CanReplaceSubscription(
            Subscription current,
            int requestedHash,
            uint requestedGeneration,
            bool matchesPublished)
        {
            if (current == null || current.Generation == 0)
                return true;

            if (requestedGeneration == current.Generation)
            {
                return requestedHash == current.ItemHash ||
                       matchesPublished;
            }

            return IsNewerEquipmentGeneration(
                requestedGeneration,
                current.Generation);
        }

        private static bool IsNewerEquipmentGeneration(
            uint incoming,
            uint current)
        {
            if (incoming == 0 ||
                current == 0 ||
                incoming > MaxEquipmentGeneration ||
                current > MaxEquipmentGeneration ||
                incoming == current)
            {
                return false;
            }

            // The publisher wraps at int.MaxValue, not uint.MaxValue.
            // Compare forward distance around that exact sequence.
            // A half-range bound rejects distant stale generations.
            uint forwardDistance =
                incoming > current
                    ? incoming - current
                    : MaxEquipmentGeneration - current + incoming;

            return forwardDistance <= MaxEquipmentGeneration / 2;
        }

        private static bool SubscriptionOrderingContractPasses()
        {
            var first = new Subscription
            {
                ItemHash = 10,
                Generation = 1
            };

            var second = new Subscription
            {
                ItemHash = 20,
                Generation = 2
            };

            return
                CanReplaceSubscription(null, 10, 1, false) &&
                CanReplaceSubscription(first, 10, 1, false) &&
                CanReplaceSubscription(first, 20, 2, false) &&
                !CanReplaceSubscription(first, 20, 1, false) &&
                CanReplaceSubscription(first, 20, 1, true) &&
                !CanReplaceSubscription(second, 10, 1, false) &&
                CanReplaceSubscription(second, 20, 2, false) &&
                IsNewerEquipmentGeneration(
                    1, MaxEquipmentGeneration) &&
                !IsNewerEquipmentGeneration(
                    MaxEquipmentGeneration, 1);
        }

        private static void SendResponse(
            long peer,
            NadaVfxRightHandPublishedState state)
        {
            if (peer == 0 || state == null)
                return;

            global::ZRoutedRpc rpc =
                global::ZRoutedRpc.instance;

            byte[] bytes = state.NativePacketBytes;

            if (rpc == null ||
                !ReferenceEquals(rpc, _registered) ||
                bytes == null ||
                bytes.Length == 0 ||
                bytes.Length > WeaponVfxNetworkCodec.MaxPacketBytes)
            {
                return;
            }

            var package = new global::ZPackage();

            package.Write(Version);
            package.Write(
                unchecked((int)state.EquipmentGeneration));
            package.Write(bytes.Length);
            package.Write(bytes);

            rpc.InvokeRoutedRPC(
                peer,
                ResponseRpc,
                package);

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [RightNativeV2ResponseSent] " +
                $"peer={peer} hash={state.ItemHash} " +
                $"generation={state.EquipmentGeneration} " +
                $"revision={state.Revision} " +
                $"bound={state.NativeBound} " +
                $"blocks={state.NativeBlockCount} " +
                $"bytes={bytes.Length}");
        }

        private static void OnResponse(
            long sender,
            global::ZPackage package)
        {
            try
            {
                if (sender == 0 ||
                    package == null ||
                    package.Size() <= 0 ||
                    package.Size() > MaxResponseBytes)
                {
                    return;
                }

                int version = package.ReadInt();

                uint generation =
                    unchecked((uint)package.ReadInt());

                int declaredLength = package.ReadInt();

                if (version != Version ||
                    generation == 0 ||
                    declaredLength <= 0 ||
                    declaredLength >
                        WeaponVfxNetworkCodec.MaxPacketBytes)
                {
                    return;
                }

                byte[] bytes = package.ReadByteArray();

                if (bytes == null ||
                    bytes.Length != declaredLength)
                {
                    return;
                }

                if (!WeaponVfxNetworkCodec.TryDecode(
                        bytes,
                        out WeaponVfxNetworkStatePacket state,
                        out string reason))
                {
                    Plugin.Log.LogWarning(
                        $"{Plugin.ModName}: [RightNativeV2ResponseRejected] " +
                        $"peer={sender} reason='{reason}' " +
                        $"bytes={declaredLength}");

                    return;
                }

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [RightNativeV2ResponseReceived] " +
                    $"peer={sender} hash={state.ItemHash} " +
                    $"generation={generation} " +
                    $"revision={state.Revision} " +
                    $"bound={state.Bound} bytes={declaredLength}");

                // The remote tracker must compare generation against the
                // current observed ZDO before consuming this packet.
                StateReceived?.Invoke(
                    sender,
                    generation,
                    state);
            }
            catch (Exception error)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [RightNativeV2ResponseFailed] " +
                    error.Message);
            }
        }

        private static void OnChanged(
            long sender,
            global::ZPackage package)
        {
            try
            {
                if (sender == 0 ||
                    package == null ||
                    package.Size() <= 0 ||
                    package.Size() > MaxControlBytes)
                {
                    return;
                }

                int version = package.ReadInt();
                int itemHash = package.ReadInt();

                uint generation =
                    unchecked((uint)package.ReadInt());

                uint revision =
                    unchecked((uint)package.ReadInt());

                int bound = package.ReadInt();

                if (version != Version ||
                    itemHash == 0 ||
                    generation == 0 ||
                    revision == 0 ||
                    (bound != 0 && bound != 1))
                {
                    return;
                }

                StateChanged?.Invoke(
                    sender,
                    itemHash,
                    generation,
                    revision,
                    bound == 1);
            }
            catch (Exception error)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [RightNativeV2ChangedReceiveFailed] " +
                    error.Message);
            }
        }

        // Routed RPC sender IDs are player sessions, not necessarily
        // direct ZNetPeer socket IDs.
        private static void TryLinkSubscriber(
            long sender,
            Subscription interest)
        {
            global::ZNet net = global::ZNet.instance;

            if (net == null)
                return;

            var players = net.GetPlayerList();

            if (players == null)
                return;

            foreach (var player in players)
            {
                if (player.m_characterID.UserID != sender ||
                    !player.m_userInfo.m_id.IsValid)
                {
                    continue;
                }

                string platformId =
                    player.m_userInfo.m_id.ToString();

                if (!string.IsNullOrEmpty(platformId))
                {
                    interest.PlatformId = platformId;

                    Plugin.Log.LogInfo(
                        $"{Plugin.ModName}: [RightNativeV2SubscriberLinked] " +
                        $"peer={sender}");
                }

                return;
            }
        }

        private static void OnPlayerRosterUpdated(
            global::ZNet net)
        {
            if (net == null ||
                InterestedPeers.Count == 0)
            {
                return;
            }

            var players = net.GetPlayerList();

            if (players == null || players.Count == 0)
                return;

            var connectedAccounts =
                new HashSet<string>(StringComparer.Ordinal);

            var peerAccounts =
                new Dictionary<long, string>();

            var accountPeers =
                new Dictionary<string, long>(StringComparer.Ordinal);

            bool localPlayerPresent = false;
            bool incompleteIdentities = false;

            long localId = global::ZNet.GetUID();

            foreach (var player in players)
            {
                long playerSession =
                    player.m_characterID.UserID;

                if (playerSession != 0 &&
                    playerSession == localId)
                {
                    localPlayerPresent = true;
                }

                if (!player.m_userInfo.m_id.IsValid)
                {
                    if (playerSession != 0)
                        incompleteIdentities = true;

                    continue;
                }

                string platformId =
                    player.m_userInfo.m_id.ToString();

                if (string.IsNullOrEmpty(platformId))
                {
                    if (playerSession != 0)
                        incompleteIdentities = true;

                    continue;
                }

                connectedAccounts.Add(platformId);

                if (playerSession != 0)
                {
                    peerAccounts[playerSession] = platformId;
                    accountPeers[platformId] = playerSession;
                }
            }

            // A temporarily incomplete roster during joining or death
            // is not proof that a subscribed player has disconnected.
            if (!localPlayerPresent ||
                connectedAccounts.Count == 0 ||
                incompleteIdentities)
            {
                return;
            }

            var departed = new List<long>();

            foreach (KeyValuePair<long, Subscription> entry
                     in InterestedPeers)
            {
                Subscription interest = entry.Value;

                if (string.IsNullOrEmpty(interest.PlatformId))
                {
                    if (peerAccounts.TryGetValue(
                            entry.Key,
                            out string identity))
                    {
                        interest.PlatformId = identity;

                        Plugin.Log.LogInfo(
                            $"{Plugin.ModName}: [RightNativeV2SubscriberLinked] " +
                            $"peer={entry.Key}");
                    }

                    continue;
                }

                bool accountLeft =
                    !connectedAccounts.Contains(interest.PlatformId);

                bool sessionReplaced =
                    accountPeers.TryGetValue(
                        interest.PlatformId,
                        out long currentSession) &&
                    currentSession != entry.Key;

                if (accountLeft || sessionReplaced)
                    departed.Add(entry.Key);
            }

            foreach (long peer in departed)
            {
                InterestedPeers.Remove(peer);

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [RightNativeV2SubscriberRemoved] " +
                    $"peer={peer} reason=RosterDeparture");
            }
        }

        [HarmonyPatch(typeof(global::ZNet), "RPC_PlayerList")]
        private static class PlayerRosterPatch
        {
            private static void Postfix(global::ZNet __instance)
            {
                try
                {
                    OnPlayerRosterUpdated(__instance);
                }
                catch (Exception error)
                {
                    Plugin.Log.LogWarning(
                        $"{Plugin.ModName}: [RightNativeV2RosterUpdateFailed] " +
                        error.Message);
                }
            }
        }

        [HarmonyPatch(typeof(global::Game), "Start")]
        private static class GameStartPatch
        {
            private static void Prefix()
            {
                try
                {
                    TryRegister();
                }
                catch (Exception error)
                {
                    Plugin.Log.LogWarning(
                        $"{Plugin.ModName}: [RightNativeV2TransportRegisterFailed] " +
                        error.Message);
                }
            }
        }
    }
}
