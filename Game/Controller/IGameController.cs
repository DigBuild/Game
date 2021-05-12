using System;
using DigBuild.Platform.Render;

namespace DigBuild.Controller
{
    public interface IGameController : IDisposable
    {
        void Tick();
        void UpdateSurface(RenderContext context, RenderSurfaceContext surface);
    }
}