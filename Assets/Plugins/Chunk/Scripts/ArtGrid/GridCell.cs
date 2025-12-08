using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GridCell : MonoBehaviour
{
    public Vector2 cellSize = new Vector2(10f, 10f);
    public Color cellColor = Color.green;

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position;
        Gizmos.color = cellColor;
        
        // 绘制边框
        Gizmos.DrawLine(origin, origin + new Vector3(cellSize.x, 0, 0));
        Gizmos.DrawLine(origin + new Vector3(cellSize.x, 0, 0), origin + new Vector3(cellSize.x, 0, cellSize.y));
        Gizmos.DrawLine(origin + new Vector3(cellSize.x, 0, cellSize.y), origin + new Vector3(0, 0, cellSize.y));
        Gizmos.DrawLine(origin + new Vector3(0, 0, cellSize.y), origin);
        
        // 绘制对角线
        Gizmos.color = new Color(cellColor.r, cellColor.g, cellColor.b, 0.3f);
        Gizmos.DrawLine(origin, origin + new Vector3(cellSize.x, 0, cellSize.y));
        Gizmos.DrawLine(origin + new Vector3(cellSize.x, 0, 0), origin + new Vector3(0, 0, cellSize.y));
    }
#endif
}