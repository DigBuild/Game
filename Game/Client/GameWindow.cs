using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using DigBuild.Blocks.Models;
using DigBuild.Client.GeneratedUniforms;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Items;
using DigBuild.Engine.Math;
using DigBuild.Engine.Physics;
using DigBuild.Engine.Render;
using DigBuild.Engine.Textures;
using DigBuild.Engine.Ui;
using DigBuild.Engine.Worlds;
using DigBuild.Entities.Models;
using DigBuild.Items.Models;
using DigBuild.Platform.Input;
using DigBuild.Platform.Render;
using DigBuild.Platform.Resource;
using DigBuild.Platform.Util;
using DigBuild.Registries;
using DigBuild.Render;
using DigBuild.Ui;
using DigBuild.Worlds;

namespace DigBuild.Client
{
    public readonly struct Vertex2
    {
        public readonly Vector2 Position;

        public Vertex2(float x, float y)
        {
            Position = new Vector2(x, y);
        }
    }

    public interface ISimpleTransform : IUniform<SimpleTransform>
    {
        Matrix4x4 Matrix { get; set; }
    }

    public interface IPixelSize : IUniform<PixelSize>
    {
        float Size { get; set; }
    }
    
    public class RenderResources
    {
        private readonly FramebufferFormat _framebufferFormat;
        private readonly FramebufferFormat _compFormat;
        private ShaderSamplerHandle _colorTextureHandle;
        private ShaderSamplerHandle _blurColorTextureHandle;
        private readonly FramebufferColorAttachment _colorAttachment;
        private readonly FramebufferColorAttachment _bloomAttachment;
        private readonly TextureSampler _sampler, _compSampler;
        private RenderPipeline<Vertex2> _compPipeline;
        private RenderPipeline<Vertex2> _compPipelineAdd;
        private RenderPipeline<Vertex2> _downscalePipeline;
        private RenderPipeline<Vertex2> _hBlurPipeline;
        private RenderPipeline<Vertex2> _vBlurPipeline;
        private TextureBinding _cursorTextureBinding;

        public readonly RenderStage MainRenderStage;
        public readonly RenderStage UiRenderStage;
        public readonly RenderStage CompRenderStage;

        public Framebuffer WorldFramebuffer { get; private set; } = null!;
        public Framebuffer UiFramebuffer { get; private set; } = null!;
        
        public Framebuffer BlurFramebuffer0 { get; private set; } = null!;
        public Framebuffer BlurFramebuffer1 { get; private set; } = null!;
        public Framebuffer BlurFramebuffer2 { get; private set; } = null!;
        
        public readonly PooledNativeBuffer<PixelSize> PixelSizeNativeUniformBuffer1;
        public UniformBuffer<PixelSize> PixelSizeUniformBuffer1 { get; private set; } = null!;

        public readonly PooledNativeBuffer<PixelSize> PixelSizeNativeUniformBuffer2;
        public UniformBuffer<PixelSize> PixelSizeUniformBuffer2 { get; private set; } = null!;

        
        public readonly CommandBuffer WorldCommandBuffer;
        public readonly CommandBuffer UiCommandBuffer;
        public readonly CommandBuffer PostCommandBuffer0;
        public readonly CommandBuffer PostCommandBuffer1;
        public readonly CommandBuffer PostCommandBuffer2;
        public readonly CommandBuffer CompCommandBuffer;

        public readonly INativeBuffer<SimplerVertex> OutlineNativeBuffer;
        public RenderPipeline<SimplerVertex> OutlinePipeline;
        public readonly VertexBuffer<SimplerVertex> OutlineVertexBuffer;
        public readonly VertexBufferWriter<SimplerVertex> OutlineVertexBufferWriter;
        public readonly PooledNativeBuffer<SimpleTransform> NativeOutlineUniformBuffer;
        public UniformBuffer<SimpleTransform> OutlineUniformBuffer { get; private set; } = null!;

        public readonly VertexBuffer<Vertex2> CompVertexBuffer;
        public RenderPipeline<Vertex2> ClearPipeline;
        
        public Texture BlockTexture;
        public Texture UiTexture;
        public Texture FontTexture;

        public readonly TextRenderer TextRenderer;

