using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using DigBuild.Engine.Impl.Worlds;
using DigBuild.Engine.Items;
using DigBuild.Engine.Particles;
using DigBuild.Engine.Physics;
using DigBuild.Engine.Render;
using DigBuild.Engine.Render.Worlds;
using DigBuild.Engine.Textures;
using DigBuild.Engine.Worldgen;
using DigBuild.Engine.Worlds;
using DigBuild.Events;
using DigBuild.Platform.Input;
using DigBuild.Platform.Render;
using DigBuild.Platform.Resource;
using DigBuild.Platform.Util;
using DigBuild.Players;
using DigBuild.Registries;
using DigBuild.Render;
using DigBuild.Render.Worlds;
using DigBuild.Ui;
using DigBuild.Worlds;
using PlayerController = DigBuild.Players.PlayerController;

namespace DigBuild.Controller
{
    public sealed class GameplayController : IGameController
    {
        private static readonly List<IRenderLayer> WorldRenderLayers = new(){
            Render.Worlds.WorldRenderLayers.Opaque,
            Render.Worlds.WorldRenderLayers.Cutout,
            Render.Worlds.WorldRenderLayers.Translucent
        };
        private static readonly List<IRenderLayer> UiRenderLayers = new(){
            // UiRenderLayer.Ui,
            // Render.WorldRenderLayers.Opaque,
            // Render.WorldRenderLayers.Cutout,
            // Render.WorldRenderLayers.Translucent,
            // UiRenderLayer.Text,
        };

        private static readonly ImmutableList<IRenderUniform> UniformTypes = ImmutableList.Create<IRenderUniform>(
            // RenderUniforms.ProjectionTransform,
            RenderUniforms.ModelViewTransform,
            RenderUniforms.WorldTime
        );
        
        private readonly DigBuildGame _game;
        private readonly World _world;
        
        private readonly GameInput _inputController = new();
        private readonly PlayerController _playerController;
        
        private readonly IList<IParticleSystem> _particleSystems;
        private readonly IList<IParticleRenderer> _particleRenderers;

        private bool _firstUpdate = true;
        private RenderResources _renderResources = null!;
        private SurfaceResources _surfaceResources = null!;

        private readonly TextureSet _textures;

        public DigBuildGame Game => _game;

        public IWorld World => _world;
        public WorldRayCastContext RayCastContext { get; }
        public IPlayer Player { get; }

        public WorldRenderer WorldRenderer { get; }
        public UiManager UiManager { get; }

        public IReadOnlyTextureSet Textures => _textures;

        public GameplayController(DigBuildGame game)
        {
            _game = game;
            _textures = new TextureSet(this);

            var config = Config.Load("server/config.json");

            var generator = new WorldGenerator(game.TickSource, config.Worldgen.Features, 0);
            _world = new World(game.TickSource, generator, pos => new RegionStorage(pos), _game.EventBus);
            RayCastContext = new WorldRayCastContext(World);
            
            _particleSystems = GameRegistries.ParticleSystems.Values.Select(d => d.System).ToImmutableList();
            _particleRenderers = GameRegistries.ParticleSystems.Values.Select(d => d.Renderer).ToImmutableList();
            
            WorldRenderer = new WorldRenderer(
                World,
                world => ImmutableList.Create<IWorldRenderer>(
                    new WorldTimeInjector(world),
                    new ChunkWorldRenderer(
                        world, _game.EventBus,
                        chunk => ImmutableList.Create<IChunkRenderer>(
                            new BlockChunkRenderer(world, chunk, _game.ModelManager.BlockModels, _game.BufferPool)
                        )
                    ),
                    new EntityWorldRenderer(_game.EventBus, _game.ModelManager.EntityModels, _game.BufferPool),
                    new ParticleWorldRenderer(_particleRenderers)
                ),
                WorldRenderLayers,
                UniformTypes,
                Textures,
                _game.BufferPool
            );

            UiManager = new UiManager(this, UiRenderLayers, UniformTypes, _game.BufferPool);
            
            Player = new Player(World.AddEntity(GameEntities.Player).WithPosition(new Vector3(0, 30, 0)));
            _playerController = new PlayerController(Player);

            var inventory = Player.Inventory;
            inventory.Hand.Item = new ItemInstance(GameRegistries.Items.GetOrNull(DigBuildGame.Domain, "campfire")!, 1);
        }

        public void Dispose()
        {
            _world.Dispose();
            WorldRenderer.Dispose();
        }

