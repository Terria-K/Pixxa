using System;
using System.Collections.Generic;
using System.IO;
using Num = System.Numerics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Teuria;

namespace Pixxa;


public class EntityState 
{
    public List<EntityTexture> TexturesPtr = new List<EntityTexture>();
    public SpriteTexture CurrentTexture;
    public EntityTexture CurrentEntity;
    private ImGuiRenderer renderer;
    private static BlendState _blendColor;
    private static BlendState _blendAlpha;

    public EntityState(ImGuiRenderer renderer) 
    {
        this.renderer = renderer;
    }

    public void AddTexture(string name, ReadOnlySpan<char> path) 
    {
        var texture = LoadTextureStream(GameApp.Instance.GraphicsDevice, path);
        var texturePtr = new EntityTexture() 
        {
            Name = name,
            ImagePath = new string(path),
            Size = new Num.Vector2(texture.Width, texture.Height),
            Texture = new SpriteTexture(texture),
            TexturePtr = renderer.BindTexture(texture)
        };
        TexturesPtr.Add(texturePtr);
    }

    public void AddTexture(string name, ReadOnlySpan<char> path, Point pos, Point size) 
    {
        var texture = LoadTextureStream(GameApp.Instance.GraphicsDevice, path);
        var texturePtr = new EntityTexture() 
        {
            Name = name,
            ImagePath = new string(path),
            Size = new Num.Vector2(size.X, size.Y),
            Texture = new SpriteTexture(texture, new Point(pos.X, pos.Y), size.X, size.Y),
            TexturePtr = renderer.BindTexture(texture)
        };
        TexturesPtr.Add(texturePtr);
    }

    public void SetTexture(SpriteTexture texture2D) 
    {
        CurrentTexture = texture2D;
    }

    public void SetEntity(EntityTexture entity) 
    {
        CurrentEntity = entity;
    }

    public static Texture2D LoadTextureStream(GraphicsDevice graphics, ReadOnlySpan<char> loc)
    {
        Texture2D file = null;
        RenderTarget2D result = null;

        using (Stream titleStream = new FileStream(loc.ToString(), FileMode.Open))
        {
            file = Texture2D.FromStream(graphics, titleStream);
        }

        //Setup a render target to hold our final texture which will have premulitplied alpha values
        result = new RenderTarget2D(graphics, file.Width, file.Height);

        graphics.SetRenderTarget(result);
        graphics.Clear(Color.Black);

        //Multiply each color by the source alpha, and write in just the color values into the final texture
        if (_blendColor == null)
        {
            _blendColor = new BlendState();
            _blendColor.ColorWriteChannels = ColorWriteChannels.Red | ColorWriteChannels.Green | ColorWriteChannels.Blue;

            _blendColor.AlphaDestinationBlend = Blend.Zero;
            _blendColor.ColorDestinationBlend = Blend.Zero;

            _blendColor.AlphaSourceBlend = Blend.SourceAlpha;
            _blendColor.ColorSourceBlend = Blend.SourceAlpha;
        }

        SpriteBatch spriteBatch = new SpriteBatch(graphics);
        spriteBatch.Begin(SpriteSortMode.Immediate, _blendColor);
        spriteBatch.Draw(file, file.Bounds, Color.White);
        spriteBatch.End();

        //Now copy over the alpha values from the PNG source texture to the final one, without multiplying them
        if (_blendAlpha == null)
        {
            _blendAlpha = new BlendState();
            _blendAlpha.ColorWriteChannels = ColorWriteChannels.Alpha;

            _blendAlpha.AlphaDestinationBlend = Blend.Zero;
            _blendAlpha.ColorDestinationBlend = Blend.Zero;

            _blendAlpha.AlphaSourceBlend = Blend.One;
            _blendAlpha.ColorSourceBlend = Blend.One;
        }

        spriteBatch.Begin(SpriteSortMode.Immediate, _blendAlpha);
        spriteBatch.Draw(file, file.Bounds, Color.White);
        spriteBatch.End();

        //Release the GPU back to drawing to the screen
        graphics.SetRenderTarget(null);

        return result as Texture2D;
    } 



    public void Dispose() 
    {
        foreach (var texture in TexturesPtr) 
        {
            texture.Texture.Unload();
        }
    }
}

public class EntityTexture 
{
    public string Name;
    public Num.Vector2 Size;
    public IntPtr TexturePtr;
    public string ImagePath;
    public SpriteTexture Texture;
    public string ScriptName = "No Script attached";
    public string ScriptPath;
}