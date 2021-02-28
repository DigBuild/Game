using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using DigBuildEngine.Math;
using DigBuildEngine.Render;

namespace DigBuild.Render
{
    public sealed class CuboidBlockModel : IBlockModel
    {
        private readonly SimpleVertex[][] _vertices = new SimpleVertex[6][];

        public CuboidBlockModel(AABB bounds, Vector2 funnyUv)
        {
            foreach (var face in BlockFaces.All)
                _vertices[(int) face] = GenerateFaceVertices(bounds, face, funnyUv).ToArray();
        }

        public void AddGeometry(BlockFaceFlags faces, DigBuildEngine.Render.GeometryBufferSet buffers)
        {
            var buf = buffers.Get(WorldRenderLayer.Opaque);
            foreach (var face in BlockFaces.All)
                if (faces.HasFace(face))
                    buf.Accept(_vertices[(int) face]);
        }
        
        private static IEnumerable<SimpleVertex> GenerateFaceVertices(AABB bounds, BlockFace face, Vector2 funnyUv)
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
                    yield return new SimpleVertex(nx + ny + nz, -Vector3.UnitX, funnyUv);
                    yield return new SimpleVertex(nx + py + nz, -Vector3.UnitX, funnyUv);
                    yield return new SimpleVertex(nx + py + pz, -Vector3.UnitX, funnyUv);

                    yield return new SimpleVertex(nx + ny + pz, -Vector3.UnitX, funnyUv);
                    yield return new SimpleVertex(nx + ny + nz, -Vector3.UnitX, funnyUv);
                    yield return new SimpleVertex(nx + py + pz, -Vector3.UnitX, funnyUv);
                    break;
                case BlockFace.PosX:
                    yield return new SimpleVertex(px + ny + nz, Vector3.UnitX, funnyUv);
                    yield return new SimpleVertex(px + py + pz, Vector3.UnitX, funnyUv);
                    yield return new SimpleVertex(px + py + nz, Vector3.UnitX, funnyUv);

                    yield return new SimpleVertex(px + py + pz, Vector3.UnitX, funnyUv);
                    yield return new SimpleVertex(px + ny + nz, Vector3.UnitX, funnyUv);
                    yield return new SimpleVertex(px + ny + pz, Vector3.UnitX, funnyUv);
                    break;
                case BlockFace.NegY:
                    yield return new SimpleVertex(nx + ny + nz, -Vector3.UnitY, funnyUv);
                    yield return new SimpleVertex(px + ny + pz, -Vector3.UnitY, funnyUv);
                    yield return new SimpleVertex(px + ny + nz, -Vector3.UnitY, funnyUv);

                    yield return new SimpleVertex(px + ny + pz, -Vector3.UnitY, funnyUv);
                    yield return new SimpleVertex(nx + ny + nz, -Vector3.UnitY, funnyUv);
                    yield return new SimpleVertex(nx + ny + pz, -Vector3.UnitY, funnyUv);
                    break;
                case BlockFace.PosY:
                    yield return new SimpleVertex(nx + py + nz, Vector3.UnitY, funnyUv);
                    yield return new SimpleVertex(px + py + nz, Vector3.UnitY, funnyUv);
                    yield return new SimpleVertex(px + py + pz, Vector3.UnitY, funnyUv);

                    yield return new SimpleVertex(px + py + pz, Vector3.UnitY, funnyUv);
                    yield return new SimpleVertex(nx + py + pz, Vector3.UnitY, funnyUv);
                    yield return new SimpleVertex(nx + py + nz, Vector3.UnitY, funnyUv);
                    break;
                case BlockFace.NegZ:
                    yield return new SimpleVertex(nx + ny + nz, -Vector3.UnitZ, funnyUv);
                    yield return new SimpleVertex(px + ny + nz, -Vector3.UnitZ, funnyUv);
                    yield return new SimpleVertex(px + py + nz, -Vector3.UnitZ, funnyUv);

                    yield return new SimpleVertex(px + py + nz, -Vector3.UnitZ, funnyUv);
                    yield return new SimpleVertex(nx + py + nz, -Vector3.UnitZ, funnyUv);
                    yield return new SimpleVertex(nx + ny + nz, -Vector3.UnitZ, funnyUv);
                    break;
                case BlockFace.PosZ:
                    yield return new SimpleVertex(px + py + pz, Vector3.UnitZ, funnyUv);
                    yield return new SimpleVertex(px + ny + pz, Vector3.UnitZ, funnyUv);
                    yield return new SimpleVertex(nx + ny + pz, Vector3.UnitZ, funnyUv);

                    yield return new SimpleVertex(nx + ny + pz, Vector3.UnitZ, funnyUv);
                    yield return new SimpleVertex(nx + py + pz, Vector3.UnitZ, funnyUv);
                    yield return new SimpleVertex(px + py + pz, Vector3.UnitZ, funnyUv);
                    break;
            }
        }
    }
}