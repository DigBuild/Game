using System.Text.Json;

namespace DigBuild.Serialization
{
    public static class JsonSerializationUtils
    {
        public static T Get<T>(this JsonElement element, JsonSerializerOptions? options = null)
        {
            var json = element.GetRawText();
            return JsonSerializer.Deserialize<T>(json, options)!;
        }
        public static T GetProperty<T>(this JsonElement element, string name, JsonSerializerOptions? options = null)
        {
            if (!element.TryGetProperty(name, out var value))
                throw new JsonException($"Could not find property \"{name}\".");
            return value.Get<T>(options);
        }
    }
}