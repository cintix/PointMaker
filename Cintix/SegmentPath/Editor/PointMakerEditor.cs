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

        // Serialized properties
        private SerializedProperty modeProp;
        private SerializedProperty segmentsProp;
        private SerializedProperty railsProp;
        private SerializedProperty defaultSegmentProp;
        private SerializedProperty defaultRailProp;
        private SerializedProperty firstSegmentProp;
        private SerializedProperty lastSegmentProp;

        private void OnEnable()
        {
            maker = target as PointMaker;

            modeProp = serializedObject.FindProperty("mode");
            segmentsProp = serializedObject.FindProperty("segments");
            railsProp = serializedObject.FindProperty("rails");
            defaultSegmentProp = serializedObject.FindProperty("defaultSegmentIndex");
            defaultRailProp = serializedObject.FindProperty("defaultRailIndex");
            firstSegmentProp = serializedObject.FindProperty("firstSegmentIndex");
            lastSegmentProp = serializedObject.FindProperty("lastSegmentIndex");
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
                Debug.Log("Applying modified properties");
                segmentLayout.SyncSegments(maker);
            }
        }

        private void DrawPrefabConfiguration()
        {
            EditorGUILayout.LabelField("Prefabs Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            DrawPrefabList("Segments", segmentsProp);
            EditorGUILayout.Space();

            DrawPrefabList("Rails", railsProp);
            EditorGUILayout.Space(10);

            DrawSegmentDropdowns();
            EditorGUILayout.Space();
            DrawRailDropdown();
        }

        private void DrawPrefabList(string label, SerializedProperty listProp)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            int removeIndex = -1;

            for (int i = 0; i < listProp.arraySize; i++)
            {
                SerializedProperty element = listProp.GetArrayElementAtIndex(i);

                GUILayout.BeginHorizontal("box");

                Texture2D preview = AssetPreview.GetAssetPreview(
                    element.objectReferenceValue
                );

                GUILayout.Label(preview, GUILayout.Width(60), GUILayout.Height(60));

                EditorGUILayout.PropertyField(element, GUIContent.none);

                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    removeIndex = i;
                }

                GUILayout.EndHorizontal();
            }

            if (removeIndex >= 0)
            {
                listProp.DeleteArrayElementAtIndex(removeIndex);
            }

            if (GUILayout.Button($"Add {label}"))
            {
                listProp.InsertArrayElementAtIndex(listProp.arraySize);
            }
        }

        private void DrawSegmentDropdowns()
        {
            if (segmentsProp.arraySize == 0)
                return;

            string[] segmentNames = new string[segmentsProp.arraySize];

            for (int i = 0; i < segmentsProp.arraySize; i++)
            {
                var element = segmentsProp.GetArrayElementAtIndex(i);
                segmentNames[i] = element.objectReferenceValue != null
                    ? element.objectReferenceValue.name
                    : "None";
            }

            EditorGUILayout.LabelField("Segment Selection", EditorStyles.boldLabel);

            defaultSegmentProp.intValue = EditorGUILayout.Popup(
                "Default Segment",
                Mathf.Clamp(defaultSegmentProp.intValue, 0, segmentsProp.arraySize - 1),
                segmentNames
            );

            firstSegmentProp.intValue = EditorGUILayout.Popup(
                "First Segment",
                Mathf.Clamp(firstSegmentProp.intValue, 0, segmentsProp.arraySize - 1),
                segmentNames
            );

            lastSegmentProp.intValue = EditorGUILayout.Popup(
                "Last Segment",
                Mathf.Clamp(lastSegmentProp.intValue, 0, segmentsProp.arraySize - 1),
                segmentNames
            );
        }

        private void DrawRailDropdown()
        {
            if (railsProp.arraySize == 0)
                return;

            string[] railNames = new string[railsProp.arraySize];

            for (int i = 0; i < railsProp.arraySize; i++)
            {
                var element = railsProp.GetArrayElementAtIndex(i);
                railNames[i] = element.objectReferenceValue != null
                    ? element.objectReferenceValue.name
                    : "None";
            }

            EditorGUILayout.LabelField("Rail Selection", EditorStyles.boldLabel);

            defaultRailProp.intValue = EditorGUILayout.Popup(
                "Default Rail",
                Mathf.Clamp(defaultRailProp.intValue, 0, railsProp.arraySize - 1),
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

            modeProp.enumValueIndex = GUILayout.Toolbar(
                modeProp.enumValueIndex,
                new string[] { "None", "Edit", "Move" }
            );
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