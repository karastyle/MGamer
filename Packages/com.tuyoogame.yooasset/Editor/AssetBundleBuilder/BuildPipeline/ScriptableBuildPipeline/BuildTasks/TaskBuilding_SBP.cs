using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline.Tasks;
using UnityEngine;

namespace YooAsset.Editor
{
    public class TaskBuilding_SBP : IBuildTask
    {
        public class BuildResultContext : IContextObject
        {
            public IBundleBuildResults Results;
            public string BuiltinShadersBundleName;
            public string MonoScriptsBundleName;
        }

        /// <summary>
        /// 打印 BuildAssetBundles 的所有参数信息
        /// </summary>
        public static void PrintBuildParameters(BundleBuildParameters buildParams, BundleBuildContent buildContent,
            IList<UnityEditor.Build.Pipeline.Interfaces.IBuildTask> taskList)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\n╔════════════════════════════════════════════════════════════════╗");
            sb.AppendLine("║         BuildAssetBundles 参数详细信息                          ║");
            sb.AppendLine("╚════════════════════════════════════════════════════════════════╝");

            // ========== BundleBuildParameters ==========
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

            // ========== BundleBuildContent ==========
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
                        // 如果资源数量不多，显示所有资源
                        for (int i = 0; i < bundle.Value.Count; i++)
                        {
                            string prefix = (i == bundle.Value.Count - 1) ? "└─" : "├─";
                            sb.AppendLine($"  │  │  {prefix} {bundle.Value[i]}");
                        }
                    }
                    else if (bundle.Value.Count > 10)
                    {
                        // 如果资源很多，只显示前5个和后5个
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

            // ========== Addressable Assets ==========
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

            // ========== Scenes ==========
            sb.AppendLine($"  ├─ Scenes Count: {buildContent.Scenes?.Count ?? 0}");
            if (buildContent.Scenes != null && buildContent.Scenes.Count > 0)
            {
                foreach (var scene in buildContent.Scenes)
                {
                    sb.AppendLine($"  │  ├─ {scene}");
                }
            }

            // ========== Custom Assets ==========
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

            // ========== Task List ==========
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

        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildMapContext = context.GetContextObject<BuildMapContext>();
            var scriptableBuildParameters = buildParametersContext.Parameters as ScriptableBuildParameters;

            // 构建内容
            var bundleBuilds = buildMapContext.GetPipelineBuilds(scriptableBuildParameters.ReplaceAssetPathWithAddress);
            var buildContent = new BundleBuildContent(bundleBuilds);

            // 开始构建
            IBundleBuildResults buildResults;
            var buildParameters = scriptableBuildParameters.GetBundleBuildParameters();
            string builtinShadersBundleName = scriptableBuildParameters.BuiltinShadersBundleName;
            string monoScriptsBundleName = scriptableBuildParameters.MonoScriptsBundleName;
            var taskList = SBPBuildTasks.Create(builtinShadersBundleName, monoScriptsBundleName);

            PrintBuildParameters(buildParameters, buildContent, taskList);

            ReturnCode exitCode =
                ContentPipeline.BuildAssetBundles(buildParameters, buildContent, out buildResults, taskList);
            if (exitCode < 0)
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.UnityEngineBuildFailed,
                    $"UnityEngine build failed ! ReturnCode : {exitCode}");
                throw new Exception(message);
            }

            // 说明：解决因为特殊资源包导致验证失败。
            // 例如：当项目里没有着色器，如果有依赖内置着色器就会验证失败。
            if (string.IsNullOrEmpty(builtinShadersBundleName) == false)
            {
                if (buildResults.BundleInfos.ContainsKey(builtinShadersBundleName))
                    buildMapContext.CreateEmptyBundleInfo(builtinShadersBundleName);
            }

            if (string.IsNullOrEmpty(monoScriptsBundleName) == false)
            {
                if (buildResults.BundleInfos.ContainsKey(monoScriptsBundleName))
                    buildMapContext.CreateEmptyBundleInfo(monoScriptsBundleName);
            }

            BuildLogger.Log("UnityEngine build success!");
            BuildResultContext buildResultContext = new BuildResultContext();
            buildResultContext.Results = buildResults;
            buildResultContext.BuiltinShadersBundleName = builtinShadersBundleName;
            buildResultContext.MonoScriptsBundleName = monoScriptsBundleName;
            context.SetContextObject(buildResultContext);
        }
    }
}