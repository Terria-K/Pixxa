using System.Numerics;
using ImGuiNET;

namespace Pixxa;

public class ViewWindowComponent : Component
{
    private MouseInput mouseInput;
    private MainScene main;
    public bool IsItemHovered;
    public ViewWindowComponent(MainScene main, MouseInput mouseInput) : base("ViewWindow")
    {
        this.mouseInput = mouseInput;
        this.main = main;
        Active = true;
    }

    public override void Layout()
    {
        ImGui.Begin("Editor Viewport", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

        Vector2 windowSize = GetLargestSizeViewport();
        Vector2 windowPos = GetCenteredViewportCenter(windowSize);

        ImGui.SetCursorPos(windowPos);

        Vector2 topLeft = ImGui.GetCursorScreenPos();
        topLeft.X -= ImGui.GetScrollX();
        topLeft.Y -= ImGui.GetScrollY();

        nint texID = main.RenderTargetID();
        ImGui.Image(texID, windowSize);
        IsItemHovered = ImGui.IsItemHovered();
        
        mouseInput.ViewportPos = topLeft;
        mouseInput.ViewportSize = windowSize;

        ImGui.End();
    }

    private Vector2 GetLargestSizeViewport() 
    {
        Vector2 windowSize = GetWindowSize();

        float aspectWidth = windowSize.X;
        float aspectHeight = aspectWidth / Pixxa.AppAspectRatio;

        if (aspectHeight > windowSize.Y) 
        {
            aspectHeight = windowSize.Y;
            aspectWidth = aspectHeight * Pixxa.AppAspectRatio;
        }
        return new Vector2(aspectWidth, aspectHeight);
    }

    private Vector2 GetCenteredViewportCenter(Vector2 aspectSize) 
    {
        var windowSize = GetWindowSize();

        var viewportX = (windowSize.X / 2.0f) - (aspectSize.X / 2.0f);
        var viewportY = (windowSize.Y / 2.0f) - (aspectSize.Y / 2.0f);

        return new Vector2(
            viewportX + ImGui.GetCursorPosX(), 
            viewportY + ImGui.GetCursorPosY());
    }

    private static Vector2 GetWindowSize() 
    {
        var windowSize = ImGui.GetContentRegionAvail();
        windowSize.X -= ImGui.GetScrollX();
        windowSize.Y -= ImGui.GetScrollY();
        return windowSize;
    }
}