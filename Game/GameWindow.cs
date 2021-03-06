using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;
using DigBuild.Blocks;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Math;
using DigBuild.Engine.Render;
using DigBuild.Engine.Textures;
using DigBuild.Engine.Voxel;
using DigBuild.GeneratedUniforms;
using DigBuild.Platform.Render;
using DigBuild.Platform.Resource;
using DigBuild.Platform.Util;
using DigBuild.Render;
using DigBuild.Voxel;

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

    public interface IOutlineTransform : IUniform<OutlineTransform>
    {
        Matrix4x4 Matrix { get; set; }
        Matrix4x4 Matrix2 { get; set; }
    }
    
    public class RenderResources
    {
        public readonly RenderStage MainRenderStage;

        public readonly Framebuffer Framebuffer;

        public readonly CommandBuffer MainCommandBuffer;
        public readonly CommandBuffer CompCommandBuffer;

        public readonly RenderPipeline<SimplerVertex> OutlinePipeline;
        public readonly VertexBuffer<SimplerVertex> OutlineVertexBuffer;
        public readonly PooledNativeBuffer<OutlineTransform> NativeOutlineUniformBuffer;
        public readonly UniformBuffer<OutlineTransform> OutlineUniformBuffer;

        public readonly Texture BlockTexture;

        internal RenderResources(RenderSurfaceContext surface, RenderContext context, ResourceManager resourceManager, NativeBufferPool bufferPool, SpriteSheet spriteSheet)
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
            
            IResource cursorResource = resourceManager.GetResource(new ResourceName(Game.Domain, "textures/cursor.png"))!;
            Texture cursorTexture = context.CreateTexture(
                new Bitmap(cursorResource.OpenStream())
            );
            TextureBinding cursorTextureBinding = context.CreateTextureBinding(
                colorTextureHandle,
                sampler,
                cursorTexture
            );
            
            // Record commandBuffers
            MainCommandBuffer = context.CreateCommandBuffer();

            CompCommandBuffer = context.CreateCommandBuffer();
            var ccmd = CompCommandBuffer.BeginRecording(surface.Format, bufferPool);
            ccmd.SetViewportAndScissor(surface);
            ccmd.Using(compPipeline, fbTextureBinding);
            ccmd.Draw(compPipeline, compVertexBuffer);
            ccmd.Using(compPipeline, cursorTextureBinding);
            ccmd.Draw(compPipeline, compVertexBuffer);
            ccmd.Commit(context);
            
            // Outline stuff idk
            IResource vsOutlineResource = resourceManager.GetResource(new ResourceName(Game.Domain, "shaders/outline.vert.spv"))!;
            IResource fsOutlineResource = resourceManager.GetResource(new ResourceName(Game.Domain, "shaders/outline.frag.spv"))!;
            
            VertexShader vsOutline = context
                .CreateVertexShader(vsOutlineResource)
                .WithUniform<OutlineTransform>(out var outlineUniform);
            FragmentShader fsOutline = context.CreateFragmentShader(fsOutlineResource);
            OutlinePipeline = context.CreatePipeline<SimplerVertex>(
                vsOutline, fsOutline,
                MainRenderStage,
                Topology.LineStrips,
                depthTest: new DepthTest(true, CompareOperation.LessOrEqual, true)
            ).WithStandardBlending(surface.ColorAttachment);
            
            using var outlineVertexData = bufferPool.Request<SimplerVertex>();
            outlineVertexData.Add(
                new SimplerVertex(0, 0.005f, 0),
                new SimplerVertex(0, 0.005f, 1),
                new SimplerVertex(1, 0.005f, 1),
                new SimplerVertex(1, 0.005f, 0),
                new SimplerVertex(0, 0.005f, 0)
            );
            OutlineVertexBuffer = context.CreateVertexBuffer(outlineVertexData);

            NativeOutlineUniformBuffer = bufferPool.Request<OutlineTransform>();
            NativeOutlineUniformBuffer.Add(new OutlineTransform()
            {
                Matrix = Matrix4x4.Identity,
                Matrix2 = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 2, 1280 / 720f, 0.001f, 10000f)
                          * Matrix4x4.CreateRotationZ(MathF.PI)
            });
            OutlineUniformBuffer = context.CreateUniformBuffer(outlineUniform, NativeOutlineUniformBuffer);

            BlockTexture = spriteSheet.Texture;
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
        private readonly WorldRayCastContext _rayCastContext;
        private readonly WorldRenderManager _worldRenderManager;
        private readonly TextureStitcher _stitcher;
        private readonly List<CuboidBlockModel> _unbakedModels = new();
        
        private readonly List<IWorldRenderLayer> _renderLayers = new()
        {
            WorldRenderLayer.Opaque
        };

        public GameWindow(TickManager tickManager, ICamera camera, WorldRayCastContext rayCastContext)
        {
            _tickManager = tickManager;
            _camera = camera;
            _rayCastContext = rayCastContext;
            _stitcher = new TextureStitcher();
            
            var dirtTexture = _stitcher.Add(ResourceManager.GetResource(Game.Domain, "textures/blocks/dirt.png")!);
            var grassTexture = _stitcher.Add(ResourceManager.GetResource(Game.Domain, "textures/blocks/grass.png")!);
            var grassSideTexture = _stitcher.Add(ResourceManager.GetResource(Game.Domain, "textures/blocks/grass_side.png")!);
            var waterTexture = _stitcher.Add(ResourceManager.GetResource(Game.Domain, "textures/blocks/water.png")!);
            var stoneTexture = _stitcher.Add(ResourceManager.GetResource(Game.Domain, "textures/blocks/stone.png")!);
            
            var dirtModel = new CuboidBlockModel(AABB.FullBlock, dirtTexture);
            var grassModel = new CuboidBlockModel(AABB.FullBlock, new[]
            {
                grassSideTexture, grassSideTexture,
                dirtTexture, grassTexture,
                grassSideTexture, grassSideTexture
            });
            var waterModel = new CuboidBlockModel(AABB.FullBlock, waterTexture);
            var stoneModel = new CuboidBlockModel(AABB.FullBlock, stoneTexture);
            _unbakedModels.Add(dirtModel);
            _unbakedModels.Add(grassModel);
            _unbakedModels.Add(waterModel);
            _unbakedModels.Add(stoneModel);

            var blockModels = new Dictionary<Block, IBlockModel>()
            {
                [GameBlocks.Dirt] = dirtModel,
                [GameBlocks.Grass] = grassModel,
                [GameBlocks.Water] = waterModel,
                [GameBlocks.Stone] = stoneModel,
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

        public void OnChunkChanged(IChunk chunk)
        {
            _worldRenderManager.QueueChunkUpdate(chunk);
        }

        public static RenderResources? Resources;

        private void Update(RenderSurfaceContext surface, RenderContext context)
        {
            if (Resources == null)
            {
                var spritesheet = _stitcher.Build(context);
                foreach (var model in _unbakedModels)
                    model.Initialize();
                Resources = new RenderResources(surface, context, ResourceManager, BufferPool, spritesheet);
                foreach (var layer in _renderLayers)
                    layer.Initialize(context, ResourceManager);
            }

            lock (_tickManager)
            {
                _worldRenderManager.UpdateChunks();
                
                var hit = RayCaster.Cast(_rayCastContext, _camera.GetInterpolatedRay(_tickManager.PartialTick));

                var cmd = Resources.MainCommandBuffer.BeginRecording(Resources.Framebuffer.Format, BufferPool);
                {
                    cmd.SetViewportAndScissor(Resources.Framebuffer);
                    _worldRenderManager.SubmitGeometry(context, cmd, _camera, _tickManager.PartialTick);
                    
                    if (hit != null)
                    {
                        Resources.NativeOutlineUniformBuffer[0].Matrix =
                            Matrix4x4.CreateTranslation(hit.Position + Vector3.UnitY)
                            * _camera.GetInterpolatedTransform(_tickManager.PartialTick);
                        Resources.OutlineUniformBuffer.Write(Resources.NativeOutlineUniformBuffer);
                        
                        cmd.Using(Resources.OutlinePipeline, Resources.OutlineUniformBuffer, 0);
                        cmd.Draw(Resources.OutlinePipeline, Resources.OutlineVertexBuffer);
                    }
                }
                cmd.Commit(context);

                context.Enqueue(Resources.Framebuffer, Resources.MainCommandBuffer);
                context.Enqueue(surface, Resources.CompCommandBuffer);
            }
        }
        
    }
}