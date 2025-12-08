using UnityEngine;
using System.Collections.Generic;

// UnityEditor 命名空间只在编辑器下使用，必须用 #if 包裹
#if UNITY_EDITOR
using UnityEditor;
#endif

// (Cell 类保持不变，但为清晰起见，我将其折叠了)
[System.Serializable]
public class Cell
{
    public Vector3 worldPos;
    public Vector2Int gridIndex;
    public bool isObstacle;
    public int cost = int.MaxValue; 
    public Vector2 flowVector = Vector2.zero;
    public Cell() { }
    public Cell(Vector3 worldPos, Vector2Int gridIndex)
    {
        this.worldPos = worldPos;
        this.gridIndex = gridIndex;
        this.isObstacle = false;
    }
}

public class FlowFieldController : MonoBehaviour
{
    public enum GizmoDisplayMode
    {
        None,
        CostField, // 热力图 (数字)
        FlowField  // 向量场 (箭头)
    }

    [Header("网格配置")]
    public Vector2Int gridSize = new Vector2Int(50, 50);
    public float cellSize = 1.0f;

    [Header("Agent 配置")]
    public GameObject agentPrefab; // <-- 新增：Agent Prefab 引用

    [Header("Gizmo 预览")]
    public GizmoDisplayMode gizmoDisplay = GizmoDisplayMode.FlowField;
    public bool showGrid = true;
    public bool showObstacles = true;
    public bool showTarget = true;

    [System.NonSerialized]
    public Cell[,] grid; 

    [SerializeField, HideInInspector] 
    private List<bool> obstacleData = new List<bool>();

    // 新增：持久化存储 Agent
    [SerializeField, HideInInspector]
    private List<GameObject> spawnedAgents = new List<GameObject>();

    private Vector2Int targetPos;
    private bool hasTarget = false;
    private int maxCost = 0;

