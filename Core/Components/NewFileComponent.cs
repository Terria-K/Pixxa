using System.IO;
using ImGuiNET;
using Num = System.Numerics;

namespace Pixxa;

public class NewFileComponent : Component
{
    private string title = "";
    private string path = "";
    private int[] size = new int[2] { 16, 16 };
    private int width;
    private int height;
    private ProjectState projectState;

    public NewFileComponent(ProjectState projectState) : base("NewFileComponent") 
    {
        this.projectState = projectState;
        path = Directory.GetCurrentDirectory();
        width = size[0];
        height = size[1];
    }

    public override void Layout()
    {
        ImGui.SetNextWindowSize(new Num.Vector2(400, 100), ImGuiCond.FirstUseEver);
        ImGui.InputText("Title", ref title, 100);
        ImGui.InputText("Path", ref path, 100);
        if (ImGui.Button("Browse")) 
        {
            var result = RfdSharp.RfdSharp.SaveFileWithFilter("/", new string[1] { "pix" });
            if (result != null)
                path = result;
        }
        
        ImGui.NewLine();
        if (ImGui.InputInt2("Size", ref size[0])) 
        {
            width = size[0];
            height = size[1];
        }
        ImGui.NewLine();
        ImGui.NewLine();
        ImGui.NewLine();
        if (ImGui.Button("Create")) 
        {
            projectState.ProjectName = title;
            if (!path.EndsWith(".pix"))
                projectState.ProjectPath = Path.Combine(path, title + ".pix");
            else
                projectState.ProjectPath = path;
            projectState.Save();
            Active = false;
        }
        ImGui.End();
    }
}