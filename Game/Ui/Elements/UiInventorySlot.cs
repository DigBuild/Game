using System;
using System.Collections.Generic;
using System.Numerics;
using DigBuild.Engine.Items;
using DigBuild.Engine.Render;
using DigBuild.Engine.Render.Models;
using DigBuild.Engine.Textures;
using DigBuild.Engine.Ui;
using DigBuild.Platform.Input;
using DigBuild.Platform.Render;

namespace DigBuild.Ui.Elements
{

    // Left click   - Pick up/swap
    // Right click  - Use
    // Middle click - Lock
    // Scroll       - Pick up/drop 1
    public sealed class UiInventorySlot : IUiElement
    {
        public const uint Scale = 32;

        internal static readonly Matrix4x4 ItemTransform = Matrix4x4.CreateTranslation(-Vector3.One / 2) * Matrix4x4.CreateRotationX(MathF.PI);
        
        private readonly IInventorySlot _slot, _pickedSlot;
        private readonly IReadOnlyDictionary<Item, IItemModel> _models;
        private readonly IRenderLayer<UiVertex> _layer;
        private readonly Func<bool>? _isActive;
        private readonly TextRenderer _textRenderer;
        private bool _hovered;
        
        private readonly UiVertex[] _vertices = new UiVertex[3 * 2];

        public UiInventorySlot(
            IInventorySlot slot, IInventorySlot pickedSlot,
            IReadOnlyDictionary<Item, IItemModel> models,
            IRenderLayer<UiVertex> layer, ISprite? background,
            Func<bool>? isActive = null, TextRenderer textRenderer = null!
        )
        {
            _slot = slot;
            _pickedSlot = pickedSlot;
            _models = models;
            _layer = layer;
            _isActive = isActive;
            _textRenderer = textRenderer ?? IUiElement.GlobalTextRenderer;
            
            var v1 = new UiVertex(
                new Vector2(-Scale, -Scale),
                background?.GetInterpolatedUV(0, 0) ?? Vector2.Zero,
                Vector4.One
            );
            var v2 = new UiVertex(
                new Vector2(Scale, -Scale),
                background?.GetInterpolatedUV(1, 0) ?? Vector2.Zero,
                Vector4.One
            );
            var v3 = new UiVertex(
                new Vector2(Scale, Scale),
                background?.GetInterpolatedUV(1, 1) ?? Vector2.Zero,
                Vector4.One
            );
            var v4 = new UiVertex(
                new Vector2(-Scale, Scale),
                background?.GetInterpolatedUV(0, 1) ?? Vector2.Zero,
                Vector4.One
            );
            
            _vertices[0 * 3 + 0] = v1;
            _vertices[0 * 3 + 1] = v2;
            _vertices[0 * 3 + 2] = v3;
            
            _vertices[1 * 3 + 0] = v3;
            _vertices[1 * 3 + 1] = v4;
            _vertices[1 * 3 + 2] = v1;
        }

        public void Draw(RenderContext context, IGeometryBuffer buffer, float partialTick)
        {
            buffer.Get(_layer).Accept(_vertices);
            // if (_isActive != null && _isActive())
            //     buffer.Get(_layer).Accept(MarkerVertices);
            
            if (_slot.Item.Count > 0 && _models.TryGetValue(_slot.Item.Type, out var model))
            {
                var originalTransform = buffer.Transform;
                buffer.Transform = ItemTransform * Matrix4x4.CreateScale(Scale) * buffer.Transform;
                var modelData = _slot.Item.Get(ModelData.ItemAttribute);
                model.AddGeometry(buffer, modelData, ItemModelTransform.Inventory, partialTick);
                
                buffer.Transform = Matrix4x4.CreateTranslation(Scale / 6f, Scale / 2f, 0) * originalTransform;
                _textRenderer.DrawLine(buffer, $"{_slot.Item.Count,2:d2}", 3);
            }
        }

        public void OnCursorMoved(IUiElementContext context, int x, int y)
        {
            _hovered = IsInsideHexagon(new Vector2(x, y), Vector2.Zero, Scale);
        }

        public void OnMouseEvent(IUiElementContext context, uint button, MouseAction action)
        {
            if (!_hovered || button != 0 || action != MouseAction.Press) return;

            var currentPicked = _pickedSlot.Item;
            if (_pickedSlot.TrySetItem(_slot.Item, false) && _slot.TrySetItem(currentPicked, false))
            {
                _pickedSlot.TrySetItem(_slot.Item);
                _slot.TrySetItem(currentPicked);
            }
        }

        private static bool IsInsideHexagon(Vector2 pos, Vector2 center, float radius)
        {
            var _hori = radius * MathF.Cos(MathF.PI / 6);
            var _vert = radius / 2;

            var q2x = MathF.Abs(pos.X - center.X);
            var q2y = MathF.Abs(pos.Y - center.Y);
            if (q2x > _hori || q2y > _vert * 2) return false;
            return 2 * _vert * _hori - _vert * q2x - _hori * q2y >= 0;
        }
    }
}