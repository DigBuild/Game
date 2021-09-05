using System.Numerics;
using DigBuild.Engine.Render;

namespace DigBuild.Render
{
    /// <summary>
    /// A basic 3D vertex with a position.
    /// </summary>
    public readonly struct Vertex3
    {
        /// <summary>
        /// The position.
        /// </summary>
        public readonly Vector3 Pos;

        public Vertex3(Vector3 pos)
        {
            Pos = pos;
        }

        public Vertex3(float x, float y, float z)
        {
            Pos = new Vector3(x, y, z);
        }

        public override string ToString()
        {
            return $"Vertex({Pos})";
        }
    }
}