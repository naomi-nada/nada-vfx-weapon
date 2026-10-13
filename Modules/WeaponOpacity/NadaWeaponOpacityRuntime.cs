
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace NADA.VFX.Weapon.Modules.WeaponOpacity
{
    /// <summary>
    /// Owns temporary transparency materials for one weapon visual.
    ///
    /// Lives on the NADA Weapon root, outside Effects/Instances.
    /// Original materials belong to Valheim or another mod; only
    /// materials created here may be destroyed by this component.
    /// </summary>
    internal sealed class NadaWeaponOpacityRuntime : MonoBehaviour
    {
        private sealed class RendererBinding
        {
            internal Renderer Renderer;
            internal Material[] Originals;
            internal Material[] Clones;
            internal ShadowCastingMode OriginalShadowCasting;

            internal RendererBinding(
                Renderer renderer,
                Material[] originals)
            {
                Renderer = renderer;
                Originals = originals;
                Clones = new Material[originals.Length];
                OriginalShadowCasting = renderer.shadowCastingMode;
            }
        }

        private readonly List<RendererBinding> _bindings =
            new List<RendererBinding>();

        private Transform _visualRoot;
        private float _appliedOpacity = 1f;

        // This is an explicit runtime entry point. The editor and
        // orchestrator can request opacity without owning materials.
        internal static bool TryApply(
            Transform rigRoot,
            Transform visualRoot,
            float opacity)
        {
            if (rigRoot == null ||
                rigRoot.name != Plugin.LocalWeaponRootName)
            {
                return false;
            }

            NadaWeaponOpacityRuntime runtime =
                rigRoot.GetComponent<NadaWeaponOpacityRuntime>();

            if (float.IsNaN(opacity) ||
                float.IsInfinity(opacity))
            {
                runtime?.RestoreOriginals();
                return false;
            }

            opacity = Mathf.Clamp01(opacity);

            // Normal opacity does not require replacement materials.
            if (opacity >= 1f)
            {
                runtime?.RestoreOriginals();
                return true;
            }

            if (visualRoot == null ||
                !rigRoot.IsChildOf(visualRoot) ||
                !rigRoot.gameObject.activeInHierarchy)
            {
                runtime?.RestoreOriginals();
                return false;
            }

            if (runtime == null)
            {
                runtime =
                    rigRoot.gameObject
                        .AddComponent<NadaWeaponOpacityRuntime>();
            }

            return runtime.ApplyOpacity(
                visualRoot,
                opacity);
        }

        // Called explicitly when a rig is retired, before Unity's
        // deferred Destroy completes.
        internal static void Restore(
            Transform rigRoot)
        {
            if (rigRoot == null)
                return;

            NadaWeaponOpacityRuntime runtime =
                rigRoot.GetComponent<NadaWeaponOpacityRuntime>();

            runtime?.RestoreOriginals();
        }

        private bool ApplyOpacity(
            Transform visualRoot,
            float opacity)
        {
            if (_visualRoot == visualRoot &&
                CanReuseBindings(visualRoot))
            {
                if (Mathf.Abs(_appliedOpacity - opacity) < 0.0001f)
                    return true;

                UpdateCloneAlpha(opacity);

                _appliedOpacity = opacity;

                Plugin.Log?.LogInfo(
                    $"{Plugin.ModName}: [WeaponOpacityUpdate] " +
                    $"visual='{visualRoot.name}' " +
                    $"opacity={opacity:0.###} " +
                    $"renderers={_bindings.Count}");

                return true;
            }

            RestoreOriginals();

            var prepared =
                new List<RendererBinding>();

            Renderer[] renderers =
                visualRoot.GetComponentsInChildren<Renderer>(true);

            // Validate every weapon mesh renderer before changing any
            // material. Mixed unsupported shaders must not produce a
            // partially transparent weapon.
            foreach (Renderer renderer in renderers)
            {
                if (!IsEligibleRenderer(renderer))
                    continue;

                Material[] originals =
                    renderer.sharedMaterials;

                if (originals == null ||
                    originals.Length == 0)
                {
                    LogUnsupported(
                        visualRoot,
                        renderer,
                        "<no-materials>");

                    return false;
                }

                foreach (Material material in originals)
                {
                    if (!SupportsStandardFade(material))
                    {
                        LogUnsupported(
                            visualRoot,
                            renderer,
                            material?.shader != null
                                ? material.shader.name
                                : "<missing-shader>");

                        return false;
                    }
                }

                prepared.Add(
                    new RendererBinding(
                        renderer,
                        originals));
            }

            if (prepared.Count == 0)
            {
                Plugin.Log?.LogWarning(
                    $"{Plugin.ModName}: [WeaponOpacityRejected] " +
                    $"visual='{visualRoot.name}' " +
                    $"reason='no-eligible-mesh-renderers'");

                return false;
            }

            try
            {
                // Create all owned resources before assigning any of
                // them to a live renderer.
                foreach (RendererBinding binding in prepared)
                {
                    for (int slot = 0;
                         slot < binding.Originals.Length;
                         slot++)
                    {
                        Material original =
                            binding.Originals[slot];

                        Material clone =
                            new Material(original)
                            {
                                name =
                                    $"NADA Opacity ({original.name})"
                            };

                        binding.Clones[slot] = clone;

                        ConfigureStandardFade(
                            clone,
                            original,
                            opacity);
                    }
                }

                // Track ownership before assignment, so even a failed
                // assignment can safely restore and destroy our clones.
                _bindings.AddRange(prepared);
                _visualRoot = visualRoot;

                foreach (RendererBinding binding in _bindings)
                {
                    binding.Renderer.sharedMaterials =
                        binding.Clones;

                    // A partially invisible weapon must not retain
                    // its fully opaque vanilla shadow.
                    binding.Renderer.shadowCastingMode =
                        ShadowCastingMode.Off;
                }

                _appliedOpacity = opacity;

                Plugin.Log?.LogInfo(
                    $"{Plugin.ModName}: [WeaponOpacityApplied] " +
                    $"visual='{visualRoot.name}' " +
                    $"opacity={opacity:0.###} " +
                    $"renderers={_bindings.Count} " +
                    $"shader='Standard'");

                return true;
            }
            catch (Exception exception)
            {
                // Includes prepared clones that may not yet have been
                // assigned to any renderer.
                RestoreBindings(prepared);

                _bindings.Clear();
                _visualRoot = null;
                _appliedOpacity = 1f;

                Plugin.Log?.LogWarning(
                    $"{Plugin.ModName}: [WeaponOpacityRejected] " +
                    $"reason='material-setup-exception' " +
                    $"error='{exception}'");

                return false;
            }
        }

        private bool IsEligibleRenderer(Renderer renderer)
        {
            if (renderer == null)
                return false;

            if (renderer is not MeshRenderer &&
                renderer is not SkinnedMeshRenderer)
            {
                return false;
            }

            // The component sits on the NADA Weapon root, which is
            // beneath the weapon visual. Anything beneath our own root
            // is an effect, not weapon geometry.
            if (renderer.transform.IsChildOf(transform))
                return false;

            return true;
        }

        private static bool SupportsStandardFade(
            Material material)
        {
            return material != null &&
                   material.shader != null &&
                   string.Equals(
                       material.shader.name,
                       "Standard",
                       StringComparison.Ordinal) &&
                   material.HasProperty("_Color") &&
                   material.HasProperty("_Mode") &&
                   material.HasProperty("_SrcBlend") &&
                   material.HasProperty("_DstBlend") &&
                   material.HasProperty("_ZWrite");
        }

        private static void ConfigureStandardFade(
            Material clone,
            Material original,
            float opacity)
        {
            // Built-in Standard shader Fade mode.
            // Use source-alpha blending rather than assuming the
            // original opaque render state supports transparency.
            clone.SetFloat("_Mode", 2f);

            clone.SetOverrideTag(
                "RenderType",
                "Transparent");

            clone.SetInt(
                "_SrcBlend",
                (int)BlendMode.SrcAlpha);

            clone.SetInt(
                "_DstBlend",
                (int)BlendMode.OneMinusSrcAlpha);

            clone.SetInt(
                "_ZWrite",
                0);

            clone.DisableKeyword("_ALPHATEST_ON");
            clone.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            clone.EnableKeyword("_ALPHABLEND_ON");

            clone.renderQueue =
                (int)RenderQueue.Transparent;

            Color color =
                original.GetColor("_Color");

            color.a *= opacity;

            clone.SetColor(
                "_Color",
                color);
        }

        private void UpdateCloneAlpha(float opacity)
        {
            foreach (RendererBinding binding in _bindings)
            {
                for (int slot = 0;
                     slot < binding.Clones.Length;
                     slot++)
                {
                    Material clone =
                        binding.Clones[slot];

                    Material original =
                        binding.Originals[slot];

                    if (clone == null ||
                        original == null)
                    {
                        continue;
                    }

                    Color color =
                        original.GetColor("_Color");

                    color.a *= opacity;

                    clone.SetColor(
                        "_Color",
                        color);
                }
            }
        }

        private bool CanReuseBindings(
            Transform visualRoot)
        {
            if (_bindings.Count == 0)
                return false;

            int eligibleCount = 0;

            Renderer[] renderers =
                visualRoot.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer renderer in renderers)
            {
                if (!IsEligibleRenderer(renderer))
                    continue;

                eligibleCount++;

                bool found = false;

                foreach (RendererBinding binding in _bindings)
                {
                    if (binding.Renderer == renderer)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                    return false;
            }

            if (eligibleCount != _bindings.Count)
                return false;

            foreach (RendererBinding binding in _bindings)
            {
                if (binding.Renderer == null)
                    return false;

                Material[] current =
                    binding.Renderer.sharedMaterials;

                if (current.Length != binding.Clones.Length)
                    return false;

                for (int slot = 0;
                     slot < current.Length;
                     slot++)
                {
                    if (current[slot] != binding.Clones[slot] ||
                        binding.Clones[slot] == null)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void RestoreOriginals()
        {
            if (_bindings.Count == 0)
            {
                _visualRoot = null;
                _appliedOpacity = 1f;
                return;
            }

            RestoreBindings(_bindings);

            _bindings.Clear();
            _visualRoot = null;
            _appliedOpacity = 1f;
        }

        private static void RestoreBindings(
            List<RendererBinding> bindings)
        {
            foreach (RendererBinding binding in bindings)
            {
                Renderer renderer =
                    binding.Renderer;

                if (renderer != null)
                {
                    // Restore only slots still referencing our clones.
                    // If another mod replaced a material after us,
                    // its new material must not be overwritten.
                    Material[] current =
                        renderer.sharedMaterials;

                    bool replacedAny = false;

                    int slots =
                        Mathf.Min(
                            current.Length,
                            binding.Clones.Length);

                    for (int slot = 0;
                         slot < slots;
                         slot++)
                    {
                        if (binding.Clones[slot] != null &&
                            current[slot] == binding.Clones[slot])
                        {
                            current[slot] =
                                binding.Originals[slot];

                            replacedAny = true;
                        }
                    }

                    if (replacedAny)
                    {
                        renderer.sharedMaterials =
                            current;

                        if (renderer.shadowCastingMode ==
                            ShadowCastingMode.Off)
                        {
                            renderer.shadowCastingMode =
                                binding.OriginalShadowCasting;
                        }
                    }
                }

                // These are always our own material instances.
                foreach (Material clone in binding.Clones)
                {
                    if (clone != null)
                        Destroy(clone);
                }
            }
        }

        private static void LogUnsupported(
            Transform visualRoot,
            Renderer renderer,
            string shaderName)
        {
            Plugin.Log?.LogInfo(
                $"{Plugin.ModName}: [WeaponOpacityUnsupported] " +
                $"visual='{visualRoot.name}' " +
                $"renderer='{renderer.name}' " +
                $"shader='{shaderName}' " +
                $"reason='requires-validated-transparency-path'");
        }

        private void OnDisable()
        {
            RestoreOriginals();
        }

        private void OnDestroy()
        {
            RestoreOriginals();
        }
    }
}
