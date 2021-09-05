using DigBuild.Engine.Render;

namespace DigBuild.Render
{
    public static class TextureHandles
    {
        public static TextureType Main { get; } = new(DigBuildGame.Domain, "main");
        public static TextureType UiMain { get; } = new(DigBuildGame.Domain, "ui_main");
        public static TextureType UiText { get; } = new(DigBuildGame.Domain, "ui_text");
    }
}