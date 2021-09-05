using System;
using DigBuild.Engine.Render.Worlds;
using DigBuild.Platform.Render;
using DigBuild.Platform.Resource;

namespace DigBuild.Render.Worlds
{
    /// <summary>
    /// A sky renderer.
    /// </summary>
    public interface ISkyRenderer : IDisposable
    {
        /// <summary>
        /// Sets up the render resources for sky rendering.
        /// </summary>
        /// <param name="context">The render context</param>
        /// <param name="resourceManager">The resource manager</param>
        /// <param name="renderStage">The render stage</param>
        void Setup(RenderContext context, ResourceManager resourceManager, RenderStage renderStage);

        /// <summary>
        /// Updates the sky rendering state.
        /// </summary>
        /// <param name="context">The render context</param>
        /// <param name="worldView">The world view</param>
        /// <param name="partialTick">The tick delta</param>
        void Update(RenderContext context, WorldView worldView, float partialTick);

        /// <summary>
        /// Draws the sky.
        /// </summary>
        /// <param name="context">The render context</param>
        /// <param name="cmd">The command buffer recorder</param>
        void Record(RenderContext context, CommandBufferRecorder cmd);
    }
}