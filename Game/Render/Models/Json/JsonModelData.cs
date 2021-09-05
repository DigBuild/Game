using System.Collections.Generic;
using System.Linq;

namespace DigBuild.Render.Models.Json
{
    /// <summary>
    /// A JSON model data container. Stored as string-string KV pairs.
    /// </summary>
    public sealed class JsonModelData
    {
        /// <summary>
        /// Empty model data.
        /// </summary>
        public static JsonModelData Empty { get; } = new();

        private readonly Dictionary<string, string> _data = new();

        /// <summary>
        /// The current data.
        /// </summary>
        public IReadOnlyDictionary<string, string> Data => _data;

        /// <summary>
        /// The value of a given key.
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>The value</returns>
        public string this[string key]
        {
            get => _data.GetValueOrDefault(key, "");
            set => _data[key] = value;
        }

        public override string ToString()
        {
            return $"Data: {string.Join(", ", Data.Select(pair => $"{pair.Key}={pair.Value}"))}";
        }
    }
}