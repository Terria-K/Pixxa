using ImGuiNET;
using Num = System.Numerics;

namespace Pixxa;

public sealed class InspectorComponent : Component
{
    public PixxaEntity ActiveEntity;

    public InspectorComponent() : base("InspectorComponent")
    {
        Active = true;
    }

    public override void Layout()
    {
        ImGui.SetNextWindowPos(new Num.Vector2(0, 20), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSize(new Num.Vector2(200, 600), ImGuiCond.FirstUseEver);

        ImGui.Begin("Inspector");
        if (ActiveEntity == null)
        {
            ImGui.End();
            return;
        }
        ImGui.Image(ActiveEntity.TexturePtr, new Num.Vector2(ActiveEntity.Size.X, ActiveEntity.Size.Y));
        ImGui.InputText("Name", ref ActiveEntity.Name, 20);
        ImGui.InputInt("Width", ref ActiveEntity.Size.X);
        ImGui.InputInt("Height", ref ActiveEntity.Size.Y);
        ImGui.End();
    }

    public void OnEntitySelected(PixxaEntity entity) 
    {
        ActiveEntity = entity;
    }
}