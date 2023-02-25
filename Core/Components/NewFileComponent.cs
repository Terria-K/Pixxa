using System.IO;
using ImGuiNET;
using Num = System.Numerics;

namespace Pixxa;

public class NewFileComponent : Component
{
    private string title = "";
    private string path;
    private int[] size = new int[2] { 16, 16 };
    private int width;
    private int height;

    public NewFileComponent() : base("NewFileComponent") 
    {
        path = Directory.GetCurrentDirectory();
        width = size[0];
        height = size[1];
    }

    public override void Layout()
    {
        ImGui.SetNextWindowSize(new Num.Vector2(400, 100), ImGuiCond.FirstUseEver);
        ImGui.Begin("New File", ref Active, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove);
        ImGui.InputText("Title", ref title, 100);
        ImGui.InputText("Path", ref path, 100);
        ImGui.NewLine();
        if (ImGui.InputInt2("Size", ref size[0])) 
        {
            width = size[0];
            height = size[1];
        }
        ImGui.NewLine();
        ImGui.NewLine();
        ImGui.NewLine();
        ImGui.Button("Create");
        ImGui.End();
    }
}