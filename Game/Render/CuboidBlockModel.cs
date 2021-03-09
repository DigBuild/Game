using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using DigBuild.Engine.Math;
using DigBuild.Engine.Render;
using DigBuild.Engine.Textures;

namespace DigBuild.Render
{
    public sealed class CuboidBlockModel : IBlockModel
    {
        private readonly SimpleVertex[][] _vertices = new SimpleVertex[6][];
        public readonly Action Initialize;

        public CuboidBlockModel(AABB bounds, ISprite sprite) : this(bounds, new[]{ sprite, sprite, sprite, sprite, sprite, sprite })
        {
        }

        public CuboidBlockModel(AABB bounds, ISprite[] sprites)
        {
            Initialize = () =>
            {
                foreach (var face in BlockFaces.All)
                    _vertices[(int) face] = GenerateFaceVertices(bounds, face, sprites[(int) face]).ToArray();
            };
        }

        public void AddGeometry(BlockFaceFlags faces, GeometryBufferSet buffers)
        {
            var buf = buffers.Get(WorldRenderLayer.Opaque);
            foreach (var face in BlockFaces.In(faces))
                buf.Accept(_vertices[(int) face]);
        }
        
        private static IEnumerable<SimpleVertex> GenerateFaceVertices(AABB bounds, BlockFace face, ISprite sprite)
        {
            var nx = new Vector3(bounds.Min.X, 0, 0);
            var ny = new Vector3(0, bounds.Min.Y, 0);
            var nz = new Vector3(0, 0, bounds.Min.Z);
            var px = new Vector3(bounds.Max.X, 0, 0);
            var py = new Vector3(0, bounds.Max.Y, 0);
            var pz = new Vector3(0, 0, bounds.Max.Z);

            switch (face)
            {
                case BlockFace.NegX:
                    yield return new SimpleVertex(nx + ny + nz, -Vector3.UnitX, sprite.GetInterpolatedUV(1, 1));
                    yield return new SimpleVertex(nx + py + pz, -Vector3.UnitX, sprite.GetInterpolatedUV(0, 0));
                    yield return new SimpleVertex(nx + py + nz, -Vector3.UnitX, sprite.GetInterpolatedUV(1, 0));

                    yield return new SimpleVertex(nx + ny + pz, -Vector3.UnitX, sprite.GetInterpolatedUV(0, 1));
                    yield return new SimpleVertex(nx + py + pz, -Vector3.UnitX, sprite.GetInterpolatedUV(0, 0));
                    yield return new SimpleVertex(nx + ny + nz, -Vector3.UnitX, sprite.GetInterpolatedUV(1, 1));
                    break;
                case BlockFace.PosX:
                    yield return new SimpleVertex(px + ny + nz, Vector3.UnitX, sprite.GetInterpolatedUV(0, 1));
                    yield return new SimpleVertex(px + py + nz, Vector3.UnitX, sprite.GetInterpolatedUV(0, 0));
                    yield return new SimpleVertex(px + py + pz, Vector3.UnitX, sprite.GetInterpolatedUV(1, 0));

                    yield return new SimpleVertex(px + py + pz, Vector3.UnitX, sprite.GetInterpolatedUV(1, 0));
                    yield return new SimpleVertex(px + ny + pz, Vector3.UnitX, sprite.GetInterpolatedUV(1, 1));
                    yield return new SimpleVertex(px + ny + nz, Vector3.UnitX, sprite.GetInterpolatedUV(0, 1));
                    break;
                case BlockFace.NegY:
                    yield return new SimpleVertex(nx + ny + nz, -Vector3.UnitY, sprite.GetInterpolatedUV(0, 1));
                    yield return new SimpleVertex(px + ny + nz, -Vector3.UnitY, sprite.GetInterpolatedUV(1, 1));
                    yield return new SimpleVertex(px + ny + pz, -Vector3.UnitY, sprite.GetInterpolatedUV(1, 0));

                    yield return new SimpleVertex(px + ny + pz, -Vector3.UnitY, sprite.GetInterpolatedUV(1, 0));
                    yield return new SimpleVertex(nx + ny + pz, -Vector3.UnitY, sprite.GetInterpolatedUV(0, 0));
                    yield return new SimpleVertex(nx + ny + nz, -Vector3.UnitY, sprite.GetInterpolatedUV(0, 1));
                    break;
                case BlockFace.PosY:
                    yield return new SimpleVertex(nx + py + nz, Vector3.UnitY, sprite.GetInterpolatedUV(0, 1));
                    yield return new SimpleVertex(px + py + pz, Vector3.UnitY, sprite.GetInterpolatedUV(1, 0));
                    yield return new SimpleVertex(px + py + nz, Vector3.UnitY, sprite.GetInterpolatedUV(1, 1));

                    yield return new SimpleVertex(nx + py + pz, Vector3.UnitY, sprite.GetInterpolatedUV(0, 0));
                    yield return new SimpleVertex(px + py + pz, Vector3.UnitY, sprite.GetInterpolatedUV(1, 0));
                    yield return new SimpleVertex(nx + py + nz, Vector3.UnitY, sprite.GetInterpolatedUV(0, 1));
                    break;
                case BlockFace.NegZ:
                    yield return new SimpleVertex(nx + ny + nz, -Vector3.UnitZ, sprite.GetInterpolatedUV(0, 1));
                    yield return new SimpleVertex(px + py + nz, -Vector3.UnitZ, sprite.GetInterpolatedUV(1, 0));
                    yield return new SimpleVertex(px + ny + nz, -Vector3.UnitZ, sprite.GetInterpolatedUV(1, 1));
                    
                    yield return new SimpleVertex(nx + py + nz, -Vector3.UnitZ, sprite.GetInterpolatedUV(0, 0));
                    yield return new SimpleVertex(px + py + nz, -Vector3.UnitZ, sprite.GetInterpolatedUV(1, 0));
                    yield return new SimpleVertex(nx + ny + nz, -Vector3.UnitZ, sprite.GetInterpolatedUV(0, 1));
                    break;
                case BlockFace.PosZ:
                    yield return new SimpleVertex(px + py + pz, Vector3.UnitZ, sprite.GetInterpolatedUV(0, 0));
                    yield return new SimpleVertex(nx + ny + pz, Vector3.UnitZ, sprite.GetInterpolatedUV(1, 1));
                    yield return new SimpleVertex(px + ny + pz, Vector3.UnitZ, sprite.GetInterpolatedUV(0, 1));

                    yield return new SimpleVertex(nx + py + pz, Vector3.UnitZ, sprite.GetInterpolatedUV(1, 0));
                    yield return new SimpleVertex(nx + ny + pz, Vector3.UnitZ, sprite.GetInterpolatedUV(1, 1));
                    yield return new SimpleVertex(px + py + pz, Vector3.UnitZ, sprite.GetInterpolatedUV(0, 0));
                    break;
            }
        }
    }
}