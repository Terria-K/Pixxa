import('Pixxa')
import('System')
local block = {}

function block:ready()
    block.tags = PhysicsTags.AsSolid
end

function block:update(delta)
    if not block.main:CollideWithTags(PhysicsTags.AsSolid, 0, 0) then
     MoveX(block, 2)
    end
    block.main:AddRectangleBody(20, 20)
end

return block