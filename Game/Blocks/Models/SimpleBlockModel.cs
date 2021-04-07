using System;
using System.Collections.Generic;
using System.Numerics;
using DigBuild.Client;
using DigBuild.Engine.Math;
using DigBuild.Engine.Render;
using DigBuild.Render;

namespace DigBuild.Blocks.Models
{
    public sealed class SimpleBlockModel : IBlockModel
    {
        private readonly SimpleVertex[][] _vertices = new SimpleVertex[6][];
        private readonly SimpleCuboidModel _model;

        public Func<RenderLayer<SimpleVertex>> Layer { get; set; } = () => WorldRenderLayer.Opaque;

        public SimpleBlockModel(SimpleCuboidModel model)
        {
            _model = model;
        }

        public void LoadTextures(MultiSpriteLoader spriteLoader)
        {
            foreach (var cuboid in _model.Cuboids)
            {
                foreach (var direction in Directions.All)
                {
                    var tex = cuboid.Textures.Get(direction);
                    if (tex.HasValue)
                        spriteLoader.Load(tex.Value);
                }
            }
        }

        public void Initialize(MultiSpriteLoader spriteLoader)
        {
            
            var vertices = new Dictionary<Direction, List<SimpleVertex>>();
            foreach (var direction in Directions.All)
                vertices[direction] = new List<SimpleVertex>();

            foreach (var cuboid in _model.Cuboids)
            {
                var aabb = new AABB(cuboid.From, cuboid.To);

                foreach (var direction in Directions.All)
                {
                    var tex = cuboid.Textures.Get(direction);
                    if (tex.HasValue)
                        vertices[direction].AddRange(GenerateFaceVertices(aabb, direction, spriteLoader.Load(tex.Value)!));
                }
            }

            foreach (var direction in Directions.All)
                _vertices[(int) direction] = vertices[direction].ToArray();
        }

        public void AddGeometry(DirectionFlags faces, GeometryBufferSet buffers, Func<Direction, byte> light)
        {
            var buf = buffers.Get(Layer());
            foreach (var face in Directions.In(faces))
                buf.Accept(_vertices[(int) face].WithBrightness(light(face) / 15f));
        }

        public bool IsFaceSolid(Direction face) => _model.Solid;

