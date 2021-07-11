using System;
using System.Collections.Generic;
using System.Text.Json;
using DigBuild.Engine.Render.Models;
using DigBuild.Platform.Resource;
using DigBuild.Render.Models.Geometry;
using DigBuild.Serialization;

namespace DigBuild.Render.Models.Json
{
    public class RawJsonModel : ICustomResource, IRawModel<IBlockModel>, IRawModel<IItemModel>
    {
        private readonly IReadOnlyList<(JsonModelRule Rule, IRawGeometry RawGeometry)> _variants;
        private readonly bool _dynamic;

        public ResourceName Name { get; }

        private RawJsonModel(
            ResourceName name,
            IReadOnlyList<(JsonModelRule Rule, IRawGeometry RawGeometry)> variants,
            bool dynamic
        )
        {
            Name = name;
            _variants = variants;
            _dynamic = dynamic;
        }

        public void LoadTextures(MultiSpriteLoader loader)
        {
            foreach (var (_, rawGeometry) in _variants)
                rawGeometry.LoadTextures(loader);
        }

        IItemModel IRawModel<IItemModel>.Build()
        {
            var geometry = new List<(JsonModelRule Rule, IGeometry Geometry)>();
            foreach (var (rule, rawGeometry) in _variants)
                geometry.Add((rule, rawGeometry.Build()));
            return new JsonModel(geometry, _dynamic);
        }

        IBlockModel IRawModel<IBlockModel>.Build()
        {
            var geometry = new List<(JsonModelRule Rule, IGeometry Geometry)>();
            foreach (var (rule, rawGeometry) in _variants)
                geometry.Add((rule, rawGeometry.Build()));
            return new JsonModel(geometry, _dynamic);
        }

        public static RawJsonModel? Load(ResourceManager manager, ResourceName name)
        {
            var actualResourceName = new ResourceName(name.Domain, $"models/{name.Path}.json");
            if (!manager.TryGetResource(actualResourceName, out var res))
                return null;

            var bytes = res.ReadAllBytes();
            var span = new ReadOnlySpan<byte>(bytes);

            var modelJson = JsonSerializer.Deserialize<JsonElement>(span);
            var variantsJson = modelJson.GetProperty("variants");

            var variants = new List<(JsonModelRule Rule, IRawGeometry RawGeometry)>();

            foreach (var variantJson in variantsJson.EnumerateArray())
            {
                var singleMatches = new Dictionary<string, string>();
                var multiMatches = new Dictionary<string, HashSet<string>>();
                if (variantJson.TryGetProperty("when", out var whenJson))
                {
                    foreach (var property in whenJson.EnumerateObject())
                    {
                        if (property.Value.ValueKind == JsonValueKind.String)
                        {
                            singleMatches[property.Name] = property.Value.GetString()!;
                        }
                        else
                        {
                            var values = new HashSet<string>();
                            foreach (var value in property.Value.EnumerateArray())
                                values.Add(value.GetString()!);
                            multiMatches[property.Name] = values;
                        }
                    }
                }
                var rule = new JsonModelRule(singleMatches, multiMatches);

                var geometryStr = variantJson.GetProperty<string>("geometry");
                var geometryName = ResourceName.Parse(geometryStr)!.Value;
                var partialGeometry = manager.Get<GeometryJson>(geometryName)!.Geometry;

                if (variantJson.TryGetProperty("variables", out var varJson))
                    partialGeometry = partialGeometry.ApplySubstitutions(IncludeGeometryProvider.CollectVariables(varJson));

                var rawGeometry = partialGeometry.Prime();

                variants.Add((rule, rawGeometry));
            }

            var dynamic = modelJson.TryGetProperty("dynamic", out var dynamicJson) && dynamicJson.GetBoolean();

            return new RawJsonModel(name, variants, dynamic);
        }
    }
}