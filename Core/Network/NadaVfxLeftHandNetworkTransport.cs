
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
        private static readonly HashSet<long> InterestedPeers = new();

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

            foreach (long peer in InterestedPeers)
            {
                if (peer == 0)
                    continue;
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

                InterestedPeers.Add(sender);
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

        private static void SendResponse(long peer,
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
                        bytes, out WeaponVfxNetworkStatePacket state,
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

                StateChanged?.Invoke(sender, hash, revision, bound == 1);
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
                try { TryRegister(); }
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
