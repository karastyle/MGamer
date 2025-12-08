// BackgroundDownloadManager.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

#if UNITY_ANDROID || UNITY_IOS
using Unity.Networking;
#endif

namespace EasyTools
{
    public class BackgroundDownloadManager
    {
        private string _hostServer;
        private string _remoteVersion;
        private ManifestData _remoteManifest;
        private List<ManifestData.BundleInfo> _downloadList;
        private bool _isDownloading = false;
        private bool _downloadComplete = false;
        private string _cacheRoot;
        private string _tempRoot;
        private string _packageName = "DefaultPackage";
        
        // 调试选项：强制移动平台使用UnityWebRequest
        private bool _forceUseUnityWebRequest = true;
        
#if UNITY_ANDROID || UNITY_IOS
        private List<BackgroundDownload> _activeDownloadsMobile = new List<BackgroundDownload>();
#endif
        private List<UnityWebRequest> _activeDownloads = new List<UnityWebRequest>();
        private Dictionary<UnityWebRequest, string> _requestPathMap = new Dictionary<UnityWebRequest, string>();
        private Dictionary<UnityWebRequest, float> _requestStartTime = new Dictionary<UnityWebRequest, float>();
        
        private Dictionary<string, ManifestData.BundleInfo> _downloadMap = new Dictionary<string, ManifestData.BundleInfo>();

        public bool IsDownloading => _isDownloading;
        public bool DownloadComplete => _downloadComplete;
        public int TotalCount { get; private set; }
        public int DownloadedCount { get; private set; }
        public ulong TotalBytes { get; private set; }
        public ulong DownloadedBytes { get; private set; }
        public float Progress => TotalBytes > 0 ? (float)DownloadedBytes / TotalBytes : 0f;

        public Action OnBackgroundDownloadComplete;

        private bool IsMobilePlatform()
        {
            return Application.platform == RuntimePlatform.Android || 
                   Application.platform == RuntimePlatform.IPhonePlayer;
        }

        private bool ShouldUseBackgroundDownload()
        {
            return IsMobilePlatform() && !_forceUseUnityWebRequest;
        }

        public void Initialize(string hostServer)
        {
            _hostServer = hostServer;
            _cacheRoot = PathConfig.GetCacheRoot();
            _tempRoot = PathConfig.GetTempDownloadRoot();
        }

        public IEnumerator CheckAndResumeDownloads()
        {
            if (!ShouldUseBackgroundDownload())
            {
                yield break;
            }

#if UNITY_ANDROID || UNITY_IOS
            var existingDownloads = BackgroundDownload.backgroundDownloads;
            if (existingDownloads.Length > 0)
            {
                Debug.Log($"后台下载: 检测到 {existingDownloads.Length} 个未完成的下载");
                
                foreach (var download in existingDownloads)
                {
                    _activeDownloadsMobile.Add(download);
                }

                yield return MonitorDownloadsMobile();
            }
#else
            yield break;
#endif
        }

        public IEnumerator StartBackgroundDownload(string remoteVersion, ManifestData remoteManifest, ManifestData localManifest)
        {
            if (_isDownloading)
            {
                Debug.LogWarning("后台下载已在进行中");
                yield break;
            }

            _isDownloading = true;
            _downloadComplete = false;
            _remoteVersion = remoteVersion;
            _remoteManifest = remoteManifest;

            Debug.Log($"开始后台下载最新资源: Version={remoteVersion}, UseUnityWebRequest={_forceUseUnityWebRequest}");

            if (ShouldUseBackgroundDownload())
            {
                yield return CheckAndResumeDownloads();
            }

            _downloadList = CalculateDownloadList(remoteManifest, localManifest);
            
            if (_downloadList.Count == 0)
            {
                Debug.Log("后台下载: 没有需要下载的文件");
                _isDownloading = false;
                _downloadComplete = true;
                OnBackgroundDownloadComplete?.Invoke();
                yield break;
            }

            TotalCount = _downloadList.Count;
            TotalBytes = 0;
            foreach (var bundle in _downloadList)
            {
                TotalBytes += bundle.FileSize;
            }

            Debug.Log($"后台下载: 需要下载 {TotalCount} 个文件, 总大小: {TotalBytes / 1024f / 1024f:F2}MB");

            PathConfig.EnsureTempDirectoryExists();

            if (ShouldUseBackgroundDownload())
            {
                yield return StartAllDownloadsMobile();
                yield return MonitorDownloadsMobile();
            }
            else
            {
                yield return StartAllDownloadsPC();
                yield return MonitorDownloadsPC();
            }

            if (DownloadedCount == TotalCount)
            {
                yield return CopyToCache();
                _downloadComplete = true;
                OnBackgroundDownloadComplete?.Invoke();
                Debug.Log("后台下载完成");
            }

            _isDownloading = false;
        }

