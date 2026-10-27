
using System;
using NADA.VFX.Weapon.Weapons.Runtime;

namespace NADA.VFX.Weapon.Core.Network
{
    // Native left-hand publication only. The original right-hand protocols
    // remain unchanged for compatibility with already-working clients.
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

            internal PublishedState(int hash, uint generation, uint revision,
                bool bound, int blocks, byte[] bytes)
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

        // ZDO replication carries equipment identity separately from VFX state.
        // A token represents one equipped ItemData reference, not an effect.
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
            global::ItemDrop.ItemData item, int itemHash, global::ZDO zdo)
        {
            if (zdo == null)
                return;

            // Do not treat a temporary missing prefab hash as a new item.
            if (item != null && itemHash == 0)
                return;

            if (item == null)
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

            bool changed = !ReferenceEquals(item, _item) ||
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
                    _nextEquipmentGeneration = 1;

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [LeftNativeEquipmentGeneration] " +
                    $"hash={itemHash} generation={_equipmentGeneration}");
            }

            // Establish the item's identity on the ZDO before publishing.
            if (zdo.GetInt(EquipmentGenerationKey, 0) !=
                unchecked((int)_equipmentGeneration))
            {
                zdo.Set(EquipmentGenerationKey,
                    unchecked((int)_equipmentGeneration));
            }

            if (_dirty)
                Publish();
        }

        internal static void MarkDirty(global::ItemDrop.ItemData item)
        {
            if (item == null || !ReferenceEquals(item, _item))
                return;

            _dirty = true;

            if (_itemHash != 0)
                Publish();
        }

        internal static bool TryGetCurrent(out PublishedState state)
        {
            state = _published;
            return state != null;
        }

        private static void Publish()
        {
            if (_item == null || _itemHash == 0 ||
                _equipmentGeneration == 0)
                return;

            var source = NadaWeaponLocalSourceResolver.Resolve(_item);
            bool bound = source.Kind == NadaWeaponLocalSourceKind.NativeBound;

            uint revision = _nextRevision++;

            if (revision == 0)
                revision = _nextRevision++;

            if (_nextRevision == 0)
                _nextRevision = 1;

            if (!WeaponVfxNetworkCodec.TryEncode(
                    _itemHash, revision, bound,
                    bound ? source.State : null,
                    bound ? source.NextInstanceId : 0,
                    out byte[] bytes, out string failure))
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [LeftNativePublishRejected] " +
                    $"hash={_itemHash} reason='{failure}'");

                // Invalid state must never resurrect a previous effect.
                bound = false;

                if (!WeaponVfxNetworkCodec.TryEncode(
                        _itemHash, revision, false, null, 0,
                        out bytes, out failure))
                {
                    _published = null;
                    _dirty = true;
                    return;
                }
            }

            if (source.Kind == NadaWeaponLocalSourceKind.InvalidNative)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [LeftNativeStateInvalid] " +
                    $"hash={_itemHash} reason='{source.FailureReason}'");
            }

            _published = new PublishedState(
                _itemHash, _equipmentGeneration, revision, bound,
                bound ? source.State.Effects.Count : 0, bytes);

            _dirty = false;

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [LeftNativePublished] " +
                $"hash={_itemHash} generation={_equipmentGeneration} " +
                $"revision={revision} bound={bound} " +
                $"blocks={_published.BlockCount} bytes={bytes.Length}");

            NadaVfxLeftHandNetworkTransport.NotifyChanged(_published);
        }
    }
}
