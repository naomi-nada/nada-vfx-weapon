using UnityEngine;
using NADA.VFX.Runtime.Binding;
using NADA.VFX.Core.State;
using NADA.VFX.Core.Visuals;
using NADA.VFX.Core.Config;

namespace NADA.VFX.Modules.Effects
{
    internal sealed class NadaAuraEffect : MonoBehaviour, INadaItemDataReceiver
    {
        private global::ItemDrop.ItemData _itemData;

        private Transform AuraSearchRoot =>
            transform.parent != null ? transform.parent : transform;

        public void SetItemData(global::ItemDrop.ItemData itemData)
        {
            _itemData = itemData;
        }

        private void Awake()
        {
            InvokeRepeating(nameof(TickApply), 0f, 0.05f);
        }

        private void OnDestroy()
        {
            try { CancelInvoke(nameof(TickApply)); } catch { }
        }

        private void TickApply()
        {
            VfxState state = ResolveState();

            ApplyEnabled(state.AuraEnabled);
            ApplyColor(state.AuraHue);
            ApplyScale(state.AuraScale);
        }

        private VfxState ResolveState()
        {
            if (_itemData != null && VfxStateIO.IsBound(_itemData))
            {
                if (VfxStateIO.TryRead(_itemData, out var itemState))
                    return itemState;
            }

            return VfxStateIO.FromConfig();
        }

        private void ApplyEnabled(bool enabled)
        {
            foreach (Renderer renderer in AuraSearchRoot.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null)
                    continue;

                if (!IsAuraRenderer(renderer.transform))
                    continue;

                renderer.enabled = enabled;
            }
        }

        private void ApplyColor(float hue)
        {
            float targetHue = NadaHueShiftUtility.SliderValueToTargetHue(hue);

            foreach (Renderer renderer in AuraSearchRoot.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null)
                    continue;

                if (!IsAuraRenderer(renderer.transform))
                    continue;

                Material material = renderer.material;
                if (material == null)
                    continue;

                if (material.HasProperty("_TintColor"))
                {
                    Color baseColor = new Color(0.925f, 0.157f, 0.953f, 0.03f);

                    material.SetColor(
                        "_TintColor",
                        NadaHueShiftUtility.RetintColorToHue(baseColor, targetHue));
                }
            }
        }

        private void ApplyScale(float scale)
        {
            float clampedScale = Mathf.Clamp(
                scale,
                PluginConfig.MinAuraScale,
                PluginConfig.MaxAuraScale);

            foreach (Transform shellTransform in AuraSearchRoot.GetComponentsInChildren<Transform>(true))
            {
                if (shellTransform == null)
                    continue;

                if (!shellTransform.name.StartsWith("Aura Shell", System.StringComparison.Ordinal))
                    continue;

                Transform sourceTransform = shellTransform.parent;
                if (sourceTransform == null)
                    continue;

                MeshFilter sourceFilter = sourceTransform.GetComponent<MeshFilter>();
                if (sourceFilter == null || sourceFilter.sharedMesh == null)
                    continue;

                shellTransform.localScale =
                    BuildAuraShellScale(sourceFilter.sharedMesh.bounds.size, clampedScale);
            }
        }

        private static bool IsAuraRenderer(Transform transform)
        {
            if (transform == null)
                return false;

            if (transform.name == "Aura Mesh")
                return true;

            Transform parent = transform.parent;
            return parent != null &&
                   parent.name.StartsWith("Aura Shell", System.StringComparison.Ordinal);
        }

        private static Vector3 BuildAuraShellScale(Vector3 meshSize, float scaleValue)
        {
            Vector3 scale = Vector3.one;

            int lengthAxis = GetLargestAxis(meshSize);

            if (lengthAxis != 0)
                scale.x = scaleValue;

            if (lengthAxis != 1)
                scale.y = scaleValue;

            if (lengthAxis != 2)
                scale.z = scaleValue;

            return scale;
        }

        private static int GetLargestAxis(Vector3 value)
        {
            if (value.x >= value.y && value.x >= value.z)
                return 0;

            if (value.y >= value.x && value.y >= value.z)
                return 1;

            return 2;
        }
    }
}