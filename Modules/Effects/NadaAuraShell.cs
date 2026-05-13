using UnityEngine;

namespace NADA.VFX.Modules.Effects
{
    internal sealed class NadaAuraShell : MonoBehaviour
    {
        private const float SurfaceOffset = 0.005f;
        private const float BoundsPadding = 0.5f;

        private Mesh _runtimeMesh;

        private Vector3[] _baseVertices;
        private Vector3[] _baseNormals;
        private Vector3[] _scaledVertices;

        private Vector3 _center;
        private Vector3 _sourceBoundsSize = Vector3.one;
        private Vector3 _basePivotLocalScale = Vector3.one;

        private float _lastAppliedScale = -1f;

        internal Vector3 SourceBoundsSize => _sourceBoundsSize;
        internal Vector3 BasePivotLocalScale => _basePivotLocalScale;

        internal bool UsesReadableMesh =>
            _runtimeMesh != null &&
            _baseVertices != null;

        internal void SetBasePivotLocalScale(Vector3 scale)
        {
            _basePivotLocalScale = scale;
        }

        internal void Initialize(Mesh sourceMesh)
        {
            if (sourceMesh == null)
                return;

            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null)
                return;

            _sourceBoundsSize = sourceMesh.bounds.size;

            if (!sourceMesh.isReadable)
            {
                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [Aura] mesh '{sourceMesh.name}' is not readable; using transform-scale fallback.");

                meshFilter.sharedMesh = sourceMesh;

                enabled = false;
                transform.localScale = Vector3.one;
                return;
            }

            InitializeReadableMesh(sourceMesh, meshFilter);
        }

        private void InitializeReadableMesh(
            Mesh sourceMesh,
            MeshFilter meshFilter)
        {
            _runtimeMesh = Object.Instantiate(sourceMesh);
            _runtimeMesh.name = $"{sourceMesh.name}_NADA_Aura";

            meshFilter.sharedMesh = _runtimeMesh;

            _baseVertices = _runtimeMesh.vertices;
            _baseNormals = _runtimeMesh.normals;
            _scaledVertices = new Vector3[_baseVertices.Length];

            _center = sourceMesh.bounds.center;

            ApplyScale(1f);
        }

        internal void ApplyScale(float scale)
        {
            if (_runtimeMesh == null ||
                _baseVertices == null ||
                _scaledVertices == null)
            {
                return;
            }

            float clamped = Mathf.Clamp(scale, 0.2f, 4f);

            if (Mathf.Approximately(_lastAppliedScale, clamped))
                return;

            _lastAppliedScale = clamped;

            Vector3 visualScale =
                BuildReadableVisualScale(_sourceBoundsSize, clamped);

            ApplyVertexScale(visualScale);
            ExpandRuntimeBounds();
        }

        private void ApplyVertexScale(Vector3 visualScale)
        {
            for (int i = 0; i < _baseVertices.Length; i++)
            {
                Vector3 offset = _baseVertices[i] - _center;

                Vector3 normalPush = Vector3.zero;
                if (_baseNormals != null && i < _baseNormals.Length)
                    normalPush = _baseNormals[i].normalized * SurfaceOffset;

                _scaledVertices[i] = new Vector3(
                    _center.x + offset.x * visualScale.x,
                    _center.y + offset.y * visualScale.y,
                    _center.z + offset.z * visualScale.z) + normalPush;
            }

            _runtimeMesh.vertices = _scaledVertices;
            _runtimeMesh.RecalculateBounds();
        }

        private void ExpandRuntimeBounds()
        {
            Bounds bounds = _runtimeMesh.bounds;
            bounds.Expand(BoundsPadding);
            _runtimeMesh.bounds = bounds;
        }

        private static Vector3 BuildReadableVisualScale(
            Vector3 sourceBoundsSize,
            float scale)
        {
            float delta = scale - 1f;

            int longAxis = GetLargestAxis(sourceBoundsSize);
            int shortAxis = GetSmallestAxis(sourceBoundsSize);

            Vector3 weights = new(0.6f, 0.6f, 0.6f);
            weights[longAxis] = 0.35f;
            weights[shortAxis] = 1f;

            return new Vector3(
                1f + delta * weights.x,
                1f + delta * weights.y,
                1f + delta * weights.z);
        }

        private static int GetLargestAxis(Vector3 value)
        {
            if (value.x >= value.y && value.x >= value.z)
                return 0;

            if (value.y >= value.x && value.y >= value.z)
                return 1;

            return 2;
        }

        private static int GetSmallestAxis(Vector3 value)
        {
            if (value.x <= value.y && value.x <= value.z)
                return 0;

            if (value.y <= value.x && value.y <= value.z)
                return 1;

            return 2;
        }
    }
}