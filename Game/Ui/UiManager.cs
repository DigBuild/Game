using System;
using System.Collections.Generic;
using System.Numerics;
using DigBuild.Controller;
using DigBuild.Engine.BuiltIn.GeneratedUniforms;
using DigBuild.Engine.Render;
using DigBuild.Engine.Textures;
using DigBuild.Engine.Ui;
using DigBuild.Platform.Input;
using DigBuild.Platform.Render;
using DigBuild.Platform.Resource;
using DigBuild.Platform.Util;
using DigBuild.Render;

namespace DigBuild.Ui
{
    public sealed class UiManager
    {
        private readonly GameplayController _controller;
        private readonly IEnumerable<IRenderLayer> _layers;
        private readonly NativeBufferPool _bufferPool;

        private readonly TextureSet _textureSet;

        private readonly GeometryBuffer _geometryBuffer;
        private readonly UniformBufferSet _uniforms;

        private CommandBuffer _commandBuffer = null!;
        private Framebuffer _framebuffer = null!;
        
        private readonly GameHud _hud;
        private IUiElement? _ui;
        private Context _uiContext = null!;

        public IUiElement? Ui
        {
            get => _ui;
            set
            {
                _ui = value;
                _uiContext = value != null ? new Context(this) : null!;
            }
        }

        public UiManager(
            GameplayController controller,
            IEnumerable<IRenderLayer> layers,
            IEnumerable<IRenderUniform> uniforms,
            NativeBufferPool bufferPool
        )
        {
            _controller = controller;
            _layers = layers;
            _bufferPool = bufferPool;

            _textureSet = new TextureSet(this);

            _hud = new GameHud(controller);

            _geometryBuffer = new GeometryBuffer(bufferPool);
            _uniforms = new UniformBufferSet(uniforms, bufferPool);

            Ui = MenuUi.Create();
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
                return _manager._controller.Textures.Get(texture);
            }
        }

        private void CloseUi()
        {
            Ui = null;
        }

        public void Setup(RenderContext context, ResourceManager resourceManager, RenderStage renderStage)
        {
            _commandBuffer = context.CreateCommandBuffer();

            _textureSet.DefaultSampler = context.CreateTextureSampler(TextureFiltering.Linear, TextureFiltering.Nearest);
            
            var fontResource = resourceManager.Get<BitmapTexture>(DigBuildGame.Domain, "textures/font.png")!;
            _textureSet.FontTexture = context.CreateTexture(fontResource.Bitmap);

            var uiStitcher = new TextureStitcher();
            uiStitcher.Add(resourceManager.GetResource(DigBuildGame.Domain, "textures/ui/white.png")!);
            _textureSet.UiTexture = context.CreateTexture(uiStitcher.Stitch(new ResourceName(DigBuildGame.Domain, "ui_texturemap")).Bitmap);

            IUiElement.GlobalTextRenderer = new TextRenderer(UiRenderLayer.Text);
            
            _uniforms.Setup(context);
        }

        public Framebuffer UpdateFramebuffer(RenderContext context, FramebufferFormat format, uint width, uint height)
        {
            _framebuffer = context.CreateFramebuffer(format, width, height);
            _hud.Setup(_framebuffer);
            return _framebuffer;
        }

        public void UpdateAndRender(RenderContext context, float partialTick)
        {
            _geometryBuffer.Clear();
            _geometryBuffer.Transform = Matrix4x4.Identity;
            _geometryBuffer.TransformNormal = false;
            {
                _hud.UpdateAndDraw(context, _geometryBuffer, partialTick);
                Ui?.Draw(context, _geometryBuffer, partialTick);
            }
            _geometryBuffer.Upload(context);
            
            var uiProjection = Matrix4x4.CreateOrthographic(_framebuffer.Width, _framebuffer.Height, -100, 100) *
                               Matrix4x4.CreateTranslation(-1, -1, 0);

            _uniforms.Clear();
            _uniforms.Push(RenderUniforms.ModelViewTransform, new SimpleTransform { Matrix = uiProjection });

            using (var cmd = _commandBuffer.Record(context, _framebuffer.Format, _bufferPool))
            {
                cmd.SetViewportAndScissor(_framebuffer);
                
                // foreach (var layer in _layers)
                // {
                //     layer.SetupCommand(cmd, _uniforms, _textureSet);
                //     _geometryBuffer.Draw(layer, cmd, _uniforms);
                // }
            }

            _uniforms.Upload(context);
            
            context.Enqueue(_framebuffer, _commandBuffer);
        }

        public bool OnCursorMoved(uint x, uint y, CursorAction action)
        {
            if (Ui == null) return false;
            Ui.OnCursorMoved(_uiContext, (int) x, (int) y);
            _hud.OnCursorMove(_uiContext, (int) x, (int) y);
            return true;
        }

        public bool OnMouseEvent(uint button, MouseAction action)
        {
            if (Ui == null) return false;
            Ui.OnMouseEvent(_uiContext, button, action);
            _hud.OnMouseEvent(_uiContext, button, action);
            return true;
        }

        public bool OnKeyboardEvent(uint code, KeyboardAction action)
        {
            if (code == 1 && action == KeyboardAction.Press)
            {
                Ui = Ui != null ? null : MenuUi.Create();
                return true;
            }
            
            if (Ui == null) return false;
            _uiContext.KeyboardEventDelegate?.Invoke(code, action);
            return true;
        }

        private sealed class Context : IUiElementContext
        {
            private readonly UiManager _manager;

            public IUiElementContext.KeyboardEventDelegate? KeyboardEventDelegate;
            private Action? _keyboardEventDelegateRemoveCallback;

            public Context(UiManager manager)
            {
                _manager = manager;
            }

            public void SetKeyboardEventHandler(IUiElementContext.KeyboardEventDelegate? handler, Action? removeCallback = null)
            {
                KeyboardEventDelegate = handler;
                _keyboardEventDelegateRemoveCallback?.Invoke();
                _keyboardEventDelegateRemoveCallback = removeCallback;
            }

            public void RequestRedraw()
            {
            }

            public void RequestClose()
            {
                _manager.CloseUi();
            }
        }
    }
}