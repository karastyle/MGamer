// PatchUpdater.cs

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using UnityEngine;
using UniFramework.Machine;
using UnityEngine.Networking;

namespace EasyTools
{
    public enum PerformanceMode
    {
        FullSpeed,
        LowSpeed
    }

    public enum DownloadSpeedMode
    {
        FullSpeed,
        Medium,
        LowSpeed
    }

    public class PerformanceConfig
    {
        public int MaxConcurrency { get; set; }
        public int FilesPerFrame { get; set; }
        public int BufferSize { get; set; }
        public int FrameDelay { get; set; }

        public static PerformanceConfig FullSpeed => new PerformanceConfig
        {
            MaxConcurrency = 4,
            FilesPerFrame = 3,
            BufferSize = 65536,
            FrameDelay = 0
        };

        public static PerformanceConfig LowSpeed => new PerformanceConfig
        {
            MaxConcurrency = 1,
            FilesPerFrame = 1,
            BufferSize = 32768,
            FrameDelay = 1
        };
    }

    public class DownloadSpeedConfig
    {
        public int MaxConcurrent { get; set; }
        public ulong SpeedLimitBytesPerSecond { get; set; }

        public static DownloadSpeedConfig FullSpeed => new DownloadSpeedConfig
        {
            MaxConcurrent = 4,
            SpeedLimitBytesPerSecond = 0
        };

        public static DownloadSpeedConfig Medium => new DownloadSpeedConfig
        {
            MaxConcurrent = 2,
            SpeedLimitBytesPerSecond = 2 * 1024 * 1024
        };

        public static DownloadSpeedConfig LowSpeed => new DownloadSpeedConfig
        {
            MaxConcurrent = 1,
            SpeedLimitBytesPerSecond = 500 * 1024
        };
    }

    public class PatchUpdater
    {
        private StateMachine _machine;
        private PatchManager _patchManager;
        private DownloadManager _downloadManager;
        private BackgroundDownloadManager _backgroundDownloadManager;
        private string _hostServer;
        private int _maxUpdateLoops = 5;
        private AppConfig _appConfig;

        private static PerformanceMode _performanceMode = PerformanceMode.FullSpeed;
        private static PerformanceConfig _currentConfig = PerformanceConfig.FullSpeed;
        private static DownloadSpeedMode _downloadSpeedMode = DownloadSpeedMode.FullSpeed;
        private static DownloadSpeedConfig _currentSpeedConfig = DownloadSpeedConfig.FullSpeed;

        public string CurrentStepName { get; private set; } = "初始化";
        public int DownloadedCount => _downloadManager?.DownloadedCount ?? 0;
        public int TotalCount => _downloadManager?.TotalCount ?? 0;
        public ulong DownloadedBytes => _downloadManager?.DownloadedBytes ?? 0;
        public ulong TotalBytes => _downloadManager?.TotalBytes ?? 0;
        public float Progress => _downloadManager?.Progress ?? 0f;
        public bool HasError { get; private set; } = false;
        public string ErrorMessage { get; private set; } = "";

        public static void SetPerformanceMode(PerformanceMode mode)
        {
            _performanceMode = mode;
            _currentConfig = mode == PerformanceMode.FullSpeed
                ? PerformanceConfig.FullSpeed
                : PerformanceConfig.LowSpeed;

            Debug.Log($"[Patch] 性能模式已切换为: {mode}");
        }

        public static PerformanceMode GetPerformanceMode()
        {
            return _performanceMode;
        }

        internal static PerformanceConfig GetCurrentConfig()
        {
            return _currentConfig;
        }

        public static void SetCustomPerformanceConfig(PerformanceConfig config)
        {
            _currentConfig = config;
        }

        public static void SetDownloadSpeedMode(DownloadSpeedMode mode)
        {
            _downloadSpeedMode = mode;

            switch (mode)
            {
                case DownloadSpeedMode.FullSpeed:
                    _currentSpeedConfig = DownloadSpeedConfig.FullSpeed;
                    break;
                case DownloadSpeedMode.Medium:
                    _currentSpeedConfig = DownloadSpeedConfig.Medium;
                    break;
                case DownloadSpeedMode.LowSpeed:
                    _currentSpeedConfig = DownloadSpeedConfig.LowSpeed;
                    break;
            }

            Debug.Log(
                $"[Patch] 下载速度模式已切换为: {mode} (并发:{_currentSpeedConfig.MaxConcurrent}, 限速:{(_currentSpeedConfig.SpeedLimitBytesPerSecond == 0 ? "无限制" : $"{_currentSpeedConfig.SpeedLimitBytesPerSecond / 1024}KB/s")})");
        }

        public static DownloadSpeedMode GetDownloadSpeedMode()
        {
            return _downloadSpeedMode;
        }

        internal static DownloadSpeedConfig GetCurrentSpeedConfig()
        {
            return _currentSpeedConfig;
        }

        public static void SetCustomDownloadSpeedConfig(DownloadSpeedConfig config)
        {
            _currentSpeedConfig = config;
        }

