using DigBuild.Engine.Registries;
using DigBuild.Platform.Audio;
using DigBuild.Platform.Resource;

namespace DigBuild.Audio
{
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

    public static class SoundRegistryBuilderExtensions
    {
        public static Sound Register(this IRegistryBuilder<Sound> registry, string domain, string path)
        {
            return registry.Register(domain, path, path);
        }

        public static Sound Register(this IRegistryBuilder<Sound> registry, string domain, string path, string clipPath)
        {
            return registry.Add(new ResourceName(domain, path), new Sound(new ResourceName(domain, $"sounds/{clipPath}.ogg")));
        }
    }
}