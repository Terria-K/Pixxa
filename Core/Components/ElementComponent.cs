using System;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Teuria;
using Num = System.Numerics;

namespace Pixxa;

public enum TabMode 
{
    Entity,
    Terrain,
    Room
}

public class ElementComponent : Component
{
    public Action<TabMode> OnTab;
    public Action<PixxaEntity> OnEntitySelected;
    private bool fileDialogOpen;
    private bool addCharacterOpen;
    private bool addTerrainOpen;
    private bool renameOpen;
    private bool setAutomaticTextureSize = true;
    private int[] size = new int[2];
    private int[] pos = new int[2];
    private EntityState entityState;
    private RoomState roomState;
    private TerrainState terrainState;
    private ImGuiRenderer renderer;
    private string entityName = "";
    private string path = "";
    private string addLayer = "";
    private TabMode tabMode;
    private Room roomToRemove;
    private PixxaEntity entityToRemove;


    public ElementComponent(
        ImGuiRenderer renderer, 
        EntityState entityState, 
        RoomState roomState,
        TerrainState terrainState
    ) : base("ElementComponent")
    {
        this.entityState = entityState;
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

        ImGui.Begin("Elements", flags);
        ImGui.PopStyleVar(2);
        if (ImGui.BeginTabBar("Tools")) 
        {
            if (ImGui.BeginTabItem("Entities"))
            {
                if (tabMode != TabMode.Entity) 
                {
                    tabMode = TabMode.Entity;
                    OnTab?.Invoke(tabMode);
                }

                if (ImGui.TreeNode("Character")) 
                {
                    foreach (var characters in entityState.TexturesPtr) 
                    {
                        if (ImGui.Selectable(characters.Name)) 
                        {
                            entityState.SetTexture(characters.Texture);
                            entityState.SetEntity(characters);
                            OnEntitySelected?.Invoke(characters);
                        }
                        ImGui.PushID(characters.Name);
                        if (ImGui.BeginPopupContextItem()) 
                        {
                            if (ImGui.Selectable("Delete")) 
                            {
                                entityToRemove = characters;
                            }
                            ImGui.EndPopup();
                        }
                        ImGui.PopID();
                    }
                    entityState.RemoveEntity(entityToRemove);
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
                if (tabMode != TabMode.Terrain) 
                {
                    tabMode = TabMode.Terrain;
                    OnTab?.Invoke(tabMode);
                }
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

            if (ImGui.BeginTabItem("Room")) 
            {
                if (tabMode != TabMode.Room) 
                {
                    tabMode = TabMode.Room;
                    OnTab?.Invoke(tabMode);
                }
                foreach (var room in roomState.Rooms) 
                {
                    if (ImGui.TreeNode(room.ID, room.RoomName)) 
                    {
                        ImGui.InputInt2("Position", ref room.RegionPosPtr[0]);
                        ImGui.InputInt2("Grid Size", ref room.GridSizePtr[0]);
                        ImGui.InputInt2("Room Size", ref room.RoomSizePtr[0]);
                        ImGui.InputInt2("Size", ref room.GetTotalRoomSize(), ImGuiInputTextFlags.ReadOnly);
                        if (ImGui.Button("Apply Changes"))
                            room.ApplyChanges();
                        
                        var layerPreviewText = room.ActiveLayer != null ? room.ActiveLayer : "No Layer Selected";

                        if (ImGui.BeginCombo("Layers", layerPreviewText)) 
                        {
                            if (room.Layers.Count <= 0) 
                            {
                                ImGui.Text("No Layers are added in the project.");
                            }
                            else
                            foreach (var layer in room.Layers) 
                            {
                                if (ImGui.Selectable(layer)) 
                                {
                                    room.ActiveLayer = layer;
                                }
                            }                           

                            ImGui.EndCombo();
                        }

                        ImGui.InputText("Layer", ref addLayer, 100);
                        if (ImGui.Button("+ Add")) 
                        {
                            room.AddLayer(addLayer);
                            addLayer = string.Empty;
                        }

                        var terrainPreviewText = room.Terrain != null ? room.Terrain.Name : "No Terrain Selected";
                        if (ImGui.BeginCombo("Terrain", terrainPreviewText)) 
                        {
                            foreach (var trn in terrainState.Terrains) 
                            {
                                if (ImGui.Selectable(trn.Name)) 
                                {
                                    room.ChangeTerrain(trn);
                                }
                            }                           
                            ImGui.EndCombo();
                        }
                        if (ImGui.Button("Rename")) 
                        {
                            renameOpen = true;
                            ImGui.OpenPopup("Rename");
                        }
                        ImGui.SameLine();
                        if (ImGui.Button("Delete")) 
                        {
                            deleteOpen = true;
                            ImGui.OpenPopup("Delete");
                        }
                        if (AddDeletePopup("Delete")) 
                        {
                            roomToRemove = room;
                        }
                        if (AddRenamePopup("Rename")) 
                        {
                            room.RoomName = bufferName;
                        }
                        ImGui.TreePop();
                    }
                }
                roomState.RemoveRoom(roomToRemove);
                if (ImGui.Button("+ Add Room")) 
                {
                    roomState.AddRoom("Unnamed Room");
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



