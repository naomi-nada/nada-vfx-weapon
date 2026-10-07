using System.Collections.Generic;
using NADA.VFX.Weapon.Core.State.Blocks;

namespace NADA.VFX.Weapon.Editor
{
    internal sealed class NadaVfxEditorEffectDefinition
    {
        internal string TypeId { get; }
        internal string PickerName { get; }
        internal string GeneratedNameBase { get; }
        internal bool IsOrbital { get; }

        internal NadaVfxEditorEffectDefinition(
            string typeId,
            string pickerName,
            string generatedNameBase,
            bool isOrbital)
        {
            TypeId =
                typeId;

            PickerName =
                pickerName;

            GeneratedNameBase =
                generatedNameBase;

            IsOrbital =
                isOrbital;
        }
    }

    internal static class NadaVfxEditorEffectCatalog
    {
        private static readonly List<NadaVfxEditorEffectDefinition>
            Definitions =
                new()
                {
                    new NadaVfxEditorEffectDefinition(
                        VfxEffectTypeIds.InnerFlames,
                        "Inner Flames",
                        "InnerFlames",
                        false),

                    new NadaVfxEditorEffectDefinition(
                        VfxEffectTypeIds.OuterFlames,
                        "Outer Flames",
                        "OuterFlames",
                        false),

                    new NadaVfxEditorEffectDefinition(
                        VfxEffectTypeIds.Strands,
                        "Strands",
                        "Strands",
                        false),

                    new NadaVfxEditorEffectDefinition(
                        VfxEffectTypeIds.Sparks,
                        "Sparks",
                        "Sparks",
                        false),

                    new NadaVfxEditorEffectDefinition(
                        VfxEffectTypeIds.Flare,
                        "Flare",
                        "Flare",
                        false),

                    new NadaVfxEditorEffectDefinition(
                        VfxEffectTypeIds.Aura,
                        "Aura",
                        "Aura",
                        false),

                    new NadaVfxEditorEffectDefinition(
                        VfxEffectTypeIds.OrbitalsOrbs,
                        "Orbs",
                        "Orbs",
                        true),

                    new NadaVfxEditorEffectDefinition(
                        VfxEffectTypeIds.OrbitalsCores,
                        "Cores",
                        "Cores",
                        true),

                    new NadaVfxEditorEffectDefinition(
                        VfxEffectTypeIds.OrbitalsFlames,
                        "Flames",
                        "Flames",
                        true),

                    new NadaVfxEditorEffectDefinition(
                        VfxEffectTypeIds.OrbitalsEmbers,
                        "Embers",
                        "Embers",
                        true)
                };

        internal static IReadOnlyList<NadaVfxEditorEffectDefinition>
            All =>
                Definitions;

        internal static NadaVfxEditorEffectDefinition Find(
            string typeId)
        {
            if (string.IsNullOrWhiteSpace(typeId))
                return null;

            foreach (NadaVfxEditorEffectDefinition definition in
                     Definitions)
            {
                if (definition.TypeId == typeId)
                    return definition;
            }

            return null;
        }
    }
}