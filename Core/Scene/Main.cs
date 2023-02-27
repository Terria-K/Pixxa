using System.Collections.Generic;
using Num = System.Numerics;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NLua;
using Teuria;
using System;

namespace Pixxa;

public partial class MainScene : Scene
{
    public EntityState EntityState;
    public ScriptState ScriptState;
    public RoomState RoomState;
    public TerrainState TerrainState;
    private ImGuiRenderer renderer;
    private RenderTarget2D canvasRT;
    private PixxaCanvas canvas;
    private Lua luaState;
    private Selected selected;

    public ViewWindowComponent ViewWindow;
    public NewFileComponent NewFile;
    public ToolbarComponent Toolbar;
    public InspectorComponent Inspector;
    public EntityPropertyComponent EntityProperty;
    public ToolMode CurrentToolMode;

    public Num.Vector2 ViewportSize;
    public Num.Vector2 ViewportPos;
    public Action ImGuiCaptured;

    private bool scriptRunning;

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
        RoomState = new RoomState();
        TerrainState = new TerrainState();
        NewFile = new NewFileComponent();
        Inspector = new InspectorComponent(renderer, EntityState, RoomState, TerrainState, ScriptState);
        Toolbar = new ToolbarComponent();
        Toolbar.ModeChanged = OnToolModeChanged;
        EntityProperty = new EntityPropertyComponent(EntityState, ScriptState);
        ViewWindow = new ViewWindowComponent(this);
        Inspector.Active = true;
        canvasRT = new RenderTarget2D(Pixxa.Instance.GraphicsDevice, Pixxa.Width, Pixxa.Height);
        canvas = new PixxaCanvas(Pixxa.Width, Pixxa.Height, EntityState, canvasRT);
        Camera = camera;
    }

    public override void Hierarchy(GraphicsDevice device)
    {
        Add(canvas);
        ImGuiIOPtr ioPtr = ImGui.GetIO();
        ioPtr.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        ImGuiCaptured = () => 
        {
            if (!ioPtr.WantCaptureMouse)
                ImGui.SetWindowFocus(null);
            
            // Prevent entity being placed outside of a viewport
            if (ViewWindow.IsItemHovered && EntityState.CurrentEntity != null) 
            {
                if (selected != null) 
                {
                    selected?.InputHovering(CurrentToolMode, luaState);
                    return;
                }
                selected = new Selected(EntityState);
                Add(selected);
            }
        };
        

        base.Hierarchy(device);
    }

    public override void AfterRender()
    {
        renderer.BeforeLayout(Pixxa.GameTime);
        SetupDockspace();
        DrawLayout();
        ViewWindow.DoLayout();
        ImGui.End();
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

        ImGuiCaptured();
        
        base.Process();
    }

    private void OnToolModeChanged(ToolMode tool) 
    {
        switch (tool) 
        {
            case ToolMode.PencilMode:
                if (selected == null)
                    break;
                selected.Active = true;
                selected.Visible = true;
                break;
            case ToolMode.EditMode:
                if (selected == null)
                    break;
                selected.Active = false;
                selected.Visible = false;
                break;
            case ToolMode.SelectMode:
                if (selected == null)
                    break;
                selected.Active = false;
                selected.Visible = false;
                break;
        }
        CurrentToolMode = tool;
    }

    private bool openEntityProperty;
    private void DrawLayout() 
    {
        // {
        //     ImGui.Text(string.Format("Application average {0:F3} ms/frame ({1:F1} FPS)", 
        //         1000f / ImGui.GetIO().Framerate, ImGui.GetIO().Framerate));
        // }

        ImGui.BeginMainMenuBar();
        if (ImGui.BeginMenu("File")) 
        {
            if (ImGui.MenuItem("New"))  
                NewFile.Active = true;
            

            ImGui.MenuItem("Open");
            ImGui.MenuItem("Save");
            ImGui.MenuItem("Save As");
            if (ImGui.MenuItem("Exit")) 
            {
                Exit();
                GameApp.Instance.ExitGame();
            }

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Edit")) 
        {
            if (ImGui.MenuItem("Edit Entity Properties", EntityState.CurrentEntity != null)) 
            {
                if (EntityState.CurrentEntity == null)
                    return;
                EntityProperty.Open(EntityState);
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
        Inspector.DoLayout();
        if (NewFile.Active)
            ImGui.OpenPopup("New File");
        if (ImGui.BeginPopupModal("New File", ref NewFile.Active)) 
        {
            NewFile.DoLayout();
            ImGui.EndPopup();
        }
    }

    private void SetupDockspace() 
    {
        var windowFlags = 
            ImGuiWindowFlags.MenuBar
            | ImGuiWindowFlags.NoDocking;
        
        ImGui.SetNextWindowPos(System.Numerics.Vector2.Zero, ImGuiCond.Always);
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(1024, 640));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
        windowFlags |= ImGuiWindowFlags.NoTitleBar 
            | ImGuiWindowFlags.NoCollapse 
            | ImGuiWindowFlags.NoResize
            | ImGuiWindowFlags.NoMove
            | ImGuiWindowFlags.NoBringToFrontOnFocus
            | ImGuiWindowFlags.NoNavFocus;

        bool dockSpaceTrue = true;
        ImGui.Begin("Dockspace", ref dockSpaceTrue, windowFlags); 
        ImGui.PopStyleVar(2);

        // Dockspace
        ImGuiIOPtr ioPtr = ImGui.GetIO();

        if ((ioPtr.ConfigFlags & ImGuiConfigFlags.DockingEnable) != 0) 
        {
            var dockspaceID = ImGui.GetID("MyDockSpace");
            ImGui.DockSpace(dockspaceID, System.Numerics.Vector2.Zero);
        }
    }

    public nint RenderTargetID() 
    {
        return renderer.BindTexture(canvasRT);
    }
}
