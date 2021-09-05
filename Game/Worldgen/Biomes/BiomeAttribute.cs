using DigBuild.Engine.Registries;
using DigBuild.Platform.Resource;

namespace DigBuild.Worldgen.Biomes
{
    /// <summary>
    /// A biome attribute.
    /// </summary>
    public interface IBiomeAttribute { }

    /// <summary>
    /// A biome attribute.
    /// </summary>
    /// <typeparam name="T">The attribute type</typeparam>
    public sealed class BiomeAttribute<T> : IBiomeAttribute
        where T : notnull
    {
        internal BiomeAttribute()
        {
        }
    }

    /// <summary>
    /// Registry extensions for biome attributes.
    /// </summary>
    public static class BiomeAttributeRegistryBuilderExtensions
    {
        /// <summary>
        /// Registers a new biome attribute.
        /// </summary>
        /// <typeparam name="T">The attribute type</typeparam>
        /// <param name="registry">The registry</param>
        /// <param name="name">The name</param>
        /// <returns>The attribute</returns>
        public static BiomeAttribute<T> Register<T>(this IRegistryBuilder<IBiomeAttribute> registry, ResourceName name)
            where T : notnull
        {
            var attribute = new BiomeAttribute<T>();
            registry.Add(name, attribute);
            return attribute;
        }
    }
}