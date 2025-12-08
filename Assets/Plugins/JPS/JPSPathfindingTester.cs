// JPSPathfindingTester.cs
using UnityEngine;
using System.Collections.Generic;

public class JPSPathfindingTester : MonoBehaviour
{
    public Transform target;
    public float moveSpeed = 5f;
    public float targetMoveThreshold = 1f;
    
    private JPSPathfinder pathfinder;
    private JPSGrid grid;
    private List<Vector3> currentPath;
    private int currentPathIndex = 0;
    private Vector3 lastTargetPosition;
    
    public JPSNode hoveredNode;
    
    void Start()
    {
        pathfinder = FindObjectOfType<JPSPathfinder>();
        grid = FindObjectOfType<JPSGrid>();
        lastTargetPosition = target.position;
    }
    
    void Update()
    {
        // 更新鼠标悬停的格子
        UpdateHoveredNode();
        
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            
            if (groundPlane.Raycast(ray, out float distance))
            {
                Vector3 hitPoint = ray.GetPoint(distance);
                target.position = hitPoint;
                RequestPath();
            }
        }
        
        if (!pathfinder.stepDebugMode && Vector3.Distance(target.position, lastTargetPosition) > targetMoveThreshold)
        {
            RequestPath();
            lastTargetPosition = target.position;
        }
        
        if (currentPath != null && currentPath.Count > 0 && !pathfinder.stepDebugMode)
        {
            FollowPath();
        }
    }
    
    void UpdateHoveredNode()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        
        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            hoveredNode = grid.NodeFromWorldPoint(hitPoint);
        }
        else
        {
            hoveredNode = null;
        }
    }
    
    void RequestPath()
    {
        if (pathfinder.stepDebugMode)
        {
            pathfinder.StartPathfinding(transform.position, target.position);
        }
        else
        {
            List<JPSNode> nodePath = pathfinder.FindPath(transform.position, target.position);
            
            if (nodePath != null)
            {
                currentPath = new List<Vector3>();
                foreach (JPSNode node in nodePath)
                {
                    currentPath.Add(node.worldPosition);
                }
                currentPathIndex = 0;
            }
        }
    }
    
    void FollowPath()
    {
        if (currentPathIndex >= currentPath.Count)
            return;
        
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
        if (!Application.isPlaying)
        {
            return;
        }
        
        this.pathfinder.DrawGizmos();
        
        // 绘制鼠标悬停的格子高亮
        if (hoveredNode != null)
        {
            Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
            Gizmos.DrawWireCube(hoveredNode.worldPosition + Vector3.up * 0.05f, Vector3.one * grid.nodeRadius * 2f);
        }
        
        if (!pathfinder.stepDebugMode && currentPath != null && currentPath.Count > 0)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
            }
            
            for (int i = 0; i < currentPath.Count; i++)
            {
                if (i == currentPathIndex)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(currentPath[i], 0.4f);
                }
                else
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(currentPath[i], 0.2f);
                }
            }
        }
    }
}