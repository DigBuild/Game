using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using DigBuild.Engine.Registries;
using DigBuild.Platform.Resource;

namespace DigBuild.Serialization
{
    public class JsonStringRegistryEntryConverter<T> : JsonConverter<T?> where T : notnull
    {
        private readonly Registry<T> _registry;

        internal JsonStringRegistryEntryConverter(Registry<T> registry)
        {
            _registry = registry;
        }

        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var name = ResourceName.Parse(reader.GetString()!);
            if (!name.HasValue)
                throw new JsonException("Failed to parse resource name. It must be in the format 'domain:path'.");
            return _registry.GetOrNull(name.Value);
        }

        public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
        {
            if (value == null)
                throw new JsonException("Value cannot be null.");
            writer.WriteStringValue(_registry.GetNameOrNull(value)!.Value.ToString());
        }
    }
}