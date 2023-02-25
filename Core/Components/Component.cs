using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Pixxa;

public abstract class Component 
{

    public Component(string name) 
    {
        Name = name;
    }
    public string Name;

    public Dictionary<string, Component> Childs = new Dictionary<string, Component>();
    public Component Parent;
    public bool Active;
    public virtual void Layout() {}
    public virtual void Update() {}
    public virtual void Render(SpriteBatch spriteBatch) {}

    public void AddChild(Component component) 
    {
        var name = component.Name;
        var i = 0;
        while (Childs.ContainsKey(name)) 
        {
            i++;
            name += i;
        }
        Childs.Add(name, component);
        component.Parent = this;
    }

    public void RemoveChild(string name) 
    {
        Childs.Remove(name);
    }

    public void DoUpdate() 
    {
        if (!Active) return;
        Update();
        foreach (var child in Childs) 
        {
            if (!child.Value.Active) continue;
            child.Value.DoUpdate();
        }
    }

    public void DoLayout() 
    {
        if (!Active) return;
        Layout();
        foreach (var child in Childs) 
        {
            if (!child.Value.Active) continue;
            child.Value.DoLayout();
        }
    }

    public void DoRender(SpriteBatch spriteBatch) 
    {
        if (!Active) return;
        Render(spriteBatch);
        foreach (var child in Childs) 
        {
            if (!child.Value.Active) continue;
            child.Value.DoRender(spriteBatch);
        }
    }
}