        internal RenderResources(RenderSurfaceContext surface, RenderContext context, ResourceManager resourceManager,
            NativeBufferPool bufferPool, TextureStitcher blockStitcher, TextureStitcher uiStitcher)
        {
            // Custom framebuffer format and render stages for preliminary rendering
            _framebufferFormat = context
                .CreateFramebufferFormat()
                .WithColorAttachment(out _colorAttachment, TextureFormat.R8G8B8A8SRGB, new Vector4(0, 0, 0, 1))
                .WithColorAttachment(out _bloomAttachment, TextureFormat.R8G8B8A8SRGB, new Vector4(0, 0, 0, 1))
                .WithDepthStencilAttachment(out var depthStencilAttachment)
                .WithStage(out MainRenderStage, depthStencilAttachment, _colorAttachment, _bloomAttachment);
            UiRenderStage = MainRenderStage;

            _compFormat = context
                .CreateFramebufferFormat()
                .WithColorAttachment(out var compColorAttachment, TextureFormat.R8G8B8A8SRGB, new Vector4(0, 0, 0, 1))
                .WithStage(out CompRenderStage, compColorAttachment);
         
            // Create sampler and texture binding
            _sampler = context.CreateTextureSampler();
            _compSampler = context.CreateTextureSampler(wrapping: TextureWrapping.ClampToEdge);
            
            PixelSizeNativeUniformBuffer1 = bufferPool.Request<PixelSize>();
            PixelSizeNativeUniformBuffer1.Add(new PixelSize { Size = 1.0f });
            
            PixelSizeNativeUniformBuffer2 = bufferPool.Request<PixelSize>();
            PixelSizeNativeUniformBuffer2.Add(new PixelSize { Size = 1.0f });

            
            RefreshResources(surface, context, resourceManager, blockStitcher, uiStitcher);

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
            CompVertexBuffer = context.CreateVertexBuffer(compVertexData);
            
            // Record commandBuffers
            WorldCommandBuffer = context.CreateCommandBuffer();
            UiCommandBuffer = context.CreateCommandBuffer();
            
            PostCommandBuffer0 = context.CreateCommandBuffer();
            PostCommandBuffer1 = context.CreateCommandBuffer();
            PostCommandBuffer2 = context.CreateCommandBuffer();
            CompCommandBuffer = context.CreateCommandBuffer();
            
            OutlineNativeBuffer = bufferPool.Request<SimplerVertex>();
            OutlineVertexBuffer = context.CreateVertexBuffer(out OutlineVertexBufferWriter);

            NativeOutlineUniformBuffer = bufferPool.Request<SimpleTransform>();
            NativeOutlineUniformBuffer.Add(new SimpleTransform()
            {
                Matrix = Matrix4x4.Identity
            });
            OutlineUniformBuffer.Write(NativeOutlineUniformBuffer);

            TextRenderer = new TextRenderer(UiRenderLayer.Text);

            OnResize(surface, context, bufferPool);
        }

