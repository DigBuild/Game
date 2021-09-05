using DigBuild.Engine.Render;
using DigBuild.Platform.Input;
using DigBuild.Platform.Render;

namespace DigBuild.Ui
{
    /// <summary>
    /// A user interface.
    /// </summary>
    public interface IUi
    {
        /// <summary>
        /// The cursor mode within this UI.
        /// </summary>
        CursorMode CursorMode { get; }
        /// <summary>
        /// Whether the UI will pause the game or not.
        /// </summary>
        bool Pause { get; }

        /// <summary>
        /// Called when the UI is opened.
        /// </summary>
        /// <param name="manager">The UI manager</param>
        void OnOpened(UiManager manager);
        /// <summary>
        /// Called when the render surface is resized.
        /// </summary>
        /// <param name="target">The render surface</param>
        void OnResized(IRenderTarget target);
        /// <summary>
        /// Called when this UI is closed.
        /// </summary>
        void OnClosed();

        /// <summary>
        /// Updates and draws this UI.
        /// </summary>
        /// <param name="context">The render context</param>
        /// <param name="buffer">The geometry buffer</param>
        /// <param name="partialTick">The tick delta</param>
        void UpdateAndDraw(RenderContext context, IGeometryBuffer buffer, float partialTick);
        
        /// <summary>
        /// Handles cursor movement.
        /// </summary>
        /// <param name="x">The cursor X coordinate</param>
        /// <param name="y">The cursor Y coordinate</param>
        /// <returns>Whether the event was handled or should be passed through</returns>
        bool OnCursorMoved(int x, int y);
        /// <summary>
        /// Handles mouse button events.
        /// </summary>
        /// <param name="button">The button</param>
        /// <param name="action">The action</param>
        /// <returns>Whether the event was handled or should be passed through</returns>
        bool OnMouseEvent(uint button, MouseAction action);
        /// <summary>
        /// Handles mouse scroll events.
        /// </summary>
        /// <param name="xOffset">The scroll X offset</param>
        /// <param name="yOffset">The scroll Y offset</param>
        /// <returns>Whether the event was handled or should be passed through</returns>
        bool OnScrollEvent(double xOffset, double yOffset);
        /// <summary>
        /// Handles keyboard events.
        /// </summary>
        /// <param name="code">The keycode</param>
        /// <param name="action">The action</param>
        /// <returns>Whether the event was handled or should be passed through</returns>
        bool OnKeyboardEvent(uint code, KeyboardAction action);
        
        /// <summary>
        /// Called when a UI is opened above this UI.
        /// </summary>
        void OnLayerAdded();
        /// <summary>
        /// Called when the UI above this one is closed.
        /// </summary>
        void OnLayerRemoved();
    }
}