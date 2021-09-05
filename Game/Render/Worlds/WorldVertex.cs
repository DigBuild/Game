using System.Linq;
using System.Numerics;
using DigBuild.Engine.Render;

namespace DigBuild.Render.Worlds
{
    /// <summary>
    /// The main world vertex format composed of position, normal, UV, bloom UV and brightness.
    /// </summary>
    public readonly struct WorldVertex
    {
        /// <summary>
        /// The position.
        /// </summary>
        public readonly Vector3 Pos;
        /// <summary>
        /// The normal.
        /// </summary>
        public readonly Vector3 Normal;
        /// <summary>
        /// The UV.
        /// </summary>
        public readonly Vector2 Uv;
        /// <summary>
        /// The bloomUV.
        /// </summary>
        public readonly Vector2 BloomUv;
        /// <summary>
        /// The brightness.
        /// </summary>
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
        
        /// <summary>
        /// Wraps a vertex consumer to apply the specified transform to all incoming vertices.
        /// </summary>
        /// <param name="next">The vertex consumer</param>
        /// <param name="transform">The transform matrix</param>
        /// <param name="transformNormal">Whether to also transform normals or not</param>
        /// <returns>The new vertex consumer</returns>
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

    /// <summary>
    /// Helpers for world vertices.
    /// </summary>
    public static class SimpleVertexExtensions
    {
        /// <summary>
        /// Creates a new vertex with updated brightness.
        /// </summary>
        /// <param name="vertex">The original vertex</param>
        /// <param name="brightness">The new brightness</param>
        /// <returns>The new vertex</returns>
        public static WorldVertex WithBrightness(this WorldVertex vertex, float brightness)
        {
            return new(vertex.Pos, vertex.Normal, vertex.Uv, vertex.BloomUv, brightness);
        }
        
        /// <summary>
        /// Creates a new set of vertices with updated brightness.
        /// </summary>
        /// <param name="vertices">The original set of vertices</param>
        /// <param name="brightness">The new brightness</param>
        /// <returns>The new set of vertices</returns>
        public static WorldVertex[] WithBrightness(this WorldVertex[] vertices, float brightness)
        {
            return vertices.Select(v => WithBrightness((WorldVertex) v, brightness)).ToArray();
        }
    }
}