using DigBuild.Engine.Math;
using DigBuild.Engine.Render;
using DigBuild.Engine.Render.Models;

namespace DigBuild.Render.Models.Geometry
{
    public interface IGeometry
    {
        void Add(IGeometryBuffer buffer, IReadOnlyModelData data, DirectionFlags visibleFaces);
    }
}