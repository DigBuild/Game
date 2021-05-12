using System;
using System.Collections.Generic;
using System.Numerics;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Render;
using DigBuild.Engine.Worlds;
using DigBuild.Platform.Render;
using DigBuild.Platform.Resource;
using DigBuild.Platform.Util;

namespace DigBuild.Render
{
    public sealed class WorldRenderer : IDisposable
    {
        private readonly NativeBufferPool _bufferPool;

        private readonly WorldRenderManager _worldRenderer;
        private readonly ISkyRenderer _skyRenderer;

        private CommandBuffer _commandBuffer = null!;
        private Framebuffer _framebuffer = null!;

        public WorldRenderer(
            IReadOnlyWorld world,
            ModelManager modelManager,
            IEnumerable<IRenderLayer> renderLayers,
            IEnumerable<IParticleRenderer> particleRenderers,
            NativeBufferPool bufferPool
        )
        {
            _bufferPool = bufferPool;
            _worldRenderer = new WorldRenderManager(
                world,
                modelManager.BlockModels,
                modelManager.EntityModels,
                renderLayers,
                particleRenderers,
                bufferPool
            );
            _skyRenderer = new SimpleSkyRenderer(world);
        }
        
        public void Dispose()
        {
            _skyRenderer.Dispose();
        }

        public void OnChunkChanged(IChunk chunk) => _worldRenderer.QueueChunkUpdate(chunk);
        public void OnChunkUnloaded(IChunk chunk) => _worldRenderer.QueueChunkRemoval(chunk);
        public void OnEntityAdded(EntityInstance entity) => _worldRenderer.AddEntity(entity);
        public void OnEntityRemoved(Guid guid) => _worldRenderer.RemoveEntity(guid);

        public void Setup(RenderContext context, ResourceManager resourceManager, RenderStage stage)
        {
            _commandBuffer = context.CreateCommandBuffer();
            _skyRenderer.Setup(context, resourceManager, stage);
        }

        public Framebuffer UpdateFramebuffer(RenderContext context, FramebufferFormat format, uint width, uint height)
        {
            return _framebuffer = context.CreateFramebuffer(format, width, height);
        }
        
        public void UpdateAndRender(RenderContext context, ICamera camera, float partialTick)
        {
            var physicalProjMat = Matrix4x4.CreatePerspectiveFieldOfView(
                camera.FieldOfView, _framebuffer.Width / (float) _framebuffer.Height, 0.001f, 500f
            );
            var projection = physicalProjMat * Matrix4x4.CreateRotationZ(MathF.PI);
            var cameraTransform = camera.Transform;
            var viewFrustum = new ViewFrustum(cameraTransform * physicalProjMat);

            _worldRenderer.UpdateChunks(context, camera, viewFrustum);
            _skyRenderer.Update(context, camera, viewFrustum, projection, partialTick);

            using (var cmd = _commandBuffer.Record(context, _framebuffer.Format, _bufferPool))
            {
                cmd.SetViewportAndScissor(_framebuffer);

                _skyRenderer.Record(context, cmd);
                _worldRenderer.SubmitGeometry(context, cmd, projection, camera, viewFrustum, partialTick);
            }

            context.Enqueue(_framebuffer, _commandBuffer);
        }
    }
}