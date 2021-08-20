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
        public static IWorldgenFeature TerrainSmoothing { get; private set; } = null!;

        public static IWorldgenFeature Water { get; private set; } = null!;
        public static IWorldgenFeature Trees { get; private set; } = null!;
        
        public static IWorldgenFeature TallGrass { get; private set; } = null!;
        public static IWorldgenFeature Barley { get; private set; } = null!;

        internal static void Register(RegistryBuilder<IWorldgenFeature> builder)
        {
            Constraints = builder.Add(new ResourceName(DigBuildGame.Domain, "constraints"), new ConstraintsWorldgenFeature());

            Biome = builder.Add(new ResourceName(DigBuildGame.Domain, "biome"), new BiomeWorldgenFeature());
            
            Terrain = builder.Add(new ResourceName(DigBuildGame.Domain, "terrain"), new TerrainWorldgenFeature(GameBlocks.Dirt, GameBlocks.Grass));
            TerrainSmoothing = builder.Add(new ResourceName(DigBuildGame.Domain, "terrain_smoothing"), new TerrainSmoothingFeature());

            Water = builder.Add(new ResourceName(DigBuildGame.Domain, "water"), new WaterWorldgenFeature(GameBlocks.Water, GameBlocks.Ice));
            Trees = builder.Add(new ResourceName(DigBuildGame.Domain, "trees"), new TreeWorldgenFeature(new TreeStructure()));
            
            TallGrass = builder.Add(new ResourceName(DigBuildGame.Domain, "tall_grass"), new LowCoverWorldgenFeature(
                GameBlocks.Tallgrass, WorldgenAttributes.TallGrass, 1658152135341, 0.075F, () => new []
                {
                    GameBiomes.Grassland,
                    GameBiomes.Pasture,
                    GameBiomes.Plateau
                }
            ));
            Barley = builder.Add(new ResourceName(DigBuildGame.Domain, "barley"), new LowCoverWorldgenFeature(
                GameBlocks.Barley, WorldgenAttributes.Barley, 84685187164651, 0.05F, () => new []{ GameBiomes.Pasture }
            ));
        }
    }
}