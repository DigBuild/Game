using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using DigBuild.Engine.Math;
using DigBuild.Engine.Render;
using DigBuild.Platform.Resource;
using DigBuild.Render.Models.Expressions;
using DigBuild.Render.Worlds;
using DigBuild.Serialization;

namespace DigBuild.Render.Models.Geometry
{
    public sealed class CuboidGeometryProvider : IGeometryProvider
    {
        public IPartialGeometry Provide(
            JsonElement json, JsonSerializerOptions jsonOptions,
            ResourceManager resourceManager, ResourceName currentFile
        )
        {
            var from = json.GetProperty<Vector3>("from", jsonOptions);
            var to = json.GetProperty<Vector3>("to", jsonOptions);

            var layerString = json.GetProperty<string>("layer", jsonOptions);
            var layer = ModelExpressionParser.Parse(layerString);

            var texturesObj = json.GetProperty("textures");
            var textures = new Dictionary<Direction, IModelExpression>();
            foreach (var direction in Directions.All)
                if (texturesObj.TryGetProperty<string>(direction.GetName(), out var exp, jsonOptions))
                    textures[direction] = ModelExpressionParser.Parse(exp);
            
            return new CuboidPartialGeometry(new AABB(from, to), layer, textures);
        }

        private sealed class CuboidPartialGeometry : IPartialGeometry
        {
            private readonly AABB _bounds;
            private readonly IModelExpression _layer;
            private readonly IReadOnlyDictionary<Direction, IModelExpression> _textures;

            public IEnumerable<string> RequiredVariables { get; }

            public CuboidPartialGeometry(
                AABB bounds,
                IModelExpression layer,
                IReadOnlyDictionary<Direction, IModelExpression> textures
            )
            {
                _layer = layer;
                _textures = textures;
                _bounds = bounds;

                var requiredVariables = new HashSet<string>();
                foreach (var var in layer.RequiredVariables)
                    requiredVariables.Add(var);
                foreach (var exp in textures.Values)
                foreach (var var in exp.RequiredVariables)
                    requiredVariables.Add(var);
                RequiredVariables = requiredVariables;
            }

            public IPartialGeometry ApplySubstitutions(IReadOnlyDictionary<string, IModelExpression> variables)
            {
                var context = new ModelExpressionSubstitutionContext(variables);

                var newLayer = _layer.Apply(context);
                var newTextures = _textures
                    .Select(pair => (pair.Key, Value: pair.Value.Apply(context)))
                    .ToDictionary(p => p.Key, p => p.Value);
                
                return new CuboidPartialGeometry(_bounds, newLayer, newTextures);
            }

            public IRawGeometry Prime()
            {
                if (RequiredVariables.Any())
                    throw new InvalidOperationException("Runtime variables are not supported by cuboid geometry.");

                var layerGetter = _layer.CompileString();
                var textureGetters = _textures
                    .Select(t => (t.Key, Value: t.Value.CompileString()))
                    .ToDictionary(p => p.Key, p => p.Value);
                
                var layer = layerGetter(ImmutableDictionary<string, string>.Empty) switch
                {
                    "digbuild:cutout" => WorldRenderLayers.Cutout,
                    "digbuild:water" => WorldRenderLayers.Water,
                    _ => WorldRenderLayers.Opaque
                };

                return new CuboidRawGeometry(_bounds, layer, textureGetters);
            }
        }

        private sealed class CuboidRawGeometry : IRawGeometry
        {
            private readonly AABB _bounds;
            private readonly IRenderLayer<WorldVertex> _layer;
            private readonly IReadOnlyDictionary<Direction, Func<IReadOnlyDictionary<string, string>, string>> _textureGetters;
            private readonly Dictionary<Direction, MultiSprite> _sprites = new();

            public CuboidRawGeometry(
                AABB bounds, IRenderLayer<WorldVertex> layer,
                IReadOnlyDictionary<Direction, Func<IReadOnlyDictionary<string, string>, string>> textureGetters
            )
            {
                _bounds = bounds;
                _layer = layer;
                _textureGetters = textureGetters;
            }

            public void LoadTextures(MultiSpriteLoader loader)
            {
                _sprites.Clear();
                foreach (var (direction, getter) in _textureGetters)
                {
                    var sprite = loader.Load(ResourceName.Parse(getter(ImmutableDictionary<string, string>.Empty))!.Value)!;
                    _sprites[direction] = sprite;
                }
            }

            public IGeometry Build()
            {
                var vertices = new WorldVertex[6][];
                foreach (var direction in Directions.All)
                {
                    if (_sprites.TryGetValue(direction, out var sprite))
                        vertices[(int)direction] = GenerateFaceVertices(_bounds, direction, sprite).ToArray();
                    else
                        vertices[(int)direction] = Array.Empty<WorldVertex>();
                }
                
                return new SimpleSidedGeometry(vertices, _layer);
            }
        }

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