using System;
using System.Collections.Generic;
using DigBuild.Engine.Math;
using DigBuild.Engine.Render;

namespace DigBuild.Render.Models
{
    public sealed class CuboidModel : IBlockModel, IItemModel
    {
        private readonly WorldVertex[][] _vertices = new WorldVertex[6][];
        private readonly bool _solid;
        private readonly RenderLayer<WorldVertex> _layer;

        public CuboidModel(IReadOnlyDictionary<Direction, List<WorldVertex>> vertices, bool solid, RenderLayer<WorldVertex> layer)
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