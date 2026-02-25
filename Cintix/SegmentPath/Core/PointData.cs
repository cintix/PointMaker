using System;
using UnityEngine;

namespace Cintix.SegmentPath.Core
{
    [Serializable]
    public struct PointData
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public PointData(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }
    }
}