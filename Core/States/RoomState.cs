using System.Collections.Generic;

namespace Pixxa;

public class RoomState 
{
    private static int roomIDBuffer;
    public List<Room> Rooms = new List<Room>();

    public void AddRoom(string name, Terrain terrain, int[] size) 
    {
        Rooms.Add(new Room() 
        {
            ID = roomIDBuffer++,
            Name = name,
            Size = size,
            Terrain = terrain
        });
    }
}

public class Room 
{
    public required nint ID;
    public string Name;
    public int[] Size;
    public Terrain Terrain;
}