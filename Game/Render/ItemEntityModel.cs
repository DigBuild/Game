using System;
using System.Collections.Generic;
using System.Numerics;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Items;
using DigBuild.Engine.Render;
using DigBuild.Entities;

namespace DigBuild.Render
{
    public sealed class ItemEntityModel : IEntityModel
    {
        private readonly IReadOnlyDictionary<Item, IItemModel> _itemModels;

        public ItemEntityModel(IReadOnlyDictionary<Item, IItemModel> itemModels)
        {
            _itemModels = itemModels;
        }

        private Matrix4x4 GetTransform(EntityInstance entity)
        {
            var joinWorldTime = entity.Type.Get(new EntityContext(entity), EntityAttributes.ItemJoinWorldTime)!.Value;

            const double rate = 0.25;
            var time = (float) ((DateTime.Now.Ticks - joinWorldTime) * rate % TimeSpan.TicksPerSecond / TimeSpan.TicksPerSecond);
            
            return Matrix4x4.CreateTranslation(-0.5f, MathF.Sin(time * MathF.PI * 2) - 0.5f, -0.5f) * 
                   Matrix4x4.CreateRotationY(time * MathF.PI * 2) *
                   Matrix4x4.CreateScale(0.25f) *
                   Matrix4x4.CreateTranslation(0, 0.5f, 0) *
                   Matrix4x4.CreateTranslation(entity.Type.Get(new EntityContext(entity), EntityAttributes.Position)!.Value);
        }

        public void AddGeometry(EntityInstance entity, GeometryBufferSet buffers)
        {
            var item = entity.Type.Get(new EntityContext(entity), EntityAttributes.Item)!;
            if (!_itemModels.TryGetValue(item.Type, out var model))
                return;

            buffers.Transform = GetTransform(entity) * buffers.Transform;
            model.AddGeometry(ItemModelTransform.None, buffers);
        }

        public bool HasDynamicGeometry => true;

        public void AddDynamicGeometry(EntityInstance entity, GeometryBufferSet buffers)
        {
            var item = entity.Type.Get(new EntityContext(entity), EntityAttributes.Item)!;
            if (!_itemModels.TryGetValue(item.Type, out var model) || !model.HasDynamicGeometry)
                return;

            buffers.Transform = GetTransform(entity) * buffers.Transform;
            model.AddDynamicGeometry(ItemModelTransform.None, buffers);
        }
    }
}