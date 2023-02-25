using Microsoft.Xna.Framework;
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


    // TODO Add Lua Script here

    public CharacterEntity(Vector2 position, EntityTexture entity, Lua lua) 
    {
        this.lua = lua;
        Depth = 1;
        Position = position;
        sprite = new Sprite(entity.Texture);
        data = entity;
        Active = false;
        AddComponent(sprite);
    }

    public void RunScript() 
    {
        if (data.ScriptPath == null) return;
        Active = true;
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
        Active = false;
        entity = null;
        Position = LastPosition;
        Rotation = LastRotation;
        Scale = LastScale;
        Modulate = LastModulate;
        Tags = LastTags;
    }

    public override void Update()
    {
        var updateFunc = (LuaFunction)entity["update"];
        updateFunc?.Call(Time.Delta);

        Position = (Vector2)entity["position"];
        Rotation = (float)(double)entity["rotation"];
        Scale = (Vector2)entity["scale"];
        Modulate = (Color)entity["color"];
        Collider.Tags = Tags = (int)(long)entity["tag"];
        base.Update();
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