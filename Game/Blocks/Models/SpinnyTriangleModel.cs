using System;
using System.Numerics;
using DigBuild.Engine.Render;
using DigBuild.Engine.Textures;
using DigBuild.Render;

namespace DigBuild.Blocks.Models
{
    public sealed class SpinnyTriangleModel : IBlockModel
    {
        private readonly ISprite _sprite;
        
        public SpinnyTriangleModel(ISprite sprite)
        {
            _sprite = sprite;
        }

        public void AddGeometry(DirectionFlags faces, GeometryBufferSet buffers)
        {
        }

        public bool HasDynamicGeometry => true;

        public void AddDynamicGeometry(GeometryBufferSet buffers)
        {
            var angle = (DateTime.Now.Ticks % TimeSpan.TicksPerSecond) / (double) TimeSpan.TicksPerSecond;
            var matrix = Matrix4x4.CreateRotationY((float) (angle * 2 * Math.PI), Vector3.One / 2);

            var normal = Vector3.TransformNormal(new Vector3(0, 0, 1), matrix);
            
            var v1 = new SimpleVertex(new Vector3(0.5f, 1f, 0.5f), normal, _sprite.GetInterpolatedUV(0.5f, 1));
            var v2 = new SimpleVertex(Vector3.Transform(new Vector3(0.5f, 0, 0), matrix), normal, _sprite.GetInterpolatedUV(0, 0));
            var v3 = new SimpleVertex(Vector3.Transform(new Vector3(0.5f, 0, 1), matrix), normal, _sprite.GetInterpolatedUV(1, 0));

            var buf = buffers.Get(WorldRenderLayer.Opaque);
            buf.Accept(v1, v2, v3);
            buf.Accept(v1, v3, v2);
        }
    }
}