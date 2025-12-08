using UnityEngine;
using System.Collections.Generic;

namespace SimpleTerrainToMesh.Editor
{
    /// <summary>
    /// Terrain转Mesh配置文件
    /// 用于保存和加载所有转换参数
    /// </summary>
    [CreateAssetMenu(fileName = "TerrainToMeshConfig", menuName = "Terrain/Terrain To Mesh Config")]
    public class TerrainToMeshConfig : ScriptableObject
    {
        [Header("网格分割设置")]
        [Tooltip("X轴分割数量")]
        public int gridSplitX = 2;
        
        [Tooltip("Z轴分割数量")]
        public int gridSplitZ = 2;
        
        [Tooltip("每个网格X方向顶点数")]
        public int verticesPerGridX = 100;
        
        [Tooltip("每个网格Z方向顶点数")]
        public int verticesPerGridZ = 100;
        
        [Header("Mesh设置")]
        [Tooltip("中心点位置")]
        public SimpleTerrainToMeshWindow.PivotPosition pivotPosition = SimpleTerrainToMeshWindow.PivotPosition.DefaultZero;
        
        [Tooltip("法线计算方式")]
        public SimpleTerrainToMeshWindow.NormalCalculationMode normalMode = SimpleTerrainToMeshWindow.NormalCalculationMode.CalculateFromMesh;
        
        [Tooltip("生成Mesh Collider")]
        public bool generateMeshCollider = true;
        
        [Header("材质设置")]
        [Tooltip("材质类型")]
        public SimpleTerrainToMeshWindow.MaterialType materialType = SimpleTerrainToMeshWindow.MaterialType.BaseMap;
        
        [Tooltip("BaseMap材质Shader")]
        public Shader materialShader_BaseMap = null;
        
        [Tooltip("SplatMap材质Shader")]
        public Shader materialShader_SplatMap = null;
        
        [Tooltip("AtlasMap材质Shader")]
        public Shader materialShader_AtlasMap = null;
        
        [Tooltip("每个Chunk导出独立材质（仅BaseMap模式）")]
        public bool exportPerChunk = false;
        
        [Tooltip("BaseMap纹理分辨率")]
        public int textureResolution_BaseMap = 2048;
        
        [Tooltip("SplatMap纹理分辨率")]
        public int textureResolution_SplatMap = 1024;
        
        [Tooltip("使用Texture 2D Array（仅SplatMap模式）")]
        public bool useTexture2DArray = false;
        
        [Tooltip("Paint纹理数组分辨率")]
        public int textureResolution_Paint = 2048;
        
        [Header("路径设置")]
        [Tooltip("根路径")]
        public string rootPath = "Assets/GeneratedMeshes";
        
        [Tooltip("Mesh输出文件夹名")]
        public string meshOutputFolder = "Meshes";
        
        [Header("父节点设置")]
        [Tooltip("父节点名称")]
        public string parentNodeName = "TerrainStatic";
        
        [Tooltip("递归设置Static")]
        public bool setStaticRecursively = true;
        
        [Header("Terrain列表")]
        [Tooltip("待转换的Terrain列表")]
        public List<Terrain> terrainList = new List<Terrain>();
        
        [Tooltip("Mesh输出文件夹名")]
        public string atlasOutputFolder = "Atlas";
        
        /// <summary>
        /// 验证配置
        /// </summary>
        public bool Validate(out string errorMessage)
        {
            errorMessage = "";
            
            if (gridSplitX < 1)
            {
                errorMessage = "X轴分割数量必须大于0";
                return false;
            }
            
            if (gridSplitZ < 1)
            {
                errorMessage = "Z轴分割数量必须大于0";
                return false;
            }
            
            if (verticesPerGridX < 2)
            {
                errorMessage = "每个网格X方向顶点数必须至少为2";
                return false;
            }
            
            if (verticesPerGridZ < 2)
            {
                errorMessage = "每个网格Z方向顶点数必须至少为2";
                return false;
            }
            
            if (textureResolution_BaseMap < 64 || textureResolution_BaseMap > 8192)
            {
                errorMessage = "BaseMap纹理分辨率必须在64-8192之间";
                return false;
            }
            
            if (textureResolution_SplatMap < 64 || textureResolution_SplatMap > 8192)
            {
                errorMessage = "SplatMap纹理分辨率必须在64-8192之间";
                return false;
            }
            
            if (string.IsNullOrEmpty(rootPath))
            {
                errorMessage = "根路径不能为空";
                return false;
            }
            
            if (string.IsNullOrEmpty(parentNodeName))
            {
                errorMessage = "父节点名称不能为空";
                return false;
            }
            
            if (terrainList == null || terrainList.Count == 0)
            {
                errorMessage = "Terrain列表不能为空";
                return false;
            }
            
            // 移除空引用
            terrainList.RemoveAll(t => t == null);
            
            if (terrainList.Count == 0)
            {
                errorMessage = "Terrain列表中没有有效的Terrain";
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 复制配置到另一个对象
        /// </summary>
        public void CopyTo(TerrainToMeshConfig target)
        {
            if (target == null) return;
            
            target.gridSplitX = this.gridSplitX;
            target.gridSplitZ = this.gridSplitZ;
            target.verticesPerGridX = this.verticesPerGridX;
            target.verticesPerGridZ = this.verticesPerGridZ;
            
            target.pivotPosition = this.pivotPosition;
            target.normalMode = this.normalMode;
            target.generateMeshCollider = this.generateMeshCollider;
            
            target.materialType = this.materialType;
            target.materialShader_BaseMap = this.materialShader_BaseMap;
            target.materialShader_SplatMap = this.materialShader_SplatMap;
            target.exportPerChunk = this.exportPerChunk;
            target.textureResolution_BaseMap = this.textureResolution_BaseMap;
            target.textureResolution_SplatMap = this.textureResolution_SplatMap;
            target.useTexture2DArray = this.useTexture2DArray;
            target.textureResolution_Paint = this.textureResolution_Paint;
            
            target.rootPath = this.rootPath;
            target.meshOutputFolder = this.meshOutputFolder;
            
            target.parentNodeName = this.parentNodeName;
            target.setStaticRecursively = this.setStaticRecursively;
            
            target.terrainList = new List<Terrain>(this.terrainList);
        }
        
        /// <summary>
        /// 从另一个对象复制配置
        /// </summary>
        public void CopyFrom(TerrainToMeshConfig source)
        {
            if (source == null) return;
            source.CopyTo(this);
        }
    }
}