// AppConfigLoader.cs
using UnityEngine;

namespace EasyTools
{
    public static class AppConfigLoader
    {
        /// <summary>
        /// 加载AppConfig（从StreamingAssetsHelper缓存读取）
        /// </summary>
        public static AppConfig LoadAppConfig()
        {
            string json = StreamingAssetsHelper.ReadText("AppConfig.json");
            
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    AppConfig config = JsonUtility.FromJson<AppConfig>(json);
                    return config;
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to parse AppConfig.json: {e.Message}");
                    return new AppConfig { PackageType = "Empty" };
                }
            }
            
            Debug.LogWarning("AppConfig.json not found, using default Empty package type");
            return new AppConfig { PackageType = "Empty" };
        }
    }
}