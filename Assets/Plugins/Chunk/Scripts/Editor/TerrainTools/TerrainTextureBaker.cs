using UnityEngine;
using UnityEditor;
using System.IO;

namespace SimpleTerrainToMesh.Editor
{
    /// <summary>
    /// Terrain纹理烘焙器
    /// 负责使用GPU烘焙Terrain的各种纹理贴图
    /// </summary>
    public class TerrainTextureBaker
    {
        private readonly Terrain terrain;
        private readonly TerrainData terrainData;
        private readonly Vector3 terrainSize;
        private readonly int textureResolution;

        // GPU烘焙用的Shader
        private static Shader bakedAlbedoShader;
        private static Shader bakedNormalShader;
        private static Shader bakedMetallicShader;

        public TerrainTextureBaker(Terrain terrain, int textureResolution)
        {
            this.terrain = terrain;
            this.terrainData = terrain.terrainData;
            this.terrainSize = terrainData.size;
            this.textureResolution = textureResolution;

            LoadBakeShaders();
        }

        /// <summary>
        /// 加载GPU烘焙用的Shader
        /// </summary>
        private static void LoadBakeShaders()
        {
            if (bakedAlbedoShader == null)
            {
                bakedAlbedoShader = Shader.Find("Hidden/TerrainBake/Albedo");
                if (bakedAlbedoShader == null)
                {
                    Debug.LogWarning("未找到 TerrainBake/Albedo Shader");
                }
            }

            if (bakedNormalShader == null)
            {
                bakedNormalShader = Shader.Find("Hidden/TerrainBake/Normal");
            }

            if (bakedMetallicShader == null)
            {
                bakedMetallicShader = Shader.Find("Hidden/TerrainBake/Metallic");
            }
        }

        /// <summary>
        /// 烘焙并应用所有纹理到材质
        /// </summary>
        public void BakeAndApplyTextures(Material material, string outputPath, string materialName, 
            float startX, float startZ, float width, float length)
        {
            // 计算UV范围
            float normalizedStartX = startX / terrainSize.x;
            float normalizedStartZ = startZ / terrainSize.z;
            float normalizedWidth = width / terrainSize.x;
            float normalizedLength = length / terrainSize.z;

            // 1. 生成 Base Map (Albedo) - sRGB空间
            Texture2D baseMap = BakeAlbedoTexture(normalizedStartX, normalizedStartZ, normalizedWidth, normalizedLength);
            if (baseMap != null)
            {
                string baseMapPath = Path.Combine(outputPath, $"{materialName}_BaseMap.png");
                baseMap = SaveTexture(baseMap, baseMapPath, false);
                material.SetTexture("_BaseMap", baseMap);
                material.SetColor("_BaseColor", Color.white);
            }

            // 2. 生成 Normal Map
            if (!HasAnyNormalMap())
            {
                Debug.Log("[TerrainBake] 没有Normal Map，跳过烘焙");
                material.SetTexture("_BumpMap", null);
            }
            else
            {
                Texture2D normalMap = BakeNormalTexture(normalizedStartX, normalizedStartZ, normalizedWidth, normalizedLength);
                if (normalMap != null)
                {
                    string normalMapPath = Path.Combine(outputPath, $"{materialName}_Normal.png");
                    normalMap = SaveTexture(normalMap, normalMapPath, true);
                    material.SetTexture("_BumpMap", normalMap);
                    material.SetFloat("_BumpScale", 1.0f);
                    material.EnableKeyword("_NORMALMAP");
                }
            }

            // 3. 生成 Metallic + Smoothness Map
            if (!HasAnyMaskMap())
            {
                SetAverageMetallicSmoothness(material);
            }
            else
            {
                Texture2D metallicMap = BakeMetallicSmoothnessTexture(normalizedStartX, normalizedStartZ, normalizedWidth, normalizedLength);
                if (metallicMap != null)
                {
                    string metallicMapPath = Path.Combine(outputPath, $"{materialName}_MetallicSmoothness.png");
                    metallicMap = SaveTexture(metallicMap, metallicMapPath, false);
                    material.SetTexture("_MetallicGlossMap", metallicMap);
                    material.SetFloat("_Metallic", 1.0f);
                    material.SetFloat("_Smoothness", 1.0f);
                    material.SetFloat("_SmoothnessTextureChannel", 0);
                }

                // 4. Height Map
                Texture2D heightMap = BakeHeightTexture(normalizedStartX, normalizedStartZ, normalizedWidth, normalizedLength);
                if (heightMap != null)
                {
                    string heightMapPath = Path.Combine(outputPath, $"{materialName}_Height.png");
                    heightMap = SaveTexture(heightMap, heightMapPath, false);
                    material.SetTexture("_ParallaxMap", heightMap);
                    material.SetFloat("_Parallax", 0.005f);
                }

                // 5. Occlusion Map
                Texture2D occlusionMap = BakeOcclusionTexture(normalizedStartX, normalizedStartZ, normalizedWidth, normalizedLength);
                if (occlusionMap != null)
                {
                    string occlusionMapPath = Path.Combine(outputPath, $"{materialName}_Occlusion.png");
                    occlusionMap = SaveTexture(occlusionMap, occlusionMapPath, false);
                    material.SetTexture("_OcclusionMap", occlusionMap);
                    material.SetFloat("_OcclusionStrength", 1.0f);
                }
            }
        }

