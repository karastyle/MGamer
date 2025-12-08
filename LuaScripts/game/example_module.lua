-- game/example_module.lua - 示例游戏模块
-- 展示如何创建一个标准的Lua模块

local CS = CS
local UnityEngine = CS.UnityEngine
local Debug = UnityEngine.Debug
local GameObject = UnityEngine.GameObject
local Time = UnityEngine.Time
local Vector3 = UnityEngine.Vector3

-- 模块类定义
local ExampleModule = {}
ExampleModule.__index = ExampleModule

-- 构造函数
function ExampleModule.new()
    local self = setmetatable({}, ExampleModule)
    
    -- 模块私有变量
    self.isActive = false
    self.timer = 0
    self.counter = 0
    self.testGameObject = nil
    
    return self
end

-- 初始化方法
function ExampleModule:OnInit()
    Debug.Log("[ExampleModule] 初始化中...")
    
    -- 创建一个测试GameObject
    self.testGameObject = GameObject("LuaTestObject")
    self.testGameObject.transform.position = Vector3(0, 0, 0)
    
    self.isActive = true
    Debug.Log("[ExampleModule] 初始化完成")
end

-- 更新方法
function ExampleModule:Update(deltaTime)
    if not self.isActive then return end
    
    self.timer = self.timer + deltaTime
    
    -- 每秒输出一次
    if self.timer >= 1.0 then
        self.timer = 0
        self.counter = self.counter + 1
        Debug.Log(string.format("[ExampleModule] 运行中... 计数: %d", self.counter))
        
        -- 让GameObject旋转
        if self.testGameObject then
            local rotation = self.testGameObject.transform.rotation
            self.testGameObject.transform:Rotate(Vector3.up * 45 * deltaTime)
        end
    end
end

-- LateUpdate方法（可选）
function ExampleModule:LateUpdate(deltaTime)
    -- 在这里处理需要在Update之后执行的逻辑
end

-- FixedUpdate方法（可选）
function ExampleModule:FixedUpdate(fixedDeltaTime)
    -- 在这里处理物理相关的逻辑
end

-- 销毁方法
function ExampleModule:OnDestroy()
    Debug.Log("[ExampleModule] 销毁中...")
    
    -- 清理GameObject
    if self.testGameObject then
        GameObject.Destroy(self.testGameObject)
        self.testGameObject = nil
    end
    
    self.isActive = false
    Debug.Log("[ExampleModule] 已销毁")
end

-- 公共方法示例
function ExampleModule:GetCounter()
    return self.counter
end

function ExampleModule:ResetCounter()
    self.counter = 0
    Debug.Log("[ExampleModule] 计数器已重置")
end

function ExampleModule:SetActive(active)
    self.isActive = active
    Debug.Log(string.format("[ExampleModule] 设置激活状态: %s", tostring(active)))
end

return ExampleModule