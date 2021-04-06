﻿using System;
using System.Collections.Generic;
using System.Numerics;
using DigBuild.Engine.Math;
using DigBuild.Engine.Render;
using DigBuild.Engine.Textures;
using DigBuild.Render;

namespace DigBuild.Blocks.Models
{
    public sealed class CuboidBlockModel : IBlockModel
    {
        private readonly SimpleVertex[][] _vertices = new SimpleVertex[6][];
        public Func<RenderLayer<SimpleVertex>> Layer { get; set; } = () => WorldRenderLayer.Opaque;
        public bool Solid { get; set; } = true;
        public readonly Action Initialize;

        public CuboidBlockModel(AABB bounds, MultiSprite sprite) : this(bounds, new[]{ sprite, sprite, sprite, sprite, sprite, sprite })
        {
        }

        public CuboidBlockModel(AABB bounds, MultiSprite[] sprites) : this(new[]{bounds}, sprites)
        {
        }
        public CuboidBlockModel(AABB[] bounds, MultiSprite sprite) : this(bounds, new[]{ sprite, sprite, sprite, sprite, sprite, sprite })
        {
        }

        public CuboidBlockModel(AABB[] bounds, MultiSprite[] sprites)
        {
            Initialize = () =>
            {
                foreach (var face in Directions.All)
                {
                    var vertices = new List<SimpleVertex>();
                    foreach (var aabb in bounds)
                        vertices.AddRange(GenerateFaceVertices(aabb, face, sprites[(int) face]));
                    _vertices[(int) face] = vertices.ToArray();
                }
            };
        }

        public void AddGeometry(DirectionFlags faces, GeometryBufferSet buffers, Func<Direction, byte> light)
        {
            var buf = buffers.Get(Layer());
            foreach (var face in Directions.In(faces))
                buf.Accept(_vertices[(int) face].WithBrightness(light(face) / 15f));
        }

        public bool IsFaceSolid(Direction face) => Solid;

        private static IEnumerable<SimpleVertex> GenerateFaceVertices(AABB bounds, Direction face, MultiSprite sprite)
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