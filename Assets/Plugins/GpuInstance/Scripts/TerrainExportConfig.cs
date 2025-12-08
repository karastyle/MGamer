// TerrainExportConfig.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TerrainExportConfig", menuName = "GPU Instancer/Terrain Export Config")]
public class TerrainExportConfig : ScriptableObject
{
    [System.Serializable]
    public class TerrainEntry
    {
        public Terrain terrain;
        public bool exportHeightmap = true;
        public bool exportTrees = true;
        public bool exportDetails = true;
        
        [Header("Detail Layers")]
        public List<int> detailLayerIndices = new List<int>(); // 空表示导出所有
    }
    
    public string exportRootPath = "Assets/TerrainData";
    public List<TerrainEntry> terrains = new List<TerrainEntry>();
    
    [Header("Export Settings")]
    public TextureFormat heightmapFormat = TextureFormat.RFloat;
    public TextureFormat detailFormat = TextureFormat.R8;
    public bool compressTextures = false;
    
    [Header("Tree Export")]
    public bool exportTreeColors = true;
}