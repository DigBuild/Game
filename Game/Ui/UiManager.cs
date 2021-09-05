using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using DigBuild.Controller;
using DigBuild.Engine.BuiltIn.GeneratedUniforms;
using DigBuild.Engine.Render;
using DigBuild.Engine.Textures;
using DigBuild.Engine.Ui;
using DigBuild.Events;
using DigBuild.Platform.Input;
using DigBuild.Platform.Render;
using DigBuild.Platform.Resource;
using DigBuild.Platform.Util;
using DigBuild.Render;
using DigBuild.Render.GeneratedUniforms;

namespace DigBuild.Ui
{
    /// <summary>
    /// The user interface manager.
    /// </summary>
    public sealed class UiManager
    {
        /// <summary>
        /// The gameplay controller.
        /// </summary>
        public GameplayController Controller { get; }

        private readonly IEnumerable<IRenderLayer> _layers;
        private readonly NativeBufferPool _bufferPool;

        private readonly TextureSet _textureSet;

        private readonly GeometryBuffer _geometryBuffer;
        private readonly UniformBufferSet _uniforms;
        private readonly RenderLayerBindingSet _bindingSet = new();

        private CommandBuffer _commandBuffer = null!;
        private Framebuffer _framebuffer = null!;
        
        private readonly Stack<IUi> _uis = new();
        
        /// <summary>
        /// The user interface stack.
        /// </summary>
        public IEnumerable<IUi> Uis => _uis;

        /// <summary>
        /// The current cursor mode as dictated by the UIs.
        /// </summary>
        public CursorMode CursorMode => _uis.Peek().CursorMode;

        public UiManager(
            GameplayController controller,
            IEnumerable<IRenderLayer> layers,
            IEnumerable<IUniformType> uniforms,
            NativeBufferPool bufferPool
        )
        {
            Controller = controller;
            _layers = layers;
            _bufferPool = bufferPool;

            _textureSet = new TextureSet(this);
            
            _geometryBuffer = new GeometryBuffer(bufferPool);
            _uniforms = new UniformBufferSet(uniforms, bufferPool);
            
            Open(new GameHud(Controller));
            Open(MenuUi.Create());
        }

        private void UpdatePauseState()
        {
            Controller.Game.TickSource.Paused = _uis.Any(ui => ui.Pause);
        }

        /// <summary>
        /// Opens a new UI.
        /// </summary>
        /// <param name="ui">The UI</param>
        public void Open(IUi ui)
        {
            var hasTop = _uis.TryPeek(out var top);

            _uis.Push(ui);

            ui.OnOpened(this);
            if (_framebuffer != null)
                ui.OnResized(_framebuffer);

            if (hasTop)
                top!.OnLayerAdded();

            UpdatePauseState();
        }

        /// <summary>
        /// Closes a UI and all the ones opened after it.
        /// </summary>
        /// <param name="ui">The UI</param>
        public void Close(IUi ui)
        {
            if (!_uis.Contains(ui))
                throw new Exception("Attempted to close invalid UI");

            while (_uis.Count > 1 && _uis.TryPop(out var top) && top != ui)
                top.OnClosed();

            if (_uis.TryPeek(out var top2))
                top2.OnLayerRemoved();

            UpdatePauseState();
        }

        /// <summary>
        /// Closes the topmost UI.
        /// </summary>
        public void CloseTop()
        {
            if (_uis.Count > 1)
                Close(_uis.First());
        }

        /// <summary>
        /// Sets up all the render resources for UI rendering.
        /// </summary>
        /// <param name="context">The render context</param>
        /// <param name="resourceManager">The resource manager</param>
        /// <param name="renderStage">The render stage</param>
        public void Setup(RenderContext context, ResourceManager resourceManager, RenderStage renderStage)
        {
            _commandBuffer = context.CreateCommandBuffer();

            _textureSet.DefaultSampler = context.CreateTextureSampler(TextureFiltering.Linear, TextureFiltering.Nearest);
            
            var fontResource = resourceManager.Get<BitmapTexture>(DigBuildGame.Domain, "textures/font.png")!;
            _textureSet.FontTexture = context.CreateTexture(fontResource.Bitmap);

            var uiStitcher = new TextureStitcher();
            uiStitcher.Add(resourceManager.GetResource(DigBuildGame.Domain, "textures/ui/white.png")!);
            Controller.Game.EventBus.Post(new TextureStitchingEvent(TextureTypes.UiMain, uiStitcher, resourceManager));
            _textureSet.UiTexture = context.CreateTexture(uiStitcher.Stitch(new ResourceName(DigBuildGame.Domain, "ui_texturemap")).Bitmap);

            IUiElement.GlobalTextRenderer = new TextRenderer(UiRenderLayers.Text);
            
            _uniforms.Setup(context);
        }

