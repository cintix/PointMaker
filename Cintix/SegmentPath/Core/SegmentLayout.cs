using System.Collections.Generic;
using Cintix.Fence.Core;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Cintix.SegmentPath.Core
{
    public class SegmentLayout
    {
        private RailsLayout _railsLayout = new();
        private const string SegmentRootName = "Segments";
        private PrefabPool _segmentPool;
        
        public void SyncSegments(PointMaker maker)
        {
            if (maker.Segments == null || maker.Segments.Count == 0)
                return;

            if (maker.DefaultSegmentIndex < 0 || 
                maker.DefaultSegmentIndex >= maker.Segments.Count)
                return;

            GameObject prefab = maker.Segments[maker.DefaultSegmentIndex];
            if (prefab == null)
                return;

            Transform root = GetOrCreateSegmentRoot(maker);
            
            _segmentPool ??= new PrefabPool(prefab, root);
            
            if (PrefabUtil.PrefabChanged(root, prefab))
            {
                _segmentPool.Clear();
                _segmentPool.Set(prefab, root);
            }
            
            var tempPointsMap = BuildTemporaryPointMap(maker);
            _segmentPool.EnsureCount(tempPointsMap.Count);
            
            _railsLayout.SyncRails(maker, tempPointsMap);
            
            for (int index = 0; index < tempPointsMap.Count; index++)
            {
                Transform instance = _segmentPool[index].transform;
                
                instance.position = tempPointsMap[index].Position;
                instance.rotation = tempPointsMap[index].Rotation;
            }
        }
        
        [CanBeNull]
        private Transform GetChildGroupName(PointMaker maker, string name)
        {
            for (int index=0; index<maker.transform.childCount; index++)
            {
                if (maker.transform.GetChild(index).name == name)
                    return maker.transform.GetChild(index);
            }    
            return null;
        }
        
        private Transform GetOrCreateSegmentRoot(PointMaker maker)
        {
            Transform root = GetChildGroupName(maker, SegmentRootName);

            if (root == null)
            {
                GameObject go = new GameObject(SegmentRootName);
                Undo.RegisterCreatedObjectUndo(go, "Create Segment Root");
                go.transform.SetParent(maker.transform);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
                root = go.transform;
            }

            return root;
        }
        
        private List<PointData> BuildTemporaryPointMap(PointMaker maker)
        {
            var map = new List<PointData>();
            
            if (maker.Points.Count == 0) return map;

            float spacing = maker.SegmentSpacing;

            for (int index = 0; index < maker.Points.Count - 1; index++)
            {
                var current = maker.Points[index];
                var next = maker.Points[index + 1];

                map.Add(current);

                Vector3 dir = next.Position - current.Position;
                float distance = dir.magnitude;

                if (distance <= Mathf.Epsilon) continue;

                Vector3 direction = dir.normalized;
                int segmentCount = CalculateSegmentCount(distance, spacing);

                for (int segmentIndex = 0; segmentIndex < segmentCount; segmentIndex++)
                {
                    float forwardPlacement = spacing * (segmentIndex + 1);
                    
                    Vector3 rawPosition = current.Position + direction * forwardPlacement;
                    Vector3 surfaceNormal = current.Rotation * Vector3.up;

                    ProjectToSurface(rawPosition, surfaceNormal, maker.RaycastLayers, out var grounded, out var normal);

                    Vector3 projectedForward = Vector3.ProjectOnPlane(Vector3.forward, normal).normalized;
                    Quaternion rotation = Quaternion.LookRotation(projectedForward, normal);

                    map.Add(new PointData(grounded, rotation));
                }
            }

            int lastIndex = maker.Points.Count - 1;
            map.Add(maker.Points[lastIndex]);

            return map;
        }
        
        private bool ProjectToSurface(Vector3 position, Vector3 surfaceNormal, LayerMask layers, out Vector3 groundedPosition, out Vector3 hitNormal)
        {
            const float rayOffset = 0.5f;
            const float rayDistance = 5f;

            Vector3 rayOrigin = position + surfaceNormal * rayOffset;
            Vector3 rayDirection = -surfaceNormal;

            Ray ray = new Ray(rayOrigin, rayDirection);

            if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, layers))
            {
                groundedPosition = hit.point;
                hitNormal = hit.normal;
                return true;
            }

            groundedPosition = position;
            hitNormal = surfaceNormal;
            return false;
        }
        
        private int CalculateSegmentCount(float distance, float spacing)
        {
            if (spacing <= 0f) return 0;

            int full = Mathf.FloorToInt(distance / spacing);
            float remainder = distance - (full * spacing);

            if (remainder > 0f && remainder < spacing * 0.5f)
                full--;

            return Mathf.Max(0, full);
        }
    }
}