using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MeshReadWriteBatch : EditorWindow
{
    [MenuItem("Tools/Mesh Read Write Batch")]
    public static void OpenWindow()
    {
        GetWindow<MeshReadWriteBatch>("Mesh Read/Write Batch");
    }

    private List<Mesh> _meshes = new List<Mesh>();
    private Vector2 _scrollPos;

    private void OnGUI()
    {
        EditorGUILayout.Space(5);

        // 列表
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
        for (int i = 0; i < _meshes.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            _meshes[i] = EditorGUILayout.ObjectField(_meshes[i], typeof(Mesh), false) as Mesh;
            if (GUILayout.Button("✕", GUILayout.Width(24)))
            {
                _meshes.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        // 拖入区域
        Rect dropRect = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));
        GUI.Box(dropRect, "拖入 Mesh 资产到此处");
        HandleDrop(dropRect);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("添加一行"))
            _meshes.Add(null);
        if (GUILayout.Button("清空"))
            _meshes.Clear();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("禁用 Read/Write", GUILayout.Height(30)))
            SetReadWrite(false);
        if (GUILayout.Button("启用 Read/Write", GUILayout.Height(30)))
            SetReadWrite(true);
        EditorGUILayout.EndHorizontal();
    }

    private void HandleDrop(Rect rect)
    {
        Event evt = Event.current;
        if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform) return;
        if (!rect.Contains(evt.mousePosition)) return;

        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        if (evt.type == EventType.DragPerform)
        {
            DragAndDrop.AcceptDrag();
            foreach (var obj in DragAndDrop.objectReferences)
            {
                if (obj is Mesh mesh && !_meshes.Contains(mesh))
                    _meshes.Add(mesh);
            }
        }
        evt.Use();
    }

    private void SetReadWrite(bool enable)
    {
        int count = 0;
        foreach (var mesh in _meshes)
        {
            if (mesh == null) continue;

            string path = AssetDatabase.GetAssetPath(mesh);
            var so = new SerializedObject(mesh);
            var prop = so.FindProperty("m_IsReadable");
            if (prop == null)
            {
                Debug.LogWarning($"[MeshReadWriteBatch] 未找到 m_IsReadable：{path}");
                continue;
            }

            prop.boolValue = enable;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(mesh);
            Debug.Log($"[MeshReadWriteBatch] {mesh.name} → Read/Write = {enable}");
            count++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[MeshReadWriteBatch] 完成，共处理 {count} 个 Mesh");
    }
}
