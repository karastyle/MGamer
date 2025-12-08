-- ============================================
-- test_hotfix_simple.lua - 简化热更新文件
-- ============================================

local hotfix = {}
hotfix.version = "2.0"

function hotfix.Apply()
    print("\n=== 开始热更新 ===")
    
    local M = package.loaded["hotfix.test"]
    
    -- 1. 热更新全局变量
    M.version = "2.0 (热更)"
    print("✓ 版本更新: " .. M.version)
    
    -- 2. 热更新普通函数
    M.Add = function(a, b)
        local result = a * b  -- 改为乘法
        print("Add[热更]: " .. a .. "×" .. b .. "=" .. result)
        return result
    end
    print("✓ Add函数: 加法→乘法")
    
    -- 3. 热更新使用私有变量的函数
    M.Tick = function(dt)
        M.count = M.count + 10  -- 改为+10
        print("Tick[热更]: count=" .. M.count .. " (快速计数)")
    end
    print("✓ Tick函数: +1→+10")
    
    -- 4. 添加新函数
    M.NewFunc = function()
        print("NewFunc: 这是新增的函数")
    end
    print("✓ 新增: NewFunc")
    
    print("=== 热更新完成 ===\n")
end

return hotfix