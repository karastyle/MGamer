// TerrainExporter.cs

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public static class TerrainExporter
{
#if UNITY_EDITOR
    public static void ExportTerrain(Terrain terrain, TerrainExportConfig.TerrainEntry entry, string rootPath, TerrainExportConfig config)
    {
        if (terrain == null || terrain.terrainData == null)
        {
            Debug.LogError("Invalid terrain!");
            return;
        }

        string terrainFolderPath = rootPath;
        if (!Directory.Exists(terrainFolderPath))
            Directory.CreateDirectory(terrainFolderPath);

        TerrainData terrainData = terrain.terrainData;

        if (entry.exportHeightmap)
        {
            ExportHeightmap(terrainData, terrainFolderPath, config);
        }

        if (entry.exportTrees)
        {
            ExportTreeInstances(terrain, terrainData, terrainFolderPath);
        }

        if (entry.exportDetails)
        {
            ExportDetailMaps(terrainData, entry.detailLayerIndices, terrainFolderPath, config);
            ExportDetailInstances(terrain, terrainData, entry.detailLayerIndices, terrainFolderPath);
        }

        AssetDatabase.Refresh();
        Debug.Log($"Terrain '{terrain.name}' exported to: {terrainFolderPath}");
    }

    private static void ExportHeightmap(TerrainData terrainData, string folderPath, TerrainExportConfig config)
    {
        int resolution = terrainData.heightmapResolution;
        float[,] heights = terrainData.GetHeights(0, 0, resolution, resolution);

        Texture2D heightmap = new Texture2D(resolution, resolution, TextureFormat.RFloat, false, true);

        Color[] pixels = new Color[resolution * resolution];
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float height = heights[y, x];
                pixels[y * resolution + x] = new Color(height, height, height, 1f);
            }
        }

        heightmap.SetPixels(pixels);
        heightmap.Apply();

        byte[] bytes = heightmap.EncodeToEXR(Texture2D.EXRFlags.OutputAsFloat);
        string path = Path.Combine(folderPath, "Heightmap.exr");
        File.WriteAllBytes(path, bytes);

        Object.DestroyImmediate(heightmap);

        AssetDatabase.ImportAsset(path);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.SingleChannel;
            importer.sRGBTexture = false;
            importer.mipmapEnabled = false;

            TextureImporterFormat importerFormat = GetImporterFormat(config.heightmapFormat);
            
            SetPlatformSettings(importer, "Default", importerFormat, config.compressTextures);
            SetPlatformSettings(importer, "Standalone", importerFormat, config.compressTextures);
            SetPlatformSettings(importer, "Android", importerFormat, config.compressTextures);
            
            importer.SaveAndReimport();
        }
    }

    private static void ExportTreeInstances(Terrain terrain, TerrainData terrainData, string folderPath)
    {
        TreeInstance[] trees = terrainData.treeInstances;
        TreePrototype[] prototypes = terrainData.treePrototypes;

        if (trees.Length == 0)
        {
            Debug.Log("No trees to export.");
            return;
        }

        TreeExportData exportData = new TreeExportData
        {
            terrainPosition = terrain.transform.position,
            terrainSize = terrainData.size,
            prototypes = new List<TreePrototypeData>(),
            instances = new List<TreeInstanceData>()
        };

        for (int i = 0; i < prototypes.Length; i++)
        {
            string prefabPath = "";
            if (prototypes[i].prefab != null)
            {
                prefabPath = AssetDatabase.GetAssetPath(prototypes[i].prefab);
            }

            TreePrototypeData protoData = new TreePrototypeData
            {
                index = i,
                prefabPath = prefabPath
            };
            exportData.prototypes.Add(protoData);
        }

        for (int i = 0; i < trees.Length; i++)
        {
            TreeInstance tree = trees[i];
            TreeInstanceData instanceData = new TreeInstanceData
            {
                prototypeIndex = tree.prototypeIndex,
                position = tree.position,
                rotation = tree.rotation,
                widthScale = tree.widthScale,
                heightScale = tree.heightScale,
                color = new float[] { tree.color.r, tree.color.g, tree.color.b, tree.color.a }
            };
            exportData.instances.Add(instanceData);
        }

        string json = JsonUtility.ToJson(exportData, true);
        string jsonPath = Path.Combine(folderPath, "TreeInstances.json");
        File.WriteAllText(jsonPath, json);

        Debug.Log($"Exported {trees.Length} tree instances.");
    }

    private static void ExportDetailMaps(TerrainData terrainData, List<int> layerIndices, string folderPath, TerrainExportConfig config)
    {
        int detailResolution = terrainData.detailResolution;
        int layerCount = terrainData.detailPrototypes.Length;

        if (layerCount == 0)
        {
            Debug.Log("No detail layers to export.");
            return;
        }

        List<int> exportIndices = new List<int>();
        if (layerIndices == null || layerIndices.Count == 0)
        {
            for (int i = 0; i < layerCount; i++)
                exportIndices.Add(i);
        }
        else
        {
            exportIndices = layerIndices;
        }

        foreach (int layerIndex in exportIndices)
        {
            if (layerIndex < 0 || layerIndex >= layerCount)
                continue;

            DetailPrototype prototype = terrainData.detailPrototypes[layerIndex];
            int[,] detailLayer = terrainData.GetDetailLayer(0, 0, detailResolution, detailResolution, layerIndex);

            Texture2D densityMap = new Texture2D(detailResolution, detailResolution, TextureFormat.R8, false, true);

            Color[] pixels = new Color[detailResolution * detailResolution];

            for (int y = 0; y < detailResolution; y++)
            {
                for (int x = 0; x < detailResolution; x++)
                {
                    int density = detailLayer[y, x];
                    float normalizedValue = density / 255.0f;   //归一化
                    pixels[y * detailResolution + x] = new Color(normalizedValue, normalizedValue, normalizedValue, 1f);
                }
            }

            densityMap.SetPixels(pixels);
            densityMap.Apply();

            byte[] bytes = densityMap.EncodeToPNG();
            string protoName = prototype.prototype != null
                ? prototype.prototype.name
                : (prototype.prototypeTexture != null ? prototype.prototypeTexture.name : $"Layer{layerIndex}");
            string path = Path.Combine(folderPath, $"DetailDensity_{layerIndex}_{protoName}.png");
            File.WriteAllBytes(path, bytes);

            Object.DestroyImmediate(densityMap);

            AssetDatabase.ImportAsset(path);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.SingleChannel;
                importer.sRGBTexture = false;
                importer.mipmapEnabled = false;
                importer.isReadable = true;

                TextureImporterFormat importerFormat = GetImporterFormat(config.detailFormat);

                SetPlatformSettings(importer, "Default", importerFormat, config.compressTextures);
                SetPlatformSettings(importer, "Standalone", importerFormat, config.compressTextures);
                SetPlatformSettings(importer, "Android", importerFormat, config.compressTextures);

                importer.SaveAndReimport();
            }

            DetailMetadata metadata = new DetailMetadata
            {
                layerIndex = layerIndex,
                alignToGround = prototype.alignToGround,
                renderMode = prototype.renderMode.ToString(),
                minWidth = prototype.minWidth,
                maxWidth = prototype.maxWidth,
                minHeight = prototype.minHeight,
                maxHeight = prototype.maxHeight,
                noiseSpread = prototype.noiseSpread,
                noiseSeed = prototype.noiseSeed,
                prototypeName = protoName
            };

            string metaJson = JsonUtility.ToJson(metadata, true);
            string metaPath = Path.Combine(folderPath, $"DetailDensity_{layerIndex}_{protoName}.json");
            File.WriteAllText(metaPath, metaJson);

            Debug.Log($"Exported layer {layerIndex} ({protoName})");
        }

        Debug.Log($"Exported {exportIndices.Count} detail layers.");
    }

    private static void ExportDetailInstances(Terrain terrain, TerrainData terrainData, List<int> layerIndices,
        string folderPath)
    {
        DetailPrototype[] prototypes = terrainData.detailPrototypes;

        if (prototypes.Length == 0)
        {
            Debug.Log("No detail prototypes to export.");
            return;
        }

        List<int> exportIndices = new List<int>();
        if (layerIndices == null || layerIndices.Count == 0)
        {
            for (int i = 0; i < prototypes.Length; i++)
                exportIndices.Add(i);
        }
        else
        {
            exportIndices = layerIndices;
        }

        DetailInstancesData exportData = new DetailInstancesData
        {
            terrainPosition = terrain.transform.position,
            terrainSize = terrainData.size,
            prototypes = new List<DetailPrototypeData>()
        };

        foreach (int index in exportIndices)
        {
            if (index < 0 || index >= prototypes.Length)
                continue;

            DetailPrototype prototype = prototypes[index];
            string prefabPath = "";
            if (prototype.prototype != null)
            {
                prefabPath = AssetDatabase.GetAssetPath(prototype.prototype);
            }

            string protoName = prototype.prototype != null
                ? prototype.prototype.name
                : (prototype.prototypeTexture != null ? prototype.prototypeTexture.name : $"Layer{index}");

            DetailPrototypeData protoData = new DetailPrototypeData
            {
                index = index,
                prefabPath = prefabPath,
                densityMapPath = $"DetailDensity_{index}_{protoName}.png",
                metadataPath = $"DetailDensity_{index}_{protoName}.json"
            };
            exportData.prototypes.Add(protoData);
        }

        string json = JsonUtility.ToJson(exportData, true);
        string jsonPath = Path.Combine(folderPath, "DetailInstances.json");
        File.WriteAllText(jsonPath, json);

        Debug.Log($"Exported DetailInstances with {exportData.prototypes.Count} prototypes.");
    }

    private static void SetPlatformSettings(TextureImporter importer, string platform, TextureImporterFormat format, bool compress)
    {
        TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings(platform);
        if (platform == "Default")
        {
            settings = importer.GetDefaultPlatformTextureSettings();
        }
        
        settings.overridden = true;
        settings.format = format;
        
        // 如果启用压缩，但格式不是自动压缩格式，可以在这里进行额外的逻辑处理
        // 例如 TextureImporterFormat.Automatic 可能会根据平台选择压缩格式
        // 但对于特定格式如 R8 或 RFloat，压缩选项可能受限或需要特定设置
        
        // 简单处理：如果不是 float 纹理且启用了压缩，尝试使用 Compressed 变体
        // 注意：R8 和 RFloat 通常不压缩，或是使用特定压缩格式（如 BC4/EAC_R）
        // 这里主要依赖传入的 format 参数，该参数由 GetImporterFormat 转换而来
        
        importer.SetPlatformTextureSettings(settings);
    }
    
    private static TextureImporterFormat GetImporterFormat(TextureFormat format)
    {
        switch (format)
        {
            case TextureFormat.R8:
                return TextureImporterFormat.R8;
            case TextureFormat.R16:
                return TextureImporterFormat.R16;
            case TextureFormat.RFloat:
                return TextureImporterFormat.RFloat;
            case TextureFormat.Alpha8:
                return TextureImporterFormat.Alpha8;
            case TextureFormat.ARGB32:
                return TextureImporterFormat.RGBA32;
            case TextureFormat.RGB24:
                return TextureImporterFormat.RGB24;
            // 添加更多映射...
            default:
                // 默认回退，或者根据需求选择 Automatic
                // 注意：TextureImporterFormat 没有直接对应所有 TextureFormat 的枚举
                // 对于 R8/RFloat 这种特定需求，直接映射是安全的
                // 如果需要压缩格式（如 ASTC/ETC），需要在这里显式返回对应的 TextureImporterFormat
                 if (format == TextureFormat.R8) return TextureImporterFormat.R8;
                 if (format == TextureFormat.RFloat) return TextureImporterFormat.RFloat;
                 
                 // 如果找不到直接映射，尝试转为 Automatic (可能不准确，根据具体需求调整)
                 return TextureImporterFormat.Automatic;
        }
    }
#endif

    [System.Serializable]
    public class TreeExportData
    {
        public Vector3 terrainPosition;
        public Vector3 terrainSize;
        public List<TreePrototypeData> prototypes;
        public List<TreeInstanceData> instances;
    }

    [System.Serializable]
    public class TreePrototypeData
    {
        public int index;
        public string prefabPath;
    }

    [System.Serializable]
    public class TreeInstanceData
    {
        public int prototypeIndex;
        public Vector3 position;
        public float rotation;
        public float widthScale;
        public float heightScale;
        public float[] color;
    }

    [System.Serializable]
    public class DetailInstancesData
    {
        public Vector3 terrainPosition;
        public Vector3 terrainSize;
        public List<DetailPrototypeData> prototypes;
    }

    [System.Serializable]
    public class DetailPrototypeData
    {
        public int index;
        public string prefabPath;
        public string densityMapPath;
        public string metadataPath;
    }
}