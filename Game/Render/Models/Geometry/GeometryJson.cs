using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using DigBuild.Engine.Math;
using DigBuild.Engine.Render;
using DigBuild.Engine.Render.Models;
using DigBuild.Platform.Resource;
using DigBuild.Registries;
using DigBuild.Render.Models.Expressions;
using DigBuild.Serialization;

namespace DigBuild.Render.Models.Geometry
{
    public sealed class GeometryJson : ICustomResource
    {
        public ResourceName Name { get; }

        public IPartialGeometry Geometry { get; }

        private GeometryJson(ResourceName name, IPartialGeometry geometry)
        {
            Name = name;
            Geometry = geometry;
        }

        public static GeometryJson? Load(ResourceManager manager, ResourceName name)
        {
            var actualName = new ResourceName(name.Domain, "geometry/" + name.Path + ".json");
            if (!manager.TryGetResource(actualName, out var jsonResource))
                return null;

            var jsonOptions = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                PropertyNamingPolicy = new JsonSnakeCaseNamingPolicy(),
                DictionaryKeyPolicy = new JsonSnakeCaseNamingPolicy(),
                Converters =
                {
                    new JsonStringEnumConverter(),
                    new JsonStringResourceNameConverter(),
                    new JsonArrayVector3Converter()
                }
            };
            
            var bytes = jsonResource.ReadAllBytes();
            var span = new ReadOnlySpan<byte>(bytes);
            var geometryJson = JsonSerializer.Deserialize<JsonElement>(span, jsonOptions);

            var elements = new List<IPartialGeometry>();
            foreach (var element in geometryJson.GetProperty("elements").EnumerateArray())
            {
                var rawGeometry = ParseElement(element, jsonOptions, manager, name);
                elements.Add(rawGeometry);
            }

            var geometry = elements.Count == 1 ? elements.First() : new PartialMergedGeometry(elements);
            return new GeometryJson(name, geometry);
        }

        public static IPartialGeometry ParseElement(
            JsonElement json, JsonSerializerOptions jsonOptions,
            ResourceManager resourceManager, ResourceName name
        )
        {
            var type = json.GetProperty<string>("_type");
            var typeName = ResourceName.Parse(type)!.Value;
            var provider = GameRegistries.GeometryProviders.GetOrNull(typeName)!;

            return provider.Provide(json, jsonOptions, resourceManager, name);
        }

        private sealed class PartialMergedGeometry : IPartialGeometry
        {
            private readonly IReadOnlyList<IPartialGeometry> _elements;

            public PartialMergedGeometry(IReadOnlyList<IPartialGeometry> elements)
            {
                _elements = elements;

                var requiredVariables = new HashSet<string>();
                foreach (var element in elements)
                foreach (var variable in element.RequiredVariables)
                    requiredVariables.Add(variable);
                RequiredVariables = requiredVariables;
            }

            public IEnumerable<string> RequiredVariables { get; }

            public IPartialGeometry ApplySubstitutions(IReadOnlyDictionary<string, IModelExpression> variables)
            {
                var newElements = _elements.Select(e => e.ApplySubstitutions(variables)).ToList();
                return new PartialMergedGeometry(newElements);
            }

            public IRawGeometry Prime()
            {
                return new RawMergedGeometry(_elements.Select(e => e.Prime()).ToList());
            }
        }

        private sealed class RawMergedGeometry : IRawGeometry
        {
            private readonly IReadOnlyList<IRawGeometry> _elements;

            public RawMergedGeometry(IReadOnlyList<IRawGeometry> elements)
            {
                _elements = elements;
            }

            public void LoadTextures(MultiSpriteLoader loader)
            {
                foreach (var rawGeometry in _elements)
                    rawGeometry.LoadTextures(loader);
            }

            public IGeometry Build()
            {
                return new MergedGeometry(_elements.Select(e => e.Build()).ToList());
            }
        }

        private sealed class MergedGeometry : IGeometry
        {
            private readonly IEnumerable<IGeometry> _geometries;

            public MergedGeometry(IEnumerable<IGeometry> geometries)
            {
                _geometries = geometries;
            }

            public void Add(IGeometryBuffer buffer, IReadOnlyModelData data, DirectionFlags visibleFaces)
            {
                foreach (var geometry in _geometries)
                    geometry.Add(buffer, data, visibleFaces);
            }
        }
    }
}