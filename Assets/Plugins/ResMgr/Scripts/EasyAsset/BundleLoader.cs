namespace EasyTools
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class BundleLoader
    {
        private string _bundleName;
        private uint _unityCRC;
        private bool _encrypted;
        private int _refCount = 0;
        
        private EProviderStatus _status = EProviderStatus.None;
        private string _lastError;
        
        private AssetBundle _bundle;
        private List<BundleLoader> _dependLoaders = new List<BundleLoader>();
        private AssetBundleCreateRequest _request;
        private DateTime _loadCompleteTime; // 加载完成时间

        // --- 调试辅助 ---
        private static int _logDepth = 0; 
        private string LogPrefix => new string('-', _logDepth * 2);

        // --- 公开属性 ---
        public string BundleName => _bundleName;
        public int RefCount => _refCount;
        public EProviderStatus Status => _status;
        public string LastError => _lastError;
        public bool IsDone => _status == EProviderStatus.Succeed || _status == EProviderStatus.Failed;
        public AssetBundle Bundle => _bundle;
        public List<BundleLoader> DependLoaders => _dependLoaders;
        
        // [恢复] 补回漏掉的属性
        public DateTime LoadCompleteTime => _loadCompleteTime;

        public BundleLoader(string bundleName, uint unityCRC, bool encrypted)
        {
            _bundleName = bundleName;
            _unityCRC = unityCRC;
            _encrypted = encrypted;
        }

        public void AddDependLoader(BundleLoader loader)
        {
            // 【修复】增加 loader != this 判断，防止自循环依赖导致的死锁
            // 比如 a -> b -> a 这种情况，  由于依赖扁平化了，可能会导致 a 自己依赖自己
            if (loader != null && loader != this && !_dependLoaders.Contains(loader))
            {
                _dependLoaders.Add(loader);
            }
        }

        public void Retain() => _refCount++;
        
        public void Release()
        {
            _refCount--;
            if (_refCount < 0) _refCount = 0;
        }

        // [恢复] 补回漏掉的反向查找方法
        public List<BundleLoader> GetReferencedBy(Dictionary<string, BundleLoader> allLoaders)
        {
            List<BundleLoader> result = new List<BundleLoader>();
            foreach (var kvp in allLoaders)
            {
                if (kvp.Value != this && kvp.Value.DependLoaders.Contains(this))
                {
                    result.Add(kvp.Value);
                }
            }
            return result;
        }

        public IEnumerator LoadAsync()
        {
            _logDepth++;
            int myId = Time.frameCount;
            
            // [LOG] 开始
            Debug.Log($"{LogPrefix}[Start] {_bundleName} | Status: {_status} | Frame: {myId}");

            // 1. 成功检查
            if (_status == EProviderStatus.Succeed)
            {
                Debug.Log($"{LogPrefix}[FastReturn] {_bundleName} 已加载完成");
                _logDepth--;
                yield break;
            }

            // 2. 并发/死锁保护
            if (_status == EProviderStatus.Loading)
            {
                Debug.LogWarning($"{LogPrefix}[Wait] {_bundleName} 已处于Loading状态，进入并发等待...");
                
                float waitTime = 0f;
                // 超时保护：如果等待超过5秒，大概率是死锁或IO卡死
                while (!IsDone)
                {
                    yield return null;
                    waitTime += Time.deltaTime;
                    if (waitTime > 5.0f)
                    {
                        Debug.LogError($"{LogPrefix}[TIMEOUT] {_bundleName} 等待超时(>5s)！可能存在循环依赖死锁！");
                        _logDepth--;
                        yield break;
                    }
                }
                
                Debug.Log($"{LogPrefix}[WaitEnd] {_bundleName} 等待结束，结果: {_status}");
                _logDepth--;
                yield break;
            }

            // 标记为加载中
            _status = EProviderStatus.Loading;

            // 3. 加载依赖
            if (_dependLoaders.Count > 0)
            {
                foreach (var loader in _dependLoaders)
                {
                    if (!loader.IsDone)
                    {
                        Debug.Log($"{LogPrefix}[CallDep] {_bundleName} -> 调用依赖: {loader.BundleName}");
                        yield return loader.LoadAsync();
                    }
                    else if (loader.Status == EProviderStatus.Loading)
                    {
                        Debug.Log($"{LogPrefix}[WaitDep] {_bundleName} -> 等待依赖: {loader.BundleName}");
                        yield return new WaitUntil(() => loader.IsDone);
                    }

                    if (loader.Status != EProviderStatus.Succeed)
                    {
                        _status = EProviderStatus.Failed;
                        _lastError = $"依赖失败: {loader.BundleName} (Err: {loader.LastError})";
                        Debug.LogError($"{LogPrefix}[Fail] {_bundleName} 因依赖失败而终止");
                        _logDepth--;
                        yield break;
                    }
                }
            }

            // 4. 加载自己
            string bundlePath = GetBundlePath();
            if (string.IsNullOrEmpty(bundlePath))
            {
                _status = EProviderStatus.Failed;
                _lastError = $"路径无效";
                Debug.LogError($"{LogPrefix}[Fail] {_bundleName} 路径无效");
                _logDepth--;
                yield break;
            }

            Debug.Log($"{LogPrefix}[IO] {_bundleName} 发起LoadFromFileAsync: {bundlePath}");

            _request = _encrypted 
                ? AssetBundle.LoadFromFileAsync(bundlePath, _unityCRC, 128)
                : AssetBundle.LoadFromFileAsync(bundlePath, _unityCRC);

            if (_request == null)
            {
                _status = EProviderStatus.Failed;
                _lastError = "Request is null";
                _logDepth--;
                yield break;
            }

            // 等待IO
            yield return _request;

            // 5. 结果处理
            if (_request.assetBundle != null)
            {
                _bundle = _request.assetBundle;
                _status = EProviderStatus.Succeed;
                _loadCompleteTime = DateTime.Now; // [恢复] 记录时间
                Debug.Log($"{LogPrefix}[Success] {_bundleName} 加载成功");
            }
            else
            {
                _status = EProviderStatus.Failed;
                _lastError = "AssetBundle is null";
                Debug.LogError($"{LogPrefix}[Fail] {_bundleName} 加载后Bundle为空 (CRC: {_unityCRC})");
            }
            
            _logDepth--;
        }

        public void LoadSync()
        {
            if (_status == EProviderStatus.Succeed) return;
            
            if (_status == EProviderStatus.Loading)
            {
                Debug.LogWarning($"[BundleLoader] 同步加载遇到正在进行的异步加载: {_bundleName}，强制等待");
                while (!IsDone) { } 
                return;
            }

            _status = EProviderStatus.Loading;

            foreach (var loader in _dependLoaders)
            {
                loader.LoadSync();
                if (loader.Status != EProviderStatus.Succeed)
                {
                    _status = EProviderStatus.Failed;
                    _lastError = $"依赖失败: {loader.BundleName}";
                    return;
                }
            }

            string bundlePath = GetBundlePath();
            if (string.IsNullOrEmpty(bundlePath))
            {
                _status = EProviderStatus.Failed; return;
            }

            try 
            {
                _bundle = _encrypted 
                    ? AssetBundle.LoadFromFile(bundlePath, _unityCRC, 128)
                    : AssetBundle.LoadFromFile(bundlePath, _unityCRC);
            }
            catch(Exception e) { _lastError = e.Message; }

            if (_bundle != null)
            {
                _status = EProviderStatus.Succeed;
                _loadCompleteTime = DateTime.Now; // [恢复] 记录时间
            }
            else
            {
                _status = EProviderStatus.Failed;
                if(string.IsNullOrEmpty(_lastError)) _lastError = "LoadFromFile failed";
                Debug.LogError($"[SyncLoad] {_bundleName} 失败: {_lastError}");
            }
        }

        public void Unload()
        {
            if (_refCount > 0) return;
            if (_bundle != null) { _bundle.Unload(true); _bundle = null; }
            _request = null;
            _status = EProviderStatus.None;
        }

        private string GetBundlePath()
        {
            if (EasyAsset.PlayMode == EPlayMode.HostPlayMode || EasyAsset.PlayMode == EPlayMode.OfflinePlayMode)
                return PathConfig.GetFilePath(_bundleName);
            return null;
        }
    }
}