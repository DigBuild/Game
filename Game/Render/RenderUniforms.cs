using DigBuild.Engine.Render.New;
using DigBuild.Render.GeneratedUniforms;

namespace DigBuild.Render
{
    public static class RenderUniforms
    {
        public static RenderUniform<SimpleTransform> SimpleTransform { get; } = new();
    }
}