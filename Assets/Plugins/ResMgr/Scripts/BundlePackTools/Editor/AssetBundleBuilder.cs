// AssetBundleBuilder.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline.Tasks;
using UnityEngine;

public class AssetBundleBuilder
{
    public static string GetPlatformName(BuildTarget target)
    {
        switch (target)
        {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return "PC";
            case BuildTarget.Android:
                return "Android";
            case BuildTarget.iOS:
                return "iOS";
            default:
                return target.ToString();
        }
    }

    private readonly static HashSet<string> IgnoreFileExtensions = new HashSet<string>()
    {
        "", ".so", ".cs", ".js", ".boo", ".meta", ".cginc", ".hlsl"
    };

    public static void Build(AssetBundleConfig config, BuildTarget target)
    {
        if (config == null)
        {
            Debug.LogError("Config is null");
            return;
        }


        // 在打AB之前,处理Lua文件的拷贝和加密
        PreprocessLuaFiles();

        var builds = CollectAssetBundles(config, out var assetToBundle, out var bundleToAssets, out var mainAssets,
            out var assetToTag);
        if (builds.Count == 0)
        {
            Debug.LogError("No assets to build");
            return;
        }

        string platformName = GetPlatformName(target);
        string outputPath = Path.Combine(config.outputPath, platformName, config.version);

        if (Directory.Exists(outputPath))
        {
            Directory.Delete(outputPath, true);
            Debug.Log($"Deleted existing directory: {outputPath}");
        }

        Directory.CreateDirectory(outputPath);

        var scriptableParams = new ScriptableBuildParameters(target, outputPath, config.disableWriteTypeTree)
        {
            BundleCompression = UnityEngine.BuildCompression.LZ4,
            BuiltinShadersBundleName = "shaders.bundle"
        };

        var buildParams = scriptableParams.GetBundleBuildParameters();
        var buildContent = new BundleBuildContent(builds.ToArray());

        DateTime startTime = DateTime.Now;
        
        var taskList = CreateBuildTasks(scriptableParams.BuiltinShadersBundleName);
        
        BuildPlayerHelper.PrintBuildParameters(buildParams, buildContent, taskList);
        
        var result =
            ContentPipeline.BuildAssetBundles(buildParams, buildContent, out IBundleBuildResults results, taskList);

        DateTime endTime = DateTime.Now;

        if (result == ReturnCode.Success)
        {
            if (config.enableEncryption)
            {
                EncryptAssetBundles(outputPath, builds);
            }

            GenerateBuildReport(config, target, outputPath, builds, assetToBundle, bundleToAssets, startTime, endTime);
            GeneratePackageManifest(config, target, outputPath, builds, assetToBundle, bundleToAssets, mainAssets,
                results, assetToTag);
            Debug.Log($"Build success! Output: {outputPath}");
        }
        else
        {
            Debug.LogError($"Build failed: {result}");
        }
    }

static IList<IBuildTask> CreateBuildTasks(string builtinShaderBundleName = null)
{
    var buildTasks = new List<IBuildTask>();

    // Stage 1: Platform and Cache
    buildTasks.Add(new SwitchToBuildPlatform());
    buildTasks.Add(new RebuildSpriteAtlasCache());

    // Stage 2: Script Building
    buildTasks.Add(new BuildPlayerScripts());
    buildTasks.Add(new PostScriptsCallback());

    // Stage 3: Dependency Calculation (CRITICAL for scenes)
    buildTasks.Add(new CalculateSceneDependencyData());  // ⚠️ 必须有这个！
    buildTasks.Add(new CalculateCustomDependencyData());
    buildTasks.Add(new CalculateAssetDependencyData());
    buildTasks.Add(new StripUnusedSpriteSources());

    // Stage 4: Shader Bundle
    if (!string.IsNullOrEmpty(builtinShaderBundleName))
    {
        buildTasks.Add(new CreateBuiltInShadersBundle(builtinShaderBundleName));
    }

    buildTasks.Add(new PostDependencyCallback());

    // Stage 5: Bundle Packing
    buildTasks.Add(new GenerateBundlePacking());
    buildTasks.Add(new UpdateBundleObjectLayout());
    buildTasks.Add(new GenerateBundleCommands());
    buildTasks.Add(new GenerateSubAssetPathMaps());
    buildTasks.Add(new GenerateBundleMaps());
    buildTasks.Add(new PostPackingCallback());

    // Stage 6: Writing
    buildTasks.Add(new WriteSerializedFiles());
    buildTasks.Add(new ArchiveAndCompressBundles());

    // Stage 7: Post Processing
    buildTasks.Add(new AppendBundleHash());
    buildTasks.Add(new GenerateLinkXml());
    buildTasks.Add(new PostWritingCallback());

    return buildTasks;
}

