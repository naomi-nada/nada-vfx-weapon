using System;

namespace NADA.VFX.Weapon.Core.Persistence
{
    // Only the dropped-object API may persist changes to a live world item.
    // ItemData alone doesn't establish who owns the object's ZDO.
    internal static class NadaDroppedItemPersistence
    {
        internal static bool TryResolveOwned(
            global::ItemDrop dropped,
            out global::ItemDrop.ItemData item,
            out global::ZDO zdo,
            out string reason)
        {
            item = null;
            zdo = null;
            reason = null;

            if (dropped == null || dropped.gameObject == null ||
                !dropped.gameObject.scene.IsValid() ||
                !dropped.gameObject.activeInHierarchy ||
                dropped.m_itemData == null)
            {
                reason = "No live dropped item to change.";
                return false;
            }

            var view = dropped.GetComponent<global::ZNetView>();
            if (view == null || !view.IsValid())
            {
                reason = "Dropped item has no valid network view.";
                return false;
            }

            if (!view.IsOwner())
            {
                reason = "This client doesn't own the dropped item's ZDO.";
                return false;
            }

            zdo = view.GetZDO();
            if (zdo == null)
            {
                reason = "Dropped item has no ZDO.";
                return false;
            }

            item = dropped.m_itemData;
            return true;
        }

        internal static bool TrySave(
            global::ItemDrop dropped,
            global::ItemDrop.ItemData expectedItem,
            global::ZDO expectedZdo,
            out string reason)
        {
            reason = null;
            if (!TryResolveOwned(dropped, out var currentItem,
                    out var currentZdo, out reason))
                return false;

            // A pending pickup or ownership handoff must not let us save a
            // different item or a replacement ZDO after validation.
            if (!ReferenceEquals(currentItem, expectedItem) ||
                !ReferenceEquals(currentZdo, expectedZdo))
            {
                reason = "Dropped item changed before it could be saved.";
                return false;
            }

            try
            {
                byte[] before = expectedZdo.GetByteArray(
                    global::ZDOVars.s_itemData, null);

                // Plain world drops use slot -1 in Valheim 1.0's packed
                // item serializer. No separate NADA keys or RPCs.
                global::ItemDrop.SaveToZDO(expectedItem, expectedZdo, -1);

                byte[] after = expectedZdo.GetByteArray(
                    global::ZDOVars.s_itemData, null);
                if (after == null || after.Length == 0)
                {
                    reason = "The dropped item's ZDO has no packed item data after saving.";
                    return false;
                }

                Plugin.Log?.LogInfo(
                    $"{Plugin.ModName}: [DroppedStateSaved] " +
                    $"object={dropped.GetInstanceID()} " +
                    $"beforeBytes={before?.Length ?? 0} " +
                    $"afterBytes={after.Length} " +
                    $"bytesChanged={!BytesEqual(before, after)}");
                return true;
            }
            catch (Exception error)
            {
                reason = "Valheim's dropped-item save failed: " +
                         error.GetBaseException().Message;
                return false;
            }
        }

        private static bool BytesEqual(byte[] a, byte[] b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a == null || b == null || a.Length != b.Length)
                return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                    return false;
            }
            return true;
        }
    }
}
