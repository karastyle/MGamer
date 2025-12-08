using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace SimpleTerrainToMesh.Editor
{
    /// <summary>
    /// Simple Terrain To Mesh 编辑器窗口
    /// 用于将 Unity Terrain 转换为 Mesh
    /// </summary>
    public class SimpleTerrainToMeshWindow : EditorWindow
    {
        #region 配置参数

        // 配置文件
        private TerrainToMeshConfig currentConfig = null;
        private const string LAST_CONFIG_KEY = "TerrainToMesh_LastConfigPath";

        // 网格分割设置
        private int gridSplitX = 2;
        private int gridSplitZ = 2;
        private int verticesPerGridX = 100;
        private int verticesPerGridZ = 100;

        // 中心点位置枚举
        public enum PivotPosition
        {
            DefaultZero,
            BoundsCenter
        }

        private PivotPosition pivotPosition = PivotPosition.DefaultZero;

        // 法线计算方式枚举
        public enum NormalCalculationMode
        {
            CalculateFromMesh,
            ReadFromTerrain
        }

        private NormalCalculationMode normalMode = NormalCalculationMode.CalculateFromMesh;

        // 材质类型枚举
        public enum MaterialType
        {
            Splatmap,
            BaseMap,
            AtlasMap,
        }

        private MaterialType materialType = MaterialType.BaseMap;

        // 材质设置
        private Shader materialShader_BaseMap = null;
        private Shader materialShader_SplatMap = null;
        private Shader materialShader_AtlasMap = null;
        private bool exportPerChunk = false; // 仅 BaseMap 模式有效
        private int textureResolution_BaseMap = 2048; // BaseMap纹理分辨率
        private int textureResolution_SplatMap = 1024; // SplatMap纹理分辨率
        private bool useTexture2DArray = false; // 使用Texture 2D Array（仅SplatMap模式）
        private int textureResolution_Paint = 2048; // Paint纹理数组分辨率

        // Mesh Collider 设置
        private bool generateMeshCollider = true;

        // 路径设置
        private string rootPath = "Assets/GeneratedMeshes";
        private string meshOutputFolder = "Meshes";
        private string atlasOutputFolder = "Atlas";

        // 父节点设置
        private string parentNodeName = "TerrainStatic";
        private bool setStaticRecursively = true;

        // Terrain 列表
        private List<Terrain> terrainList = new List<Terrain>();

        // 滚动视图
        private Vector2 scrollPosition;
        private Vector2 terrainListScrollPosition;

        #endregion

        #region Unity 编辑器方法

        [MenuItem("Tools/Chunk/Simple Terrain To Mesh")]
        public static void ShowWindow()
        {
            var window = GetWindow<SimpleTerrainToMeshWindow>("Simple Terrain To Mesh");
            window.minSize = new Vector2(400, 600);
            window.Show();
        }

        private void OnEnable()
        {
            // 尝试加载上次使用的配置
            LoadLastConfig();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawHeader();
            EditorGUILayout.Space(10);

            DrawConfigSection();
            EditorGUILayout.Space(10);

            DrawTerrainSection();
            EditorGUILayout.Space(10);

            DrawGridSettings();
            EditorGUILayout.Space(10);

            DrawMeshSettings();
            EditorGUILayout.Space(10);

            DrawMaterialSettings();
            EditorGUILayout.Space(10);

            DrawPathSettings();
            EditorGUILayout.Space(10);

            DrawConversionButton();

            EditorGUILayout.EndScrollView();
        }

        #endregion

        #region UI 绘制方法

        private void DrawHeader()
        {
            GUILayout.Label("Simple Terrain To Mesh", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("将 Unity Terrain 转换为标准 Mesh 对象", MessageType.Info);
        }

        private void DrawConfigSection()
        {
            EditorGUILayout.LabelField("配置管理", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical("box");

            // 显示当前配置
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(true);
            currentConfig = (TerrainToMeshConfig)EditorGUILayout.ObjectField("当前配置", currentConfig, typeof(TerrainToMeshConfig), false);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // 按钮行
            EditorGUILayout.BeginHorizontal();

            // 加载配置
            if (GUILayout.Button("加载配置", GUILayout.Height(25)))
            {
                LoadConfigFromFile();
            }

            // 保存配置
            if (GUILayout.Button("保存配置", GUILayout.Height(25)))
            {
                SaveConfigToFile();
            }

            // 新建配置
            if (GUILayout.Button("新建配置", GUILayout.Height(25)))
            {
                CreateNewConfig();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // 提示信息
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

        private void DrawTerrainSection()
        {
            EditorGUILayout.LabelField("Terrain 设置", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical("box");

            // 添加 Terrain 按钮
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("添加当前场景所有 Terrain", GUILayout.Height(30)))
            {
                AddAllTerrainsFromScene();
            }

            if (GUILayout.Button("添加选中的 Terrain", GUILayout.Height(30)))
            {
                AddSelectedTerrains();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("清空列表", GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("确认", "是否清空所有 Terrain？", "确定", "取消"))
                {
                    terrainList.Clear();
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // 显示 Terrain 列表
            if (terrainList.Count > 0)
            {
                EditorGUILayout.LabelField($"Terrain 列表 ({terrainList.Count} 个)", EditorStyles.miniBoldLabel);

                // 列表滚动视图
                terrainListScrollPosition = EditorGUILayout.BeginScrollView(terrainListScrollPosition,
                    GUILayout.MaxHeight(300));

                // 遍历显示每个 Terrain
                for (int i = terrainList.Count - 1; i >= 0; i--)
                {
                    if (terrainList[i] == null)
                    {
                        terrainList.RemoveAt(i);
                        continue;
                    }

                    DrawTerrainListItem(terrainList[i], i);
                }

                EditorGUILayout.EndScrollView();

                EditorGUILayout.Space(5);

                // 统计信息
                DrawTerrainStatistics();
            }
            else
            {
                EditorGUILayout.HelpBox("未添加 Terrain，请点击上方按钮添加", MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制单个 Terrain 列表项
        /// </summary>
        private void DrawTerrainListItem(Terrain terrain, int index)
        {
            EditorGUILayout.BeginVertical("helpbox");

            EditorGUILayout.BeginHorizontal();

            // Terrain 引用
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField($"#{index + 1}", terrain, typeof(Terrain), true);
            EditorGUI.EndDisabledGroup();

            // 删除按钮
            if (GUILayout.Button("×", GUILayout.Width(25), GUILayout.Height(18)))
            {
                terrainList.RemoveAt(index);
                return;
            }

            EditorGUILayout.EndHorizontal();

            // Terrain 信息（可折叠）
            TerrainData terrainData = terrain.terrainData;
            if (terrainData != null)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("详细信息:", EditorStyles.miniLabel);

                Vector3 terrainSize = terrainData.size;
                EditorGUILayout.LabelField($"  尺寸: {terrainSize.x:F1} × {terrainSize.y:F1} × {terrainSize.z:F1}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"  高度图: {terrainData.heightmapResolution} × {terrainData.heightmapResolution}", EditorStyles.miniLabel);

                int layerCount = terrainData.terrainLayers != null ? terrainData.terrainLayers.Length : 0;
                int treeCount = terrainData.treeInstanceCount;
                EditorGUILayout.LabelField($"  图层: {layerCount} | 树木: {treeCount}", EditorStyles.miniLabel);

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(3);
        }

        /// <summary>
        /// 绘制 Terrain 统计信息
        /// </summary>
        private void DrawTerrainStatistics()
        {
            EditorGUILayout.BeginVertical("helpbox");
            EditorGUILayout.LabelField("汇总信息:", EditorStyles.miniBoldLabel);

            int totalGrids = gridSplitX * gridSplitZ * terrainList.Count;
            int verticesPerGrid = verticesPerGridX * verticesPerGridZ;

            EditorGUILayout.LabelField($"  将为 {terrainList.Count} 个 Terrain 生成 {totalGrids} 个网格");
            EditorGUILayout.LabelField($"  每个网格约 {verticesPerGrid:N0} 个顶点");
            EditorGUILayout.LabelField($"  预计总顶点数: {totalGrids * verticesPerGrid:N0}");

            EditorGUILayout.EndVertical();
        }

        private void DrawGridSettings()
        {
            EditorGUILayout.LabelField("网格分割设置", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical("box");

            // 分割数量
            EditorGUILayout.LabelField("分割数量 (N × N)", EditorStyles.miniBoldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("X 方向:", GUILayout.Width(60));
            gridSplitX = EditorGUILayout.IntSlider(gridSplitX, 1, 10);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Z 方向:", GUILayout.Width(60));
            gridSplitZ = EditorGUILayout.IntSlider(gridSplitZ, 1, 10);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // 每个网格的顶点数
            EditorGUILayout.LabelField("每个网格顶点数", EditorStyles.miniBoldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("X 方向:", GUILayout.Width(60));
            verticesPerGridX = EditorGUILayout.IntSlider(verticesPerGridX, 2, 500);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Z 方向:", GUILayout.Width(60));
            verticesPerGridZ = EditorGUILayout.IntSlider(verticesPerGridZ, 2, 500);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawMeshSettings()
        {
            EditorGUILayout.LabelField("Mesh 设置", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical("box");

            // 中心点位置
            pivotPosition = (PivotPosition)EditorGUILayout.EnumPopup("Pivot Position", pivotPosition);

            EditorGUILayout.Space(3);

            // 法线计算方式
            normalMode = (NormalCalculationMode)EditorGUILayout.EnumPopup("Normal Calculation", normalMode);

            EditorGUILayout.Space(3);

            // MeshCollider
            generateMeshCollider = EditorGUILayout.Toggle("Generate MeshCollider", generateMeshCollider);

            EditorGUILayout.EndVertical();
        }

        private void DrawMaterialSettings()
        {
            EditorGUILayout.LabelField("材质设置", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical("box");

            // 材质类型
            materialType = (MaterialType)EditorGUILayout.EnumPopup("Material Type", materialType);

            EditorGUILayout.Space(3);

            // BaseMap 模式下的设置
            if (materialType == MaterialType.BaseMap)
            {
                // 纹理分辨率
                textureResolution_BaseMap = EditorGUILayout.IntSlider("Texture Resolution", textureResolution_BaseMap, 256, 4096);
                EditorGUILayout.HelpBox(
                    $"BaseMap纹理分辨率: {textureResolution_BaseMap}x{textureResolution_BaseMap}\n(应用于Albedo、Normal、Metallic、Height、Occlusion等所有纹理)",
                    MessageType.Info);

                EditorGUILayout.Space(3);

                // Shader 引用
                materialShader_BaseMap = (Shader)EditorGUILayout.ObjectField("Shader", materialShader_BaseMap, typeof(Shader), false);

                EditorGUILayout.Space(3);

                EditorGUI.indentLevel++;
                exportPerChunk = EditorGUILayout.Toggle("Export Per Chunk", exportPerChunk);
                EditorGUI.indentLevel--;

                // 提示信息
                if (exportPerChunk)
                {
                    EditorGUILayout.HelpBox("每个网格将使用独立的材质和纹理（适合需要单独调整材质的情况）", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox("所有网格共享一个材质和纹理（节省内存）", MessageType.Info);
                }

                // Shader 验证提示
                if (materialShader_BaseMap == null)
                {
                    EditorGUILayout.HelpBox("⚠️ 请指定 Shader，否则将使用默认 URP/Lit Shader", MessageType.Warning);
                }
            }
            else if (materialType == MaterialType.Splatmap)
            {
                // 纹理分辨率
                textureResolution_SplatMap = EditorGUILayout.IntSlider("Texture Resolution", textureResolution_SplatMap, 256, 4096);
                EditorGUILayout.HelpBox($"SplatMap纹理分辨率: {textureResolution_SplatMap}x{textureResolution_SplatMap}\n(应用于所有生成的Splatmap纹理)",
                    MessageType.Info);

                EditorGUILayout.Space(3);

                // Shader 引用
                materialShader_SplatMap = (Shader)EditorGUILayout.ObjectField("Shader", materialShader_SplatMap, typeof(Shader), false);

                EditorGUILayout.Space(3);

                // Texture 2D Array 选项
                EditorGUI.indentLevel++;
                useTexture2DArray = EditorGUILayout.Toggle("Use Texture 2D Array", useTexture2DArray);
                EditorGUI.indentLevel--;

                // 如果使用 Texture 2D Array，显示 Paint 纹理分辨率
                if (useTexture2DArray)
                {
                    EditorGUI.indentLevel++;
                    textureResolution_Paint = EditorGUILayout.IntSlider("Paint Resolution", textureResolution_Paint, 256, 4096);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.HelpBox($"Paint纹理数组分辨率: {textureResolution_Paint}x{textureResolution_Paint}\n(应用于Diffuse、Normal、Mask纹理数组)",
                        MessageType.Info);
                }

                // Splatmap 模式的提示
                EditorGUILayout.HelpBox("Splatmap 模式：所有网格共享一个材质，使用 Terrain 的混合贴图", MessageType.Info);

                // Shader 验证提示
                if (materialShader_SplatMap == null)
                {
                    EditorGUILayout.HelpBox("⚠️ 请指定支持Splatmap的自定义Shader", MessageType.Warning);
                }
            }
            else if (materialType == MaterialType.AtlasMap)
            {
                // Shader 引用
                materialShader_AtlasMap = (Shader)EditorGUILayout.ObjectField("Shader", materialShader_AtlasMap, typeof(Shader), false);
                
                // Mesh Output Folder
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Atlas Folder:", GUILayout.Width(100));
                atlasOutputFolder = EditorGUILayout.TextField(atlasOutputFolder);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(3);

                // 显示完整路径
                string fullPath = Path.Combine(rootPath, atlasOutputFolder);
                EditorGUILayout.HelpBox($"Atlas 保存路径: {fullPath}", MessageType.None);
                
                // Shader 验证提示
                if (materialShader_AtlasMap == null)
                {
                    EditorGUILayout.HelpBox("⚠️ 请指定支持Atlasmap的自定义Shader", MessageType.Warning);
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawPathSettings()
        {
            EditorGUILayout.LabelField("路径和层级设置", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical("box");

            // Root Path
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Root Path:", GUILayout.Width(100));
            rootPath = EditorGUILayout.TextField(rootPath);
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string path = EditorUtility.OpenFolderPanel("选择根目录", "Assets", "");
                if (!string.IsNullOrEmpty(path))
                {
                    if (path.StartsWith(Application.dataPath))
                    {
                        rootPath = "Assets" + path.Substring(Application.dataPath.Length);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("错误", "请选择 Assets 文件夹内的路径", "确定");
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(3);

            // Mesh Output Folder
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Mesh Folder:", GUILayout.Width(100));
            meshOutputFolder = EditorGUILayout.TextField(meshOutputFolder);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(3);

            // 显示完整路径
            string fullPath = Path.Combine(rootPath, meshOutputFolder);
            EditorGUILayout.HelpBox($"Mesh 保存路径: {fullPath}", MessageType.None);

            EditorGUILayout.Space(10);

            // 父节点设置
            EditorGUILayout.LabelField("场景层级设置", EditorStyles.miniBoldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Parent Node:", GUILayout.Width(100));
            parentNodeName = EditorGUILayout.TextField(parentNodeName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(3);

            setStaticRecursively = EditorGUILayout.Toggle("Set Static (Recursive)", setStaticRecursively);

            EditorGUILayout.HelpBox($"生成的 Mesh 将挂载到 '{parentNodeName}' 节点下" +
                (setStaticRecursively ? "，并递归设置为 Static" : ""),
                MessageType.Info);

            EditorGUILayout.EndVertical();
        }

        private void DrawConversionButton()
        {
            EditorGUI.BeginDisabledGroup(terrainList.Count == 0);

            if (GUILayout.Button("转换所有 Terrain 为 Mesh", GUILayout.Height(40)))
            {
                ConvertTerrainToMesh();
            }

            EditorGUI.EndDisabledGroup();

            if (terrainList.Count == 0)
            {
                EditorGUILayout.HelpBox("请先添加 Terrain 才能开始转换", MessageType.Info);
            }
        }

        #endregion

        #region 功能实现方法

        /// <summary>
        /// 从场景中添加所有 Terrain
        /// </summary>
        private void AddAllTerrainsFromScene()
        {
            // 查找场景中的所有 Terrain
            Terrain[] terrainsInScene = FindObjectsByType<Terrain>(FindObjectsSortMode.None);

            if (terrainsInScene.Length == 0)
            {
                EditorUtility.DisplayDialog("错误", "场景中没有找到 Terrain 对象！", "确定");
                return;
            }

            // 过滤掉已经添加的 Terrain
            int addedCount = 0;
            foreach (var terrain in terrainsInScene)
            {
                if (terrain.terrainData == null)
                {
                    Debug.LogWarning($"Terrain '{terrain.name}' 没有 TerrainData，已跳过");
                    continue;
                }

                if (!terrainList.Contains(terrain))
                {
                    terrainList.Add(terrain);
                    addedCount++;
                }
            }

            if (addedCount > 0)
            {
                Debug.Log($"成功添加 {addedCount} 个 Terrain，当前列表共 {terrainList.Count} 个");
            }
            else
            {
                EditorUtility.DisplayDialog("提示", "所有 Terrain 已经在列表中", "确定");
            }
        }

        /// <summary>
        /// 添加当前选中的 Terrain
        /// </summary>
        private void AddSelectedTerrains()
        {
            GameObject[] selectedObjects = Selection.gameObjects;

            if (selectedObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("提示", "请先在 Hierarchy 中选中 Terrain 对象", "确定");
                return;
            }

            int addedCount = 0;
            int skippedCount = 0;

            foreach (var obj in selectedObjects)
            {
                Terrain terrain = obj.GetComponent<Terrain>();
                if (terrain == null)
                {
                    skippedCount++;
                    continue;
                }

                if (terrain.terrainData == null)
                {
                    Debug.LogWarning($"Terrain '{terrain.name}' 没有 TerrainData，已跳过");
                    skippedCount++;
                    continue;
                }

                if (!terrainList.Contains(terrain))
                {
                    terrainList.Add(terrain);
                    addedCount++;
                }
                else
                {
                    skippedCount++;
                }
            }

            if (addedCount > 0)
            {
                Debug.Log($"成功添加 {addedCount} 个 Terrain，当前列表共 {terrainList.Count} 个");
            }

            if (skippedCount > 0)
            {
                string message = addedCount > 0
                        ? $"已添加 {addedCount} 个，跳过 {skippedCount} 个（非 Terrain 或已存在）"
                        : "所选对象中没有新的 Terrain 可添加";
                EditorUtility.DisplayDialog("提示", message, "确定");
            }
        }

        /// <summary>
        /// 转换所有 Terrain 为 Mesh
        /// </summary>
        private void ConvertTerrainToMesh()
        {
            if (terrainList.Count == 0)
            {
                EditorUtility.DisplayDialog("错误", "请先添加 Terrain！", "确定");
                return;
            }

            // 移除空引用
            terrainList.RemoveAll(t => t == null || t.terrainData == null);

            if (terrainList.Count == 0)
            {
                EditorUtility.DisplayDialog("错误", "列表中的 Terrain 无效！", "确定");
                return;
            }

            int totalGrids = gridSplitX * gridSplitZ * terrainList.Count;

            // 确认对话框
            if (!EditorUtility.DisplayDialog("确认转换",
                    $"将为 {terrainList.Count} 个 Terrain 生成共 {totalGrids} 个 Mesh 文件，是否继续？",
                    "继续", "取消"))
            {
                return;
            }

            try
            {
                // 如果是BaseMap模式，先设置所有Terrain的地形层纹理为可读
                if (materialType == MaterialType.BaseMap)
                {
                    EditorUtility.DisplayProgressBar("准备中", "设置地形层纹理为可读...", 0f);
                    SetTerrainLayerTexturesReadable(terrainList);
                }

                // 创建根输出目录
                string basePath = Path.Combine(rootPath, meshOutputFolder);
                if (!AssetDatabase.IsValidFolder(basePath))
                {
                    CreateFolder(basePath);
                }

                // 查找或创建父节点
                GameObject parentNode = GameObject.Find(parentNodeName);
                if (parentNode == null)
                {
                    parentNode = new GameObject(parentNodeName);
                    Debug.Log($"创建父节点: {parentNodeName}");
                }

                int processedTerrains = 0;
                int totalTerrains = terrainList.Count;

                // 遍历处理每个 Terrain
                foreach (var terrain in terrainList)
                {
                    processedTerrains++;

                    // 使用简化的名称（不带 GUID）
                    string terrainMeshesName = $"{terrain.name}_Meshes";

                    // 检查并删除场景中已存在的同名节点
                    Transform existingChild = parentNode.transform.Find(terrainMeshesName);
                    if (existingChild != null)
                    {
                        Debug.Log($"删除已存在的节点: {terrainMeshesName}");
                        DestroyImmediate(existingChild.gameObject);
                    }

                    // 为每个 Terrain 创建单独的文件夹
                    string terrainFolderName = terrain.name;
                    string terrainOutputPath = Path.Combine(basePath, terrainFolderName);

                    if (!AssetDatabase.IsValidFolder(terrainOutputPath))
                    {
                        string parentFolder = basePath;
                        AssetDatabase.CreateFolder(parentFolder, terrainFolderName);
                    }

                    // Prefab 路径
                    string prefabPath = Path.Combine(terrainOutputPath, $"{terrainMeshesName}.prefab");

                    // 删除已存在的 Prefab
                    if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
                    {
                        Debug.Log($"删除已存在的 Prefab: {prefabPath}");
                        AssetDatabase.DeleteAsset(prefabPath);
                        AssetDatabase.Refresh();
                    }

                    // 显示进度
                    float overallProgress = (float)(processedTerrains - 1) / totalTerrains;
                    EditorUtility.DisplayProgressBar("转换中",
                        $"处理 Terrain {processedTerrains}/{totalTerrains}: {terrain.name}",
                        overallProgress);

                    // 根据材质类型选择shader和分辨率
                    Shader materialShader = null;
                    int textureResolution = 1024;

                    if (materialType == MaterialType.BaseMap)
                    {
                        materialShader = materialShader_BaseMap;
                        textureResolution = textureResolution_BaseMap;
                    }
                    else if(materialType == MaterialType.Splatmap)
                    {
                        materialShader = materialShader_SplatMap;
                        textureResolution = textureResolution_SplatMap;
                    }else if (materialType == MaterialType.AtlasMap)
                    {
                        materialShader = materialShader_AtlasMap;
                    }

                    string atlasPath = Path.Combine(rootPath, atlasOutputFolder);
                    
                    // 转换当前 Terrain（创建临时对象）
                    TerrainToMeshConverter converter = new TerrainToMeshConverter(terrain,
                        terrain.terrainData,
                        gridSplitX,
                        gridSplitZ,
                        verticesPerGridX,
                        verticesPerGridZ,
                        pivotPosition,
                        normalMode,
                        generateMeshCollider,
                        materialType,
                        materialShader,
                        exportPerChunk,
                        textureResolution,
                        useTexture2DArray,
                        textureResolution_Paint,
                        atlasPath);

                    GameObject tempMeshesRoot = converter.Convert(terrainOutputPath,
                        processedTerrains,
                        totalTerrains,
                        terrainMeshesName);

                    // 先保存为 Prefab（临时对象不在场景层级中）
                    if (tempMeshesRoot != null)
                    {
                        // 保存为 Prefab
                        GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(tempMeshesRoot, prefabPath);
                        Debug.Log($"已保存 Prefab: {prefabPath}");

                        // 删除临时对象
                        DestroyImmediate(tempMeshesRoot);

                        // 在场景中实例化 Prefab
                        GameObject prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset);
                        prefabInstance.name = terrainMeshesName;

                        // 设置父节点
                        prefabInstance.transform.SetParent(parentNode.transform, true);

                        Debug.Log($"已在场景中实例化 Prefab: {terrainMeshesName}");
                    }

                    Debug.Log($"完成 Terrain '{terrain.name}' 的转换，保存到: {terrainOutputPath}");
                }

                // 设置 Static（递归）
                if (setStaticRecursively && parentNode != null)
                {
                    EditorUtility.DisplayProgressBar("设置 Static", "递归设置 Static 标志...", 0.95f);
                    SetStaticRecursively(parentNode);
                    Debug.Log($"已将 '{parentNodeName}' 及其所有子对象设置为 Static");
                }

                EditorUtility.ClearProgressBar();

                // 刷新资源
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog("完成",
                    $"成功为 {totalTerrains} 个 Terrain 生成了 {totalGrids} 个 Mesh！\n" +
                    $"保存路径: {basePath}\n" +
                    $"场景层级: {parentNodeName}",
                    "确定");

                // 在 Hierarchy 中选中父节点
                if (parentNode != null)
                {
                    Selection.activeGameObject = parentNode;
                    EditorGUIUtility.PingObject(parentNode);
                }
            }
            catch (System.Exception e)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("错误", $"转换失败: {e.Message}", "确定");
                Debug.LogError($"Terrain 转换失败: {e}");
            }
        }

        /// <summary>
        /// 递归设置 Static 标志
        /// </summary>
        private void SetStaticRecursively(GameObject obj)
        {
            if (obj == null) return;

            // 设置当前对象为 Static
            obj.isStatic = true;

            // 递归设置所有子对象
            foreach (Transform child in obj.transform)
            {
                SetStaticRecursively(child.gameObject);
            }
        }

        /// <summary>
        /// 递归创建文件夹
        /// </summary>
        private void CreateFolder(string path)
        {
            string[] folders = path.Split('/');
            string currentPath = folders[0];

            for (int i = 1; i < folders.Length; i++)
            {
                string newFolder = folders[i];
                string checkPath = currentPath + "/" + newFolder;

                if (!AssetDatabase.IsValidFolder(checkPath))
                {
                    AssetDatabase.CreateFolder(currentPath, newFolder);
                }

                currentPath = checkPath;
            }
        }

        /// <summary>
        /// 设置所有Terrain的地形层纹理为可读
        /// </summary>
        private void SetTerrainLayerTexturesReadable(List<Terrain> terrains)
        {
            HashSet<Texture2D> processedTextures = new HashSet<Texture2D>();

            foreach (var terrain in terrains)
            {
                if (terrain == null || terrain.terrainData == null) continue;

                TerrainLayer[] layers = terrain.terrainData.terrainLayers;
                if (layers == null) continue;

                foreach (var layer in layers)
                {
                    if (layer == null) continue;

                    // 处理漫反射纹理
                    if (layer.diffuseTexture != null && !processedTextures.Contains(layer.diffuseTexture))
                    {
                        SetTextureReadable(layer.diffuseTexture);
                        processedTextures.Add(layer.diffuseTexture);
                    }

                    // 处理法线贴图
                    if (layer.normalMapTexture != null && !processedTextures.Contains(layer.normalMapTexture))
                    {
                        SetTextureReadable(layer.normalMapTexture);
                        processedTextures.Add(layer.normalMapTexture);
                    }

                    // 处理Mask贴图（如果有）
                    if (layer.maskMapTexture != null && !processedTextures.Contains(layer.maskMapTexture))
                    {
                        SetTextureReadable(layer.maskMapTexture);
                        processedTextures.Add(layer.maskMapTexture);
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"已设置 {processedTextures.Count} 个纹理为可读");
        }

        /// <summary>
        /// 设置单个纹理为可读
        /// </summary>
        private void SetTextureReadable(Texture2D texture)
        {
            if (texture == null) return;

            string path = AssetDatabase.GetAssetPath(texture);
            if (string.IsNullOrEmpty(path)) return;

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;

            if (!importer.isReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
                Debug.Log($"已设置纹理为可读: {texture.name}");
            }
        }

        #endregion

        #region 配置管理方法

        /// <summary>
        /// 加载上次使用的配置
        /// </summary>
        private void LoadLastConfig()
        {
            string lastConfigPath = EditorPrefs.GetString(LAST_CONFIG_KEY, "");
            if (!string.IsNullOrEmpty(lastConfigPath))
            {
                TerrainToMeshConfig config = AssetDatabase.LoadAssetAtPath<TerrainToMeshConfig>(lastConfigPath);
                if (config != null)
                {
                    LoadConfigFromObject(config);
                    Debug.Log($"已加载上次使用的配置: {lastConfigPath}");
                }
            }
        }

        /// <summary>
        /// 从文件加载配置
        /// </summary>
        private void LoadConfigFromFile()
        {
            string path = EditorUtility.OpenFilePanel("选择配置文件", "Assets", "asset");
            if (string.IsNullOrEmpty(path)) return;

            // 转换为相对路径
            if (path.StartsWith(Application.dataPath))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
            }

            TerrainToMeshConfig config = AssetDatabase.LoadAssetAtPath<TerrainToMeshConfig>(path);
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

        /// <summary>
        /// 从配置对象加载
        /// </summary>
        private void LoadConfigFromObject(TerrainToMeshConfig config)
        {
            if (config == null) return;

            currentConfig = config;

            // 加载所有参数
            gridSplitX = config.gridSplitX;
            gridSplitZ = config.gridSplitZ;
            verticesPerGridX = config.verticesPerGridX;
            verticesPerGridZ = config.verticesPerGridZ;

            pivotPosition = config.pivotPosition;
            normalMode = config.normalMode;
            generateMeshCollider = config.generateMeshCollider;

            materialType = config.materialType;
            materialShader_BaseMap = config.materialShader_BaseMap;
            materialShader_SplatMap = config.materialShader_SplatMap;
            materialShader_AtlasMap = config.materialShader_AtlasMap;
            exportPerChunk = config.exportPerChunk;
            textureResolution_BaseMap = config.textureResolution_BaseMap;
            textureResolution_SplatMap = config.textureResolution_SplatMap;
            useTexture2DArray = config.useTexture2DArray;
            textureResolution_Paint = config.textureResolution_Paint;

            rootPath = config.rootPath;
            meshOutputFolder = config.meshOutputFolder;
            atlasOutputFolder = config.atlasOutputFolder;

            parentNodeName = config.parentNodeName;
            setStaticRecursively = config.setStaticRecursively;

            terrainList = new List<Terrain>(config.terrainList);

            Repaint();
        }

        /// <summary>
        /// 保存当前参数到配置文件
        /// </summary>
        private void SaveConfigToFile()
        {
            // 如果有当前配置，直接保存
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

            // 另存为
            string path = EditorUtility.SaveFilePanelInProject("保存配置文件",
                "TerrainToMeshConfig",
                "asset",
                "请选择保存位置");

            if (string.IsNullOrEmpty(path)) return;

            TerrainToMeshConfig newConfig = ScriptableObject.CreateInstance<TerrainToMeshConfig>();
            SaveToConfigObject(newConfig);

            AssetDatabase.CreateAsset(newConfig, path);
            AssetDatabase.SaveAssets();

            currentConfig = newConfig;
            EditorPrefs.SetString(LAST_CONFIG_KEY, path);

            Debug.Log($"已保存配置到: {path}");
            EditorUtility.DisplayDialog("成功", $"配置已保存到:\n{path}", "确定");
        }

        /// <summary>
        /// 将当前参数保存到配置对象
        /// </summary>
        private void SaveToConfigObject(TerrainToMeshConfig config)
        {
            if (config == null) return;

            config.gridSplitX = gridSplitX;
            config.gridSplitZ = gridSplitZ;
            config.verticesPerGridX = verticesPerGridX;
            config.verticesPerGridZ = verticesPerGridZ;

            config.pivotPosition = pivotPosition;
            config.normalMode = normalMode;
            config.generateMeshCollider = generateMeshCollider;

            config.materialType = materialType;
            config.materialShader_BaseMap = materialShader_BaseMap;
            config.materialShader_SplatMap = materialShader_SplatMap;
            config.materialShader_AtlasMap = materialShader_AtlasMap;
            config.exportPerChunk = exportPerChunk;
            config.textureResolution_BaseMap = textureResolution_BaseMap;
            config.textureResolution_SplatMap = textureResolution_SplatMap;
            config.useTexture2DArray = useTexture2DArray;
            config.textureResolution_Paint = textureResolution_Paint;

            config.rootPath = rootPath;
            config.meshOutputFolder = meshOutputFolder;
            config.atlasOutputFolder = atlasOutputFolder;

            config.parentNodeName = parentNodeName;
            config.setStaticRecursively = setStaticRecursively;

            config.terrainList = new List<Terrain>(terrainList);
        }

        /// <summary>
        /// 创建新配置
        /// </summary>
        private void CreateNewConfig()
        {
            string path = EditorUtility.SaveFilePanelInProject("创建新配置文件",
                "TerrainToMeshConfig",
                "asset",
                "请选择保存位置");

            if (string.IsNullOrEmpty(path)) return;

            TerrainToMeshConfig newConfig = ScriptableObject.CreateInstance<TerrainToMeshConfig>();
            SaveToConfigObject(newConfig);

            AssetDatabase.CreateAsset(newConfig, path);
            AssetDatabase.SaveAssets();

            currentConfig = newConfig;
            EditorPrefs.SetString(LAST_CONFIG_KEY, path);

            Debug.Log($"已创建新配置: {path}");
            EditorUtility.DisplayDialog("成功", $"新配置已创建:\n{path}", "确定");
        }

        #endregion
    }
}