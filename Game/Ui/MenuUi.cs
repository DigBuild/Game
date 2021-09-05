using System.Numerics;
using DigBuild.Engine.Ui.Elements;
using DigBuild.Render;

namespace DigBuild.Ui
{
    /// <summary>
    /// The pause menu user interface.
    /// </summary>
    public static class MenuUi
    {
        /// <summary>
        /// Creates a new instance of the pause menu.
        /// </summary>
        /// <returns>The UI</returns>
        public static IUi Create()
        {
            var container = new UiContainer();
            return new SimpleUi(container)
            {
                Resized = target =>
                {
                    container.Clear();

                    container.Add(0, 0, new UiRectangle(
                        target.Width, target.Height, UiRenderLayers.UiOverlay,
                        null, new Vector4(0.0f, 0.0f, 0.0f, 0.95f)
                    ));
                },
                PassEventsThrough = false,
                Pause = true
            };
        }
    }
}