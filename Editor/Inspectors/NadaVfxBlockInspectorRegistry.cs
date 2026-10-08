using NADA.VFX.Weapon.Core.State.Blocks;

namespace NADA.VFX.Weapon.Editor.Inspectors
{
    /// <summary>
    /// Routes a selected effect block to its strongly typed inspector.
    /// </summary>
    internal static class NadaVfxBlockInspectorRegistry
    {
        internal static bool Draw(VfxEffectBlock block)
        {
            if (block == null)
                return false;

            switch (block.TypeId)
            {
                case VfxEffectTypeIds.InnerFlames:
                    return NadaInnerFlamesInspector.Draw(block);

                case VfxEffectTypeIds.OuterFlames:
                    return NadaOuterFlamesInspector.Draw(block);

                case VfxEffectTypeIds.Strands:
                    return NadaStrandsInspector.Draw(block);

                case VfxEffectTypeIds.Sparks:
                    return NadaSparksInspector.Draw(block);

                case VfxEffectTypeIds.Flare:
                    return NadaFlareInspector.Draw(block);

                case VfxEffectTypeIds.Aura:
                    return NadaAuraInspector.Draw(block);

                case VfxEffectTypeIds.OrbitalsOrbs:
                    return NadaOrbitalsOrbsInspector.Draw(block);

                case VfxEffectTypeIds.OrbitalsCores:
                    return NadaOrbitalsCoresInspector.Draw(block);

                case VfxEffectTypeIds.OrbitalsFlames:
                    return NadaOrbitalsFlamesInspector.Draw(block);

                case VfxEffectTypeIds.OrbitalsEmbers:
                    return NadaOrbitalsEmbersInspector.Draw(block);

                default:
                    return NadaVfxFallbackBlockInspector.Draw(block);
            }
        }
    }
}