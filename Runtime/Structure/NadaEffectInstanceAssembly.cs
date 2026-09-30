using System;
using NADA.VFX.Weapon.Core.Debug;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NADA.VFX.Weapon.Runtime.Structure
{
    internal static class NadaEffectInstanceAssembly
    {
        internal static NadaEffectInstance EnsureInstance(
            Transform localEffectsRootTransform,
            uint instanceId,
            string typeId,
            string ownerNameForLogs)
        {
            if (localEffectsRootTransform == null)
                return null;

            if (string.IsNullOrWhiteSpace(typeId))
                return null;

            Transform instancesRootTransform =
                EnsureInstancesRoot(
                    localEffectsRootTransform);

            if (instancesRootTransform == null)
                return null;

            NadaEffectInstance existing =
                FindInstance(
                    instancesRootTransform,
                    instanceId);

            if (existing != null)
            {
                if (!string.Equals(
                        existing.TypeId,
                        typeId,
                        StringComparison.Ordinal))
                {
                    Plugin.Log.LogWarning(
                        $"{Plugin.ModName}: [EffectInstanceTypeMismatch] " +
                        $"owner='{ownerNameForLogs}' " +
                        $"id={instanceId} " +
                        $"existingType='{existing.TypeId}' " +
                        $"requestedType='{typeId}'. " +
                        $"Removing stale instance.");

                    if (existing.RootObject != null)
                    {
                        Object.Destroy(
                            existing.RootObject);
                    }

                    // Destroy is deferred by Unity. Fail closed for this
                    // pass instead of creating two owners for the same ID.
                    return null;
                }

                NormalizeInstanceRoot(
                    existing.RootTransform);

                NadaLogControl.Info(
                    $"effect-instance-reused:" +
                    $"{instancesRootTransform.GetInstanceID()}:" +
                    $"{instanceId}",
                    $"{Plugin.ModName}: [EffectInstanceReused] " +
                    $"owner='{ownerNameForLogs}' " +
                    $"id={instanceId} " +
                    $"type='{typeId}' " +
                    $"root='{existing.RootTransform.name}' " +
                    $"rootId={existing.RootTransform.GetInstanceID()}");

                return existing;
            }

            GameObject rootObject =
                new GameObject(
                    BuildDebugName(
                        instanceId,
                        typeId));

            Transform rootTransform =
                rootObject.transform;

            rootTransform.SetParent(
                instancesRootTransform,
                false);

            NormalizeInstanceRoot(
                rootTransform);

            NadaEffectInstanceIdentity identity =
                rootObject.AddComponent<
                    NadaEffectInstanceIdentity>();

            identity.Configure(
                instanceId,
                typeId);

            var created =
                new NadaEffectInstance(
                    identity);

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [EffectInstanceCreated] " +
                $"owner='{ownerNameForLogs}' " +
                $"id={instanceId} " +
                $"type='{typeId}' " +
                $"root='{rootTransform.name}' " +
                $"rootId={rootTransform.GetInstanceID()}");

            return created;
        }

        internal static NadaEffectInstance FindInstance(
            Transform instancesRootTransform,
            uint instanceId)
        {
            if (instancesRootTransform == null)
                return null;

            foreach (Transform child in instancesRootTransform)
            {
                if (child == null)
                    continue;

                NadaEffectInstanceIdentity identity =
                    child.GetComponent<
                        NadaEffectInstanceIdentity>();

                if (identity == null)
                    continue;

                if (identity.InstanceId != instanceId)
                    continue;

                return new NadaEffectInstance(
                    identity);
            }

            return null;
        }

        internal static bool RemoveInstance(
            Transform instancesRootTransform,
            uint instanceId)
        {
            NadaEffectInstance instance =
                FindInstance(
                    instancesRootTransform,
                    instanceId);

            if (instance == null ||
                instance.RootObject == null)
            {
                return false;
            }

            Object.Destroy(
                instance.RootObject);

            return true;
        }

        private static Transform EnsureInstancesRoot(
            Transform localEffectsRootTransform)
        {
            Transform instancesRootTransform =
                NadaRigPaths.FindDirectChild(
                    localEffectsRootTransform,
                    NadaRigPaths.EffectInstancesRootName);

            if (instancesRootTransform == null)
            {
                instancesRootTransform =
                    NadaRigTransforms.EnsureChild(
                        localEffectsRootTransform,
                        NadaRigPaths.EffectInstancesRootName);
            }

            if (instancesRootTransform == null)
                return null;

            instancesRootTransform.localPosition =
                Vector3.zero;

            instancesRootTransform.localRotation =
                Quaternion.identity;

            instancesRootTransform.localScale =
                Vector3.one;

            return instancesRootTransform;
        }

        private static void NormalizeInstanceRoot(
            Transform rootTransform)
        {
            if (rootTransform == null)
                return;

            rootTransform.localPosition =
                Vector3.zero;

            rootTransform.localRotation =
                Quaternion.identity;

            rootTransform.localScale =
                Vector3.one;
        }

        private static string BuildDebugName(
            uint instanceId,
            string typeId)
        {
            return
                $"Effect_{instanceId:D8}_{typeId}";
        }
    }
}