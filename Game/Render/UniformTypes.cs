using DigBuild.Engine.BuiltIn;
using DigBuild.Engine.BuiltIn.GeneratedUniforms;
using DigBuild.Engine.Render;
using DigBuild.Platform.Render;
using DigBuild.Render.GeneratedUniforms;

namespace DigBuild.Render
{
    /// <summary>
    /// The game's render uniforms.
    /// </summary>
    public static class UniformTypes
    {
        /// <summary>
        /// The engine's built-in model-view-projection transform.
        /// </summary>
        public static UniformType<SimpleTransform> ModelViewProjectionTransform => BuiltInRenderUniforms.ModelViewProjectionTransform;

        /// <summary>
        /// The world time uniform.
        /// </summary>
        public static UniformType<WorldTimeUniform> WorldTime { get; } = new();
    }

    /// <summary>
    /// A world time uniform.
    /// </summary>
    public interface IWorldTimeUniform : IUniform<WorldTimeUniform>
    {
        /// <summary>
        /// The world time.
        /// </summary>
        float WorldTime { get; set; }
    }
}