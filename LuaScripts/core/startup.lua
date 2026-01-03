-- startup.lua - Lua入口脚本
-- 这是Xlua的启动入口，负责初始化Lua环境和设置更新循环

print("==========================================")
print("Lua环境启动中...")
print("==========================================")

-- 导入Unity引擎相关
local CS = CS
local UnityEngine = CS.UnityEngine
local GameObject = UnityEngine.GameObject
local Time = UnityEngine.Time
local Debug = UnityEngine.Debug

-- Lua模块管理
local ModuleManager = require "core/module_manager"

-- 全局变量
local isInitialized = false
local deltaTime = 0
local frameCount = 0

-- 初始化函数
local function Initialize()
    if isInitialized then return end

    Debug.Log("Lua环境初始化...")

    -- 初始化模块管理器
    ModuleManager.Initialize()

    collectgarbage("stop")

    isInitialized = true
    Debug.Log("Lua环境初始化完成！")
end

-- Update循环 - 对应Unity的Update
function Update()
    if not isInitialized then
        Initialize()
        return
    end

    deltaTime = Time.deltaTime
    frameCount = frameCount + 1

    -- 更新模块管理器
    ModuleManager.Update(deltaTime)
end

-- LateUpdate循环 - 对应Unity的LateUpdate
function LateUpdate()
    if not isInitialized then return end

    ModuleManager.LateUpdate(deltaTime)
end

-- FixedUpdate循环 - 对应Unity的FixedUpdate（物理更新）
function FixedUpdate()
    if not isInitialized then return end

    local fixedDeltaTime = Time.fixedDeltaTime
    ModuleManager.FixedUpdate(fixedDeltaTime)
end

-- 清理函数 - 对应Unity的OnDestroy
function OnDestroy()
    Debug.Log("Lua环境销毁中...")

    -- 清理模块
    ModuleManager.Destroy()

    -- 这里可以清理其他资源

    isInitialized = false
    Debug.Log("Lua环境已销毁")
end

-- 工具函数：创建GameObject
function CreateGameObject(name)
    local go = GameObject(name)
    Debug.Log("Lua创建了GameObject: " .. name)
    return go
end

-- 导出一些全局函数供C#调用
_G.GetFrameCount = function()
    return frameCount
end

_G.GetDeltaTime = function()
    return deltaTime
end

print("Lua入口脚本加载完成！")
print("等待初始化...")

-- dofile("D:/UnityProject/MGame/LuaScripts/hotfix/test_runner.lua")
-- dofile("D:/UnityProject/MGame/LuaScripts/hotfix/debug_runner.lua")
