using UnityEngine;

namespace Cintix.Fence.Core
{
    public struct RailSegment
    {
        public Vector3 Start;
        public Vector3 End;
        public Vector3 Direction;
        public float Length;

        public RailSegment(Vector3 start, Vector3 end)
        {
            Start = start;
            End = end;

            Vector3 delta = end - start;
            Length = delta.magnitude;
            Direction = Length > 0f ? delta / Length : Vector3.forward;
        }
    }
}