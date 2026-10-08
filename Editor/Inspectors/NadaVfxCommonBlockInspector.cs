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
            VfxEffectBlock block,
            bool editable = true)
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

            // Followers borrow their leader's transform. Keep the stored
            // independent values visible but inactive until Glue is removed.
            bool previousEnabled = GUI.enabled;
            Color previousColor = GUI.color;
            if (!editable)
            {
                GUI.enabled = false;
                GUI.color = new Color(
                    previousColor.r,
                    previousColor.g,
                    previousColor.b,
                    previousColor.a * 0.78f);
            }

            if (NadaVfxEditorControls.FloatSlider(
                    $"block:{block.InstanceId}:xOffset",
                    "Position X",
                    block.Transform.XOffset,
                    PluginConfig.MinEffectOffset,
                    PluginConfig.MaxEffectOffset,
                    out float xOffset,
                    decimals: 3,
                    enabled: editable,
                    resetValue:
                        WeaponVfxBlockDefaults.DefaultEffectOffset))
            {
                block.Transform.XOffset =
                    xOffset;

                changed =
                    true;
            }

            if (NadaVfxEditorControls.FloatSlider(
                    $"block:{block.InstanceId}:yOffset",
                    "Position Y",
                    block.Transform.YOffset,
                    PluginConfig.MinEffectOffset,
                    PluginConfig.MaxEffectOffset,
                    out float yOffset,
                    decimals: 3,
                    enabled: editable,
                    resetValue:
                        WeaponVfxBlockDefaults.DefaultEffectOffset))
            {
                block.Transform.YOffset =
                    yOffset;

                changed =
                    true;
            }

            if (NadaVfxEditorControls.FloatSlider(
                    $"block:{block.InstanceId}:zOffset",
                    "Position Z",
                    block.Transform.ZOffset,
                    PluginConfig.MinEffectOffset,
                    PluginConfig.MaxEffectOffset,
                    out float zOffset,
                    decimals: 3,
                    enabled: editable,
                    resetValue:
                        WeaponVfxBlockDefaults.DefaultEffectOffset))
            {
                block.Transform.ZOffset =
                    zOffset;

                changed =
                    true;
            }

            if (NadaVfxEditorControls.FloatSlider(
                    $"block:{block.InstanceId}:xRotation",
                    "Rotation X",
                    block.Transform.XRotation,
                    PluginConfig.MinEffectRotation,
                    PluginConfig.MaxEffectRotation,
                    out float xRotation,
                    decimals: 1,
                    enabled: editable,
                    resetValue:
                        WeaponVfxBlockDefaults.DefaultEffectRotation))
            {
                block.Transform.XRotation =
                    xRotation;

                changed =
                    true;
            }

            if (NadaVfxEditorControls.FloatSlider(
                    $"block:{block.InstanceId}:yRotation",
                    "Rotation Y",
                    block.Transform.YRotation,
                    PluginConfig.MinEffectRotation,
                    PluginConfig.MaxEffectRotation,
                    out float yRotation,
                    decimals: 1,
                    enabled: editable,
                    resetValue:
                        WeaponVfxBlockDefaults.DefaultEffectRotation))
            {
                block.Transform.YRotation =
                    yRotation;

                changed =
                    true;
            }

            if (NadaVfxEditorControls.FloatSlider(
                    $"block:{block.InstanceId}:zRotation",
                    "Rotation Z",
                    block.Transform.ZRotation,
                    PluginConfig.MinEffectRotation,
                    PluginConfig.MaxEffectRotation,
                    out float zRotation,
                    decimals: 1,
                    enabled: editable,
                    resetValue:
                        WeaponVfxBlockDefaults.DefaultEffectRotation))
            {
                block.Transform.ZRotation =
                    zRotation;

                changed =
                    true;
            }

            GUI.enabled = previousEnabled;
            GUI.color = previousColor;

            NadaVfxEditorControls.SpaceAfterSection();

            return changed;
        }
    }
}