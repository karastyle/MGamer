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

    public string loadingSceneName;
    private bool _isInitilized = false;
    
    // 模块字典
    private Dictionary<Type, object> modules = new Dictionary<Type, object>();
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        if (resourceBootstrap.playMode == EPlayMode.HostPlayMode && EasyAsset.Instance.NeedUnZipPack())
        {
            // Host模式  且没有manifest，说明需要首包解压，否则启动不了
            Debug.LogWarning("请点击更新按钮，进行首包资源解压！");
        }
        else
        {
            StartCoroutine(InitializeAll());
        }
    }

    public EPlayMode GetPlayMode()
    {
        return resourceBootstrap.playMode;
    }
    
    public void EnterScene(string sceneName)
    {
        loadingSceneName = sceneName;
        StartCoroutine(TryEnterScene());
    }

    public IEnumerator TryEnterScene()
    {
        //有可能未初始化完成就调用了EnterScene，所以这里做个协程等待初始化完成
        yield return InitializeAll();
        yield return resourceBootstrap.Initialize();
        yield return xLuaManager.StartLua();
        
        uiManager.OpenPanel(PanelType.Loading);
        uiManager.ClosePanel(PanelType.Update);
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