        /// <summary>
        /// 设置平均Metallic和Smoothness值
        /// </summary>
        private void SetAverageMetallicSmoothness(Material material)
        {
            float metallic = 0f;
            float smoothness = 0f;
            var layers = terrainData.terrainLayers;
            var alphamap = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
            float totalWeight = 0;
            int numLayers = layers.Length;

            int stepX = terrainData.alphamapWidth / 16;
            int stepY = terrainData.alphamapHeight / 16;
            for (int y = 0; y < terrainData.alphamapHeight; y += stepY)
            {
                for (int x = 0; x < terrainData.alphamapWidth; x += stepX)
                {
                    for (int l = 0; l < numLayers; l++)
                    {
                        float w = alphamap[y, x, l];
                        if (w > 0.0001f)
                        {
                            metallic += layers[l].metallic * w;
                            smoothness += layers[l].smoothness * w;
                            totalWeight += w;
                        }
                    }
                }
            }

            metallic /= totalWeight;
            smoothness /= totalWeight;

            // Gamma → Linear 转换
            metallic = Mathf.Pow(metallic, 2.2f);
            smoothness = Mathf.Pow(smoothness, 2.2f);

            material.SetTexture("_MetallicGlossMap", null);
            material.SetTexture("_OcclusionMap", null);
            material.SetTexture("_ParallaxMap", null);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
        }

        #region Texture Baking Methods

        private Texture2D BakeAlbedoTexture(float startX, float startZ, float width, float length)
        {
            if (bakedAlbedoShader == null) return null;

            TerrainLayer[] layers = terrainData.terrainLayers;
            if (layers == null || layers.Length == 0) return null;

            Material bakeMaterial = new Material(bakedAlbedoShader);
            try
            {
                SetupBakeMaterial(bakeMaterial, startX, startZ, width, length);
                
                for (int i = 0; i < Mathf.Min(layers.Length, 8); i++)
                {
                    SetupLayerTextures(bakeMaterial, layers[i], i, true, false, false);
                }

                return BakeToTexture(bakeMaterial, textureResolution, textureResolution, false);
            }
            finally
            {
                Object.DestroyImmediate(bakeMaterial);
            }
        }

        private Texture2D BakeNormalTexture(float startX, float startZ, float width, float length)
        {
            if (bakedNormalShader == null) return null;

            TerrainLayer[] layers = terrainData.terrainLayers;
            if (layers == null || layers.Length == 0) return null;

            Material bakeMaterial = new Material(bakedNormalShader);
            try
            {
                SetupBakeMaterial(bakeMaterial, startX, startZ, width, length);
                
                for (int i = 0; i < Mathf.Min(layers.Length, 8); i++)
                {
                    SetupLayerTextures(bakeMaterial, layers[i], i, false, true, false);
                }

                return BakeToTexture(bakeMaterial, textureResolution, textureResolution, true);
            }
            finally
            {
                Object.DestroyImmediate(bakeMaterial);
            }
        }

