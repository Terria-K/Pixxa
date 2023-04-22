using System.Collections.Generic;
using TeuJson;
using TeuJson.Attributes;
using Teuria;

namespace Pixxa;


public partial class ProjectState : State, ISerialize, IDeserialize
{
    public string ProjectName { get; set; }
    public string ProjectPath { get; set; }
    public string Version { get; set; } = "1.0.0";
    [Ignore]
    public List<ISaveable> States { get; set; }

    public ProjectState(MainScene scene, List<ISaveable> states) : base(scene)
    {
        States = states;
    }

    public void AddState(ISaveable state) 
    {
        States.Add(state);
    }

    public static ProjectState Open(MainScene scene) 
    {
        var path = RfdSharp.RfdSharp.OpenFileWithFilter("/", new string[1] { "pix" });
        if (string.IsNullOrEmpty(path))
            return null;
        
        var projectState = new ProjectState(scene, scene.ProjectState.States);
        var obj = JsonTextReader.FromFile(path);
        projectState.Deserialize(obj.AsJsonObject);
        return projectState;
    }

    public void Save() 
    {
        if (string.IsNullOrEmpty(ProjectPath)) 
        {
            ProjectPath = RfdSharp.RfdSharp.SaveFileWithFilter("/", new string[1] { "pix" });
            if (string.IsNullOrEmpty(ProjectPath))
                return;
        }
        InternalSave();
    }

    public void SaveAs() 
    {
        ProjectPath = RfdSharp.RfdSharp.SaveFileWithFilter("/", new string[1] { "pix" });
        if (string.IsNullOrEmpty(ProjectPath))
            return;
        InternalSave();
    }

    private void InternalSave() 
    {
        var serialized = JsonConvert.Serialize(this);
        var array = new JsonArray();
        foreach (var state in States) 
        {
            array.Add(state.Save());
        }
        serialized["States"] = array;
        JsonTextWriter.WriteToFile(ProjectPath, serialized);
    }
}