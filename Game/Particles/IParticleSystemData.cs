using DigBuild.Engine.Particles;
using DigBuild.Engine.Registries;
using DigBuild.Engine.Render;
using DigBuild.Platform.Resource;
using DigBuild.Platform.Util;
using DigBuild.Render;

namespace DigBuild.Particles
{
    /// <summary>
    /// A particle system's data.
    /// </summary>
    public interface IParticleSystemData
    {
        /// <summary>
        /// The particle system.
        /// </summary>
        IParticleSystem System { get; }
        /// <summary>
        /// The particle renderer.
        /// </summary>
        IParticleRenderer Renderer { get; }

        internal void InitializeRenderer(NativeBufferPool pool, ResourceManager resourceManager);
    }
}