        public void Tick()
        {
            _inputController.Update();
            _playerController.UpdateMovement(_inputController);
            _playerController.UpdateHotbar(_inputController);
            var hit = Raycast.Cast(RayCastContext, Player.GetCamera(0).Ray);
            _playerController.UpdateInteraction(_inputController, hit);

            var particleUpdateContext = new ParticleUpdateContext();
            foreach (var particleSystem in _particleSystems)
                particleSystem.Update(particleUpdateContext);
        }

        public void UpdateSurface(RenderContext context, RenderSurfaceContext surface)
        {
            if (!_firstUpdate)
            {
                surface.InputContext.ConsumeCursorEvents(OnCursorMoved);
                surface.InputContext.ConsumeMouseEvents(OnMouseEvent);
                surface.InputContext.ConsumeKeyboardEvents(OnKeyboardEvent);
            }

            lock (_game.TickSource)
            {
                if (_firstUpdate)
                    SetupResources(context, surface);
                if (surface.Resized || _firstUpdate)
                    SetupSurfaceResources(context, surface);
                
                surface.InputContext.CursorMode = UiManager.Ui == null ? CursorMode.Raw : CursorMode.Normal;
                // if (UiManager.Ui != null)
                //     surface.InputContext.CenterCursor();
                
                var partialTick = _game.TickSource.CurrentTick.Value;
                WorldRenderer.UpdateAndRender(context, Player.GetCamera(partialTick), partialTick);
                UiManager.UpdateAndRender(context, partialTick);

                context.Enqueue(surface, _surfaceResources.CompositionCommandBuffer);
            }

            _firstUpdate = false;
        }

        private void OnCursorMoved(uint x, uint y, CursorAction action)
        {
            if (UiManager.OnCursorMoved(x, y, action))
                return;
            _inputController.OnCursorMoved(x, y, action);
        }

        private void OnMouseEvent(uint button, MouseAction action)
        {
            if (UiManager.OnMouseEvent(button, action))
                return;
            _inputController.OnMouseEvent(button, action);
        }

        private void OnKeyboardEvent(uint code, KeyboardAction action)
        {
            if (UiManager.OnKeyboardEvent(code, action))
                return;
            _inputController.OnKeyboardEvent(code, action);
        }

        private void SetupResources(RenderContext context, RenderSurfaceContext surface)
        {
            _renderResources = new RenderResources(context, surface, _game.ResourceManager);
            WorldRenderer.Setup(context, _game.ResourceManager, _renderResources.RenderStage);
            UiManager.Setup(context, _game.ResourceManager, _renderResources.RenderStage);

            _game.ModelManager.Load(_game.ResourceManager);

            var stitcher = new TextureStitcher();
            var loader = new MultiSpriteLoader(_game.ResourceManager, stitcher);
            _game.ModelManager.LoadTextures(loader);
            var spriteSheet = stitcher.Stitch(new ResourceName(DigBuildGame.Domain, "texturemap"));
            _textures.TextureSheet = context.CreateTexture(spriteSheet.Bitmap);

            _game.ModelManager.Bake();
            _game.EventBus.Post(new ModelsBakedEvent(_game.ModelManager));

            var allRenderLayers = new HashSet<IRenderLayer>();
            WorldRenderLayers.ForEach(l => allRenderLayers.Add(l));
            UiRenderLayers.ForEach(l => allRenderLayers.Add(l));

            foreach (var layer in allRenderLayers)
                layer.InitResources(context, _game.ResourceManager, _renderResources.RenderStage);

            foreach (var particleRenderer in _particleRenderers)
                particleRenderer.Initialize(context, _renderResources.RenderStage);
        }

        private void SetupSurfaceResources(RenderContext context, RenderSurfaceContext surface)
        {
            var worldFB = WorldRenderer.UpdateFramebuffer(context, _renderResources.Format, surface.Width, surface.Height);
            var uiFB = UiManager.UpdateFramebuffer(context, _renderResources.Format, surface.Width, surface.Height);
            _surfaceResources = new SurfaceResources(context, surface, _renderResources, _game.BufferPool, worldFB, uiFB);
        }

        private sealed class RenderResources
        {
            public readonly FramebufferFormat Format;
            public readonly FramebufferColorAttachment DiffuseColorAttachment;
            public readonly FramebufferColorAttachment BloomColorAttachment;
            public readonly FramebufferDepthStencilAttachment DepthStencilAttachment;
            public readonly RenderStage RenderStage;

