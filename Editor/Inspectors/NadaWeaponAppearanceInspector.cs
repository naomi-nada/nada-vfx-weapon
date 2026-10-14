using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Blocks.Effects;
using NADA.VFX.Weapon.Core.State.Defaults;

namespace NADA.VFX.Weapon.Editor.Inspectors
{
    // This block changes the weapon mesh, not the rig's spatial effects.
    // The required common Transform stays at its default; it has no UI here.
    internal static class NadaWeaponAppearanceInspector
    {
        internal static bool Draw(VfxEffectBlock block)
        {
            if (block == null ||
                block.TypeId != VfxEffectTypeIds.WeaponAppearance)
                return false;

            if (block.Settings is not WeaponAppearanceVfxSettings appearance)
            {
                NadaVfxEditorControls.Placeholder(
                    "Invalid Weapon Appearance block: expected typed settings.");
                return false;
            }

            string prefix = $"appearance:{block.InstanceId}";
            bool changed = false;
            if (NadaVfxEditorControls.Section($"{prefix}:visual", "VISUAL"))
            {
                if (NadaVfxEditorControls.FloatSlider(
                        $"{prefix}:visibility",
                        "Visibility",
                        appearance.Visibility * 100f,
                        0f,
                        100f,
                        out float percent,
                        decimals: 1,
                        description:
                            "Weapon mesh visibility. 100 is original; 0 is invisible. Effects stay visible.",
                        resetValue:
                            WeaponVfxBlockDefaults.DefaultWeaponVisibility * 100f))
                {
                    appearance.Visibility = percent / 100f;
                    changed = true;
                }
            }

            NadaVfxEditorControls.SpaceAfterSection();
            return changed;
        }
    }
}
