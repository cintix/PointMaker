using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using Cintix.Fence.Runtime;
using Cintix.Fence.Core;

namespace Cintix.Fence.Editor
{
    [EditorTool("Fence Placement Tool")]
    public class FencePlacementTool : EditorTool
    {
        private Runtime.Fence activeFence;

        public override void OnActivated()
        {
            activeFence = null;
        }

        public override void OnWillBeDeactivated()
        {
            activeFence = null;
        }

        public override void OnToolGUI(EditorWindow window)
        {
            Event e = Event.current;

            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                activeFence = null;
                ToolManager.RestorePreviousTool();
                e.Use();
                return;
            }

            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
            {
                HandleClick(e);
            }

            DrawPreview();
        }

        private void HandleClick(Event e)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

            if (!Physics.Raycast(ray, out RaycastHit hit))
                return;

            if (activeFence == null)
            {
                GameObject go = new GameObject("Fence");
                Undo.RegisterCreatedObjectUndo(go, "Create Fence");

                activeFence = go.AddComponent<Runtime.Fence>();

                Undo.RecordObject(go.transform, "Initialize Fence Pivot");
                go.transform.position = hit.point;

                activeFence.Points.Add(new FencePoint(Vector3.zero));

                Selection.activeGameObject = go;
                e.Use();
                return;
            }

            if (e.shift)
            {
                RemoveNearestPoint(hit.point);
            }
            else
            {
                Vector3 local = activeFence.transform.InverseTransformPoint(hit.point);
                activeFence.Points.Add(new FencePoint(local));
            }

            activeFence.Rebuild();
            e.Use();
        }

        private void RemoveNearestPoint(Vector3 worldPos)
        {
            var list = activeFence.Points;

            float minDist = float.MaxValue;
            int index = -1;

            for (int i = 0; i < list.Count; i++)
            {
                Vector3 wp = activeFence.transform.TransformPoint(list[i].LocalPosition);
                float dist = Vector3.Distance(worldPos, wp);

                if (dist < minDist)
                {
                    minDist = dist;
                    index = i;
                }
            }

            if (index >= 0)
                list.RemoveAt(index);
        }

        private void DrawPreview()
        {
            if (activeFence == null)
                return;

            var list = activeFence.Points;

            if (list.Count < 2)
                return;

            Handles.color = Color.green;

            for (int i = 0; i < list.Count - 1; i++)
            {
                Vector3 a = activeFence.transform.TransformPoint(list[i].LocalPosition);
                Vector3 b = activeFence.transform.TransformPoint(list[i + 1].LocalPosition);
                Handles.DrawLine(a, b);
            }
        }
    }
}