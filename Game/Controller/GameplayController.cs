﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using DigBuild.Engine.BuiltIn;
using DigBuild.Engine.Items;
using DigBuild.Engine.Math;
using DigBuild.Engine.Particles;
using DigBuild.Engine.Physics;
using DigBuild.Engine.Render;
using DigBuild.Engine.Render.Worlds;
using DigBuild.Engine.Textures;
using DigBuild.Engine.Worldgen;
using DigBuild.Engine.Worlds;
using DigBuild.Engine.Worlds.Impl;
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
    /// <summary>
    /// The controller in charge of gameplay.
    /// Manages all the game state.
    /// </summary>
    public sealed class GameplayController : IGameController
    {
        /// <summary>
        /// The render layers used for world rendering in order.
        /// </summary>
        public static readonly List<IRenderLayer> WorldRenderLayers = new(){
            Render.Worlds.WorldRenderLayers.Opaque,
            Render.Worlds.WorldRenderLayers.Cutout,
            Render.Worlds.WorldRenderLayers.Water,
        };
        /// <summary>
        /// The render layers used for UI rendering in order.
        /// </summary>
        public static readonly List<IRenderLayer> UiRenderLayers = new(){
            Render.UiRenderLayers.Ui,
            Render.Worlds.WorldRenderLayers.Opaque,
            Render.Worlds.WorldRenderLayers.Cutout,
            Render.Worlds.WorldRenderLayers.Water,
            Render.UiRenderLayers.Text,
            Render.UiRenderLayers.UiOverlay
        };

        /// <summary>
        /// The uniform types.
        /// </summary>
        public static readonly ImmutableList<IUniformType> UniformTypes = ImmutableList.Create<IUniformType>(
            Render.UniformTypes.ModelViewProjectionTransform,
            Render.UniformTypes.WorldTime
        );
        
        private readonly DigBuildGame _game;
        private readonly World _world;

        private readonly PlayerController _playerController;
        
        private readonly IList<IParticleSystem> _particleSystems;
        private readonly IList<IParticleRenderer> _particleRenderers;

        private bool _firstUpdate = true;
        private RenderResources _renderResources = null!;
        private SurfaceResources _surfaceResources = null!;

        private readonly TextureSet _textures;
        
        /// <summary>
        /// The game instance.
        /// </summary>
        public DigBuildGame Game => _game;

        /// <summary>
        /// The world.
        /// </summary>
        public IWorld World => _world;
        /// <summary>
        /// The world's raycast context.
        /// </summary>
        public WorldRayCastContext RayCastContext { get; }
        /// <summary>
        /// The player.
        /// </summary>
        public IPlayer Player { get; }
        
        /// <summary>
        /// The input controller.
        /// </summary>
        public GameInput InputController { get; } = new();

        /// <summary>
        /// The world render manager.
        /// </summary>
        public WorldRenderManager WorldRenderManager { get; }
        /// <summary>
        /// The UI manager.
        /// </summary>
        public UiManager UiManager { get; }

        /// <summary>
        /// The main texture set.
        /// </summary>
        public IReadOnlyTextureSet Textures => _textures;

        public GameplayController(DigBuildGame game)
        {
            _game = game;
            _textures = new TextureSet(this);
            
            var config = Config.Load("config.json");

            Dictionary<ChunkPos, BlockChunkRenderer> blockChunkRenderers = new();
            void NotifyChunkReRender(ChunkPos pos)
            {
                if (blockChunkRenderers.TryGetValue(pos, out var r))
                    r.OnChanged();
            }

            var generator = new WorldGenerator(game.TickSource, config.Worldgen.Features, config.Seed);
            _world = new World(game.TickSource, generator, (world, pos) => new RegionStorageHandler(world, pos, game.TickSource), _game.EventBus, NotifyChunkReRender);
            RayCastContext = new WorldRayCastContext(World);
            
            _particleSystems = GameRegistries.ParticleSystems.Values.Select(d => d.System).ToImmutableList();
            _particleRenderers = GameRegistries.ParticleSystems.Values.Select(d => d.Renderer).ToImmutableList();
            
            WorldRenderManager = new WorldRenderManager(
                World,
                RayCastContext,
                world => ImmutableList.Create<IWorldRenderer>(
                    new WorldTimeInjector(world),
                    new ChunkWorldRenderer(
                        world, _game.EventBus,
                        chunk =>
                        {
                            var bcr = new BlockChunkRenderer(world, chunk, _game.ModelManager.BlockModels, _game.BufferPool);
                            blockChunkRenderers[chunk.Position] = bcr;
                            return ImmutableList.Create<IChunkRenderer>(
                                bcr
                            );
                        }),
                    new EntityWorldRenderer(_game.EventBus, _game.ModelManager.EntityModels, _game.BufferPool),
                    new ParticleWorldRenderer(_particleRenderers)
                ),
                WorldRenderLayers,
                UniformTypes,
                Textures,
                _game.EventBus,
                _game.BufferPool
            );
            
            UiManager = new UiManager(this, UiRenderLayers, UniformTypes, _game.BufferPool);
            game.EventBus.Subscribe<TextureStitchingEvent>(GameHud.OnTextureStitching);

            var top = (int) WorldDimensions.ChunkHeight - 1;
            for (; top > 0; top--)
            {
                if (World.GetBlock(new BlockPos(0, top, 0)) == null)
                    continue;

                top += 1;
                break;
            }

            var playerEntity = World.GetEntity(Guid.Empty);
            playerEntity ??= World.AddEntity(GameEntities.Player, Guid.Empty).WithPosition(new Vector3(0.5f, top + 2, 0.5f));
            
            Player = new Player(this, playerEntity);
            _playerController = new PlayerController(Player);

            var inventory = Player.Inventory;
            
            inventory.Hotbar[1].TrySetItem(new ItemInstance(GameRegistries.Items.GetOrNull(DigBuildGame.Domain, "campfire")!, 1));
            inventory.Hotbar[2].TrySetItem(new ItemInstance(GameRegistries.Items.GetOrNull(DigBuildGame.Domain, "stone")!, 20));
            
            inventory.Equipment.EquipTopLeft.TrySetItem(new ItemInstance(GameRegistries.Items.GetOrNull(DigBuildGame.Domain, "pouch")!, 1));
        }

        public void Dispose()
        {
            _world.Dispose();
            WorldRenderManager.Dispose();
        }

        /// <summary>
        /// Plays a "boop" sound.
        /// </summary>
        public void Boop()
        {
            _game.AudioManager.Play(GameSounds.Boop, pitch: 0.8f - (float) (new Random().NextDouble() * 0.05), gain: 0.1f);
        }

        public void SystemTick()
        {
            InputController.Update(UiManager);
        }

        public void Tick()
        {
            _playerController.UpdateMovement(InputController);
            _playerController.UpdateHotbar(InputController);
            var hit = RayCaster.TryCast(RayCastContext, Player.GetCamera(0).Ray, out var h) ? h : null;
            var interacted = _playerController.UpdateInteraction(InputController, hit);

            if (interacted)
            {
                Boop();
            }

            // var z = Player.PhysicsEntity.Position.Z;
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
                surface.InputContext.ConsumeScrollEvents(OnScrollEvent);
                surface.InputContext.ConsumeKeyboardEvents(OnKeyboardEvent);
            }

            lock (_game.TickSource)
            {
                if (_firstUpdate)
                    SetupResources(context, surface);
                if (surface.Resized || _firstUpdate || Game.ResourceManager.GetAndClearModifiedResources().Count > 0)
                    SetupSurfaceResources(context, surface);
                
                surface.InputContext.CursorMode = UiManager.CursorMode;
                // if (UiManager.Ui != null)
                //     surface.InputContext.CenterCursor();
                
                var partialTick = _game.TickSource.CurrentTick.Value;
                WorldRenderManager.UpdateAndRender(context, Player.GetCamera(partialTick), partialTick);
                UiManager.UpdateAndRender(context, partialTick);

                var playerCamera = Player.GetCamera(_game.TickSource.CurrentTick.Value);
                var projMat = WorldRenderManager.GetProjectionMatrix(playerCamera);
                var worldTime = World.AbsoluteTime;
                var underwater = playerCamera.IsUnderwater;

                _surfaceResources.Enqueue(context, surface, _renderResources, projMat, worldTime, underwater);
            }

            _firstUpdate = false;
        }

        private void OnCursorMoved(uint x, uint y, CursorAction action)
        {
            UiManager.OnCursorMoved(x, y, action);
            // _inputController.OnCursorMoved(x, y, action);
        }

        private void OnMouseEvent(uint button, MouseAction action)
        {
            UiManager.OnMouseEvent(button, action);
            // _inputController.OnMouseEvent(button, action);
        }

        private void OnScrollEvent(double xOffset, double yOffset)
        {
            UiManager.OnScrollEvent(xOffset, yOffset);
        }

        private void OnKeyboardEvent(uint code, KeyboardAction action)
        {
            UiManager.OnKeyboardEvent(code, action);
            // _inputController.OnKeyboardEvent(code, action);
        }

        private void SetupResources(RenderContext context, RenderSurfaceContext surface)
        {
            _renderResources = new RenderResources(context, surface, _game.ResourceManager);
            WorldRenderManager.Setup(context, _game.ResourceManager, _renderResources.RenderStage);
            UiManager.Setup(context, _game.ResourceManager, _renderResources.RenderStage);

            _game.ModelManager.Load(_game.ResourceManager);

            var stitcher = new TextureStitcher();
            var loader = new MultiSpriteLoader(_game.ResourceManager, stitcher);
            _game.ModelManager.LoadTextures(loader);
            _game.EventBus.Post(new TextureStitchingEvent(TextureTypes.WorldMain, stitcher, _game.ResourceManager));
            var spriteSheet = stitcher.Stitch(new ResourceName(DigBuildGame.Domain, "texturemap"));
            _textures.TextureSheet = context.CreateTexture(spriteSheet.Bitmap);

            _game.ModelManager.Bake();
            _game.EventBus.Post(new ModelsBakedEvent(_game.ModelManager));

            var allRenderLayers = new HashSet<IRenderLayer>();
            WorldRenderLayers.ForEach(l => allRenderLayers.Add(l));
            UiRenderLayers.ForEach(l => allRenderLayers.Add(l));

            foreach (var layer in allRenderLayers)
                layer.InitResources(context, _game.ResourceManager, _renderResources.RenderStage);
            WorldRenderManager.InitLayerBindings(context);
            UiManager.InitLayerBindings(context);

            foreach (var particleRenderer in _particleRenderers)
                particleRenderer.Initialize(context, _renderResources.RenderStage);
        }

        private void SetupSurfaceResources(RenderContext context, RenderSurfaceContext surface)
        {
            var worldFB = WorldRenderManager.UpdateFramebuffer(context, _renderResources.Format, surface.Width, surface.Height);
            var uiFB = UiManager.UpdateFramebuffer(context, _renderResources.Format, surface.Width, surface.Height);
            var projMat = WorldRenderManager.GetProjectionMatrix(Player.GetCamera(0));
            _surfaceResources = new SurfaceResources(context, surface, _game.ResourceManager, _renderResources, _game.BufferPool, worldFB, uiFB, projMat);
        }

        private sealed class RenderResources
        {
            public readonly FramebufferFormat Format;
            public readonly FramebufferColorAttachment DiffuseColorAttachment;
            public readonly FramebufferColorAttachment BloomColorAttachment;
            public readonly FramebufferColorAttachment WaterColorAttachment;
            public readonly FramebufferColorAttachment NormalColorAttachment;
            public readonly FramebufferColorAttachment PositionColorAttachment;
            public readonly FramebufferDepthStencilAttachment DepthStencilAttachment;
            public readonly RenderStage RenderStage;
            
            public readonly FramebufferFormat CompositionFormat;
            public readonly WorldPostProcessingEffect WorldEffect;
            public readonly BlurPostProcessingEffect VBlurEffect;
            public readonly BlurPostProcessingEffect HBlurEffect;
            public readonly HDRPostProcessingEffect HDREffect;

            public readonly VertexBuffer<Vertex2> CompositionVertexBuffer;
            public readonly RenderPipeline<Vertex2> CompositionPipeline;
            public readonly RenderPipeline<Vertex2> CompositionPipelineAdd;
            public readonly RenderPipeline<Vertex2> SurfaceCompositionPipeline;
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
                    .WithColorAttachment(out WaterColorAttachment, TextureFormat.R32G32B32A32SFloat)
                    .WithColorAttachment(out NormalColorAttachment, TextureFormat.R32G32B32A32SFloat)
                    .WithColorAttachment(out PositionColorAttachment, TextureFormat.R32G32B32A32SFloat)
                    .WithDepthStencilAttachment(out DepthStencilAttachment)
                    .WithStage(
                        out RenderStage, DepthStencilAttachment,
                        DiffuseColorAttachment, BloomColorAttachment, WaterColorAttachment,
                        NormalColorAttachment, PositionColorAttachment
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
                    .WithStage(out var compositionStage, compColor);
                WorldEffect = new WorldPostProcessingEffect();
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
                    compositionStage,
                    Topology.Triangles
                ).WithStandardBlending(surface.ColorAttachment);
                CompositionPipelineAdd = context.CreatePipeline<Vertex2>(
                    vsComp, fsComp,
                    compositionStage,
                    Topology.Triangles
                ).WithBlending(compColor, BlendFactor.SrcAlpha, BlendFactor.One, BlendOperation.Add);

                SurfaceCompositionPipeline = context.CreatePipeline<Vertex2>(
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
            public readonly Framebuffer WorldFramebuffer; // Managed by WorldRenderManager
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
                Framebuffer uiFramebuffer,
                Matrix4x4 projectionMatrix
            )
            {
                WorldFramebuffer = worldFramebuffer;
                UiFramebuffer = uiFramebuffer;

                WorldCompFramebuffer = context.CreateFramebuffer(renderResources.CompositionFormat, surface.Width, surface.Height);
                
                renderResources.WorldEffect.ProjectionMatrix = projectionMatrix;
                renderResources.WorldEffect.InputWorld = WorldFramebuffer.Get(renderResources.DiffuseColorAttachment);
                renderResources.WorldEffect.InputPosition = WorldFramebuffer.Get(renderResources.PositionColorAttachment);
                renderResources.WorldEffect.InputWater = WorldFramebuffer.Get(renderResources.WaterColorAttachment);
                renderResources.WorldEffect.Setup(
                    context, surface, resourceManager,
                    renderResources.CompositionFormat,
                    renderResources.CompositionVertexBuffer,
                    renderResources.CompositionSampler,
                    bufferPool
                );

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
                    renderResources.WorldEffect.Output
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

                    cmd.Using(renderResources.SurfaceCompositionPipeline, worldCompBinding);
                    cmd.Draw(renderResources.SurfaceCompositionPipeline, renderResources.CompositionVertexBuffer);

                    cmd.Using(renderResources.SurfaceCompositionPipeline, uiDiffuseBinding);
                    cmd.Draw(renderResources.SurfaceCompositionPipeline, renderResources.CompositionVertexBuffer);
                }
            }

            public void Enqueue(
                RenderContext context,
                RenderSurfaceContext surface,
                RenderResources renderResources,
                Matrix4x4 projectionMatrix,
                ulong worldTime,
                bool underwater
            )
            {
                renderResources.WorldEffect.ProjectionMatrix = projectionMatrix;
                renderResources.WorldEffect.WorldTime = worldTime;
                renderResources.WorldEffect.Flags = WorldPostProcessingFlags.None;
                if (underwater)
                    renderResources.WorldEffect.Flags |= WorldPostProcessingFlags.Underwater;
                
                renderResources.WorldEffect.Apply(context);
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

            public Texture Get(TextureType textureType)
            {
                if (textureType == TextureTypes.WorldMain)
                    return TextureSheet;
                throw new ArgumentException("Invalid render textureType.", nameof(textureType));
            }
        }
    }
}