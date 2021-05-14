using System;
using DigBuild.Engine.Math;
using DigBuild.Engine.Render;

namespace DigBuild.Render.Models
{
    public sealed class ItemBlockModel : IItemModel
    {
        private static readonly Func<Direction, byte> FullBrightness = _ => 0xF;

        private readonly IBlockModel _parent;

        public ItemBlockModel(IBlockModel parent)
        {
            _parent = parent;
        }

        public void AddGeometry(GeometryBufferSet buffers, IReadOnlyModelData data, ItemModelTransform transform, float partialTick)
        {
            buffers.Transform = transform.GetMatrix() * buffers.Transform;

            var modelData = new ModelData();
            _parent.AddGeometry(buffers, modelData, FullBrightness, DirectionFlags.All);
            if (_parent.HasDynamicGeometry)
                _parent.AddDynamicGeometry(buffers, modelData, FullBrightness, partialTick);
        }
    }
}