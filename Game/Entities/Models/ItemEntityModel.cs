using System;
using System.Collections.Generic;
using System.Numerics;
using DigBuild.Behaviors;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Items;
using DigBuild.Engine.Render;
using DigBuild.Registries;

namespace DigBuild.Entities.Models
{
    public sealed class ItemEntityModel : IEntityModel
    {
        private readonly IReadOnlyDictionary<Item, IItemModel> _itemModels;

        public ItemEntityModel(IReadOnlyDictionary<Item, IItemModel> itemModels)
        {
            _itemModels = itemModels;
        }

        private Matrix4x4 GetTransform(long joinWorldTime, Vector3 position)
        {
            const double rate = 0.25;
            var time = (float) ((DateTime.Now.Ticks - joinWorldTime) * rate % TimeSpan.TicksPerSecond / TimeSpan.TicksPerSecond);
            
            return Matrix4x4.CreateTranslation(-0.5f, MathF.Sin(time * MathF.PI * 2) - 0.5f, -0.5f) * 
                   Matrix4x4.CreateRotationY(time * MathF.PI * 2) *
                   Matrix4x4.CreateScale(0.25f) *
                   Matrix4x4.CreateTranslation(0, 0.5f, 0) *
                   Matrix4x4.CreateTranslation(position);
        }

        public void AddGeometry(GeometryBufferSet buffers, IReadOnlyModelData data, float partialTick)
        {
            var position = data.Get<PhysicalEntityModelData>()!.Position;
            var itemInfo = data.Get<ItemEntityModelData>()!;
            
            if (!_itemModels.TryGetValue(itemInfo.Item.Type, out var model))
                return;
            
            buffers.Transform = GetTransform(itemInfo.JoinWorldTime, position) * buffers.Transform;
            model.AddGeometry(buffers, data, ItemModelTransform.None, partialTick);
        }
    }
}