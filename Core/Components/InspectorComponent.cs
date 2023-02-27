using System;
using System.Collections.Generic;
using System.IO;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Teuria;
using Num = System.Numerics;

namespace Pixxa;

public class InspectorComponent : Component
{
    private bool fileDialogOpen;
    private bool addCharacterOpen;
    private bool addScriptOpen;
    private bool addTerrainOpen;
    private bool renameOpen;
    private bool setAutomaticTextureSize = true;
    private int[] size = new int[2];
    private int[] pos = new int[2];
    private EntityState entityState;
    private ScriptState scriptState;
    private RoomState roomState;
    private TerrainState terrainState;
    private ImGuiRenderer renderer;
    private string entityName = "";
    private string path = "";
    private Room roomToRemove;


    public InspectorComponent(
        ImGuiRenderer renderer, 
        EntityState entityState, 
        RoomState roomState,
        TerrainState terrainState,
        ScriptState scriptState
    ) : base("InspectorComponent")
    {
        this.entityState = entityState;
        this.scriptState = scriptState;
        this.roomState = roomState;
        this.terrainState = terrainState;
        this.renderer = renderer;
    }

    public override void Layout()
    {
        ImGui.SetNextWindowPos(new Num.Vector2(0, 20), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSize(new Num.Vector2(200, 600), ImGuiCond.FirstUseEver);
        ImGuiWindowFlags flags = 
            ImGuiWindowFlags.NoScrollbar;
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 2);

        

        ImGui.Begin("Inspector", flags);

        if (ImGui.BeginTabBar("Tools")) 
        {
            if (ImGui.BeginTabItem("Entities"))
            {
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
                        if (!setAutomaticTextureSize)
                            entityState.AddTexture(entityName, path, new Point(pos[0], pos[1]), new Point(size[0], size[1]));
                        else
                            entityState.AddTexture(entityName, path);
                        entityName = "";
                        path = "";
                    }
                
                    ImGui.TreePop();
                }
                if (ImGui.Button("+ Add Category")) 
                {

                }
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Terrain")) 
            {
                foreach (var terrain in terrainState.Terrains) 
                {
                    if (ImGui.TreeNode(terrain.ID, terrain.Name)) 
                    {
                        if (ImGui.TreeNode("Image")) 
                        {
                            ImGui.InputText("Image Path", ref terrain.ImagePath, 100);
                            ImGui.Image(terrain.Bind(renderer), terrain.Size);
                            ImGui.TreePop();
                        }

                        ImGui.TreePop();
                    }
                }
                if (ImGui.Button("+ Add Terrain")) 
                {
                    addTerrainOpen = true;
                    ImGui.OpenPopup("Add Terrain");
                }
                if (AddTerrainPopUp("Add Terrain")) 
                {
                    var texture2D = EntityState.LoadTextureStream(GameApp.Instance.GraphicsDevice, path);
                    var spriteTexture = new SpriteTexture(texture2D);
                    terrainState.AddTerrain(
                        entityName, path, new Num.Vector2(texture2D.Width, texture2D.Height),
                        Tileset.LoadTileset(spriteTexture, texture2D.Width, texture2D.Height));
                    entityName = "";
                    path = "";
                }
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Scripts")) 
            {
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
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Room")) 
            {
                foreach (var room in roomState.Rooms) 
                {
                    if (ImGui.TreeNode(room.ID, room.Name)) 
                    {
                        ImGui.InputInt2("Size", ref room.Size[0]);
                        if (ImGui.BeginMenu("Terrains"))
                        {
                            foreach (var trn in terrainState.Terrains) 
                            {
                                if (ImGui.Selectable(trn.Name)) 
                                {
                                    room.Terrain = trn;
                                }
                            }
                            ImGui.EndMenu();
                        }
                        if (ImGui.TreeNode("Terrain")) 
                        {
                            if (room.Terrain != null) 
                            {
                                ImGui.InputText("Terrain Name", ref room.Terrain.Name, 15);
                                ImGui.InputText("Image Path", ref room.Terrain.ImagePath, 100);
                                ImGui.Image(room.Terrain.Bind(renderer), room.Terrain.Size);
                            }
                            ImGui.TreePop();
                        }
                        if (ImGui.Button("Rename")) 
                        {
                            renameOpen = true;
                            ImGui.OpenPopup("Rename Popup");
                        }
                        if (ImGui.Button("Delete")) 
                        {
                            deleteOpen = true;
                            ImGui.OpenPopup("Delete Popup");
                        }
                        if (AddDeletePopup("Delete Popup")) 
                        {
                            roomToRemove = room;
                        }
                        if (AddRenamePopup("Rename Popup")) 
                        {
                            room.Name = bufferName;
                        }
                        
                        ImGui.TreePop();
                    }
                }
                roomState.Rooms.Remove(roomToRemove);
                if (ImGui.Button("+ Add Room")) 
                {
                    roomState.AddRoom("Unnamed Room", null, new int[2] { 300, 300 });
                }
                ImGui.EndTabItem();
            }
            ImGui.EndTabBar();
        }

        ImGui.End();
    }

    private string bufferName = "";
    private bool deleteOpen;

    private bool AddDeletePopup(string id) 
    {
        if (ImGui.BeginPopupModal(id, ref deleteOpen)) 
        {
            ImGui.Text("Are you sure you want to delete this room?");
            if (ImGui.Button("Yes")) 
            {
                ImGui.CloseCurrentPopup();
                return true;
            }

            ImGui.SameLine();
            if (ImGui.Button("No"))  
            {
                ImGui.CloseCurrentPopup();
                return false;           
            }

            ImGui.EndPopup();
        }
        return false;
    }

    private bool AddRenamePopup(string id) 
    {
        if (ImGui.BeginPopupModal(id, ref renameOpen)) 
        {
            entityState.CurrentTexture = null;
            ImGui.InputText("Name", ref bufferName, 15);
            if (ImGui.Button("Ok")) 
            {
                ImGui.CloseCurrentPopup();
                return true;
            }
            ImGui.EndPopup();
        }
        return false;
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
                FileDialog.OpenFolderDialog("add-script", this, ref fileDialogOpen, ref path);
            } else 
            {
                if (ImGui.Button("Open Script") && !string.IsNullOrEmpty(entityName) && !string.IsNullOrEmpty(path)) 
                {
                    ImGui.CloseCurrentPopup();
                    return true;
                }
                FileDialog.OpenFileDialog("add-script", ".lua", this, ref fileDialogOpen, ref path);
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
            if (ImGui.Button("Add Character") 
            && !string.IsNullOrEmpty(entityName) 
            && !string.IsNullOrEmpty(path)) 
            {
                ImGui.CloseCurrentPopup();
                return true;
            }

            FileDialog.OpenFileDialog("add-texture", ".png", this, ref fileDialogOpen, ref path);

            ImGui.EndPopup();
        }
        return false;
    }

    private bool AddTerrainPopUp(string id) 
    {
        if (ImGui.BeginPopupModal(id, ref addTerrainOpen)) 
        {
            entityState.CurrentTexture = null;
            ImGui.InputText("Name", ref entityName, 15);
            path ??= "";
            ImGui.InputText("Path", ref path, 100);
            if (ImGui.Button("Browse")) 
            {
                fileDialogOpen = true;
                ImGui.OpenPopup("add-terrain");
            }

            if (ImGui.Button("Add Terrain")
                && !string.IsNullOrEmpty(entityName) 
                && !string.IsNullOrEmpty(path)) 
            {
                ImGui.CloseCurrentPopup();
                return true;
            }
            FileDialog.OpenFileDialog("add-terrain", ".png", this, ref fileDialogOpen, ref path);
            ImGui.EndPopup();
        }
        return false;
    }
}



