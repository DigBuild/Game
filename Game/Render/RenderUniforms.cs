using DigBuild.Engine.BuiltIn;
using DigBuild.Engine.BuiltIn.GeneratedUniforms;
using DigBuild.Engine.Render;
using DigBuild.Platform.Render;
using DigBuild.Render.GeneratedUniforms;

namespace DigBuild.Render
{
    public static class RenderUniforms
    {
        public static Engine.Render.UniformType<SimpleTransform> ModelViewTransform => BuiltInRenderUniforms.ModelViewProjectionTransform;

        public static Engine.Render.UniformType<WorldTimeUniform> WorldTime { get; } = new();
    }

    public interface IWorldTimeUniform : IUniform<WorldTimeUniform>
    {
        float WorldTime { get; set; }
    }
}