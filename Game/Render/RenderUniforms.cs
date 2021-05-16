using DigBuild.Engine.BuiltIn;
using DigBuild.Engine.BuiltIn.GeneratedUniforms;
using DigBuild.Engine.Render;
using DigBuild.Platform.Render;
using DigBuild.Render.GeneratedUniforms;

namespace DigBuild.Render
{
    public static class RenderUniforms
    {
        // public static RenderUniform<SimpleTransform> ProjectionTransform { get; } = new();
        public static RenderUniform<SimpleTransform> ModelViewTransform => BuiltInRenderUniforms.ModelViewTransform;

        public static RenderUniform<WorldTimeUniform> WorldTime { get; } = new();
    }

    public interface IWorldTimeUniform : IUniform<WorldTimeUniform>
    {
        float WorldTime { get; set; }
    }
}