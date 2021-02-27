using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using DigBuildEngine.Render;
using DigBuildEngine.Voxel;
using DigBuildPlatformCS;
using DigBuildPlatformCS.Render;
using DigBuildPlatformCS.Resource;
using DigBuildPlatformCS.Util;

namespace DigBuild
{
    public readonly struct Vertex2
    {
        public readonly Vector2 Position;

        public Vertex2(float x, float y)
        {
            Position = new Vector2(x, y);
        }
    }
    internal class RenderResources
    {
        public readonly RenderStage MainRenderStage;

        public readonly Framebuffer Framebuffer;

        public readonly CommandBuffer MainCommandBuffer;
        public readonly CommandBuffer CompCommandBuffer;

        internal RenderResources(RenderSurfaceContext surface, RenderContext context, ResourceManager resourceManager, NativeBufferPool bufferPool)
        {
            // Custom framebuffer format and render stages for preliminary rendering
            FramebufferFormat framebufferFormat = context
                .CreateFramebufferFormat()
                .WithDepthStencilAttachment(out var depthStencilAttachment)
                .WithColorAttachment(out var colorAttachment, TextureFormat.R8G8B8A8SRGB, new Vector4(0, 0, 0, 1))
                .WithStage(out MainRenderStage, depthStencilAttachment, colorAttachment);

            // Framebuffer for preliminary rendering
            Framebuffer = context.CreateFramebuffer(framebufferFormat, surface.Width, surface.Height);
            
            IResource vsCompResource = resourceManager.GetResource(new ResourceName(Game.Domain, "shaders/comp.vert.spv"))!;
            IResource fsCompResource = resourceManager.GetResource(new ResourceName(Game.Domain, "shaders/comp.frag.spv"))!;
            
            // Secondary geometry pipeline for compositing
            VertexShader vsComp = context.CreateVertexShader(vsCompResource);
            FragmentShader fsComp = context
                .CreateFragmentShader(fsCompResource)
                .WithSampler(out var colorTextureHandle);
            RenderPipeline<Vertex2> compPipeline = context.CreatePipeline<Vertex2>(
                vsComp, fsComp,
                surface.RenderStage,
                Topology.Triangles
            ).WithStandardBlending(surface.ColorAttachment);

            // Composition vertex buffer, pre-filled with screen rectangle
            using var compVertexData = bufferPool.Request<Vertex2>();
            compVertexData.Add(
                // Tri 1
                new Vertex2(0, 0),
                new Vertex2(1, 0),
                new Vertex2(1, 1),
                // Tri 2
                new Vertex2(1, 1),
                new Vertex2(0, 1),
                new Vertex2(0, 0)
            );
            VertexBuffer<Vertex2> compVertexBuffer = context.CreateVertexBuffer(compVertexData);

            // Create sampler and texture binding
            TextureSampler sampler = context.CreateTextureSampler();
            TextureBinding fbTextureBinding = context.CreateTextureBinding(
                colorTextureHandle,
                sampler,
                Framebuffer.Get(colorAttachment)
            );
            
            
            // Record commandBuffers
            MainCommandBuffer = context.CreateCommandBuffer();

            CompCommandBuffer = context.CreateCommandBuffer();
            var ccmd = CompCommandBuffer.BeginRecording(surface.Format, bufferPool);
            ccmd.SetViewportAndScissor(surface);
            ccmd.Using(compPipeline, fbTextureBinding);
            ccmd.Draw(compPipeline, compVertexBuffer);
            ccmd.Commit(context);
        }
    }
    
    public class GameWindow
    {
        private static readonly NativeBufferPool BufferPool = new();
        private static readonly ResourceManager ResourceManager = new(
            new FileSystemResourceProvider(
                new Dictionary<string, string>
                {
                    [Game.Domain] = "../../Game/Resources"
                }
            )
        );

        private readonly TickManager _tickManager;
        private readonly WorldRenderManager _worldRenderManager;

        private RenderResources? _resources;

        public GameWindow(TickManager tickManager, ICamera camera)
        {
            _tickManager = tickManager;
            _worldRenderManager = new WorldRenderManager(camera);
        }

        public async Task OpenWaitClosed()
        {
            var surface = await Platform.RequestRenderSurface(
                Update,
                titleHint: "DigBuild",
                widthHint: 1280,
                heightHint: 720
            );
            _tickManager.Start(surface.Closed);
            await surface.Closed;
        }

        public void OnChunkChanged(Chunk chunk)
        {
            _worldRenderManager.QueueChunkUpdate(chunk);
        }

        private void Update(RenderSurfaceContext surface, RenderContext context)
        {
            if (_resources == null)
            {
                _resources = new RenderResources(surface, context, ResourceManager, BufferPool);
                ChunkRenderData.Initialize(context, _resources.MainRenderStage, ResourceManager, BufferPool);
            }

            lock (_tickManager)
            {
                _worldRenderManager.UpdateChunks(context);

                var cmd = _resources.MainCommandBuffer.BeginRecording(_resources.Framebuffer.Format, BufferPool);
                {
                    cmd.SetViewportAndScissor(_resources.Framebuffer);
                    _worldRenderManager.SubmitGeometry(cmd, surface.Width / (float)surface.Height, _tickManager.PartialTick);
                }
                cmd.Commit(context);

                context.Enqueue(_resources.Framebuffer, _resources.MainCommandBuffer);
                context.Enqueue(surface, _resources.CompCommandBuffer);
            }
        }
        
    }
}