        public void RefreshResources(RenderSurfaceContext surface, RenderContext context,
            ResourceManager resourceManager, TextureStitcher blockStitcher, TextureStitcher uiStitcher)
        {
            IResource vsCompResource = resourceManager.GetResource(new ResourceName(Game.Domain, "shaders/comp.vert.spv"))!;
            IResource fsCompResource = resourceManager.GetResource(new ResourceName(Game.Domain, "shaders/comp.frag.spv"))!;
            
            // Secondary geometry pipeline for compositing
            VertexShader vsComp = context.CreateVertexShader(vsCompResource);
            FragmentShader fsComp = context.CreateFragmentShader(fsCompResource)
                .WithSampler(out _colorTextureHandle);
            _compPipeline = context.CreatePipeline<Vertex2>(
                vsComp, fsComp,
                surface.RenderStage,
                Topology.Triangles
            ).WithStandardBlending(surface.ColorAttachment);
            _compPipelineAdd = context.CreatePipeline<Vertex2>(
                vsComp, fsComp,
                surface.RenderStage,
                Topology.Triangles
            ).WithBlending(surface.ColorAttachment, BlendFactor.SrcAlpha, BlendFactor.One, BlendOperation.Add);


            
            IResource vsHBlurResource = resourceManager.GetResource(new ResourceName(Game.Domain, "shaders/effects/hblur.vert.spv"))!;
            IResource vsVBlurResource = resourceManager.GetResource(new ResourceName(Game.Domain, "shaders/effects/vblur.vert.spv"))!;
            IResource fsBlurResource = resourceManager.GetResource(new ResourceName(Game.Domain, "shaders/effects/blur.frag.spv"))!;
            
            VertexShader vsHBlur = context.CreateVertexShader(vsHBlurResource)
                .WithUniform<PixelSize>(out var pixelSizeH);
            VertexShader vsVBlur = context.CreateVertexShader(vsVBlurResource)
                .WithUniform<PixelSize>(out var pixelSizeV);
            FragmentShader fsBlur = context.CreateFragmentShader(fsBlurResource)
                .WithSampler(out _blurColorTextureHandle);
            _downscalePipeline = context.CreatePipeline<Vertex2>(
                vsComp, fsComp,
                CompRenderStage,
                Topology.Triangles
            ).WithStandardBlending(surface.ColorAttachment);
            _hBlurPipeline = context.CreatePipeline<Vertex2>(
                vsHBlur, fsBlur,
                CompRenderStage,
                Topology.Triangles
            ).WithStandardBlending(surface.ColorAttachment);
            _vBlurPipeline = context.CreatePipeline<Vertex2>(
                vsVBlur, fsBlur,
                CompRenderStage,
                Topology.Triangles
            ).WithStandardBlending(surface.ColorAttachment);
            
            PixelSizeUniformBuffer1 = context.CreateUniformBuffer(pixelSizeH, PixelSizeNativeUniformBuffer1);
            PixelSizeUniformBuffer2 = context.CreateUniformBuffer(pixelSizeV, PixelSizeNativeUniformBuffer2);


            
            IResource cursorResource = resourceManager.GetResource(new ResourceName(Game.Domain, "textures/cursor.png"))!;
            Texture cursorTexture = context.CreateTexture(
                new Bitmap(cursorResource.OpenStream())
            );
            _cursorTextureBinding = context.CreateTextureBinding(
                _colorTextureHandle,
                _compSampler,
                cursorTexture
            );

            IResource vsClearResource = resourceManager.GetResource(new ResourceName(Game.Domain, "shaders/clear.vert.spv"))!;
            IResource fsClearResource = resourceManager.GetResource(new ResourceName(Game.Domain, "shaders/clear.frag.spv"))!;
            
            VertexShader vsClear = context.CreateVertexShader(vsClearResource);
            FragmentShader fsClear = context.CreateFragmentShader(fsClearResource);
            ClearPipeline = context.CreatePipeline<Vertex2>(
                vsClear, fsClear,
                MainRenderStage,
                Topology.Triangles
            );

            // Outline stuff idk
            IResource vsOutlineResource = resourceManager.GetResource(new ResourceName(Game.Domain, "shaders/world/outline.vert.spv"))!;
            IResource fsOutlineResource = resourceManager.GetResource(new ResourceName(Game.Domain, "shaders/world/outline.frag.spv"))!;
            
            VertexShader vsOutline = context
                .CreateVertexShader(vsOutlineResource)
                .WithUniform<SimpleTransform>(out var outlineUniform);
            FragmentShader fsOutline = context.CreateFragmentShader(fsOutlineResource);
            OutlinePipeline = context.CreatePipeline<SimplerVertex>(
                    vsOutline, fsOutline,
                    MainRenderStage,
                    Topology.Lines
                )
                .WithDepthTest(CompareOperation.LessOrEqual, true)
                .WithStandardBlending(_colorAttachment)
                .WithStandardBlending(_bloomAttachment);
            
            OutlineUniformBuffer = context.CreateUniformBuffer(outlineUniform);
            
            BlockTexture = context.CreateTexture(blockStitcher.Stitch(new ResourceName(Game.Domain, "blocks"), "block_spritesheet.png").Bitmap);
            UiTexture = context.CreateTexture(uiStitcher.Stitch(new ResourceName(Game.Domain, "ui"), "ui_spritesheet.png").Bitmap);

            IResource fontResource = resourceManager.GetResource(new ResourceName(Game.Domain, "textures/font.png"))!;
            FontTexture = context.CreateTexture(
                new Bitmap(fontResource.OpenStream())
            );
        }