        // 修改后的PatchUpdater构造函数代码段

        public PatchUpdater(string hostServer, bool isOfflineMode = false)
        {
            _hostServer = hostServer;
            _patchManager = new PatchManager();
            _downloadManager = new DownloadManager();
            _backgroundDownloadManager = new BackgroundDownloadManager();

            _machine = new StateMachine(this);
            _machine.AddNode<FsmCheckAppConfig>();
            _machine.AddNode<FsmUnpackFirstPackage>(); // ✅ 新增首包解压节点
            _machine.AddNode<FsmRequestVersion>();
            _machine.AddNode<FsmVerifyLocalSimple>();
            _machine.AddNode<FsmDownloadManifest>();
            _machine.AddNode<FsmVerifyLocal>();
            _machine.AddNode<FsmCompareDifference>();
            _machine.AddNode<FsmDownloadPatch>();
            _machine.AddNode<FsmDownloadBuildinPatch>();
            _machine.AddNode<FsmVerifyAndMove>();
            _machine.AddNode<FsmVerifyAndMoveBuildin>();
            _machine.AddNode<FsmPatchComplete>();
            _machine.AddNode<FsmStartBackgroundDownload>();

            _machine.SetBlackboardValue("PatchManager", _patchManager);
            _machine.SetBlackboardValue("DownloadManager", _downloadManager);
            _machine.SetBlackboardValue("BackgroundDownloadManager", _backgroundDownloadManager);
            _machine.SetBlackboardValue("HostServer", _hostServer);
            _machine.SetBlackboardValue("PatchUpdater", this);
            _machine.SetBlackboardValue("UpdateLoopCount", 0);
            _machine.SetBlackboardValue("MaxUpdateLoops", _maxUpdateLoops);
            _machine.SetBlackboardValue("IsOfflineMode", isOfflineMode);
        }

        public void SetStepName(string stepName)
        {
            CurrentStepName = stepName;
        }

        public void SetError(string error)
        {
            HasError = true;
            ErrorMessage = error;
        }

        public IEnumerator StartUpdate()
        {
            // ✅ 预加载StreamingAssets中的必要文件
            yield return StreamingAssetsHelper.ReadTextAsync("major.version", text =>
            {
                if (text != null)
                {
                    Debug.Log($"[Patch] major.version预加载完成: {text}");
                }
            });

            yield return StreamingAssetsHelper.ReadTextAsync("AppConfig.json", json =>
            {
                if (json != null)
                {
                    Debug.Log($"[Patch] AppConfig.json预加载完成");
                }
            });

            _patchManager.Initialize(_hostServer);
            _downloadManager.Initialize(_hostServer);
            _backgroundDownloadManager.Initialize(_hostServer);

            _machine.Run<FsmCheckAppConfig>();

            yield return new WaitUntil(() => string.IsNullOrEmpty(_machine.CurrentNode));
        }
    }

    // 修改后的FsmCheckAppConfig代码

    internal class FsmCheckAppConfig : IStateNode
    {
        private StateMachine _machine;

        void IStateNode.OnCreate(StateMachine machine)
        {
            _machine = machine;
        }

        void IStateNode.OnEnter()
        {
            var updater = (PatchUpdater)_machine.GetBlackboardValue("PatchUpdater");
            updater.SetStepName("检查安装包配置");

            // ✅ 从缓存读取AppConfig（已在StartUpdate中预加载）
            AppConfig appConfig = AppConfigLoader.LoadAppConfig();
            _machine.SetBlackboardValue("AppConfig", appConfig);

            Debug.Log($"[Patch] AppConfig PackageType: {appConfig.PackageType}");

            if (appConfig.PackageType == "Small")
            {
                string downloadInfoPath = Path.Combine(PathConfig.GetCacheRoot(), "DownloadInfo.json");

                if (File.Exists(downloadInfoPath))
                {
                    try
                    {
                        string json = File.ReadAllText(downloadInfoPath);
                        DownloadInfo downloadInfo = JsonUtility.FromJson<DownloadInfo>(json);

                        if (downloadInfo.HasSmallDownloadFullRes == 1)
                        {
                            Debug.Log("[Patch] Small包已有完整资源，走正常热更流程");
                            _machine.ChangeState<FsmUnpackFirstPackage>();
                            return;
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"[Patch] 读取DownloadInfo.json失败: {e.Message}");
                    }
                }

                Debug.Log("[Patch] Small包首次启动，走首包资源下载流程");
                _machine.ChangeState<FsmUnpackFirstPackage>();
                _machine.SetBlackboardValue("IsSmallPackageFirstRun", true);
            }
            else
            {
                Debug.Log($"[Patch] {appConfig.PackageType}包，走正常热更流程");
                _machine.ChangeState<FsmUnpackFirstPackage>();
            }
        }

        void IStateNode.OnUpdate()
        {
        }

        void IStateNode.OnExit()
        {
        }
    }

    internal class FsmUnpackFirstPackage : IStateNode
    {
        private StateMachine _machine;

