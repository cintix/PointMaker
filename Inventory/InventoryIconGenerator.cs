#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

public class InventoryIconGenerator : EditorWindow
{
    Camera iconCamera;
    Transform itemRoot;

    [MenuItem("Tools/Inventory/Generate Icons")]
    static void Open()
    {
        GetWindow<InventoryIconGenerator>("Icon Generator");
    }

    void OnGUI()
    {
        iconCamera = (Camera)EditorGUILayout.ObjectField(
            "Icon Camera",
            iconCamera,
            typeof(Camera),
            true
        );

        itemRoot = (Transform)EditorGUILayout.ObjectField(
            "Item Root",
            itemRoot,
            typeof(Transform),
            true
        );

        GUILayout.Space(10);

        if (GUILayout.Button("Generate Icons"))
        {
            GenerateIcons();
        }
    }


    void GenerateIcons()
    {
        if (!iconCamera || !itemRoot)
        {
            EditorUtility.DisplayDialog(
                "Missing references",
                "Please assign Icon Camera and Item Root",
                "OK"
            );
            return;
        }

        var guids = AssetDatabase.FindAssets("t:InventoryItem");
        var items = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<InventoryItem>(
                AssetDatabase.GUIDToAssetPath(guid)))
            .Where(item => item.prefab != null)
            .ToList();

        foreach (var item in items)
        {
            GenerateIconForItem(item);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    void GenerateIconForItem(InventoryItem item)
    {
        foreach (Transform c in itemRoot)
            DestroyImmediate(c.gameObject);

        GameObject instance = Instantiate(item.prefab, itemRoot);
        instance.layer = LayerMask.NameToLayer("IconLayer");

        RenderTexture rt = new RenderTexture(256, 256, 24);
        iconCamera.targetTexture = rt;
        iconCamera.Render();

        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(256, 256, TextureFormat.ARGB32, false);
        tex.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);
        tex.Apply();

        string path = $"Assets/Icons/{item.name}.png";
        File.WriteAllBytes(path, tex.EncodeToPNG());

        AssetDatabase.ImportAsset(path);

        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();

        item.icon = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        EditorUtility.SetDirty(item);

        iconCamera.targetTexture = null;
        RenderTexture.active = null;
        rt.Release();
    }
}
#endif

