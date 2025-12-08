// EasyAsset.cs (完整的双重索引机制)
namespace EasyTools
{
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum EPlayMode
{
    EditorSimulateMode,
    OfflinePlayMode,
    HostPlayMode
}

public class EasyAsset : MonoBehaviour
{
    private static EasyAsset _instance;
    public static EasyAsset Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("[EasyAsset]");
                _instance = go.AddComponent<EasyAsset>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private static EPlayMode _playMode;
    private bool _isInitialized = false;
    private PackageManifest _manifest;

    private Dictionary<string, ProviderBase> _providerDict = new Dictionary<string, ProviderBase>();
    private Dictionary<string, BundleLoader> _bundleLoaders = new Dictionary<string, BundleLoader>();
    
    // 双重索引：同一个AssetInfo可以通过Address或AssetPath访问
    private Dictionary<string, PackageManifest.AssetInfo> _assetInfoDict;
    private Dictionary<int, PackageManifest.BundleInfo> _bundleInfoDict;

    public static EPlayMode PlayMode => _playMode;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    #region 调试接口

    public Dictionary<string, ProviderBase> GetAllProviders()
    {
        return _providerDict;
    }

    public Dictionary<string, BundleLoader> GetAllBundleLoaders()
    {
        return _bundleLoaders;
    }

    public List<BundleLoader> GetLoadedBundlesWithRef()
    {
        return _bundleLoaders.Values.Where(l => l.RefCount > 0).ToList();
    }

    public List<ProviderBase> GetProvidersUsingBundle(BundleLoader loader)
    {
        List<ProviderBase> result = new List<ProviderBase>();
        foreach (var provider in _providerDict.Values)
        {
            if (provider is AssetProvider ap && ap.MainLoader == loader)
            {
                result.Add(provider);
            }
            else if (provider is SubAssetsProvider sap && sap.MainLoader == loader)
            {
                result.Add(provider);
            }
            else if (provider is SceneProvider sp && sp.MainLoader == loader)
            {
                result.Add(provider);
            }
        }
        return result;
    }

    #endregion

    #region 公开方法供外部使用

    /// <summary>
    /// 【公开】通过 Address 或 AssetPath 查找资源信息（供XLuaManager使用）
    /// </summary>
    public bool TryGetAssetInfo(string location, out PackageManifest.AssetInfo assetInfo)
    {
        if (_assetInfoDict == null)
        {
            assetInfo = null;
            return false;
        }
        return _assetInfoDict.TryGetValue(location, out assetInfo);
    }

    /// <summary>
    /// 【公开】获取或创建BundleLoader（供XLuaManager使用）
    /// </summary>
    public BundleLoader GetOrCreateBundleLoader(int bundleID)
    {
        return GetOrCreateBundleLoaderInternal(bundleID);
    }
    
    /// <summary>
    /// 【公开】检查场景是否存在
    /// </summary>
    public bool HasScene(string location)
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("EasyAsset未初始化");
            return false;
        }

