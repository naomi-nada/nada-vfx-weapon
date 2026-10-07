using NADA.VFX.Weapon.Core.State.Blocks;

namespace NADA.VFX.Weapon.Editor.Inspectors
{
    /// <summary>
    /// Routes one selected effect block to its strongly typed editor
    /// inspector.
    ///
    /// Adding a new effect type should require one explicit registration here,
    /// not changes throughout the editor shell.
    /// </summary>
    internal static class NadaVfxBlockInspectorRegistry
    {
        internal static bool Draw(
            VfxEffectBlock block)
        {
            if (block == null)
                return false;

            switch (block.TypeId)
            {
                case VfxEffectTypeIds.InnerFlames:
                    return NadaInnerFlamesInspector.Draw(
                        block);

                case VfxEffectTypeIds.OuterFlames:
                    return NadaOuterFlamesInspector.Draw(
                        block);

                case VfxEffectTypeIds.Strands:
                    return NadaStrandsInspector.Draw(
                        block);

                case VfxEffectTypeIds.Sparks:
                    return NadaSparksInspector.Draw(
                        block);

                default:
                    return NadaVfxFallbackBlockInspector.Draw(
                        block);
            }
        }
    }
}