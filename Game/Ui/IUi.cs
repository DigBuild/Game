using DigBuild.Engine.Render;
using DigBuild.Platform.Input;
using DigBuild.Platform.Render;

namespace DigBuild.Ui
{
    public interface IUi
    {
        CursorMode CursorMode { get; }
        bool Pause { get; }

        void OnOpened(UiManager manager);
        void OnResized(IRenderTarget target);
        void OnClosed();

        void UpdateAndDraw(RenderContext context, IGeometryBuffer buffer, float partialTick);

        bool OnCursorMoved(int x, int y);
        bool OnMouseEvent(uint button, MouseAction action);
        bool OnScrollEvent(double xOffset, double yOffset);
        bool OnKeyboardEvent(uint code, KeyboardAction action);

        void OnLayerAdded();
        void OnLayerRemoved();
    }
}