using DigBuild.Engine.Render;

namespace DigBuild.Render
{
    public static class RenderTextures
    {
        public static RenderTexture Main { get; } = new(DigBuildGame.Domain, "main");
        public static RenderTexture UiMain { get; } = new(DigBuildGame.Domain, "ui_main");
        public static RenderTexture UiText { get; } = new(DigBuildGame.Domain, "ui_text");
    }
}