        /// <summary>
        /// Updates the framebuffer when the surface is created or resized.
        /// </summary>
        /// <param name="context">The render context</param>
        /// <param name="format">The framebuffer format</param>
        /// <param name="width">The width</param>
        /// <param name="height">The height</param>
        /// <returns>The framebuffer</returns>
        public Framebuffer UpdateFramebuffer(RenderContext context, FramebufferFormat format, uint width, uint height)
        {
            _framebuffer = context.CreateFramebuffer(format, width, height);
            foreach (var ui in _uis)
                ui.OnResized(_framebuffer);
            return _framebuffer;
        }

        /// <summary>
        /// Initializes the render layer bindings.
        /// </summary>
        /// <param name="context">The render context</param>
        public void InitLayerBindings(RenderContext context)
        {
            foreach (var layer in _layers)
                layer.InitBindings(context, _bindingSet);
        }

        /// <summary>
        /// Updates the UIs and renders them.
        /// </summary>
        /// <param name="context">The render context</param>
        /// <param name="partialTick">The tick delta</param>
        public void UpdateAndRender(RenderContext context, float partialTick)
        {
            _geometryBuffer.Clear();
            _geometryBuffer.Transform = Matrix4x4.Identity;
            _geometryBuffer.TransformNormal = false;
            {
                foreach (var ui in _uis.Reverse())
                    ui.UpdateAndDraw(context, _geometryBuffer, partialTick);
            }
            _geometryBuffer.Upload(context);
            
            var uiProjection = Matrix4x4.CreateOrthographic(_framebuffer.Width, _framebuffer.Height, -100, 100) *
                               Matrix4x4.CreateTranslation(-1, -1, 0);

            _uniforms.Clear();
            _uniforms.Push(UniformTypes.ModelViewProjectionTransform, new SimpleTransform
            {
                ModelView = Matrix4x4.Identity,
                Projection = uiProjection
            });
            _uniforms.Push(UniformTypes.WorldTime, new WorldTimeUniform
            {
                WorldTime = 1
            });

            using (var cmd = _commandBuffer.Record(context, _framebuffer.Format, _bufferPool))
            {
                cmd.SetViewportAndScissor(_framebuffer);
                
                foreach (var layer in _layers)
                {
                    layer.SetupCommand(cmd, _bindingSet, _uniforms, _textureSet);
                    _geometryBuffer.Draw(cmd, layer, _bindingSet, _uniforms);
                }
            }

            _uniforms.Upload(context);
            
            context.Enqueue(_framebuffer, _commandBuffer);
        }

        /// <summary>
        /// Handles cursor movement.
        /// </summary>
        /// <param name="x">The cursor X coordinate</param>
        /// <param name="y">The cursor Y coordinate</param>
        /// <param name="action">The cursor action</param>
        public void OnCursorMoved(uint x, uint y, CursorAction action)
        {
            foreach (var ui in _uis)
                if (ui.OnCursorMoved((int)x, (int)y))
                    break;
        }

        /// <summary>
        /// Handles mouse button events.
        /// </summary>
        /// <param name="button">The button</param>
        /// <param name="action">The action</param>
        public void OnMouseEvent(uint button, MouseAction action)
        {
            foreach (var ui in _uis)
                if (ui.OnMouseEvent(button, action))
                    break;
        }

        /// <summary>
        /// Handles mouse scroll events.
        /// </summary>
        /// <param name="xOffset">The scroll X offset</param>
        /// <param name="yOffset">The scroll Y offset</param>
        public void OnScrollEvent(double xOffset, double yOffset)
        {
            foreach (var ui in _uis)
                if (ui.OnScrollEvent(xOffset, yOffset))
                    break;
        }

        /// <summary>
        /// Handles keyboard events.
        /// </summary>
        /// <param name="code">The keycode</param>
        /// <param name="action">The action</param>
        public void OnKeyboardEvent(uint code, KeyboardAction action)
        {
            foreach (var ui in _uis)
                if (ui.OnKeyboardEvent(code, action))
                    break;
        }

        private sealed class TextureSet : IReadOnlyTextureSet
        {
            private readonly UiManager _manager;

            public TextureSampler DefaultSampler { get; internal set; } = null!;

            public Texture UiTexture { get; internal set; } = null!;
            public Texture FontTexture { get; internal set; } = null!;

            public TextureSet(UiManager manager)
            {
                _manager = manager;
            }

            public Texture Get(TextureType textureType)
            {
                if (textureType == TextureTypes.UiMain)
                    return UiTexture;
                if (textureType == TextureTypes.UiText)
                    return FontTexture;
                return _manager.Controller.Textures.Get(textureType);
            }
        }
    }
}