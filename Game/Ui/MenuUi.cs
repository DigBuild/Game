using System.Numerics;
using DigBuild.Engine.Textures;
using DigBuild.Engine.Ui;
using DigBuild.Render;

namespace DigBuild.Ui
{
    public static class MenuUi
    {
        public static IUiElement Create(ISprite white)
        {
            var ui = new UiContainer();
            ui.Add(0, 0, new UiRectangle(10000, 10000, UiRenderLayer.Ui, white, new Vector4(0.1f, 0.1f, 0.1f, 0.1f)));
            return ui;
        }
    }
}