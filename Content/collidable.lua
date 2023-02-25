import('Pixxa')
local collidable = {}

function collidable:ready()
    collidable.tags = PhysicsTags.AsSolid
end

function collidable:update(delta)
    collidable.main:AddRectangleBody(20, 20)
end

return collidable