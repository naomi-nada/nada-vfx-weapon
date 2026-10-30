
using System;
using NADA.VFX.Weapon.Weapons.Runtime;

namespace NADA.VFX.Weapon.Core.Network
{
    // Native left-hand publication only. Equipment identity lives in the
    // player's ZDO; the effect payload travels through the RPC transport.
    internal static class NadaVfxLeftHandStatePublisher
    {
        internal sealed class PublishedState
        {
            internal readonly int ItemHash;
            internal readonly uint EquipmentGeneration;
            internal readonly uint Revision;
            internal readonly bool Bound;
            internal readonly int BlockCount;
            internal readonly byte[] Bytes;

            internal PublishedState(
                int hash,
                uint generation,
                uint revision,
                bool bound,
                int blocks,
                byte[] bytes)
            {
                ItemHash = hash;
                EquipmentGeneration = generation;
                Revision = revision;
                Bound = bound;
                BlockCount = blocks;
                Bytes = bytes;
            }
        }

        private static global::ItemDrop.ItemData _item;

        // A generation identifies one equipped ItemData reference. It is
        // deliberately separate from the item's visual state revision.
        internal const string EquipmentGenerationKey =
            "nada.vfx.weapon.left.equip_generation.v1";

        private static int _itemHash;
        private static uint _equipmentGeneration;
        private static uint _nextEquipmentGeneration = 1;
        private static PublishedState _published;
        private static uint _nextRevision = 1;
        private static bool _dirty;

        internal static void ResetSession()
        {
            _item = null;
            _itemHash = 0;
            _equipmentGeneration = 0;
            _nextEquipmentGeneration = 1;
            _published = null;
            _nextRevision = 1;
            _dirty = false;
        }

        internal static void Observe(
            global::ItemDrop.ItemData item,
            int itemHash,
            global::ZDO zdo)
        {
            if (zdo == null)
                return;

            // Both missing means an empty hand. If ItemData and the
            // vanilla hash disagree, equipment is still transitioning.
            // Don't clear a valid generation during that transition.
            if (item == null && itemHash == 0)
            {
                _item = null;
                _itemHash = 0;
                _equipmentGeneration = 0;
                _published = null;
                _dirty = false;

                if (zdo.GetInt(EquipmentGenerationKey, 0) != 0)
                    zdo.Set(EquipmentGenerationKey, 0);

                return;
            }

            if (item == null || itemHash == 0)
                return;

            bool changed =
                !ReferenceEquals(item, _item) ||
                _itemHash != itemHash;

            if (changed)
            {
                _item = item;
                _itemHash = itemHash;
                _published = null;
                _dirty = true;

                _equipmentGeneration = _nextEquipmentGeneration++;

                if (_nextEquipmentGeneration == 0 ||
                    _nextEquipmentGeneration > int.MaxValue)
                {
                    _nextEquipmentGeneration = 1;
                }

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [LeftNativeEquipmentGeneration] " +
                    $"hash={itemHash} generation={_equipmentGeneration}");
            }

            // The observer must see the correct equipment generation
            // before accepting a newly published VFX state.
            if (zdo.GetInt(EquipmentGenerationKey, 0) !=
                unchecked((int)_equipmentGeneration))
            {
                zdo.Set(
                    EquipmentGenerationKey,
                    unchecked((int)_equipmentGeneration));
            }

            if (_dirty)
                Publish();
        }

        internal static void MarkDirty(
            global::ItemDrop.ItemData item)
        {
            // Never publish changes for a different equipped item.
            if (item == null ||
                !ReferenceEquals(item, _item))
            {
                return;
            }

            _dirty = true;

            if (_itemHash != 0)
                Publish();
        }

        internal static bool TryGetCurrent(
            out PublishedState state)
        {
            state = _published;
            return state != null;
        }

