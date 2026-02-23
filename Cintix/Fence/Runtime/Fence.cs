using UnityEngine;
using Cintix.Fence.Core;
using System.Collections.Generic;

namespace Cintix.Fence.Runtime
{
    [ExecuteAlways]
    public class Fence : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject pillarPrefab;
        [SerializeField] private GameObject railPrefab;

        [Header("Settings")]
        [SerializeField] private float spacing = 3f;
        [SerializeField] private bool autoClose = false;
        [SerializeField] private float railHeightOffset = 0.5f;
        [SerializeField] private float railInset = 0.02f;

        [SerializeField] private List<FencePoint> points = new();

        [Header("Ground Snapping")]
        [SerializeField] private LayerMask groundLayers;
        [SerializeField] private float raycastHeight = 100f;

        private FenceLayout layout = new();
        private PrefabPool pillarPool;
        private PrefabPool railPool;
        private float cachedRailLength;

        public List<FencePoint> Points => points;

        [ContextMenu("Rebuild")]
        public void Rebuild()
        {
            if (pillarPrefab == null || railPrefab == null)
                return;

            if (spacing <= 0.001f)
                return;

            CacheRailLength();

            var geometry = layout.Build(points, spacing, autoClose, railInset);

            Transform pillarsRoot = GetOrCreateRoot("Pillars");
            Transform railsRoot   = GetOrCreateRoot("Rails");

            pillarPool ??= new PrefabPool(pillarPrefab, pillarsRoot);
            railPool   ??= new PrefabPool(railPrefab, railsRoot);

            // ----- Apply Pillars -----

            pillarPool.EnsureCount(geometry.Pillars.Count);

            for (int i = 0; i < geometry.Pillars.Count; i++)
            {
                Vector3 local = geometry.Pillars[i];
                Vector3 world = transform.TransformPoint(local);

                TryResolveHeight(ref world);

                local = transform.InverseTransformPoint(world);

                var pillar = pillarPool[i];
                pillar.localPosition = local;
                pillar.localRotation = Quaternion.identity;
            }

            // ----- Apply Rails -----

            railPool.EnsureCount(geometry.Rails.Count);

            for (int i = 0; i < geometry.Rails.Count; i++)
            {
                var seg = geometry.Rails[i];
                var rail = railPool[i];

                Vector3 mid = (seg.Start + seg.End) * 0.5f;
                mid.y += railHeightOffset;

                rail.localPosition = mid;
                rail.localRotation = Quaternion.LookRotation(seg.Direction, Vector3.up);

                float scaleZ = seg.Length / cachedRailLength;

                Vector3 scale = rail.localScale;
                scale.z = scaleZ;
                rail.localScale = scale;
            }
        }

        private void CacheRailLength()
        {
            if (cachedRailLength > 0f)
                return;

            var mf = railPrefab.GetComponentInChildren<MeshFilter>();
            cachedRailLength = mf != null ? mf.sharedMesh.bounds.size.z : 1f;
        }

        private Transform GetOrCreateRoot(string name)
        {
            var t = transform.Find(name);
            if (t != null)
                return t;

            var go = new GameObject(name);
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            return go.transform;
        }

        private bool TryResolveHeight(ref Vector3 world)
        {
            Vector3 origin = world + Vector3.up * raycastHeight;

            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit,
                raycastHeight * 2f, groundLayers, QueryTriggerInteraction.Ignore))
            {
                world.y = hit.point.y;
                return true;
            }

            return false;
        }
    }
}