-- game/example_module.lua - 示例游戏模块
-- 展示如何创建一个标准的Lua模块

local CS = CS
local UnityEngine = CS.UnityEngine
local Debug = UnityEngine.Debug
local GameObject = UnityEngine.GameObject
local Time = UnityEngine.Time
local Vector3 = UnityEngine.Vector3
local TestXluaInteract = CS.Test.TestXluaInteract

-- 模块类定义
local GameModule = {}
GameModule.__index = GameModule

-- 构造函数
function GameModule.new()
    local self = setmetatable({}, GameModule)

    self.timer = 0
    self.counter = 0

    self.go = GameObject.Find("TestXluaInteract")
    self.transform = self.go.transform
    self.interactComponent = self.go:GetComponent(typeof(CS.Test.TestXluaInteract))
    self.clickInfo = {}
    self.events = {}

    return self
end

-- 初始化方法
function GameModule:OnInit()
    Debug.Log("[GameModule] 初始化中...")


    Debug.Log("[GameModule] 初始化完成")
end

function GameModule:ClickCall()
    Debug.Log("[TestInteract] Lua 回调被调用！")
end

-- 更新方法
function GameModule:Update(deltaTime)
    self.timer = self.timer + deltaTime

    --取事件
    local len = TestXluaInteract.GetEvents(self.events)
    if len > 0 then
        for i = 0, len - 1 do
            local ev = self.events[i]

            -- 直接访问 struct 中的字段
            local event_id = ev.eventId
            local pos_x = ev.x
            local pos_y = ev.y

            -- 执行你的业务逻辑
            local info = self.clickInfo[event_id]
            if info then
                info.cb(pos_x, pos_y, info.param)
            end
        end

        -- 【重要】处理完所有事件后，通知 C# 清空计数器
        TestXluaInteract.Clear()
    end

    -- 每秒输出一次
    if self.timer >= 1.0 then
        self.timer = 0
        self.counter = self.counter + 1
        Debug.Log(string.format("[GameModule] 运行中... 计数: %d", self.counter))

        self:test1()
        self:test2()
        self:test3()
        self:test4()
        self:test5()
        self:test6()
    end
end

function GameModule:test1()
    for i = 1, 1000 do
        local luaCallback = function()
            Debug.Log("[TestInteract] Lua 回调被调用！" .. i)
            -- 在这里添加你的回调逻辑
        end
        self.interactComponent:Bind(luaCallback)
    end
end

function GameModule:test2()
    for i = 1, 1000 do
        self.interactComponent:Bind(self.ClickCall)
    end
end

function GameModule:test3()
    for i = 1, 1000 do
        self.interactComponent:BindAction(self.ClickCall)
    end
end

function GameModule:test4()
    for i = 1, 1000 do
        self.interactComponent:BindParamsAction(self.ClickCall)
    end
end

function GameModule:test5()
    for i = 1, 1000 do
        self.interactComponent:BindParamsAction_Default(self.ClickCall)
    end
end

function GameModule:test6()
    local com_btn = self.interactComponent
    for i = 1, 1000 do
        local ev_key = com_btn.ClickEventId
        if ev_key <= 0 then
            ev_key = com_btn:AddClickListener() --c#分配一个唯一的id
        end
        local info = self.clickInfo[ev_key]
        if info then
            info.cb = self.ClickCall_btn
            info.param = i
        else
            info = { cb = self.ClickCall_btn, param = i }
            self.clickInfo[ev_key] = info
        end
    end
end

function GameModule:ClickCall_btn(x, y, param)
    Debug.Log("[TestInteract] Lua 回调被调用！" .. param)
end

-- LateUpdate方法（可选）
function GameModule:LateUpdate(deltaTime)
    -- 在这里处理需要在Update之后执行的逻辑
end

-- FixedUpdate方法（可选）
function GameModule:FixedUpdate(fixedDeltaTime)
    -- 在这里处理物理相关的逻辑
end

-- 销毁方法
function GameModule:OnDestroy()
    Debug.Log("[GameModule] 销毁中...")

    Debug.Log("[GameModule] 已销毁")
end

-- 公共方法示例
function GameModule:GetCounter()
    return self.counter
end

function GameModule:ResetCounter()
    self.counter = 0
    Debug.Log("[GameModule] 计数器已重置")
end

return GameModule
