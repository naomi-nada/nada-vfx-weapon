using UnityEngine;

namespace NADA.VFX.Runtime.Binding
{
    internal static class NadaRigTransforms
    {
        internal static Transform EnsureChild(Transform parent, string name)
        {
            if (parent == null) return null;

            foreach (Transform child in parent)
            {
                if (child != null && child.name == name)
                    return child;
            }

            var go = new GameObject(name);
            var tf = go.transform;
            tf.SetParent(parent, false);
            ResetLocalTransform(tf);
            return tf;
        }

        internal static void ResetLocalTransform(Transform tf)
        {
            if (tf == null) return;

            tf.localPosition = Vector3.zero;
            tf.localEulerAngles = Vector3.zero;
            tf.localScale = Vector3.one;
        }

        internal static void RemoveDirectChildIfPresent(Transform parent, string childName)
        {
            if (parent == null) return;

            Transform child = NadaRigPaths.FindDirectChild(parent, childName);
            if (child == null) return;

            Object.Destroy(child.gameObject);
        }

        internal static void CopyMeshFilterIfMissing(Transform sourceTf, Transform targetTf)
        {
            if (sourceTf == null || targetTf == null) return;

            var source = sourceTf.GetComponent<MeshFilter>();
            if (source == null) return;

            var target = targetTf.GetComponent<MeshFilter>();
            if (target == null)
                target = targetTf.gameObject.AddComponent<MeshFilter>();

            target.sharedMesh = source.sharedMesh;
        }

        internal static void CopyMeshRendererIfMissing(Transform sourceTf, Transform targetTf)
        {
            if (sourceTf == null || targetTf == null) return;

            var source = sourceTf.GetComponent<MeshRenderer>();
            if (source == null) return;

            var target = targetTf.GetComponent<MeshRenderer>();
            if (target == null)
                target = targetTf.gameObject.AddComponent<MeshRenderer>();

            target.enabled = source.enabled;
            target.sharedMaterials = source.sharedMaterials;
            target.shadowCastingMode = source.shadowCastingMode;
            target.receiveShadows = source.receiveShadows;
            target.motionVectorGenerationMode = source.motionVectorGenerationMode;
            target.lightProbeUsage = source.lightProbeUsage;
            target.reflectionProbeUsage = source.reflectionProbeUsage;
            target.allowOcclusionWhenDynamic = source.allowOcclusionWhenDynamic;
            target.sortingLayerID = source.sortingLayerID;
            target.sortingOrder = source.sortingOrder;
        }

        internal static void DisableRootVisualContent(Transform root)
        {
            if (root == null) return;

            foreach (var r in root.GetComponents<Renderer>())
            {
                try { r.enabled = false; } catch { }
            }

            foreach (var l in root.GetComponents<Light>())
            {
                try { l.enabled = false; } catch { }
            }

            foreach (var ps in root.GetComponents<ParticleSystem>())
            {
                try
                {
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
                catch { }
            }
        }

        internal static void NormalizeParticleSpacesUnder(
            Transform root,
            ParticleSystemSimulationSpace simulationSpace)
        {
            if (root == null) return;

            var systems = root.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in systems)
            {
                if (ps == null) continue;

                try
                {
                    var main = ps.main;
                    main.simulationSpace = simulationSpace;
                    main.scalingMode = ParticleSystemScalingMode.Local;
                }
                catch { }
            }
        }
    }
}