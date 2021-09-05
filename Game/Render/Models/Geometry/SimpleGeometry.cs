using DigBuild.Engine.Math;
using DigBuild.Engine.Render;
using DigBuild.Engine.Render.Models;
using DigBuild.Render.Worlds;

namespace DigBuild.Render.Models.Geometry
{
    /// <summary>
    /// A simple geometry composed of world vertices and a layer.
    /// </summary>
    public sealed class SimpleGeometry : IGeometry
    {
        private readonly WorldVertex[] _vertices;
        private readonly IRenderLayer<WorldVertex> _layer;

        public SimpleGeometry(WorldVertex[] vertices, IRenderLayer<WorldVertex> layer)
        {
            _vertices = vertices;
            _layer = layer;
        }

        public void Add(IGeometryBuffer buffer, IReadOnlyModelData data, DirectionFlags visibleFaces)
        {
            var buf = buffer.Get(_layer);
            buf.Accept(_vertices);
        }
    }
}