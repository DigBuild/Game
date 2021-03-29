using System.Numerics;
using DigBuild.Engine.Render;

namespace DigBuild.Render
{
    public readonly struct SimplerVertex
    {
        public readonly Vector3 Pos;

        public SimplerVertex(Vector3 pos)
        {
            Pos = pos;
        }

        public SimplerVertex(float x, float y, float z)
        {
            Pos = new Vector3(x, y, z);
        }

        public override string ToString()
        {
            return $"Vertex({Pos})";
        }

        public static VertexTransformer<SimplerVertex> CreateTransformer(IVertexConsumer<SimplerVertex> next, Matrix4x4 transform)
        {
            return new(next, v => new SimplerVertex(
                Vector3.Transform(v.Pos, transform)
            ));
        }
    }
}