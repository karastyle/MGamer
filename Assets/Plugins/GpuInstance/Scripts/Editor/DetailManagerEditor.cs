// DetailManagerEditor.cs
using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(DetailManager))]
public class DetailManagerEditor : Editor
{
    private DetailManager manager;
    private int selectedPrototypeIndex = -1;
    private Vector2 scrollPos;

    private void OnEnable()
    {
        manager = (DetailManager)target;
    }

    public override void OnInspectorGUI()
    {
        if (manager == null) return;

        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Detail Manager (Grass)", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // DetailInstances Json Section
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Detail Instances Data", EditorStyles.boldLabel);
        
        SerializedProperty instancesProp = serializedObject.FindProperty("detailInstancesJson");
        EditorGUILayout.PropertyField(instancesProp);

        if (manager.detailInstancesJson != null)
        {
            EditorGUILayout.Space();
            if (GUILayout.Button("Parse Detail Instances", GUILayout.Height(25)))
            {
                string path = AssetDatabase.GetAssetPath(manager.detailInstancesJson);
                manager.ParseDetailInstances(path);
                serializedObject.Update();
                EditorUtility.SetDirty(manager);
            }
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // Terrain Settings
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Terrain Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("terrainPosition"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("terrainSize"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("heightMap"));
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // Runtime Settings
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Runtime Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("grassDensity"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxDistance"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("fadeStart"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxDrawLayer"));
        
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // Culling Settings
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Culling Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("occlusionOffset"));
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // Rendering
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Rendering & Culling", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("enableFrustumCulling"));
        
        EditorGUI.BeginDisabledGroup(!manager.enableFrustumCulling);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("enableHZBCulling"));
        EditorGUI.EndDisabledGroup();

        if (Application.isPlaying)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Re-Initialize"))
            {
                manager.Initialize();
            }
            if (GUILayout.Button("Cleanup"))
            {
                manager.Cleanup();
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.HelpBox("Enter Play Mode to generate grass.", MessageType.Info);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // Debug Settings
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Debug Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("bDebug"), new GUIContent("Enable Debug"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("debugReadbackInterval"), new GUIContent("Readback Interval (s)"));
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // 显示调试统计信息
        if (Application.isPlaying && manager.bDebug)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Culling Statistics:", EditorStyles.boldLabel);
            
            GUI.enabled = false;
            EditorGUILayout.IntField("Total Instances", (int)manager.totalInstances);
            EditorGUILayout.IntField("After Frustum Culling", (int)manager.afterFrustumCulling);
            EditorGUILayout.IntField("After HZB Culling (Final)", (int)manager.afterHZBCulling);
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Rendering Statistics:", EditorStyles.boldLabel);
            EditorGUILayout.IntField("Final Triangles", (int)manager.finalTris);
            EditorGUILayout.IntField("Final Vertices", (int)manager.finalVerts);
            GUI.enabled = true;
            
            if (manager.totalInstances > 0)
            {
                float frustumPercent = (manager.afterFrustumCulling / (float)manager.totalInstances) * 100f;
                float finalPercent = (manager.afterHZBCulling / (float)manager.totalInstances) * 100f;
                
                EditorGUILayout.Space(3);
                EditorGUILayout.LabelField($"Frustum Pass: {frustumPercent:F1}%");
                EditorGUILayout.LabelField($"Final Visible: {finalPercent:F1}%");
                
                if (manager.afterHZBCulling > 0)
                {
                    EditorGUILayout.LabelField($"Avg Tris/Instance: {(manager.finalTris / (float)manager.afterHZBCulling):F0}");
                }
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            
            Repaint();
        }

        // Shaders
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Compute Shaders", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("instancedShader"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("grassGenerateShader"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("frustumCullingShader"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("hzbGeneratorShader"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("hzbCullingShader"));
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // Prototypes Preview
        DrawPrototypesPreview();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawPrototypesPreview()
    {
        if (manager.detailLayers == null || manager.detailLayers.Count == 0)
            return;

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField($"Prototypes ({manager.detailLayers.Count})", EditorStyles.boldLabel);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(200));
        
        int columns = Mathf.Max(1, (int)(EditorGUIUtility.currentViewWidth / 150));
        int rows = Mathf.CeilToInt(manager.detailLayers.Count / (float)columns);

        for (int row = 0; row < rows; row++)
        {
            EditorGUILayout.BeginHorizontal();
            
            for (int col = 0; col < columns; col++)
            {
                int index = row * columns + col;
                if (index >= manager.detailLayers.Count)
                    break;

                DrawPrototypeButton(index);
            }
            
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        if (selectedPrototypeIndex >= 0 && selectedPrototypeIndex < manager.detailLayers.Count)
        {
            EditorGUILayout.Space();
            DrawSelectedPrototypeDetails();
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawPrototypeButton(int index)
    {
        var layer = manager.detailLayers[index];
        
        EditorGUILayout.BeginVertical(GUILayout.Width(120));

        bool isSelected = selectedPrototypeIndex == index;
        Color oldColor = GUI.backgroundColor;
        if (isSelected)
            GUI.backgroundColor = new Color(0.5f, 0.7f, 1f);

        Texture2D preview = null;
        if (layer.prefab != null)
        {
            preview = AssetPreview.GetAssetPreview(layer.prefab);
        }
        
        if (preview == null && layer.densityMap != null)
        {
            preview = layer.densityMap;
        }

        if (preview == null)
        {
            preview = EditorGUIUtility.whiteTexture;
        }

        Rect rect = EditorGUILayout.GetControlRect(false, 100, GUILayout.Width(100));
        rect.x += 10;

        if (GUI.Button(rect, preview, GUIStyle.none))
        {
            selectedPrototypeIndex = index;
        }

        GUI.backgroundColor = oldColor;

        string label = $"#{index}";
        if (layer.metadata != null && !string.IsNullOrEmpty(layer.metadata.prototypeName))
        {
            label += $"\n{layer.metadata.prototypeName}";
        }
        else if (layer.prefab != null)
        {
            label += $"\n{layer.prefab.name}";
        }

        EditorGUILayout.LabelField(label, EditorStyles.centeredGreyMiniLabel, GUILayout.Width(120));
        
        EditorGUILayout.EndVertical();
    }

    private void DrawSelectedPrototypeDetails()
    {
        EditorGUILayout.LabelField("Selected Prototype Details", EditorStyles.boldLabel);
        
        var layer = manager.detailLayers[selectedPrototypeIndex];

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Index:", GUILayout.Width(120));
        EditorGUILayout.LabelField(selectedPrototypeIndex.ToString());
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Prefab Path:", GUILayout.Width(120));
        string prefabPath = layer.prefab != null ? AssetDatabase.GetAssetPath(layer.prefab) : "None";
        EditorGUILayout.LabelField(prefabPath);
        EditorGUILayout.EndHorizontal();

        if (layer.metadata != null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Instance Count:", GUILayout.Width(120));
            EditorGUILayout.LabelField("N/A (Runtime)");
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Prefab Reference:", GUILayout.Width(120));
        GameObject prefab = EditorGUILayout.ObjectField(layer.prefab, typeof(GameObject), false) as GameObject;
        EditorGUILayout.EndHorizontal();

        if (layer.densityMap != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Density Map Preview:");
            Rect previewRect = EditorGUILayout.GetControlRect(false, 150);
            EditorGUI.DrawPreviewTexture(previewRect, layer.densityMap, null, ScaleMode.ScaleToFit);
        }
    }
}