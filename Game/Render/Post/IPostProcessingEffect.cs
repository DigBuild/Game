using DigBuild.Platform.Render;
using DigBuild.Platform.Resource;
using DigBuild.Platform.Util;

namespace DigBuild.Render.Post
{
    /// <summary>
    /// A post-processing effect.
    /// </summary>
    public interface IPostProcessingEffect
    {
        /// <summary>
        /// Sets up all the resources needed for the post-processing effect.
        /// </summary>
        /// <param name="context">The render context</param>
        /// <param name="surface">The surface context</param>
        /// <param name="resourceManager">The resource manager</param>
        /// <param name="compFormat">The composition format</param>
        /// <param name="compVertexBuffer">The composition vertex buffer</param>
        /// <param name="compSampler">The composition sampler</param>
        /// <param name="bufferPool">The buffer pool</param>
        void Setup(
            RenderContext context, RenderSurfaceContext surface,
            ResourceManager resourceManager, FramebufferFormat compFormat,
            VertexBuffer<Vertex2> compVertexBuffer, TextureSampler compSampler,
            NativeBufferPool bufferPool
        );

        /// <summary>
        /// Applies the post-processing effect.
        /// </summary>
        /// <param name="context">The render context</param>
        void Apply(RenderContext context);
    }
}