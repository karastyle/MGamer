// 放置在 Assets/Scripts/JPSRuntimeTester.cs
using System.Collections.Generic;
using UnityEngine;

namespace JPSPlus
{
// 放置在 Assets/Scripts/JPSRuntimeTester.cs
using System.Collections.Generic;
using UnityEngine;

public class JPSRuntimeTester : MonoBehaviour
{
    [Header("References")]
    public JPSGrid grid;
    public Transform startMarker;
    public Transform endMarker;

    [Header("Movement")]
    public float moveSpeed = 5f;
    
    private JPSPathfinder pathfinder;
    private List<Vector3> path = new List<Vector3>();
    private int currentPathIndex = 0;
    private Camera mainCamera;
    private Plane gridPlane;
    
    // 用于预览
    private HashSet<Int2> exploredJumpPoints;

    void Start()
    {
        if (grid == null || grid.bakedData == null)
        {
            Debug.LogError("JPS Grid or Baked Data is missing!", this);
            this.enabled = false;
            return;
        }

        if (startMarker == null || endMarker == null)
        {
            Debug.LogError("Start or End marker is not assigned!", this);
            this.enabled = false;
            return;
        }
        
        // 1. 初始化寻路器
        pathfinder = new JPSPathfinder(grid.bakedData);
        mainCamera = Camera.main;
        
        // 创建一个用于鼠标射线检测的平面
        gridPlane = new Plane(Vector3.up, grid.transform.position);
        
        // 立即为初始位置寻路
        RequestPath();
    }

    void Update()
    {
        // 1. 检查鼠标点击
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (gridPlane.Raycast(ray, out float enter))
            {
                // 设置目标点位置
                endMarker.position = ray.GetPoint(enter);
                // 请求新路径
                RequestPath();
            }
        }

        // 2. 跟随路径
        if (path.Count > 0)
        {
            FollowPath();
        }
    }

    /// <summary>
    /// 请求JPS+寻路
    /// </summary>
    void RequestPath()
    {
        if (pathfinder == null) return;
        
        // 重置路径索引
        currentPathIndex = 0;
        
        // 调用寻路
        bool foundPath = pathfinder.FindPath(startMarker.position, endMarker.position, path);
        
        // 获取探索过的节点用于预览
        exploredJumpPoints = pathfinder.GetExploredJumpPoints();
        
        if (!foundPath)
        {
            Debug.LogWarning("JPS+ 未找到路径");
        }
    }

    /// <summary>
    /// 沿路径移动StartMarker
    /// </summary>
    void FollowPath()
    {
        if (currentPathIndex >= path.Count)
        {
            return; // 已到达终点
        }

        // 1. 获取当前目标路点
        Vector3 targetWaypoint = path[currentPathIndex];
        
        // 2. 移动
        startMarker.position = Vector3.MoveTowards(
            startMarker.position, 
            targetWaypoint, 
            moveSpeed * Time.deltaTime
        );
        
        // 3. 转向
        Vector3 direction = (targetWaypoint - startMarker.position).normalized;
        direction.y = 0; // 保持水平
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            startMarker.rotation = Quaternion.Slerp(startMarker.rotation, lookRotation, Time.deltaTime * moveSpeed * 2f);
        }

        // 4. 检查是否到达路点
        if (Vector3.Distance(startMarker.position, targetWaypoint) < 0.1f)
        {
            currentPathIndex++; // 移动到下一个路点
        }
    }

    // 5. 绘制预览
    void OnDrawGizmos()
    {
        if (!Application.isPlaying || pathfinder == null || grid == null)
            return;

        // --- 绘制A*探索过的跳点 (Closed List) ---
        if (exploredJumpPoints != null)
        {
            Gizmos.color = new Color(0, 1, 1, 0.3f); // 青色
            foreach (var nodePos in exploredJumpPoints)
            {
                Gizmos.DrawCube(
                    grid.GetWorldPosition(nodePos.x, nodePos.y), 
                    new Vector3(grid.NodeDiameter, 0.05f, grid.NodeDiameter)
                );
            }
        }

        // --- 绘制最终路径 ---
        if (path != null && path.Count > 0)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < path.Count - 1; i++)
            {
                Gizmos.DrawLine(path[i] + Vector3.up * 0.1f, path[i+1] + Vector3.up * 0.1f);
            }
            
            Gizmos.color = Color.yellow;
            foreach (var point in path)
            {
                Gizmos.DrawSphere(point + Vector3.up * 0.1f, grid.nodeRadius * 0.2f);
            }
        }
    }
}

}