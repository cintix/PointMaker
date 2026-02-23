using System.Collections.Generic;
using Cintix.SegmentPath.Core;
using UnityEditor;
using UnityEngine;
using Cintix.SegmentPath.Runtime;

namespace Cintix.SegmentPath.Editor
{
    
    [CustomEditor(typeof(PointMaker))]
    public class PointMakerEditor : UnityEditor.Editor
    {
        private SegmentLayout segmentLayout = new();
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

            DrawPrefabConfiguration();

            EditorGUILayout.Space();
            DrawDefaultInspectorExceptModeAndPrefabs();

            if (serializedObject.ApplyModifiedProperties())
            {
                segmentLayout.SyncSegments(maker);
            }
            
        }
        
        private void DrawPrefabConfiguration()
        {
            EditorGUILayout.LabelField("Prefabs Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            DrawPrefabList("Segments", maker.Segments);
            EditorGUILayout.Space();

            DrawPrefabList("Rails", maker.Rails);
            EditorGUILayout.Space(10);

            DrawSegmentDropdowns();
            EditorGUILayout.Space();
            DrawRailDropdown();
        }
        
        private void DrawPrefabList(string label, List<GameObject> list)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            int removeIndex = -1;

            for (int i = 0; i < list.Count; i++)
            {
                GUILayout.BeginHorizontal("box");

                Texture2D preview = AssetPreview.GetAssetPreview(list[i]);
                GUILayout.Label(preview, GUILayout.Width(60), GUILayout.Height(60));

                list[i] = (GameObject)EditorGUILayout.ObjectField(
                    list[i],
                    typeof(GameObject),
                    false
                );

                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    removeIndex = i;
                }

                GUILayout.EndHorizontal();
            }

            if (removeIndex >= 0)
            {
                Undo.RecordObject(maker, "Remove Prefab");
                list.RemoveAt(removeIndex);
            }

            if (GUILayout.Button($"Add {label}"))
            {
                Undo.RecordObject(maker, "Add Prefab");
                list.Add(null);
            }
        }
        
        private void DrawSegmentDropdowns()
        {
            string[] segmentNames = maker.Segments.Count > 0
                ? maker.Segments.ConvertAll(s => s != null ? s.name : "None").ToArray()
                : new string[] { "None" };

            if (maker.Segments.Count == 0)
                return;

            EditorGUILayout.LabelField("Segment Selection", EditorStyles.boldLabel);

            maker.DefaultSegmentIndex = EditorGUILayout.Popup(
                "Default Segment",
                Mathf.Clamp(maker.DefaultSegmentIndex, 0, maker.Segments.Count - 1),
                segmentNames
            );

            maker.FirstSegmentIndex = EditorGUILayout.Popup(
                "First Segment",
                Mathf.Clamp(maker.FirstSegmentIndex, 0, maker.Segments.Count - 1),
                segmentNames
            );

            maker.LastSegmentIndex = EditorGUILayout.Popup(
                "Last Segment",
                Mathf.Clamp(maker.LastSegmentIndex, 0, maker.Segments.Count - 1),
                segmentNames
            );
        }
        
        private void DrawRailDropdown()
        {
            string[] railNames = maker.Rails.Count > 0
                ? maker.Rails.ConvertAll(r => r != null ? r.name : "None").ToArray()
                : new string[] { "None" };

            if (maker.Rails.Count == 0)
                return;

            EditorGUILayout.LabelField("Rail Selection", EditorStyles.boldLabel);

            maker.DefaultRailIndex = EditorGUILayout.Popup(
                "Default Rail",
                Mathf.Clamp(maker.DefaultRailIndex, 0, maker.Rails.Count - 1),
                railNames
            );
        }
        
        private void DrawDefaultInspectorExceptModeAndPrefabs()
        {
            SerializedProperty prop = serializedObject.GetIterator();
            prop.NextVisible(true);

            while (prop.NextVisible(false))
            {
                if (prop.name == "mode" ||
                    prop.name == "segments" ||
                    prop.name == "rails" ||
                    prop.name == "defaultSegmentIndex" ||
                    prop.name == "defaultRailIndex" ||
                    prop.name == "firstSegmentIndex" ||
                    prop.name == "lastSegmentIndex")
                    continue;

                EditorGUILayout.PropertyField(prop, true);
            }
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
                segmentLayout.SyncSegments(maker);
                guiEvent.Use();
            }

            if (guiEvent.shift && guiEvent.type == EventType.MouseDrag && guiEvent.button == 0)
            {
                Undo.RecordObject(maker, "Remove Points");
                maker.RemovePointsNear(hit.point, maker.RemoveBrushRadius);
                segmentLayout.SyncSegments(maker);
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
                        segmentLayout.SyncSegments(maker);
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