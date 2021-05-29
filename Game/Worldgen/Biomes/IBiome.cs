using DigBuild.Engine.Blocks;
using DigBuild.Engine.Collections;
using DigBuild.Engine.Math;
using DigBuild.Engine.Registries;
using DigBuild.Engine.Worldgen;
using DigBuild.Platform.Resource;

namespace DigBuild.Worldgen.Biomes
{
    public interface IBiome
    {
        Block SurfaceBlock { get; }

        Grid<float> GetScores(WorldSliceDescriptionContext context);
        RangeT<T>? GetConstraints<T>(WorldgenAttribute<Grid<T>> attribute);
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