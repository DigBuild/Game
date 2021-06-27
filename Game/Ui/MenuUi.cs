using System.Numerics;
using DigBuild.Engine.Ui;
using DigBuild.Render;

namespace DigBuild.Ui
{
    public static class MenuUi
    {
        public static IUi Create()
        {
            var container = new UiContainer();
            return new SimpleUi(container)
            {
                Resized = target =>
                {
                    container.Add(0, 0, new UiRectangle(
                        target.Width, target.Height, UiRenderLayer.Ui,
                        null, new Vector4(0.0f, 0.0f, 0.0f, 0.95f)
                    ));
                },
                PassEventsThrough = false
            };
        }
    }
}