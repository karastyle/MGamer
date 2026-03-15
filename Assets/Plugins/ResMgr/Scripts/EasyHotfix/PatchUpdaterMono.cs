// PatchUpdaterMono.cs
using System;
using System.Collections;
using UnityEngine;

namespace EasyTools
{
    public class PatchUpdaterMono : MonoBehaviour
    {
        [Header("本地服务器配置")]
        public string hostLocalServer = "http://127.0.0.1:8080/CDN";

        [Header("局域网服务器配置")]
        public string hostNetServer = "http://192.168.1.15:8080/CDN";

        private PatchUpdater _updater;
        private bool _isUpdating = false;

        public string CurrentStepName { get; private set; } = "未开始";
        public int DownloadedCount { get; private set; } = 0;
        public int TotalCount { get; private set; } = 0;
        public ulong DownloadedBytes { get; private set; } = 0;
        public ulong TotalBytes { get; private set; } = 0;
        public float DownloadSpeed { get; private set; } = 0f;
        public float Progress { get; private set; } = 0f;
        public bool IsUpdating => _isUpdating;
        public bool UpdateComplete { get; private set; } = false;
        public bool UpdateFailed { get; private set; } = false;
        public string ErrorMessage { get; private set; } = "";

        private Action _onUpdateComplete;
        private Action<string> _onUpdateFailed;

        public void RegisterOnUpdateComplete(Action callback) => _onUpdateComplete += callback;
        public void RegisterOnUpdateFailed(Action<string> callback) => _onUpdateFailed += callback;
        public void UnregisterOnUpdateComplete(Action callback) => _onUpdateComplete -= callback;
        public void UnregisterOnUpdateFailed(Action<string> callback) => _onUpdateFailed -= callback;

        private float _lastUpdateTime;
        private ulong _lastDownloadedBytes;

        [HideInInspector] public bool isOfflineMode = false;

        public void StartUpdate()
        {
            if (_isUpdating)
            {
                Debug.LogWarning("更新已在进行中");
                return;
            }

            //全速下载
            PatchUpdater.SetDownloadSpeedMode(DownloadSpeedMode.FullSpeed);
            PatchUpdater.SetPerformanceMode(PerformanceMode.FullSpeed);
            
            StartCoroutine(UpdateCoroutine());
        }

        public void ResetUpdateComplete()
        {
            UpdateComplete = false;
            UpdateFailed = false;
            ErrorMessage = "";
        }
        
        private IEnumerator UpdateCoroutine()
        {
            _isUpdating = true;
            UpdateComplete = false;
            UpdateFailed = false;
            ErrorMessage = "";

            //移动平台用网络服务器，其他平台用本地服务器。 测试专用。 后续应该是部署到真正的线上服务器。
            var bUseNetServer = Application.platform == RuntimePlatform.Android ||
                                Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.WindowsPlayer;
            var hostServer = bUseNetServer ? hostNetServer : hostLocalServer;
            
            Debug.Log($"Use hostServer : {hostServer}");
            
            _updater = new PatchUpdater(hostServer, isOfflineMode);
            
            yield return _updater.StartUpdate();

            _isUpdating = false;
            
            if (_updater.HasError)
            {
                UpdateFailed = true;
                ErrorMessage = _updater.ErrorMessage;
                CurrentStepName = "更新失败";
                _onUpdateFailed?.Invoke(ErrorMessage);
            }
            else
            {
                UpdateComplete = true;
                CurrentStepName = "更新完成";
                _onUpdateComplete?.Invoke();
            }
        }

        void Update()
        {
            if (!_isUpdating || _updater == null) return;

            CurrentStepName = _updater.CurrentStepName;
            DownloadedCount = _updater.DownloadedCount;
            TotalCount = _updater.TotalCount;
            DownloadedBytes = _updater.DownloadedBytes;
            TotalBytes = _updater.TotalBytes;
            Progress = _updater.Progress;

            if (Time.time - _lastUpdateTime >= 0.5f)
            {
                ulong deltaBytes = DownloadedBytes - _lastDownloadedBytes;
                float deltaTime = Time.time - _lastUpdateTime;
                DownloadSpeed = deltaBytes / deltaTime;

                _lastDownloadedBytes = DownloadedBytes;
                _lastUpdateTime = Time.time;
            }
        }

        public string GetProgressText()
        {
            if (!_isUpdating)
            {
                if (UpdateComplete) return "更新完成";
                if (UpdateFailed) return $"更新失败: {ErrorMessage}";
                return "未开始";
            }

            if (TotalCount > 0)
            {
                float speedMB = DownloadSpeed / 1024f / 1024f;
                float downloadedMB = DownloadedBytes / 1024f / 1024f;
                float totalMB = TotalBytes / 1024f / 1024f;
                
                return $"{CurrentStepName}\n{DownloadedCount}/{TotalCount} 文件 {downloadedMB:F2}/{totalMB:F2}MB \n速度: {speedMB:F2}MB/s";
            }

            return CurrentStepName;
        }
    }
}