        private List<ManifestData.BundleInfo> CalculateDownloadList(ManifestData remoteManifest, ManifestData localManifest)
        {
            List<ManifestData.BundleInfo> downloadList = new List<ManifestData.BundleInfo>();
            HashSet<string> localBundleSet = new HashSet<string>();

            if (localManifest != null && localManifest.BundleList != null)
            {
                foreach (var bundle in localManifest.BundleList)
                {
                    localBundleSet.Add(bundle.BundleName);
                }
            }

            foreach (var remoteBundle in remoteManifest.BundleList)
            {
                bool needDownload = false;

                if (!localBundleSet.Contains(remoteBundle.BundleName))
                {
                    needDownload = true;
                }
                else
                {
                    var localBundle = localManifest.GetBundleInfo(remoteBundle.BundleName);
                    if (localBundle.FileHash != remoteBundle.FileHash)
                    {
                        needDownload = true;
                    }
                    else
                    {
                        string cachePath = Path.Combine(_cacheRoot, remoteBundle.BundleName);
                        
                        bool fileValid = false;
                        if (File.Exists(cachePath))
                        {
                            uint crc = CRCCalculator.CalculateFileCRC(cachePath);
                            if (crc == remoteBundle.FileCRC)
                            {
                                fileValid = true;
                            }
                        }

                        if (!fileValid)
                        {
                            needDownload = true;
                        }
                    }
                }

                if (needDownload)
                {
                    downloadList.Add(remoteBundle);
                }
            }

            return downloadList;
        }

