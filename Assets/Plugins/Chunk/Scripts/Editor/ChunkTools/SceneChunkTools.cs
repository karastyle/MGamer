using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 场景分块工具主窗口 v2.0
/// 代码已重构为模块化结构
/// 添加了烘焙功能
/// </summary>
public class SceneChunkTool : EditorWindow
{
    // 配置文件
    private SceneChunkConfig currentConfig = null;
    private const string LAST_CONFIG_KEY = "SceneChunkTool_LastConfigPath";
    
    // 配置参数
    private string baseNodeName = "Base";
    private string staticNodeName = "Static";
    private string terrainNodeName = "Terrain";
    private float chunkSize = 100f;
    private bool showPreview = true;
    
    private int loadRadius = 1;
    
    // 全局光照配置
    private GameObject globalLightingPrefab;
    
    // 烘焙模式选项
    private enum BakeMode
    {
        Preview = 0,  // 预览
        Final = 1     // 正式
    }
    private BakeMode currentBakeMode = BakeMode.Preview;
    
    private string sceneRootPath = "Assets/Scenes";
    private string _lightingPrefabPath = "Lighting/GlobalLightingSettings.prefab";
    private string _exportPath = "Chunks";
    private string _lightmapOutputPath = "Lightmaps";
    
    // 完整路径属性
    private string lightingPrefabPath
    {
        get => Path.Combine(sceneRootPath, _lightingPrefabPath);
        set => _lightingPrefabPath = value;
    }

    private string exportPath
    {
        get => Path.Combine(sceneRootPath, _exportPath);
        set => _exportPath = value;
    }

    private string lightmapOutputPath
    {
        get => Path.Combine(sceneRootPath, _lightmapOutputPath);
        set => _lightmapOutputPath = value;
    }
    
    // EditorPrefs 键名
    private const string PREF_BASE_NODE = "SceneChunkTool_BaseNode";
    private const string PREF_STATIC_NODE = "SceneChunkTool_StaticNode";
    private const string PREF_TERRAIN_NODE = "SceneChunkTool_TerrainNode";
    private const string PREF_CHUNK_SIZE = "SceneChunkTool_ChunkSize";
    private const string PREF_EXPORT_PATH = "SceneChunkTool_ExportPath";
    private const string PREF_LIGHTING_PREFAB = "SceneChunkTool_LightingPrefab";
    private const string PREF_SCENE_ROOT = "SceneChunkTool_SceneRoot";
    private const string PREF_LOAD_RADIUS = "SceneChunkTool_LoadRadius";
    private const string PREF_BAKE_MODE = "SceneChunkTool_BakeMode";
    private const string PREF_LIGHTMAP_OUTPUT = "SceneChunkTool_LightmapOutput";
    
    // 滚动视图位置
    private Vector2 scrollPosition;

    [MenuItem("Tools/Chunk/场景分块工具 v2.0")]
    public static void ShowWindow()
    {
        SceneChunkTool window = GetWindow<SceneChunkTool>("场景分块工具 v2.0");
        window.minSize = new Vector2(450, 800);
    }

