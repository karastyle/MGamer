using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XLua;
using EasyTools;

public class XLuaManager : MonoBehaviour
{
    [Header("Lua配置")] [SerializeField] private string luaEntryScript = "startup";
    [SerializeField] private string luaScriptFolder = "LuaScripts";
    [SerializeField] private bool loadFromAssetBundle = true;
    [SerializeField] private string luaBundleDir = "Assets/LuaScriptsTemp"; // Lua资源目录

    [Header("性能配置")] [SerializeField] private bool enableLuaCache = true; // 启用Lua脚本缓存
    [SerializeField] private int gcInterval = 1; // GC间隔（秒）

    private LuaEnv luaEnv;

    private LuaFunction luaUpdate;
    private LuaFunction luaLateUpdate;
    private LuaFunction luaFixedUpdate;
    private LuaFunction luaOnDestroy;

    private string luaScriptPath;

    // Lua脚本缓存，避免重复加载和解密
    private Dictionary<string, byte[]> luaScriptCache;

    // 用于优化GC调用频率
    private float lastGcTime;

    // 预分配的字符串Builder，减少GC
    private System.Text.StringBuilder pathBuilder;

    // 【新增】缓存Lua所在的Bundle
    private BundleLoader _luaBundleLoader;
    private AssetBundle _luaBundle;

    private void Awake()
    {
        if (enableLuaCache)
        {
            luaScriptCache = new Dictionary<string, byte[]>(32); // 预分配容量
        }

        pathBuilder = new System.Text.StringBuilder(128); // 预分配路径构建器
        lastGcTime = Time.realtimeSinceStartup;
    }

    /// <summary>
    /// 初始化接口，由外部调用
    /// </summary>
    public IEnumerator Initialize(bool loadFromBundle)
    {
        loadFromAssetBundle = loadFromBundle;
        yield return null;
    }

    public IEnumerable StartLua()
    {
        if (loadFromAssetBundle)
        {
            Debug.Log("[XLuaManager] 初始化Lua环境 - AssetBundle模式");

            // 【关键】先加载Lua Bundle
            yield return PreloadLuaBundle();

            InitLuaEnv();
            StartupLua();
        }
        else
        {
            luaScriptPath = Path.Combine(Application.dataPath, "..", luaScriptFolder);
            luaScriptPath = Path.GetFullPath(luaScriptPath);

            Debug.Log($"[XLuaManager] Lua脚本路径: {luaScriptPath}");

            if (!Directory.Exists(luaScriptPath))
            {
                Debug.LogError($"[XLuaManager] Lua脚本目录不存在: {luaScriptPath}");
                yield break;
            }

            InitLuaEnv();
            StartupLua();
        }
    }

    /// <summary>
    /// 预加载Lua Bundle（所有Lua脚本都在这个Bundle里）
    /// </summary>
    private IEnumerator PreloadLuaBundle()
    {
        Debug.Log("[XLuaManager] 预加载Lua Bundle...");

        // 判断当前平台是32位还是64位
        string platformSuffix = IntPtr.Size == 8 ? "_64" : "_32";
        
        // 通过第一个Lua文件找到它所在的Bundle
        pathBuilder.Clear();
        pathBuilder.Append(luaBundleDir);
        pathBuilder.Append('/');
        pathBuilder.Append(luaEntryScript);
        pathBuilder.Append(platformSuffix); // 添加平台后缀
        pathBuilder.Append(".txt");
        string startupPath = pathBuilder.ToString();

        // 使用EasyAsset查找资源信息
        bool foundBundle = false;

        // 尝试通过AssetPath查找
        if (EasyAsset.Instance.TryGetAssetInfo(startupPath, out var assetInfo))
        {
            _luaBundleLoader = EasyAsset.Instance.GetOrCreateBundleLoader(assetInfo.BundleID);
            foundBundle = true;
        }

        if (!foundBundle)
        {
            Debug.LogError($"[XLuaManager] 找不到Lua Bundle，路径: {startupPath}");
            yield break;
        }

        if (_luaBundleLoader != null)
        {
            _luaBundleLoader.Retain();
            yield return _luaBundleLoader.LoadAsync();

            if (_luaBundleLoader.Status == EProviderStatus.Succeed)
            {
                _luaBundle = _luaBundleLoader.Bundle;
                string[] allAssets = _luaBundle.GetAllAssetNames();
                Debug.Log($"[XLuaManager] Lua Bundle加载成功: {_luaBundleLoader.BundleName}，包含 {allAssets.Length} 个资源");

                // 打印前10个资源名称（调试用）
                Debug.Log("[XLuaManager] Bundle中的资源示例:");
                for (int i = 0; i < Mathf.Min(10, allAssets.Length); i++)
                {
                    Debug.Log($"  [{i}] {allAssets[i]}");
                }
            }
            else
            {
                Debug.LogError($"[XLuaManager] Lua Bundle加载失败: {_luaBundleLoader.LastError}");
            }
        }
    }

