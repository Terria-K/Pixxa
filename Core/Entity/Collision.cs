using Microsoft.Xna.Framework;
using Teuria;

namespace Pixxa;

public delegate void Collision(CollisionData data);
public struct CollisionData 
{
    public Vector2 TargetPosition;
    public ICollidableEntity Solid;
}