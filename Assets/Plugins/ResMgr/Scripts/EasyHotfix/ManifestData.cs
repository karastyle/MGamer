// ManifestData.cs
using System;
using System.Collections.Generic;

namespace EasyTools
{
    [Serializable]
    public class ManifestData
    {
        public string FileVersion;
        public bool EnableAddressable;
        public bool SupportExtensionless;
        public bool LocationToLower;
        public bool IncludeAssetGUID;
        public bool ReplaceAssetPathWithAddress;
        public int OutputNameStyle;
        public int BuildBundleType;
        public string BuildPipeline;
        public string PackageName;
        public string PackageVersion;
        public string PackageNote;
        public List<AssetInfo> AssetList = new List<AssetInfo>();
        public List<BundleInfo> BundleList = new List<BundleInfo>();

        [Serializable]
        public class AssetInfo
        {
            public string Address;
            public string AssetPath;
            public string AssetGUID;
            public List<string> AssetTags = new List<string>();
            public int BundleID;
            public List<int> DependBundleIDs = new List<int>();
        }

        [Serializable]
        public class BundleInfo
        {
            public string BundleName;
            public uint UnityCRC;
            public string FileHash;
            public uint FileCRC;
            public ulong FileSize;
            public bool Encrypted;
            public List<string> Tags = new List<string>();
            public List<int> DependBundleIDs = new List<int>();
        }

        public AssetInfo GetAssetInfo(string address)
        {
            return AssetList.Find(a => a.Address == address);
        }

        public BundleInfo GetBundleInfo(int bundleID)
        {
            return bundleID >= 0 && bundleID < BundleList.Count ? BundleList[bundleID] : null;
        }

        public BundleInfo GetBundleInfo(string bundleName)
        {
            return BundleList.Find(b => b.BundleName == bundleName);
        }
    }
}