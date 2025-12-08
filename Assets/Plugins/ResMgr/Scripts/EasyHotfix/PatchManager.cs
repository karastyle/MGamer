// PatchManager.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace EasyTools
{
    public class PatchManager
    {
        private string _hostServer;
        private string _packageName = "DefaultPackage";
        private string _majorVersionName = "major";
        private string _versionFileName;
        private string _manifestFileName;
        private string _localVersionFilePath;
        private string _localManifestPath;
        private string _cacheRoot;

        public string CurrentVersion { get; private set; }
        public string RemoteVersion { get; private set; }
        public ManifestData LocalManifest { get; private set; }
        public ManifestData RemoteManifest { get; private set; }

        public void Initialize(string hostServer)
        {
            _hostServer = hostServer;
            _cacheRoot = PathConfig.GetCacheRoot();
            _versionFileName = $"{_packageName}_Manifest";
            _manifestFileName = $"{_packageName}_Manifest";
            _localVersionFilePath = Path.Combine(_cacheRoot, $"{_versionFileName}.version");
            _localManifestPath = Path.Combine(_cacheRoot, $"{_manifestFileName}.json");
            PathConfig.EnsureCacheDirectoryExists();

            CurrentVersion = LoadLocalVersion();
        }

        private string LoadLocalVersion()
        {
            if (File.Exists(_localVersionFilePath))
            {
                return File.ReadAllText(_localVersionFilePath).Trim();
            }

            // ✅ 使用StreamingAssetsHelper读取包内文件
            string majorVersionText = StreamingAssetsHelper.ReadText($"{_majorVersionName}.version");
            if (!string.IsNullOrEmpty(majorVersionText))
            {
                return majorVersionText.Trim();
            }

            return "0.0.0";
        }

        /// <summary>
        /// 重新加载本地版本号（首包解压后调用）
        /// </summary>
        public void ReloadLocalVersion()
        {
            CurrentVersion = LoadLocalVersion();
            Debug.Log($"[PatchManager] 重新加载本地版本: {CurrentVersion}");
        }

        public static string GetPlatformName()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    return "PC";
                case RuntimePlatform.Android:
                    return "Android";
                case RuntimePlatform.IPhonePlayer:
                    return "iOS";
                default:
                    return Application.platform.ToString();
            }
        }
        
        public IEnumerator RequestPackageVersion()
        {
            string platform = GetPlatformName();
            string url = $"{_hostServer}/{platform}/{_versionFileName}.version";
            
            Debug.Log($"[PatchManager] RequestPackageVersion: {url}");
            
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    RemoteVersion = request.downloadHandler.text.Trim();
                }
                else
                {
                    Debug.LogError($"请求版本失败: {request.error}");
                    RemoteVersion = null;
                }
            }
        }

        public bool NeedUpdate()
        {
            if (string.IsNullOrEmpty(RemoteVersion)) return false;
            return CompareVersion(RemoteVersion, CurrentVersion) > 0;
        }

        private int CompareVersion(string v1, string v2)
        {
            string[] parts1 = v1.Split('.');
            string[] parts2 = v2.Split('.');
            
            for (int i = 0; i < Mathf.Max(parts1.Length, parts2.Length); i++)
            {
                int num1 = i < parts1.Length ? int.Parse(parts1[i]) : 0;
                int num2 = i < parts2.Length ? int.Parse(parts2[i]) : 0;
                
                if (num1 > num2) return 1;
                if (num1 < num2) return -1;
            }
            return 0;
        }

        public IEnumerator DownloadManifest(string version)
        {
            string platform = GetPlatformName();
            string jsonUrl = $"{_hostServer}/{platform}/{version}/{_manifestFileName}.json";
            using (UnityWebRequest request = UnityWebRequest.Get(jsonUrl))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string json = request.downloadHandler.text;
                    RemoteManifest = JsonUtility.FromJson<ManifestData>(json);
                }
                else
                {
                    Debug.LogError($"下载清单失败: {request.error}");
                    yield break;
                }
            }
            
            yield return DownloadFile($"{_manifestFileName}.bytes", Path.Combine(_cacheRoot, $"{_manifestFileName}.bytes"));
            yield return DownloadFile($"{_manifestFileName}.hash", Path.Combine(_cacheRoot, $"{_manifestFileName}.hash"));
            yield return DownloadFile($"{_manifestFileName}.json", Path.Combine(_cacheRoot, $"{_manifestFileName}.json"));
        }

        private IEnumerator DownloadFile(string fileName, string savePath)
        {
            string url = $"{_hostServer}/{fileName}";
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    File.WriteAllBytes(savePath, request.downloadHandler.data);
                }
            }
        }

        public void LoadLocalManifest()
        {
            if (File.Exists(_localManifestPath))
            {
                string json = File.ReadAllText(_localManifestPath);
                LocalManifest = JsonUtility.FromJson<ManifestData>(json);
                return;
            }

            LocalManifest = new ManifestData();
        }

        public List<string> VerifyLocalBundles()
        {
            List<string> validBundles = new List<string>();

            if (LocalManifest == null || LocalManifest.BundleList.Count == 0)
                return validBundles;

            foreach (var bundleInfo in LocalManifest.BundleList)
            {
                string filePath = Path.Combine(_cacheRoot, bundleInfo.BundleName);
                
                if (File.Exists(filePath))
                {
                    // 使用优化后的CRC计算
                    uint crc = CRCCalculator.CalculateFileCRC(filePath);
                    if (crc == bundleInfo.FileCRC)
                    {
                        validBundles.Add(bundleInfo.BundleName);
                    }
                }
            }

            return validBundles;
        }

        public List<ManifestData.BundleInfo> VerifyLocalBundlesBySize()
        {
            List<ManifestData.BundleInfo> invalidBundles = new List<ManifestData.BundleInfo>();

            if (LocalManifest == null || LocalManifest.BundleList.Count == 0)
                return invalidBundles;

            foreach (var bundleInfo in LocalManifest.BundleList)
            {
                string filePath = Path.Combine(_cacheRoot, bundleInfo.BundleName);

                if (!File.Exists(filePath))
                {
                    invalidBundles.Add(bundleInfo);
                    continue;
                }

                FileInfo fileInfo = new FileInfo(filePath);
                if ((ulong)fileInfo.Length != bundleInfo.FileSize)
                {
                    invalidBundles.Add(bundleInfo);
                }
            }

            return invalidBundles;
        }
        

        /// <summary>
        /// 校验本地Buildin资源（CRC校验）
        /// </summary>
        public List<string> VerifyBuildinBundles()
        {
            List<string> validBundles = new List<string>();

            if (LocalManifest == null || LocalManifest.BundleList.Count == 0)
                return validBundles;

            foreach (var bundleInfo in LocalManifest.BundleList)
            {
                // 只校验buildin资源
                bool isBuildin = false;
                if (bundleInfo.Tags != null)
                {
                    foreach (var tag in bundleInfo.Tags)
                    {
                        if (tag.ToLower() == "buildin")
                        {
                            isBuildin = true;
                            break;
                        }
                    }
                }

                if (!isBuildin)
                    continue;

                string filePath = Path.Combine(_cacheRoot, bundleInfo.BundleName);
                
                if (File.Exists(filePath))
                {
                    uint crc = CRCCalculator.CalculateFileCRC(filePath);
                    if (crc == bundleInfo.FileCRC)
                    {
                        validBundles.Add(bundleInfo.BundleName);
                    }
                }
            }

            return validBundles;
        }

        /// <summary>
        /// 校验本地Buildin资源（文件大小校验）
        /// </summary>
        public List<ManifestData.BundleInfo> VerifyBuildinBundlesBySize()
        {
            List<ManifestData.BundleInfo> invalidBundles = new List<ManifestData.BundleInfo>();

            if (LocalManifest == null || LocalManifest.BundleList.Count == 0)
                return invalidBundles;

            foreach (var bundleInfo in LocalManifest.BundleList)
            {
                // 只校验buildin资源
                bool isBuildin = false;
                if (bundleInfo.Tags != null)
                {
                    foreach (var tag in bundleInfo.Tags)
                    {
                        if (tag.ToLower() == "buildin")
                        {
                            isBuildin = true;
                            break;
                        }
                    }
                }

                if (!isBuildin)
                    continue;

                string filePath = Path.Combine(_cacheRoot, bundleInfo.BundleName);
                
                if (!File.Exists(filePath))
                {
                    invalidBundles.Add(bundleInfo);
                    continue;
                }

                FileInfo fileInfo = new FileInfo(filePath);
                if ((ulong)fileInfo.Length != bundleInfo.FileSize)
                {
                    invalidBundles.Add(bundleInfo);
                }
            }

            return invalidBundles;
        }
        
        /// <summary>
        /// 获取所有Buildin资源的数量
        /// </summary>
        public int GetBuildinBundleCount()
        {
            if (LocalManifest == null || LocalManifest.BundleList.Count == 0)
                return 0;

            int count = 0;
            foreach (var bundleInfo in LocalManifest.BundleList)
            {
                if (bundleInfo.Tags != null)
                {
                    foreach (var tag in bundleInfo.Tags)
                    {
                        if (tag.ToLower() == "buildin")
                        {
                            count++;
                            break;
                        }
                    }
                }
            }

            return count;
        }

        public List<ManifestData.BundleInfo> CompareDifference(List<string> validBundles)
        {
            List<ManifestData.BundleInfo> patchList = new List<ManifestData.BundleInfo>();

            if (RemoteManifest == null) return patchList;

            HashSet<string> validSet = new HashSet<string>(validBundles);
            HashSet<string> localBundleSet = new HashSet<string>();

            if (LocalManifest != null && LocalManifest.BundleList != null)
            {
                foreach (var bundle in LocalManifest.BundleList)
                {
                    localBundleSet.Add(bundle.BundleName);
                }
            }

            foreach (var remoteBundle in RemoteManifest.BundleList)
            {
                bool needDownload = false;

                if (!localBundleSet.Contains(remoteBundle.BundleName))
                {
                    needDownload = true;
                }
                else
                {
                    var localBundle = LocalManifest.GetBundleInfo(remoteBundle.BundleName);
                    if (localBundle.FileHash != remoteBundle.FileHash)
                    {
                        needDownload = true;
                    }
                    else if (!validSet.Contains(remoteBundle.BundleName))
                    {
                        needDownload = true;
                    }
                }

                if (needDownload)
                {
                    patchList.Add(remoteBundle);
                }
            }

            return patchList;
        }

        public void SaveManifest()
        {
            if (RemoteManifest != null)
            {
                string json = JsonUtility.ToJson(RemoteManifest, true);
                File.WriteAllText(_localManifestPath, json);
                LocalManifest = RemoteManifest;
            }
        }

        public void SaveVersion(string version)
        {
            File.WriteAllText(_localVersionFilePath, version);
            CurrentVersion = version;
        }

        public string GetCachePath(string fileName)
        {
            return Path.Combine(_cacheRoot, fileName);
        }
    }
}