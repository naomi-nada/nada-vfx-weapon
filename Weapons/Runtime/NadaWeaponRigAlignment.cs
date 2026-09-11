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
        /// <summary>
        /// Most weapon visuals are laid out lengthwise on their local Y axis, which is what the rig expects.
        /// Some weapons instead run lengthwise on local Z, which makes the rig appear rotated about 90 degrees sideways.
        /// If anyone at Iron Gate reads this, please fix this. ^-^
        /// Testing showed that a +90 degree local X rotation fixes these cases consistently.
        /// The ratio check is just here to avoid treating nearly square or round geometry as having a meaningful dominant axis.
        /// </summary>
        private const float LongitudinalBasisConfidenceRatio = 2.0f;

        private static readonly Quaternion ZBasisCorrection =
            Quaternion.Euler(
                90f,
                0f,
                0f);

        internal static NadaWeaponRigAlignment Resolve(
            global::ItemDrop.ItemData itemData,
            Transform weaponVisualRootTransform)
        {
            if (weaponVisualRootTransform == null)
                return Default();

            string itemName =
                itemData?.m_shared?.m_name ??
                string.Empty;

            string itemType =
                itemData?.m_shared?.m_itemType.ToString() ??
                string.Empty;

            Vector3 localCenter =
                ResolveVisualLocalCenter(
                    weaponVisualRootTransform);

            Vector3 baseEuler =
                Plugin.RigLocalEulerAngles;

            bool appliedBasisCorrection =
                TryResolveVisualBasisCorrection(
                    itemType,
                    weaponVisualRootTransform,
                    out Quaternion basisCorrection,
                    out string dominantAxis,
                    out float dominanceRatio,
                    out Vector3 visualBoundsSize);

            if (appliedBasisCorrection)
            {
                Quaternion correctedRotation =
                    Quaternion.Euler(baseEuler) *
                    basisCorrection;

                baseEuler =
                    correctedRotation.eulerAngles;
            }

            NadaLogControl.Equip(
                $"equip:{itemName}:{itemType}:{weaponVisualRootTransform.name}:{weaponVisualRootTransform.GetInstanceID()}",
                $"{Plugin.ModName}: [Equip] " +
                $"item='{itemName}' " +
                $"type='{itemType}' " +
                $"visual='{weaponVisualRootTransform.name}' " +
                $"center={localCenter} " +
                $"basis='{dominantAxis}' " +
                $"basisRatio={dominanceRatio:F2} " +
                $"basisBounds={Format(visualBoundsSize)} " +
                $"basisCorrection={(appliedBasisCorrection ? "X+90" : "none")}");

            return new NadaWeaponRigAlignment(
                localCenter,
                baseEuler,
                ResolveScaleCompensated(
                    weaponVisualRootTransform));
        }

        /// <summary>
        /// Checks whether this weapon has a clear Z-based visual layout that needs the +90 X correction.
        /// Only applies this to normal carried weapon types for now, and leaves anything ambiguous alone.
        /// </summary>
        private static bool TryResolveVisualBasisCorrection(
            string itemType,
            Transform weaponVisualRootTransform,
            out Quaternion correction,
            out string dominantAxis,
            out float dominanceRatio,
            out Vector3 visualBoundsSize)
        {
            correction =
                Quaternion.identity;

            dominantAxis =
                "Unknown";

            dominanceRatio =
                0f;

            visualBoundsSize =
                Vector3.zero;

            if (weaponVisualRootTransform == null)
                return false;

            if (!IsSupportedWeaponTypeForBasisNormalization(
                    itemType))
            {
                return false;
            }

            if (!TryResolveCombinedVisualLocalBounds(
                    weaponVisualRootTransform,
                    out Bounds visualBounds))
            {
                return false;
            }

            visualBoundsSize =
                visualBounds.size;

            dominantAxis =
                ResolveDominantAxis(
                    visualBoundsSize);

            dominanceRatio =
                ResolveDominanceRatio(
                    visualBoundsSize);

            if (dominantAxis != "Z")
                return false;

            if (dominanceRatio <
                LongitudinalBasisConfidenceRatio)
            {
                return false;
            }

            correction =
                ZBasisCorrection;

            return true;
        }

        /// <summary>
        /// Measures all solid geometry under the visual root in the same local space that NADA is attached to.
        /// This is more reliable than trusting one mesh asset's own X/Y/Z, since Valheim weapons can use different imported layouts.
        /// </summary>
        private static bool TryResolveCombinedVisualLocalBounds(
            Transform weaponVisualRootTransform,
            out Bounds localBounds)
        {
            localBounds =
                default;

            if (weaponVisualRootTransform == null)
                return false;

            bool hasBounds =
                false;

            MeshRenderer[] meshRenderers =
                weaponVisualRootTransform
                    .GetComponentsInChildren<MeshRenderer>(
                        true);

            foreach (MeshRenderer renderer in meshRenderers)
            {
                if (renderer == null)
                    continue;

                if (IsInsideNadaRig(
                        renderer.transform))
                {
                    continue;
                }

                if (IsLikelyVfxRenderer(
                        renderer))
                {
                    continue;
                }

                MeshFilter meshFilter =
                    renderer.GetComponent<MeshFilter>();

                if (meshFilter == null ||
                    meshFilter.sharedMesh == null)
                {
                    continue;
                }

                EncapsulateSourceBoundsAsVisualLocal(
                    weaponVisualRootTransform,
                    renderer.transform,
                    meshFilter.sharedMesh.bounds,
                    ref localBounds,
                    ref hasBounds);
            }

            SkinnedMeshRenderer[] skinnedRenderers =
                weaponVisualRootTransform
                    .GetComponentsInChildren<SkinnedMeshRenderer>(
                        true);

            foreach (SkinnedMeshRenderer renderer in skinnedRenderers)
            {
                if (renderer == null ||
                    renderer.sharedMesh == null)
                {
                    continue;
                }

                if (IsInsideNadaRig(
                        renderer.transform))
                {
                    continue;
                }

                if (IsLikelyVfxRenderer(
                        renderer))
                {
                    continue;
                }

                EncapsulateSourceBoundsAsVisualLocal(
                    weaponVisualRootTransform,
                    renderer.transform,
                    renderer.localBounds,
                    ref localBounds,
                    ref hasBounds);
            }

            return hasBounds &&
                   IsFinite(localBounds.center) &&
                   IsFinite(localBounds.size);
        }

        /// <summary>
        /// Converts all eight corners of a renderer's bounds into the visual root's local space.
        /// I use the corners instead of just Bounds.size because child renderers can be rotated.
        /// </summary>
        private static void EncapsulateSourceBoundsAsVisualLocal(
            Transform visualRoot,
            Transform sourceTransform,
            Bounds sourceBounds,
            ref Bounds combinedBounds,
            ref bool hasBounds)
        {
            if (visualRoot == null ||
                sourceTransform == null)
            {
                return;
            }

            Vector3 center =
                sourceBounds.center;

            Vector3 extents =
                sourceBounds.extents;

            for (int x = -1;
                 x <= 1;
                 x += 2)
            {
                for (int y = -1;
                     y <= 1;
                     y += 2)
                {
                    for (int z = -1;
                         z <= 1;
                         z += 2)
                    {
                        Vector3 sourceLocalPoint =
                            center +
                            new Vector3(
                                extents.x * x,
                                extents.y * y,
                                extents.z * z);

                        Vector3 worldPoint =
                            sourceTransform.TransformPoint(
                                sourceLocalPoint);

                        Vector3 visualLocalPoint =
                            visualRoot.InverseTransformPoint(
                                worldPoint);

                        if (!hasBounds)
                        {
                            combinedBounds =
                                new Bounds(
                                    visualLocalPoint,
                                    Vector3.zero);

                            hasBounds =
                                true;
                        }
                        else
                        {
                            combinedBounds.Encapsulate(
                                visualLocalPoint);
                        }
                    }
                }
            }
        }

        private static Vector3 ResolveVisualLocalCenter(
            Transform weaponVisualRootTransform)
        {
            Renderer[] renderers =
                weaponVisualRootTransform
                    .GetComponentsInChildren<Renderer>(
                        true);

            bool hasBounds =
                false;

            Bounds localBounds =
                default;

            foreach (Renderer renderer in renderers)
            {
                if (renderer == null)
                    continue;

                if (IsInsideNadaRig(
                        renderer.transform))
                {
                    continue;
                }

                if (IsLikelyVfxRenderer(
                        renderer))
                {
                    continue;
                }

                Bounds rendererWorldBounds =
                    renderer.bounds;

                EncapsulateWorldBoundsAsLocal(
                    weaponVisualRootTransform,
                    rendererWorldBounds,
                    ref localBounds,
                    ref hasBounds);
            }

            if (!hasBounds)
                return Plugin.RigLocalPosition;

            Vector3 center =
                localBounds.center;

            if (!IsFinite(center))
                return Plugin.RigLocalPosition;

            return center;
        }

        private static void EncapsulateWorldBoundsAsLocal(
            Transform rootTransform,
            Bounds worldBounds,
            ref Bounds localBounds,
            ref bool hasBounds)
        {
            Vector3 min =
                worldBounds.min;

            Vector3 max =
                worldBounds.max;

            EncapsulateWorldPointAsLocal(
                rootTransform,
                new Vector3(
                    min.x,
                    min.y,
                    min.z),
                ref localBounds,
                ref hasBounds);

            EncapsulateWorldPointAsLocal(
                rootTransform,
                new Vector3(
                    min.x,
                    min.y,
                    max.z),
                ref localBounds,
                ref hasBounds);

            EncapsulateWorldPointAsLocal(
                rootTransform,
                new Vector3(
                    min.x,
                    max.y,
                    min.z),
                ref localBounds,
                ref hasBounds);

            EncapsulateWorldPointAsLocal(
                rootTransform,
                new Vector3(
                    min.x,
                    max.y,
                    max.z),
                ref localBounds,
                ref hasBounds);

            EncapsulateWorldPointAsLocal(
                rootTransform,
                new Vector3(
                    max.x,
                    min.y,
                    min.z),
                ref localBounds,
                ref hasBounds);

            EncapsulateWorldPointAsLocal(
                rootTransform,
                new Vector3(
                    max.x,
                    min.y,
                    max.z),
                ref localBounds,
                ref hasBounds);

            EncapsulateWorldPointAsLocal(
                rootTransform,
                new Vector3(
                    max.x,
                    max.y,
                    min.z),
                ref localBounds,
                ref hasBounds);

            EncapsulateWorldPointAsLocal(
                rootTransform,
                new Vector3(
                    max.x,
                    max.y,
                    max.z),
                ref localBounds,
                ref hasBounds);
        }

        private static void EncapsulateWorldPointAsLocal(
            Transform rootTransform,
            Vector3 worldPoint,
            ref Bounds localBounds,
            ref bool hasBounds)
        {
            Vector3 localPoint =
                rootTransform.InverseTransformPoint(
                    worldPoint);

            if (!hasBounds)
            {
                localBounds =
                    new Bounds(
                        localPoint,
                        Vector3.zero);

                hasBounds =
                    true;

                return;
            }

            localBounds.Encapsulate(
                localPoint);
        }

        private static Vector3 ResolveScaleCompensated(
            Transform weaponVisualRootTransform)
        {
            if (weaponVisualRootTransform == null)
                return Plugin.RigLocalScale;

            Vector3 parentScale =
                weaponVisualRootTransform.lossyScale;

            return new Vector3(
                SafeDivide(
                    Plugin.RigLocalScale.x,
                    parentScale.x),

                SafeDivide(
                    Plugin.RigLocalScale.y,
                    parentScale.y),

                SafeDivide(
                    Plugin.RigLocalScale.z,
                    parentScale.z));
        }

        private static float SafeDivide(
            float value,
            float divisor)
        {
            if (Mathf.Abs(divisor) <
                0.0001f)
            {
                return value;
            }

            return
                value /
                divisor;
        }

        /// <summary>
        /// These are the carried weapon categories for which we currently
        /// have direct evidence that visual-basis normalization is valid.
        ///
        /// Keep this deliberately narrow until other categories are tested.
        /// </summary>
        private static bool IsSupportedWeaponTypeForBasisNormalization(
            string itemType)
        {
            if (string.IsNullOrEmpty(
                    itemType))
            {
                return false;
            }

            switch (itemType)
            {
                case "OneHandedWeapon":
                case "TwoHandedWeapon":
                case "TwoHandedWeaponLeft":
                    return true;

                default:
                    return false;
            }
        }

        private static string ResolveDominantAxis(
            Vector3 size)
        {
            float x =
                Mathf.Abs(size.x);

            float y =
                Mathf.Abs(size.y);

            float z =
                Mathf.Abs(size.z);

            if (x >= y &&
                x >= z)
            {
                return "X";
            }

            if (y >= x &&
                y >= z)
            {
                return "Y";
            }

            return "Z";
        }

        private static float ResolveDominanceRatio(
            Vector3 size)
        {
            float x =
                Mathf.Abs(size.x);

            float y =
                Mathf.Abs(size.y);

            float z =
                Mathf.Abs(size.z);

            float largest =
                Mathf.Max(
                    x,
                    Mathf.Max(
                        y,
                        z));

            float smallest =
                Mathf.Min(
                    x,
                    Mathf.Min(
                        y,
                        z));

            float secondLargest =
                x +
                y +
                z -
                largest -
                smallest;

            if (secondLargest <=
                0.000001f)
            {
                return float.PositiveInfinity;
            }

            return
                largest /
                secondLargest;
        }

        private static bool IsFinite(
            Vector3 value)
        {
            return
                !float.IsNaN(value.x) &&
                !float.IsNaN(value.y) &&
                !float.IsNaN(value.z) &&
                !float.IsInfinity(value.x) &&
                !float.IsInfinity(value.y) &&
                !float.IsInfinity(value.z);
        }

        private static bool IsLikelyVfxRenderer(
            Renderer renderer)
        {
            if (renderer == null)
                return true;

            return
                renderer is ParticleSystemRenderer ||
                renderer is TrailRenderer ||
                renderer is LineRenderer;
        }

        private static bool IsInsideNadaRig(
            Transform transform)
        {
            while (transform != null)
            {
                if (transform.name ==
                    Plugin.LocalWeaponRootName)
                {
                    return true;
                }

                transform =
                    transform.parent;
            }

            return false;
        }

        private static string Format(
            Vector3 value)
        {
            return
                $"({value.x:F3}," +
                $"{value.y:F3}," +
                $"{value.z:F3})";
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