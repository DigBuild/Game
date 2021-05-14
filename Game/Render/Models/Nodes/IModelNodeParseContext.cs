using System.Text.Json;
using DigBuild.Engine.Render;

namespace DigBuild.Render.Models.Nodes
{
    public interface IModelNodeParseContext
    {
        JsonSerializerOptions SerializerOptions { get; }

        RenderLayer<WorldVertex>? GetLayer(string name);
    }
}