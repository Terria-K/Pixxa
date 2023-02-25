using System.Collections.Generic;

namespace Pixxa;

public class ScriptState 
{
    public List<ScriptReference> Scripts = new List<ScriptReference>();

    public void AddScript(string name, string path) 
    {
        Scripts.Add(new ScriptReference() { Name = name, Path = path});
    }
}

public class ScriptReference 
{
    public required string Name; 
    public required string Path;
}