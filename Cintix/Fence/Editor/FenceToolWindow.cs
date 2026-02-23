using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using Cintix.Fence.Core;

namespace Cintix.Fence.Editor
{
    /*
    public class FenceToolWindow : EditorWindow
    {
        private ToolMode currentMode = ToolMode.None;

        private Cintix.Fence.Runtime.Fence activeFence;
        private SerializedObject serializedFence;

        [MenuItem("Tools/Cintix/Fence Tool")]
        public static void Open()
        {
            GetWindow<FenceToolWindow>("Fence Tool");
        }

        private void OnEnable()
        {
            RefreshActiveFence();
        }

        private void OnSelectionChange()
        {
            RefreshActiveFence();
            Repaint();
        }

        private void OnFocus()
        {
            RefreshActiveFence();
        }

        private void RefreshActiveFence()
        {
            if (Selection.activeGameObject != null)
            {
                activeFence = Selection.activeGameObject.GetComponent<Cintix.Fence.Runtime.Fence>();

                if (activeFence != null)
                    serializedFence = new SerializedObject(activeFence);
                else
                    serializedFence = null;
            }
            else
            {
                activeFence = null;
                serializedFence = null;
            }
        }

        private void OnGUI()
        {
            GUILayout.Space(10);

            DrawModeToolbar();

            GUILayout.Space(10);

            if (activeFence == null)
            {
                EditorGUILayout.HelpBox("No Fence selected.", MessageType.Info);

                if (GUILayout.Button("Create New Fence", GUILayout.Height(30)))
                {
                    CreateNewFence();
                }

                return;
            }

            EditorGUILayout.LabelField("Active Fence", EditorStyles.boldLabel);
            EditorGUILayout.ObjectField(activeFence, typeof(Cintix.Fence.Runtime.Fence), true);

            GUILayout.Space(10);

            DrawFenceProperties();
        }

        private void DrawModeToolbar()
        {
            EditorGUILayout.LabelField("Tool Mode", EditorStyles.boldLabel);

            ToolMode newMode = (ToolMode)GUILayout.Toolbar(
                (int)currentMode,
                new[] { "None", "Edit", "Move" }
            );

            if (newMode == currentMode)
                return;

            currentMode = newMode;

            if (currentMode == ToolMode.None)
            {
                ToolManager.RestorePreviousTool();
            }
            else
            {
                ToolManager.SetActiveTool<FencePlacementTool>();
            }
        }

        private void DrawFenceProperties()
        {
            if (serializedFence == null)
                return;

            serializedFence.Update();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(serializedFence.FindProperty("pillarPrefab"));
            EditorGUILayout.PropertyField(serializedFence.FindProperty("railPrefab"));

            GUILayout.Space(5);
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(serializedFence.FindProperty("spacing"));
            EditorGUILayout.PropertyField(serializedFence.FindProperty("autoClose"));
            EditorGUILayout.PropertyField(serializedFence.FindProperty("railHeightOffset"));
            EditorGUILayout.PropertyField(serializedFence.FindProperty("railInset"));

            GUILayout.Space(5);
            EditorGUILayout.LabelField("Ground Snapping", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(serializedFence.FindProperty("groundLayers"));
            EditorGUILayout.PropertyField(serializedFence.FindProperty("raycastHeight"));

            if (EditorGUI.EndChangeCheck())
            {
                serializedFence.ApplyModifiedProperties();
                activeFence.Rebuild();
            }
            else
            {
                serializedFence.ApplyModifiedProperties();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Rebuild Fence"))
            {
                activeFence.Rebuild();
            }
        }

        private void CreateNewFence()
        {
            GameObject go = new GameObject("Fence");
            Undo.RegisterCreatedObjectUndo(go, "Create Fence");

            activeFence = go.AddComponent<Cintix.Fence.Runtime.Fence>();
            serializedFence = new SerializedObject(activeFence);

            Selection.activeGameObject = go;
        }

        public ToolMode CurrentMode => currentMode;
    }
    */
}