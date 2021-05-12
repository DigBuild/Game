using System.Numerics;
using DigBuild.Engine.Render;

namespace DigBuild.Render
{
    public readonly struct Vertex2
    {
        public readonly Vector2 Pos;

        public Vertex2(Vector2 pos)
        {
            Pos = pos;
        }

        public Vertex2(float x, float y)
        {
            Pos = new Vector2(x, y);
        }

        public override string ToString()
        {
            return $"Vertex({Pos})";
        }

        public static VertexTransformer<Vertex2> CreateTransformer(IVertexConsumer<Vertex2> next, Matrix4x4 transform)
        {
            return new(next, v => new Vertex2(
                Vector2.Transform(v.Pos, transform)
            ));
        }
    }
}