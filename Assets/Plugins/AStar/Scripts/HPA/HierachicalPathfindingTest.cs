using UnityEngine;
using System.Collections.Generic;

public class HierachicalPathfindingTest : MonoBehaviour
{
    public Transform target;
    public float moveSpeed = 5f;
    
    private HierarchicalPathfinding pathfinding;
    private List<Vector3> currentPath;
    private int currentPathIndex = 0;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    void Start()
    {
        pathfinding = FindObjectOfType<HierarchicalPathfinding>();
        
        if (pathfinding == null)
        {
            Debug.LogError("HierarchicalPathfinding not found!");
        }
    }
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                target.position = hit.point;
                RequestPath();
            }
        }
        
        if (currentPath != null && currentPath.Count > 0)
        {
            FollowPath();
        }
    }
    
    void RequestPath()
    {
        float startTime = Time.realtimeSinceStartup;
        
        currentPath = pathfinding.FindPath(transform.position, target.position);
        
        float elapsed = (Time.realtimeSinceStartup - startTime) * 1000f;
        
        if (currentPath != null)
        {
            currentPathIndex = 0;
            
            if (showDebugInfo)
            {
                Debug.Log($"Path found in {elapsed:F2}ms - {currentPath.Count} waypoints");
            }
        }
        else
        {
            Debug.LogWarning($"No path found! (took {elapsed:F2}ms)");
        }
    }
    
    void FollowPath()
    {
        if (currentPathIndex >= currentPath.Count)
        {
            return;
        }
        
        Vector3 targetPosition = currentPath[currentPathIndex];
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
        
        if (Vector3.Distance(transform.position, targetPosition) < 0.3f)
        {
            currentPathIndex++;
        }
    }
    
    void OnDrawGizmos()
    {
        if (Application.isPlaying && pathfinding != null)
        {
            pathfinding.DrawGizmos();
        }
        
        // 绘制最终路径
        if (currentPath != null && currentPath.Count > 0)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                Gizmos.DrawLine(currentPath[i] + Vector3.up * 0.1f, currentPath[i + 1] + Vector3.up * 0.1f);
            }
            
            for (int i = 0; i < currentPath.Count; i++)
            {
                Gizmos.color = (i == currentPathIndex) ? Color.yellow : Color.green;
                float size = (i == currentPathIndex) ? 0.5f : 0.25f;
                Gizmos.DrawSphere(currentPath[i] + Vector3.up * 0.1f, size);
            }
        }
        
        // 绘制起点和终点
        if (target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(target.position, 0.5f);
        }
    }
}