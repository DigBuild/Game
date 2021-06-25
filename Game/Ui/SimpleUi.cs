using System;
using DigBuild.Engine.Render;
using DigBuild.Engine.Ui;
using DigBuild.Platform.Input;
using DigBuild.Platform.Render;

namespace DigBuild.Ui
{
    public class SimpleUi : IUi
    {
        private readonly IUiElement _root;
        private Context _context = null!;

        public bool PassEventsThrough { get; set; } = true;
        
        public Action? Resized = null;
        public Action? Closed = null;

        public Action? LayerAdded = null;
        public Action? LayerRemoved = null;

        public CursorMode CursorMode { get; set; } = CursorMode.Normal;

        public SimpleUi(IUiElement root)
        {
            _root = root;
        }

        public void OnOpened(UiManager manager)
        {
            _context = new Context(this, manager);
        }

        public void OnResized(IRenderTarget target)
        {
            Resized?.Invoke();
        }

        public void OnClosed()
        {
            Closed?.Invoke();
        }

        public void UpdateAndDraw(RenderContext context, IGeometryBuffer buffer, float partialTick)
        {
            _root.Draw(context, buffer, partialTick);
        }

        public bool OnCursorMoved(int x, int y)
        {
            _root.OnCursorMoved(_context, x, y);
            return !PassEventsThrough;
        }

        public bool OnMouseEvent(uint button, MouseAction action)
        {
            _root.OnMouseEvent(_context, button, action);
            return !PassEventsThrough;
        }

        public bool OnKeyboardEvent(uint code, KeyboardAction action)
        {
            if (_context.KeyboardEventDelegate != null)
            {
                _context.KeyboardEventDelegate(code, action);
                return true;
            }

            if (code == 1 && action == KeyboardAction.Press)
            {
                _context.RequestClose();
                return true;
            }

            return !PassEventsThrough;
        }

        public void OnLayerAdded()
        {
            LayerAdded?.Invoke();
        }

        public void OnLayerRemoved()
        {
            LayerRemoved?.Invoke();
        }

        public sealed class Context : IUiElementContext
        {
            private readonly IUi _owner;
            private readonly UiManager _manager;

            public IUiElementContext.KeyboardEventDelegate? KeyboardEventDelegate;
            private Action? _keyboardEventDelegateRemoveCallback;

            public Context(IUi owner, UiManager manager)
            {
                _owner = owner;
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
                _manager.Close(_owner);
            }
        }
    }
}