        private IEnumerator StartAllDownloadsMobile()
        {
#if UNITY_ANDROID || UNITY_IOS
            string platform = PatchManager.GetPlatformName();

            foreach (var bundleInfo in _downloadList)
            {
                string url = $"{_hostServer}/{platform}/{_remoteVersion}/{bundleInfo.BundleName}";
                string tempPath = Path.Combine(_tempRoot, bundleInfo.BundleName);

                if (File.Exists(tempPath))
                {
                    uint crc = CRCCalculator.CalculateFileCRC(tempPath);
                    if (crc == bundleInfo.FileCRC)
                    {
                        DownloadedCount++;
                        DownloadedBytes += bundleInfo.FileSize;
                        Debug.Log($"后台下载: 文件已存在且校验通过，跳过 {bundleInfo.BundleName}");
                        continue;
                    }
                    else
                    {
                        File.Delete(tempPath);
                    }
                }

                try
                {
                    var config = new BackgroundDownloadConfig();
                    config.url = new Uri(url);
                    config.filePath = tempPath;
                    
                    var download = BackgroundDownload.Start(config);
                    _activeDownloadsMobile.Add(download);
                    _downloadMap[tempPath] = bundleInfo;
                    
                    Debug.Log($"后台下载: 启动下载 {bundleInfo.BundleName}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"后台下载启动失败: {bundleInfo.BundleName}, Error: {e.Message}");
                }

                yield return null;
            }
#else
            yield break;
#endif
        }

        private IEnumerator StartAllDownloadsPC()
        {
            string platform = PatchManager.GetPlatformName();
            var speedConfig = PatchUpdater.GetCurrentSpeedConfig();
            int maxConcurrent = speedConfig.MaxConcurrent;
            
            int startedCount = 0;

            foreach (var bundleInfo in _downloadList)
            {
                string url = $"{_hostServer}/{platform}/{_remoteVersion}/{bundleInfo.BundleName}";
                string tempPath = Path.Combine(_tempRoot, bundleInfo.BundleName);

                if (File.Exists(tempPath))
                {
                    uint crc = CRCCalculator.CalculateFileCRC(tempPath);
                    if (crc == bundleInfo.FileCRC)
                    {
                        DownloadedCount++;
                        DownloadedBytes += bundleInfo.FileSize;
                        Debug.Log($"下载: 文件已存在且校验通过，跳过 {bundleInfo.BundleName}");
                        continue;
                    }
                    else
                    {
                        File.Delete(tempPath);
                    }
                }

                string directory = Path.GetDirectoryName(tempPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                UnityWebRequest request = UnityWebRequest.Get(url);
                request.downloadHandler = new DownloadHandlerFile(tempPath);
                
                _activeDownloads.Add(request);
                _requestPathMap[request] = tempPath;
                _requestStartTime[request] = Time.realtimeSinceStartup;
                _downloadMap[tempPath] = bundleInfo;
                
                request.SendWebRequest();
                
                startedCount++;
                if (startedCount >= maxConcurrent)
                {
                    yield return null;
                }
                
                Debug.Log($"下载: 启动下载 {bundleInfo.BundleName}");
                
                yield return null;
            }
        }

        private IEnumerator MonitorDownloadsMobile()
        {
#if UNITY_ANDROID || UNITY_IOS
            while (_activeDownloadsMobile.Count > 0)
            {
                List<BackgroundDownload> completedDownloads = new List<BackgroundDownload>();
                ulong totalDownloaded = 0;

                foreach (var download in _activeDownloadsMobile)
                {
                    if (download.status == BackgroundDownloadStatus.Done)
                    {
                        string filePath = download.config.filePath;
                        
                        if (_downloadMap.ContainsKey(filePath))
                        {
                            var bundleInfo = _downloadMap[filePath];
                            
                            if (File.Exists(filePath))
                            {
                                uint crc = CRCCalculator.CalculateFileCRC(filePath);
                                if (crc == bundleInfo.FileCRC)
                                {
                                    DownloadedCount++;
                                    Debug.Log($"后台下载成功: {bundleInfo.BundleName} ({DownloadedCount}/{TotalCount})");
                                }
                                else
                                {
                                    Debug.LogError($"后台下载CRC校验失败: {bundleInfo.BundleName}");
                                    File.Delete(filePath);
                                }
                            }
                        }

                        completedDownloads.Add(download);
                    }
                    else if (download.status == BackgroundDownloadStatus.Failed)
                    {
                        Debug.LogError($"后台下载失败: {download.config.url}, Error: {download.error}");
                        completedDownloads.Add(download);
                    }
                    else if (download.status == BackgroundDownloadStatus.Downloading)
                    {
                        if (download.progress >= 0)
                        {
                            string filePath = download.config.filePath;
                            if (_downloadMap.ContainsKey(filePath))
                            {
                                var bundleInfo = _downloadMap[filePath];
                                totalDownloaded += (ulong)(bundleInfo.FileSize * download.progress);
                            }
                        }
                    }
                }

                DownloadedBytes = totalDownloaded;

                foreach (var download in completedDownloads)
                {
                    _activeDownloadsMobile.Remove(download);
                    download.Dispose();
                }

                yield return new WaitForSeconds(0.5f);
            }
#else
            yield break;
#endif
        }

        private IEnumerator MonitorDownloadsPC()
        {
            var speedConfig = PatchUpdater.GetCurrentSpeedConfig();
            ulong speedLimit = speedConfig.SpeedLimitBytesPerSecond;
            
            while (_activeDownloads.Count > 0)
            {
                List<UnityWebRequest> completedDownloads = new List<UnityWebRequest>();
                ulong totalDownloaded = 0;

                foreach (var request in _activeDownloads)
                {
                    if (request.isDone)
                    {
                        string filePath = _requestPathMap[request];
                        
                        if (_downloadMap.ContainsKey(filePath))
                        {
                            var bundleInfo = _downloadMap[filePath];
                            
                            if (request.result == UnityWebRequest.Result.Success)
                            {
                                if (File.Exists(filePath))
                                {
                                    uint crc = CRCCalculator.CalculateFileCRC(filePath);
                                    if (crc == bundleInfo.FileCRC)
                                    {
                                        DownloadedCount++;
                                        DownloadedBytes += bundleInfo.FileSize;
                                        Debug.Log($"下载成功: {bundleInfo.BundleName} ({DownloadedCount}/{TotalCount})");
                                    }
                                    else
                                    {
                                        Debug.LogError($"下载CRC校验失败: {bundleInfo.BundleName}");
                                        File.Delete(filePath);
                                    }
                                }
                            }
                            else
                            {
                                Debug.LogError($"下载失败: {bundleInfo.BundleName}, Error: {request.error}");
                            }
                        }

                        completedDownloads.Add(request);
                    }
                    else
                    {
                        string filePath = _requestPathMap[request];
                        if (_downloadMap.ContainsKey(filePath))
                        {
                            var bundleInfo = _downloadMap[filePath];
                            
                            if (speedLimit > 0 && _requestStartTime.ContainsKey(request))
                            {
                                float elapsed = Time.realtimeSinceStartup - _requestStartTime[request];
                                if (elapsed > 0)
                                {
                                    ulong downloaded = request.downloadedBytes;
                                    float currentSpeed = downloaded / elapsed;
                                    
                                    if (currentSpeed > speedLimit)
                                    {
                                        continue;
                                    }
                                }
                            }
                            
                            totalDownloaded += (ulong)(bundleInfo.FileSize * request.downloadProgress);
                        }
                    }
                }

                ulong completedBytes = (ulong)DownloadedCount * (TotalBytes / (ulong)TotalCount);
                DownloadedBytes = completedBytes + totalDownloaded;

                foreach (var request in completedDownloads)
                {
                    _activeDownloads.Remove(request);
                    _requestPathMap.Remove(request);
                    _requestStartTime.Remove(request);
                    request.Dispose();
                }

                yield return new WaitForSeconds(0.5f);
            }
        }

        private IEnumerator CopyToCache()
        {
            Debug.Log("后台下载: 开始拷贝文件到cache目录");

            PathConfig.EnsureCacheDirectoryExists();

            foreach (var bundleInfo in _downloadList)
            {
                string tempPath = Path.Combine(_tempRoot, bundleInfo.BundleName);
                string cachePath = Path.Combine(_cacheRoot, bundleInfo.BundleName);

                if (File.Exists(tempPath))
                {
                    string cacheDir = Path.GetDirectoryName(cachePath);
                    if (!Directory.Exists(cacheDir))
                    {
                        Directory.CreateDirectory(cacheDir);
                    }
                    
                    File.Copy(tempPath, cachePath, true);
                }

                yield return null;
            }

            if (ShouldUseBackgroundDownload())
            {
                yield return DownloadManifestFilesMobile();
            }
            else
            {
                yield return DownloadManifestFilesPC();
            }

            PathConfig.ClearTempDirectory();

            WriteDownloadInfo();

            Debug.Log("后台下载: 文件拷贝完成");
        }

        private void WriteDownloadInfo()
        {
            try
            {
                string downloadInfoPath = Path.Combine(_cacheRoot, "DownloadInfo.json");
                
                DownloadInfo downloadInfo = new DownloadInfo
                {
                    HasSmallDownloadFullRes = 1
                };
                
                string json = JsonUtility.ToJson(downloadInfo, true);
                File.WriteAllText(downloadInfoPath, json);
                
                Debug.Log($"[Background] DownloadInfo.json写入成功: {downloadInfoPath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Background] DownloadInfo.json写入失败: {e.Message}");
            }
        }

        private IEnumerator DownloadManifestFilesMobile()
        {
#if UNITY_ANDROID || UNITY_IOS
            string platform = PatchManager.GetPlatformName();
            string manifestName = $"{_packageName}_Manifest";
            string[] manifestFiles = { $"{manifestName}.json", $"{manifestName}.bytes", $"{manifestName}.hash", $"{manifestName}.version" };
            
            foreach (string fileName in manifestFiles)
            {
                string url = $"{_hostServer}/{platform}/{_remoteVersion}/{fileName}";
                string tempPath = Path.Combine(_tempRoot, fileName);
                string cachePath = Path.Combine(_cacheRoot, fileName);
                
                var config = new BackgroundDownloadConfig();
                config.url = new Uri(url);
                config.filePath = tempPath;
                    
                using (var download = BackgroundDownload.Start(config))
                {
                    yield return download;
                        
                    if (download.status == BackgroundDownloadStatus.Done && File.Exists(tempPath))
                    {
                        File.Copy(tempPath, cachePath, true);
                        Debug.Log($"后台下载: Manifest文件下载完成 {fileName}");
                    }
                }
            }
#else
            yield break;
#endif
        }

        private IEnumerator DownloadManifestFilesPC()
        {
            string platform = PatchManager.GetPlatformName();
            string manifestName = $"{_packageName}_Manifest";
            string[] manifestFiles = { $"{manifestName}.json", $"{manifestName}.bytes", $"{manifestName}.hash", $"{manifestName}.version" };
            
            foreach (string fileName in manifestFiles)
            {
                string url = $"{_hostServer}/{platform}/{_remoteVersion}/{fileName}";
                string tempPath = Path.Combine(_tempRoot, fileName);
                string cachePath = Path.Combine(_cacheRoot, fileName);
                
                using (UnityWebRequest request = UnityWebRequest.Get(url))
                {
                    request.downloadHandler = new DownloadHandlerFile(tempPath);
                    yield return request.SendWebRequest();
                    
                    if (request.result == UnityWebRequest.Result.Success && File.Exists(tempPath))
                    {
                        File.Copy(tempPath, cachePath, true);
                        Debug.Log($"下载: Manifest文件下载完成 {fileName}");
                    }
                    else
                    {
                        Debug.LogError($"下载Manifest文件失败: {fileName}, Error: {request.error}");
                    }
                }
            }
        }

        public void StopDownload()
        {
#if UNITY_ANDROID || UNITY_IOS
            foreach (var download in _activeDownloadsMobile)
            {
                download.Dispose();
            }
            _activeDownloadsMobile.Clear();
#endif
            
            foreach (var download in _activeDownloads)
            {
                download.Dispose();
            }
            _activeDownloads.Clear();
            _requestPathMap.Clear();
            _requestStartTime.Clear();
            
            _isDownloading = false;
        }
    }
}