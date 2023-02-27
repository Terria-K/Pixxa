using System.Collections.Generic;
using Teuria;

namespace Pixxa;

public class TerrainState 
{
    private static int terrainBufferID;
    public List<Terrain> Terrains = new List<Terrain>();

    public void AddTerrain(string name, string imagePath, System.Numerics.Vector2 size, Tileset tileset)
    {
        Terrains.Add(new Terrain() 
        {
            ID = terrainBufferID++,
            Name = name,
            Tileset = tileset,
            ImagePath = imagePath,
            Size = size
        });
    }
}

public class Terrain
{
    public required nint ID;
    public string Name;
    public string ImagePath;
    public System.Numerics.Vector2 Size;
    public Tileset Tileset;

    public nint Bind(ImGuiRenderer renderer) 
    {
        return renderer.BindTexture(Tileset.TilesetAtlas.Texture.Texture);
    }
}