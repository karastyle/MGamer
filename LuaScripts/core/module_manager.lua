-- core/module_manager.lua - Lua模块管理器
-- 管理所有Lua模块的生命周期

local CS = CS
local Debug = CS.UnityEngine.Debug

local ModuleManager = {}

-- 已注册的模块列表
local modules = {}
local moduleCount = 0

-- 注册模块
function ModuleManager.RegisterModule(moduleName, moduleInstance)
    if modules[moduleName] then
        Debug.LogWarning("模块已存在: " .. moduleName)
        return false
    end
    
    modules[moduleName] = moduleInstance
    moduleCount = moduleCount + 1
    Debug.Log(string.format("注册模块: %s (总数: %d)", moduleName, moduleCount))
    
    -- 调用模块的初始化方法
    if moduleInstance.OnInit then
        moduleInstance:OnInit()
    end
    
    return true
end

-- 获取模块
function ModuleManager.GetModule(moduleName)
    return modules[moduleName]
end

-- 移除模块
function ModuleManager.UnregisterModule(moduleName)
    local module = modules[moduleName]
    if not module then
        Debug.LogWarning("模块不存在: " .. moduleName)
        return false
    end
    
    -- 调用模块的销毁方法
    if module.OnDestroy then
        module:OnDestroy()
    end
    
    modules[moduleName] = nil
    moduleCount = moduleCount - 1
    Debug.Log(string.format("移除模块: %s (剩余: %d)", moduleName, moduleCount))
    
    return true
end

-- 初始化所有模块
function ModuleManager.Initialize()
    Debug.Log("模块管理器初始化...")
    
    -- 这里可以注册你的模块
    -- local GameModule = require("game/game_module")
    -- ModuleManager.RegisterModule("GameModule", GameModule.new())

    local SnapShotModule = require("game/snapshot_module")
    ModuleManager.RegisterModule("SnapShotModule", SnapShotModule.new())
    
    Debug.Log("模块管理器初始化完成")
end

-- 更新所有模块
function ModuleManager.Update(deltaTime)
    for name, module in pairs(modules) do
        if module.Update then
            local success, err = pcall(function()
                module:Update(deltaTime)
            end)
            
            if not success then
                Debug.LogError(string.format("模块 %s Update错误: %s", name, err))
            end
        end
    end
end

-- LateUpdate所有模块
function ModuleManager.LateUpdate(deltaTime)
    for name, module in pairs(modules) do
        if module.LateUpdate then
            local success, err = pcall(function()
                module:LateUpdate(deltaTime)
            end)
            
            if not success then
                Debug.LogError(string.format("模块 %s LateUpdate错误: %s", name, err))
            end
        end
    end
end

-- FixedUpdate所有模块
function ModuleManager.FixedUpdate(fixedDeltaTime)
    for name, module in pairs(modules) do
        if module.FixedUpdate then
            local success, err = pcall(function()
                module:FixedUpdate(fixedDeltaTime)
            end)
            
            if not success then
                Debug.LogError(string.format("模块 %s FixedUpdate错误: %s", name, err))
            end
        end
    end
end

-- 销毁所有模块
function ModuleManager.Destroy()
    Debug.Log("销毁所有模块...")
    
    for name, module in pairs(modules) do
        if module.OnDestroy then
            local success, err = pcall(function()
                module:OnDestroy()
            end)
            
            if not success then
                Debug.LogError(string.format("模块 %s OnDestroy错误: %s", name, err))
            end
        end
    end
    
    modules = {}
    moduleCount = 0
    
    Debug.Log("所有模块已销毁")
end

-- 获取模块数量
function ModuleManager.GetModuleCount()
    return moduleCount
end

-- 列出所有模块
function ModuleManager.ListModules()
    Debug.Log("已注册的模块列表:")
    for name, _ in pairs(modules) do
        Debug.Log("  - " .. name)
    end
end

return ModuleManager