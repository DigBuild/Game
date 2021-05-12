using System.Threading.Tasks;
using DigBuild.Platform.Render;

namespace DigBuild.Render
{
    public sealed class GameWindow
    {
        public const string Title = "DigBuild";
        public const uint Width = 1280;
        public const uint Height = 720;

        private readonly DigBuildGame _game;

        public RenderSurface Surface { get; private set; } = null!;

        public GameWindow(DigBuildGame game)
        {
            _game = game;
        }

        public async Task Open()
        {
            Surface = await Platform.Platform.RequestRenderSurface(
                Update,
                titleHint: Title,
                widthHint: Width,
                heightHint: Height
            );
        }

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