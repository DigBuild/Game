using System.Collections.Generic;
using System.Numerics;
using DigBuild.Engine.Items;
using DigBuild.Engine.Items.Inventories;
using DigBuild.Engine.Render;
using DigBuild.Engine.Render.Models;
using DigBuild.Engine.Ui;
using DigBuild.Platform.Render;

namespace DigBuild.Ui.Elements
{
    public sealed class UiUnboundInventorySlot : IUiElement
    {
        private const uint Scale = UiInventorySlot.Scale;
        
        private readonly IInventorySlot _slot;
        private readonly IReadOnlyDictionary<Item, IItemModel> _models;
        private readonly ITextRenderer _textRenderer;

        public int PosX { get; set; }
        public int PosY { get; set; }

        public UiUnboundInventorySlot(IInventorySlot slot, IReadOnlyDictionary<Item, IItemModel> models, ITextRenderer textRenderer = null!)
        {
            _slot = slot;
            _models = models;
            _textRenderer = textRenderer ?? IUiElement.GlobalTextRenderer;
        }

        public void Draw(RenderContext context, IGeometryBuffer buffer, float partialTick)
        {
            if (_slot.Item.Count > 0 && _models.TryGetValue(_slot.Item.Type, out var model))
            {
                var originalTransform = Matrix4x4.CreateTranslation(PosX, PosY, Scale) * buffer.Transform;
                var transform = UiInventorySlot.ItemTransform *
                                Matrix4x4.CreateScale(Scale) *
                                originalTransform;
                buffer.Transform = transform;
                var modelData = _slot.Item.Get(ModelData.ItemAttribute);
                model.AddGeometry(buffer, modelData, ItemModelTransform.Inventory, partialTick);
                
                buffer.Transform = Matrix4x4.CreateTranslation(Scale / 6f, Scale / 2f, 0) * originalTransform;
                _textRenderer.DrawLine(buffer, $"{_slot.Item.Count,2:d2}", 3);
            }
        }

        public void OnCursorMoved(IUiElementContext context, int x, int y)
        {
            PosX = x;
            PosY = y;
        }
    }
}