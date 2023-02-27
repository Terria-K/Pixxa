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

    public Selected(EntityState state) 
    {
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

    public void InputHovering(ToolMode toolMode, Lua luaState) 
    {
        if (state.CurrentTexture != null) 
        {
            var mouseState = Mouse.GetState();
            var scene = Scene.SceneAs<MainScene>();
            float currentX = mouseState.X - scene.ViewportPos.X;
            float currentY = mouseState.Y - scene.ViewportPos.Y;

            var mousePosX = (currentX / (float)scene.ViewportSize.X) * Pixxa.Width;
            var mousePosY = (currentY / (float)scene.ViewportSize.Y) * Pixxa.Height;

            var mousePos = new Vector2(mousePosX, mousePosY);

            var snappedPosition = Vector2.Transform(
                new Vector2(mousePos.X, mousePos.Y), 
                Matrix.Invert(Scene.Camera.Transform)
            )
            .Snapped(Vector2.One * 8);
                
            Position = snappedPosition;

            if (TInput.Mouse.JustLeftClicked()
            && toolMode == ToolMode.PencilMode) 
            {
                Spawn(Position, luaState);
            }
            if (Sprite.Texture != state.CurrentTexture) 
            {
                RenewSprite();
            }
        }
    }

    public void Spawn(Vector2 pos, Lua lua) 
    {
        var characterEntity = new CharacterEntity(pos, state.CurrentEntity, lua);
        Scene.Add(characterEntity);
    }
}