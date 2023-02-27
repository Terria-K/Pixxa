using System;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Teuria;
using Num = System.Numerics;

namespace Pixxa;

public class Pixxa : GameApp
{
    public const int AppWidth = 1024;
    public const int AppHeight = 640;
    public const float AppAspectRatio = AppWidth / AppHeight;
    public ImGuiRenderer ImGuiRenderer;
    public static GameTime GameTime;


    public const int Width = 544;
    public const int Height = 480;

    public Pixxa(int width, int height, int screenWidth, int screenHeight, string windowTitle, bool fullScreen) : base(width, height, screenWidth, screenHeight, windowTitle, fullScreen)
    {
    }

    protected override void Init()
    {

        Window.AllowUserResizing = false;
        var camera = new Teuria.Camera(Width, Height);
        ImGuiRenderer = new ImGuiRenderer(this);
        ImGuiRenderer.RebuildFontAtlas();
        PhysicsTags.Init();
        Scene = new MainScene(Content, camera, ImGuiRenderer);
    }

    protected override void Process(GameTime gameTime)
    {
        GameTime = gameTime;
        base.Process(gameTime);
    }
}