    private readonly Vector2Int[] neighborDirections = new Vector2Int[]
    {
        new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(1, 0), new Vector2Int(-1, 0),
        new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1)
    };

    private void OnValidate()
    {
        #if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            // 清理在编辑器中可能被手动删除的 Agent 引用
            spawnedAgents.RemoveAll(item => item == null);
            CheckAndRebuildGrid();
        }
        #endif
    }

    void Awake()
    {
        // 清理在编辑器中可能被手动删除的 Agent 引用
        spawnedAgents.RemoveAll(item => item == null);
        CheckAndRebuildGrid();
    }
    
    public bool HasTarget()
    {
        return hasTarget;
    }

    public void CheckAndRebuildGrid()
    {
        int requiredSize = gridSize.x * gridSize.y;
        if (requiredSize <= 0) { grid = null; return; }
        if (grid == null || grid.GetLength(0) != gridSize.x || grid.GetLength(1) != gridSize.y)
        {
            grid = new Cell[gridSize.x, gridSize.y];
        }

        Vector3 offset = transform.position;
        if (obstacleData.Count != requiredSize)
        {
            obstacleData.Clear();
            for (int i = 0; i < requiredSize; i++) obstacleData.Add(false);
            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
        }

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; (y < gridSize.y); y++)
            {
                Vector3 worldPos = offset + new Vector3((x * cellSize) + cellSize * 0.5f, 0, (y * cellSize) + cellSize * 0.5f);
                if (grid[x, y] == null) grid[x, y] = new Cell(worldPos, new Vector2Int(x, y));
                else { grid[x, y].worldPos = worldPos; grid[x, y].gridIndex = new Vector2Int(x, y); }
                grid[x, y].isObstacle = obstacleData[y * gridSize.x + x];
                grid[x, y].cost = int.MaxValue;
                grid[x, y].flowVector = Vector2.zero;
            }
        }
    }

    public void SetObstacle(Vector2Int pos, bool isObstacle)
    {
        if (!IsValidGridPos(pos)) return;
        int index = pos.y * gridSize.x + pos.x;
        if (index < 0 || index >= obstacleData.Count) return;
        if (grid != null) grid[pos.x, pos.y].isObstacle = isObstacle;
        obstacleData[index] = isObstacle;
    }
    
    // (Update, SetTarget, GenerateCostField, GenerateFlowField 保持不变... )
    #region 核心算法 (无改动)
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Camera.main == null) return;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, transform.position);

            if (groundPlane.Raycast(ray, out float distance))
            {
                Vector3 worldPoint = ray.GetPoint(distance);
                Vector2Int gridPos = WorldToGrid(worldPoint);
                SetTarget(gridPos);
            }
        }
    }

    public void SetTarget(Vector2Int pos)
    {
        if (!IsValidGridPos(pos)) return;
        if (grid == null) CheckAndRebuildGrid();
        if (grid[pos.x, pos.y].isObstacle)
        {
            Debug.LogWarning("目标点无效或在障碍物上！"); return;
        }
        targetPos = pos;
        hasTarget = true;
        Debug.Log($"新目标点: {targetPos}");

        GenerateCostField();
        GenerateFlowField();
    }
    
    private void GenerateCostField()
    {
        if (!hasTarget || grid == null) return;
        maxCost = 0;
        foreach (Cell cell in grid) { cell.cost = int.MaxValue; cell.flowVector = Vector2.zero; }
        Queue<Cell> queue = new Queue<Cell>();
        Cell targetCell = grid[targetPos.x, targetPos.y];
        targetCell.cost = 0;
        queue.Enqueue(targetCell);
        while (queue.Count > 0)
        {
            Cell current = queue.Dequeue();
            foreach (Vector2Int dir in neighborDirections)
            {
                Vector2Int neighborPos = current.gridIndex + dir;
                if (IsValidGridPos(neighborPos))
                {
                    Cell neighbor = grid[neighborPos.x, neighborPos.y];
                    if (neighbor.isObstacle) continue;
                    int newCost = current.cost + 1;
                    if (newCost < neighbor.cost)
                    {
                        neighbor.cost = newCost;
                        queue.Enqueue(neighbor);
                        if (newCost > maxCost) maxCost = newCost;
                    }
                }
            }
        }
        Debug.Log("热力图（代价场）生成完毕。");
    }

    private void GenerateFlowField()
    {
        if (!hasTarget || grid == null) return;
        foreach (Cell cell in grid)
        {
            if (cell.isObstacle || cell.cost == 0) { cell.flowVector = Vector2.zero; continue; }
            int bestCost = cell.cost;
            Vector2Int bestDir = Vector2Int.zero;
            foreach (Vector2Int dir in neighborDirections)
            {
                Vector2Int neighborPos = cell.gridIndex + dir;
                if (IsValidGridPos(neighborPos))
                {
                    Cell neighbor = grid[neighborPos.x, neighborPos.y];
                    if (neighbor.isObstacle) continue;
                    if (neighbor.cost < bestCost)
                    {
                        bestCost = neighbor.cost;
                        bestDir = dir;
                    }
                }
            }
            cell.flowVector = new Vector2(bestDir.x, bestDir.y).normalized;
        }
        Debug.Log("向量场（流场）生成完毕。");
    }
    #endregion

    #region Agent 管理 (新增)

    /// <summary>
    /// (公共) 编辑器调用：添加 Agent
    /// </summary>
    public void AddAgent(Vector3 worldPos)
    {
        if (agentPrefab == null)
        {
            Debug.LogError("Agent Prefab 未配置！");
            return;
        }
        
        // 确保新 Agent 位于父级 (FlowFieldManager) 下
        GameObject newAgent = Instantiate(agentPrefab, worldPos, Quaternion.identity, this.transform);
        spawnedAgents.Add(newAgent);
        
        #if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(newAgent, "Add Agent");
        EditorUtility.SetDirty(this);
        #endif
    }

    /// <summary>
    /// (公共) 编辑器调用：删除 Agent
    /// </summary>
    public void RemoveAgent(Vector3 worldPos)
    {
        GameObject closestAgent = null;
        float minDistance = 2f * cellSize; // 只在合理范围内搜索

        foreach (GameObject agent in spawnedAgents)
        {
            if (agent == null) continue;
            float dist = Vector3.Distance(agent.transform.position, worldPos);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestAgent = agent;
            }
        }

        if (closestAgent != null)
        {
            spawnedAgents.Remove(closestAgent);
            #if UNITY_EDITOR
            Undo.DestroyObjectImmediate(closestAgent);
            EditorUtility.SetDirty(this);
            #else
            Destroy(closestAgent);
            #endif
        }
    }

    #endregion

    #region 核心：步骤 3 - 向量插值 (新增)

    /// <summary>
    /// 获取世界坐标对应的平滑流场向量（双线性插值）
    /// </summary>
    public Vector2 GetFlowVector(Vector3 worldPos)
    {
        if (grid == null) return Vector2.zero;

        // 1. 将世界坐标转换为 "浮点" 网格坐标
        // (0.5, 0.5) 是格子 (0,0) 的中心
        Vector3 localPos = worldPos - transform.position;
        float x = (localPos.x / cellSize) - 0.5f;
        float y = (localPos.z / cellSize) - 0.5f;

        // 2. 获取左下角的整数网格索引
        int x0 = Mathf.FloorToInt(x);
        int y0 = Mathf.FloorToInt(y);
        
        // 3. 获取其他三个角的索引
        int x1 = x0 + 1;
        int y1 = y0 + 1;

        // 4. 计算插值百分比 (tx, ty)
        float tx = x - x0; // 水平占比
        float ty = y - y0; // 垂直占比

        // 5. 获取四个角的向量 (必须检查边界和障碍物)
        Vector2 v00 = GetVectorAt(x0, y0); // 左下
        Vector2 v10 = GetVectorAt(x1, y0); // 右下
        Vector2 v01 = GetVectorAt(x0, y1); // 左上
        Vector2 v11 = GetVectorAt(x1, y1); // 右上

        // 6. 执行双线性插值
        // a = 水平插值底部
        Vector2 a = Vector2.Lerp(v00, v10, tx);
        // b = 水平插值顶部
        Vector2 b = Vector2.Lerp(v01, v11, tx);

        // 最终 = 垂直插值 a 和 b
        Vector2 result = Vector2.Lerp(a, b, ty);
        
        return result.normalized;
    }

    /// <summary>
    /// 插值帮助函数：安全地获取指定格子的向量
    /// </summary>
    private Vector2 GetVectorAt(int x, int y)
    {
        Vector2Int pos = new Vector2Int(x, y);
        if (IsValidGridPos(pos) && !grid[x, y].isObstacle)
        {
            return grid[x, y].flowVector;
        }
        // 如果是障碍物或越界，返回 0 向量 (停止)
        return Vector2.zero;
    }


    #endregion

    #region 帮助函数 (无改动)
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3 localPos = worldPos - transform.position;
        int x = Mathf.FloorToInt(localPos.x / cellSize);
        int y = Mathf.FloorToInt(localPos.z / cellSize);
        return new Vector2Int(x, y);
    }

    public bool IsValidGridPos(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridSize.x &&
               pos.y >= 0 && pos.y < gridSize.y;
    }
    #endregion

    #region Gizmo 预览 (无改动)
    private void OnDrawGizmos()
    {
        if (grid == null) { CheckAndRebuildGrid(); if (grid == null) return; }
        Vector3 cellGizmoSize = new Vector3(cellSize, 0.1f, cellSize);

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Cell cell = grid[x, y];
                Vector3 cellCenter = cell.worldPos;

                if (showGrid)
                {
                    Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.2f);
                    Gizmos.DrawWireCube(cellCenter, new Vector3(cellSize, 0, cellSize));
                }
                if (showObstacles && cell.isObstacle)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(cellCenter, cellGizmoSize);
                }
                if (showTarget && hasTarget && targetPos.x == x && targetPos.y == y)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawCube(cellCenter, cellGizmoSize * 1.5f);
                }

                #if UNITY_EDITOR
                if (cell.isObstacle) continue;
                switch (gizmoDisplay)
                {
                    case GizmoDisplayMode.CostField:
                        if (cell.cost != int.MaxValue)
                        {
                            GUIStyle style = new GUIStyle(GUI.skin.label);
                            style.alignment = TextAnchor.MiddleCenter;
                            float normalizedCost = (maxCost > 0) ? (float)cell.cost / maxCost : 0;
                            style.normal.textColor = Color.Lerp(Color.white, Color.red, normalizedCost);
                            Handles.Label(cellCenter, cell.cost.ToString(), style);
                        }
                        break;
                    case GizmoDisplayMode.FlowField:
                        if (cell.flowVector != Vector2.zero)
                        {
                            Gizmos.color = Color.cyan;
                            Vector3 start = cellCenter;
                            Vector3 end = start + new Vector3(cell.flowVector.x, 0, cell.flowVector.y) * (cellSize * 0.4f);
                            Gizmos.DrawLine(start, end);
                            Gizmos.DrawSphere(end, cellSize * 0.05f);
                        }
                        break;
                }
                #endif
            }
        }
    }
    #endregion
}