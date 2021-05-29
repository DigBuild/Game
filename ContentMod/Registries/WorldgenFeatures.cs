using DigBuild.Content.Worldgen;
using DigBuild.Content.Worldgen.Structure;
using DigBuild.Engine.Registries;
using DigBuild.Engine.Worldgen;
using DigBuild.Platform.Resource;

namespace DigBuild.Content.Registries
{
    public static class WorldgenFeatures
    {
        public static IWorldgenFeature Constraints { get; private set; } = null!;
        public static IWorldgenFeature Biome { get; private set; } = null!;

        public static IWorldgenFeature Terrain { get; private set; } = null!;
        public static IWorldgenFeature Water { get; private set; } = null!;
        public static IWorldgenFeature Lushness { get; private set; } = null!;
        public static IWorldgenFeature Trees { get; private set; } = null!;

        internal static void Register(RegistryBuilder<IWorldgenFeature> builder)
        {
            Constraints = builder.Add(new ResourceName(DigBuildGame.Domain, "constraints"), new ConstraintsWorldgenFeature());
            Biome = builder.Add(new ResourceName(DigBuildGame.Domain, "biome"), new BiomeWorldgenFeature());
            
            Terrain = builder.Add(new ResourceName(DigBuildGame.Domain, "terrain"), new TerrainWorldgenFeature(GameBlocks.Dirt, GameBlocks.Grass));
            Water = builder.Add(new ResourceName(DigBuildGame.Domain, "water"), new WaterWorldgenFeature(GameBlocks.Water));
            Lushness = builder.Add(new ResourceName(DigBuildGame.Domain, "lushness"), new LushnessWorldgenFeature());
            Trees = builder.Add(new ResourceName(DigBuildGame.Domain, "trees"), new TreeWorldgenFeature(new TreeStructure()));
        }
    }
}