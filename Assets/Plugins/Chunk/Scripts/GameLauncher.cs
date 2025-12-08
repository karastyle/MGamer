using UnityEngine;
using System.Collections;

/// <summary>
/// 游戏启动管理器（纯驱动版）
/// 只负责协调初始化流程，所有参数在各自组件上配置
/// </summary>
public class GameLauncher : MonoBehaviour
{
    [Header("调试")]
    public bool showDebugLog = true;
    public bool showInitProgress = true;
    
    // 初始化状态
    public enum InitState
    {
        None,
        InitializingYooAsset,
        WaitingForChunkLoader,
        Completed,
        Failed
    }
    
    public InitState CurrentState { get; private set; } = InitState.None;
    public float Progress { get; private set; } = 0f;
    public string StatusText { get; private set; } = "";

    private void Start()
    {
        StartCoroutine(InitializeGame());
    }

    /// <summary>
    /// 游戏初始化流程
    /// </summary>
    private IEnumerator InitializeGame()
    {
        LogInfo("========== 游戏启动流程开始 ==========");
        
        // 步骤1: 初始化YooAsset
        yield return InitializeYooAsset();
        if (CurrentState == InitState.Failed)
        {
            LogError("YooAsset初始化失败，启动中止");
            yield break;
        }
        
        // 步骤2: 等待ChunkLoader准备好
        yield return WaitForChunkLoader();
        
        // 完成
        CurrentState = InitState.Completed;
        Progress = 1f;
        StatusText = "初始化完成";
        LogInfo("========== 游戏启动流程完成 ==========");
    }

    /// <summary>
    /// 初始化YooAsset
    /// </summary>
    private IEnumerator InitializeYooAsset()
    {
        CurrentState = InitState.InitializingYooAsset;
        Progress = 0f;
        StatusText = "正在初始化资源系统...";
        LogInfo("查找YooAssetInitializer...");
        
        // 查找场景中的YooAssetInitializer
        YooAssetInitializer initializer = FindObjectOfType<YooAssetInitializer>();
        
        if (initializer == null)
        {
            CurrentState = InitState.Failed;
            StatusText = "未找到YooAssetInitializer组件";
            LogError(StatusText);
            yield break;
        }
        
        LogInfo("开始初始化YooAsset...");
        
        // 执行初始化
        yield return initializer.Initialize();
        
        if (!YooAssetInitializer.IsInitialized)
        {
            CurrentState = InitState.Failed;
            StatusText = "资源系统初始化失败";
            LogError(StatusText);
            yield break;
        }
        
        Progress = 0.5f;
        LogInfo("YooAsset初始化完成");
    }

    /// <summary>
    /// 等待ChunkLoader准备好
    /// </summary>
    private IEnumerator WaitForChunkLoader()
    {
        CurrentState = InitState.WaitingForChunkLoader;
        Progress = 0.7f;
        StatusText = "正在初始化场景加载系统...";
        LogInfo("查找ChunkStreamingLoader...");
        
        // 查找场景中的ChunkStreamingLoader
        ChunkStreamingLoader chunkLoader = FindObjectOfType<ChunkStreamingLoader>();
        
        if (chunkLoader == null)
        {
            LogInfo("未找到ChunkStreamingLoader，跳过");
            Progress = 0.9f;
            yield return null;
            yield break;
        }

        yield return chunkLoader.Initialize();
        
        LogInfo("等待ChunkLoader准备完成...");
        
        if (!ChunkStreamingLoader.IsInitialized)
        {
            CurrentState = InitState.Failed;
            StatusText = "ChunkLoader初始化失败";
            LogError(StatusText);
            yield break;
        }
        
        Progress = 0.9f;
        LogInfo("ChunkLoader准备完成");
        
        yield return null;
    }

    private void LogInfo(string message)
    {
        if (showDebugLog)
        {
            Debug.Log($"[GameLauncher] {message}");
        }
    }

    private void LogError(string message)
    {
        Debug.LogError($"[GameLauncher] {message}");
    }

    /// <summary>
    /// 显示初始化UI
    /// </summary>
    private void OnGUI()
    {
        if (!showInitProgress) return;
        if (CurrentState == InitState.Completed) return;
        
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.fontSize = 16;
        boxStyle.alignment = TextAnchor.MiddleCenter;
        
        GUIStyle textStyle = new GUIStyle(GUI.skin.label);
        textStyle.fontSize = 14;
        textStyle.alignment = TextAnchor.MiddleCenter;
        textStyle.normal.textColor = Color.white;
        
        float boxWidth = 400;
        float boxHeight = 120;
        float boxX = (Screen.width - boxWidth) / 2;
        float boxY = (Screen.height - boxHeight) / 2;
        
        GUI.Box(new Rect(boxX, boxY, boxWidth, boxHeight), "", boxStyle);
        
        GUI.Label(new Rect(boxX, boxY + 20, boxWidth, 30), "游戏初始化中...", textStyle);
        GUI.Label(new Rect(boxX, boxY + 50, boxWidth, 20), StatusText, textStyle);
        
        // 进度条
        float barWidth = boxWidth - 40;
        float barHeight = 20;
        float barX = boxX + 20;
        float barY = boxY + 80;
        
        GUI.Box(new Rect(barX, barY, barWidth, barHeight), "");
        GUI.Box(new Rect(barX + 2, barY + 2, (barWidth - 4) * Progress, barHeight - 4), "");
        
        GUI.Label(new Rect(boxX, boxY + 105, boxWidth, 20), 
            $"{Progress * 100:F0}%", textStyle);
    }
}