-- util/util.lua - Lua工具函数集合

local Util = {}

-- 深拷贝表
function Util.DeepCopy(obj, seen)
    if type(obj) ~= 'table' then return obj end
    if seen and seen[obj] then return seen[obj] end
    
    local s = seen or {}
    local res = setmetatable({}, getmetatable(obj))
    s[obj] = res
    
    for k, v in pairs(obj) do
        res[Util.DeepCopy(k, s)] = Util.DeepCopy(v, s)
    end
    
    return res
end

-- 浅拷贝表
function Util.ShallowCopy(obj)
    if type(obj) ~= 'table' then return obj end
    local res = {}
    for k, v in pairs(obj) do
        res[k] = v
    end
    return res
end

-- 打印表内容（调试用）
function Util.PrintTable(t, indent)
    indent = indent or 0
    local prefix = string.rep("  ", indent)
    
    for k, v in pairs(t) do
        if type(v) == "table" then
            print(prefix .. tostring(k) .. ":")
            Util.PrintTable(v, indent + 1)
        else
            print(prefix .. tostring(k) .. " = " .. tostring(v))
        end
    end
end

-- 判断表是否为空
function Util.IsTableEmpty(t)
    return next(t) == nil
end

-- 获取表的长度（包括非连续索引）
function Util.GetTableLength(t)
    local count = 0
    for _ in pairs(t) do
        count = count + 1
    end
    return count
end

-- 合并表
function Util.MergeTables(...)
    local result = {}
    for _, t in ipairs({...}) do
        for k, v in pairs(t) do
            result[k] = v
        end
    end
    return result
end

-- 数组包含检查
function Util.ArrayContains(array, value)
    for _, v in ipairs(array) do
        if v == value then
            return true
        end
    end
    return false
end

-- 移除数组元素
function Util.ArrayRemove(array, value)
    for i, v in ipairs(array) do
        if v == value then
            table.remove(array, i)
            return true
        end
    end
    return false
end

-- 字符串分割
function Util.Split(str, delimiter)
    local result = {}
    local pattern = string.format("([^%s]+)", delimiter)
    
    for match in string.gmatch(str, pattern) do
        table.insert(result, match)
    end
    
    return result
end

-- 字符串去除首尾空格
function Util.Trim(str)
    return (str:gsub("^%s*(.-)%s*$", "%1"))
end

-- 随机种子初始化（使用当前时间）
function Util.InitRandomSeed()
    math.randomseed(os.time())
end

-- 在范围内随机浮点数
function Util.RandomFloat(min, max)
    return min + math.random() * (max - min)
end

-- 在范围内随机整数
function Util.RandomInt(min, max)
    return math.random(min, max)
end

-- 限制值在范围内
function Util.Clamp(value, min, max)
    if value < min then return min end
    if value > max then return max end
    return value
end

-- 线性插值
function Util.Lerp(a, b, t)
    return a + (b - a) * Util.Clamp(t, 0, 1)
end

-- 角度转弧度
function Util.DegToRad(degrees)
    return degrees * math.pi / 180
end

-- 弧度转角度
function Util.RadToDeg(radians)
    return radians * 180 / math.pi
end

-- 格式化时间（秒转为 mm:ss 格式）
function Util.FormatTime(seconds)
    local mins = math.floor(seconds / 60)
    local secs = math.floor(seconds % 60)
    return string.format("%02d:%02d", mins, secs)
end

-- 性能计时器
Util.Timer = {}
Util.Timer.__index = Util.Timer

function Util.Timer.new()
    local self = setmetatable({}, Util.Timer)
    self.startTime = os.clock()
    return self
end

function Util.Timer:Reset()
    self.startTime = os.clock()
end

function Util.Timer:GetElapsed()
    return os.clock() - self.startTime
end

function Util.Timer:PrintElapsed(label)
    local elapsed = self:GetElapsed()
    print(string.format("[Timer] %s: %.4f秒", label or "耗时", elapsed))
    return elapsed
end

-- 简单的事件系统
Util.Event = {}
Util.Event.__index = Util.Event

function Util.Event.new()
    local self = setmetatable({}, Util.Event)
    self.listeners = {}
    return self
end

function Util.Event:AddListener(callback)
    table.insert(self.listeners, callback)
end

function Util.Event:RemoveListener(callback)
    for i, listener in ipairs(self.listeners) do
        if listener == callback then
            table.remove(self.listeners, i)
            return true
        end
    end
    return false
end

function Util.Event:Invoke(...)
    for _, listener in ipairs(self.listeners) do
        listener(...)
    end
end

function Util.Event:Clear()
    self.listeners = {}
end

return Util