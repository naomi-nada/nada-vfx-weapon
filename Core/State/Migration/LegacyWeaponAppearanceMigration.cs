using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Blocks.Effects;
using NADA.VFX.Weapon.Core.State.Defaults;

namespace NADA.VFX.Weapon.Core.State.Migration
{
    // Only call at a deliberate editable-draft transition (Unbind or Import).
    // Never rewrite persisted ItemData, remote replication, or runtime state.
    internal static class LegacyWeaponAppearanceMigration
    {
        internal static bool TryPromote(
            WeaponVfxState draft,
            ref uint nextInstanceId,
            out string reason)
        {
            reason = null;
            if (draft?.Effects == null)
            {
                reason = "Weapon Appearance migration has no draft.";
                return false;
            }

            if (float.IsNaN(draft.WeaponOpacity) ||
                float.IsInfinity(draft.WeaponOpacity) ||
                draft.WeaponOpacity < 0f || draft.WeaponOpacity > 1f)
            {
                reason = "Invalid legacy weapon opacity.";
                return false;
            }

            foreach (VfxEffectBlock block in draft.Effects)
            {
                if (block?.TypeId != VfxEffectTypeIds.WeaponAppearance)
                    continue;

                // A native Appearance block always outranks the transitional
                // scalar. Clearing the scalar prevents a deleted block from
                // reviving an old opacity override.
                draft.WeaponOpacity = 1f;
                return true;
            }

            if (draft.WeaponOpacity >= 1f)
                return true;

            // The existing v3 codec has a maximum of 128 effect blocks.
            // Reject before Unbind retires the persisted source.
            if (draft.Effects.Count >= 128)
            {
                reason = "No room for a migrated Weapon Appearance block.";
                return false;
            }

            // Reserve the allocated identity and advance its cursor, just
            // like the editor's normal Add Effect action.
            if (nextInstanceId == 0 || nextInstanceId == uint.MaxValue)
            {
                reason = "No InstanceId available for legacy appearance migration.";
                return false;
            }

            VfxEffectBlock appearance = WeaponVfxBlockDefaults.Create(
                VfxEffectTypeIds.WeaponAppearance, nextInstanceId);
            if (appearance?.Settings is not WeaponAppearanceVfxSettings settings)
            {
                reason = "Could not create Weapon Appearance block.";
                return false;
            }

            settings.Visibility = draft.WeaponOpacity;
            appearance.DisplayName = $"Appearance{nextInstanceId:000}";
            draft.Effects.Add(appearance);
            draft.WeaponOpacity = 1f;
            nextInstanceId++;
            return true;
        }
    }
}
