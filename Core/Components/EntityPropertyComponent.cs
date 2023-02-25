using ImGuiNET;

namespace Pixxa;

public class EntityPropertyComponent : Component
{
    private EntityTexture targetEntity;
    private ScriptState scriptState;
    public EntityPropertyComponent(ScriptState scriptState) : base("EntityPropertyComponent") 
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
    }

    public void Open(EntityTexture entity) 
    {
        targetEntity = entity;

    }

    public void Close() 
    {
        Active = false;
        targetEntity = null;
    }
}