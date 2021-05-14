using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using DigBuild.Engine.Math;
using DigBuild.Platform.Resource;
using DigBuild.Serialization;

namespace DigBuild.Render.Models
{
    public sealed class RawCuboidModelDefinition : ICustomResource
    {
        [JsonIgnore]
        public ResourceName Name { get; private set; }

        public List<Cuboid> Cuboids { get; set; } = null!;

        public bool Solid { get; set; } = true;
        
        public string? Layer { get; set; } = "solid";
        
        public sealed class Cuboid
        {
            public Vector3 From { get; set; }
            public Vector3 To { get; set; }
            public CuboidTextures Textures { get; set; } = null!;

            public sealed class CuboidTextures
            {
                [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
                public ResourceName? NegX { get; set; }

                [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
                public ResourceName? PosX { get; set; }

                [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
                public ResourceName? NegY { get; set; }

                [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
                public ResourceName? PosY { get; set; }

                [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
                public ResourceName? NegZ { get; set; }

                [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
                public ResourceName? PosZ { get; set; }

                public ResourceName? Get(Direction direction)
                {
                    return direction switch
                    {
                        Direction.NegX => NegX,
                        Direction.PosX => PosX,
                        Direction.NegY => NegY,
                        Direction.PosY => PosY,
                        Direction.NegZ => NegZ,
                        Direction.PosZ => PosZ,
                        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
                    };
                }
            }
        }

        public static RawCuboidModelDefinition? Load(ResourceManager manager, ResourceName name)
        {
            var actualResourceName = new ResourceName(name.Domain, $"models/{name.Path}.json");
            if (!manager.TryGetResource(actualResourceName, out var res))
                return null;

            var bytes = res.ReadAllBytes();
            var span = new ReadOnlySpan<byte>(bytes);

            var model = JsonSerializer.Deserialize<RawCuboidModelDefinition>(span, new JsonSerializerOptions
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
            });

            if (model == null)
                return null;

            model.Name = name;
            return model;
        }
    }
}