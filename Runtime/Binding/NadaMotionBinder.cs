    using NADA.VFX.Weapon.Core.Debug;
    using NADA.VFX.Weapon.Core.State;
    using NADA.VFX.Weapon.Core.State.Blocks;
    using NADA.VFX.Weapon.Modules.Motion;
    using NADA.VFX.Weapon.Runtime.Formation;
    using UnityEngine;

    namespace NADA.VFX.Weapon.Runtime.Binding
    {
        internal static class NadaMotionBinder
        {
            internal static void BindOrbitalsMotion(
                Transform orbitalsMotionRootTransform,
                NadaOrbitalsFamily family,
                global::ItemDrop.ItemData itemData,
                Transform externalHeadVisualTransform,
                Transform externalFollowerPoolRootTransform)
            {
                if (orbitalsMotionRootTransform == null)
                    return;

                NadaOrbitalsMotion orbitalsMotion =
                    GetOrAddMotion<NadaOrbitalsMotion>(
                        orbitalsMotionRootTransform);

                orbitalsMotion.Configure(
                    family,
                    itemData);

                orbitalsMotion.SetExternalVisualChain(
                    externalHeadVisualTransform,
                    externalFollowerPoolRootTransform);
            }

            internal static void BindOrbitalsMotion(
                Transform orbitalsMotionRootTransform,
                NadaOrbitalsFamily family,
                VfxState state,
                Transform externalHeadVisualTransform,
                Transform externalFollowerPoolRootTransform)
            {
                if (orbitalsMotionRootTransform == null)
                    return;

                NadaOrbitalsMotion orbitalsMotion =
                    GetOrAddMotion<NadaOrbitalsMotion>(
                        orbitalsMotionRootTransform);

                orbitalsMotion.Configure(
                    family,
                    state);

                orbitalsMotion.SetExternalVisualChain(
                    externalHeadVisualTransform,
                    externalFollowerPoolRootTransform);
            }

            
            internal static bool BindOrbitalsOrbsBlockMotion(
                Transform orbitalsMotionRootTransform,
                Transform headVisualTransform,
                Transform followerPoolRootTransform,
                VfxEffectBlock block,
                ResolvedOrbitalsFormation resolved)
            {
                if (orbitalsMotionRootTransform == null)
                    return false;

                // Block-owned Orbs now use the shared block motion implementation.
                // Retire any transitional Orbs controller first so we never have
                // two motion components writing to the same visual chain.
                NadaOrbitalsMotion legacyMotion =
                    orbitalsMotionRootTransform
                        .GetComponent<NadaOrbitalsMotion>();

                if (legacyMotion != null)
                {
                    legacyMotion.enabled = false;

                    Object.Destroy(
                        legacyMotion);

                    Plugin.Log.LogInfo(
                        $"{Plugin.ModName}: [OrbitalsOrbsMotionRetired] " +
                        $"root='{orbitalsMotionRootTransform.name}' " +
                        $"reason='block-motion-ownership'");
                }

                bool accepted =
                    NadaOrbitalsBlockBinder.BindMotion(
                        orbitalsMotionRootTransform,
                        headVisualTransform,
                        followerPoolRootTransform,
                        block,
                        resolved);

                NadaLogControl.Info(
                    $"orbitals-orbs-motion-block-bind:" +
                    $"{orbitalsMotionRootTransform.GetInstanceID()}",
                    $"{Plugin.ModName}: [OrbitalsOrbsMotionBlockBind] " +
                    $"root='{orbitalsMotionRootTransform.name}' " +
                    $"id={(block != null ? block.InstanceId.ToString() : "null")} " +
                    $"mode={(resolved == null ? "invalid" : resolved.IsFollower ? "follower" : resolved.IsLeader ? "leader" : "independent")} " +
                    $"accepted={accepted}");

                return accepted;
            }
            

            internal static void ClearOrbitalsGlueSource(
                Transform dependentMotionRootTransform)
            {
                if (dependentMotionRootTransform == null)
                    return;

                NadaOrbitalsMotion dependentMotion =
                    dependentMotionRootTransform
                        .GetComponent<NadaOrbitalsMotion>();

                if (dependentMotion == null)
                    return;

                dependentMotion.ClearExplicitGlueSourceMotion();
            }

            internal static void BindOrbitalsRigFollow(
                Transform orbitalsRigRootTransform,
                Transform followTargetTransform,
                Vector3 localPositionOffset,
                Quaternion localRotationOffset)
            {
                BindTargetFollowInternal(
                    orbitalsRigRootTransform,
                    followTargetTransform,
                    localPositionOffset,
                    localRotationOffset);
            }

            internal static void BindOuterFlamesMotion(
                Transform outerFlamesRootTransform,
                global::ItemDrop.ItemData itemData)
            {
                if (outerFlamesRootTransform == null)
                    return;

                NadaOuterFlamesMotion outerFlamesMotion =
                    GetOrAddMotion<NadaOuterFlamesMotion>(
                        outerFlamesRootTransform);

                outerFlamesMotion.SetItemData(
                    itemData);
            }

            internal static void BindOuterFlamesMotion(
                Transform outerFlamesRootTransform,
                VfxState state)
            {
                if (outerFlamesRootTransform == null)
                    return;

                NadaOuterFlamesMotion outerFlamesMotion =
                    GetOrAddMotion<NadaOuterFlamesMotion>(
                        outerFlamesRootTransform);

                outerFlamesMotion.SetResolvedState(
                    state);
            }

            internal static void BindOuterFlamesBlockMotion(
                Transform outerFlamesRootTransform,
                VfxEffectBlock block)
            {
                if (outerFlamesRootTransform == null)
                    return;

                NadaOuterFlamesMotion outerFlamesMotion =
                    GetOrAddMotion<NadaOuterFlamesMotion>(
                        outerFlamesRootTransform);

                bool accepted =
                    outerFlamesMotion.SetBlockState(
                        block);

                NadaLogControl.Info(
                    $"outer-flames-motion-block-bind:{outerFlamesRootTransform.GetInstanceID()}",
                    $"{Plugin.ModName}: [OuterFlamesMotionBlockBind] " +
                    $"root='{outerFlamesRootTransform.name}' " +
                    $"id={(block != null ? block.InstanceId.ToString() : "null")} " +
                    $"accepted={accepted}");
            }

            private static void BindTargetFollowInternal(
                Transform targetFollowerTransform,
                Transform followTargetTransform,
                Vector3 localPositionOffset,
                Quaternion localRotationOffset)
            {
                if (targetFollowerTransform == null ||
                    followTargetTransform == null)
                {
                    return;
                }

                NadaTargetFollowMotion targetFollowMotion =
                    GetOrAddMotion<NadaTargetFollowMotion>(
                        targetFollowerTransform);

                targetFollowMotion.SetLocalOffset(
                    localPositionOffset,
                    localRotationOffset);

                targetFollowMotion.SetTargetTransform(
                    followTargetTransform);
            }

            private static TMotion GetOrAddMotion<TMotion>(
                Transform rootTransform)
                where TMotion : Component
            {
                TMotion motion =
                    rootTransform.GetComponent<TMotion>();

                if (motion == null)
                {
                    motion =
                        rootTransform.gameObject
                            .AddComponent<TMotion>();
                }

                return motion;
            }
        }
    }