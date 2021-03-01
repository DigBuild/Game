using System.Numerics;
using DigBuild.Engine.Render;

namespace DigBuild.Render
{
    public readonly struct SimpleVertex
    {
        public readonly Vector3 Pos;
        public readonly Vector3 Normal;
        public readonly Vector2 Uv;

        public SimpleVertex(Vector3 pos, Vector3 normal, Vector2 uv)
        {
            Pos = pos;
            Normal = normal;
            Uv = uv;
        }

        public override string ToString()
        {
            return $"Vertex({Pos})";
        }

        public static VertexTransformer<SimpleVertex> CreateTransformer(IVertexConsumer<SimpleVertex> next, Matrix4x4 transform)
        {
            return new(next, v => new SimpleVertex(
                Vector3.Transform(v.Pos, transform),
                Vector3.TransformNormal(v.Normal, transform),
                v.Uv
            ));
        }
    }
}