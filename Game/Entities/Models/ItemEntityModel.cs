using System;
using System.Collections.Generic;
using System.Numerics;
using DigBuild.Behaviors;
using DigBuild.Engine.Items;
using DigBuild.Engine.Render;
using DigBuild.Engine.Render.Models;

namespace DigBuild.Entities.Models
{
    /// <summary>
    /// The model for item entities.
    /// Consumes <see cref="PhysicalEntityModelData"/> and <see cref="ItemEntityModelData"/>.
    /// <para>
    /// Spins and bobs in place constantly.
    /// </para>
    /// </summary>
    public sealed class ItemEntityModel : IEntityModel
    {
        private readonly IReadOnlyDictionary<Item, IItemModel> _itemModels;

        public ItemEntityModel(IReadOnlyDictionary<Item, IItemModel> itemModels)
        {
            _itemModels = itemModels;
        }

        private Matrix4x4 GetTransform(ulong worldTime, ulong joinWorldTime, float partialTick, Vector3 position)
        {
            const float rate = 0.02f;
            var time = (float) (((double) (worldTime - joinWorldTime) + partialTick)) * rate;
            
            return Matrix4x4.CreateTranslation(-0.5f, MathF.Sin(time * MathF.PI * 2) - 0.5f, -0.5f) * 
                   Matrix4x4.CreateRotationY(time * MathF.PI * 2) *
                   Matrix4x4.CreateScale(0.25f) *
                   Matrix4x4.CreateTranslation(0, 0.5f, 0) *
                   Matrix4x4.CreateTranslation(position);
        }

        public void AddGeometry(IGeometryBuffer buffer, IReadOnlyModelData data, float partialTick)
        {
            var physicalEntityModelData = data.Get<PhysicalEntityModelData>()!;
            var position = physicalEntityModelData.Position + physicalEntityModelData.Velocity * partialTick;
            var itemInfo = data.Get<ItemEntityModelData>()!;
            
            if (!_itemModels.TryGetValue(itemInfo.Item.Type, out var model))
                return;
            
            buffer.Transform = GetTransform(itemInfo.WorldTime, itemInfo.JoinWorldTime, partialTick, position) * buffer.Transform;
            var itemData = itemInfo.Item.Get(ModelData.ItemAttribute);
            model.AddGeometry(buffer, itemData, ItemModelTransform.None, partialTick);
        }
    }
}