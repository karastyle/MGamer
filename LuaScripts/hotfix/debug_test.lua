-- test.lua
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