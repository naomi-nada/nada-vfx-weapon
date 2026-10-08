using System;
using System.Collections.Generic;
using NADA.VFX.Weapon.Core.State.Blocks;

namespace NADA.VFX.Weapon.Core.Persistence
{
    internal enum WeaponVfxStateReadStatus
    {
        Missing,
        Valid,
        Invalid
    }

    /// <summary>
    /// Stores a native weapon state in ItemData, without knowing anything
    /// about runtime resolution, editor previews, config, or legacy state.
    /// Presence of PayloadKey is the native binding marker. A malformed
    /// payload is INVALID, never equivalent to an absent binding.
    /// </summary>
    internal static class WeaponVfxItemStore
    {
        internal const string PayloadKey = "nada.vfx.weapon.state.v1";

        // Matches the codec's 65,536-byte limit. Reject huge input before
        // allocating a byte[]; the codec still owns binary payload validation.
        private const int MaxBase64Characters = 4 * ((65536 + 2) / 3);

        internal static WeaponVfxStateReadStatus Read(
            global::ItemDrop.ItemData item,
            out WeaponVfxState state,
            out uint nextInstanceId,
            out string reason)
        {
            return Read(
                item?.m_customData,
                out state,
                out nextInstanceId,
                out reason);
        }

        // Dictionary seam also allows testing persistence independently of
        // Unity objects and actual Valheim inventory instances.
        internal static WeaponVfxStateReadStatus Read(
            Dictionary<string, string> customData,
            out WeaponVfxState state,
            out uint nextInstanceId,
            out string reason)
        {
            state = null;
            nextInstanceId = 0;
            reason = null;

            if (customData == null ||
                !customData.TryGetValue(PayloadKey, out string encoded))
            {
                return WeaponVfxStateReadStatus.Missing;
            }

            if (string.IsNullOrEmpty(encoded) ||
                encoded.Length > MaxBase64Characters ||
                encoded.Length % 4 != 0)
            {
                reason = "Empty, oversized, or incorrectly sized native payload.";
                return WeaponVfxStateReadStatus.Invalid;
            }

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(encoded);
            }
            catch (FormatException)
            {
                reason = "Native payload is not valid Base64.";
                return WeaponVfxStateReadStatus.Invalid;
            }

            // Convert.FromBase64String permits whitespace. Requiring canonical
            // encoding prevents multiple textual forms of the same binding.
            if (!string.Equals(
                    Convert.ToBase64String(bytes),
                    encoded,
                    StringComparison.Ordinal))
            {
                reason = "Native payload is not canonical Base64.";
                return WeaponVfxStateReadStatus.Invalid;
            }

            if (!WeaponVfxStateCodec.TryDecode(
                    bytes,
                    out WeaponVfxState decoded,
                    out uint decodedCursor,
                    out reason))
            {
                return WeaponVfxStateReadStatus.Invalid;
            }

            state = decoded;
            nextInstanceId = decodedCursor;
            return WeaponVfxStateReadStatus.Valid;
        }

        internal static bool TryWrite(
            global::ItemDrop.ItemData item,
            WeaponVfxState state,
            uint nextInstanceId,
            out string reason)
        {
            reason = null;
            if (item == null)
            {
                reason = "No target ItemData.";
                return false;
            }

            // Encode completely before touching ItemData. Failed validation
            // must never replace a previously saved weapon state.
            if (!TryPreparePayload(
                    state, nextInstanceId, out string encoded, out reason))
            {
                return false;
            }

            if (item.m_customData == null)
                item.m_customData = new Dictionary<string, string>();

            item.m_customData[PayloadKey] = encoded;
            return true;
        }

        internal static bool TryWrite(
            Dictionary<string, string> customData,
            WeaponVfxState state,
            uint nextInstanceId,
            out string reason)
        {
            reason = null;
            if (customData == null)
            {
                reason = "Missing customData dictionary.";
                return false;
            }

            if (!TryPreparePayload(
                    state, nextInstanceId, out string encoded, out reason))
            {
                return false;
            }

            customData[PayloadKey] = encoded;
            return true;
        }

        internal static bool Remove(global::ItemDrop.ItemData item)
        {
            return Remove(item?.m_customData);
        }

        internal static bool Remove(Dictionary<string, string> customData)
        {
            // Native removal must not erase another mod's data or the old
            // VfxState keys. Unbind migration will handle source precedence.
            return customData != null && customData.Remove(PayloadKey);
        }

        private static bool TryPreparePayload(
            WeaponVfxState state,
            uint nextInstanceId,
            out string encoded,
            out string reason)
        {
            encoded = null;
            if (!WeaponVfxStateCodec.TryEncode(
                    state, nextInstanceId, out byte[] bytes, out reason))
            {
                return false;
            }

            encoded = Convert.ToBase64String(bytes);
            return true;
        }
    }
}
