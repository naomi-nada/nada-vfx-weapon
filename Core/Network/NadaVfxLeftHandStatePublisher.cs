
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
            internal readonly uint Revision;
            internal readonly bool Bound;
            internal readonly int BlockCount;
            internal readonly byte[] Bytes;

            internal PublishedState(int hash, uint revision, bool bound,
                int blocks, byte[] bytes)
            {
                ItemHash = hash;
                Revision = revision;
                Bound = bound;
                BlockCount = blocks;
                Bytes = bytes;
            }
        }

        private static global::ItemDrop.ItemData _item;
        private static int _itemHash;
        private static PublishedState _published;
        private static uint _nextRevision = 1;
        private static bool _dirty;

        internal static void ResetSession()
        {
            _item = null;
            _itemHash = 0;
            _published = null;
            _nextRevision = 1;
            _dirty = false;
        }

        internal static void Observe(
            global::ItemDrop.ItemData item, int itemHash)
        {
            // Clearing the cache on a real unequip prevents answering a later
            // request using an earlier item with the same vanilla prefab hash.
            // A brief zero-hash transition while the item still exists is
            // ignored until vanilla has a usable identity again.
            if (item == null)
            {
                _item = null;
                _itemHash = 0;
                _published = null;
                _dirty = false;
                return;
            }

            if (itemHash == 0)
                return;

            if (!ReferenceEquals(item, _item) || _itemHash != itemHash)
            {
                _item = item;
                _itemHash = itemHash;
                _published = null;
                _dirty = true;
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
            if (_item == null || _itemHash == 0)
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
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [LeftNativeStateInvalid] " +
                    $"hash={_itemHash} reason='{source.FailureReason}'");

            _published = new PublishedState(
                _itemHash, revision, bound,
                bound ? source.State.Effects.Count : 0, bytes);
            _dirty = false;

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [LeftNativePublished] " +
                $"hash={_itemHash} revision={revision} bound={bound} " +
                $"blocks={_published.BlockCount} bytes={bytes.Length}");

            NadaVfxLeftHandNetworkTransport.NotifyChanged(_published);
        }
    }
}
