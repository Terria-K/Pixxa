using Teuria;

namespace Pixxa;

public static class PhysicsTags 
{
    public static Tag AsSolid;
    public static Tag UI;
    
    public static void Init() 
    {
        AsSolid = new Tag("AsSolid");
        UI = new Tag("UI");
    }
}