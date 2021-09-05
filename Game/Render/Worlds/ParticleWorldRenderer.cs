using System.Collections.Generic;
using DigBuild.Engine.Render;
using DigBuild.Engine.Render.Worlds;
using DigBuild.Platform.Render;

namespace DigBuild.Render.Worlds
{
    /// <summary>
    /// A world renderer that delegates work to particle renderers.
    /// </summary>
    public sealed class ParticleWorldRenderer : IWorldRenderer
    {
        private readonly IEnumerable<IParticleRenderer> _particleRenderers;

        public ParticleWorldRenderer(IEnumerable<IParticleRenderer> particleRenderers)
        {
            _particleRenderers = particleRenderers;
        }

        public void Dispose()
        {
        }

        public void Update(RenderContext context, WorldView worldView, float partialTick)
        {
            foreach (var renderer in _particleRenderers)
                renderer.Update(partialTick);
        }

        public void BeforeDraw(RenderContext context, CommandBufferRecorder cmd, UniformBufferSet uniforms, WorldView worldView, float partialTick)
        {
        }

        public void Draw(
            RenderContext context, CommandBufferRecorder cmd, IRenderLayer layer, RenderLayerBindingSet bindings,
            IReadOnlyUniformBufferSet uniforms, WorldView worldView, float partialTick
        )
        {
        }

        public void AfterDraw(RenderContext context, CommandBufferRecorder cmd, WorldView worldView, float partialTick)
        {
            var mat = worldView.Camera.Transform * worldView.Projection;
            foreach (var renderer in _particleRenderers)
                renderer.Draw(cmd, mat, worldView.Camera.FlattenTransform, partialTick);
        }
    }
}