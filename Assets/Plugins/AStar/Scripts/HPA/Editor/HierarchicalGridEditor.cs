using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

[CustomEditor(typeof(HierarchicalGrid))]
public class HierarchicalGridEditor : Editor
{
    private HierarchicalGrid grid;
    private bool isEditMode = false;
    private Vector3 lastCameraPosition;
    private Quaternion lastCameraRotation;
    private bool lastOrthographic;
    private float lastCameraSize;
    private bool lastDisplayFineGrid;
    
    private Vector2Int? lastPaintedCell = null;
    
    void OnEnable()
    {
        grid = (HierarchicalGrid)target;
        SceneView.duringSceneGui += OnSceneGUI;
    }
    
    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        if (isEditMode)
        {
            ExitEditMode();
        }
    }
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("=== HPA* Preprocessing ===", EditorStyles.boldLabel);
        
        // HPA 预处理区域
        EditorGUILayout.BeginVertical("box");
        {
            if (grid.hpaData == null)
            {
                EditorGUILayout.HelpBox("No HPA data found. Click 'Preprocess HPA*' to generate.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox($"HPA Data loaded:\n" +
                    $"- Clusters: {grid.hpaData.clusters.Count}\n" +
                    $"- Entrance Points: {grid.hpaData.entrancePoints.Count}\n" +
                    $"- Edges: {grid.hpaData.abstractEdges.Count}", 
                    MessageType.Info);
            }
            
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Preprocess HPA* (Generate Data)", GUILayout.Height(40)))
            {
                PreprocessHPA();
            }
            GUI.backgroundColor = Color.white;
            
            if (grid.hpaData != null)
            {
                if (GUILayout.Button("Clear HPA Data"))
                {
                    if (EditorUtility.DisplayDialog("Clear HPA Data", 
                        "Are you sure you want to clear HPA preprocessing data?", 
                        "Yes", "No"))
                    {
                        grid.hpaData = null;
                        EditorUtility.SetDirty(grid);
                    }
                }
            }
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("=== Obstacle Editor ===", EditorStyles.boldLabel);
        
        // 障碍物编辑区域
        EditorGUILayout.BeginVertical("box");
        {
            if (!isEditMode)
            {
                if (GUILayout.Button("Enter Edit Mode", GUILayout.Height(40)))
                {
                    EnterEditMode();
                }
                
                EditorGUILayout.HelpBox(
                    "Click 'Enter Edit Mode' to manually paint obstacles.\n" +
                    "Left Mouse: Paint obstacles\n" +
                    "Right Mouse: Erase obstacles",
                    MessageType.Info
                );
            }
            else
            {
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Exit Edit Mode", GUILayout.Height(40)))
                {
                    ExitEditMode();
                }
                GUI.backgroundColor = Color.white;
                
                EditorGUILayout.HelpBox(
                    "🖌️ EDIT MODE ACTIVE\n" +
                    "Left Mouse + Drag: Paint obstacles\n" +
                    "Right Mouse + Drag: Erase obstacles\n" +
                    "Click 'Exit Edit Mode' to save",
                    MessageType.Warning
                );
                
                EditorGUILayout.Space(5);
                if (GUILayout.Button("Clear All Obstacles"))
                {
                    if (EditorUtility.DisplayDialog("Clear Obstacles", 
                        "Are you sure you want to clear all manually painted obstacles?", 
                        "Yes", "No"))
                    {
                        Undo.RecordObject(grid, "Clear All Obstacles");
                        grid.manualObstacles.Clear();
                        RegenerateGrids();
                        EditorUtility.SetDirty(grid);
                    }
                }
                
                EditorGUILayout.LabelField($"Obstacles Count: {grid.manualObstacles.Count}");
            }
        }
        EditorGUILayout.EndVertical();
    }
    
    /// <summary>
    /// HPA* 预处理
    /// </summary>
    void PreprocessHPA()
    {
        if (grid == null)
        {
            EditorUtility.DisplayDialog("Error", "Grid is null!", "OK");
            return;
        }
        
        // 确保网格已初始化
        grid.InitializeGrid();
        
        EditorUtility.DisplayProgressBar("HPA* Preprocessing", "Initializing...", 0f);
        
        try
        {
            HPAPreprocessor preprocessor = new HPAPreprocessor(grid);
            
            EditorUtility.DisplayProgressBar("HPA* Preprocessing", "Creating clusters...", 0.2f);
            HPAData hpaData = preprocessor.Preprocess();
            
            EditorUtility.DisplayProgressBar("HPA* Preprocessing", "Saving data...", 0.9f);
            
            // 保存为 ScriptableObject
            string path = EditorUtility.SaveFilePanelInProject(
                "Save HPA Data",
                "HPAData",
                "asset",
                "Save HPA preprocessing data"
            );
            
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(hpaData, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                grid.hpaData = hpaData;
                EditorUtility.SetDirty(grid);
                
                EditorUtility.DisplayDialog("Success", 
                    $"HPA* preprocessing complete!\n\n" +
                    $"Clusters: {hpaData.clusters.Count}\n" +
                    $"Entrance Points: {hpaData.entrancePoints.Count}\n" +
                    $"Edges: {hpaData.abstractEdges.Count}\n\n" +
                    $"Saved to: {path}", 
                    "OK");
            }
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Preprocessing failed:\n{e.Message}", "OK");
            Debug.LogError($"HPA Preprocessing Error: {e}");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }
    
    void EnterEditMode()
    {
        isEditMode = true;
        lastDisplayFineGrid = grid.displayFineGrid;
        
        grid.InitializeGrid();
        
        grid.displayFineGrid = true;
        grid.displayClusters = false;
        grid.displayEntrancePoints = false;
        
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView == null && SceneView.sceneViews.Count > 0)
        {
            sceneView = SceneView.sceneViews[0] as SceneView;
        }
        
        if (sceneView != null)
        {
            lastCameraPosition = sceneView.camera.transform.position;
            lastCameraRotation = sceneView.camera.transform.rotation;
            lastOrthographic = sceneView.orthographic;
            lastCameraSize = sceneView.size;
            
            sceneView.orthographic = true;
            sceneView.rotation = Quaternion.Euler(90, 0, 0);
            
            float gridSize = Mathf.Max(grid.gridWorldSize.x, grid.gridWorldSize.y);
            sceneView.size = gridSize * 0.6f;
            sceneView.pivot = grid.transform.position;
            
            sceneView.Repaint();
        }
        
        Tools.current = Tool.None;
        Repaint();
    }
    
    void ExitEditMode()
    {
        isEditMode = false;
        
        grid.displayFineGrid = lastDisplayFineGrid;
        grid.displayClusters = true;
        grid.displayEntrancePoints = true;
        
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView == null && SceneView.sceneViews.Count > 0)
        {
            sceneView = SceneView.sceneViews[0] as SceneView;
        }
        
        if (sceneView != null)
        {
            sceneView.orthographic = lastOrthographic;
            sceneView.rotation = lastCameraRotation;
            sceneView.size = lastCameraSize;
            sceneView.pivot = lastCameraPosition;
            sceneView.Repaint();
        }
        
        EditorUtility.SetDirty(grid);
        Repaint();
    }
    
    void OnSceneGUI(SceneView sceneView)
    {
        if (!isEditMode) return;
        
        HandleMouseInput();
        DrawGridPreview();
        
        sceneView.Repaint();
    }
    
    void DrawGridPreview()
    {
        if (grid == null) return;
        
        System.Reflection.FieldInfo fineGridField = grid.GetType().GetField("fineGrid", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (fineGridField == null) return;
        
        AStarNode[,] fineGrid = fineGridField.GetValue(grid) as AStarNode[,];
        if (fineGrid == null) return;
        
        float nodeRadius = grid.fineNodeRadius;
        
        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
        
        for (int x = 0; x < fineGrid.GetLength(0); x++)
        {
            for (int y = 0; y < fineGrid.GetLength(1); y++)
            {
                AStarNode node = fineGrid[x, y];
                if (node == null) continue;
                
                Color cellColor;
                if (grid.IsManualObstacle(x, y))
                {
                    cellColor = new Color(1f, 0f, 0f, 0.8f);
                }
                else
                {
                    cellColor = new Color(1f, 1f, 1f, 0.15f);
                }
                
                Handles.color = cellColor;
                Vector3 center = node.worldPosition;
                Vector3[] verts = new Vector3[4];
                verts[0] = center + new Vector3(-nodeRadius, 0, -nodeRadius);
                verts[1] = center + new Vector3(-nodeRadius, 0, nodeRadius);
                verts[2] = center + new Vector3(nodeRadius, 0, nodeRadius);
                verts[3] = center + new Vector3(nodeRadius, 0, -nodeRadius);
                
                Handles.DrawSolidRectangleWithOutline(verts, cellColor, Color.gray * 0.5f);
            }
        }
        
        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
    }
    
    void HandleMouseInput()
    {
        Event e = Event.current;
        
        if (e.type != EventType.MouseDown && e.type != EventType.MouseDrag)
        {
            lastPaintedCell = null;
            return;
        }
        
        if (e.button != 0 && e.button != 1)
        {
            return;
        }
        
        if (e.alt)
        {
            return;
        }
        
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        Plane gridPlane = new Plane(Vector3.up, grid.transform.position);
        
        if (gridPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            AStarNode hitNode = grid.FineNodeFromWorldPoint(hitPoint);
            
            if (hitNode != null)
            {
                Vector2Int cellPos = new Vector2Int(hitNode.gridX, hitNode.gridY);
                
                if (lastPaintedCell.HasValue && lastPaintedCell.Value == cellPos)
                {
                    e.Use();
                    return;
                }
                
                lastPaintedCell = cellPos;
                
                bool paintObstacle = (e.button == 0);
                
                Undo.RecordObject(grid, paintObstacle ? "Paint Obstacle" : "Erase Obstacle");
                grid.SetObstacle(hitNode.gridX, hitNode.gridY, paintObstacle);
                
                RegenerateGrids();
                EditorUtility.SetDirty(grid);
                
                e.Use();
            }
        }
    }
    
    void RegenerateGrids()
    {
        grid.InitializeGrid();
    }
}