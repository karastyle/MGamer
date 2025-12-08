// AssetBundleConfig.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AssetBundleConfig", menuName = "AssetBundle/Config")]
public class AssetBundleConfig : ScriptableObject
{
    public string outputPath = "AssetBundles";
    public string version = "1.0.0";
    public bool disableWriteTypeTree = true;
    public List<CollectorGroup> collectorGroups = new List<CollectorGroup>();
    
    [Header("Shader Settings")]
    [Tooltip("ShaderVariant收集目录")]
    public UnityEngine.Object shaderVariantPath;
    
    [Header("Encryption Settings")]
    [Tooltip("启用AB加密（头部插入随机数据）")]
    public bool enableEncryption = false;
    
    [Tooltip("拷贝后压缩BundlePackTools为zip")]
    public bool compressResToZip = false;
    
    [Header("Build Player Settings")]
    public string buildOutputPath = "Build";
    public string buildVersion = "1.0.0";
    public string copyAbVersion = "1.0.0";
    public BuildInCopyOption buildInCopyOption = BuildInCopyOption.None;
    public List<UnityEngine.Object> buildScenes = new List<UnityEngine.Object>();
}

[Serializable]
public enum BuildInCopyOption
{
    None,
    CopyAll,
    CopyBuildin
}

[Serializable]
public enum AssetTag
{
    None,
    Buildin
}

[Serializable]
public class CollectorGroup
{
    public string groupName = "Default";
    public bool active = true;
    public List<Collector> collectors = new List<Collector>();
}

[Serializable]
public class Collector
{
    public UnityEngine.Object collectorPath;
    public PackRule packRule = PackRule.PackDirectory;
    public CollectType collectType = CollectType.CollectAll;
    public AssetTag assetTag = AssetTag.None;
}

public enum PackRule
{
    PackSeparately,
    PackDirectory,
    PackTopDirectory
}

public enum CollectType
{
    CollectAll,
    CollectPrefab,
    CollectSprite,
    CollectSpriteAtlas,
    CollectScene
}