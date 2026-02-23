using UnityEngine;

namespace Cintix.Fence.Core
{
    [System.Serializable]
    public class FencePoint
    {
        public Vector3 LocalPosition;

        public bool OverrideInset;
        public float CustomInset;

        public FencePoint(Vector3 localPosition)
        {
            LocalPosition = localPosition;
            OverrideInset = false;
            CustomInset = 0f;
        }
    }
}