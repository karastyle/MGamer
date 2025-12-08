using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PathfindingTest : MonoBehaviour
{
    public Transform target;
    public float moveSpeed = 5f;
    public float targetMoveThreshold = 1f; // 目标移动超过这个距离才重新寻路
    
    private Pathfinding pathfinding;
    private List<Vector3> currentPath;
    private int currentPathIndex = 0;
    private Vector3 lastTargetPosition; // 记录上次目标位置
    
    void Start()
    {
        pathfinding = FindObjectOfType<Pathfinding>();
        lastTargetPosition = target.position;
    }
    
    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                target.position = hit.point;
                RequestPath();
            }
        }
        
        // 只有目标移动了才重新寻路
        if (Vector3.Distance(target.position, lastTargetPosition) > targetMoveThreshold)
        {
            RequestPath();
            lastTargetPosition = target.position;
        }
        
        if (currentPath != null && currentPath.Count > 0)
        {
            FollowPath();
        }
    }
    
    void RequestPath()
    {
        currentPath = pathfinding.FindPath(transform.position, target.position);
        currentPathIndex = 0;
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
        if(Application.isPlaying && pathfinding != null)
            this.pathfinding.DrawGizmos();
        
        if (currentPath != null && currentPath.Count > 0)
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