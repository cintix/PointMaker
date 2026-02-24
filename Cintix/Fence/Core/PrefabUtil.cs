using UnityEditor;
using UnityEngine;

namespace Cintix.Fence.Core
{
    public class PrefabUtil
    {
        public static bool PrefabChanged(Transform root, GameObject prefab)
        {
            if (root.childCount == 0)
                return true;

            for (int index = 0; index < root.childCount; index++)
            {
                var child = root.GetChild(index).gameObject;
                var source = PrefabUtility.GetCorrespondingObjectFromSource(child);
                if (source == null || source != prefab)
                    return true;
            }

            return false;
        }
    }
}