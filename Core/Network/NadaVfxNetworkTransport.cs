using System;
using System.Collections.Generic;
using HarmonyLib;
using NADA.VFX.Weapon.Core.State;

namespace NADA.VFX.Weapon.Core.Network
{
    internal static class NadaVfxNetworkTransport
    {
        private const string StateRequestRpcName =
            "naomi.nada.vfx.weapon.state_request_v2";

        private const string StateResponseRpcName =
            "naomi.nada.vfx.weapon.state_response_v2";

        private const string StateChangedRpcName =
            "naomi.nada.vfx.weapon.state_changed_v2";

        private const string NativeStateRequestRpcName =
            "naomi.nada.vfx.weapon.state_request_native_v1";

        private const string NativeStateResponseRpcName =
            "naomi.nada.vfx.weapon.state_response_native_v1";

        private const string NativeStateChangedRpcName =
            "naomi.nada.vfx.weapon.state_changed_native_v1";

        private const int TransportVersion = 1;

        private const int MaxControlPackageBytes = 128;

        private const int MaxStatePackageBytes =
            NadaVfxNetworkProtocol.MaxPacketBytes +
            128;

        private const int MaxNativeStatePackageBytes =
            WeaponVfxNetworkCodec.MaxPacketBytes +
            128;

        private static global::ZRoutedRpc _registeredRpc;

        // A peer becomes interested when it asks us for weapon state.
        // We only send tiny revision notifications to those peers.
        private static readonly HashSet<long>
            InterestedPeers = new();

        // Only native requesters receive native change notifications.
        // This keeps the new RPCs separate from legacy-only peers.
        private static readonly HashSet<long>
            NativeInterestedPeers = new();

        internal static event Action<
            long,
            NadaVfxNetworkPacket,
            VfxState>
            StateReceived;

        internal static event Action<
            long,
            int,
            uint,
            bool>
            StateChangedReceived;

        // Stage 8b validates/decodes these but deliberately has no runtime
        // subscriber. Stage 8c will add authoritative remote reconciliation.
        internal static event Action<long, WeaponVfxNetworkStatePacket>
            NativeStateReceived;

        internal static event Action<long, int, uint, bool>
            NativeStateChangedReceived;

        internal static event Action
            SessionReset;

        internal static void TryRegister()
        {
            global::ZRoutedRpc rpc =
                global::ZRoutedRpc.instance;

            if (rpc == null)
                return;

            if (ReferenceEquals(
                    _registeredRpc,
                    rpc))
            {
                return;
            }

            _registeredRpc =
                rpc;

            InterestedPeers.Clear();
            NativeInterestedPeers.Clear();

            NadaVfxLocalStatePublisher
                .ResetSession();

            rpc.Register(
                StateRequestRpcName,
                new Action<long, global::ZPackage>(
                    OnStateRequest));

            rpc.Register(
                StateResponseRpcName,
                new Action<long, global::ZPackage>(
                    OnStateResponse));

            rpc.Register(
                StateChangedRpcName,
                new Action<long, global::ZPackage>(
                    OnStateChanged));

            rpc.Register(
                NativeStateRequestRpcName,
                new Action<long, global::ZPackage>(
                    OnNativeStateRequest));

            rpc.Register(
                NativeStateResponseRpcName,
                new Action<long, global::ZPackage>(
                    OnNativeStateResponse));

            rpc.Register(
                NativeStateChangedRpcName,
                new Action<long, global::ZPackage>(
                    OnNativeStateChanged));

            SessionReset?.Invoke();

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [NetworkTransportRegistered]");
        }

