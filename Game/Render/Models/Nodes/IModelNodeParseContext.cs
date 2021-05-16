using System.Text.Json;
using DigBuild.Engine.Render;
using DigBuild.Render.Worlds;

namespace DigBuild.Render.Models.Nodes
{
    public interface IModelNodeParseContext
    {
        JsonSerializerOptions SerializerOptions { get; }

        IRenderLayer<WorldVertex>? GetLayer(string name);
    }
}