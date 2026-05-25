using UnityEngine;
using NADA.VFX.Weapon.Core.Debug;
using NADA.VFX.Weapon.Modules.Effects;
using NADA.VFX.Weapon.Weapons.Targets;

namespace NADA.VFX.Weapon.Runtime.Structure
{
    internal static class NadaAuraRigAssembly
    {
        internal static Transform EnsureLocalAuraBranch(
            Transform localWeaponRootTransform,
            Transform weaponVisualRootTransform,
            string ownerNameForLogs)
        {
            if (!NadaRigCache.CacheReady)
                return null;

            if (NadaRigCache.AuraMaterial == null)
                return null;

            if (localWeaponRootTransform == null || weaponVisualRootTransform == null)
                return null;

            Transform localEffectsRootTransform =
                NadaRigPaths.FindLocalEffectsRoot(localWeaponRootTransform);

            if (localEffectsRootTransform == null)
                return null;

            Transform auraRootTransform =
                NadaRigPaths.FindDirectChild(localEffectsRootTransform, Plugin.AuraName);

            bool createdAura = false;

            if (auraRootTransform == null)
            {
                auraRootTransform =
                    NadaRigTransforms.EnsureChild(localEffectsRootTransform, Plugin.AuraName);

                createdAura = true;

                NadaLogControl.Info(
                    $"aura:{auraRootTransform.GetInstanceID()}",
                    $"{Plugin.ModName}: Added Aura branch under '{NadaWeaponTargets.FullPath(localEffectsRootTransform)}' " +
                    $"(owner='{ownerNameForLogs}').");
            }

            auraRootTransform.localPosition = Vector3.zero;
            auraRootTransform.localRotation = Quaternion.identity;
            auraRootTransform.localScale = Vector3.one;

            if (createdAura)
                BuildAuraShells(auraRootTransform, weaponVisualRootTransform);

            return auraRootTransform;
        }

        private static void BuildAuraShells(
            Transform auraRootTransform,
            Transform weaponVisualRootTransform)
        {
            if (auraRootTransform == null || weaponVisualRootTransform == null)
                return;

            foreach (Transform child in weaponVisualRootTransform.GetComponentsInChildren<Transform>(true))
            {
                if (child != null &&
                    child.name.StartsWith("Aura Shell", System.StringComparison.Ordinal))
                {
                    Object.Destroy(child.gameObject);
                }
            }

            Transform nadaWeaponRoot =
                NadaRigPaths.FindDirectChild(weaponVisualRootTransform, Plugin.LocalWeaponRootName);

            MeshRenderer[] meshRenderers =
                weaponVisualRootTransform.GetComponentsInChildren<MeshRenderer>(true);

            int created = 0;

            foreach (MeshRenderer sourceRenderer in meshRenderers)
            {
                if (sourceRenderer == null)
                    continue;

                Transform sourceTransform = sourceRenderer.transform;

                if (sourceTransform.name == "VFX")
                    continue;

                if (sourceTransform.IsChildOf(auraRootTransform))
                    continue;

                if (nadaWeaponRoot != null && sourceTransform.IsChildOf(nadaWeaponRoot))
                    continue;

                if (sourceTransform.name.StartsWith("Aura Shell", System.StringComparison.Ordinal))
                    continue;

                MeshFilter sourceFilter = sourceRenderer.GetComponent<MeshFilter>();
                if (sourceFilter == null || sourceFilter.sharedMesh == null)
                    continue;

                Mesh sourceMesh = sourceFilter.sharedMesh;
                Vector3 meshCenter = sourceMesh.bounds.center;

                NadaLogControl.Info(
                    $"aura-source:{sourceRenderer.GetInstanceID()}",
                    $"{Plugin.ModName}: [AuraSource] " +
                    $"renderer='{sourceRenderer.name}' " +
                    $"mesh='{sourceMesh.name}' " +
                    $"readable={sourceMesh.isReadable} " +
                    $"bounds={sourceMesh.bounds.size} " +
                    $"path='{NadaWeaponTargets.FullPath(sourceRenderer.transform)}'.");

                GameObject shellPivotObject = new GameObject($"Aura Shell {created:00}");
                Transform shellPivotTransform = shellPivotObject.transform;

                shellPivotTransform.SetParent(auraRootTransform, false);
                shellPivotTransform.position = sourceTransform.TransformPoint(meshCenter);
                shellPivotTransform.rotation = sourceTransform.rotation;

                NadaRigTransforms.MatchWorldScale(
                    shellPivotTransform,
                    sourceTransform.lossyScale);

                GameObject shellMeshObject = new GameObject("Aura Mesh");
                Transform shellMeshTransform = shellMeshObject.transform;

                shellMeshTransform.SetParent(shellPivotTransform, false);
                shellMeshTransform.localPosition = -meshCenter;
                shellMeshTransform.localRotation = Quaternion.identity;
                shellMeshTransform.localScale = Vector3.one;

                shellMeshObject.AddComponent<MeshFilter>();

                NadaAuraShell auraShell =
                    shellMeshObject.AddComponent<NadaAuraShell>();

                auraShell.Initialize(sourceMesh);
                auraShell.SetBasePivotLocalScale(shellPivotTransform.localScale);

                MeshRenderer shellRenderer = shellMeshObject.AddComponent<MeshRenderer>();
                shellRenderer.enabled = true;

                Material auraMaterial = new Material(NadaRigCache.AuraMaterial);

                Color tintColor = auraMaterial.GetColor("_TintColor");
                tintColor.a = 0.05f;
                auraMaterial.SetColor("_TintColor", tintColor);

                ConfigureAuraMaterial(auraMaterial);

                int materialCount = Mathf.Max(1, sourceMesh.subMeshCount);
                Material[] auraMaterials = new Material[materialCount];

                for (int i = 0; i < auraMaterials.Length; i++)
                    auraMaterials[i] = auraMaterial;

                shellRenderer.sharedMaterials = auraMaterials;

                shellRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                shellRenderer.receiveShadows = false;
                shellRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                shellRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                shellRenderer.allowOcclusionWhenDynamic = false;

                created++;
            }

            if (created == 0)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [Aura] no valid weapon MeshRenderer/MeshFilter pairs found under '{NadaWeaponTargets.FullPath(weaponVisualRootTransform)}'.");
            }
        }

        private static void ConfigureAuraMaterial(Material material)
        {
            if (material == null)
                return;

            material.renderQueue = 3500;

            if (material.HasProperty("_MainTex"))
                material.SetTexture("_MainTex", Texture2D.whiteTexture);

            if (material.HasProperty("_EmissionMap"))
                material.SetTexture("_EmissionMap", Texture2D.whiteTexture);

            if (material.HasProperty("_MaskTex"))
                material.SetTexture("_MaskTex", Texture2D.whiteTexture);

            if (material.HasProperty("_Cutoff"))
                material.SetFloat("_Cutoff", 0f);

            if (material.HasProperty("_ZWrite"))
                material.SetFloat("_ZWrite", 0f);

            if (material.HasProperty("_ZTest"))
                material.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);

            if (material.HasProperty("_Cull"))
                material.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Off);
        }
    }
}