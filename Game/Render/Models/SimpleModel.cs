using System;
using DigBuild.Engine.Math;
using DigBuild.Engine.Render;

namespace DigBuild.Render.Models
{
    public sealed class SimpleModel : IBlockModel, IItemModel
    {
        private readonly WorldVertex[] _vertices;

        public SimpleModel(WorldVertex[] vertices)
        {
            _vertices = vertices;
        }

        public bool IsFaceSolid(Direction face) => false;

        public void AddGeometry(GeometryBufferSet buffers, IReadOnlyModelData data, Func<Direction, byte> light, DirectionFlags faces)
        {
            var consumer = buffers.Get(WorldRenderLayer.Opaque);
            consumer.Accept(_vertices);
        }

        public void AddGeometry(GeometryBufferSet buffers, IReadOnlyModelData data, ItemModelTransform transform, float partialTick)
        {
            buffers.Transform = transform.GetMatrix() * buffers.Transform;

            var consumer = buffers.Get(WorldRenderLayer.Opaque);
            consumer.Accept(_vertices);
        }
    }
}