using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Teuria;

namespace Pixxa;

public class RoomState : State
{
    private static int roomIDBuffer;
    public List<Room> Rooms = new List<Room>();
    private List<TileMap> maps = new List<TileMap>();
    private MouseInput mouseInput;

    public RoomState(MainScene scene, MouseInput mouseInput) : base(scene)
    {
        this.mouseInput = mouseInput;
    }

    public void DrawMaps(SpriteBatch spriteBatch) 
    {
        foreach (var map in maps) 
        {
            map.BeforeRender(spriteBatch);
        }
    }

    public void AddRoom(string name) 
    {
        var room = new Room() 
        {
            ID = roomIDBuffer++,
            RoomName = name,
            RegionPosPtr = new int[2] { 0, 0 },
            GridSizePtr = new int[2] { 8, 8 },
            RoomSizePtr = new int[2] { 10, 10 },
            MouseInput = mouseInput
        };
        var map = new TileMap(10, 10, 8, 8);
        map.AddGridLayer("Solid", Tileset.LoadTileset(
            "images/tile/tile.json", SpriteTexture.FromFile("Content/images/tile/tile.png")));
        room.Tile = map;
        room.ApplyChanges();
        
        Rooms.Add(room);
        maps.Add(map);
        AddRoomToWorld(room, map);
    }

    public void RemoveRoom(Room room) 
    {
        if (room == null)
            return;
        var index = Rooms.IndexOf(room);
        var map = maps[index];
        Rooms.Remove(room);
        maps.Remove(map);
        DeleteRoom(room, map);
    }

    public void AddRoomToWorld(Room room, TileMap map) 
    {
        Scene.Add(room);
    }

    public void DeleteRoom(Room room, TileMap map) 
    {
        Scene.Remove(room);
    }
}

public class Room : Entity
{
    public int ID;
    public string RoomName;
    public int[] RegionPosPtr;
    public int[] GridSizePtr;
    public int[] RoomSizePtr;
    public int TerrainID;
    public List<string> Layers = new List<string>();
    public string ActiveLayer;


    public Terrain Terrain;
    public MouseInput MouseInput;
    public TileMap Tile;
    private bool hovering;

    private Point regionPos;
    private Point gridSize;
    private Point roomSize;

    public Point RegionPos => regionPos;
    public Point GridSize => gridSize;
    public Point RoomSize => roomSize;
    public Point Size => new Point(roomSize.X * gridSize.X, roomSize.Y * gridSize.Y);


    public Rectangle Bounds => new Rectangle(
        regionPos.X, regionPos.Y, 
        roomSize.X * gridSize.X, roomSize.Y * gridSize.Y
    );

    public void ChangeTerrain(Terrain terrain) 
    {
        Terrain = terrain;
        TerrainID = terrain.ID;
    }

    public void AddLayer(string layer) 
    {
        Layers.Add(layer);
    }

    public void RemoveLayer(string layer) 
    {
        Layers.Remove(layer);
    }

    public void ApplyChanges() 
    {
        regionPos = new Point(RegionPosPtr[0], RegionPosPtr[1]);
        gridSize = new Point(GridSizePtr[0], GridSizePtr[1]);
        roomSize = new Point(RoomSizePtr[0], RoomSizePtr[1]);
        Tile.Position = new Vector2(regionPos.X, regionPos.Y);
        Tile.Width = roomSize.X;
        Tile.Height = roomSize.Y;
    }

    public void CouldPlaceTile() 
    {
        if (!hovering)
            return;
        var mousePos = MouseInput.GetCanvasMousePosition();
        if (TInput.Mouse.LeftClicked()) 
        {
            Tile.SetTile("Solid", new Point(
                ((int)(mousePos.X - regionPos.X) / 8), 
                ((int)(mousePos.Y - regionPos.Y) / 8)), "2"
            );
        }
        if (TInput.Mouse.RightClicked()) 
        {
            Tile.SetTile("Solid", new Point(
                ((int)(mousePos.X - regionPos.X) / 8), 
                ((int)(mousePos.Y - regionPos.Y) / 8)), "0"
            );
        }
    }

    public void InputHovering(ToolMode toolMode) 
    {
        if (toolMode != ToolMode.SelectMode)
            return;

        var mousePos = MouseInput.GetCanvasMousePosition();
        if (Bounds.Contains(mousePos)) 
            hovering = true;
        else hovering = false;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Tile.Draw(spriteBatch);
        base.Draw(spriteBatch);
        var color = hovering ? Color.Yellow : Color.Black;

        Canvas.DrawRect(
            spriteBatch, 
            RegionPosPtr[0], RegionPosPtr[1], 
            RoomSizePtr[0] * GridSizePtr[0], 
            RoomSizePtr[1] * GridSizePtr[1], 2, Color.Gray * 0.8f);

        Canvas.DrawRect(
            spriteBatch, 
            regionPos.X, regionPos.Y, 
            roomSize.X * gridSize.X, 
            roomSize.Y * gridSize.Y, 2, color);
    }

    public ref int GetTotalRoomSize() 
    {
        int[] roomSize = new int[2] { RoomSizePtr[0] * GridSizePtr[0], RoomSizePtr[1] * GridSizePtr[1] };
        return ref roomSize[0];
    }
}