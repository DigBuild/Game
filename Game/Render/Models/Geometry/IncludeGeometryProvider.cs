using System.Collections.Generic;
using System.Text.Json;
using DigBuild.Platform.Resource;
using DigBuild.Render.Models.Expressions;
using DigBuild.Serialization;

namespace DigBuild.Render.Models.Geometry
{
    public class IncludeGeometryProvider : IGeometryProvider
    {
        public IPartialGeometry Provide(
            JsonElement json, JsonSerializerOptions jsonOptions,
            ResourceManager resourceManager, ResourceName currentFile
        )
        {
            var geometryPath = json.GetProperty<string>("geometry");
            var geometryName = ResourceName.Parse(geometryPath) ?? currentFile.GetSibling(geometryPath);
            var geometryJson = resourceManager.Get<GeometryJson>(geometryName)!;
            
            if (!json.TryGetProperty("variables", out var jsonVars))
                return geometryJson.Geometry;

            var variables = CollectVariables(jsonVars);
            return geometryJson.Geometry.ApplySubstitutions(variables);
        }

        public static Dictionary<string, IModelExpression> CollectVariables(JsonElement json)
        {
            var variables = new Dictionary<string, IModelExpression>();
            AddVariables(variables, json, "");
            return variables;
        }

        private static void AddVariables(IDictionary<string, IModelExpression> variables, JsonElement json, string prefix)
        {
            foreach (var property in json.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.String)
                    variables[prefix + property.Name] = ModelExpressionParser.Parse(property.Value.GetString()!);
                else
                    AddVariables(variables, property.Value, prefix + property.Name + "/");
            }
        }
    }
}