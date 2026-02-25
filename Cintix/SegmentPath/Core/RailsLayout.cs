using System.Collections.Generic;
using Cintix.Fence.Core;
using UnityEditor;
using UnityEngine;

namespace Cintix.SegmentPath.Core
{

    public class RailsLayout
    {
        private Dictionary<GameObject, float> _railLengths = new();
        private const string RailsRootName = "Rails";
        private PrefabPool _railsPool;

        private void ResyncRailsLength(PointMaker maker)
        {
            var currentPrefabs = new HashSet<GameObject>(maker.Rails);
            var keysToRemove = new List<GameObject>();
            
            foreach (var key in _railLengths.Keys)
            {
                if (!currentPrefabs.Contains(key))
                    keysToRemove.Add(key);
            }

            foreach (var key in keysToRemove)
                _railLengths.Remove(key);

            foreach (var railPrefab in maker.Rails)
            {
                if (railPrefab == null)
                    continue;

                if (!_railLengths.ContainsKey(railPrefab))
                    _railLengths[railPrefab] = CalculatePrefabLength(railPrefab);
            }
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        public void SyncRails(PointMaker maker,  List<PointData> segments)
        {
            if (maker.DefaultRailIndex < 0 || maker.DefaultRailIndex >= maker.Rails.Count)
                return;

            ResyncRailsLength(maker);
            
            GameObject prefab = maker.Rails[maker.DefaultRailIndex];
            if (prefab == null)
                return;

            Transform root = GetOrCreateRailsRoot(maker);
            _railsPool ??= new PrefabPool(prefab, root);
            
            if (PrefabUtil.PrefabChanged(root, prefab))
            {
                _railsPool.Clear();
                _railsPool.Set(prefab, root);
            }

            var tempPointsMap = BuildRailsPointMap(maker, prefab, segments);
            _railsPool.EnsureCount(tempPointsMap.Count);
            
            for (int index = 0; index < tempPointsMap.Count; index++)
            {
                Transform instance = _railsPool[index].transform;
                
                instance.position = tempPointsMap[index].Position;
                instance.rotation = tempPointsMap[index].Rotation;
            }            
        }

        private float CalculatePrefabLength(GameObject prefab)
        {
            var renderers = prefab.GetComponentsInChildren<Renderer>(true);

            if (renderers.Length == 0)
                return 0f;

            Bounds combined = renderers[0].bounds;

            for (int index = 1; index < renderers.Length; index++)
            {
                combined.Encapsulate(renderers[index].bounds);
            }

            Vector3 localMin = prefab.transform.InverseTransformPoint(combined.min);
            Vector3 localMax = prefab.transform.InverseTransformPoint(combined.max);

            return Mathf.Abs(localMax.z - localMin.z);
        }
        
        private List<PointData> BuildRailsPointMap(PointMaker maker, GameObject prefab, List<PointData> segments)
        {
           var map = new List<PointData>();
           for (int index = 0; index < segments.Count - 1; index++)
           {
               var current = segments[index];
               var next = segments[index + 1];

               int railsToPlace  = GetRailsBetweenPoints(prefab, current, next);
               map.AddRange(PlaceRailsBetweenPoints(prefab, current, next, railsToPlace, maker.RailsOffset));
           }

           return map;           
        }

        private int GetRailsBetweenPoints(GameObject prefab, PointData a, PointData b)
        {
            float prefabLength = _railLengths[prefab];
            float distance = Vector3.Distance(a.Position, b.Position);
            int railCount = Mathf.CeilToInt(distance / prefabLength);
            return railCount;
        }

        private List<PointData> PlaceRailsBetweenPoints(GameObject prefab, PointData a, PointData b, int railCount, float railsOffset)
        {
            var map = new List<PointData>();

            if (railCount <= 0)
                return map;

            float length = _railLengths[prefab];

            Vector3 direction = (b.Position - a.Position).normalized;
            Vector3 firstPos = a.Position + direction * (length * 0.5f);
            Vector3 lastPos  = b.Position - direction * (length * 0.5f);

            for (int index = 0; index < railCount; index++)
            {
                float normalizedPosition = railCount == 1 ? 0f : (float)index / (railCount - 1);
                
                Vector3 pos = Vector3.Lerp(firstPos, lastPos, normalizedPosition);
                Vector3 normal = Vector3.Slerp(a.Rotation * Vector3.up, b.Rotation * Vector3.up, normalizedPosition).normalized;
                Quaternion rotation = Quaternion.LookRotation(direction, normal);
                
                pos += normal * railsOffset;

                map.Add(new PointData(pos, rotation));
            }

            return map;
        }     
        
        private Transform GetChildGroupName(PointMaker maker, string name)
        {
            for (int index=0; index<maker.transform.childCount; index++)
            {
                if (maker.transform.GetChild(index).name == name)
                    return maker.transform.GetChild(index);
            }    
            return null;
        }
        
        private Transform GetOrCreateRailsRoot(PointMaker maker)
        {
            Transform root = GetChildGroupName(maker, RailsRootName);
            if (root == null)
            {
                GameObject gameObject = new GameObject(RailsRootName);
                Undo.RegisterCreatedObjectUndo(gameObject, "Create Rails Root");
                gameObject.transform.SetParent(maker.transform);
                gameObject.transform.localPosition = Vector3.zero;
                gameObject.transform.localRotation = Quaternion.identity;
                gameObject.transform.localScale = Vector3.one;
                root = gameObject.transform;
            }

            return root;
        }        
    }
}