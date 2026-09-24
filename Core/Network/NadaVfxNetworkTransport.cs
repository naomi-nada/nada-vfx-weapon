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

        private const int TransportVersion = 1;

        private const int MaxControlPackageBytes = 128;

        private const int MaxStatePackageBytes =
            NadaVfxNetworkProtocol.MaxPacketBytes +
            128;

        private static global::ZRoutedRpc _registeredRpc;

        // A peer becomes interested when it asks us for weapon state.
        // We only send tiny revision notifications to those peers.
        private static readonly HashSet<long>
            InterestedPeers = new();

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
                InterestedPeers.Count == 0)
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