using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Teuria;

namespace Pixxa;

public class CharacterEntity : Entity, ICollidableEntity 
{
    private Sprite sprite;
    private PixxaEntity data;
    private MouseInput mouseInput;
    private Shape shape;
    private PhysicsBody body;
    public Shape Collider => shape;
    public PhysicsComponent PhysicsComponent => body;
    public bool Hovered;
    public Rectangle Size 
    {
        get 
        {
            var rectangle = new Rectangle((int)Position.X, (int)Position.Y, data.Size.X, data.Size.Y);
            return rectangle;
        }
    }


    public CharacterEntity(MouseInput mouseInput, Vector2 position, PixxaEntity entity) 
    {
        this.mouseInput = mouseInput;
        Depth = 1;
        Position = position;
        if (entity.Texture != null) 
        {
            sprite = new Sprite(entity.Texture);
            AddComponent(sprite);
        }

        data = entity;
        data.OnRemoved += () => Scene.Remove(this);
    }

    public override void Update()
    {
        var snappedPosition = mouseInput.GetCanvasMousePosition()
            .Snapped(Vector2.One * 8);

        if (Size.Contains(snappedPosition)) 
        {
            Hovered = true;
        }
        else    
            Hovered = false;
        
        if (Hovered && TInput.Mouse.JustRightClicked()) 
        {
            Scene.Remove(this);
        }
        
            
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