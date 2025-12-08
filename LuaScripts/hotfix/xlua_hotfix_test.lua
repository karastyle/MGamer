-- hotfix/xlua_hotfix_test.lua
print('[Lua] 🔥 开始执行热更补丁...')

-- 热更静态方法（注意：静态方法没有self参数）
xlua.hotfix(CS.TestHotfix, 'SayHello', function(name)
    return '🎉 Lua热更成功! 你好, ' .. name .. '! 时间: ' .. os.date('%H:%M:%S')
end)

xlua.hotfix(CS.TestHotfix, 'Calculate', function(a, b)
    local result = a * b  -- 改成乘法
    print(string.format('[Lua] Calculate被热更: %d × %d = %d', a, b, result))
    return result
end)

print('[Lua] ✅ 热更补丁加载完成！')
