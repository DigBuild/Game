using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace DigBuild.Serialization
{
    /// <summary>
    /// JSON helpers.
    /// </summary>
    public static class JsonSerializationUtils
    {
        /// <summary>
        /// Converts the JSON element into an object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type</typeparam>
        /// <param name="element">The JSON element</param>
        /// <param name="options">The serializer options</param>
        /// <returns>The object</returns>
        public static T Get<T>(this JsonElement element, JsonSerializerOptions? options = null)
        {
            var json = element.GetRawText();
            return JsonSerializer.Deserialize<T>(json, options)!;
        }
        
        /// <summary>
        /// Converts a property of the JSON element into an object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type</typeparam>
        /// <param name="element">The JSON element</param>
        /// <param name="name">The name of the property</param>
        /// <param name="options">The serializer options</param>
        /// <returns>The object</returns>
        public static T GetProperty<T>(this JsonElement element, string name, JsonSerializerOptions? options = null)
        {
            if (!element.TryGetProperty(name, out var value))
                throw new JsonException($"Could not find property \"{name}\".");
            return value.Get<T>(options);
        }
        
        /// <summary>
        /// Tries to convert a property of the JSON element into an object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type</typeparam>
        /// <param name="element">The JSON element</param>
        /// <param name="name">The name of the property</param>
        /// <param name="value">The object</param>
        /// <param name="options">The serializer options</param>
        /// <returns>Whether the conversion was successful or not</returns>
        public static bool TryGetProperty<T>(this JsonElement element, string name, [MaybeNullWhen(false)] out T value, JsonSerializerOptions? options = null)
        {
            if (!element.TryGetProperty(name, out var v))
            {
                value = default;
                return false;
            }
            value = v.Get<T>(options);
            return true;
        }
    }
}