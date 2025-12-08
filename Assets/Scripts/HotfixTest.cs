using System;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public class HotfixTest : MonoBehaviour
{
    public XLuaManager manager;
    
    [ContextMenu("1. 测试热更前")]
    public void TestBeforeHotfix()
    {
        string result = TestHotfix.SayHello("World");
        Debug.Log($"<color=cyan>【热更前】</color> {result}");
    }
    
    [ContextMenu("2. 执行热更")]
    public void ExecuteHotfix()
    {
        if (manager == null)
        {
            Debug.LogError("XLuaManager未设置！");
            return;
        }
        
        Debug.Log("<color=yellow>========== 开始执行热更 ==========</color>");
        manager.DoString("require 'hotfix.xlua_hotfix_test'");
        Debug.Log("<color=yellow>========== 热更执行完成 ==========</color>");
    }
    
    [ContextMenu("3. 测试热更后")]
    public void TestAfterHotfix()
    {
        string result = TestHotfix.SayHello("World");
        Debug.Log($"<color=green>【热更后】</color> {result}");
    }
    
    [ContextMenu("完整测试流程")]
    public void RunFullTest()
    {
        Debug.Log("\n==================== 完整热更测试流程 ====================");
        TestBeforeHotfix();
        ExecuteHotfix();
        TestAfterHotfix();
        Debug.Log("==================== 测试流程结束 ====================\n");
    }
}

public static class MyHotfixConfig
{
    // 1. 这一步你已经做了：开启热更
    [Hotfix]
    public static List<Type> by_property
    {
        get { return new List<Type>() { typeof(TestHotfix) }; }
    }
}

[Hotfix]
public static class TestHotfix
{
    public static string SayHello(string name)
    {
        return "C# Hello, " + name;
    }
    
    public static int Calculate(int a, int b)
    {
        return a + b;
    }
}