    static List<AssetBundleBuild> CollectAssetBundles(AssetBundleConfig config,
        out Dictionary<string, string> assetToBundle,
        out Dictionary<string, List<string>> bundleToAssets,
        out HashSet<string> mainAssets,
        out Dictionary<string, string> assetToTag)
    {
        Dictionary<string, List<string>> bundleDict = new Dictionary<string, List<string>>();
        assetToBundle = new Dictionary<string, string>();
        bundleToAssets = new Dictionary<string, List<string>>();
        mainAssets = new HashSet<string>();
        assetToTag = new Dictionary<string, string>();

        //第一步：收集主资源
        foreach (var group in config.collectorGroups)
        {
            if (!group.active) continue;

            foreach (var collector in group.collectors)
            {
                if (collector.collectorPath == null) continue;

                string path = AssetDatabase.GetAssetPath(collector.collectorPath);
                if (string.IsNullOrEmpty(path)) continue;

                CollectAssets(path, collector, bundleDict, assetToBundle, mainAssets, assetToTag);
            }
        }

        //第二部：收集Shader资源 
        const string shaderBundleName = "shaders.bundle";
        CollectShaders(config, bundleDict, assetToBundle, shaderBundleName, mainAssets);

        Dictionary<string, HashSet<string>> dependencyToReferencers = new Dictionary<string, HashSet<string>>();
        int filteredBuiltinCount = 0;

        foreach (var kvp in assetToBundle)
        {
            string assetPath = kvp.Key;
            string bundleName = kvp.Value;

            string[] dependencies = AssetDatabase.GetDependencies(assetPath, true);

            foreach (string depPath in dependencies)
            {
                if (depPath == assetPath) continue;

                if (ShouldIgnoreAsset(depPath))
                {
                    filteredBuiltinCount++;
                    continue;
                }

                if (assetToBundle.ContainsKey(depPath)) continue;

                if (!dependencyToReferencers.ContainsKey(depPath))
                    dependencyToReferencers[depPath] = new HashSet<string>();

                dependencyToReferencers[depPath].Add(bundleName);
            }
        }


        Debug.Log($"Filtered {filteredBuiltinCount} builtin/ignored dependencies");

        List<string> sharedAssets = new List<string>();
        foreach (var kvp in dependencyToReferencers)
        {
            if (kvp.Value.Count > 1)
            {
                sharedAssets.Add(kvp.Key);
            }
        }

        Debug.Log($"Found {sharedAssets.Count} shared assets (referenced by multiple bundles)");

        Dictionary<string, List<string>> directoryToSharedAssets = new Dictionary<string, List<string>>();

        foreach (string sharedAsset in sharedAssets)
        {
            string directory = Path.GetDirectoryName(sharedAsset).Replace("\\", "/");

            if (!directoryToSharedAssets.ContainsKey(directory))
                directoryToSharedAssets[directory] = new List<string>();

            directoryToSharedAssets[directory].Add(sharedAsset);
        }

        int sharedBundleCount = 0;
        foreach (var kvp in directoryToSharedAssets)
        {
            string directory = kvp.Key;
            List<string> assets = kvp.Value;

            string shareBundleName = GetShareBundleName(directory);

            if (!bundleDict.ContainsKey(shareBundleName))
                bundleDict[shareBundleName] = new List<string>();

            foreach (string asset in assets)
            {
                bundleDict[shareBundleName].Add(asset);
                assetToBundle[asset] = shareBundleName;
            }

            sharedBundleCount++;
            Debug.Log($"Created shared bundle: {shareBundleName} with {assets.Count} assets");
        }

        Debug.Log($"Created {sharedBundleCount} shared bundles");

        List<AssetBundleBuild> builds = new List<AssetBundleBuild>();
        foreach (var kvp in bundleDict)
        {
            bundleToAssets[kvp.Key] = new List<string>(kvp.Value);
            builds.Add(new AssetBundleBuild
            {
                assetBundleName = kvp.Key,
                assetNames = kvp.Value.ToArray(),
                addressableNames = kvp.Value.Select(x => Path.GetFileNameWithoutExtension(x)).ToArray()
            });
        }

        Debug.Log("============= 开始对比 AssetBundleBuild 列表 =============");
        foreach (var build in builds)
        {
            // 只打印包含那个报错 Scene 的包，或者是所有包（如果不多的话）
            // 如果你知道报错的场景名，比如 "Login.unity"，可以加个过滤
            // if (!build.assetNames.Any(p => p.Contains("Login.unity"))) continue;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"[Bundle Name]: {build.assetBundleName}");
            sb.AppendLine($"[Asset Names Count]: {build.assetNames.Length}");

            foreach (var assetPath in build.assetNames)
            {
                sb.AppendLine($"   -> {assetPath}");
            }

            // 如果有 addressableNames 也顺便看一眼
            if (build.addressableNames != null && build.addressableNames.Length > 0)
            {
                sb.AppendLine($"   (Address): {build.addressableNames[0]}");
            }

            Debug.Log(sb.ToString());
        }

        Debug.Log("============= 列表结束 =============");

        return builds;
    }