    private void OnEnable()
    {
        LoadLastConfig();
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        GUILayout.Label("场景分块工具 v2.0", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();

        DrawConfigSection();
        EditorGUILayout.Space(10);

        DrawBasicSettings();
        EditorGUILayout.Space(10);
        
        DrawLightingSettings();
        EditorGUILayout.Space(10);
        
        DrawBakingSection();
        EditorGUILayout.Space(10);
        
        DrawFunctionButtons();
        EditorGUILayout.Space(10);

        if (EditorGUI.EndChangeCheck())
        {
            SavePreferences();
        }
        
        EditorGUILayout.EndScrollView();
    }

    #region GUI绘制

    private void DrawConfigSection()
    {
        EditorGUILayout.LabelField("配置管理", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical("box");
        
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginDisabledGroup(true);
        currentConfig = (SceneChunkConfig)EditorGUILayout.ObjectField("当前配置", currentConfig, typeof(SceneChunkConfig), false);
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("加载配置", GUILayout.Height(25)))
        {
            LoadConfigFromFile();
        }
        
        if (GUILayout.Button("保存配置", GUILayout.Height(25)))
        {
            SaveConfigToFile();
        }
        
        if (GUILayout.Button("新建配置", GUILayout.Height(25)))
        {
            CreateNewConfig();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        if (currentConfig != null)
        {
            EditorGUILayout.HelpBox($"正在使用配置: {currentConfig.name}", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("未加载配置文件，当前使用默认参数", MessageType.Warning);
        }
        
        EditorGUILayout.EndVertical();
    }

    private void DrawBasicSettings()
    {
        GUILayout.Label("基础设置", EditorStyles.boldLabel);
    
        baseNodeName = EditorGUILayout.TextField("Base节点名称:", baseNodeName);
        staticNodeName = EditorGUILayout.TextField("Static节点名称:", staticNodeName);
        terrainNodeName = EditorGUILayout.TextField("Terrain节点名称:", terrainNodeName);
        chunkSize = EditorGUILayout.FloatField("分块大小(米):", chunkSize);
    
        EditorGUILayout.Space();
    
        EditorGUILayout.BeginHorizontal();
        sceneRootPath = EditorGUILayout.TextField("场景根路径:", sceneRootPath);
        if (GUILayout.Button("浏览", GUILayout.Width(60)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("选择场景根目录", "Assets", "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    sceneRootPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                }
                else
                {
                    EditorUtility.DisplayDialog("警告", "请选择项目Assets目录下的文件夹", "确定");
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        _exportPath = EditorGUILayout.TextField("导出相对路径:", _exportPath);
        EditorGUILayout.LabelField($"完整: {exportPath}", EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();
    
        EditorGUILayout.Space();
        
        EditorGUILayout.BeginHorizontal();
        _lightmapOutputPath = EditorGUILayout.TextField("Lightmap输出路径:", _lightmapOutputPath);
        EditorGUILayout.LabelField($"完整: {lightmapOutputPath}", EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();
    
        EditorGUILayout.Space();
        showPreview = EditorGUILayout.Toggle("显示详细日志:", showPreview);
    }

    private void DrawLightingSettings()
    {
        GUILayout.Label("全局光照配置", EditorStyles.boldLabel);
    
        EditorGUILayout.BeginHorizontal();
        _lightingPrefabPath = EditorGUILayout.TextField("Prefab相对路径:", _lightingPrefabPath);
        if (GUILayout.Button("刷新", GUILayout.Width(60)))
        {
            globalLightingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(lightingPrefabPath);
            if (globalLightingPrefab == null)
            {
                EditorUtility.DisplayDialog("提示", "未找到光照配置Prefab", "确定");
            }
        }
        EditorGUILayout.EndHorizontal();
    
        EditorGUILayout.BeginVertical("box");
    
        globalLightingPrefab = (GameObject)EditorGUILayout.ObjectField(
            "光照配置Prefab:", 
            globalLightingPrefab, 
            typeof(GameObject), 
            false
        );
    
        if (globalLightingPrefab == null)
        {
            EditorGUILayout.HelpBox(
                "⚠️ 未设置全局光照配置！\n" +
                "建议先创建全局光照配置Prefab以保证各chunk光照一致。",
                MessageType.Warning
            );
        }
        else
        {
            EditorGUILayout.HelpBox(
                "✅ 已设置全局光照配置\n" +
                "包含4套光照配置（BaseScene/Chunk × 预览/正式）",
                MessageType.Info
            );
        }
    
        if (GUILayout.Button("创建/更新全局光照配置Prefab", GUILayout.Height(30)))
        {
            CreateOrUpdateGlobalLightingPrefab();
        }
    
        EditorGUILayout.EndVertical();
    }

    private void DrawBakingSection()
    {
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("烘焙管理", EditorStyles.boldLabel);
        
        // 烘焙模式选择
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("烘焙模式:", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        // 预览模式按钮
        GUI.backgroundColor = currentBakeMode == BakeMode.Preview ? new Color(0.7f, 1f, 0.7f) : Color.white;
        if (GUILayout.Button("预览模式", GUILayout.Height(35)))
        {
            currentBakeMode = BakeMode.Preview;
        }
        
        // 正式模式按钮
        GUI.backgroundColor = currentBakeMode == BakeMode.Final ? new Color(1f, 0.8f, 0.4f) : Color.white;
        if (GUILayout.Button("正式模式", GUILayout.Height(35)))
        {
            currentBakeMode = BakeMode.Final;
        }
        
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        
        // 当前模式提示
        string modeStr = currentBakeMode == BakeMode.Preview ? "预览" : "正式";
        Color modeColor = currentBakeMode == BakeMode.Preview ? 
            new Color(0.7f, 1f, 0.7f) : new Color(1f, 0.8f, 0.4f);
        
        EditorGUILayout.Space(5);
        EditorGUILayout.HelpBox(
            $"当前模式: {modeStr}\n" +
            (currentBakeMode == BakeMode.Preview ? 
                "• 预览模式：较低质量，快速烘焙\n• 适合测试和调整光照" :
                "• 正式模式：高质量烘焙\n• 适合最终发布"),
            MessageType.Info
        );
        
        EditorGUILayout.Space(10);
        
        // 烘焙按钮
        EditorGUILayout.LabelField("执行烘焙:", EditorStyles.boldLabel);
        
        bool hasLightingPrefab = globalLightingPrefab != null;
        GUI.enabled = hasLightingPrefab;
        
        // 烘焙BaseScene按钮
        GUI.backgroundColor = modeColor;
        if (GUILayout.Button($"烘焙（{modeStr}）", GUILayout.Height(40)))
        {
            BakeBaseScene();
        }
        
        EditorGUILayout.Space(5);
        
        GUI.backgroundColor = Color.white;
        
        GUI.enabled = true;
        
        if (!hasLightingPrefab)
        {
            EditorGUILayout.HelpBox("⚠️ 请先设置全局光照配置Prefab", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox(
                "💡 烘焙说明:\n" +
                "• BaseScene: 单独烘焙基础场景\n" +
                "• Chunk: 同时加载所有Chunk一起烘焙\n" +
                "• 烘焙完成后会自动记录lightmap信息并清除引用",
                MessageType.Info
            );
        }
        
        EditorGUILayout.EndVertical();
    }

    private void DrawFunctionButtons()
    {
        GUILayout.Label("场景管理", EditorStyles.boldLabel);
        
        // ===== 日常工作流程 =====
        EditorGUILayout.BeginVertical("box");
    
        // 打开工作场景
        GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);
        if (GUILayout.Button("1. 打开基础场景（BaseScene）", GUILayout.Height(40)))
        {
            ChunkExporter.OpenWorkScene(exportPath, globalLightingPrefab);
        }
        GUI.backgroundColor = Color.white;
    
        EditorGUILayout.Space(5);
    
        // 加载周围 chunk
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("加载半径:", GUILayout.Width(80));
        loadRadius = EditorGUILayout.IntField(loadRadius);
        EditorGUILayout.EndHorizontal();
    
        int totalChunks = (loadRadius * 2 + 1) * (loadRadius * 2 + 1);
        EditorGUILayout.HelpBox(
            $"加载半径 {loadRadius} 将加载约 {totalChunks} 个 chunk\n" +
            $"• r=1: 加载 3×3 = 9 个chunk\n" +
            $"• r=2: 加载 5×5 = 25 个chunk",
            MessageType.Info
        );
    
        GUI.backgroundColor = new Color(0.7f, 0.9f, 1f);
        if (GUILayout.Button($"2. 加载 EditPoint 周围 {loadRadius} 范围的Chunk", GUILayout.Height(40)))
        {
            ChunkExporter.LoadChunksAroundEditPoint(exportPath, chunkSize, loadRadius);
        }
        GUI.backgroundColor = Color.white;
    
        EditorGUILayout.Space(5);
    
        // 卸载按钮
        if (GUILayout.Button("卸载所有Chunk（保留BaseScene）", GUILayout.Height(30)))
        {
            ChunkExporter.UnloadAllChunksExceptBase();
            EditorUtility.DisplayDialog("完成", "已卸载所有 Chunk", "确定");
        }
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.EndVertical();
    
        EditorGUILayout.Space(10);
        
        // 阶段1：初始拆分
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("初始拆分（仅执行一次）", EditorStyles.boldLabel);

        GameObject existingChunk = GameObject.Find("Chunk");
        bool hasPreview = existingChunk != null && existingChunk.transform.parent == null;

        if (!hasPreview)
        {
            if (GUILayout.Button("1. 预览分块结构", GUILayout.Height(40)))
            {
                ChunkExporter.PreviewChunking(staticNodeName, chunkSize);
            }
        }
        else
        {
            GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button("1. 取消预览（恢复原结构）", GUILayout.Height(40)))
            {
                ChunkExporter.CancelPreview(staticNodeName);
            }
            GUI.backgroundColor = Color.white;
        }

        EditorGUILayout.Space(5);

        GUI.enabled = !hasPreview;
        if (GUILayout.Button("2. 导出Chunk场景", GUILayout.Height(40)))
        {
            ChunkExporter.ExportChunkScenes(
                baseNodeName,
                staticNodeName,
                chunkSize,
                exportPath,
                globalLightingPrefab,
                showPreview
            );
        }
        GUI.enabled = true;

        if (hasPreview)
        {
            EditorGUILayout.HelpBox("⚠️ 当前处于预览模式，请先取消预览再导出", MessageType.Warning);
        }

        EditorGUILayout.EndVertical();
    }

    #endregion

    #region 烘焙功能

    private void BakeBaseScene()
    {
        bool isFinal = currentBakeMode == BakeMode.Final;
        ChunkBaker.BakeAll(exportPath, globalLightingPrefab, isFinal, lightmapOutputPath);
    }

    #endregion

    #region 功能实现

    private void CreateOrUpdateGlobalLightingPrefab()
    {
        GameObject prefab;
        if (LightingConfigHelper.CreateOrUpdateGlobalLightingPrefab(lightingPrefabPath, out prefab))
        {
            globalLightingPrefab = prefab;
        }
    }

    #endregion

    #region 配置管理

    private void LoadLastConfig()
    {
        string lastConfigPath = EditorPrefs.GetString(LAST_CONFIG_KEY, "");
        if (!string.IsNullOrEmpty(lastConfigPath))
        {
            SceneChunkConfig config = AssetDatabase.LoadAssetAtPath<SceneChunkConfig>(lastConfigPath);
            if (config != null)
            {
                LoadConfigFromObject(config);
                Debug.Log($"已加载上次使用的配置: {lastConfigPath}");
            }
            else
            {
                LoadPreferences();
            }
        }
        else
        {
            LoadPreferences();
        }
    }

    private void LoadConfigFromFile()
    {
        string path = EditorUtility.OpenFilePanel("选择配置文件", "Assets", "asset");
        if (string.IsNullOrEmpty(path)) return;
        
        if (path.StartsWith(Application.dataPath))
        {
            path = "Assets" + path.Substring(Application.dataPath.Length);
        }
        
        SceneChunkConfig config = AssetDatabase.LoadAssetAtPath<SceneChunkConfig>(path);
        if (config != null)
        {
            LoadConfigFromObject(config);
            EditorPrefs.SetString(LAST_CONFIG_KEY, path);
            Debug.Log($"已加载配置: {path}");
        }
        else
        {
            EditorUtility.DisplayDialog("错误", "无法加载配置文件！", "确定");
        }
    }

    private void LoadConfigFromObject(SceneChunkConfig config)
    {
        if (config == null) return;
        
        currentConfig = config;
        
        baseNodeName = config.baseNodeName;
        staticNodeName = config.staticNodeName;
        terrainNodeName = config.terrainNodeName;
        chunkSize = config.chunkSize;
        showPreview = config.showPreview;
        loadRadius = config.loadRadius;
        
        sceneRootPath = config.sceneRootPath;
        _lightingPrefabPath = config.lightingPrefabPath;
        _exportPath = config.exportPath;
        _lightmapOutputPath = config.lightmapOutputPath;
        
        globalLightingPrefab = config.globalLightingPrefab;
        
        Repaint();
    }

    private void SaveConfigToFile()
    {
        if (currentConfig != null)
        {
            if (EditorUtility.DisplayDialog("保存配置", 
                $"是否覆盖当前配置文件 '{currentConfig.name}'?", 
                "覆盖", "另存为"))
            {
                SaveToConfigObject(currentConfig);
                EditorUtility.SetDirty(currentConfig);
                AssetDatabase.SaveAssets();
                Debug.Log($"已保存配置到: {AssetDatabase.GetAssetPath(currentConfig)}");
                return;
            }
        }
        
        string path = EditorUtility.SaveFilePanelInProject(
            "保存配置文件",
            "SceneChunkConfig",
            "asset",
            "请选择保存位置");
        
        if (string.IsNullOrEmpty(path)) return;
        
        SceneChunkConfig newConfig = ScriptableObject.CreateInstance<SceneChunkConfig>();
        SaveToConfigObject(newConfig);
        
        AssetDatabase.CreateAsset(newConfig, path);
        AssetDatabase.SaveAssets();
        
        currentConfig = newConfig;
        EditorPrefs.SetString(LAST_CONFIG_KEY, path);
        
        Debug.Log($"已保存配置到: {path}");
        EditorUtility.DisplayDialog("成功", $"配置已保存到:\n{path}", "确定");
    }

    private void SaveToConfigObject(SceneChunkConfig config)
    {
        if (config == null) return;
        
        config.baseNodeName = baseNodeName;
        config.staticNodeName = staticNodeName;
        config.terrainNodeName = terrainNodeName;
        config.chunkSize = chunkSize;
        config.showPreview = showPreview;
        config.loadRadius = loadRadius;
        
        config.sceneRootPath = sceneRootPath;
        config.lightingPrefabPath = _lightingPrefabPath;
        config.exportPath = _exportPath;
        config.lightmapOutputPath = _lightmapOutputPath;
        
        config.globalLightingPrefab = globalLightingPrefab;
    }

    private void CreateNewConfig()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "创建新配置文件",
            "SceneChunkConfig",
            "asset",
            "请选择保存位置");
        
        if (string.IsNullOrEmpty(path)) return;
        
        SceneChunkConfig newConfig = ScriptableObject.CreateInstance<SceneChunkConfig>();
        SaveToConfigObject(newConfig);
        
        AssetDatabase.CreateAsset(newConfig, path);
        AssetDatabase.SaveAssets();
        
        currentConfig = newConfig;
        EditorPrefs.SetString(LAST_CONFIG_KEY, path);
        
        Debug.Log($"已创建新配置: {path}");
        EditorUtility.DisplayDialog("成功", $"新配置已创建:\n{path}", "确定");
    }

    private void LoadPreferences()
    {
        baseNodeName = EditorPrefs.GetString(PREF_BASE_NODE, "Base");
        staticNodeName = EditorPrefs.GetString(PREF_STATIC_NODE, "Static");
        terrainNodeName = EditorPrefs.GetString(PREF_TERRAIN_NODE, "Terrain");
        chunkSize = EditorPrefs.GetFloat(PREF_CHUNK_SIZE, 100f);
        _exportPath = EditorPrefs.GetString(PREF_EXPORT_PATH, "Chunks");
        sceneRootPath = EditorPrefs.GetString(PREF_SCENE_ROOT, "Assets/Scenes");
        _lightingPrefabPath = EditorPrefs.GetString(PREF_LIGHTING_PREFAB, "Lighting/GlobalLightingSettings.prefab");
        loadRadius = EditorPrefs.GetInt(PREF_LOAD_RADIUS, 1);
        currentBakeMode = (BakeMode)EditorPrefs.GetInt(PREF_BAKE_MODE, 0);
        _lightmapOutputPath = EditorPrefs.GetString(PREF_LIGHTMAP_OUTPUT, "Lightmaps");
        
        string savedPrefabPath = EditorPrefs.GetString(PREF_LIGHTING_PREFAB, _lightingPrefabPath);
        if (!string.IsNullOrEmpty(savedPrefabPath))
        {
            globalLightingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(lightingPrefabPath);
        }
    }

    private void SavePreferences()
    {
        EditorPrefs.SetString(PREF_BASE_NODE, baseNodeName);
        EditorPrefs.SetString(PREF_STATIC_NODE, staticNodeName);
        EditorPrefs.SetString(PREF_TERRAIN_NODE, terrainNodeName);
        EditorPrefs.SetFloat(PREF_CHUNK_SIZE, chunkSize);
        EditorPrefs.SetString(PREF_EXPORT_PATH, _exportPath);
        EditorPrefs.SetString(PREF_SCENE_ROOT, sceneRootPath);
        EditorPrefs.SetString(PREF_LIGHTING_PREFAB, _lightingPrefabPath);
        EditorPrefs.SetInt(PREF_LOAD_RADIUS, loadRadius);
        EditorPrefs.SetInt(PREF_BAKE_MODE, (int)currentBakeMode);
        EditorPrefs.SetString(PREF_LIGHTMAP_OUTPUT, _lightmapOutputPath);
    }

    #endregion

    private void OnInspectorUpdate()
    {
        Repaint();
    }
}