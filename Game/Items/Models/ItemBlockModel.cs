using System;
using System.Numerics;
using DigBuild.Engine.Math;
using DigBuild.Engine.Render;

namespace DigBuild.Items.Models
{
    public sealed class ItemBlockModel : IItemModel
    {
        private static readonly Matrix4x4 Ortho = Matrix4x4.CreateRotationY(-MathF.PI / 4, Vector3.One / 2) *
                                                  Matrix4x4.CreateRotationX(MathF.PI - MathF.Asin(1 / MathF.Sqrt(3)), Vector3.One / 2);

        private static readonly Func<Direction, byte> FullBrightness = _ => 0xF;

        private readonly IBlockModel _parent;

        public ItemBlockModel(IBlockModel parent)
        {
            _parent = parent;
        }

        public void AddGeometry(GeometryBufferSet buffers, IReadOnlyModelData data, ItemModelTransform transform, float partialTick)
        {
            if (transform == ItemModelTransform.Inventory)
                buffers.Transform = Ortho * buffers.Transform;
            var modelData = new ModelData();
            _parent.AddGeometry(buffers, modelData, FullBrightness, DirectionFlags.All);
            if (_parent.HasDynamicGeometry)
                _parent.AddDynamicGeometry(buffers, modelData, FullBrightness, partialTick);
        }
    }
}