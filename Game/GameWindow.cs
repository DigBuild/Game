using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;
using DigBuild.Blocks;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Items;
using DigBuild.Engine.Math;
using DigBuild.Engine.Render;
using DigBuild.Engine.Textures;
using DigBuild.Engine.UI;
using DigBuild.Engine.Voxel;
using DigBuild.Entities;
using DigBuild.GeneratedUniforms;
using DigBuild.Items;
using DigBuild.Platform.Input;
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

    public interface ISimpleTransform : IUniform<SimpleTransform>
    {
        Matrix4x4 Matrix { get; set; }
    }
    
    public class RenderResources
    {
        public readonly RenderStage MainRenderStage;
        public readonly RenderStage UiRenderStage;
        
        public readonly Framebuffer WorldFramebuffer;
        public readonly Framebuffer UiFramebuffer;
        
        public readonly CommandBuffer WorldCommandBuffer;
        public readonly CommandBuffer UiCommandBuffer;
        public readonly CommandBuffer CompCommandBuffer;

        public readonly RenderPipeline<SimplerVertex> OutlinePipeline;
        public readonly VertexBuffer<SimplerVertex> OutlineVertexBuffer;
        public readonly PooledNativeBuffer<SimpleTransform> NativeOutlineUniformBuffer;
        public readonly UniformBuffer<SimpleTransform> OutlineUniformBuffer;

        public readonly VertexBuffer<Vertex2> CompVertexBuffer;
        public readonly RenderPipeline<Vertex2> ClearPipeline;
        
        public readonly Texture BlockTexture;
        public readonly Texture UiTexture;
        public readonly Texture FontTexture;

        public readonly TextRenderer TextRenderer;

        internal RenderResources(RenderSurfaceContext surface, RenderContext context, ResourceManager resourceManager,
            NativeBufferPool bufferPool, SpriteSheet blockSpriteSheet, SpriteSheet uiSpriteSheet)
        {
            // Custom framebuffer format and render stages for preliminary rendering
            FramebufferFormat framebufferFormat = context
                .CreateFramebufferFormat()
                .WithColorAttachment(out var colorAttachment, TextureFormat.R8G8B8A8SRGB, new Vector4(0, 0, 0, 1))
                .WithDepthStencilAttachment(out var depthStencilAttachment)
                .WithStage(out MainRenderStage, depthStencilAttachment, colorAttachment);
            UiRenderStage = MainRenderStage;

            // Framebuffer for preliminary rendering
            WorldFramebuffer = context.CreateFramebuffer(framebufferFormat, surface.Width, surface.Height);
            UiFramebuffer = context.CreateFramebuffer(framebufferFormat, surface.Width, surface.Height);
            
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
            CompVertexBuffer = context.CreateVertexBuffer(compVertexData);

            // Create sampler and texture binding
            TextureSampler sampler = context.CreateTextureSampler();
            TextureBinding worldFramebufferTextureBinding = context.CreateTextureBinding(
                colorTextureHandle,
                sampler,
                WorldFramebuffer.Get(colorAttachment)
            );
            TextureBinding uiFramebufferTextureBinding = context.CreateTextureBinding(
                colorTextureHandle,
                sampler,
                UiFramebuffer.Get(colorAttachment)
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
            
            
            IResource vsClearResource = resourceManager.GetResource(new ResourceName(Game.Domain, "shaders/clear.vert.spv"))!;
            IResource fsClearResource = resourceManager.GetResource(new ResourceName(Game.Domain, "shaders/clear.frag.spv"))!;
            
            VertexShader vsClear = context.CreateVertexShader(vsClearResource);
            FragmentShader fsClear = context.CreateFragmentShader(fsClearResource);
            ClearPipeline = context.CreatePipeline<Vertex2>(
                vsClear, fsClear,
                MainRenderStage,
                Topology.Triangles
            );
            
            // Record commandBuffers
            WorldCommandBuffer = context.CreateCommandBuffer();
            UiCommandBuffer = context.CreateCommandBuffer();

            CompCommandBuffer = context.CreateCommandBuffer();
            using (var cmd = CompCommandBuffer.Record(context, surface.Format, bufferPool))
            {
                cmd.SetViewportAndScissor(surface);
                cmd.Using(compPipeline, worldFramebufferTextureBinding);
                cmd.Draw(compPipeline, CompVertexBuffer);
                cmd.Using(compPipeline, cursorTextureBinding);
                cmd.Draw(compPipeline, CompVertexBuffer);
                cmd.Using(compPipeline, uiFramebufferTextureBinding);
                cmd.Draw(compPipeline, CompVertexBuffer);
            }

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
                Topology.LineStrips
            )
                .WithDepthTest(CompareOperation.LessOrEqual, true)
                .WithStandardBlending(surface.ColorAttachment);
            
            using var outlineVertexData = bufferPool.Request<SimplerVertex>();
            outlineVertexData.Add(
                new SimplerVertex(0, 0.005f, 0),
                new SimplerVertex(0, 0.005f, 1),
                new SimplerVertex(1, 0.005f, 1),
                new SimplerVertex(1, 0.005f, 0),
                new SimplerVertex(0, 0.005f, 0)
            );
            OutlineVertexBuffer = context.CreateVertexBuffer(outlineVertexData);

            NativeOutlineUniformBuffer = bufferPool.Request<SimpleTransform>();
            NativeOutlineUniformBuffer.Add(new SimpleTransform()
            {
                Matrix = Matrix4x4.Identity
            });
            OutlineUniformBuffer = context.CreateUniformBuffer(outlineUniform, NativeOutlineUniformBuffer);

            BlockTexture = blockSpriteSheet.Texture;
            UiTexture = uiSpriteSheet.Texture;

            IResource fontResource = resourceManager.GetResource(new ResourceName(Game.Domain, "textures/font.png"))!;
            FontTexture = context.CreateTexture(
                new Bitmap(fontResource.OpenStream())
            );

            TextRenderer = new TextRenderer(UIRenderLayer.Text);
        }
    }
    
    public class GameWindow : IUIElementContext
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

        private readonly TickSource _tickSource;
        private readonly PlayerController _player;
        private readonly WorldRayCastContext _rayCastContext;
        private readonly WorldRenderManager _worldRenderManager;
        private readonly TextureStitcher _blockStitcher = new();
        private readonly TextureStitcher _uiStitcher = new();
        private readonly List<CuboidBlockModel> _unbakedModels = new();
        private readonly Dictionary<Item, IItemModel> _itemModels;
        
        private readonly List<IRenderLayer> _worldRenderLayers = new(){
            WorldRenderLayer.Opaque
        };
        private readonly List<IRenderLayer> _uiRenderLayers = new(){
            UIRenderLayer.Ui,
            WorldRenderLayer.Opaque,
            UIRenderLayer.Text,
        };

        private readonly GeometryBufferSet _uiGbs = new(BufferPool);
        private readonly UniformBufferSet _uiUbs = new(BufferPool);

        public GameWindow(TickSource tickSource, PlayerController player, WorldRayCastContext rayCastContext)
        {
            _tickSource = tickSource;
            _player = player;
            _rayCastContext = rayCastContext;
            
            var dirtTexture = _blockStitcher.Add(ResourceManager.GetResource(Game.Domain, "textures/blocks/dirt.png")!);
            var grassTexture = _blockStitcher.Add(ResourceManager.GetResource(Game.Domain, "textures/blocks/grass.png")!);
            var grassSideTexture = _blockStitcher.Add(ResourceManager.GetResource(Game.Domain, "textures/blocks/grass_side.png")!);
            var waterTexture = _blockStitcher.Add(ResourceManager.GetResource(Game.Domain, "textures/blocks/water.png")!);
            var stoneTexture = _blockStitcher.Add(ResourceManager.GetResource(Game.Domain, "textures/blocks/stone.png")!);
            
            var dirtModel = new CuboidBlockModel(AABB.FullBlock, dirtTexture);
            var grassModel = new CuboidBlockModel(AABB.FullBlock, new[]
            {
                grassSideTexture, grassSideTexture,
                dirtTexture, grassTexture,
                grassSideTexture, grassSideTexture
            });
            var waterModel = new CuboidBlockModel(AABB.FullBlock, waterTexture);
            var stoneModel = new CuboidBlockModel(AABB.FullBlock, stoneTexture);
            var triangleModel = new SpinnyTriangleModel(stoneTexture);
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
                [GameBlocks.TriangleBlock] = triangleModel
            };
            _itemModels = new Dictionary<Item, IItemModel>()
            {
                [GameItems.Dirt] = new ItemBlockModel(dirtModel),
                [GameItems.Grass] = new ItemBlockModel(grassModel),
                [GameItems.Water] = new ItemBlockModel(waterModel),
                [GameItems.Stone] = new ItemBlockModel(stoneModel),
                [GameItems.TriangleItem] = new ItemBlockModel(triangleModel)
            };
            var entityModels = new Dictionary<Entity, IEntityModel>()
            {
                [GameEntities.Item] = new ItemEntityModel(_itemModels)
            };

            _worldRenderManager = new WorldRenderManager(blockModels, entityModels, _worldRenderLayers, BufferPool);
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

        public void OnEntityAdded(EntityInstance entity)
        {
            _worldRenderManager.AddEntity(entity);
        }

        public void OnEntityRemoved(Guid guid)
        {
            _worldRenderManager.RemoveEntity(guid);
        }

        public static RenderResources? Resources;
        private readonly UIContainer _ui = new();
        private UILabel _positionLabel = null!;
        private UILabel _lookLabel = null!;
        private UIInventorySlot[] _hotbarSlots = null!;
        private UIUnboundInventorySlot _pickedSlot = null!;
        
        private UIInventorySlot[] _shapedSlots = null!;
        private UIInventorySlot _outputSlot = null!;

        private uint _curX, _curY;
        private IUIElementContext.KeyboardEventDelegate? _keyboardEventDelegate;
        private Action? _keyboardEventDelegateRemoveCallback;

        private void OnCursorMoved(uint x, uint y, CursorAction action)
        {
            _curX = x;
            _curY = y;
            _player.HotbarTransfer = false;
        }

        private void OnKeyboardEvent(uint code, KeyboardAction action)
        {
            _keyboardEventDelegate?.Invoke(code, action);
        }

        private void Update(RenderSurfaceContext surface, RenderContext context)
        {
            surface.InputContext.ConsumeCursorEvents(OnCursorMoved);
            _ui.OnCursorMoved(this, (int) _curX, (int) _curY);
            surface.InputContext.ConsumeMouseEvents((button, action) => _ui.OnMouseEvent(this, button, action));
            surface.InputContext.ConsumeKeyboardEvents(OnKeyboardEvent);

            if (Resources == null)
            {
                var blockSpritesheet = _blockStitcher.Build(context, "block_spritesheet.png");
                foreach (var model in _unbakedModels)
                    model.Initialize();

                _uiStitcher.Add(ResourceManager.GetResource(Game.Domain, "textures/ui/white.png")!);
                // var inactiveButton = _uiStitcher.Add(ResourceManager.GetResource(Game.Domain, "textures/ui/button_inactive.png")!);
                // var hoveredButton = _uiStitcher.Add(ResourceManager.GetResource(Game.Domain, "textures/ui/button_hovered.png")!);
                // var clickedButton = _uiStitcher.Add(ResourceManager.GetResource(Game.Domain, "textures/ui/button_clicked.png")!);
                var uiSpritesheet = _uiStitcher.Build(context, "ui_spritesheet.png");

                Resources = new RenderResources(surface, context, ResourceManager, BufferPool, blockSpritesheet, uiSpritesheet);

                foreach (var layer in _worldRenderLayers)
                    layer.Initialize(context, ResourceManager);
                foreach (var layer in _uiRenderLayers)
                    layer.Initialize(context, ResourceManager);

                IUIElement.GlobalTextRenderer = Resources.TextRenderer;

                _ui.Add(20, 20, _positionLabel = new UILabel(""));
                _ui.Add(20, 50, _lookLabel = new UILabel(""));

                {
                    uint x = 120u, y = 120u;
                    _shapedSlots = new UIInventorySlot[_player.ShapedSlots.Length];
                    for (var i = 0; i < _shapedSlots.Length; i++)
                    {
                        _ui.Add(x, y, _shapedSlots[i] = new UIInventorySlot(
                            _player.ShapedSlots[i], _player.PickedItem, _itemModels, UIRenderLayer.Ui
                        ));
                        if (i == 1 || i == 4)
                        {
                            x -= 45 * 3;
                            y += 76;
                        }
                        else
                        {
                            x += 90;
                        }
                    }
                    _ui.Add(120 + 90 * 3, 120 + 76, _outputSlot = new UIInventorySlot(
                        _player.OutputSlot, _player.PickedItem, _itemModels, UIRenderLayer.Ui
                    ));
                }


                {
                    var off = 60u;
                    _hotbarSlots = new UIInventorySlot[_player.Hotbar.Length];
                    for (var i = 0; i < _hotbarSlots.Length; i++)
                    {
                        var i1 = i;
                        _ui.Add(off, surface.Height - 60, _hotbarSlots[i] = new UIInventorySlot(
                            _player.Hotbar[i], _player.PickedItem, _itemModels, UIRenderLayer.Ui, 
                            () => _player.ActiveHotbarSlot == i1)
                        );
                        off += 100;
                    }
                }

                _ui.Add(0, 0, _pickedSlot = new UIUnboundInventorySlot(_player.PickedItem, _itemModels));
            }

            lock (_tickSource)
            {
                var camera = _player.GetCamera(_tickSource.CurrentTick.Value);
                var hit = RayCaster.Cast(_rayCastContext, camera.Ray);
                var physicalProjMat = Matrix4x4.CreatePerspectiveFieldOfView(
                    MathF.PI / 2, surface.Width / (float) surface.Height, 0.001f, 10000f
                );
                var renderProjMat = physicalProjMat * Matrix4x4.CreateRotationZ(MathF.PI);
                var viewFrustum = new ViewFrustum(camera.Transform * physicalProjMat);

                _worldRenderManager.UpdateChunks();
                
                using (var cmd = Resources.WorldCommandBuffer.Record(context, Resources.WorldFramebuffer.Format, BufferPool))
                {
                    cmd.SetViewportAndScissor(Resources.WorldFramebuffer);
                    cmd.Draw(Resources.ClearPipeline, Resources.CompVertexBuffer);
                    
                    _worldRenderManager.SubmitGeometry(context, cmd, renderProjMat, camera, viewFrustum);
                    
                    if (hit != null)
                    {
                        Resources.NativeOutlineUniformBuffer[0].Matrix =
                            Matrix4x4.CreateTranslation(hit.Position + Vector3.UnitY)
                            * camera.Transform * renderProjMat;
                        Resources.OutlineUniformBuffer.Write(Resources.NativeOutlineUniformBuffer);
                        
                        cmd.Using(Resources.OutlinePipeline, Resources.OutlineUniformBuffer, 0);
                        cmd.Draw(Resources.OutlinePipeline, Resources.OutlineVertexBuffer);
                    }
                }

                _positionLabel.Text = $"Position: {new BlockPos(_player.Position)}";
                _lookLabel.Text = $"Look: {hit?.Position}";

                if (_player.HotbarTransfer)
                {
                    _pickedSlot.PosX = (int) (60 + 100 * _player.ActiveHotbarSlot);
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
                context.Enqueue(Resources.UiFramebuffer, Resources.UiCommandBuffer);
                context.Enqueue(surface, Resources.CompCommandBuffer);
            }
        }

        public void SetKeyboardEventHandler(IUIElementContext.KeyboardEventDelegate? handler, Action? removeCallback = null)
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