using DigBuild.Engine.Registries;
using DigBuild.Platform.Resource;
using DigBuild.Worldgen.Biomes;

namespace DigBuild.Content.Registries
{
    public class GameBiomes
    {
        // Plains
        public static IBiome Grassland { get; private set; } = null!;

        // Water
        public static IBiome Ocean { get; private set; } = null!;

        internal static void Register(RegistryBuilder<IBiome> registry)
        {
            Grassland = registry.Add(new ResourceName(DigBuildGame.Domain, "grassland"), new SimpleBiome
            {
                EnvironmentConstraints = new WorldgenRangeSet
                {
                    { WorldgenAttributes.Inlandness, 0.5f, 1.0f }
                },
                GenerationConstraints = new WorldgenRangeSet
                {
                    { WorldgenAttributes.TerrainHeight, 2, 5 }
                },
                SurfaceBlock = GameBlocks.Grass
            });

            Ocean = registry.Add(new ResourceName(DigBuildGame.Domain, "ocean"), new SimpleBiome
            {
                EnvironmentConstraints = new WorldgenRangeSet
                {
                    { WorldgenAttributes.Inlandness, 0.0f, 0.5f }
                },
                GenerationConstraints = new WorldgenRangeSet
                {
                    { WorldgenAttributes.TerrainHeight, 2, 5 }
                },
                SurfaceBlock = GameBlocks.Stone
            });
        }
    }
}