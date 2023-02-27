using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NLua;
using Teuria;

namespace Pixxa;

public class CharacterEntity : Entity, ICollidableEntity 
{
    private Sprite sprite;
    private Lua lua;
    private LuaTable entity;
    private EntityTexture data;
    private Vector2 LastPosition;
    private float LastRotation;
    private Vector2 LastScale;
    private Color LastModulate;
    private int LastTags;
    private Shape shape;
    private PhysicsBody body;
    public Shape Collider => shape;
    public PhysicsComponent PhysicsComponent => body;
    public bool Hovered;
    public bool LuaActive;
    public Rectangle Size 
    {
        get 
        {
            var rectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)data.Size.X, (int)data.Size.Y);
            return rectangle;
        }
    }


    // TODO Add Lua Script here

    public CharacterEntity(Vector2 position, EntityTexture entity, Lua lua) 
    {
        this.lua = lua;
        Depth = 1;
        Position = position;
        if (entity.Texture != null) 
        {
            sprite = new Sprite(entity.Texture);
            AddComponent(sprite);
        }

        data = entity;
    }

    public void RunScript() 
    {
        if (data.ScriptPath == null) return;
        LuaActive = true;
        entity = (LuaTable)lua.LoadFile(data.ScriptPath).Call()[0];
        entity["main"] = this;
        entity["position"] = LastPosition = Position;
        entity["rotation"] = LastRotation = Rotation;
        entity["scale"] = LastScale = Scale;
        entity["color"] = LastModulate = Modulate;
        entity["tag"] = LastTags = Tags;
        var updateFunc = (LuaFunction)entity["ready"];
        updateFunc?.Call();
    }

    public void StopScript() 
    {
        if (data.ScriptPath == null) return;
        LuaActive = false;
        entity = null;
        Position = LastPosition;
        Rotation = LastRotation;
        Scale = LastScale;
        Modulate = LastModulate;
        Tags = LastTags;
    }

    public override void Update()
    {
        if (LuaActive) 
        {
            var updateFunc = (LuaFunction)entity["update"];
            updateFunc?.Call(Time.Delta);

            Position = (Vector2)entity["position"];
            Rotation = (float)(double)entity["rotation"];
            Scale = (Vector2)entity["scale"];
            Modulate = (Color)entity["color"];
            Collider.Tags = Tags = (int)(long)entity["tag"];
        }

        var mouseState = Mouse.GetState();

        var snappedPosition = Vector2.Transform(
            new Vector2(mouseState.X - Pixxa.Width/2, 
            (mouseState.Y - Pixxa.Height/2) + 150), Matrix.Invert(
                Scene.Camera.Transform)).Snapped(Vector2.One * 8);
        if (Size.Contains(snappedPosition)) 
            Hovered = true;
        else    
            Hovered = false;
        
            
        base.Update();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
        if (Hovered)
            Canvas.DrawRect(spriteBatch, Size.X - 1, Size.Y - 1, Size.Width + 2, Size.Height + 2, 1, Color.Gray);
    }

    public void AddRectangleBody(int width, int height) 
    {
        shape = new RectangleShape(width, height, Vector2.Zero);
        body = new PhysicsBody(shape, true);
        AddComponent(body);
    }

    public bool CollideWithTags(Tag tags, int offsetX, int offsetY) 
    {
        if (body == null) return false;
        var direction = new Vector2(offsetX, offsetY);
        return body.Check(this, tags, direction);
    }
}