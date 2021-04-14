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

        private readonly IBlockModel _parent;

        public ItemBlockModel(IBlockModel parent)
        {
            _parent = parent;
        }

        public void AddGeometry(ItemModelTransform transform, GeometryBufferSet buffers)
        {
            if (transform == ItemModelTransform.Inventory)
                buffers.Transform = Ortho * buffers.Transform;
            _parent.AddGeometry(DirectionFlags.All, buffers, _ => 0xF);
        }

        public bool HasDynamicGeometry => _parent.HasDynamicGeometry;

        public void AddDynamicGeometry(ItemModelTransform transform, GeometryBufferSet buffers)
        {
            if (transform == ItemModelTransform.Inventory)
                buffers.Transform = Ortho * buffers.Transform;
            _parent.AddDynamicGeometry(buffers, _ => 0xF);
        }
    }
}