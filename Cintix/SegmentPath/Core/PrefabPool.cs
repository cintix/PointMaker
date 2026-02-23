using UnityEditor;

namespace Cintix.SegmentPath.Core
{
    using System.Collections.Generic;
    using UnityEngine;

    public class PrefabPool
    {
        private GameObject prefab;
        private Transform parent;

        public GameObject Prefab => prefab;
        
        private readonly List<GameObject> instances = new();

        public PrefabPool(GameObject prefab, Transform parent)
        {
            this.prefab = prefab;
            this.parent = parent;
        }

        public void Set(GameObject newPrefab, Transform newParent)
        {
            prefab = newPrefab;
            parent = newParent;
        }

        public void EnsureCount(int targetCount)
        {
            if (prefab == null || parent == null)
                return;

            CleanupDeadReferences();
            SyncFromHierarchy();

            // Add missing
            while (instances.Count < targetCount)
            {
                var go = (GameObject) PrefabUtility.InstantiatePrefab(prefab, parent);
                instances.Add(go);
            }

            // Remove excess
            while (instances.Count > targetCount)
            {
                var t = instances[^1];

#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Object.DestroyImmediate(t.gameObject);
                else
#endif
                    Object.Destroy(t.gameObject);

                instances.RemoveAt(instances.Count - 1);
            }

            SyncFromHierarchy();
        }

        public GameObject this[int index] => instances[index];

        public int Count => instances.Count;

        private void CleanupDeadReferences()
        {
            for (int i = instances.Count - 1; i >= 0; i--)
            {
                if (instances[i] == null)
                    instances.RemoveAt(i);
            }
        }

        private void SyncFromHierarchy()
        {
            instances.Clear();

            if (parent == null)
                return;

            for (int i = 0; i < parent.childCount; i++)
            {
                instances.Add(parent.GetChild(i).gameObject);
            }
        }

        public void Clear()
        {
            if (parent == null)
                return;

            for (int i = parent.childCount - 1; i >= 0; i--)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Object.DestroyImmediate(parent.GetChild(i).gameObject);
                else
#endif
                    Object.Destroy(parent.GetChild(i).gameObject);
            }

            instances.Clear();
        }
    }
}