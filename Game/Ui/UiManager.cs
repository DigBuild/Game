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
    public sealed class UiManager
    {
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

        public CursorMode CursorMode => _uis.Peek().CursorMode;

        public UiManager(
            GameplayController controller,
            IEnumerable<IRenderLayer> layers,
            IEnumerable<IRenderUniform> uniforms,
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

        public void Open(IUi ui)
        {
            var hasTop = _uis.TryPeek(out var top);

            _uis.Push(ui);

            ui.OnOpened(this);
            if (_framebuffer != null)
                ui.OnResized(_framebuffer);

            if (hasTop)
                top!.OnLayerAdded();
        }

        public void Close(IUi ui)
        {
            if (!_uis.Contains(ui))
                throw new Exception("Attempted to close invalid UI");

            while (_uis.Count > 1 && _uis.TryPop(out var top) && top != ui)
                top.OnClosed();

            if (_uis.TryPeek(out var top2))
                top2.OnLayerRemoved();
        }

        public void Setup(RenderContext context, ResourceManager resourceManager, RenderStage renderStage)
        {
            _commandBuffer = context.CreateCommandBuffer();

            _textureSet.DefaultSampler = context.CreateTextureSampler(TextureFiltering.Linear, TextureFiltering.Nearest);
            
            var fontResource = resourceManager.Get<BitmapTexture>(DigBuildGame.Domain, "textures/font.png")!;
            _textureSet.FontTexture = context.CreateTexture(fontResource.Bitmap);

            var uiStitcher = new TextureStitcher();
            uiStitcher.Add(resourceManager.GetResource(DigBuildGame.Domain, "textures/ui/white.png")!);
            Controller.Game.EventBus.Post(new UiTextureStitchingEvent(uiStitcher, resourceManager));
            _textureSet.UiTexture = context.CreateTexture(uiStitcher.Stitch(new ResourceName(DigBuildGame.Domain, "ui_texturemap")).Bitmap);

            IUiElement.GlobalTextRenderer = new TextRenderer(UiRenderLayer.Text);
            
            _uniforms.Setup(context);
        }

        public Framebuffer UpdateFramebuffer(RenderContext context, FramebufferFormat format, uint width, uint height)
        {
            _framebuffer = context.CreateFramebuffer(format, width, height);
            foreach (var ui in _uis)
                ui.OnResized(_framebuffer);
            return _framebuffer;
        }

        public void InitLayerBindings(RenderContext context)
        {
            foreach (var layer in _layers)
                layer.InitBindings(context, _bindingSet);
        }

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
            _uniforms.Push(RenderUniforms.ModelViewTransform, new SimpleTransform
            {
                ModelView = Matrix4x4.Identity,
                Projection = uiProjection
            });
            _uniforms.Push(RenderUniforms.WorldTime, new WorldTimeUniform
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

        public void OnCursorMoved(uint x, uint y, CursorAction action)
        {
            foreach (var ui in _uis)
                if (ui.OnCursorMoved((int)x, (int)y))
                    break;
        }

        public void OnMouseEvent(uint button, MouseAction action)
        {
            foreach (var ui in _uis)
                if (ui.OnMouseEvent(button, action))
                    break;
        }

        public void OnKeyboardEvent(uint code, KeyboardAction action)
        {
            foreach (var ui in _uis)
                if (ui.OnKeyboardEvent(code, action))
                    break;
        }

        public sealed class TextureSet : IReadOnlyTextureSet
        {
            private readonly UiManager _manager;

            public TextureSampler DefaultSampler { get; internal set; } = null!;

            public Texture UiTexture { get; internal set; } = null!;
            public Texture FontTexture { get; internal set; } = null!;

            public TextureSet(UiManager manager)
            {
                _manager = manager;
            }

            public Texture Get(RenderTexture texture)
            {
                if (texture == RenderTextures.UiMain)
                    return UiTexture;
                if (texture == RenderTextures.UiText)
                    return FontTexture;
                return _manager.Controller.Textures.Get(texture);
            }
        }
    }
}