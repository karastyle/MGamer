using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(JPSGrid))]
public class JPSGridEditor : Editor
{
    private JPSGrid grid;
    private bool isEditMode = false;
    private Vector3 lastCameraPosition;
    private Quaternion lastCameraRotation;
    private bool lastOrthographic;
    private float lastCameraSize;
    private bool lastDisplayGridGizmos;
    
    private Vector2Int? lastPaintedCell = null;
    
    void OnEnable()
    {
        grid = (JPSGrid)target;
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
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Obstacle Editor", EditorStyles.boldLabel);
        
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
                    RegenerateGrid();
                    EditorUtility.SetDirty(grid);
                }
            }
            
            EditorGUILayout.LabelField($"Obstacles Count: {grid.manualObstacles.Count}");
        }
    }
    
    void EnterEditMode()
    {
        isEditMode = true;
        lastDisplayGridGizmos = grid.displayGridGizmos;
        
        // 强制初始化网格
        System.Reflection.MethodInfo initGrid = grid.GetType().GetMethod("InitializeGrid", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (initGrid != null)
        {
            initGrid.Invoke(grid, null);
        }
        
        grid.displayGridGizmos = true;
        
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
        grid.displayGridGizmos = lastDisplayGridGizmos;
        
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
        if (isEditMode)
        {
            // 先处理鼠标输入（优先级高）
            HandleMouseInput();
            
            // 再绘制网格预览
            DrawGridPreview();
        }
        
        sceneView.Repaint();
    }
    
    void DrawGridPreview()
    {
        if (grid == null) return;
        
        // 关键：使用 Always 深度测试，确保总是可见
        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
        
        float nodeRadius = grid.nodeRadius;
        int gridSizeX = grid.GridSizeX;
        int gridSizeY = grid.GridSizeY;
        
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                JPSNode node = grid.GetNode(x, y);
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
        
        // 恢复默认深度测试
        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
    }
    
    void HandleMouseInput()
    {
        Event e = Event.current;
        
        // 只处理鼠标按下和拖动事件
        if (e.type != EventType.MouseDown && e.type != EventType.MouseDrag)
        {
            lastPaintedCell = null;
            return;
        }
        
        // 只处理左键和右键
        if (e.button != 0 && e.button != 1)
        {
            return;
        }
        
        // 忽略 Alt 键（相机旋转）
        if (e.alt)
        {
            return;
        }
        
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        Plane gridPlane = new Plane(Vector3.up, grid.transform.position);
        
        if (gridPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            JPSNode hitNode = grid.NodeFromWorldPoint(hitPoint);
            
            if (hitNode != null)
            {
                Vector2Int cellPos = new Vector2Int(hitNode.gridX, hitNode.gridY);
                
                // 防止同一格子重复绘制
                if (lastPaintedCell.HasValue && lastPaintedCell.Value == cellPos)
                {
                    e.Use(); // 只在成功处理时消耗事件
                    return;
                }
                
                lastPaintedCell = cellPos;
                
                bool paintObstacle = (e.button == 0); // 左键绘制，右键擦除
                
                Undo.RecordObject(grid, paintObstacle ? "Paint Obstacle" : "Erase Obstacle");
                grid.SetObstacle(hitNode.gridX, hitNode.gridY, paintObstacle);
                
                RegenerateGrid();
                EditorUtility.SetDirty(grid);
                
                // 成功处理后才消耗事件
                e.Use();
            }
        }
    }
    
    void RegenerateGrid()
    {
        System.Reflection.MethodInfo initGrid = grid.GetType().GetMethod("InitializeGrid", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (initGrid != null)
        {
            initGrid.Invoke(grid, null);
        }
    }
}