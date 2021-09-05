using System.Diagnostics.CodeAnalysis;
using DigBuild.Engine.Collections;
using DigBuild.Engine.Registries;
using DigBuild.Engine.Worldgen;
using DigBuild.Platform.Resource;

namespace DigBuild.Worldgen.Biomes
{
    /// <summary>
    /// A world generation biome.
    /// </summary>
    public interface IBiome
    {
        /// <summary>
        /// Computes the grid of scores for this biome in the given chunk description context.
        /// </summary>
        /// <param name="context">The context</param>
        /// <returns>The score grid</returns>
        Grid<float> ComputeScores(ChunkDescriptionContext context);

        /// <summary>
        /// Tries to get the value of a given attribute.
        /// </summary>
        /// <typeparam name="T">The attribute type</typeparam>
        /// <param name="attribute">The attribute</param>
        /// <param name="value">The value</param>
        /// <returns>Whether it was found or not</returns>
        bool TryGet<T>(BiomeAttribute<T> attribute, [MaybeNullWhen(false)] out T value)
            where T : notnull;
        
        /// <summary>
        /// Gets the value of a given attribute, or a default.
        /// </summary>
        /// <typeparam name="T">The attribute type</typeparam>
        /// <param name="attribute">The attribute</param>
        /// <param name="defaultValue">The default value</param>
        /// <returns>The attribute's value if found, otherwise the provideddefault</returns>
        T Get<T>(BiomeAttribute<T> attribute, T defaultValue)
            where T : notnull
        {
            return TryGet(attribute, out var value) ? value : defaultValue;
        }
    }

    /// <summary>
    /// Registry extensions for biomes.
    /// </summary>
    public static class BiomeRegistryBuilderExtensions
    {
        /// <summary>
        /// Registers a new biome.
        /// </summary>
        /// <typeparam name="T">The biome type</typeparam>
        /// <param name="registry">The registry</param>
        /// <param name="name">The name</param>
        /// <param name="biome">The biome</param>
        /// <returns>The biome</returns>
        public static T Register<T>(this IRegistryBuilder<IBiome> registry, ResourceName name, T biome) where T : IBiome
        {
            registry.Add(name, biome);
            return biome;
        }
    }
}