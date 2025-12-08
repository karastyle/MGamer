-- ============================================
-- test.lua - 简化测试模块
-- ============================================

local M = {}

-- 全局变量
M.version = "1.0"
M.count = 0

-- 私有变量（局部变量）
local secret = "private"
local timer = 0
-- 闭包函数（持有私有变量）
local function getSecret()
    return secret  -- 闭包：访问外部局部变量
end

-- 普通函数
function M.Add(a, b)
    local result = a + b  -- 局部变量
    print("Add: " .. a .. "+" .. b .. "=" .. result)
    return result
end

-- 使用私有变量的函数
function M.Tick(dt)
    timer = timer + dt
    M.count = M.count + 1
    print("Tick: count=" .. M.count .. ", timer=" .. string.format("%.1f", timer))
end

-- 访问闭包
function M.GetSecret()
    return getSecret()
end

-- 修改私有变量
function M.SetSecret(val)
    secret = val
end

-- 重置
function M.Reset()
    M.count = 0
    timer = 0
    print("Reset")
end

return M