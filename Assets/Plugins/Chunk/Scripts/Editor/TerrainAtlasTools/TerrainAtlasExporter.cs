using System.IO;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class AtlasInfo
{
    public int atlasResolution;
    public int tileSize;
    public int padding;
}

public class TerrainAtlasExporter : EditorWindow
{
    private Terrain _terrain;
    private string _exportDirectory = "";
    private TerrainAtlasExporterConfig _config;

    private int _atlasResolution = 2048;
    private int _tileSize = 512;
    private int _padding = 2;

    private int _firstLayer = 1;
    private int _secondLayerFrom = 2;
    private int _secondLayerTo = 9;
    private int _thirdLayerFrom = 10;
    private int _thirdLayerTo = 16;

    private static Shader _normalDecodeShader;

    [MenuItem("Tools/Chunk/地形贴图图集")]
    public static void ShowWindow()
    {
        TerrainAtlasExporter window = GetWindow<TerrainAtlasExporter>("地形图集导出");
        window.minSize = new Vector2(400, 300);
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("地形贴图图集导出工具", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // 配置管理
        EditorGUILayout.BeginHorizontal();
        _config = (TerrainAtlasExporterConfig)EditorGUILayout.ObjectField("配置文件", _config, typeof(TerrainAtlasExporterConfig), false);
        if (GUILayout.Button("加载配置", GUILayout.Width(80)))
        {
            LoadConfig();
        }

        if (GUILayout.Button("保存配置", GUILayout.Width(80)))
        {
            SaveConfig();
        }

        if (GUILayout.Button("新建配置", GUILayout.Width(80)))
        {
            CreateNewConfig();
        }

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        // 选择地形
        EditorGUILayout.BeginHorizontal();
        _terrain = (Terrain)EditorGUILayout.ObjectField("选择地形", _terrain, typeof(Terrain), true);
        if (GUILayout.Button("从选中添加", GUILayout.Width(100)))
        {
            if (Selection.activeGameObject != null)
            {
                Terrain terrain = Selection.activeGameObject.GetComponent<Terrain>();
                if (terrain != null)
                {
                    _terrain = terrain;
                }
                else
                {
                    EditorUtility.DisplayDialog("错误", "选中的对象不包含Terrain组件！", "确定");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("提示", "请先在场景中选中一个Terrain对象", "确定");
            }
        }

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        // 设置导出目录
        GUILayout.Label("导出目录:", EditorStyles.label);
        EditorGUILayout.BeginHorizontal();
        _exportDirectory = EditorGUILayout.TextField(_exportDirectory);
        if (GUILayout.Button("选择", GUILayout.Width(60)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("选择导出目录", "Assets", "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    _exportDirectory = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                }
                else
                {
                    EditorUtility.DisplayDialog("错误", "请选择项目内的目录（Assets文件夹下）", "确定");
                }
            }
        }

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        // 图集分辨率设置
        GUILayout.Label("图集设置:", EditorStyles.boldLabel);
        _atlasResolution = EditorGUILayout.IntField("图集分辨率", _atlasResolution);
        _tileSize = EditorGUILayout.IntField("单个Layer分辨率", _tileSize);
        _padding = EditorGUILayout.IntField("albedo和normal的扩充像素", _padding);

        // 计算可容纳的layer数量
        int maxLayers = (_atlasResolution / _tileSize) * (_atlasResolution / _tileSize);
        EditorGUILayout.LabelField($"当前设置可容纳: {maxLayers} 层", EditorStyles.miniLabel);

        GUILayout.Space(10);

        // 分层配置
        GUILayout.Label("分层配置 (用于Index/Blend贴图):", EditorStyles.boldLabel);
        _firstLayer = EditorGUILayout.IntField("打底层", _firstLayer);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("第二层区间:", GUILayout.Width(100));
        _secondLayerFrom = EditorGUILayout.IntField(_secondLayerFrom, GUILayout.Width(50));
        GUILayout.Label("-", GUILayout.Width(10));
        _secondLayerTo = EditorGUILayout.IntField(_secondLayerTo, GUILayout.Width(50));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("第三层区间:", GUILayout.Width(100));
        _thirdLayerFrom = EditorGUILayout.IntField(_thirdLayerFrom, GUILayout.Width(50));
        GUILayout.Label("-", GUILayout.Width(10));
        _thirdLayerTo = EditorGUILayout.IntField(_thirdLayerTo, GUILayout.Width(50));
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(20);

        // 显示地形信息
        if (_terrain != null)
        {
            TerrainData terrainData = _terrain.terrainData;
            if (terrainData != null)
            {
                SplatPrototype[] splats = terrainData.splatPrototypes;

                EditorGUILayout.HelpBox($"当前地形包含 {splats.Length} 层贴图", MessageType.Info);

                // 检查是否能放下
                if (splats.Length > maxLayers)
                {
                    EditorGUILayout.HelpBox($"警告: 地形层数({splats.Length})超过图集可容纳数量({maxLayers})！\n请增加图集分辨率或减小Layer分辨率", MessageType.Error);
                }

                GUILayout.Space(10);

                // 显示贴图预览
                EditorGUILayout.LabelField("贴图层预览:", EditorStyles.boldLabel);
                for (int i = 0; i < Mathf.Min(splats.Length, maxLayers); i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"Layer {i + 1}:", GUILayout.Width(60));

                    Texture2D albedoTex = splats[i].texture;
                    if (albedoTex != null)
                    {
                        if (GUILayout.Button(albedoTex, GUILayout.Width(50), GUILayout.Height(50)))
                        {
                            Selection.activeObject = albedoTex;
                            EditorGUIUtility.PingObject(albedoTex);
                        }
                    }
                    else
                    {
                        GUILayout.Box("无Albedo", GUILayout.Width(50), GUILayout.Height(50));
                    }

                    GUILayout.Space(10);

                    Texture2D normalTex = splats[i].normalMap;
                    if (normalTex != null)
                    {
                        if (GUILayout.Button(normalTex, GUILayout.Width(50), GUILayout.Height(50)))
                        {
                            Selection.activeObject = normalTex;
                            EditorGUIUtility.PingObject(normalTex);
                        }
                    }
                    else
                    {
                        GUILayout.Box("无Normal", GUILayout.Width(50), GUILayout.Height(50));
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("请先选择一个Terrain对象", MessageType.Warning);
        }

        GUILayout.Space(20);

        // 导出按钮
        GUI.enabled = _terrain != null && !string.IsNullOrEmpty(_exportDirectory);
        if (GUILayout.Button("导出Albedo和Normal图集", GUILayout.Height(40)))
        {
            ExportAtlas();
        }

        if (GUILayout.Button("导出Index和Blend贴图", GUILayout.Height(40)))
        {
            ExportIndexAndBlend();
        }

        GUI.enabled = true;
    }

    private void LoadConfig()
    {
        if (_config != null)
        {
            _terrain = _config.terrain;
            _exportDirectory = _config.exportDirectory;
            _atlasResolution = _config.atlasResolution;
            _tileSize = _config.tileSize;
            _padding = _config.padding;
            Debug.Log("[地形图集] 配置已加载");
        }
        else
        {
            EditorUtility.DisplayDialog("提示", "请先选择一个配置文件", "确定");
        }
    }

    private void SaveConfig()
    {
        if (_config != null)
        {
            _config.terrain = _terrain;
            _config.exportDirectory = _exportDirectory;
            _config.atlasResolution = _atlasResolution;
            _config.tileSize = _tileSize;
            _config.padding = _padding;
            EditorUtility.SetDirty(_config);
            AssetDatabase.SaveAssets();
            Debug.Log("[地形图集] 配置已保存");
            EditorUtility.DisplayDialog("成功", "配置已保存", "确定");
        }
        else
        {
            EditorUtility.DisplayDialog("提示", "请先选择一个配置文件或新建配置", "确定");
        }
    }

    private void CreateNewConfig()
    {
        string path = EditorUtility.SaveFilePanelInProject("新建配置文件", "TerrainAtlasConfig", "asset", "选择保存位置");
        if (!string.IsNullOrEmpty(path))
        {
            TerrainAtlasExporterConfig newConfig = ScriptableObject.CreateInstance<TerrainAtlasExporterConfig>();
            newConfig.terrain = _terrain;
            newConfig.exportDirectory = _exportDirectory;
            newConfig.atlasResolution = _atlasResolution;
            newConfig.tileSize = _tileSize;
            newConfig.padding = _padding;
            AssetDatabase.CreateAsset(newConfig, path);
            AssetDatabase.SaveAssets();
            _config = newConfig;
            Selection.activeObject = newConfig;
            EditorGUIUtility.PingObject(newConfig);
            Debug.Log($"[地形图集] 新配置已创建: {path}");
        }
    }

    private void ExportAtlas()
    {
        TerrainData terrainData = _terrain.terrainData;
        SplatPrototype[] splats = terrainData.splatPrototypes;

        if (splats.Length == 0)
        {
            EditorUtility.DisplayDialog("错误", "地形没有贴图层！", "确定");
            return;
        }

        int maxLayers = (_atlasResolution / _tileSize) * (_atlasResolution / _tileSize);
        int layerCount = Mathf.Min(splats.Length, maxLayers);

        if (splats.Length > maxLayers)
        {
            if (!EditorUtility.DisplayDialog("警告",
                    $"地形层数({splats.Length})超过图集容量({maxLayers})，只会导出前{maxLayers}层，是否继续？",
                    "继续", "取消"))
            {
                return;
            }
        }

        // 导出Albedo
        string terrainOutputDir = Path.Combine(_exportDirectory, _terrain.name);
        if (!Directory.Exists(terrainOutputDir))
        {
            Directory.CreateDirectory(terrainOutputDir);
        }

        Texture2D albedoAtlas = CreateAtlas(splats, layerCount, false);
        string albedoPath = Path.Combine(terrainOutputDir, "TerrainAlbedo_Atlas.png");
        SaveTexture(albedoAtlas, albedoPath, false, terrainOutputDir);

        // 导出Normal
        Texture2D normalAtlas = CreateAtlas(splats, layerCount, true);
        string normalPath = Path.Combine(terrainOutputDir, "TerrainNormal_Atlas.png");
        SaveTexture(normalAtlas, normalPath, true, terrainOutputDir);

        // 保存Atlas配置信息
        AtlasInfo info = new AtlasInfo
        {
            atlasResolution = _atlasResolution,
            tileSize = _tileSize,
            padding = _padding
        };

        string jsonPath = Path.Combine(terrainOutputDir, "AtlasInfo.json");
        string json = JsonUtility.ToJson(info, true);
        File.WriteAllText(jsonPath, json);
        Debug.Log($"[地形图集] 配置信息已保存: {jsonPath}");

        EditorUtility.DisplayDialog("导出成功",
            $"图集已导出:\n{albedoPath}\n{normalPath}\n\n图集分辨率: {_atlasResolution}x{_atlasResolution}\nLayer分辨率: {_tileSize}x{_tileSize}\n包含层数: {layerCount}",
            "确定");

        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Texture2D>(albedoPath);
        EditorGUIUtility.PingObject(Selection.activeObject);

        Debug.Log($"[地形图集] 导出成功");
    }

    private void ExportIndexAndBlend()
    {
        TerrainData terrainData = _terrain.terrainData;
        SplatPrototype[] splats = terrainData.splatPrototypes;

        if (splats.Length == 0)
        {
            EditorUtility.DisplayDialog("错误", "地形没有贴图层！", "确定");
            return;
        }

        Texture2D[] alphamaps = terrainData.alphamapTextures;
        if (alphamaps == null || alphamaps.Length == 0)
        {
            EditorUtility.DisplayDialog("错误", "地形没有AlphaMap数据！", "确定");
            return;
        }

        int alphaWidth = alphamaps[0].width;
        int alphaHeight = alphamaps[0].height;

        Texture2D indexTex = new Texture2D(alphaWidth, alphaHeight, TextureFormat.RGB24, false, true);
        Texture2D indexTexOriginal = new Texture2D(alphaWidth, alphaHeight, TextureFormat.RGB24, false, true);
        Texture2D blendTex = new Texture2D(alphaWidth, alphaHeight, TextureFormat.RGB24, false, true);

        int splatCount = splats.Length;

        for (int y = 0; y < alphaHeight; y++)
        {
            for (int x = 0; x < alphaWidth; x++)
            {
                float firstBlend = 0f, secondBlend = 0f, thirdBlend = 0f;
                int firstIndex = 0, secondIndex = 0, thirdIndex = 0;

                for (int i = 0; i < splatCount; i++)
                {
                    int alphaMapIndex = i / 4;
                    int channelIndex = i % 4;

                    if (alphaMapIndex >= alphamaps.Length) break;

                    Color alphaColor = alphamaps[alphaMapIndex].GetPixel(x, y);
                    float blend = 0f;

                    switch (channelIndex)
                    {
                        case 0: blend = alphaColor.r; break;
                        case 1: blend = alphaColor.g; break;
                        case 2: blend = alphaColor.b; break;
                        case 3: blend = alphaColor.a; break;
                    }

                    if (i == _firstLayer - 1)
                    {
                        firstBlend = blend;
                        firstIndex = i;
                    }
                    else if (blend > 0f && i >= _secondLayerFrom - 1 && i <= _secondLayerTo - 1)
                    {
                        secondBlend = blend;
                        secondIndex = i;
                    }
                    else if (blend > 0f && i >= _thirdLayerFrom - 1 && i <= _thirdLayerTo - 1)
                    {
                        thirdBlend = blend;
                        thirdIndex = i;
                    }
                }

                Color indexColor = new Color(firstIndex / 15f,
                    secondIndex / 15f,
                    thirdIndex / 15f);

                Color blendColor = new Color(firstBlend,
                    secondBlend,
                    thirdBlend);

                indexTex.SetPixel(x, y, indexColor);
                indexTexOriginal.SetPixel(x, y, indexColor);
                blendTex.SetPixel(x, y, blendColor);
            }
        }

        // 扩充IndexTexture边缘
        FillIndexEdge(indexTex, indexTexOriginal, alphaWidth, alphaHeight, _firstLayer - 1, 1);

        indexTex.Apply();
        blendTex.Apply();

        // 保存
        string terrainOutputDir = Path.Combine(_exportDirectory, _terrain.name);
        if (!Directory.Exists(terrainOutputDir))
        {
            Directory.CreateDirectory(terrainOutputDir);
        }

        string indexPath = Path.Combine(terrainOutputDir, "TerrainIndex.png");
        string blendPath = Path.Combine(terrainOutputDir, "TerrainBlend.png");

        SaveIndexTexture(indexTex, indexPath, terrainOutputDir);
        SaveBlendTexture(blendTex, blendPath, terrainOutputDir);

        EditorUtility.DisplayDialog("导出成功",
            $"Index和Blend贴图已导出:\n{indexPath}\n{blendPath}\n\n分辨率: {alphaWidth}x{alphaHeight}",
            "确定");

        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Texture2D>(indexPath);
        EditorGUIUtility.PingObject(Selection.activeObject);

        Debug.Log($"[地形图集] Index和Blend导出成功");
    }

    private void FillIndexEdge(Texture2D indexTex, Texture2D indexTexOriginal, int width, int height, int firstIndex, int expandPixels)
    {
        float firstIndexNormalized = firstIndex / 15f;

        // 迭代扩展N次
        for (int iter = 0; iter < expandPixels; iter++)
        {
            // 创建当前帧的快照
            Color[,] snapshot = new Color[width, height];
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                snapshot[x, y] = indexTex.GetPixel(x, y);

            // 遍历所有像素
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color currentColor = snapshot[x, y];
                    float g = currentColor.g;
                    float b = currentColor.b;

                    // 如果当前像素的G或B不是firstIndex（有实际内容）
                    if (g != firstIndexNormalized || b != firstIndexNormalized)
                    {
                        Color expandColor = new Color(firstIndexNormalized, g, b);

                        // 向四周的firstIndex像素扩展
                        if (x > 0)
                        {
                            Color leftColor = snapshot[x - 1, y];
                            if (leftColor.g == firstIndexNormalized && leftColor.b == firstIndexNormalized)
                                indexTex.SetPixel(x - 1, y, expandColor);
                        }

                        if (x < width - 1)
                        {
                            Color rightColor = snapshot[x + 1, y];
                            if (rightColor.g == firstIndexNormalized && rightColor.b == firstIndexNormalized)
                                indexTex.SetPixel(x + 1, y, expandColor);
                        }

                        if (y > 0)
                        {
                            Color bottomColor = snapshot[x, y - 1];
                            if (bottomColor.g == firstIndexNormalized && bottomColor.b == firstIndexNormalized)
                                indexTex.SetPixel(x, y - 1, expandColor);
                        }

                        if (y < height - 1)
                        {
                            Color topColor = snapshot[x, y + 1];
                            if (topColor.g == firstIndexNormalized && topColor.b == firstIndexNormalized)
                                indexTex.SetPixel(x, y + 1, expandColor);
                        }
                    }
                }
            }
        }
    }

    private void SaveIndexTexture(Texture2D texture, string path, string outputDir)
    {
        byte[] pngData = texture.EncodeToPNG();
        File.WriteAllBytes(path, pngData);

        AssetDatabase.ImportAsset(path);

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            //不压缩， Point过滤（不能设置有插值，所以不能用Bilinear采样，否则混合后的灰度值是匹配不到索引的）
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.sRGBTexture = false;
            importer.SaveAndReimport();
        }
    }

    private void SaveBlendTexture(Texture2D texture, string path, string outputDir)
    {
        byte[] pngData = texture.EncodeToPNG();
        File.WriteAllBytes(path, pngData);

        AssetDatabase.ImportAsset(path);

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.sRGBTexture = false;
            importer.SaveAndReimport();
        }
    }

    private Texture2D CreateAtlas(SplatPrototype[] splats, int layerCount, bool isNormal)
    {
        Texture2D atlas = new Texture2D(_atlasResolution, _atlasResolution, TextureFormat.RGBA32, false, !isNormal);

        Material blitMaterial;
        if (isNormal)
        {
            if (_normalDecodeShader == null)
            {
                _normalDecodeShader = CreateNormalDecodeShader();
            }

            blitMaterial = new Material(_normalDecodeShader);
        }
        else
        {
            Shader unlitShader = Shader.Find("Unlit/Texture");
            blitMaterial = new Material(unlitShader);
        }

        int padding = _padding; // 每个tile四周预留2像素
        int tileSizeWithPadding = _tileSize + padding * 2; // tile实际占用空间
        int tilesPerRow = _atlasResolution / tileSizeWithPadding;

        for (int i = 0; i < layerCount; i++)
        {
            int column = i % tilesPerRow;
            int row = i / tilesPerRow;

            // tile在图集中的起始位置（包含padding）
            int tileStartX = column * tileSizeWithPadding;
            int tileStartY = row * tileSizeWithPadding;

            // 纹理内容的实际位置（跳过padding）
            int contentStartX = tileStartX + padding;
            int contentStartY = tileStartY + padding;

            Texture2D sourceTexture = isNormal ? splats[i].normalMap : splats[i].texture;

            if (sourceTexture == null)
            {
                Color fillColor = isNormal ? new Color(0.5f, 0.5f, 1f, 1f) : Color.white;
                Color[] emptyPixels = new Color[_tileSize * _tileSize];
                for (int j = 0; j < emptyPixels.Length; j++)
                    emptyPixels[j] = fillColor;

                atlas.SetPixels(contentStartX, contentStartY, _tileSize, _tileSize, emptyPixels);

                // 填充padding区域
                FillTilePadding(atlas, contentStartX, contentStartY, _tileSize, padding, fillColor);
                continue;
            }

            // 渲染纹理内容
            RenderTexture rt = RenderTexture.GetTemporary(_tileSize, _tileSize, 0, RenderTextureFormat.ARGB32,
                isNormal ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB);
            Graphics.Blit(sourceTexture, rt, blitMaterial);

            RenderTexture.active = rt;
            Texture2D temp = new Texture2D(_tileSize, _tileSize, TextureFormat.RGBA32, false, true);
            temp.ReadPixels(new Rect(0, 0, _tileSize, _tileSize), 0, 0);
            temp.Apply();
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);

            // 写入纹理内容到中心区域
            Color[] tilePixels = temp.GetPixels();
            atlas.SetPixels(contentStartX, contentStartY, _tileSize, _tileSize, tilePixels);

            // 扩展边缘到padding区域
            ExpandTilePadding(atlas, contentStartX, contentStartY, _tileSize, padding);

            DestroyImmediate(temp);
        }

        atlas.Apply();
        DestroyImmediate(blitMaterial);

        return atlas;
    }

