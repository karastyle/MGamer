// TreeManagerEditor.cs - 添加Segmented Big Buffer支持

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(TreeManager))]
public class TreeManagerEditor : Editor
{
    private TreeManager manager;
    private int selectedPrototypeIndex = -1;
    private Vector2 scrollPosition;
    private Dictionary<int, Texture2D> prototypePreviewCache = new Dictionary<int, Texture2D>();
    private Dictionary<int, GameObject> loadedPrefabs = new Dictionary<int, GameObject>();

    private const int THUMBNAIL_SIZE = 80;
    private const int THUMBNAILS_PER_ROW = 4;

    private void OnEnable()
    {
        manager = (TreeManager)target;
        if (manager != null)
        {
            manager.LoadTreeData();
            LoadPrefabsFromJson();
        }
    }

    private void OnDisable()
    {
        prototypePreviewCache.Clear();
        loadedPrefabs.Clear();
    }

    private void LoadPrefabsFromJson()
    {
        loadedPrefabs.Clear();

        if (manager.treeData == null || manager.treeData.prototypes == null)
            return;

        for (int i = 0; i < manager.treeData.prototypes.Count; i++)
        {
            TreePrototypeInfo prototype = manager.treeData.prototypes[i];

            if (!string.IsNullOrEmpty(prototype.prefabPath))
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prototype.prefabPath);
                if (prefab != null)
                {
                    loadedPrefabs[i] = prefab;

                    if (manager.prototypePrefabs != null && i < manager.prototypePrefabs.Length)
                    {
                        manager.prototypePrefabs[i] = prefab;
                    }
                }
            }
        }
    }

    public override void OnInspectorGUI()
    {
        if (manager == null)
            return;

        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Tree Manager", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Data Section
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Data", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        SerializedProperty jsonProp = serializedObject.FindProperty("treeInstancesJson");
        EditorGUILayout.PropertyField(jsonProp, new GUIContent("Tree Instances JSON"));

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            manager.LoadTreeData();
            selectedPrototypeIndex = -1;
            prototypePreviewCache.Clear();
            LoadPrefabsFromJson();
        }

        if (manager.treeData != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Terrain Position: {manager.treeData.terrainPosition}");
            EditorGUILayout.LabelField($"Terrain Size: {manager.treeData.terrainSize}");

            if (manager.treeData.instances != null)
            {
                EditorGUILayout.LabelField($"Total Instances: {manager.treeData.instances.Count}");
            }
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // Runtime Rendering Section
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Runtime Rendering", EditorStyles.boldLabel);

        SerializedProperty enableGPUProp = serializedObject.FindProperty("enableGPUInstancing");
        if (enableGPUProp != null)
        {
            EditorGUILayout.PropertyField(enableGPUProp);
        }

        EditorGUILayout.Space();

        // Runtime控制按钮
        if (Application.isPlaying)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Initialize GPU Instancing"))
            {
                manager.InitializeGPUInstancing();
            }

            if (GUILayout.Button("Cleanup"))
            {
                manager.CleanupGPUInstancing();
            }

            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.HelpBox("Enter Play Mode to test GPU instancing.", MessageType.Info);
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // ✅ Compute Shaders Section - 添加Segmented Big Buffer支持
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Compute Shaders", EditorStyles.boldLabel);

        SerializedProperty instancedShaderProp = serializedObject.FindProperty("treeInstancedShader");
        if (instancedShaderProp != null)
        {
            EditorGUILayout.PropertyField(instancedShaderProp);
        }

        // ✅ 检测是否使用Segmented Big Buffer方案
        SerializedProperty megaCullShaderProp = serializedObject.FindProperty("megaCullShader");
        bool hasMegaCullShader = megaCullShaderProp != null;

        if (hasMegaCullShader)
        {
            EditorGUILayout.Space(5);

            // ✅ 显示优化方案选择
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Optimization Method:", EditorStyles.boldLabel, GUILayout.Width(140));

            ComputeShader megaCullShader = megaCullShaderProp.objectReferenceValue as ComputeShader;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // ✅ Mega Cull Shader字段
            EditorGUILayout.PropertyField(megaCullShaderProp,
                new GUIContent("Mega Cull Shader", "GPU Driven: Frustum+LOD+HZB合并在1个kernel"));

            if (megaCullShader != null)
            {
                // 验证Mega Cull Shader的kernel
                bool hasResetKernel = megaCullShader.HasKernel("ClearArgCounters");
                bool hasMegaKernel = megaCullShader.HasKernel("MegaCullKernel");

                if (!hasResetKernel || !hasMegaKernel)
                {
                    EditorGUILayout.HelpBox(
                        "MegaCull shader missing required kernels:\n• ClearArgCounters\n• MegaCullKernel",
                        MessageType.Error);
                }
                else
                {
                    EditorGUILayout.HelpBox("✓ Segmented Big Buffer enabled", MessageType.None);
                }
            }

            EditorGUILayout.Space(5);
        }

        // 原有shader字段
        SerializedProperty frustumCullingShaderProp = serializedObject.FindProperty("frustumCullingShader");
        if (frustumCullingShaderProp != null)
        {
            EditorGUILayout.PropertyField(frustumCullingShaderProp,
                new GUIContent("Frustum Culling", "Legacy: Per-prototype frustum culling"));
        }

        SerializedProperty hzbGeneratorShaderProp = serializedObject.FindProperty("hzbGeneratorShader");
        if (hzbGeneratorShaderProp != null)
        {
            EditorGUILayout.PropertyField(hzbGeneratorShaderProp, new GUIContent("HZB Generator"));
        }

        SerializedProperty hzbCullingShaderProp = serializedObject.FindProperty("hzbCullingShader");
        if (hzbCullingShaderProp != null)
        {
            EditorGUILayout.PropertyField(hzbCullingShaderProp,
                new GUIContent("HZB Culling", "Legacy: Per-prototype HZB culling"));
        }
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("useHZB"));

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // Debug Section
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);

        SerializedProperty bDebugProp = serializedObject.FindProperty("bDebug");
        if (bDebugProp != null)
        {
            EditorGUILayout.PropertyField(bDebugProp, new GUIContent("Enable Debug"));
        }

        SerializedProperty debugReadbackIntervalProp = serializedObject.FindProperty("debugReadbackInterval");
        if (debugReadbackIntervalProp != null)
        {
            EditorGUILayout.PropertyField(debugReadbackIntervalProp, new GUIContent("Readback Interval (s)"));
        }

        // 显示调试统计信息
        if (Application.isPlaying && manager.bDebug)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Culling Statistics:", EditorStyles.boldLabel);
            
            GUI.enabled = false;
            EditorGUILayout.IntField("Total Instances", (int)manager.totalInstances);
            EditorGUILayout.IntField("After Frustum Culling", (int)manager.afterFrustumCulling);
            EditorGUILayout.IntField("After LOD Culling", (int)manager.afterLODCulling);
            EditorGUILayout.IntField("After HZB Culling (Final)", (int)manager.afterHZBCulling);
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Rendering Statistics:", EditorStyles.boldLabel);
            EditorGUILayout.IntField("Final Triangles", (int)manager.finalTris);
            EditorGUILayout.IntField("Final Vertices", (int)manager.finalVerts);
            GUI.enabled = true;
            
            if (manager.totalInstances > 0)
            {
                float frustumPercent = (manager.afterFrustumCulling / (float)manager.totalInstances) * 100f;
                float lodPercent = (manager.afterLODCulling / (float)manager.totalInstances) * 100f;
                float finalPercent = (manager.afterHZBCulling / (float)manager.totalInstances) * 100f;
                
                EditorGUILayout.Space(3);
                EditorGUILayout.LabelField($"Frustum Pass: {frustumPercent:F1}%");
                EditorGUILayout.LabelField($"LOD Pass: {lodPercent:F1}%");
                EditorGUILayout.LabelField($"Final Visible: {finalPercent:F1}%");
                EditorGUILayout.LabelField($"Avg Tris/Instance: {(manager.afterHZBCulling > 0 ? manager.finalTris / (float)manager.afterHZBCulling : 0):F0}");
            }
            
            Repaint();
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // Gizmo Settings
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Gizmo", EditorStyles.boldLabel);

        SerializedProperty showGizmoProp = serializedObject.FindProperty("showGizmo");
        SerializedProperty gizmoColorProp = serializedObject.FindProperty("gizmoColor");

        if (showGizmoProp != null)
            EditorGUILayout.PropertyField(showGizmoProp);
        if (gizmoColorProp != null)
            EditorGUILayout.PropertyField(gizmoColorProp);

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // Prototypes Preview
        if (manager.treeData != null && manager.treeData.prototypes != null && manager.treeData.prototypes.Count > 0)
        {
            DrawPrototypesPreview();
        }
        else
        {
            EditorGUILayout.HelpBox("Load a Tree Instances JSON file to preview prototypes.", MessageType.Info);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawPrototypesPreview()
    {
        if (manager.treeData == null || manager.treeData.prototypes == null)
            return;

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField($"Prototypes ({manager.treeData.prototypes.Count})", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));

        int prototypeCount = manager.treeData.prototypes.Count;
        int rows = Mathf.CeilToInt(prototypeCount / (float)THUMBNAILS_PER_ROW);

        for (int row = 0; row < rows; row++)
        {
            EditorGUILayout.BeginHorizontal();

            for (int col = 0; col < THUMBNAILS_PER_ROW; col++)
            {
                int index = row * THUMBNAILS_PER_ROW + col;
                if (index >= prototypeCount)
                    break;

                DrawPrototypeThumbnail(index);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        if (selectedPrototypeIndex >= 0 && selectedPrototypeIndex < prototypeCount)
        {
            DrawSelectedPrototypeDetails();
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawPrototypeThumbnail(int index)
    {
        if (manager.treeData == null || manager.treeData.prototypes == null ||
            index >= manager.treeData.prototypes.Count)
            return;

        TreePrototypeInfo prototype = manager.treeData.prototypes[index];

        bool isSelected = selectedPrototypeIndex == index;
        Color originalColor = GUI.backgroundColor;

        if (isSelected)
            GUI.backgroundColor = new Color(0.3f, 0.6f, 1f);

        EditorGUILayout.BeginVertical("box", GUILayout.Width(THUMBNAIL_SIZE + 10),
            GUILayout.Height(THUMBNAIL_SIZE + 50));

        Texture2D preview = GetOrCreatePreview(index);
        Rect thumbnailRect = GUILayoutUtility.GetRect(THUMBNAIL_SIZE, THUMBNAIL_SIZE);

        if (preview != null)
        {
            GUI.DrawTexture(thumbnailRect, preview, ScaleMode.ScaleToFit);
        }
        else
        {
            EditorGUI.DrawRect(thumbnailRect, new Color(0.2f, 0.2f, 0.2f));
            GUI.Label(thumbnailRect, "No Preview", EditorStyles.centeredGreyMiniLabel);
        }

        if (Event.current.type == EventType.MouseDown && thumbnailRect.Contains(Event.current.mousePosition))
        {
            selectedPrototypeIndex = index;
            GUI.changed = true;
            Event.current.Use();
        }

        string prefabName = "Missing";
        if (loadedPrefabs.ContainsKey(index) && loadedPrefabs[index] != null)
        {
            prefabName = loadedPrefabs[index].name;
        }
        else if (!string.IsNullOrEmpty(prototype.prefabPath))
        {
            prefabName = System.IO.Path.GetFileNameWithoutExtension(prototype.prefabPath);
        }

        string label = $"#{index}\n{prefabName}";
        EditorGUILayout.LabelField(label, EditorStyles.centeredGreyMiniLabel, GUILayout.Height(40));

        EditorGUILayout.EndVertical();

        GUI.backgroundColor = originalColor;
    }

    private Texture2D GetOrCreatePreview(int index)
    {
        if (prototypePreviewCache.ContainsKey(index))
            return prototypePreviewCache[index];

        Texture2D preview = null;

        if (loadedPrefabs.ContainsKey(index) && loadedPrefabs[index] != null)
        {
            GameObject prefab = loadedPrefabs[index];
            preview = AssetPreview.GetAssetPreview(prefab);

            if (preview == null)
            {
                preview = AssetPreview.GetMiniThumbnail(prefab);
            }
        }

        prototypePreviewCache[index] = preview;
        return preview;
    }

    private void DrawSelectedPrototypeDetails()
    {
        if (manager.treeData == null || manager.treeData.prototypes == null ||
            selectedPrototypeIndex < 0 || selectedPrototypeIndex >= manager.treeData.prototypes.Count)
            return;

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Selected Prototype Details", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        TreePrototypeInfo prototype = manager.treeData.prototypes[selectedPrototypeIndex];

        EditorGUILayout.LabelField("Index:", selectedPrototypeIndex.ToString());
        EditorGUILayout.LabelField("Prefab Path:", prototype.prefabPath);

        int instanceCount = 0;
        if (manager.treeData.instances != null)
        {
            foreach (var instance in manager.treeData.instances)
            {
                if (instance.prototypeIndex == selectedPrototypeIndex)
                    instanceCount++;
            }
        }

        EditorGUILayout.LabelField("Instance Count:", instanceCount.ToString());

        EditorGUILayout.Space();

        GameObject prefab = null;
        if (loadedPrefabs.ContainsKey(selectedPrototypeIndex))
        {
            prefab = loadedPrefabs[selectedPrototypeIndex];
        }

        GUI.enabled = false;
        EditorGUILayout.ObjectField("Prefab Reference", prefab, typeof(GameObject), false);
        GUI.enabled = true;

        if (prefab == null)
        {
            EditorGUILayout.HelpBox("Prefab not found or failed to load from path.", MessageType.Warning);
        }

        EditorGUILayout.EndVertical();
    }
}