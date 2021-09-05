using System.Threading.Tasks;
using DigBuild.Platform.Render;

namespace DigBuild.Render
{
    /// <summary>
    /// The main game window.
    /// </summary>
    public sealed class GameWindow
    {
        /// <summary>
        /// The title.
        /// </summary>
        public const string Title = "DigBuild";
        /// <summary>
        /// The default width.
        /// </summary>
        public const uint Width = 1280;
        /// <summary>
        /// The default height.
        /// </summary>
        public const uint Height = 720;

        private readonly DigBuildGame _game;

        /// <summary>
        /// The render surface.
        /// </summary>
        public RenderSurface Surface { get; private set; } = null!;

        public GameWindow(DigBuildGame game)
        {
            _game = game;
        }

        /// <summary>
        /// Opens the window.
        /// </summary>
        /// <returns>A task that completes once the window is open</returns>
        public async Task Open()
        {
            Surface = await Platform.Platform.RequestRenderSurface(
                Update,
                titleHint: Title,
                widthHint: Width,
                heightHint: Height
            );
        }

        /// <summary>
        /// Closes the window.
        /// </summary>
        /// <returns>A task that completes once the window is closed</returns>
        public async Task Close()
        {
            await Surface.Close();
        }

        private void Update(RenderSurfaceContext surface, RenderContext context)
        {
            _game.Controller.UpdateSurface(context, surface);
        }
    }
}