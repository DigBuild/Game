using System;
using System.Collections.Generic;
using System.Numerics;
using DigBuild.Controller;
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
        public static Texture FontTexture { get; private set; } = null!;
        public static Texture UiTexture { get; private set; } = null!;
        public static ISprite UiWhiteSprite { get; private set; } = null!;

        private readonly IEnumerable<IRenderLayer> _layers;
        private readonly NativeBufferPool _bufferPool;

        private readonly GeometryBufferSet _uiGbs;
        private readonly UniformBufferSet _uiUbs;

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
        
        public UiManager(GameplayController controller, IEnumerable<IRenderLayer> layers, NativeBufferPool bufferPool)
        {
            _layers = layers;
            _bufferPool = bufferPool;

            _hud = new GameHud(controller);

            _uiGbs = new GeometryBufferSet(bufferPool);
            _uiUbs = new UniformBufferSet(bufferPool);

            Ui = MenuUi.Create();
        }

        private void CloseUi()
        {
            Ui = null;
        }

        public void Setup(RenderContext context, ResourceManager resourceManager, RenderStage renderStage)
        {
            _commandBuffer = context.CreateCommandBuffer();
            
            var fontResource = resourceManager.Get<BitmapTexture>(DigBuildGame.Domain, "textures/font.png")!;
            FontTexture = context.CreateTexture(fontResource.Bitmap);

            var uiStitcher = new TextureStitcher();
            UiWhiteSprite = uiStitcher.Add(resourceManager.GetResource(DigBuildGame.Domain, "textures/ui/white.png")!);
            UiTexture = context.CreateTexture(uiStitcher.Stitch(new ResourceName(DigBuildGame.Domain, "ui_texturemap")).Bitmap);

            IUiElement.GlobalTextRenderer = new TextRenderer(UiRenderLayer.Text);
        }

        public Framebuffer UpdateFramebuffer(RenderContext context, FramebufferFormat format, uint width, uint height)
        {
            _framebuffer = context.CreateFramebuffer(format, width, height);
            _hud.Setup(_framebuffer);
            return _framebuffer;
        }

        public void UpdateAndRender(RenderContext context, float partialTick)
        {
            _uiGbs.Clear();
            _uiGbs.Transform = Matrix4x4.Identity;
            _uiGbs.TransformNormal = false;
            {
                _hud.UpdateAndDraw(context, _uiGbs, partialTick);
                Ui?.Draw(context, _uiGbs, partialTick);
            }
            _uiGbs.Upload(context);

            
            var uiProjection = Matrix4x4.CreateOrthographic(_framebuffer.Width, _framebuffer.Height, -100, 100) *
                               Matrix4x4.CreateTranslation(-1, -1, 0);
            using (var cmd = _commandBuffer.Record(context, _framebuffer.Format, _bufferPool))
            {
                cmd.SetViewportAndScissor(_framebuffer);
                _uiUbs.Clear();
                // _uiUbs.Setup(context, cmd);
                foreach (var layer in _layers)
                {
                    layer.InitializeCommand(cmd);
                        
                    _uiUbs.AddAndUse(context, cmd, layer, uiProjection);
                    _uiGbs.Draw(layer, context, cmd);
                }
                _uiUbs.Finalize(context, cmd);
            }
            
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