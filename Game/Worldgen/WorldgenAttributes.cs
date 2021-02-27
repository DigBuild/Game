using DigBuildEngine.Reg;
using DigBuildEngine.Util;
using DigBuildEngine.Worldgen;
using DigBuildPlatformCS.Resource;

namespace DigBuild.Worldgen
{
    public static class WorldgenAttributes
    {
        public static WorldgenAttribute<ImmutableMap2D<ushort>> TerrainHeight { get; private set; } = null!;
        public static WorldgenAttribute<ImmutableMap2D<TerrainType>> TerrainType { get; private set; } = null!;

        public static WorldgenAttribute<ImmutableMap2D<ushort>> WaterHeight { get; private set; } = null!;

        internal static void Register(RegistryBuilder<IWorldgenAttribute> builder)
        {
            TerrainHeight = builder.Create<ImmutableMap2D<ushort>>(new ResourceName(Game.Domain, "terrain_height"));
            TerrainType = builder.Create<ImmutableMap2D<TerrainType>>(new ResourceName(Game.Domain, "terrain_type"));

            WaterHeight = builder.Create<ImmutableMap2D<ushort>>(new ResourceName(Game.Domain, "water_height"));
        }
    }
}