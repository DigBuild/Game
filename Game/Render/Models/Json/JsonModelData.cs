using System.Collections.Generic;
using System.Linq;

namespace DigBuild.Render.Models.Json
{
    public sealed class JsonModelData
    {
        public static JsonModelData Empty { get; } = new();

        private readonly Dictionary<string, string> _data = new();

        public IReadOnlyDictionary<string, string> Data => _data;

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