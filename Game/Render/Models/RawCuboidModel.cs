using System.Collections.Generic;
using System.Numerics;
using DigBuild.Engine.Math;
using DigBuild.Engine.Render.Models;
using DigBuild.Platform.Resource;
using DigBuild.Render.Worlds;

namespace DigBuild.Render.Models
{
    public sealed class RawCuboidModel : IRawModel<IBlockModel>, IRawModel<IItemModel>
    {
        private readonly RawCuboidModelDefinition _modelDefinition;
        private readonly Dictionary<ResourceName, MultiSprite?> _sprites = new();

        public RawCuboidModel(RawCuboidModelDefinition modelDefinition)
        {
            _modelDefinition = modelDefinition;
        }
        
        public void LoadTextures(MultiSpriteLoader loader)
        {
            foreach (var cuboid in _modelDefinition.Cuboids)
            {
                foreach (var direction in Directions.All)
                {
                    var tex = cuboid.Textures.Get(direction);
                    if (tex.HasValue && !_sprites.ContainsKey(tex.Value))
                        _sprites[tex.Value] = loader.Load(tex.Value);
                }
            }
        }

        private CuboidModel Build()
        {
            var vertices = new Dictionary<Direction, List<WorldVertex>>();
            foreach (var direction in Directions.All)
                vertices[direction] = new List<WorldVertex>();

            foreach (var cuboid in _modelDefinition.Cuboids)
            {
                var aabb = new AABB(cuboid.From, cuboid.To);

                foreach (var direction in Directions.All)
                {
                    var tex = cuboid.Textures.Get(direction);
                    if (tex.HasValue)
                        vertices[direction].AddRange(GenerateFaceVertices(aabb, direction, _sprites[tex.Value]!));
                }
            }

            var layer = _modelDefinition.Layer switch
            {
                "cutout" => WorldRenderLayers.Cutout,
                "translucent" => WorldRenderLayers.Translucent,
                _ => WorldRenderLayers.Opaque
            };

            return new CuboidModel(vertices, layer);
        }

        IBlockModel IRawModel<IBlockModel>.Build() => Build();
        IItemModel IRawModel<IItemModel>.Build() => Build();

        public static IEnumerable<WorldVertex> GenerateFaceVertices(AABB bounds, Direction face, MultiSprite sprite)
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
                    yield return new WorldVertex(nx + ny + nz, -Vector3.UnitX, sprite, bounds.Max.Z, bounds.Max.Y, 1);
                    yield return new WorldVertex(nx + py + pz, -Vector3.UnitX, sprite, bounds.Min.Z, bounds.Min.Y, 1);
                    yield return new WorldVertex(nx + py + nz, -Vector3.UnitX, sprite, bounds.Max.Z, bounds.Min.Y, 1);

                    yield return new WorldVertex(nx + ny + pz, -Vector3.UnitX, sprite, bounds.Min.Z, bounds.Max.Y, 1);
                    yield return new WorldVertex(nx + py + pz, -Vector3.UnitX, sprite, bounds.Min.Z, bounds.Min.Y, 1);
                    yield return new WorldVertex(nx + ny + nz, -Vector3.UnitX, sprite, bounds.Max.Z, bounds.Max.Y, 1);
                    break;
                case Direction.PosX:
                    yield return new WorldVertex(px + ny + nz, Vector3.UnitX, sprite, bounds.Min.Z, bounds.Max.Y, 1);
                    yield return new WorldVertex(px + py + nz, Vector3.UnitX, sprite, bounds.Min.Z, bounds.Min.Y, 1);
                    yield return new WorldVertex(px + py + pz, Vector3.UnitX, sprite, bounds.Max.Z, bounds.Min.Y, 1);

