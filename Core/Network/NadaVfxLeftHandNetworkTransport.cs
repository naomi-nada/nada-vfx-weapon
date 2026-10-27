
using System;
using System.Collections.Generic;
using HarmonyLib;

namespace NADA.VFX.Weapon.Core.Network
{
    // Slot identity is carried by dedicated RPC names. This is intentional:
    // the existing native v1 envelope contains an item hash but no slot, and
    // two equipped items can have the same prefab hash.
    internal static class NadaVfxLeftHandNetworkTransport
    {
        private const string RequestRpc =
            "naomi.nada.vfx.weapon.left_request_native_v1";
        private const string ResponseRpc =
            "naomi.nada.vfx.weapon.left_response_native_v1";
        private const string ChangedRpc =
            "naomi.nada.vfx.weapon.left_changed_native_v1";
        private const int Version = 1;
        private const int MaxControlBytes = 128;
        private const int MaxResponseBytes =
            WeaponVfxNetworkCodec.MaxPacketBytes + 128;

        private static global::ZRoutedRpc _registered;

        // Retain early requests so a later publication can notify them.
        // Account identity is learned from Valheim's server-synced roster.
        private sealed class Subscription
        {
            internal int ItemHash;
            internal string PlatformId;
        }

        private static readonly Dictionary<long, Subscription> InterestedPeers = new();

        internal static event Action<long, WeaponVfxNetworkStatePacket>
            StateReceived;
        internal static event Action<long, int, uint, bool>
            StateChanged;
        internal static event Action SessionReset;

        internal static void TryRegister()
        {
            global::ZRoutedRpc rpc = global::ZRoutedRpc.instance;
            if (rpc == null || ReferenceEquals(rpc, _registered))
                return;

            _registered = rpc;
            InterestedPeers.Clear();
            NadaVfxLeftHandStatePublisher.ResetSession();

            rpc.Register(RequestRpc,
                new Action<long, global::ZPackage>(OnRequest));
            rpc.Register(ResponseRpc,
                new Action<long, global::ZPackage>(OnResponse));
            rpc.Register(ChangedRpc,
                new Action<long, global::ZPackage>(OnChanged));
            SessionReset?.Invoke();

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [LeftNativeTransportRegistered]");
        }