    static void CollectAssets(string path, Collector collector, Dictionary<string, List<string>> bundleDict,
        Dictionary<string, string> assetToBundle, HashSet<string> mainAssets, Dictionary<string, string> assetToTag)
    {
        if (File.Exists(path))
        {
            if (ShouldCollect(path, collector.collectType))
            {
                string bundleName = GetBundleName(path, collector.packRule, path);
                AddToBundleDict(bundleDict, bundleName, path);
                assetToBundle[path] = bundleName;
                mainAssets.Add(path);

                if (collector.assetTag != AssetTag.None)
                {
                    assetToTag[path] = collector.assetTag.ToString();
                }
            }
        }
        else if (Directory.Exists(path))
        {
            string[] guids = AssetDatabase.FindAssets("", new[] { path });
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (Directory.Exists(assetPath)) continue;
                if (!ShouldCollect(assetPath, collector.collectType)) continue;

                string bundleName = GetBundleName(assetPath, collector.packRule, path);
                AddToBundleDict(bundleDict, bundleName, assetPath);
                assetToBundle[assetPath] = bundleName;
                mainAssets.Add(assetPath);

                if (collector.assetTag != AssetTag.None)
                {
                    assetToTag[assetPath] = collector.assetTag.ToString();
                }
            }
        }
    }

    static bool ShouldCollect(string assetPath, CollectType collectType)
    {
        if (assetPath.EndsWith(".cs") || assetPath.EndsWith(".meta")) return false;

        switch (collectType)
        {
            case CollectType.CollectAll:
                return true;
            case CollectType.CollectPrefab:
                return assetPath.EndsWith(".prefab");
            case CollectType.CollectSprite:
                return assetPath.EndsWith(".png") || assetPath.EndsWith(".jpg") || assetPath.EndsWith(".tga");
            case CollectType.CollectSpriteAtlas:
                return assetPath.EndsWith(".spriteatlas");
            case CollectType.CollectScene:
                return assetPath.EndsWith(".unity");
            default:
                return true;
        }
    }

    static string GetBundleName(string assetPath, PackRule packRule, string rootPath)
    {
        string pathWithoutExt = Path.ChangeExtension(assetPath, null).Replace("\\", "/");

        switch (packRule)
        {
            case PackRule.PackSeparately:
                return pathWithoutExt.Replace("Assets/", "").Replace("/", "_").ToLower() + ".bundle";

            case PackRule.PackDirectory:
                string dir = Path.GetDirectoryName(assetPath).Replace("\\", "/");
                string bundleName = dir.Replace("Assets/", "").Replace("/", "_").ToLower() + ".bundle";
                return bundleName;

            case PackRule.PackTopDirectory:
                string topDir = GetTopDirectory(assetPath);
                return topDir.Replace("Assets/", "").Replace("/", "_").ToLower() + ".bundle";

            default:
                return "default.bundle";
        }
    }

    static string GetTopDirectory(string assetPath)
    {
        string[] parts = assetPath.Split('/');
        if (parts.Length >= 2)
            return parts[0] + "/" + parts[1];
        return assetPath;
    }

    static string GetShareBundleName(string directory)
    {
        return "share_" + directory.Replace("Assets/", "").Replace("/", "_").ToLower() + ".bundle";
    }

    static void AddToBundleDict(Dictionary<string, List<string>> bundleDict, string bundleName, string assetPath)
    {
        if (!bundleDict.ContainsKey(bundleName))
            bundleDict[bundleName] = new List<string>();

        bundleDict[bundleName].Add(assetPath);
    }

    static bool ShouldIgnoreAsset(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath))
            return true;

        if (assetPath.StartsWith("Assets/") == false && assetPath.StartsWith("Packages/") == false)
            return true;

        if (AssetDatabase.IsValidFolder(assetPath))
            return true;

        if (assetPath.Contains("/Gizmos/"))
            return true;

        if (assetPath.Contains("/Editor/") || assetPath.Contains("/Editor Resources/"))
            return true;

        string extension = Path.GetExtension(assetPath).ToLower();

        if (IgnoreFileExtensions.Contains(extension))
            return true;

        if (extension == ".shader" || extension == ".shadervariants")
            return true;

        return false;
    }

    static bool IsBuiltinAsset(string assetPath)
    {
        return ShouldIgnoreAsset(assetPath);
    }

    static void CollectShaders(AssetBundleConfig config, Dictionary<string, List<string>> bundleDict,
        Dictionary<string, string> assetToBundle, string shaderBundleName, HashSet<string> mainAssets)
    {
        // 使用 HashSet 自动去重
        HashSet<string> shaderPaths = new HashSet<string>();

        // =========================================================
        // 来源 1: 从变体收集文件 (.shadervariants) 中提取 Shader
        // =========================================================
        if (config.shaderVariantPath != null)
        {
            string variantPath = AssetDatabase.GetAssetPath(config.shaderVariantPath);
            if (!string.IsNullOrEmpty(variantPath))
            {
                // 把变体文件本身加进去
                shaderPaths.Add(variantPath);
                mainAssets.Add(variantPath);

                // 【关键点】这里会解析变体文件，找出它依赖的 .shader 文件
                CollectShadersFromVariantCollection(variantPath, shaderPaths);
            }
        }

        // =========================================================
        // 来源 2: 扫描所有已收集资源的依赖 (兜底策略)     如果忘记了跑变体收集，这里根据材质当前设置进行收集，可能会不全。
        // =========================================================
        // 注意：必须先执行 CollectAssets 填充 assetToBundle，这里才能扫到东西
        foreach (var kvp in assetToBundle.ToList())
        {
            string[] dependencies = AssetDatabase.GetDependencies(kvp.Key, true);
            foreach (string dep in dependencies)
            {
                if (dep.EndsWith(".shader"))
                {
                    // 即使变体集里漏了，这里也能扫到（比如内置 Skybox）
                    shaderPaths.Add(dep);
                }
            }
        }

        // =========================================================
        // 最后: 统一打包到 shaderBundleName 中
        // =========================================================
        if (!bundleDict.ContainsKey(shaderBundleName))
            bundleDict[shaderBundleName] = new List<string>();

        foreach (string shaderPath in shaderPaths)
        {
            if (!bundleDict[shaderBundleName].Contains(shaderPath))
            {
                bundleDict[shaderBundleName].Add(shaderPath);
                // 建立映射关系，这样 SBP 就能找到 Shader 在哪个包了
                assetToBundle[shaderPath] = shaderBundleName;
            }
        }
    }

    static void CollectShadersFromVariantCollection(string variantPath, HashSet<string> shaderPaths)
    {
        ShaderVariantCollection collection = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(variantPath);
        if (collection == null)
        {
            Debug.LogWarning($"Failed to load ShaderVariantCollection: {variantPath}");
            return;
        }

        SerializedObject so = new SerializedObject(collection);
        SerializedProperty shaderArray = so.FindProperty("m_Shaders");

        int shaderCount = 0;

        if (shaderArray != null && shaderArray.isArray)
        {
            for (int i = 0; i < shaderArray.arraySize; i++)
            {
                SerializedProperty shaderEntry = shaderArray.GetArrayElementAtIndex(i);
                SerializedProperty shaderProp = shaderEntry.FindPropertyRelative("first");

                if (shaderProp != null && shaderProp.objectReferenceValue != null)
                {
                    Shader shader = shaderProp.objectReferenceValue as Shader;
                    if (shader != null)
                    {
                        string shaderPath = AssetDatabase.GetAssetPath(shader);

                        // 【核心修改】新增内置 Shader 过滤
                        // Unity 的内置 Shader 路径通常是 "Resources/unity_builtin_extra"
                        // 或者是 "Library/unity default resources"
                        if (IsBuiltinShaderPath(shaderPath))
                        {
                            // Debug.Log($"Skipping built-in shader: {shader.name}");
                            continue;
                        }

                        shaderPaths.Add(shaderPath);
                        shaderCount++;
                        Debug.Log($"Collected shader from variant: {shader.name} at {shaderPath}");
                    }
                }
            }

            Debug.Log($"Collected {shaderCount} shaders from variant collection: {Path.GetFileName(variantPath)}");
        }
    }

    // 辅助函数：判断是否为内置资源路径
    static bool IsBuiltinShaderPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return false;

        // 1. 最常见的内置 Shader 路径
        if (path == "Resources/unity_builtin_extra")
            return true;

        // 2. 另一种常见的内置资源路径
        if (path == "Library/unity default resources")
            return true;

        // 3. 有些旧版本可能是这种
        if (path.StartsWith("Resources/unity_builtin_extra"))
            return true;

        return false;
    }

    static void GenerateBuildReport(AssetBundleConfig config, BuildTarget target, string outputPath,
        List<AssetBundleBuild> builds, Dictionary<string, string> assetToBundle,
        Dictionary<string, List<string>> bundleToAssets, DateTime startTime, DateTime endTime)
    {
        BuildReportData report = new BuildReportData();

        report.Summary = new SummaryData
        {
            UnityVersion = Application.unityVersion,
            BuildDate = startTime.ToString("yyyy/MM/dd HH:mm:ss"),
            BuildSeconds = (int)(endTime - startTime).TotalSeconds,
            BuildTarget = (int)target,
            BuildPipeline = "ScriptableBuildPipeline",
            BuildPackageName = "DefaultPackage",
            BuildPackageVersion = config.version,
            EnableAddressable = true,
            AssetFileTotalCount = assetToBundle.Count,
            MainAssetTotalCount = assetToBundle.Count,
            AllBundleTotalCount = builds.Count,
            AllBundleTotalSize = 0
        };

        report.AssetInfos = new List<AssetInfoData>();
        report.BundleInfos = new List<BundleInfoData>();

        Dictionary<string, HashSet<string>> bundleDependencies = new Dictionary<string, HashSet<string>>();
        Dictionary<string, HashSet<string>> bundleReferences = new Dictionary<string, HashSet<string>>();

        foreach (var assetKvp in assetToBundle)
        {
            string assetPath = assetKvp.Key;
            string mainBundle = assetKvp.Value;

            string[] dependencies = AssetDatabase.GetDependencies(assetPath, true);
            List<DependAssetData> dependAssets = new List<DependAssetData>();
            HashSet<string> dependBundles = new HashSet<string>();

            foreach (string dep in dependencies)
            {
                if (dep == assetPath) continue;
                if (ShouldIgnoreAsset(dep)) continue;

                if (assetToBundle.ContainsKey(dep))
                {
                    string depBundle = assetToBundle[dep];
                    if (depBundle != mainBundle)
                    {
                        dependBundles.Add(depBundle);

                        if (!bundleReferences.ContainsKey(depBundle))
                            bundleReferences[depBundle] = new HashSet<string>();
                        bundleReferences[depBundle].Add(mainBundle);
                    }

                    dependAssets.Add(new DependAssetData
                    {
                        AssetPath = dep,
                        AssetGUID = AssetDatabase.AssetPathToGUID(dep)
                    });
                }
            }

            if (!bundleDependencies.ContainsKey(mainBundle))
                bundleDependencies[mainBundle] = new HashSet<string>();
            foreach (var db in dependBundles)
                bundleDependencies[mainBundle].Add(db);

            report.AssetInfos.Add(new AssetInfoData
            {
                Address = Path.GetFileNameWithoutExtension(assetPath),
                AssetPath = assetPath,
                AssetGUID = AssetDatabase.AssetPathToGUID(assetPath),
                AssetTags = new string[0],
                MainBundleName = mainBundle,
                MainBundleSize = 0,
                DependAssets = dependAssets,
                DependBundles = dependBundles.ToList()
            });
        }

        long totalSize = 0;
        foreach (var build in builds)
        {
            string bundlePath = Path.Combine(outputPath, build.assetBundleName);
            long fileSize = File.Exists(bundlePath) ? new FileInfo(bundlePath).Length : 0;
            totalSize += fileSize;

            foreach (var assetInfo in report.AssetInfos)
            {
                if (assetInfo.MainBundleName == build.assetBundleName)
                {
                    assetInfo.MainBundleSize = fileSize;
                }
            }

            List<BundleContentData> contents = new List<BundleContentData>();
            if (bundleToAssets.ContainsKey(build.assetBundleName))
            {
                foreach (string asset in bundleToAssets[build.assetBundleName])
                {
                    contents.Add(new BundleContentData
                    {
                        AssetPath = asset,
                        AssetGUID = AssetDatabase.AssetPathToGUID(asset)
                    });
                }
            }

            report.BundleInfos.Add(new BundleInfoData
            {
                BundleName = build.assetBundleName,
                FileName = build.assetBundleName,
                FileHash = "",
                FileCRC = 0,
                FileSize = fileSize,
                Encrypted = false,
                Tags = new string[0],
                DependBundles = bundleDependencies.ContainsKey(build.assetBundleName)
                    ? bundleDependencies[build.assetBundleName].ToList()
                    : new List<string>(),
                ReferenceBundles = bundleReferences.ContainsKey(build.assetBundleName)
                    ? bundleReferences[build.assetBundleName].ToList()
                    : new List<string>(),
                BundleContents = contents
            });
        }

        report.Summary.AllBundleTotalSize = totalSize;
        report.IndependAssets = new object[0];

        string reportFileName = $"BuildReport.report";
        string reportPath = Path.Combine(outputPath, reportFileName);
        string json = JsonUtility.ToJson(report, true);
        File.WriteAllText(reportPath, json);

        Debug.Log($"Build report saved to: {reportPath}");
        Debug.Log($"Total bundles: {builds.Count}, Total size: {FormatFileSize(totalSize)}");
    }

    static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }

    static void GeneratePackageManifest(AssetBundleConfig config, BuildTarget target, string outputPath,
        List<AssetBundleBuild> builds, Dictionary<string, string> assetToBundle,
        Dictionary<string, List<string>> bundleToAssets,
        HashSet<string> mainAssets,
        IBundleBuildResults buildResults,
        Dictionary<string, string> assetToTag)
    {
        string packageName = "DefaultPackage";

        PackageManifest manifest = new PackageManifest();
        manifest.PackageName = packageName;
        manifest.PackageVersion = $"{System.DateTime.Now:yyyy-MM-dd-fff}";
        manifest.PackageNote = System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

        Dictionary<string, int> bundleNameToID = new Dictionary<string, int>();
        for (int i = 0; i < builds.Count; i++)
        {
            bundleNameToID[builds[i].assetBundleName] = i;
        }

        const string shaderBundleName = "shaders.bundle";
        int shaderBundleID = bundleNameToID.ContainsKey(shaderBundleName) ? bundleNameToID[shaderBundleName] : -1;

        Dictionary<string, HashSet<int>> bundleDirectDeps = new Dictionary<string, HashSet<int>>();

        foreach (var build in builds)
        {
            if (!bundleDirectDeps.ContainsKey(build.assetBundleName))
                bundleDirectDeps[build.assetBundleName] = new HashSet<int>();

            if (bundleToAssets.ContainsKey(build.assetBundleName))
            {
                bool hasMaterial = false;

                foreach (string assetPath in bundleToAssets[build.assetBundleName])
                {
                    if (assetPath.EndsWith(".mat"))
                    {
                        hasMaterial = true;
                    }

                    string[] dependencies = AssetDatabase.GetDependencies(assetPath, true);
                    foreach (string dep in dependencies)
                    {
                        if (dep == assetPath) continue;
                        if (ShouldIgnoreAsset(dep)) continue;

                        if (assetToBundle.TryGetValue(dep, out string depBundleName))
                        {
                            if (depBundleName != build.assetBundleName && bundleNameToID.ContainsKey(depBundleName))
                            {
                                bundleDirectDeps[build.assetBundleName].Add(bundleNameToID[depBundleName]);
                            }
                        }
                    }
                }

                if (hasMaterial && shaderBundleID >= 0 && build.assetBundleName != shaderBundleName)
                {
                    bundleDirectDeps[build.assetBundleName].Add(shaderBundleID);
                }
            }
        }

        Dictionary<string, HashSet<int>> bundleAllDeps = new Dictionary<string, HashSet<int>>();
        foreach (var build in builds)
        {
            bundleAllDeps[build.assetBundleName] = new HashSet<int>();
            CollectAllDependencies(build.assetBundleName, bundleDirectDeps, bundleAllDeps, builds);
        }

        // 收集每个bundle的直接tags（从bundle包含的主资源）
        Dictionary<string, HashSet<string>> bundleDirectTags = new Dictionary<string, HashSet<string>>();
        foreach (var build in builds)
        {
            bundleDirectTags[build.assetBundleName] = new HashSet<string>();

            if (bundleToAssets.ContainsKey(build.assetBundleName))
            {
                foreach (string assetPath in bundleToAssets[build.assetBundleName])
                {
                    if (mainAssets.Contains(assetPath) && assetToTag.ContainsKey(assetPath))
                    {
                        bundleDirectTags[build.assetBundleName].Add(assetToTag[assetPath]);
                    }
                }
            }
        }

        // 将bundle的tags传播到它依赖的所有bundles
        Dictionary<string, HashSet<string>> bundleAllTags = new Dictionary<string, HashSet<string>>();
        foreach (var build in builds)
        {
            bundleAllTags[build.assetBundleName] = new HashSet<string>();
        }

        // 先复制直接tags
        foreach (var build in builds)
        {
            if (bundleDirectTags.ContainsKey(build.assetBundleName))
            {
                foreach (var tag in bundleDirectTags[build.assetBundleName])
                {
                    bundleAllTags[build.assetBundleName].Add(tag);
                }
            }
        }

        // 传播tags到依赖的bundles
        foreach (var build in builds)
        {
            if (bundleAllTags[build.assetBundleName].Count > 0)
            {
                PropagateTags(build.assetBundleName, bundleAllDeps, bundleAllTags, builds);
            }
        }

        foreach (var assetPath in mainAssets)
        {
            if (!assetToBundle.ContainsKey(assetPath))
                continue;

            string bundleName = assetToBundle[assetPath];

            if (!bundleNameToID.ContainsKey(bundleName))
                continue;

            int bundleID = bundleNameToID[bundleName];

            int[] dependBundleIDs = bundleAllDeps.ContainsKey(bundleName)
                ? bundleAllDeps[bundleName].ToArray()
                : new int[0];

            string[] assetTags = new string[0];
            if (assetToTag.ContainsKey(assetPath))
            {
                assetTags = new string[] { assetToTag[assetPath] };
            }

            PackageManifest.AssetInfo assetInfo = new PackageManifest.AssetInfo
            {
                Address = Path.GetFileNameWithoutExtension(assetPath),
                AssetPath = assetPath,
                AssetGUID = AssetDatabase.AssetPathToGUID(assetPath),
                AssetTags = assetTags,
                BundleID = bundleID,
                DependBundleIDs = dependBundleIDs
            };

            manifest.AssetList.Add(assetInfo);
        }

        for (int i = 0; i < builds.Count; i++)
        {
            var build = builds[i];
            string bundlePath = Path.Combine(outputPath, build.assetBundleName);

            if (!File.Exists(bundlePath))
            {
                Debug.LogWarning($"Bundle file not found: {bundlePath}");
                continue;
            }

            FileInfo fileInfo = new FileInfo(bundlePath);
            uint unityCRC = 0;

            if (buildResults.BundleInfos.TryGetValue(build.assetBundleName, out var bundleDetails))
            {
                unityCRC = bundleDetails.Crc;
            }
            else
            {
                Debug.LogError($"Failed to get CRC for bundle: {build.assetBundleName}");
            }

            string fileHash = CalculateMD5(bundlePath);
            uint fileCRC = CalculateCRC32(bundlePath);

            int[] dependBundleIDs = bundleAllDeps.ContainsKey(build.assetBundleName)
                ? bundleAllDeps[build.assetBundleName].ToArray()
                : new int[0];

            string[] bundleTags = new string[0];
            if (bundleAllTags.ContainsKey(build.assetBundleName) && bundleAllTags[build.assetBundleName].Count > 0)
            {
                bundleTags = bundleAllTags[build.assetBundleName].ToArray();
            }

            // ⭐ 给 shader bundle 添加 builtin 标签
            if (build.assetBundleName == shaderBundleName)
            {
                List<string> tagList = new List<string>(bundleTags);
                if (!tagList.Contains("Buildin"))
                {
                    tagList.Add("Buildin");
                }
                bundleTags = tagList.ToArray();
            }
            
            PackageManifest.BundleInfo bundleInfo = new PackageManifest.BundleInfo
            {
                BundleName = build.assetBundleName,
                UnityCRC = unityCRC,
                FileHash = fileHash,
                FileCRC = fileCRC,
                FileSize = fileInfo.Length,
                Encrypted = config.enableEncryption,
                Tags = bundleTags,
                DependBundleIDs = dependBundleIDs
            };

            manifest.BundleList.Add(bundleInfo);
        }

        string manifestName = $"{packageName}_Manifest";

        string jsonPath = Path.Combine(outputPath, $"{manifestName}.json");
        string json = JsonUtility.ToJson(manifest, true);
        File.WriteAllText(jsonPath, json);
        Debug.Log($"Generated manifest JSON: {jsonPath}");

        string bytesPath = Path.Combine(outputPath, $"{manifestName}.bytes");
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
        File.WriteAllBytes(bytesPath, bytes);

        string hash = CalculateMD5FromBytes(bytes);
        string hashPath = Path.Combine(outputPath, $"{manifestName}.hash");
        File.WriteAllText(hashPath, hash);

        string versionPath = Path.Combine(outputPath, $"{manifestName}.version");
        File.WriteAllText(versionPath, config.version);
    }

    static void CollectAllDependencies(string bundleName,
        Dictionary<string, HashSet<int>> bundleDirectDeps,
        Dictionary<string, HashSet<int>> bundleAllDeps,
        List<AssetBundleBuild> builds)
    {
        if (!bundleDirectDeps.ContainsKey(bundleName))
            return;

        foreach (int depID in bundleDirectDeps[bundleName])
        {
            if (bundleAllDeps[bundleName].Contains(depID))
                continue;

            bundleAllDeps[bundleName].Add(depID);

            string depBundleName = builds[depID].assetBundleName;

            // 确保依赖bundle的entry存在
            if (!bundleAllDeps.ContainsKey(depBundleName))
                bundleAllDeps[depBundleName] = new HashSet<int>();

            CollectAllDependencies(depBundleName, bundleDirectDeps, bundleAllDeps, builds);

            if (bundleAllDeps.ContainsKey(depBundleName))
            {
                foreach (int transitiveDep in bundleAllDeps[depBundleName])
                {
                    bundleAllDeps[bundleName].Add(transitiveDep);
                }
            }
        }
    }

    static void PropagateTags(string bundleName,
        Dictionary<string, HashSet<int>> bundleAllDeps,
        Dictionary<string, HashSet<string>> bundleAllTags,
        List<AssetBundleBuild> builds)
    {
        if (!bundleAllDeps.ContainsKey(bundleName) || !bundleAllTags.ContainsKey(bundleName))
            return;

        HashSet<string> tags = bundleAllTags[bundleName];
        if (tags.Count == 0)
            return;

        // 将tags传播到所有依赖的bundles
        foreach (int depBundleID in bundleAllDeps[bundleName])
        {
            if (depBundleID >= 0 && depBundleID < builds.Count)
            {
                string depBundleName = builds[depBundleID].assetBundleName;
                if (bundleAllTags.ContainsKey(depBundleName))
                {
                    foreach (var tag in tags)
                    {
                        bundleAllTags[depBundleName].Add(tag);
                    }
                }
            }
        }
    }

    static string CalculateMD5(string filePath)
    {
        using (var md5 = System.Security.Cryptography.MD5.Create())
        {
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }

    static string CalculateMD5FromBytes(byte[] bytes)
    {
        using (var md5 = System.Security.Cryptography.MD5.Create())
        {
            byte[] hash = md5.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }

    static uint CalculateCRC32(string filePath)
    {
        uint crc = 0xFFFFFFFF;
        byte[] buffer = File.ReadAllBytes(filePath);

        for (int i = 0; i < buffer.Length; i++)
        {
            crc ^= buffer[i];
            for (int j = 0; j < 8; j++)
            {
                if ((crc & 1) != 0)
                    crc = (crc >> 1) ^ 0xEDB88320;
                else
                    crc >>= 1;
            }
        }

        return ~crc;
    }

    /// <summary>
    /// 加密AssetBundle文件（在头部插入确定性的随机数据）
    /// </summary>
    static void EncryptAssetBundles(string outputPath, List<AssetBundleBuild> builds)
    {
        const int OFFSET_SIZE = 128;

        // 注意：不要在这里实例化 Random，否则所有文件的随机头是相关联的，且每次运行都不一样

        int encryptedCount = 0;

        foreach (var build in builds)
        {
            string bundlePath = Path.Combine(outputPath, build.assetBundleName);

            if (!File.Exists(bundlePath))
            {
                Debug.LogWarning($"Bundle file not found for encryption: {bundlePath}");
                continue;
            }

            try
            {
                byte[] originalData = File.ReadAllBytes(bundlePath);

                // 【核心修改】：使用文件名的 HashCode 作为随机数种子
                // 只要文件名不变，GetHashCode() 返回的整数就不变（在同一运行环境下）
                // 从而保证 new Random(seed) 生成的随机序列是固定的
                int seed = build.assetBundleName.GetHashCode();
                System.Random random = new System.Random(seed);

                byte[] offsetData = new byte[OFFSET_SIZE];
                random.NextBytes(offsetData); // 生成该文件专属的“固定”随机头

                byte[] encryptedData = new byte[OFFSET_SIZE + originalData.Length];
                Buffer.BlockCopy(offsetData, 0, encryptedData, 0, OFFSET_SIZE);
                Buffer.BlockCopy(originalData, 0, encryptedData, OFFSET_SIZE, originalData.Length);

                File.WriteAllBytes(bundlePath, encryptedData);
                encryptedCount++;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to encrypt bundle {bundlePath}: {e.Message}");
            }
        }

        Debug.Log($"Encrypted {encryptedCount} AssetBundle files with {OFFSET_SIZE} bytes offset");
    }

    /// <summary>
    /// 预处理Lua文件：拷贝到临时目录并加密
    /// </summary>
    static void PreprocessLuaFiles()
    {
        string projectRoot = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, ".."));
        string sourceLuaDir = Path.Combine(projectRoot, "LuaScripts");
        string destLuaTempDir = Path.Combine(UnityEngine.Application.dataPath, "LuaScriptsTemp");

        Debug.Log($"[PreprocessLua] 开始预处理Lua文件");
        Debug.Log($"[PreprocessLua] 源目录: {sourceLuaDir}");
        Debug.Log($"[PreprocessLua] 目标目录: {destLuaTempDir}");

        // 检查源目录是否存在
        if (!Directory.Exists(sourceLuaDir))
        {
            Debug.LogWarning($"[PreprocessLua] Lua源目录不存在: {sourceLuaDir}，跳过Lua预处理");
            return;
        }

        // 清理旧的临时目录
        if (Directory.Exists(destLuaTempDir))
        {
            Directory.Delete(destLuaTempDir, true);
            Debug.Log($"[PreprocessLua] 已清理旧的临时目录");
        }

        // 创建临时目录
        Directory.CreateDirectory(destLuaTempDir);

        // 加密并拷贝Lua文件
        int encryptedCount = LuaEncryptor.EncryptLuaDirectory(sourceLuaDir, destLuaTempDir);

        if (encryptedCount > 0)
        {
            AssetDatabase.Refresh();
            Debug.Log($"[PreprocessLua] Lua预处理完成，共加密 {encryptedCount} 个文件");
        }
        else
        {
            Debug.LogWarning($"[PreprocessLua] 没有找到Lua文件进行加密");
        }
    }
}

