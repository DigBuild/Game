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
    /// <summary>
    /// The main world render manager.
    /// </summary>
    public sealed class WorldRenderManager : IDisposable
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

        public WorldRenderManager(
            IReadOnlyWorld world,
            IGridAlignedRayCastingContext<WorldRayCastContext.Hit> rayCastingContext,
            Func<IReadOnlyWorld, ImmutableList<IWorldRenderer>> rendererProvider,
            IEnumerable<IRenderLayer> layers,
            IEnumerable<IUniformType> uniforms,
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
            _selectionBoxRenderer = new SelectionBoxRenderer(rayCastingContext, world, eventBus);

            _uniforms = new UniformBufferSet(uniforms, bufferPool);
        }
        
        public void Dispose()
        {
            _skyRenderer.Dispose();
            _selectionBoxRenderer.Dispose();
        }
        
        /// <summary>
        /// Sets up all the resources needed for world rendering.
        /// </summary>
        /// <param name="context">The render context</param>
        /// <param name="resourceManager">The resource manager</param>
        /// <param name="stage">The render stage</param>
        public void Setup(RenderContext context, ResourceManager resourceManager, RenderStage stage)
        {
            _commandBuffer = context.CreateCommandBuffer();
            _skyRenderer.Setup(context, resourceManager, stage);
            _selectionBoxRenderer.Setup(context, resourceManager, stage);
            _uniforms.Setup(context);
        }

        /// <summary>
        /// Updates the framebuffer when the surface is created/resized.
        /// </summary>
        /// <param name="context">The render context</param>
        /// <param name="format">The format</param>
        /// <param name="width">The width</param>
        /// <param name="height">The height</param>
        /// <returns>The new framebuffer</returns>
        public Framebuffer UpdateFramebuffer(RenderContext context, FramebufferFormat format, uint width, uint height)
        {
            return _framebuffer = context.CreateFramebuffer(format, width, height);
        }

        /// <summary>
        /// Initializes all the render layer bindings.
        /// </summary>
        /// <param name="context">The render context</param>
        public void InitLayerBindings(RenderContext context)
        {
            foreach (var layer in _layers)
                layer.InitBindings(context, _bindingSet);
        }

        /// <summary>
        /// Gets the projection matrix for the given camera.
        /// </summary>
        /// <param name="camera">The camera</param>
        /// <returns>The projection matrix</returns>
        public Matrix4x4 GetProjectionMatrix(ICamera camera)
        {
            var viewDist = (DigBuildGame.ViewRadius + 1) * WorldDimensions.ChunkWidth * MathF.Sqrt(2);
            return Matrix4x4.CreatePerspectiveFieldOfView(
                camera.FieldOfView, _framebuffer.Width / (float) _framebuffer.Height, 0.1f, viewDist
            );
        }
        
        /// <summary>
        /// Updates all systems as needed and draws the world.
        /// </summary>
        /// <param name="context">The render context</param>
        /// <param name="camera">The camera</param>
        /// <param name="partialTick">The tick delta</param>
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

                _selectionBoxRenderer.Draw(context, cmd);
                
                _uniforms.Upload(context);
            }

            context.Enqueue(_framebuffer, _commandBuffer);
        }
    }
}