using System;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NLua;
using Teuria;

namespace Pixxa;

public partial class MainScene : Scene
{
    public EntityState EntityState;
    public ScriptState ScriptState;
    private ImGuiRenderer renderer;
    private RenderTarget2D canvasRT;
    private Selected selected;
    private PixxaCanvas canvas;
    private Lua luaState;

    public NewFileComponent NewFile;
    public ToolbarComponent Toolbar;
    public InspectorComponent Tools;
    public EntityPropertyComponent EntityProperty;
    public ToolMode CurrentToolMode;

    private bool scriptRunning;

    public const int Width = 544;
    public const int Height = 480;

    public MainScene(ContentManager content, Teuria.Camera camera, ImGuiRenderer renderer) : base(content, camera)
    {
        luaState = new Lua();
        luaState.LoadCLRPackage();

        luaState.DoString("function MoveX(entity, move) entity.position.X = entity.position.X + move end");
        luaState.DoString("function MoveY(entity, move) entity.position.Y = entity.position.Y + move end");
        LuaBuiltin.RegisterFunctions(luaState);

        this.renderer = renderer;
        EntityState = new EntityState(renderer);
        ScriptState = new ScriptState();
        NewFile = new NewFileComponent();
        Tools = new InspectorComponent(EntityState, ScriptState);
        Toolbar = new ToolbarComponent();
        Toolbar.ModeChanged = OnToolModeChanged;
        EntityProperty = new EntityPropertyComponent(ScriptState);
        Tools.Active = true;
        canvasRT = new RenderTarget2D(Pixxa.Instance.GraphicsDevice, Width, Height);
        Camera = camera;
    }



    public override void Hierarchy(GraphicsDevice device)
    {
        canvasRT = new RenderTarget2D(GameApp.Instance.GraphicsDevice, Width, Height);
        canvas = new PixxaCanvas(Width, Height, EntityState, canvasRT);
        Add(canvas);

        base.Hierarchy(device);
    }

    public override void AfterRender()
    {
        renderer.BeforeLayout(Pixxa.GameTime);
        DrawLayout();
        renderer.AfterLayout();
        base.AfterRender();
    }

    public override void Process()
    {
        if (Keyboard.GetState().IsKeyDown(Keys.W))
            Camera.Zoom -= new Vector2(0.1f, 0.1f);
        if (Keyboard.GetState().IsKeyDown(Keys.S))
            Camera.Zoom += new Vector2(0.1f, 0.1f);
        if (Keyboard.GetState().IsKeyDown(Keys.Left))
            Camera.X += 1f * 10f;
        if (Keyboard.GetState().IsKeyDown(Keys.Right))
            Camera.X -= 1f * 10f;
        if (Keyboard.GetState().IsKeyDown(Keys.Up))
            Camera.Y += 1f * 10f;
        if (Keyboard.GetState().IsKeyDown(Keys.Down))
            Camera.Y -= 1f * 10f;

        if (EntityState.CurrentTexture != null)
            GetSelected();
        
        base.Process();
    }

    private void OnToolModeChanged(ToolMode tool) 
    {
        switch (tool) 
        {
            case ToolMode.PencilMode:
                selected.Active = true;
                selected.Visible = true;
                break;
            case ToolMode.EditMode:
                selected.Active = false;
                selected.Visible = false;
                break;
            case ToolMode.SelectMode:
                selected.Active = false;
                selected.Visible = false;
                break;
        }
        CurrentToolMode = tool;
    }

    private void GetSelected() 
    {
        if (selected != null) 
        {
            var mouse = Mouse.GetState();
            var mousePos = new Point(mouse.Position.X - Width/2, (mouse.Position.Y - Height/2) - 150);
            if (canvas.Bounds().Contains(mousePos) 
            && TInput.Mouse.JustLeftClicked()
            && CurrentToolMode == ToolMode.PencilMode) 
            {
                selected.Spawn(selected.Position, luaState);
            }
            if (selected.Sprite.Texture != EntityState.CurrentTexture) 
            {
                selected.RenewSprite();
            }
            return;
        }
        selected = new Selected(EntityState);
        Add(selected);
    }

    private bool openEntityProperty;
    private void DrawLayout() 
    {
        {
            ImGui.Text(string.Format("Application average {0:F3} ms/frame ({1:F1} FPS)", 
                1000f / ImGui.GetIO().Framerate, ImGui.GetIO().Framerate));
        }

        ImGui.BeginMainMenuBar();
        if (ImGui.BeginMenu("File")) 
        {
            if (ImGui.MenuItem("New")) 
                NewFile.Active = true;

            ImGui.MenuItem("Open");
            ImGui.MenuItem("Save");
            ImGui.MenuItem("Save As");
            if (ImGui.MenuItem("Exit"))
                Exit();
            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Edit")) 
        {
            if (ImGui.MenuItem("Edit Entity Properties", EntityState.CurrentEntity != null)) 
            {
                EntityProperty.Open(EntityState.CurrentEntity);
                openEntityProperty = true;
            }
            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Run")) 
        {
            if (ImGui.MenuItem("Play Scripts", !scriptRunning)) 
            {
                scriptRunning = true;
                foreach (var entity in Entities) 
                {
                    if (entity is CharacterEntity characterEntity)
                        characterEntity.RunScript();
                }    
            }
            if (ImGui.MenuItem("Stop Scripts", scriptRunning)) 
            {
                scriptRunning = false;
                foreach (var entity in Entities) 
                {
                    if (entity is CharacterEntity characterEntity)
                        characterEntity.StopScript();
                }    
            }
            ImGui.EndMenu();
        }

        ImGui.EndMainMenuBar();

        if (openEntityProperty)
            ImGui.OpenPopup("Entity Property");


        if (ImGui.BeginPopupModal("Entity Property", ref openEntityProperty)) 
        {
            EntityProperty.DoLayout();
            ImGui.EndPopup();
        }

        Toolbar.DoLayout();
        Tools.DoLayout();
        NewFile.DoLayout();
    }
}
