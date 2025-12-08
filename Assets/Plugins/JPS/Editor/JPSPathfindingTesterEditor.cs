// Editor/JPSPathfindingTesterEditor.cs
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class JPSPathfindingTesterEditor
{
    private static Vector3 labelOffset = new Vector3(0, 0.5f, 0);
    private static Color backgroundColor = new Color(0f, 0f, 0f, 0.6f);
    private static Vector2 backgroundPadding = new Vector2(4f, 2f);

    static JPSPathfindingTesterEditor()
    {
        // ✅ 注册全局 Scene 绘制回调（不依赖选中）
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        // ✅ 找到场景中唯一的 tester（如果有多个，可以改成 foreach）
        JPSPathfindingTester tester = Object.FindFirstObjectByType<JPSPathfindingTester>();
        if (tester == null || tester.hoveredNode == null) return;
        if (!Application.isPlaying) return;

        JPSNode node = tester.hoveredNode;

        // ✅ GUI 样式
        GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
        {
            normal = { textColor = Color.white },
            fontSize = 14,
            alignment = TextAnchor.MiddleCenter
        };

        // ✅ 计算屏幕位置
        Vector3 labelWorldPos = node.worldPosition + labelOffset;
        Vector2 guiPos = HandleUtility.WorldToGUIPoint(labelWorldPos);

        // ✅ 文本内容
        string labelText = $"({node.gridX}, {node.gridY})";
        string infoText = node.gCost < float.MaxValue
            ? $"G:{node.gCost:F1} H:{node.hCost:F1} F:{node.fCost:F1}"
            : string.Empty;

        string fullText = labelText + (string.IsNullOrEmpty(infoText) ? "" : "\n" + infoText);

        // ✅ 绘制 GUI
        Handles.BeginGUI();

        GUIContent content = new GUIContent(fullText);
        Vector2 size = style.CalcSize(content);
        size += backgroundPadding;

        Rect rect = new Rect(
            guiPos.x - size.x / 2,
            guiPos.y - size.y / 2,
            size.x,
            size.y
        );

        // ✅ 背景矩形
        EditorGUI.DrawRect(rect, backgroundColor);

        // ✅ 文字
        GUI.Label(rect, content, style);

        Handles.EndGUI();

        sceneView.Repaint();
    }
}