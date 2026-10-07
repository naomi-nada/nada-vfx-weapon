using NADA.VFX.Weapon.Core.Config;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Defaults;
using UnityEngine;

namespace NADA.VFX.Weapon.Editor.Inspectors
{
    /// <summary>
    /// Inspector sections backed by state common to every VfxEffectBlock.
    /// </summary>
    internal static class NadaVfxCommonBlockInspector
    {
        internal static bool DrawTransform(
            VfxEffectBlock block)
        {
            if (block == null)
                return false;

            string sectionKey =
                $"block:{block.InstanceId}:transform";

            if (!NadaVfxEditorControls.Section(
                    sectionKey,
                    "TRANSFORM",
                    defaultExpanded: false))
            {
                NadaVfxEditorControls.SpaceAfterSection();

                return false;
            }

            if (block.Transform == null)
            {
                NadaVfxEditorControls.Placeholder(
                    "This block has no transform state.");

                NadaVfxEditorControls.SpaceAfterSection();

                return false;
            }

            bool changed =
                false;

            if (NadaVfxEditorControls.FloatSlider(
                    $"block:{block.InstanceId}:position-x",
                    "Position X",
                    block.Transform.XOffset,
                    PluginConfig.MinEffectOffset,
                    PluginConfig.MaxEffectOffset,
                    out float xOffset,
                    decimals: 3,
                    resetValue:
                        WeaponVfxBlockDefaults.DefaultEffectOffset))
            {
                block.Transform.XOffset =
                    xOffset;

                changed =
                    true;
            }

            if (NadaVfxEditorControls.FloatSlider(
                    $"block:{block.InstanceId}:position-y",
                    "Position Y",
                    block.Transform.YOffset,
                    PluginConfig.MinEffectOffset,
                    PluginConfig.MaxEffectOffset,
                    out float yOffset,
                    decimals: 3,
                    resetValue:
                        WeaponVfxBlockDefaults.DefaultEffectOffset))
            {
                block.Transform.YOffset =
                    yOffset;

                changed =
                    true;
            }

            if (NadaVfxEditorControls.FloatSlider(
                    $"block:{block.InstanceId}:position-z",
                    "Position Z",
                    block.Transform.ZOffset,
                    PluginConfig.MinEffectOffset,
                    PluginConfig.MaxEffectOffset,
                    out float zOffset,
                    decimals: 3,
                    resetValue:
                        WeaponVfxBlockDefaults.DefaultEffectOffset))
            {
                block.Transform.ZOffset =
                    zOffset;

                changed =
                    true;
            }

            GUILayout.Space(
                3f);

            if (NadaVfxEditorControls.FloatSlider(
                    $"block:{block.InstanceId}:rotation-x",
                    "Rotation X",
                    block.Transform.XRotation,
                    PluginConfig.MinEffectRotation,
                    PluginConfig.MaxEffectRotation,
                    out float xRotation,
                    decimals: 1,
                    resetValue:
                        WeaponVfxBlockDefaults.DefaultEffectRotation))
            {
                block.Transform.XRotation =
                    xRotation;

                changed =
                    true;
            }

            if (NadaVfxEditorControls.FloatSlider(
                    $"block:{block.InstanceId}:rotation-y",
                    "Rotation Y",
                    block.Transform.YRotation,
                    PluginConfig.MinEffectRotation,
                    PluginConfig.MaxEffectRotation,
                    out float yRotation,
                    decimals: 1,
                    resetValue:
                        WeaponVfxBlockDefaults.DefaultEffectRotation))
            {
                block.Transform.YRotation =
                    yRotation;

                changed =
                    true;
            }

            if (NadaVfxEditorControls.FloatSlider(
                    $"block:{block.InstanceId}:rotation-z",
                    "Rotation Z",
                    block.Transform.ZRotation,
                    PluginConfig.MinEffectRotation,
                    PluginConfig.MaxEffectRotation,
                    out float zRotation,
                    decimals: 1,
                    resetValue:
                        WeaponVfxBlockDefaults.DefaultEffectRotation))
            {
                block.Transform.ZRotation =
                    zRotation;

                changed =
                    true;
            }

            NadaVfxEditorControls.SpaceAfterSection();

            return changed;
        }
    }
}