using NADA.VFX.Core.Debug;
using UnityEngine;

namespace NADA.VFX.Weapons.Runtime
{
    internal readonly struct NadaWeaponRigAlignment
    {
        public readonly Vector3 LocalPosition;
        public readonly Vector3 LocalEulerAngles;
        public readonly Vector3 LocalScale;

        public NadaWeaponRigAlignment(
            Vector3 localPosition,
            Vector3 localEulerAngles,
            Vector3 localScale)
        {
            LocalPosition = localPosition;
            LocalEulerAngles = localEulerAngles;
            LocalScale = localScale;
        }
    }

    internal static class NadaWeaponRigAlignmentResolver
    {
        internal static NadaWeaponRigAlignment Resolve(
            global::ItemDrop.ItemData itemData,
            Transform weaponVisualRootTransform)
        {
            if (weaponVisualRootTransform == null)
                return Default();

            string itemName = itemData?.m_shared?.m_name ?? string.Empty;
            string itemType = itemData?.m_shared?.m_itemType.ToString() ?? string.Empty;

            Vector3 localCenter = ResolveVisualLocalCenter(weaponVisualRootTransform);

            NadaLogControl.Equip(
                $"equip:{itemName}:{itemType}:{weaponVisualRootTransform.name}:{weaponVisualRootTransform.GetInstanceID()}",
                $"{Plugin.ModName}: [Equip] item='{itemName}' type='{itemType}' visual='{weaponVisualRootTransform.name}' center={localCenter}");

            return new NadaWeaponRigAlignment(
                localCenter,
                Plugin.RigLocalEulerAngles,
                ResolveScaleCompensated(weaponVisualRootTransform));
        }

        private static Vector3 ResolveVisualLocalCenter(Transform weaponVisualRootTransform)
        {
            Renderer[] renderers =
                weaponVisualRootTransform.GetComponentsInChildren<Renderer>(true);

            bool hasBounds = false;
            Bounds combinedBounds = default;

            foreach (Renderer renderer in renderers)
            {
                if (renderer == null)
                    continue;

                if (IsInsideNadaRig(renderer.transform))
                    continue;

                if (IsLikelyVfxRenderer(renderer))
                    continue;

                if (!hasBounds)
                {
                    combinedBounds = renderer.bounds;
                    hasBounds = true;
                    continue;
                }

                combinedBounds.Encapsulate(renderer.bounds);
            }

            if (!hasBounds)
                return Plugin.RigLocalPosition;

            return weaponVisualRootTransform.InverseTransformPoint(combinedBounds.center);
        }
        
        private static Vector3 ResolveScaleCompensated(Transform weaponVisualRootTransform)
        {
            if (weaponVisualRootTransform == null)
                return Plugin.RigLocalScale;

            Vector3 parentScale = weaponVisualRootTransform.lossyScale;

            return new Vector3(
                SafeDivide(Plugin.RigLocalScale.x, parentScale.x),
                SafeDivide(Plugin.RigLocalScale.y, parentScale.y),
                SafeDivide(Plugin.RigLocalScale.z, parentScale.z));
        }

        private static float SafeDivide(float value, float divisor)
        {
            if (Mathf.Abs(divisor) < 0.0001f)
                return value;

            return value / divisor;
        }

        private static bool IsLikelyVfxRenderer(Renderer renderer)
        {
            if (renderer == null)
                return true;

            return renderer is ParticleSystemRenderer ||
                   renderer is TrailRenderer ||
                   renderer is LineRenderer;
        }

        private static bool IsInsideNadaRig(Transform transform)
        {
            while (transform != null)
            {
                if (transform.name == Plugin.LocalWeaponRootName)
                    return true;

                transform = transform.parent;
            }

            return false;
        }

        private static NadaWeaponRigAlignment Default()
        {
            return new NadaWeaponRigAlignment(
                Plugin.RigLocalPosition,
                Plugin.RigLocalEulerAngles,
                Plugin.RigLocalScale);
        }
    }
}