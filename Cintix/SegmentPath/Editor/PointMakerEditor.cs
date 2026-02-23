using UnityEditor;
using UnityEngine;
using Cintix.SegmentPath.Runtime;

namespace Cintix.SegmentPath.Editor
{
    [CustomEditor(typeof(PointMaker))]
    public class PointMakerEditor : UnityEditor.Editor
    {
        private PointMaker maker;
        private Ray ray;
        private RaycastHit hit;
        private bool toolsWereHidden;
        
        private void OnEnable()
        {
            maker = target as PointMaker;
            toolsWereHidden = Tools.hidden;
        }
        
        private void OnDisable()
        {
            Tools.hidden = false;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawModeToolbar();

            EditorGUILayout.Space();

            DrawDefaultInspectorExceptMode();

            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawModeToolbar()
        {
            EditorGUILayout.LabelField("Mode", EditorStyles.boldLabel);

            PointMakerMode current = maker.Mode;

            EditorGUI.BeginChangeCheck();

            int selected = GUILayout.Toolbar(
                (int)current,
                new string[] { "None", "Edit", "Move" }
            );

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(maker, "Change PointMaker Mode");
                maker.Mode = (PointMakerMode)selected;
                EditorUtility.SetDirty(maker);
            }
        }
        
        private void SetMode(PointMakerMode newMode)
        {
            if (maker.Mode == newMode)
                return;

            Undo.RecordObject(maker, "Change PointMaker Mode");
            maker.Mode = newMode;
            EditorUtility.SetDirty(maker);
        }
        
        private void DrawDefaultInspectorExceptMode()
        {
            SerializedProperty prop = serializedObject.GetIterator();
            prop.NextVisible(true);

            while (prop.NextVisible(false))
            {
                if (prop.name == "mode")
                    continue;

                EditorGUILayout.PropertyField(prop, true);
            }
        }
        
        private void OnSceneGUI()
        {
            if (maker == null)
                return;

            Tools.hidden = true;

            if (maker.Mode == PointMakerMode.None)
            {
                Tools.hidden = false;
                return;
            }
            
            Event guiEvent = Event.current;

            ray = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
            bool hasHit = Physics.Raycast(ray, out hit, 1000f, maker.RaycastLayers);

            if (guiEvent.type == EventType.Layout)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            }

            if (hasHit && (maker.Mode == PointMakerMode.Edit || maker.Mode == PointMakerMode.Move))
            {
                HandleEditInput(guiEvent);
            }

            if (hasHit && maker.Mode == PointMakerMode.Edit)
            {
                DrawPreview();
            }

            DrawStoredPoints();
        }

        private void HandleEditInput(Event guiEvent)
        {
            if (guiEvent.control && guiEvent.type == EventType.MouseDown && guiEvent.button == 0)
            {
                Undo.RecordObject(maker, "Add Point");

                Quaternion rotation = Quaternion.LookRotation(
                    Vector3.ProjectOnPlane(Vector3.forward, hit.normal),
                    hit.normal
                );

                maker.AddPoint(hit.point, rotation);
                guiEvent.Use();
            }

            if (guiEvent.shift && guiEvent.type == EventType.MouseDrag && guiEvent.button == 0)
            {
                Undo.RecordObject(maker, "Remove Points");
                maker.RemovePointsNear(hit.point, maker.RemoveBrushRadius);
                guiEvent.Use();
            }
        }

        private void DrawPreview()
        {
            if (!maker.ShowPreview)
                return;

            float size = HandleUtility.GetHandleSize(hit.point) * maker.PreviewSizeMultiplier;

            Handles.color = maker.PreviewColor;
            Handles.DrawSolidDisc(hit.point, hit.normal, size);
            Handles.color = new Color(maker.PreviewColor.r, maker.PreviewColor.g, maker.PreviewColor.b, 1f);
            Handles.DrawWireDisc(hit.point, hit.normal, size);
        }

        private void DrawStoredPoints()
        {
            if (!maker.ShowPoints)
                return;

            for (int i = 0; i < maker.Points.Count; i++)
            {
                var p = maker.Points[i];
                float size = HandleUtility.GetHandleSize(p.Position) * maker.PointDrawSize;

                if (maker.Mode == PointMakerMode.Move)
                {
                    EditorGUI.BeginChangeCheck();

                    Vector3 newPos = Handles.PositionHandle(p.Position, p.Rotation);

                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(maker, "Move Point");
                        p.Position = newPos;
                        maker.Points[i] = p;
                    }

                    if (maker.DrawPointNormals)
                    {
                        Vector3 normal = p.Rotation * Vector3.up;
                        Handles.DrawLine(p.Position, p.Position + normal * size * 2f);
                    }
                }
                else
                {
                    Handles.color = maker.PointColor;

                    Handles.SphereHandleCap(
                        0,
                        p.Position,
                        Quaternion.identity,
                        size,
                        EventType.Repaint
                    );

                    if (maker.DrawPointNormals)
                    {
                        Vector3 normal = p.Rotation * Vector3.up;
                        Handles.DrawLine(p.Position, p.Position + normal * (size * 2f));
                    }
                }
            }
        }
    }
}