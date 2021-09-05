using DigBuild.Engine.Math;
using DigBuild.Engine.Render;
using DigBuild.Engine.Render.Models;

namespace DigBuild.Render.Models.Geometry
{
    /// <summary>
    /// A piece of geometry.
    /// </summary>
    public interface IGeometry
    {
        /// <summary>
        /// Adds this geometry to the given geometry buffer.
        /// </summary>
        /// <param name="buffer">The buffer</param>
        /// <param name="data">The model data</param>
        /// <param name="visibleFaces">The visible faces</param>
        void Add(IGeometryBuffer buffer, IReadOnlyModelData data, DirectionFlags visibleFaces);
    }
}