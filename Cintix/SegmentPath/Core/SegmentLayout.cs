using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Cintix.SegmentPath.Core
{
    public class SegmentLayout
    {
        private const string SEGMENT_ROOT_NAME = "Segments";
        private PrefabPool SegmentPool;
        
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
            
            SegmentPool ??= new PrefabPool(prefab, root);
            Debug.Log( SegmentPool.Prefab.gameObject.name + " VS " + prefab.gameObject.name );
            if (prefab != SegmentPool.Prefab)
            {
                Debug.LogWarning($"Segment prefab changed from {SegmentPool.Prefab.name} to {prefab.name}");
                SegmentPool.Clear();
                SegmentPool.Set(prefab, root);
            }
            
            SegmentPool.EnsureCount(maker.Points.Count);
            
            for (int index = 0; index < maker.Points.Count; index++)
            {
                Transform instance = SegmentPool[index].transform;
                
                instance.position = maker.Points[index].Position;
                instance.rotation = maker.Points[index].Rotation;
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
            
            Transform root = GetChildGroupName(maker, SEGMENT_ROOT_NAME);

            if (root == null)
            {
                GameObject go = new GameObject(SEGMENT_ROOT_NAME);
                Undo.RegisterCreatedObjectUndo(go, "Create Segment Root");
                go.transform.SetParent(maker.transform);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
                root = go.transform;
            }

            return root;
        }
        
    }
}