        internal static bool RequestState(
            long targetPeerId,
            int itemHash,
            uint knownRevision)
        {
            if (targetPeerId == 0L ||
                itemHash == 0)
            {
                return false;
            }

            global::ZRoutedRpc rpc =
                global::ZRoutedRpc.instance;

            if (rpc == null ||
                !ReferenceEquals(
                    rpc,
                    _registeredRpc))
            {
                return false;
            }

            try
            {
                var package =
                    new global::ZPackage();

                package.Write(
                    TransportVersion);

                package.Write(
                    itemHash);

                package.Write(
                    unchecked(
                        (int)knownRevision));

                rpc.InvokeRoutedRPC(
                    targetPeerId,
                    StateRequestRpcName,
                    package);

                // Request native state in parallel; V2 remains the fallback.
                // A native RPC failure must not cancel a valid V2 request.
                try
                {
                    var nativeRequest = new global::ZPackage();
                    nativeRequest.Write(TransportVersion);
                    nativeRequest.Write(itemHash);
                    nativeRequest.Write(unchecked((int)knownRevision));
                    rpc.InvokeRoutedRPC(
                        targetPeerId,
                        NativeStateRequestRpcName,
                        nativeRequest);
                }
                catch (Exception nativeError)
                {
                    Plugin.Log.LogWarning(
                        $"{Plugin.ModName}: [NetworkNativeStateRequest FAIL] " +
                        $"target={targetPeerId} hash={itemHash} " +
                        $"error={nativeError}");
                }

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [NetworkStateRequest] " +
                    $"target={targetPeerId} " +
                    $"itemHash={itemHash} " +
                    $"knownRevision={knownRevision}");

                return true;
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [NetworkStateRequest FAIL] " +
                    $"target={targetPeerId} " +
                    $"itemHash={itemHash} " +
                    $"error={e}");

