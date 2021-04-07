using System.Text.Json;
using System.Text.RegularExpressions;

namespace DigBuild.Serialization
{
    public class JsonSnakeCaseNamingPolicy : JsonNamingPolicy
    {
        private static readonly Regex Regex = new("(?<!^)([A-Z])");

        public override string ConvertName(string name)
        {
            var convertName = Regex.Replace(name, "_$1").ToLower();
            return convertName;
        }
    }
}