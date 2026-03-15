// GlobalInitializer.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using EasyTools;

public class GlobalInitializer : MonoBehaviour
{
    public static GlobalInitializer Instance { get; private set; }

    [Header("初始化配置")]
    [SerializeField] private ResourceBootstrap resourceBootstrap;
    [SerializeField] private XLuaManager xLuaManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private PatchUpdaterMono patchUpdaterMono;

    private bool _isInitilized = false;

    // 模块字典
    private Dictionary<Type, object> modules = new Dictionary<Type, object>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (patchUpdaterMono != null)
            RegisterModule(patchUpdaterMono);

        var playMode = resourceBootstrap.playMode;

        if (playMode == EPlayMode.EditorSimulateMode)
        {
            // Editor模式：直接初始化
            StartCoroutine(InitializeAll());
        }
        else if (playMode == EPlayMode.OfflinePlayMode)
        {
            if (!EasyAsset.Instance.NeedUnZipPack())
            {
                // 已完成首包解压，直接初始化
                StartCoroutine(InitializeAll());
            }
            else
            {
                // 需要首包解压，走热更流程（仅解压），完成后回调初始化
                StartPatch(isOfflineMode: true);
            }
        }
        else if (playMode == EPlayMode.HostPlayMode)
        {
            // Host模式：走完整热更流程，完成后回调初始化
            StartPatch(isOfflineMode: false);
        }
    }

    private void StartPatch(bool isOfflineMode)
    {
        if (patchUpdaterMono == null)
        {
            Debug.LogError("[GlobalInitializer] PatchUpdaterMono未配置，无法执行热更流程");
            return;
        }

        Debug.Log($"[GlobalInitializer] 启动热更流程 (isOfflineMode={isOfflineMode})");

        patchUpdaterMono.isOfflineMode = isOfflineMode;
        patchUpdaterMono.ResetUpdateComplete();
        patchUpdaterMono.RegisterOnUpdateComplete(OnPatchComplete);
        patchUpdaterMono.RegisterOnUpdateFailed(OnPatchFailed);
        patchUpdaterMono.StartUpdate();
    }

    private void OnPatchComplete()
    {
        Debug.Log("[GlobalInitializer] 热更流程完成，开始初始化");
        patchUpdaterMono.UnregisterOnUpdateComplete(OnPatchComplete);
        patchUpdaterMono.UnregisterOnUpdateFailed(OnPatchFailed);
        StartCoroutine(InitializeAll());
    }

    private void OnPatchFailed(string error)
    {
        Debug.LogError($"[GlobalInitializer] 热更流程失败: {error}");
        patchUpdaterMono.UnregisterOnUpdateComplete(OnPatchComplete);
        patchUpdaterMono.UnregisterOnUpdateFailed(OnPatchFailed);
    }

    public EPlayMode GetPlayMode()
    {
        return resourceBootstrap.playMode;
    }

    private IEnumerator InitializeAll()
    {
        if (_isInitilized)
        {
            Debug.LogWarning("[GlobalInitializer] 已经初始化完成，跳过重复初始化");
            yield break;
        }

        Debug.Log("[GlobalInitializer] 开始全局初始化流程");

        // 第一步：初始化并注册ResourceBootstrap
        if (resourceBootstrap != null)
        {
            Debug.Log("[GlobalInitializer] 步骤1: 初始化ResourceBootstrap");
            yield return resourceBootstrap.Initialize();
            RegisterModule(resourceBootstrap);
            Debug.Log("[GlobalInitializer] ResourceBootstrap初始化并注册完成");
        }
        else
        {
            Debug.LogError("[GlobalInitializer] ResourceBootstrap未配置");
        }

        // 第二步：初始化并注册XLuaManager
        if (xLuaManager != null)
        {
            Debug.Log("[GlobalInitializer] 步骤2: 初始化XLuaManager");
            var loadFromBundle = resourceBootstrap.playMode == EPlayMode.HostPlayMode || resourceBootstrap.playMode == EPlayMode.OfflinePlayMode;
            yield return xLuaManager.Initialize(loadFromBundle);
            RegisterModule(xLuaManager);
            Debug.Log("[GlobalInitializer] XLuaManager初始化并注册完成");
        }
        else
        {
            Debug.LogError("[GlobalInitializer] XLuaManager未配置");
        }

        // 第三步：初始化并注册UIManager
        if (uiManager != null)
        {
            Debug.Log("[GlobalInitializer] 步骤3: 初始化UIManager");
            yield return uiManager.Initialize();
            RegisterModule(uiManager);
            Debug.Log("[GlobalInitializer] UIManager初始化并注册完成");
        }
        else
        {
            Debug.LogError("[GlobalInitializer] UIManager未配置");
        }

        // 配置帧率
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 300;
        _isInitilized = true;

        Debug.Log("[GlobalInitializer] 全局初始化完成");
    }

    /// <summary>
    /// 注册模块
    /// </summary>
    public void RegisterModule<T>(T module) where T : class
    {
        Type type = typeof(T);
        if (modules.ContainsKey(type))
        {
            Debug.LogWarning($"[GlobalInitializer] 模块 {type.Name} 已存在，将被覆盖");
            modules[type] = module;
        }
        else
        {
            modules.Add(type, module);
            Debug.Log($"[GlobalInitializer] 模块 {type.Name} 注册成功");
        }
    }

    /// <summary>
    /// 获取模块
    /// </summary>
    public T GetModule<T>() where T : class
    {
        Type type = typeof(T);
        if (modules.TryGetValue(type, out object module))
        {
            return module as T;
        }

        Debug.LogError($"[GlobalInitializer] 未找到模块 {type.Name}");
        return null;
    }

    /// <summary>
    /// 检查模块是否存在
    /// </summary>
    public bool HasModule<T>() where T : class
    {
        return modules.ContainsKey(typeof(T));
    }

    /// <summary>
    /// 移除模块
    /// </summary>
    public void UnregisterModule<T>() where T : class
    {
        Type type = typeof(T);
        if (modules.Remove(type))
        {
            Debug.Log($"[GlobalInitializer] 模块 {type.Name} 已移除");
        }
    }
}
