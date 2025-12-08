local function HotReload(moduleName)
    local old = package.loaded[moduleName]
    if not old then return false end

    package.loaded[moduleName] = nil
    local new = require(moduleName)

    -- 1. 依然保留 package.loaded 引用，保证外部引用不断
    package.loaded[moduleName] = old

    -- ========================================================================
    -- 【核心改进步骤 1】：构建 "全量 Upvalue 池"
    -- 扫描旧模块所有函数，记录所有变量名对应的 upvalue 位置
    -- 格式: map[变量名] = { func = 原函数引用, index = upvalue索引 }
    -- ========================================================================
    local upvalueMap = {}
    
    for _, func in pairs(old) do
        if type(func) == "function" then
            local i = 1
            while true do
                local name, val = debug.getupvalue(func, i)
                if not name then break end
                
                -- 如果池子里还没有这个变量，记录下来
                -- (多个函数引用同一个local变量时，记录任意一个即可，因为它们指向同一块内存)
                if not upvalueMap[name] then
                    upvalueMap[name] = { func = func, index = i }
                end
                i = i + 1
            end
        end
    end

    -- ========================================================================
    -- 【核心改进步骤 2】：新旧函数 Upvalue 嫁接
    -- ========================================================================
    for k, v in pairs(new) do
        if type(v) == "function" then
            local oldFunc = old[k]
            
            -- 遍历新函数的所有 Upvalue
            local i = 1
            while true do
                local name, val = debug.getupvalue(v, i)
                if not name then break end
                
                local isJoined = false
                
                -- >>> 策略 A：优先尝试从同名旧函数 (oldFunc) 中查找 <<<
                if oldFunc and type(oldFunc) == "function" then
                    local j = 1
                    while true do
                        local oldName = debug.getupvalue(oldFunc, j)
                        if not oldName then break end
                        
                        if oldName == name then
                            debug.upvaluejoin(v, i, oldFunc, j)
                            isJoined = true
                            break
                        end
                        j = j + 1
                    end
                end
                
                -- >>> 策略 B：如果同名函数里没找到，去 "全量池" 里找 <<<
                if not isJoined then
                    local target = upvalueMap[name]
                    if target then
                        debug.upvaluejoin(v, i, target.func, target.index)
                        -- print("  [HotReload] 从池中恢复变量:", name)
                    end
                end
                
                i = i + 1
            end
            
            -- 将旧模块表里的函数指针更新为新的
            old[k] = v
            
        elseif type(v) ~= "function" then
            -- 数据字段处理：仅新增，不覆盖
            if old[k] == nil then
                old[k] = v
            end
        end
    end

    return true
end

return HotReload