    private void InitLuaEnv()
    {
        luaEnv = new LuaEnv();
        luaEnv.AddLoader(CustomLuaLoader);
        Debug.Log("[XLuaManager] Lua虚拟机初始化完成");
    }

    /// <summary>
    /// 自定义Lua加载器：从已加载的Bundle中直接获取
    /// </summary>
    private byte[] CustomLuaLoader(ref string filepath)
    {
        try
        {
            // 检查缓存
            if (enableLuaCache && luaScriptCache != null &&
                luaScriptCache.TryGetValue(filepath, out byte[] cachedBytes))
            {
                Debug.Log($"[XLuaManager] 从缓存加载Lua: {filepath}");
                return cachedBytes;
            }

            byte[] luaBytes = loadFromAssetBundle
                ? LoadLuaFromBundleSync(filepath)
                : LoadLuaFromFileSystem(filepath);

            // 存入缓存
            if (enableLuaCache && luaBytes != null && luaScriptCache != null)
            {
                luaScriptCache[filepath] = luaBytes;
            }

            return luaBytes;
        }
        catch (Exception e)
        {
            Debug.LogError($"[XLuaManager] 加载Lua脚本失败: {filepath}, 错误: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// 从已加载的Bundle中同步获取Lua脚本（真正的同步，无需等待）
    /// </summary>
    private byte[] LoadLuaFromBundleSync(string filepath)
    {
        if (_luaBundle == null)
        {
            Debug.LogError("[XLuaManager] Lua Bundle未加载");
            return null;
        }

        try
        {
            // 判断当前平台是32位还是64位
            string platformSuffix = IntPtr.Size == 8 ? "_64" : "_32";

            // 构建资源路径
            pathBuilder.Clear();
            pathBuilder.Append(luaBundleDir);
            pathBuilder.Append('/');
            pathBuilder.Append(filepath.Replace('.', '/'));
            pathBuilder.Append(platformSuffix); // 添加平台后缀
            pathBuilder.Append(".txt");
            string fullPath = pathBuilder.ToString();

            Debug.Log($"[XLuaManager] 从Bundle同步加载Lua ({platformSuffix}): {fullPath}");

            // 【关键】直接从已加载的Bundle中同步获取资源
            TextAsset luaAsset = _luaBundle.LoadAsset<TextAsset>(fullPath);

            // 尝试其他路径格式（AssetBundle中的资源名通常是小写）
            if (luaAsset == null)
            {
                string lowerPath = fullPath.ToLower();
                Debug.Log($"[XLuaManager] 尝试小写路径: {lowerPath}");
                luaAsset = _luaBundle.LoadAsset<TextAsset>(lowerPath);
            }

            // 尝试只用文件名
            if (luaAsset == null)
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(fullPath);
                Debug.Log($"[XLuaManager] 尝试文件名: {fileName}");
                luaAsset = _luaBundle.LoadAsset<TextAsset>(fileName);
            }

            // 尝试相对路径（去掉Assets/）
            if (luaAsset == null && fullPath.StartsWith("Assets/"))
            {
                string relativePath = fullPath.Substring("Assets/".Length).ToLower();
                Debug.Log($"[XLuaManager] 尝试相对路径: {relativePath}");
                luaAsset = _luaBundle.LoadAsset<TextAsset>(relativePath);
            }

            if (luaAsset == null)
            {
                Debug.LogError($"[XLuaManager] 找不到Lua脚本: {fullPath}");

                // 调试：列出Bundle中所有资源
                string[] allAssets = _luaBundle.GetAllAssetNames();
                Debug.LogError($"[XLuaManager] Bundle中的所有资源 ({allAssets.Length}个):");
                foreach (string name in allAssets)
                {
                    Debug.LogError($"  - {name}");
                }

                return null;
            }

            // 解密Lua内容
            byte[] decryptedBytes = LuaEncryptor.DecryptLua(luaAsset.bytes);

            if (decryptedBytes == null)
            {
                Debug.LogError($"[XLuaManager] Lua脚本解密失败: {fullPath}");
                return null;
            }

            Debug.Log($"[XLuaManager] Lua脚本加载成功 ({platformSuffix}): {fullPath}, 大小: {decryptedBytes.Length} bytes");
            return decryptedBytes;
        }
        catch (Exception e)
        {
            Debug.LogError($"[XLuaManager] 从Bundle加载Lua失败: {filepath}, 错误: {e.Message}\n{e.StackTrace}");
            return null;
        }
    }

    /// <summary>
    /// 从文件系统加载Lua脚本（开发模式，未加密）
    /// </summary>
    private byte[] LoadLuaFromFileSystem(string filepath)
    {
        try
        {
            string scriptPath = filepath.Replace('.', Path.DirectorySeparatorChar);
            string fullPath = Path.Combine(luaScriptPath, scriptPath + ".lua");

            if (File.Exists(fullPath))
            {
                Debug.Log($"[XLuaManager] 从文件系统加载Lua: {fullPath}");
                return File.ReadAllBytes(fullPath);
            }
            else
            {
                Debug.LogWarning($"[XLuaManager] Lua脚本不存在: {fullPath}");
                return null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[XLuaManager] 从文件系统加载Lua失败: {filepath}, 错误: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// 同步启动Lua
    /// </summary>
    private void StartupLua()
    {
        try
        {
            Debug.Log($"[XLuaManager] 开始执行Lua入口脚本: {luaEntryScript}");

            luaEnv.DoString($"require('{luaEntryScript}')");

            LuaTable global = luaEnv.Global;
            luaUpdate = global.Get<LuaFunction>("Update");
            luaLateUpdate = global.Get<LuaFunction>("LateUpdate");
            luaFixedUpdate = global.Get<LuaFunction>("FixedUpdate");
            luaOnDestroy = global.Get<LuaFunction>("OnDestroy");

            Debug.Log($"[XLuaManager] Lua入口脚本 '{luaEntryScript}' 启动成功");
        }
        catch (Exception e)
        {
            Debug.LogError($"[XLuaManager] Lua启动失败: {e.Message}\n{e.StackTrace}");
        }
    }

    private void Update()
    {
        if (luaEnv == null) return;

        // 调用Lua的Update
        try
        {
            luaUpdate?.Call();
        }
        catch (Exception e)
        {
            Debug.LogError($"[XLuaManager] Lua Update执行错误: {e.Message}");
        }

        // 优化GC调用频率
        float currentTime = Time.realtimeSinceStartup;
        if (currentTime - lastGcTime >= gcInterval)
        {
            luaEnv.Tick();
            lastGcTime = currentTime;
        }
    }

    private void LateUpdate()
    {
        if (luaLateUpdate == null) return;

        try
        {
            luaLateUpdate.Call();
        }
        catch (Exception e)
        {
            Debug.LogError($"[XLuaManager] Lua LateUpdate执行错误: {e.Message}");
        }
    }

    private void FixedUpdate()
    {
        if (luaFixedUpdate == null) return;

        try
        {
            luaFixedUpdate.Call();
        }
        catch (Exception e)
        {
            Debug.LogError($"[XLuaManager] Lua FixedUpdate执行错误: {e.Message}");
        }
    }

    private void OnDestroy()
    {
        // 调用Lua的销毁函数
        try
        {
            luaOnDestroy?.Call();
        }
        catch (Exception e)
        {
            Debug.LogError($"[XLuaManager] Lua OnDestroy调用失败: {e.Message}");
        }

        // 手动释放所有 LuaFunction
        DisposeLuaFunction(ref luaUpdate);
        DisposeLuaFunction(ref luaLateUpdate);
        DisposeLuaFunction(ref luaFixedUpdate);
        DisposeLuaFunction(ref luaOnDestroy);

        // 卸载Lua Bundle
        if (_luaBundleLoader != null)
        {
            _luaBundleLoader.Release();
            _luaBundleLoader = null;
        }

        if (_luaBundle != null)
        {
            _luaBundle.Unload(true);
            _luaBundle = null;
        }

        // 销毁Lua虚拟机
        if (luaEnv != null)
        {
            try
            {
                luaEnv.Tick();
                luaEnv.FullGc();
                luaEnv.Dispose();
                luaEnv = null;
                Debug.Log("[XLuaManager] Lua虚拟机已销毁");
            }
            catch (Exception e)
            {
                Debug.LogError($"[XLuaManager] Lua虚拟机销毁失败: {e.Message}");
            }
        }

        // 清理缓存
        luaScriptCache?.Clear();
        luaScriptCache = null;
    }

    /// <summary>
    /// 安全释放LuaFunction
    /// </summary>
    private void DisposeLuaFunction(ref LuaFunction luaFunc)
    {
        if (luaFunc != null)
        {
            try
            {
                luaFunc.Dispose();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[XLuaManager] LuaFunction释放失败: {e.Message}");
            }
            finally
            {
                luaFunc = null;
            }
        }
    }

    /// <summary>
    /// 清除Lua脚本缓存
    /// </summary>
    public void ClearLuaCache()
    {
        if (luaScriptCache != null)
        {
            luaScriptCache.Clear();
            Debug.Log("[XLuaManager] Lua脚本缓存已清空");
        }
    }

    public LuaEnv GetLuaEnv()
    {
        return luaEnv;
    }

    public object[] DoString(string luaCode)
    {
        if (luaEnv == null)
        {
            Debug.LogError("[XLuaManager] Lua虚拟机未初始化");
            return null;
        }

        try
        {
            return luaEnv.DoString(luaCode);
        }
        catch (Exception e)
        {
            Debug.LogError($"[XLuaManager] 执行Lua代码失败: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// 手动触发Lua GC
    /// </summary>
    public void ManualGC()
    {
        if (luaEnv != null)
        {
            luaEnv.FullGc();
            Debug.Log("[XLuaManager] 手动触发Lua GC完成");
        }
    }

    /// <summary>
    /// 获取Lua内存使用量（KB）
    /// </summary>
    public long GetLuaMemory()
    {
        if (luaEnv != null)
        {
            return luaEnv.Memroy;
        }

        return 0;
    }
}