        public void OnResize(RenderSurfaceContext surface, RenderContext context, NativeBufferPool bufferPool)
        {
            // Framebuffer for preliminary rendering
            WorldFramebuffer = context.CreateFramebuffer(_framebufferFormat, surface.Width, surface.Height);
            UiFramebuffer = context.CreateFramebuffer(_framebufferFormat, surface.Width, surface.Height);
            
            BlurFramebuffer0 = context.CreateFramebuffer(_compFormat, surface.Width / 2, surface.Height / 2);
            BlurFramebuffer1 = context.CreateFramebuffer(_compFormat, surface.Width / 4, surface.Height / 4);
            BlurFramebuffer2 = context.CreateFramebuffer(_compFormat, surface.Width / 4, surface.Height / 4);

            {
                TextureBinding worldFBTex = context.CreateTextureBinding(
                    _colorTextureHandle,
                    _compSampler,
                    WorldFramebuffer.Get(_bloomAttachment)
                );
            
                using (var cmd = PostCommandBuffer0.Record(context, _compFormat, bufferPool))
                {
                    cmd.SetViewportAndScissor(BlurFramebuffer0);
                    cmd.Using(_downscalePipeline, worldFBTex);
                    cmd.Draw(_downscalePipeline, CompVertexBuffer);
                }
            }
            
            {
                TextureBinding tex = context.CreateTextureBinding(
                    _blurColorTextureHandle,
                    _compSampler,
                    BlurFramebuffer0.Get(BlurFramebuffer0.Format.Attachments[0])
                );
            
                PixelSizeNativeUniformBuffer1[0] = new PixelSize {Size = 1f / BlurFramebuffer0.Width};
                PixelSizeUniformBuffer1.Write(PixelSizeNativeUniformBuffer1);
            
                using (var cmd = PostCommandBuffer1.Record(context, _compFormat, bufferPool))
                {
                    cmd.SetViewportAndScissor(BlurFramebuffer1);
                    cmd.Using(_hBlurPipeline, tex);
                    cmd.Using(_hBlurPipeline, PixelSizeUniformBuffer1, 0);
                    cmd.Draw(_hBlurPipeline, CompVertexBuffer);
                }
            }
            
            {
                TextureBinding tex = context.CreateTextureBinding(
                    _blurColorTextureHandle,
                    _compSampler,
                    BlurFramebuffer1.Get(BlurFramebuffer1.Format.Attachments[0])
                );
            
                PixelSizeNativeUniformBuffer2[0] = new PixelSize {Size = 1f / BlurFramebuffer1.Height};
                PixelSizeUniformBuffer2.Write(PixelSizeNativeUniformBuffer2);
            
                using (var cmd = PostCommandBuffer2.Record(context, _compFormat, bufferPool))
                {
                    cmd.SetViewportAndScissor(BlurFramebuffer2);
                    cmd.Using(_vBlurPipeline, tex);
                    cmd.Using(_vBlurPipeline, PixelSizeUniformBuffer2, 0);
                    cmd.Draw(_vBlurPipeline, CompVertexBuffer);
                }
            }


            {
                TextureBinding worldFBTex = context.CreateTextureBinding(
                    _colorTextureHandle,
                    _compSampler,
                    WorldFramebuffer.Get(_colorAttachment)
                );
                TextureBinding uiFBTex = context.CreateTextureBinding(
                    _colorTextureHandle,
                    _compSampler,
                    UiFramebuffer.Get(_colorAttachment)
                );
                TextureBinding blur2FBTex = context.CreateTextureBinding(
                    _colorTextureHandle,
                    _compSampler,
                    BlurFramebuffer2.Get(BlurFramebuffer2.Format.Attachments[0])
                );
            
                using (var cmd = CompCommandBuffer.Record(context, surface.Format, bufferPool))
                {
                    cmd.SetViewportAndScissor(surface);

                    cmd.Using(_compPipeline, worldFBTex);
                    cmd.Draw(_compPipeline, CompVertexBuffer);

                    cmd.Using(_compPipelineAdd, blur2FBTex);
                    cmd.Draw(_compPipelineAdd, CompVertexBuffer);

                    cmd.Using(_compPipeline, _cursorTextureBinding);
                    cmd.Draw(_compPipeline, CompVertexBuffer);
                    cmd.Using(_compPipeline, uiFBTex);
                    cmd.Draw(_compPipeline, CompVertexBuffer);
                }
            }
        }

