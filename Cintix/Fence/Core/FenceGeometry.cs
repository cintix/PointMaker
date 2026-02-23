using System.Collections.Generic;
using UnityEngine;

namespace Cintix.Fence.Core
{
    public class FenceGeometry
    {
        public readonly List<Vector3> Pillars = new();
        public readonly List<RailSegment> Rails = new();
    }
}