        void IStateNode.OnCreate(StateMachine machine)
        {
            _machine = machine;
        }

        void IStateNode.OnEnter()
        {
            var updater = (PatchUpdater)_machine.GetBlackboardValue("PatchUpdater");
            updater.SetStepName("解压首包资源");

            EasyAsset.Instance.StartCoroutine(UnpackZip());
        }

        void IStateNode.OnUpdate()
        {
        }

        void IStateNode.OnExit()
        {
        }

        private IEnumerator UnpackZip()
        {
            string cacheRoot = PathConfig.GetCacheRoot();
            string manifestPath = Path.Combine(cacheRoot, "DefaultPackage_Manifest.json");
            var patchManager = (PatchManager)_machine.GetBlackboardValue("PatchManager");
            bool isOffline = (bool)_machine.GetBlackboardValue("IsOfflineMode");
            
            // ✅ 如果cache目录已有manifest文件，跳过解压
            if (File.Exists(manifestPath))
            {
                Debug.Log("[Patch] Cache目录已有manifest文件，跳过首包解压");
                patchManager.ReloadLocalVersion();

                if (isOffline)
                {
                    _machine.ChangeState<FsmPatchComplete>();
                }
                else
                {
                    _machine.ChangeState<FsmRequestVersion>();
                }
                yield break;
            }

            string zipFileName = "BundlePackTools.zip";
            string zipPath;

            // ✅ 移动平台需要先下载zip到临时目录
            // DownloadHandlerFile是流式写入，不需要完整加载到内存；而ReadBytes需要将整个zip加载到byte[]数组。因此推荐使用DownloadHandlerFile
            if (Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.IPhonePlayer)
            {
                string tempZipPath = Path.Combine(Application.temporaryCachePath, zipFileName);
                string streamingZipPath = Path.Combine(Application.streamingAssetsPath, zipFileName);

                Debug.Log($"[Patch] 移动平台，开始读取首包zip: {streamingZipPath}");

                bool downloadSuccess = false;
                using (UnityWebRequest request = UnityWebRequest.Get(streamingZipPath))
                {
                    request.downloadHandler = new DownloadHandlerFile(tempZipPath);
                    yield return request.SendWebRequest();

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        downloadSuccess = true;
                    }
                    else
                    {
                        Debug.LogWarning($"[Patch] BundlePackTools.zip不存在或读取失败: {request.error}");
                    }
                }

                if (!downloadSuccess)
                {
                    _machine.ChangeState<FsmRequestVersion>();
                    yield break;
                }

                zipPath = tempZipPath;
            }
            else
            {
                // ✅ PC/Editor平台直接使用StreamingAssets路径
                zipPath = Path.Combine(Application.streamingAssetsPath, zipFileName);

                if (!File.Exists(zipPath))
                {
                    Debug.LogWarning("[Patch] BundlePackTools.zip不存在，跳过解压");
                    _machine.ChangeState<FsmRequestVersion>();
                    yield break;
                }
            }

            Debug.Log($"[Patch] 开始解压首包资源: {zipPath}");

            PathConfig.EnsureCacheDirectoryExists();

            Task unpackTask = Task.Run(() => { ZipFile.ExtractToDirectory(zipPath, cacheRoot, true); });

            while (!unpackTask.IsCompleted)
            {
                yield return null;
            }

            // ✅ 移动平台删除临时zip文件
            if (Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.IPhonePlayer)
            {
                try
                {
                    if (File.Exists(zipPath))
                    {
                        File.Delete(zipPath);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[Patch] 删除临时zip文件失败: {e.Message}");
                }
            }

            if (unpackTask.IsFaulted)
            {
                var updater = (PatchUpdater)_machine.GetBlackboardValue("PatchUpdater");
                updater.SetError($"首包资源解压失败: {unpackTask.Exception?.GetBaseException().Message}");
                Debug.LogError($"[Patch] 首包资源解压失败: {unpackTask.Exception?.GetBaseException().Message}");
                yield break;
            }

            Debug.Log($"[Patch] 首包资源解压完成: {cacheRoot}");

            patchManager.ReloadLocalVersion();

            if (isOffline)
            {
                _machine.ChangeState<FsmPatchComplete>();
            }
            else
            {
                _machine.ChangeState<FsmRequestVersion>();
            }
        }
    }

    internal class FsmRequestVersion : IStateNode
    {
        private StateMachine _machine;

        void IStateNode.OnCreate(StateMachine machine)
        {
            _machine = machine;
        }

        void IStateNode.OnEnter()
        {
            var updater = (PatchUpdater)_machine.GetBlackboardValue("PatchUpdater");
            updater.SetStepName("请求版本号");

            var patchManager = (PatchManager)_machine.GetBlackboardValue("PatchManager");
            EasyAsset.Instance.StartCoroutine(RequestVersion(patchManager, updater));
        }

        void IStateNode.OnUpdate()
        {
        }

        void IStateNode.OnExit()
        {
        }

