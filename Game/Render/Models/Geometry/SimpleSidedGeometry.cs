using DigBuild.Engine.Math;
using DigBuild.Engine.Render;
using DigBuild.Engine.Render.Models;
using DigBuild.Render.Worlds;

namespace DigBuild.Render.Models.Geometry
{
    public sealed class SimpleSidedGeometry : IGeometry
    {
        private readonly WorldVertex[][] _vertices;
        private readonly IRenderLayer<WorldVertex> _layer;

        public SimpleSidedGeometry(WorldVertex[][] vertices, IRenderLayer<WorldVertex> layer)
        {
            _vertices = vertices;
            _layer = layer;
        }

        public void Add(IGeometryBuffer buffer, IReadOnlyModelData data, DirectionFlags visibleFaces)
        {
            var buf = buffer.Get(_layer);
            foreach (var face in Directions.In(visibleFaces))
                buf.Accept(_vertices[(int) face]);
        }
    }
}