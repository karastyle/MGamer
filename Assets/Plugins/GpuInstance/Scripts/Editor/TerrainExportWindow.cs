// TerrainExportWindow.cs
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class TerrainExportWindow : EditorWindow
{
    private TerrainExportConfig config;
    private Vector2 scrollPos;
    private string configPath = "";

    [MenuItem("Tools/GPU Instancer/Terrain Export Tool")]
    public static void ShowWindow()
    {
        GetWindow<TerrainExportWindow>("Terrain Export");
    }

    private void OnGUI()
    {
        GUILayout.Label("Terrain Data Export Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 配置管理
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("New Config", GUILayout.Width(100)))
        {
            CreateNewConfig();
        }
        if (GUILayout.Button("Load Config", GUILayout.Width(100)))
        {
            LoadConfig();
        }
        if (GUILayout.Button("Save Config", GUILayout.Width(100)))
        {
            SaveConfig();
        }
        EditorGUILayout.EndHorizontal();

        if (config != null)
        {
            EditorGUILayout.LabelField("Current Config:", configPath);
        }
        else
        {
            EditorGUILayout.HelpBox("No config loaded. Create or load a config to begin.", MessageType.Info);
            return;
        }

        EditorGUILayout.Space();

        // 导出路径
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Export Root Path:", GUILayout.Width(120));
        config.exportRootPath = EditorGUILayout.TextField(config.exportRootPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Export Folder", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {
                if (path.StartsWith(Application.dataPath))
                {
                    config.exportRootPath = "Assets" + path.Substring(Application.dataPath.Length);
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Path", "Please select a folder inside the Assets directory.", "OK");
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Terrain管理按钮
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Selected Terrain", GUILayout.Height(30)))
        {
            AddSelectedTerrain();
        }
        if (GUILayout.Button("Add All Scene Terrains", GUILayout.Height(30)))
        {
            AddAllSceneTerrains();
        }
        if (GUILayout.Button("Clear All", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Clear All", "Remove all terrains from the list?", "Yes", "No"))
            {
                config.terrains.Clear();
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Terrain列表
        EditorGUILayout.LabelField($"Terrains ({config.terrains.Count}):", EditorStyles.boldLabel);
        
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        
        for (int i = config.terrains.Count - 1; i >= 0; i--)
        {
            var entry = config.terrains[i];
            
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.BeginHorizontal();
            entry.terrain = (Terrain)EditorGUILayout.ObjectField(entry.terrain, typeof(Terrain), true);
            
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                config.terrains.RemoveAt(i);
                continue;
            }
            EditorGUILayout.EndHorizontal();

            if (entry.terrain != null)
            {
                EditorGUI.indentLevel++;
                
                entry.exportHeightmap = EditorGUILayout.Toggle("Export Heightmap", entry.exportHeightmap);
                entry.exportTrees = EditorGUILayout.Toggle("Export Trees", entry.exportTrees);
                entry.exportDetails = EditorGUILayout.Toggle("Export Details", entry.exportDetails);

                if (entry.exportDetails)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Detail Layers:", GUILayout.Width(100));
                    
                    if (GUILayout.Button("Select Layers", GUILayout.Width(100)))
                    {
                        ShowDetailLayerSelector(entry);
                    }
                    
                    string layerText = entry.detailLayerIndices.Count == 0 ? "All" : string.Join(", ", entry.detailLayerIndices);
                    EditorGUILayout.LabelField(layerText);
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
        
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        // 全局导出设置
        EditorGUILayout.LabelField("Export Settings:", EditorStyles.boldLabel);
        config.heightmapFormat = (TextureFormat)EditorGUILayout.EnumPopup("Heightmap Format", config.heightmapFormat);
        config.detailFormat = (TextureFormat)EditorGUILayout.EnumPopup("Detail Format", config.detailFormat);
        config.compressTextures = EditorGUILayout.Toggle("Compress Textures", config.compressTextures);
        config.exportTreeColors = EditorGUILayout.Toggle("Export Tree Colors", config.exportTreeColors);

        EditorGUILayout.Space();

        // 导出按钮
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Export All Terrains", GUILayout.Height(40)))
        {
            ExportAllTerrains();
        }
        GUI.backgroundColor = Color.white;
    }

    private void CreateNewConfig()
    {
        string path = EditorUtility.SaveFilePanelInProject("Create Terrain Export Config", "TerrainExportConfig", "asset", "Create new config");
        if (!string.IsNullOrEmpty(path))
        {
            config = CreateInstance<TerrainExportConfig>();
            AssetDatabase.CreateAsset(config, path);
            AssetDatabase.SaveAssets();
            configPath = path;
            EditorUtility.DisplayDialog("Success", "Config created successfully!", "OK");
        }
    }

    private void LoadConfig()
    {
        string path = EditorUtility.OpenFilePanel("Load Terrain Export Config", "Assets", "asset");
        if (!string.IsNullOrEmpty(path))
        {
            if (path.StartsWith(Application.dataPath))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
                config = AssetDatabase.LoadAssetAtPath<TerrainExportConfig>(path);
                
                if (config != null)
                {
                    configPath = path;
                    EditorUtility.DisplayDialog("Success", "Config loaded successfully!", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Failed to load config file.", "OK");
                }
            }
        }
    }

    private void SaveConfig()
    {
        if (config == null)
        {
            EditorUtility.DisplayDialog("Error", "No config to save!", "OK");
            return;
        }

        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Success", "Config saved successfully!", "OK");
    }

    private void AddSelectedTerrain()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        
        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "No object selected!", "OK");
            return;
        }

        // 只处理第一个选中对象
        GameObject selected = selectedObjects[0];
        Terrain terrain = selected.GetComponent<Terrain>();
        
        if (terrain == null)
        {
            EditorUtility.DisplayDialog("Error", "Selected object is not a Terrain!", "OK");
            return;
        }

        // 检查是否已存在
        foreach (var entry in config.terrains)
        {
            if (entry.terrain == terrain)
            {
                EditorUtility.DisplayDialog("Info", "Terrain already in list.", "OK");
                return;
            }
        }

        // 添加新地形
        config.terrains.Add(new TerrainExportConfig.TerrainEntry { terrain = terrain });
        EditorUtility.SetDirty(config);
        
        EditorUtility.DisplayDialog("Success", $"Added terrain: {terrain.name}", "OK");
        Repaint();
    }

    private void AddAllSceneTerrains()
    {
        Terrain[] allTerrains = FindObjectsOfType<Terrain>();
        int addedCount = 0;
        
        foreach (Terrain terrain in allTerrains)
        {
            bool exists = false;
            foreach (var entry in config.terrains)
            {
                if (entry.terrain == terrain)
                {
                    exists = true;
                    break;
                }
            }
            
            if (!exists)
            {
                config.terrains.Add(new TerrainExportConfig.TerrainEntry { terrain = terrain });
                addedCount++;
            }
        }
        
        if (addedCount > 0)
        {
            EditorUtility.SetDirty(config);
        }
        
        EditorUtility.DisplayDialog("Success", $"Added {addedCount} terrains. Total: {config.terrains.Count}", "OK");
    }

    private void ShowDetailLayerSelector(TerrainExportConfig.TerrainEntry entry)
    {
        if (entry.terrain == null || entry.terrain.terrainData == null)
            return;

        DetailPrototype[] prototypes = entry.terrain.terrainData.detailPrototypes;
        
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("All Layers"), entry.detailLayerIndices.Count == 0, () => {
            entry.detailLayerIndices.Clear();
            EditorUtility.SetDirty(config);
        });
        
        menu.AddSeparator("");
        
        for (int i = 0; i < prototypes.Length; i++)
        {
            int index = i;
            string name = prototypes[i].prototype != null ? prototypes[i].prototype.name : 
                         (prototypes[i].prototypeTexture != null ? prototypes[i].prototypeTexture.name : $"Layer {i}");
            
            bool isSelected = entry.detailLayerIndices.Contains(index);
            
            menu.AddItem(new GUIContent($"Layer {index}: {name}"), isSelected, () => {
                if (entry.detailLayerIndices.Contains(index))
                    entry.detailLayerIndices.Remove(index);
                else
                    entry.detailLayerIndices.Add(index);
                EditorUtility.SetDirty(config);
            });
        }
        
        menu.ShowAsContext();
    }

    private void ExportAllTerrains()
    {
        if (config.terrains.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "No terrains to export!", "OK");
            return;
        }

        if (!Directory.Exists(config.exportRootPath))
        {
            Directory.CreateDirectory(config.exportRootPath);
        }

        int successCount = 0;
        for (int i = 0; i < config.terrains.Count; i++)
        {
            var entry = config.terrains[i];
            if (entry.terrain != null)
            {
                EditorUtility.DisplayProgressBar("Exporting Terrains", $"Exporting {entry.terrain.name}...", (float)i / config.terrains.Count);
                
                try
                {
                    TerrainExporter.ExportTerrain(entry.terrain, entry, config.exportRootPath, config);
                    successCount++;
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to export terrain {entry.terrain.name}: {e.Message}");
                }
            }
        }

        EditorUtility.ClearProgressBar();
        EditorUtility.DisplayDialog("Export Complete", $"Successfully exported {successCount} out of {config.terrains.Count} terrains.", "OK");
    }
}