        private IEnumerator RequestVersion(PatchManager manager, PatchUpdater updater)
        {
            yield return manager.RequestPackageVersion();

            if (manager.RemoteVersion == null)
            {
                updater.SetError("请求版本失败");
                Debug.LogError("[Patch] 请求版本失败");
                yield break;
            }

            Debug.Log($"[Patch] 当前版本: {manager.CurrentVersion}, 远端版本: {manager.RemoteVersion}");

            if (manager.NeedUpdate())
            {
                _machine.ChangeState<FsmDownloadManifest>();
            }
            else
            {
                Debug.Log("[Patch] 版本号相同,进行快速完整性校验");
                _machine.ChangeState<FsmVerifyLocalSimple>();
            }
        }
    }

    internal class FsmVerifyLocalSimple : IStateNode
    {
        private StateMachine _machine;

        void IStateNode.OnCreate(StateMachine machine)
        {
            _machine = machine;
        }

        void IStateNode.OnEnter()
        {
            var updater = (PatchUpdater)_machine.GetBlackboardValue("PatchUpdater");
            updater.SetStepName("校验本地文件");

            var patchManager = (PatchManager)_machine.GetBlackboardValue("PatchManager");

            patchManager.LoadLocalManifest();

            // ✅ 检查是否只需要验证buildin资源
            bool isSmallPackageFirstRun = _machine.GetBlackboardValue("IsSmallPackageFirstRun") != null &&
                                          (bool)_machine.GetBlackboardValue("IsSmallPackageFirstRun");
            List<ManifestData.BundleInfo> invalidBundles;
            if (isSmallPackageFirstRun)
            {
                Debug.Log("[Patch] Small包未下载完整资源，只校验Buildin资源");
                invalidBundles = patchManager.VerifyBuildinBundlesBySize();
            }
            else
            {
                invalidBundles = patchManager.VerifyLocalBundlesBySize();
            }

            Debug.Log($"[Patch] 需要修复的Bundle数: {invalidBundles.Count}");

            if (invalidBundles.Count > 0)
            {
                _machine.SetBlackboardValue("RepairList", invalidBundles);

                if (isSmallPackageFirstRun)
                {
                    //如果是首包资源坏了，判断损坏百分比
                    int totalBuildinCount = patchManager.GetBuildinBundleCount();
                    float damageRatio = (float)invalidBundles.Count / totalBuildinCount;
                    Debug.Log($"[Patch] Buildin资源损坏百分比: {damageRatio * 100f}%");
                    if (damageRatio > 0.5f)
                    {
                        //损坏超过50%， 重新首包解压更快
                        string cacheRoot = PathConfig.GetCacheRoot();
                        string manifestPath = Path.Combine(cacheRoot, "DefaultPackage_Manifest.json");
                        // 删除manifest，强制重新解压首包
                        if (File.Exists(manifestPath))
                        {
                            File.Delete(manifestPath);
                            Debug.Log($"[Patch] 已删除manifest文件: {manifestPath}, 强制重新解压首包");
                        }

                        _machine.ChangeState<FsmUnpackFirstPackage>();
                    }
                    else
                    {
                        //下载更快
                        _machine.ChangeState<FsmDownloadPatch>();
                    }
                }
                else
                {
                    _machine.ChangeState<FsmDownloadPatch>();
                }
            }
            else
            {
                Debug.Log("[Patch] 本地文件完整,无需修复");

                _machine.ChangeState<FsmPatchComplete>();
            }
        }

        void IStateNode.OnUpdate()
        {
        }

        void IStateNode.OnExit()
        {
        }
    }

    internal class FsmDownloadManifest : IStateNode
    {
        private StateMachine _machine;

        void IStateNode.OnCreate(StateMachine machine)
        {
            _machine = machine;
        }

        void IStateNode.OnEnter()
        {
            var updater = (PatchUpdater)_machine.GetBlackboardValue("PatchUpdater");
            updater.SetStepName("下载资源清单");

            var patchManager = (PatchManager)_machine.GetBlackboardValue("PatchManager");
            EasyAsset.Instance.StartCoroutine(Download(patchManager, updater));
        }

        void IStateNode.OnUpdate()
        {
        }

        void IStateNode.OnExit()
        {
        }

        private IEnumerator Download(PatchManager manager, PatchUpdater updater)
        {
            yield return manager.DownloadManifest(manager.RemoteVersion);

            if (manager.RemoteManifest == null)
            {
                updater.SetError("下载清单失败");
                Debug.LogError("[Patch] 下载清单失败");
                yield break;
            }

            Debug.Log($"[Patch] 清单下载完成,Bundle数量: {manager.RemoteManifest.BundleList.Count}");
            _machine.ChangeState<FsmVerifyLocal>();
        }
    }

    internal class FsmVerifyLocal : IStateNode
    {
        private StateMachine _machine;

        void IStateNode.OnCreate(StateMachine machine)
        {
            _machine = machine;
        }

