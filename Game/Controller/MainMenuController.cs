using DigBuild.Platform.Render;

namespace DigBuild.Controller
{
    public sealed class MainMenuController : IGameController
    {
        private readonly DigBuildGame _game;

        public MainMenuController(DigBuildGame game)
        {
            _game = game;
        }

        public void Dispose()
        {
        }

        public void Tick()
        {
            throw new System.NotImplementedException();
        }

        public void UpdateSurface(RenderContext context, RenderSurfaceContext surface)
        {
            throw new System.NotImplementedException();
        }
    }
}