        public static IEnumerable<SimpleVertex> GenerateFaceVertices(AABB bounds, Direction face, MultiSprite sprite)
        {
            var nx = new Vector3(bounds.Min.X, 0, 0);
            var ny = new Vector3(0, bounds.Min.Y, 0);
            var nz = new Vector3(0, 0, bounds.Min.Z);
            var px = new Vector3(bounds.Max.X, 0, 0);
            var py = new Vector3(0, bounds.Max.Y, 0);
            var pz = new Vector3(0, 0, bounds.Max.Z);

            switch (face)
            {
                case Direction.NegX:
                    yield return new SimpleVertex(nx + ny + nz, -Vector3.UnitX, sprite, bounds.Max.Z, bounds.Max.Y, 1);
                    yield return new SimpleVertex(nx + py + pz, -Vector3.UnitX, sprite, bounds.Min.Z, bounds.Min.Y, 1);
                    yield return new SimpleVertex(nx + py + nz, -Vector3.UnitX, sprite, bounds.Max.Z, bounds.Min.Y, 1);

                    yield return new SimpleVertex(nx + ny + pz, -Vector3.UnitX, sprite, bounds.Min.Z, bounds.Max.Y, 1);
                    yield return new SimpleVertex(nx + py + pz, -Vector3.UnitX, sprite, bounds.Min.Z, bounds.Min.Y, 1);
                    yield return new SimpleVertex(nx + ny + nz, -Vector3.UnitX, sprite, bounds.Max.Z, bounds.Max.Y, 1);
                    break;
                case Direction.PosX:
                    yield return new SimpleVertex(px + ny + nz, Vector3.UnitX, sprite, bounds.Min.Z, bounds.Max.Y, 1);
                    yield return new SimpleVertex(px + py + nz, Vector3.UnitX, sprite, bounds.Min.Z, bounds.Min.Y, 1);
                    yield return new SimpleVertex(px + py + pz, Vector3.UnitX, sprite, bounds.Max.Z, bounds.Min.Y, 1);

                    yield return new SimpleVertex(px + py + pz, Vector3.UnitX, sprite, bounds.Max.Z, bounds.Min.Y, 1);
                    yield return new SimpleVertex(px + ny + pz, Vector3.UnitX, sprite, bounds.Max.Z, bounds.Max.Y, 1);
                    yield return new SimpleVertex(px + ny + nz, Vector3.UnitX, sprite, bounds.Min.Z, bounds.Max.Y, 1);
                    break;
                case Direction.NegY:
                    yield return new SimpleVertex(nx + ny + nz, -Vector3.UnitY, sprite, bounds.Min.X, bounds.Max.Z, 1);
                    yield return new SimpleVertex(px + ny + nz, -Vector3.UnitY, sprite, bounds.Max.X, bounds.Max.Z, 1);
                    yield return new SimpleVertex(px + ny + pz, -Vector3.UnitY, sprite, bounds.Max.X, bounds.Min.Z, 1);

                    yield return new SimpleVertex(px + ny + pz, -Vector3.UnitY, sprite, bounds.Max.X, bounds.Min.Z, 1);
                    yield return new SimpleVertex(nx + ny + pz, -Vector3.UnitY, sprite, bounds.Min.X, bounds.Min.Z, 1);
                    yield return new SimpleVertex(nx + ny + nz, -Vector3.UnitY, sprite, bounds.Min.X, bounds.Max.Z, 1);
                    break;
                case Direction.PosY:
                    yield return new SimpleVertex(nx + py + nz, Vector3.UnitY, sprite, bounds.Min.X, bounds.Max.Z, 1);
                    yield return new SimpleVertex(px + py + pz, Vector3.UnitY, sprite, bounds.Max.X, bounds.Min.Z, 1);
                    yield return new SimpleVertex(px + py + nz, Vector3.UnitY, sprite, bounds.Max.X, bounds.Max.Z, 1);

                    yield return new SimpleVertex(nx + py + pz, Vector3.UnitY, sprite, bounds.Min.X, bounds.Min.Z, 1);
                    yield return new SimpleVertex(px + py + pz, Vector3.UnitY, sprite, bounds.Max.X, bounds.Min.Z, 1);
                    yield return new SimpleVertex(nx + py + nz, Vector3.UnitY, sprite, bounds.Min.X, bounds.Max.Z, 1);
                    break;
                case Direction.NegZ:
                    yield return new SimpleVertex(nx + ny + nz, -Vector3.UnitZ, sprite, bounds.Min.X, bounds.Max.Y, 1);
                    yield return new SimpleVertex(px + py + nz, -Vector3.UnitZ, sprite, bounds.Max.X, bounds.Min.Y, 1);
                    yield return new SimpleVertex(px + ny + nz, -Vector3.UnitZ, sprite, bounds.Max.X, bounds.Max.Y, 1);
                    
                    yield return new SimpleVertex(nx + py + nz, -Vector3.UnitZ, sprite, bounds.Min.X, bounds.Min.Y, 1);
                    yield return new SimpleVertex(px + py + nz, -Vector3.UnitZ, sprite, bounds.Max.X, bounds.Min.Y, 1);
                    yield return new SimpleVertex(nx + ny + nz, -Vector3.UnitZ, sprite, bounds.Min.X, bounds.Max.Y, 1);
                    break;
                case Direction.PosZ:
                    yield return new SimpleVertex(px + py + pz, Vector3.UnitZ, sprite, bounds.Min.X, bounds.Min.Y, 1);
                    yield return new SimpleVertex(nx + ny + pz, Vector3.UnitZ, sprite, bounds.Max.X, bounds.Max.Y, 1);
                    yield return new SimpleVertex(px + ny + pz, Vector3.UnitZ, sprite, bounds.Min.X, bounds.Max.Y, 1);

                    yield return new SimpleVertex(nx + py + pz, Vector3.UnitZ, sprite, bounds.Max.X, bounds.Min.Y, 1);
                    yield return new SimpleVertex(nx + ny + pz, Vector3.UnitZ, sprite, bounds.Max.X, bounds.Max.Y, 1);
                    yield return new SimpleVertex(px + py + pz, Vector3.UnitZ, sprite, bounds.Min.X, bounds.Min.Y, 1);
                    break;
            }
        }
    }
}