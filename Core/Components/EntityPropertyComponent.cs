using ImGuiNET;

namespace Pixxa;

public class EntityPropertyComponent : Component
{
    private EntityTexture targetEntity;
    private ScriptState scriptState;
    private string path;
    private bool fileDialogOpen;
    public EntityPropertyComponent(EntityState entityState, ScriptState scriptState) : base("EntityPropertyComponent") 
    { 
        Active = true; 
        this.scriptState = scriptState;
    }

    public override void Layout()
    {
        ImGui.InputText("Entity Name", ref targetEntity.Name, 15);
        ImGui.InputText("Script Name", ref targetEntity.ScriptName, 15);
        if (ImGui.BeginMenu("Scripts"))
        {
            foreach (var scripts in scriptState.Scripts) 
            {
                if (ImGui.Selectable(scripts.Name)) 
                {
                    targetEntity.ScriptName = scripts.Name;
                    targetEntity.ScriptPath = scripts.Path;
                }
            }
            ImGui.EndMenu();
        }
        ImGui.InputText("Image Path", ref path, 100);
        if (ImGui.Button("Browse")) 
        {
            fileDialogOpen = true;
            ImGui.OpenPopup("edit-texture");
        }
// TODO Ability to edit the texture
        FileDialog.OpenFileDialog("edit-texture", ".png", this, ref fileDialogOpen, ref path);
        if (ImGui.Button("Apply")) 
        {
            
        }
    }

    public void Open(EntityState entity) 
    {
        entity.CurrentTexture = null;
        targetEntity = entity.CurrentEntity;
        path = targetEntity.ImagePath;
    }

    public void Close() 
    {
        Active = false;
        targetEntity = null;
    }
}