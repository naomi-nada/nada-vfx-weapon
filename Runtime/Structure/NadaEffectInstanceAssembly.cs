using System;
using System.Collections.Generic;
using NADA.VFX.Weapon.Core.Debug;
using NADA.VFX.Weapon.Core.State.Blocks;
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

            if (instanceId == 0)
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
                        existing.RootObject.SetActive(false);

                        if (existing.RootTransform != null)
                        {
                            // Destroy is deferred. Detach immediately so the
                            // stale instance no longer owns this runtime ID.
                            existing.RootTransform.SetParent(
                                null,
                                false);
                        }

                        Object.Destroy(
                            existing.RootObject);
                    }

                    // Fail closed for this pass. A later reconciliation can
                    // create the requested type after the stale owner is gone.
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

            instance.RootObject.SetActive(false);

            if (instance.RootTransform != null)
            {
                // Relinquish hierarchy ownership immediately even though
                // Unity will not destroy the object until end-of-frame.
                instance.RootTransform.SetParent(
                    null,
                    false);
            }

            Object.Destroy(
                instance.RootObject);

            return true;
        }

        internal static void ReconcileInstancesOfType(
            Transform localEffectsRootTransform,
            WeaponVfxState state,
            string typeId,
            string ownerNameForLogs)
        {
            if (localEffectsRootTransform == null)
                return;

            if (string.IsNullOrWhiteSpace(typeId))
                return;

            Transform instancesRootTransform =
                NadaRigPaths.FindDirectChild(
                    localEffectsRootTransform,
                    NadaRigPaths.EffectInstancesRootName);

            if (instancesRootTransform == null)
                return;

            var desiredIds =
                new HashSet<uint>();

            if (state?.Effects != null)
            {
                foreach (VfxEffectBlock block in state.Effects)
                {
                    if (block == null ||
                        block.InstanceId == 0)
                    {
                        continue;
                    }

                    if (!string.Equals(
                            block.TypeId,
                            typeId,
                            StringComparison.Ordinal))
                    {
                        continue;
                    }

                    desiredIds.Add(
                        block.InstanceId);
                }
            }

            var seenRuntimeIds =
                new HashSet<uint>();

            for (int i =
                     instancesRootTransform.childCount - 1;
                 i >= 0;
                 i--)
            {
                Transform child =
                    instancesRootTransform.GetChild(i);

                if (child == null)
                    continue;

                NadaEffectInstanceIdentity identity =
                    child.GetComponent<
                        NadaEffectInstanceIdentity>();

                if (identity == null)
                    continue;

                if (!string.Equals(
                        identity.TypeId,
                        typeId,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                bool duplicateRuntimeInstance =
                    !seenRuntimeIds.Add(
                        identity.InstanceId);

                bool stillDesired =
                    desiredIds.Contains(
                        identity.InstanceId);

                if (stillDesired &&
                    !duplicateRuntimeInstance)
                {
                    continue;
                }

                string reason =
                    duplicateRuntimeInstance
                        ? "duplicate-runtime"
                        : "not-in-state";

                uint removedInstanceId =
                    identity.InstanceId;

                string removedTypeId =
                    identity.TypeId;

                string rootName =
                    child.name;

                int rootId =
                    child.GetInstanceID();

                GameObject childObject =
                    child.gameObject;

                childObject.SetActive(false);

                // Destroy is deferred. Remove this object from Instances now
                // so another reconciliation pass cannot rediscover it.
                child.SetParent(
                    null,
                    false);

                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: [EffectInstanceRemoved] " +
                    $"owner='{ownerNameForLogs}' " +
                    $"id={removedInstanceId} " +
                    $"type='{removedTypeId}' " +
                    $"root='{rootName}' " +
                    $"rootId={rootId} " +
                    $"reason='{reason}'");

                Object.Destroy(
                    childObject);
            }
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