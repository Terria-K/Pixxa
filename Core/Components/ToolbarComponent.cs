using System;
using ImGuiNET;
using Num = System.Numerics;

namespace Pixxa;

public class ToolbarComponent : Component
{
    public Action<ToolMode> ModeChanged;
    public ToolbarComponent() : base("ToolbarComponent")
    {
        Active = true;
    }

    public override void Layout()
    {
        ImGui.SetNextWindowSize(new Num.Vector2(400, 80));
        ImGui.Begin("Toolbars", ImGuiWindowFlags.NoResize);
        if (ImGui.Button("Pencil")) 
            ModeChanged?.Invoke(ToolMode.PencilMode);
        
        ImGui.SameLine();
        if (ImGui.Button("Edit"))
            ModeChanged?.Invoke(ToolMode.EditMode);
        ImGui.SameLine();
        if (ImGui.Button("Select"))
            ModeChanged?.Invoke(ToolMode.SelectMode);
        ImGui.End();
    }
}


public enum ToolMode 
{
    PencilMode,
    EditMode,
    SelectMode
}