        private Texture2D BakeMetallicSmoothnessTexture(float startX, float startZ, float width, float length)
        {
            if (bakedMetallicShader == null) return null;

            TerrainLayer[] layers = terrainData.terrainLayers;
            if (layers == null || layers.Length == 0) return null;

            Material bakeMaterial = new Material(bakedMetallicShader);
            try
            {
                SetupBakeMaterial(bakeMaterial, startX, startZ, width, length);
                
                for (int i = 0; i < Mathf.Min(layers.Length, 8); i++)
                {
                    SetupLayerTextures(bakeMaterial, layers[i], i, false, false, true);
                }

                return BakeToTexture(bakeMaterial, textureResolution, textureResolution, true);
            }
            finally
            {
                Object.DestroyImmediate(bakeMaterial);
            }
        }

        private Texture2D BakeHeightTexture(float startX, float startZ, float width, float length)
        {
            Shader heightShader = Shader.Find("Hidden/TerrainBake/Height");
            if (heightShader == null) return GenerateHeightMapFromTerrain(startX, startZ, width, length);

            TerrainLayer[] layers = terrainData.terrainLayers;
            if (layers == null || layers.Length == 0) return GenerateHeightMapFromTerrain(startX, startZ, width, length);

            Material bakeMaterial = new Material(heightShader);
            try
            {
                SetupBakeMaterial(bakeMaterial, startX, startZ, width, length);
                
                bool hasMaskMap = false;
                for (int i = 0; i < Mathf.Min(layers.Length, 8); i++)
                {
                    if (SetupMaskTexture(bakeMaterial, layers[i], i))
                    {
                        hasMaskMap = true;
                    }
                }

                if (!hasMaskMap) return GenerateHeightMapFromTerrain(startX, startZ, width, length);

                return BakeToTexture(bakeMaterial, textureResolution, textureResolution, true);
            }
            finally
            {
                Object.DestroyImmediate(bakeMaterial);
            }
        }

        private Texture2D BakeOcclusionTexture(float startX, float startZ, float width, float length)
        {
            Shader occlusionShader = Shader.Find("Hidden/TerrainBake/Occlusion");
            if (occlusionShader == null) return GenerateOcclusionMapFromTerrain(startX, startZ, width, length);

            TerrainLayer[] layers = terrainData.terrainLayers;
            if (layers == null || layers.Length == 0) return GenerateOcclusionMapFromTerrain(startX, startZ, width, length);

            Material bakeMaterial = new Material(occlusionShader);
            try
            {
                SetupBakeMaterial(bakeMaterial, startX, startZ, width, length);
                
                bool hasMaskMap = false;
                for (int i = 0; i < Mathf.Min(layers.Length, 8); i++)
                {
                    if (SetupMaskTexture(bakeMaterial, layers[i], i))
                    {
                        hasMaskMap = true;
                    }
                }

                if (!hasMaskMap) return GenerateOcclusionMapFromTerrain(startX, startZ, width, length);

                return BakeToTexture(bakeMaterial, textureResolution, textureResolution, true);
            }
            finally
            {
                Object.DestroyImmediate(bakeMaterial);
            }
        }

        #endregion

        #region Helper Methods

        private void SetupBakeMaterial(Material bakeMaterial, float startX, float startZ, float width, float length)
        {
            bakeMaterial.SetVector("_UVOffset", new Vector4(startX, startZ, 0, 0));
            bakeMaterial.SetVector("_UVScale", new Vector4(width, length, 1, 1));

            if (terrainData.alphamapTextures.Length > 0)
            {
                bakeMaterial.SetTexture("_Control0", terrainData.alphamapTextures[0]);
            }

            if (terrainData.alphamapTextures.Length > 1)
            {
                bakeMaterial.SetTexture("_Control1", terrainData.alphamapTextures[1]);
            }
        }

