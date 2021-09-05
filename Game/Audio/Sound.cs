using DigBuild.Engine.Registries;
using DigBuild.Platform.Audio;
using DigBuild.Platform.Resource;

namespace DigBuild.Audio
{
    /// <summary>
    /// A sound resource.
    /// </summary>
    public sealed class Sound
    {
        private readonly ResourceName _resourceName;

        internal AudioClip Clip { get; private set; } = null!;

        public Sound(ResourceName resourceName)
        {
            _resourceName = resourceName;
        }

        internal void Load(ResourceManager resourceManager, AudioSystem audioSystem)
        {
            Clip = audioSystem.Load(resourceManager.GetResource(_resourceName)!);
        }
    }

    /// <summary>
    /// Registry builder extensions for sound resources.
    /// </summary>
    public static class SoundRegistryBuilderExtensions
    {
        /// <summary>
        /// Registers a new sound.
        /// </summary>
        /// <param name="registry">The registry</param>
        /// <param name="domain">The domain</param>
        /// <param name="path">The path</param>
        /// <returns>The sound</returns>
        public static Sound Register(this IRegistryBuilder<Sound> registry, string domain, string path)
        {
            return registry.Register(domain, path, path);
        }
        
        /// <summary>
        /// Registers a new sound.
        /// </summary>
        /// <param name="registry">The registry</param>
        /// <param name="domain">The domain</param>
        /// <param name="path">The path</param>
        /// <param name="clipPath">The path to the sound clip</param>
        /// <returns>The sound</returns>
        public static Sound Register(this IRegistryBuilder<Sound> registry, string domain, string path, string clipPath)
        {
            return registry.Add(new ResourceName(domain, path), new Sound(new ResourceName(domain, $"sounds/{clipPath}.ogg")));
        }
    }
}