                return false;
            }
        }

        internal static void NotifyLocalStateChanged(
            NadaVfxLocalPublishedState publishedState)
        {
            if (publishedState == null)
                return;

            global::ZRoutedRpc rpc =
                global::ZRoutedRpc.instance;

            if (rpc == null ||
                !ReferenceEquals(
                    rpc,
                    _registeredRpc) ||
                (InterestedPeers.Count == 0 && NativeInterestedPeers.Count == 0))
            {
                return;
            }

            long[] peers =
                new long[
                    InterestedPeers.Count];

            InterestedPeers.CopyTo(
                peers);

            foreach (long peerId in peers)
            {
                if (peerId == 0L)
                    continue;

                try
                {
                    SendStateChanged(
                        rpc,
                        peerId,
                        publishedState);
                }
                catch (Exception e)
                {
                    Plugin.Log.LogWarning(
                        $"{Plugin.ModName}: [NetworkStateChanged FAIL] " +
                        $"target={peerId} " +
                        $"hash={publishedState.ItemHash} " +
                        $"revision={publishedState.Revision} " +
                        $"error={e}");
                }
            }

            long[] nativePeers = new long[NativeInterestedPeers.Count];
            NativeInterestedPeers.CopyTo(nativePeers);

            foreach (long peerId in nativePeers)
            {
                if (peerId == 0L)
                    continue;

                try
                {
                    SendNativeStateChanged(rpc, peerId, publishedState);
                }
                catch (Exception e)
                {
                    Plugin.Log.LogWarning(
                        $"{Plugin.ModName}: [NetworkNativeStateChanged FAIL] " +
                        $"target={peerId} " +
                        $"hash={publishedState.ItemHash} " +
                        $"revision={publishedState.Revision} " +
                        $"error={e}");
                }
            }
        }

        private static void OnStateRequest(
            long sender,
            global::ZPackage package)
        {
            try
            {
                if (sender == 0L ||
                    package == null)
                {
                    return;
                }

                int packageSize =
                    package.Size();

                if (packageSize <= 0 ||
                    packageSize >
                        MaxControlPackageBytes)
                {
                    return;
                }

                int version =
                    package.ReadInt();

                int requestedItemHash =
                    package.ReadInt();

                uint knownRevision =
                    unchecked(
                        (uint)package.ReadInt());

                if (version !=
                        TransportVersion ||
                    requestedItemHash == 0)
                {
                    return;
                }

                InterestedPeers.Add(
                    sender);

                if (!NadaVfxLocalStatePublisher
                        .TryGetCurrent(
                            out NadaVfxLocalPublishedState
                                publishedState))
                {
                    Plugin.Log.LogInfo(
                        $"{Plugin.ModName}: [NetworkStateRequestDeferred] " +
                        $"sender={sender} " +
                        $"requestedHash={requestedItemHash} " +
                        $"reason=no-local-state");

                    return;
                }

                // Never answer a request for weapon A with weapon B's state.
                // Equipment replication ordering can briefly make the two
                // clients disagree about what's visible.
                if (publishedState.ItemHash !=
                    requestedItemHash)
                {
                    Plugin.Log.LogInfo(
                        $"{Plugin.ModName}: [NetworkStateRequestDeferred] " +
                        $"sender={sender} " +
                        $"requestedHash={requestedItemHash} " +
                        $"currentHash={publishedState.ItemHash} " +
                        $"reason=item-mismatch");

                    return;
                }

                SendStateResponse(
                    sender,
                    publishedState);

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [NetworkStateRequestHandled] " +
                    $"sender={sender} " +
                    $"itemHash={requestedItemHash} " +
                    $"knownRevision={knownRevision} " +
                    $"sentRevision={publishedState.Revision}");
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [NetworkStateRequestReceive FAIL] {e}");
            }
        }

        private static void SendStateResponse(
            long targetPeerId,
            NadaVfxLocalPublishedState publishedState)
        {
            if (targetPeerId == 0L ||
                publishedState == null)
            {
                return;
            }

            byte[] bytes =
                publishedState.PacketBytes;

            if (bytes == null ||
                bytes.Length <= 0 ||
                bytes.Length >
                    NadaVfxNetworkProtocol.MaxPacketBytes)
            {
                return;
            }

            global::ZRoutedRpc rpc =
                global::ZRoutedRpc.instance;

            if (rpc == null ||
                !ReferenceEquals(
                    rpc,
                    _registeredRpc))
            {
                return;
            }

            var package =
                new global::ZPackage();

            package.Write(
                TransportVersion);

            package.Write(
                bytes.Length);

            package.Write(
                bytes);

            rpc.InvokeRoutedRPC(
                targetPeerId,
                StateResponseRpcName,
                package);

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [NetworkStateResponse] " +
                $"target={targetPeerId} " +
                $"itemHash={publishedState.ItemHash} " +
                $"revision={publishedState.Revision} " +
                $"bound={publishedState.Bound} " +
                $"records={publishedState.RecordCount} " +
                $"bytes={bytes.Length}");
        }

        private static void OnStateResponse(
            long sender,
            global::ZPackage package)
        {
            try
            {
                if (sender == 0L ||
                    package == null)
                {
                    return;
                }

                int packageSize =
                    package.Size();

                if (packageSize <= 0 ||
                    packageSize >
                        MaxStatePackageBytes)
                {
                    Plugin.Log.LogWarning(
                        $"{Plugin.ModName}: [NetworkStateReceiveReject] " +
                        $"sender={sender} " +
                        $"reason=package-size " +
                        $"bytes={packageSize}");

                    return;
                }

                int version =
                    package.ReadInt();

                if (version !=
                    TransportVersion)
                {
                    Plugin.Log.LogWarning(
                        $"{Plugin.ModName}: [NetworkStateReceiveReject] " +
                        $"sender={sender} " +
                        $"reason=transport-version " +
                        $"version={version}");

                    return;
                }

                int declaredLength =
                    package.ReadInt();

                if (declaredLength <= 0 ||
                    declaredLength >
                        NadaVfxNetworkProtocol.MaxPacketBytes)
                {
                    Plugin.Log.LogWarning(
                        $"{Plugin.ModName}: [NetworkStateReceiveReject] " +
                        $"sender={sender} " +
                        $"reason=declared-length " +
                        $"bytes={declaredLength}");

                    return;
                }

                byte[] bytes =
                    package.ReadByteArray();

                if (bytes == null ||
                    bytes.Length !=
                        declaredLength ||
                    bytes.Length >
                        NadaVfxNetworkProtocol.MaxPacketBytes)
                {
                    Plugin.Log.LogWarning(
                        $"{Plugin.ModName}: [NetworkStateReceiveReject] " +
                        $"sender={sender} " +
                        $"reason=payload-length " +
                        $"declared={declaredLength} " +
                        $"actual={bytes?.Length ?? 0}");

                    return;
                }

                if (!NadaVfxNetworkCodecV2
                        .TryDeserializeState(
                            bytes,
                            out NadaVfxNetworkPacket packet,
                            out VfxState state))
                {
                    Plugin.Log.LogWarning(
                        $"{Plugin.ModName}: [NetworkStateReceiveReject] " +
                        $"sender={sender} " +
                        $"reason=codec-decode " +
                        $"bytes={bytes.Length}");

                    return;
                }

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [NetworkStateReceive] " +
                    $"sender={sender} " +
                    $"itemHash={packet.ItemHash} " +
                    $"revision={packet.Revision} " +
                    $"bound={packet.Bound} " +
                    $"records={packet.Records.Count} " +
                    $"bytes={bytes.Length}");

                StateReceived?.Invoke(
                    sender,
                    packet,
                    state);
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [NetworkStateReceive FAIL] {e}");
            }
        }

        private static void SendStateChanged(
            global::ZRoutedRpc rpc,
            long targetPeerId,
            NadaVfxLocalPublishedState publishedState)
        {
            var package =
                new global::ZPackage();

            package.Write(
                TransportVersion);

            package.Write(
                publishedState.ItemHash);

            package.Write(
                unchecked(
                    (int)publishedState.Revision));

            package.Write(
                publishedState.Bound
                    ? 1
                    : 0);

            rpc.InvokeRoutedRPC(
                targetPeerId,
                StateChangedRpcName,
                package);

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [NetworkStateChanged] " +
                $"target={targetPeerId} " +
                $"itemHash={publishedState.ItemHash} " +
                $"revision={publishedState.Revision} " +
                $"bound={publishedState.Bound}");
        }

        private static void OnStateChanged(
            long sender,
            global::ZPackage package)
        {
            try
            {
                if (sender == 0L ||
                    package == null)
                {
                    return;
                }

                int packageSize =
                    package.Size();

                if (packageSize <= 0 ||
                    packageSize >
                        MaxControlPackageBytes)
                {
                    return;
                }

                int version =
                    package.ReadInt();

                int itemHash =
                    package.ReadInt();

                uint revision =
                    unchecked(
                        (uint)package.ReadInt());

                int boundValue =
                    package.ReadInt();

                if (version !=
                        TransportVersion ||
                    itemHash == 0 ||
                    (boundValue != 0 &&
                     boundValue != 1))
                {
                    return;
                }

                bool bound =
                    boundValue == 1;

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [NetworkStateChangedReceive] " +
                    $"sender={sender} " +
                    $"itemHash={itemHash} " +
                    $"revision={revision} " +
                    $"bound={bound}");

                StateChangedReceived?.Invoke(
                    sender,
                    itemHash,
                    revision,
                    bound);
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [NetworkStateChangedReceive FAIL] {e}");
            }
        }

        private static void OnNativeStateRequest(
            long sender,
            global::ZPackage package)
        {
            try
            {
                if (sender == 0L || package == null ||
                    package.Size() <= 0 ||
                    package.Size() > MaxControlPackageBytes)
                    return;

                int version = package.ReadInt();
                int itemHash = package.ReadInt();
                uint knownRevision = unchecked((uint)package.ReadInt());

                if (version != TransportVersion || itemHash == 0)
                    return;

                NativeInterestedPeers.Add(sender);

                if (!NadaVfxLocalStatePublisher.TryGetCurrent(
                        out NadaVfxLocalPublishedState published) ||
                    published.ItemHash != itemHash)
                {
                    Plugin.Log.LogInfo(
                        $"{Plugin.ModName}: [NetworkNativeStateRequestDeferred] " +
                        $"sender={sender} hash={itemHash} " +
                        $"reason=missing-or-mismatched-local-item");
                    return;
                }

                SendNativeStateResponse(sender, published);
                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [NetworkNativeStateRequestHandled] " +
                    $"sender={sender} hash={itemHash} " +
                    $"knownRevision={knownRevision} " +
                    $"sentRevision={published.Revision}");
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [NetworkNativeStateRequestReceive FAIL] {e}");
            }
        }

        private static void SendNativeStateResponse(
            long peerId,
            NadaVfxLocalPublishedState published)
        {
            if (peerId == 0L || published == null)
                return;

            byte[] bytes = published.NativePacketBytes;
            if (bytes == null || bytes.Length == 0 ||
                bytes.Length > WeaponVfxNetworkCodec.MaxPacketBytes)
                return;

            global::ZRoutedRpc rpc = global::ZRoutedRpc.instance;
            if (rpc == null || !ReferenceEquals(rpc, _registeredRpc))
                return;

            var package = new global::ZPackage();
            package.Write(TransportVersion);
            package.Write(bytes.Length);
            package.Write(bytes);

            rpc.InvokeRoutedRPC(peerId, NativeStateResponseRpcName, package);

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [NetworkNativeStateResponse] " +
                $"target={peerId} hash={published.ItemHash} " +
                $"revision={published.Revision} bound={published.NativeBound} " +
                $"blocks={published.NativeBlockCount} bytes={bytes.Length}");
        }

        private static void OnNativeStateResponse(
            long sender,
            global::ZPackage package)
        {
            try
            {
                if (sender == 0L || package == null)
                    return;

                int size = package.Size();
                if (size <= 0 || size > MaxNativeStatePackageBytes)
                {
                    LogNativeReject(sender, "package-size", size);
                    return;
                }

                int version = package.ReadInt();
                if (version != TransportVersion)
                {
                    LogNativeReject(sender, "transport-version", size);
                    return;
                }

                int declaredLength = package.ReadInt();
                if (declaredLength <= 0 ||
                    declaredLength > WeaponVfxNetworkCodec.MaxPacketBytes)
                {
                    LogNativeReject(sender, "declared-length", declaredLength);
                    return;
                }

                byte[] bytes = package.ReadByteArray();
                if (bytes == null || bytes.Length != declaredLength ||
                    bytes.Length > WeaponVfxNetworkCodec.MaxPacketBytes)
                {
                    LogNativeReject(sender, "payload-length", bytes?.Length ?? 0);
                    return;
                }

                if (!WeaponVfxNetworkCodec.TryDecode(
                        bytes,
                        out WeaponVfxNetworkStatePacket packet,
                        out string reason))
                {
                    Plugin.Log.LogWarning(
                        $"{Plugin.ModName}: [NetworkNativeStateReceiveReject] " +
                        $"sender={sender} reason={reason} bytes={bytes.Length}");
                    return;
                }

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [NetworkNativeStateReceive] " +
                    $"sender={sender} hash={packet.ItemHash} " +
                    $"revision={packet.Revision} bound={packet.Bound} " +
                    $"blocks={(packet.Bound ? packet.State.Effects.Count : 0)} " +
                    $"bytes={bytes.Length} mode=runtime-dispatch");

                NativeStateReceived?.Invoke(sender, packet);
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [NetworkNativeStateReceive FAIL] {e}");
            }
        }

        private static void SendNativeStateChanged(
            global::ZRoutedRpc rpc,
            long peerId,
            NadaVfxLocalPublishedState published)
        {
            if (rpc == null || published == null)
                return;

            var package = new global::ZPackage();
            package.Write(TransportVersion);
            package.Write(published.ItemHash);
            package.Write(unchecked((int)published.Revision));
            package.Write(published.NativeBound ? 1 : 0);

            rpc.InvokeRoutedRPC(peerId, NativeStateChangedRpcName, package);

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [NetworkNativeStateChanged] " +
                $"target={peerId} hash={published.ItemHash} " +
                $"revision={published.Revision} bound={published.NativeBound}");
        }

        private static void OnNativeStateChanged(
            long sender,
            global::ZPackage package)
        {
            try
            {
                if (sender == 0L || package == null ||
                    package.Size() <= 0 ||
                    package.Size() > MaxControlPackageBytes)
                    return;

                int version = package.ReadInt();
                int itemHash = package.ReadInt();
                uint revision = unchecked((uint)package.ReadInt());
                int boundValue = package.ReadInt();

                if (version != TransportVersion || itemHash == 0 ||
                    revision == 0 || (boundValue != 0 && boundValue != 1))
                    return;

                bool bound = boundValue == 1;
                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [NetworkNativeStateChangedReceive] " +
                    $"sender={sender} hash={itemHash} " +
                    $"revision={revision} bound={bound} mode=runtime-dispatch");

                NativeStateChangedReceived?.Invoke(
                    sender, itemHash, revision, bound);
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [NetworkNativeStateChangedReceive FAIL] {e}");
            }
        }

        private static void LogNativeReject(long sender, string reason, int size)
        {
            Plugin.Log.LogWarning(
                $"{Plugin.ModName}: [NetworkNativeStateReceiveReject] " +
                $"sender={sender} reason={reason} bytes={size}");
        }

        [HarmonyPatch(
            typeof(global::Game),
            "Start")]
        private static class GameStartPatch
        {
            private static void Prefix()
            {
                try
                {
                    TryRegister();
                }
                catch (Exception e)
                {
                    Plugin.Log.LogWarning(
                        $"{Plugin.ModName}: [NetworkTransportRegister FAIL] {e}");
                }
            }
        }
    }
}