        void IStateNode.OnEnter()
        {
            var updater = (PatchUpdater)_machine.GetBlackboardValue("PatchUpdater");
            updater.SetStepName("校验本地文件");

            var patchManager = (PatchManager)_machine.GetBlackboardValue("PatchManager");

            patchManager.LoadLocalManifest();

            List<string> validBundles;
            validBundles = patchManager.VerifyLocalBundles();

            Debug.Log($"[Patch] 本地有效Bundle: {validBundles.Count}");

            _machine.SetBlackboardValue("ValidBundles", validBundles);
            _machine.ChangeState<FsmCompareDifference>();
        }

        void IStateNode.OnUpdate()
        {
        }

        void IStateNode.OnExit()
        {
        }
    }

    internal class FsmCompareDifference : IStateNode
    {
        private StateMachine _machine;

        void IStateNode.OnCreate(StateMachine machine)
        {
            _machine = machine;
        }

        void IStateNode.OnEnter()
        {
            var updater = (PatchUpdater)_machine.GetBlackboardValue("PatchUpdater");
            updater.SetStepName("对比差异");

            var patchManager = (PatchManager)_machine.GetBlackboardValue("PatchManager");
            var validBundles = (List<string>)_machine.GetBlackboardValue("ValidBundles");

            List<ManifestData.BundleInfo> patchList = patchManager.CompareDifference(validBundles);

            Debug.Log($"[Patch] 需要下载Bundle数: {patchList.Count}");

            bool isSmallPackageFirstRun = _machine.GetBlackboardValue("IsSmallPackageFirstRun") != null &&
                                          (bool)_machine.GetBlackboardValue("IsSmallPackageFirstRun");

            if (patchList.Count > 0)
            {
                if (isSmallPackageFirstRun)
                {
                    List<ManifestData.BundleInfo> buildinPatchList = new List<ManifestData.BundleInfo>();
                    foreach (var bundle in patchList)
                    {
                        if (bundle.Tags != null)
                        {
                            foreach (var tag in bundle.Tags)
                            {
                                if (tag.ToLower() == "buildin")
                                {
                                    buildinPatchList.Add(bundle);
                                    break;
                                }
                            }
                        }
                    }

                    Debug.Log($"[Patch] Small包首包资源: 需要下载Buildin Bundle数: {buildinPatchList.Count}");
                    _machine.SetBlackboardValue("BuildinPatchList", buildinPatchList);
                    _machine.SetBlackboardValue("FullPatchList", patchList);
                    _machine.ChangeState<FsmDownloadBuildinPatch>();
                }
                else
                {
                    _machine.SetBlackboardValue("PatchList", patchList);
                    _machine.ChangeState<FsmDownloadPatch>();
                }
            }
            else
            {
                _machine.ChangeState<FsmPatchComplete>();
            }
        }

        void IStateNode.OnUpdate()
        {
        }

        void IStateNode.OnExit()
        {
        }
    }

    internal class FsmDownloadPatch : IStateNode
    {
        private StateMachine _machine;

        void IStateNode.OnCreate(StateMachine machine)
        {
            _machine = machine;
        }

        void IStateNode.OnEnter()
        {
            var updater = (PatchUpdater)_machine.GetBlackboardValue("PatchUpdater");
            updater.SetStepName("下载补丁文件");

            var patchManager = (PatchManager)_machine.GetBlackboardValue("PatchManager");
            var downloadManager = (DownloadManager)_machine.GetBlackboardValue("DownloadManager");

            List<ManifestData.BundleInfo> downloadList = null;

            if (_machine.GetBlackboardValue("PatchList") != null)
            {
                downloadList = (List<ManifestData.BundleInfo>)_machine.GetBlackboardValue("PatchList");
            }
            else if (_machine.GetBlackboardValue("RepairList") != null)
            {
                downloadList = (List<ManifestData.BundleInfo>)_machine.GetBlackboardValue("RepairList");
            }

            EasyAsset.Instance.StartCoroutine(Download(patchManager, downloadManager, downloadList, updater));
        }

        void IStateNode.OnUpdate()
        {
        }

        void IStateNode.OnExit()
        {
        }

        private IEnumerator Download(PatchManager patchManager, DownloadManager downloadManager,
            List<ManifestData.BundleInfo> downloadList, PatchUpdater updater)
        {
            if (downloadList == null || downloadList.Count == 0)
            {
                Debug.Log("[Patch] 没有需要下载的文件");
                _machine.ChangeState<FsmPatchComplete>();
                yield break;
            }

            downloadManager.Clear();

            string remoteVersion = patchManager.RemoteVersion != null
                ? patchManager.RemoteVersion
                : patchManager.CurrentVersion;

            foreach (var bundle in downloadList)
            {
                string tempPath = Path.Combine(PathConfig.GetTempDownloadRoot(), bundle.BundleName);
                downloadManager.AddDownloadTask(bundle.BundleName, remoteVersion, tempPath, bundle.FileSize,
                    bundle.FileCRC);
            }

            bool hasFailed = false;
            downloadManager.OnDownloadFailed = (fileName, error) =>
            {
                hasFailed = true;
                updater.SetError($"下载失败: {fileName}");
            };

            yield return downloadManager.StartDownload();

            if (hasFailed)
            {
                Debug.LogError("[Patch] 补丁下载失败");
                yield break;
            }

            Debug.Log("[Patch] 补丁下载完成");
            _machine.ChangeState<FsmVerifyAndMove>();
        }
    }

