using System;
using UnityEngine;
using YooAsset;
using System.Collections;

/// <summary>
/// YooAsset简易初始化器
/// 仅支持编辑器模拟模式和单机模式
/// </summary>
public class YooAssetInitializer : MonoBehaviour
{
    [Header("Package配置")]
    [Tooltip("Package名称")]
    public string packageName = "DefaultPackage";
    
    [Header("调试")]
    public bool showDebugLog = true;
    
    /// <summary>
    /// 是否已初始化完成
    /// </summary>
    public static bool IsInitialized { get; private set; } = false;
    
    private ResourcePackage _package;

    private void OnDestroy()
    {
        YooAssets.Destroy();
    }

    public IEnumerator Initialize()
    {
        LogInfo("开始初始化YooAsset...");
        
        // 初始化YooAsset
        YooAssets.Initialize();
        
        // 创建Package
        _package = YooAssets.CreatePackage(packageName);
        YooAssets.SetDefaultPackage(_package);
        
        // 根据运行环境选择模式
        InitializationOperation initOperation;
        
#if UNITY_EDITOR
        // 编辑器下使用模拟模式
        initOperation = InitializeEditorSimulateMode();
        LogInfo("运行模式: 编辑器模拟模式");
#else
        // 打包后使用单机模式
        initOperation = InitializeOfflineMode();
        LogInfo("运行模式: 单机模式");
#endif
        
        yield return initOperation;
        
        // 检查初始化结果
        if (initOperation.Status == EOperationStatus.Succeed)
        {
            IsInitialized = true;
            LogInfo("YooAsset初始化完成!");
        }
        else
        {
            LogError($"YooAsset初始化失败: {initOperation.Error}");
        }
    }

    /// <summary>
    /// 编辑器模拟模式初始化
    /// </summary>
    private InitializationOperation InitializeEditorSimulateMode()
    {
        var buildResult = EditorSimulateModeHelper.SimulateBuild(packageName);
        var packageRoot = buildResult.PackageRootDirectory;
        var createParameters = new EditorSimulateModeParameters();
        createParameters.EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
        return _package.InitializeAsync(createParameters);
    }

    /// <summary>
    /// 单机模式初始化
    /// </summary>
    private InitializationOperation InitializeOfflineMode()
    {
        var initParameters = new OfflinePlayModeParameters();
        return _package.InitializeAsync(initParameters);
    }

    private void LogInfo(string message)
    {
        if (showDebugLog)
        {
            Debug.Log($"[YooAsset] {message}");
        }
    }

    private void LogError(string message)
    {
        Debug.LogError($"[YooAsset] {message}");
    }
}