        public static void GenerateBoundingBoxGeometry(INativeBuffer<SimplerVertex> buffer, AABB aabb)
        {
            var offset = new Vector3(0.005f);
            var min = aabb.Min - offset;
            var max = aabb.Max + offset;
            
            var vNNN = new Vector3(min.X, min.Y, min.Z);
            var vNNP = new Vector3(min.X, min.Y, max.Z);
            var vNPN = new Vector3(min.X, max.Y, min.Z);
            var vNPP = new Vector3(min.X, max.Y, max.Z);
            var vPNN = new Vector3(max.X, min.Y, min.Z);
            var vPNP = new Vector3(max.X, min.Y, max.Z);
            var vPPN = new Vector3(max.X, max.Y, min.Z);
            var vPPP = new Vector3(max.X, max.Y, max.Z);

            buffer.Add(
                new SimplerVertex(vNNN), new SimplerVertex(vNNP),
                new SimplerVertex(vNNP), new SimplerVertex(vPNP),
                new SimplerVertex(vPNP), new SimplerVertex(vPNN),
                new SimplerVertex(vPNN), new SimplerVertex(vNNN),
                new SimplerVertex(vNNN), new SimplerVertex(vNPN),
                new SimplerVertex(vNNP), new SimplerVertex(vNPP),
                new SimplerVertex(vPNP), new SimplerVertex(vPPP),
                new SimplerVertex(vPNN), new SimplerVertex(vPPN),
                new SimplerVertex(vNPN), new SimplerVertex(vNPP),
                new SimplerVertex(vNPP), new SimplerVertex(vPPP),
                new SimplerVertex(vPPP), new SimplerVertex(vPPN),
                new SimplerVertex(vPPN), new SimplerVertex(vNPN)
            );
        }
    }
    
    public class GameWindow : IUiElementContext
    {
        private static readonly NativeBufferPool BufferPool = new();
        private static readonly ResourceManager ResourceManager = new(
            new ShaderCompiler("shader_out"),
            new FileSystemResourceProvider(
                new Dictionary<string, string>
                {
                    [Game.Domain] = "../../Game/Resources"
                },
                true
            )
        );

        private readonly TickSource _tickSource;
        private readonly PlayerController _player;
        private readonly GameInput _input;
        private readonly WorldRayCastContext _rayCastContext;
        private readonly WorldRenderManager _worldRenderManager;
        private readonly TextureStitcher _blockStitcher = new();
        private readonly TextureStitcher _uiStitcher = new();
        private readonly List<CuboidBlockModel> _unbakedModels = new();
        public static readonly Dictionary<Item, IItemModel> ItemModels = new();
        public static IInventorySlot PickedItemSlot { get; private set; } = null!;
        
        private readonly List<IRenderLayer> _worldRenderLayers = new(){
            WorldRenderLayer.Opaque,
            WorldRenderLayer.Glowy,
            WorldRenderLayer.Translucent
        };
        private readonly List<IRenderLayer> _uiRenderLayers = new(){
            UiRenderLayer.Ui,
            WorldRenderLayer.Opaque,
            WorldRenderLayer.Glowy,
            WorldRenderLayer.Translucent,
            UiRenderLayer.Text,
        };

        private readonly GeometryBufferSet _uiGbs = new(BufferPool);
        private readonly UniformBufferSet _uiUbs = new(BufferPool);