[Serializable]
public class BuildReportData
{
    public SummaryData Summary;
    public List<AssetInfoData> AssetInfos;
    public List<BundleInfoData> BundleInfos;
    public object[] IndependAssets;
}

[Serializable]
public class SummaryData
{
    public string UnityVersion;
    public string BuildDate;
    public int BuildSeconds;
    public int BuildTarget;
    public string BuildPipeline;
    public string BuildPackageName;
    public string BuildPackageVersion;
    public bool EnableAddressable;
    public int AssetFileTotalCount;
    public int MainAssetTotalCount;
    public int AllBundleTotalCount;
    public long AllBundleTotalSize;
}

[Serializable]
public class AssetInfoData
{
    public string Address;
    public string AssetPath;
    public string AssetGUID;
    public string[] AssetTags;
    public string MainBundleName;
    public long MainBundleSize;
    public List<DependAssetData> DependAssets;
    public List<string> DependBundles;
}

[Serializable]
public class DependAssetData
{
    public string AssetPath;
    public string AssetGUID;
}

[Serializable]
public class BundleInfoData
{
    public string BundleName;
    public string FileName;
    public string FileHash;
    public uint FileCRC;
    public long FileSize;
    public bool Encrypted;
    public string[] Tags;
    public List<string> DependBundles;
    public List<string> ReferenceBundles;
    public List<BundleContentData> BundleContents;
}

[Serializable]
public class BundleContentData
{
    public string AssetPath;
    public string AssetGUID;
}