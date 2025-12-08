-- run_test.lua
print("\n=== 热更新测试 ===\n")

-- 模拟test.lua模块
package.preload["hotfix.debug_test"] = function()
    local M = {}
    local count = 0
    
    function M.add()
        count = count + 1
        print("add: count = " .. count)
    end
    
    function M.get()
        return count
    end
    
    return M
end

-- 1. 加载模块
local test = require("hotfix.debug_test")

-- 2. 使用一段时间
test.add()
test.add()
test.add()
print("热更前: count =", test.get())

-- 3. 模拟修改test.lua（改为+10）
package.preload["hotfix.debug_test"] = function()
    local M = {}
    local count = 0
    
    function M.add()
        count = count + 10  -- 改为+10
        print("add[热更]: count = " .. count)
    end
    
    function M.get()
        return count
    end
    
    function M.reset()  -- 新增函数
        count = 0
        print("reset")
    end
    
    return M
end

-- 4. 执行热更新
local HotReload = require("hotfix.debug_hotfix")
print("\n执行热更新...")
HotReload("hotfix.debug_test")

-- 5. 验证
print("\n热更后: count =", test.get(), "(状态保持)")
test.add()
print("执行add后: count =", test.get(), "(新逻辑+10)")
test.reset()
print("执行reset后: count =", test.get(), "(新函数)")

print("\n=== 完成 ===\n")