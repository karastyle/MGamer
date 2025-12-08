using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FlowFieldController))]
public class FlowFieldControllerEditor : Editor
{
    private FlowFieldController controller;
    private bool isEditingObstacles = false; // 原来的 'isEditing'
    private bool isEditingAgents = false;    // 新增：Agent 编辑模式

    // 用于防止在拖动时在同一个格子上多次添加/删除
    private Vector2Int lastEditedGridPos = new Vector2Int(-1, -1);

    private void OnEnable()
    {
        controller = (FlowFieldController)target;
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EditorGUILayout.Space();
        
        // --- 障碍物编辑按钮 ---
        GUI.color = isEditingObstacles ? Color.yellow : Color.white;
        bool newObstacleEditState = GUILayout.Toggle(isEditingObstacles, "1. 进入障碍编辑模式", "Button", GUILayout.Height(30));
        
        if (newObstacleEditState != isEditingObstacles)
        {
            isEditingObstacles = newObstacleEditState;
            if (isEditingObstacles)
            {
                isEditingAgents = false; // 互斥
                SetTopDownView();
            }
            SceneView.RepaintAll();
        }

        // --- Agent 编辑按钮 ---
        GUI.color = isEditingAgents ? Color.cyan : Color.white;
        bool newAgentEditState = GUILayout.Toggle(isEditingAgents, "2. 进入 Agent 编辑模式", "Button", GUILayout.Height(30));

        if (newAgentEditState != isEditingAgents)
        {
            isEditingAgents = newAgentEditState;
            if (isEditingAgents)
            {
                isEditingObstacles = false; // 互斥
                SetTopDownView();
            }
            SceneView.RepaintAll();
        }
        
        GUI.color = Color.white;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        // 如果两个模式都没开，则退出
        if (!isEditingObstacles && !isEditingAgents)
        {
            return;
        }

        if (controller.grid == null)
        {
            controller.CheckAndRebuildGrid();
            if (controller.grid == null) return;
        }

        // 锁定 Scene 视图
        Event e = Event.current;
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        // 释放鼠标时重置，以便下次点击可以编辑同一格子
        if (e.type == EventType.MouseUp)
        {
            lastEditedGridPos = new Vector2Int(-1, -1);
        }

        if (e.type == EventType.MouseDrag || e.type == EventType.MouseDown)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, controller.transform.position);

            if (groundPlane.Raycast(ray, out float distance))
            {
                Vector3 worldPoint = ray.GetPoint(distance);
                Vector2Int gridPos = controller.WorldToGrid(worldPoint);

                if (controller.IsValidGridPos(gridPos) && gridPos != lastEditedGridPos)
                {
                    bool changed = false;
                    
                    if (isEditingObstacles)
                    {
                        // --- 障碍物编辑逻辑 ---
                        if (e.button == 0) // 左键
                        {
                            controller.SetObstacle(gridPos, true);
                            changed = true;
                        }
                        else if (e.button == 1) // 右键
                        {
                            controller.SetObstacle(gridPos, false);
                            changed = true;
                        }
                    }
                    else if (isEditingAgents)
                    {
                        // --- Agent 编辑逻辑 ---
                        if (e.button == 0) // 左键
                        {
                            // 在格子中心添加 Agent
                            Vector3 cellCenter = controller.grid[gridPos.x, gridPos.y].worldPos;
                            controller.AddAgent(cellCenter);
                            changed = true;
                        }
                        else if (e.button == 1) // 右键
                        {
                            // 删除点击位置附近的 Agent
                            controller.RemoveAgent(worldPoint);
                            changed = true;
                        }
                    }

                    if (changed)
                    {
                        lastEditedGridPos = gridPos; // 标记此格已编辑
                        EditorUtility.SetDirty(controller);
                        e.Use();
                        SceneView.RepaintAll();
                    }
                }
            }
        }
    }

    private void SetTopDownView()
    {
        SceneView sv = SceneView.lastActiveSceneView;
        if (sv == null) return;
        if (controller.gridSize.x == 0 || controller.gridSize.y == 0) return;

        Vector3 center = controller.transform.position + new Vector3(
            controller.gridSize.x * controller.cellSize * 0.5f, 0,
            controller.gridSize.y * controller.cellSize * 0.5f);

        sv.camera.orthographic = true;
        sv.rotation = Quaternion.Euler(90f, 0f, 0f);
        sv.pivot = center;
        float size = Mathf.Max(controller.gridSize.x, controller.gridSize.y) * controller.cellSize * 0.5f;
        sv.size = size + (controller.cellSize * 2);
        sv.Repaint();
    }
}