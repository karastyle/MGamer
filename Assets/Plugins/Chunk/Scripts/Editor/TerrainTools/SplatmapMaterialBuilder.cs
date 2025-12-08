using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace SimpleTerrainToMesh.Editor
{
    /// <summary>
    /// Splatmap材质构建器
    /// 负责从Terrain数据创建和配置Splatmap材质
    /// </summary>
    public class SplatmapMaterialBuilder
    {
        private readonly Terrain terrain;
        private readonly TerrainData terrainData;
        private readonly Vector3 terrainSize;
        private readonly int textureResolution;
        private readonly bool useTexture2DArray;
        private readonly int textureResolution_Paint;
        private static Shader _normalDecodeShader;

        public SplatmapMaterialBuilder(Terrain terrain, int textureResolution, bool useTexture2DArray, int textureResolution_Paint)
        {
            this.terrain = terrain;
            this.terrainData = terrain.terrainData;
            this.terrainSize = terrainData.size;
            this.textureResolution = textureResolution;
            this.useTexture2DArray = useTexture2DArray;
            this.textureResolution_Paint = textureResolution_Paint;
        }

        /// <summary>
        /// 创建并配置Splatmap材质
        /// </summary>
        public Material CreateSplatmapMaterial(Shader splatmapShader, string outputPath, string materialName)
        {
            if (splatmapShader == null)
            {
                Debug.LogError("Splatmap Shader未指定！");
                return null;
            }

            Material material = new Material(splatmapShader);
            material.name = materialName;

            TerrainLayer[] layers = terrainData.terrainLayers;
            if (layers == null || layers.Length == 0)
            {
                Debug.LogWarning("Terrain没有地形层，无法创建Splatmap材质");
                return material;
            }

            // 1. 设置Layer数量
            int layerCount = layers.Length;
            material.SetFloat("_T2M_Layer_Count", layerCount);

            // 启用Layer Count关键字
            string layerCountKeyword = $"_T2M_LAYER_COUNT_{layerCount}";
            material.EnableKeyword(layerCountKeyword);

            Debug.Log($"[Splatmap] 设置Layer数量: {layerCount}, 关键字: {layerCountKeyword}");

            // 2. 设置每个Layer的参数
            for (int i = 0; i < layerCount; i++)
            {
                SetLayerProperties(material, layers[i], i);
            }

            // 3. 设置Splatmap贴图（Control贴图/权重图）
            SetSplatmapTextures(material, outputPath);

            // 4. 如果使用Texture 2D Array，生成并设置纹理数组
            if (useTexture2DArray)
            {
                GenerateAndSetTexture2DArrays(material, outputPath, materialName);

                // ⭐ 启用纹理数组关键字
                material.EnableKeyword("_T2M_TEXTURE_SAMPLE_TYPE_ARRAY");
                Debug.Log("[Splatmap] 已启用纹理数组关键字: _T2M_TEXTURE_SAMPLE_TYPE_ARRAY");
            }

            // 保存材质前诊断
            DiagnoseMaterial(material);

            // 5. 保存材质
            string materialPath = Path.Combine(outputPath, $"{materialName}.mat");
            AssetDatabase.CreateAsset(material, materialPath);
            AssetDatabase.SaveAssets();

            return material;
        }

        /// <summary>
        /// 在SplatmapMaterialBuilder中添加诊断方法
        /// </summary>
        public void DiagnoseMaterial(Material material)
        {
            Debug.Log("========== 材质诊断 ==========");

            // 1. 检查是否使用纹理数组模式
            bool hasArrayKeyword = material.IsKeywordEnabled("_T2M_TEXTURE_SAMPLE_TYPE_ARRAY");
            Debug.Log($"纹理数组关键字启用: {hasArrayKeyword}");

            // 2. 检查纹理数组是否赋值
            Texture2DArray splatmapsArray = material.GetTexture("_T2M_SplatMaps2DArray") as Texture2DArray;
            Texture2DArray diffuseArray = material.GetTexture("_T2M_DiffuseMaps2DArray") as Texture2DArray;
            Texture2DArray normalArray = material.GetTexture("_T2M_NormalMaps2DArray") as Texture2DArray;
            Texture2DArray maskArray = material.GetTexture("_T2M_MaskMaps2DArray") as Texture2DArray;

            Debug.Log($"Splatmaps Array: {(splatmapsArray != null ? $"{splatmapsArray.depth}层" : "未设置")}");
            Debug.Log($"Diffuse Array: {(diffuseArray != null ? $"{diffuseArray.depth}层" : "未设置")}");
            Debug.Log($"Normal Array: {(normalArray != null ? $"{normalArray.depth}层" : "未设置")}");
            Debug.Log($"Mask Array: {(maskArray != null ? $"{maskArray.depth}层" : "未设置")}");

            // 3. 检查Layer Count
            int layerCount = (int)material.GetFloat("_T2M_Layer_Count");
            Debug.Log($"Layer Count: {layerCount}");

            // 4. 检查所有启用的关键字
            Debug.Log("启用的关键字:");
            foreach (string keyword in material.shaderKeywords)
            {
                Debug.Log($"  - {keyword}");
            }

            // 5. 检查每个Layer的MapsUsage
            for (int i = 0; i < layerCount; i++)
            {
                Vector4 mapsUsage = material.GetVector($"_T2M_Layer_{i}_MapsUsage");
                Debug.Log($"Layer {i} MapsUsage: {mapsUsage} (x={mapsUsage.x} 表示是否使用Diffuse数组)");
            }

            Debug.Log("==============================");
        }

        /// <summary>
        /// 设置单个Layer的所有属性
        /// </summary>
        private void SetLayerProperties(Material material, TerrainLayer layer, int layerIndex)
        {
            if (layer == null)
            {
                Debug.LogWarning($"Layer {layerIndex} 为空，跳过");
                return;
            }

            string prefix = $"_T2M_Layer_{layerIndex}";

            // === 1. Diffuse 贴图和颜色 ===
            if (useTexture2DArray)
            {
                // Texture2DArray模式：设置MapsUsage来标记是否使用数组
                Vector4 mapsUsage = Vector4.zero;
                if (layer.diffuseTexture != null)
                {
                    mapsUsage.x = 1f; // 标记使用Diffuse数组
                }

                material.SetVector($"{prefix}_MapsUsage", mapsUsage);
            }
            else
            {
                // 单纹理模式
                if (layer.diffuseTexture != null)
                {
                    material.SetTexture($"{prefix}_Diffuse", layer.diffuseTexture);
                }
                else
                {
                    material.SetTexture($"{prefix}_Diffuse", Texture2D.whiteTexture);
                    Debug.LogWarning($"Layer {layerIndex} 没有Diffuse贴图");
                }
            }

            material.SetColor($"{prefix}_ColorTint", Color.white);

            // === 2. Normal 贴图 ===
            if (layer.normalMapTexture != null)
            {
                if (!useTexture2DArray)
                {
                    material.SetTexture($"{prefix}_NormalMap", layer.normalMapTexture);
                }

                material.SetFloat($"{prefix}_NormalScale", layer.normalScale);
                material.EnableKeyword($"_T2M_LAYER_{layerIndex}_NORMAL");
                Debug.Log($"Layer {layerIndex}: 启用Normal关键字");
            }
            else
            {
                if (!useTexture2DArray)
                {
                    material.SetTexture($"{prefix}_NormalMap", Texture2D.normalTexture);
                }

                material.SetFloat($"{prefix}_NormalScale", 1.0f);
                material.DisableKeyword($"_T2M_LAYER_{layerIndex}_NORMAL");
            }

            // === 3. Mask 贴图 ===
            if (layer.maskMapTexture != null)
            {
                if (!useTexture2DArray)
                {
                    material.SetTexture($"{prefix}_Mask", layer.maskMapTexture);
                }

                material.EnableKeyword($"_T2M_LAYER_{layerIndex}_MASK");

                Vector4 remapMin = Vector4.zero;
                Vector4 remapMax = Vector4.one;
                GetMaskMapRemapping(layer, ref remapMin, ref remapMax);
                material.SetVector($"{prefix}_MaskMapRemapMin", remapMin);
                material.SetVector($"{prefix}_MaskMapRemapMax", remapMax);

                Debug.Log($"Layer {layerIndex}: 启用Mask关键字");
            }
            else
            {
                if (!useTexture2DArray)
                {
                    material.SetTexture($"{prefix}_Mask", null);
                }

                material.DisableKeyword($"_T2M_LAYER_{layerIndex}_MASK");

                Vector4 metallicOcclusionSmoothness = new Vector4(layer.metallic,
                    1.0f,
                    0,
                    layer.smoothness);
                material.SetVector($"{prefix}_MetallicOcclusionSmoothness", metallicOcclusionSmoothness);

                Debug.Log($"Layer {layerIndex}: 使用直接值 Metallic={layer.metallic}, Smoothness={layer.smoothness}");
            }

            // === 4. UV 缩放和偏移 ===
            // Terrain的tileSize表示纹理在多少米范围内重复一次
            // 所以Scale = terrainSize / tileSize     这里就表示在整个terrain要重复多少次
            // shader内  uv的计算应该是： uv * scale + offset
            // 注意：mesh的UV是0-1的terrain归一化坐标，所以直接用terrainSize计算
            Vector2 tileSize = layer.tileSize;
            Vector2 tileOffset = layer.tileOffset;

            float scaleX = terrainSize.x / tileSize.x;
            float scaleY = terrainSize.z / tileSize.y;

            Vector4 uvScaleOffset = new Vector4(scaleX, // Scale X - 整个terrain的缩放
                scaleY, // Scale Y
                tileOffset.x, // Offset X
                tileOffset.y // Offset Y
            );
            material.SetVector($"{prefix}_uvScaleOffset", uvScaleOffset);

            Debug.Log(
                $"Layer {layerIndex}: TileSize=({tileSize.x}, {tileSize.y}), TerrainSize=({terrainSize.x}, {terrainSize.z}), UV Scale=({scaleX}, {scaleY}), Offset=({tileOffset.x}, {tileOffset.y})");

            // === 5. 其他参数（使用默认值）===
            material.SetFloat($"{prefix}_SmoothnessFromDiffuseAlpha", 0);
            material.SetFloat($"{prefix}_OpacityAsDensity", 0);

            // === 6. 启用Layer渲染关键字（Layer 2及以上需要）===
            if (layerIndex >= 2)
            {
                material.EnableKeyword($"RENDER_LAYER_{layerIndex}");
            }
        }

        /// <summary>
        /// 设置Splatmap贴图（权重图）
        /// </summary>
        private void SetSplatmapTextures(Material material, string outputPath)
        {
            int alphamapCount = terrainData.alphamapTextureCount;

            Debug.Log($"[Splatmap] Alphamap数量: {alphamapCount}");

            for (int i = 0; i < alphamapCount; i++)
            {
                Texture2D originalAlphamap = terrainData.alphamapTextures[i];

                if (originalAlphamap != null)
                {
                    // 生成新的Splatmap贴图
                    Texture2D newSplatmap = GenerateSplatmapTexture(i, originalAlphamap);

                    if (newSplatmap != null)
                    {
                        // 保存为PNG文件
                        string splatmapPath = Path.Combine(outputPath, $"Splatmap_{i}.png");
                        byte[] pngData = newSplatmap.EncodeToPNG();
                        File.WriteAllBytes(splatmapPath, pngData);

                        // 刷新AssetDatabase
                        AssetDatabase.Refresh();

                        // 重新加载并设置导入设置
                        TextureImporter importer = AssetImporter.GetAtPath(splatmapPath) as TextureImporter;
                        if (importer != null)
                        {
                            importer.isReadable = true;
                            importer.textureCompression = TextureImporterCompression.Uncompressed;
                            importer.mipmapEnabled = false;
                            importer.sRGBTexture = false; // Splatmap是线性数据
                            AssetDatabase.ImportAsset(splatmapPath, ImportAssetOptions.ForceUpdate);
                        }

                        // 加载保存的贴图
                        Texture2D savedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(splatmapPath);

                        material.SetTexture($"_T2M_SplatMap_{i}", savedTexture);
                        Debug.Log($"设置SplatMap_{i}: {savedTexture.name} ({savedTexture.width}x{savedTexture.height})");

                        // 启用Splatmap渲染关键字（Splatmap 1及以上需要）
                        if (i >= 1)
                        {
                            material.EnableKeyword($"RENDER_SPLATMAP_{i}");
                            Debug.Log($"启用关键字: RENDER_SPLATMAP_{i}");
                        }

                        // 清理临时纹理
                        Object.DestroyImmediate(newSplatmap);
                    }
                }
                else
                {
                    Debug.LogWarning($"Alphamap {i} 为空");
                }
            }
        }

        /// <summary>
        /// 获取Mask Map的Channel Remapping参数
        /// </summary>
        private void GetMaskMapRemapping(TerrainLayer layer, ref Vector4 remapMin, ref Vector4 remapMax)
        {
            remapMin = Vector4.zero;
            remapMax = Vector4.one;

#if UNITY_2021_2_OR_NEWER
            try
            {
                // 使用反射获取maskMapRemapMin和maskMapRemapMax属性
                var maskMapRemapMinProp = layer.GetType().GetProperty("maskMapRemapMin");
                var maskMapRemapMaxProp = layer.GetType().GetProperty("maskMapRemapMax");

                if (maskMapRemapMinProp != null && maskMapRemapMaxProp != null)
                {
                    remapMin = (Vector4)maskMapRemapMinProp.GetValue(layer);
                    remapMax = (Vector4)maskMapRemapMaxProp.GetValue(layer);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"获取MaskMapRemap失败: {e.Message}");
            }
#endif
        }

        /// <summary>
        /// 生成指定分辨率的Splatmap贴图（使用双线性插值）
        /// </summary>
        private Texture2D GenerateSplatmapTexture(int splatmapIndex, Texture2D originalAlphamap)
        {
            int width = textureResolution;
            int height = textureResolution;

            Texture2D newTexture = new Texture2D(width, height, TextureFormat.RGBA32, false, true);

            // 获取原始alphamap的分辨率
            int originalWidth = terrainData.alphamapWidth;
            int originalHeight = terrainData.alphamapHeight;

            // 获取alphamap数据（每个像素包含4个通道的权重值）
            float[,,] alphamaps = terrainData.GetAlphamaps(0, 0, originalWidth, originalHeight);

            // 计算当前splatmap对应的layer起始索引
            int layerStartIndex = splatmapIndex * 4;
            int totalLayers = terrainData.alphamapLayers;

            Color[] pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // 计算在原始alphamap中的精确坐标（浮点数）
                    float u = (float)x / (width - 1) * (originalWidth - 1);
                    float v = (float)y / (height - 1) * (originalHeight - 1);

                    // 获取四个相邻像素的坐标
                    int x0 = Mathf.FloorToInt(u);
                    int x1 = Mathf.Min(x0 + 1, originalWidth - 1);
                    int y0 = Mathf.FloorToInt(v);
                    int y1 = Mathf.Min(y0 + 1, originalHeight - 1);

                    // 计算插值系数
                    float fx = u - x0;
                    float fy = v - y0;

                    // 对每个通道进行双线性插值
                    float r = BilinearInterpolate(alphamaps, x0, y0, x1, y1, fx, fy, layerStartIndex + 0, totalLayers);
                    float g = BilinearInterpolate(alphamaps, x0, y0, x1, y1, fx, fy, layerStartIndex + 1, totalLayers);
                    float b = BilinearInterpolate(alphamaps, x0, y0, x1, y1, fx, fy, layerStartIndex + 2, totalLayers);
                    float a = BilinearInterpolate(alphamaps, x0, y0, x1, y1, fx, fy, layerStartIndex + 3, totalLayers);

                    pixels[y * width + x] = new Color(r, g, b, a);
                }
            }

            newTexture.SetPixels(pixels);
            newTexture.Apply();

            Debug.Log($"生成Splatmap_{splatmapIndex}: {width}x{height} (双线性插值)");

            return newTexture;
        }

        /// <summary>
        /// 双线性插值辅助方法
        /// </summary>
        private float BilinearInterpolate(float[,,] alphamaps, int x0, int y0, int x1, int y1, float fx, float fy, int layerIndex, int totalLayers)
        {
            // 检查layer索引是否有效
            if (layerIndex >= totalLayers)
                return 0f;

            // 获取四个角的值
            float v00 = alphamaps[y0, x0, layerIndex]; // 左下
            float v10 = alphamaps[y0, x1, layerIndex]; // 右下
            float v01 = alphamaps[y1, x0, layerIndex]; // 左上
            float v11 = alphamaps[y1, x1, layerIndex]; // 右上

            // 双线性插值公式
            float v0 = Mathf.Lerp(v00, v10, fx); // 下边插值
            float v1 = Mathf.Lerp(v01, v11, fx); // 上边插值
            float result = Mathf.Lerp(v0, v1, fy); // 垂直插值

            return result;
        }

        /// <summary>
        /// 生成并设置Texture 2D Array
        /// </summary>
        private void GenerateAndSetTexture2DArrays(Material material, string outputPath, string materialName)
        {
            TerrainLayer[] layers = terrainData.terrainLayers;
            int layerCount = layers.Length;

            if (layerCount == 0)
            {
                Debug.LogWarning("[Texture2DArray] 没有地形层，跳过Texture2DArray生成");
                return;
            }

            Debug.Log($"[Texture2DArray] 开始生成Texture 2D Arrays，共{layerCount}层");

            // 1. 生成 Splatmaps Texture2DArray - 修改属性名 ⭐
            Texture2DArray splatmapsArray = GenerateSplatmapsArray(outputPath, materialName);
            if (splatmapsArray != null)
            {
                material.SetTexture("_T2M_SplatMaps2DArray", splatmapsArray); // ⭐ 改这里
                Debug.Log($"[Texture2DArray] 已设置 Splatmaps Array");
            }

            // 2. 生成 Paint (Diffuse) Texture2DArray - 修改属性名 ⭐
            Texture2DArray diffuseArray = GeneratePaintDiffuseArray(layers, outputPath, materialName);
            if (diffuseArray != null)
            {
                material.SetTexture("_T2M_DiffuseMaps2DArray", diffuseArray); // ⭐ 改这里
                Debug.Log($"[Texture2DArray] 已设置 Diffuse Array");
            }

            // 3. 生成 Paint (Normal) Texture2DArray - 修改属性名 ⭐
            Texture2DArray normalArray = GeneratePaintNormalArray(layers, outputPath, materialName);
            if (normalArray != null)
            {
                material.SetTexture("_T2M_NormalMaps2DArray", normalArray); // ⭐ 改这里
                Debug.Log($"[Texture2DArray] 已设置 Normal Array");
            }

            // 4. 生成 Paint (Mask) Texture2DArray - 修改属性名 ⭐
            Texture2DArray maskArray = GeneratePaintMaskArray(layers, outputPath, materialName);
            if (maskArray != null)
            {
                material.SetTexture("_T2M_MaskMaps2DArray", maskArray); // ⭐ 改这里
                Debug.Log($"[Texture2DArray] 已设置 Mask Array");
            }

            Debug.Log($"[Texture2DArray] Texture 2D Arrays 生成并设置完成");
        }

        /// <summary>
        /// 生成Splatmaps Texture2DArray
        /// </summary>
        private Texture2DArray GenerateSplatmapsArray(string outputPath, string materialName)
        {
            int alphamapCount = terrainData.alphamapTextureCount;

            if (alphamapCount == 0)
            {
                Debug.LogWarning("[Texture2DArray] 没有Alphamap");
                return null;
            }

            // 创建Texture2DArray
            Texture2DArray splatmapsArray = new Texture2DArray(textureResolution,
                textureResolution,
                alphamapCount,
                TextureFormat.RGBA32,
                true,
                false);

            splatmapsArray.filterMode = FilterMode.Bilinear;
            splatmapsArray.wrapMode = TextureWrapMode.Clamp;

            // 为每个alphamap生成纹理并添加到数组
            for (int i = 0; i < alphamapCount; i++)
            {
                Texture2D originalAlphamap = terrainData.alphamapTextures[i];

                if (originalAlphamap != null)
                {
                    Texture2D splatmap = GenerateSplatmapTexture(i, originalAlphamap);

                    if (splatmap != null)
                    {
                        // 将纹理数据复制到数组的对应层
                        Graphics.CopyTexture(splatmap, 0, 0, splatmapsArray, i, 0);

                        // 清理临时纹理
                        Object.DestroyImmediate(splatmap);
                    }
                }
            }

            splatmapsArray.Apply(true, false);

            // 保存为asset
            string arrayPath = Path.Combine(outputPath, $"{materialName}_SplatmapsArray.asset");
            AssetDatabase.CreateAsset(splatmapsArray, arrayPath);

            return AssetDatabase.LoadAssetAtPath<Texture2DArray>(arrayPath);
        }

        /// <summary>
        /// 生成Paint (Diffuse) Texture2DArray
        /// </summary>
        private Texture2DArray GeneratePaintDiffuseArray(TerrainLayer[] layers, string outputPath, string materialName)
        {
            int layerCount = layers.Length;

            // 先统计有多少layer有diffuse纹理
            List<int> validLayerIndices = new List<int>();
            for (int i = 0; i < layerCount; i++)
            {
                if (layers[i] != null && layers[i].diffuseTexture != null)
                {
                    validLayerIndices.Add(i);
                }
            }

            if (validLayerIndices.Count == 0)
            {
                Debug.LogWarning("[Texture2DArray] 没有有效的Diffuse贴图");
                return null;
            }

            // 创建Texture2DArray - 只包含有纹理的layer
            Texture2DArray diffuseArray = new Texture2DArray(textureResolution_Paint,
                textureResolution_Paint,
                validLayerIndices.Count, // 数组深度 = 有效layer数量
                TextureFormat.RGBA32,
                true,
                false);

            diffuseArray.filterMode = FilterMode.Trilinear;
            diffuseArray.wrapMode = TextureWrapMode.Repeat;
            diffuseArray.anisoLevel = 9;

            // 只添加有纹理的layer
            for (int arrayIndex = 0; arrayIndex < validLayerIndices.Count; arrayIndex++)
            {
                int layerIndex = validLayerIndices[arrayIndex];
                TerrainLayer layer = layers[layerIndex];

                Texture2D scaledTex = ScaleTexture(layer.diffuseTexture, textureResolution_Paint, textureResolution_Paint, false);
                Graphics.CopyTexture(scaledTex, 0, 0, diffuseArray, arrayIndex, 0);
                Object.DestroyImmediate(scaledTex);
            }

            diffuseArray.Apply(true, false);

            string arrayPath = Path.Combine(outputPath, $"{materialName}_DiffuseArray.asset");
            AssetDatabase.CreateAsset(diffuseArray, arrayPath);

            return AssetDatabase.LoadAssetAtPath<Texture2DArray>(arrayPath);
        }

        /// <summary>
        /// 生成Paint (Normal) Texture2DArray
        /// </summary>
        private Texture2DArray GeneratePaintNormalArray(TerrainLayer[] layers, string outputPath, string materialName)
        {
            int layerCount = layers.Length;

            // 统计有normal的layer
            List<int> validLayerIndices = new List<int>();
            for (int i = 0; i < layerCount; i++)
            {
                if (layers[i] != null && layers[i].normalMapTexture != null)
                {
                    validLayerIndices.Add(i);
                }
            }

            if (validLayerIndices.Count == 0)
            {
                Debug.Log("[Texture2DArray] 没有有效的Normal贴图，跳过Normal Array生成");
                return null;
            }

            Texture2DArray normalArray = new Texture2DArray(textureResolution_Paint,
                textureResolution_Paint,
                validLayerIndices.Count,
                TextureFormat.RGBA32,
                true,
                true);

            normalArray.filterMode = FilterMode.Trilinear;
            normalArray.wrapMode = TextureWrapMode.Repeat;
            normalArray.anisoLevel = 9;

            for (int arrayIndex = 0; arrayIndex < validLayerIndices.Count; arrayIndex++)
            {
                int layerIndex = validLayerIndices[arrayIndex];
                TerrainLayer layer = layers[layerIndex];

                Texture2D scaledTex = ScaleNormalTexture(layer.normalMapTexture, textureResolution_Paint, textureResolution_Paint);
                Graphics.CopyTexture(scaledTex, 0, 0, normalArray, arrayIndex, 0);
                Object.DestroyImmediate(scaledTex);
            }

            normalArray.Apply(true, false);

            string arrayPath = Path.Combine(outputPath, $"{materialName}_NormalArray.asset");
            AssetDatabase.CreateAsset(normalArray, arrayPath);

            return AssetDatabase.LoadAssetAtPath<Texture2DArray>(arrayPath);
        }

        /// <summary>
        /// 生成Paint (Mask) Texture2DArray
        /// </summary>
        private Texture2DArray GeneratePaintMaskArray(TerrainLayer[] layers, string outputPath, string materialName)
        {
            int layerCount = layers.Length;

            // 统计有mask的layer
            List<int> validLayerIndices = new List<int>();
            for (int i = 0; i < layerCount; i++)
            {
                if (layers[i] != null && layers[i].maskMapTexture != null)
                {
                    validLayerIndices.Add(i);
                }
            }

            if (validLayerIndices.Count == 0)
            {
                Debug.Log("[Texture2DArray] 没有有效的Mask贴图，跳过Mask Array生成");
                return null;
            }

            Texture2DArray maskArray = new Texture2DArray(textureResolution_Paint,
                textureResolution_Paint,
                validLayerIndices.Count,
                TextureFormat.RGBA32,
                true,
                true);

            maskArray.filterMode = FilterMode.Trilinear;
            maskArray.wrapMode = TextureWrapMode.Repeat;
            maskArray.anisoLevel = 9;

            for (int arrayIndex = 0; arrayIndex < validLayerIndices.Count; arrayIndex++)
            {
                int layerIndex = validLayerIndices[arrayIndex];
                TerrainLayer layer = layers[layerIndex];

                Texture2D scaledTex = ScaleTexture(layer.maskMapTexture, textureResolution_Paint, textureResolution_Paint, true);
                Graphics.CopyTexture(scaledTex, 0, 0, maskArray, arrayIndex, 0);
                Object.DestroyImmediate(scaledTex);
            }

            maskArray.Apply(true, false);

            string arrayPath = Path.Combine(outputPath, $"{materialName}_MaskArray.asset");
            AssetDatabase.CreateAsset(maskArray, arrayPath);

            return AssetDatabase.LoadAssetAtPath<Texture2DArray>(arrayPath);
        }

        /// <summary>
        /// 缩放纹理到目标分辨率
        /// </summary>
        private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight, bool isLinear)
        {
            RenderTexture rt = RenderTexture.GetTemporary(targetWidth,
                targetHeight,
                0,
                RenderTextureFormat.ARGB32,
                isLinear ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB);

            rt.filterMode = FilterMode.Bilinear;

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;

            Graphics.Blit(source, rt);

            Texture2D result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, true, isLinear);
            result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
            result.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);

            return result;
        }

        /// <summary>
        /// 缩放法线贴图（从AG通道解码）
        /// </summary>
        private Texture2D ScaleNormalTexture(Texture2D source, int targetWidth, int targetHeight)
        {
            if (_normalDecodeShader == null)
            {
                _normalDecodeShader = CreateNormalDecodeShader();
            }

            Material normalDecodeMaterial = new Material(_normalDecodeShader);
            normalDecodeMaterial.SetTexture("_MainTex", source);

            RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            Graphics.Blit(source, rt, normalDecodeMaterial);

            RenderTexture.active = rt;
            Texture2D result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, true, true);
            result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
            result.Apply(true);
            RenderTexture.active = null;

            RenderTexture.ReleaseTemporary(rt);
            Object.DestroyImmediate(normalDecodeMaterial);

            return result;
        }

        /// <summary>
        /// 创建法线解码Shader（从AG通道读取DXT5nm格式）
        /// </summary>
        private Shader CreateNormalDecodeShader()
        {
            string shaderCode = @"
Shader ""Hidden/NormalDecode""
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

        public Material CreateAtlasmapMaterial(Shader atlasmapShader, string outputPath, string materialName, string atlasOutputFolder)
        {
            if (atlasmapShader == null)
            {
                Debug.LogError("Atlasmap Shader未指定！");
                return null;
            }

            Material material = new Material(atlasmapShader);
            material.name = materialName;

            TerrainLayer[] layers = terrainData.terrainLayers;
            if (layers == null || layers.Length == 0)
            {
                Debug.LogWarning("Terrain没有地形层，无法创建Atlasmap材质");
                return material;
            }

            // 加载图集纹理
            // 拼接terrain名字得到图集目录
            string atlasDir = Path.Combine(atlasOutputFolder, terrainData.name);

            // 加载图集纹理
            Texture2D albedoAtlas = AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(atlasDir, "TerrainAlbedo_Atlas.png"));
            if (albedoAtlas == null)
                Debug.LogWarning($"[Material] 未找到Albedo图集: {Path.Combine(atlasDir, "TerrainAlbedo_Atlas.png")}");

            Texture2D normalAtlas = AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(atlasDir, "TerrainNormal_Atlas.png"));
            if (normalAtlas == null)
                Debug.LogWarning($"[Material] 未找到Normal图集: {Path.Combine(atlasDir, "TerrainNormal_Atlas.png")}");

            Texture2D indexMap = AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(atlasDir, "TerrainIndex.png"));
            if (indexMap == null)
                Debug.LogWarning($"[Material] 未找到Index贴图: {Path.Combine(atlasDir, "TerrainIndex.png")}");

            Texture2D blendMap = AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(atlasDir, "TerrainBlend.png"));
            if (blendMap == null)
                Debug.LogWarning($"[Material] 未找到Blend贴图: {Path.Combine(atlasDir, "TerrainBlend.png")}");

            // 设置材质属性
            if (albedoAtlas != null) material.SetTexture("_AlbedoAtlas", albedoAtlas);
            if (normalAtlas != null) material.SetTexture("_NormalAtlas", normalAtlas);
            if (indexMap != null) material.SetTexture("_IndexMap", indexMap);
            if (blendMap != null) material.SetTexture("_BlendMap", blendMap);

            // 检查并设置UV缩放偏移
            Vector3 terrainSize = terrainData.size;
            Vector4? atlasUVScaleOffset = null;
            bool uvConsistent = true;

            for (int i = 0; i < layers.Length; i++)
            {
                TerrainLayer layer = layers[i];
                if (layer == null || layer.diffuseTexture == null) continue;

                Vector2 tileSize = layer.tileSize;
                Vector2 tileOffset = layer.tileOffset;

                float scaleX = terrainSize.x / tileSize.x;
                float scaleY = terrainSize.z / tileSize.y;

                Vector4 currentUV = new Vector4(scaleX, scaleY, tileOffset.x, tileOffset.y);

                if (atlasUVScaleOffset == null)
                {
                    atlasUVScaleOffset = currentUV;
                }
                else
                {
                    // 检查是否一致（允许小误差）
                    if (Mathf.Abs(atlasUVScaleOffset.Value.x - currentUV.x) > 0.001f ||
                        Mathf.Abs(atlasUVScaleOffset.Value.y - currentUV.y) > 0.001f ||
                        Mathf.Abs(atlasUVScaleOffset.Value.z - currentUV.z) > 0.001f ||
                        Mathf.Abs(atlasUVScaleOffset.Value.w - currentUV.w) > 0.001f)
                    {
                        Debug.LogWarning($"[Material] Layer {i} ({layer.name}) 的UV缩放偏移不一致！" +
                            $"预期: {atlasUVScaleOffset.Value}, 实际: {currentUV}");
                        uvConsistent = false;
                    }
                }
            }

            // 设置UV缩放偏移
            if (atlasUVScaleOffset.HasValue)
            {
                material.SetVector("_AtlasUVScaleOffset", atlasUVScaleOffset.Value);
                Debug.Log($"[Material] 设置UV缩放偏移: {atlasUVScaleOffset.Value}");
            }

            if (!uvConsistent)
            {
                Debug.LogError("[Material] 警告：不同Layer的UV缩放偏移不一致，可能导致纹理错位！");
            }
            
            // 读取Atlas配置信息
            string jsonPath = Path.Combine(atlasDir, "AtlasInfo.json");
            if (File.Exists(jsonPath))
            {
                string json = File.ReadAllText(jsonPath);
                AtlasInfo info = JsonUtility.FromJson<AtlasInfo>(json);
    
                int atlasResolution = info.atlasResolution;
                int tileSize = info.tileSize;
                int padding = info.padding;
                
                material.SetFloat("_AtlasPadding", padding);
                material.SetFloat("_TileSize", tileSize);
                material.SetFloat("_AtlasResolution", atlasResolution);
    
                Debug.Log($"[Atlas] 加载配置: 分辨率={atlasResolution}, TileSize={tileSize}, Padding={padding}");
            }
            else
            {
                Debug.LogWarning($"[Atlas] 未找到配置文件: {jsonPath}");
            }
           

            // 5. 保存材质
            string materialPath = Path.Combine(outputPath, $"{materialName}.mat");
            AssetDatabase.CreateAsset(material, materialPath);
            AssetDatabase.SaveAssets();

            return material;
        }
    }
}