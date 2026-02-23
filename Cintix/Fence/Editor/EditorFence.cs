using UnityEditor;
using UnityEngine;
using Cintix.Fence.Runtime;
using Cintix.Fence.Core;

namespace Cintix.Fence.Editor
{
    [CustomEditor(typeof(Runtime.Fence))]
    public class EditorFence : UnityEditor.Editor
    {
        private Runtime.Fence _fence;

        private void OnEnable()
        {
            _fence = (Runtime.Fence)target;
        }

        private void OnSceneGUI()
        {
            if (_fence == null)
                return;

            Event e = Event.current;

            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            DrawPolyline();
            DrawHandles();

            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
            {
                HandleMouseClick(e);
            }
        }

        private void HandleMouseClick(Event e)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

            if (!Physics.Raycast(ray, out RaycastHit hit))
                return;

            Undo.RecordObject(_fence, "Modify Fence");

            var points = _fence.Points;

            if (points.Count == 0)
            {
                Undo.RecordObject(_fence.transform, "Initialize Fence Pivot");

                _fence.transform.position = hit.point;
                points.Add(new FencePoint(Vector3.zero));

                _fence.Rebuild();
                e.Use();
                return;
            }

            if (e.shift)
            {
                RemoveNearestPoint(hit.point);
            }
            else
            {
                Vector3 local = _fence.transform.InverseTransformPoint(hit.point);
                points.Add(new FencePoint(local));
            }

            _fence.Rebuild();
            e.Use();
        }

        private void RemoveNearestPoint(Vector3 worldPos)
        {
            var points = _fence.Points;

            float minDist = float.MaxValue;
            int index = -1;

            for (int i = 0; i < points.Count; i++)
            {
                Vector3 wp = _fence.transform.TransformPoint(points[i].LocalPosition);
                float dist = Vector3.Distance(worldPos, wp);

                if (dist < minDist)
                {
                    minDist = dist;
                    index = i;
                }
            }

            if (index >= 0)
                points.RemoveAt(index);
        }

        private void DrawHandles()
        {
            var points = _fence.Points;

            for (int i = 0; i < points.Count; i++)
            {
                Vector3 world = _fence.transform.TransformPoint(points[i].LocalPosition);

                EditorGUI.BeginChangeCheck();
                Vector3 newWorld = Handles.PositionHandle(world, Quaternion.identity);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_fence, "Move Fence Point");
                    points[i].LocalPosition =
                        _fence.transform.InverseTransformPoint(newWorld);

                    _fence.Rebuild();
                }
            }
        }

        private void DrawPolyline()
        {
            var points = _fence.Points;

            if (points.Count < 2)
                return;

            Handles.color = Color.green;

            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector3 a = _fence.transform.TransformPoint(points[i].LocalPosition);
                Vector3 b = _fence.transform.TransformPoint(points[i + 1].LocalPosition);
                Handles.DrawLine(a, b);
            }
        }
    }
}