using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
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

    public override void Update()
    {
        if (state.CurrentTexture != null) 
        {
            var mouseState = Mouse.GetState();

            Position = Vector2.Transform(
                new Vector2(mouseState.X - MainScene.Width/2, 
                (mouseState.Y - MainScene.Height/2) + 150), Matrix.Invert(
                    Scene.Camera.Transform));
        }
        base.Update();
    }

    public void Spawn(Vector2 pos, Lua lua) 
    {
        var characterEntity = new CharacterEntity(pos, state.CurrentEntity, lua);
        Scene.Add(characterEntity);
    }
}