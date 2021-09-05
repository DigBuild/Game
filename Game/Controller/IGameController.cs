using System;
using DigBuild.Platform.Render;

namespace DigBuild.Controller
{
    /// <summary>
    /// A gameplay controller.
    /// Manages ticking and rendering for a part of the game.
    /// </summary>
    public interface IGameController : IDisposable
    {
        /// <summary>
        /// Handles system ticks.
        /// </summary>
        void SystemTick();
        /// <summary>
        /// Handles gameplay ticks.
        /// </summary>
        void Tick();

        /// <summary>
        /// Draws to the render surface.
        /// </summary>
        /// <param name="context">The render context</param>
        /// <param name="surface">The render surface</param>
        void UpdateSurface(RenderContext context, RenderSurfaceContext surface);
    }
}