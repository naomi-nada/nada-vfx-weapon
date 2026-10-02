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

            if (localWeaponRootTransform == null ||
                weaponVisualRootTransform == null)
            {
                return null;
            }

            Transform localEffectsRootTransform =
                NadaRigPaths.FindLocalEffectsRoot(
                    localWeaponRootTransform);

            if (localEffectsRootTransform == null)
                return null;

            return EnsureAuraBranchInternal(
                localEffectsRootTransform,
                weaponVisualRootTransform,
                ownerNameForLogs,
                false);
        }

        internal static Transform EnsureAuraBranch(
            Transform parentTransform,
            Transform weaponVisualRootTransform,
            string ownerNameForLogs)
        {
            if (!NadaRigCache.CacheReady)
                return null;

            if (NadaRigCache.AuraMaterial == null)
                return null;

            if (parentTransform == null ||
                weaponVisualRootTransform == null)
            {
                return null;
            }

            return EnsureAuraBranchInternal(
                parentTransform,
                weaponVisualRootTransform,
                ownerNameForLogs,
                true);
        }

        internal static void RemoveDirectAuraBranch(
            Transform localEffectsRootTransform,
            string ownerNameForLogs)
        {
            if (localEffectsRootTransform == null)
                return;

            Transform auraRootTransform =
                NadaRigPaths.FindDirectChild(
                    localEffectsRootTransform,
                    Plugin.AuraName);

            if (auraRootTransform == null)
                return;

            int rootId =
                auraRootTransform.GetInstanceID();

            string rootName =
                auraRootTransform.name;

            // Destroy is deferred. Remove the old owner from the active
            // hierarchy immediately so block reconciliation cannot find or
            // render the direct Aura while its shells are waiting for destroy.
            auraRootTransform
                .gameObject
                .SetActive(false);

            auraRootTransform.SetParent(
                null,
                false);

            Object.Destroy(
                auraRootTransform.gameObject);

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [AuraBranchRemoved] " +
                $"owner='{ownerNameForLogs}' " +
                $"parent='{localEffectsRootTransform.name}' " +
                $"root='{rootName}' " +
                $"rootId={rootId} " +
                $"reason='ownership-change'");
        }

        private static Transform EnsureAuraBranchInternal(
            Transform parentTransform,
            Transform weaponVisualRootTransform,
            string ownerNameForLogs,
            bool blockOwned)
        {
            Transform auraRootTransform =
                NadaRigPaths.FindDirectChild(
                    parentTransform,
                    Plugin.AuraName);

            bool createdAura = false;

            if (auraRootTransform == null)
            {
                auraRootTransform =
                    NadaRigTransforms.EnsureChild(
                        parentTransform,
                        Plugin.AuraName);

                if (auraRootTransform == null)
                    return null;

                createdAura = true;

                if (blockOwned)
                {
                    Plugin.Log.LogInfo(
                        $"{Plugin.ModName}: [AuraBranchCreated] " +
                        $"owner='{ownerNameForLogs}' " +
                        $"parent='{parentTransform.name}' " +
                        $"root='{auraRootTransform.name}' " +
                        $"rootId={auraRootTransform.GetInstanceID()}");
                }
                else
                {
                    NadaLogControl.Info(
                        $"aura:{auraRootTransform.GetInstanceID()}",
                        $"{Plugin.ModName}: Added Aura branch under " +
                        $"'{NadaWeaponTargets.FullPath(parentTransform)}' " +
                        $"(owner='{ownerNameForLogs}').");
                }
            }

            if (createdAura)
            {
                // Structure owns the Aura's starting pose.
                // NadaAuraEffect owns it once the branch is running.
                auraRootTransform.localPosition =
                    Vector3.zero;

                auraRootTransform.localRotation =
                    Quaternion.identity;

                auraRootTransform.localScale =
                    Vector3.one;
            }

            bool hasAuraShells =
                HasAuraShells(
                    auraRootTransform);

            if (createdAura ||
                !hasAuraShells)
            {
                if (!createdAura)
                {
                    NadaLogControl.Info(
                        $"aura-repair:{auraRootTransform.GetInstanceID()}",
                        $"{Plugin.ModName}: [AuraRepair] " +
                        $"Aura branch existed without shells under " +
                        $"'{NadaWeaponTargets.FullPath(auraRootTransform)}'. " +
                        $"Rebuilding.");
                }

                BuildAuraShells(
                    auraRootTransform,
                    weaponVisualRootTransform);
            }

            return auraRootTransform;
        }

        private static bool HasAuraShells(
            Transform auraRootTransform)
        {
            if (auraRootTransform == null)
                return false;

            NadaAuraShell[] shells =
                auraRootTransform.GetComponentsInChildren<NadaAuraShell>(
                    true);

            if (shells == null ||
                shells.Length == 0)
            {
                return false;
            }

            foreach (NadaAuraShell shell in shells)
            {
                if (shell != null)
                    return true;
            }

            return false;
        }

        private static void BuildAuraShells(
            Transform auraRootTransform,
            Transform weaponVisualRootTransform)
        {
            if (auraRootTransform == null ||
                weaponVisualRootTransform == null)
            {
                return;
            }

            foreach (Transform child in
                     auraRootTransform.GetComponentsInChildren<Transform>(
                         true))
            {
                if (child == null ||
                    child == auraRootTransform)
                {
                    continue;
                }

                if (child.name.StartsWith(
                        "Aura Shell",
                        System.StringComparison.Ordinal))
                {
                    Object.Destroy(
                        child.gameObject);
                }
            }

            Transform nadaWeaponRoot =
                NadaRigPaths.FindDirectChild(
                    weaponVisualRootTransform,
                    Plugin.LocalWeaponRootName);

            MeshRenderer[] meshRenderers =
                weaponVisualRootTransform
                    .GetComponentsInChildren<MeshRenderer>(
                        true);

            int created = 0;

            foreach (MeshRenderer sourceRenderer in meshRenderers)
            {
                if (sourceRenderer == null)
                    continue;

                Transform sourceTransform =
                    sourceRenderer.transform;

                if (sourceTransform.name == "VFX")
                    continue;

                if (sourceTransform.IsChildOf(
                        auraRootTransform))
                {
                    continue;
                }

                if (nadaWeaponRoot != null &&
                    sourceTransform.IsChildOf(
                        nadaWeaponRoot))
                {
                    continue;
                }

                if (sourceTransform.name.StartsWith(
                        "Aura Shell",
                        System.StringComparison.Ordinal))
                {
                    continue;
                }

                MeshFilter sourceFilter =
                    sourceRenderer.GetComponent<MeshFilter>();

                if (sourceFilter == null ||
                    sourceFilter.sharedMesh == null)
                {
                    continue;
                }

                Mesh sourceMesh =
                    sourceFilter.sharedMesh;

                Vector3 meshCenter =
                    sourceMesh.bounds.center;

                NadaLogControl.Info(
                    $"aura-source:{sourceRenderer.GetInstanceID()}",
                    $"{Plugin.ModName}: [AuraSource] " +
                    $"renderer='{sourceRenderer.name}' " +
                    $"mesh='{sourceMesh.name}' " +
                    $"readable={sourceMesh.isReadable} " +
                    $"bounds={sourceMesh.bounds.size} " +
                    $"path='{NadaWeaponTargets.FullPath(sourceRenderer.transform)}'.");

                GameObject shellPivotObject =
                    new GameObject(
                        $"Aura Shell {created:00}");

                Transform shellPivotTransform =
                    shellPivotObject.transform;

                shellPivotTransform.SetParent(
                    auraRootTransform,
                    false);

                shellPivotTransform.position =
                    sourceTransform.TransformPoint(
                        meshCenter);

                shellPivotTransform.rotation =
                    sourceTransform.rotation;

                NadaRigTransforms.MatchWorldScale(
                    shellPivotTransform,
                    sourceTransform.lossyScale);

                GameObject shellMeshObject =
                    new GameObject(
                        "Aura Mesh");

                Transform shellMeshTransform =
                    shellMeshObject.transform;

                shellMeshTransform.SetParent(
                    shellPivotTransform,
                    false);

                shellMeshTransform.localPosition =
                    -meshCenter;

                shellMeshTransform.localRotation =
                    Quaternion.identity;

                shellMeshTransform.localScale =
                    Vector3.one;

                shellMeshObject.AddComponent<MeshFilter>();

                NadaAuraShell auraShell =
                    shellMeshObject.AddComponent<NadaAuraShell>();

                auraShell.Initialize(
                    sourceMesh);

                auraShell.SetBasePivotLocalScale(
                    shellPivotTransform.localScale);

                MeshRenderer shellRenderer =
                    shellMeshObject.AddComponent<MeshRenderer>();

                shellRenderer.enabled =
                    true;

                Material auraMaterial =
                    new Material(
                        NadaRigCache.AuraMaterial);

                auraShell.SetOwnedMaterial(
                    auraMaterial);

                Color tintColor =
                    auraMaterial.GetColor(
                        "_TintColor");

                tintColor.a =
                    0.05f;

                auraMaterial.SetColor(
                    "_TintColor",
                    tintColor);

                ConfigureAuraMaterial(
                    auraMaterial);

                int materialCount =
                    Mathf.Max(
                        1,
                        sourceMesh.subMeshCount);

                Material[] auraMaterials =
                    new Material[
                        materialCount];

                for (int i = 0;
                     i < auraMaterials.Length;
                     i++)
                {
                    auraMaterials[i] =
                        auraMaterial;
                }

                shellRenderer.sharedMaterials =
                    auraMaterials;

                shellRenderer.shadowCastingMode =
                    UnityEngine.Rendering
                        .ShadowCastingMode.Off;

                shellRenderer.receiveShadows =
                    false;

                shellRenderer.lightProbeUsage =
                    UnityEngine.Rendering
                        .LightProbeUsage.Off;

                shellRenderer.reflectionProbeUsage =
                    UnityEngine.Rendering
                        .ReflectionProbeUsage.Off;

                shellRenderer.allowOcclusionWhenDynamic =
                    false;

                created++;
            }

            if (created == 0)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [Aura] " +
                    $"no valid weapon MeshRenderer/MeshFilter pairs found under " +
                    $"'{NadaWeaponTargets.FullPath(weaponVisualRootTransform)}'.");
            }
        }

        private static void ConfigureAuraMaterial(
            Material material)
        {
            if (material == null)
                return;

            material.renderQueue =
                3500;

            if (material.HasProperty(
                    "_MainTex"))
            {
                material.SetTexture(
                    "_MainTex",
                    Texture2D.whiteTexture);
            }

            if (material.HasProperty(
                    "_EmissionMap"))
            {
                material.SetTexture(
                    "_EmissionMap",
                    Texture2D.whiteTexture);
            }

            if (material.HasProperty(
                    "_MaskTex"))
            {
                material.SetTexture(
                    "_MaskTex",
                    Texture2D.whiteTexture);
            }

            if (material.HasProperty(
                    "_Cutoff"))
            {
                material.SetFloat(
                    "_Cutoff",
                    0f);
            }

            if (material.HasProperty(
                    "_ZWrite"))
            {
                material.SetFloat(
                    "_ZWrite",
                    0f);
            }

            if (material.HasProperty(
                    "_ZTest"))
            {
                material.SetFloat(
                    "_ZTest",
                    (float)UnityEngine.Rendering
                        .CompareFunction.Always);
            }

            if (material.HasProperty(
                    "_Cull"))
            {
                material.SetFloat(
                    "_Cull",
                    (float)UnityEngine.Rendering
                        .CullMode.Off);
            }
        }
    }
}