        private void SetupLayerTextures(Material bakeMaterial, TerrainLayer layer, int index, 
            bool setupDiffuse, bool setupNormal, bool setupMetallic)
        {
            if (layer == null) return;

            // 设置ST参数
            Vector2 tileSize = layer.tileSize;
            Vector2 tileOffset = layer.tileOffset;
            float scaleX = terrainSize.x / tileSize.x;
            float scaleY = terrainSize.z / tileSize.y;
            bakeMaterial.SetVector($"_Splat{index}_ST", new Vector4(scaleX, scaleY, tileOffset.x, tileOffset.y));

            if (setupDiffuse)
            {
                bakeMaterial.SetTexture($"_Splat{index}", layer.diffuseTexture ?? Texture2D.whiteTexture);
            }

            if (setupNormal)
            {
                bakeMaterial.SetTexture($"_Normal{index}", layer.normalMapTexture ?? Texture2D.normalTexture);
                bakeMaterial.SetFloat($"_NormalScale{index}", layer.normalScale);
            }

            if (setupMetallic)
            {
                bakeMaterial.SetTexture($"_Mask{index}", layer.maskMapTexture ?? Texture2D.whiteTexture);
                bakeMaterial.SetFloat($"_Metallic{index}", layer.metallic);
                bakeMaterial.SetFloat($"_Smoothness{index}", layer.smoothness);

                Vector4 remapMin = Vector4.zero;
                Vector4 remapMax = Vector4.one;
                GetMaskMapRemapping(layer, ref remapMin, ref remapMax);
                bakeMaterial.SetVector($"_MaskRemapMin{index}", remapMin);
                bakeMaterial.SetVector($"_MaskRemapMax{index}", remapMax);
            }
        }

        private bool SetupMaskTexture(Material bakeMaterial, TerrainLayer layer, int index)
        {
            if (layer == null) return false;

            Vector2 tileSize = layer.tileSize;
            Vector2 tileOffset = layer.tileOffset;
            float scaleX = terrainSize.x / tileSize.x;
            float scaleY = terrainSize.z / tileSize.y;
            bakeMaterial.SetVector($"_Splat{index}_ST", new Vector4(scaleX, scaleY, tileOffset.x, tileOffset.y));

            if (layer.maskMapTexture != null)
            {
                bakeMaterial.SetTexture($"_Mask{index}", layer.maskMapTexture);
                
                Vector4 remapMin = Vector4.zero;
                Vector4 remapMax = Vector4.one;
                GetMaskMapRemapping(layer, ref remapMin, ref remapMax);
                bakeMaterial.SetVector($"_MaskRemapMin{index}", remapMin);
                bakeMaterial.SetVector($"_MaskRemapMax{index}", remapMax);
                
                return true;
            }
            else
            {
                bakeMaterial.SetTexture($"_Mask{index}", Texture2D.whiteTexture);
                return false;
            }
        }

        private Texture2D BakeToTexture(Material bakeMaterial, int width, int height, bool isLinear)
        {
            RenderTextureFormat format = RenderTextureFormat.ARGB32;
            RenderTextureReadWrite readWrite = isLinear ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB;

            RenderTexture rt = RenderTexture.GetTemporary(width, height, 0, format, readWrite);
            rt.filterMode = FilterMode.Bilinear;

            RenderTexture previousRT = RenderTexture.active;

            try
            {
                RenderTexture.active = rt;
                GL.Clear(true, true, Color.clear);
                Graphics.Blit(null, rt, bakeMaterial);

                Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, true, isLinear);
                result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                result.Apply();

                return result;
            }
            finally
            {
                RenderTexture.active = previousRT;
                RenderTexture.ReleaseTemporary(rt);
            }
        }

