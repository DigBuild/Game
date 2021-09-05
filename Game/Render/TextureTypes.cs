using DigBuild.Engine.Render;

namespace DigBuild.Render
{
    /// <summary>
    /// The game's texture types.
    /// </summary>
    public static class TextureTypes
    {
        /// <summary>
        /// The main world spritesheet.
        /// </summary>
        public static TextureType WorldMain { get; } = new(DigBuildGame.Domain, "world_main");
        /// <summary>
        /// The main UI spritesheet.
        /// </summary>
        public static TextureType UiMain { get; } = new(DigBuildGame.Domain, "ui_main");
        /// <summary>
        /// The UI text texture.
        /// </summary>
        public static TextureType UiText { get; } = new(DigBuildGame.Domain, "ui_text");
    }
}