// ScriptableBuildParameters.cs
using System;
using UnityEditor;
using UnityEditor.Build.Pipeline;

/// <summary>
/// 自定义构建参数（模仿YooAsset实现）
/// </summary>
public class ScriptableBuildParameters
{
    public BuildTarget BuildTarget;
    public string OutputPath;
    public bool DisableWriteTypeTree;
    public UnityEngine.BuildCompression BundleCompression = UnityEngine.BuildCompression.LZ4;
    
    /// <summary>
    /// 内置Shader资源包名称
    /// 注意：必须和自定义Shader包名一致！
    /// </summary>
    public string BuiltinShadersBundleName;

    public ScriptableBuildParameters(BuildTarget target, string outputPath, bool disableWriteTypeTree)
    {
        BuildTarget = target;
        OutputPath = outputPath;
        DisableWriteTypeTree = disableWriteTypeTree;
    }

    /// <summary>
    /// 获取Unity原生的BundleBuildParameters
    /// </summary>
    public BundleBuildParameters GetBundleBuildParameters()
    {
        var targetGroup = BuildPipeline.GetBuildTargetGroup(BuildTarget);
        var buildParams = new BundleBuildParameters(BuildTarget, targetGroup, OutputPath)
        {
            BundleCompression = BundleCompression,
            UseCache = true,
            WriteLinkXML = true
        };
        
        // ✅ 剥离TypeTree，减少包体和内存占用
        if (DisableWriteTypeTree)
        {
            buildParams.ContentBuildFlags |= UnityEditor.Build.Content.ContentBuildFlags.DisableWriteTypeTree;
        }
        
        return buildParams;
    }
}