        private Texture2D SaveTexture(Texture2D texture, string path, bool isNormalMap)
        {
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(path);

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.isReadable = true;
                importer.mipmapEnabled = true;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Trilinear;
                importer.anisoLevel = 9;

                TextureImporterPlatformSettings settings = new TextureImporterPlatformSettings();
                settings.maxTextureSize = 4096;
                settings.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.SetPlatformTextureSettings(settings);

                if (isNormalMap)
                {
                    importer.textureType = TextureImporterType.NormalMap;
                    importer.sRGBTexture = false;
                }
                else
                {
                    importer.textureType = TextureImporterType.Default;
                    importer.sRGBTexture = path.Contains("BaseMap") || path.Contains("Albedo");
                }

                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private void GetMaskMapRemapping(TerrainLayer layer, ref Vector4 remapMin, ref Vector4 remapMax)
        {
            remapMin = Vector4.zero;
            remapMax = Vector4.one;

#if UNITY_2021_2_OR_NEWER
            try
            {
                var maskMapRemapMinProp = layer.GetType().GetProperty("maskMapRemapMin");
                var maskMapRemapMaxProp = layer.GetType().GetProperty("maskMapRemapMax");

                if (maskMapRemapMinProp != null && maskMapRemapMaxProp != null)
                {
                    remapMin = (Vector4)maskMapRemapMinProp.GetValue(layer);
                    remapMax = (Vector4)maskMapRemapMaxProp.GetValue(layer);
                }
            }
            catch { }
#endif
        }

        private bool HasAnyNormalMap()
        {
            TerrainLayer[] layers = terrainData.terrainLayers;
            if (layers == null || layers.Length == 0) return false;

            float[,,] alphaMaps = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
            int sampleStep = Mathf.Max(1, Mathf.Min(terrainData.alphamapWidth, terrainData.alphamapHeight) / 32);

            for (int i = 0; i < layers.Length; i++)
            {
                if (i >= alphaMaps.GetLength(2)) break;
                TerrainLayer layer = layers[i];
                if (layer == null || layer.normalMapTexture == null) continue;

                for (int y = 0; y < alphaMaps.GetLength(0); y += sampleStep)
                {
                    for (int x = 0; x < alphaMaps.GetLength(1); x += sampleStep)
                    {
                        if (alphaMaps[y, x, i] > 0.001f) return true;
                    }
                }
            }

            return false;
        }

        private bool HasAnyMaskMap()
        {
            TerrainLayer[] layers = terrainData.terrainLayers;
            if (layers == null || layers.Length == 0) return false;

            float[,,] alphaMaps = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
            int sampleStep = Mathf.Max(1, Mathf.Min(terrainData.alphamapWidth, terrainData.alphamapHeight) / 32);

            for (int i = 0; i < layers.Length; i++)
            {
                if (i >= alphaMaps.GetLength(2)) break;
                TerrainLayer layer = layers[i];
                if (layer == null || layer.maskMapTexture == null) continue;

                for (int y = 0; y < alphaMaps.GetLength(0); y += sampleStep)
                {
                    for (int x = 0; x < alphaMaps.GetLength(1); x += sampleStep)
                    {
                        if (alphaMaps[y, x, i] > 0.001f) return true;
                    }
                }
            }

            return false;
        }

        private Texture2D GenerateHeightMapFromTerrain(float startX, float startZ, float width, float length)
        {
            Texture2D heightMap = new Texture2D(textureResolution, textureResolution, TextureFormat.R8, true, true);

            for (int y = 0; y < textureResolution; y++)
            {
                for (int x = 0; x < textureResolution; x++)
                {
                    float u = (float)x / (textureResolution - 1);
                    float v = (float)y / (textureResolution - 1);
                    float normalizedX = Mathf.Clamp01(startX + u * width);
                    float normalizedZ = Mathf.Clamp01(startZ + v * length);
                    float height = terrainData.GetInterpolatedHeight(normalizedX, normalizedZ);
                    float normalizedHeight = height / terrainSize.y;
                    heightMap.SetPixel(x, y, new Color(normalizedHeight, normalizedHeight, normalizedHeight, 1));
                }
            }

            heightMap.Apply();
            return heightMap;
        }

        private Texture2D GenerateOcclusionMapFromTerrain(float startX, float startZ, float width, float length)
        {
            Texture2D occlusionMap = new Texture2D(textureResolution, textureResolution, TextureFormat.R8, true, true);

            for (int y = 0; y < textureResolution; y++)
            {
                for (int x = 0; x < textureResolution; x++)
                {
                    float u = (float)x / (textureResolution - 1);
                    float v = (float)y / (textureResolution - 1);
                    float normalizedX = Mathf.Clamp01(startX + u * width);
                    float normalizedZ = Mathf.Clamp01(startZ + v * length);
                    float centerHeight = terrainData.GetInterpolatedHeight(normalizedX, normalizedZ);
                    float occlusion = 1.0f;

                    int samples = 8;
                    float radius = 0.01f;
                    for (int i = 0; i < samples; i++)
                    {
                        float angle = i * Mathf.PI * 2 / samples;
                        float sampleX = Mathf.Clamp01(normalizedX + Mathf.Cos(angle) * radius);
                        float sampleZ = Mathf.Clamp01(normalizedZ + Mathf.Sin(angle) * radius);
                        float sampleHeight = terrainData.GetInterpolatedHeight(sampleX, sampleZ);
                        if (sampleHeight > centerHeight) occlusion -= 0.05f;
                    }

                    occlusion = Mathf.Clamp01(occlusion);
                    occlusionMap.SetPixel(x, y, new Color(occlusion, occlusion, occlusion, 1));
                }
            }

            occlusionMap.Apply();
            return occlusionMap;
        }

        #endregion
    }
}