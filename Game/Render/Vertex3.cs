using System.Numerics;
using DigBuild.Engine.Render;

namespace DigBuild.Render
{
    public readonly struct Vertex3
    {
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

        public static VertexTransformer<Vertex3> CreateTransformer(IVertexConsumer<Vertex3> next, Matrix4x4 transform)
        {
            return new(next, v => new Vertex3(
                Vector3.Transform(v.Pos, transform)
            ));
        }
    }
}