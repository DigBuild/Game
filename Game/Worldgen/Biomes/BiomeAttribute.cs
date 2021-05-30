using DigBuild.Engine.Registries;
using DigBuild.Platform.Resource;

namespace DigBuild.Worldgen.Biomes
{
    public interface IBiomeAttribute { }

    public sealed class BiomeAttribute<T> : IBiomeAttribute
        where T : notnull
    {
        internal BiomeAttribute()
        {
        }
    }

    public static class BiomeAttributeRegistryBuilderExtensions
    {
        public static BiomeAttribute<TStorage> Create<TStorage>(this IRegistryBuilder<IBiomeAttribute> registry, ResourceName name)
            where TStorage : notnull
        {
            var attribute = new BiomeAttribute<TStorage>();
            registry.Add(name, attribute);
            return attribute;
        }
    }
}