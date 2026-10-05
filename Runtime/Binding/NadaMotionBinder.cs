using NADA.VFX.Weapon.Core.Debug;
using NADA.VFX.Weapon.Core.State;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Modules.Motion;
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

        internal static void BindOrbitalsOrbsBlockMotion(
            Transform orbitalsMotionRootTransform,
            Transform headVisualTransform,
            Transform followerPoolRootTransform,
            VfxEffectBlock block)
        {
            if (orbitalsMotionRootTransform == null)
                return;

            NadaOrbitalsMotion orbitalsMotion =
                GetOrAddMotion<NadaOrbitalsMotion>(
                    orbitalsMotionRootTransform);

            orbitalsMotion.SetExternalVisualChain(
                headVisualTransform,
                followerPoolRootTransform);

            bool accepted =
                orbitalsMotion.ConfigureOrbitalsOrbsBlock(
                    block);

            NadaLogControl.Info(
                $"orbitals-orbs-motion-block-bind:{orbitalsMotionRootTransform.GetInstanceID()}",
                $"{Plugin.ModName}: [OrbitalsOrbsMotionBlockBind] " +
                $"root='{orbitalsMotionRootTransform.name}' " +
                $"id={(block != null ? block.InstanceId.ToString() : "null")} " +
                $"accepted={accepted}");
        }

        internal static void BindLegacyOrbitalsOrbsBlockMotion(
            Transform orbitalsRootTransform,
            VfxEffectBlock block)
        {
            if (orbitalsRootTransform == null)
                return;

            NadaOrbitalsMotion orbsMotion =
                null;

            // Transitional only. The legacy Orbitals subtree has one motion
            // component per family, so find the existing Orbs component here.
            // Once Orbs owns an instance root, binding must be direct instead.
            foreach (NadaOrbitalsMotion motion in
                     orbitalsRootTransform
                         .GetComponentsInChildren<NadaOrbitalsMotion>(
                             true))
            {
                if (motion == null ||
                    !motion.IsFamily(
                        NadaOrbitalsFamily.Orbs))
                {
                    continue;
                }

                orbsMotion =
                    motion;

                break;
            }

            if (orbsMotion == null)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [OrbitalsOrbsMotionBlockBind] " +
                    $"Could not find legacy Orbs motion under " +
                    $"'{orbitalsRootTransform.name}'.");

                return;
            }

            bool accepted =
                orbsMotion.SetOrbitalsOrbsBlockState(
                    block);

            NadaLogControl.Info(
                $"orbitals-orbs-motion-block-bind:{orbsMotion.GetInstanceID()}",
                $"{Plugin.ModName}: [OrbitalsOrbsMotionBlockBind] " +
                $"root='{orbsMotion.name}' " +
                $"id={(block != null ? block.InstanceId.ToString() : "null")} " +
                $"accepted={accepted}");
        }

        internal static void BindOrbitalsGlueSource(
            Transform dependentMotionRootTransform,
            Transform orbsSourceMotionRootTransform)
        {
            if (dependentMotionRootTransform == null)
                return;

            NadaOrbitalsMotion dependentMotion =
                dependentMotionRootTransform
                    .GetComponent<NadaOrbitalsMotion>();

            if (dependentMotion == null)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [OrbitalsGlueBind] " +
                    $"Dependent motion root " +
                    $"'{dependentMotionRootTransform.name}' " +
                    $"has no NadaOrbitalsMotion.");

                return;
            }

            if (dependentMotion.IsFamily(
                    NadaOrbitalsFamily.Orbs))
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [OrbitalsGlueBind] " +
                    $"Refusing to bind Orbs motion " +
                    $"'{dependentMotionRootTransform.name}' " +
                    $"as its own glue dependent.");

                return;
            }

            NadaOrbitalsMotion sourceMotion =
                null;

            if (orbsSourceMotionRootTransform != null)
            {
                sourceMotion =
                    orbsSourceMotionRootTransform
                        .GetComponent<NadaOrbitalsMotion>();

                if (sourceMotion == null ||
                    !sourceMotion.IsFamily(
                        NadaOrbitalsFamily.Orbs))
                {
                    Plugin.Log.LogWarning(
                        $"{Plugin.ModName}: [OrbitalsGlueBind] " +
                        $"Source motion root " +
                        $"'{orbsSourceMotionRootTransform.name}' " +
                        $"does not contain an Orbs motion.");

                    dependentMotion.SetGlueSourceMotion(
                        null);

                    return;
                }
            }

            dependentMotion.SetGlueSourceMotion(
                sourceMotion);

            NadaLogControl.Info(
                $"orbitals-glue-bind:{dependentMotionRootTransform.GetInstanceID()}",
                $"{Plugin.ModName}: [OrbitalsGlueBind] " +
                $"dependent='{dependentMotionRootTransform.name}' " +
                $"source='{(orbsSourceMotionRootTransform != null ? orbsSourceMotionRootTransform.name : "null")}'");
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