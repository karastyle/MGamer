using System;
using System.Collections.Generic;

[Serializable]
public class PackageManifest
{
    public string FileVersion = "2025.9.30";
    public bool EnableAddressable = true;
    public bool SupportExtensionless = true;
    public bool LocationToLower = false;
    public bool IncludeAssetGUID = false;
    public bool ReplaceAssetPathWithAddress = false;
    public int OutputNameStyle = 1;
    public int BuildBundleType = 2;
    public string BuildPipeline = "ScriptableBuildPipeline";
    public string PackageName = "DefaultPackage";
    public string PackageVersion = "";
    public string PackageNote = "";
    public List<AssetInfo> AssetList = new List<AssetInfo>();
    public List<BundleInfo> BundleList = new List<BundleInfo>();

    [Serializable]
    public class AssetInfo
    {
        public string Address;
        public string AssetPath;
        public string AssetGUID = "";
        public string[] AssetTags = new string[0];
        public int BundleID;
        public int[] DependBundleIDs = new int[0];
    }

    [Serializable]
    public class BundleInfo
    {
        public string BundleName;
        public uint UnityCRC;
        public string FileHash;
        public uint FileCRC;
        public long FileSize;
        public bool Encrypted = false;
        public string[] Tags = new string[0];
        public int[] DependBundleIDs = new int[0];
    }
}