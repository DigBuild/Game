using System.Text.Json;
using DigBuild.Engine.Render;
using DigBuild.Render;

namespace DigBuild.Client.Models.Nodes
{
    public interface IModelNodeParseContext
    {
        JsonSerializerOptions SerializerOptions { get; }

        RenderLayer<SimpleVertex>? GetLayer(string name);
    }
}