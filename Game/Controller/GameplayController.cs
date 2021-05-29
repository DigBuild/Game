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
using DigBuild.Render.Post;
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
                RayCastContext,
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
            inventory.Hand.Item = new ItemInstance(GameRegistries.Items.GetOrNull(DigBuildGame.Domain, "campfire")!, 64);
            // inventory.Hand.Item = new ItemInstance(GameRegistries.Items.GetOrNull(DigBuildGame.Domain, "stone_stairs")!, 3);
        }

        public void Dispose()
        {
            _world.Dispose();
            WorldRenderer.Dispose();
        }

        public void Boop()
        {
            _game.AudioManager.Play(GameSounds.Boop, pitch: 0.8f - (float) (new Random().NextDouble() * 0.05), gain: 0.1f);
        }

        public void Tick()
        {
            _inputController.Update();
            _playerController.UpdateMovement(_inputController);
            _playerController.UpdateHotbar(_inputController);
            var hit = Raycast.Cast(RayCastContext, Player.GetCamera(0).Ray);
            var interacted = _playerController.UpdateInteraction(_inputController, hit);

            if (interacted)
            {
                Boop();
            }

            // var z = Player.PhysicalEntity.Position.Z;
            // const float distance = 30;
            // const float halfDist = distance / 2;
            //
            // var blend = MathF.Max(0, MathF.Min((z + halfDist) / distance, 1));
            //
            // _nature1Player.Gain = (1 - blend) * 0.2f;
            // _nature2Player.Gain = blend * 0.03f;

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

                _surfaceResources.Enqueue(context, surface, _renderResources);
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
            _surfaceResources = new SurfaceResources(context, surface, _game.ResourceManager, _renderResources, _game.BufferPool, worldFB, uiFB);
        }

        private sealed class RenderResources
        {
            public readonly FramebufferFormat Format;
            public readonly FramebufferColorAttachment DiffuseColorAttachment;
            public readonly FramebufferColorAttachment BloomColorAttachment;
            public readonly FramebufferColorAttachment NormalColorAttachment;
            public readonly FramebufferColorAttachment PositionColorAttachment;
            public readonly FramebufferDepthStencilAttachment DepthStencilAttachment;
            public readonly RenderStage RenderStage;
            
            public readonly FramebufferFormat CompositionFormat;
            public readonly BlurPostProcessingEffect VBlurEffect;
            public readonly BlurPostProcessingEffect HBlurEffect;
            public readonly HDRPostProcessingEffect HDREffect;

            public readonly VertexBuffer<Vertex2> CompositionVertexBuffer;
            public readonly RenderPipeline<Vertex2> CompositionPipeline;
            public readonly RenderPipeline<Vertex2> CompositionPipelineAdd;
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
                    .WithColorAttachment(out DiffuseColorAttachment, TextureFormat.R32G32B32A32SFloat)
                    .WithColorAttachment(out BloomColorAttachment, TextureFormat.R32G32B32A32SFloat)
                    .WithColorAttachment(out NormalColorAttachment, TextureFormat.R32G32B32A32SFloat)
                    .WithColorAttachment(out PositionColorAttachment, TextureFormat.R32G32B32A32SFloat)
                    .WithDepthStencilAttachment(out DepthStencilAttachment)
                    .WithStage(out RenderStage, DepthStencilAttachment,
                        DiffuseColorAttachment, BloomColorAttachment, NormalColorAttachment, PositionColorAttachment
                    );

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

                CompositionFormat = context.CreateFramebufferFormat()
                    .WithColorAttachment(out var compColor, TextureFormat.R32G32B32A32SFloat)
                    .WithStage(out _, compColor);
                VBlurEffect = new BlurPostProcessingEffect(BlurDirection.Vertical, 2);
                HBlurEffect = new BlurPostProcessingEffect(BlurDirection.Horizontal, 2);
                HDREffect = new HDRPostProcessingEffect();

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
                CompositionPipelineAdd = context.CreatePipeline<Vertex2>(
                    vsComp, fsComp,
                    surface.RenderStage,
                    Topology.Triangles
                ).WithBlending(surface.ColorAttachment, BlendFactor.SrcAlpha, BlendFactor.One, BlendOperation.Add);
                
                CompositionSampler = context.CreateTextureSampler(wrapping: TextureWrapping.ClampToEdge);

                DefaultSampler = context.CreateTextureSampler(TextureFiltering.Linear, TextureFiltering.Nearest);
            }
        }

        private sealed class SurfaceResources
        {
            public readonly Framebuffer WorldFramebuffer; // Managed by WorldRenderer
            public readonly Framebuffer UiFramebuffer; // Managed by UiManager
            
            public readonly Framebuffer WorldCompFramebuffer;

            public readonly CommandBuffer WorldCompositionCommandBuffer;
            public readonly CommandBuffer CompositionCommandBuffer;
            
            public SurfaceResources(
                RenderContext context,
                RenderSurfaceContext surface,
                ResourceManager resourceManager,
                RenderResources renderResources,
                NativeBufferPool bufferPool,
                Framebuffer worldFramebuffer,
                Framebuffer uiFramebuffer
            )
            {
                WorldFramebuffer = worldFramebuffer;
                UiFramebuffer = uiFramebuffer;

                WorldCompFramebuffer = context.CreateFramebuffer(renderResources.CompositionFormat, surface.Width, surface.Height);
                
                renderResources.VBlurEffect.Input = WorldFramebuffer.Get(renderResources.BloomColorAttachment);
                renderResources.VBlurEffect.Setup(
                    context, surface, resourceManager,
                    renderResources.CompositionFormat,
                    renderResources.CompositionVertexBuffer,
                    renderResources.CompositionSampler,
                    bufferPool
                );

                renderResources.HBlurEffect.Input = renderResources.VBlurEffect.Output;
                renderResources.HBlurEffect.Setup(
                    context, surface, resourceManager,
                    renderResources.CompositionFormat,
                    renderResources.CompositionVertexBuffer,
                    renderResources.CompositionSampler,
                    bufferPool
                );
                
                TextureBinding worldDiffuseBinding = context.CreateTextureBinding(
                    renderResources.CompositionSamplerHandle,
                    renderResources.CompositionSampler,
                    WorldFramebuffer.Get(renderResources.DiffuseColorAttachment)
                );
                TextureBinding worldBloomBinding = context.CreateTextureBinding(
                    renderResources.CompositionSamplerHandle,
                    renderResources.CompositionSampler,
                    renderResources.HBlurEffect.Output
                );

                WorldCompositionCommandBuffer = context.CreateCommandBuffer();
                using (var cmd = WorldCompositionCommandBuffer.Record(context, WorldCompFramebuffer.Format, bufferPool))
                {
                    cmd.SetViewportAndScissor(WorldCompFramebuffer);

                    cmd.Using(renderResources.CompositionPipeline, worldDiffuseBinding);
                    cmd.Draw(renderResources.CompositionPipeline, renderResources.CompositionVertexBuffer);

                    cmd.Using(renderResources.CompositionPipelineAdd, worldBloomBinding);
                    cmd.Draw(renderResources.CompositionPipelineAdd, renderResources.CompositionVertexBuffer);
                }

                renderResources.HDREffect.Input = WorldCompFramebuffer.Get(WorldCompFramebuffer.Format.Attachments[0]);
                renderResources.HDREffect.Setup(
                    context, surface, resourceManager,
                    renderResources.CompositionFormat,
                    renderResources.CompositionVertexBuffer,
                    renderResources.CompositionSampler,
                    bufferPool
                );
                
                TextureBinding worldCompBinding = context.CreateTextureBinding(
                    renderResources.CompositionSamplerHandle,
                    renderResources.CompositionSampler,
                    renderResources.HDREffect.Output
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

                    cmd.Using(renderResources.CompositionPipeline, worldCompBinding);
                    cmd.Draw(renderResources.CompositionPipeline, renderResources.CompositionVertexBuffer);

                    cmd.Using(renderResources.CompositionPipeline, uiDiffuseBinding);
                    cmd.Draw(renderResources.CompositionPipeline, renderResources.CompositionVertexBuffer);
                }
            }

            public void Enqueue(RenderContext context, RenderSurfaceContext surface, RenderResources renderResources)
            {
                renderResources.VBlurEffect.Apply(context);
                renderResources.HBlurEffect.Apply(context);
                context.Enqueue(WorldCompFramebuffer, WorldCompositionCommandBuffer);

                renderResources.HDREffect.Apply(context);
                context.Enqueue(surface, CompositionCommandBuffer);
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