            public readonly VertexBuffer<Vertex2> CompositionVertexBuffer;
            public readonly RenderPipeline<Vertex2> CompositionPipeline;
            public readonly ShaderSamplerHandle CompositionSamplerHandle;
            public readonly TextureSampler CompositionSampler;

            public readonly TextureSampler DefaultSampler;

            public RenderResources(
                RenderContext context,
                RenderSurfaceContext surface,
                ResourceManager resourceManager
            )
            {
                Format = context.CreateFramebufferFormat()
                    .WithColorAttachment(out DiffuseColorAttachment, TextureFormat.B8G8R8A8SRGB)
                    .WithColorAttachment(out BloomColorAttachment, TextureFormat.B8G8R8A8SRGB)
                    .WithDepthStencilAttachment(out DepthStencilAttachment)
                    .WithStage(out RenderStage, DepthStencilAttachment, DiffuseColorAttachment, BloomColorAttachment);

                CompositionVertexBuffer = context.CreateVertexBuffer(
                    // Tri 1
                    new Vertex2(0, 0),
                    new Vertex2(1, 0),
                    new Vertex2(1, 1),
                    // Tri 2
                    new Vertex2(1, 1),
                    new Vertex2(0, 1),
                    new Vertex2(0, 0)
                );

                var vsCompResource = resourceManager.Get<Shader>(DigBuildGame.Domain, "comp.vert")!;
                var fsCompResource = resourceManager.Get<Shader>(DigBuildGame.Domain, "comp.frag")!;
                VertexShader vsComp = context.CreateVertexShader(vsCompResource.Resource);
                FragmentShader fsComp = context.CreateFragmentShader(fsCompResource.Resource)
                    .WithSampler(out CompositionSamplerHandle);
                CompositionPipeline = context.CreatePipeline<Vertex2>(
                    vsComp, fsComp,
                    surface.RenderStage,
                    Topology.Triangles
                ).WithStandardBlending(surface.ColorAttachment);
                
                CompositionSampler = context.CreateTextureSampler(wrapping: TextureWrapping.ClampToEdge);

                DefaultSampler = context.CreateTextureSampler(TextureFiltering.Linear, TextureFiltering.Nearest);
            }
        }

        private sealed class SurfaceResources
        {
            public readonly Framebuffer WorldFramebuffer; // Managed by WorldRenderer
            public readonly Framebuffer UiFramebuffer; // Managed by UiManager

            public readonly CommandBuffer CompositionCommandBuffer;
            
            public SurfaceResources(
                RenderContext context,
                RenderSurfaceContext surface,
                RenderResources renderResources,
                NativeBufferPool bufferPool,
                Framebuffer worldFramebuffer,
                Framebuffer uiFramebuffer
            )
            {
                WorldFramebuffer = worldFramebuffer;
                UiFramebuffer = uiFramebuffer;
                
                TextureBinding worldDiffuseBinding = context.CreateTextureBinding(
                    renderResources.CompositionSamplerHandle,
                    renderResources.CompositionSampler,
                    WorldFramebuffer.Get(renderResources.DiffuseColorAttachment)
                );
                TextureBinding uiDiffuseBinding = context.CreateTextureBinding(
                    renderResources.CompositionSamplerHandle,
                    renderResources.CompositionSampler,
                    UiFramebuffer.Get(renderResources.DiffuseColorAttachment)
                );

                CompositionCommandBuffer = context.CreateCommandBuffer();
                using (var cmd = CompositionCommandBuffer.Record(context, surface.Format, bufferPool))
                {
                    cmd.SetViewportAndScissor(surface);

                    cmd.Using(renderResources.CompositionPipeline, worldDiffuseBinding);
                    cmd.Draw(renderResources.CompositionPipeline, renderResources.CompositionVertexBuffer);

                    cmd.Using(renderResources.CompositionPipeline, uiDiffuseBinding);
                    cmd.Draw(renderResources.CompositionPipeline, renderResources.CompositionVertexBuffer);
                }
            }
        }

        private class TextureSet : IReadOnlyTextureSet
        {
            private readonly GameplayController _controller;

            public TextureSampler DefaultSampler => _controller._renderResources.DefaultSampler;
            public Texture TextureSheet { get; set; } = null!;

            public TextureSet(GameplayController controller)
            {
                _controller = controller;
            }

            public Texture Get(RenderTexture texture)
            {
                if (texture == RenderTextures.Main)
                    return TextureSheet;
                throw new ArgumentException("Invalid render texture.", nameof(texture));
            }
        }
    }
}