    private void FillTilePadding(Texture2D atlas, int contentX, int contentY, int tileSize, int padding, Color fillColor)
    {
        // 填充整个tile区域（包括padding）
        for (int y = contentY - padding; y < contentY + tileSize + padding; y++)
        {
            for (int x = contentX - padding; x < contentX + tileSize + padding; x++)
            {
                atlas.SetPixel(x, y, fillColor);
            }
        }
    }

    private void ExpandTilePadding(Texture2D atlas, int contentX, int contentY, int tileSize, int padding)
    {
        // 上边缘扩展
        for (int p = 1; p <= padding; p++)
        {
            for (int x = 0; x < tileSize; x++)
            {
                Color edgeColor = atlas.GetPixel(contentX + x, contentY + tileSize - 1);
                atlas.SetPixel(contentX + x, contentY + tileSize - 1 + p, edgeColor);
            }
        }

        // 下边缘扩展
        for (int p = 1; p <= padding; p++)
        {
            for (int x = 0; x < tileSize; x++)
            {
                Color edgeColor = atlas.GetPixel(contentX + x, contentY);
                atlas.SetPixel(contentX + x, contentY - p, edgeColor);
            }
        }

        // 右边缘扩展
        for (int p = 1; p <= padding; p++)
        {
            for (int y = 0; y < tileSize; y++)
            {
                Color edgeColor = atlas.GetPixel(contentX + tileSize - 1, contentY + y);
                atlas.SetPixel(contentX + tileSize - 1 + p, contentY + y, edgeColor);
            }
        }

        // 左边缘扩展
        for (int p = 1; p <= padding; p++)
        {
            for (int y = 0; y < tileSize; y++)
            {
                Color edgeColor = atlas.GetPixel(contentX, contentY + y);
                atlas.SetPixel(contentX - p, contentY + y, edgeColor);
            }
        }

        // 四个角扩展
        Color cornerLB = atlas.GetPixel(contentX, contentY);
        Color cornerRB = atlas.GetPixel(contentX + tileSize - 1, contentY);
        Color cornerLT = atlas.GetPixel(contentX, contentY + tileSize - 1);
        Color cornerRT = atlas.GetPixel(contentX + tileSize - 1, contentY + tileSize - 1);

        for (int px = 1; px <= padding; px++)
        {
            for (int py = 1; py <= padding; py++)
            {
                atlas.SetPixel(contentX - px, contentY - py, cornerLB); // 左下
                atlas.SetPixel(contentX + tileSize - 1 + px, contentY - py, cornerRB); // 右下
                atlas.SetPixel(contentX - px, contentY + tileSize - 1 + py, cornerLT); // 左上
                atlas.SetPixel(contentX + tileSize - 1 + px, contentY + tileSize - 1 + py, cornerRT); // 右上
            }
        }
    }

