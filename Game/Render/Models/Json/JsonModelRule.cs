using System.Collections.Generic;

namespace DigBuild.Render.Models.Json
{
    /// <summary>
    /// A JSON model rule.
    /// </summary>
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

        /// <summary>
        /// Tests if some model data matches the rule.
        /// </summary>
        /// <param name="data">The data</param>
        /// <returns>Whether it matches the rule or not</returns>
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