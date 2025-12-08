using UnityEngine;

/// <summary>
/// EditPoint 标记组件
/// 用于在 Scene 视图中显示编辑中心点
/// </summary>
public class EditPointMarker : MonoBehaviour
{
    [Header("显示设置")]
    public Color gizmoColor = Color.yellow;
    public float gizmoSize = 2f;
    
    private void OnDrawGizmos()
    {
        // 绘制黄色球体
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, gizmoSize);
        
        // 绘制坐标轴
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * gizmoSize * 2);
        
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * gizmoSize * 2);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.forward * gizmoSize * 2);
        
        // 绘制文字标签
#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * (gizmoSize + 1), 
            "EditPoint\n编辑中心",
            new GUIStyle()
            {
                normal = new GUIStyleState() { textColor = Color.yellow },
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            }
        );
#endif
    }
    
    private void OnDrawGizmosSelected()
    {
        // 选中时绘制实心球体
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, gizmoSize);
    }
}