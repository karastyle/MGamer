using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SceneChunkConfig", menuName = "Scene/Scene Chunk Config")]
public class SceneChunkConfig : ScriptableObject
{
    [Header("基础设置")]
    public string baseNodeName = "Base";
    public string staticNodeName = "Static";
    public string terrainNodeName = "Terrain";
    public float chunkSize = 100f;
    public bool showPreview = true;
    public int loadRadius = 1;
    
    [Header("路径设置")]
    public string sceneRootPath = "Assets/Scenes";
    public string lightingPrefabPath = "Lighting/GlobalLightingSettings.prefab";
    public string exportPath = "Chunks";
    public string lightmapOutputPath = "Lightmaps";
    
    [Header("光照设置")]
    public GameObject globalLightingPrefab;
    
    public bool Validate(out string errorMessage)
    {
        errorMessage = "";
        
        if (string.IsNullOrEmpty(baseNodeName))
        {
            errorMessage = "Base节点名称不能为空";
            return false;
        }
        
        if (string.IsNullOrEmpty(staticNodeName))
        {
            errorMessage = "Static节点名称不能为空";
            return false;
        }
        
        if (chunkSize <= 0)
        {
            errorMessage = "分块大小必须大于0";
            return false;
        }
        
        if (loadRadius < 1)
        {
            errorMessage = "加载半径必须至少为1";
            return false;
        }
        
        if (string.IsNullOrEmpty(sceneRootPath))
        {
            errorMessage = "场景根路径不能为空";
            return false;
        }
        
        return true;
    }
    
    public void CopyTo(SceneChunkConfig target)
    {
        if (target == null) return;
        
        target.baseNodeName = this.baseNodeName;
        target.staticNodeName = this.staticNodeName;
        target.terrainNodeName = this.terrainNodeName;
        target.chunkSize = this.chunkSize;
        target.showPreview = this.showPreview;
        target.loadRadius = this.loadRadius;
        
        target.sceneRootPath = this.sceneRootPath;
        target.lightingPrefabPath = this.lightingPrefabPath;
        target.exportPath = this.exportPath;
        target.lightmapOutputPath = this.lightmapOutputPath;
        
        target.globalLightingPrefab = this.globalLightingPrefab;
    }
    
    public void CopyFrom(SceneChunkConfig source)
    {
        if (source == null) return;
        source.CopyTo(this);
    }
}