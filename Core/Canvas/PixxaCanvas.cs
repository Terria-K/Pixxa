using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Teuria;

namespace Pixxa;

public class PixxaCanvas : CanvasLayer
{
    private RenderTarget2D canvasRT;
    private EntityState state;
    private int width;
    private int height;


    public PixxaCanvas(int width, int height, EntityState state, RenderTarget2D canvasRT) 
    {
        this.width = width;
        this.height = height;
        this.state = state;
        this.canvasRT = canvasRT;
    }

    public Rectangle Bounds() 
    {
        var rect = canvasRT.Bounds;
        return new Rectangle(rect.X, rect.Y - 300, rect.Width, rect.Height);
    }

    public override void PreDraw(Scene scene, SpriteBatch spriteBatch)
    {
        Pixxa.Instance.GraphicsDevice.SetRenderTarget(canvasRT);
        Pixxa.Instance.GraphicsDevice.Clear(Color.CornflowerBlue);
        spriteBatch.Begin(
            transformMatrix: scene.Camera?.Transform, 
            sortMode: SpriteSortMode.Immediate, blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
        foreach (var entity in scene.Entities) 
        {
            entity.Draw(spriteBatch);
        }
        spriteBatch.End();
        base.PreDraw(scene, spriteBatch);
    }
    public override void Draw(Scene scene, SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
        spriteBatch.Draw(canvasRT, new Vector2(width/2, (height/2) - 150), Color.White);
        spriteBatch.End();
    }
}