
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace NADA.VFX.Weapon.Modules.WeaponOpacity
{
    // We only own the transparency clones. Valheim (or another mod)
    // still owns the original materials, so never destroy those.
    internal sealed class NadaWeaponOpacityRuntime : MonoBehaviour
    {
        private sealed class RendererBinding
        {
            internal Renderer Renderer;
            internal Material[] Originals;
            internal Material[] Clones;
            internal ShadowCastingMode OriginalShadowCasting;
            internal bool ChangedShadowCasting;

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
        private string _lastUnsupportedKey;
        private bool _lookedForLoadedStandardShader;

        // Shader.Find can miss Standard even when an item already uses it.
        // Borrow the shader reference, never someone else's material.
        private static Shader _loadedStandardShader;

        private const string CreatureShaderName = "Custom/Creature";
        private const string StandardShaderName = "Standard";

        // Callers request visibility here; this component owns the materials.
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

        // Destroy happens later, so put the original materials back now.
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

                return true;
            }

            RestoreOriginals();

            var prepared =
                new List<RendererBinding>();
            bool requiresCreatureBridge = false;

            Renderer[] renderers =
                visualRoot.GetComponentsInChildren<Renderer>(true);

            // Don't fade half a weapon if one of its materials can't fade.
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
                    if (SupportsStandardFade(material))
                        continue;

                    if (SupportsCreatureBridge(material))
                    {
                        requiresCreatureBridge = true;
                        continue;
                    }

                    LogUnsupported(
                        visualRoot,
                        renderer,
                        material?.shader != null
                            ? material.shader.name
                            : "<missing-shader>");

                    return false;
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

            // Creature can't use Standard's fade controls directly.
            // Use our own Standard clone and copy what the source exposes.
            Shader standardShader = null;
            if (requiresCreatureBridge)
            {
                standardShader = ResolveStandardShader();
                if (standardShader == null)
                {
                    LogUnsupported(
                        visualRoot,
                        null,
                        "Standard (no usable loaded shader)");
                    return false;
                }
            }

            try
            {
                // Prepare everything before touching a live renderer.
                foreach (RendererBinding binding in prepared)
                {
                    for (int slot = 0;
                         slot < binding.Originals.Length;
                         slot++)
                    {
                        Material original =
                            binding.Originals[slot];

                        bool needsBridge =
                            IsCreatureShader(original);

                        Material clone = needsBridge
                            ? new Material(standardShader)
                            : new Material(original);

                        // Track each clone right away in case mapping fails.
                        binding.Clones[slot] = clone;
                        clone.name = $"NADA Opacity ({original.name})";

                        if (needsBridge)
                            CopyCreatureProperties(original, clone);

                        ConfigureStandardFade(
                            clone,
                            original,
                            opacity);
                    }
                }

                // Keep our originals before swapping anything on the weapon.
                _bindings.AddRange(prepared);
                _visualRoot = visualRoot;

                foreach (RendererBinding binding in _bindings)
                {
                    binding.Renderer.sharedMaterials =
                        binding.Clones;

                    // Remember if we actually changed the shadow setting.
                    // A skin mod might replace our materials before cleanup.
                    binding.ChangedShadowCasting =
                        binding.OriginalShadowCasting != ShadowCastingMode.Off;
                    binding.Renderer.shadowCastingMode =
                        ShadowCastingMode.Off;
                }

                _appliedOpacity = opacity;
                _lastUnsupportedKey = null;

                if (requiresCreatureBridge)
                {
                    Plugin.Log?.LogInfo(
                        $"{Plugin.ModName}: [WeaponOpacityCreatureBridge] " +
                        $"visual='{visualRoot.name}' " +
                        $"source='{CreatureShaderName}' " +
                        $"replacement='{StandardShaderName}' " +
                        "status='prototype'");
                }

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

        private Shader ResolveStandardShader()
        {
            // Try the direct lookup first; it works on some installations.
            Shader direct = Shader.Find(StandardShaderName);
            if (direct != null && direct.isSupported)
                return direct;

            if (_loadedStandardShader != null &&
                _loadedStandardShader.isSupported)
            {
                return _loadedStandardShader;
            }

            // Don't scan loaded materials again for every slider step.
            if (_lookedForLoadedStandardShader)
                return null;

            _lookedForLoadedStandardShader = true;

            Material[] loaded =
                Resources.FindObjectsOfTypeAll<Material>();

            foreach (Material candidate in loaded)
            {
                if (!SupportsStandardFade(candidate) ||
                    !candidate.shader.isSupported)
                {
                    continue;
                }

                _loadedStandardShader = candidate.shader;

                Plugin.Log?.LogInfo(
                    $"{Plugin.ModName}: [WeaponOpacityShaderResolved] " +
                    "source='loaded-material' " +
                    $"material='{candidate.name}' " +
                    $"shader='{candidate.shader.name}'");

                return _loadedStandardShader;
            }

            Plugin.Log?.LogInfo(
                $"{Plugin.ModName}: [WeaponOpacityShaderMissing] " +
                "shader='Standard' " +
                "reason='no-loaded-compatible-material'");

            return null;
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

        private static bool IsCreatureShader(Material material)
        {
            return material != null &&
                   material.shader != null &&
                   string.Equals(
                       material.shader.name,
                       CreatureShaderName,
                       StringComparison.Ordinal);
        }

        private static bool SupportsCreatureBridge(Material material)
        {
            // Fail closed if we cannot preserve even the weapon's albedo.
            // Other Creature shader features may still differ; validate the
            // result visually before expanding this prototype's support.
            return IsCreatureShader(material) &&
                   material.HasProperty("_Color") &&
                   material.HasProperty("_MainTex") &&
                   material.GetTexture("_MainTex") != null;
        }

        private static void CopyCreatureProperties(
            Material original,
            Material clone)
        {
            // A new Standard material avoids carrying incompatible Creature
            // shader keywords and render-state overrides into the clone.
            CopyTexture(original, clone, "_MainTex", null);
            clone.SetColor("_Color", original.GetColor("_Color"));

            // Copy only shared, well-known property meanings. Valheim's
            // Creature shader has additional lighting behavior that cannot
            // automatically be reproduced by Standard.
            CopyTexture(original, clone, "_BumpMap", "_NORMALMAP");
            CopyTexture(original, clone, "_MetallicGlossMap", "_METALLICGLOSSMAP");
            CopyTexture(original, clone, "_OcclusionMap", null);
            CopyTexture(original, clone, "_EmissionMap", "_EMISSION");

            CopyFloat(original, clone, "_BumpScale");
            CopyFloat(original, clone, "_Metallic");
            CopyFloat(original, clone, "_Glossiness");
            CopyFloat(original, clone, "_OcclusionStrength");

            if (original.HasProperty("_EmissionColor") &&
                clone.HasProperty("_EmissionColor"))
            {
                Color emission = original.GetColor("_EmissionColor");
                clone.SetColor("_EmissionColor", emission);
                if (emission.maxColorComponent > 0f)
                    clone.EnableKeyword("_EMISSION");
            }
        }

        private static void CopyTexture(
            Material original,
            Material clone,
            string property,
            string keyword)
        {
            if (!original.HasProperty(property) ||
                !clone.HasProperty(property))
                return;

            Texture texture = original.GetTexture(property);
            if (texture == null)
                return;

            clone.SetTexture(property, texture);
            clone.SetTextureScale(
                property, original.GetTextureScale(property));
            clone.SetTextureOffset(
                property, original.GetTextureOffset(property));

            if (keyword != null)
                clone.EnableKeyword(keyword);
        }

        private static void CopyFloat(
            Material original,
            Material clone,
            string property)
        {
            if (original.HasProperty(property) &&
                clone.HasProperty(property))
            {
                clone.SetFloat(property, original.GetFloat(property));
            }
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
                    }

                    // Skin swaps can remove all our material clones first.
                    // Restore the shadow setting we changed either way.
                    if (binding.ChangedShadowCasting &&
                        renderer.shadowCastingMode == ShadowCastingMode.Off)
                    {
                        renderer.shadowCastingMode =
                            binding.OriginalShadowCasting;
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

        private void LogUnsupported(
            Transform visualRoot,
            Renderer renderer,
            string shaderName)
        {
            // Preview may retry every slider step. One rejection per
            // visual/renderer/shader is enough until the binding changes.
            string key =
                $"{visualRoot.GetInstanceID()}:" +
                $"{(renderer != null ? renderer.GetInstanceID() : 0)}:" +
                shaderName;

            if (_lastUnsupportedKey == key)
                return;

            _lastUnsupportedKey = key;
            Plugin.Log?.LogInfo(
                $"{Plugin.ModName}: [WeaponOpacityUnsupported] " +
                $"visual='{visualRoot.name}' " +
                $"renderer='{(renderer != null ? renderer.name : "<none>")}' " +
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
