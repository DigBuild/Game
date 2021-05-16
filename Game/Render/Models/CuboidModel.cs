using System;
using System.Collections.Generic;
using DigBuild.Engine.Math;
using DigBuild.Engine.Render;
using DigBuild.Engine.Render.Models;
using DigBuild.Render.Worlds;

namespace DigBuild.Render.Models
{
    public sealed class CuboidModel : IBlockModel, IItemModel
    {
        private readonly WorldVertex[][] _vertices = new WorldVertex[6][];
        private readonly IRenderLayer<WorldVertex> _layer;

        public CuboidModel(IReadOnlyDictionary<Direction, List<WorldVertex>> vertices, IRenderLayer<WorldVertex> layer)
        {
            foreach (var direction in Directions.All)
                _vertices[(int) direction] = vertices[direction].ToArray();
            _layer = layer;
        }
        
        public void AddGeometry(IGeometryBuffer buffer, IReadOnlyModelData data, DirectionFlags visibleFaces)
        {
            var buf = buffer.Get(_layer);
            foreach (var face in Directions.In(visibleFaces))
                buf.Accept(_vertices[(int) face]);
        }

        public bool HasDynamicGeometry => false;

        public void AddDynamicGeometry(IGeometryBuffer buffer, IReadOnlyModelData data, DirectionFlags visibleFaces, float partialTick)
        {
            throw new NotSupportedException();
        }

        public void AddGeometry(IGeometryBuffer buffer, IReadOnlyModelData data, ItemModelTransform transform, float partialTick)
        {
            buffer.Transform = transform.GetMatrix() * buffer.Transform;

            var buf = buffer.Get(_layer);
            foreach (var face in Directions.All)
                buf.Accept(_vertices[(int) face].WithBrightness(1));
        }
    }
}