        public GameWindow(TickSource tickSource, PlayerController player, GameInput input, WorldRayCastContext rayCastContext)
        {
            _tickSource = tickSource;
            _player = player;
            _input = input;
            PickedItemSlot = _player.Inventory.PickedItem;
            _rayCastContext = rayCastContext;
            
            var dirtTexture = _blockStitcher.Add(ResourceManager.GetResource(Game.Domain, "textures/blocks/dirt.png")!);
            var grassTexture = _blockStitcher.Add(ResourceManager.GetResource(Game.Domain, "textures/blocks/grass.png")!);
            var grassSideTexture = _blockStitcher.Add(ResourceManager.GetResource(Game.Domain, "textures/blocks/grass_side.png")!);
            var waterTexture = _blockStitcher.Add(ResourceManager.GetResource(Game.Domain, "textures/blocks/water.png")!);
            var stoneTexture = _blockStitcher.Add(ResourceManager.GetResource(Game.Domain, "textures/blocks/stone.png")!);
            var glowyTexture = _blockStitcher.Add(ResourceManager.GetResource(Game.Domain, "textures/blocks/glowy.png")!);
            
            var dirtModel = new CuboidBlockModel(AABB.FullBlock, dirtTexture);
            var grassModel = new CuboidBlockModel(AABB.FullBlock, new[]
            {
                grassSideTexture, grassSideTexture,
                dirtTexture, grassTexture,
                grassSideTexture, grassSideTexture
            });
            var waterModel = new CuboidBlockModel(AABB.FullBlock, waterTexture);
            var stoneModel = new CuboidBlockModel(AABB.FullBlock, stoneTexture);
            var stoneStairsModel = new CuboidBlockModel(GameBlocks.StoneStairAABBs, stoneTexture);
            var triangleModel = new SpinnyTriangleModel(stoneTexture);
            var glowyModel = new CuboidBlockModel(AABB.FullBlock, glowyTexture);
            _unbakedModels.Add(dirtModel);
            _unbakedModels.Add(grassModel);
            _unbakedModels.Add(waterModel);
            _unbakedModels.Add(stoneModel);
            _unbakedModels.Add(stoneStairsModel);
            _unbakedModels.Add(glowyModel);

            waterModel.Layer = () => WorldRenderLayer.Translucent;
            waterModel.Solid = false;

            glowyModel.Layer = () => WorldRenderLayer.Glowy;

            var blockModels = new Dictionary<Block, IBlockModel>()
            {
                [GameBlocks.Dirt] = dirtModel,
                [GameBlocks.Grass] = grassModel,
                [GameBlocks.Water] = waterModel,
                [GameBlocks.Stone] = stoneModel,
                [GameBlocks.StoneStairs] = stoneStairsModel,
                [GameBlocks.Crafter] = triangleModel,
                [GameBlocks.Glowy] = glowyModel
            };
            ItemModels[GameItems.Dirt] = new ItemBlockModel(dirtModel);
            ItemModels[GameItems.Grass] = new ItemBlockModel(grassModel);
            ItemModels[GameItems.Water] = new ItemBlockModel(waterModel);
            ItemModels[GameItems.Stone] = new ItemBlockModel(stoneModel);
            ItemModels[GameItems.StoneStairs] = new ItemBlockModel(stoneStairsModel);
            ItemModels[GameItems.Crafter] = new ItemBlockModel(triangleModel);
            var entityModels = new Dictionary<Entity, IEntityModel>()
            {
                [GameEntities.Item] = new ItemEntityModel(ItemModels)
            };

            _worldRenderManager = new WorldRenderManager(player.Entity.World, blockModels, entityModels, _worldRenderLayers, BufferPool);
        }

        public async Task OpenWaitClosed()
        {
            var surface = await Platform.Platform.RequestRenderSurface(
                Update,
                titleHint: "DigBuild",
                widthHint: 1280,
                heightHint: 720
            );
            _tickSource.Start(surface.Closed);
            await surface.Closed;
        }

        public void OnChunkChanged(IChunk chunk)
        {
            _worldRenderManager.QueueChunkUpdate(chunk);
        }

        public void OnChunkUnloaded(IChunk chunk)
        {
            _worldRenderManager.QueueChunkRemoval(chunk);
        }

        public void OnEntityAdded(EntityInstance entity)
        {
            _worldRenderManager.AddEntity(entity);
        }

        public void OnEntityRemoved(Guid guid)
        {
            _worldRenderManager.RemoveEntity(guid);
        }

        public static RenderResources? Resources;
        private readonly UiContainer _ui = new();
        private UiLabel _positionLabel = null!;
        private UiLabel _lookLabel = null!;
        private UiLabel _lightLabel = null!;
        private UiInventorySlot[] _hotbarSlots = null!;
        private UiUnboundInventorySlot _pickedSlot = null!;

        public static IUiElement? FunnyUi { get; set; }
        private IUiElement? _currentFunnyUi;
        private ISprite _whiteSprite;
        

        private uint _curX, _curY;
        private IUiElementContext.KeyboardEventDelegate? _keyboardEventDelegate;
        private Action? _keyboardEventDelegateRemoveCallback;

        private void OnCursorMoved(uint x, uint y, CursorAction action)
        {
            if (FunnyUi != null)
            {
                _curX = x;
                _curY = y;
                _player.HotbarTransfer = false;
            }
            else
            {
                _curX = uint.MaxValue;
                _curY = uint.MaxValue;
                _input.OnCursorMoved(x, y, action);
            }
        }

        private void OnMouseEvent(uint button, MouseAction action)
        {
            if (FunnyUi != null)
                _ui.OnMouseEvent(this, button, action);
            else
                _input.OnMouseEvent(button, action);
        }

