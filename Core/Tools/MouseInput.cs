using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Num = System.Numerics;

namespace Pixxa;

public class MouseInput 
{
    public Num.Vector2 ViewportSize;
    public Num.Vector2 ViewportPos;
    private Teuria.Camera camera;

    public MouseInput(Teuria.Camera camera) 
    {
        this.camera = camera;
    }

    public Vector2 GetViewportMousePosition() 
    {
        var mouseState = Mouse.GetState();
        float currentX = mouseState.X - ViewportPos.X;
        float currentY = mouseState.Y - ViewportPos.Y;

        var mousePosX = (currentX / ViewportSize.X) * Pixxa.Width;
        var mousePosY = (currentY / ViewportSize.Y) * Pixxa.Height;

        return new Vector2(mousePosX, mousePosY);
    }

    public Vector2 GetCanvasMousePosition() => CameraInverse(GetViewportMousePosition());

    public Vector2 CameraInverse(Vector2 position) 
    {
        return Vector2.Transform(
            new Vector2(position.X, position.Y), 
            Matrix.Invert(camera.Transform)
        );
    }
}