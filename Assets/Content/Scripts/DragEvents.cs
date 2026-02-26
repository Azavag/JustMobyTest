using System;

public static class DragEvents
{
    public static Action<CubeController> OnDragStarted;
    public static Action<CubeController> OnDragEnded;

    public static void RaiseDragStarted(CubeController cube)
    {
        OnDragStarted?.Invoke(cube);
    }

    public static void RaiseDragEnded(CubeController cube)
    {
        OnDragEnded?.Invoke(cube);
    }
}