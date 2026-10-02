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

            var orbitalsMotion =
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

            var orbitalsMotion =
                GetOrAddMotion<NadaOrbitalsMotion>(
                    orbitalsMotionRootTransform);

            orbitalsMotion.Configure(
                family,
                state);

            orbitalsMotion.SetExternalVisualChain(
                externalHeadVisualTransform,
                externalFollowerPoolRootTransform);
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

            var outerFlamesMotion =
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

            var outerFlamesMotion =
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

            var targetFollowMotion =
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