using DigBuild.Blocks;
using DigBuildEngine.Reg;
using DigBuildEngine.Util;
using DigBuildEngine.Worldgen;
using DigBuildPlatformCS.Resource;

namespace DigBuild.Worldgen
{
    public static class WorldgenFeatures
    {
        public static IWorldgenFeature Terrain { get; private set; } = null!;
        public static IWorldgenFeature Water { get; private set; } = null!;

        internal static void Register(RegistryBuilder<IWorldgenFeature> builder)
        {
            Terrain = builder.Add(new ResourceName(Game.Domain, "terrain"), new TerrainWorldgenFeature(GameBlocks.Terrain));
            Water = builder.Add(new ResourceName(Game.Domain, "water"), new WaterWorldgenFeature(GameBlocks.Water));
        }
    }
}