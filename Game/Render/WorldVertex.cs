using System.Linq;
using System.Numerics;
using DigBuild.Engine.Render;

namespace DigBuild.Render
{
    public readonly struct WorldVertex
    {
        public readonly Vector3 Pos;
        public readonly Vector3 Normal;
        public readonly Vector2 Uv, BloomUv;
        public readonly float Brightness;
        
        public WorldVertex(Vector3 pos, Vector3 normal, Vector2 uv, Vector2 bloomUv, float brightness)
        {
            Pos = pos;
            Normal = normal;
            Uv = uv;
            BloomUv = bloomUv;
            Brightness = brightness;
        }
        public WorldVertex(Vector3 pos, Vector3 normal, MultiSprite sprite, float u, float v, float brightness)
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

        public static VertexTransformer<WorldVertex> CreateTransformer(IVertexConsumer<WorldVertex> next, Matrix4x4 transform, bool transformNormal)
        {
            return transformNormal ?
                new VertexTransformer<WorldVertex>(next, v => new WorldVertex(
                    Vector3.Transform(v.Pos, transform),
                    Vector3.Normalize(Vector3.TransformNormal(v.Normal, transform)),
                    v.Uv,
                    v.BloomUv,
                    v.Brightness
                )) :
                new VertexTransformer<WorldVertex>(next, v => new WorldVertex(
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

        public static WorldVertex WithBrightness(this WorldVertex vertex, float brightness)
        {
            return new(vertex.Pos, vertex.Normal, vertex.Uv, vertex.BloomUv, brightness);
        }

        public static WorldVertex[] WithBrightness(this WorldVertex[] vertices, float brightness)
        {
            return vertices.Select(v => v.WithBrightness(brightness)).ToArray();
        }
    }
}