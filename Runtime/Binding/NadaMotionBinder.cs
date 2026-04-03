using NADA.VFX.Modules.Motion;
using UnityEngine;

namespace NADA.VFX.Runtime.Execution
{
    internal static class NadaMotionBinder
    {
        public static void BindOrbsMotion(
            Transform orbsRoot,
            global::ItemDrop.ItemData itemData)
        {
            if (orbsRoot == null) return;

            var motion = orbsRoot.GetComponent<NadaOrbitalsOrbsMotion>();
            if (motion == null)
                motion = orbsRoot.gameObject.AddComponent<NadaOrbitalsOrbsMotion>();

            motion.SetItemData(itemData);
        }

        public static void BindOrbitalsRigFollow(
            Transform orbitalsRigRoot,
            Transform followTarget)
        {
            if (orbitalsRigRoot == null || followTarget == null) return;

            var follow = orbitalsRigRoot.GetComponent<NadaWorldFollowMotion>();
            if (follow == null)
                follow = orbitalsRigRoot.gameObject.AddComponent<NadaWorldFollowMotion>();

            follow.SetOffset(Vector3.zero, Quaternion.identity);
            follow.SetTarget(followTarget);
        }

        public static void BindWorldFollow(
            Transform effectTf,
            Transform followTarget)
        {
            if (effectTf == null || followTarget == null) return;

            var follow = effectTf.GetComponent<NadaWorldFollowMotion>();
            if (follow == null)
                follow = effectTf.gameObject.AddComponent<NadaWorldFollowMotion>();

            follow.SetOffset(Vector3.zero, Quaternion.identity);
            follow.SetTarget(followTarget);
        }
    }
}