        if (_playMode == EPlayMode.EditorSimulateMode)
        {
#if UNITY_EDITOR
            // 假设 location 传入的是 "Assets/Game/Map/Chunk_0_0" 这样的相对工程路径
            string checkPath = location;

            // 1. 确保后缀存在 (AssetDatabase 需要带后缀的完整路径)
            if (!checkPath.EndsWith(".unity"))
            {
                checkPath += ".unity";
            }

            // 2. 方法A: 使用 AssetPathToGUID (推荐，最快)
            // 如果路径对应的资源存在，它会返回一个GUID字符串；如果不存在，返回空字符串
            string guid = UnityEditor.AssetDatabase.AssetPathToGUID(checkPath);
            return !string.IsNullOrEmpty(guid);

            /* // 方法B: 或者使用 LoadAssetAtPath (如果你不仅想检查存在，还想确认它确实是个场景)
            // var sceneAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(checkPath);
            // return sceneAsset != null;
            */
#else
    return false;
#endif
        }
        else
        {
            // 运行时模式: 检查manifest中是否有该场景
            return TryGetAssetInfo(location, out _);
        }
    }

    //是否需要首包解压
    public bool NeedUnZipPack()
    {  
        string cacheRoot = PathConfig.GetCacheRoot();
        string manifestPath = Path.Combine(cacheRoot, "DefaultPackage_Manifest.json");
        if (!File.Exists(manifestPath))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 【公开】批量检查场景是否存在
    /// </summary>
    public Dictionary<string, bool> CheckScenesExist(List<string> sceneNames)
    {
        Dictionary<string, bool> result = new Dictionary<string, bool>();
        
        foreach (string sceneName in sceneNames)
        {
            result[sceneName] = HasScene(sceneName);
        }
        
        return result;
    }


    #endregion

    #region 初始化

    public IEnumerator InitializeAsync(EPlayMode playMode)
    {
        if (_isInitialized)
        {
            Debug.LogWarning("EasyAsset已经初始化");
            yield break;
        }

        _playMode = playMode;
        _assetInfoDict = new Dictionary<string, PackageManifest.AssetInfo>();
        _bundleInfoDict = new Dictionary<int, PackageManifest.BundleInfo>();

        Debug.Log($"EasyAsset初始化: {playMode}");

        if (playMode == EPlayMode.HostPlayMode || playMode == EPlayMode.OfflinePlayMode)
        {
            PathConfig.EnsureCacheDirectoryExists();
            
            string cachePath = PathConfig.GetCacheRoot();
            
            if (Directory.Exists(cachePath) && HasManifest(cachePath))
            {
                yield return LoadManifest(cachePath);
            }
            else
            {
                Debug.LogError("未找到Manifest文件");
            }
        }

        _isInitialized = true;
        Debug.Log("EasyAsset初始化完成");
    }

    public bool HasInitialized()
    {
        return _isInitialized;
    }

    public bool HasManifest(string path)
    {
        if (!Directory.Exists(path)) return false;
        
        string[] jsonFiles = Directory.GetFiles(path, "*.json", SearchOption.TopDirectoryOnly);
        return jsonFiles.Any(f => !f.EndsWith("buildlogtep.json", StringComparison.OrdinalIgnoreCase));
    }

    private IEnumerator LoadManifest(string manifestPath)
    {
        if (!Directory.Exists(manifestPath))
        {
            Debug.LogError($"目录不存在: {manifestPath}");
            yield break;
        }

        string[] jsonFiles = Directory.GetFiles(manifestPath, "*.json", SearchOption.TopDirectoryOnly);

        string jsonPath = jsonFiles.FirstOrDefault(f => 
            !f.EndsWith("buildlogtep.json", StringComparison.OrdinalIgnoreCase));

        if (string.IsNullOrEmpty(jsonPath))
        {
            Debug.LogError($"未找到有效的manifest文件 (已排除 buildlogtep.json)，路径: {manifestPath}");
            yield break;
        }

        Debug.Log($"加载manifest: {jsonPath}");

        string json = File.ReadAllText(jsonPath);
        _manifest = JsonUtility.FromJson<PackageManifest>(json);

        // 【核心】构建双重索引：Address 和 AssetPath 都指向同一个 AssetInfo
        foreach (var assetInfo in _manifest.AssetList)
        {
            // 索引1: 通过 Address（别名）访问
            if (!string.IsNullOrEmpty(assetInfo.Address))
            {
                _assetInfoDict[assetInfo.Address] = assetInfo;
            }

            // 索引2: 通过 AssetPath（全路径）访问
            if (!string.IsNullOrEmpty(assetInfo.AssetPath))
            {
                _assetInfoDict[assetInfo.AssetPath] = assetInfo;
            }
        }

        // 构建 Bundle 索引
        for (int i = 0; i < _manifest.BundleList.Count; i++)
        {
            _bundleInfoDict[i] = _manifest.BundleList[i];
        }

        Debug.Log($"Manifest加载完成: {_manifest.AssetList.Count}个资源, {_manifest.BundleList.Count}个Bundle");
        Debug.Log($"双重索引构建完成: _assetInfoDict 包含 {_assetInfoDict.Count} 个Key");
        yield return null;
    }

    #endregion

    #region 同步加载

    public AssetHandle LoadAssetSync(string location)
    {
        CheckInitialize();
        
        if (_providerDict.TryGetValue(location, out var provider))
        {
            AssetHandle handle = new AssetHandle();
            handle.SetProvider(provider);
            return handle;
        }

        AssetProvider newProvider = new AssetProvider();
        newProvider.SetLocation(location);
        _providerDict[location] = newProvider;

        if (_playMode == EPlayMode.EditorSimulateMode)
        {
            StartCoroutine(newProvider.LoadAsync());
        }
        else
        {
            if (TryGetAssetInfo(location, out var assetInfo))
            {
                BundleLoader mainLoader = GetOrCreateBundleLoaderInternal(assetInfo.BundleID);
                newProvider.MainLoader = mainLoader;
                
                List<BundleLoader> allDependLoaders = new List<BundleLoader>();
                allDependLoaders.Add(mainLoader);
                
                if (assetInfo.DependBundleIDs != null)
                {
                    foreach (int depID in assetInfo.DependBundleIDs)
                    {
                        BundleLoader depLoader = GetOrCreateBundleLoaderInternal(depID);
                        if (depLoader != null && !allDependLoaders.Contains(depLoader))
                        {
                            allDependLoaders.Add(depLoader);
                        }
                    }
                }
                
                newProvider.SetDependLoaders(allDependLoaders);
                
                foreach (var loader in allDependLoaders)
                {
                    loader.Retain();
                }
                
                StartCoroutine(newProvider.LoadAsync());
            }
            else
            {
                newProvider.SetStatus(EProviderStatus.Failed);
                newProvider.SetError($"资源未找到: {location}");
            }
        }

        AssetHandle resultHandle = new AssetHandle();
        resultHandle.SetProvider(newProvider);
        return resultHandle;
    }

    public SubAssetsHandle LoadSubAssetsSync(string location)
    {
        CheckInitialize();

        if (_providerDict.TryGetValue(location, out var provider))
        {
            SubAssetsHandle handle = new SubAssetsHandle();
            handle.SetProvider(provider);
            return handle;
        }

        SubAssetsProvider newProvider = new SubAssetsProvider();
        newProvider.SetLocation(location);
        _providerDict[location] = newProvider;

        if (_playMode == EPlayMode.EditorSimulateMode)
        {
            StartCoroutine(newProvider.LoadAsync());
        }
        else
        {
            if (TryGetAssetInfo(location, out var assetInfo))
            {
                BundleLoader mainLoader = GetOrCreateBundleLoaderInternal(assetInfo.BundleID);
                newProvider.MainLoader = mainLoader;
                
                List<BundleLoader> allDependLoaders = new List<BundleLoader>();
                allDependLoaders.Add(mainLoader);
                
                if (assetInfo.DependBundleIDs != null)
                {
                    foreach (int depID in assetInfo.DependBundleIDs)
                    {
                        BundleLoader depLoader = GetOrCreateBundleLoaderInternal(depID);
                        if (depLoader != null && !allDependLoaders.Contains(depLoader))
                        {
                            allDependLoaders.Add(depLoader);
                        }
                    }
                }
                
                newProvider.SetDependLoaders(allDependLoaders);
                
                foreach (var loader in allDependLoaders)
                {
                    loader.Retain();
                }
                
                StartCoroutine(newProvider.LoadAsync());
            }
            else
            {
                newProvider.SetStatus(EProviderStatus.Failed);
                newProvider.SetError($"资源未找到: {location}");
            }
        }

        SubAssetsHandle resultHandle = new SubAssetsHandle();
        resultHandle.SetProvider(newProvider);
        return resultHandle;
    }

    public SubAssetsHandle LoadAllAssetsSync(string location)
    {
        return LoadSubAssetsSync(location);
    }

    public RawFileHandle LoadRawFileSync(string location)
    {
        CheckInitialize();

        if (_providerDict.TryGetValue(location, out var provider))
        {
            RawFileHandle handle = new RawFileHandle();
            handle.SetProvider(provider);
            return handle;
        }

        RawFileProvider newProvider = new RawFileProvider();
        newProvider.SetLocation(location);
        _providerDict[location] = newProvider;
        StartCoroutine(newProvider.LoadAsync());

        RawFileHandle resultHandle = new RawFileHandle();
        resultHandle.SetProvider(newProvider);
        return resultHandle;
    }

    #endregion

    #region 异步加载

    public AssetHandle LoadAssetAsync(string location)
    {
        CheckInitialize();

        if (_providerDict.TryGetValue(location, out var provider))
        {
            AssetHandle handle = new AssetHandle();
            handle.SetProvider(provider);
            return handle;
        }

        AssetProvider newProvider = new AssetProvider();
        newProvider.SetLocation(location);
        _providerDict[location] = newProvider;

        if (_playMode == EPlayMode.EditorSimulateMode)
        {
            StartCoroutine(newProvider.LoadAsync());
        }
        else
        {
            if (TryGetAssetInfo(location, out var assetInfo))
            {
                BundleLoader mainLoader = GetOrCreateBundleLoaderInternal(assetInfo.BundleID);
                newProvider.MainLoader = mainLoader;
                
                List<BundleLoader> allDependLoaders = new List<BundleLoader>();
                allDependLoaders.Add(mainLoader);
                
                if (assetInfo.DependBundleIDs != null)
                {
                    foreach (int depID in assetInfo.DependBundleIDs)
                    {
                        BundleLoader depLoader = GetOrCreateBundleLoaderInternal(depID);
                        if (depLoader != null && !allDependLoaders.Contains(depLoader))
                        {
                            allDependLoaders.Add(depLoader);
                        }
                    }
                }
                
                newProvider.SetDependLoaders(allDependLoaders);
                
                foreach (var loader in allDependLoaders)
                {
                    loader.Retain();
                }
                
                StartCoroutine(newProvider.LoadAsync());
            }
            else
            {
                newProvider.SetStatus(EProviderStatus.Failed);
                newProvider.SetError($"资源未找到: {location}");
            }
        }

        AssetHandle resultHandle = new AssetHandle();
        resultHandle.SetProvider(newProvider);
        return resultHandle;
    }

    public SubAssetsHandle LoadSubAssetsAsync(string location)
    {
        CheckInitialize();

        if (_providerDict.TryGetValue(location, out var provider))
        {
            SubAssetsHandle handle = new SubAssetsHandle();
            handle.SetProvider(provider);
            return handle;
        }

        SubAssetsProvider newProvider = new SubAssetsProvider();
        newProvider.SetLocation(location);
        _providerDict[location] = newProvider;

        if (_playMode == EPlayMode.EditorSimulateMode)
        {
            StartCoroutine(newProvider.LoadAsync());
        }
        else
        {
            if (TryGetAssetInfo(location, out var assetInfo))
            {
                BundleLoader mainLoader = GetOrCreateBundleLoaderInternal(assetInfo.BundleID);
                newProvider.MainLoader = mainLoader;
                
                List<BundleLoader> allDependLoaders = new List<BundleLoader>();
                allDependLoaders.Add(mainLoader);
                
                if (assetInfo.DependBundleIDs != null)
                {
                    foreach (int depID in assetInfo.DependBundleIDs)
                    {
                        BundleLoader depLoader = GetOrCreateBundleLoaderInternal(depID);
                        if (depLoader != null && !allDependLoaders.Contains(depLoader))
                        {
                            allDependLoaders.Add(depLoader);
                        }
                    }
                }
                
                newProvider.SetDependLoaders(allDependLoaders);
                
                foreach (var loader in allDependLoaders)
                {
                    loader.Retain();
                }
                
                StartCoroutine(newProvider.LoadAsync());
            }
            else
            {
                newProvider.SetStatus(EProviderStatus.Failed);
                newProvider.SetError($"资源未找到: {location}");
            }
        }

        SubAssetsHandle resultHandle = new SubAssetsHandle();
        resultHandle.SetProvider(newProvider);
        return resultHandle;
    }

    public SubAssetsHandle LoadAllAssetsAsync(string location)
    {
        return LoadSubAssetsAsync(location);
    }

    public RawFileHandle LoadRawFileAsync(string location)
    {
        CheckInitialize();

        if (_providerDict.TryGetValue(location, out var provider))
        {
            RawFileHandle handle = new RawFileHandle();
            handle.SetProvider(provider);
            return handle;
        }

        RawFileProvider newProvider = new RawFileProvider();
        newProvider.SetLocation(location);
        _providerDict[location] = newProvider;
        StartCoroutine(newProvider.LoadAsync());

        RawFileHandle resultHandle = new RawFileHandle();
        resultHandle.SetProvider(newProvider);
        return resultHandle;
    }

    public SceneHandle LoadSceneAsync(string location, LoadSceneMode sceneMode = LoadSceneMode.Single)
    {
        CheckInitialize();

        if (_providerDict.TryGetValue(location, out var provider))
        {
            // 正常复用
            SceneHandle handle = new SceneHandle();
            handle.SetProvider(provider);
            return handle;
        }

        SceneProvider newProvider = new SceneProvider(sceneMode);
        newProvider.SetLocation(location);
        _providerDict[location] = newProvider;

        if (_playMode == EPlayMode.EditorSimulateMode)
        {
            StartCoroutine(newProvider.LoadAsync());
        }
        else
        {
            if (TryGetAssetInfo(location, out var assetInfo))
            {
                
                BundleLoader mainLoader = GetOrCreateBundleLoaderInternal(assetInfo.BundleID);
                newProvider.MainLoader = mainLoader;
                
                List<BundleLoader> allDependLoaders = new List<BundleLoader>();
                allDependLoaders.Add(mainLoader);
                
                if (assetInfo.DependBundleIDs != null)
                {
                    foreach (int depID in assetInfo.DependBundleIDs)
                    {
                        BundleLoader depLoader = GetOrCreateBundleLoaderInternal(depID);
                        if (depLoader != null && !allDependLoaders.Contains(depLoader))
                        {
                            allDependLoaders.Add(depLoader);
                        }
                    }
                }
                
                newProvider.SetDependLoaders(allDependLoaders);
                
                foreach (var loader in allDependLoaders)
                {
                    loader.Retain();
                }
                
                StartCoroutine(newProvider.LoadAsync());
            }
            else
            {
                newProvider.SetStatus(EProviderStatus.Failed);
                newProvider.SetError($"场景未找到: {location}");
            }
        }

        SceneHandle resultHandle = new SceneHandle();
        resultHandle.SetProvider(newProvider);
        return resultHandle;
    }

    #endregion

    #region BundleLoader管理

    private BundleLoader GetOrCreateBundleLoaderInternal(int bundleID)
    {
        if (!_bundleInfoDict.TryGetValue(bundleID, out var bundleInfo))
        {
            Debug.LogError($"Bundle信息未找到: ID={bundleID}");
            return null;
        }

        if (_bundleLoaders.TryGetValue(bundleInfo.BundleName, out var loader))
        {
            return loader;
        }

        BundleLoader newLoader = new BundleLoader(bundleInfo.BundleName, bundleInfo.UnityCRC, bundleInfo.Encrypted);
        _bundleLoaders[bundleInfo.BundleName] = newLoader;

        if (bundleInfo.DependBundleIDs != null && bundleInfo.DependBundleIDs.Length > 0)
        {
            foreach (int depID in bundleInfo.DependBundleIDs)
            {
                if (_bundleInfoDict.TryGetValue(depID, out var depBundleInfo))
                {
                    if (_bundleLoaders.TryGetValue(depBundleInfo.BundleName, out var depLoader))
                    {
                        newLoader.AddDependLoader(depLoader);
                    }
                    else
                    {
                        BundleLoader newDepLoader = new BundleLoader(depBundleInfo.BundleName, depBundleInfo.UnityCRC, bundleInfo.Encrypted);
                        _bundleLoaders[depBundleInfo.BundleName] = newDepLoader;
                        newLoader.AddDependLoader(newDepLoader);
                    }
                }
            }
        }

        return newLoader;
    }

    #endregion

    #region 资源卸载

    public void UnloadUnusedAssets()
    {
        List<string> toRemoveProviders = new List<string>();
        
        foreach (var kvp in _providerDict)
        {
            if (kvp.Value.RefCount <= 0)
            {
                toRemoveProviders.Add(kvp.Key);
            }
        }

        foreach (string key in toRemoveProviders)
        {
            if (_providerDict.TryGetValue(key, out var provider))
            {
                provider.Dispose();
                _providerDict.Remove(key);
            }
        }

        List<string> toRemoveLoaders = new List<string>();
        foreach (var kvp in _bundleLoaders)
        {
            if (kvp.Value.RefCount <= 0)
            {
                toRemoveLoaders.Add(kvp.Key);
            }
        }

        foreach (string bundleName in toRemoveLoaders)
        {
            if (_bundleLoaders.TryGetValue(bundleName, out var loader))
            {
                loader.Unload();
                _bundleLoaders.Remove(bundleName);
                Debug.Log($"卸载Bundle: {bundleName}");
            }
        }

        if (toRemoveProviders.Count > 0 || toRemoveLoaders.Count > 0)
        {
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
            Debug.Log($"卸载了 {toRemoveProviders.Count} 个Provider, {toRemoveLoaders.Count} 个Bundle");
        }
    }
    
    /// <summary>
    /// 内部方法：从字典移除Provider
    /// </summary>
    internal void RemoveProviderInternal(string location)
    {
        if (_providerDict.ContainsKey(location))
        {
            _providerDict.Remove(location);
            Debug.Log($"[EasyAsset] 已从字典移除Provider: {location}");
        }
    }

    #endregion

    #region 工具方法

    private void CheckInitialize()
    {
        if (!_isInitialized)
        {
            Debug.LogError("EasyAsset未初始化，请先调用InitializeAsync");
        }
    }

    public int GetProviderCount()
    {
        return _providerDict.Count;
    }

    public int GetLoadedBundleCount()
    {
        return _bundleLoaders.Count;
    }

    public void GetStatistics(out int providerCount, out int loaderCount, out int zeroRefProviders, out int zeroRefLoaders)
    {
        providerCount = _providerDict.Count;
        loaderCount = _bundleLoaders.Count;
        
        zeroRefProviders = 0;
        foreach (var provider in _providerDict.Values)
        {
            if (provider.RefCount <= 0) zeroRefProviders++;
        }

        zeroRefLoaders = 0;
        foreach (var loader in _bundleLoaders.Values)
        {
            if (loader.RefCount <= 0) zeroRefLoaders++;
        }
    }

    #endregion
}
}