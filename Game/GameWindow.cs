using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using DigBuild.Blocks;
using DigBuild.Engine.Math;
using DigBuild.Engine.Render;
using DigBuild.Engine.Voxel;
using DigBuild.Platform.Render;
using DigBuild.Platform.Resource;
using DigBuild.Platform.Util;
using DigBuild.Render;

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
    
    public class RenderResources
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
        private readonly ICamera _camera;
        private readonly WorldRenderManager _worldRenderManager;
        
        private readonly List<IWorldRenderLayer> _renderLayers = new()
        {
            WorldRenderLayer.Opaque
        };
        
        public GameWindow(TickManager tickManager, ICamera camera)
        {
            _tickManager = tickManager;
            _camera = camera;
            
            var blockModels = new Dictionary<Block, IBlockModel>()
            {
                [GameBlocks.Terrain] = new CuboidBlockModel(AABB.FullBlock, new Vector2(0.9f, 0.8f)),
                [GameBlocks.Water] = new CuboidBlockModel(AABB.FullBlock, new Vector2(0.2f, 0.7f)),
            };
            _worldRenderManager = new WorldRenderManager(blockModels, _renderLayers, BufferPool);
        }

        public async Task OpenWaitClosed()
        {
            var surface = await Platform.Platform.RequestRenderSurface(
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

        public static RenderResources? Resources;

        private void Update(RenderSurfaceContext surface, RenderContext context)
        {
            if (Resources == null)
            {
                Resources = new RenderResources(surface, context, ResourceManager, BufferPool);
                foreach (var layer in _renderLayers)
                    layer.Initialize(context, ResourceManager);
            }

            lock (_tickManager)
            {
                _worldRenderManager.UpdateChunks();

                var cmd = Resources.MainCommandBuffer.BeginRecording(Resources.Framebuffer.Format, BufferPool);
                {
                    cmd.SetViewportAndScissor(Resources.Framebuffer);
                    _worldRenderManager.SubmitGeometry(context, cmd, _camera, _tickManager.PartialTick);
                }
                cmd.Commit(context);

                context.Enqueue(Resources.Framebuffer, Resources.MainCommandBuffer);
                context.Enqueue(surface, Resources.CompCommandBuffer);
            }
        }
        
    }
}