                    yield return new WorldVertex(px + py + pz, Vector3.UnitX, sprite, bounds.Max.Z, bounds.Min.Y, 1);
                    yield return new WorldVertex(px + ny + pz, Vector3.UnitX, sprite, bounds.Max.Z, bounds.Max.Y, 1);
                    yield return new WorldVertex(px + ny + nz, Vector3.UnitX, sprite, bounds.Min.Z, bounds.Max.Y, 1);
                    break;
                case Direction.NegY:
                    yield return new WorldVertex(nx + ny + nz, -Vector3.UnitY, sprite, bounds.Min.X, bounds.Max.Z, 1);
                    yield return new WorldVertex(px + ny + nz, -Vector3.UnitY, sprite, bounds.Max.X, bounds.Max.Z, 1);
                    yield return new WorldVertex(px + ny + pz, -Vector3.UnitY, sprite, bounds.Max.X, bounds.Min.Z, 1);

                    yield return new WorldVertex(px + ny + pz, -Vector3.UnitY, sprite, bounds.Max.X, bounds.Min.Z, 1);
                    yield return new WorldVertex(nx + ny + pz, -Vector3.UnitY, sprite, bounds.Min.X, bounds.Min.Z, 1);
                    yield return new WorldVertex(nx + ny + nz, -Vector3.UnitY, sprite, bounds.Min.X, bounds.Max.Z, 1);
                    break;
                case Direction.PosY:
                    yield return new WorldVertex(nx + py + nz, Vector3.UnitY, sprite, bounds.Min.X, bounds.Max.Z, 1);
                    yield return new WorldVertex(px + py + pz, Vector3.UnitY, sprite, bounds.Max.X, bounds.Min.Z, 1);
                    yield return new WorldVertex(px + py + nz, Vector3.UnitY, sprite, bounds.Max.X, bounds.Max.Z, 1);

                    yield return new WorldVertex(nx + py + pz, Vector3.UnitY, sprite, bounds.Min.X, bounds.Min.Z, 1);
                    yield return new WorldVertex(px + py + pz, Vector3.UnitY, sprite, bounds.Max.X, bounds.Min.Z, 1);
                    yield return new WorldVertex(nx + py + nz, Vector3.UnitY, sprite, bounds.Min.X, bounds.Max.Z, 1);
                    break;
                case Direction.NegZ:
                    yield return new WorldVertex(nx + ny + nz, -Vector3.UnitZ, sprite, bounds.Min.X, bounds.Max.Y, 1);
                    yield return new WorldVertex(px + py + nz, -Vector3.UnitZ, sprite, bounds.Max.X, bounds.Min.Y, 1);
                    yield return new WorldVertex(px + ny + nz, -Vector3.UnitZ, sprite, bounds.Max.X, bounds.Max.Y, 1);
                    
                    yield return new WorldVertex(nx + py + nz, -Vector3.UnitZ, sprite, bounds.Min.X, bounds.Min.Y, 1);
                    yield return new WorldVertex(px + py + nz, -Vector3.UnitZ, sprite, bounds.Max.X, bounds.Min.Y, 1);
                    yield return new WorldVertex(nx + ny + nz, -Vector3.UnitZ, sprite, bounds.Min.X, bounds.Max.Y, 1);
                    break;
                case Direction.PosZ:
                    yield return new WorldVertex(px + py + pz, Vector3.UnitZ, sprite, bounds.Min.X, bounds.Min.Y, 1);
                    yield return new WorldVertex(nx + ny + pz, Vector3.UnitZ, sprite, bounds.Max.X, bounds.Max.Y, 1);
                    yield return new WorldVertex(px + ny + pz, Vector3.UnitZ, sprite, bounds.Min.X, bounds.Max.Y, 1);

                    yield return new WorldVertex(nx + py + pz, Vector3.UnitZ, sprite, bounds.Max.X, bounds.Min.Y, 1);
                    yield return new WorldVertex(nx + ny + pz, Vector3.UnitZ, sprite, bounds.Max.X, bounds.Max.Y, 1);
                    yield return new WorldVertex(px + py + pz, Vector3.UnitZ, sprite, bounds.Min.X, bounds.Min.Y, 1);
                    break;
            }
        }
    }
}