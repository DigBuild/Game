using System;
using System.Collections.Generic;
using System.Numerics;
using DigBuild.Controller;
using DigBuild.Engine.Items;
using DigBuild.Engine.Items.Inventories;
using DigBuild.Engine.Render;
using DigBuild.Engine.Render.Models;
using DigBuild.Engine.Textures;
using DigBuild.Engine.Ui;
using DigBuild.Items;
using DigBuild.Platform.Input;
using DigBuild.Platform.Render;
using DigBuild.Registries;

namespace DigBuild.Ui.Elements
{
    /// <summary>
    /// A basic inventory slot UI element.
    /// Supports transfering items in and out of the slot, as well as locking.
    /// </summary>
    public sealed class UiInventorySlot : IUiElement
    {
        /// <summary>
        /// The scaling factor.
        /// </summary>
        public const uint Scale = 32;

        internal static readonly Matrix4x4 ItemTransform =
            Matrix4x4.CreateTranslation(-Vector3.One / 2) * Matrix4x4.CreateRotationX(MathF.PI);

        private readonly IInventorySlot _slot, _pickedSlot;
        private readonly IReadOnlyDictionary<Item, IItemModel> _models;
        private readonly IRenderLayer<UiVertex> _layer;
        private readonly Func<bool>? _isActive;
        private readonly ITextRenderer _textRenderer;
        private bool _hovered;
        
        private readonly UiVertex[] _vertices, _verticesActive;

        public UiInventorySlot(
            IInventorySlot slot, IInventorySlot pickedSlot,
            IReadOnlyDictionary<Item, IItemModel> models,
            IRenderLayer<UiVertex> layer,
            ISprite? background, ISprite? backgroundActive = null,
            Func<bool>? isActive = null, ITextRenderer textRenderer = null!
        )
        {
            _slot = slot;
            _pickedSlot = pickedSlot;
            _models = models;
            _layer = layer;
            _isActive = isActive;
            _textRenderer = textRenderer ?? IUiElement.GlobalTextRenderer;
            
            _vertices = GenerateVertices(background);
            _verticesActive = GenerateVertices(backgroundActive);
        }

        public void Draw(RenderContext context, IGeometryBuffer buffer, float partialTick)
        {
            buffer.Get(_layer).Accept(_isActive != null && _isActive() ? _verticesActive : _vertices);

            var locked = _slot is ILockableInventorySlot { IsLocked: true };
            if (
                (locked || _slot.Item.Count > 0) &&
                _models.TryGetValue(_slot.Item.Type, out var model)
            )
            {
                var originalTransform = buffer.Transform;
                buffer.Transform = ItemTransform * Matrix4x4.CreateScale(Scale) * buffer.Transform;
                var modelData = _slot.Item.Get(ModelData.ItemAttribute);
                model.AddGeometry(buffer, modelData, ItemModelTransform.Inventory, partialTick);

                buffer.Transform = Matrix4x4.CreateTranslation(Scale / 6f, Scale / 2f, 0) * originalTransform;
                _textRenderer.DrawLine(buffer, $"{_slot.Item.Count,2:d2}", 3, locked);
            }
        }

        public void OnCursorMoved(IUiElementContext context, int x, int y)
        {
            _hovered = IsInsideHexagon(new Vector2(x, y), Vector2.Zero, Scale);
        }

        public void OnMouseEvent(IUiElementContext context, uint button, MouseAction action)
        {
            if (!_hovered || action != MouseAction.Press) return;


            switch (button)
            {
                case 0:
                    HandleLeftClick();
                    break;
                case 1:
                    HandleRightClick();
                    break;
                case 2:
                    HandleMiddleClick();
                    break;
            }
        }

        private void HandleLeftClick()
        {
            var current = _slot.Item;
            var currentPicked = _pickedSlot.Item;

            // Drop off item
            if (currentPicked.Count > 0)
            {
                if (current.Count == 0) // If the slot is empty, just put the item in
                {
                    if (_slot.TrySetItem(currentPicked))
                    {
                        _pickedSlot.TrySetItem(ItemInstance.Empty);
                        return;
                    }
                }
                else if (current.Equals(currentPicked, true)) // Ensure they're stackable
                {
                    var maxStackSize = current.Get(GameItemAttributes.MaxStackSize);

                    var newItem = current.Copy();
                    var total = current.Count + currentPicked.Count;
                    newItem.Count = (ushort)Math.Min(total, maxStackSize);

                    if (_slot.TrySetItem(newItem))
                    {
                        if (total > newItem.Count)
                            currentPicked.Count = (ushort)(total - newItem.Count);
                        else
                            _pickedSlot.TrySetItem(ItemInstance.Empty);
                        return;
                    }
                }
            }

            // If both slots are empty, skip
            if (current.Count <= 0)
                return;
            
            // Pick up or swap item
            if (currentPicked.Count > 0) // Swap
            {
                if (
                    _pickedSlot.TrySetItem(_slot.Item, false) &&
                    _slot.TrySetItem(currentPicked, false)
                )
                {
                    _pickedSlot.TrySetItem(_slot.Item);
                    _slot.TrySetItem(currentPicked);
                }
            }
            else // Pick up
            {
                if (_slot.TrySetItem(ItemInstance.Empty))
                {
                    _pickedSlot.TrySetItem(current);
                }
            }
        }

        private void HandleRightClick()
        {
            var current = _slot.Item;
            if (current.Count > 0)
            {
                var gc = (GameplayController)DigBuildGame.Instance.Controller;
                current.OnUse(gc.Player);
            }
        }

        private void HandleMiddleClick()
        {
            if (_slot is not ILockableInventorySlot lockable)
                return;

            if (lockable.IsLocked || lockable.Item.Count > 0)
                lockable.ToggleLocked();
        }

        private static bool IsInsideHexagon(Vector2 pos, Vector2 center, float radius)
        {
            var hor = radius * MathF.Cos(MathF.PI / 6);
            var vert = radius / 2;

            var q2X = MathF.Abs(pos.X - center.X);
            var q2Y = MathF.Abs(pos.Y - center.Y);
            return !(q2X > hor) && !(q2Y > vert * 2) && 2 * vert * hor - vert * q2X - hor * q2Y >= 0;
        }

        private static UiVertex[] GenerateVertices(ISprite? background)
        {
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

            var vertices = new UiVertex[3 * 2];

            vertices[0 * 3 + 0] = v1;
            vertices[0 * 3 + 1] = v2;
            vertices[0 * 3 + 2] = v3;

            vertices[1 * 3 + 0] = v3;
            vertices[1 * 3 + 1] = v4;
            vertices[1 * 3 + 2] = v1;

            return vertices;
        }
    }
}