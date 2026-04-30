using NADA.VFX.Modules.Motion;
using UnityEngine;

namespace NADA.VFX.Runtime.Binding
{
    internal static class NadaMotionBinder
    {
        internal static void BindOrbitalsMotion(
            Transform orbitalsFamilyRootTransform,
            NadaOrbitalsFamily family,
            global::ItemDrop.ItemData itemData)
        {
            if (orbitalsFamilyRootTransform == null)
                return;

            var orbitalsMotion = GetOrAddMotion<NadaOrbitalsMotion>(orbitalsFamilyRootTransform);
            orbitalsMotion.Configure(family, itemData);
        }

        internal static void BindOrbitalsMotion(
            Transform orbitalsMotionRootTransform,
            NadaOrbitalsFamily family,
            global::ItemDrop.ItemData itemData,
            Transform externalHeadVisualTransform,
            Transform externalFollowerPoolRootTransform)
        {
            if (orbitalsMotionRootTransform == null)
                return;

            var orbitalsMotion = GetOrAddMotion<NadaOrbitalsMotion>(orbitalsMotionRootTransform);
            orbitalsMotion.Configure(family, itemData);
            orbitalsMotion.SetExternalVisualChain(
                externalHeadVisualTransform,
                externalFollowerPoolRootTransform);
        }

        internal static void BindOrbsMotion(
            Transform orbsRootTransform,
            global::ItemDrop.ItemData itemData)
        {
            BindOrbitalsMotion(
                orbsRootTransform,
                NadaOrbitalsFamily.Orbs,
                itemData);
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

            var outerFlamesMotion = GetOrAddMotion<NadaOuterFlamesMotion>(outerFlamesRootTransform);
            outerFlamesMotion.SetItemData(itemData);
        }

        private static void BindTargetFollowInternal(
            Transform targetFollowerTransform,
            Transform followTargetTransform,
            Vector3 localPositionOffset,
            Quaternion localRotationOffset)
        {
            if (targetFollowerTransform == null || followTargetTransform == null)
                return;

            var targetFollowMotion = GetOrAddMotion<NadaTargetFollowMotion>(targetFollowerTransform);
            targetFollowMotion.SetLocalOffset(localPositionOffset, localRotationOffset);
            targetFollowMotion.SetTargetTransform(followTargetTransform);
        }

        private static TMotion GetOrAddMotion<TMotion>(Transform rootTransform)
            where TMotion : Component
        {
            TMotion motion = rootTransform.GetComponent<TMotion>();
            if (motion == null)
                motion = rootTransform.gameObject.AddComponent<TMotion>();

            return motion;
        }
    }
}