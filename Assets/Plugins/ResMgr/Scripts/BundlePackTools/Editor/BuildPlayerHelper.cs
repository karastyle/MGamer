// BuildPlayerHelper.cs

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;
using System.Text;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;

public static class BuildPlayerHelper
{
    public static void BuildPlayer(AssetBundleConfig config, BuildTarget target)
    {
        string platformName = GetPlatformName(target);
        string outputPath = Path.Combine(config.buildOutputPath, platformName);

        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        if (EditorUserBuildSettings.activeBuildTarget != target)
        {
            Debug.Log($"Switching platform to {target}...");
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildPipeline.GetBuildTargetGroup(target), target);
        }
        
        string executableName = GetExecutableName(target);
        string fullPath = Path.Combine(outputPath, executableName);

        WriteMajorVersion(config.buildVersion);

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = GetBuildScenes(config);
        buildPlayerOptions.locationPathName = fullPath;
        buildPlayerOptions.target = target;
        buildPlayerOptions.options = BuildOptions.Development 
                                     | BuildOptions.AllowDebugging 
                                     | BuildOptions.ConnectWithProfiler;

        Debug.Log($"开始打包: {fullPath}");

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"打包成功: {fullPath}");
            Debug.Log($"包体大小: {summary.totalSize / 1024f / 1024f:F2} MB");
            EditorUtility.DisplayDialog("Success",
                $"打包成功!\n\n输出路径: {fullPath}\n包体大小: {summary.totalSize / 1024f / 1024f:F2} MB",
                "OK");
        }
        else
        {
            Debug.LogError($"打包失败: {summary.result}");
            EditorUtility.DisplayDialog("Error", $"打包失败: {summary.result}", "OK");
        }
    }

    private static void WriteMajorVersion(string version)
    {
        string versionPath = Path.Combine(Application.streamingAssetsPath, "major.version");

        if (!Directory.Exists(Application.streamingAssetsPath))
        {
            Directory.CreateDirectory(Application.streamingAssetsPath);
        }

        File.WriteAllText(versionPath, version);
        AssetDatabase.Refresh();

        Debug.Log($"写入major.version: {version}");
    }

    private static string[] GetBuildScenes(AssetBundleConfig config)
    {
        if (config.buildScenes != null && config.buildScenes.Count > 0)
        {
            var scenes = new System.Collections.Generic.List<string>();
            foreach (var sceneAsset in config.buildScenes)
            {
                if (sceneAsset != null)
                {
                    string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
                    if (!string.IsNullOrEmpty(scenePath) && scenePath.EndsWith(".unity"))
                    {
                        scenes.Add(scenePath);
                    }
                }
            }
            return scenes.ToArray();
        }
        
        return GetEnabledScenes();
    }

    private static string[] GetEnabledScenes()
    {
        var scenes = new System.Collections.Generic.List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
            {
                scenes.Add(scene.path);
            }
        }

        return scenes.ToArray();
    }

    private static string GetExecutableName(BuildTarget target)
    {
        string productName = PlayerSettings.productName;

        switch (target)
        {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return $"{productName}.exe";
            case BuildTarget.Android:
                return $"{productName}.apk";
            case BuildTarget.iOS:
                return productName;
            case BuildTarget.StandaloneOSX:
                return $"{productName}.app";
            case BuildTarget.WebGL:
                return productName;
            default:
                return productName;
        }
    }

    private static string GetPlatformName(BuildTarget target)
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
            case BuildTarget.StandaloneOSX:
                return "Mac";
            case BuildTarget.WebGL:
                return "WebGL";
            default:
                return target.ToString();
        }
    }

    public static void PrintBuildParameters(BundleBuildParameters buildParams, BundleBuildContent buildContent,
        IList<IBuildTask> taskList)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("\n╔════════════════════════════════════════════════════════════════╗");
        sb.AppendLine("║         BuildAssetBundles 参数详细信息                          ║");
        sb.AppendLine("╚════════════════════════════════════════════════════════════════╝");

        sb.AppendLine("\n【1. BundleBuildParameters】");
        sb.AppendLine($"  ├─ Target: {buildParams.Target}");
        sb.AppendLine($"  ├─ Group: {buildParams.Group}");
        sb.AppendLine($"  ├─ OutputFolder: {buildParams.OutputFolder}");
        sb.AppendLine($"  ├─ ScriptOutputFolder: {buildParams.ScriptOutputFolder}");
        sb.AppendLine($"  ├─ TempOutputFolder: {buildParams.TempOutputFolder}");
        sb.AppendLine($"  ├─ UseCache: {buildParams.UseCache}");
        sb.AppendLine($"  ├─ CacheServerHost: {buildParams.CacheServerHost}");
        sb.AppendLine($"  ├─ CacheServerPort: {buildParams.CacheServerPort}");
        sb.AppendLine($"  ├─ WriteLinkXML: {buildParams.WriteLinkXML}");
        sb.AppendLine($"  ├─ ContentBuildFlags: {buildParams.ContentBuildFlags}");

        sb.AppendLine("\n【2. BundleBuildContent】");
        sb.AppendLine($"  ├─ BundleLayout Count: {buildContent.BundleLayout?.Count ?? 0}");

        if (buildContent.BundleLayout != null && buildContent.BundleLayout.Count > 0)
        {
            sb.AppendLine("  │");
            int bundleIndex = 0;
            foreach (var bundle in buildContent.BundleLayout)
            {
                bundleIndex++;
                sb.AppendLine($"  ├─ Bundle #{bundleIndex}: {bundle.Key}");
                sb.AppendLine($"  │  ├─ Asset Count: {bundle.Value.Count}");

                if (bundle.Value.Count > 0 && bundle.Value.Count <= 10)
                {
                    for (int i = 0; i < bundle.Value.Count; i++)
                    {
                        string prefix = (i == bundle.Value.Count - 1) ? "└─" : "├─";
                        sb.AppendLine($"  │  │  {prefix} {bundle.Value[i]}");
                    }
                }
                else if (bundle.Value.Count > 10)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        sb.AppendLine($"  │  │  ├─ {bundle.Value[i]}");
                    }

                    sb.AppendLine($"  │  │  ├─ ... ({bundle.Value.Count - 10} more) ...");
                    for (int i = bundle.Value.Count - 5; i < bundle.Value.Count; i++)
                    {
                        string prefix = (i == bundle.Value.Count - 1) ? "└─" : "├─";
                        sb.AppendLine($"  │  │  {prefix} {bundle.Value[i]}");
                    }
                }

                sb.AppendLine("  │");
            }
        }

        sb.AppendLine($"  ├─ Addressable Assets Count: {buildContent.Addresses?.Count ?? 0}");
        if (buildContent.Addresses != null && buildContent.Addresses.Count > 0)
        {
            sb.AppendLine("  │  (Showing first 20)");
            int count = 0;
            foreach (var addr in buildContent.Addresses)
            {
                if (count >= 20) break;
                sb.AppendLine($"  │  ├─ {addr.Key} -> {addr.Value}");
                count++;
            }

            if (buildContent.Addresses.Count > 20)
            {
                sb.AppendLine($"  │  └─ ... and {buildContent.Addresses.Count - 20} more");
            }
        }

        sb.AppendLine($"  ├─ Scenes Count: {buildContent.Scenes?.Count ?? 0}");
        if (buildContent.Scenes != null && buildContent.Scenes.Count > 0)
        {
            foreach (var scene in buildContent.Scenes)
            {
                sb.AppendLine($"  │  ├─ {scene}");
            }
        }

        sb.AppendLine($"  └─ CustomAssets Count: {buildContent.CustomAssets?.Count ?? 0}");
        if (buildContent.CustomAssets != null && buildContent.CustomAssets.Count > 0)
        {
            int count = 0;
            foreach (var asset in buildContent.CustomAssets)
            {
                if (count >= 10) break;
                sb.AppendLine($"     ├─ {asset.ToString()}");
                count++;
            }

            if (buildContent.CustomAssets.Count > 10)
            {
                sb.AppendLine($"     └─ ... and {buildContent.CustomAssets.Count - 10} more");
            }
        }

        sb.AppendLine("\n【3. Build Task List】");
        sb.AppendLine($"  ├─ Total Tasks: {taskList?.Count ?? 0}");
        if (taskList != null && taskList.Count > 0)
        {
            for (int i = 0; i < taskList.Count; i++)
            {
                string prefix = (i == taskList.Count - 1) ? "└─" : "├─";
                sb.AppendLine($"  {prefix} Task #{i + 1}: {taskList[i].GetType().Name}");
            }
        }

        sb.AppendLine("\n╔════════════════════════════════════════════════════════════════╗");
        sb.AppendLine("║                     参数打印结束                                ║");
        sb.AppendLine("╚════════════════════════════════════════════════════════════════╝\n");

        Debug.Log(sb.ToString());
    }
}