        private static void Publish()
        {
            if (_item == null ||
                _itemHash == 0 ||
                _equipmentGeneration == 0)
            {
                return;
            }

            NadaWeaponLocalSourceSelection source =
                NadaWeaponLocalSourceResolver.Resolve(_item);

            // Dirty can be raised by more than one caller for the same
            // persisted state. Compare the actual network representation
            // using the last published revision before allocating a new one.
            //
            // Generation is checked separately because it is transported
            // through the equipment identity protocol, not these bytes.
            if (_published != null &&
                _published.ItemHash == _itemHash &&
                _published.EquipmentGeneration == _equipmentGeneration &&
                TryEncodePacket(
                    source,
                    _published.Revision,
                    out bool previousBound,
                    out byte[] previousRevisionBytes,
                    out _,
                    out _) &&
                previousBound == _published.Bound &&
                PacketBytesEqual(
                    previousRevisionBytes,
                    _published.Bytes))
            {
                _dirty = false;
                return;
            }

            // Only a genuinely changed outgoing state receives a revision.
            uint revision = _nextRevision++;

            if (revision == 0)
                revision = _nextRevision++;

            if (_nextRevision == 0)
                _nextRevision = 1;

            if (!TryEncodePacket(
                    source,
                    revision,
                    out bool bound,
                    out byte[] bytes,
                    out string encodeFailure,
                    out string fallbackFailure))
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [LeftNativePublishRejected] " +
                    $"hash={_itemHash} " +
                    $"reason='{fallbackFailure ?? encodeFailure ?? "encode-failed"}'");

                // Never expose an older cached payload as the current state
                // after an unrecoverable encoding failure.
                _published = null;
                _dirty = true;
                return;
            }

            if (!string.IsNullOrEmpty(encodeFailure))
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [LeftNativePublishRejected] " +
                    $"hash={_itemHash} reason='{encodeFailure}'");
            }

            if (source.Kind == NadaWeaponLocalSourceKind.InvalidNative)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [LeftNativeStateInvalid] " +
                    $"hash={_itemHash} " +
                    $"reason='{source.FailureReason}'");
            }

            _published = new PublishedState(
                _itemHash,
                _equipmentGeneration,
                revision,
                bound,
                bound ? source.State.Effects.Count : 0,
                bytes);

            _dirty = false;

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [LeftNativePublished] " +
                $"hash={_itemHash} generation={_equipmentGeneration} " +
                $"revision={revision} bound={bound} " +
                $"blocks={_published.BlockCount} bytes={bytes.Length}");

            NadaVfxLeftHandNetworkTransport.NotifyChanged(
                _published);
        }

        private static bool TryEncodePacket(
            NadaWeaponLocalSourceSelection source,
            uint revision,
            out bool bound,
            out byte[] bytes,
            out string encodeFailure,
            out string fallbackFailure)
        {
            bound =
                source.Kind == NadaWeaponLocalSourceKind.NativeBound;

            fallbackFailure = null;

            if (WeaponVfxNetworkCodec.TryEncode(
                    _itemHash,
                    revision,
                    bound,
                    bound ? source.State : null,
                    bound ? source.NextInstanceId : 0,
                    out bytes,
                    out encodeFailure))
            {
                return true;
            }

            // Malformed/unsupported state must never be replaced with
            // effects from local configuration or a previous publication.
            // The remote presentation fails closed to an unbound packet.
            bound = false;

            return WeaponVfxNetworkCodec.TryEncode(
                _itemHash,
                revision,
                false,
                null,
                0,
                out bytes,
                out fallbackFailure);
        }

        private static bool PacketBytesEqual(
            byte[] first,
            byte[] second)
        {
            if (ReferenceEquals(first, second))
                return true;

            if (first == null ||
                second == null ||
                first.Length != second.Length)
            {
                return false;
            }

            for (int i = 0; i < first.Length; i++)
            {
                if (first[i] != second[i])
                    return false;
            }

            return true;
        }
    }
}
