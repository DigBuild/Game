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
            var ui = new SimpleUi(container);

            ui.Resized += () =>
            {
                container.Add(0, 0, new UiRectangle(10000, 10000, UiRenderLayer.Ui, null, new Vector4(0.1f, 0.1f, 0.1f, 0.8f)));
            };
            
            return ui;
        }
    }
}