    private Shader CreateNormalDecodeShader()
    {
        string shaderCode = @"
Shader ""Hidden/TerrainAtlas/NormalDecode""
{
    Properties
    {
        _MainTex (""Texture"", 2D) = ""white"" {}
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include ""UnityCG.cginc""
            
            sampler2D _MainTex;
            
            fixed4 frag (v2f_img i) : SV_Target
            {
                // Unity DXT5nm格式：法线XY存储在AG通道
                fixed4 normalData = tex2D(_MainTex, i.uv);
                fixed3 normal;
                normal.x = normalData.a * 2.0 - 1.0;
                normal.y = normalData.g * 2.0 - 1.0;
                normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
                
                // 转换回0-1范围，保存为RGB
                return fixed4(normal * 0.5 + 0.5, 1.0);
            }
            ENDCG
        }
    }
}";
        Shader shader = ShaderUtil.CreateShaderAsset(shaderCode);
        return shader;
    }

    private void SaveTexture(Texture2D texture, string path, bool isNormal, string outputDir)
    {
        byte[] pngData = texture.EncodeToPNG();
        File.WriteAllBytes(path, pngData);

        AssetDatabase.ImportAsset(path);

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            if (isNormal)
            {
                importer.textureType = TextureImporterType.NormalMap;
            }
            else
            {
                importer.sRGBTexture = true;
            }

            importer.SaveAndReimport();
        }
    }
}