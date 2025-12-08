// StreamingAssetsHelper.cs
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace EasyTools
{
    public static class StreamingAssetsHelper
    {
        private static Dictionary<string, string> _cachedTexts = new Dictionary<string, string>();
        private static Dictionary<string, byte[]> _cachedBytes = new Dictionary<string, byte[]>();

        /// <summary>
        /// 异步读取StreamingAssets中的文本文件
        /// </summary>
        public static IEnumerator ReadTextAsync(string fileName, System.Action<string> onComplete)
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

            if (Application.platform == RuntimePlatform.Android || 
                Application.platform == RuntimePlatform.IPhonePlayer)
            {
                using (UnityWebRequest request = UnityWebRequest.Get(filePath))
                {
                    yield return request.SendWebRequest();

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        string text = request.downloadHandler.text;
                        _cachedTexts[fileName] = text;
                        onComplete?.Invoke(text);
                    }
                    else
                    {
                        Debug.LogWarning($"读取StreamingAssets文件失败: {fileName}, Error: {request.error}");
                        onComplete?.Invoke(null);
                    }
                }
            }
            else
            {
                if (File.Exists(filePath))
                {
                    try
                    {
                        string text = File.ReadAllText(filePath);
                        _cachedTexts[fileName] = text;
                        onComplete?.Invoke(text);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"读取StreamingAssets文件失败: {fileName}, Error: {e.Message}");
                        onComplete?.Invoke(null);
                    }
                }
                else
                {
                    Debug.LogWarning($"StreamingAssets文件不存在: {fileName}");
                    onComplete?.Invoke(null);
                }

                yield break;
            }
        }

        /// <summary>
        /// 异步读取StreamingAssets中的二进制文件
        /// </summary>
        public static IEnumerator ReadBytesAsync(string fileName, System.Action<byte[]> onComplete)
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

            if (Application.platform == RuntimePlatform.Android || 
                Application.platform == RuntimePlatform.IPhonePlayer)
            {
                using (UnityWebRequest request = UnityWebRequest.Get(filePath))
                {
                    yield return request.SendWebRequest();

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        byte[] data = request.downloadHandler.data;
                        _cachedBytes[fileName] = data;
                        onComplete?.Invoke(data);
                    }
                    else
                    {
                        Debug.LogWarning($"读取StreamingAssets文件失败: {fileName}, Error: {request.error}");
                        onComplete?.Invoke(null);
                    }
                }
            }
            else
            {
                if (File.Exists(filePath))
                {
                    try
                    {
                        byte[] data = File.ReadAllBytes(filePath);
                        _cachedBytes[fileName] = data;
                        onComplete?.Invoke(data);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"读取StreamingAssets文件失败: {fileName}, Error: {e.Message}");
                        onComplete?.Invoke(null);
                    }
                }
                else
                {
                    Debug.LogWarning($"StreamingAssets文件不存在: {fileName}");
                    onComplete?.Invoke(null);
                }

                yield break;
            }
        }

        /// <summary>
        /// 同步读取StreamingAssets中的文本文件（返回缓存值或PC直接读取）
        /// </summary>
        public static string ReadText(string fileName)
        {
            if (_cachedTexts.ContainsKey(fileName))
            {
                return _cachedTexts[fileName];
            }

            if (Application.platform != RuntimePlatform.Android && 
                Application.platform != RuntimePlatform.IPhonePlayer)
            {
                string filePath = Path.Combine(Application.streamingAssetsPath, fileName);
                
                if (File.Exists(filePath))
                {
                    try
                    {
                        string text = File.ReadAllText(filePath);
                        _cachedTexts[fileName] = text;
                        return text;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"读取StreamingAssets文件失败: {fileName}, Error: {e.Message}");
                        return null;
                    }
                }
                
                Debug.LogWarning($"StreamingAssets文件不存在: {fileName}");
                return null;
            }

            Debug.LogWarning($"移动平台需要先调用ReadTextAsync预加载: {fileName}");
            return null;
        }

        /// <summary>
        /// 同步读取StreamingAssets中的二进制文件（返回缓存值或PC直接读取）
        /// </summary>
        public static byte[] ReadBytes(string fileName)
        {
            if (_cachedBytes.ContainsKey(fileName))
            {
                return _cachedBytes[fileName];
            }

            if (Application.platform != RuntimePlatform.Android && 
                Application.platform != RuntimePlatform.IPhonePlayer)
            {
                string filePath = Path.Combine(Application.streamingAssetsPath, fileName);
                
                if (File.Exists(filePath))
                {
                    try
                    {
                        byte[] data = File.ReadAllBytes(filePath);
                        _cachedBytes[fileName] = data;
                        return data;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"读取StreamingAssets文件失败: {fileName}, Error: {e.Message}");
                        return null;
                    }
                }
                
                Debug.LogWarning($"StreamingAssets文件不存在: {fileName}");
                return null;
            }

            Debug.LogWarning($"移动平台需要先调用ReadBytesAsync预加载: {fileName}");
            return null;
        }

        /// <summary>
        /// 清除缓存
        /// </summary>
        public static void ClearCache()
        {
            _cachedTexts.Clear();
            _cachedBytes.Clear();
        }
    }
}