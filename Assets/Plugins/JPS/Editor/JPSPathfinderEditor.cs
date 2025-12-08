// Editor/JPSPathfinderEditor.cs
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(JPSPathfinder))]
public class JPSPathfinderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        JPSPathfinder pathfinder = (JPSPathfinder)target;
        
        if (!Application.isPlaying) return;
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Step Debug Controls", EditorStyles.boldLabel);
        
        if (pathfinder.stepDebugMode)
        {
            if (GUILayout.Button("▶ Next Step", GUILayout.Height(40)))
            {
                pathfinder.StepOnce();
            }
            
            EditorGUILayout.HelpBox(
                "单步调试说明：\n" +
                "• 橙色圆：当前探索节点\n" +
                "• 紫色方框：本步新找到的跳点\n" +
                "• 蓝色方块：待探索队列(OpenList)\n" +
                "• 青色球：所有跳点\n" +
                "• 红色箭头+球：强制邻居\n" +
                "• 绿色射线：探索路径",
                MessageType.Info
            );
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField($"当前探索节点: {(pathfinder.currentExploringNode != null ? $"({pathfinder.currentExploringNode.gridX}, {pathfinder.currentExploringNode.gridY})" : "None")}");
            EditorGUILayout.LabelField($"总跳点数: {pathfinder.JumpPoints.Count}");
            EditorGUILayout.LabelField($"已探索节点: {pathfinder.ExploredNodes.Count}");
        }
    }
}