    internal class FsmDownloadBuildinPatch : IStateNode
    {
        private StateMachine _machine;

        void IStateNode.OnCreate(StateMachine machine)
        {
            _machine = machine;
        }

        void IStateNode.OnEnter()
        {
            var updater = (PatchUpdater)_machine.GetBlackboardValue("PatchUpdater");
            updater.SetStepName("下载首包资源");

            var patchManager = (PatchManager)_machine.GetBlackboardValue("PatchManager");
            var downloadManager = (DownloadManager)_machine.GetBlackboardValue("DownloadManager");
            var buildinPatchList = (List<ManifestData.BundleInfo>)_machine.GetBlackboardValue("BuildinPatchList");

            EasyAsset.Instance.StartCoroutine(Download(patchManager, downloadManager, buildinPatchList, updater));
        }

        void IStateNode.OnUpdate()
        {
        }

        void IStateNode.OnExit()
        {
        }

        private IEnumerator Download(PatchManager patchManager, DownloadManager downloadManager,
            List<ManifestData.BundleInfo> downloadList, PatchUpdater updater)
        {
            if (downloadList == null || downloadList.Count == 0)
            {
                Debug.Log("[Patch] 没有需要下载的首包资源");
                _machine.ChangeState<FsmPatchComplete>();
                yield break;
            }

            downloadManager.Clear();

            string remoteVersion = patchManager.RemoteVersion;
            string firstPackTempRoot = PathConfig.GetFirstPackTempRoot();

            foreach (var bundle in downloadList)
            {
                string tempPath = Path.Combine(firstPackTempRoot, bundle.BundleName);
                downloadManager.AddDownloadTask(bundle.BundleName, remoteVersion, tempPath, bundle.FileSize,
                    bundle.FileCRC);
            }

            bool hasFailed = false;
            downloadManager.OnDownloadFailed = (fileName, error) =>
            {
                hasFailed = true;
                updater.SetError($"下载失败: {fileName}");
            };

            yield return downloadManager.StartDownload();

            if (hasFailed)
            {
                Debug.LogError("[Patch] 首包资源下载失败");
                yield break;
            }

            Debug.Log("[Patch] 首包资源下载完成");
            _machine.ChangeState<FsmVerifyAndMoveBuildin>();
        }
    }

    internal class FsmVerifyAndMove : IStateNode
    {
        private StateMachine _machine;

        void IStateNode.OnCreate(StateMachine machine)
        {
            _machine = machine;
        }

        void IStateNode.OnEnter()
        {
            var updater = (PatchUpdater)_machine.GetBlackboardValue("PatchUpdater");
            updater.SetStepName("校验并移动文件");

            var patchManager = (PatchManager)_machine.GetBlackboardValue("PatchManager");
            EasyAsset.Instance.StartCoroutine(VerifyAndMove(patchManager, updater));
        }

        void IStateNode.OnUpdate()
        {
        }

        void IStateNode.OnExit()
        {
        }

        private IEnumerator VerifyAndMove(PatchManager patchManager, PatchUpdater updater)
        {
            var config = PatchUpdater.GetCurrentConfig();
            string tempRoot = PathConfig.GetTempDownloadRoot();
            string cacheRoot = PathConfig.GetCacheRoot();

            if (!Directory.Exists(tempRoot))
            {
                Debug.Log("[Patch] 临时目录不存在,跳过校验和移动");
                _machine.ChangeState<FsmPatchComplete>();
                yield break;
            }

            string[] tempFiles = Directory.GetFiles(tempRoot, "*", SearchOption.AllDirectories);
            if (tempFiles.Length == 0)
            {
                Debug.Log("[Patch] 临时目录为空,跳过移动");
                _machine.ChangeState<FsmPatchComplete>();
                yield break;
            }

            List<string> filesToVerify = new List<string>();
            ManifestData manifest = patchManager.RemoteManifest != null
                ? patchManager.RemoteManifest
                : patchManager.LocalManifest;

            foreach (string tempFile in tempFiles)
            {
                string relativePath = tempFile.Substring(tempRoot.Length + 1);

                if (relativePath.EndsWith(".bytes") || relativePath.EndsWith(".hash") || relativePath.EndsWith(".json"))
                    continue;

                if (manifest.GetBundleInfo(relativePath) != null)
                {
                    filesToVerify.Add(tempFile);
                }
            }

            Debug.Log($"[Patch] 开始校验 {filesToVerify.Count} 个文件");

            Task<Dictionary<string, uint>> crcTask = CRCCalculator.CalculateMultipleCRCAsync(filesToVerify);

            while (!crcTask.IsCompleted)
            {
                yield return null;
            }

            if (crcTask.IsFaulted)
            {
                updater.SetError($"CRC计算失败: {crcTask.Exception?.GetBaseException().Message}");
                yield break;
            }

            Dictionary<string, uint> crcResults = crcTask.Result;

            bool allValid = true;
            foreach (string tempFile in filesToVerify)
            {
                string relativePath = tempFile.Substring(tempRoot.Length + 1);
                var bundleInfo = manifest.GetBundleInfo(relativePath);

                if (bundleInfo != null)
                {
                    if (!crcResults.TryGetValue(tempFile, out uint crc) || crc != bundleInfo.FileCRC)
                    {
                        Debug.LogError($"[Patch] 文件CRC校验失败: {relativePath}");
                        allValid = false;
                        break;
                    }
                }
            }

            if (!allValid)
            {
                updater.SetError("文件校验失败");
                yield break;
            }

            Debug.Log("[Patch] 所有文件校验通过,开始移动文件");

            int fileCount = 0;
            foreach (string tempFile in tempFiles)
            {
                string relativePath = tempFile.Substring(tempRoot.Length + 1);
                string destPath = Path.Combine(cacheRoot, relativePath);

                string destDir = Path.GetDirectoryName(destPath);
                if (!Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                File.Copy(tempFile, destPath, true);

                fileCount++;
                if (fileCount % config.FilesPerFrame == 0)
                {
                    for (int i = 0; i < config.FrameDelay; i++)
                    {
                        yield return null;
                    }

                    yield return null;
                }
            }

            PathConfig.ClearTempDirectory();

            Debug.Log("[Patch] 文件移动完成");
            _machine.ChangeState<FsmPatchComplete>();
        }
    }

