using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace SimpleTerrainToMesh.Editor
{
    /// <summary>
    /// Terrain 转 Mesh 核心转换器（重构版）
    /// 负责网格生成和整体流程控制
    /// </summary>
    public class TerrainToMeshConverter
    {
        private readonly Terrain terrain;
        private readonly TerrainData terrainData;
        private readonly int gridSplitX;
        private readonly int gridSplitZ;
        private readonly int verticesPerGridX;
        private readonly int verticesPerGridZ;
        private readonly SimpleTerrainToMeshWindow.PivotPosition pivotPosition;
        private readonly SimpleTerrainToMeshWindow.NormalCalculationMode normalMode;
        private readonly bool generateMeshCollider;

        // 材质设置
        private readonly SimpleTerrainToMeshWindow.MaterialType materialType;
        private readonly Shader materialShader;
        private readonly bool exportPerChunk;
        private readonly int textureResolution;
        private readonly bool useTexture2DArray;
        private readonly int textureResolution_Paint;
        private readonly string atlasOutputFolder;

        // 缓存材质（当 exportPerChunk = false 时使用）
        private Material sharedMaterial;

        // Terrain 尺寸信息
        private Vector3 terrainSize;
        private Vector3 terrainPosition;

        // 辅助类
        private SplatmapMaterialBuilder splatmapBuilder;
        private TerrainTextureBaker textureBaker;

        public TerrainToMeshConverter(
            Terrain terrain,
            TerrainData terrainData,
            int gridSplitX,
            int gridSplitZ,
            int verticesPerGridX,
            int verticesPerGridZ,
            SimpleTerrainToMeshWindow.PivotPosition pivotPosition,
            SimpleTerrainToMeshWindow.NormalCalculationMode normalMode,
            bool generateMeshCollider,
            SimpleTerrainToMeshWindow.MaterialType materialType,
            Shader materialShader,
            bool exportPerChunk,
            int textureResolution,
            bool useTexture2DArray,
            int textureResolution_Paint,
            string atlasOutputFolder)
        {
            this.terrain = terrain;
            this.terrainData = terrainData;
            this.gridSplitX = gridSplitX;
            this.gridSplitZ = gridSplitZ;
            this.verticesPerGridX = verticesPerGridX;
            this.verticesPerGridZ = verticesPerGridZ;
            this.pivotPosition = pivotPosition;
            this.normalMode = normalMode;
            this.generateMeshCollider = generateMeshCollider;
            this.materialType = materialType;
            this.materialShader = materialShader;
            this.exportPerChunk = exportPerChunk;
            this.textureResolution = textureResolution;
            this.useTexture2DArray = useTexture2DArray;
            this.textureResolution_Paint = textureResolution_Paint;
            this.atlasOutputFolder = atlasOutputFolder;

            // 缓存 Terrain 信息
            this.terrainSize = terrainData.size;
            this.terrainPosition = terrain.transform.position;

            // 初始化辅助类
            this.splatmapBuilder = new SplatmapMaterialBuilder(terrain, textureResolution, useTexture2DArray, textureResolution_Paint);
            this.textureBaker = new TerrainTextureBaker(terrain, textureResolution);
        }

        /// <summary>
        /// 执行转换
        /// </summary>
        public GameObject Convert(string outputPath, int currentTerrainIndex = 1, int totalTerrains = 1, string customName = null)
        {
            int totalGrids = gridSplitX * gridSplitZ;
            int currentGrid = 0;

            string parentName = string.IsNullOrEmpty(customName) ? $"{terrain.name}_Meshes" : customName;
            GameObject parentObject = new GameObject(parentName);
            parentObject.transform.position = terrainPosition;

            for (int z = 0; z < gridSplitZ; z++)
            {
                for (int x = 0; x < gridSplitX; x++)
                {
                    currentGrid++;

                    float terrainProgress = (float)currentGrid / totalGrids;
                    float overallProgress = ((currentTerrainIndex - 1) + terrainProgress) / totalTerrains;

                    EditorUtility.DisplayProgressBar("转换中",
                        $"Terrain {currentTerrainIndex}/{totalTerrains} ({terrain.name}) - 网格 {currentGrid}/{totalGrids}",
                        overallProgress);

                    GenerateGridMesh(x, z, outputPath, parentObject.transform);
                }
            }

            Debug.Log($"[{terrain.name}] 转换完成！生成了 {totalGrids} 个网格");

            return parentObject;
        }

        /// <summary>
        /// 生成单个网格块
        /// </summary>
        private void GenerateGridMesh(int gridX, int gridZ, string outputPath, Transform parent)
        {
            Mesh mesh = new Mesh();
            mesh.name = $"TerrainMesh_{gridX}_{gridZ}";

            float gridWidth = terrainSize.x / gridSplitX;
            float gridLength = terrainSize.z / gridSplitZ;
            float startX = gridX * gridWidth;
            float startZ = gridZ * gridLength;

            // 生成顶点、UV和三角形
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> triangles = new List<int>();

            GenerateVerticesAndUVs(vertices, uvs, startX, startZ, gridWidth, gridLength);
            GenerateTriangles(triangles);

            // 应用Pivot偏移
            Vector3 pivotOffset = CalculatePivotOffset(vertices);
            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i] -= pivotOffset;
            }

            // 设置Mesh数据
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);

            // 计算法线
            if (normalMode == SimpleTerrainToMeshWindow.NormalCalculationMode.CalculateFromMesh)
            {
                mesh.RecalculateNormals();
            }
            else
            {
                CalculateNormalsFromTerrain(mesh, vertices, pivotOffset, startX, startZ);
            }

            mesh.RecalculateTangents();
            mesh.RecalculateBounds();

            // 保存Mesh
            string meshPath = Path.Combine(outputPath, $"{mesh.name}.asset");
            
            if (!Directory.Exists(outputPath))
            {
                // 创建目录（如果不存在）
                Directory.CreateDirectory(outputPath);
            }
            
            AssetDatabase.CreateAsset(mesh, meshPath);

            // 创建GameObject
            GameObject meshObject = new GameObject(mesh.name);
            meshObject.transform.parent = parent;
            meshObject.transform.position = terrainPosition + pivotOffset;

            MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();

            // 创建材质
            Material material = GetOrCreateMaterial(gridX, gridZ, outputPath, startX, startZ, gridWidth, gridLength);
            if (material != null)
            {
                meshRenderer.sharedMaterial = material;
            }

            // 添加碰撞器
            if (generateMeshCollider)
            {
                MeshCollider meshCollider = meshObject.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = mesh;
            }
        }

        /// <summary>
        /// 生成顶点和UV
        /// </summary>
        private void GenerateVerticesAndUVs(List<Vector3> vertices, List<Vector2> uvs, 
        float startX, float startZ, float gridWidth, float gridLength)
        {
            float stepX = gridWidth / (verticesPerGridX - 1);
            float stepZ = gridLength / (verticesPerGridZ - 1);

            for (int z = 0; z < verticesPerGridZ; z++)
            {
                for (int x = 0; x < verticesPerGridX; x++)
                {
                    float worldX = startX + x * stepX;
                    float worldZ = startZ + z * stepZ;

                    float normalizedX = worldX / terrainSize.x;
                    float normalizedZ = worldZ / terrainSize.z;
                    float height = terrainData.GetInterpolatedHeight(normalizedX, normalizedZ);

                    Vector3 vertex = new Vector3(worldX, height, worldZ);
                    vertices.Add(vertex);

                    Vector2 uv;
                    // Splatmap和Atlasmap模式：UV必须是整个terrain的归一化坐标
                    if (materialType == SimpleTerrainToMeshWindow.MaterialType.Splatmap || this.materialType == SimpleTerrainToMeshWindow.MaterialType.AtlasMap)
                    {
                        uv = new Vector2(normalizedX, normalizedZ);
                    }
                    // BaseMap模式 + exportPerChunk：UV是chunk内的0-1坐标
                    else if (exportPerChunk)
                    {
                        uv = new Vector2((float)x / (verticesPerGridX - 1), (float)z / (verticesPerGridZ - 1));
                    }
                    // BaseMap模式 + 不exportPerChunk：UV是整个terrain的归一化坐标
                    else
                    {
                        uv = new Vector2(normalizedX, normalizedZ);
                    }

                    uvs.Add(uv);
                }
            }
        }

        /// <summary>
        /// 生成三角形索引
        /// </summary>
        private void GenerateTriangles(List<int> triangles)
        {
            for (int z = 0; z < verticesPerGridZ - 1; z++)
            {
                for (int x = 0; x < verticesPerGridX - 1; x++)
                {
                    int topLeft = z * verticesPerGridX + x;
                    int topRight = topLeft + 1;
                    int bottomLeft = (z + 1) * verticesPerGridX + x;
                    int bottomRight = bottomLeft + 1;

                    triangles.Add(topLeft);
                    triangles.Add(bottomLeft);
                    triangles.Add(topRight);

                    triangles.Add(topRight);
                    triangles.Add(bottomLeft);
                    triangles.Add(bottomRight);
                }
            }
        }

        /// <summary>
        /// 计算Pivot偏移
        /// </summary>
        private Vector3 CalculatePivotOffset(List<Vector3> vertices)
        {
            if (pivotPosition == SimpleTerrainToMeshWindow.PivotPosition.DefaultZero)
            {
                return Vector3.zero;
            }
            else
            {
                Vector3 min = vertices[0];
                Vector3 max = vertices[0];

                foreach (var vertex in vertices)
                {
                    min = Vector3.Min(min, vertex);
                    max = Vector3.Max(max, vertex);
                }

                return (min + max) * 0.5f;
            }
        }

        /// <summary>
        /// 从Terrain计算法线
        /// </summary>
        private void CalculateNormalsFromTerrain(Mesh mesh, List<Vector3> vertices, Vector3 pivotOffset, float startX, float startZ)
        {
            List<Vector3> normals = new List<Vector3>();
            float sampleDistance = 1f;

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 vertex = vertices[i] + pivotOffset;

                float normalizedX = vertex.x / terrainSize.x;
                float normalizedZ = vertex.z / terrainSize.z;

                float heightL = SampleTerrainHeight(normalizedX - sampleDistance / terrainSize.x, normalizedZ);
                float heightR = SampleTerrainHeight(normalizedX + sampleDistance / terrainSize.x, normalizedZ);
                float heightD = SampleTerrainHeight(normalizedX, normalizedZ - sampleDistance / terrainSize.z);
                float heightU = SampleTerrainHeight(normalizedX, normalizedZ + sampleDistance / terrainSize.z);

                Vector3 tangent = new Vector3(sampleDistance * 2, heightR - heightL, 0);
                Vector3 bitangent = new Vector3(0, heightU - heightD, sampleDistance * 2);
                Vector3 normal = Vector3.Cross(bitangent, tangent).normalized;

                normals.Add(normal);
            }

            mesh.SetNormals(normals);
        }

        /// <summary>
        /// 采样Terrain高度
        /// </summary>
        private float SampleTerrainHeight(float normalizedX, float normalizedZ)
        {
            normalizedX = Mathf.Clamp01(normalizedX);
            normalizedZ = Mathf.Clamp01(normalizedZ);
            return terrainData.GetInterpolatedHeight(normalizedX, normalizedZ);
        }

        /// <summary>
        /// 获取或创建材质
        /// </summary>
        private Material GetOrCreateMaterial(int gridX, int gridZ, string outputPath, 
        float startX, float startZ, float gridWidth, float gridLength)
        {
            // Splatmap模式：所有网格共享一个材质
            if (materialType == SimpleTerrainToMeshWindow.MaterialType.Splatmap)
            {
                if (sharedMaterial == null)
                {
                    string materialName = $"{terrain.name}_Splatmap";
                    sharedMaterial = splatmapBuilder.CreateSplatmapMaterial(materialShader, outputPath, materialName);
                }

                // 注意：Splatmap模式下mesh的UV应该是整个terrain的归一化坐标
                // 不需要为每个chunk调整材质参数，UV已经在GenerateVerticesAndUVs中正确设置
                return sharedMaterial;
            }

            // BaseMap模式：整个Terrain共享一个材质
            if (materialType == SimpleTerrainToMeshWindow.MaterialType.BaseMap && !exportPerChunk)
            {
                if (sharedMaterial == null)
                {
                    string materialName = $"{terrain.name}_Material";
                    sharedMaterial = CreateBaseMapMaterial(materialName, outputPath, 0, 0, terrainSize.x, terrainSize.z);
                }

                return sharedMaterial;
            }
            
            // AtlasMap模式：所有网格共享一个材质
            if (materialType == SimpleTerrainToMeshWindow.MaterialType.AtlasMap)
            {
                if (sharedMaterial == null)
                {
                    string materialName = $"{terrain.name}_Atlasmap";
                    sharedMaterial = splatmapBuilder.CreateAtlasmapMaterial(materialShader,  outputPath, materialName, atlasOutputFolder);
                }
                return sharedMaterial;
            }

            // BaseMap模式：每个Chunk单独材质
            string chunkMaterialName = $"Material_{gridX}_{gridZ}";
            return CreateBaseMapMaterial(chunkMaterialName, outputPath, startX, startZ, gridWidth, gridLength);
        }

        /// <summary>
        /// 创建BaseMap材质
        /// </summary>
        private Material CreateBaseMapMaterial(string materialName, string outputPath, 
            float startX, float startZ, float width, float length)
        {
            Material material;

            if (materialShader != null)
            {
                material = new Material(materialShader);
            }
            else
            {
                Shader defaultShader = Shader.Find("Universal Render Pipeline/Lit");
                if (defaultShader == null)
                {
                    defaultShader = Shader.Find("Standard");
                }

                material = new Material(defaultShader);
            }

            material.name = materialName;

            // 烘焙并应用纹理
            textureBaker.BakeAndApplyTextures(material, outputPath, materialName, startX, startZ, width, length);

            // 保存材质
            string materialPath = Path.Combine(outputPath, $"{materialName}.mat");
            AssetDatabase.CreateAsset(material, materialPath);

            return material;
        }
    }
}