        private void OnKeyboardEvent(uint code, KeyboardAction action)
        {
            if (code == 1 && action == KeyboardAction.Press)
            {
                FunnyUi = FunnyUi != null ? null : MenuUi.Create(_whiteSprite);
                return;
            }

            if (FunnyUi != null)
                _keyboardEventDelegate?.Invoke(code, action);
            else
                _input.OnKeyboardEvent(code, action);
        }

        private void Update(RenderSurfaceContext surface, RenderContext context)
        {
            surface.InputContext.ConsumeCursorEvents(OnCursorMoved);
            _ui.OnCursorMoved(this, (int) _curX, (int) _curY);
            surface.InputContext.ConsumeMouseEvents(OnMouseEvent);
            surface.InputContext.ConsumeKeyboardEvents(OnKeyboardEvent);

            if (Resources == null)
            {
                _whiteSprite = _uiStitcher.Add(ResourceManager.GetResource(Game.Domain, "textures/ui/white.png")!);
                // var inactiveButton = _uiStitcher.Add(ResourceManager.GetResource(Game.Domain, "textures/ui/button_inactive.png")!);
                // var hoveredButton = _uiStitcher.Add(ResourceManager.GetResource(Game.Domain, "textures/ui/button_hovered.png")!);
                // var clickedButton = _uiStitcher.Add(ResourceManager.GetResource(Game.Domain, "textures/ui/button_clicked.png")!);

                Resources = new RenderResources(surface, context, ResourceManager, BufferPool, _blockStitcher, _uiStitcher);
                
                foreach (var model in _unbakedModels)
                    model.Initialize();

                foreach (var layer in _worldRenderLayers)
                    layer.Initialize(context, ResourceManager);
                foreach (var layer in _uiRenderLayers.Where(l => !_worldRenderLayers.Contains(l)))
                    layer.Initialize(context, ResourceManager);

                IUiElement.GlobalTextRenderer = Resources.TextRenderer;

                _ui.Add(20, 20, _positionLabel = new UiLabel(""));
                _ui.Add(20, 50, _lookLabel = new UiLabel(""));
                _ui.Add(20, 80, _lightLabel = new UiLabel(""));
                
                {
                    var off = 60u;
                    var i = 0;
                    _hotbarSlots = new UiInventorySlot[_player.Inventory.Hotbar.Count];
                    foreach (var slot in _player.Inventory.Hotbar)
                    {
                        var i1 = i;
                        _ui.Add(off, surface.Height - 60, _hotbarSlots[i] = new UiInventorySlot(
                            slot, _player.Inventory.PickedItem, ItemModels, UiRenderLayer.Ui, 
                            () => _player.Inventory.ActiveHotbarSlot == i1)
                        );
                        off += 100;
                        i++;
                    }
                }

                _ui.Add(0, 0, _pickedSlot = new UiUnboundInventorySlot(_player.Inventory.PickedItem, ItemModels));

                FunnyUi = MenuUi.Create(_whiteSprite);
            }

            var modifiedResources = ResourceManager.GetAndClearModifiedResources();
            if (modifiedResources.Count > 0)
            {
                Resources.RefreshResources(surface, context, ResourceManager, _blockStitcher, _uiStitcher);
                
                foreach (var model in _unbakedModels)
                    model.Initialize();

                foreach (var layer in _worldRenderLayers)
                    layer.Initialize(context, ResourceManager);
                foreach (var layer in _uiRenderLayers.Where(l => !_worldRenderLayers.Contains(l)))
                    layer.Initialize(context, ResourceManager);
            }

            if (surface.Resized)
            {
                Resources.OnResize(surface, context, BufferPool);
                
                var off = 60u;
                foreach (var slot in _hotbarSlots)
                {
                    _ui.Remove(slot);
                    _ui.Add(off, surface.Height - 60, slot);
                    off += 100;
                }
            }

            lock (_tickSource)
            {
                var camera = _player.GetCamera(_tickSource.CurrentTick.Value);
                var hit = Raycast.Cast(_rayCastContext, camera.Ray);
                var physicalProjMat = Matrix4x4.CreatePerspectiveFieldOfView(
                    MathF.PI / 2, surface.Width / (float) surface.Height, 0.001f, 500f
                );
                var renderProjMat = physicalProjMat * Matrix4x4.CreateRotationZ(MathF.PI);
                var viewFrustum = new ViewFrustum(camera.Transform * physicalProjMat);
                
                if (GameInput.ReRender)
                    _worldRenderManager.ReRender(true);

                _worldRenderManager.UpdateChunks(camera, viewFrustum);
                
                using (var cmd = Resources.WorldCommandBuffer.Record(context, Resources.WorldFramebuffer.Format, BufferPool))
                {
                    cmd.SetViewportAndScissor(Resources.WorldFramebuffer);
                    cmd.Draw(Resources.ClearPipeline, Resources.CompVertexBuffer);
                    
                    _worldRenderManager.SubmitGeometry(context, cmd, renderProjMat, camera, viewFrustum);
                    
                    if (hit != null)
                    {
                        Resources.OutlineNativeBuffer.Clear();
                        RenderResources.GenerateBoundingBoxGeometry(Resources.OutlineNativeBuffer, hit.Bounds);
                        Resources.OutlineVertexBufferWriter.Write(Resources.OutlineNativeBuffer);
                    
                        Resources.NativeOutlineUniformBuffer[0].Matrix =
                            Matrix4x4.CreateTranslation(hit.Position)
                            * camera.Transform * renderProjMat;
                        Resources.OutlineUniformBuffer.Write(Resources.NativeOutlineUniformBuffer);
                        
                        cmd.Using(Resources.OutlinePipeline, Resources.OutlineUniformBuffer, 0);
                        cmd.Draw(Resources.OutlinePipeline, Resources.OutlineVertexBuffer);
                    }
                }

                if (FunnyUi != _currentFunnyUi)
                {
                    if (_currentFunnyUi != null)
                        _ui.Remove(_currentFunnyUi);
                    if (FunnyUi != null)
                        _ui.Add(0, 0, FunnyUi);
                    _currentFunnyUi = FunnyUi;
                    surface.InputContext.CursorMode = FunnyUi == null ? CursorMode.Raw : CursorMode.Normal;
                    if (FunnyUi != null)
                        surface.InputContext.CenterCursor();
                }

                _positionLabel.Text = $"Position: {new BlockPos(_player.PhysicalEntity.Position)}";
                _lookLabel.Text = $"Look: {hit?.Position}";
                _lightLabel.Text = $"Light: {(hit == null ? "" : _player.Entity.World.GetLight(hit.BlockPos.Offset(hit.Face)))}";

                if (_player.HotbarTransfer)
                {
                    _pickedSlot.PosX = (int) (60 + 100 * _player.Inventory.ActiveHotbarSlot);
                    _pickedSlot.PosY = (int) (surface.Height - 60 - 100);
                }

                _uiGbs.Clear();
                _uiGbs.Transform = Matrix4x4.Identity;
                _uiGbs.TransformNormal = false;
                _ui.Draw(context, _uiGbs);

                var uiProjection = Matrix4x4.CreateOrthographic(surface.Width, surface.Height, -100, 100) *
                                   Matrix4x4.CreateTranslation(-1, -1, 0);

                using (var cmd = Resources.UiCommandBuffer.Record(context, Resources.UiFramebuffer.Format, BufferPool))
                {
                    cmd.SetViewportAndScissor(Resources.UiFramebuffer);
                    _uiUbs.Clear();
                    // _uiUbs.Setup(context, cmd);
                    foreach (var layer in _uiRenderLayers)
                    {
                        layer.InitializeCommand(cmd);
                        
                        _uiUbs.AddAndUse(context, cmd, layer, uiProjection);
                        _uiGbs.Draw(layer, context, cmd);
                    }
                    _uiUbs.Finalize(context, cmd);
                }
                
                context.Enqueue(Resources.WorldFramebuffer, Resources.WorldCommandBuffer);
                context.Enqueue(Resources.BlurFramebuffer0, Resources.PostCommandBuffer0);
                context.Enqueue(Resources.BlurFramebuffer1, Resources.PostCommandBuffer1);
                context.Enqueue(Resources.BlurFramebuffer2, Resources.PostCommandBuffer2);
                context.Enqueue(Resources.UiFramebuffer, Resources.UiCommandBuffer);
                context.Enqueue(surface, Resources.CompCommandBuffer);
            }
        }

        public void SetKeyboardEventHandler(IUiElementContext.KeyboardEventDelegate? handler, Action? removeCallback = null)
        {
            _keyboardEventDelegate = handler;
            _keyboardEventDelegateRemoveCallback?.Invoke();
            _keyboardEventDelegateRemoveCallback = removeCallback;
        }

        public void RequestRedraw()
        {
        }
    }
}