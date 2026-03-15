
local CS = CS
local UnityEngine = CS.UnityEngine
local Debug = UnityEngine.Debug
local GameObject = UnityEngine.GameObject
local Time = UnityEngine.Time
local Vector3 = UnityEngine.Vector3
local TestXluaInteract = CS.Test.TestXluaInteract

-- 模块类定义
local SnapShotModule = {}
SnapShotModule.__index = SnapShotModule

-- 构造函数
function SnapShotModule.new()
    local self = setmetatable({}, SnapShotModule)


    return self
end

-- 初始化方法
function SnapShotModule:OnInit()
    Debug.Log("[SnapShotModule] 初始化中...")

    self.timer = 0
    self.test_root = {} -- Root table to hold test objects
    
    -- Register to global for easy access or just keep local reference
    _G.SnapShotTestRoot = self.test_root

    local mri = require("luaDumpMemory/MemoryReferenceInfo")
    mri.m_cConfig.m_bAllMemoryRefFileAddTime = false

    local author = 
    {
        Name = "yaukeywang",
        Job = "Game Developer",
        Hobby = "Game, Travel, Gym",
        City = "Beijing",
        Country = "China",
        aaa = "aaa",
        Ask = function (question)
            return "My answer is for your question: " .. question .. "."
        end
    }

    _G.Author = author

    mri.m_cMethods.DumpMemorySnapshot("./", "1-Before", -1)

    local obj = GameObject("LeakTestObj1")
    self.test_root["LeakTestObj1_1"] = obj
    self.test_root["LeakTestObj1_2"] = obj
    GameObject.DestroyImmediate(obj)


    local obj = GameObject("LeakTestObj2")
    self.test_root["LeakTestObj2_1"] = obj
    self.test_root["LeakTestObj2_2"] = obj
    GameObject.DestroyImmediate(obj)

    
    Debug.Log("[SnapShotModule] 初始化完成")
end

-- 更新方法
function SnapShotModule:Update(deltaTime)
    self.timer = (self.timer or 0) + deltaTime
    if self.timer >= 1.0 then
        self.timer = self.timer - 1.0
        
        local count = 0
        for _ in pairs(self.test_root) do count = count + 1 end
        local index = count + 1
        
        local objName = "Test_Gen_Obj_" .. tostring(index)
        local newObj = {
            name = objName,
            desc = "Created at " .. tostring(Time.time),
            data = { 1, 2, 3, 4, index }
        }
        
        -- Use the name as key so it shows up clearly in snapshot
        self.test_root[objName] = newObj
        
        Debug.Log("[SnapShotModule] Created Object: " .. objName)
    end
end

-- LateUpdate方法（可选）
function SnapShotModule:LateUpdate(deltaTime)
    -- 在这里处理需要在Update之后执行的逻辑
end

-- FixedUpdate方法（可选）
function SnapShotModule:FixedUpdate(fixedDeltaTime)
    -- 在这里处理物理相关的逻辑
end

-- 销毁方法
function SnapShotModule:OnDestroy()
    Debug.Log("[SnapShotModule] 销毁中...")

    Debug.Log("[SnapShotModule] 已销毁")
end





--测试内存大小
global1 =
{
	[1] = "item1",
	[2] = "item2",
	key1 = "value1",
	key2 = "value2",
}

global2 =
{
	g = global1,
	f = function()
		local s = "local value"
		return s;
	end,
}

local s = "local string"
local f1 = function()
    print(s)
end

local f2 = function(v)
	local t = v
	return function()
		print(t)
	end
end










return SnapShotModule