        internal static bool Request(long peer, int hash, uint knownRevision)
        {
            if (peer == 0 || hash == 0)
                return false;

            global::ZRoutedRpc rpc = global::ZRoutedRpc.instance;
            if (rpc == null || !ReferenceEquals(rpc, _registered))
                return false;

            try
            {
                var package = new global::ZPackage();
                package.Write(Version);
                package.Write(hash);
                package.Write(unchecked((int)knownRevision));
                rpc.InvokeRoutedRPC(peer, RequestRpc, package);
                return true;
            }
            catch (Exception error)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [LeftNativeRequestFailed] " +
                    $"peer={peer} hash={hash} reason={error.Message}");
                return false;
            }
        }

        internal static void NotifyChanged(
            NadaVfxLeftHandStatePublisher.PublishedState state)
        {
            global::ZRoutedRpc rpc = global::ZRoutedRpc.instance;
            if (state == null || rpc == null ||
                !ReferenceEquals(rpc, _registered) ||
                InterestedPeers.Count == 0)
                return;

            // Snapshot matching observers before invoking any RPC. A peer can
            // be interested in a different shield while our item is changing.
            var recipients = new List<long>();

            foreach (KeyValuePair<long, Subscription> subscription in InterestedPeers)
            {
                if (subscription.Key != 0 &&
                    subscription.Value.ItemHash == state.ItemHash)
                    recipients.Add(subscription.Key);
            }

            foreach (long peer in recipients)
            {
                try
                {
                    var package = new global::ZPackage();
                    package.Write(Version);
                    package.Write(state.ItemHash);
                    package.Write(unchecked((int)state.Revision));
                    package.Write(state.Bound ? 1 : 0);

                    rpc.InvokeRoutedRPC(peer, ChangedRpc, package);

                    Plugin.Log.LogInfo(
                        $"{Plugin.ModName}: [LeftNativeChangedSent] " +
                        $"peer={peer} hash={state.ItemHash} " +
                        $"revision={state.Revision} bound={state.Bound}");
                }
                catch (Exception error)
                {
                    Plugin.Log.LogWarning(
                        $"{Plugin.ModName}: [LeftNativeChangedFailed] " +
                        $"peer={peer} error={error.Message}");
                }
            }
        }

        private static void OnRequest(long sender, global::ZPackage package)
        {
            try
            {
                if (sender == 0 || package == null ||
                    package.Size() <= 0 || package.Size() > MaxControlBytes)
                    return;

                int version = package.ReadInt();
                int hash = package.ReadInt();
                uint known = unchecked((uint)package.ReadInt());

                if (version != Version || hash == 0)
                    return;

                // Record interest before checking publication. An early
                // request must still receive the later change notification.
                if (!InterestedPeers.TryGetValue(sender, out Subscription interest))
                {
                    interest = new Subscription();
                    InterestedPeers.Add(sender, interest);
                }

                interest.ItemHash = hash;

                if (string.IsNullOrEmpty(interest.PlatformId))
                    TryLinkSubscriber(sender, interest);

                if (!NadaVfxLeftHandStatePublisher.TryGetCurrent(
                        out var published) || published.ItemHash != hash)
                    return;

                SendResponse(sender, published);

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [LeftNativeRequestHandled] " +
                    $"peer={sender} hash={hash} known={known} " +
                    $"revision={published.Revision}");
            }
            catch (Exception error)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [LeftNativeRequestReceiveFailed] " +
                    error.Message);
            }
        }

        // A routed RPC sender may be another client behind a dedicated server.
        // ZNet.GetPeers() only exposes direct sockets, so it cannot be used to
        // decide whether a remote observer is still in the session.
        private static void TryLinkSubscriber(long sender, Subscription interest)
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
                    continue;

                string platformId = player.m_userInfo.m_id.ToString();

                if (!string.IsNullOrEmpty(platformId))
                {
                    interest.PlatformId = platformId;

                    Plugin.Log.LogInfo(
                        $"{Plugin.ModName}: [LeftNativeSubscriberLinked] " +
                        $"peer={sender}");
                }

                return;
            }
        }

        private static void OnPlayerRosterUpdated(global::ZNet net)
        {
            if (net == null || InterestedPeers.Count == 0)
                return;

            var players = net.GetPlayerList();
            if (players == null || players.Count == 0)
                return;

            var connectedAccounts =
                new HashSet<string>(StringComparer.Ordinal);

            var peerAccounts = new Dictionary<long, string>();

            var accountPeers =
                new Dictionary<string, long>(StringComparer.Ordinal);

            bool localPlayerPresent = false;
            bool incompleteIdentities = false;
            long localId = global::ZNet.GetUID();

            foreach (var player in players)
            {
                long playerSession = player.m_characterID.UserID;

                if (playerSession != 0 && playerSession == localId)
                    localPlayerPresent = true;

                if (!player.m_userInfo.m_id.IsValid)
                {
                    if (playerSession != 0)
                        incompleteIdentities = true;

                    continue;
                }

                string platformId = player.m_userInfo.m_id.ToString();

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

            // An incomplete roster during joining/loading/death must never
            // revoke a legitimate subscription. Our own character in the
            // roster is evidence that this is a populated session snapshot.
            if (!localPlayerPresent || connectedAccounts.Count == 0 ||
                incompleteIdentities)
                return;

            var departed = new List<long>();

            foreach (KeyValuePair<long, Subscription> pair in InterestedPeers)
            {
                Subscription interest = pair.Value;

                if (string.IsNullOrEmpty(interest.PlatformId))
                {
                    if (peerAccounts.TryGetValue(pair.Key, out string identity))
                    {
                        interest.PlatformId = identity;

                        Plugin.Log.LogInfo(
                            $"{Plugin.ModName}: [LeftNativeSubscriberLinked] " +
                            $"peer={pair.Key}");
                    }

                    continue;
                }

                // Accounts remain in the roster across character death.
                // If the same account reconnects with a new session ID,
                // the earlier routed endpoint must still be retired.
                bool accountLeft =
                    !connectedAccounts.Contains(interest.PlatformId);

                bool sessionReplaced =
                    accountPeers.TryGetValue(
                        interest.PlatformId,
                        out long currentSession) &&
                    currentSession != pair.Key;

                if (accountLeft || sessionReplaced)
                    departed.Add(pair.Key);
            }

            foreach (long peer in departed)
            {
                InterestedPeers.Remove(peer);

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [LeftNativeSubscriberRemoved] " +
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
                        $"{Plugin.ModName}: [LeftNativeRosterUpdateFailed] " +
                        error.Message);
                }
            }
        }

        private static void SendResponse(
            long peer,
            NadaVfxLeftHandStatePublisher.PublishedState state)
        {
            global::ZRoutedRpc rpc = global::ZRoutedRpc.instance;

            if (rpc == null || !ReferenceEquals(rpc, _registered) ||
                state.Bytes == null || state.Bytes.Length == 0 ||
                state.Bytes.Length > WeaponVfxNetworkCodec.MaxPacketBytes)
                return;

            var package = new global::ZPackage();
            package.Write(Version);
            package.Write(state.Bytes.Length);
            package.Write(state.Bytes);

            rpc.InvokeRoutedRPC(peer, ResponseRpc, package);

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [LeftNativeResponseSent] " +
                $"peer={peer} hash={state.ItemHash} revision={state.Revision} " +
                $"bound={state.Bound} blocks={state.BlockCount} " +
                $"bytes={state.Bytes.Length}");
        }

        private static void OnResponse(long sender, global::ZPackage package)
        {
            try
            {
                if (sender == 0 || package == null ||
                    package.Size() <= 0 || package.Size() > MaxResponseBytes)
                    return;

                int version = package.ReadInt();
                int length = package.ReadInt();

                if (version != Version || length <= 0 ||
                    length > WeaponVfxNetworkCodec.MaxPacketBytes)
                    return;

                byte[] bytes = package.ReadByteArray();

                if (bytes == null || bytes.Length != length)
                    return;

                if (!WeaponVfxNetworkCodec.TryDecode(
                        bytes,
                        out WeaponVfxNetworkStatePacket state,
                        out string reason))
                {
                    Plugin.Log.LogWarning(
                        $"{Plugin.ModName}: [LeftNativeResponseRejected] " +
                        $"peer={sender} reason='{reason}' bytes={length}");
                    return;
                }

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [LeftNativeResponseReceived] " +
                    $"peer={sender} hash={state.ItemHash} " +
                    $"revision={state.Revision} bound={state.Bound} " +
                    $"bytes={length}");

                StateReceived?.Invoke(sender, state);
            }
            catch (Exception error)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [LeftNativeResponseFailed] " +
                    error.Message);
            }
        }

        private static void OnChanged(long sender, global::ZPackage package)
        {
            try
            {
                if (sender == 0 || package == null ||
                    package.Size() <= 0 || package.Size() > MaxControlBytes)
                    return;

                int version = package.ReadInt();
                int hash = package.ReadInt();
                uint revision = unchecked((uint)package.ReadInt());
                int bound = package.ReadInt();

                if (version != Version || hash == 0 || revision == 0 ||
                    (bound != 0 && bound != 1))
                    return;

                StateChanged?.Invoke(
                    sender,
                    hash,
                    revision,
                    bound == 1);
            }
            catch (Exception error)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [LeftNativeChangedReceiveFailed] " +
                    error.Message);
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
                        $"{Plugin.ModName}: [LeftNativeTransportRegisterFailed] " +
                        error.Message);
                }
            }
        }
    }
}
