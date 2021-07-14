using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;
using DigBuild.Engine.Events;
using DigBuild.Engine.Math;
using DigBuild.Engine.Physics;
using DigBuild.Engine.Render;
using DigBuild.Engine.Render.Worlds;
using DigBuild.Engine.Worlds;
using DigBuild.Platform.Render;
using DigBuild.Platform.Resource;
using DigBuild.Platform.Util;
using DigBuild.Worlds;
using IRenderLayer = DigBuild.Engine.Render.IRenderLayer;
using UniformBufferSet = DigBuild.Engine.Render.UniformBufferSet;

namespace DigBuild.Render.Worlds
{
    public sealed class WorldRenderer : IDisposable
    {
        private readonly IEnumerable<IRenderLayer> _layers;
        private readonly IReadOnlyTextureSet _textures;
        private readonly NativeBufferPool _bufferPool;

        private readonly ISkyRenderer _skyRenderer;
        private readonly ImmutableList<IWorldRenderer> _worldRenderers;
        private readonly SelectionBoxRenderer _selectionBoxRenderer;

        private readonly UniformBufferSet _uniforms;
        private readonly RenderLayerBindingSet _bindingSet = new();

        private CommandBuffer _commandBuffer = null!;
        private Framebuffer _framebuffer = null!;

        public WorldRenderer(
            IReadOnlyWorld world,
            IGridAlignedRayCastingContext<WorldRayCastContext.Hit> rayCastingContext,
            Func<IReadOnlyWorld, ImmutableList<IWorldRenderer>> rendererProvider,
            IEnumerable<IRenderLayer> layers,
            IEnumerable<IRenderUniform> uniforms,
            IReadOnlyTextureSet textures,
            EventBus eventBus,
            NativeBufferPool bufferPool
        )
        {
            _layers = layers;
            _textures = textures;
            _bufferPool = bufferPool;

            _skyRenderer = new SimpleSkyRenderer(world);
            _worldRenderers = rendererProvider(world);
            _selectionBoxRenderer = new SelectionBoxRenderer(rayCastingContext, eventBus);

            _uniforms = new UniformBufferSet(uniforms, bufferPool);
        }
        
        public void Dispose()
        {
            _skyRenderer.Dispose();
            _selectionBoxRenderer.Dispose();
        }
        
        public void Setup(RenderContext context, ResourceManager resourceManager, RenderStage stage)
        {
            _commandBuffer = context.CreateCommandBuffer();
            _skyRenderer.Setup(context, resourceManager, stage);
            _selectionBoxRenderer.Setup(context, resourceManager, stage);
            _uniforms.Setup(context);
        }

        public Framebuffer UpdateFramebuffer(RenderContext context, FramebufferFormat format, uint width, uint height)
        {
            return _framebuffer = context.CreateFramebuffer(format, width, height);
        }

        public void InitLayerBindings(RenderContext context)
        {
            foreach (var layer in _layers)
                layer.InitBindings(context, _bindingSet);
        }

        public Matrix4x4 GetProjectionMatrix(ICamera camera)
        {
            var viewDist = (DigBuildGame.ViewRadius + 1) * WorldDimensions.ChunkSize * MathF.Sqrt(2);
            return Matrix4x4.CreatePerspectiveFieldOfView(
                camera.FieldOfView, _framebuffer.Width / (float) _framebuffer.Height, 0.1f, viewDist
            );
        }
        
        public void UpdateAndRender(RenderContext context, ICamera camera, float partialTick)
        {
            var physicalProjMat = GetProjectionMatrix(camera);
            var projection = physicalProjMat * Matrix4x4.CreateRotationZ(MathF.PI);
            var cameraTransform = camera.Transform;
            var viewFrustum = new ViewFrustum(cameraTransform * physicalProjMat);

            var worldView = new WorldView(projection, camera, viewFrustum);

            _skyRenderer.Update(context, worldView, partialTick);
            foreach (var renderer in _worldRenderers)
                renderer.Update(context, worldView, partialTick);
            _selectionBoxRenderer.Update(context, worldView, partialTick);

            using (var cmd = _commandBuffer.Record(context, _framebuffer.Format, _bufferPool))
            {
                cmd.SetViewportAndScissor(_framebuffer);
                
                _uniforms.Clear();
                
                _skyRenderer.Record(context, cmd);

                foreach (var renderer in _worldRenderers)
                    renderer.BeforeDraw(context, cmd, _uniforms, worldView, partialTick);
                
                foreach (var layer in _layers)
                {
                    layer.SetupCommand(cmd, _bindingSet, _uniforms, _textures);
                    foreach (var renderer in _worldRenderers)
                        renderer.Draw(context, cmd, layer, _bindingSet, _uniforms, worldView, partialTick);
                }

                foreach (var renderer in _worldRenderers)
                    renderer.AfterDraw(context, cmd, worldView, partialTick);

                _selectionBoxRenderer.Record(context, cmd);
                
                _uniforms.Upload(context);
            }

            context.Enqueue(_framebuffer, _commandBuffer);
        }
    }
}