    internal class FsmVerifyAndMoveBuildin : IStateNode
    {
        private StateMachine _machine;

        void IStateNode.OnCreate(StateMachine machine)
        {
            _machine = machine;
        }

        void IStateNode.OnEnter()
        {
            var updater = (PatchUpdater)_machine.GetBlackboardValue("PatchUpdater");
            updater.SetStepName("校验并移动首包资源");

            var patchManager = (PatchManager)_machine.GetBlackboardValue("PatchManager");
            EasyAsset.Instance.StartCoroutine(VerifyAndMove(patchManager, updater));
        }

        void IStateNode.OnUpdate()
        {
        }

        void IStateNode.OnExit()
        {
        }

        private IEnumerator VerifyAndMove(PatchManager patchManager, PatchUpdater updater)
        {
            var config = PatchUpdater.GetCurrentConfig();
            string tempRoot = PathConfig.GetFirstPackTempRoot();
            string cacheRoot = PathConfig.GetCacheRoot();

            if (!Directory.Exists(tempRoot))
            {
                Debug.Log("[Patch] 首包资源临时目录不存在");
                _machine.ChangeState<FsmPatchComplete>();
                yield break;
            }

            string[] tempFiles = Directory.GetFiles(tempRoot, "*", SearchOption.AllDirectories);
            if (tempFiles.Length == 0)
            {
                Debug.Log("[Patch] 首包资源临时目录为空");
                _machine.ChangeState<FsmPatchComplete>();
                yield break;
            }

            List<string> filesToVerify = new List<string>();
            ManifestData manifest = patchManager.RemoteManifest;

            foreach (string tempFile in tempFiles)
            {
                string relativePath = tempFile.Substring(tempRoot.Length + 1);
                if (manifest.GetBundleInfo(relativePath) != null)
                {
                    filesToVerify.Add(tempFile);
                }
            }

            Debug.Log($"[Patch] 开始校验首包资源 {filesToVerify.Count} 个文件");

            Task<Dictionary<string, uint>> crcTask = CRCCalculator.CalculateMultipleCRCAsync(filesToVerify);

            while (!crcTask.IsCompleted)
            {
                yield return null;
            }

            if (crcTask.IsFaulted)
            {
                updater.SetError($"首包资源CRC计算失败: {crcTask.Exception?.GetBaseException().Message}");
                yield break;
            }

            Dictionary<string, uint> crcResults = crcTask.Result;

            bool allValid = true;
            foreach (string tempFile in filesToVerify)
            {
                string relativePath = tempFile.Substring(tempRoot.Length + 1);
                var bundleInfo = manifest.GetBundleInfo(relativePath);

                if (bundleInfo != null)
                {
                    if (!crcResults.TryGetValue(tempFile, out uint crc) || crc != bundleInfo.FileCRC)
                    {
                        Debug.LogError($"[Patch] 首包资源CRC校验失败: {relativePath}");
                        allValid = false;
                        break;
                    }
                }
            }

            if (!allValid)
            {
                updater.SetError("首包资源校验失败");
                yield break;
            }

            Debug.Log("[Patch] 首包资源校验通过,开始移动文件");

            PathConfig.EnsureCacheDirectoryExists();

            int fileCount = 0;
            foreach (string tempFile in tempFiles)
            {
                string relativePath = tempFile.Substring(tempRoot.Length + 1);
                string destPath = Path.Combine(cacheRoot, relativePath);

                string destDir = Path.GetDirectoryName(destPath);
                if (!Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                File.Copy(tempFile, destPath, true);

                fileCount++;
                if (fileCount % config.FilesPerFrame == 0)
                {
                    for (int i = 0; i < config.FrameDelay; i++)
                    {
                        yield return null;
                    }

                    yield return null;
                }
            }

            PathConfig.ClearFirstPackTempDirectory();

            Debug.Log("[Patch] 首包资源移动完成");
            _machine.ChangeState<FsmPatchComplete>();
        }
    }

