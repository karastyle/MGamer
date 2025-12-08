// ProviderBase.cs (使用双重索引机制)

namespace EasyTools
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SceneManagement;
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.SceneManagement;
#endif
    using Object = UnityEngine.Object;

    public enum EProviderStatus
    {
        None,
        Loading,
        Succeed,
        Failed
    }

    public abstract class ProviderBase
    {
        protected string _location;
        protected EProviderStatus _status = EProviderStatus.None;
        protected string _lastError;
        protected int _refCount = 0;
        protected DateTime _loadCompleteTime;
        protected List<HandleBase> _handles = new List<HandleBase>();
        protected List<BundleLoader> _dependLoaders = new List<BundleLoader>();

        public string Location => _location;
        public EProviderStatus Status => _status;
        public string LastError => _lastError;
        public bool IsDone => _status == EProviderStatus.Succeed || _status == EProviderStatus.Failed;
        public int RefCount => _refCount;
        public DateTime LoadCompleteTime => _loadCompleteTime;
        public List<HandleBase> Handles => _handles;

        internal void SetLocation(string location)
        {
            _location = location;
        }

        internal void SetStatus(EProviderStatus status)
        {
            _status = status;
        }

        internal void SetError(string error)
        {
            _lastError = error;
        }

        internal void AddHandle(HandleBase handle)
        {
            if (!_handles.Contains(handle))
            {
                _handles.Add(handle);
            }
        }

        internal void RemoveHandle(HandleBase handle)
        {
            _handles.Remove(handle);
        }

        internal void SetDependLoaders(List<BundleLoader> loaders)
        {
            _dependLoaders = loaders;
        }

        public void Retain()
        {
            _refCount++;
        }

        public void Release()
        {
            _refCount--;
            if (_refCount < 0)
            {
                Debug.LogError($"Provider引用计数异常: {_location}, RefCount={_refCount}");
                _refCount = 0;
            }
        }

        public virtual void Dispose()
        {
            foreach (var loader in _dependLoaders)
            {
                loader.Release();
            }

            _dependLoaders.Clear();

            _refCount = 0;
            _handles.Clear();
        }

        public abstract IEnumerator LoadAsync();
        public abstract string GetAssetTypeName();
    }

    public class AssetProvider : ProviderBase
    {
        public Object AssetObject;
        public BundleLoader MainLoader;
        private AssetBundleRequest _request;

        public override string GetAssetTypeName()
        {
            if (AssetObject == null) return "Unknown";
            return AssetObject.GetType().Name;
        }

        public override void Dispose()
        {
            if (AssetObject != null && !(AssetObject is GameObject))
            {
                Resources.UnloadAsset(AssetObject);
            }

            AssetObject = null;
            MainLoader = null;
            _request = null;
            base.Dispose();
        }

        /// <summary>
        /// 真正的同步加载（不使用协程）
        /// </summary>
        public void LoadSync()
        {
            if (_status == EProviderStatus.Succeed || _status == EProviderStatus.Failed)
                return;

            _status = EProviderStatus.Loading;

            if (EasyAsset.PlayMode == EPlayMode.EditorSimulateMode)
            {
#if UNITY_EDITOR
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(_location);
                if (asset == null)
                {
                    string[] guids = AssetDatabase.FindAssets(System.IO.Path.GetFileNameWithoutExtension(_location));
                    foreach (string guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        if (System.IO.Path.GetFileNameWithoutExtension(path) ==
                            System.IO.Path.GetFileNameWithoutExtension(_location))
                        {
                            asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                            if (asset != null) break;
                        }
                    }
                }

                if (asset != null)
                {
                    AssetObject = asset;
                    _status = EProviderStatus.Succeed;
                    _loadCompleteTime = DateTime.Now;
                }
                else
                {
                    _status = EProviderStatus.Failed;
                    _lastError = $"资源不存在: {_location}";
                }
#endif
            }
            else
            {
                if (MainLoader == null)
                {
                    _status = EProviderStatus.Failed;
                    _lastError = "BundleLoader未设置";
                    return;
                }

                // 【关键】同步加载Bundle
                MainLoader.LoadSync();

                if (MainLoader.Status != EProviderStatus.Succeed)
                {
                    _status = EProviderStatus.Failed;
                    _lastError = $"Bundle加载失败: {MainLoader.LastError}";
                    return;
                }

                // 【关键】使用同步API：LoadAsset（不是Async）
                Object loadedAsset = MainLoader.Bundle.LoadAsset(_location);

                // 尝试其他路径
                if (loadedAsset == null)
                {
                    string lowerLocation = _location.ToLower();
                    loadedAsset = MainLoader.Bundle.LoadAsset(lowerLocation);
                }

                if (loadedAsset == null)
                {
                    string fileName = System.IO.Path.GetFileNameWithoutExtension(_location);
                    loadedAsset = MainLoader.Bundle.LoadAsset(fileName);
                }

                if (loadedAsset != null)
                {
                    AssetObject = loadedAsset;
                    _status = EProviderStatus.Succeed;
                    _loadCompleteTime = DateTime.Now;
                    Debug.Log($"[AssetProvider] 同步加载资源成功: {_location}");
                }
                else
                {
                    _status = EProviderStatus.Failed;
                    _lastError = $"从Bundle加载资源失败: {_location}";

                    string[] allAssets = MainLoader.Bundle.GetAllAssetNames();
                    Debug.LogError($"Bundle '{MainLoader.BundleName}' 中所有资源:\n{string.Join("\n", allAssets)}");
                }
            }
        }

        public override IEnumerator LoadAsync()
        {
            if (EasyAsset.PlayMode == EPlayMode.EditorSimulateMode)
            {
#if UNITY_EDITOR
                yield return null;
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(_location);
                if (asset == null)
                {
                    string[] guids = AssetDatabase.FindAssets(System.IO.Path.GetFileNameWithoutExtension(_location));
                    foreach (string guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        if (System.IO.Path.GetFileNameWithoutExtension(path) ==
                            System.IO.Path.GetFileNameWithoutExtension(_location))
                        {
                            asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                            if (asset != null) break;
                        }
                    }
                }

                if (asset != null)
                {
                    AssetObject = asset;
                    _status = EProviderStatus.Succeed;
                    _loadCompleteTime = DateTime.Now;
                }
                else
                {
                    _status = EProviderStatus.Failed;
                    _lastError = $"资源不存在: {_location}";
                }
#else
            yield return null;
#endif
            }
            else
            {
                if (MainLoader == null)
                {
                    _status = EProviderStatus.Failed;
                    _lastError = "BundleLoader未设置";
                    yield break;
                }

                if (!MainLoader.IsDone)
                {
                    yield return MainLoader.LoadAsync();
                }

                if (MainLoader.Status != EProviderStatus.Succeed)
                {
                    _status = EProviderStatus.Failed;
                    _lastError = $"Bundle加载失败: {MainLoader.LastError}";
                    yield break;
                }

                // AssetBundle中的资源名通常是全路径（小写）
                // 尝试按优先级加载：原路径 -> 小写路径 -> 文件名
                Object loadedAsset = null;

                // 方式1: 使用原始location（可能是Address或AssetPath）
                _request = MainLoader.Bundle.LoadAssetAsync(_location);
                yield return _request;
                loadedAsset = _request.asset;

                // 方式2: AssetBundle通常使用小写路径，尝试转小写
                if (loadedAsset == null)
                {
                    string lowerLocation = _location.ToLower();
                    _request = MainLoader.Bundle.LoadAssetAsync(lowerLocation);
                    yield return _request;
                    loadedAsset = _request.asset;
                }

                // 方式3: 兼容只使用文件名的情况
                if (loadedAsset == null)
                {
                    string fileName = System.IO.Path.GetFileNameWithoutExtension(_location);
                    _request = MainLoader.Bundle.LoadAssetAsync(fileName);
                    yield return _request;
                    loadedAsset = _request.asset;
                }

                if (loadedAsset != null)
                {
                    AssetObject = loadedAsset;
                    _status = EProviderStatus.Succeed;
                    _loadCompleteTime = DateTime.Now;
                }
                else
                {
                    _status = EProviderStatus.Failed;
                    _lastError = $"从Bundle加载资源失败: {_location}";

                    // 调试信息：输出Bundle中所有资源名
                    string[] allAssets = MainLoader.Bundle.GetAllAssetNames();
                    Debug.LogError($"Bundle '{MainLoader.BundleName}' 中所有资源:\n{string.Join("\n", allAssets)}");
                }
            }
        }
    }

    public class SubAssetsProvider : ProviderBase
    {
        public Object[] AllAssets;
        public BundleLoader MainLoader;
        private AssetBundleRequest _request;

        public override string GetAssetTypeName()
        {
            return "SubAssets";
        }

        public override void Dispose()
        {
            if (AllAssets != null)
            {
                foreach (var asset in AllAssets)
                {
                    if (asset != null && !(asset is GameObject))
                    {
                        Resources.UnloadAsset(asset);
                    }
                }
            }

            AllAssets = null;
            MainLoader = null;
            _request = null;
            base.Dispose();
        }

        public override IEnumerator LoadAsync()
        {
            if (EasyAsset.PlayMode == EPlayMode.EditorSimulateMode)
            {
#if UNITY_EDITOR
                yield return null;
                Object[] assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(_location);
                if (assets != null && assets.Length > 0)
                {
                    AllAssets = assets;
                    _status = EProviderStatus.Succeed;
                    _loadCompleteTime = DateTime.Now;
                }
                else
                {
                    _status = EProviderStatus.Failed;
                    _lastError = $"子资源不存在: {_location}";
                }
#else
            yield return null;
#endif
            }
            else
            {
                if (MainLoader == null)
                {
                    _status = EProviderStatus.Failed;
                    _lastError = "BundleLoader未设置";
                    yield break;
                }

                if (!MainLoader.IsDone)
                {
                    yield return MainLoader.LoadAsync();
                }

                if (MainLoader.Status != EProviderStatus.Succeed)
                {
                    _status = EProviderStatus.Failed;
                    _lastError = $"Bundle加载失败: {MainLoader.LastError}";
                    yield break;
                }

                Object[] loadedAssets = null;

                // 方式1: 使用原始location
                _request = MainLoader.Bundle.LoadAssetWithSubAssetsAsync(_location);
                yield return _request;
                loadedAssets = _request.allAssets;

                // 方式2: 尝试小写路径
                if (loadedAssets == null || loadedAssets.Length == 0)
                {
                    string lowerLocation = _location.ToLower();
                    _request = MainLoader.Bundle.LoadAssetWithSubAssetsAsync(lowerLocation);
                    yield return _request;
                    loadedAssets = _request.allAssets;
                }

                // 方式3: 尝试文件名
                if (loadedAssets == null || loadedAssets.Length == 0)
                {
                    string fileName = System.IO.Path.GetFileName(_location);
                    _request = MainLoader.Bundle.LoadAssetWithSubAssetsAsync(fileName);
                    yield return _request;
                    loadedAssets = _request.allAssets;
                }

                if (loadedAssets != null && loadedAssets.Length > 0)
                {
                    AllAssets = loadedAssets;
                    _status = EProviderStatus.Succeed;
                    _loadCompleteTime = DateTime.Now;
                }
                else
                {
                    _status = EProviderStatus.Failed;
                    _lastError = $"加载子资源失败: {_location}";
                }
            }
        }
    }

    public class SceneProvider : ProviderBase
    {
        public string SceneName;
        public BundleLoader MainLoader;
        private AsyncOperation _asyncOp;
        private LoadSceneMode _sceneMode;

        public float Progress => _asyncOp?.progress ?? 0f;

        public override string GetAssetTypeName()
        {
            return "Scene";
        }

        public SceneProvider(LoadSceneMode sceneMode)
        {
            _sceneMode = sceneMode;
        }

        public override void Dispose()
        {
            _asyncOp = null;
            MainLoader = null;
            base.Dispose();
        }

        public override IEnumerator LoadAsync()
        {
            if (EasyAsset.PlayMode == EPlayMode.EditorSimulateMode)
            {
#if UNITY_EDITOR
                SceneName = System.IO.Path.GetFileNameWithoutExtension(_location);
                AsyncOperation asyncOp = EditorSceneManager.LoadSceneAsyncInPlayMode(
                    _location,
                    new LoadSceneParameters(_sceneMode)
                );

                if (asyncOp != null)
                {
                    _asyncOp = asyncOp;
                    yield return asyncOp;
                    _status = EProviderStatus.Succeed;
                    _loadCompleteTime = DateTime.Now;
                }
                else
                {
                    _status = EProviderStatus.Failed;
                    _lastError = $"场景加载失败: {_location}";
                }
#else
            yield return null;
#endif
            }
            else
            {
                if (MainLoader == null)
                {
                    _status = EProviderStatus.Failed;
                    _lastError = "BundleLoader未设置";
                    yield break;
                }

                if (!MainLoader.IsDone)
                {
                    yield return MainLoader.LoadAsync();
                }

                if (MainLoader.Status != EProviderStatus.Succeed)
                {
                    _status = EProviderStatus.Failed;
                    _lastError = $"Bundle加载失败: {MainLoader.LastError}";
                    yield break;
                }

                SceneName = System.IO.Path.GetFileNameWithoutExtension(_location);
                AsyncOperation asyncOp = SceneManager.LoadSceneAsync(SceneName, _sceneMode);

                if (asyncOp != null)
                {
                    _asyncOp = asyncOp;
                    yield return asyncOp;
                    _status = EProviderStatus.Succeed;
                    _loadCompleteTime = DateTime.Now;
                }
                else
                {
                    _status = EProviderStatus.Failed;
                    _lastError = $"场景加载失败: {SceneName}";
                }
            }
        }

        public bool ActivateScene()
        {
            if (_status != EProviderStatus.Succeed) return false;
            Scene scene = SceneManager.GetSceneByName(SceneName);
            return scene.IsValid() && SceneManager.SetActiveScene(scene);
        }

        public bool IsActiveScene()
        {
            Scene scene = SceneManager.GetSceneByName(SceneName);
            return scene.IsValid() && scene == SceneManager.GetActiveScene();
        }

        /// <summary>
        /// 异步卸载场景
        /// </summary>
        public IEnumerator UnloadAsync()
        {
            if (string.IsNullOrEmpty(SceneName))
            {
                Debug.LogWarning("[SceneProvider] 场景名称为空,无法卸载");
                yield break;
            }

            Scene scene = SceneManager.GetSceneByName(SceneName);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                Debug.LogWarning($"[SceneProvider] 场景未加载或无效: {SceneName}");
                yield break;
            }

            Debug.Log($"[SceneProvider] 开始卸载场景: {SceneName}");

            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(SceneName);
            if (unloadOp != null)
            {
                yield return unloadOp;
                Debug.Log($"[SceneProvider] 场景卸载完成: {SceneName}");
            }
            else
            {
                Debug.LogError($"[SceneProvider] 场景卸载失败: {SceneName}");
            }
        }
    }

    public class RawFileProvider : ProviderBase
    {
        public byte[] Data;
        public string FilePath;

        public override string GetAssetTypeName()
        {
            return "RawFile";
        }

        public override void Dispose()
        {
            Data = null;
            FilePath = null;
            base.Dispose();
        }

        public override IEnumerator LoadAsync()
        {
            yield return null;

            if (EasyAsset.PlayMode == EPlayMode.EditorSimulateMode)
            {
#if UNITY_EDITOR
                if (System.IO.File.Exists(_location))
                {
                    Data = System.IO.File.ReadAllBytes(_location);
                    FilePath = _location;
                    _status = EProviderStatus.Succeed;
                    _loadCompleteTime = DateTime.Now;
                }
                else
                {
                    _status = EProviderStatus.Failed;
                    _lastError = $"文件不存在: {_location}";
                }
#endif
            }
            else if (EasyAsset.PlayMode == EPlayMode.HostPlayMode || EasyAsset.PlayMode == EPlayMode.OfflinePlayMode)
            {
                string filePath = PathConfig.GetFilePath(_location);
                if (!string.IsNullOrEmpty(filePath))
                {
                    Data = System.IO.File.ReadAllBytes(filePath);
                    FilePath = filePath;
                    _status = EProviderStatus.Succeed;
                    _loadCompleteTime = DateTime.Now;
                }
                else
                {
                    _status = EProviderStatus.Failed;
                    _lastError = $"文件不存在: {_location}";
                }
            }
        }
    }
}