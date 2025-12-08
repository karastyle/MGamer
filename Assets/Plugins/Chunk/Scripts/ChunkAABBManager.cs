using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ChunkAABBData
{
    public string chunkName;
    public Vector2Int chunkIndex;
    public Bounds bounds;
    
    public ChunkAABBData(string name, Vector2Int index, Bounds aabb)
    {
        chunkName = name;
        chunkIndex = index;
        bounds = aabb;
    }
}

public class ChunkAABBManager : MonoBehaviour
{
    [Header("AABB包围盒数据")]
    public List<ChunkAABBData> chunkAABBs = new List<ChunkAABBData>();
    
    [Header("Gizmo设置")]
    public bool showGizmos = false;
    public Color gizmoColor = new Color(0f, 1f, 0f, 0.3f);
    public Color gizmoWireColor = Color.green;
    
    public void AddOrUpdateChunkAABB(string chunkName, Vector2Int chunkIndex, Bounds bounds)
    {
        int existingIndex = chunkAABBs.FindIndex(x => x.chunkName == chunkName);
        
        if (existingIndex >= 0)
        {
            chunkAABBs[existingIndex].bounds = bounds;
        }
        else
        {
            chunkAABBs.Add(new ChunkAABBData(chunkName, chunkIndex, bounds));
        }
        
        // 按chunk索引排序
        chunkAABBs.Sort((a, b) => 
        {
            if (a.chunkIndex.x != b.chunkIndex.x)
                return a.chunkIndex.x.CompareTo(b.chunkIndex.x);
            return a.chunkIndex.y.CompareTo(b.chunkIndex.y);
        });
    }
    
    public void Clear()
    {
        chunkAABBs.Clear();
    }
    
    private void OnDrawGizmos()
    {
        if (!showGizmos || chunkAABBs == null || chunkAABBs.Count == 0)
            return;
        
        foreach (var data in chunkAABBs)
        {
            // 绘制半透明实心盒子
            Gizmos.color = gizmoColor;
            Gizmos.DrawCube(data.bounds.center, data.bounds.size);
            
            // 绘制线框
            Gizmos.color = gizmoWireColor;
            Gizmos.DrawWireCube(data.bounds.center, data.bounds.size);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ChunkAABBManager))]
public class ChunkAABBManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ChunkAABBManager manager = (ChunkAABBManager)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("AABB包围盒管理器", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Gizmo设置
        EditorGUILayout.LabelField("Gizmo设置", EditorStyles.boldLabel);
        manager.showGizmos = EditorGUILayout.Toggle("显示Gizmo", manager.showGizmos);
        manager.gizmoColor = EditorGUILayout.ColorField("填充颜色", manager.gizmoColor);
        manager.gizmoWireColor = EditorGUILayout.ColorField("线框颜色", manager.gizmoWireColor);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"Chunk数量: {manager.chunkAABBs.Count}", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        if (manager.chunkAABBs.Count == 0)
        {
            EditorGUILayout.HelpBox("暂无AABB数据。在导出Chunk时会自动生成。", MessageType.Info);
            return;
        }
        
        // 统计信息
        if (GUILayout.Button("清空所有AABB数据"))
        {
            if (EditorUtility.DisplayDialog("确认", "确定要清空所有AABB数据吗？", "确定", "取消"))
            {
                manager.Clear();
                EditorUtility.SetDirty(manager);
            }
        }
        
        EditorGUILayout.Space();
        
        // 显示AABB列表
        EditorGUILayout.LabelField("AABB列表", EditorStyles.boldLabel);
        
        for (int i = 0; i < manager.chunkAABBs.Count; i++)
        {
            var data = manager.chunkAABBs[i];
            
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.LabelField($"[{i}] {data.chunkName}", EditorStyles.boldLabel);
            
            EditorGUI.indentLevel++;
            
            EditorGUILayout.LabelField($"索引: ({data.chunkIndex.x}, {data.chunkIndex.y})");
            EditorGUILayout.LabelField($"中心: {data.bounds.center.ToString("F2")}");
            EditorGUILayout.LabelField($"尺寸: {data.bounds.size.ToString("F2")}");
            EditorGUILayout.LabelField($"最小点: {data.bounds.min.ToString("F2")}");
            EditorGUILayout.LabelField($"最大点: {data.bounds.max.ToString("F2")}");
            
            EditorGUI.indentLevel--;
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }
        
        if (GUI.changed)
        {
            EditorUtility.SetDirty(manager);
        }
    }
}
#endif