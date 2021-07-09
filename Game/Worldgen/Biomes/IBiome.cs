using System.Diagnostics.CodeAnalysis;
using DigBuild.Engine.Collections;
using DigBuild.Engine.Registries;
using DigBuild.Engine.Worldgen;
using DigBuild.Platform.Resource;

namespace DigBuild.Worldgen.Biomes
{
    public interface IBiome
    {
        Grid<float> GetScores(ChunkDescriptionContext context);

        bool TryGetAttribute<T>(BiomeAttribute<T> attribute, [MaybeNullWhen(false)] out T value)
            where T : notnull;
        T Get<T>(BiomeAttribute<T> attribute, T defaultValue)
            where T : notnull
        {
            return TryGetAttribute(attribute, out var value) ? value : defaultValue;
        }
    }

    public static class BiomeRegistryBuilderExtensions
    {
        public static T Add<T>(this IRegistryBuilder<IBiome> registry, ResourceName name, T biome) where T : IBiome
        {
            registry.Add(name, biome);
            return biome;
        }
    }
}