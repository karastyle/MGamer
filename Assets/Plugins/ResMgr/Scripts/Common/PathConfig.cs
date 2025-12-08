// PathConfig.cs
using System.IO;
using UnityEngine;

namespace EasyTools
{
    public static class PathConfig
    {
        private const string DefaultYooFolderName = "BundleCache";
        private const string TempFolderName = "DownloadTemp";
        private const string FirstPackTempFolderName = "FirstPackTemp";

        public static string GetCacheRoot()
        {
#if UNITY_EDITOR
            return GetEditorCacheRoot();
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
            return GetPCCacheRoot();
#elif UNITY_STANDALONE_OSX
            return GetMacCacheRoot();
#else
            return GetMobileCacheRoot();
#endif
        }

        public static string GetTempDownloadRoot()
        {
            string parentPath = Path.GetDirectoryName(GetCacheRoot());
            return Path.Combine(parentPath, TempFolderName);
        }

        public static string GetFirstPackTempRoot()
        {
            string parentPath = Path.GetDirectoryName(GetCacheRoot());
            return Path.Combine(parentPath, FirstPackTempFolderName);
        }

        private static string GetEditorCacheRoot()
        {
            string projectPath = Path.GetDirectoryName(Application.dataPath);
            return Path.Combine(projectPath, DefaultYooFolderName);
        }

        private static string GetPCCacheRoot()
        {
            return Path.Combine(Application.dataPath, DefaultYooFolderName);
        }

        private static string GetMacCacheRoot()
        {
            return Path.Combine(Application.persistentDataPath, DefaultYooFolderName);
        }

        private static string GetMobileCacheRoot()
        {
            return Path.Combine(Application.persistentDataPath, DefaultYooFolderName);
        }

        public static string GetFilePath(string fileName)
        {
            string cachePath = Path.Combine(GetCacheRoot(), fileName);
            if (File.Exists(cachePath))
            {
                return cachePath;
            }

            return null;
        }

        public static bool FileExists(string fileName)
        {
            string cachePath = Path.Combine(GetCacheRoot(), fileName);
            if (File.Exists(cachePath))
            {
                return true;
            }

            return false;
        }

        public static void EnsureCacheDirectoryExists()
        {
            string cacheRoot = GetCacheRoot();
            if (!Directory.Exists(cacheRoot))
            {
                Directory.CreateDirectory(cacheRoot);
            }
        }

        public static void EnsureTempDirectoryExists()
        {
            string tempRoot = GetTempDownloadRoot();
            if (!Directory.Exists(tempRoot))
            {
                Directory.CreateDirectory(tempRoot);
            }
        }

        public static void ClearTempDirectory()
        {
            string tempRoot = GetTempDownloadRoot();
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, true);
            }
        }

        public static void EnsureFirstPackTempDirectoryExists()
        {
            string tempRoot = GetFirstPackTempRoot();
            if (!Directory.Exists(tempRoot))
            {
                Directory.CreateDirectory(tempRoot);
            }
        }

        public static void ClearFirstPackTempDirectory()
        {
            string tempRoot = GetFirstPackTempRoot();
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, true);
            }
        }
    }
}