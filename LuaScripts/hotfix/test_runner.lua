-- ============================================
-- test_runner_simple.lua - 简化测试脚本
-- ============================================

print("\n" .. string.rep("=", 50))
print("XLua 热更新测试")
print(string.rep("=", 50) .. "\n")

-- 步骤1: 加载模块
print("【1】加载test模块")
local test = require("hotfix.test")
print("版本: " .. test.version)
print()

-- 步骤2: 测试原始功能
print("【2】测试原始功能")
test.Add(5, 3)              -- 加法: 5+3=8
test.Tick(1.0)              -- 计数+1
test.SetSecret("原始秘密")
print("Secret: " .. test.GetSecret())
print()

-- 步骤3: 执行热更新
print("【3】执行热更新")
-- package.loaded["test_hotfix"] = nil
-- local hotfix = require("hotfix.test_hotfix")
local loadFunc = loadfile("D:/UnityProject/MGame/LuaScripts/hotfix/test_hotfix.lua")
if loadFunc then
    local hotfix = loadFunc()  -- ✅ 执行函数，获取模块表
    if hotfix and hotfix.Apply then
        hotfix.Apply()
    end
end
print()

-- 步骤4: 测试热更新后的功能
print("【4】测试热更新后")
test.Add(5, 3)              -- 乘法: 5×3=15
test.Tick(1.0)              -- 计数+10
test.NewFunc()              -- 新函数
print("版本: " .. test.version)
print()

-- 步骤5: 验证私有变量
print("【5】验证私有变量（闭包）")
print("Secret仍然是: " .. test.GetSecret())  -- 私有变量不受影响
test.SetSecret("新秘密")
print("修改后: " .. test.GetSecret())
print()

print(string.rep("=", 50))
print("测试完成")
print(string.rep("=", 50) .. "\n")

--[[
预期输出:

==================================================
XLua 热更新测试
==================================================

【1】加载test模块
版本: 1.0

【2】测试原始功能
Add: 5+3=8
Tick: count=1, timer=1.0
Secret: 原始秘密

【3】执行热更新

=== 开始热更新 ===
✓ 版本更新: 2.0 (热更)
✓ Add函数: 加法→乘法
✓ Tick函数: +1→+10
✓ 新增: NewFunc
=== 热更新完成 ===

【4】测试热更新后
Add[热更]: 5×3=15
Tick[热更]: count=11 (快速计数)
NewFunc: 这是新增的函数
版本: 2.0 (热更)

【5】验证私有变量（闭包）
Secret仍然是: 原始秘密
修改后: 新秘密

==================================================
测试完成
==================================================

关键点:
1. ✅ 全局变量(version)可热更
2. ✅ 普通函数(Add)可热更
3. ✅ 使用私有变量的函数(Tick)可热更
4. ✅ 可以添加新函数(NewFunc)
5. ⚠️  私有变量(secret)通过闭包访问，热更后仍保持原值
6. ✅ 但可以通过接口函数(SetSecret)修改私有变量
]]