    internal class FsmStartBackgroundDownload : IStateNode
    {
        private StateMachine _machine;

        void IStateNode.OnCreate(StateMachine machine)
        {
            _machine = machine;
        }

        void IStateNode.OnEnter()
        {
            var updater = (PatchUpdater)_machine.GetBlackboardValue("PatchUpdater");
            updater.SetStepName("启动后台下载");

            var patchManager = (PatchManager)_machine.GetBlackboardValue("PatchManager");
            var backgroundDownloadManager =
                (BackgroundDownloadManager)_machine.GetBlackboardValue("BackgroundDownloadManager");

            EasyAsset.Instance.StartCoroutine(StartBackgroundDownloadCoroutine(patchManager, backgroundDownloadManager,
                updater));
        }

        private IEnumerator StartBackgroundDownloadCoroutine(PatchManager patchManager,
            BackgroundDownloadManager backgroundDownloadManager, PatchUpdater updater)
        {
            Debug.Log("[Patch] 首包资源准备完成，启动后台下载完整资源");

            yield return patchManager.DownloadManifest(patchManager.RemoteVersion);

            if (patchManager.RemoteManifest == null)
            {
                updater.SetError("下载清单失败");
                Debug.LogError("[Patch] 下载清单失败");
                yield break;
            }

            Debug.Log($"[Patch] 清单下载完成,Bundle数量: {patchManager.RemoteManifest.BundleList.Count}");

            EasyAsset.Instance.StartCoroutine(backgroundDownloadManager.StartBackgroundDownload(
                patchManager.RemoteVersion,
                patchManager.RemoteManifest,
                patchManager.LocalManifest
            ));
        }

        void IStateNode.OnUpdate()
        {
        }

        void IStateNode.OnExit()
        {
        }
    }

    internal class FsmPatchComplete : IStateNode
    {
        private StateMachine _machine;

        void IStateNode.OnCreate(StateMachine machine)
        {
            _machine = machine;
        }

        void IStateNode.OnEnter()
        {
            var updater = (PatchUpdater)_machine.GetBlackboardValue("PatchUpdater");
            updater.SetStepName("更新完成");

            var patchManager = (PatchManager)_machine.GetBlackboardValue("PatchManager");

            if (patchManager.RemoteManifest != null)
            {
                patchManager.SaveManifest();
                patchManager.SaveVersion(patchManager.RemoteVersion);
            }

            Debug.Log($"[Patch] 当前版本: {patchManager.CurrentVersion}");

            bool isOffline = (bool)_machine.GetBlackboardValue("IsOfflineMode");
            if (isOffline)
            {
                Debug.Log("[Patch] 离线模式，更新流程结束");
                _machine.Stop();
            }
            else
            {
                EasyAsset.Instance.StartCoroutine(CheckForNewUpdates(patchManager, updater));
            }
        }

        void IStateNode.OnUpdate()
        {
        }

        void IStateNode.OnExit()
        {
        }

        private IEnumerator CheckForNewUpdates(PatchManager patchManager, PatchUpdater updater)
        {
            int loopCount = (int)_machine.GetBlackboardValue("UpdateLoopCount");
            int maxLoops = (int)_machine.GetBlackboardValue("MaxUpdateLoops");

            if (loopCount >= maxLoops)
            {
                Debug.Log($"[Patch] 已达到最大更新循环次数 {maxLoops}, 停止检测");
                yield break;
            }

            loopCount++;
            _machine.SetBlackboardValue("UpdateLoopCount", loopCount);

            Debug.Log($"[Patch] 检测是否有新版本 (循环 {loopCount}/{maxLoops})");

            yield return patchManager.RequestPackageVersion();

            if (patchManager.RemoteVersion != null && patchManager.NeedUpdate())
            {
                Debug.Log($"[Patch] 发现新版本 {patchManager.RemoteVersion}, 重新开始更新流程");
                _machine.ChangeState<FsmDownloadManifest>();
            }
            else
            {
                Debug.Log("[Patch] 无新版本,更新流程结束");

                //判断是否进入后台下载
                bool isSmallPackageFirstRun = _machine.GetBlackboardValue("IsSmallPackageFirstRun") != null &&
                                              (bool)_machine.GetBlackboardValue("IsSmallPackageFirstRun");
                if (isSmallPackageFirstRun)
                {
                    //后台下载完整资源
                    _machine.ChangeState<FsmStartBackgroundDownload>();
                }
                else
                {
                    _machine.Stop();
                }
            }
        }
    }
}