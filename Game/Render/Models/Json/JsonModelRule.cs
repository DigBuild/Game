using System.Collections.Generic;

namespace DigBuild.Render.Models.Json
{
    public sealed class JsonModelRule
    {
        private readonly IReadOnlyDictionary<string, string>? _singleMatches;
        private readonly IReadOnlyDictionary<string, HashSet<string>>? _multiMatches;

        public JsonModelRule(
            IReadOnlyDictionary<string, string>? singleMatches,
            IReadOnlyDictionary<string, HashSet<string>>? multiMatches
        )
        {
            _singleMatches = singleMatches;
            _multiMatches = multiMatches;
        }

        public bool Test(JsonModelData data)
        {
            if (_singleMatches != null)
            {
                foreach (var (key, value) in _singleMatches)
                {
                    if (data[key] != value)
                        return false;
                }
            }

            if (_multiMatches != null)
            {
                foreach (var (key, values) in _multiMatches)
                {
                    if (!values.Contains(data[key]))
                        return false;
                }
            }

            return true;
        }

    }
}