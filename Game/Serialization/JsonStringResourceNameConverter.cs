using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using DigBuild.Platform.Resource;

namespace DigBuild.Serialization
{
    /// <summary>
    /// A <see cref="ResourceName"/> JSON serdes.
    /// </summary>
    public class JsonStringResourceNameConverter : JsonConverter<ResourceName>
    {
        public override ResourceName Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var name = ResourceName.Parse(reader.GetString()!);
            if (!name.HasValue)
                throw new JsonException("Failed to parse resource name. It must be in the format 'domain:path'.");
            return name.Value;
        }

        public override void Write(Utf8JsonWriter writer, ResourceName value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}