using System;
using NADA.VFX.Weapon.Core.State.Blocks.Effects;

namespace NADA.VFX.Weapon.Core.State.Blocks
{
    // Resolves appearance data only. The renderer/material runtime has one owner
    // and receives one final value, regardless of how many blocks exist.
    internal static class WeaponAppearanceResolver
    {
        internal static bool TryResolveVisibility(
            WeaponVfxState state,
            out float visibility)
        {
            visibility = 1f;
            if (state?.Effects == null)
                return false;

            bool hasAppearanceBlock = false;
            foreach (VfxEffectBlock block in state.Effects)
            {
                if (block == null ||
                    !string.Equals(block.TypeId,
                        VfxEffectTypeIds.WeaponAppearance,
                        StringComparison.Ordinal))
                    continue;

                hasAppearanceBlock = true;
                if (block.Settings is not WeaponAppearanceVfxSettings settings ||
                    !IsValid(settings.Visibility))
                    return false;

                // List position is priority. Disabled blocks don't contribute;
                // the last enabled block wins without a second order field.
                if (block.Enabled)
                    visibility = settings.Visibility;
            }

            if (hasAppearanceBlock)
                return true;

            // v3 scalar is read-only compatibility data. New authoring never
            // writes it; explicit draft migration converts it into a block.
            if (!IsValid(state.WeaponOpacity))
                return false;

            visibility = state.WeaponOpacity;
            return true;
        }

        private static bool IsValid(float value) =>
            !float.IsNaN(value) && !float.IsInfinity(value) &&
            value >= 0f && value <= 1f;
    }
}