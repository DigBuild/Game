using System;
using System.Collections.Generic;
using DigBuild.Engine.Math;
using DigBuild.Engine.Render;
using DigBuild.Render;

namespace DigBuild.Client.Models
{
    public sealed class CuboidModel : IBlockModel, IItemModel
    {
        private readonly SimpleVertex[][] _vertices = new SimpleVertex[6][];
        private readonly bool _solid;
        private readonly RenderLayer<SimpleVertex> _layer;

        public CuboidModel(IReadOnlyDictionary<Direction, List<SimpleVertex>> vertices, bool solid, RenderLayer<SimpleVertex> layer)
        {
            foreach (var direction in Directions.All)
                _vertices[(int) direction] = vertices[direction].ToArray();
            _solid = solid;
            _layer = layer;
        }

        public bool IsFaceSolid(Direction face) => _solid;

        public void AddGeometry(GeometryBufferSet buffers, IReadOnlyModelData data, Func<Direction, byte> light, DirectionFlags faces)
        {
            var buf = buffers.Get(_layer);
            foreach (var face in Directions.In(faces))
                buf.Accept(_vertices[(int) face].WithBrightness(light(face) / 15f));
        }

        public void AddGeometry(GeometryBufferSet buffers, IReadOnlyModelData data, ItemModelTransform transform, float partialTick)
        {
            buffers.Transform = transform.GetMatrix() * buffers.Transform;

            var buf = buffers.Get(_layer);
            foreach (var face in Directions.All)
                buf.Accept(_vertices[(int) face].WithBrightness(1));
        }
    }
}