using System;
using Microsoft.Xna.Framework;
using Teuria;

namespace Pixxa;

public class PhysicsBody : PhysicsComponent
{
    private Vector2 velocity;
    private Vector2 remainder;

    public Vector2 Velocity { get => velocity; set => velocity = value; }
    public Vector2 Remainder { get => remainder; set => remainder = value; }

    public PhysicsBody(Shape collider, bool collidable) : base(collider, collidable) {}

    public Vector2 MoveAndSlide(Vector2 motion, Vector2 direction) 
    {
        velocity = motion;
        velocity.X += direction.X;
        velocity.Y -= direction.Y;
        MoveX(velocity.X * Time.Delta, OnCollisionX);
        MoveY(velocity.Y * Time.Delta, OnCollisionY);
        return velocity;
    }

    private void OnCollisionX(CollisionData data) 
    {
        velocity.X = 0;
        remainder.X = 0;
    }

    private void OnCollisionY(CollisionData data) 
    {
        velocity.Y = 0;
        remainder.Y = 0;
    }

    public void MoveX(float amount, Collision onCollide = null, ICollidableEntity pusher = null)
    {
        remainder.X += amount;
        int move = (int)Math.Round(remainder.X);
        if (move == 0)
        {
            return;
        }
        remainder.X -= move;
        MoveXExact(move, onCollide, pusher);
    }

    public void MoveY(float amount, Collision onCollide = null, ICollidableEntity pusher = null)
    {
        remainder.Y += amount;
        int move = (int)Math.Round(remainder.Y);
        if (move == 0)
        {
            return;
        }
        remainder.Y -= move;
        MoveYExact(move, onCollide, pusher);
    }

    public void MoveXExact(float move, Collision onCollide = null, ICollidableEntity pusher = null) 
    {
        var targetPosition = Vector2.UnitX * move;
        int sign = Math.Sign(move);

        while (move != 0)
        {
            if (Check(PhysicsEntity, PhysicsTags.AsSolid, new Vector2(sign, 0)))
            {
                onCollide?.Invoke(new CollisionData() 
                {
                    TargetPosition = targetPosition,
                    Solid = pusher 
                });

                break;
            }
            Entity.PosX += sign;
            move -= sign;
        }
    }

    public void MoveYExact(float move, Collision onCollide = null, ICollidableEntity pusher = null) 
    {
        var targetPosition = Vector2.UnitX * move;
        int sign = Math.Sign(move);

        while (move != 0)
        {
            if (Check(PhysicsEntity, PhysicsTags.AsSolid, new Vector2(0, sign)))
            {
                onCollide?.Invoke(new CollisionData() 
                {
                    TargetPosition = targetPosition,
                    Solid = pusher 
                });
                break;
            }
            Entity.PosX += sign;
            move -= sign;
        }
    }
}