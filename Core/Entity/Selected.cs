using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using NLua;
using Teuria;
namespace Pixxa;

public class Selected : Entity 
{
    public Sprite Sprite;
    private EntityState state;
    private MouseInput mouseInput;

    public Selected(EntityState state, MouseInput mouseInput) 
    {
        this.mouseInput = mouseInput;
        this.state = state;
        Tags = 1;
    }

    public void RenewSprite() 
    {
        RemoveComponent(Sprite);
        Sprite = new Sprite(state.CurrentTexture);
        AddComponent(Sprite);
    }

    public override void EnterScene(Scene scene, ContentManager content)
    {
        Sprite = new Sprite(state.CurrentTexture);
        AddComponent(Sprite);
        base.EnterScene(scene, content);
    }

    public void InputHovering(TabMode tabMode, ToolMode toolMode) 
    {
        if (tabMode != TabMode.Entity)
            return;
        if (state.CurrentTexture != null) 
        {
            var snappedPosition = mouseInput.GetCanvasMousePosition()
                .Snapped(Vector2.One * 8);
                
            Position = snappedPosition;

            if (TInput.Mouse.JustLeftClicked()
            && toolMode == ToolMode.PencilMode) 
            {
                Spawn(Position);
            }
            if (Sprite.Texture != state.CurrentTexture) 
            {
                RenewSprite();
            }
        }
    }

    public void Spawn(Vector2 pos) 
    {
        var characterEntity = new CharacterEntity(mouseInput, pos, state.CurrentEntity);
        Scene.Add(characterEntity);
    }
}