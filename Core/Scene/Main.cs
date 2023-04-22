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
    public ProjectState ProjectState;
    public EntityState EntityState;
    public RoomState RoomState;
    public TerrainState TerrainState;
    private ImGuiRenderer renderer;
    private RenderTarget2D canvasRT;
    private PixxaCanvas canvas;
    private Selected selected;
    private MouseInput mouseInput;

    public ViewWindowComponent ViewWindow;
    public NewFileComponent NewFile;
    public ToolbarComponent Toolbar;
    public ElementComponent Element;
    public InspectorComponent Inspector;
    public ToolMode CurrentToolMode;
    public TabMode TabMode;

    public Action ImGuiCaptured;


    public MainScene(ContentManager content, Teuria.Camera camera, ImGuiRenderer renderer) : base(content, camera)
    {
        this.renderer = renderer;
        mouseInput = new MouseInput(Camera);
        ProjectState = new ProjectState(this, new List<ISaveable>());
        EntityState = new EntityState(renderer, this);
        RoomState = new RoomState(this, mouseInput);
        TerrainState = new TerrainState(this);
        NewFile = new NewFileComponent(ProjectState);
        Element = new ElementComponent(renderer, EntityState, RoomState, TerrainState) {
            OnTab = t => TabMode = t,
        };
        Toolbar = new ToolbarComponent() {
            ModeChanged = OnToolModeChanged
        };
        Inspector = new InspectorComponent();
        Element.OnEntitySelected += Inspector.OnEntitySelected;
        ViewWindow = new ViewWindowComponent(this, mouseInput);
        Element.Active = true;
        canvasRT = new RenderTarget2D(Pixxa.Instance.GraphicsDevice, Pixxa.Width, Pixxa.Height);
        canvas = new PixxaCanvas(Pixxa.Width, Pixxa.Height, EntityState, canvasRT);
        Camera = camera;
        ProjectState.AddState(EntityState);
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
            
            if (!ioPtr.WantCaptureKeyboard) 
            {
                if (TInput.Keyboard.Pressed(Keys.LeftControl) && TInput.Keyboard.JustPressed(Keys.S)) 
                {
                    ProjectState.Save();
                }
            }
            
            // Prevent entity being placed outside of a viewport
            if (ViewWindow.IsItemHovered) 
            {
                foreach (var room in RoomState.Rooms) 
                {
                    room.InputHovering(CurrentToolMode);
                    room.CouldPlaceTile();
                }

                if (EntityState.CurrentEntity != null) 
                {
                    if (selected != null) 
                    {
                        selected.InputHovering(TabMode, CurrentToolMode);
                        return;
                    }
                    selected = new Selected(EntityState, mouseInput);
                    Add(selected);
                }
            }
        };
        

        base.Hierarchy(device);
    }

    public override void BeforeRender()
    {
        RoomState.DrawMaps(Canvas.SpriteBatch);
        base.BeforeRender();
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

    private void DrawLayout() 
    {
        // {
        //     ImGui.Text(string.Format("Application average {0:F3} ms/frame ({1:F1} FPS)", 
        //         1000f / ImGui.GetIO().Framerate, ImGui.GetIO().Framerate));
        // }
        {
            ImGui.ShowDemoWindow();
        }

        ImGui.BeginMainMenuBar();
        if (ImGui.BeginMenu("File")) 
        {
            if (ImGui.MenuItem("New"))  
                NewFile.Active = true;
            

            if (ImGui.MenuItem("Open")) 
            {
                var pixxaProject = global::Pixxa.ProjectState.Open(this);
                if (pixxaProject != null)
                    ProjectState = pixxaProject;
            }
            if (ImGui.MenuItem("Save")) 
            {
                ProjectState.Save();
            }
            if (ImGui.MenuItem("Save As")) 
            {
                ProjectState.SaveAs();
            }
            if (ImGui.MenuItem("Exit")) 
            {
                Exit();
                GameApp.Instance.ExitGame();
            }

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Edit")) 
        {
            if (ImGui.MenuItem("Refresh")) 
            {
                ChangeScene(new MainScene(Content, Camera, renderer));
            }
            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Run")) 
        {
            ImGui.Text("Not yet :(");
            ImGui.EndMenu();
        }

        ImGui.EndMainMenuBar();


        Toolbar.DoLayout();
        Inspector.DoLayout();
        Element.DoLayout();
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
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(GameApp.Instance.WindowScreen.Width, GameApp.Instance.WindowScreen.Height));
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
