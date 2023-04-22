namespace Pixxa;

public abstract class State 
{
    public MainScene Scene;
    public State(MainScene scene) 
    {
        Scene = scene;
    }
}