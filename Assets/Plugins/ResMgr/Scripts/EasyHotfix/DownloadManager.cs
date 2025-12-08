// DownloadManager.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace EasyTools
{
    public class DownloadManager
    {
        private string _hostServer;
        private List<DownloadTask> _downloadQueue = new List<DownloadTask>();
        private int _retryCount = 3;
        private int _activeDownloads = 0;

        public int TotalCount { get; private set; }
        public int DownloadedCount { get; private set; }
        public ulong TotalBytes { get; private set; }
        public ulong DownloadedBytes { get; private set; }
        public float Progress => TotalBytes > 0 ? (float)DownloadedBytes / TotalBytes : 0f;

        public Action<string, string> OnDownloadSuccess;
        public Action<string, string> OnDownloadFailed;
        public Action<int, int, ulong, ulong> OnProgressUpdate;

        public class DownloadTask
        {
            public string RemoteVersion;
            public string FileName;
            public string SavePath;
            public ulong FileSize;
            public uint FileCRC;
            public int RetryTimes;
        }

        public void Initialize(string hostServer, int retryCount = 3)
        {
            _hostServer = hostServer;
            _retryCount = retryCount;
        }

        public void AddDownloadTask(string fileName, string remoteVersion, string savePath, ulong fileSize, uint fileCRC)
        {
            var task = new DownloadTask
            {
                RemoteVersion = remoteVersion,
                FileName = fileName,
                SavePath = savePath,
                FileSize = fileSize,
                FileCRC = fileCRC,
                RetryTimes = 0
            };
            _downloadQueue.Add(task);
            TotalCount++;
            TotalBytes += fileSize;
        }

        public IEnumerator StartDownload()
        {
            if (_downloadQueue.Count == 0)
            {
                Debug.Log("没有需要下载的文件");
                yield break;
            }

            PathConfig.EnsureTempDirectoryExists();

            var speedConfig = PatchUpdater.GetCurrentSpeedConfig();
            int maxConcurrent = speedConfig.MaxConcurrent;

            Debug.Log($"开始下载 {TotalCount} 个文件, 总大小: {TotalBytes / 1024f / 1024f:F2}MB, 并发数: {maxConcurrent}");

            List<Coroutine> coroutines = new List<Coroutine>();

            for (int i = 0; i < Mathf.Min(maxConcurrent, _downloadQueue.Count); i++)
            {
                var coroutine = EasyAsset.Instance.StartCoroutine(DownloadNext());
                coroutines.Add(coroutine);
            }

            yield return new WaitUntil(() => _activeDownloads == 0 && _downloadQueue.Count == 0);
        }

        private IEnumerator DownloadNext()
        {
            while (_downloadQueue.Count > 0)
            {
                var task = _downloadQueue[0];
                _downloadQueue.RemoveAt(0);
                _activeDownloads++;

                yield return DownloadFile(task);

                _activeDownloads--;
            }
        }

        private IEnumerator DownloadFile(DownloadTask task)
        {
            string platform = PatchManager.GetPlatformName();
            string url = $"{_hostServer}/{platform}/{task.RemoteVersion}/{task.FileName}";

            string tempPath = task.SavePath;
            string directory = Path.GetDirectoryName(tempPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            long existingFileSize = 0;
            if (File.Exists(tempPath))
            {
                FileInfo fileInfo = new FileInfo(tempPath);
                existingFileSize = fileInfo.Length;
            }

            yield return CheckRemoteFileSize(url, task, existingFileSize, tempPath);
        }

        private IEnumerator CheckRemoteFileSize(string url, DownloadTask task, long existingFileSize, string tempPath)
        {
            using (UnityWebRequest headRequest = UnityWebRequest.Head(url))
            {
                yield return headRequest.SendWebRequest();

                if (headRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"HEAD请求失败: {task.FileName}, Error: {headRequest.error}");
                    RetryOrFail(task);
                    yield break;
                }

                string contentLength = headRequest.GetResponseHeader("Content-Length");
                if (string.IsNullOrEmpty(contentLength))
                {
                    Debug.LogError($"无法获取文件大小: {task.FileName}");
                    RetryOrFail(task);
                    yield break;
                }

                long remoteFileSize = long.Parse(contentLength);

                if (existingFileSize > 0 && existingFileSize == remoteFileSize)
                {
                    // 使用优化后的CRC计算
                    uint localCRC = CRCCalculator.CalculateFileCRC(tempPath);
                    if (localCRC == task.FileCRC)
                    {
                        DownloadedCount++;
                        UpdateProgress(task.FileSize);
                        OnDownloadSuccess?.Invoke(task.FileName, tempPath);
                        Debug.Log($"文件已存在且校验通过,跳过下载: {task.FileName} ({DownloadedCount}/{TotalCount})");
                        yield break;
                    }
                    else
                    {
                        Debug.LogWarning($"本地文件CRC校验失败,重新下载: {task.FileName}");
                        File.Delete(tempPath);
                        existingFileSize = 0;
                    }
                }
                else if (existingFileSize >= remoteFileSize)
                {
                    Debug.LogWarning($"本地文件大小异常,重新下载: {task.FileName}");
                    File.Delete(tempPath);
                    existingFileSize = 0;
                }
            }

            yield return DownloadWithResume(url, task, tempPath, existingFileSize);
        }

        private IEnumerator DownloadWithResume(string url, DownloadTask task, string tempPath, long existingFileSize)
        {
            var speedConfig = PatchUpdater.GetCurrentSpeedConfig();
            ulong speedLimit = speedConfig.SpeedLimitBytesPerSecond;

            UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
            var needAppend = existingFileSize > 0;
            DownloadHandlerFile downloadHandler = new DownloadHandlerFile(tempPath, needAppend);
            downloadHandler.removeFileOnAbort = false;

            if (needAppend)
            {
                request.SetRequestHeader("Range", $"bytes={existingFileSize}-");
            }

            request.downloadHandler = downloadHandler;

            var operation = request.SendWebRequest();
            ulong lastDownloaded = 0;
            float lastTime = Time.realtimeSinceStartup;

            while (!operation.isDone)
            {
                ulong currentDownloaded = request.downloadedBytes;
                ulong delta = currentDownloaded - lastDownloaded;
                
                if (delta > 0)
                {
                    UpdateProgress(delta);
                    lastDownloaded = currentDownloaded;
                }

                // 限速处理
                if (speedLimit > 0)
                {
                    float currentTime = Time.realtimeSinceStartup;
                    float elapsedTime = currentTime - lastTime;
                    
                    if (elapsedTime > 0)
                    {
                        float currentSpeed = currentDownloaded / elapsedTime;
                        
                        if (currentSpeed > speedLimit)
                        {
                            float delay = (currentDownloaded / (float)speedLimit) - elapsedTime;
                            if (delay > 0)
                            {
                                yield return new WaitForSeconds(delay);
                            }
                        }
                    }
                }

                yield return null;
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                ulong finalDelta = request.downloadedBytes - lastDownloaded;
                if (finalDelta > 0)
                {
                    UpdateProgress(finalDelta);
                }

                // 使用优化后的CRC计算
                uint localCRC = CRCCalculator.CalculateFileCRC(tempPath);
                if (localCRC == task.FileCRC)
                {
                    DownloadedCount++;
                    OnDownloadSuccess?.Invoke(task.FileName, tempPath);
                    Debug.Log($"下载成功: {task.FileName} ({DownloadedCount}/{TotalCount})");
                }
                else
                {
                    Debug.LogError($"CRC校验失败: {task.FileName}");
                    File.Delete(tempPath);
                    RetryOrFail(task);
                }
            }
            else
            {
                Debug.LogWarning($"下载失败: {task.FileName}, Error: {request.error}");
                RetryOrFail(task);
            }

            request.Dispose();
        }

        private void RetryOrFail(DownloadTask task)
        {
            task.RetryTimes++;
            if (task.RetryTimes < _retryCount)
            {
                Debug.LogWarning($"重试 {task.RetryTimes}/{_retryCount}: {task.FileName}");
                _downloadQueue.Add(task);
            }
            else
            {
                OnDownloadFailed?.Invoke(task.FileName, "下载失败");
                Debug.LogError($"下载失败: {task.FileName}");
            }
        }

        private void UpdateProgress(ulong bytes)
        {
            DownloadedBytes += bytes;
            OnProgressUpdate?.Invoke(DownloadedCount, TotalCount, DownloadedBytes, TotalBytes);
        }

        public void Clear()
        {
            _downloadQueue.Clear();
            TotalCount = 0;
            DownloadedCount = 0;
            TotalBytes = 0;
            DownloadedBytes = 0;
        }
    }
}