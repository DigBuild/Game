﻿using System;
using System.Numerics;
using DigBuild.Engine.Render;

namespace DigBuild.Render
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

        public void AddGeometry(GeometryBufferSet buffers)
        {
            buffers.Transform = Ortho * buffers.Transform;
            _parent.AddGeometry(BlockFaceFlags.All, buffers);
        }

        public bool HasDynamicGeometry => _parent.HasDynamicGeometry;

        public void AddDynamicGeometry(GeometryBufferSet buffers)
        {
            buffers.Transform = Ortho * buffers.Transform;
            _parent.AddDynamicGeometry(buffers);
        }
    }
}