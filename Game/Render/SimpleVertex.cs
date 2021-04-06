using System.Linq;
using System.Numerics;
using DigBuild.Engine.Render;

namespace DigBuild.Render
{
    public readonly struct SimpleVertex
    {
        public readonly Vector3 Pos;
        public readonly Vector3 Normal;
        public readonly Vector2 Uv, BloomUv;
        public readonly float Brightness;
        
        public SimpleVertex(Vector3 pos, Vector3 normal, Vector2 uv, Vector2 bloomUv, float brightness)
        {
            Pos = pos;
            Normal = normal;
            Uv = uv;
            BloomUv = bloomUv;
            Brightness = brightness;
        }
        public SimpleVertex(Vector3 pos, Vector3 normal, MultiSprite sprite, float u, float v, float brightness)
        {
            Pos = pos;
            Normal = normal;
            Uv = sprite.Color.GetInterpolatedUV(u, v);
            BloomUv = sprite.Bloom.GetInterpolatedUV(u, v);
            Brightness = brightness;
        }

        public override string ToString()
        {
            return $"Vertex({Pos})";
        }

        public static VertexTransformer<SimpleVertex> CreateTransformer(IVertexConsumer<SimpleVertex> next, Matrix4x4 transform, bool transformNormal)
        {
            return transformNormal ?
                new VertexTransformer<SimpleVertex>(next, v => new SimpleVertex(
                    Vector3.Transform(v.Pos, transform),
                    Vector3.Normalize(Vector3.TransformNormal(v.Normal, transform)),
                    v.Uv,
                    v.BloomUv,
                    v.Brightness
                )) :
                new VertexTransformer<SimpleVertex>(next, v => new SimpleVertex(
                    Vector3.Transform(v.Pos, transform),
                    v.Normal,
                    v.Uv,
                    v.BloomUv,
                    v.Brightness
                ));
        }
    }

    public static class SimpleVertexExtensions
    {

        public static SimpleVertex WithBrightness(this SimpleVertex vertex, float brightness)
        {
            return new(vertex.Pos, vertex.Normal, vertex.Uv, vertex.BloomUv, brightness);
        }

        public static SimpleVertex[] WithBrightness(this SimpleVertex[] vertices, float brightness)
        {
            return vertices.Select(v => v.WithBrightness(brightness)).ToArray();
        }
    }
}