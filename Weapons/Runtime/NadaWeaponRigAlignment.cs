using NADA.VFX.Weapon.Core.Debug;
using UnityEngine;

namespace NADA.VFX.Weapon.Weapons.Runtime
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
            Bounds localBounds = default;

            foreach (Renderer renderer in renderers)
            {
                if (renderer == null)
                    continue;

                if (IsInsideNadaRig(renderer.transform))
                    continue;

                if (IsLikelyVfxRenderer(renderer))
                    continue;

                Bounds rendererWorldBounds = renderer.bounds;

                EncapsulateWorldBoundsAsLocal(
                    weaponVisualRootTransform,
                    rendererWorldBounds,
                    ref localBounds,
                    ref hasBounds);
            }

            if (!hasBounds)
                return Plugin.RigLocalPosition;

            Vector3 center = localBounds.center;

            if (float.IsNaN(center.x) || float.IsNaN(center.y) || float.IsNaN(center.z) ||
                float.IsInfinity(center.x) || float.IsInfinity(center.y) || float.IsInfinity(center.z))
            {
                return Plugin.RigLocalPosition;
            }

            return center;
        }

        private static void EncapsulateWorldBoundsAsLocal(
            Transform rootTransform,
            Bounds worldBounds,
            ref Bounds localBounds,
            ref bool hasBounds)
        {
            Vector3 min = worldBounds.min;
            Vector3 max = worldBounds.max;

            EncapsulateWorldPointAsLocal(rootTransform, new Vector3(min.x, min.y, min.z), ref localBounds, ref hasBounds);
            EncapsulateWorldPointAsLocal(rootTransform, new Vector3(min.x, min.y, max.z), ref localBounds, ref hasBounds);
            EncapsulateWorldPointAsLocal(rootTransform, new Vector3(min.x, max.y, min.z), ref localBounds, ref hasBounds);
            EncapsulateWorldPointAsLocal(rootTransform, new Vector3(min.x, max.y, max.z), ref localBounds, ref hasBounds);
            EncapsulateWorldPointAsLocal(rootTransform, new Vector3(max.x, min.y, min.z), ref localBounds, ref hasBounds);
            EncapsulateWorldPointAsLocal(rootTransform, new Vector3(max.x, min.y, max.z), ref localBounds, ref hasBounds);
            EncapsulateWorldPointAsLocal(rootTransform, new Vector3(max.x, max.y, min.z), ref localBounds, ref hasBounds);
            EncapsulateWorldPointAsLocal(rootTransform, new Vector3(max.x, max.y, max.z), ref localBounds, ref hasBounds);
        }

        private static void EncapsulateWorldPointAsLocal(
            Transform rootTransform,
            Vector3 worldPoint,
            ref Bounds localBounds,
            ref bool hasBounds)
        {
            Vector3 localPoint = rootTransform.InverseTransformPoint(worldPoint);

            if (!hasBounds)
            {
                localBounds = new Bounds(localPoint, Vector3.zero);
                hasBounds = true;
                return;
            }

            localBounds.Encapsulate(localPoint);
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