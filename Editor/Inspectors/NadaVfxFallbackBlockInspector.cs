using NADA.VFX.Weapon.Core.State.Blocks;

namespace NADA.VFX.Weapon.Editor.Inspectors
{
    /// <summary>
    /// Temporary editor presentation for block types whose typed inspector has
    /// not been migrated yet.
    ///
    /// This never attempts reflection-driven editing or guesses settings.
    /// </summary>
    internal static class NadaVfxFallbackBlockInspector
    {
        internal static bool Draw(
            VfxEffectBlock block)
        {
            if (block == null)
                return false;

            if (NadaVfxEditorControls.Section(
                    $"{block.TypeId}:visual",
                    "VISUAL"))
            {
                NadaVfxEditorControls.Placeholder(
                    "Typed controls for this effect have not been connected yet.");
            }

            NadaVfxEditorControls.SpaceAfterSection();

            if (IsOrbital(
                    block.TypeId))
            {
                if (NadaVfxEditorControls.Section(
                        $"{block.TypeId}:formation",
                        "FORMATION"))
                {
                    NadaVfxEditorControls.Placeholder(
                        "Formation and Glue controls will appear here when this inspector is connected.");
                }

                NadaVfxEditorControls.SpaceAfterSection();

                if (NadaVfxEditorControls.Section(
                        $"{block.TypeId}:motion",
                        "MOTION"))
                {
                    NadaVfxEditorControls.Placeholder(
                        "Independent orbital motion controls will live here.");
                }

                NadaVfxEditorControls.SpaceAfterSection();

            }

            bool isFollower =
                IsOrbital(block.TypeId) &&
                NadaOrbitalsGlueInspector.IsActiveFollower(block);

            return
                NadaVfxCommonBlockInspector.DrawTransform(
                    block,
                    editable: !isFollower);
        }

        private static bool IsOrbital(
            string typeId)
        {
            return
                typeId ==
                VfxEffectTypeIds.OrbitalsOrbs ||

                typeId ==
                VfxEffectTypeIds.OrbitalsCores ||

                typeId ==
                VfxEffectTypeIds.OrbitalsFlames ||

                typeId ==
                VfxEffectTypeIds.OrbitalsEmbers;
        }
    }
}