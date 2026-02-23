using System.Collections.Generic;
using UnityEngine;

namespace Cintix.Fence.Core
{
    public class FenceLayout
    {
        public FenceGeometry Build(
            List<FencePoint> points,
            float spacing,
            bool autoClose,
            float globalInset)
        {
            var geometry = new FenceGeometry();

            if (points == null || points.Count < 2)
                return geometry;

            var working = GetWorkingPoints(points, autoClose);

            for (int i = 0; i < working.Count - 1; i++)
            {
                FencePoint pA = working[i];
                FencePoint pB = working[i + 1];

                Vector3 start = pA.LocalPosition;
                Vector3 end   = pB.LocalPosition;

                Vector3 delta = end - start;
                float distance = delta.magnitude;

                if (distance < 0.0001f)
                    continue;

                Vector3 dir = delta / distance;

                float insetA = pA.OverrideInset ? pA.CustomInset : globalInset;
                float insetB = pB.OverrideInset ? pB.CustomInset : globalInset;

                float traveled = 0f;
                bool firstPillarAdded = false;

                while (traveled <= distance)
                {
                    Vector3 pillarPos = start + dir * traveled;

                    if (!firstPillarAdded)
                    {
                        geometry.Pillars.Add(pillarPos);
                        firstPillarAdded = true;
                    }
                    else if (traveled < distance)
                    {
                        geometry.Pillars.Add(pillarPos);
                    }

                    float next = traveled + spacing;

                    if (next > distance)
                        break;

                    // Create rail between this pillar and next
                    Vector3 nextPos = start + dir * next;

                    Vector3 railStart = pillarPos;
                    Vector3 railEnd   = nextPos;

                    if (traveled == 0f)
                        railStart += dir * insetA;

                    if (next >= distance)
                        railEnd -= dir * insetB;

                    if ((railEnd - railStart).sqrMagnitude > 0f)
                        geometry.Rails.Add(new RailSegment(railStart, railEnd));

                    traveled = next;
                }

                // Ensure last pillar added
                geometry.Pillars.Add(end);
            }

            return geometry;
        }

        private List<FencePoint> GetWorkingPoints(
            List<FencePoint> original,
            bool autoClose)
        {
            if (!autoClose || original.Count < 3)
                return original;

            var list = new List<FencePoint>(original);
            list.Add(original[0]);
            return list;
        }
    }
}