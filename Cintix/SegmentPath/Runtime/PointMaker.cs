using System;
using System.Collections.Generic;
using Cintix.SegmentPath.Runtime;
using UnityEngine;

namespace Cintix.SegmentPath
{

    [ExecuteAlways]
    public class PointMaker : MonoBehaviour
    {
        [Header("Mode")]
        [SerializeField] private PointMakerMode mode = PointMakerMode.Edit;
        public PointMakerMode Mode
        {
            get => mode;
            set => mode = value;
        }
        [Header("Raycasting")]
        [SerializeField] private LayerMask raycastLayers = ~0;
        public LayerMask RaycastLayers => raycastLayers;

        [Header("Remove Brush")]
        [SerializeField] private float removeBrushRadius = 0.5f;
        public float RemoveBrushRadius => removeBrushRadius;

        [Header("Preview")]
        [SerializeField] private bool showPreview = true;
        [SerializeField] private float previewSizeMultiplier = 0.25f;
        [SerializeField] private Color previewColor = Color.cyan;

        public bool ShowPreview => showPreview;
        public float PreviewSizeMultiplier => previewSizeMultiplier;
        public Color PreviewColor => previewColor;

        [Header("Point Visualization")]
        [SerializeField] private bool showPoints = true;
        [SerializeField] private float pointDrawSize = 0.1f;
        [SerializeField] private Color pointColor = Color.yellow;
        [SerializeField] private bool drawPointNormals = true;

        public bool ShowPoints => showPoints;
        public float PointDrawSize => pointDrawSize;
        public Color PointColor => pointColor;
        public bool DrawPointNormals => drawPointNormals;

        [Header("Prefabs Configuration")]

        [SerializeField] private List<GameObject> segments = new();
        [SerializeField] private List<GameObject> rails = new();

        [SerializeField] private int defaultSegmentIndex = 0;
        [SerializeField] private int defaultRailIndex = 0;

        [SerializeField] private int firstSegmentIndex = 0;
        [SerializeField] private int lastSegmentIndex = 0;

        public List<GameObject> Segments => segments;
        public List<GameObject> Rails => rails;

        public int DefaultSegmentIndex { get => defaultSegmentIndex; set => defaultSegmentIndex = value; }
        public int DefaultRailIndex { get => defaultRailIndex; set => defaultRailIndex = value; }

        public int FirstSegmentIndex { get => firstSegmentIndex; set => firstSegmentIndex = value; }
        public int LastSegmentIndex { get => lastSegmentIndex; set => lastSegmentIndex = value; }
        
        
        [SerializeField] private List<PointData> points = new();
        public List<PointData> Points => points;

        public void AddPoint(Vector3 position, Quaternion rotation)
        {
            points.Add(new PointData(position, rotation));
        }

        public void RemovePointsNear(Vector3 position, float radius)
        {
            float sqr = radius * radius;
            points.RemoveAll(p => (p.Position - position).sqrMagnitude <= sqr);
        }
    }

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