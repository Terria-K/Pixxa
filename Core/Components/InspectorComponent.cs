using System;
using System.IO;
using ImGuiNET;
using Num = System.Numerics;

namespace Pixxa;

public class InspectorComponent : Component
{
    private bool fileDialogOpen;
    private bool addCharacterOpen;
    private bool addScriptOpen;
    private bool setAutomaticTextureSize = true;
    private int[] size = new int[2];
    private int[] pos = new int[2];
    private EntityState entityState;
    private ScriptState scriptState;
    private string entityName = "";
    private string path = "";

    public InspectorComponent(EntityState entityState, ScriptState scriptState) : base("InspectorComponent")
    {
        this.entityState = entityState;
        this.scriptState = scriptState;
    }

    public override void Layout()
    {
        var viewportPtr = ImGui.GetMainViewport();
        ImGui.SetNextWindowPos(new Num.Vector2(viewportPtr.Pos.X, viewportPtr.Pos.Y + 20));
        ImGui.SetNextWindowSize(new Num.Vector2(viewportPtr.Pos.X + 200, 640));
        ImGui.SetNextWindowViewport(viewportPtr.ID);
        ImGuiWindowFlags flags = 
            ImGuiWindowFlags.NoDocking
            | ImGuiWindowFlags.NoTitleBar
            | ImGuiWindowFlags.NoResize
            | ImGuiWindowFlags.NoMove
            | ImGuiWindowFlags.NoScrollbar
            | ImGuiWindowFlags.NoSavedSettings;
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 2);
        ImGui.Begin("Toolbar", ref Active, flags);

        ImGui.BeginChild("Child", new Num.Vector2(0, 200), true);
        ImGui.Text("Entities");
        if (ImGui.TreeNode("Character")) 
        {
            foreach (var characters in entityState.TexturesPtr) 
            {
                if (ImGui.Selectable(characters.Name)) 
                {
                    entityState.SetTexture(characters.Texture);
                    entityState.SetEntity(characters);
                }
            }
            if (ImGui.Button("+ Add Character")) 
            {
                addCharacterOpen = true;

                ImGui.OpenPopup("Add Character");
            }

            if (AddCharacterPopup("Add Character")) 
            {
                entityState.AddTexture(entityName, path);
                entityName = "";
                path = "";
            }
        
            ImGui.TreePop();
        }

        ImGui.EndChild();

        ImGui.BeginChild("Child2", new Num.Vector2(0, 200), true);
        ImGui.Text("Terrain");
        ImGui.Button("+ Add Terrain");
        ImGui.EndChild();

        ImGui.BeginChild("Child3", new Num.Vector2(0, 200), true);
        ImGui.Text("Scripts");
        if (ImGui.Button("+ Add Scripts")) 
        {
            addScriptOpen = true;
            ImGui.OpenPopup("Add Script");
        }
        if (ImGui.Button("+ Open Scripts")) 
        {
            addScriptOpen = true;
            ImGui.OpenPopup("Open Script");
        }
        foreach (var script in scriptState.Scripts) 
        {
            ImGui.Text(script.Name);
            ImGui.SameLine();
            ImGui.Button("X");
        }
        AddScriptPopup("Add Script");
        if (AddScriptPopup("Open Script")) 
        {
            scriptState.AddScript(entityName, path);
            entityName = "";
            path = "";
        }
        ImGui.EndChild();
        ImGui.End();
    }

    private bool AddScriptPopup(string id) 
    {
        if (ImGui.BeginPopupModal(id, ref addScriptOpen)) 
        {
            entityState.CurrentTexture = null;
            ImGui.InputText("Name", ref entityName, 15);
            ImGui.InputText("Path", ref path, 100);
            if (ImGui.Button("Browse")) 
            {
                fileDialogOpen = true;
                ImGui.OpenPopup("add-script");
            }
            if (id == "Add Script") 
            {
                ImGui.Button("Create Script");
                OpenFolderDialog("add-script");
            } else 
            {
                if (ImGui.Button("Open Script") && !string.IsNullOrEmpty(entityName) && !string.IsNullOrEmpty(path)) 
                {
                    ImGui.CloseCurrentPopup();
                    return true;
                }
                OpenFileDialog("add-script", ".lua");
            }


            ImGui.EndPopup();
        }
        return false;
    }

    private bool AddCharacterPopup(string id) 
    {
        if (ImGui.BeginPopupModal(id, ref addCharacterOpen)) 
        {
            entityState.CurrentTexture = null;
            ImGui.InputText("Name", ref entityName, 15);
            if (!ImGui.Checkbox("Automatically set texture size", ref setAutomaticTextureSize)) 
            {
                if (!setAutomaticTextureSize) 
                {
                    ImGui.InputInt2("Position", ref pos[0]);
                    ImGui.InputInt2("Size", ref size[0]);
                }
                ImGui.NewLine();
            }
            path ??= "";
            ImGui.InputText("Path", ref path, 100);
            if (ImGui.Button("Browse")) 
            {
                fileDialogOpen = true;
                ImGui.OpenPopup("add-texture");
            }
            if (ImGui.Button("Add Character") && !string.IsNullOrEmpty(entityName) && !string.IsNullOrEmpty(path)) 
            {
                ImGui.CloseCurrentPopup();
                return true;
            }

            OpenFileDialog("add-texture", ".png");

            ImGui.EndPopup();
        }
        return false;
    }

    private void OpenFileDialog(string id, string filter) 
    {
        if (ImGui.BeginPopupModal(id, ref fileDialogOpen, ImGuiWindowFlags.NoTitleBar)) 
        {
            var picker = FilePicker.GetFilePicker(this, Path.Combine(Environment.CurrentDirectory), filter);
            if (picker.Draw()) 
            {
                path = picker.SelectedFile;
                FilePicker.RemoveFilePicker(this);
            }
            ImGui.EndPopup();
        }
    }

    private void OpenFolderDialog(string id) 
    {
        if (ImGui.BeginPopupModal(id, ref fileDialogOpen, ImGuiWindowFlags.NoTitleBar)) 
        {
            var picker = FilePicker.GetFolderPicker(this, Path.Combine(Environment.CurrentDirectory));
            if (picker.Draw()) 
            {
                path = picker.SelectedFile;
                FilePicker.RemoveFilePicker(this);
            }
            ImGui.EndPopup();
        }
    }
}